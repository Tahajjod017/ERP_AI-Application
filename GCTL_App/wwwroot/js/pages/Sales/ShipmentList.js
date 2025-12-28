let currentPage = 1;
let pageSize = 10;
let searchTerm = '';
let totalPages = 0;
let totalRecords = 0;
let sortColumn = 'CreatedAt';
let sortDirection = 'desc';

$(document).ready(function () {
    loadShipments();

    $('#pageSizeSelect').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        loadShipments();
    });

    let searchTimeout;
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            searchTerm = $('#searchInput').val();
            currentPage = 1;
            loadShipments();
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
        loadShipments();
    });
});

function loadShipments() {
    $.ajax({
        url: '/ChallanList/GetShipmentList',
        type: 'GET',
        data: {
            page: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortDirection: sortDirection
        },
        beforeSend: function () {
            $('#shipment-table').html('<tr><td colspan="10" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div></td></tr>');
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
            showError('Failed to load shipments: ' + error);
        }
    });
}

function renderTable(data) {
    const tbody = $('#shipment-table');
    tbody.empty();

    if (data.length === 0) {
        tbody.append('<tr><td colspan="10" class="text-center">No data found</td></tr>');
    } else {
        data.forEach(item => {
            const statusBadge = getStatusBadge(item.status);
            const deliveryDate = item.actualDeliveryDate
                ? new Date(item.actualDeliveryDate).toLocaleDateString()
                : (item.expectedDeliveryDate ? new Date(item.expectedDeliveryDate).toLocaleDateString() : '-');

            const row = `
                <tr>
                    <td class="align-middle">${item.shipmentID}</td>
                    <td class="align-middle">${item.shipmentNumber || '-'}</td>
                    <td class="align-middle">
                        <div class="fs-9">${item.sourceType || '-'}</div>
                        <div class="fs-10 text-muted">${item.sourceNumber || ''}</div>
                    </td>
                    <td class="align-middle">${item.shipmentDate ? new Date(item.shipmentDate).toLocaleDateString() : '-'}</td>
                    <td class="align-middle">${deliveryDate}</td>
                    <td class="align-middle">
                        ${item.trackingNumber ? `<span class="badge badge-phoenix-info fs-10">${item.trackingNumber}</span>` : '-'}
                    </td>
                    <td class="align-middle text-end">${(item.shippingCost || 0).toFixed(2)}</td>
                    <td class="align-middle text-center">${item.totalItems}</td>
                    <td class="align-middle">${statusBadge}</td>
                    <td class="align-middle text-end">
                        <button class="btn btn-sm btn-phoenix-primary" onclick="viewDetails(${item.shipmentID})" title="View Details">
                            <i class="fas fa-eye"></i>
                        </button>
                        <button class="btn btn-sm btn-phoenix-info" onclick="editShipment(${item.shipmentID})" title="Edit">
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
        'Pending': 'badge-phoenix-warning',
        'Packed': 'badge-phoenix-info',
        'Shipped': 'badge-phoenix-primary',
        'InTransit': 'badge-phoenix-primary',
        'Delivered': 'badge-phoenix-success',
        'Cancelled': 'badge-phoenix-danger'
    };

    const statusLabel = {
        'InTransit': 'In Transit'
    };

    const badgeClass = statusMap[status] || 'badge-phoenix-secondary';
    const label = statusLabel[status] || status;

    return `<span class="badge ${badgeClass} fs-9">${label}</span>`;
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
    loadShipments();
    $('html, body').animate({
        scrollTop: $("#ShipmentList").offset().top - 100
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
    window.location.href = `/ChallanDetails/index/${id}`;
}

function editShipment(id) {
    window.location.href = `/ChallanDetails/Edit/${id}`;
}
