$(document).ready(function () {

    const pathSegments = window.location.pathname.split('/');
    const idString = pathSegments.length > 2 ? pathSegments[pathSegments.length - 1] : null;
    const id = idString ? parseInt(idString) : null;
    if (!id || isNaN(id))
    {
        toastr.error("Id Not Found or Invalid");
        return;
    }


    //$(document).on('click', '#PaySlipSave', async function () {
    //    const postData = {
    //        EmployeeID: id,
    //        PayPeriodStart: $('#PayPeriodStart').val(),
    //        PayPeriodEnd: $('#PayPeriodEnd').val(),
    //        IsPaid: $('#IsPaid').is(':checked')
    //    };

    //    try {
    //        const response = await $.ajax({
    //            url: '/PaySlipForEmp/Save',
    //            type: 'POST',
    //            contentType: 'application/json',
    //            data: JSON.stringify(postData),
    //            dataType: 'json'
    //        });

    //        if (response.success) {
    //            toastr.success(response.message || "Payslip saved successfully!");
    //        } else {
    //            if (Array.isArray(response.message)) {
    //                response.Message.forEach(err => toastr.error(err));
    //            } else if (typeof response.message === 'string') {
    //                toastr.error(response.message);
    //            }
    //        }
    //    } catch (err) {
    //        console.error("AJAX Error:", err);
    //        toastr.error("An error occurred while saving the payslip.");
    //    }
    //});


  
    function updatePaymentTime() {
        const now = new Date();
        const options = {
            month: 'short', day: '2-digit', year: 'numeric',
            hour: '2-digit', minute: '2-digit', second: '2-digit',
            hour12: true
        };
        document.getElementById('paymentDateTime').innerText = now.toLocaleString('en-US', options);
    }

    // Update immediately and then every second
    updatePaymentTime();
    setInterval(updatePaymentTime, 1000);

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
                    $('#NetPay').text('Net Pay : ' + parseFloat(data.netPay).toFixed(2));
                    $('#TotalDeductions').text(parseFloat(data.totalDeductions).toFixed(2));
                    $('#ProfessionalTax').text(parseFloat(data.professionalTax).toFixed(2));
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


    // for pdf generate


    //$(".print a").on("click", function (e) {
    //    e.preventDefault();

    //    let empId = $("#EmployeeID").val(); // Hidden field with employee ID

    //    fetch("/PaySlipForEmp/Pdf?id=" + empId, {
    //        method: "POST"
    //    })
    //        .then(response => response.blob())
    //        .then(blob => {
    //            const url = window.URL.createObjectURL(blob);
    //            const a = document.createElement("a");
    //            a.href = url;
    //            a.download = `PaySlip_${empId}.pdf`; // File name
    //            document.body.appendChild(a);
    //            a.click();
    //            a.remove();
    //            window.URL.revokeObjectURL(url);
    //        })
    //        .catch(error => {
    //            alert("Error generating PDF: " + error);
    //        });
    //});


    //

    $(document).on('click', '#PaySlipSave, .print a', async function (e) {
        e.preventDefault();

        const empId = $("#EmployeeID").val();
        const postData = {
            EmployeeID: empId,
            PayPeriodStart: $('#PayPeriodStart').val(),
            PayPeriodEnd: $('#PayPeriodEnd').val(),
            IsPaid: $('#IsPaid').is(':checked')
        };

        try {
            const pdfResponse = await fetch("/PaySlipForEmp/SaveAndPdf", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(postData)
            });

            const blob = await pdfResponse.blob();
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            a.href = url;
            a.download = `PaySlip_${empId}.pdf`;
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
            toastr.success("Payslip saved and PDF generated!");
        } catch (err) {
            console.error(err);
            toastr.error("Error saving payslip or generating PDF.");
        }
    });


    //


});