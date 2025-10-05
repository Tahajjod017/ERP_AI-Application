$(document).ready(function () {

    var percentageOptionsHtml = $("#percentageOptionsTemplate").html();

    var lastInt=0

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
        let $row = $(this).closest(".row"); // only this row

        let selected = $(this).val();

        if (selected == "1") { // Fixed
            $row.find(".fixedRateEditable").show();
            $row.find(".percentRateEditable").hide();

        } else if (selected == "2") { // Percentage
            $row.find(".fixedRateEditable").hide();
            $row.find(".percentRateEditable").show();

        } else {
            $row.find(".fixedRateEditable").hide();
            $row.find(".percentRateEditable").hide();
        }
    });



    function loadAllowanceTypes(id) {
        $.ajax({
            url: '/EmployeeBenifitController/SelectBenefitsTypeAsync',
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
                            <select class="form-select fixedPercentageSelect" name="Benefits[${i}].CalculationTypeID">
                                <option value="">Select One</option>
                                <option value="1" ${calculationType == 1 ? "selected" : ""}>Fixed</option>
                                <option value="2" ${calculationType == 2 ? "selected" : ""}>Percentage</option>
                            </select>
                        </div>

                        <div class="col-lg-4 col-md-6 col-sm-12 fixedRateEditable" style="display:${calculationType == 1 ? 'block' : 'none'};">
                            <input type="text" class="form-control fixedInput" name="Benefits[${i}].Value"
                                   placeholder="Enter Fixed Rate" value="${calculationType == 1 ? value : ''}">
                        </div>

                        <div class="col-lg-4 col-md-6 col-sm-12 percentRateEditable" style="display:${calculationType == 2 ? 'block' : 'none'};">
                            <select class="form-select percentInput" name="Benefits[${i}].Value">
                                <option value="">Select %</option>
                                ${getPercentageOptionsHtml(value)}
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
                    accordionHtml = '<p class="text-warning">No Benefits found for this organization.</p>';
                }

                $('#EmployeeAllowanceAccordion').html(accordionHtml);
               // showHide();
            },
            error: function (err) {
                console.error("Error fetching allowance types:", err);
            }
        });
    }


    $("#benefitForm").on("submit", function (e) {
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

    $("select[name='OrganizationID']").on("changed", function () {
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

    $(document).on('click', '.PayRollEmpBenefitsSave', function (e) {
        e.preventDefault();
        let $card = $(this).closest('.accordion-item');
        let jsonData = {
            OrganizationID: parseInt($('#OrganizationID').val()) || 0,
            Benefits: []
        };
        let dateStr = $card.find('.flatpickr-input').val();
        let effectiveDate = dateStr ? new Date(dateStr).toISOString().split('T')[0] : null;
        let benefit = {
            BenefitID: parseInt($card.find('input[name$=".BenefitID"]').val()) || 0,
            BenefitTypeID: parseInt($card.find('input[name$=".BenefitTypeID"]').val()) || 0,
            IsActive: $card.find('input[name$=".IsActive"]').is(':checked'),
            EffectiveDate: effectiveDate,
            BenefitSetups: []
        };
        $card.find('.houseRentContainer .houseRentRow').each(function () {
            const $row = $(this);

            let calcTypeId = parseInt($row.find('.fixedPercentageSelect').val()) || 0;
            let value = 0;
            if (calcTypeId === 1) {
                value = parseFloat($row.find('.fixedInput').val()) || 0;
            } else if (calcTypeId === 2) {
                value = parseFloat($row.find('.percentInput').val()) || 0;
            }
            let salaryMin = parseFloat($row.find('input[name$=".SalaryMin"]').val()) || 0;
            let salaryMax = parseFloat($row.find('input[name$=".SalaryMax"]').val()) || 0;
            benefit.BenefitSetups.push({
                BenefitSetupID: 0,
                SalaryMin: salaryMin,
                SalaryMax: salaryMax,
                CalculationTypeID: calcTypeId,
                Value: value
            });
        });
        jsonData.Benefits.push(benefit);
        console.log("Sending JSON for only this card:", JSON.stringify(jsonData, null, 2));
        $.ajax({
            url: '/PayRollEmpBenefitsUpdate/Save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(jsonData),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    // resetPayRollEmpBenefitsForm();
                }
                else {
                    toastr.error(res.message || "Save failed");
                }

            },
            error: function (err) {
                toastr.error("Error while saving");
                console.error(err);
            }
        });
    });

    //

    function resetPayRollEmpBenefitsForm() {
        const $form = $('#payrollEmpBenefitsForm');

        $form[0].reset();

        if (typeof choiceManager !== "undefined") {
            choiceManager.resetChoice('OrganizationID');
        } else {
            // fallback if plain select
            $('#OrganizationID').val("").trigger('change');
        }

        $form.find('.flatpickr-input').each(function () {
            if (this._flatpickr) {
                this._flatpickr.clear();
            } else {
                $(this).val("");
            }
        });
        $form.find('input[type="checkbox"]').prop('checked', false);

        $('#EmployeeAllowanceAccordion').empty();

        $form.find('.text-danger').text('');
    }


    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetPayRollEmpBenefitsForm();

    })

    
    //#endregion




});




