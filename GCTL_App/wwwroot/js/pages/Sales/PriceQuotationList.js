let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let totalPages = 0;
let totalRecords = 0;
let sortColumn = 'CreatedAt';
let sortDirection = 'desc';

$(document).ready(function () {
    loadPriceQuotations();

    // Page size change
    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadPriceQuotations();
    });

    // Search with debounce
    let searchTimeout;
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            searchTerm = $('#searchInput').val();
            currentPage = 1;
            loadPriceQuotations();
        }, 500);
    });

    // Sort headers click event
    $(document).on('click', '.sort-header', function (e) {
        e.preventDefault();
        const column = $(this).data('column');

        // Toggle sort direction if same column, otherwise set to asc
        if (sortColumn === column) {
            sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            sortColumn = column;
            sortDirection = 'asc';
        }

        currentPage = 1;
        updateSortIcons();
        loadPriceQuotations();
    });
});

function loadPriceQuotations() {
    $.ajax({
        url: '/PriceQuotationList/GetPriceQuotationList',
        type: 'GET',
        data: {
            page: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortDirection: sortDirection
        },
        beforeSend: function () {
            $('#product-Requisition-history-table').html('<tr><td colspan="11" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
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
            showError('Failed to load price quotations: ' + error);
        }
    });
}

function renderTable(data) {
    const tbody = $('#product-Requisition-history-table');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="11" class="text-center">No data found</td></tr>');
    } else {
        data.forEach(item => {
            const row = `
                <tr>
                    <td class="align-middle">${item.priceQuotationID}</td>
                    <td class="align-middle">${item.quotationNumber || '-'}</td>
                    <td class="align-middle">${item.customerName || '-'}</td>
                    <td class="align-middle">${item.createdBy || '-'}</td>
                    <td class="align-middle">${item.quotationDate ? new Date(item.quotationDate).toLocaleDateString() : '-'}</td>
                    <td class="align-middle text-center">${item.totalItems}</td>
                    <td class="align-middle">${item.vatPercentage || 0}%</td>
                   
                    <td class="align-middle">${item.note || '-'}</td>
                    <td class="align-middle text-center">
                        <button class="btn btn-sm btn-phoenix-danger" onclick="exportPDF(${item.priceQuotationID})" title="Export PDF">
                            <i class="fas fa-file-pdf"></i>
                        </button>
                    </td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-primary" onclick="viewDetails(${item.priceQuotationID})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-phoenix-info" onclick="editQuotation(${item.priceQuotationID})" title="Edit">
                            <i class="fas fa-edit"></i>
                        </button>
                    </td>
                </tr>
            `;
            tbody.append(row);
        });
        //<td class="align-middle"><span class="badge badge-phoenix badge-phoenix-success fs-9">Active</span></td>
        DynamicTableDrag.refreshTableSettings('PriceQuotationList');


    }
}

function updateSortIcons() {
    // Reset all sort icons
    $('[id^="sort-"]').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

    // Update current sort column icon
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

    // Clear existing pagination
    paginationContainer.find('li:not([data-list-pagination])').remove();

    // Previous button
    const prevBtn = paginationContainer.find('[data-list-pagination="prev"]');
    if (currentPage === 1 || totalPages === 0) {
        prevBtn.addClass('disabled');
    } else {
        prevBtn.removeClass('disabled');
    }

    if (totalPages > 0) {
        // Generate page numbers
        const maxVisiblePages = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2));
        let endPage = Math.min(totalPages, startPage + maxVisiblePages - 1);

        if (endPage - startPage < maxVisiblePages - 1) {
            startPage = Math.max(1, endPage - maxVisiblePages + 1);
        }

        // Add first page if not visible
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

        // Page numbers
        for (let i = startPage; i <= endPage; i++) {
            const pageItem = `
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="goToPage(${i}); return false;">${i}</a>
                </li>
            `;
            paginationContainer.find('[data-list-pagination="next"]').before(pageItem);
        }

        // Add last page if not visible
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

    // Next button
    const nextBtn = paginationContainer.find('[data-list-pagination="next"]');
    if (currentPage === totalPages || totalPages === 0) {
        nextBtn.addClass('disabled');
    } else {
        nextBtn.removeClass('disabled');
    }

    // Add click handlers for prev/next
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
    loadPriceQuotations();
    // Scroll to top of table
    $('html, body').animate({
        scrollTop: $("#PurchaseWaitingList").offset().top - 100
    }, 300);
}

function showError(message) {
    $('#errorContainer').html(`
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <strong>Error!</strong> ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `).show();

    // Auto hide after 5 seconds
    setTimeout(function () {
        $('#errorContainer').fadeOut();
    }, 5000);
}

function exportPDF(id) {
    console.log('Export PDF for ID:', id);
    // Add your PDF export logic here
    window.open(`/PriceQuotationList/ExportPDF/${id}`, '_blank');
}

function viewDetails(id) {
    window.location.href = `/PriceQuotationDetails/index/${id}`;
}

function editQuotation(id) {
    window.location.href = `/PriceQuotationDetails/Edit/${id}`;
}