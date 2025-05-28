(function ($) {
    $.addshift = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#addShift-form',
            saveBtn: '#addShift-saveBtn',
            editBtn: '#addShift-editBtn',
            resetBtn: '#addShift-resetBtn',
            bulkDelBtn: '#addShift-bulkDelBtn',
            singleDeleteBtn: '#addShift-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {


            $(settings.saveBtn).on('click', function (e) {
                debugger
                e.preventDefault();

                /*var formData = new FormData($('#addShift-form')[0]);*/

                var token = $('#addShift-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    ShiftName: $('#ShiftName').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    StartTime: $('#StartTime').val(),
                    EndTime: $('#EndTime').val(),
                    IsLateCount: $('#IsLateCount').prop('checked'),
                    IsAutomaticORManualBreakTime: $('#IsAutomaticORManualBreakTime').prop('checked'),
                    /*IsMealBreakCompulsaryDeductWithShift: $('input[name=IsMealBreakCompulsaryDeductWithShift]:checked').val() === "true",*/
                    IsMealBreakCompulsaryDeductWithShift: $('#IsMealBreakCompulsaryDeductWithShift').prop('checked'),
                    IsMealBreakComplementoryWithShift: $('#IsMealBreakComplementoryWithShift').prop('checked'),
                    IsAllowStartAndEndTime: $('#IsAllowStartAndEndTime').prop('checked'),
                    MealBreakStartTime: $('#MealBreakStartTime').val(),
                    MealBreakEndTime: $('#MealBreakEndTime').val(),
                    IsAllowOvertime: $('#IsAllowOvertime').prop('checked'),
                    GraceTime: $('#GraceTime').val(),
                    MinimumWorkingTime: $('#MinimumWorkingTime').val(),
                    MinimumRequiredOvertime: $('#MinimumRequiredOvertime').val(),
                    MaximumAllowedOvertime: $('#MaximumAllowedOvertime').val(),
                    MealBreakTime: $('#MealBreakTime').val(),
                }

                validateName();

                var id = $(settings.form).find('#ShiftID').val();
                var url = '';
                if (id > 0) {
                    url = updateUrl;
                } else {
                    url = createUrl;
                }

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (data) {
                        if (data.isSuccess) {
                            toastr.success(data.message);
                        } else {
                            toastr.info(data.message);
                        }
                    },
                    error: function (err) {
                        console.log(err);
                    }
                });
            });


            $(document).on('click', settings.editBtn, function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: getByIdUrl,
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $(settings.form).find('#ShiftID').val(data.shiftID);
                            $(settings.form).find('#ShiftName').val(data.shiftName);
                            $(settings.form).find('#OrganizationID').val(data.organizationID);
                            $(settings.form).find('#StartTime')[0]._flatpickr.setDate(data.startTime);
                            $(settings.form).find('#EndTime').val(data.endTime);
                            $(settings.form).find('#IsLateCount').prop('checked', data.isLateCount);
                            $(settings.form).find('#IsAutomaticORManualBreakTime').val(data.isAutomaticORManualBreakTime);
                            $(settings.form).find('#IsMealBreakCompulsaryDeductWithShift').val(data.isMealBreakCompulsaryDeductWithShift);
                            $(settings.form).find('#IsMealBreakComplementoryWithShift').val(data.isMealBreakComplementoryWithShift);
                            $(settings.form).find('#IsAllowStartAndEndTime').val(data.isAllowStartAndEndTime);
                            $(settings.form).find('#MealBreakStartTime').val(data.mealBreakStartTime);
                            $(settings.form).find('#MealBreakEndTime').val(data.mealBreakEndTime);
                            $(settings.form).find('#IsAllowOvertime').val(data.isAllowOvertime);
                            $(settings.form).find('#GraceTime').val(data.graceTime);
                            $(settings.form).find('#MinimumWorkingTime').val(data.minimumWorkingTime);
                            $(settings.form).find('#MinimumRequiredOvertime').val(data.minimumRequiredOvertime);
                            $(settings.form).find('#MaximumAllowedOvertime').val(data.maximumAllowedOvertime);
                            $(settings.form).find('#MealBreakTime').val(data.mealBreakTime);                            

                            $(settings.form).find(settings.saveBtn).text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });




            $(settings.resetBtn).on('click', function () {
                clear();
            });

            function clear() {
                $(settings.form)[0].reset();
                $('#ShiftID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.form).find(settings.saveBtn).text('Save');
                $("#addShift-check-all").prop('checked', false);
                $('.addShift-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
            }




            $('#ShiftName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#ShiftName').val().trim();

                if (name === '') {
                    $('#ShiftName').css('border', '1px solid red');
                } else {
                    $('#ShiftName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#ShiftName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: uniqueNameUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="ShiftName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="ShiftName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }




            $(document).ready(function () {
                $('#addShift-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.addShift-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.addShift-selectItem', function () {
                    toggleBulkActions();
                });
            });



            function toggleBulkActions() {
                const allItems = $('.addShift-selectItem');
                const checkedItems = $('.addShift-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#addShift-check-all').prop('checked', allChecked);
                $('#addShift-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#addShift-bulkSelectActions').removeClass('d-none');
                    $('#addShift-searchBox').addClass('d-none');
                    $('.addShift-bulkDelete').addClass('disabled');
                    $('.addShift-bulkEdit').addClass('disabled');
                } else {
                    $('#addShift-bulkSelectActions').addClass('d-none');
                    $('#addShift-searchBox').removeClass('d-none');
                    $('.addShift-bulkDelete').removeClass('disabled');
                    $('.addShift-bulkEdit').removeClass('disabled');
                }
            }




            $('#IsLateCount').on('change', function (e) {
                e.preventDefault();

                if ($(this).is(':checked')) {
                    $('#addShift-GraceTimeDiv').removeClass('d-none');
                    $('#addShift-EnableLateCount').addClass('d-none');
                    $('#addShift-DisableLateCount').removeClass('d-none');
                } else {
                    $('#addShift-GraceTimeDiv').addClass('d-none');
                    $('#addShift-EnableLateCount').removeClass('d-none');
                    $('#addShift-DisableLateCount').addClass('d-none');
                }
            });

            $('#IsAutomaticORManualBreakTime').on('change', function (e) {
                e.preventDefault();

                if ($(this).is(':checked')) {
                    $('#addShift-BreakTimeDiv').removeClass('d-none');
                    $('#addShift-AutomaticBreakTime').addClass('d-none');
                    $('#addShift-ManualBreakTime').removeClass('d-none');
                } else {
                    $('#addShift-BreakTimeDiv').addClass('d-none');
                    $('#addShift-AutomaticBreakTime').removeClass('d-none');
                    $('#addShift-ManualBreakTime').addClass('d-none');
                }
            });


            $('#IsAllowStartAndEndTime').on('change', function (e) {
                e.preventDefault();

                if ($(this).is(':checked')) {
                    $('#addShift-StartEndTimeDiv').removeClass('d-none');
                    $('#addShift-AllowStartEndTime').addClass('d-none');
                    $('#addShift-DenyStartEndTime').removeClass('d-none');
                } else {
                    $('#addShift-StartEndTimeDiv').addClass('d-none');
                    $('#addShift-AllowStartEndTime').removeClass('d-none');
                    $('#addShift-DenyStartEndTime').addClass('d-none');
                }
            });


            $('#IsAllowOvertime').on('change', function (e) {
                e.preventDefault();

                if ($(this).is(':checked')) {
                    $('#addShift-OvertimeDiv').removeClass('d-none');
                    $('#addShift-AllowOvertime').addClass('d-none');
                    $('#addShift-DisableOvertime').removeClass('d-none');
                } else {
                    $('#addShift-OvertimeDiv').addClass('d-none');
                    $('#addShift-AllowOvertime').removeClass('d-none');
                    $('#addShift-DisableOvertime').addClass('d-none');
                }
            });


            //$('#addShift-CompulsaryDeduct').on('change', function () {
            //    if ($(this).is(':checked')) {
            //        $('#addShift-Complementary').prop('disabled', true);
            //    } else {
            //        $('#addShift-Complementary').prop('disabled', false);
            //    }
            //});

            //$('#addShift-Complementary').on('change', function () {
            //    if ($(this).is(':checked')) {
            //        $('#addShift-CompulsaryDeduct').prop('disabled', true);
            //    } else {
            //        $('#addShift-CompulsaryDeduct').prop('disabled', false);
            //    }
            //});



            $(".timepicker-12hr").flatpickr({
                enableTime: true,       // ✅ Enables time selection (hours & minutes)
                noCalendar: true,       // ✅ Hides the calendar view, showing only the time picker
                dateFormat: "H:i",      // h = 12-hour, H = 24-hour, i = minutes, K = AM/PM
                time_24hr: true,        // ✅ Uses 24-hour time format (00:00–23:59 instead of 12-hour AM/PM)
                disableMobile: true,    // ✅ Prevents the native mobile date/time picker
                allowInput: true,        // optional: lets user leave it blank
                clickOpens: true,        // opens on click only
                defaultDate: null,       // explicitly prevents pre-filling
                //// ✅ Sets the default time to show when the picker opens
                //defaultHour: 9,         // default hour (0–23)
                //defaultMinute: 30,      // default minute (0–59)
                //minuteIncrement: 5,
                //minTime: "09:00",       // ✅ Restricts the minimum allowed time
                //maxTime: "18:00",       // ✅ Restricts the maximum allowed time
                //enableSeconds: true,    // ✅ Whether seconds can be selected (you’ll also need to update dateFormat)
                //allowInput: false,      // ✅ Disables manual typing into the input field
                //// ✅ Disables the entire picker
                //// Can be used to toggle state from JS: instance.set('disable', true/false)
                //disable: [function (date) {
                //    return false; // no disable by default
                //}],
                //// ✅ Hook that runs when a date/time is selected
                //onChange: function (selectedDates, dateStr, instance) {
                //    console.log("Time selected:", dateStr);
                //}
            });

            //$(".timepicker-12hr").each(function () {
            //    const selected = this._flatpickr.selectedDates[0];
            //    if (selected) {
            //        const timeStr = dayjs(selected).format("h:mm A");
            //        console.log(`${this.id}: ${timeStr}`);
            //    }
            //});

            

        });


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                  Pagination Starts
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        var currentPage = 1;
        var pageSize = 5;

        $('#addShift-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#addShift-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#addShift-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#addShift-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'ShiftID';
        let currentSortOrder = 'desc';

        $('th.sort').on('click', function () {
            const column = $(this).data('sort');

            if (currentSortColumn === column) {
                currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
            } else {
                currentSortColumn = column;
                currentSortOrder = 'asc';
            }

            loadTableData(currentSortColumn, currentSortOrder);
            updateSortingIndicator(column, currentSortOrder);
        });


        function updateSortingIndicator() {
            $('th.sort').each(function () {
                const $th = $(this);
                const column = $th.data('sort');
                $th.find('.sort-icon').remove();

                if (column === currentSortColumn) {
                    const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
                    $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
                } else {
                    $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
                }
            });
        }


        function loadTableData(sortColumn, sortOrder) {
            var searchTerm = $("#addShift-searchInput").val();

            $.ajax({
                url: gridUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#addShift-tBody");
                    tableBody.empty();
                    var totalItems = response.paginationInfo.totalItems;

                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            tableBody.append(`
                                <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input addShift-selectItem" data-id="${item.addShiftID}" />
                                    </td>
                                    <td class="shiftName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-2">
                                        <h5>${item.shiftName}</h5>
                                    </td>
                                    <td class="companyName align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
                                        <span>${item.organizationName}</span>
                                    </td>
                                    <td class="startTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.startTime}</td>
                                    <td class="endTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.endTime}</td>
                                    <td class="graceTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.graceTime}</td>
                                    <td class="breakTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mealBreakTime}</td>
                                    <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                                        <div class="btn-reveal-trigger position-static">
                                            <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#edit_shift">
                                                <i class="fas fa-edit text-success"></i>
                                            </a>
                                            <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal" id="addShift-singleDelBtn" data-id="${item.addShiftID}">
                                                <i class="fas fa-trash text-danger"></i>
                                            </a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="8" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#addShift-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#addShift-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#addShift-paginationLinks");
            paginationLinks.empty();
            // Window size (number of pages before/after the current page)
            const windowSize = 1;

            const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;

            // Helper function for ellipsis
            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
            // Add "First Page" and ellipsis if needed
            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }
            // Add page number buttons within the window range
            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }
            // Add ellipsis and "Last Page" button if needed
            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }
            // Disable or enable previous/next buttons
            $("#addShift-prevPageBtn").prop('disabled', currentPage === 1);
            $("#addShift-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));