// from sheafain  vaii
//$(document).ready(function () {

//    //#region employeeChoices with onchange

//    let employeeChoices;
//    function initEmployeeChoices() {
//        employeeChoices = new Choices('#EmployeePersonalId', {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select Employee'
//        });

//        const employeeElement = document.getElementById('EmployeePersonalId');
//        if (employeeElement) {
//            employeeElement.addEventListener('change', function (e) {
//                const selectedEmployeeId = e.detail.value || e.target.value;
//                if (selectedEmployeeId && selectedEmployeeId !== '') {
//                    loadEmployeeBenifitData(selectedEmployeeId);
//                    TabChange(selectedEmployeeId) // this function is located in EmployeeTabChange.js
//                } else {
//                    clearForm();
//                }
//            });

//        }
      
//    }
//    document.addEventListener('DOMContentLoaded', initEmployeeChoices);
//    initEmployeeChoices();

//    //#endregion



//    //#region Choice dropdowns

  


//    let yearlyEndBonusTypeChoices;
//    function initYearlyEndBonusTypeChoices() {
//        yearlyEndBonusTypeChoices = new Choices('#YearlyEndBonusTypeID', {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select Year'
//        });
//    }
//    document.addEventListener('DOMContentLoaded', initYearlyEndBonusTypeChoices);
//    initYearlyEndBonusTypeChoices();

//    let serviceYearChoices;
//    function initServiceYearChoices() {
//        serviceYearChoices = new Choices('#serviceYearSelect', {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select Year'
//        });
//    }
//    document.addEventListener('DOMContentLoaded', initServiceYearChoices);
//    initServiceYearChoices();

//    let festivalBonusRateChoices;
//    function initFestivalBonusRateChoices() {
//        festivalBonusRateChoices = new Choices('#festivalBonusRateSelect', {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select %'
//        });
//    }
//    document.addEventListener('DOMContentLoaded', initFestivalBonusRateChoices);
//    initFestivalBonusRateChoices();

    

//    let employeeContributionChoices;
//    function initEmployeeContributionChoices() {
//        employeeContributionChoices = new Choices('#employeeContributionSelect', {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select %'
//        });
//    }
//    document.addEventListener('DOMContentLoaded', initEmployeeContributionChoices);
//    initEmployeeContributionChoices();

//    let organizationContributionChoices;
//    function initOrganizationContributionChoices() {
//        organizationContributionChoices = new Choices('#organizationContributionSelect', {
//            removeItemButton: true,
//            shouldSort: false,
//            placeholderValue: 'Select %'
//        });
//    }
//    document.addEventListener('DOMContentLoaded', initOrganizationContributionChoices);
//    initOrganizationContributionChoices();

   



//    //#endregion




//    //#region Submit

//    $('form').on('submit', function (e) {
//        e.preventDefault();

//        // Get the form element
//        var form = $(this);
//        var formData = new FormData(form[0]);

//        $.ajax({
//            url: form.attr('action'),
//            type: form.attr('method'),
//            data: formData,
//            processData: false,
//            contentType: false,
//            success: function (response) {
//                if (response.success) {
//                    toastr.success(response.message || 'Employee benefits saved successfully.');
//                    window.location.href = '/EmployeeAllowance/Index/' + response.data;
                    
