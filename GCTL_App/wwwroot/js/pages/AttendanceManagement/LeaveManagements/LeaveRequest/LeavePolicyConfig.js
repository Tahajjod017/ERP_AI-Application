$(document).ready(function () {


    initializeDatepickerDMY("LeaveBalanceResetDate");
    function resetForm()
    {
        // Reset radio buttons to default (usually the first option)
        $('input[type=radio][name=IsWeekendCountedAsLeave][value=false]').prop('checked', false);
        $('input[type=radio][name=IsHolidayCountedAsLeave][value=true]').prop('checked', true);
        $('input[type=radio][name=IsExceedLeaveBalance][value=true]').prop('checked', true);

        // Uncheck all checkboxes
        $('#IsAllowRequestForPastDates,#IsRoundOffHour').prop('checked', false);
        $('#IsAllowRequestForFutureDays').prop('checked', false);
        $('#IsMaximumleaveDaysPerAplication').prop('checked', false);
        $('#IsMaximumGapDaysBetweenAplications').prop('checked', false);
       
        // Clear textboxes
        $('#AllowRequestForFutureDays, #MaxLeavePerApplication, #MaxGapBetweenApplications,#WorkingHour,#ShortLeaveMaxInADay').val('');
        choiceManager.clearChoice('RoundOffHour');
        $('#LeavePolicyConfigurationID').val('');
        $('#EnableLeaveBalanceResetDate').prop('checked', false);
        $('#LeaveBalanceResetDate').val('').trigger('change');
        $('input[type=radio][name=IsAllowCrossLeave][value=true]').prop('checked', true);
        $('#IsEmailSendEnabled').prop('checked', false);
        $('#AbsentMultiplier').prop('checked', true);
        initializeDatepickerDMY("LeaveBalanceResetDate");
    }


    $('#resetBtn').on('click', function () {
        resetForm();
    });




    //Update Leave Data

    $('#AddLeavePolicyBtn').on('click', function (e) {
        e.preventDefault();
        // if (!validateLeaveForm()) return;
        if ($('#WorkingHour').val() === '') {
            alert('Please enter a value for Working Hour.');
            return;
        }
       


        const leaveData = {

            IsWeekendCountedAsLeave: $('input[name="IsWeekendCountedAsLeave"]:checked').val() === 'true',
            IsHolidayCountedAsLeave: $('input[name="IsHolidayCountedAsLeave"]:checked').val() === 'true',
            IsExceedLeaveBalance: $('input[name="IsExceedLeaveBalance"]:checked').val() === 'true',

            IsAllowRequestForPastDates: $('#IsAllowRequestForPastDates').is(':checked'),

            IsRoundOffHour: $('#IsRoundOffHour').is(':checked'),
            RoundOffHour: $('#IsRoundOffHour').is(':checked') ? $('#RoundOffHour').val() : null,
            LeavePolicyConfigurationID: parseInt($('#LeavePolicyConfigurationID').val()) || 0,
            AbsentMultiplier: parseFloat($('#AbsentMultiplier').val()) || 0,

            //
            IsAllowRequestForFutureDays: $('#IsAllowRequestForFutureDays').is(':checked'),
            // *Always* read the user's input (default to null if blank/invalid)
            AllowRequestForFutureDays: (function () {
                const v = parseInt($('#AllowRequestForFutureDays').val());
                return isNaN(v) ? null : v;
            })(),

            IsMaximumleaveDaysPerAplication: $('#IsMaximumleaveDaysPerAplication').is(':checked'),
            MaximumleaveDaysPerAplication: (function () {
                const v = parseInt($('#MaxLeavePerApplication').val());
                return isNaN(v) ? null : v;
            })(),

            IsMaximumGapDaysBetweenAplications: $('#IsMaximumGapDaysBetweenAplications').is(':checked'),
            MaximumGapDaysBetweenAplications: (function () {
                const v = parseInt($('#MaxGapBetweenApplications').val());
                return isNaN(v) ? null : v;
            })(),


            EnableLeaveBalanceResetDate: $('#EnableLeaveBalanceResetDate').is(':checked'),
            LeaveBalanceResetDate: $('#LeaveBalanceResetDate').val() || null,

            IsAllowCrossLeave: $('input[name="IsAllowCrossLeave"]:checked').val() === 'true',
        
            //
            WorkingHour: (function () { const val = $('input[name="WorkingHour"]').val(); return val === '' ? null : parseFloat(val); })(),

            ShortLeaveMaxInADay: (function () { const v = parseInt($('#ShortLeaveMaxInADay').val()); return isNaN(v) ? null : v; })(),
            IsEmailSendEnabled: (function () {
                return $('#IsEmailSendEnabled').is(':checked');
            })(),

            
        };
    
       
        $.ajax({
            url: '/LeaveSettings/LeavePolicyConfig', // Replace with your controller name
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(leaveData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    resetForm();
                    GetAll();
                
                    // Optionally reload data or close modal
                } else {
                    toastr.error(response.message);
                    if (response.errors) {
                        response.errors.forEach(error => toastr.warning(error));
                    }
                }
            },
            error: function () {
                toastr.error("An error occurred while sending data.");
            }
        });
    });
    //

    //
    GetAll();
    function GetAll() {
        $.ajax({
            url: '/LeaveSettingsRoute/GetDataLeavePolicy',
            type: 'GET',
            success: function (data) {
                if (data && data.length > 0) {
                    let config = data[0];
                 
                    $(`#IsWeekendCountedAsLeaveYes`).prop('checked', config.isWeekendCountedAsLeave === true);
                    $(`#IsWeekendCountedAsLeaveNo`).prop('checked', config.isWeekendCountedAsLeave === false);

                    $(`#IsHolidayCountedAsLeaveYes`).prop('checked', config.isHolidayCountedAsLeave === true);
                    $(`#IsHolidayCountedAsLeaveNo`).prop('checked', config.isHolidayCountedAsLeave === false);

                    $(`#IsExceedLeaveBalanceYes`).prop('checked', config.isExceedLeaveBalance === true);
                    $(`#IsExceedLeaveBalanceNo`).prop('checked', config.isExceedLeaveBalance === false);

                    // Checkboxes
                    $('#IsAllowRequestForPastDates').prop('checked', config.isAllowRequestForPastDates === true);
                    $('#IsAllowRequestForFutureDays').prop('checked', config.isAllowRequestForFutureDays === true);
                    $('#IsMaximumleaveDaysPerAplication').prop('checked', config.isMaximumleaveDaysPerAplication === true);
                    $('#IsMaximumGapDaysBetweenAplications').prop('checked', config.isMaximumGapDaysBetweenAplications === true);
                    $('#IsRoundOffHour').prop('checked', config.isRoundOffHour === true);

                    // Textboxes
                    $('#AllowRequestForFutureDays').val(config.allowRequestForFutureDays);
                    $('#MaxLeavePerApplication').val(config.maximumleaveDaysPerAplication);
                    $('#MaxGapBetweenApplications').val(config.maximumGapDaysBetweenAplications);
                    $('#AbsentMultiplier').val(config.absentMultiplier);
                    // Dropdown
                    $('#LeavePolicyConfigurationID').val(config.leavePolicyConfigurationID);
                    choiceManager.setChoiceValue('RoundOffHour', config.roundOffHour);

                    $('#EnableLeaveBalanceResetDate').prop('checked', config.enableLeaveBalanceResetDate === true);
                    if ($('#LeaveBalanceResetDate')[0]._flatpickr) {
                        $('#LeaveBalanceResetDate')[0]._flatpickr.setDate(config.leaveBalanceResetDate, true);
                    }

                    $('input[name="IsAllowCrossLeave"][value="' + config.isAllowCrossLeave + '"]').prop('checked', true);

                    $('#WorkingHour').val(config.workingHour);
                    $('#ShortLeaveMaxInADay').val(config.shortLeaveMaxInADay);
                    $('input[name="IsEmailSendEnabled"]').prop('checked', config.isEmailSendEnabled === true);

                    $('#IsSickLeaveDocumentRequired').prop('checked', config.isSickLeaveDocumentRequired === true);
                    $('#SickLeaveDocumentWithinDays').val(config.sickLeaveDocumentWithinDays);
                }
            },
            error: function () {
                alert('Error retrieving leave policy configuration.');
            }
        });
    }


    //


});


