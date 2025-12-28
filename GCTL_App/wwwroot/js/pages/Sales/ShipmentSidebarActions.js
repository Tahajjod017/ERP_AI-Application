// ==============================================================
// SHIPMENT SIDEBAR ACTIONS
// ==============================================================

function markAsPacked(id) {
    if (!confirm('Are you sure you want to mark this shipment as Packed?')) {
        return;
    }

    updateShipmentStatus(id, 2, 'Shipment marked as Packed successfully!');
}

function markAsShipped(id) {
    if (!confirm('Are you sure you want to mark this shipment as Shipped? This will deduct inventory.')) {
        return;
    }

    updateShipmentStatus(id, 3, 'Shipment marked as Shipped successfully!');
}

function markAsInTransit(id) {
    if (!confirm('Are you sure you want to mark this shipment as In Transit?')) {
        return;
    }

    updateShipmentStatus(id, 4, 'Shipment marked as In Transit successfully!');
}

function markAsDelivered(id) {
    if (!confirm('Are you sure you want to mark this shipment as Delivered?')) {
        return;
    }

    updateShipmentStatus(id, 5, 'Shipment marked as Delivered successfully!');
}

function cancelShipment(id) {
    if (!confirm('Are you sure you want to cancel this shipment? This action cannot be undone.')) {
        return;
    }

    updateShipmentStatus(id, 6, 'Shipment cancelled successfully!');
}

function updateShipmentStatus(id, status, successMessage) {
    $.ajax({
        url: '/ChallanDetails/UpdateStatus',
        method: 'POST',
        data: {
            id: id,
            status: status,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success(successMessage);
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

function printShipment(id) {
    window.print();
    // Or open in new window for PDF generation
    // window.open('/ChallanDetails/PrintPDF/' + id, '_blank');
}

function deleteShipment(id) {
    if (!confirm('Are you sure you want to delete this shipment? This action cannot be undone.')) {
        return;
    }

    $.ajax({
        url: '/ChallanDetails/Delete',
        method: 'POST',
        data: {
            id: id,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success('Shipment deleted successfully!');
                window.location.href = '/ChallanList/Index';
            } else {
                toastr.error(response.message || 'Failed to delete shipment');
            }
        },
        error: function () {
            toastr.error('Failed to delete shipment. Please try again.');
        }
    });
}

// Track shipment - external tracking link
function trackShipment(trackingNumber, carrier) {
    if (!trackingNumber) {
        toastr.warning('No tracking number available');
        return;
    }

    // Default tracking URLs for common carriers
    const trackingUrls = {
        'fedex': `https://www.fedex.com/fedextrack/?trknbr=${trackingNumber}`,
        'ups': `https://www.ups.com/track?tracknum=${trackingNumber}`,
        'dhl': `https://www.dhl.com/en/express/tracking.html?AWB=${trackingNumber}`,
        'usps': `https://tools.usps.com/go/TrackConfirmAction?tLabels=${trackingNumber}`
    };

    const url = trackingUrls[carrier?.toLowerCase()] || trackingUrls['fedex'];
    window.open(url, '_blank');
}

// Generate shipping label
function generateShippingLabel(id) {
    toastr.info('Shipping label generation - to be implemented');

    // TODO: Implement shipping label generation
    // window.open('/ChallanDetails/GenerateLabel/' + id, '_blank');
}

// Send shipment notification
function sendShipmentNotification(id) {
    if (!confirm('Send shipment notification to customer?')) {
        return;
    }

    $.ajax({
        url: '/ChallanDetails/SendNotification',
        method: 'POST',
        data: {
            id: id,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function (response) {
            if (response.success) {
                toastr.success('Notification sent successfully!');
            } else {
                toastr.error(response.message || 'Failed to send notification');
            }
        },
        error: function () {
            toastr.error('Failed to send notification. Please try again.');
        }
    });
}
