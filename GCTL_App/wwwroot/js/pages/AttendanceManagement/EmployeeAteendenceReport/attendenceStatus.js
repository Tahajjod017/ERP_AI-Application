//$(document).ready(function () {
//    // Fetch attendance data for a specific employee (e.g., user ID 1)
//    $.ajax({
//        url: '/EmployeesAttendance/GetEmployeeAttendanceData',  // Adjust URL based on your backend route
//        type: 'GET',
//        data: { userId: 1 },  // Pass the userId dynamically if needed
//        dataType: 'json',
//        success: function (data) {
//            alert()
//            // Populate the data into the HTML elements
//            $('#totalWorkingHours').text(data.totalWorkingHours); // Example: "12h 36m"
//            $('#productiveHours').text(data.productiveHours); // Example: "08h 36m"
//            $('#breakHours').text(data.breakHours); // Example: "22m 15s"
//            $('#overtime').text(data.overtime); // Example: "02h 15m"

//            let total = parseFloat(data.totalWorkingHours) || 1; // avoid division by 0
//            let productive = parseFloat(data.productiveHours) || 0;
//            let overtime = parseFloat(data.overtime) || 0;

//            $('#progressTotal').css('width', '100%');
//            $('#progressProductive').css('width', `${(productive / total) * 100}%`);
//            $('#progressOvertime').css('width', `${(overtime / total) * 100}%`);

            
//        },
//        error: function (xhr, status, error) {
//            console.error('Error fetching data:', error);
//        }
//    });
//});
function parseTime(timeStr) {
    const [hh, mm] = timeStr.split(':').map(Number);
    return new Date(0, 0, 0, hh, mm);
}

function parseDuration(durationStr) {
    const hMatch = durationStr.match(/(\d+)h/);
    const mMatch = durationStr.match(/(\d+)m/);
    const hours = hMatch ? parseInt(hMatch[1]) : 0;
    const minutes = mMatch ? parseInt(mMatch[1]) : 0;
    return hours * 60 + minutes;
}

function formatTime(date) {
    return date.toTimeString().substring(0, 5); // "HH:mm"
}

function updateProgressBarWithStart(totalMins, offsetMins, durationMins, barId) {
    const left = (offsetMins / totalMins) * 100;
    const width = (durationMins / totalMins) * 100;

    $(`#${barId}`).css({
        left: `${left}%`,
        width: `${width}%`
    });
}

function updateAttendanceUI(data) {
    // Display summary values
    $('#totalWorkingHours').text(data.totalWorkingHours || '00h 00m');
    $('#productiveHours').text(data.productionTime || '00h 00m');
    $('#breakHours').text(data.break || '00h 00m');
    $('#overtime').text(data.overtime || '00h 00m');

    const totalMins = parseDuration(data.totalWorkingHours);
    const prodMins = parseDuration(data.productionTime);
    const breakMins = parseDuration(data.break);
    const overtimeMins = parseDuration(data.overtime);
    alert(overtimeMins)

    // Progress bar logic
    updateProgressBarWithStart(totalMins, 0, prodMins, 'progressProductive');
    updateProgressBarWithStart(totalMins, prodMins, breakMins, 'progressBreak');
    updateProgressBarWithStart(totalMins, prodMins + breakMins, overtimeMins, 'progressOvertime');

    // Generate timeline labels (from check-in  to end)
    const timeline = $('#timelineLabels');
    timeline.empty();

    const checkIn = parseTime(data.checkInShiftTime || '08:00');
    //const start = new Date(checkIn.getTime() - 60 * 60000); // +1h
    const start = new Date(checkIn.getTime()); // +1h
    const end = new Date(checkIn.getTime() + totalMins * 60000);

    let current = new Date(start);
    while (current <= end) {
        const span = $('<span>').addClass('fs-10').text(formatTime(current));
        timeline.append(span);
        current.setMinutes(current.getMinutes() + 60);
    }
}



$(document).ready(function () {
    const userId = 123; // replace with your actual user ID
    $.get(`/EmployeesAttendance/GetEmployeeAttendanceData`, function (data) {
        if (data) updateAttendanceUI(data);
    });
});
   





var currentPage = 1;
var pageSize = 5;

$('#attendanceStatus-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#attendanceStatus-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#attendanceStatus-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#attendanceStatus-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});


let currentSortColumn = 'BloodGroupName';
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
    var searchTerm = $("#attendanceStatus-searchInput").val();

    $.ajax({
        url: '/EmployeesAttendance/GetAlls',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#attendanceStatus-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0">${item.attendanceDate}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.checkInTime}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.checkOutTime}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.break}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.lateHour}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.overtimeHour}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.workingHours}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.shiftName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.statusName}</td>
                            
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#attendanceStatus-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#attendanceStatus-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#attendanceStatus-paginationLinks");
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
    $("#attendanceStatus-prevPageBtn").prop('disabled', currentPage === 1);
    $("#attendanceStatus-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});