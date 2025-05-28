$(document).ready(function () {

    //#region employeeChoices with onchange

    let employeeChoices;
    function initEmployeeChoices() {
        employeeChoices = new Choices('#EmployeePersonalId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee'
        });

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

    //#region Choice dropdowns

  


    let yearlyEndBonusTypeChoices;
    function initYearlyEndBonusTypeChoices() {
        yearlyEndBonusTypeChoices = new Choices('#YearlyEndBonusTypeID', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Year'
        });
    }
    document.addEventListener('DOMContentLoaded', initYearlyEndBonusTypeChoices);
    initYearlyEndBonusTypeChoices();

    let serviceYearChoices;
    function initServiceYearChoices() {
        serviceYearChoices = new Choices('#serviceYearSelect', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Year'
        });
    }
    document.addEventListener('DOMContentLoaded', initServiceYearChoices);
    initServiceYearChoices();

    let festivalBonusRateChoices;
    function initFestivalBonusRateChoices() {
        festivalBonusRateChoices = new Choices('#festivalBonusRateSelect', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select %'
        });
    }
    document.addEventListener('DOMContentLoaded', initFestivalBonusRateChoices);
    initFestivalBonusRateChoices();

    

    let employeeContributionChoices;
    function initEmployeeContributionChoices() {
        employeeContributionChoices = new Choices('#employeeContributionSelect', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select %'
        });
    }
    document.addEventListener('DOMContentLoaded', initEmployeeContributionChoices);
    initEmployeeContributionChoices();

    let organizationContributionChoices;
    function initOrganizationContributionChoices() {
        organizationContributionChoices = new Choices('#organizationContributionSelect', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select %'
        });
    }
    document.addEventListener('DOMContentLoaded', initOrganizationContributionChoices);
    initOrganizationContributionChoices();

   



    //#endregion

    

    //#region Submit

    $('form').on('submit', function (e) {
        e.preventDefault(); // Prevent default form submission

        // Collect form data
        var formData = {
            EmployeeBaseBenefitID: $('#EmployeeBaseBenefitID').val(),
            EmployeePersonalId: $('#EmployeePersonalId').val(),
            PersonalEmail: $('#PersonalEmail').val(),
            PersonalPhone: $('#PersonalPhone').val(),
            IsBenifitEnabled: $('#empBeniftSwitch').is(':checked'),
            HealthInsurance: $('#HealthInsurance').val(),
            IsHealthInsuranceEnabled: $('#healthInsuranceSwitch').is(':checked'),
            PerformanceBonus: $('#PerformanceBonus').val(),
            IsPerformanceBonusEnabled: $('#performanceBonusSwitch').is(':checked'),
            YearlyEndBonusTypeID: $('#YearlyEndBonusTypeID').val(),
            IsYearlyEndBonusTypeIDEnabled: $('#yearlyEndBonusSwitch').is(':checked'),
            FastivalBonusPercentage: $('#festivalBonusRateSelect').val(),
            IsFastivalBonusPercentageEnabled: $('#festivalBonusSwitch').is(':checked'),
            ProvidantFundEmployeePercentage: $('#employeeContributionSelect').val(),
            ProvidantFundOrganizationPercentage: $('#organizationContributionSelect').val(),
            IsProvidantFundEnabled: $('#providentFundSwitch1').is(':checked'),
            ServiceYearID: $('#serviceYearSelect').val()
        };

    

        $.ajax({
            url: '/EmployeeBenifit/Index',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    alert('Employee benefits saved successfully.');
                    clearForm(); 
                } else {
                    alert('Error saving employee benefits: ' + (response.message || 'Unknown error'));
                }
            },
            error: function (xhr, status, error) {
                console.error('Error saving employee benefits:', error);
                alert('Failed to save employee benefits. Please try again.');
            }
        });
    });

    //#endregion

    //#region Populate Data
   
    function loadEmployeeBenifitData(employeeId) {
        if (!employeeId) {
            clearForm();
            return;
        }

        $.ajax({
            url: '/EmployeeBenifit/GetEmployeeBenefits',
            type: 'GET',
            data: { employeeId: employeeId },
            success: function (data) {
                if (data) {
                    populateForm(data);
                } else {
                    clearForm();
                }
            },
            error: function (xhr, status, error) {
                console.error('Error loading employee benefits:', error);
                clearForm();
                alert('Failed to load employee benefits. Please try again.');
            }
        });
    }

    // Populate form with employee benefit data
    function populateForm(employee) {

        var data = employee.data;
        debugger

        $('#EmployeeBaseBenefitID').val(data.employeeBaseBenefitID || '');
        $('#EmployeePersonalId').val(data.employeePersonalId || '');

        
        $('#PersonalEmail').val(data.personalEmail || '');
        $('#PersonalPhone').val(data.personalPhone || '');
        $('#HealthInsurance').val(data.healthInsurance || '');
        $('#PerformanceBonus').val(data.performanceBonus || '');

        // Update switch checkboxes
        $('#empBeniftSwitch').prop('checked', data.isBenifitEnabled || false);
        $('#healthInsuranceSwitch').prop('checked', data.isHealthInsuranceEnabled || false);
        $('#performanceBonusSwitch').prop('checked', data.isPerformanceBonusEnabled || false);
        $('#yearlyEndBonusSwitch').prop('checked', data.isYearlyEndBonusTypeIDEnabled || false);
        $('#festivalBonusSwitch').prop('checked', data.isFastivalBonusPercentageEnabled || false);
        $('#providentFundSwitch1').prop('checked', data.isProvidantFundEnabled || false);

        // Update Choices.js dropdowns
        employeeChoices.setChoiceByValue(data.employeePersonalId || '');
        yearlyEndBonusTypeChoices.setChoiceByValue(data.yearlyEndBonusTypeID || '');
        serviceYearChoices.setChoiceByValue(data.serviceYearID || '');
        festivalBonusRateChoices.setChoiceByValue(data.fastivalBonusPercentage || '');
        employeeContributionChoices.setChoiceByValue(data.providantFundEmployeePercentage || '');
        organizationContributionChoices.setChoiceByValue(data.providantFundOrganizationPercentage || '');
    }

    
    function clearForm() {
        
        $('#EmployeeBaseBenefitID').val('');
        $('#PersonalEmail').val('');
        $('#PersonalPhone').val('');
        $('#HealthInsurance').val('');
        $('#PerformanceBonus').val('');

        
        $('#empBeniftSwitch').prop('checked', false);
        $('#healthInsuranceSwitch').prop('checked', false);
        $('#performanceBonusSwitch').prop('checked', false);
        $('#yearlyEndBonusSwitch').prop('checked', false);
        $('#festivalBonusSwitch').prop('checked', false);
        $('#providentFundSwitch1').prop('checked', false);

        
        employeeChoices.clearStore();
        yearlyEndBonusTypeChoices.clearStore();
        serviceYearChoices.clearStore();
        festivalBonusRateChoices.clearStore();
        employeeContributionChoices.clearStore();
        organizationContributionChoices.clearStore();

       
    }



    //#endregion

});



