let currentPage = 1;
const pageSize = 5;

function loadJobs(pageNumber = 1) {
    currentPage = pageNumber;

    $.ajax({
        url: '/JobLists/GetJobList',
        type: 'GET',
        data: {
            pageNumber: pageNumber,
            pageSize: pageSize,
            searchTerm: $('.search-input.search').val() || '',  // Fixed selector
            sortColumn: 'CreateJobID',
            sortOrder: 'asc'
        },
        beforeSend: function () {
            $('#leal-tables-body').html('<tr><td colspan="9" class="text-center">Loading...</td></tr>');
            $('#pageNumber').empty(); // Clear pagination while loading
        },
        success: function (response) {
            const jobs = response?.data || [];
            const totalItems = response?.totalItem || 0;
            const totalPages = Math.ceil(totalItems / pageSize);

            // Update title
            $("#card-title").text(`${totalItems} Job${totalItems !== 1 ? 's' : ''}`);

            // Render table rows
            if (jobs.length === 0) {
                $('#leal-tables-body').html('<tr><td colspan="9" class="text-center text-muted py-5">No jobs found</td></tr>');
                $('#pageNumber').empty();
                return;
            }

            let rows = '';
            $.each(jobs, function (i, job) {
                rows += `
                        <tr class="hover-actions-trigger position-relative">
                            <td class="ps-4 text-body fw-medium">${i + 1 + (currentPage - 1) * pageSize}</td>
                            <td class="ps-0">
                                <a href="/JobDetails/Index/${job.jobID}" class="text-primary fw-bold text-decoration-none">
                                    ${job.jobTitle || 'Untitled Job'}
                                </a>
                                <div class="text-muted small">${job.note || 'No notes'}</div>
                            </td>
                            <td class="fw-medium">${job.customerName || '-'}</td>
                            <td><span class="badge badge-phoenix badge-phoenix-info fs-10">${job.jobType || 'General'}</span></td>
                            <td>${job.startDate || '-'}</td>
                            <td>${job.endDate || '-'}</td>
                            <td>
                                <span class="badge job-status-badge ${job.statusName?.toLowerCase().includes('completed') ? 'badge-phoenix-success' :
                                                job.statusName?.toLowerCase().includes('in progress') ? 'badge-phoenix-primary' :
                                                    job.statusName?.toLowerCase().includes('pending') ? 'badge-phoenix-warning' :
                                                        job.statusName?.toLowerCase().includes('cancelled') ? 'badge-phoenix-danger' :
                                                            'badge-phoenix-secondary'
                                            }">
                                    ${job.statusName || 'Unknown'}
                                </span>
                            </td>
                            <td class="text-muted">${job.jobLocation || '-'}</td>
                            <td class="text-end pe-4">
                                <button class="btn btn-sm btn-outline-primary me-1" onclick="viewJob(${job.jobID})">
                                    <i class="fas fa-eye"></i>
                                </button>
                                <button class="btn btn-sm btn-outline-secondary dropdown-toggle dropdown-caret-none" type="button" data-bs-toggle="dropdown">
                                    <i class="fas fa-ellipsis-h"></i>
                                </button>
                                <div class="dropdown-menu dropdown-menu-end py-2">
                                    <a class="dropdown-item" href="/JobDetails/Index/${job.jobID}">View Details</a>
                                    <a class="dropdown-item" href="#!">Edit</a>
                                    <div class="dropdown-divider"></div>
                                    <a class="dropdown-item text-danger" href="#!">Delete</a>
                                </div>
                            </td>
                        </tr>`;
            });
            $('#leal-tables-body').html(rows);

            // Render Pagination
            renderPagination(totalPages, currentPage);
        },
        error: function () {
            $('#leal-tables-body').html('<tr><td colspan="9" class="text-center text-danger">Error loading data</td></tr>');
            $('#pageNumber').empty();
        }
    });
}

function renderPagination(totalPages, currentPage) {
    if (totalPages <= 1) {
        $('#pageNumber').empty();
        return;
    }

    let paginationHtml = '';

    // Previous button
    paginationHtml += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a>
    </li>`;

    // Page numbers (show max 5 pages, with ellipsis if needed)
    let startPage = Math.max(1, currentPage - 2);
    let endPage = Math.min(totalPages, currentPage + 2);

    if (startPage > 1) {
        paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
        if (startPage > 2) paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
    }

    for (let i = startPage; i <= endPage; i++) {
        paginationHtml += `<li class="page-item ${i === currentPage ? 'active' : ''}">
            <a class="page-link" href="#" data-page="${i}">${i}</a>
        </li>`;
    }

    if (endPage < totalPages) {
        if (endPage < totalPages - 1) paginationHtml += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
        paginationHtml += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
    }

    // Next button
    paginationHtml += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
        <a class="page-link" href="#" data-page="${currentPage + 1}">Next</a>
    </li>`;

    $('#pageNumber').html(paginationHtml);
}

// Delegate click event for pagination links
$(document).on('click', '#pageNumber .page-link', function (e) {
    e.preventDefault();
    const page = parseInt($(this).data('page'));
    if (page && page !== currentPage) {
        loadJobs(page);
    }
});

// Search input - trigger on Enter or button (optional debounce)
$('.search-input.search').on('keypress', function (e) {
    if (e.which === 13) { // Enter key
        loadJobs(1);
    }
});

// Optional: Add a search button if you want
// Or use input event with debounce for live search

$(function () {
    loadJobs(1);
});