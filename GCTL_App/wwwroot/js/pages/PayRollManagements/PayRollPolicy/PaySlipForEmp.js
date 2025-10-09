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

                    // Organization Info
                    $('#OrganizationName').text(data.organizationName || 'N/A');
                    $('#OrganizationAddress').html(`
                    ${data.organizationAddress.replace(/,/g, '<br>')} <br> 
                    <a href="mailto:${data.organizationEmailAddress}">${data.organizationEmailAddress}</a>`);
                    let organizationLogoPic = data.organizationLogoPic || "";
                    let logoPath = data.organizationLogoPic
                        ? data.organizationLogoPic
                        : "../../assets/img/icons/No-Image-Placeholder.svg.png";
                    document.getElementById("orgLogo").src = logoPath;

                    // Employee Info
                    $('#empName').text(data.employeeName);
                    $('#empAddress').html(`
                    ${data.employeeAddress.replace(/,/g, '<br>')} <br> 
                    <a href="mailto:${data.employeeEmail}">${data.employeeEmail}</a>`);
                    $('#BasicSalary').text(parseFloat(data.basicSalary).toFixed(2));

                    // ✅ Allowances (use AllowanceSalary from backend)
                    let allowanceRows = "";
                    data.allowances.forEach(a => {
                        allowanceRows += `
                        <tr>
                            <td>
                                <strong class="ms-2">${a.type} (${a.displayValue})</strong>  
                                <span class="me-2 float-end">${Math.floor(a.allowanceSalary)}</span>
                            </td>
                        </tr>`;
                    });
                    $("#allowanceTable").html(allowanceRows);

                    // ✅ Benefits (use BenefitsSalary from backend)
                    let benefitRows = "";
                    data.beneFits.forEach(b => {
                        benefitRows += `
                        <tr>
                            <td>
                                <strong class="ms-2">${b.type} (${b.displayValue})</strong>  
                               <span class="me-2 float-end">${Math.floor(b.benefitsSalary)}</span>
                            </td>
                        </tr>`;
                    });
                    $('#benefitTable').html(benefitRows);

                    // Totals
                    $('#TotalSalary').text(parseFloat(data.totalSalary).toFixed(2));
                    $('#SalaryInWords').text(data.salaryInWords);
                } else {
                    toastr.error('Failed to load payslip: ' + response.message);
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