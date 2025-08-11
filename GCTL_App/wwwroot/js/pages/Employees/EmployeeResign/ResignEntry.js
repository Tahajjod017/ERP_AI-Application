$(document).ready(function () {
    showDev("EmployeeResignEntry.js");

    let currentPage = 1;
    let pageSize = 10;
    let sortColumn = 'rEmpName';
    let sortDirection = 'asc';
    let fromDate = '';
    let toDate = '';

    // Initialize flatpickr for date range picker
    $("#timepicker2").flatpickr({
        mode: "range",
        dateFormat: "d/m/y",
        disableMobile: true,
        onChange: function (selectedDates) {
            if (selectedDates.length === 2) {
                fromDate = selectedDates[0].toLocaleDateString('en-GB').replace(/\//g, '/');
                toDate = selectedDates[1].toLocaleDateString('en-GB').replace(/\//g, '/');
                currentPage = 1; // Reset to first page on filter change
                loadTableData();
            }
        }
    });

    // Function to fetch and render table data
    function loadTableData() {
        $.ajax({
            url: '/EmployeeResign/GetResignations',
            type: 'GET',
            data: {
                page: currentPage,
                pageSize: pageSize,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (response) {
                const data = response.data;
                const totalRecords = response.recordsTotal;
                const tbody = $('#purchasers-sellers-body');
                tbody.empty();

                // Render table rows
                data.forEach(item => {
                    const row = `
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" data-bulk-select-row='${JSON.stringify(item)}' />
                                </div>
                            </td>
                            <td class="rEmpName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-0" data-column="0">
                                <div class="d-flex align-items-center position-relative">
                                    <a href="" class="avatar avatar-md me-2">
                                        <img src="https://placehold.co/400" class="rounded-circle" alt="user">
                                    </a>
                                    <a class="text-body-highlight fw-bold stretched-link" href="#!">${item.rEmpName}</a>
                                </div>
                            </td>
                            <td class="rEmpDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="1">${item.rEmpDept}</td>
                            <td class="resignResons align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="2">${item.resignResons}</td>
                            <td class="resNoticeDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="3">${item.resNoticeDate}</td>
                            <td class="resinDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0" data-column="4">${item.resinDate}</td>
                            <td class="align-middle white-space-nowrap text-end pe-0 ps-4" data-column="5">
                                <div class="btn-reveal-trigger position-static">
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#edit_resignation">
                                        <i class="fas fa-edit text-success"></i>
                                    </a>
                                    <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                        <i class="fas fa-trash text-danger"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>`;
                    tbody.append(row);
                });

                // Update pagination info
                const start = (currentPage - 1) * pageSize + 1;
                const end = Math.min(currentPage * pageSize, totalRecords);
                $('[data-list-info]').text(`Showing ${start} to ${end} of ${totalRecords} entries`);

                // Render pagination
                renderPagination(totalRecords);
            },
            error: function () {
                toastr.error('Failed to load resignation data');
            }
        });
    }

    // Function to render pagination
    function renderPagination(totalRecords) {
        const totalPages = Math.ceil(totalRecords / pageSize);
        const pagination = $('.pagination');
        pagination.empty();

        // Previous button
        pagination.append(`
            <li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                <button class="page-link" data-list-pagination="prev"><span class="fas fa-chevron-left"></span></button>
            </li>
        `);

        // Page numbers
        for (let i = 1; i <= totalPages; i++) {
            pagination.append(`
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <button class="page-link" data-page="${i}">${i}</button>
                </li>
            `);
        }

        // Next button
        pagination.append(`
            <li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                <button class="page-link" data-list-pagination="next"><span class="fas fa-chevron-right"></span></button>
            </li>
        `);
    }

    // Handle page size change
    $('#pageSize').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1; // Reset to first page
        loadTableData();
    });

    // Handle pagination clicks
    $(document).on('click', '.page-link', function () {
        const paginationType = $(this).attr('data-list-pagination');
        const pageNumber = $(this).attr('data-page');
        const totalRecords = parseInt($('[data-list-info]').text().match(/of (\d+)/)[1]);
        const totalPages = Math.ceil(totalRecords / pageSize);

        if (paginationType === 'prev' && currentPage > 1) {
            currentPage--;
        } else if (paginationType === 'next' && currentPage < totalPages) {
            currentPage++;
        } else if (pageNumber) {
            currentPage = parseInt(pageNumber);
        }

        loadTableData();
    });

    // Handle column sorting
    $('#resignTBL th.sort').on('click', function () {
        const column = $(this).data('sort');
        if (sortColumn === column) {
            sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            sortColumn = column;
            sortDirection = 'asc';
        }
        loadTableData();
    });

    // Initial load
    loadTableData();

   
});