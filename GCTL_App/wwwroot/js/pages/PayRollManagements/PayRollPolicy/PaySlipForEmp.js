$(document).ready(function () {

    const pathSegments = window.location.pathname.split('/');
    const idString = pathSegments.length > 2 ? pathSegments[pathSegments.length - 1] : null;
    const id = idString ? parseInt(idString) : null;
    if (!id || isNaN(id))
    {
        toastr.error("Id Not Found or Invalid");
        return;
    }

    //let id = parseInt($('#EmployeeID').val());
    //debugger
    //// If input is empty or invalid, fallback to URL
    //if (!id || isNaN(id)) {
    //    const pathSegments = window.location.pathname.split('/');
    //    const idString = pathSegments.length > 2 ? pathSegments[pathSegments.length - 1] : null;
    //    id = idString ? parseInt(idString) : null;
    //}

    //if (!id || isNaN(id)) {
    //    toastr.error("Id Not Found or Invalid");
    //    return;
    //}

    function getPaySlip() {
        $.ajax({
            url: '/PaySlipForEmp/GetPaySlip',
            type: 'GET',
            data: { id: id }, 
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    const data = response.data;
                    console.log(data);
                    $('#OrganizationName').text(data.organizationName || 'N/A');
                    $('#OrganizationAddress').html(`
                    ${data.organizationAddress.replace(/,/g, '<br>')} <br> 
                    <a href="mailto:${data.organizationEmailAddress}">${data.organizationEmailAddress}</a>`);
                    let organizationLogoPic = data.organizationLogoPic || "";
                    let logoPath = organizationLogoPic
                        ? `/uploads/company/logo/${organizationLogoPic}` : "../../assets/img/icons/No-Image-Placeholder.svg.png";
                    document.getElementById("orgLogo").src = logoPath;
                    $('#empName').text(data.employeeName);
                    $('#empAddress').html(`
                    ${data.employeeAddress.replace(/,/g, '<br>')} <br> 
                    <a href="mailto:${data.employeeAddress}">${data.employeeEmail}</a>`);
                    $('#BasicSalary').text(parseFloat(data.basicSalary).toFixed(2));

                    let allowanceRows = "";
                    data.allowances.forEach(a => {
                        allowanceRows += `
                <tr>
                    <td>
                        <strong class="ms-2">${a.type}(${a.amount}%)</strong>  
                        <span class="me-2 float-end">${parseFloat((a.amount * data.basicSalary) / 100).toFixed(2)}</span>

                    </td>
                </tr>`;
                    });
                    // Append to table
                    $("#allowanceTable").html(allowanceRows);
                    $('#TotalSalary').text(parseFloat(data.totalSalary).toFixed(2));
                    $('#SalaryInWords').text(data.salaryInWords);
                } else
                {
                    toastr.error('Failed to load payslip: ' + response.Message);
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error: ', status, error);
                toastr.error('An error occurred while fetching the payslip.');
            }
        });
    }
    getPaySlip();
    $('#refreshPaySlip').on('click', function () {
        getPaySlip();
    });
});