// QuotationSidebarActions.js - JavaScript functions for sidebar actions

function duplicateQuotation(quotationId) {
    if (confirm('Are you sure you want to duplicate this quotation?')) {
        $.ajax({
            url: '/PriceQuotationDetails/Duplicate',
            method: 'POST',
            data: { id: quotationId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Quotation duplicated successfully!');
                    window.location.href = '/PriceQuotationDetails/Index/' + response.newQuotationId;
                } else {
                    toastr.warning('Failed to duplicate quotation: ' + response.message);
                }
            },
            error: function () {
                alert('Failed to duplicate quotation. Please try again.');
            }
        });
    }
}

function convertToSalesOrder(quotationId) {
    if (confirm('Are you sure you want to convert this quotation to a Work Order? This action cannot be undone.')) {
        $.ajax({
            url: '/PriceQuotationDetails/ConvertToSalesOrder',
            method: 'POST',
            data: { id: quotationId },
            success: function (response) {
                if (response.success) {
                    toastr.success('Quotation converted to Work Order successfully!');
                    window.location.href = '/SalesOrder/Index/' + response.workOrderId;
                } else {
                    toastr.warning('Failed to convert to Work Order: ' + response.message);
                }
            },
            error: function () {
                toastr.warning('Failed to convert to Work Order. Please try again.');
            }
        });
    }
}
function convertToWorkOrder(quotationId) {
    if (confirm('Are you sure you want to convert this quotation to a Work Order? This action cannot be undone.')) {
        $.ajax({
            url: '/PriceQuotationDetails/ConvertToWorkOrder',
            method: 'POST',
            data: { id: quotationId },
            success: function (response) {
                if (response.success) {
                    alert('Quotation converted to Work Order successfully!');
                    window.location.href = '/WorkOrder/Index/' + response.workOrderId;
                } else {
                    alert('Failed to convert to Work Order: ' + response.message);
                }
            },
            error: function () {
                alert('Failed to convert to Work Order. Please try again.');
            }
        });
    }
}

function markAsApproved(quotationId) {
    if (confirm('Mark this quotation as Approved?')) {
        $.ajax({
            url: '/PriceQuotationDetails/UpdateStatus',
            method: 'POST',
            data: {
                id: quotationId,
                status: 3 // QuotationStatus.Approved
            },
            success: function (response) {
                if (response.success) {
                    alert('Quotation marked as Approved!');
                    location.reload();
                } else {
                    alert('Failed to update status: ' + response.message);
                }
            },
            error: function () {
                alert('Failed to update status. Please try again.');
            }
        });
    }
}

function sendEmailNow(quotationId) {
    const recipient = $('#recipientEmail').val();
    const subject = $('#emailSubject').val();
    const message = $('#emailMessage').val();

    if (!recipient) {
        alert('Please enter recipient email address');
        return;
    }

    $.ajax({
        url: '/PriceQuotationDetails/SendEmail',
        method: 'POST',
        data: {
            id: quotationId,
            recipientEmail: recipient,
            subject: subject,
            message: message
        },
        success: function (response) {
            if (response.success) {
                alert('Email sent successfully!');
                $('#sendEmailModal').modal('hide');

                // Update status to "Sent"
                location.reload();
            } else {
                alert('Failed to send email: ' + response.message);
            }
        },
        error: function () {
            alert('Failed to send email. Please try again.');
        }
    });
}

function printQuotation(quotationId) {
    window.open('/PriceQuotationDetails/PrintPDF/' + quotationId, '_blank');
}

function deleteQuotation(quotationId) {
    if (confirm('Are you sure you want to delete this quotation? This action cannot be undone.')) {
        $.ajax({
            url: '/PriceQuotationDetails/Delete',
            method: 'POST',
            data: { id: quotationId },
            success: function (response) {
                if (response.success) {
                    alert('Quotation deleted successfully!');
                    window.location.href = '/PriceQuotation/Index';
                } else {
                    alert('Failed to delete quotation: ' + response.message);
                }
            },
            error: function () {
                alert('Failed to delete quotation. Please try again.');
            }
        });
    }
}