$(document).ready(function ()
{
    //#region Month Picker
    let today = new Date();
    today.setMonth(today.getMonth() - 1);
    var currentDate = new Date();
    $('#year-month-picker-1').flatpickr({
        mode: "single",
        dateFormat: "F Y",
        altFormat: "F Y",
        defaultDate: today,
        maxDate: currentDate, 
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


    var currentPage = 1;
    var pageSize = 5;

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

    $(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

        currentPage = 1;
        loadTableData();
    });

    $('#OrganizationIDD').on('changed.coreui.multi-select', function () {
        currentPage = 1;
        loadTableData(); // Make AJAX call or reload the table
    });

    // Filtering according to formdate to ToDate

    function loadTableData(currentSortColumn, currentSortOrder) {
        var searchTerm = $("#payRollEmp-searchInput").val();
        const organizationId = $('#OrganizationIDD').val();

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
            },
            //success: function (response) {



            //    console.log("Datassssss", response);
            //    var tableBody = $("#PayRollEmpSalary-body");
            //    tableBody.empty();
            //    var totalItems = response.paginationInfo.totalItems;

            //    if (response.data.length > 0) {
            //        response.data.forEach(function (item, index) {
            //
            //            if (currentSortOrder === 'asc') {
            //                rowIndex = (currentPage - 1) * pageSize + index + 1;
            //            } else {
            //                rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
            //            }
            //            const avatar = getAvatarHtml(item);
            //            tableBody.append(`
            //             <tr class="hover-actions-trigger btn-reveal-trigger position-static">
            //                <td class="empId align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
            //                    <span>#Emp54736</span>
            //                </td>
            //                <td class="approveByEmployee align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
            //              <div class="d-flex align-items-center file-name-icon">
            //                <div class="avatar avatar-m avatar-bordered me-2">
            //                 ${avatar}
            //                </div>
            //                <div class="ms-1">
            //                  <h6 class="fw-bold">${item.employeeName}</h6>
            //                  <span class="fs-12 fw-normal ">${item.employeeDepartment || 'HRM'}</span>
            //                </div>
            //              </div>
            //            </td>
            //                <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
            //                    <div class="d-flex align-items-center position-relative">
            //                        <div class="avatar avatar-m me-3">
            //                            <img class="rounded-circle avatar-placeholder" src="../../assets/img/team/avatar.webp" alt="" />
            //                        </div><a class="text-body-highlight fw-bold stretched-link" href="#!">Hasan Ali</a>
            //                    </div>
            //                </td>
            //                <td class="empDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">Developer</td>
            //                <td class="empSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">50000</td>
            //                <td class="empBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">10000</td>
            //                <td class="empDeduction align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">5000</td>
            //                <td class="netSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">55000</td>
            //                <td class="paySlip align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">
            //                    <a  asp-action="Index" asp-controller="PaySlipForEmp"><i class="fas fa-download text-success"></i></a>
            //                </td>


            //            </tr>
            //       `);
            //        });
            //    } else {
            //        tableBody.append('<tr><td colspan="10" class="text-center">No data available</td></tr>');
            //    }

            //    var paginationInfo = response.paginationInfo;

            //    $("#payRollEmp-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            //    $("#payRollEmp-totalCount").text(`(${paginationInfo.totalItems})`);

            //    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            //}

            //
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
                    <td class="empId align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
                        <span>#${item.employeeId || 'N/A'}</span>
                    </td>
                    <td class="approveByEmployee align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
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
                    <td class="empSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.salary || 0}</td>
                    <td class="empBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.bonus || 0}</td>
                    <td class="empDeduction align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.deduction || 0}</td>
                    <td class="netSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.netSalary || 0}</td>
                    <td class="paySlip align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">
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