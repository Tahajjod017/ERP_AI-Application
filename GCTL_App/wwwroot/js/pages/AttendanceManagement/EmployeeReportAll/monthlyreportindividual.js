$(document).ready(function () {
    // Fetch the data from the backend
    $.ajax({
        url: '/path/to/your/api/endpoint',  // Adjust the endpoint to your backend
        method: 'GET',
        dataType: 'json',
        success: function (response) {
            // Assuming response contains 'months' and 'attendanceData'

            // Dynamically populate the table header (months)
            var months = response.months; // Example: ["01", "02", ..., "31"]
            var theadHtml = '<tr><th>Date</th>';

            // Add the month headers (01, 02, ..., 31)
            months.forEach(function (month) {
                theadHtml += '<th>' + month + '</th>';
            });

            theadHtml += '</tr>';
            $('thead').html(theadHtml);  // Update the thead directly

            // Dynamically populate the table body with attendance data
            var attendanceData = response.attendanceData; // Array of attendance records

            var tbodyHtml = '';
            attendanceData.forEach(function (record) {
                tbodyHtml += '<tr>';
                tbodyHtml += '<th>' + record.checkInDate + '</th>';

                months.forEach(function (month) {
                    var monthData = record.attendance[month] || '-'; // If no data, display '-'
                    tbodyHtml += '<td>' + monthData + '</td>';
                });

                tbodyHtml += '</tr>';
            });

            $('tbody').html(tbodyHtml);  // Update the tbody directly
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
        }
    });
});



