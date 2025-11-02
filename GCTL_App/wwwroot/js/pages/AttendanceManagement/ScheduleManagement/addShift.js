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


            //function validateCoreUIMultiselect($select) {
            //    const value = $select.val();
            //    const fieldName = $select.attr('name');
            //    const $error = $('[asp-validation-for="' + fieldName + '"]');
            //    const $coreUIWrapper = $select.next('.form-multi-select.coreUiDD');

            //    if (!value || value.length === 0) {
            //        $coreUIWrapper.css({
            //            borderColor: '#FF0000',
            //            outline: '0',
            //            borderWidth: '1px',
            //            borderStyle: 'solid',
            //            borderRadius: '0.375rem',
            //        });
            //        $error.text('This field is required.').show();
            //        return false;
            //    } else {
            //        // Clear styles and error if valid
            //        $coreUIWrapper.css({
            //            borderColor: '',
            //            outline: '',
            //            borderWidth: '',
            //            borderStyle: '',
            //            borderRadius: '',
            //        });
            //        $error.text('').hide();
            //        return true;
            //    }
            //}




            //$(settings.addform).on('submit', function () {
            //    $(this).find('input[required], select[required], textarea[required]').each(function () {
            //        const $field = $(this);

            //        if (!$field.valid()) {
            //            if ($field.hasClass('coreUiDD')) {
            //                // Add CoreUI invalid styles + show error
            //                validateCoreUIMultiselect($field);
            //                // Don't add 'is-invalid' class on the hidden select itself
            //            } else {
            //                // Normal inputs just get the bootstrap class
            //                $field.addClass('is-invalid');
            //            }
            //        } else {
            //            // On valid, remove all invalid indicators (both CoreUI and normal)
            //            if ($field.hasClass('coreUiDD')) {
            //                clearCoreUIMultiselectValidation($field);
            //            } else {
            //                $field.removeClass('is-invalid');
            //            }
            //        }
            //    });
            //});


            //$('input[required], select[required], textarea[required]').on('keyup change', function () {
            //    const $field = $(this);

            //    if ($field.hasClass('coreUiDD')) {
            //        validateCoreUIMultiselect($field);
            //    } else {
            //        if ($field.valid()) {
            //            $field.removeClass('is-invalid');
            //        } else {
            //            $field.addClass('is-invalid');
            //        }
            //    }
            //});




            //$(settings.addform).on('submit', function () {
            //    $(this).find('input[required], select[required], textarea[required]').each(function () {
            //        if (!$(this).valid()) {
            //            $(this).addClass('is-invalid'); 
            //        } else {
            //            $(this).removeClass('is-invalid'); 
            //        }
            //    });
            //});

            //$('input[required], select[required], textarea[required]').on('keyup change', function () {
            //    if ($(this).valid()) {
            //        $(this).removeClass('is-invalid');
            //    }
            //});


            // #region Save 
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#addShift-Addform input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    ShiftName: $('#ShiftName').val(),
                    OrganizationIDs: $('#OrganizationIDs').val(),
                    StartTime: $('#StartTime').val(),
                    EndTime: $('#EndTime').val(),
                    IsLateCount: $('#IsLateCount').prop('checked'),
                    IsRestrictFlexibleInTime: $('#IsRestrictFlexibleInTime').prop('checked'),
                    EarlyInTimeHour: $('#EarlyInTimeHour').val(),
                    EarlyInTimeMinute: $('#EarlyInTimeMinute').val(),
                    IsRestrictFlexibleOutTime: $('#IsRestrictFlexibleOutTime').prop('checked'),
                    EarlyOutTimeHour: $('#EarlyOutTimeHour').val(),
                    EarlyOutTimeMinute: $('#EarlyOutTimeMinute').val(),
                    IsAutomaticORManualBreakTime: $('#IsAutomaticORManualBreakTime').prop('checked'),
                    IsMealBreakCompulsaryOrComplementaryDeductWithShift: $('input[name=IsMealBreakCompulsaryOrComplementaryDeductWithShift]:checked').val() === "true",
                    IsAllowStartAndEndTime: $('#IsAllowStartAndEndTime').prop('checked'),
                    MealBreakStartTime: $('#MealBreakStartTime').val(),
                    MealBreakEndTime: $('#MealBreakEndTime').val(),
                    IsAllowOvertime: $('#IsAllowOvertime').prop('checked'),
                    GraceTimeHour: $('#GraceTimeHour').val(),
                    GraceTimeMinute: $('#GraceTimeMinute').val(),
                    MinimumWorkingTimeHour: $('#MinimumWorkingTimeHour').val(),
                    MinimumWorkingTimeMinute: $('#MinimumWorkingTimeMinute').val(),
                    MinimumRequiredOvertimeHour: $('#MinimumRequiredOvertimeHour').val(),
                    MinimumRequiredOvertimeMinute: $('#MinimumRequiredOvertimeMinute').val(),
                    MaximumAllowedOvertimeHour: $('#MaximumAllowedOvertimeHour').val(),
                    MaximumAllowedOvertimeMinute: $('#MaximumAllowedOvertimeMinute').val(),
                    MealBreakTimeHour: $('#MealBreakTimeHour').val(),
                    MealBreakTimeMinute: $('#MealBreakTimeMinute').val(),
                }

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
                    beforeSend: function () {
                        showLoadingIndicator();
                    },
                    success: function (response) {
                        const allFields = ["OrganizationIDs", "ShiftName", "StartTime", "EndTime"];

                        allFields.forEach(function (fieldId) {
                            validateField(fieldId, response);
                        });

                        if (response.isSuccess) {
                            clear();
                            toastr.success(response.message);
                        } else {
                            toastr.info(response.message);
                        }
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    },
                    error: function (err) {
                        console.log(err);
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    },
                    complete: function () {
                        hideLoadingIndicator();
                    }
                });
            });
            // #endregion


            // #region modal update btn on click
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
                    UpdateIsRestrictFlexibleInTime: $('#UpdateIsRestrictFlexibleInTime').prop('checked'),
                    UpdateIsRestrictFlexibleOutTime: $('#UpdateIsRestrictFlexibleOutTime').prop('checked'),
                    UpdatePunchCountFrom: $('#UpdatePunchCountFrom').val(),
                    UpdateIsAutomaticORManualBreakTime: $('#UpdateIsAutomaticORManualBreakTime').prop('checked'),
                    UpdateIsMBCompulsaryOrComplementaryDeductWithShift: $('input[name=UpdateIsMBCompulsaryOrComplementaryDeductWithShift]:checked').val() === "true",
                    UpdateIsAllowStartAndEndTime: $('#UpdateIsAllowStartAndEndTime').prop('checked'),
                    UpdateMealBreakStartTime: $('#UpdateMealBreakStartTime').val(),
                    UpdateMealBreakEndTime: $('#UpdateMealBreakEndTime').val(),
                    UpdateIsAllowOvertime: $('#UpdateIsAllowOvertime').prop('checked'),
                    UpdateGraceTimeHour: $('#UpdateGraceTimeHour').val(),
                    UpdateGraceTimeMinute: $('#UpdateGraceTimeMinute').val(),
                    UpdateMinimumWorkingTimeHour: $('#UpdateMinimumWorkingTimeHour').val(),
                    UpdateMinimumWorkingTimeMinute: $('#UpdateMinimumWorkingTimeMinute').val(),
                    UpdateMinimumRequiredOvertimeHour: $('#UpdateMinimumRequiredOvertimeHour').val(),
                    UpdateMinimumRequiredOvertimeMinute: $('#UpdateMinimumRequiredOvertimeMinute').val(),
                    UpdateMaximumAllowedOvertimeHour: $('#UpdateMaximumAllowedOvertimeHour').val(),
                    UpdateMaximumAllowedOvertimeMinute: $('#UpdateMaximumAllowedOvertimeMinute').val(),
                    UpdateMealBreakTimeHour: $('#UpdateMealBreakTimeHour').val(),
                    UpdateMealBreakTimeMinute: $('#UpdateMealBreakTimeMinute').val(),
                    UpdateEarlyInTimeHour: $('#UpdateEarlyInTimeHour').val(),
                    UpdateEarlyInTimeMinute: $('#UpdateEarlyInTimeMinute').val(),
                    UpdateEarlyOutTimeHour: $('#UpdateEarlyOutTimeHour').val(),
                    UpdateEarlyOutTimeMinute: $('#UpdateEarlyOutTimeMinute').val(),
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
            // #endregion


            // #region Edit 
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

                            $(settings.updateform).find('#UpdateStartTime')[0]._flatpickr.setDate(parseTime12hToDate(data.updateStartTime));
                            $(settings.updateform).find('#UpdateEndTime')[0]._flatpickr.setDate(parseTime12hToDate(data.updateEndTime));
                            $(settings.updateform).find('#UpdateMealBreakStartTime')[0]._flatpickr.setDate(parseTime12hToDate(data.updateMealBreakStartTime));
                            $(settings.updateform).find('#UpdateMealBreakEndTime')[0]._flatpickr.setDate(parseTime12hToDate(data.updateMealBreakEndTime)); 

                            $(settings.updateform).find('#UpdateIsLateCount').prop('checked', data.updateIsLateCount);
                            if ($('#UpdateIsLateCount').is(':checked')) {
                                $('#addShift-UpdateGraceTimeDiv').removeClass('d-none');
                            } else {
                                $('#addShift-UpdateGraceTimeDiv').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateIsRestrictFlexibleInTime').prop('checked', data.updateIsRestrictFlexibleInTime);
                            if ($('#UpdateIsRestrictFlexibleInTime').is(':checked')) {
                                $('#addShift-UpdatePunchCountFromDiv').removeClass('d-none');
                            } else {
                                $('#addShift-UpdatePunchCountFromDiv').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateEarlyInTimeHour').val(data.updateEarlyInTimeHour);
                            $(settings.updateform).find('#UpdateEarlyInTimeMinute').val(data.updateEarlyInTimeMinute);
                            $(settings.updateform).find('#UpdateIsRestrictFlexibleOutTime').prop('checked', data.updateIsRestrictFlexibleOutTime);
                            if ($('#UpdateIsRestrictFlexibleOutTime').is(':checked')) {
                                $('#addShift-UpdatePunchCountOutDiv').removeClass('d-none');
                            } else {
                                $('#addShift-UpdatePunchCountOutDiv').addClass('d-none');
                            }
                            $(settings.updateform).find('#UpdateEarlyOutTimeHour').val(data.updateEarlyOutTimeHour);
                            $(settings.updateform).find('#UpdateEarlyOutTimeMinute').val(data.updateEarlyOutTimeMinute);
                            $(settings.updateform).find('#UpdateIsAutomaticORManualBreakTime').prop('checked', data.updateIsAutomaticORManualBreakTime);
                            if ($('#UpdateIsAutomaticORManualBreakTime').is(':checked')) {
                                $('#addShift-UpdateBreakTimeDiv').removeClass('d-none');
                            } else {
                                $('#addShift-UpdateBreakTimeDiv').addClass('d-none');
                            }
                            $(settings.updateform).find('input[name="UpdateIsMBCompulsaryOrComplementaryDeductWithShift"][value="' + data.updateIsMBCompulsaryOrComplementaryDeductWithShift + '"]').prop('checked', true);
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
                            $(settings.updateform).find('#UpdateGraceTimeHour').val(data.updateGraceTimeHour);
                            $(settings.updateform).find('#UpdateGraceTimeMinute').val(data.updateGraceTimeMinute);
                            $(settings.updateform).find('#UpdateMinimumWorkingTimeHour').val(data.updateMinimumWorkingTimeHour);
                            $(settings.updateform).find('#UpdateMinimumWorkingTimeMinute').val(data.updateMinimumWorkingTimeMinute);
                            $(settings.updateform).find('#UpdateMinimumRequiredOvertimeHour').val(data.updateMinimumRequiredOvertimeHour);
                            $(settings.updateform).find('#UpdateMinimumRequiredOvertimeMinute').val(data.updateMinimumRequiredOvertimeMinute);
                            $(settings.updateform).find('#UpdateMaximumAllowedOvertimeHour').val(data.updateMaximumAllowedOvertimeHour);
                            $(settings.updateform).find('#UpdateMaximumAllowedOvertimeMinute').val(data.updateMaximumAllowedOvertimeMinute);
                            $(settings.updateform).find('#UpdateMealBreakTimeHour').val(data.updateMealBreakTimeHour);
                            $(settings.updateform).find('#UpdateMealBreakTimeMinute').val(data.updateMealBreakTimeMinute);
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });
            // #endregion


            // #region Delete
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
            // #endregion


            // #region Clear
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
                $('.coreUiDD').css({
                    'border': '1px solid #ccc',
                    'border-radius': '7px'
                });
                loadTableData();
                toggleBulkActions();
                $('#addShift-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            // #region Clear up on Uncheck
            $('#IsLateCount').on('change', function (e) {
                e.preventDefault();

                clearLateCount();
            });

            function clearLateCount() {
                if ($('#IsLateCount').is(':checked')) {
                    $('#addShift-GraceTimeDiv').removeClass('d-none');
                } else {
                    $('#addShift-GraceTimeDiv').addClass('d-none');
                    $('#GraceTimeHour').val('');
                    $('#GraceTimeMinute').val('');
                }
            }

            $('#IsRestrictFlexibleInTime').on('change', function () {
                clearRestrictInTime();
            });

            function clearRestrictInTime() {
                
                if ($('#IsRestrictFlexibleInTime').is(':checked')) {
                    $('#addShift-PunchCountFromDiv').removeClass('d-none');
                } else {
                    $('#addShift-PunchCountFromDiv').addClass('d-none');
                    $('#EarlyInTimeHour').val('');
                    $('#EarlyInTimeMinute').val('');
                }
            }

            $('#IsRestrictFlexibleOutTime').on('change', function () {
                clearRestrictOutTime();
            });

            function clearRestrictOutTime() {

                if ($('#IsRestrictFlexibleOutTime').is(':checked')) {
                    $('#addShift-PunchCountOutDiv').removeClass('d-none');
                    $('#EarlyOutTimeHour').val('');
                    $('#EarlyOutTimeMinute').val('');
                } else {
                    $('#addShift-PunchCountOutDiv').addClass('d-none');
                }
            }

            $('#IsAutomaticORManualBreakTime').on('change', function () {
                clearBreakTime();
            });

            function clearBreakTime() {
                
                if ($('#IsAutomaticORManualBreakTime').is(':checked')) {
                    $('#addShift-BreakTimeDiv').removeClass('d-none');
                } else {
                    $('#addShift-BreakTimeDiv').addClass('d-none');
                    $('#MealBreakTimeHour').val('');
                    $('#MealBreakTimeMinute').val('');
                    $('#addShift-Complementary').prop('checked', true);
                    $('#IsAllowStartAndEndTime').prop('checked', false);
                    clearMealBreakStartEndTime();
                }
            }

            $('#IsAllowStartAndEndTime').on('change', function () {
                clearMealBreakStartEndTime();
            });

            function clearMealBreakStartEndTime() {
                
                if ($('#IsAllowStartAndEndTime').is(':checked')) {
                    $('#addShift-StartEndTimeDiv').removeClass('d-none');
                    $('#addShift-AllowStartEndTime').addClass('d-none');
                    $('#addShift-DenyStartEndTime').removeClass('d-none');
                } else {
                    $('#addShift-StartEndTimeDiv').addClass('d-none');
                    $('#addShift-AllowStartEndTime').removeClass('d-none');
                    $('#addShift-DenyStartEndTime').addClass('d-none');
                    $('#MealBreakStartTime')[0]._flatpickr.clear();
                    $('#MealBreakEndTime')[0]._flatpickr.clear();
                }
            }

            $('#IsAllowOvertime').on('change', function () {
                clearOverTime();
            });

            function clearOverTime() {
                
                if ($('#IsAllowOvertime').is(':checked')) {
                    $('#addShift-OvertimeDiv').removeClass('d-none');
                    $('#addShift-AllowOvertime').addClass('d-none');
                    $('#addShift-DisableOvertime').removeClass('d-none');
                } else {
                    $('#addShift-OvertimeDiv').addClass('d-none');
                    $('#addShift-AllowOvertime').removeClass('d-none');
                    $('#addShift-DisableOvertime').addClass('d-none');
                    $('#MinimumWorkingTimeHour').val('');
                    $('#MinimumWorkingTimeMinute').val('');
                    $('#MinimumRequiredOvertimeHour').val('');
                    $('#MinimumRequiredOvertimeMinute').val('');
                    $('#MaximumAllowedOvertimeHour').val('');
                    $('#MaximumAllowedOvertimeMinute').val('');
                }
            }
            // #endregion


            // #region Clear up on Uncheck in Edit Modal
            $('#editShiftModal').on('hide.bs.modal', function () {
                $(settings.updateform)[0].reset();
                clearUpLateCount();
                clearUpRestrictInTime();
                clearUpRestrictOutTime();
                clearUpBreakTime();
                clearUpMealBreakStartEndTime();
                clearUpOverTime();
            });

            $('#UpdateIsLateCount').on('change', function () {
                clearUpLateCount();
            });

            $('#UpdateIsRestrictFlexibleInTime').on('change', function () {
                clearUpRestrictInTime();
            });

            $('#UpdateIsRestrictFlexibleOutTime').on('change', function () {
                clearUpRestrictOutTime();
            });

            $('#UpdateIsAutomaticORManualBreakTime').on('change', function () {
                clearUpBreakTime();
            });

            $('#UpdateIsAllowStartAndEndTime').on('change', function () {
                clearUpMealBreakStartEndTime();
            });

            $('#UpdateIsAllowOvertime').on('change', function () {
                clearUpOverTime();
            });

            function clearUpLateCount() {
                if (!$('#UpdateIsLateCount').is(':checked')) {
                    $('#UpdateGraceTimeHour').val('');
                    $('#UpdateGraceTimeMinute').val('');
                }
            }

            function clearUpRestrictInTime() {
                if (!$('#UpdateIsRestrictFlexibleInTime').is(':checked')) {
                    $('#UpdateEarlyInTimeHour').val('');
                    $('#UpdateEarlyInTimeMinute').val('');
                }
            }

            function clearUpRestrictOutTime() {
                if (!$('#UpdateIsRestrictFlexibleOutTime').is(':checked')) {
                    $('#UpdateEarlyOutTimeHour').val('');
                    $('#UpdateEarlyOutTimeMinute').val('');
                }
            }

            function clearUpBreakTime() {
                if (!$('#UpdateIsAutomaticORManualBreakTime').is(':checked')) {
                    $('#UpdateMealBreakTimeHour').val('');
                    $('#UpdateMealBreakTimeMinute').val('');
                    $('#UpdateIsAllowStartAndEndTime').prop('checked', false);
                    $('#UpdateMealBreakStartTime')[0]._flatpickr.clear();
                    $('#UpdateMealBreakEndTime')[0]._flatpickr.clear(); 
                }
            }

            function clearUpMealBreakStartEndTime() {
                if (!$('#UpdateIsAllowStartAndEndTime').is(':checked')) {
                    $('#UpdateMealBreakStartTime')[0]._flatpickr.clear();
                    $('#UpdateMealBreakEndTime')[0]._flatpickr.clear(); 
                }
            }

            function clearUpOverTime() {
                if (!$('#UpdateIsAllowOvertime').is(':checked')) {
                    $('#UpdateMinimumWorkingTimeHour').val('');
                    $('#UpdateMinimumWorkingTimeMinute').val('');
                    $('#UpdateMinimumRequiredOvertimeHour').val('');
                    $('#UpdateMinimumRequiredOvertimeMinute').val('');
                    $('#UpdateMaximumAllowedOvertimeHour').val('');
                    $('#UpdateMaximumAllowedOvertimeMinute').val('');
                }
            }
            // #endregion


            // #region Duplicate check
            $(document).ready(function () {
                $('#ShiftName').on('input', function () {
                    checkNameUnique();
                });

                $('#OrganizationIDs').on('hidden.coreui.multi-select', function () {
                    checkNameUnique();
                    //validateCompany();
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
            // #endregion


            // #region toggleBulkActions
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
            // #endregion

            function parseTime12hToDate(time12h) {
                return flatpickr.parseDate(time12h, "h:i K");
            }
            // #region flatpicker
            $(".timepicker-12hr").flatpickr({
                enableTime: true,       // ✅ Enables time selection (hours & minutes)
                noCalendar: true,       // ✅ Hides the calendar view, showing only the time picker
                dateFormat: "H:i",      // h = 12-hour, H = 24-hour, i = minutes, K = AM/PM in
                altInput: true,         // Creates a hidden input for display
                altFormat: "h:i K",     // Shown to user (12-hour with AM/PM)
                time_24hr: false,        // ✅ Uses 24-hour time format (00:00–23:59 instead of 12-hour AM/PM)
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

            $(".timepicker-24hr").flatpickr({
                enableTime: true,
                noCalendar: true,
                dateFormat: "H:i",
                time_24hr: true,
                disableMobile: true,
                allowInput: true,
                clickOpens: true,
                defaultDate: null,
            });

            //$(".timepicker-12hr").each(function () {
            //    const selected = this._flatpickr.selectedDates[0];
            //    if (selected) {
            //        const timeStr = dayjs(selected).format("h:mm A");
            //        console.log(`${this.id}: ${timeStr}`);
            //    }
            //});
            // #endregion


            // #region Toggols
            $(document).ready(function () {

                $('#UpdateIsLateCount').on('change', function () {
                    $('#addShift-UpdateGraceTimeDiv').toggleClass('d-none', !this.checked);
                });

                $('#UpdateIsRestrictFlexibleInTime').on('change', function () {
                    $('#addShift-UpdatePunchCountFromDiv').toggleClass('d-none', !this.checked);
                });

                $('#UpdateIsRestrictFlexibleOutTime').on('change', function () {
                    $('#addShift-UpdatePunchCountOutDiv').toggleClass('d-none', !this.checked);
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
            // #endregion

            $(settings.modalCloseBtn).on('click', function () {
                $('#editShiftModal').modal('hide');
            });
            $(settings.modalCancelBtn).on('click', function () {
                $('#editShiftModal').modal('hide');
            });


            // #region Dropdowns
            let companyChoice;
            function initcompanyChoice() {
                companyChoice = new Choices('#UpdateOrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select...'
                });
            }
            document.addEventListener('DOMContentLoaded', initcompanyChoice);
            // #endregion


            $('#addShift-dd-search').on('change', function () {
                const selectedValue = $(this).val();
                if (selectedValue) {
                    currentPage = 1;
                    loadTableData(selectedValue);
                } else {
                    loadTableData();
                }
            })



            // #region CoreUI Multiselect with Pagination + Search (fixed)
            //let page = 1;
            //let term = '';
            //const pageSize = 20;

            //let hasMore = true;
            //let loading = false;
            //let debounce;
            //let scrollPosition = 0;

            //const selectEl = document.getElementById('OrganizationIDs');
            //if (!selectEl) return;

            //const apiUrl = '/AddShift/SearchOrganizations';
            //const ms = coreui.MultiSelect.getOrCreateInstance(selectEl); // CoreUI instance

            //// Attach search handler to the current DOM search input (safe to call multiple times)
            //function ensureSearchHandler() {
            //    const wrapper = selectEl.nextElementSibling;
            //    if (!wrapper) return;
            //    const searchInput = wrapper.querySelector('.form-multi-select-search');
            //    const box = wrapper.querySelector('.form-multi-select-options');
            //    if (!searchInput) return;
            //    if (searchInput.dataset.listenerAttached) return; // already attached

            //    searchInput.dataset.listenerAttached = '1';

            //    // prevent dropdown from closing when focus/mousedown inside input occurs
            //    searchInput.addEventListener('mousedown', (e) => e.stopPropagation());

            //    // the input handler: debounced server search
            //    searchInput.addEventListener('input', (e) => {
            //        const val = e.target.value.trim();
            //        clearTimeout(debounce);

            //        if (val.length < 3) {
            //            // too short → clear results (but keep selected options)
            //            addOptions([], { reset: true });
            //            page = 1;
            //            term = '';
            //            hasMore = false;
            //            if (box) box.scrollTop = 0;
            //            return;
            //        }

            //        // wait for 1 seconds of no typing before sending request
            //        debounce = setTimeout(() => {
            //            term = val;
            //            page = 1; // reset paging for fresh search
            //            hasMore = true;
            //            fetchPage({ append: false });
            //            if (box) box.scrollTop = 0;
            //        }, 1000); // 1 second delay on search
            //    });
            //}

            //// append <option> nodes to <select>, then refresh CoreUI
            //function addOptions(items, { reset = false } = {}) {
            //    // For remember the scroll position
            //    const wrapper = selectEl.nextElementSibling;
            //    const box = wrapper?.querySelector('.form-multi-select-options');
            //    if (box) {
            //        scrollPosition = box.scrollTop;
            //    }

            //    // keep already selected options so tags remain
            //    if (reset) {
            //        const keep = new Set([...selectEl.options].filter(o => o.selected).map(o => o.value));
            //        [...selectEl.options].forEach(o => { if (!keep.has(o.value)) o.remove(); });
            //    }

            //    // avoid duplicates
            //    const existing = new Set([...selectEl.options].map(o => String(o.value)));
            //    for (const it of (items || [])) {
            //        const v = String(it.value);
            //        if (existing.has(v)) continue;
            //        const opt = document.createElement('option');
            //        opt.value = v;
            //        opt.textContent = it.label;
            //        selectEl.appendChild(opt);
            //    }

            //    // preserve open state + search value while updating
            //    const wrapperBefore = selectEl.nextElementSibling;
            //    const oldSearchInput = wrapperBefore?.querySelector('.form-multi-select-search');
            //    const oldSearchValue = oldSearchInput ? oldSearchInput.value : '';
            //    const oldSelStart = oldSearchInput?.selectionStart;
            //    const oldSelEnd = oldSearchInput?.selectionEnd;

            //    const wasOpen = !!ms._isShown;
            //    ms.update(); // rebuild dropdown UI

            //    if (wasOpen) {
            //        ms.show();
            //    }

            //    // re-attach handlers to the new input and restore text/caret
            //    ensureSearchHandler();

            //    const wrapperAfter = selectEl.nextElementSibling;
            //    const newSearchInput = wrapperAfter?.querySelector('.form-multi-select-search');
            //    if (newSearchInput && oldSearchValue) {
            //        try {
            //            newSearchInput.value = oldSearchValue;
            //            if (typeof oldSelStart === 'number' && typeof oldSelEnd === 'number') {
            //                newSearchInput.setSelectionRange(oldSelStart, oldSelEnd);
            //            }
            //        } catch (err) {
            //            // ignore selection-range errors in some browsers
            //        }
            //    }

            //    // Restore scroll position after a small delay to ensure DOM is ready
            //    setTimeout(() => {
            //        const wrapper = selectEl.nextElementSibling;
            //        const box = wrapper?.querySelector('.form-multi-select-options');
            //        if (box) {
            //            if (reset) {
            //                // new search → always scroll to top
            //                box.scrollTop = 0;
            //                scrollPosition = 0;
            //            } else {
            //                // infinite scroll append → restore position
            //                box.scrollTop = scrollPosition;
            //            }
            //        }
            //    }, 10);


            //    // rebind scroll for the newly-created options container
            //    rebindScroll();
            //}

            //async function fetchPage({ append }) {
            //    if (loading || (!hasMore && append)) return;
            //    loading = true;
            //    try {
            //        const res = await fetch(`${apiUrl}?search=${encodeURIComponent(term)}&page=${page}&pageSize=${pageSize}`);
            //        const data = await res.json();

            //        addOptions(data.items, { reset: !append });
            //        hasMore = !!data.hasMore;

            //        if (append) {
            //            page += 1;
            //        } else {
            //            // we've just loaded page 1 for a fresh search; next scroll should fetch page 2
            //            page = 2;
            //        }
            //    } catch (e) {
            //        console.error(e);
            //    } finally {
            //        loading = false;
            //    }
            //}

            //function rebindScroll() {
            //    const wrapper = selectEl.nextElementSibling;
            //    const box = wrapper?.querySelector('.form-multi-select-options');
            //    if (!box) return;

            //    // If we already attached to this box DOM node, skip
            //    if (box.dataset.infiniteAttached) return;
            //    box.dataset.infiniteAttached = '1';

            //    // prevent dropdown from closing when interacting with the scroll area (optional)
            //    // If you previously needed to stopImmediatePropagation for CoreUI, you can uncomment:
            //    // ['mousedown', 'mouseup', 'click'].forEach(evt => box.addEventListener(evt, e => e.stopImmediatePropagation()));

            //    box.addEventListener('scroll', () => {
            //        if (box.scrollTop + box.clientHeight >= box.scrollHeight - 10) {
            //            if (hasMore && !loading) fetchPage({ append: true });
            //        }
            //    });
            //}

            //// on open, wire search + initial load
            //selectEl.addEventListener('shown.coreui.multi-select', () => {
            //    const wrapper = selectEl.nextElementSibling;
            //    const searchInput = wrapper?.querySelector('.form-multi-select-search');
            //    const box = wrapper?.querySelector('.form-multi-select-options');

            //    // ensure handler on first open too
            //    ensureSearchHandler();

            //    // first open: load first page (empty term)
            //    if (selectEl.options.length === 0) {
            //        page = 1; term = ''; hasMore = true;
            //        fetchPage({ append: false });
            //    }

            //    rebindScroll();
            //});
            // #endregion


        });


        // #region convertUtcTimeOnlyToLocal
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
        // #endregion
        

        // #region loadTableData
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
                                    <td class="startTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.startTime ?? '-'}</td>
                                    <td class="endTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.endTime ?? '-'}</td>
                                    <td class="graceTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.graceTimeHour ?? '-'}</td>
                                    <td class="breakTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mealBreakTimeHour ?? '-'}</td>
                                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mealBreakStartTime ?? '-'}</td>
                                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mealBreakEndTime ?? '-'}</td>
                                    <td class="text-start align-middle white-space-nowrap pe-3">
                                        <div class="d-flex justify-content-end align-items-center">
                                            <a href="#!" class="btn btn-outline-light btn-icon addShift-bulkEdit me-2" id="addShift-editBtn" data-id="${item.shiftID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon addShift-bulkDelete" id="addShift-singleDelBtn" data-id="${item.shiftID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="10" class="text-center">No data available</td></tr>');
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
    // #endregion
}(jQuery));