$(document).ready(function () {
    toastr.info("Increment Form Loaded");


    //#region Form Submit


    $('#incrementForm').on('submit', function (e) {
        e.preventDefault();

        const formData = new FormData();

        formData.append("EmployeeId", $('#incrementEmpId').val());
        formData.append("OrganizationId", $('#incrementOrganization').val());
        formData.append("DesignationId", $('#incrementDesigantion').val());
        formData.append("DepartmentId", $('#incrementDepatment').val());

        formData.append("ChangeType", $('input[name="changeType2"]:checked').val());
        formData.append("EffectiveDate", $('#incrementWef').val());

        formData.append("CurrentSalary", $('#incrementPrevSalary').val());
        formData.append("NewSalary", $('#incrementNewSalary').val());
        formData.append("Remarks", $('#incrementRemark').val());

        $.ajax({
            url: '/Increment/SaveSalaryChange', 
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message ||"Increment saved successfully.");
                    clearIncrementForm()
                } else {
                    toastr.warning(res.message || "Failed to save.");
                }
            },
            error: function (xhr) {
                toastr.error("Error: " + xhr.responseText);
            }
        });
    });



    //#endregion

    //#region Form Reset
   

    function clearIncrementForm() {
       
        choiceManager.resetChoice('incrementEmpId', 'incrementOrganization', 'incrementDesigantion', 'incrementDepatment' );

        $('input[name="changeType2"][value="increment"]').prop('checked', true);

        $('#incrementWef').val('');
        $('#incrementPrevSalary').val('');
        $('#incrementNewSalary').val('');
        $('#incrementRemark').val('');
    }

    // Optional: bind cancel button to clear
    $('#btnIncrementCancel').on('click', function () {
        clearIncrementForm();
    });

    //#endregion

    //#region Onchange

   
        $('#incrementEmpId').on('change', function () {
        const employeeId = $(this).val();

        if (employeeId) {
            $.ajax({
                url: '/Increment/GetEmployeeDetails', // Update controller name
                type: 'GET',
                data: { employeeId: employeeId },
                success: function (res) {
                    if (res.success) {
                        choiceManager.setChoiceValue('incrementOrganization', res.data.organizationId);
                        choiceManager.setChoiceValue('incrementDesigantion', res.data.designationId);
                        choiceManager.setChoiceValue('incrementDepatment', res.data.departmentId);
                        $('#incrementPrevSalary').val(res.data.currentSalary);
                       
                    } else {
                        alert("Employee data not found.");
                    }
                },
                error: function () {
                    alert("Something went wrong while fetching employee data.");
                }
            });
        }
    });
    


    //#endregion

})