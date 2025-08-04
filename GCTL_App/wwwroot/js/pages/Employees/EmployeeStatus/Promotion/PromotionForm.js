$(document).ready(function () {
    const developmentMode = true;

    if (developmentMode) {
        toastr.info("Welcome to the Promotion Form!");
    }

    //#region Save

    $("#promotionForm").on("submit", function (e) {
        e.preventDefault();

        const newDesignation = $("#promotionNewDes").val();
        const effectiveDate = $("#promotionWef").val();

        if (!newDesignation || !effectiveDate) {
            toastr.error("New Designation and Effective Date are required.");
            return; // Stop form submission
        }


        const formData = new FormData();
        formData.append("EmployeeID", $("#promotionEmployee").val());
        formData.append("OrganizationID", $("#promotionOrganization").val());
        formData.append("DesignationID", $("#promotionDesignation").val());
        formData.append("DepartmentID", $("#promotionDepartment").val());
        formData.append("ChangeType", $("input[name='changeType']:checked").val());
        formData.append("EffectiveDate", $("#promotionWef").val());
        formData.append("CurrentDesignationID", $("#promotionPrevDes").val());
        formData.append("NewDesignationID", $("#promotionNewDes").val());
        formData.append("CurrentSalary", $("#promotionPrevSalary").val());
        //formData.append("IncrementAmount", $("#incrementAmt").val());
        //formData.append("IncrementPercent", $("#incrementPercent").val());
        formData.append("NewSalary", $("#promotionNewSalary").val());
        formData.append("Remarks", $("#promotionRemark").val());

        $.ajax({
            url: "/Promotion/SavePromotion",
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message || "Promotion saved!");
                    clearPromotionForm();
                    $("#add_promotion").modal("hide");
                    loadPromotionList(); // reload table
                } else {
                    toastr.warning(res.message || "Promotion Not saved!");
                }

                var applyModalEl = document.getElementById('add_promotion');
                var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                if (!applyModal) {
                    applyModal = new bootstrap.Modal(applyModalEl);
                }
                applyModal.hide();
                
            },
            error: function (xhr) {
                toastr.error("Error saving promotion");
            }
        });
    });

    //#endregion Save


    //#region Clear Form

    $("#btnPromotionCancel").on("click", function () {
        clearPromotionForm();
    });

    function clearPromotionForm() {
        $("#promotionForm")[0].reset();
      

        choiceManager.resetChoice('promotionEmployee', 'promotionOrganization', 'promotionDesignation', 'promotionDepartment', 'promotionPrevDes', 'promotionNewDes' );

        $("input[name='changeType']").prop("checked", false);
        $("#promotionWef").val("");
       
        $("#promotionPrevSalary").val("");
        $("#incrementAmt").val("");
        $("#incrementPercent").val("");
        $("#promotionNewSalary").val("");
        $("#promotionRemark").val("");
    }

    //#endregion Clear Form


    //#region Onchange


    $('#promotionEmployee').on('change', function () {
        const employeeId = $(this).val();

        if (employeeId) {
            $.ajax({
                url: '/Increment/GetEmployeeDetails', // Update controller name
                type: 'GET',
                data: { employeeId: employeeId },
                success: function (res) {
                    if (res.success) {
                        choiceManager.setChoiceValue('promotionOrganization', res.data.organizationId);
                        choiceManager.setChoiceValue('promotionDesignation', res.data.designationId);
                        choiceManager.setChoiceValue('promotionPrevDes', res.data.designationId);
                        choiceManager.setChoiceValue('promotionDepartment', res.data.departmentId);
                        
                        $('#promotionPrevSalary').val(res.data.currentSalary);

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


    //#region Radio To label Chng

    updateIncrementLabel();

    $("input[name='changeType']").on("change", function () {
        updateIncrementLabel();
        updateFromAmount();
    });


    function updateIncrementLabel() {
        const type = $("input[name='changeType']:checked").val();
        const isPromotion = type === "promotion";

        const labelText = isPromotion ? "Increment Amount" : "Decrement Amount";
        const labelPercent = isPromotion ? "Increment Percent" : "Decrement Percent";

        $("label[for='incrementAmt']").text(labelText);
        $("label[for='incrementPercent']").text(labelPercent);
    }



    //#endregion

    //#region Amout Dynamic

   


    // Live recalculation
    $("#incrementAmt").on("input", updateFromAmount);
    $("#incrementPercent").on("input", updateFromPercent);
    $("#promotionNewSalary").on("input", updateFromNewSalary);

    function getCurrentSalary() {
        return parseFloat($("#promotionPrevSalary").val()) || 0;
    }

    function isPromotion() {
        return $("input[name='changeType']:checked").val() === "promotion";
    }

    function updateFromAmount() {
        const current = getCurrentSalary();
        const amount = parseFloat($("#incrementAmt").val()) || 0;
        const sign = isPromotion() ? 1 : -1;
        const newSalary = current + (amount * sign);
        const percent = (amount / current) * 100;

        $("#promotionNewSalary").val(newSalary.toFixed(2));
        $("#incrementPercent").val(percent.toFixed(2));
    }

    function updateFromPercent() {
        const current = getCurrentSalary();
        const percent = parseFloat($("#incrementPercent").val()) || 0;
        const sign = isPromotion() ? 1 : -1;
        const amount = (current * percent) / 100;
        const newSalary = current + (amount * sign);

        $("#incrementAmt").val(amount.toFixed(2));
        $("#promotionNewSalary").val(newSalary.toFixed(2));
    }

   


    function updateFromNewSalary() {
        const current = getCurrentSalary();
        const newSalary = parseFloat($("#promotionNewSalary").val()) || 0;

        if (!current || !newSalary) return;

        const isPromo = isPromotion();
        const isValid =
            (isPromo && newSalary >= current) ||
            (!isPromo && newSalary <= current);

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