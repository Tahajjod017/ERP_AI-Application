
$(document).ready(function () {

    $("select[name='OrganizationID']").on("changed.coreui.multi-select", function () {
        validateOrganization();
    });

    function validateOrganization() {
        var orgSelect = $("select[name='OrganizationID']");
        var selectedValues = orgSelect.val();
        var errorSpan = $("#OrganizationID-error");
        if (!selectedValues || selectedValues.length === 0) {
            errorSpan.text("Please select an organization.");
            orgSelect.css('border', '1px solid red');
            return false;
        } else {
            errorSpan.text("");
            orgSelect.css('border', '1px solid #ccc');
            return true;
        }
    }

    $(document).on("click", '#PayRollEmpAllowanceSave', function (e) {
            e.preventDefault();
        if (!validateOrganization()) {
            return;
        }
            var formData = new FormData();

            // --- Append values manually (if you want explicit control) ---
            formData.append("OrganizationID", $("[name='OrganizationID']").val());
            formData.append("IsMobileInternetAllowanceEnabled", $("[name='IsMobileInternetAllowanceEnabled']").is(":checked"));
            formData.append("MobileInternetAllowance", $("[name='MobileInternetAllowance']").val());

            formData.append("IsShiftAllowanceEnabled", $("[name='IsShiftAllowanceEnabled']").is(":checked"));
            formData.append("ShiftAllowance", $("[name='ShiftAllowance']").val());

            formData.append("IsHouseRentAllowanceEnabled", $("[name='IsHouseRentAllowanceEnabled']").is(":checked"));
            formData.append("HouseRentAllowanceRate", $("[name='HouseRentAllowanceRate']").val());
            formData.append("HRentDependsOnSalaryTypeID", $("[name='HRentDependsOnSalaryTypeID']").val());

            formData.append("IsMedicalAllowanceEnabled", $("[name='IsMedicalAllowanceEnabled']").is(":checked"));
            formData.append("MedicalAllowanceRate", $("[name='MedicalAllowanceRate']").val());
            formData.append("MediAllowDepOnSalaryTypeID", $("[name='MediAllowDepOnSalaryTypeID']").val());

            formData.append("IsConveyanceAllowanceEnabled", $("[name='IsConveyanceAllowanceEnabled']").is(":checked"));
            formData.append("ConveyanceAllowanceRate", $("[name='ConveyanceAllowanceRate']").val());
            formData.append("ConAllowDepOnSalaryTypeID", $("[name='ConAllowDepOnSalaryTypeID']").val());

            
            $.ajax({
                url: '/PayRollEmployeesAllowance/SavePayRollEmpAlowance',
                type: 'POST',
                data: formData,
                processData: false,  
                contentType: false,
                dataType:'json',
                success: function (response) {
                    debugger
                    if (response.success) {
                        toastr.success(response.message);   
                       resetPayRollEmpAllowanceForm();
                    } else {
                        toastr.error(response.message);    
                    }
                },
                error: function (xhr, status, error) {
                    let msg = "Something went wrong!";
                    if (xhr.responseJSON && xhr.responseJSON.message) {
                        msg = xhr.responseJSON.message;
                    }
                    toastr.error(msg);  
                }

            });
    });


    function resetPayRollEmpAllowanceForm() {
        var form = $("#payrollEmpAllowanceForm");

        // Reset all inputs, selects, checkboxes
        form[0].reset();

        // If you are using bootstrap/coreui "select" with tags/search → reset them manually
        choiceManager.resetChoice('OrganizationID',
            'HouseRentAllowanceRate',
            'HRentDependsOnSalaryTypeID',
            'MedicalAllowanceRate',
            'MediAllowDepOnSalaryTypeID',
            'ConveyanceAllowanceRate',
            'ConAllowDepOnSalaryTypeID')

       
    }

    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetPayRollEmpAllowanceForm();
    })

});

