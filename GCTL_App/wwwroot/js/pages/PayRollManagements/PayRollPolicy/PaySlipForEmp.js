$(document).ready(function () {

    const pathSegments = window.location.pathname.split('/');
    const idString = pathSegments.length > 2 ? pathSegments[pathSegments.length - 1] : null; // Get the last segment (e.g., '8')

    // Convert to integer
    const id = idString ? parseInt(idString) : null; // Base 10 to ensure proper integer conversion

    debugger; // For debugging

    if (!id || isNaN(id)) {
        toastr.error("Id Not Found or Invalid");
        return; 
    }
    function getPaySlip() {
        $.ajax({
            url: '/PaySlipForEmp/GetPaySlip',
            type: 'GET',
            data: { id: id }, // Pass the id as a query parameter
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    // Handle success - populate the page with data
                    const data = response.data;
                    debugger
                    $('#employeeName').text(data.EmployeeName || 'N/A');
                    $('#organizationName').text(data.organizationName || 'N/A');
                    $('#OrganizationAddress').text(data.organizationAddress || 'N/A');

                    $('#basic').text(data.Basic.toFixed(2) || '0.00');
                    $('#hra').text(data.HRA.toFixed(2) || '0.00');
                    $('#da').text(data.DA.toFixed(2) || '0.00');
                    $('#specialAllowance').text(data.SpecialAllowance.toFixed(2) || '0.00');
                    $('#bonus').text(data.Bonus.toFixed(2) || '0.00');
                    $('#totalEarnings').text(data.TotalEarnings.toFixed(2) || '0.00');
                    $('#providentFund').text(data.ProvidentFund.toFixed(2) || '0.00');
                    $('#professionalTax').text(data.ProfessionalTax.toFixed(2) || '0.00');
                    $('#esi').text(data.ESI.toFixed(2) || '0.00');
                    $('#homeLoan').text(data.HomeLoan.toFixed(2) || '0.00');
                    $('#tds').text(data.TDS.toFixed(2) || '0.00');
                    $('#totalDeductions').text(data.TotalDeductions.toFixed(2) || '0.00');
                    $('#netPay').text(data.NetPay.toFixed(2) || '0.00');
                    $('#payslipNo').text(data.PayslipNo || 'N/A');
                    $('#paymentDate').text(new Date(data.PaymentDate).toLocaleDateString() || 'N/A');
                } else {
                    alert('Failed to load payslip: ' + response.Message);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error: ', status, error);
                alert('An error occurred while fetching the payslip.');
            }
        });
    }

    // Call the function when the page loads
    getPaySlip();

    // Optional: Add a button to refresh the payslip
    $('#refreshPaySlip').on('click', function () {
        getPaySlip();
    });
});