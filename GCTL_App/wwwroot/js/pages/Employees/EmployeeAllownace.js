$(document).ready(function () {

    var percentageOptionsHtml = $("#percentageOptionsTemplate").html();
    
    var lastInt = 0

    console.log(percentageOptionsHtml);
    function getLastIntFromUrl() {
        const parts = window.location.pathname.split('/').filter(Boolean).reverse();
        return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
    }

    //#endregion

    $(document).ready(function () {

        lastInt = getLastIntFromUrl();
        if (lastInt) {

            TabChange(lastInt);
        }

        $('#EmployeeId').val(lastInt);
        loadAllowanceTypes(lastInt);



    });
    function getPercentageOptionsHtml(selectedValue) {
        let html = '';
        $("#percentageOptionsTemplate option").each(function () {
            const val = $(this).val();
            const text = $(this).text();
            html += `<option value="${val}" ${val == selectedValue ? 'selected' : ''}>${text}</option>`;
        });
        return html;
    }





    $(document).on("change", ".fixedPercentageSelect", function () {
        let $row = $(this).closest(".row");
        let selected = $(this).val();

        if (selected == "1") { // Fixed
            $row.find(".fixedRateEditable").show().find("input").prop("disabled", false);
            $row.find(".percentRateEditable").hide().find("select").prop("disabled", true);

        } else if (selected == "2") { // Percentage
            $row.find(".fixedRateEditable").hide().find("input").prop("disabled", true);
            $row.find(".percentRateEditable").show().find("select").prop("disabled", false);

        } else {
            $row.find(".fixedRateEditable").hide().find("input").prop("disabled", true);
            $row.find(".percentRateEditable").hide().find("select").prop("disabled", true);
        }
    });


    function loadAllowanceTypes(id) {
        $.ajax({
            url: '/EmployeeAllowance/SelectAsync',
            type: 'GET',
            data: { id: id },
            dataType: 'json',
            success: function (res) {
                console.log("Allowance Types Fetched:", res);

                let accordionHtml = '';

                if (res && res.length > 0) {
                    accordionHtml += '<div class="accordion" id="accordionAllowance">';

                    res.forEach(function (item, i) {
                        let collapseId = `collapse-${i}`;
                        let headingId = `heading-${i}`;
                        let isFirst = i === 0;
                        let buttonClass = isFirst ? "accordion-button" : "accordion-button collapsed";
                        let collapseClass = isFirst ? "accordion-collapse collapse show" : "accordion-collapse collapse";
                        let ariaExpanded = isFirst.toString();

                        // Get employee benefit
                        let benefit = (item.empBenefitVMM && item.empBenefitVMM.length > 0) ? item.empBenefitVMM[0] : null;

                        let benefitId = benefit ? benefit.benefitID : 0;
                        let calculationType = benefit ? benefit.calculationTypeID : "";
                        let value = benefit ? benefit.value : "";

                        //
                        let baseCalculationType = benefit ? benefit.baseCalculationTypeID : "";
                        let baseValue = benefit ? benefit.baseValue : "";
                        //
                        accordionHtml += `
                                    <div class="accordion-item">
                                     
    <h2 class="accordion-header" id="${headingId}">
        <button class="${buttonClass}" type="button" data-bs-toggle="collapse" data-bs-target="#${collapseId}"
            aria-expanded="${ariaExpanded}" aria-controls="${collapseId}">
            ${item.name}
        </button>
    </h2>

    <div id="${collapseId}" class="${collapseClass}" aria-labelledby="${headingId}">
        <div class="accordion-body">
            <div class="card shadow-sm rounded-3 mb-1">
                <div class="card-body">
                    <input type="hidden" name="Benefits[${i}].BenefitTypeID" value="${item.id}" />
                    <input type="hidden" name="Benefits[${i}].BenefitID" value="${benefitId}" />

                    <div class="row g-3 align-items-center">

                        <!-- LEFT SIDE: EDITABLE (8 columns) -->
                        <div class="col-lg-4 col-md-6 col-sm-12">
                            <select class="form-select fixedPercentageSelect" name="Benefits[${i}].BaseCalculationTypeID">
                                <option value="">Select One</option>
                                <option value="1" ${baseCalculationType == 1 ? "selected" : ""}>Fixed</option>
                                <option value="2" ${baseCalculationType == 2 ? "selected" : ""}>Percentage</option>
                            </select>
                        </div>

                        <div class="col-lg-4 col-md-6 col-sm-12 fixedRateEditable" style="display:${baseCalculationType == 1 ? 'block' : 'none'};">
                            <input type="text" class="form-control fixedInput integerOnly" name="Benefits[${i}].BaseValue" placeholder="Enter Fixed Rate" value="${baseCalculationType == 1 ? baseValue : ''}" ${baseCalculationType == 1 ? "" : "disabled"}>
                        </div>

                        <div class="col-lg-4 col-md-6 col-sm-12 percentRateEditable" style="display:${baseCalculationType == 2 ? 'block' : 'none'};">
                            <select class="form-select percentInput"
                name="Benefits[${i}].BaseValue" ${baseCalculationType == 2 ? "" : "disabled"}>
              <option value="">Select %</option>${getPercentageOptionsHtml(baseValue)}
                </select>
                        </div>

                        <!-- RIGHT SIDE: READONLY (4 columns) -->
                        <div class="col-lg-2 col-md-6 col-sm-12">
                            
                            <select class="form-select" disabled>
                                <option value="1" ${calculationType == 1 ? "selected" : ""}>Fixed</option>
                                <option value="2" ${calculationType == 2 ? "selected" : ""}>Percentage</option>
                            </select>
                           

                           
                        </div>  
                        
                        <div class="col-lg-2 col-md-6 col-sm-12 fixedRate" style="display:${calculationType == 1 ? 'block' : 'none'};">
                            <input type="text" class="form-control fixedInput" disabled 
                                   placeholder="Enter Fixed Rate" value="${calculationType == 1 ? value : ''}">
                        </div>
                         <div class="col-lg-2 col-md-6 col-sm-12 percentRate" style="display:${calculationType == 2 ? 'block' : 'none'};">
                            <select class="form-select percentInput" disabled>
                                <option value="">Select %</option>
                                ${getPercentageOptionsHtml(value)}
                            </select>
                        </div>

                    </div> <!-- row -->

                </div> <!-- card-body -->
            </div> <!-- card -->
        </div> <!-- accordion-body -->
    </div>
</div>
`;
                    });

                    accordionHtml += '</div>';
                } else {
                    accordionHtml = '<p class="text-warning">No allowance found for this employee.</p>';
                }

                $('#EmployeeAllowanceAccordion').html(accordionHtml);
                // showHide();
            },
            error: function (err) {
                console.error("Error fetching allowance types:", err);
            }
        });
    }

    


    $(document).on("input", ".integerOnly", function () {
        let oldValue = this.value;
        let newValue = oldValue.replace(/[^0-9]/g, '');

        if (oldValue !== newValue) {
            toastr.warning("Only numbers are allowed!");
            this.value = newValue;
        }
    });



    $("#EmployeeAllowanceForm").on("submit", function (e) {
        e.preventDefault();

        var formData = $(this).serializeArray();
        formData.push({ name: "EmployeeId", value: lastInt });

        $.ajax({
            url: $(this).attr("action"),
            type: "POST",
            data: $.param(formData),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    loadAllowanceTypes(lastInt);
                } else {
                    toastr.error(res.message);
                }
            },
            error: function (err) {
                toastr.error("Something went wrong!");
                console.error("Save error:", err);
            }
        });
    });

    // End Iniatially Loaded 



    //#endregion




});





