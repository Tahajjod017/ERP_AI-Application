(function ($) {
    $.addshift = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#addShift-Addform',
            updateform: '#addShift-Updateform',
            saveBtn: '#addShift-saveBtn',
            editBtn: '#addShift-editBtn',
            resetBtn: '#addShift-resetBtn',
            bulkDelBtn: '#addShift-bulkDelBtn',
            singleDeleteBtn: '#addShift-singleDelBtn',
            modalCloseBtn: '#editShiftModalCloseBtn',
            modalCancelBtn: '#editShiftModalCancelBtn',
            modalUpdateBtn: '#editShiftModalUpdateBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {


            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                /*var formData = new FormData($('#addShift-Addform')[0]);*/

                var token = $('#addShift-Addform input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    ShiftName: $('#ShiftName').val(),
                    OrganizationIDs: $('#OrganizationIDs').val(),
                    StartTime: $('#StartTime').val(),
                    EndTime: $('#EndTime').val(),
                    IsLateCount: $('#IsLateCount').prop('checked'),
                    IsAutomaticORManualBreakTime: $('#IsAutomaticORManualBreakTime').prop('checked'),
                    IsMealBreakCompulsaryOrComplementaryDeductWithShift: $('input[name=IsMealBreakCompulsaryOrComplementaryDeductWithShift]:checked').val() === "true",
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
                validateCompany();

                var id = $(settings.updateform).find('#UpdateShiftID').val();
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
                            clear();
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



            $(settings.modalUpdateBtn).on('click', function (e) {
                e.preventDefault();
                /*var formData = new FormData($('#addShift-Addform')[0]);*/

                var token = $('#addShift-Updateform input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    UpdateShiftID: $('#UpdateShiftID').val(),
                    UpdateShiftName: $('#UpdateShiftName').val(),
                    UpdateOrganizationID: $('#UpdateOrganizationID').val(),
                    UpdateStartTime: $('#UpdateStartTime').val(),
                    UpdateEndTime: $('#UpdateEndTime').val(),
                    UpdateIsLateCount: $('#UpdateIsLateCount').prop('checked'),
                    UpdateIsAutomaticORManualBreakTime: $('#UpdateIsAutomaticORManualBreakTime').prop('checked'),
                    UpdateIsMBCompulsaryOrComplementaryDeductWithShift: $('input[name=UpdateIsMBCompulsaryOrComplementaryDeductWithShift]:checked').val() === "true",
                    UpdateIsAllowStartAndEndTime: $('#UpdateIsAllowStartAndEndTime').prop('checked'),
                    UpdateMealBreakStartTime: $('#UpdateMealBreakStartTime').val(),
                    UpdateMealBreakEndTime: $('#UpdateMealBreakEndTime').val(),
                    UpdateIsAllowOvertime: $('#UpdateIsAllowOvertime').prop('checked'),
                    UpdateGraceTime: $('#UpdateGraceTime').val(),
                    UpdateMinimumWorkingTime: $('#UpdateMinimumWorkingTime').val(),
                    UpdateMinimumRequiredOvertime: $('#UpdateMinimumRequiredOvertime').val(),
                    UpdateMaximumAllowedOvertime: $('#UpdateMaximumAllowedOvertime').val(),
                    UpdateMealBreakTime: $('#UpdateMealBreakTime').val(),
                }

                //validateName();
                //validateCompany();

                var id = $(settings.updateform).find('#UpdateShiftID').val();
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
                            clear();
                            $('#editShiftModal').modal('hide');
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

                            $('#editShiftModal').modal('show');

                            $(settings.updateform).find('#UpdateShiftID').val(data.updateShiftID);
                            $(settings.updateform).find('#UpdateShiftName').val(data.updateShiftName);
                            $(settings.updateform).find('#UpdateOrganizationID').val(data.updateOrganizationID).trigger('change');
                            $(settings.updateform).find('#UpdateStartTime').val(data.updateStartTime);
                            $(settings.updateform).find('#UpdateEndTime').val(data.updateEndTime);
                            $(settings.updateform).find('#UpdateIsLateCount').prop('checked', data.updateIsLateCount);
                            if ($('#UpdateIsLateCount').is(':checked')) {
                                $('#addShift-UpdateGraceTimeDiv').removeClass('d-none');
                            } else {
                                $('#addShift-UpdateGraceTimeDiv').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateIsAutomaticORManualBreakTime').prop('checked', data.updateIsAutomaticORManualBreakTime);
                            if ($('#UpdateIsAutomaticORManualBreakTime').is(':checked')) {
                                $('#addShift-UpdateBreakTimeDiv').removeClass('d-none');
                            } else {
                                $('#addShift-UpdateBreakTimeDiv').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateIsMBCompulsaryOrComplementaryDeductWithShift').val(data.updateIsMBCompulsaryOrComplementaryDeductWithShift);
                            $(settings.updateform).find('#UpdateIsAllowStartAndEndTime').prop('checked', data.updateIsAllowStartAndEndTime);
                            if ($('#UpdateIsAllowStartAndEndTime').is(':checked')) {
                                $('#addShift-UpdateStartEndTimeDiv').removeClass('d-none');
                                $('#addShift-UpdateAllowStartEndTime').addClass('d-none');
                                $('#addShift-UpdateDenyStartEndTime').removeClass('d-none');
                            } else {
                                $('#addShift-UpdateStartEndTimeDiv').addClass('d-none');
                                $('#addShift-UpdateAllowStartEndTime').removeClass('d-none');
                                $('#addShift-UpdateDenyStartEndTime').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateMealBreakStartTime').val(data.updateMealBreakStartTime);
                            $(settings.updateform).find('#UpdateMealBreakEndTime').val(data.updateMealBreakEndTime);
                            $(settings.updateform).find('#UpdateIsAllowOvertime').prop('checked', data.updateIsAllowOvertime);
                            if ($('#UpdateIsAllowOvertime').is(':checked')) {
                                $('#addShift-UpdateOvertimeDiv').removeClass('d-none');
                                $('#addShift-UpdateAllowOvertime').addClass('d-none');
                                $('#addShift-UpdateDisableOvertime').removeClass('d-none');
                            } else {
                                $('#addShift-UpdateOvertimeDiv').addClass('d-none');
                                $('#addShift-UpdateAllowOvertime').removeClass('d-none');
                                $('#addShift-UpdateDisableOvertime').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateGraceTime').val(data.updateGraceTime);
                            $(settings.updateform).find('#UpdateMinimumWorkingTime').val(data.updateMinimumWorkingTime);
                            $(settings.updateform).find('#UpdateMinimumRequiredOvertime').val(data.updateMinimumRequiredOvertime);
                            $(settings.updateform).find('#UpdateMaximumAllowedOvertime').val(data.updateMaximumAllowedOvertime);
                            $(settings.updateform).find('#UpdateMealBreakTime').val(data.updateMealBreakTime);                            

                            $(settings.form).find(settings.saveBtn).text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });




            $(document).on('click', settings.bulkDelBtn, function () {
                var selectedItems = $(".addShift-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
                            method: 'POST',
                            data: { ids: selectedIds },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    clear();
                                } else {
                                    toastr.error(response.message);
                                }
                            },
                            error: function () {
                                toastr.error("Error occurred while deleting.");
                            }
                        });
                    });
                } else {
                    toastr.info("Please select at least one item to delete.");
                }
            });

            $(document).on('click', settings.singleDeleteBtn, function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
                            method: 'POST',
                            data: { ids: [id] },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    clear();
                                } else {
                                    toastr.error(response.message);
                                }
                            },
                            error: function () {
                                toastr.error("Error occurred while deleting.");
                            }
                        });
                    });
                } else {
                    toastr.error("Invalid action.");
                }
            });




            $(settings.resetBtn).on('click', function () {
                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $(settings.updateform)[0].reset();
                $('#ShiftID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
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

            function validateCompany() {
                var selectedOrgs = $('#OrganizationIDs').val();

                if (!selectedOrgs || selectedOrgs.length === 0) {
                    $('.coreUiDD').css({
                        'border': '1px solid red',
                        'border-radius': '7px'
                    });
                } else {
                    $('coreUiDD').css({
                        'border': '1px solid #ccc',
                        'border-radius': ''
                    })
                }
            }


            $(document).ready(function () {
                $('#ShiftName').on('input', function () {
                    checkNameUnique();
                });

                $('#OrganizationIDs').on('hidden.coreui.multi-select', function () {
                    checkNameUnique();
                    validateCompany();
                });                
            });

            function checkNameUnique() {
                var name = $('#ShiftName').val().trim();
                var orgId = $('#OrganizationIDs').val();

                if (!name || orgId === null || orgId.length === 0) {
                    $('#nameError').hide();
                    $('input[name="ShiftName"]').removeClass('is-invalid');
                    return;
                }

                //if (Array.isArray(orgId)) {
                //    orgId = orgId[0];
                //}

                $.ajax({
                    url: uniqueNameUrl,
                    type: 'POST',
                    data: {
                        id: orgId, 
                        name: name
                    },
                    success: function (response) {
                        if (response === true) {
                            $('#nameError').hide();
                            $('input[name="ShiftName"]').removeClass('is-invalid');
                            $('.coreUiDD').removeClass('is-invalid');
                        } else {
                            $('#nameError').text(response).show();
                            $('input[name="ShiftName"]').addClass('is-invalid');
                            $('.coreUiDD').addClass('is-invalid');
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("Error checking name uniqueness:", error);
                    }
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
                    $('#addShift-tBody .addShift-bulkDelete').addClass('disabled');
                    $('#addShift-tBody .addShift-bulkEdit').addClass('disabled');
                } else {
                    $('#addShift-bulkSelectActions').addClass('d-none');
                    $('#addShift-searchBox').removeClass('d-none');
                    $('#addShift-tBody .addShift-bulkDelete').removeClass('disabled');
                    $('#addShift-tBody .addShift-bulkEdit').removeClass('disabled');
                }
            }




            $('#IsLateCount').on('change', function (e) {
                e.preventDefault();

                if ($(this).is(':checked')) {
                    $('#addShift-GraceTimeDiv').removeClass('d-none');
                } else {
                    $('#addShift-GraceTimeDiv').addClass('d-none');
                }
            });

            $('#IsAutomaticORManualBreakTime').on('change', function (e) {
                e.preventDefault();

                if ($(this).is(':checked')) {
                    $('#addShift-BreakTimeDiv').removeClass('d-none');
                } else {
                    $('#addShift-BreakTimeDiv').addClass('d-none');
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




            $(document).ready(function () {
                
                $('#UpdateIsLateCount').on('change', function () {
                    $('#addShift-UpdateGraceTimeDiv').toggleClass('d-none', !this.checked);
                });

                $('#UpdateIsAutomaticORManualBreakTime').on('change', function () {
                    $('#addShift-UpdateBreakTimeDiv').toggleClass('d-none', !this.checked);
                });


                $('#UpdateIsAllowStartAndEndTime').on('change', function (e) {
                    e.preventDefault();

                    if ($(this).is(':checked')) {
                        $('#addShift-UpdateStartEndTimeDiv').removeClass('d-none');
                        $('#addShift-UpdateAllowStartEndTime').addClass('d-none');
                        $('#addShift-UpdateDenyStartEndTime').removeClass('d-none');
                    } else {
                        $('#addShift-UpdateStartEndTimeDiv').addClass('d-none');
                        $('#addShift-UpdateAllowStartEndTime').removeClass('d-none');
                        $('#addShift-UpdateDenyStartEndTime').addClass('d-none');
                    }
                });


                $('#UpdateIsAllowOvertime').on('change', function (e) {
                    e.preventDefault();

                    if ($(this).is(':checked')) {
                        $('#addShift-UpdateOvertimeDiv').removeClass('d-none');
                        $('#addShift-UpdateAllowOvertime').addClass('d-none');
                        $('#addShift-UpdateDisableOvertime').removeClass('d-none');
                    } else {
                        $('#addShift-UpdateOvertimeDiv').addClass('d-none');
                        $('#addShift-UpdateAllowOvertime').removeClass('d-none');
                        $('#addShift-UpdateDisableOvertime').addClass('d-none');
                    }
                });
            });


            $(settings.modalCloseBtn).on('click', function () {
                $('#editShiftModal').modal('hide');
            });
            $(settings.modalCancelBtn).on('click', function () {
                $('#editShiftModal').modal('hide');
            });



            let companyChoice;
            function initcompanyChoice() {
                companyChoice = new Choices('#UpdateOrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select...'
                });
            }
            document.addEventListener('DOMContentLoaded', initcompanyChoice);



            $('#addShift-dd-search').on('change', function () {
                const selectedValue = $(this).val();
                if (selectedValue) {
                    currentPage = 1;
                    loadTableData(selectedValue);
                } else {
                    loadTableData();
                }
            })



            




        });


        function convertUtcTimeOnlyToLocal(timeString) {
            if (!timeString) return "-";

            const [hours, minutes, seconds] = timeString.split(":").map(Number);

            // Treat this time as if it were in UTC on today's date
            const utcDate = new Date(Date.UTC(
                new Date().getFullYear(),
                new Date().getMonth(),
                new Date().getDate(),
                hours,
                minutes,
                seconds || 0
            ));

            // Convert to local time string
            return utcDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        }

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
            var organizationID = $("#addShift-dd-search").val();
            $.ajax({
                url: gridUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    organizationID: organizationID
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
                                        <input type="checkbox" class="form-check-input addShift-selectItem" data-id="${item.shiftID}" />
                                    </td>
                                    <td class="shiftName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-2">
                                        <h5>${item.shiftName}</h5>
                                    </td>
                                    <td class="companyName align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
                                        <span>${item.organizationName}</span>
                                    </td>
                                    <td class="startTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${convertUtcTimeOnlyToLocal(item.startTime ?? '-')}</td>
                                    <td class="endTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${convertUtcTimeOnlyToLocal(item.endTime ?? '-')}</td>
                                    <td class="graceTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.graceTime ?? '-'}</td>
                                    <td class="breakTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mealBreakTime ?? '-'}</td>
                                    <td class="text-end align-middle white-space-nowrap pe-3">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon addShift-bulkEdit me-2" id="addShift-editBtn" data-id="${item.shiftID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon addShift-bulkEdit" id="addShift-singleDelBtn" data-id="${item.shiftID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="8" class="text-center">No data available</td></tr>');
                    }
                    //<td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                    //    <div class="btn-reveal-trigger position-static">
                    //        <a href="#!" class="nav-item mx-2 addShift-bulkEdit" id="addShift-editBtn" data-id="${item.shiftID}"><i class="fas fa-edit text-black"></i></a>
                    //        <a href="#!" class="nav-item mx-2 addShift-bulkDelete" id="addShift-singleDelBtn" data-id="${item.shiftID}"><i class="far fa-trash-alt text-black"></i></a>
                    //    </div>
                    //</td>

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

