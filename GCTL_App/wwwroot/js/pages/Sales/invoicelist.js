let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let totalPages = 0;
let totalRecords = 0;
let sortColumn = 'CreatedAt';
let sortDirection = 'desc';

$(document).ready(function () {
    loadInvoices();

    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadInvoices();
    });

    let searchTimeout;
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            searchTerm = $('#searchInput').val();
            currentPage = 1;
            loadInvoices();
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
        loadInvoices();
    });
});

function loadInvoices() {
    $.ajax({
        url: '/InvoiceList/GetInvoiceList',
        type: 'GET',
        data: {
            page: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortDirection: sortDirection
        },
        beforeSend: function () {
            $('#invoice-table').html('<tr><td colspan="13" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
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
            showError('Failed to load invoices: ' + error);
        }
    });
}

function renderTable(data) {

    showDev(data, 'ddd')

    const tbody = $('#invoice-table');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="13" class="text-center">No data found</td></tr>');
    } else {
        data.forEach(item => {
            const statusBadge = item.isDraft ? '<span class="badge badge-phoenix badge-phoenix-warning">Draft</span>' :
                item.status === 'Paid' ? '<span class="badge badge-phoenix badge-phoenix-success">Paid</span>' :
                    '<span class="badge badge-phoenix badge-phoenix-danger">Unpaid</span>';

            const row = `
                <tr>
                    <td class="align-middle">${item.invoiceID}</td>
                    <td class="align-middle">${item.invoiceNumber || '-'}</td>
                    <td class="align-middle">${item.customerName || '-'}</td>
                    <td class="align-middle">${item.salesOrderNumber || '-'}</td>
                    <td class="align-middle">${item.createdBy || '-'}</td>
                    <td class="align-middle">${item.invoiceDate ? new Date(item.invoiceDate).toLocaleDateString() : '-'}</td>
                    <td class="align-middle text-center">${item.totalItems}</td>
                    <td class="align-middle">${item.vatPercentage || 0}%</td>
                    <td class="align-middle text-end">${item.grandTotal.toFixed(2)}</td>
                    <td class="align-middle text-end">${item.paidAmount.toFixed(2)}</td>
                    <td class="align-middle text-end">${item.dueAmount.toFixed(2)}</td>
                    <td class="align-middle text-center">${statusBadge}</td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-danger" onclick="exportPDF(${item.invoiceID})" title="Export PDF">
                            <i class="fas fa-file-pdf"></i>
                        </button>
                    </td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-primary" onclick="viewDetails(${item.invoiceID})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        ${!item.isDraft ? '' : `
                        <button class="btn btn-sm btn-phoenix-info" onclick="editInvoice(${item.invoiceID})" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>`}
                    </td>
                </tr>
            `;
            tbody.append(row);
        });

        DynamicTableDrag.refreshTableSettings('InvoiceList');
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
    loadInvoices();
    $('html, body').animate({
        scrollTop: $("#InvoiceList").offset().top - 100
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
    window.open(`/InvoiceList/ExportPDF/${id}`, '_blank');
}

function viewDetails(id) {
    
    window.location.href = `/InvoiceDetails/Index/${id}`;
}

function editInvoice(id) {
    window.location.href = `/InvoiceDetails/Edit/${id}`;
}
