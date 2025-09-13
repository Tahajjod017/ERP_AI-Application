
$(document).ready(function () {


    FixedValue();
    initializeFlatpickr();
    function FixedValue() {
        $('.fixedPercentageSelect').val('Fixed');
        $('.fixedPercentageSelect').each(function () {
            const selected = $(this).val();
            showHide($(this), selected);
        });
    };
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

        if (selected === 'Fixed') {
            $fixedRate.show();
            $percentRate.hide();
        } else if (selected === 'Percentage') {
            $percentRate.show();
            $fixedRate.hide();
        } else {
            $fixedRate.hide();
            $percentRate.hide();
        }
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

    // Show/Hide Fixed/Percentage fields
    $(document).on('change', '.fixedPercentageSelect', function () {
        const selected = $(this).val();
        const $parent = $(this).closest('.input-group');
        const $fixedRate = $parent.find('.fixedRate');
        const $percentRate = $parent.find('.percentRate');

        if (selected === 'Fixed') {
            $fixedRate.show();
            $percentRate.hide();
        } else if (selected === 'Percentage') {
            $percentRate.show();
            $fixedRate.hide();
        } else {
            $fixedRate.hide();
            $percentRate.hide();
        }
    });

    // Add new row
    $(document).on('click', '.addRow', function () {
        const $container = $(this).closest('.houseRentContainer');
        let index = $container.find(".houseRentRow").length;

        $.ajax({
            url: '/PayRollEmpBenefitsUpdate/GetHouseRentAllowanceRow',
            type: 'GET',
            data: { index: index },
            success: function (response) {

                $container.find('.addRow').closest('div.col-lg-12').remove();

                // Append new row with X button
                $container.append(response);

                const $newRow = $container.find('.houseRentRow').last();
                $newRow.find('.removeRow').show(); // make sure X button is visible
                $newRow.find('.fixedPercentageSelect').val('Fixed').trigger('change');
            },
            error: function () {
                toastr.error("Failed to add new row.");
            }
        });
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
                   name="Benefits[${i}].BenefitSetups[0].EffectiveDate"
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
                                                    <input type="hidden"
               name="Benefits[${i}].BenefitSetups[0].CalculationTypeID" />
                                                        <div class="input-group mb-3">
                                                            <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
                                                                <option value="">Select One</option>
                                                                <option value="Fixed">Fixed</option>
                                                                <option value="Percentage">Percentage</option>
                                                            </select>

                                                            <div class="percentRate" style="display: none;">
                                                                <label class="form-label">Percentage(%)</label>
                                                                 <select class="form-select choiceDD"
                        name="Benefits[${i}].BenefitSetups[0].Value" 
                        style="max-width:185px;">
                    <option value="">Select %</option>
                </select>
                                                            </div>
                                                            <div class="fixedRate" style="display: none;">
                                                                <label class="form-label">Fixed</label>
                                                                <input type="text" class="form-control"
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
                toastr.error('Failed to fetch allowance types');
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

        // Find the accordion card for the clicked Save button
        let $card = $(this).closest('.accordion-item');
        let benefitIndex = $card.index(); // Index of the card

        // Initialize JSON data
        let jsonData = {
            OrganizationID: parseInt($('#OrganizationID').val()) || 0,
            Benefits: []
        };

        // Collect data only for this card
        let benefit = {
            BenefitID: parseInt($card.find(`input[name="Benefits[${benefitIndex}].BenefitID"]`).val()) || 0,
            BenefitTypeID: parseInt($card.find(`input[name="Benefits[${benefitIndex}].BenefitTypeID"]`).val()) || 0,
            IsActive: $card.find(`input[name="Benefits[${benefitIndex}].IsActive"]`).is(':checked'),
            BenefitSetups: []
        };

        // Loop only through the rows inside this card
        $card.find('.houseRentRow').each(function (setupIndex) {
            let dateStr = $(this).find(`input[name="Benefits[${benefitIndex}].BenefitSetups[${setupIndex}].EffectiveDate"]`).val();
            let effectiveDate = dateStr ? new Date(dateStr).toISOString().split('T')[0] : null;

            let setup = {
                BenefitSetupID: 0,
                EffectiveDate: effectiveDate,
                SalaryMin: parseFloat($(this).find(`input[name="Benefits[${benefitIndex}].BenefitSetups[${setupIndex}].SalaryMin"]`).val()) || 0,
                SalaryMax: parseFloat($(this).find(`input[name="Benefits[${benefitIndex}].BenefitSetups[${setupIndex}].SalaryMax"]`).val()) || 0,
                CalculationTypeID: parseInt($(this).find(`input[name="Benefits[${benefitIndex}].BenefitSetups[${setupIndex}].CalculationTypeID"]`).val()) || 0,
                Value: parseFloat($(this).find(`[name="Benefits[${benefitIndex}].BenefitSetups[${setupIndex}].Value"]`).val()) || 0
            };

            benefit.BenefitSetups.push(setup);
        });

        jsonData.Benefits.push(benefit);

        console.log("Sending JSON for only this card:", JSON.stringify(jsonData, null, 2));

        // Send via AJAX
        $.ajax({
            url: '/PayRollEmpBenefitsUpdate/Save',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(jsonData),
            success: function (res) {
                if (res.success) toastr.success("Benefit saved successfully");
                else toastr.error(res.message || "Save failed");
            },
            error: function (err) {
                toastr.error("Error while saving");
                console.error(err);
            }
        });
    });

    //

    function resetPayRollEmpAllowanceForm() {
        const $form = $('#payrollEmpAllowanceForm');

        // Reset all form fields
        $form[0].reset();
        choiceManager.resetChoice('OrganizationID');
        $form.find('.flatpickr-input').each(function () {
            if (this._flatpickr) {
                this._flatpickr.clear();
            }
        });


    }

    $(document).on('click', '#ResetButton', function (e) {
        e.preventDefault();
        resetPayRollEmpAllowanceForm();

    })

    function resetPayRollEmpAllowanceFormEdit() {

        var form = $("#payrollEmpAllowanceFormUpdate");
        form[0].reset();


    }


    // Edit Button Click
    $(document).ready(function () {
        loadPayRollEmpAllowanceData();
    });

    function loadPayRollEmpAllowanceData() {
        $.ajax({
            url: '/PayRollEmpBenefitsUpdate/GetAllDataAfterSave',
            type: 'GET',
            success: function (res) {
                if (res.success && res.data && res.data.length > 0) {
                    res.data.forEach((item, index) => {
                        console.log("Get All Data", item);
                        // --- Effective Date ---
                        const $effectiveDateInput = $(`input[name="HouseRentAllowances[${index}].EffectiveDate"]`);
                        if ($effectiveDateInput.length && item.effectiveDate) {
                            try {
                                const date = new Date(item.effectiveDate);
                                if (!isNaN(date.getTime())) {
                                    const monthNames = ["January", "February", "March", "April", "May", "June",
                                        "July", "August", "September", "October", "November", "December"];
                                    const formattedDate = `${monthNames[date.getMonth()]} ${date.getFullYear()}`;
                                    $effectiveDateInput.val(formattedDate);
                                    if ($effectiveDateInput[0]._flatpickr) {
                                        $effectiveDateInput[0]._flatpickr.setDate(formattedDate, true);
                                    }
                                }
                            } catch (e) {
                                console.error(`Failed to set date for index ${index}:`, e.message);
                            }
                        }

                        // --- IsActive ---
                        const $isActiveCheckbox = $(`input[name="HouseRentAllowances[${index}].IsActive"]`);
                        if ($isActiveCheckbox.length && item.hasOwnProperty('isActive')) {
                            $isActiveCheckbox.prop('checked', item.isActive === true || item.isActive === 'true');
                        }

                        // --- EmployeeAllowanceID (hidden input) ---
                        const $allowanceIdInput = $('input[name="EmployeeAllowanceID"]');
                        if ($allowanceIdInput.length && item.hasOwnProperty('employeeAllowanceID')) {
                            $allowanceIdInput.val(item.employeeAllowanceID);
                        }


                        const $row = $(`#payrollEmpAllowanceForm .houseRentRow`).eq(index);

                        if (item.houseRentAllowances && item.houseRentAllowances.length > 0) {
                            const hra = item.houseRentAllowances[0];

                            // Min Salary
                            const $minSalaryInput = $row.find(`[name="HouseRentAllowances[${index}].SalaryMin"]`);
                            if ($minSalaryInput.length && hra.salaryMin != null) {
                                $minSalaryInput.val(hra.salaryMin);
                            }

                            // Max Salary
                            const $maxSalaryInput = $row.find(`[name="HouseRentAllowances[${index}].SalaryMax"]`);
                            if ($maxSalaryInput.length && hra.salaryMax != null) {
                                $maxSalaryInput.val(hra.salaryMax);
                            }

                            // CalculationTypeID
                            const $calcTypeInput = $row.find(`[name="HouseRentAllowances[${index}].CalculationTypeID"]`);
                            if ($calcTypeInput.length && hra.calculationTypeID != null) {
                                $calcTypeInput.val(hra.calculationTypeID);
                            }

                            // Fixed / Percentage handling
                            const $fixedSelect = $row.find('.fixedPercentageSelect');
                            const $fixedRateInput = $row.find(`input[name="HouseRentAllowances[${index}].Value"]`);
                            const $percentRateSelect = $row.find('.percentRate select.choiceDD');

                            let calcTypeStr = '';
                            if (hra.calculationTypeID == 1) calcTypeStr = 'Fixed';
                            else if (hra.calculationTypeID == 2) calcTypeStr = 'Percentage';

                            if ($fixedSelect.length && calcTypeStr) {
                                $fixedSelect.val(calcTypeStr);

                                $row.find('.fixedRate').toggle(calcTypeStr === 'Fixed');
                                $row.find('.percentRate').toggle(calcTypeStr === 'Percentage');

                                if (calcTypeStr === 'Fixed' && $fixedRateInput.length) {
                                    $fixedRateInput.val(hra.value);
                                } else if (calcTypeStr === 'Percentage' && $percentRateSelect.length) {
                                    $percentRateSelect.val(hra.value);
                                }
                            }
                        }




                    });

                    toastr.success(res.message || "Data loaded successfully");
                } else {
                    toastr.warning(res.message || "No data found.");
                }
            },


            error: function (err) {
                console.error("Error fetching data:", err);
                toastr.error("Failed to load payroll employee allowance data.");
            }
        });
    }



    // Initialize dropdown states
    $('.fixedPercentageSelect').each(function () {
        const selected = $(this).val();
        showHide($(this), selected);
    });



    // Save/Update Button
    $(document).on('click', '#PayRollEmpAllowanceUpdate', function (e) {
        e.preventDefault();

        var formData = $('#payrollEmpAllowanceFormUpdate').serialize();

        $.ajax({
            url: '/PayRollEmpBenefitsUpdate/UpdatePayRollEmpAllowance',
            type: 'POST',
            data: formData,
            success: function (response) {
                debugger
                if (response.success) {
                    toastr.success(response.message);
                    resetPayRollEmpAllowanceFormEdit();
                    var applyModalEl = document.getElementById('edit_alloance');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                } else {
                    toastr.error(response.message);
                }
            },
            error: function (xhr, status, error) {
                let msg = "Something went wrong!";
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    msg = xhr.responseJSON.message;
                }
                toastr.error(msg);
            }
        });
    });


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

