$(document).ready(function () {
    loadAttendanceSummary();
});
function loadAttendanceSummary() {
    $.ajax({
        url: '/DailyReportForAll/GetSummary',
        method: 'GET',
        dataType: 'json',
        success: function (data) {
            function update(idPrefix, count, percent) {
                $(`#${idPrefix}-count`).text(count);
                $(`#${idPrefix}-percent`).text(`${percent} %`);
            }

            update('present', data.present, data.presentPercent);
            update('late-present', data.latePresent, data.latePresentPercent);
            update('leave', data.leave, data.leavePercent);
            update('absent', data.absent, data.absentPercent);
        },
        error: function (err) {
            console.error('Error fetching attendance summary:', err);
        }
    });
}

function populateOrganizationDropdown(organizations) {
    $.ajax({
        url: '/WeekendSettings/GetOrganizations',  // API to get organizations
        type: 'GET',
        success: function (organizations) {
            const simplifiedRoles = organizations.map(role => ({
                id: role.value,
                name: role.text
            }));
            choiceManager.populateDropdown('OrganizationEditID', simplifiedRoles);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching organizations:', error);
        }
    });
}

//////////////////////////////Data Table Initialization//////////////////////////////

$(document).ready(function () {
    var currentPage = 1;
    var pageSize = 5;
    loadTableData();
    // Initialize page size select
    $('#empAttendencAll-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });





    $("#empAttendencAll-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#empAttendencAll-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#empAttendencAll-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
  

    let currentSortColumn = 'HolidayTitle';
    let currentSortOrder = 'asc';

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

    function loadTableData(sortColumn, sortOrder) {
        var searchTerm = $("#empAttendencAll-searchInput").val();
        $.ajax({
            url: '/DailyReportForAll/GetAllEmployeeAttendanceReport',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                var tableBody = $("#empAttendencAll-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        var statusClass = item.statusName.toLowerCase() === "present" ? "badge-phoenix-success" : "badge-phoenix-warning";

                        var employeeImage = item.employeeImageUrl ? item.employeeImageUrl : null; // Adjust this if your API has a specific field for image URL
                        var employeeInitials = item.employeeName ? item.employeeName.charAt(0).toUpperCase() : ''; // Get the first letter of the employee's name

                        // Function to generate random color based on initials (or name)
                        function getRandomColor(name) {
                            const colors = [
                                "#A9D6F0", // Light Blue
                                "#B8E1B2", // Light Green
                                "#F1D0D6", // Light Pink
                                "#F7E3B6", // Light Yellow
                                "#D0C8FF", // Light Purple
                                "#FFD4A1", // Light Orange
                                "#A8D8D4", // Light Cyan
                                "#F2B3B0"  // Light Coral
                            ];
                            let index = name.charCodeAt(0) % colors.length;  // Use the first character of the name to select a color
                            return colors[index];
                        }
                        // Get a random or specific color based on the first letter of the employee's name
                        var color = employeeImage ? '' : getRandomColor(item.employeeName);

                        tableBody.append(`
                        <tr class="position-static">
                            
                            <td class="align-middle text-center white-space-nowrap pe-4">${rowIndex}</td>
                            
                            <!-- Employee Name Column -->
                            <td class="align-middle white-space-nowrap ps-0">
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-2">
                                        <!-- If image is available, display it, else show initials with random color -->
                                        ${employeeImage
                                ? `<img class="rounded-circle" src="${employeeImage}" alt="Employee Avatar" />`
                                : `<div class="rounded-circle" style="width: 35px; height: 35px; background-color: ${color}; color: white; display: flex; align-items: center; justify-content: center;">
                                                    ${employeeInitials}
                                   </div>`
                            }
                                    </div>
                                    <div class="ms-1 mt-1">
                                        <h6 class="fw-bold">${item.employeeName}</h6>
                                        <span class="fs-12 fw-normal">${item.jobTitle}</span> <!-- Assuming jobTitle exists -->
                                    </div>
                                </div>
                            </td>

                             <!-- Status Name Column with Conditional Badge Color -->
                             <td class="attStatus align-middle white-space-nowrap ps-0 fw-semibold text-body py-0">
                                <span class="badge ${statusClass}">${item.statusName}</span>
                             </td>
                             <td class="align-middle white-space-nowrap ps-0">${item.checkInTime}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.checkOutTime}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.break}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.lateHour}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.workingHours}</td>
                           
                             
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#empAttendencAll-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#empAttendencAll-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#empAttendencAll-paginationLinks");
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
        $("#empAttendencAll-prevPageBtn").prop('disabled', currentPage === 1);
        $("#empAttendencAll-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
});
