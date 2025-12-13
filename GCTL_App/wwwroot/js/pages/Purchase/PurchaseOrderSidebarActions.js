// ==============================================================
// PURCHASE ORDER SIDEBAR ACTIONS
// ==============================================================

function duplicatePurchaseOrder(id) {
    if (!confirm('Are you sure you want to duplicate this purchase order?')) {
        return;
    }

    $.ajax({
        url: '/PurchaseOrderDetails/Duplicate',
        method: 'POST',
        data: {
            id: id,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success('Purchase Order duplicated successfully!');
                window.location.href = '/PurchaseOrderDetails/Index/' + response.newPurchaseOrderId;
            } else {
                toastr.error(response.message || 'Failed to duplicate purchase order');
            }
        },
        error: function () {
            toastr.error('Failed to duplicate purchase order. Please try again.');
        }
    });
}

function markAsApproved(id) {
    if (!confirm('Are you sure you want to mark this purchase order as approved?')) {
        return;
    }

    $.ajax({
        url: '/PurchaseOrderDetails/UpdateStatus',
        method: 'POST',
        data: {
            id: id,
            status: 3, // Approved status
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success('Purchase Order marked as approved!');
                location.reload();
            } else {
                toastr.error(response.message || 'Failed to update status');
            }
        },
        error: function () {
            toastr.error('Failed to update status. Please try again.');
        }
    });
}

function convertToReceive(id) {
    if (!confirm('Are you sure you want to convert this purchase order to a receive?')) {
        return;
    }

    toastr.info('Receive conversion functionality - to be implemented');

    // TODO: Implement conversion to purchase receive
    // $.ajax({
    //     url: '/PurchaseOrderDetails/ConvertToReceive',
    //     method: 'POST',
    //     data: {
    //         id: id,
    //         __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    //     },
    //     success: function (response) {
    //         if (response.success) {
    //             toastr.success('Converted to Receive successfully!');
    //             window.location.href = '/PurchaseReceive/Index/' + response.receiveId;
    //         } else {
    //             toastr.error(response.message || 'Failed to convert');
    //         }
    //     },
    //     error: function () {
    //         toastr.error('Failed to convert. Please try again.');
    //     }
    // });
}

function sendEmailNow(id) {
    const recipientEmail = $('#recipientEmail').val();
    const subject = $('#emailSubject').val();
    const message = $('#emailMessage').val();

    if (!recipientEmail) {
        toastr.warning('Please enter recipient email');
        return;
    }

    toastr.info('Email functionality - to be implemented');

    // TODO: Implement email sending
    // $.ajax({
    //     url: '/PurchaseOrderDetails/SendEmail',
    //     method: 'POST',
    //     data: {
    //         id: id,
    //         recipientEmail: recipientEmail,
    //         subject: subject,
    //         message: message,
    //         __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    //     },
    //     success: function (response) {
    //         if (response.success) {
    //             toastr.success('Email sent successfully!');
    //             $('#showSendModal').modal('hide');
    //         } else {
    //             toastr.error(response.message || 'Failed to send email');
    //         }
    //     },
    //     error: function () {
    //         toastr.error('Failed to send email. Please try again.');
    //     }
    // });
}

function printPurchaseOrder(id) {
    window.print();
    // Or open in new window for PDF generation
    // window.open('/PurchaseOrderDetails/PrintPDF/' + id, '_blank');
}

function deletePurchaseOrder(id) {
    if (!confirm('Are you sure you want to delete this purchase order? This action cannot be undone.')) {
        return;
    }

    $.ajax({
        url: '/PurchaseOrderDetails/Delete',
        method: 'POST',
        data: {
            id: id,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success('Purchase Order deleted successfully!');
                window.location.href = '/PurchaseOrderList/Index';
            } else {
                toastr.error(response.message || 'Failed to delete purchase order');
            }
        },
        error: function () {
            toastr.error('Failed to delete purchase order. Please try again.');
        }
    });
}
