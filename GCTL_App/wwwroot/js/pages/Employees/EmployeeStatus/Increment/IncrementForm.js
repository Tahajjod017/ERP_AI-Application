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


    //#region Label Update on Radio Change

    updateIncrementLabel();

    $("input[name='changeType2']").on("change", function () {
        updateIncrementLabel();
        updateFromAmount(); // recalculate on change
    });

    function updateIncrementLabel() {
        const type = $("input[name='changeType2']:checked").val();
        const isIncrement = type === "increment";

        const labelText = isIncrement ? "Increment Amount" : "Decrement Amount";
        const labelPercent = isIncrement ? "Increment Percent" : "Decrement Percent";

        $("label[for='incrementAmt']").text(labelText);
        $("label[for='incrementPercent']").text(labelPercent);
    }

    //#endregion

    //#region Amount / Percent / Salary Live Sync

    $("#incrementAmt").on("input", updateFromAmount);
    $("#incrementPercent").on("input", updateFromPercent);
    $("#incrementNewSalary").on("input", updateFromNewSalary);

    function getCurrentSalary() {
        return parseFloat($("#incrementPrevSalary").val()) || 0;
    }

    function isIncrementMode() {
        return $("input[name='changeType2']:checked").val() === "increment";
    }

    function updateFromAmount() {
        const current = getCurrentSalary();
        const amount = parseFloat($("#incrementAmt").val()) || 0;
        const sign = isIncrementMode() ? 1 : -1;
        const newSalary = current + (amount * sign);
        const percent = (amount / current) * 100;

        $("#incrementNewSalary").val(newSalary.toFixed(2));
        $("#incrementPercent").val(percent.toFixed(2));
    }

    function updateFromPercent() {
        const current = getCurrentSalary();
        const percent = parseFloat($("#incrementPercent").val()) || 0;
        const sign = isIncrementMode() ? 1 : -1;
        const amount = (current * percent) / 100;
        const newSalary = current + (amount * sign);

        $("#incrementAmt").val(amount.toFixed(2));
        $("#incrementNewSalary").val(newSalary.toFixed(2));
    }

    function updateFromNewSalary() {
        const current = getCurrentSalary();
        const newSalary = parseFloat($("#incrementNewSalary").val()) || 0;

        if (!current || !newSalary) return;

        const isInc = isIncrementMode();
        const isValid =
            (isInc && newSalary >= current) ||
            (!isInc && newSalary <= current);

        if (!isValid) {
            $("#incrementAmt").val("");
            $("#incrementPercent").val("");
            return;
        }

        const amount = Math.abs(newSalary - current);
        const percent = (amount / current) * 100;

        $("#incrementAmt").val(amount.toFixed(2));
        $("#incrementPercent").val(percent.toFixed(2));
    }

    //#endregion



})