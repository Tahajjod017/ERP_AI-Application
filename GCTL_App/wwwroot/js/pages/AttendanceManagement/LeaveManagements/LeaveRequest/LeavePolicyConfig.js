$(document).ready(function () {


    function resetForm() {
        // Reset radio buttons to default (usually the first option)
        $('input[type=radio][name=IsWeekendCountedAsLeave][value=true]').prop('checked', true);
        $('input[type=radio][name=IsHolidayCountedAsLeave][value=true]').prop('checked', true);
        $('input[type=radio][name=IsExceedLeaveBalance][value=true]').prop('checked', true);

        // Uncheck all checkboxes
        $('#IsAllowRequestForPastDates,#IsRoundOffHour').prop('checked', false);
        $('#IsAllowRequestForFutureDays').prop('checked', false);
        $('#IsMaximumleaveDaysPerAplication').prop('checked', false);
        $('#IsMaximumGapDaysBetweenAplications').prop('checked', false);

        // Clear textboxes
        $('#AllowRequestForFutureDays, #MaxLeavePerApplication, #MaxGapBetweenApplications').val('');
        choiceManager.clearChoice('RoundOffHour');
    }


    $('#resetBtn').on('click', function () {
        resetForm();
    });

    // Validation

    $('#OrganizationID, #LeaveTypeName, #Code, #LeaveDays').on('input change', function () {
        let value = $(this).val();

        if (Array.isArray(value)) {
            if (value.length > 0) {
                $(this).removeClass('is-invalid');
                $(this).next('.text-danger').remove();
            }
        } else if (typeof value === 'string' && value.trim() !== '') {
            $(this).removeClass('is-invalid');
            $(this).next('.text-danger').remove();
        }
    });

    $('#OrganizationID').on('change', function () {
        const $select = $(this);
        const value = $select.val();

        // Target the CoreUI wrapper (adjust class based on actual CoreUI structure)
        const $visibleWrapper = $select.next('.c-multi-select, .coreui-multiselect');

        if (Array.isArray(value) && value.length > 0) {
            $visibleWrapper.removeClass('is-invalid');
            $visibleWrapper.next('.text-danger').remove();
        }
    });


    //
    function validateLeaveForm() {

        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

        let isValid = true;
        if (!$('#OrganizationID').val() || $('#OrganizationID').val().length === 0) {
            const $select = $('#OrganizationID');
            const $visibleWrapper = $select.next();

            $visibleWrapper.addClass('is-invalid');
            if ($visibleWrapper.next('.text-danger').length === 0) {
                $visibleWrapper.after('<div class="text-danger">This field is required</div>');
            }

            isValid = false;
        }


        //
        if (!$('#LeaveTypeName').val().trim()) {
            $('#LeaveTypeName').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

        if (!$('#Code').val().trim()) {
            $('#Code').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

        if (!$('#LeaveDays').val()) {
            $('#LeaveDays').addClass('is-invalid')
                .after('<div class="text-danger">This field is required</div>');
            isValid = false;
        }

        return isValid;
    }



    //



    //Update Leave Data

    $('#AddLeavePolicyBtn').on('click', function (e) {
        e.preventDefault();
        // if (!validateLeaveForm()) return;
      
        const leaveData = {
            

          

            IsWeekendCountedAsLeave: $('input[name="IsWeekendCountedAsLeave"]:checked').val() === 'true',
            IsHolidayCountedAsLeave: $('input[name="IsHolidayCountedAsLeave"]:checked').val() === 'true',
            IsExceedLeaveBalance: $('input[name="IsExceedLeaveBalance"]:checked').val() === 'true',

            IsAllowRequestForPastDates: $('#IsAllowRequestForPastDates').is(':checked'),

            IsAllowRequestForFutureDays: $('#IsAllowRequestForFutureDays').is(':checked'),
            AllowRequestForFutureDays: $('#IsAllowRequestForFutureDays').is(':checked')
                ? parseInt($('#AllowRequestForFutureDays').val()) || null
                : null,

            IsMaximumleaveDaysPerAplication: $('#IsMaximumleaveDaysPerAplication').is(':checked'),
            MaximumleaveDaysPerAplication: $('#IsMaximumleaveDaysPerAplication').is(':checked')
                ? parseInt($('#MaxLeavePerApplication').val()) || null
                : null,

            IsMaximumGapDaysBetweenAplications: $('#IsMaximumGapDaysBetweenAplications').is(':checked'),
            MaximumGapDaysBetweenAplications: $('#IsMaximumGapDaysBetweenAplications').is(':checked')
                ? parseInt($('#MaxGapBetweenApplications').val()) || null
                : null,

                IsRoundOffHour: $('#IsRoundOffHour').is(':checked'),
            RoundOffHour: $('#IsRoundOffHour').is(':checked')
                ? $('#RoundOffHour').val()
                : null,

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



});