//#region backup
////#region loadEmployeeBenifitData

//function loadEmployeeBenifitData(employeeId) {
//    $.ajax({
//        url: `/EmployeeBenifit/GetEmployeeBenifitData?employeeId=${employeeId}`,
//        type: 'GET',
//        success: function (data) {
//            if (data) {
//                populateForm(data);
//            } else {
//                clearForm();
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error('Error loading employee benefit data:', error);
//            clearForm();
//        }
//    });
//}

////#endregion

////#region populateForm
//function populateForm(data) {
//    $('#EmployeePersonalId').val(data.EmployeePersonalId);
//    $('#HealthInsurance').val(data.HealthInsurance);
//    $('#RetirementPlan').val(data.RetirementPlan);
//    $('#PaidTimeOff').val(data.PaidTimeOff);
//    $('#OtherBenefits').val(data.OtherBenefits);
//    $('#EmployeeBenifitId').val(data.EmployeeBenifitId);
//    $('#CreatedBy').val(data.CreatedBy);
//    $('#CreatedDate').val(data.CreatedDate);
//    $('#ModifiedBy').val(data.ModifiedBy);
//    $('#ModifiedDate').val(data.ModifiedDate);
//    $('#IsActive').prop('checked', data.IsActive);
//    initializeToggles();
//}
////#endregion

