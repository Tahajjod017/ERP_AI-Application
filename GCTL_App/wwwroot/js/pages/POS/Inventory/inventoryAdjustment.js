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

    $('#adjustmentForm').on('submit', function (e) {
        e.preventDefault();

        const model = {
            productID: $('#productId').val(),
            locationID: $('#locationId').val(),
            adjustmentType: $('#adjustmentType').val(),
            quantity: parseFloat($('#quantity').val()),
            reason: $('#reason').val(),
            note: $('#note').val()
        };

        $.ajax({
            url: '/Inventory/CreateAdjustment',
            method: 'POST',
            data: { model: model },
            success: function (res) {
                if (res.success) {
                    alert('Adjustment saved successfully!');
                    $('#adjustmentForm')[0].reset();
                    loadAdjustmentHistory();
                } else {
                    let errors = '';
                    $.each(res.errors, function (key, msgs) {
                        errors += msgs.join('\n') + '\n';
                    });
                    alert('Error:\n' + errors);
                }
            },
            error: function () {
                alert('Failed to save adjustment.');
            }
        });
    });

    // Initialize
    loadAdjustmentHistory();
});