
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
            dateFormat: "Y-m",
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
            url: '/PayRollEmployeesAllowance/GetHouseRentAllowanceRow',
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
            url: '/PayRollEmployeesAllowance/GetHouseRentAllowanceRow',
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
        return toastr.error('No Organization Id Found');
    }

    $(document).on('change', '#OrganizationID', function () {
        var id = $(this).val();
        if (!id)
        {
            return toastr.error('No Organization Id Found');
        }
        loadAllowanceTypes(id);
    });
        function loadAllowanceTypes(id) {
            $.ajax({
                url: '/PayRollEmployeesAllowance/SelectAllowanceTypeAsync',
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
                                                        <input type="hidden" name="HouseRentAllowances[${i}].EmployeeAllowanceTypeID" value="${item.id}" />

                                                        <label class="form-label">Effective Date</label>
                                                        <div class="input-icon-end position-relative">
                                                            <input class="form-control ps-6 flatpickr-input"
                                                                   name="HouseRentAllowances[${i}].EffectiveDate"
                                                                   type="text" readonly placeholder="Select Month">
                                                            <span class="uil uil-calendar-alt position-absolute top-50 end-0 translate-middle-y me-3 text-body-tertiary"></span>
                                                        </div>
                                                    </div>
                                                    <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
                                                        <div class="form-check form-switch mt-4">
                                                            <input class="form-check-input" type="checkbox" name="HouseRentAllowances[${i}].IsActive">
                                                            <label class="form-check-label ms-2">Active</label>
                                                        </div>
                                                    </div>
                                                </div>

                                                <div class="row g-3 houseRentRow">
                                                    <div class="col-lg-4 col-md-6 col-sm-12">
                                                        <label class="form-label">Min Salary</label>
                                                        <input type="text" class="form-control" name="HouseRentAllowances[${i}].SalaryMin" placeholder="Enter Min Salary" />
                                                    </div>
                                                    <div class="col-lg-4 col-md-6 col-sm-12">
                                                        <label class="form-label">Max Salary</label>
                                                        <input type="text" class="form-control" name="HouseRentAllowances[${i}].SalaryMax" placeholder="Enter Max Salary" />
                                                    </div>
                                                    <div class="col-lg-3 col-md-3 col-sm-12 d-flex align-items-center gap-3">
                                                     <input type="hidden" name="HouseRentAllowances[${i}].CalculationTypeID" />
                                                        <div class="input-group mb-3">
                                                            <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
                                                                <option value="">Select One</option>
                                                                <option value="Fixed">Fixed</option>
                                                                <option value="Percentage">Percentage</option>
                                                            </select>

                                                            <div class="percentRate" style="display: none;">
                                                                <label class="form-label">Percentage(%)</label>
                                                                <select class="form-select choiceDD"  style="max-width:185px;">
                                                                    <option value="">Select %</option>
                                                                </select>
                                                            </div>
                                                            <div class="fixedRate" style="display: none;">
                                                                <label class="form-label">Fixed</label>
                                                                <input type="text" class="form-control" name="HouseRentAllowances[${i}].Value" placeholder="Enter Fixed Rate" style="max-width: 185px;">
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
                                                    <button class="btn btn-primary px-5 PayRollEmpAllowanceSave">Save</button>
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
                        accordionHtml = '<p class="text-warning">No allowance types found for this organization.</p>';
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


    //
    // Function to generate dynamic row HTML
    //function getHouseRentRowHtml(allowanceIndex, rowIndex) {
    //    return `
    //<div class="row g-3 houseRentRow" data-allowance-index="${allowanceIndex}" data-row-index="${rowIndex}">
    //    <div class="col-lg-4 col-md-6 col-sm-12">
    //        <label class="form-label">Min Salary</label>
    //        <input type="text" class="form-control" name="HouseRentAllowances[${allowanceIndex}][${rowIndex}].SalaryMin" placeholder="Enter Min Salary" />
    //    </div>
    //    <div class="col-lg-4 col-md-6 col-sm-12">
    //        <label class="form-label">Max Salary</label>
    //        <input type="text" class="form-control" name="HouseRentAllowances[${allowanceIndex}][${rowIndex}].SalaryMax" placeholder="Enter Max Salary" />
    //    </div>
    //    <div class="col-lg-3 col-md-3 col-sm-12 d-flex align-items-center gap-3">
    //        <input type="hidden" name="HouseRentAllowances[${allowanceIndex}][${rowIndex}].CalculationTypeID" />
    //        <div class="input-group mb-3">
    //            <select class="form-select mt-4 fixedPercentageSelect" style="max-width: 125px;height:37px;">
    //                <option value="">Select One</option>
    //                <option value="Fixed">Fixed</option>
    //                <option value="Percentage">Percentage</option>
    //            </select>
    //            <div class="percentRate" style="display: none;">
    //                <label class="form-label">Percentage(%)</label>
    //                <select class="form-select choiceDD" style="max-width:185px;">
    //                    <option value="">Select %</option>
    //                </select>
    //            </div>
    //            <div class="fixedRate" style="display: none;">
    //                <label class="form-label">Fixed</label>
    //                <input type="text" class="form-control" name="HouseRentAllowances[${allowanceIndex}][${rowIndex}].Value" placeholder="Enter Fixed Rate" style="max-width: 185px;">
    //            </div>
    //        </div>
    //    </div>
    //    <div class="col-lg-1 px-0 col-md-1 col-sm-1">
    //        <button type="button" class="btn btn-primary px-3 mt-4 addRow">
    //            <i class="bi bi-plus-circle"></i>+
    //        </button>
    //    </div>
    //</div>`;
    //    initializeFlatpickr();
    //    FixedValue();
    //}

    // Main function
    //function loadAllowanceTypes(id) {
    //    $.ajax({
    //        url: '/PayRollEmployeesAllowance/SelectAllowanceTypeAsync',
    //        type: 'GET',
    //        data: { id: id },
    //        dataType: 'json',
    //        success: function (res) {
    //            console.log("Allowance Types Fetched:", res);

    //            let accordionHtml = '';

    //            if (res && res.length > 0) {
    //                accordionHtml += '<div class="accordion" id="accordionAllowance">';

    //                res.forEach(function (item, i) {
    //                    let collapseId = `collapse-${i}`;
    //                    let headingId = `heading-${i}`;
    //                    let isFirst = i === 0;
    //                    let buttonClass = isFirst ? "accordion-button" : "accordion-button collapsed";
    //                    let collapseClass = isFirst ? "accordion-collapse collapse show" : "accordion-collapse collapse";
    //                    let ariaExpanded = isFirst.toString();

    //                    accordionHtml += `
    //            <div class="accordion-item">
    //                <h2 class="accordion-header" id="${headingId}">
    //                    <button class="${buttonClass}" type="button"
    //                            data-bs-toggle="collapse"
    //                            data-bs-target="#${collapseId}"
    //                            aria-expanded="${ariaExpanded}"
    //                            aria-controls="${collapseId}">
    //                        ${item.name}
    //                    </button>
    //                </h2>
                  
    //                <div id="${collapseId}" class="${collapseClass}" aria-labelledby="${headingId}">
    //                    <div class="accordion-body">
    //                        <div class="card shadow-sm rounded-3 mb-1">
    //                            <div class="card-body">
    //                                <div class="houseRentContainer">
    //                                    <div class="row">
    //                                        <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
    //                                            <input type="hidden" name="HouseRentAllowances[${i}].EmployeeAllowanceTypeID" value="${item.id}" />
                                               
    //                                            <label class="form-label">Effective Date</label>
    //                                            <div class="input-icon-end position-relative">
    //                                                <input class="form-control ps-6 flatpickr-input"
    //                                                       name="HouseRentAllowances[${i}].EffectiveDate"
    //                                                       type="text" readonly placeholder="Select Month">
    //                                                <span class="uil uil-calendar-alt position-absolute top-50 end-0 translate-middle-y me-3 text-body-tertiary"></span>
    //                                            </div>
    //                                        </div>
    //                                        <div class="col-lg-4 col-md-6 col-sm-12 mb-3">
    //                                            <div class="form-check form-switch mt-4">
    //                                                <input class="form-check-input" type="checkbox" name="HouseRentAllowances[${i}].IsActive">
    //                                                <label class="form-check-label ms-2">Active</label>
    //                                            </div>
    //                                        </div>
    //                                    </div>
    //                                   ${getHouseRentRowHtml(i, 0)}
                                
    //                        </div>
    //                                </div>
    //                                <div class="row g-3 justify-content-end">
    //                                    <div class="col-auto">
    //                                        <button class="btn btn-primary px-5 PayRollEmpAllowanceSave">Save</button>
    //                                    </div>
    //                                </div>
    //                            </div>
    //                        </div>
    //                    </div>
    //                </div>
    //            </div>`;
    //                });

    //                accordionHtml += '</div>';

    //            } else {
    //                accordionHtml = '<p class="text-warning">No allowance types found for this organization.</p>';
    //            }

    //            $('#EmployeeAllowanceAccordion').html(accordionHtml);
    //            initializeFlatpickr();
    //            FixedValue();
    //        },
    //        error: function (err) {
    //            toastr.error('Failed to fetch allowance types');
    //            console.error(err);
    //        }
    //    });
    //}

    //// Add Row Click Handler (Dynamic)
    //$(document).on('click', '.addRow', function () {
    //    let container = $(this).closest('.houseRentContainer');
    //    let allowanceIndex = container.find('[name^="HouseRentAllowances"]').first().attr('name').match(/\d+/)[0];
    //    let rowCount = container.find('.houseRentRow').length;
    //    container.append(getHouseRentRowHtml(allowanceIndex, rowCount));
    //});

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

    $(document).on("click", '.PayRollEmpAllowanceSave', function (e) {
        e.preventDefault();
        if (!validateOrganization()) {
            return;
        }
        const $container = $(this).closest('.card'); 
        const formData = new FormData();
        formData.append("OrganizationID", $("select[name='OrganizationID']").val());
        const employeeAllowanceTypeID = $container.find("input[type='hidden']").val();
        formData.append("EmployeeAllowanceTypeID", employeeAllowanceTypeID);
        formData.append("EffectiveDate", $container.find("[name='EffectiveDate']").val());
        formData.append("IsActive", $container.find("[name='IsActive']").is(":checked"));

        

        $container.find(".houseRentRow").each(function (index) {
            const $row = $(this);
            const calculationType = $row.find(".fixedPercentageSelect").val();
           
            formData.append(`HouseRentAllowances[${index}].SalaryMin`, $row.find("[name*='SalaryMin']").val());
            formData.append(`HouseRentAllowances[${index}].SalaryMax`, $row.find("[name*='SalaryMax']").val());
            let value;
            if (calculationType === "Fixed") {
                value = $row.find(".fixedRate input").val();
            } else if (calculationType === "Percentage") {
                value = $row.find(".percentRate select").val(); 
            }
            formData.append(`HouseRentAllowances[${index}].Value`, value);

            const calculationTypeID = calculationType === "Fixed" ? 1 : 2;
            formData.append(`HouseRentAllowances[${index}].CalculationTypeID`, calculationTypeID);

        });
            
            $.ajax({
                url: '/PayRollEmployeesAllowance/SavePayRollEmpAlowance',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        resetPayRollEmpAllowanceForm();
                        loadPayRollEmpAllowanceData();
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
            url: '/PayRollEmployeesAllowance/GetAllDataAfterSave',
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
                            } catch (e)
                            {
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
            url: '/PayRollEmployeesAllowance/UpdatePayRollEmpAllowance',   
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
                    url: '/PayRollEmployeesAllowance/SoftDeletePayRollEmpAllowance',
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

    // #region  Data Table for Peresonal
    //var currentPage = 1;
    //var pageSize = 5;

    //$('#payRollEmp-pageSizeSelect').on('change', function () {
    //    var selectedSize = $(this).val();

    //    if (selectedSize) {
    //        pageSize = parseInt(selectedSize, 10);
    //        currentPage = 1;
    //        loadTableData();
    //    }
    //});

    //$(document).ready(function () {
    //    loadTableData();

    //    $("#payRollEmp-searchInput").on("input", function () {
    //        currentPage = 1;
    //        loadTableData();
    //    });

    //    $("#payRollEmp-prevPageBtn").on('click', function () {
    //        if (currentPage > 1) {
    //            currentPage--;
    //            loadTableData();
    //        }
    //    });

    //    $("#payRollEmp-nextPageBtn").on('click', function () {
    //        currentPage++;
    //        loadTableData();
    //    });
    //});
    //let currentSortColumn = '';
    //let currentSortOrder = '';

    //$('th.sort').on('click', function () {
    //    const column = $(this).data('sort');
    //    if (currentSortColumn === column) {
    //        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    //    } else {
    //        currentSortColumn = column;
    //        currentSortOrder = 'asc';
    //    }

    //    loadTableData(currentSortColumn, currentSortOrder);
    //    updateSortingIndicator(column, currentSortOrder);
    //});
    //function updateSortingIndicator() {
    //    $('th.sort').each(function () {
    //        const $th = $(this);
    //        const column = $th.data('sort');
    //        $th.find('.sort-icon').remove();

    //        if (column === currentSortColumn) {
    //            const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    //            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
    //        } else {
    //            $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
    //        }
    //    });
    //}

    //$(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

    //    currentPage = 1;
    //    loadTableData();
    //});

    //$('#OrganizationIDD').on('changed.coreui.multi-select', function () {
    //    currentPage = 1;
    //    loadTableData(); // Make AJAX call or reload the table
    //});

    //// Filtering according to formdate to ToDate

    //function loadTableData(currentSortColumn, currentSortOrder) {
    //    var searchTerm = $("#payRollEmp-searchInput").val();
    //    const organizationId = $('#OrganizationIDD').val();

    //    $.ajax({
    //        url: '/PayRollEmployeesAllowance/GetAllTableListAsync',
    //        method: 'GET',
    //        traditional: true,
    //        data: {
    //            pageNumber: currentPage,
    //            pageSize: pageSize,
    //            searchTerm: searchTerm,
    //            currentSortColumn: currentSortColumn,
    //            currentSortOrder: currentSortOrder,
    //            organizationId: organizationId,
    //        },
    //        success: function (response) {



    //            console.log("Datassssss", response);
    //            var tableBody = $("#PayRollEmployeeAllowance-body");
    //            tableBody.empty();
    //            var totalItems = response.paginationInfo.totalItems;

    //            if (response.data.length > 0) {
    //                response.data.forEach(function (item, index) {

    //                    if (currentSortOrder === 'asc') {
    //                        rowIndex = (currentPage - 1) * pageSize + index + 1;
    //                    } else {
    //                        rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
    //                    }

    //                    tableBody.append(`
    //                  <tr class="hover-actions-trigger btn-reveal-trigger position-static">
    //                        <td class="companyName align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
    //                            <span>${item.organizationName}</span>
    //                        </td>
    //                        <td class="helthInsurance align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mobileInternetAllowance || ''} tk</td>
    //                        <td class="providantFundEC align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.shiftAllowance}%</td>
    //                        <td class="providantFundOC align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.houseRentAllowanceRate}%</td>
    //                        <td class="providantFundT align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.hRentDependsOnSalaryTypeIDName}</td>
    //                        <td class="providantFundS align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.medicalAllowanceRate || ''}</td>
    //                        <td class="FestivalBonusR align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.mediAllowDepOnSalaryTypeIDName || ''}</td>
    //                        <td class="FestivalBonusS align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.conveyanceAllowanceRate || ''}</td>
    //                        <td class="align-middle white-space-nowrap text-end pe-3">
    //                          <div class="d-flex justify-content-end align-items-center">
    //                              <a href="#" class="nav-item mx-2" title="Edit" data-bs-toggle="modal" data-bs-target="#edit_alloance"  id="EditEmpAllowance" data-id="${item.employeeAllowanceID}">
    //                                  <i class="fas fa-edit text-black"></i>
    //                              </a>

    //                              <a href="#" title="Delete" data-id="${item.employeeAllowanceID}"
    //                                 class="btn btn-outline-light btn-icon"
    //                                 id="SoftDeletePayRollEmpAllowanceDelete-singleDelBtn">
    //                                  <i class="far fa-trash-alt text-black"></i>
    //                              </a>
    //                          </div>
    //                       </td>

    //                    </tr>
    //               `);
    //                });
    //            } else {
    //                tableBody.append('<tr><td colspan="10" class="text-center">No data available</td></tr>');
    //            }

    //            var paginationInfo = response.paginationInfo;

    //            $("#payRollEmp-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
    //            $("#payRollEmp-totalCount").text(`(${paginationInfo.totalItems})`);

    //            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
    //        }
    //    });
    //}

    //function updatePagination(pageNumbers, currentPage, totalPages) {
    //    const paginationLinks = $("#payRollEmp-paginationLinks");
    //    paginationLinks.empty();
    //    // Window size (number of pages before/after the current page)
    //    const windowSize = 1;

    //    const createPageButton = (page) => `
    //            <li class="page-item ${page === currentPage ? 'active' : ''}">
    //                <button class="page-link page-btn" data-page="${page}">${page}</button>
    //            </li>
    //        `;

    //    // Helper function for ellipsis
    //    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
    //    // Add "First Page" and ellipsis if needed
    //    if (currentPage > windowSize + 1) {
    //        paginationLinks.append(createPageButton(1), addEllipsis());
    //    }
    //    // Add page number buttons within the window range
    //    const startPage = Math.max(1, currentPage - windowSize);
    //    const endPage = Math.min(totalPages, currentPage + windowSize);
    //    for (let i = startPage; i <= endPage; i++) {
    //        paginationLinks.append(createPageButton(i));
    //    }
    //    // Add ellipsis and "Last Page" button if needed
    //    if (currentPage < totalPages - windowSize) {
    //        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    //    }
    //    // Disable or enable previous/next buttons
    //    $("#payRollEmp-prevPageBtn").prop('disabled', currentPage === 1);
    //    $("#payRollEmp-nextPageBtn").prop('disabled', currentPage === totalPages);
    //}

    //$(document).on('click', '.page-btn', function () {
    //    const page = $(this).data('page');
    //    currentPage = page;
    //    loadTableData();
    //});
    //#endregion


});