////#region clearForm
//function clearForm() {
//    $('#EmployeePersonalId').val('');
//    $('#HealthInsurance').val('');
//    $('#RetirementPlan').val('');
//    $('#PaidTimeOff').val('');
//    $('#OtherBenefits').val('');
//    $('#EmployeeBenifitId').val('');
//    $('#CreatedBy').val('');
//    $('#CreatedDate').val('');
//    $('#ModifiedBy').val('');
//    $('#ModifiedDate').val('');
//    $('#IsActive').prop('checked', false);

//    $('#HealthInsurance').val('');
//    $('#PerformanceBonus').val('');
//    $('#YearlyEndBonusTypeID').val('');
//    $('#FastivalBonusPercentage').val('');
//    $('#ProvidantFundEmployeePercentage').val('');
//    $('#ProvidantFundOrganizationPercentage').val('');
//    $('#ServiceYearID').val('');

//    $('#healthInsuranceSwitch').prop('checked', false);
//    $('#performanceBonusSwitch').prop('checked', false);
//    $('#yearlyEndBonusSwitch').prop('checked', false);
//    $('#festivalBonusSwitch').prop('checked', false);
//    $('#providentFundSwitch').prop('checked', false);
//}
//    //#endregion

////#region Dynamic Toggle Logic

//// Health Insurance
//$('#HealthInsurance').on('input keyup change', function () {
//    debugger
//    var value = $(this).val().trim();
//    $('#healthInsuranceSwitch').prop('checked', value !== '' && value !== '0');
//});

//$('#healthInsuranceSwitch').on('change', function () {
//    if (!$(this).is(':checked')) {
//        $('#HealthInsurance').val('');
//    }
//});

//// Performance Bonus
//$('#PerformanceBonus').on('input keyup change', function () {
//    debugger
//    var value = $(this).val().trim();
//    $('#performanceBonusSwitch').prop('checked', value !== '' && value !== '0');
//});

//$('#performanceBonusSwitch').on('change', function () {
//    if (!$(this).is(':checked')) {
//        $('#PerformanceBonus').val('');
//    }
//});

//// Yearly End Bonus
//$('#YearlyEndBonusTypeID').on('change', function () {
//    var value = $(this).val();
//    $('#yearlyEndBonusSwitch').prop('checked', value !== '' && value !== '0');
//});

//$('#yearlyEndBonusSwitch').on('change', function () {
//    if (!$(this).is(':checked')) {
//        $('#YearlyEndBonusTypeID').val('');
//        if (yearlyEndBonusTypeChoices) {
//            yearlyEndBonusTypeChoices.setChoiceByValue('');
//        }
//    }
//});

//// Festival Bonus
//$('#festivalBonusRateSelect').on('change', function () {
//    var value = $(this).val();
//    $('#festivalBonusSwitch').prop('checked', value !== '' && value !== '0');
//});

//$('#festivalBonusSwitch').on('change', function () {
//    if (!$(this).is(':checked')) {
//        if (festivalBonusRateChoices) {
//            festivalBonusRateChoices.removeActiveItems();
//            $('#festivalBonusRateSelect').val('');
//            festivalBonusRateChoices.setChoiceByValue('');
//            $('#festivalBonusRateSelect').trigger('change');
//        }

//        if (bonusDependsOnChoices) {
//            bonusDependsOnChoices.removeActiveItems();
//            $('#bonusDependsOnSelect').val('');
//            bonusDependsOnChoices.setChoiceByValue('');
//            $('#bonusDependsOnSelect').trigger('change');
//        }
//    }
//});




//function clearChoicesSelection(choicesInstance, selector) {
//    if (choicesInstance) {
//        choicesInstance.removeActiveItems();

//        $(selector).val('');

//        choicesInstance.setChoiceByValue('');

//        $(selector).trigger('change');
//    } else {

//        $(selector).val('').trigger('change');
//    }
//}

//function checkProvidentFund() {
//    const empValue = $('#employeeContributionSelect').val();
//    const orgValue = $('#organizationContributionSelect').val();
//    const serviceYearValue = $('#serviceYearSelect').val();

//    $('#providentFundSwitch1').prop('checked',
//        (empValue !== '' && empValue !== '0') ||
//        (orgValue !== '' && orgValue !== '0') ||
//        (serviceYearValue !== '' && serviceYearValue !== '0')
//    );
//}

