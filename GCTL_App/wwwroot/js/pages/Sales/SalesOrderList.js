let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let totalPages = 0;
let totalRecords = 0;
let sortColumn = 'CreatedAt';
let sortDirection = 'desc';

$(document).ready(function () {
    loadSalesOrders();

    // Page size change
    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadSalesOrders();
    });

    // Search with debounce
    let searchTimeout;
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            searchTerm = $('#searchInput').val();
            currentPage = 1;
            loadSalesOrders();
        }, 500);
    });

    // Sort headers click event
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
        loadSalesOrders();
    });
});

function loadSalesOrders() {
    $.ajax({
        url: '/SalesOrderList/GetSalesOrderList',
        type: 'GET',
        data: {
            page: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortDirection: sortDirection
        },
        beforeSend: function () {
            $('#sales-order-table').html('<tr><td colspan="11" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
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
            showError('Failed to load sales orders: ' + error);
        }
    });
}

function renderTable(data) {
    const tbody = $('#sales-order-table');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="11" class="text-center">No data found</td></tr>');
    } else {
        data.forEach(item => {
            const row = `
                <tr>
                    <td class="align-middle">${item.salesOrderID}</td>
                    <td class="align-middle">${item.salesOrderNumber || '-'}</td>
                    <td class="align-middle">${item.customerName || '-'}</td>
                    <td class="align-middle">${item.quotationNumber || '-'}</td>
                    <td class="align-middle">${item.createdBy || '-'}</td>
                    <td class="align-middle">${item.salesOrderDate ? new Date(item.salesOrderDate).toLocaleDateString() : '-'}</td>
                    <td class="align-middle text-center">${item.totalItems}</td>
                    <td class="align-middle">${item.vatPercentage || 0}%</td>
                    <td class="align-middle">${item.note || '-'}</td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-danger" onclick="exportPDF(${item.salesOrderID})" title="Export PDF">
                            <i class="fas fa-file-pdf"></i>
                        </button>
                    </td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-primary" onclick="viewDetails(${item.salesOrderID})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-phoenix-info" onclick="editSalesOrder(${item.salesOrderID})" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        DynamicTableDrag.refreshTableSettings('SalesOrderList');
    }
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
    loadSalesOrders();
    $('html, body').animate({
        scrollTop: $("#SalesOrderList").offset().top - 100
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

function exportPDF(id) {
    console.log('Export PDF for ID:', id);
    window.open(`/SalesOrderList/ExportPDF/${id}`, '_blank');
}

function viewDetails(id) {
    window.location.href = `/SalesOrderDetails/Index/${id}`;
}

function editSalesOrder(id) {
    window.location.href = `/SalesOrderDetails/Edit/${id}`;
}
