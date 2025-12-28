$(document).ready(function () {
    // Stock Report
    $('#stockReportForm').on('submit', function (e) {
        e.preventDefault();
        const locationId = $('#reportLocation').val() || null;
        const productTypeId = $('#reportProductType').val() || null;
        const lowStockOnly = $('#reportLowStockOnly').is(':checked');

        const url = `/Inventory/GenerateStockReport?locationId=${locationId}&productTypeId=${productTypeId}&lowStockOnly=${lowStockOnly}`;
        window.open(url, '_blank');
    });

    $('#excelBtn').on('click', function () {
        const locationId = $('#reportLocation').val() || null;
        const productTypeId = $('#reportProductType').val() || null;
        const lowStockOnly = $('#reportLowStockOnly').is(':checked');

        const url = `/Inventory/DownloadStockExcel?locationId=${locationId}&productTypeId=${productTypeId}&lowStockOnly=${lowStockOnly}`;
        window.location.href = url;
    });

    // Movement Report
    $('#movementReportForm').on('submit', function (e) {
        e.preventDefault();

        const fromDate = $('#moveFromDate').val();
        const toDate = $('#moveToDate').val();
        const locationId = $('#moveLocation').val() || null;
        const productId = $('#moveProduct').val() || null;

        if (!fromDate || !toDate) {
            alert('Please select date range');
            return;
        }

        const url = `/Inventory/GenerateMovementReport?fromDate=${fromDate}&toDate=${toDate}&locationId=${locationId}&productId=${productId}`;
        window.open(url, '_blank');
    });
});