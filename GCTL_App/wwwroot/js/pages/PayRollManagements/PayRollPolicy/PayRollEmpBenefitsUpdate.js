
$(document).ready(function () {

    var percentageOptionsHtml = $("#percentageOptionsTemplate").html();
    initializeFlatpickr();
   

    function getPercentageOptionsHtml(selectedValue) {
        let html = '';
        $("#percentageOptionsTemplate option").each(function () {
            const val = $(this).val();
            const text = $(this).text();
            html += `<option value="${val}" ${val == selectedValue ? 'selected' : ''}>${text}</option>`;
        });
        return html;
    }

    function initializeFlatpickr() {
        $('.flatpickr-input').each(function () {
            const $input = $(this);
            const effectiveDate = $input.data('effective-date');

            // Initialize Flatpickr
            const fp = flatpickr(this, {
                altInput: true,
                altFormat: "F Y",
                dateFormat: "Y-m-d",
                plugins: [
                    new monthSelectPlugin({
                        shorthand: true,
                        dateFormat: "Y-m-d",
                        altFormat: "F Y",
                        theme: "light"
                    })
                ]
            });

            // Set the date if it exists
            if (effectiveDate) {
                try {
                    const date = new Date(effectiveDate);
                    if (!isNaN(date.getTime())) {
                        const year = date.getFullYear();
                        const month = String(date.getMonth() + 1).padStart(2, '0');
                        const day = String(date.getDate()).padStart(2, '0');
                        const formattedDate = `${year}-${month}-${day}`;

                        // Set the date
                        fp.setDate(formattedDate, true);
                    }
                } catch (e) {
                    console.error('Failed to set date:', e.message);
                }
            }
        });
    }


    function FixedValue() {
        $('.fixedPercentageSelect').each(function () {
            const selected = $(this).val();
            // Only set to '1' if no value is selected
            if (!selected || selected === '') {
                $(this).val('1');
            }
            showHide($(this), $(this).val());
        });
    }

    $(document).on('change', '.fixedPercentageSelect', function () {
        const selected = $(this).val();
        const $parent = $(this).closest('.input-group');
        const $calcTypeHidden = $parent.find('.calculationTypeHidden');

        // Update hidden input value
        if ($calcTypeHidden.length) {
            $calcTypeHidden.val(selected);
        }

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

    // Add new row
    $(document).on('click', '.addRow', function () {
        let container = $(this).closest('.houseRentContainer');
        let rows = container.find('.houseRentRow');
        let newIndex = rows.length;

        // Clone first row
        let newRow = rows.first().clone();

        // Clear input values
        newRow.find('input[type="text"]').val('');
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

        // Change button from + to -
        newRow.find('.addRow')
            .removeClass('addRow btn-primary')
            .addClass('removeRow btn-danger')
            .html('<i class="bi bi-dash-circle"></i>-');

        // Append new row
        container.append(newRow);

        // Set default to Fixed (1) for new row
        newRow.find('.fixedPercentageSelect').val('1');
        newRow.find('.fixedRate').show();
    });

    // Remove row
    $(document).on('click', '.removeRow', function () {
        $(this).closest('.houseRentRow').remove();
    });

    // Initially Loaded
    var id = $("#OrganizationID").val();
    if (id) {
        loadAllowanceTypes(id);
    } else {
        toastr.info('No Organization Id Found');
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
            url: '/PayRollEmpBenefitsUpdate/SelectAllowanceTypeAsync',
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

                        // Check if benefit data exists for this type
                        let hasBenefit = item.empBenefitVMM && item.empBenefitVMM.length > 0;
                        let benefit = hasBenefit ? item.empBenefitVMM[0] : null;

                        // Get BenefitID and other values
                        let benefitId = benefit ? benefit.benefitID : 0;
                        let effectiveDate = benefit ? benefit.effectiveDate : '';
                        let isActive = benefit ? benefit.isActive : false;
                        let benefitSetups = benefit && benefit.benefitSetups ? benefit.benefitSetups : [];

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
                            <div class="card shadow-sm rounded-3 mb-1">
                                <div class="card-body">
                                    <div class="houseRentContainer">
                                        <div class="row">
                                            <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
                                                <input type="hidden" name="Benefits[${i}].BenefitTypeID" value="${item.id}" />
                                                <input type="hidden" name="Benefits[${i}].BenefitID" value="${benefitId}" />

                                                <label class="form-label">Effective Date</label>
                                                <div class="input-icon-end position-relative">
                                                    <input class="form-control ps-6 flatpickr-input"
                                                           name="Benefits[${i}].EffectiveDate"
                                                           type="text" 
                                                           readonly 
                                                           placeholder="Select Month"
                                                           value="${effectiveDate}">
                                                    <span class="uil uil-calendar-alt position-absolute top-50 end-0 translate-middle-y me-3 text-body-tertiary"></span>
                                                </div>
                                            </div>
                                            <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
                                                <div class="form-check form-switch mt-4">
                                                    <input class="form-check-input" 
                                                           type="checkbox" 
                                                           name="Benefits[${i}].IsActive"
                                                           ${isActive ? 'checked' : ''}>
                                                    <label class="form-check-label ms-2">Active</label>
                                                </div>
                                            </div>
                                        </div>`;

                        // Generate rows for benefit setups
                        if (benefitSetups.length > 0) {
                            benefitSetups.forEach(function (setup, setupIndex) {
                                let calculationType = setup.calculationTypeID || '';
                                let setupValue = setup.value || '';

                                accordionHtml += `
                                        <div class="row g-3 houseRentRow">
                                            <div class="col-lg-4 col-md-6 col-sm-12">
                                                <label class="form-label">Min Salary</label>
                                                <input type="text" 
                                                       class="form-control"
                                                       name="Benefits[${i}].BenefitSetups[${setupIndex}].SalaryMin"
                                                       placeholder="Enter Min Salary"
                                                       value="${setup.salaryMin || ''}" />
                                            </div>
                                            <div class="col-lg-4 col-md-6 col-sm-12">
                                                <label class="form-label">Max Salary</label>
                                                <input type="text" 
                                                       class="form-control"
                                                       name="Benefits[${i}].BenefitSetups[${setupIndex}].SalaryMax"
                                                       placeholder="Enter Max Salary"
                                                       value="${setup.salaryMax || ''}" />
                                            </div>
                                            <div class="col-lg-3 col-md-3 col-sm-12 d-flex align-items-center gap-3">
                                                <div class="input-group mb-3">
                                                    <input type="hidden" 
                                                           name="Benefits[${i}].BenefitSetups[${setupIndex}].CalculationTypeID" 
                                                           value="${calculationType}" 
                                                           class="calculationTypeHidden" />
                                                    
                                                    <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
                                                        <option value="">Select One</option>
                                                        <option value="1" ${calculationType == 1 ? 'selected' : ''}>Fixed</option>
                                                        <option value="2" ${calculationType == 2 ? 'selected' : ''}>Percentage</option>
                                                    </select>

                                                    <div class="percentRate" style="display: ${calculationType == 2 ? 'block' : 'none'};">
                                                        <label class="form-label">Percentage(%)</label>
                                                        <select class="form-select choiceDD percentInput"
                                                                name="Benefits[${i}].BenefitSetups[${setupIndex}].Value"
                                                                style="width:160px;">
                                                            <option value="">Select %</option>
                                                            ${getPercentageOptionsHtml(calculationType == 2 ? setupValue : '')}
                                                        </select>
                                                    </div>

                                                    <div class="fixedRate" style="display: ${calculationType == 1 ? 'block' : 'none'};">
                                                        <label class="form-label">Fixed</label>
                                                        <input type="text" 
                                                               class="form-control fixedInput"
                                                               name="Benefits[${i}].BenefitSetups[${setupIndex}].Value"
                                                               placeholder="Enter Fixed Rate"
                                                               style="max-width: 185px;"
                                                               value="${calculationType == 1 ? setupValue : ''}">
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-lg-1 px-0 col-md-1 col-sm-12">
                                                ${setupIndex === 0 ? `
                                                <button type="button" class="btn btn-primary px-3 mt-4 addRow">
                                                    <i class="bi bi-plus-circle"></i>+
                                                </button>` : `
                                                <button type="button" class="btn btn-danger px-3 mt-4 removeRow">
                                                    <i class="bi bi-dash-circle"></i>-
                                                </button>`}
                                            </div>
                                        </div>`;
                            });
                        } else {
                            // Default empty row if no setups exist
                            accordionHtml += `
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
                                                <div class="input-group mb-3">
                                                    <input type="hidden" 
                                                           name="Benefits[${i}].BenefitSetups[0].CalculationTypeID" 
                                                           value="" 
                                                           class="calculationTypeHidden" />
                                                    
                                                    <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
                                                        <option value="">Select One</option>
                                                        <option value="1">Fixed</option>
                                                        <option value="2">Percentage</option>
                                                    </select>
                                                    
                                                    <div class="percentRate" style="display: none;">
                                                        <label class="form-label">Percentage(%)</label>
                                                        <select class="form-select choiceDD percentInput" 
                                                                name="Benefits[${i}].BenefitSetups[0].Value" 
                                                                style="width:160px;">
                                                            <option value="">Select %</option>
                                                            ${getPercentageOptionsHtml('')}
                                                        </select>
                                                    </div>
                                                    
                                                    <div class="fixedRate" style="display: none;">
                                                        <label class="form-label">Fixed</label>
                                                        <input type="text" class="form-control fixedInput" 
                                                               name="Benefits[${i}].BenefitSetups[0].Value" 
                                                               placeholder="Enter Fixed Rate" 
                                                               style="max-width: 185px;">
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="col-lg-1 px-0 col-md-1 col-sm-12">
                                                <button type="button" class="btn btn-primary px-3 mt-4 addRow">
                                                    <i class="bi bi-plus-circle"></i>+
                                                </button>
                                            </div>
                                        </div>`;
                        }

                        accordionHtml += `
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
                console.error(err);
                toastr.error('Error loading allowance types');
            }
        });
    }

    // Remove the current validation and replace with this:
    $(document).on("input", ".fixedInput", function (e) {
        let input = $(this);
        let oldValue = input.val();
        let newValue = oldValue.replace(/[^0-9.]/g, ''); // Allow digits and decimal point

        // Ensure only one decimal point
        let parts = newValue.split('.');
        if (parts.length > 2) {
            newValue = parts[0] + '.' + parts.slice(1).join('');
        }

        if (oldValue !== newValue) {
            input.val(newValue);
            toastr.warning("Only numbers are allowed!");
        }
    });

    // Also add validation on keypress to prevent invalid characters
    $(document).on("keypress", ".fixedInput", function (e) {
        let charCode = e.which ? e.which : e.keyCode;
        // Allow: backspace, delete, tab, escape, enter, decimal point
        if (charCode === 46 || charCode === 8 || charCode === 9 || charCode === 27 || charCode === 13) {
            return true;
        }
        // Allow: Ctrl+A, Ctrl+C, Ctrl+V, Ctrl+X
        if ((charCode === 65 || charCode === 67 || charCode === 86 || charCode === 88) && (e.ctrlKey === true || e.metaKey === true)) {
            return true;
        }
        // Ensure that it is a number and stop the keypress
        if (charCode < 48 || charCode > 57) {
            e.preventDefault();
            toastr.warning("Only numbers are allowed!");
            return false;
        }
    });

    // End Iniatially Loaded 

    

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
                    loadAllowanceTypes(id);
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

    
    


});

