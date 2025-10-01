$(document).ready(function () {

    var percentageOptionsHtml = $("#percentageOptionsTemplate").html();

    console.log(percentageOptionsHtml);
    function getLastIntFromUrl() {
        const parts = window.location.pathname.split('/').filter(Boolean).reverse();
        return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
    }

    


    FixedValue();
    function FixedValue() {
        $('.fixedPercentageSelect').val('1');
        $('.fixedPercentageSelect').each(function () {
            const selected = $(this).val();
            showHide($(this), selected);
        });
    };
    $(document).on('change', '.fixedPercentageSelect', function () {
        const selected = $(this).val();
        const $parent = $(this).closest('.input-group');
        const $fixedRate = $parent.find('.fixedRate');
        const $percentRate = $parent.find('.percentRate');
        const $calcTypeInput = $parent.closest('.col-lg-3').find('.calculationTypeInput');

        if (selected === 'Fixed') {
            $fixedRate.show();
            $percentRate.hide();
            $calcTypeInput.val(1); 
        } else if (selected === 'Percentage') {
            $percentRate.show();
            $fixedRate.hide();
            $calcTypeInput.val(2); 
        } else {
            $fixedRate.hide();
            $percentRate.hide();
            $calcTypeInput.val(0);
        }
    });
    //#region Fixed Percentage


    $(document).on('change', '.fixedPercentageSelect', function () {
        const selected = $(this).val();
        showHide($(this), selected);
    });
    function showHide($select, selected) {
        
        const $parent = $select.closest('.input-group');
        const $fixedRate = $parent.find('.fixedRate');
        const $percentRate = $parent.find('.percentRate');

        if (selected == 1) {
            $fixedRate.show();
            $percentRate.hide();
        } else if (selected == 2) {
            $percentRate.show();
            $fixedRate.hide();
        } else {
            $fixedRate.hide();
            $percentRate.hide();
        }
    }

    //#endregion

    
   
    

    $(document).ready(function () {

        const lastInt = getLastIntFromUrl();
        if (lastInt) {

            TabChange(lastInt);
        }


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

    function loadAllowanceTypes(id) {
        $.ajax({
            url: '/EmployeeBenifitController/SelectAllowanceTypeAsync',
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

                        <div class="col-lg-4 col-md-6 col-sm-12 fixedRate" style="display:${calculationType == 1 ? 'block' : 'none'};">
                            <input type="text" class="form-control fixedInput" name="Benefits[${i}].Value"
                                   placeholder="Enter Fixed Rate" value="${calculationType == 1 ? value : ''}">
                        </div>

                        <div class="col-lg-4 col-md-6 col-sm-12 percentRate" style="display:${calculationType == 2 ? 'block' : 'none'};">
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
                            <input type="text" class="form-control fixedInput" disabled name="Benefits[${i}].Value"
                                   placeholder="Enter Fixed Rate" value="${calculationType == 1 ? value : ''}">
                        </div>
                         <div class="col-lg-2 col-md-6 col-sm-12 percentRate" style="display:${calculationType == 2 ? 'block' : 'none'};">
                            <select class="form-select percentInput" disabled name="Benefits[${i}].Value">
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


    //

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




    // Edit Button Click
    //$(document).ready(function () {
    //    loadPayRollEmpAllowanceData();
    //});

    //function loadPayRollEmpAllowanceData() {
    //    $.ajax({
    //        url: '/PayRollEmpBenefitsUpdate/GetAllDataAfterSave',
    //        type: 'GET',
    //        success: function (res) {
    //            if (res.success && res.data && res.data.length > 0) {
    //                res.data.forEach((item, index) => {
    //                    console.log("Get All Data", item);
    //                    // --- Effective Date ---
    //                    const $effectiveDateInput = $(`input[name="HouseRentAllowances[${index}].EffectiveDate"]`);
    //                    if ($effectiveDateInput.length && item.effectiveDate) {
    //                        try {
    //                            const date = new Date(item.effectiveDate);
    //                            if (!isNaN(date.getTime())) {
    //                                const monthNames = ["January", "February", "March", "April", "May", "June",
    //                                    "July", "August", "September", "October", "November", "December"];
    //                                const formattedDate = `${monthNames[date.getMonth()]} ${date.getFullYear()}`;
    //                                $effectiveDateInput.val(formattedDate);
    //                                if ($effectiveDateInput[0]._flatpickr) {
    //                                    $effectiveDateInput[0]._flatpickr.setDate(formattedDate, true);
    //                                }
    //                            }
    //                        } catch (e) {
    //                            console.error(`Failed to set date for index ${index}:`, e.message);
    //                        }
    //                    }

    //                    // --- IsActive ---
    //                    const $isActiveCheckbox = $(`input[name="HouseRentAllowances[${index}].IsActive"]`);
    //                    if ($isActiveCheckbox.length && item.hasOwnProperty('isActive')) {
    //                        $isActiveCheckbox.prop('checked', item.isActive === true || item.isActive === 'true');
    //                    }

    //                    // --- EmployeeAllowanceID (hidden input) ---
    //                    const $allowanceIdInput = $('input[name="EmployeeAllowanceID"]');
    //                    if ($allowanceIdInput.length && item.hasOwnProperty('employeeAllowanceID')) {
    //                        $allowanceIdInput.val(item.employeeAllowanceID);
    //                    }

    //                });

    //                toastr.success(res.message || "Data loaded successfully");
    //            } else {
    //                toastr.warning(res.message || "No data found.");
    //            }
    //        },


    //        error: function (err) {
    //            console.error("Error fetching data:", err);
    //            toastr.error("Failed to load payroll employee Benefits data.");
    //        }
    //    });
    //}





    
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


