$(document).ready(function () {
    // Make an AJAX request to the backend
    $.ajax({
        url: '/YearlyReport/GetYearlySpecialDaysReport', // Replace with your actual endpoint
        type: 'GET',
        dataType: 'json',
        success: function (data) {
            debugger
            // Iterate over the response and generate table rows
            data.forEach(function (monthData) {
                var rowHtml = '<tr>';
                rowHtml += '<td class="empName align-middle fw-semibold text-body-emphasis ps-2 py-1">' + monthData.month + '</td>';

                // Loop through each day's data for the month
                monthData.specialDays.forEach(function (day) {
                    var dayClass = '';
                    switch (day.specialDays) {
                        case 'Present':
                            dayClass = 'bg-success';
                            break;
                        case 'Absent':
                            dayClass = 'bg-danger';
                            break;
                        case 'Leave':
                            dayClass = 'bg-warning';
                            break;
                        case 'Weekend':
                            dayClass = 'bg-warning-subtle';
                            break;
                        case 'Holiday':
                            dayClass = 'bg-primary';
                            break;
                        default:
                            dayClass = 'bg-secondary';
                    }

                    rowHtml += '<td class="' + dayClass + '">' + day.specialDays.charAt(0) + '</td>';
                });

                // Add the total counts for the month
                rowHtml += '<td>' + monthData.totalPresent + '</td>';
                rowHtml += '<td>' + monthData.totalAbsent + '</td>';
                rowHtml += '<td>' + monthData.totalLeave + '</td>';
                rowHtml += '<td>' + monthData.totalWeekend + '</td>';
                rowHtml += '<td>' + monthData.totalHoliday + '</td>';
                rowHtml += '</tr>';

                // Append the generated row to the table body
                $('#attendanceTableBody').append(rowHtml);
            });
        },
        error: function (err) {
            console.error('Error fetching data:', err);
        }
    });
    // Print button functionality
    // Print button functionality
    $('#printButton').on('click', function () {
        printTable();
    });

    // Function to print the table
   
});

function printTable() {
    var printWindow = window.open('', '', 'height=500,width=800');
    printWindow.document.write('<html><head><title>Yearly Special Days Report</title>');
    printWindow.document.write('<style>');
    printWindow.document.write('body { font-family: Arial, sans-serif; }');
    printWindow.document.write('table { width: 100%; border-collapse: collapse; }');
    printWindow.document.write('table, th, td { border: 1px solid black; }');
    printWindow.document.write('th, td { padding: 8px; text-align: center; }');
    printWindow.document.write('.bg-success { background-color: green; color: white; }');
    printWindow.document.write('.bg-danger { background-color: red; color: white; }');
    printWindow.document.write('.bg-warning { background-color: yellow; color: black; }');
    printWindow.document.write('.bg-warning-subtle { background-color: lightyellow; color: black; }');
    printWindow.document.write('.bg-primary { background-color: blue; color: white; }');
    printWindow.document.write('.bg-secondary { background-color: gray; color: white; }');
    printWindow.document.write('</style>');
    printWindow.document.write('</head><body>');
    printWindow.document.write('<h1>Yearly Special Days Report</h1>');
    printWindow.document.write('<table>');
    printWindow.document.write($('#attendanceTableBody').closest('table').prop('outerHTML'));
    printWindow.document.write('</table>');
    printWindow.document.write('</body></html>');
    printWindow.document.close(); // Close the document
    printWindow.print(); // Trigger the print dialog
}