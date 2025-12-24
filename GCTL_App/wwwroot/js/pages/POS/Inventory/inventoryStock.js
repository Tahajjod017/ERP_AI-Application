$(document).ready(function () {
    let page = 1;
    let pageSize = 10;
    let search = "";
    let sortColumn = "ProductName";
    let sortDirection = "asc";
    let locationId = "";
    let productTypeId = "";
    let lowStockOnly = false;

    function loadInventoryStock() {
        $.ajax({
            url: '/Inventory/GetInventoryStock',
            data: {
                page, pageSize, search, sortColumn, sortDirection,
                locationId, productTypeId, lowStockOnly
            },
            success: function (res) {
                let rows = '';
                $.each(res.data, function (i, item) {
                    const stockClass = item.isOutOfStock ? 'text-danger fw-bold' :
                        item.isLowStock ? 'text-warning fw-bold' : '';

                    const stockIcon = item.isOutOfStock ? '⚠️' :
                        item.isLowStock ? '⚠' : '';

                    rows += `
                        <tr>
                            <td class="ps-0">${item.productName}</td>
                            <td class="ps-2">${item.productType}</td>
                            <td class="ps-2">${item.brand}</td>
                            <td class="ps-2">${item.locationName}</td>
                            <td class="ps-2 ${stockClass}">${stockIcon} ${item.quantity}</td>
                            <td class="ps-2">${item.reservedQuantity}</td>
                            <td class="ps-2 fw-bold">${item.availableQuantity}</td>
                            <td class="ps-2 text-muted">${item.minimumQuantity}</td>
                            <td class="ps-2 fw-bold">৳${item.totalValue.toFixed(2)}</td>
                            <td class="text-end">
                                <a href="#" class="nav-item viewStockBtn" data-id="${item.productID}" 
                                   data-bs-toggle="modal" data-bs-target="#viewStockModal" title="View all locations">
                                    <i class="fas fa-eye text-black"></i>
                                </a>
                            </td>
                        </tr>`;
                });

                if (rows === '') {
                    rows = '<tr><td colspan="10" class="text-center text-muted">No stock data found</td></tr>';
                }

                $("#inventory-stock-table").html(rows);

                DynamicTableDrag.refreshTableSettings('InventoryStock');
                renderPagination(res.totalRecords, page, pageSize);
                $("[data-list-info='list-info']").text(
                    `Showing ${(page - 1) * pageSize + 1} to ${Math.min(page * pageSize, res.totalRecords)} of ${res.totalRecords}`
                );
            },
            error: function () {
                $("#inventory-stock-table").html(
                    '<tr><td colspan="10" class="text-center text-danger">Failed to load data</td></tr>'
                );
            }
        });
    }

    // Filters
    $("#searchInput").on("keyup", function () {
        search = $(this).val();
        page = 1;
        loadInventoryStock();
    });

    $("#pageSizeSelect, #locationFilter, #productTypeFilter").on("change", function () {
        pageSize = $("#pageSizeSelect").val();
        locationId = $("#locationFilter").val();
        productTypeId = $("#productTypeFilter").val();
        page = 1;
        loadInventoryStock();
    });

    $("#lowStockOnlyFilter").on("change", function () {
        lowStockOnly = $(this).is(":checked");
        page = 1;
        loadInventoryStock();
    });

    // Pagination
    $(document).on("click", "#pagination .page-link", function (e) {
        e.preventDefault();
        page = parseInt($(this).text());
        loadInventoryStock();
    });

    $("#prevBtn").on("click", function (e) {
        e.preventDefault();
        if (page > 1) { page--; loadInventoryStock(); }
    });

    $("#nextBtn").on("click", function (e) {
        e.preventDefault();
        page++; loadInventoryStock();
    });

    // Sorting
    $("#InventoryStock th.sort").on("click", function () {
        sortColumn = $(this).data("sort");
        sortDirection = sortDirection === "asc" ? "desc" : "asc";
        loadInventoryStock();
    });

    // View stock by location
    $(document).on("click", ".viewStockBtn", function () {
        const productId = $(this).data('id');

        $.ajax({
            url: '/Inventory/GetProductStock',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    populateStockModal(response.data);
                }
            }
        });
    });

    function populateStockModal(data) {
        $("#modal_ProductName").text(data.productName);

        let html = '';
        if (data.locations && data.locations.length > 0) {
            $.each(data.locations, function (i, loc) {
                html += `
                    <tr>
                        <td>${loc.locationName}</td>
                        <td class="fw-bold">${loc.quantity}</td>
                        <td class="text-warning">${loc.reservedQuantity}</td>
                        <td class="text-success fw-bold">${loc.availableQuantity}</td>
                    </tr>`;
            });
        } else {
            html = '<tr><td colspan="4" class="text-center text-muted">No stock in any location</td></tr>';
        }

        $("#modal_LocationsTable").html(html);
    }

    function renderPagination(totalRecords, currentPage, pageSize) {
        const totalPages = Math.ceil(totalRecords / pageSize);
        let html = '';

        for (let i = 1; i <= totalPages; i++) {
            html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                <a class="page-link" href="#">${i}</a>
            </li>`;
        }

        $("#pagination").html(html);
    }

    // Initialize
    loadInventoryStock();
});
