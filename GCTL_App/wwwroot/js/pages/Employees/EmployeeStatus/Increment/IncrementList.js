// Global variables
let currentPage = 1;
let currentSortColumn = "effectiveDate";
let currentSortDirection = "desc";
let columnVisibility = {};

// Document ready - only initialization
$(document).ready(function () {
    initializeColumnVisibility();
    loadIncrementList(1);

    showDev('hi there');
    
    $('#searchInput').on('input', debounce(function () {
        loadIncrementList(1);
    }, 500));

    $('#departmentFilter, #incrementTypeFilter, #dateRangePicker, #pageSizeSelect').on('change', function () {
        loadIncrementList(1);
    });

    $(document).on('click', '[data-list-pagination="prev"]', function (e) {
        e.preventDefault();
        if (currentPage > 1) {
            loadIncrementList(currentPage - 1);
        }
    });

    $(document).on('click', '[data-list-pagination="next"]', function (e) {
        e.preventDefault();
        if (currentPage < totalPages) {
            loadIncrementList(currentPage + 1);
        }
    });

    $(document).on('click', 'th.sort', function () {
        const clickedColumn = $(this).data('sort');

        $('.sort-icon').remove();

        if (currentSortColumn === clickedColumn) {
            currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = clickedColumn;
            currentSortDirection = 'asc';
        }

        const icon = currentSortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
        $(this).append(`<i class="fas ${icon} sort-icon ms-1"></i>`);

        loadIncrementList(1);
    });
});

//#region Column Visibility Management
function initializeColumnVisibility() {
    const columnToggles = document.querySelectorAll('.column-toggle');

    columnToggles.forEach(toggle => {
        columnVisibility[toggle.dataset.column] = toggle.checked;
    });

    loadColumnVisibility();

    columnToggles.forEach(toggle => {
        toggle.addEventListener('change', function () {
            const columnIndex = this.dataset.column;
            const isVisible = this.checked;
            toggleColumn(columnIndex, isVisible);
        });
    });

    $('#selectAllColumns').on('click', function () {
        columnToggles.forEach(toggle => {
            toggle.checked = true;
            const columnIndex = toggle.dataset.column;
            toggleColumn(columnIndex, true);
        });
    });

    $('#resetColumns').on('click', function () {
        columnToggles.forEach(toggle => {
            toggle.checked = true;
            const columnIndex = toggle.dataset.column;
            toggleColumn(columnIndex, true);
        });
        localStorage.removeItem('incrementList_columnVisibility');
    });

    $('.column-toggle-dropdown').on('click', function (e) {
        e.stopPropagation();
    });
}

function toggleColumn(columnIndex, isVisible) {
    const headers = $(`th[data-column="${columnIndex}"]`);
    const cells = $(`td[data-column="${columnIndex}"]`);
    console.log(`Toggling column ${columnIndex}: headers found=${headers.length}, cells found=${cells.length}`);
    if (isVisible) {
        headers.show();
        cells.show();
    } else {
        headers.hide();
        cells.hide();
    }
    columnVisibility[columnIndex] = isVisible;
    localStorage.setItem('incrementList_columnVisibility', JSON.stringify(columnVisibility));
}



function loadColumnVisibility() {
    try {
        const saved = localStorage.getItem('incrementList_columnVisibility');
        if (saved) {
            const savedVisibility = JSON.parse(saved);
            console.log('Loaded column visibility:', savedVisibility);
            const columnToggles = document.querySelectorAll('.column-toggle');
            columnToggles.forEach(toggle => {
                const columnIndex = toggle.dataset.column;
                if (savedVisibility.hasOwnProperty(columnIndex)) {
                    toggle.checked = savedVisibility[columnIndex];
                    toggleColumn(columnIndex, savedVisibility[columnIndex]);
                }
            });
        }
    } catch (e) {
        console.error('Failed to load column visibility from localStorage:', e);
    }
}



function applyColumnVisibilityToNewRows() {
    Object.keys(columnVisibility).forEach(columnIndex => {
        if (!columnVisibility[columnIndex]) {
            $(`td[data-column="${columnIndex}"]`).hide();
        }
    });
}
//#endregion

