(function ($) {
    $.createSpiralPattern = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#createSpiralPattern-form',
            saveBtn: '#createSpiralPattern-saveBtn',
            resetBtn: '#createSpiralPattern-resetBtn',
            editShiftModal: '#editShiftModal',
        }, options);

        var weeklyListUrl = settings.baseUrl + "/GetAllSpiralWeeklyPatternAsync";
        var bioWeeklyListUrl = settings.baseUrl + '/GetAllSpiralFortnightlyPatternAsync';
        var monthlyListUrl = settings.baseUrl + '/GetAllSpiralMonthlyPatternAsync';
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        $(() => {


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

                loadSpiralWeeklyPatterns();
                loadSpiralBioWeeklyPatterns();
                loadSpiralMonthlyPatterns();
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


            // #region Load shift by opening edit shift modal
            $(settings.editShiftModal).on('show.bs.modal', function (e) {
                var btn = $(e.relatedTarget);
                var detailID = btn.data('id');
                var organizationId = btn.data('organization-id');
                var shiftId = btn.data('shift-id');
                //var depId = btn.data('dep-id');
                //var empId = btn.data('emp-id');
                //var overrideDate = btn.data('date');

                $('#RosterInHolyDayIdEdit').val(detailID);
                $('#OrganizationIdEdit').val(organizationId);
                //$('#DepartmentIdEdit').val(depId);
                //$('#EmployeeIdEdit').val(empId);
                //$('#DayDateEdit').val(overrideDate);

                $.ajax({
                    url: '/CreateSpiralPattern/GetShiftByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (shifts) {
                        const $shiftSelect = $('#editShiftModalShiftID');

                        $shiftSelect.empty();

                        // Add default placeholder
                        $shiftSelect.append(`<option value="">Select Shift...</option>`);

                        // Populate shift options
                        shifts.forEach(shift => {
                            const isSelected = shift.id === shiftId;
                            $shiftSelect.append(
                                `<option value="${shift.id}" ${isSelected ? 'selected' : ''}>
                                    ${shift.name}
                                </option>`
                            );
                        });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading shifts:', error);
                    }
                });
            });
            // #endregion
            

            //// Close the modal
            //const modal = bootstrap.Modal.getInstance(document.getElementById('addShiftModal'));
            //modal.hide();
            


            
        });



        // #region loadSpiralWeeklyPatterns
        var currentPage = 1;
        var pageSize = 5;
        let currentSortColumn = 'SpiralWeeklyPatternID';
        let currentSortOrder = 'desc';

        $(document).ready(function () {
            loadSpiralWeeklyPatterns();

            $("#createSpiralPattern-searchInput").on("input", function () {
                currentPage = 1;
                loadSpiralWeeklyPatterns();
            });

            $("#createSpiralPattern-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadSpiralWeeklyPatterns();
                }
            });

            $("#createSpiralPattern-nextPageBtn").on('click', function () {
                currentPage++;
                loadSpiralWeeklyPatterns();
            });
        });

        function loadSpiralWeeklyPatterns(sortColumn, sortOrder) {
            var searchTerm = $("#createSpiralPattern-searchInput").val();
            $.ajax({
                url: weeklyListUrl,
                type: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    if (response.isSuccess === true) {
                        renderSpiralWeeklyPatternTable(response.data);

                        const pageInfo = response.pagination;

                        $('#startItem').text(pageInfo.startItem);
                        $('#endItem').text(pageInfo.endItem);
                        $('#totalItems').text(pageInfo.totalItems);

                        spiralWeeklyPatternsUpdatePagination(pageInfo.pageNumbers, pageInfo.currentPage, pageInfo.totalPages);
                    } else {
                        console.log('Something went wrong!');
                    }
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
                    var shiftDetail = pattern.spiralWeeklyPatternDetailsListVMs.find(d => d.dayOfWeek === day);
                    if (shiftDetail) {
                        row += `<td class="startTime py-1">
                            <div class="badge badge-phoenix-primary p-2">
                                <p class="my-2 fs-10">${shiftDetail.shiftName}</p>
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                                <div class="add-shift-btn2">
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal" 
                                        data-id="${shiftDetail.spiralWeeklyPatternDetailID}" 
                                        data-organization-id="${pattern.organizationID}"
                                        data-shift-id="${shiftDetail.shiftID}">
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
                                <div class="add-shift-btn2">
                                    
                                </div>
                            </div>
                        </td>`;
                    }
                }

                row += '</tr>';
                $tableBody.append(row);
            });
        }

        function spiralWeeklyPatternsUpdatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#createSpiralPattern-paginationLinks");
            paginationLinks.empty();
            const windowSize = 1;

            const createPageButton = (page) => `
            <li class="page-item ${page === currentPage ? 'active' : ''}">
                <button class="page-link page-btn" data-page="${page}">${page}</button>
            </li>`;

            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }

            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }

            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }

            $("#createSpiralPattern-prevPageBtn").prop('disabled', currentPage === 1);
            $("#createSpiralPattern-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        // 🔁 Page button click
        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadSpiralWeeklyPatterns(currentSortColumn, currentSortOrder);
        });
        // #endregion


        // #region loadSpiralBioWeeklyPatterns
        let currentSortColumnBioWeekly = 'SpiralBioWeeklyPatternID';

        $(document).ready(function () {
            loadSpiralBioWeeklyPatterns();

            $("#createFortnightlySpiralPattern-searchInput").on("input", function () {
                currentPage = 1;
                loadSpiralBioWeeklyPatterns();
            });

            $("#createFortnightlySpiralPattern-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadSpiralBioWeeklyPatterns();
                }
            });

            $("#createFortnightlySpiralPattern-nextPageBtn").on('click', function () {
                currentPage++;
                loadSpiralBioWeeklyPatterns();
            });
        });

        function loadSpiralBioWeeklyPatterns(sortColumn, sortOrder) {
            var searchTerm = $("#createFortnightlySpiralPattern-searchInput").val();
            $.ajax({
                url: bioWeeklyListUrl,
                type: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    if (response.isSuccess === true) {
                        renderSpiralBioWeeklyPatternTable(response.data);

                        const pageInfo = response.pagination;

                        $('#createFortnightlySpiralPattern-startItem').text(pageInfo.startItem);
                        $('#createFortnightlySpiralPattern-endItem').text(pageInfo.endItem);
                        $('#createFortnightlySpiralPattern-totalItems').text(pageInfo.totalItems);

                        spiralBioWeeklyPatternsUpdatePagination(pageInfo.pageNumbers, pageInfo.currentPage, pageInfo.totalPages);
                    } else {
                        console.log('Something went wrong!');
                    }
                },
                error: function (err) {
                    console.error('Failed to load Spiral Bio Weekly Patterns:', err);
                }
            });
        }

        function renderSpiralBioWeeklyPatternTable(data) {
            var $tableBody = $('#fortnightlyTblBody');
            $tableBody.empty(); // Clear existing rows

            if (data.length === 0) {
                $tableBody.append('<tr><td colspan="8" class="text-center">No Data Found</td></tr>');
                return;
            }

            data.forEach(function (pattern) {
                var row = '<tr class="hover-actions-trigger btn-reveal-trigger position-static">';
                row += `<td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                    <h5>${pattern.spiralBioWeeklyPatternName}</h5></ br>
                    <p class="fs-9 mb-0">${pattern.organizationName}</p>

                </td>`;

                // Loop for 7 days (0 = Saturday, 6 = Friday)
                for (var day = 0; day < 14; day++) {
                    var shiftDetail = pattern.spiralBioWeeklyPatternDetailsListVMs.find(d => d.dayOfMonth === day);
                    if (shiftDetail) {
                        // Example: you need to format Shift Time here (hardcoded now)
                        row += `<td class="startTime py-1">
                            <div class="badge badge-phoenix-primary p-2">
                                <p class="my-2 fs-10">${shiftDetail.shiftName}</p>
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                                <div class="add-shift-btn2">
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal"
                                        data-id="${shiftDetail.spiralWeeklyPatternDetailID}" 
                                        data-organization-id="${pattern.organizationID}"
                                        data-shift-id="${shiftDetail.shiftID}">
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

        function spiralBioWeeklyPatternsUpdatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#createFortnightlySpiralPattern-paginationLinks");
            paginationLinks.empty();
            const windowSize = 1;

            const createPageButton = (page) => `
            <li class="page-item ${page === currentPage ? 'active' : ''}">
                <button class="page-link page-btn" data-page="${page}">${page}</button>
            </li>`;

            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }

            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }

            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }

            $("#createFortnightlySpiralPattern-prevPageBtn").prop('disabled', currentPage === 1);
            $("#createFortnightlySpiralPattern-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        // 🔁 Page button click
        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadSpiralBioWeeklyPatterns(currentSortColumnBioWeekly, currentSortOrder);
        });
        // #endregion


        // #region loadSpiralMonthlyPatterns
        let currentSortColumnMonthly = 'SpiralMonthlyPatternID';

        $(document).ready(function () {
            loadSpiralMonthlyPatterns();

            $("#createMonthlySpiralPattern-searchInput").on("input", function () {
                currentPage = 1;
                loadSpiralMonthlyPatterns();
            });

            $("#createMonthlySpiralPattern-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadSpiralMonthlyPatterns();
                }
            });

            $("#createMonthlySpiralPattern-nextPageBtn").on('click', function () {
                currentPage++;
                loadSpiralMonthlyPatterns();
            });
        });

        function loadSpiralMonthlyPatterns(sortColumn, sortOrder) {
            var searchTerm = $("#createMonthlySpiralPattern-searchInput").val();
            $.ajax({
                url: monthlyListUrl,
                type: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    if (response.isSuccess === true) {
                        renderSpiralMonthlyPatternTable(response.data);

                        const pageInfo = response.pagination;

                        $('#createMonthlySpiralPattern-startItem').text(pageInfo.startItem);
                        $('#createMonthlySpiralPattern-endItem').text(pageInfo.endItem);
                        $('#createMonthlySpiralPattern-totalItems').text(pageInfo.totalItems);

                        spiralMonthlyPatternsUpdatePagination(pageInfo.pageNumbers, pageInfo.currentPage, pageInfo.totalPages);
                    } else {
                        console.log('Something went wrong!');
                    }
                },
                error: function (err) {
                    console.error('Failed to load Spiral Monthly Patterns:', err);
                }
            });
        }

        function renderSpiralMonthlyPatternTable(data) {
            var $tableBody = $('#monthlyTblBody');
            $tableBody.empty(); // Clear existing rows

            if (data.length === 0) {
                $tableBody.append('<tr><td colspan="8" class="text-center">No Data Found</td></tr>');
                return;
            }

            data.forEach(function (pattern) {
                var row = '<tr class="hover-actions-trigger btn-reveal-trigger position-static">';
                row += `<td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                    <h5>${pattern.spiralMonthlyPatternName}</h5></ br>
                    <p class="fs-9 mb-0">${pattern.organizationName}</p>

                </td>`;

                // Loop for 7 days (0 = Saturday, 6 = Friday)
                for (var day = 0; day < 30; day++) {
                    var shiftDetail = pattern.spiralMonthlyPatternDetailsListVMs.find(d => d.dayOfMonth === day);
                    if (shiftDetail) {
                        // Example: you need to format Shift Time here (hardcoded now)
                        row += `<td class="startTime py-1">
                            <div class="badge badge-phoenix-primary p-2">
                                <p class="my-2 fs-10">${shiftDetail.shiftName}</p>
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                                <div class="add-shift-btn2">
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal"
                                        data-id="${shiftDetail.spiralWeeklyPatternDetailID}" 
                                        data-organization-id="${pattern.organizationID}"
                                        data-shift-id="${shiftDetail.shiftID}">
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

        function spiralMonthlyPatternsUpdatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#createMonthlySpiralPattern-paginationLinks");
            paginationLinks.empty();
            const windowSize = 1;

            const createPageButton = (page) => `
            <li class="page-item ${page === currentPage ? 'active' : ''}">
                <button class="page-link page-btn" data-page="${page}">${page}</button>
            </li>`;

            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }

            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }

            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }

            $("#createMonthlySpiralPattern-prevPageBtn").prop('disabled', currentPage === 1);
            $("#createMonthlySpiralPattern-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        // 🔁 Page button click
        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadSpiralMonthlyPatterns(currentSortColumnMonthly, currentSortOrder);
        });
        // #endregion
    }
}(jQuery));