$(document).ready(function () {

    //#region employeeChoices with onchange

    let employeeChoices;
    function initEmployeeChoices() {
        employeeChoices = new Choices('#EmployeePersonalId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee'
        });

        // Add event listener for employee selection change
        const employeeElement = document.getElementById('EmployeePersonalId');
        if (employeeElement) {
            employeeElement.addEventListener('change', function (e) {
                const selectedEmployeeId = e.detail.value || e.target.value;
                if (selectedEmployeeId && selectedEmployeeId !== '') {
                    loadEmployeeBenifitData(selectedEmployeeId);
                } else {
                    clearForm();
                }
            });
        }
    }
    document.addEventListener('DOMContentLoaded', initEmployeeChoices);
    initEmployeeChoices();

    //#endregion

    //#region loadEmployeeBenifitData

    function loadEmployeeBenifitData(employeeId) {
        $.ajax({
            url: `/EmployeeBenifit/GetEmployeeBenifitData?employeeId=${employeeId}`,
            type: 'GET',
            success: function (data) {
                if (data) {
                    populateForm(data);
                } else {
                    clearForm();
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading employee benefit data:', error);
                clearForm();
            }
        });
    }

    //#endregion

    //#region populateForm
    function populateForm(data) {
        $('#EmployeePersonalId').val(data.EmployeePersonalId);
        $('#HealthInsurance').val(data.HealthInsurance);
        $('#RetirementPlan').val(data.RetirementPlan);
        $('#PaidTimeOff').val(data.PaidTimeOff);
        $('#OtherBenefits').val(data.OtherBenefits);
        $('#EmployeeBenifitId').val(data.EmployeeBenifitId);
        $('#CreatedBy').val(data.CreatedBy);
        $('#CreatedDate').val(data.CreatedDate);
        $('#ModifiedBy').val(data.ModifiedBy);
        $('#ModifiedDate').val(data.ModifiedDate);
        $('#IsActive').prop('checked', data.IsActive);

    }
    //#endregion

    //#region clearForm
    function clearForm() {
        $('#EmployeePersonalId').val('');
        $('#HealthInsurance').val('');
        $('#RetirementPlan').val('');
        $('#PaidTimeOff').val('');
        $('#OtherBenefits').val('');
        $('#EmployeeBenifitId').val('');
        $('#CreatedBy').val('');
        $('#CreatedDate').val('');
        $('#ModifiedBy').val('');
        $('#ModifiedDate').val('');
        $('#IsActive').prop('checked', false);
    }

});