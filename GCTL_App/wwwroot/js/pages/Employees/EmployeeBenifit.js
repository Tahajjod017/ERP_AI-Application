$(document).ready(function () {

    var percentageOptionsHtml = $("#percentageOptionsTemplate").html();
    initializeFlatpickr();
    function initializeFlatpickr() {
        $('.flatpickr-input').flatpickr({
            altInput: true,
            altFormat: "F Y",
            dateFormat: "Y-m-d",
            plugins: [
                new monthSelectPlugin({
                    shorthand: true,
                    dateFormat: "F Y",
                    altFormat: "F Y",
                    theme: "light"
                })
            ]
        });
    }

    //#endregion
    // If no rows exist initially
    if ($(".houseRentRow").length === 0) {
        // Simulate click or directly call AJAX with index 0
        const $firstContainer = $(".houseRentContainer").first();

        $.ajax({
            url: '/PayRollEmpBenefitsUpdate/GetHouseRentAllowanceRow',
            type: 'GET',
            data: { index: 0 },
            success: function (response) {
                $firstContainer.append(response);
                const firstRow = $firstContainer.find(".houseRentRow").first();
                firstRow.find('.fixedPercentageSelect').val('Fixed').trigger('change');
            }
        });
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
            $calcTypeInput.val(1); // 1 = Fixed
        } else if (selected === 'Percentage') {
            $percentRate.show();
            $fixedRate.hide();
            $calcTypeInput.val(2); // 2 = Percentage
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
        // Find the closest input-group to scope the fixedRate and percentRate elements
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

    // Add new row
    $(document).on('click', '.addRow', function () {
        let container = $(this).closest('.houseRentContainer');
        let rows = container.find('.houseRentRow');
        let newIndex = rows.length;

        // Clone first row
        let newRow = rows.first().clone();

        // Clear input values
        newRow.find('input').val('');
        newRow.find('select').val('');
        newRow.find('.percentRate').hide();
        newRow.find('.fixedRate').hide();

        // Update name attributes with new index
        newRow.find('[name]').each(function () {
            let name = $(this).attr('name');
            if (name) {
                let updatedName = name.replace(/BenefitSetups\[\d+\]/, `BenefitSetups[${newIndex}]`);
                $(this).attr('name', updatedName);
            }
        });
        newRow.find('.addRow')
            .removeClass('addRow btn-outline-primary')
            .addClass('removeRow  btn-danger')
            .text('X');
        container.append(newRow);
        FixedValue();
    });

    // Remove row
    $(document).on('click', '.removeRow', function () {
        $(this).closest('.houseRentRow').remove();
    });

    // Initailly Loaded

    var id = $("#OrganizationID").val();
    if (id) {
        loadAllowanceTypes(id);
    } else {
        return toastr.info('No Organization Id Found');
    }

    $(document).on('change', '#OrganizationID', function () {
        var id = $(this).val();
        if (!id) {
            return toastr.info('No Organization Id Found');
        }
        loadAllowanceTypes(id);
    });
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

                        accordionHtml += `
                        <div class="accordion-item">
                            <h2 class="accordion-header" id="${headingId}">
                                <button class="${buttonClass}" type="button"
                                        data-bs-toggle="collapse"
                                        data-bs-target="#${collapseId}"
                                        aria-expanded="${ariaExpanded}"
                                        aria-controls="${collapseId}">
                                    ${item.name}
                                </button>
                            </h2>

                            <div id="${collapseId}" class="${collapseClass}" aria-labelledby="${headingId}">
                                <div class="accordion-body">

                                    <div class="card shadow-sm rounded-3 mb-1 houseRentRow">
                                        <div class="card-body">
                                            <div class="houseRentContainer">
                                                <div class="row">
                                                    <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
                                                        <input type="hidden" name="Benefits[${i}].BenefitTypeID" value="${item.id}" />
                                                         <input type="hidden" name="Benefits[${i}].BenefitID" value="0" />

                                                        <label class="form-label">Effective Date</label>
                                                        <div class="input-icon-end position-relative">
                                                            <input class="form-control ps-6 flatpickr-input"
                   name="Benefits[${i}].EffectiveDate"
                   type="text" readonly placeholder="Select Month">
                                                            <span class="uil uil-calendar-alt position-absolute top-50 end-0 translate-middle-y me-3 text-body-tertiary"></span>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
                                                        <div class="form-check form-switch mt-4">
                                                            <input class="form-check-input" type="checkbox" name="Benefits[${i}].IsActive">
                                                            <label class="form-check-label ms-2">Active</label>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="row g-3 houseRentRow">
                                                    <div class="col-lg-4 col-md-6 col-sm-12">
                                                        <label class="form-label">Min Salary</label>
                                                        <input type="text" class="form-control"
               name="Benefits[${i}].BenefitSetups[0].SalaryMin"
               placeholder="Enter Min Salary" />
                                                    </div>
                                                    <div class="col-lg-4 col-md-6 col-sm-12">
                                                        <label class="form-label">Max Salary</label>
                                                       <input type="text" class="form-control"
               name="Benefits[${i}].BenefitSetups[0].SalaryMax"
               placeholder="Enter Max Salary" />
                                                    </div>
                                                                         <div class="col-lg-3 col-md-3 col-sm-12 d-flex align-items-center gap-3">
                                <!-- Hidden CalculationTypeID -->



                                <div class="input-group mb-3">
                                    <!-- Selector -->
                                    <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
                                        <option value="">Select One</option>
                                        <option value="1">Fixed</option>
                                        <option value="2">Percentage</option>
                                    </select>

                                    <!-- Percentage -->
                                    <div class="percentRate" style="display: none;">
                                        <label class="form-label">Percentage(%)</label>
                                        <select class="form-select choiceDD percentInput"
                                                name="Benefits[${i}].BenefitSetups[0].Value"
                                                style="max-width:185px;">
                                            <option value="">Select %</option>
                                           ${percentageOptionsHtml}
                                        </select>
                                    </div>

                                    <!-- Fixed -->
                                    <div class="fixedRate" style="display: none;">
                                        <label class="form-label">Fixed</label>
                                        <input type="text" class="form-control fixedInput"
                                               name="Benefits[${i}].BenefitSetups[0].Value"
                                               placeholder="Enter Fixed Rate"
                                               style="max-width: 185px;">
                                    </div>
                                </div>
                            </div>
                                                    <div class="col-lg-1 px-0 col-md-1 col-sm-1">
                                                        <button type="button" class="btn btn-primary px-3 mt-4 addRow">
                                                            <i class="bi bi-plus-circle"></i>+
                                                        </button>
                                                    </div>
                                                </div>

                                            </div>
                                            <div class="row g-3 justify-content-end">
                                                <div class="col-auto">
                                                    <button class="btn btn-primary px-5 PayRollEmpBenefitsSave">Save</button>
                                                </div>
                                            </div>
                                        </div>
                                    </div>

                                </div>
                            </div>
                        </div>`;
                    });

                    accordionHtml += '</div>';

                } else {
                    accordionHtml = '<p class="text-warning">No Benefits found for this organization.</p>';
                }

                $('#EmployeeAllowanceAccordion').html(accordionHtml);
                initializeFlatpickr();
                FixedValue();
            },
            error: function (err) {
                //toastr.error('Failed to fetch allowance types');
                console.error(err);
            }
        });
    }

    //


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


