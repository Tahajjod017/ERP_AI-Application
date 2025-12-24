$(document).ready(function () {
    let page = 1;
    let pageSize = 10;
    let search = "";
    let sortColumn = "TransactionDate";
    let sortDirection = "desc";
    let locationId = "";
    let productId = "";
    let transactionType = "";
    let fromDate = "";
    let toDate = "";

    function loadTransactions() {
        $.ajax({
            url: '/Inventory/GetTransactionHistory',
            data: {
                page, pageSize, search, sortColumn, sortDirection,
                locationId, productId, transactionType, fromDate, toDate
            },
            success: function (res) {
                let rows = '';
                $.each(res.data, function (i, item) {
                    const typeClass = item.transactionType === 'IN' ? 'text-success' : 'text-danger';
                    const typeIcon = item.transactionType === 'IN' ? '↑' : '↓';

                    rows += `
                        <tr>
                            <td>${new Date(item.transactionDate).toLocaleString('en-GB')}</td>
                            <td class="${typeClass} fw-bold">${typeIcon} ${item.transactionType}</td>
                            <td>${item.productName}</td>
                            <td>${item.locationName || '-'}</td>
                            <td class="text-end">${item.quantity}</td>
                            <td class="text-end">৳${item.unitCost.toFixed(2)}</td>
                            <td class="text-end">${item.balanceAfter}</td>
                            <td>${item.referenceType} ${item.referenceNumber || ''}</td>
                            <td class="text-muted">${item.note || '-'}</td>
                            <td>${item.createdByName}</td>
                        </tr>`;
                });

                if (rows === '') {
                    rows = '<tr><td colspan="10" class="text-center text-muted">No transactions found</td></tr>';
                }

                $("#transactions-table-body").html(rows);
                renderPagination(res.totalRecords, page, pageSize);
                $("[data-list-info='list-info']").text(
                    `Showing ${(page - 1) * pageSize + 1} to ${Math.min(page * pageSize, res.totalRecords)} of ${res.totalRecords}`
                );
            },
            error: function () {
                $("#transactions-table-body").html(
                    '<tr><td colspan="10" class="text-center text-danger">Failed to load data</td></tr>'
                );
            }
        });
    }

    // Filters & Search
    $("#searchInput").on("keyup", function () {
        search = $(this).val();
        page = 1;
        loadTransactions();
    });

    $("#pageSizeSelect, #locationFilter, #productFilter, #typeFilter").on("change", function () {
        pageSize = $("#pageSizeSelect").val();
        locationId = $("#locationFilter").val();
        productId = $("#productFilter").val();
        transactionType = $("#typeFilter").val();
        page = 1;
        loadTransactions();
    });

    $("#fromDate, #toDate").on("change", function () {
        fromDate = $("#fromDate").val();
        toDate = $("#toDate").val();
        page = 1;
        loadTransactions();
    });

    // Pagination
    $(document).on("click", "#pagination .page-link", function (e) {
        e.preventDefault();
        page = parseInt($(this).text());
        loadTransactions();
    });

    $("#prevBtn").on("click", function (e) {
        e.preventDefault();
        if (page > 1) { page--; loadTransactions(); }
    });

    $("#nextBtn").on("click", function (e) {
        e.preventDefault();
        page++; loadTransactions();
    });

    // Sorting
    $("#InventoryTransactions th.sort").on("click", function () {
        sortColumn = $(this).data("sort");
        sortDirection = sortDirection === "asc" ? "desc" : "asc";
        loadTransactions();
    });

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
    loadTransactions();
});