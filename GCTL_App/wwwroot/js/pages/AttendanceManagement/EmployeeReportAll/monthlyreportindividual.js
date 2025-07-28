$(document).ready(function () {
    // Assuming we have the employeeId and month/year
    var employeeId = 1; // Example employee ID
    var month = 6; // Example month (June)
    var year = 2023; // Example year

    // Generate days for the given month and year
    var daysInMonth = new Date(year, month, 0).getDate(); // Gets the number of days in the month

    // Dynamically generate columns for each day
    var tableHeader = $('<tr><th>Date</th>'); // Start the header row with "Date"
    
    // Generate a header for each day of the month
    for (var day = 1; day <= daysInMonth; day++) {
        tableHeader.append('<th>' + (day < 10 ? '0' + day : day) + '</th>');
    }
    
    // Insert the generated header row into the table
    $('table thead').append(tableHeader);

    // Fetch data for the employee's attendance (Assuming you have the endpoint ready to return the data)
    $.ajax({
        url: '/HolidayController/GetEmployeeMonthlyAttendance',  // Make sure the URL is correct
        type: 'GET',
        data: { employeeId: employeeId, month: month, year: year },
        success: function (data) {
            var tableBody = $('#attendanceBody');

            // Generate rows dynamically for each employee's attendance
            var checkinRow = $('<tr><th>Check in at</th>'); // Start with "Check in at" row
            var checkoutRow = $('<tr><th>Check out at</th>'); // Start with "Check out at" row
            var breakRow = $('<tr><th>Break</th>'); // Start with "Break" row

            // Loop through the days of the month and populate rows
            for (var day = 1; day <= daysInMonth; day++) {
                var attendance = data.attendance.find(a => new Date(a.AttendanceDate).getDate() === day);

                // Check-in Row
                if (attendance) {
                    checkinRow.append('<td>' + (attendance.CheckInTime ? attendance.CheckInTime : '-') + '</td>');
                } else {
                    checkinRow.append('<td>-</td>');
                }

                // Check-out Row
                if (attendance) {
                    checkoutRow.append('<td>' + (attendance.CheckOutTime ? attendance.CheckOutTime : '-') + '</td>');
                } else {
                    checkoutRow.append('<td>-</td>');
                }

                // Break Row
                if (attendance) {
                    breakRow.append('<td>' + (attendance.Remarks ? attendance.Remarks : '-') + '</td>');
                } else {
                    breakRow.append('<td>-</td>');
                }
            }

            // Append the generated rows to the table body
            tableBody.append(checkinRow);
            tableBody.append(checkoutRow);
            tableBody.append(breakRow);
        }
    });
});