//                } else {
//                    toastr.error( 'Error saving employee benefits');
//                    console.error(response.message)
//                }
//            },
//            error: function (xhr) {
//                if (xhr.status === 400 && xhr.responseJSON) {
//                    // Handle validation errors
//                    var errors = xhr.responseJSON;
//                    for (var field in errors) {
//                        if (errors.hasOwnProperty(field)) {
//                            toastr.error(errors[field][0]); 
//                        }
//                    }
//                } else {
//                    toastr.error('Failed to save employee benefits. Please try again.');
//                }
//            }
//        });
//    });

//    //#endregion

  

//    //#region Populate Data
   
//    function loadEmployeeBenifitData(employeeId) {
//        if (!employeeId) {
//            clearForm();
//            return;
//        }

//        $.ajax({
//            url: '/EmployeeBenifit/GetEmployeeBenefits',
//            type: 'GET',
//            data: { employeeId: employeeId },
//            success: function (data) {
//                if (data) {
//                    populateForm(data);
//                } else {
//                    clearForm();
//                }
//            },
//            error: function (xhr, status, error) {
//                console.error('Error loading employee benefits:', error);
//                clearForm();
//                alert('Failed to load employee benefits. Please try again.');
//            }
//        });
//    }

//    // Populate form with employee benefit data
//    function populateForm(employee) {

//        var data = employee.data;
//        debugger

//        $('#EmployeeBaseBenefitID').val(data.employeeBaseBenefitID || '');
//        $('#EmployeePersonalId').val(data.employeePersonalId || '');

//        choiceManager.setChoiceValue('organizationDD', data.organizationID || '');
        
//        $('#PersonalEmail').val(data.personalEmail || '');
//        $('#PersonalPhone').val(data.personalPhone || '');
//        $('#HealthInsurance').val(data.healthInsurance || '');
//        $('#PerformanceBonus').val(data.performanceBonus || '');

//        // Update switch checkboxes
//        $('#empBeniftSwitch').prop('checked', data.isBenifitEnabled || false);
//        $('#healthInsuranceSwitch').prop('checked', data.isHealthInsuranceEnabled || false);
//        $('#performanceBonusSwitch').prop('checked', data.isPerformanceBonusEnabled || false);
//        $('#yearlyEndBonusSwitch').prop('checked', data.isYearlyEndBonusTypeIDEnabled || false);
//        $('#festivalBonusSwitch').prop('checked', data.isFastivalBonusPercentageEnabled || false);
//        $('#providentFundSwitch1').prop('checked', data.isProvidantFundEnabled || false);

//        // Update Choices.js dropdowns
//        employeeChoices.setChoiceByValue(data.employeePersonalId || '');
//        yearlyEndBonusTypeChoices.setChoiceByValue(data.yearlyEndBonusTypeID || '');
//        serviceYearChoices.setChoiceByValue(data.serviceYearID || '');
//        festivalBonusRateChoices.setChoiceByValue(data.fastivalBonusPercentage || '');
//        employeeContributionChoices.setChoiceByValue(data.providantFundEmployeePercentage || '');
//        organizationContributionChoices.setChoiceByValue(data.providantFundOrganizationPercentage || '');
//    }

    
//    function clearForm() {
        
//        $('#EmployeeBaseBenefitID').val('');
//        $('#PersonalEmail').val('');
//        $('#PersonalPhone').val('');
//        $('#HealthInsurance').val('');
//        $('#PerformanceBonus').val('');

        
//        $('#empBeniftSwitch').prop('checked', false);
//        $('#healthInsuranceSwitch').prop('checked', false);
//        $('#performanceBonusSwitch').prop('checked', false);
//        $('#yearlyEndBonusSwitch').prop('checked', false);
//        $('#festivalBonusSwitch').prop('checked', false);
//        $('#providentFundSwitch1').prop('checked', false);

//        choiceManager.clearChoice('organizationDD')
        
//        employeeChoices.clearStore();
//        yearlyEndBonusTypeChoices.clearStore();
//        serviceYearChoices.clearStore();
//        festivalBonusRateChoices.clearStore();
//        employeeContributionChoices.clearStore();
//        organizationContributionChoices.clearStore();

       
//    }



//    //#endregion

//});


