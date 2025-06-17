$(document).ready(function () {

    $('#addNewLeaveForm').on('submit', function (e) {
        
        e.preventDefault();
        if (!validateLeaveForm()) return;
        var formData = {
            OrganizationID: $('#OrganizationID').val(),
            LeaveTypeName: $('#LeaveTypeName').val(),
            IsPaid: $('input[name="IsPaid"]:checked').val() === 'true',
            IsActive: $('input[name="IsActive"]:checked').val()==='true',
            LeaveDays: $('#LeaveDays').val(),
            Code: $('#Code').val(),
            EffectiveFrom: $('#EffectiveFrom').val(),
            EffectiveFromMonthYear: $('#EffectiveFromMonthYear').val(),
            EffectiveAfter: $('#EffectiveAfter').val()
        };

        $.ajax({
            url: '/LeaveSettingsAddLeaveRoute/AddNewLeave', 
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
        $('#IsPaidYes').prop('checked', true);    
        $('#IsActiveYes').prop('checked', true);

        // Reset dropdowns
        $('#OrganizationID').val([]).trigger('change');
        $('#EffectiveFromMonthYear').val('Months').trigger('change');
        $('#EffectiveAfter').val('After Joining Date');

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
    //<a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0"
    //    href="#!"
    //    id="leaveDelete-singleDelBtn"
    //    data-id="${item.leaveTypeID}">
    //    <span class="fas fa-trash"></span>
    //</a>
    function loadLeaveTypeCard() {
        $.ajax({
            url: '/LeaveSettingsRoute/GetAllLeaveTypesAsync',
            type: 'GET',
            success: function (response) {
                console.log("Datassssss", response);
                var container = $("#leaveTypesContainer");
                container.empty();
                const protectedKeywords = [
                    "lop",
                    "paternity",
                    "casual",
                    "sick",
                    "annual",
                    "maternity"
                ];
                if (response.length > 0) {
                    response.forEach(function (item, index) {
                        //
                        const leaveName = (item.leaveTypeName || "").trim().toLowerCase();
                       
                        // Check if any protected keyword is included in the leave name
                        const hideDelete = protectedKeywords.some(keyword => leaveName.includes(keyword));
                        //
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
                                        <a class="text-decoration-none" title="Edit" data-bs-toggle="modal" data-id="${item.leaveTypeID}" data-bs-target="#annual_leave_settings">
                                            <span class="text-primary" data-feather="settings" style="height: 15px; width: 15px;"></span>
                                        </a>
                                         ${!hideDelete ? `
                                          <a href="#!" title="Delete" class="nav-item mx-2 assignDefaultShift-bulkDelete" id="leaveDelete-singleDelBtn" data-id="${item.leaveTypeID}">
                                        <i class="far fa-trash-alt text-black"></i>
                                        </a>
                                           ` : ''
                                        }
                                       
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
            
                console.log("Get By Data", response);
                if (response && response.leaveTypeID !== 0) {

                    $('#LeaveTypeIDEdit').val(response.leaveTypeID);
                    $('#LeaveNameTitle').text(response.leaveTypeName +' Settings');
                    // Populate fields in modal
                    $('#LeaveTypeNameEdit').val(response.leaveTypeName);
                    $('#CodeEdit').val(response.code);
                    $('#LeaveDaysEdit').val(response.leaveDays);
                    $('#EffectiveFromEdit').val(response.effectiveFrom);
                    //$('#EffectiveFromMonthYear').val(response.effectiveFromMonthYear);
                    choiceManager.setChoiceValue('EffectiveFromMonthYearEdit', response.effectiveFromMonthYear);
                    $('#EffectiveAfterEdit').val(response.effectiveAfter || 'After Joining Date');
                    $('#minEncash').val(response.minimumDaysRequiredEncashement);
                    $('#maxEncash').val(response.maximumDaysAllowedEncashement);
                    // Checkbox example
                    $('input[name="IsPaidEdit"][value="' + response.isPaid + '"]').prop('checked', true);



                    if (response.leaveTypeName === "Annual Leave") {
                        $('#encashmentSection').show();
                    } else {
                        $('#encashmentSection').hide();
                        $('#toggleEncashementCheckbox').prop('checked', false);
                        $('#hiddenEncashmentDiv').hide();
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

    //Update Leave Data

    $('#updateLeaveBtn').on('click', function (e) {
        e.preventDefault();
        
        const leaveData = {
            //LeaveTypeID: $('#LeaveTypeID').val(),
            //LeaveTypeName: $('#LeaveTypeName').val(),
            ////OrganizationID: $('#OrganizationID').val(),
            //IsPaid: $('input[name="IsPaid"]:checked').val() === 'true',
            //IsActive: $('input[name="IsActive"]:checked').val() === 'true',
            //LeaveDays: $('#LeaveDays').val(),
            //Code: $('#Code').val(),
            //EffectiveFrom: $('#EffectiveFrom').val(),
            //EffectiveFromMonthYear: $('#EffectiveFromMonthYear').val(),
            //EffectiveAfter: $('#EffectiveAfter').val(),
            //MinimumDaysRequiredEncashement: $('#minEncash').val(),
            //MaximumDaysAllowedEncashement: $('#maxEncash').val()

            LeaveTypeID: $('#LeaveTypeIDEdit').val(),
            LeaveTypeName: $('#LeaveTypeNameEdit').val(),
            IsPaid: $('input[name="IsPaidEdit"]:checked').val() === 'true',
            IsActive: $('input[name="IsActiveEdit"]:checked').val() === 'true',
            LeaveDays: $('#LeaveDaysEdit').val(),
            Code: $('#CodeEdit').val(),
            EffectiveFrom: $('#EffectiveFromEdit').val(),
            EffectiveFromMonthYear: $('#EffectiveFromMonthYearEdit').val(),
            EffectiveAfter: $('#EffectiveAfterEdit').val(),
            MinimumDaysRequiredEncashement: $('#minEncash').val(),
            MaximumDaysAllowedEncashement: $('#maxEncash').val()
        };
    
        $.ajax({
            url: '/LeaveSettings/UpdateLeave', // Replace with your controller name
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(leaveData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    resetLeaveForm();
                    loadLeaveTypeCard();
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

    function resetLeaveForm() {
        // Clear basic inputs
        //$('#LeaveTypeID').val('');
        //$('#LeaveTypeName').val('');
        //$('#Code').val('');
        //$('#LeaveDays').val('');
        //$('#EffectiveFrom').val('');
        //// $('#EffectiveFromMonthYear').val('');
        //choiceManager.clearChoice('EffectiveFromMonthYear');
        //$('#EffectiveAfter').val('After Joining Date'); // Default readonly value

        //// Reset radio buttons
        //$('input[name="IsPaid"]').prop('checked', true);
        //$('input[name="IsActive"]').prop('checked', true); // if you have this group

        //// Hide encashment section
        //$('#encashmentSection').hide();

        //// Reset encashment fields
        //// Keep checkbox visible, just uncheck and hide the fields
        //$('#toggleEncashementCheckbox').prop('checked', false);
        //$('#hiddenEncashmentDiv').hide(); // only hide encashme
        //$('#minEncash').val('');
        //$('#maxEncash').val('');

        //// Remove validation errors if any
        //$('.is-invalid').removeClass('is-invalid');
        //$('.text-danger').remove();

        $('#LeaveTypeIDEdit').val('');
        $('#LeaveTypeNameEdit').val('');
        $('#CodeEdit').val('');
        $('#LeaveDaysEdit').val('');
        $('#EffectiveFromEdit').val('');
        $('#EffectiveFromMonthYearEdit').val('');
        $('#EffectiveAfterEdit').val('After Joining Date');

        // Reset radio buttons
        $('input[name="IsPaidEdit"]').prop('checked', true);
        $('input[name="IsActiveEdit"]').prop('checked', true);

        // Hide encashment section
        $('#encashmentSection').show();
        $('#toggleEncashementCheckbox').prop('checked', false);
        $('#hiddenEncashmentDiv').hide();
        $('#minEncash').val('');
        $('#maxEncash').val('');

        // Remove validation errors
        $('.is-invalid').removeClass('is-invalid');
        $('.text-danger').remove();
    }

    // Bind to reset button click
    $('#resetBtnUpdated').on('click', function () {
        resetLeaveForm();
    });


    //
    $(document).on('click', '#leaveDelete-singleDelBtn', function () {
        var id = $(this).data('id');
      
        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/LeaveDeleteRoute/LeaveDelete',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.success(response.message);
                            loadLeaveTypeCard();
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
    //
    

    $('#add_new_leave,#annual_leave_settings').on('hidden.bs.modal', function () {
        resetForm();
        resetLeaveForm();
    });
    //
});


