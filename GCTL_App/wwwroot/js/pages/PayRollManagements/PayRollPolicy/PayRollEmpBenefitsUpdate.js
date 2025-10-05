
$(document).ready(function () {

    var percentageOptionsHtml = $("#percentageOptionsTemplate").html();
    initializeFlatpickr();
   

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

        if (selected == "1") {  // Fixed
            $fixedRate.show();
            $percentRate.hide();
            $calcTypeInput.val(1);
        } else if (selected == "2") {  // Percentage
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
            .addClass('removeRow btn btn-danger')
            .text('-');

        // Append new row
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

                                <div class="card shadow-sm rounded-3 mb-1 houseRentRow">
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
                                                                    style="max-width:185px;">
                                                                <option value="">Select %</option>
                                                                ${percentageOptionsHtml}
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
                                                        <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
                                                            <option value="">Select One</option>
                                                           <option value="1" ${setup.calculationTypeID == 1 ? "selected" : ""}>Fixed</option>
                                                            <option value="2" ${setup.calculationTypeID == 2 ? "selected" : ""}>Percentage</option>

                                                        </select>

                                                        <div class="percentRate" style="display: none;">
                                                            <label class="form-label">Percentage(%)</label>
                                                            <select class="form-select choiceDD percentInput"
                                                                    name="Benefits[${i}].BenefitSetups[0].Value"
                                                                    style="max-width:185px;">
                                                                <option value="">Select %</option>
                                                                ${percentageOptionsHtml}
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

                // Set selected percentage values after DOM is ready
                if (res && res.length > 0) {
                    res.forEach(function (item, i) {
                        let benefit = item.empBenefitVMM && item.empBenefitVMM.length > 0 ? item.empBenefitVMM[0] : null;
                        let benefitSetups = benefit && benefit.benefitSetups ? benefit.benefitSetups : [];

                        benefitSetups.forEach(function (setup, setupIndex) {
                            if (setup.calculationTypeID == 2 && setup.value) {
                                $(`select[name="Benefits[${i}].BenefitSetups[${setupIndex}].Value"]`).val(setup.value);
                            }
                        });
                    });
                }

            },
            error: function (err) {
                console.error(err);
            }
        });
    }

   
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

   


    //#region Delete Soft Leave Request
    $(document).on('click', '#SoftDeletePayRollEmpAllowanceDelete-singleDelBtn', function () {
        var id = $(this).data('id');
        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/PayRollEmpBenefitsUpdate/SoftDeletePayRollEmpAllowance',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {

                        if (response.success) {
                            toastr.success(response.message);
                            loadTableData();
                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function () {
                        toastr.error("Error occurred while deleting.");
                    }
                });
            });
        } else {
            toastr.error("Invalid action.");
        }
    });
    //#endregion

    


});

