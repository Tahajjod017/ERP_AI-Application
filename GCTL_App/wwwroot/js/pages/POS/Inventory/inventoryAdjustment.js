$(document).ready(function () {
    function loadAdjustmentHistory() {
        $.ajax({
            url: '/Inventory/GetAdjustmentHistory',
            data: { page: 1, pageSize: 50 },
            success: function (res) {
                let html = '';
                $.each(res.data, function (i, item) {
                    const typeClass = item.adjustmentType === 'Add' ? 'text-success' : 'text-danger';
                    html += `
                        <tr>
                            <td class="fs-10">${new Date(item.adjustmentDate).toLocaleDateString('en-GB')}</td>
                            <td>${item.productName}</td>
                            <td>${item.locationName}</td>
                            <td class="${typeClass} fw-bold">${item.adjustmentType}</td>
                            <td>${item.quantity}</td>
                            <td>${item.reason}</td>
                            <td>${item.adjustedByName}</td>
                        </tr>`;
                });

                if (res.data.length === 0) {
                    html = '<tr><td colspan="7" class="text-center text-muted">No adjustments yet</td></tr>';
                }

                $('#adjustmentHistoryBody').html(html);
            }
        });
    }

    //$('#adjustmentForm').on('submit', function (e) {
    //    e.preventDefault();

    //    const model = {
    //        productID: $('#productId').val(),
    //        locationID: $('#locationId').val(),
    //        adjustmentType: $('#adjustmentType').val(),
    //        quantity: parseFloat($('#quantity').val()),
    //        reason: $('#reason').val(),
    //        note: $('#note').val()
    //    };

    //    $.ajax({
    //        url: '/Inventory/CreateAdjustment',
    //        method: 'POST',
    //        data: { model: model },
    //        success: function (res) {

    $('#adjustmentForm').on('submit', function (e) {
        e.preventDefault();

        const formData = new FormData();

        formData.append("ProductID", choiceManager.getChoiceValue('productId'));
        formData.append("LocationID", choiceManager.getChoiceValue('locationId') );
        formData.append("AdjustmentType", choiceManager.getChoiceValue('adjustmentType'));
        formData.append("Quantity", $('#quantity').val());
        formData.append("Reason", choiceManager.getChoiceValue('reason'));
        formData.append("Note", $('#note').val());

        // If you want to send NewAverageCost when available:
        const newAverageCost = $('#newAverageCost').val();
        if (newAverageCost) {
            formData.append("NewAverageCost", newAverageCost);
        }
        formData.append("__RequestVerificationToken", $('input[name="__RequestVerificationToken"]').val());

        $.ajax({
            url: '/Inventory/CreateAdjustment',
            type: 'POST',
            data: formData,
            processData: false,   // prevent jQuery from processing data
            contentType: false,   // prevent jQuery from setting content type
            success: function (res) {


                if (res.success) {
                    toastr.success(res.message || 'Adjustment saved successfully!');
                    $('#adjustmentForm')[0].reset();
                    choiceManager.resetAllChoices();
                    loadAdjustmentHistory();
                } else {
                    let errors = '';
                    $.each(res.errors, function (key, msgs) {
                        errors += msgs.join('\n') + '\n';
                    });
                    toastr.warning('Error:\n' + errors);
                }
            },
            error: function () {
                toastr.error('Failed to save adjustment.');
            }
        });
    });

    // Initialize
    loadAdjustmentHistory();
});