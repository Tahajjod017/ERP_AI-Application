let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let totalPages = 0;
let totalRecords = 0;
let sortColumn = 'CreatedAt';
let sortDirection = 'desc';

$(document).ready(function () {
    loadPurchaseOrders();

    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadPurchaseOrders();
    });

    let searchTimeout;
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            searchTerm = $('#searchInput').val();
            currentPage = 1;
            loadPurchaseOrders();
        }, 500);
    });

    $(document).on('click', '.sort-header', function (e) {
        e.preventDefault();
        const column = $(this).data('column');

        if (sortColumn === column) {
            sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            sortColumn = column;
            sortDirection = 'asc';
        }

        currentPage = 1;
        updateSortIcons();
        loadPurchaseOrders();
    });
});

function loadPurchaseOrders() {
    $.ajax({
        url: '/PurchaseOrderList/GetPurchaseOrderList',
        type: 'GET',
        data: {
            page: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortDirection: sortDirection
        },
        beforeSend: function () {
            $('#purchase-order-table').html('<tr><td colspan="11" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
        },
        success: function (response) {
            if (response.success) {
                totalPages = response.totalPages;
                totalRecords = response.totalRecords;
                currentPage = response.currentPage;
                renderTable(response.data);
                updatePaginationInfo();
                renderPagination();
            } else {
                showError(response.message);
            }
        },
        error: function (xhr, status, error) {
            showError('Failed to load purchase orders: ' + error);
        }
    });
}

function renderTable(data) {
    const tbody = $('#purchase-order-table');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="11" class="text-center">No data found</td></tr>');
    } else {
        data.forEach(item => {
            const statusBadge = getStatusBadge(item.status);
            const row = `
                <tr>
                    <td class="align-middle">${item.purchaseOrderID}</td>
                    <td class="align-middle">${item.poid || '-'}</td>
                    <td class="align-middle">${item.supplierName || '-'}</td>
                    <td class="align-middle">${item.createdBy || '-'}</td>
                    <td class="align-middle">${item.purchaseDate ? new Date(item.purchaseDate).toLocaleDateString() : '-'}</td>
                    <td class="align-middle text-end">${(item.grandTotalAmount || 0).toFixed(2)}</td>
                    <td class="align-middle text-end">${(item.paidAmount || 0).toFixed(2)}</td>
                    <td class="align-middle text-end">${(item.dueAmount || 0).toFixed(2)}</td>
                    <td class="align-middle text-center">${item.totalItems}</td>
                    <td class="align-middle">${statusBadge}</td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-primary" onclick="viewDetails(${item.purchaseOrderID})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-phoenix-info" onclick="editPurchaseOrder(${item.purchaseOrderID})" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
    }
}

function getStatusBadge(status) {
    const statusMap = {
        'Draft': 'badge-phoenix-warning',
        'Pending': 'badge-phoenix-info',
        'Approved': 'badge-phoenix-success',
        'PartiallyReceived': 'badge-phoenix-primary',
        'Received': 'badge-phoenix-success',
        'Cancelled': 'badge-phoenix-danger'
    };

    const badgeClass = statusMap[status] || 'badge-phoenix-secondary';
    return `<span class="badge ${badgeClass} fs-9">${status}</span>`;
}

function updateSortIcons() {
    $('[id^="sort-"]').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

    const icon = $('#sort-' + sortColumn);
    icon.removeClass('fa-sort');
    if (sortDirection === 'asc') {
        icon.addClass('fa-sort-up');
    } else {
        icon.addClass('fa-sort-down');
    }
}

function updatePaginationInfo() {
    const startIndex = totalRecords === 0 ? 0 : (currentPage - 1) * pageSize + 1;
    const endIndex = Math.min(currentPage * pageSize, totalRecords);

    $('#showDown2').text(`Showing ${startIndex} to ${endIndex} of ${totalRecords}`);
}

function renderPagination() {
    const paginationContainer = $('.pagination');

    paginationContainer.find('li:not([data-list-pagination])').remove();

    const prevBtn = paginationContainer.find('[data-list-pagination="prev"]');
    if (currentPage === 1 || totalPages === 0) {
        prevBtn.addClass('disabled');
    } else {
        prevBtn.removeClass('disabled');
    }

    if (totalPages > 0) {
        const maxVisiblePages = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage < maxVisiblePages - 1) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        if (startPage > 1) {
            const firstPage = `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="goToPage(1); return false;">1</a>
                </li>
            `;
            paginationContainer.find('[data-list-pagination="next"]').before(firstPage);

            if (startPage > 2) {
                paginationContainer.find('[data-list-pagination="next"]').before('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            const pageItem = `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="goToPage(${i}); return false;">${i}</a>
                </li>
            `;
            paginationContainer.find('[data-list-pagination="next"]').before(pageItem);
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationContainer.find('[data-list-pagination="next"]').before('<li class="page-item disabled"><span class="page-link">...</span></li>');
            }

            const lastPage = `
                <li class="page-item">
                    <a class="page-link" href="#" onclick="goToPage(${totalPages}); return false;">${totalPages}</a>
                </li>
            `;
            paginationContainer.find('[data-list-pagination="next"]').before(lastPage);
        }
    }

    const nextBtn = paginationContainer.find('[data-list-pagination="next"]');
    if (currentPage === totalPages || totalPages === 0) {
        nextBtn.addClass('disabled');
    } else {
        nextBtn.removeClass('disabled');
    }

    prevBtn.off('click').on('click', function (e) {
        e.preventDefault();
        if (currentPage > 1) {
            goToPage(currentPage - 1);
        }
    });

    nextBtn.off('click').on('click', function (e) {
        e.preventDefault();
        if (currentPage < totalPages) {
            goToPage(currentPage + 1);
        }
    });
}

function goToPage(page) {
    currentPage = page;
    loadPurchaseOrders();
    $('html, body').animate({
        scrollTop: $("#PurchaseOrderList").offset().top - 100
    }, 300);
}

function showError(message) {
    $('#errorContainer').html(`
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <strong>Error!</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `).show();

    setTimeout(function () {
        $('#errorContainer').fadeOut();
    }, 5000);
}

function viewDetails(id) {
    window.location.href = `/PurchaseOrderDetails/index/${id}`;
}

function editPurchaseOrder(id) {
    window.location.href = `/PurchaseOrderDetails/Edit/${id}`;
}
