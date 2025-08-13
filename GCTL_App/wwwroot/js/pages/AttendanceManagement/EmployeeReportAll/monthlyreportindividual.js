// Function to fetch and populate attendance data dynamically
function fetchAttendanceData() {
    var employeeId = 123; // Example employeeId (this can be dynamic)
    var monthyear = '2025-06'; // Example month and year (this can be dynamic)

    $.ajax({
        url: '/MonthlyIndividualReport/SomeAction2',  // Update with your actual API endpoint
        type: 'GET',
        data: {
            monthyear: monthyear,
            employeeId: employeeId
        },
        success: function (data) {
            // Populate the table with dynamic dates
            populateAttendanceTable(data, monthyear);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching data: ', error);
        }
    });
}

// Function to populate the table with the fetched attendance data
function populateAttendanceTable(data, monthyear) {
    var tableBody = $('#attendanceTableBody');
    var tableHeader = $('thead tr');

    // First, clear any existing table rows and headers
    tableBody.empty();
    tableHeader.empty();

    // Dynamically generate the date columns based on the month and year
    var daysInMonth = new Date(monthyear.split('-')[0], monthyear.split('-')[1], 0).getDate(); // Get number of days in the month

    // Add the "Date" header
    tableHeader.append('<th>Date</th>');

    // Loop through each day of the month and add it to the table header
    for (var i = 1; i <= daysInMonth; i++) {
        tableHeader.append('<th>' + (i < 10 ? '0' + i : i) + '</th>');
    }

    // Generate rows for check-in, check-out, etc.
    var rows = [
        { label: 'Check in at', key: 'CheckInTime' },
        { label: 'Check out at', key: 'CheckOutTime' },
        { label: 'Break', key: 'Break' },
        { label: 'Late', key: 'LateHour' },
        { label: 'Early Leave', key: 'EarlyHour' },
        { label: 'Over Time', key: 'OvertimeHour' },
        { label: 'Production Hours', key: 'WorkingHour' },
        { label: 'Status', key: 'Status' }
    ];

    // Loop through each row (check-in, check-out, etc.) and populate it
    rows.forEach(function (row) {
        var tableRow = '<tr><th>' + row.label + '</th>';

        // Loop through each day and check if the data exists for that day
        for (var i = 1; i <= daysInMonth; i++) {
            var attendanceData = data.find(function (d) {
                return parseInt(d.Date) === i;
            });

            if (attendanceData) {
                // Add specific data based on the key (e.g., CheckInTime, Status)
                if (attendanceData[row.key]) {
                    tableRow += '<td>' + attendanceData[row.key] + '</td>';
                } else {
                    tableRow += '<td>-</td>';
                }
            } else {
                tableRow += '<td>-</td>';
            }
        }
        tableRow += '</tr>';
        tableBody.append(tableRow);
    });
}

// Call the fetchAttendanceData function when the page loads
$(document).ready(function () {
    fetchAttendanceData();
});