//#region Table Load 
function loadIncrementList(page = 1) {
    currentPage = page;

    const formData = new FormData();
    formData.append("searchTerm", $('#searchInput').val());
    formData.append("departmentId", $('#departmentFilter').val());
    formData.append("incrementType", $('#incrementTypeFilter').val());
    formData.append("dateRange", $('#dateRangePicker').val());
    formData.append("pageSize", $('#pageSizeSelect').val());
    formData.append("pageNumber", page);
    formData.append("sortColumn", currentSortColumn);
    formData.append("sortDirection", currentSortDirection);

    showLoadingIndicator();

    $.ajax({
        url: '/IncrementList/GetIncrementList',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            hideLoadingIndicator();
            if (res.success) {
                populateTable(res.data.items);
                updatePagination(res.data.pagination);
                updateResultCount(res.data.pagination);
                applyColumnVisibilityToNewRows();
            } else {
                $('#incrementList-body').html('<tr><td colspan="8">No data found.</td></tr>');
            }
        },
        error: function () {
            hideLoadingIndicator();
            if (typeof toastr !== 'undefined') {
                toastr.error("Failed to load increment list.");
            } else {
                alert("Failed to load increment list.");
            }
        }
    });
}

function populateTable(items) {
    const tbody = $('#incrementList-body');
    tbody.empty();
    if (!items || items.length === 0) {
        tbody.append('<tr><td colspan="8" class="text-center py-4">No data found.</td></tr>');
        return;
    }
    items.forEach(item => {
        const incrementAmount = item.incrementAmount || 0;
        const incrementSign = incrementAmount >= 0 ? '+' : '';
        const incrementClass = incrementAmount >= 0 ? 'text-success' : 'text-danger';
        tbody.append(`
            <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" data-column="0">
                    <div class="d-flex align-items-center file-name-icon">
                        <div class="avatar avatar-m avatar-bordered me-4">
                            <img class="rounded-circle" src="${item.avatarUrl || '/images/default-avatar.png'}"
                                 alt="${item.employeeName}" onerror="this.src='/images/default-avatar.png'" />
                        </div>
                        <div class="ms-1">
                            <h6 class="fw-bold mb-0">${item.employeeName}</h6>
                            <span class="fs-12 fw-normal text-muted">#${item.id || 'N/A'}</span>
                        </div>
                    </div>
                </td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="1">${item.department || 'N/A'}</td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="2">${formatCurrency(item.currentSalary)}</td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0 ${incrementClass}" data-column="3">${incrementSign}${formatCurrency(Math.abs(incrementAmount))}</td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="4">${formatCurrency(item.newSalary)}</td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="5">${formatDate(item.effectiveDate)}</td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="6">
                    <span class="badge ${getIncrementTypeBadgeClass(item.incrementType)}">${item.incrementType}</span>
                </td>
                <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="7">
                    <span class="badge ${getStatusBadgeClass(item.status)}">${item.status}</span>
                        <div class="universal-tooltip-container position-relative d-inline-block">
                                <i class="fa-solid fa-circle-info universal-tooltip-trigger"
                                   data-tooltip-url="/EmployeeResign/GetDetails"
                                   data-tooltip-id="${item.id}"
                                   data-tooltip-data-key="id"
                                   data-tooltip-template="timelineApprovalTemplate"
                                   style="cursor: pointer; font-size: 12px; color: #007bff; margin-left: 5px;"></i>
                          </div>
                </td>
            </tr>
        `);
    });
}


