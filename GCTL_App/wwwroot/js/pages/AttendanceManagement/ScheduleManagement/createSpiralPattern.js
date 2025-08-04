(function ($) {
    $.createSpiralPattern = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#createSpiralPattern-form',
            saveBtn: '#createSpiralPattern-saveBtn',
            resetBtn: '#createSpiralPattern-resetBtn',
        }, options);

        var weeklyListUrl = settings.baseUrl + "/GetAllSpiralWeeklyPatternAsync";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        $(() => {

            loadSpiralWeeklyPatterns();


            // #region Save
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                // Determine which table is visible and get SpiralPatternName
                var patternName = '';

                if ($('#Weekly').is(':visible')) {
                    patternName = $('#Weekly input[name="SpiralPatternName"]').val();
                } else if ($('#Fortnightly').is(':visible')) {
                    patternName = $('#Fortnightly input[name="SpiralPatternName"]').val();
                } else if ($('#Monthly').is(':visible')) {
                    patternName = $('#Monthly input[name="SpiralPatternName"]').val();
                }

                // Set the value to the hidden field before serialize()
                $('#finalSpiralPatternName').val(patternName);

                var form = $('#createSpiralPattern-form');
                var formData = form.serialize();

                $.ajax({
                    url: form.attr('action'),
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        if (response.isSuccess === true) {
                            toastr.success(response.message);
                            clear();
                            //loadTableData();
                        } else {
                            const allFields = ["OrganizationID", "SpiralPatternTypeID", "SpiralPatternName"];

                            allFields.forEach(function (fieldId) {
                                validateField(fieldId, response);
                            });
                            toastr.info(response.message);
                        }
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                    }
                });
            });
            // #endregion


            // #region Toggle by SpiralPatternTypeID
            $('#SpiralPatternTypeID').val('1').trigger('change');
            $('#Weekly').removeClass('d-none');

            $('#SpiralPatternTypeID').on('change', function (e) {
                e.preventDefault();

                toggleSpiralPattern();
            });
            function toggleSpiralPattern() {
                var patternType = $('#SpiralPatternTypeID').val();

                if (patternType === '1') {
                    $('#Weekly').removeClass('d-none');
                    $('#Fortnightly').addClass('d-none');
                    $('#Monthly').addClass('d-none');
                } else if (patternType === '2') {
                    $('#Fortnightly').removeClass('d-none');
                    $('#Weekly').addClass('d-none');
                    $('#Monthly').addClass('d-none');
                } else if (patternType === '3') {
                    $('#Monthly').removeClass('d-none');
                    $('#Weekly').addClass('d-none');
                    $('#Fortnightly').addClass('d-none');
                } 
            }
            // #endregion


            // #region loadShiftByOrg
            // $('#addShiftModal').on('show.bs.modal', function () {
            $('#OrganizationID').on('change', function () {

                var organizationId = $(this).val();

                $.ajax({
                    url: '/CreateSpiralPattern/GetShiftByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (shifts) {
                        $('.shift-dropdown').each(function () {
                            const $dropdown = $(this);
                            $dropdown.empty();

                            $dropdown.append(`<option value="">Select Shift...</option>`);

                            shifts.forEach(shift => {
                                $dropdown.append(
                                    `<option value="${shift.id}">
                                    ${shift.name}
                                </option>`
                                );
                            });
                        });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading shifts:', error);
                    }
                });
            });
            // #endregion


            // #region clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();

                clear();
            })

            function clear() {
                $(settings.form)[0].reset();

                if (organizationDD) {
                    organizationDD.destroy();
                }

                if (spiralPatternTypeDD) {
                    spiralPatternTypeDD.destroy();
                }

                ["OrganizationID", "SpiralPatternTypeID", "SpiralPatternName"].forEach(function (fieldId) {
                    $('#' + fieldId).removeClass('is-valid is-invalid');
                    $('#' + fieldId + 'Error').hide().text('');
                    $('#' + fieldId).val('');
                });

                initOrganizationDD();
                initSpiralPatternTypeDD();

                spiralPatternTypeDD.setChoiceByValue('1');
                $('#Weekly').removeClass('d-none');
                toggleSpiralPattern();
            }
            // #endregion


            // #region Dropdowns
            function initOrganizationDD() {
                organizationDD = new Choices('#OrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Organization...'
                });
            }
            document.addEventListener('DOMContentLoaded', initOrganizationDD);
            initOrganizationDD();


            function initSpiralPatternTypeDD() {
                spiralPatternTypeDD = new Choices('#SpiralPatternTypeID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Pattern Type...'
                });
            }
            document.addEventListener('DOMContentLoaded', initSpiralPatternTypeDD);
            initSpiralPatternTypeDD();
            // #endregion


            
                       

            let $activeShiftButton = null;

            $(document).on('click', '.add-shift-btn', function () {
                $activeShiftButton = $(this); // Save reference to clicked button
            });

            $('#saveShift').on('click', function () {
                const $select = $('#AddShiftModalShiftID');
                const selectedOption = $select.find('option:selected');
                const shiftName = selectedOption.text();
                const shiftId = selectedOption.val();

                if (!shiftId) {
                    alert('Please select a shift.');
                    return;
                }

                // ⬇ Get dynamic time range from selected option
                const startTime = selectedOption.data('start');
                const endTime = selectedOption.data('end');
                const timeRange = `${startTime} - ${endTime}`;

                const badgeHtml = `
        <div class="badge badge-phoenix-primary p-2">
            <p class="my-2 fs-10">${timeRange}</p>
            <div class="add-shift-btn2">
                <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal">
                    <i class="fas fa-edit text-success"></i>
                </a>
                <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                    <i class="fas fa-trash text-danger"></i>
                </a>
            </div>
        </div>
    `;

                if ($activeShiftButton) {
                    const $td = $activeShiftButton.closest('td');
                    $td.html(badgeHtml);
                    $activeShiftButton = null;
                }

                // Close the modal
                const modal = bootstrap.Modal.getInstance(document.getElementById('addShiftModal'));
                modal.hide();
            });


            
        });



        // #region
        function loadSpiralWeeklyPatterns(pageNumber = 1, pageSize = 5, searchTerm = "", sortColumn = "SpiralWeeklyPatternID", sortOrder = "desc") {
            $.ajax({
                url: weeklyListUrl,
                type: 'GET',
                data: {
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    renderSpiralWeeklyPatternTable(response.data);
                    // You can handle pagination info from response.pagination if needed
                },
                error: function (err) {
                    console.error('Failed to load Spiral Weekly Patterns:', err);
                }
            });
        }

        function renderSpiralWeeklyPatternTable(data) {
            var $tableBody = $('#weeklyTblBody');
            $tableBody.empty(); // Clear existing rows

            if (data.length === 0) {
                $tableBody.append('<tr><td colspan="8" class="text-center">No Data Found</td></tr>');
                return;
            }

            data.forEach(function (pattern) {
                var row = '<tr class="hover-actions-trigger btn-reveal-trigger position-static">';
                row += `<td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                    <h5>${pattern.spiralPatternName}</h5></ br>
                    <p class="fs-9 mb-0">${pattern.organizationName}</p>

                </td>`;

                // Loop for 7 days (0 = Saturday, 6 = Friday)
                for (var day = 0; day < 7; day++) {
                    var shiftDetail = pattern.spiralWeeklyPatternDetailsVMs.find(d => d.dayOfWeek === day);
                    if (shiftDetail) {
                        // Example: you need to format Shift Time here (hardcoded now)
                        row += `<td class="startTime py-1">
                            <div class="badge badge-phoenix-primary p-2">
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                                <div class="add-shift-btn2">
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal">
                                        <i class="fas fa-edit text-success"></i>
                                    </a>
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                        <i class="fas fa-trash text-danger"></i>
                                    </a>
                                </div>
                            </div>
                        </td>`;
                    } else {
                        row += `<td class="startTime py-1">
                            <div class="badge badge-phoenix-primary p-2">
                                <p class="my-2 fs-10">Off Day</p>
                                <div class="add-shift-btn2">
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal">
                                        <i class="fas fa-edit text-success"></i>
                                    </a>
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                        <i class="fas fa-trash text-danger"></i>
                                    </a>
                                </div>
                            </div>
                        </td>`;
                    }
                }

                row += '</tr>';
                $tableBody.append(row);
            });
        }
        // #endregion
    }
}(jQuery));