//$('#employeeContributionSelect').on('change', checkProvidentFund);
//$('#organizationContributionSelect').on('change', checkProvidentFund);
//$('#serviceYearSelect').on('change', checkProvidentFund);

//$('#providentFundSwitch1').on('change', function () {
//    if (!$(this).is(':checked')) {
//        clearChoicesSelection(employeeContributionChoices, '#employeeContributionSelect');
//        clearChoicesSelection(organizationContributionChoices, '#organizationContributionSelect');
//        clearChoicesSelection(serviceYearChoices, '#serviceYearSelect');
//        clearChoicesSelection(providentFundDependsOnChoices, '#providentFundDependsOnSelect');
//    }
//});


////#endregion


////#region Masteer Toggle

//// List all selectors that should toggle empBeniftSwitch
//const monitoredFields = [
//    '#HealthInsurance',
//    '#PerformanceBonus',
//    '#YearlyEndBonusTypeID',
//    '#festivalBonusRateSelect',
//    '#bonusDependsOnSelect',
//    '#employeeContributionSelect',
//    '#organizationContributionSelect',
//    '#serviceYearSelect',
//    '#providentFundDependsOnSelect'
//];

//// Choices instances map (adjust names as per your variables)
//const choicesMap = {
//    '#festivalBonusRateSelect': festivalBonusRateChoices,
//    '#bonusDependsOnSelect': bonusDependsOnChoices,
//    '#YearlyEndBonusTypeID': yearlyEndBonusTypeChoices,
//    '#employeeContributionSelect': employeeContributionChoices,
//    '#organizationContributionSelect': organizationContributionChoices,
//    '#serviceYearSelect': serviceYearChoices,
//    '#providentFundDependsOnSelect': providentFundDependsOnChoices
//};

//// Check if any monitored field has a non-empty/non-zero value
//function updateEmpBenefitSwitch() {
//    let anyValueSet = false;
//    monitoredFields.forEach(selector => {
//        const val = $(selector).val();
//        if (val !== '' && val !== '0' && val !== null) {
//            anyValueSet = true;
//        }
//    });
//    $('#empBeniftSwitch').prop('checked', anyValueSet);
//}

//// Clear a Choices instance and its underlying select
//function clearChoicesSelection(choicesInstance, selector) {
//    if (choicesInstance) {
//        choicesInstance.removeActiveItems();
//        $(selector).val('');
//        choicesInstance.setChoiceByValue('');
//        $(selector).trigger('change');
//    } else {
//        $(selector).val('').trigger('change');
//    }
//}

//// Reset all monitored fields
//function resetAllFields() {
//    monitoredFields.forEach(selector => {
//        const choicesInstance = choicesMap[selector];
//        if (choicesInstance) {
//            clearChoicesSelection(choicesInstance, selector);
//        } else {
//            $(selector).val('').trigger('change');
//        }
//    });
//}

//// Attach event listeners to all monitored fields to update the empBeniftSwitch
//monitoredFields.forEach(selector => {
//    $(document).on('input keyup change', selector, updateEmpBenefitSwitch);
//});

//// When the master switch is turned off, reset all fields
//$('#empBeniftSwitch').on('change', function () {
//    if (!$(this).is(':checked')) {
//        resetAllFields();
//    }
//});

//// Optional: Call updateEmpBenefitSwitch() once on page load to sync initial state
//$(document).ready(updateEmpBenefitSwitch);


////#endregion

////#region Initialize toggles on page load

//function initializeToggles() {
//    var healthValue = $('#HealthInsurance').val();
//    $('#healthInsuranceSwitch').prop('checked', healthValue && healthValue.trim() !== '' && healthValue !== '0');

//    var bonusValue = $('#PerformanceBonus').val();
//    $('#performanceBonusSwitch').prop('checked', bonusValue && bonusValue.trim() !== '' && bonusValue !== '0');

//    var yearlyValue = $('#YearlyEndBonusTypeID').val();
//    $('#yearlyEndBonusSwitch').prop('checked', yearlyValue && yearlyValue !== '' && yearlyValue !== '0');

//    var festivalValue = $('#FastivalBonusPercentage').val();
//    $('#festivalBonusSwitch').prop('checked', festivalValue && festivalValue !== '' && festivalValue !== '0');

//    checkProvidentFund();
//}

//setTimeout(function () {
//    initializeToggles();
//}, 500);

////#endregion

//#endregion