function timelineApprovalTemplate(data) {
    const steps = Array.isArray(data) ? data : [data];
    let html = '';
    

    if (steps.length > 0) {
        steps.forEach((item, index) => {
            const approverStep = item.approverStep ?? '';
            const statusName = item.statusName ?? '';
            const author = item.approvarPerson ?? '';
            const statusDescription = item.approvarNote ?? '';
            const approvedOrDeclineDate = item.approvedOrDeclineDate ?? '';

            // Determine status color
            let statusColor = 'text-body-secondary';
            if (statusName === 'APPROVED') statusColor = 'text-success';
            if (statusName === 'DECLINED') statusColor = 'text-danger';
            if (statusName === 'PENDING') statusColor = 'text-warning';

            html += `

            <style>
                .timeline-item-bar {
                    position: relative;
                }

                .timeline-bar {
                    position: absolute;
                    top: 100%;
                    left: 50%;
                    transform: translateX(-50%);
                    height: 20px;
                    border-right: 2px dashed #dee2e6 !important;
                }

                .timeline-item:last-child .timeline-bar {
                    display: none;
                }

                .icon-item-sm {
                    width: 24px;
                    height: 24px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                }

                .bg-primary-subtle {
                    background-color: #e8f4ff !important;
                }

                .text-primary-dark {
                    color: #0d6efd !important;
                }
            </style>

                <div class="timeline-item" style="margin-bottom:1px">
                    <div class="timeline-item position-relative">
                        <div class="row g-md-3">
                            <div class="col-12 col-md-auto d-flex">
                                <!--<div class="timeline-item-date order-1 order-md-0 me-md-4">
                                    <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                        ${approverStep} 
                                    </p>
                                </div> -->
                                <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                    <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                        <span class="fa-solid far fa-file-alt text-primary-dark fs-10"></span>
                                    </div>
                                    <span class="timeline-bar border-end border-dashed"></span>
                                </div>
                            </div>
                            <div class="col">
                                <div class="timeline-item-content ps-6 ps-md-3">
                                    <h5 class="fs-9 lh-sm ${statusColor}">${statusName}</h5>
                                    <p class="fs-9 mb-0">by <a class="fw-semibold" href="#!">${author}</a></p>
                                    <h5 class="fs-9 lh-sm">${approvedOrDeclineDate}</h5>
                                    <p class="fs-9 text-body-secondary">${statusDescription}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`;
        });
    } else {
        html = '<div class="text-muted" style="color: #999;">No approval steps found</div>';
    }

    return html;
}


    UniversalTooltipService.registerTemplate('timelineApprovalTemplate', timelineApprovalTemplate);


function updatePagination(pagination) {
    const paginationUl = $('.pagination');
    paginationUl.empty();

    $('[data-list-pagination="prev"]').prop('disabled', pagination.currentPage === 1);

    const maxVisiblePages = 5;
    let startPage = Math.max(1, pagination.currentPage - Math.floor(maxVisiblePages / 2));
    let endPage = Math.min(pagination.totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage < maxVisiblePages - 1) {
        startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    if (startPage > 1) {
        paginationUl.append(`<li class="page-item"><a class="page-link" href="#" onclick="loadIncrementList(1)">1</a></li>`);
        if (startPage > 2) {
            paginationUl.append(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
        }
    }

    for (let i = startPage; i <= endPage; i++) {
        paginationUl.append(`
            <li class="page-item ${i === pagination.currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadIncrementList(${i})">${i}</a>
            </li>
        `);
    }

    if (endPage < pagination.totalPages) {
        if (endPage < pagination.totalPages - 1) {
            paginationUl.append(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
        }
        paginationUl.append(`<li class="page-item"><a class="page-link" href="#" onclick="loadIncrementList(${pagination.totalPages})">${pagination.totalPages}</a></li>`);
    }

    $('[data-list-pagination="next"]').prop('disabled', pagination.currentPage === pagination.totalPages);
}

function updateResultCount(pagination) {
    const infoText = `Showing ${pagination.startRecord} - ${pagination.endRecord} of ${pagination.totalRecords} results`;
    $('[data-list-info]').text(infoText);
}
//#endregion

//#region Utility Functions
function formatCurrency(amount) {
    if (!amount && amount !== 0) return 'N/A';
    return '৳' + parseFloat(amount).toLocaleString('en-BD', {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    });
}

function formatDate(dateString) {
    return dateString || 'N/A';
}

function getStatusBadgeClass(status) {
    switch (status?.toLowerCase()) {
        case 'active':
        case 'approved': return 'badge badge-phoenix badge-phoenix-success';
        case 'pending': return 'badge badge-phoenix badge-phoenix-warning text-dark';
        case 'rejected':
        case 'cancelled': return 'badge badge-phoenix badge-phoenix-danger';
        case 'draft': return 'badge badge-phoenix badge-phoenix-info';
        default: return 'badge badge-phoenix badge-phoenix-secondary';
    }
}

function getIncrementTypeBadgeClass(type) {
    switch (type?.toLowerCase()) {
        case 'increment': return 'badge badge-phoenix badge-phoenix-success';
        case 'decrement': return 'badge badge-phoenix badge-phoenix-danger';
        case 'promotion': return 'badge badge-phoenix badge-phoenix-primary';
        case 'bonus': return 'bg-warning text-dark';
        default: return 'bg-secondary';
    }
}

function showLoadingIndicator() {
    $('#incrementList-body').html(`
        <tr>
            <td colspan="8" class="text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <div class="mt-2">Loading increment data...</div>
            </td>
        </tr>
    `);
}

function hideLoadingIndicator() {
    // Loading indicator will be replaced by actual data
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}
//#endregion



 