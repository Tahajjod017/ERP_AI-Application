$(document).ready(function ()
{
    //#region Month Picker
    let today = new Date();
    
    $('#year-month-picker-1').flatpickr({
        mode: "single",
        dateFormat: "F Y",
        altFormat: "F Y",
        defaultDate: today,
       
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
    
    // #region  Data Table for Peresonal

    // #region 🟣 Get Employee Avatar HTML (Initial or Image)
    function getAvatarHtml(employee) {
        if (employee.employeeImage && employee.employeeImage !== '') {
            return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
        } else {
            const initial = employee.employeeName.charAt(0).toUpperCase();
            return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
        }
    }
    // #endregion


    $(document).on("change", ".PayRolltaxSettings-selectItem, #PayRollPaySlip-check-all", function () {
        let anyChecked = $(".PayRolltaxSettings-selectItem:checked").length > 0;

        if (anyChecked) {
            $("#markAsSaveContainerPartial").removeClass("d-none"); // show button
        } else {
            $("#markAsSaveContainerPartial").addClass("d-none"); // hide button
        }
    });


    // 🟢 Show partial payment modal with data
    $("#MarkasSavedPartial").on("click", async function (e) {
        e.preventDefault();

        // Assuming you have a selected employee or stored ID
        let selectedEmployeeId = $(".PayRolltaxSettings-selectItem:checked").first().data("id");
        if (!selectedEmployeeId) {
            toastr.info("Please select an employee first!");
            return;
        }

        // Load payslip data
        try {
            const response = await $.ajax({
                url: '/PaySlipForEmp/GetPaySlip',
                type: 'GET',
                data: { id: selectedEmployeeId },
                dataType: 'json'
            });

            if (response.success) {
                const data = response.data;
                console.log("Partial Pay Data:", data);
                $('#empName').text(data.employeeName);
                // Fill modal fields
                $('#BasicSalary').text(parseFloat(data.basicSalary).toFixed(2));

                // ✅ Allowances (use AllowanceSalary from backend)
                let allowanceRows = "";
                data.allowances.forEach(a => {
                    allowanceRows += `
                 <tr>
                     <td class="align-middle border-end" style="width:60%;">
                         <strong class="ms-2">${a.type} (${a.displayValue})</strong>
                     </td>
                     <td class="align-middle text-end border-end" style="width:20%;">
                         ${Math.floor(a.allowanceSalary).toFixed(2)}
                     </td>
                     <td class="align-middle text-end" style="width:20%;">
                         <input type="number" class="form-control form-control-sm text-end allowance-input" 
                                style="max-width: 138px; margin: 0 auto;" 
                                data-type="${a.type}" value="${Math.floor(a.allowanceSalary)}" />
                     </td>
                 </tr>`;
                             });
                $("#allowanceTable").html(allowanceRows);

                // ✅ Benefits (same structure)
                let benefitRows = "";
                data.beneFits.forEach(b => {
                    benefitRows += `
                   <tr>
                       <td class="align-middle border-end" style="width:60%;">
                           <strong class="ms-2">${b.type} (${b.displayValue})</strong>
                       </td>
                       <td class="align-middle text-end border-end" style="width:20%;">
                           ${Math.floor(b.benefitsSalary).toFixed(2)}
                       </td>
                       <td class="align-middle text-end" style="width:20%;">
                           <input type="number" class="form-control form-control-sm text-end benefit-input" 
                                  style="max-width:138px; margin: 0 auto;" 
                                  data-type="${b.type}" value="${Math.floor(b.benefitsSalary)}" />
                       </td>
                   </tr>`;
                });
                $('#benefitTable').html(benefitRows);



                // Totals
                $('#TotalSalary').text(parseFloat(data.totalSalary).toFixed(2));
                $('#NetPay').text('Net Pay: ' + parseFloat(data.netPay).toFixed(2));

                // Show modal
                const modal = new bootstrap.Modal(document.getElementById('confirmMarkPaidModalPartial'));
                modal.show();

                // Save employee ID to modal data for later
                $("#confirmMarkPaidModalPartial").data("employeeId", selectedEmployeeId);
            } else {
                toastr.error("Failed to load payslip: " + response.message);
            }
        } catch (err) {
            console.error("Error fetching payslip:", err);
            toastr.error("An error occurred while fetching the payslip.");
        }
    });

    // 🟢 Confirm "Mark as Paid Partial" button
    $("#confirmMarkPaidBtnPartial").on("click", async function () {
        const empId = $("#confirmMarkPaidModalPartial").data("employeeId");
        if (!empId) return;

        try {
            showLoadingIndicator();
            const response = await $.ajax({
                url: "/PaySlipForEmp/MarkPartialPaid",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ EmployeeID: empId })
            });

            if (response.success) {
                toastr.success(response.message || "Marked as partially paid!");
                loadTableData();
            } else {
                toastr.error(response.message || "Failed to mark partial payment!");
            }
        } catch (err) {
            console.error("Error marking partial payment:", err);
            toastr.error("Something went wrong.");
        } finally {
            hideLoadingIndicator();
            const modalEl = document.getElementById('confirmMarkPaidModalPartial');
            const modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide(); 
        }
    });




    //#region



    

   

    $("#PayRollPaySlip-check-all").on("change", function () {
        $(".PayRolltaxSettings-selectItem").prop("checked", $(this).prop("checked"));
    });


    // 🟢 Show or hide "Mark as Save" button when checkboxes change
    $(document).on("change", ".PayRolltaxSettings-selectItem, #PayRollPaySlip-check-all", function () {
        let anyChecked = $(".PayRolltaxSettings-selectItem:checked").length > 0;

        if (anyChecked) {
            $("#markAsSaveContainer").removeClass("d-none"); // show button
        } else {
            $("#markAsSaveContainer").addClass("d-none"); // hide button
        }
    });

    // 🟢 Handle Save click
    //

    // 🟢 Handle Save click - show confirmation first
    $("#MarkasSaved").on("click", function (e) {
        e.preventDefault();

        let selectedItems = $(".PayRolltaxSettings-selectItem:checked").map(function () {
            let empId = $(this).data("id");
            let isPaid = $(`.IsPaidCheckbox-selectItem[data-id='${empId}']`).prop("checked");
            return { EmployeeID: empId, IsPaid: isPaid };
        }).get();

        if (selectedItems.length === 0) {
            toastr.info("Please select at least one employee!");
            return;
        }

        // Store selectedItems temporarily in modal data
        $("#confirmMarkPaidModal").data("selectedItems", selectedItems);

        // Show confirmation modal
        var modal = new bootstrap.Modal(document.getElementById('confirmMarkPaidModal'));
        modal.show();
    });

    // 🟢 Confirm button in modal
    $("#confirmMarkPaidBtn").on("click", async function () {
        var $button = $("#MarkasSaved");
        var selectedItems = $("#confirmMarkPaidModal").data("selectedItems");

        $button.prop("disabled", true).html('<i class="fa fa-spinner fa-spin"></i> Processing...');
        showLoadingIndicator();

        try {
            const response = await $.ajax({
                url: "/PaySlipForEmp/GenerateMultiplePDFs",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify({ Employees: selectedItems }),
            });

            if (response.success || response.Success) {
                toastr.success(response.message || "Payslips saved successfully!");
                loadTableData();
            } else {
                toastr.error(response.message || "Failed to save payslips!");
            }
        } catch (err) {
            console.error("Error generating payslips:", err);
            toastr.error("Error generating payslips. Please try again.");
        } finally {
            $button.prop("disabled", false).html("Mark as Paid");
            $(".PayRolltaxSettings-selectItem, #PayRollPaySlip-check-all, .IsPaidCheckbox-selectItem").prop("checked", false);
            hideLoadingIndicator();
            $("#markAsSaveContainer").addClass("d-none");

            // Hide modal
            var modalEl = document.getElementById('confirmMarkPaidModal');
            var modal = bootstrap.Modal.getInstance(modalEl);
            modal.hide();
        }
    });

    //


    $("#export-pdf-btn").on("click", async function (e) {
        e.preventDefault();

        const $btn = $(this);
        $btn.prop("disabled", true).html('<i class="fa fa-spinner fa-spin"></i> Generating...');

        try {
            const response = await $.ajax({
                url: "/PaySlipForEmp/pdf",
                type: "POST"
            });

            if (response.success || response.Success)
            {
                toastr.success(response.message || "All payslips exported successfully!");
            } else
            {
                toastr.error(response.message || "Failed to generate payslip PDFs.");
            }
        } catch (err)
        {
            console.error("Error exporting PDFs:", err);
            toastr.error("An error occurred while generating payslips.");
        } finally
        {
            $btn.prop("disabled", false).html('<span data-feather="download"></span> PDF');
        }
    });

    //#endregion


    var currentPage = 1;
    var pageSize = 10;

    $('#payRollEmp-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });

    $(document).ready(function () {
        loadTableData();

        $("#payRollEmp-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#payRollEmp-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#payRollEmp-nextPageBtn").on('click', function () {
            currentPage++;
            loadTableData();
        });
    });
    let currentSortColumn = '';
    let currentSortOrder = '';

    $('th.sort').on('click', function () {
        const column = $(this).data('sort');
        if (currentSortColumn === column) {
            currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = column;
            currentSortOrder = 'asc';
        }

        loadTableData(currentSortColumn, currentSortOrder);
        updateSortingIndicator(column, currentSortOrder);
    });
    function updateSortingIndicator() {
        $('th.sort').each(function () {
            const $th = $(this);
            const column = $th.data('sort');
            $th.find('.sort-icon').remove();

            if (column === currentSortColumn) {
                const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
                $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
            } else {
                $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
            }
        });
    }

    $(document).on("change", "#OrganizationID", function () {

        currentPage = 1;
        loadTableData();
    });

    $('#EmployeeID,#DepartmentID').on('changed.coreui.multi-select', function () {
        currentPage = 1;
        loadTableData(); // Make AJAX call or reload the table
    });

    $('#PaidUnpaid').on('change', function () {
        currentPage = 1;
        loadTableData(); // Reload table with Paid/Unpaid filter
    });
    $('#year-month-picker-1').on('click', function () {
        currentPage = 1;
        loadTableData(); // Reload table with Paid/Unpaid filter
    });
    // Filtering according to formdate to ToDate

    function loadTableData(currentSortColumn, currentSortOrder) {
        var searchTerm = $("#payRollEmp-searchInput").val();
        const organizationId = $('#OrganizationID').val();
        const empID = $('#EmployeeID').val();
        const deptID = $('#DepartmentID').val();
        const paidUnpaid = $('#PaidUnpaid').val();
        const selectedMonth = $('#year-month-picker-1').val(); // ✅ new line
        $.ajax({
            url: '/PayRollEmpSalary/GetAllTableListAsync',
            method: 'GET',
            traditional: true,
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                currentSortColumn: currentSortColumn,
                currentSortOrder: currentSortOrder,
                organizationId: organizationId,
                deptID: deptID || [],
                empID: empID || [],
                paidUnpaid: paidUnpaid,
                month: selectedMonth,
            },
            
            success: function (response) {
                console.log("Data:", response);
                var tableBody = $("#PayRollEmpSalary-sellers-body");
                if (!tableBody.length) {
                    console.error("Table body element not found!");
                    return;
                }
                tableBody.empty();
                var totalItems = response.paginationInfo.totalItems;

                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = currentSortOrder === 'asc'
                            ? (currentPage - 1) * pageSize + index + 1
                            : totalItems - ((currentPage - 1) * pageSize + index);

                        const avatar = getAvatarHtml(item);

                        tableBody.append(`
                <tr class="hover-actions-trigger btn-reveal-trigger position-static">

                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input PayRolltaxSettings-selectItem IsPaidCheckbox-selectItem" data-id="${item.employeeId}" />
                     </td>
                    <td class="empId align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
                        <span>${item.employeeCode || 'N/A'}</span>
                    </td>
                    <td class="approveByEmployee align-middle white-space-nowrap fw-semibold text-body-emphasis ps-1 py-1">
                        <div class="d-flex align-items-center file-name-icon">
                            <div class="avatar avatar-m avatar-bordered me-2">
                                ${avatar}
                            </div>
                            <div class="ms-1">
                                <h6 class="fw-bold">${item.employeeName}</h6>
                                <span class="fs-12 fw-normal">${item.empDepartment || 'HRM'}</span>
                            </div>
                        </div>
                    </td>
                    <td class="empDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.empDepartment || 'N/A'}</td>
                    <td class="empSalary align-right white-space-nowrap ps-4 fw-semibold text-body py-1">${item.salary || 0}</td>
                    <td class="empBonus align-right white-space-nowrap ps-4 fw-semibold text-body py-1">${item.bonus || 0}</td>
                    <td class="empDeduction align-right white-space-nowrap ps-4 fw-semibold text-body py-1">${item.deduction || 0}</td>
                    <td class="netSalary align-right white-space-nowrap ps-4 fw-semibold text-body py-1">${item.netSalary || 0}</td>
                     <!-- Paid/Unpaid Checkbox -->
               
                <!-- Paid/Unpaid Label -->
                <td class="align-middle ps-4 fw-semibold text-body py-1">
                    ${item.isPaid ? '<span class="badge bg-success">Paid</span>' : '<span class="badge bg-danger">Unpaid</span>'}
                </td>

                <!-- Payslip -->
                <td class="paySlip align-middle ps-4 fw-semibold text-body py-1">
                    <a href="/PaySlipForEmp/Index/${item.employeeId}"><i class="fas fa-download text-success"></i></a>
                </td>
                </tr>
            `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="8" class="text-center">No data available</td></tr>');
                }


        


                var paginationInfo = response.paginationInfo;
                $("#payRollEmp-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#payRollEmp-totalCount").text(`(${paginationInfo.totalItems})`);
                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            }
            //
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#payRollEmp-paginationLinks");
        paginationLinks.empty();
        // Window size (number of pages before/after the current page)
        const windowSize = 1;

        const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;

        // Helper function for ellipsis
        const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
        // Add "First Page" and ellipsis if needed
        if (currentPage > windowSize + 1) {
            paginationLinks.append(createPageButton(1), addEllipsis());
        }
        // Add page number buttons within the window range
        const startPage = Math.max(1, currentPage - windowSize);
        const endPage = Math.min(totalPages, currentPage + windowSize);
        for (let i = startPage; i <= endPage; i++) {
            paginationLinks.append(createPageButton(i));
        }
        // Add ellipsis and "Last Page" button if needed
        if (currentPage < totalPages - windowSize) {
            paginationLinks.append(addEllipsis(), createPageButton(totalPages));
        }
        // Disable or enable previous/next buttons
        $("#payRollEmp-prevPageBtn").prop('disabled', currentPage === 1);
        $("#payRollEmp-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endregion


});