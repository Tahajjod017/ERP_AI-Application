
$(document).ready(function () {

    $('.flatpickr-input').flatpickr({
        mode: "single",
        dateFormat: "F Y",
        altFormat: "F Y",
        plugins: [
            new monthSelectPlugin({
                shorthand: true,
                dateFormat: "F Y",
                altFormat: "F Y",
                theme: "light"
            })
        ]
    });
    //#endregion


    //#region Fixed Percentage

    $('.fixedPercentageSelect').val('Fixed');
    $('.fixedPercentageSelect').each(function () {
        const selected = $(this).val();
        showHide($(this), selected);
    });
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
        const $firstContainer = $(".houseRentContainer").first(); // CHANGED

        $.ajax({
            url: '/PayRollEmployeesAllowance/GetHouseRentAllowanceRow',
            type: 'GET',
            data: { index: 0 },
            success: function (response) {
                $firstContainer.append(response); // CHANGED
                const firstRow = $firstContainer.find(".houseRentRow").first(); // CHANGED
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
                //$container.append(response);
                //const newRow = $container.find('.houseRentRow').last();
                //choiceManager.initAll();
                //newRow.find('.fixedPercentageSelect').val('Fixed').trigger('change');
                $container.find('.addRow').closest('div.col-lg-12').remove();

                // Append new row with X button
                $container.append(response);

                const $newRow = $container.find('.houseRentRow').last();
                $newRow.find('.removeRow').show(); // make sure X button is visible

                // Move + button to last row
                $container.append('<div class="col-lg-12 mt-2 text-end"><button type="button" class="btn btn-primary addRow">+ Add Row</button></div>');

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

    //


   
 

    //
    
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

    function setFormattedChoiceValue(fieldName, value) {
        const formattedValue = Number(value).toFixed(2);
        choiceManager.setChoiceValue(fieldName, formattedValue);
    }

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
                        console.log("Get All Data" + item)
                        let picker = document.querySelector(`#year-month-picker-${index}`);
                        if (picker && item.effectiveDate) {
                            try {
                                const date = new Date(item.effectiveDate);
                                if (isNaN(date.getTime())) throw new Error("Invalid date");
                                // Format to "F Y" (e.g., "May 2025")
                                const monthNames = ["January", "February", "March", "April", "May", "June",
                                    "July", "August", "September", "October", "November", "December"];
                                const formattedDate = `${monthNames[date.getMonth()]} ${date.getFullYear()}`;
                                picker._flatpickr.setDate(formattedDate, true);
                                picker.value = formattedDate;
                            } catch (e) {
                                console.error(`Failed to set date for index ${index}:`, e.message);
                            }
                        }
                        let checkbox = document.querySelector(`#IsActive_${index}`);
                        if (checkbox && item.hasOwnProperty('isActive')) {
                            checkbox.checked = item.isActive === true || item.isActive === 'true';
                        }
                        let allowanceIdInput = document.querySelector(`#EmployeeAllowanceID_${index}`);
                        if (allowanceIdInput && item.hasOwnProperty('employeeAllowanceID'))
                        {
                            allowanceIdInput.value = item.employeeAllowanceID;
                        }
                        debugger
                        let rowIndex = 0;
                        const $container = $('#payrollEmpAllowanceForm');
                        if (item.houseRentAllowances && item.houseRentAllowances.length > 0) {
                            const hra = item.houseRentAllowances[0]; 
                            console.log(`HRA Data:`, JSON.stringify(hra));
                            const $row = $container.find('.houseRentRow').eq(rowIndex);
                            // Set SalaryMin and SalaryMax
                            const $minSalaryInput = $row.find('[name*="SalaryMin"]');
                            if ($minSalaryInput.length && hra.salaryMin != null) {
                                $minSalaryInput.val(hra.salaryMin);
                                console.log(`Set SalaryMin_${rowIndex} to:`, hra.salaryMin);
                            }

                            // Set SalaryMax
                            const $maxSalaryInput = $row.find('[name*="SalaryMax"]');
                            if ($maxSalaryInput.length && hra.salaryMax != null) {
                                $maxSalaryInput.val(hra.salaryMax);
                                console.log(`Set SalaryMax_${rowIndex} to:`, hra.salaryMax);
                            }


                            // Set CalculationType and Value
                            let fixedPercentageSelect = document.querySelector(`#fixedPercentageSelect_${index}`);
                            let fixedRateInput = document.querySelector(`#fixedRate-${index}`);
                            let percentRateSelect = document.querySelector(`#percentRate-${index}`);
                            let hiddenCalcTypeInput = document.querySelector(`#hidden-calculation-type-${index}`);
                            if (fixedPercentageSelect && hra.calculationType) {
                                fixedPercentageSelect.value = hra.calculationType;
                                $(`#percentRateContainer_${index}`).toggle(hra.calculationType === 'Percentage');
                                $(`#fixedRateContainer_${index}`).toggle(hra.calculationType === 'Fixed');
                                if (hiddenCalcTypeInput) hiddenCalcTypeInput.value = hra.calculationType;
                                if (hra.calculationType === 'Fixed' && fixedRateInput) {
                                    fixedRateInput.value = hra.value;
                                } else if (hra.calculationType === 'Percentage' && percentRateSelect) {
                                    percentRateSelect.value = hra.value;
                                }
                            }
                        }

                    });
                    toastr.success(res.message || "Data loaded successfully");
                } else {
                    toastr.warning(res.message || "No data found.");
                }
            },
            //success: function (res) {
            //    console.log('Full res.data:', JSON.stringify(res.data));
            //    if (res.success && res.data && res.data.length > 0) {
            //        const $container = $('#payrollEmpAllowanceForm');
            //        const monthNames = ["January", "February", "March", "April", "May", "June",
            //            "July", "August", "September", "October", "November", "December"];
            //        let rowIndex = 0;

            //        res.data.forEach((item) => {
            //            if (item.houseRentAllowances && item.houseRentAllowances.length > 0) {
            //                item.houseRentAllowances.forEach((hra) => {
            //                    const $row = $container.find('.houseRentRow').eq(rowIndex);
            //                    if (!$row.length) {
            //                        console.warn(`No houseRentRow found for rowIndex ${rowIndex}`);
            //                        return;
            //                    }
            //                    console.log(`Processing row ${rowIndex}, HRA:`, JSON.stringify(hra));

            //                    // Set SalaryMin
            //                    const $minSalaryInput = $row.find('[name*="SalaryMin"]');
            //                    if ($minSalaryInput.length && hra.salaryMin != null) {
            //                        $minSalaryInput.val(hra.salaryMin);
            //                        console.log(`Set SalaryMin_${rowIndex} to:`, hra.salaryMin);
            //                    }

            //                    // Set SalaryMax
            //                    const $maxSalaryInput = $row.find('[name*="SalaryMax"]');
            //                    if ($maxSalaryInput.length && hra.salaryMax != null) {
            //                        $maxSalaryInput.val(hra.salaryMax);
            //                        console.log(`Set SalaryMax_${rowIndex} to:`, hra.salaryMax);
            //                    }

            //                    // Set CalculationType and Value
            //                    const $fixedPercentageSelect = $row.find('.fixedPercentageSelect');
            //                    const $hiddenCalcTypeInput = $row.find(`[name*="CalculationTypeID"]`);
            //                    if ($fixedPercentageSelect.length && hra.calculationType) {
            //                        $fixedPercentageSelect.val(hra.calculationType);
            //                        $(`#percentRateContainer_${rowIndex}`).toggle(hra.calculationType === 'Percentage');
            //                        $(`#fixedRateContainer_${rowIndex}`).toggle(hra.calculationType === 'Fixed');
            //                        const calculationTypeID = hra.calculationType === 'Fixed' ? 1 : hra.calculationType === 'Percentage' ? 2 : '';
            //                        if ($hiddenCalcTypeInput.length) {
            //                            $hiddenCalcTypeInput.val(calculationTypeID);
            //                            console.log(`Set hidden-calculation-type-${rowIndex} to:`, calculationTypeID);
            //                        }
            //                        if (hra.calculationType === 'Fixed') {
            //                            const $fixedRateInput = $row.find('.fixedRate input');
            //                            if ($fixedRateInput.length && hra.value != null) {
            //                                $fixedRateInput.val(hra.value);
            //                                console.log(`Set fixedRate-${rowIndex} to:`, hra.value);
            //                            }
            //                        } else if (hra.calculationType === 'Percentage') {
            //                            const $percentRateSelect = $row.find('.percentRate select');
            //                            if ($percentRateSelect.length && hra.value != null) {
            //                                $percentRateSelect.val(hra.value);
            //                                console.log(`Set percentRate-${rowIndex} to:`, hra.value);
            //                            }
            //                        }
            //                    }

            //                    // Set EffectiveDate
            //                    const $picker = $row.closest('.card-body').find('.flatpickr-input');
            //                    const $hiddenDateInput = $row.closest('.card-body').find('[name*="EffectiveDate"][type="hidden"]');
            //                    if ($picker.length && hra.effectiveDate) {
            //                        try {
            //                            const date = new Date(hra.effectiveDate);
            //                            if (isNaN(date.getTime())) throw new Error("Invalid date");
            //                            const displayDate = `${monthNames[date.getMonth()]} ${date.getFullYear()}`;
            //                            const serverDate = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, '0')}-01`;
            //                            $picker[0]._flatpickr.setDate(displayDate, true);
            //                            $picker.val(displayDate);
            //                            if ($hiddenDateInput.length) {
            //                                $hiddenDateInput.val(serverDate);
            //                                console.log(`Set hidden-effective-date-${rowIndex} to:`, serverDate);
            //                            }
            //                        } catch (e) {
            //                            console.error(`Failed to set date for row ${rowIndex}:`, e.message);
            //                        }
            //                    }

            //                    // Set IsActive
            //                    const $checkbox = $row.closest('.card-body').find('[name*="IsActive"]');
            //                    if ($checkbox.length && item.hasOwnProperty('isActive')) {
            //                        $checkbox.prop('checked', item.isActive === true || item.isActive === 'true');
            //                        console.log(`Set IsActive_${rowIndex} to:`, $checkbox.prop('checked'));
            //                    }

            //                    // Set EmployeeAllowanceID
            //                    const $allowanceIdInput = $row.closest('.card-body').find('[name*="EmployeeAllowanceID"]');
            //                    if ($allowanceIdInput.length && item.hasOwnProperty('employeeAllowanceID')) {
            //                        $allowanceIdInput.val(item.employeeAllowanceID);
            //                        console.log(`Set EmployeeAllowanceID_${rowIndex} to:`, item.employeeAllowanceID);
            //                    }

            //                    rowIndex++;
            //                });
            //            }
            //        });

            //        toastr.success(res.message || "Data loaded successfully");
            //    } else {
            //        toastr.warning(res.message || "No data found.");
            //    }
            //},
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

