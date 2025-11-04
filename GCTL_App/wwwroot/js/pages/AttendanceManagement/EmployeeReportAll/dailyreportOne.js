$(document).ready(function () {
    // Initialize the functions
    loadDepartments();
    loadEmployees();
});

// Function to load departments based on selected organization
function loadDepartments() {
    $('#organizationid').change(function () {
        var organizationId = $(this).val();

        if (organizationId) {
            $.ajax({
                url: '/DailyIndividualReport/GetDepartments',
                type: 'GET',
                data: { organizationId: organizationId },
                success: function (data) {
                    debugger
                    const select = $('#multiple-dep-select');
                    select.empty();
                    if (data && data.length > 0) {
                        data.forEach(data => {
                            select.append(
                                $('<option>', {
                                    value: data.id,
                                    text: data.name
                                })
                            );
                        });
                    }

                    // CoreUI MultiSelect reinitialization/refresh
                    const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                    if (multiSelectInstance) {
                        multiSelectInstance.update(); // Refresh UI to reflect new options
                    } else {
                        new coreui.MultiSelect(select[0]); // First time init
                    }

                   
                  
                },
                error: function () {
                    alert('Error loading departments');
                }
            });
        } else {
            $('#multiple-dep-select').empty().append('<option value="">Select Department</option>');
        }
    });
}

// Function to load employees based on selected department
function loadEmployees() {
    $('#multiple-dep-select').change(function () {
        var departmentId = $(this).val();

        if (departmentId) {
            $.ajax({
                url: '/DailyIndividualReport/GetEmployees',
                type: 'GET',
                data: { departmentId: departmentId },
                success: function (data) {
                    $('#multiple-emp-select').empty().append('<option value="">Select Employee</option>');
                    data.forEach(function (employee) {
                        $('#multiple-emp-select').append('<option value="' + employee.id + '">' + employee.name + '</option>');
                    });
                },
                error: function () {
                    alert('Error loading employees');
                }
            });
        } else {
            $('#multiple-emp-select').empty().append('<option value="">Select Employee</option>');
        }
    });
}

function loadAttendanceIndividualSummary() {
    $.ajax({
        url: '/DailyIndividualReport/GetEmployeeSummary', // Replace with your actual endpoint
        method: 'GET',
       
        success: function (response) {
            // Update employee name
            
            $('#employee_name').text(`${response.employeeName} (${response.employeeCode})`);

            // Optional: update description if dynamic
            $('#attendance_description').text(response.description ||
                `The table below presents all the information about when ${response.employeeName} came to the office and when he left the office this month.`);

            // Update attendance metrics
            $('#total_days_count').text(`${response.totalDays} Days`);
            $('#total_days_percent').html(`<i class="ti ti-arrow-wave-right-down me-1"></i>${response.percentages.totalDays}%`);

            $('#late_days_count').text(`${response.lateDays} Days`);
            $('#late_days_percent').html(`<i class="ti ti-arrow-wave-right-down me-1"></i>${response.percentages.lateDays}%`);

            $('#absent_days_count').text(`${response.absentDays} Days`);
            $('#absent_days_percent').html(`<i class="ti ti-arrow-wave-right-down me-1"></i>${response.percentages.absentDays}%`);

            $('#leave_days_count').text(`${response.leaveDays} Days`);
            $('#leave_days_percent').html(`<i class="ti ti-arrow-wave-right-down me-1"></i>${response.percentages.leaveDays}%`);

            $('#day_off_days_count').text(`${response.dayOffDays} Days`);
            $('#day_off_days_percent').html(`<i class="ti ti-arrow-wave-right-down me-1"></i>${response.percentages.dayOffDays}%`);
        },
        error: function (err) {
            console.error('Error loading attendance summary:', err);
            alert('Failed to load attendance data. Please try again later.');
        }
    });
}

//function employeeDropdown() {
//    $.ajax({
//        url: '/DailyIndividualReport/GetEmployees',
//        type: 'GET',
//        success: function (data) {
           
//        },
//        error: function () {
//            console.error('Error fetching employees');
//        }
//    });
//}


//$(document).ready(function () {
//    // AJAX call to fetch employee data
//    $.ajax({
//        url: '/DailyIndividualReport/GetEmployeeData', // Replace with your actual endpoint
//        type: 'GET', // Or 'POST' depending on your requirement
//        success: function (response) {
//            // Assuming response contains data like:
//            // { "developers": [{ "id": 2, "name": "Alam" }, { "id": 3, "name": "Momin" }],
//            //   "designers": [{ "id": 4, "name": "Name" }, { "id": 5, "name": "Name" }] }

//            // Empty the select first
//            $('#multiple-emp-select').empty();

//            // Create optgroups dynamically
//            if (response.developers && response.developers.length > 0) {
//                var developerGroup = $('<optgroup label="Developer">');
//                response.developers.forEach(function (dev) {
//                    developerGroup.append('<option value="' + dev.id + '">' + dev.name + ' (id)</option>');
//                });
//                $('#multiple-emp-select').append(developerGroup);
//            }

//            if (response.designers && response.designers.length > 0) {
//                var designerGroup = $('<optgroup label="Designer">');
//                response.designers.forEach(function (des) {
//                    designerGroup.append('<option value="' + des.id + '">' + des.name + ' (id)</option>');
//                });
//                $('#multiple-emp-select').append(designerGroup);
//            }
//        },
//        error: function (xhr, status, error) {
//            console.error('Error loading data:', error);
//        }
//    });
//});


//////////////////////////////Data Table Initialization//////////////////////////////

$(document).ready(function () {
    var currentPage = 1;
    var pageSize = 5;
    loadTableData();
    // Initialize page size select
    $('#empAttendencindividual-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });





    $("#empAttendencindividual-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#empAttendencindividual-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#empAttendencindividual-nextPageBtn").on('click', function () {
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
        var searchTerm = $("#empAttendencindividual-searchInput").val();
        $.ajax({
            url: '/DailyIndividualReport/GetEmployeeAttendance',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                var tableBody = $("#empAttendencindividual-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        var statusClass = item.statusName.toLowerCase() === "present" ? "badge-phoenix-success" : "badge-phoenix-warning";

                    

                        // Function to generate random color based on initials (or name)
                    

                        tableBody.append(`
                        <tr class="position-static">
                            
                            <td class="align-middle text-center white-space-nowrap pe-4">${rowIndex}</td>
                            
                            <td class="align-middle white-space-nowrap ps-0">${item.attendanceDate}</td>

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

                $("#empAttendencindividual-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#empAttendencindividual-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#empAttendencindividual-paginationLinks");
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
        $("#empAttendencindividual-prevPageBtn").prop('disabled', currentPage === 1);
        $("#empAttendencindividual-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
});
