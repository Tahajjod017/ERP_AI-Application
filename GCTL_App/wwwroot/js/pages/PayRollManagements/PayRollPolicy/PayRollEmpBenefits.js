$(document).ready(function () {

   

    $("#PayRollEmpSave").on("click", function (e) {
        e.preventDefault();

        var formData = new FormData();

        // ====== Dropdowns / Inputs ======
        formData.append("OrganizationID", $("select[name='OrganizationID']").val());
        formData.append("HealthInsurance", $("input[name='HealthInsurance']").val());
        formData.append("PerformanceBonus", $("input[name='PerformanceBonus']").val());
        formData.append("FastivalBonusRate", $("select[name='FastivalBonusRate']").val());
        formData.append("FastivalBonusOnSalaryTypeID", $("select[name='FastivalBonusOnSalaryTypeID']").val());
        formData.append("YearlyEndBonusTypeID", $("select[name='YearlyEndBonusTypeID']").val());
        formData.append("ProvidentFundEmployeeContrebution", $("select[name='ProvidentFundEmployeeContrebution']").val());
        formData.append("ProvidentFundOrganizationContrebution", $("select[name='ProvidentFundOrganizationContrebution']").val());
        formData.append("ProvidentFundOnSalaryTypeID", $("select[name='ProvidentFundOnSalaryTypeID']").val());
        formData.append("ProvidentFundMinimumServiceYear", $("select[name='ProvidentFundMinimumServiceYear']").val());

        // ====== Checkboxes (bool?) ======
        formData.append("IsHealthInsuranceEnabled", $("#IsHealthInsuranceEnabled").is(":checked"));
        formData.append("IsPerformanceBonusEnabled", $("#IsPerformanceBonusEnabled").is(":checked"));
        formData.append("IsFastivalBonusEnabled", $("#IsFastivalBonusEnabled").is(":checked"));
        formData.append("IsProvidentFundEnabled", $("#IsProvidentFundEnabled").is(":checked"));
        formData.append("IsYearEndBonusEnabled", $("#IsYearEndBonusEnabled").is(":checked"));
        $.ajax({
            url: '/EmployeeBenefits/Create',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message); // "Save Successfully"
                    resetPayRollForm();
                } else {
                    toastr.error(res.message); 
                }
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                toastr.error(res.message); 
            }
        });
    });

    // for RESET Button

    function resetPayRollForm() {

        const branchSelect = document.getElementById('OrganizationID');
        const branchInstance = coreui.MultiSelect.getInstance(branchSelect);
        if (branchInstance) {
            branchInstance.deselectAll();
        }

        choiceManager.resetChoice('FastivalBonusRate', 'FastivalBonusOnSalaryTypeID', 'YearlyEndBonusTypeID', 'ProvidentFundOnSalaryTypeID', 'ProvidentFundOrganizationContrebution','ProvidentFundOnSalaryTypeID','ProvidentFundMinimumServiceYear')
        // Reset all input fields
        $("input[name='HealthInsurance']").val("");
        $("input[name='PerformanceBonus']").val("");

        // Reset all checkboxes to unchecked
        $("#IsHealthInsuranceEnabled").prop('checked', false);
        $("#IsPerformanceBonusEnabled").prop('checked', false);
        $("#IsFastivalBonusEnabled").prop('checked', false);
        $("#IsProvidentFundEnabled").prop('checked', false);
        $("#IsYearEndBonusEnabled").prop('checked', false);
        console.log("Form reset successfully");
    }

    $("#ResetBtn").on("click", function (e) {
        e.preventDefault();
        resetPayRollForm();
        toastr.info("Form has been reset");
    });




});
