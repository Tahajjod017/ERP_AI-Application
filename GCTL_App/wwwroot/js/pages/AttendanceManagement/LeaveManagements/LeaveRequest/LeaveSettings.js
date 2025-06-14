$(document).ready(function () {

    $('#addNewLeaveForm').on('submit', function (e) {
        e.preventDefault();
        if (!validateLeaveForm()) return;
        var formData = {
            OrganizationID: $('#OrganizationID').val(),
            LeaveTypeName: $('#LeaveTypeName').val(),
            IsPaid: $('input[name="IsPaid"]:checked').val() === 'true',
            LeaveDays: $('#LeaveDays').val(),
            Code: $('#Code').val(),
            EffectiveFrom: $('#EffectiveFrom').val(),
            EffectiveFromMonthYear: $('#EffectiveFromMonthYear').val(),
            EffectiveAfter: $('#EffectiveAfter').val()
        };

        $.ajax({
            url: '/LeaveSettings/AddNewLeave', 
            type: 'POST',
            data: formData,
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success(response.message);
                    loadLeaveTypeCard();
                    resetForm();
                } else {
                    // Show server-side validation errors
                    if (response.errors && response.errors.length > 0)
                    {
                        response.errors.forEach(function (error)
                        {
                            toastr.error(error);
                        });

                    } else
                    {
                        toastr.error(response.message);
                    }
                }
            },
            error: function () {
                toastr.error("An unexpected error occurred.");
            }
        });
    });

    function resetForm()
    {
        $('#LeaveTypeName, #LeaveDays, #Code, #EffectiveFrom').val('');
        $('#IsApidPaid').prop('checked', true);

        // Reset dropdowns
        $('#OrganizationID').val([]).trigger('change');
        $('#EffectiveFromMonthYear').val('Months').trigger('change');
        $('#EffectiveAfter').val('Joining Date').trigger('change');

        // Clear validation styles and messages
        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();

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
    
    loadLeaveTypeCard();
    //
    function loadLeaveTypeCard() {
        $.ajax({
            url: '/LeaveSettingsRoute/GetAllLeaveTypesAsync',
            type: 'GET',
            success: function (response) {
                console.log("Datassssss", response);
                var container = $("#leaveTypesContainer");
                container.empty();
              
                if (response.length > 0) {
                    response.forEach(function (item, index) {
                        container.append(`
                        <div class="col-xl-4 col-md-6">
                            <div class="card mb-3">
                                <div class="card-body d-flex align-items-center justify-content-between">
                                    <div class="d-flex align-items-center">
                                        <div class="form-check form-check-md form-switch me-1">
                                            <label class="form-check-label">
                                              <input class="form-check-input" type="checkbox" role="switch" ${item.isActive ? 'checked' : ''}>

                                            </label>
                                        </div>
                                        <h6 class="d-flex align-items-center">${item.leaveTypeName}</h6>
                                    </div>
                                    <div class="d-flex align-items-center">
                                        <a class="text-decoration-none" data-bs-toggle="modal" data-id="${item.leaveTypeID}" data-bs-target="#annual_leave_settings">
                                            <span class="text-primary" data-feather="settings" style="height: 15px; width: 15px;"></span>
                                        </a>
                                    </div>
                                </div>
                            </div>
                        </div>
                    `);
                    });

                    feather.replace(); // Important to re-render icons
                } else {
                    container.append('<div class="col-12 text-center">No leave types found.</div>');
                }
            },
            error: function () {
                toastr.error('Failed to fetch leave types.');
            }
        });
    }

    //

    $(document).on('click', 'a[data-bs-target="#annual_leave_settings"]', function (e) {
        e.preventDefault();
        const leaveTypeID = $(this).data("id");

        $.ajax({
            url: '/LeaveSettingsRoute/GetLeaveTypesDataByID',
            type: 'GET',
            data: { leaveTypeID: leaveTypeID },
            success: function (response) {
                if (response && response.leaveTypeID !== 0) {

                    $('#LeaveNameTitle').text(response.leaveTypeName +' Settings');
                    // Populate fields in modal
                    $('#annual_leave_settings select#leaveNameDropdown').val(response.leaveTypeName);
                    $('#annual_leave_settings input#leaveCode').val(response.code);
                    $('#annual_leave_settings input#leaveDays').val(response.leaveDays);
                    $('#annual_leave_settings input#effectiveFrom').val(response.effectiveFrom);
                    $('#annual_leave_settings select#monthYear').val(response.effectiveFromMonthYear);
                    $('#annual_leave_settings select#afterType').val(response.effectiveAfter);
                    $('#annual_leave_settings input#minEncash').val(response.minimumDaysRequiredEncashement);
                    $('#annual_leave_settings input#maxEncash').val(response.maximumDaysAllowedEncashement);
                    // Checkbox example
                    $('#annual_leave_settings input#isApid').prop('checked', response.isApid);

                    if (response.leaveTypeName && response.leaveTypeName.toLowerCase() === "annual leave") {
                        $('#toggleEncashementCheckbox').prop('checked', true);
                        $('#encashmentCheckboxContainer').show(); // Show checkbox div
                        $('#hiddenEncashmentDiv').show(); // Show encashment fields
                    } else {
                        $('#toggleEncashementCheckbox').prop('checked', false);
                        $('#encashmentCheckboxContainer').hide(); // Hide checkbox div
                        $('#hiddenEncashmentDiv').hide(); // Hide encashment fields
                    }
                  
                } else {
                    toastr.error(response.message || "Data not found.");
                }
            },
            error: function () {
                toastr.error("Error loading leave type data.");
            }
        });
    });

   
    //
});


