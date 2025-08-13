$(document).ready(function () {
    // Initialize tables with default page and size
    $('#resignPending').data('page', 1).data('size', 10).data('search', '').data('sort', '').data('dir', 'asc');
    $('#resignProcessed').data('page', 1).data('size', 10).data('search', '').data('sort', '').data('dir', 'asc');

    // Load tables on page load
    loadPendingTable();
    loadProcessedTable();

    // Search input handler
    $('#pendingSearch').on('input', function () {
        $('#resignPending').data('search', $(this).val()).data('page', 1);
        loadPendingTable();
    });

    $('#processedSearch').on('input', function () {
        $('#resignProcessed').data('search', $(this).val()).data('page', 1);
        loadProcessedTable();
    });

    // Filter change handlers
    $('#timepicker2, #processedDepartment, #processedDesignation').on('change', function () {
        $('#resignPending').data('page', 1);
        loadPendingTable();
    });

    $('#approveDateRange, #approveDepartment, #approveDesignation').on('change', function () {
        $('#resignProcessed').data('page', 1);
        loadProcessedTable();
    });

    // Sorting handler
    $('.sort').on('click', function () {
        var tableId = $(this).closest('table').attr('id');
        var column = $(this).data('sort');
        var currentSort = $('#' + tableId).data('sort');
        var direction = (currentSort === column && $('#' + tableId).data('dir') === 'asc') ? 'desc' : 'asc';

        $('#' + tableId).data('sort', column).data('dir', direction).data('page', 1);
        if (tableId === 'resignPending') {
            loadPendingTable();
        } else {
            loadProcessedTable();
        }
    });

    // Pagination handlers
    $('[data-list-pagination="prev"]').on('click', function () {
        var tableId = $(this).closest('.card').find('table').attr('id');
        var page = $('#' + tableId).data('page');
        if (page > 1) {
            $('#' + tableId).data('page', page - 1);
            if (tableId === 'resignPending') {
                loadPendingTable();
            } else {
                loadProcessedTable();
            }
        }
    });

    $('[data-list-pagination="next"]').on('click', function () {
        var tableId = $(this).closest('.card').find('table').attr('id');
        var page = $('#' + tableId).data('page');
        var totalPages = Math.ceil($('#' + tableId).data('total') / $('#' + tableId).data('size'));
        if (page < totalPages) {
            $('#' + tableId).data('page', page + 1);
            if (tableId === 'resignPending') {
                loadPendingTable();
            } else {
                loadProcessedTable();
            }
        }
    });

    $('.pagination').on('click', '.page-link', function () {
        var tableId = $(this).closest('.card').find('table').attr('id');
        var page = parseInt($(this).text());
        $('#' + tableId).data('page', page);
        if (tableId === 'resignPending') {
            loadPendingTable();
        } else {
            loadProcessedTable();
        }
    });

    // Modal click handler for reviewing resignations
    $(document).on('click', '[data-bs-target="#resignation_approval_modal"]', function (e) {
        e.preventDefault();
        loadResignationDetails($(this).data('resignation-id'));
    });

    // Modal action handler
    var currentAction = '';
    var currentResignationId = '';
    $('#resignation_approval_modal').on('click', '[data-action]', function () {
        currentAction = $(this).data('action');
        currentResignationId = $(this).data('resignation-id');
        $('#confirmation_action').text(currentAction.charAt(0).toUpperCase() + currentAction.slice(1));
        $('#confirmation_modal').modal('show');
    });

    // Confirm action handler
    $('#confirm_action').on('click', function () {
        $('#confirmation_modal').modal('hide');
        processResignation(currentResignationId, currentAction);
    });

    // Load pending resignations
    function loadPendingTable() {
        var page = $('#resignPending').data('page');
        var size = $('#resignPending').data('size');
        var search = $('#resignPending').data('search');
        var sort = $('#resignPending').data('sort');
        var dir = $('#resignPending').data('dir');
        var dateRange = $('#timepicker2').val();
        var department = $('#processedDepartment').val();
        var designation = $('#processedDesignation').val();

        $.ajax({
            url: '/EmployeeResignApproval/GetPendingResignations',
            type: 'GET',
            data: {
                dateRange: dateRange,
                department: department,
                designation: designation,
                pageNumber: page,
                pageSize: size,
                searchTerm: search,
                sortColumn: sort,
                sortDirection: dir
            },
            success: function (data) {
                var tbody = $('#pending-resignation-body');
                tbody.empty();
                $.each(data.resignations, function (index, item) {
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-1">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                                <div class="d-flex align-items-center position-relative">
                                    <a href="" class="avatar avatar-md me-2">
                                        <img src="${item.profileImage || 'https://placehold.co/400'}" class="rounded-circle" alt="user">
                                    </a>
                                    <a class="text-body-highlight fw-bold stretched-link" href="#!">${item.employeeName}</a>
                                </div>
                            </td>
                            <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.department}</td>
                            <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.position}</td>
                            <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.reason}</td>
                            <td class="noticeDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.noticeDate}</td>
                            <td class="lastWorkingDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.lastWorkingDay}</td>
                            <td class="decision align-middle white-space-nowrap text-end pe-0 ps-4">
                                <div class="btn-reveal-trigger position-static">
                                    <button class="btn btn-sm btn-outline-primary me-1" data-bs-toggle="modal" data-bs-target="#resignation_approval_modal" data-resignation-id="${item.id}">
                                        <i class="fas fa-eye me-1"></i>Review
                                    </button>
                                </div>
                            </td>
                        </tr>
                    `);
                });

                updatePagination('#resignPending', data.totalCount, page, size);
                $('#resignPending').data('total', data.totalCount);
            },
            error: function () {
                console.error('Error loading pending resignations');
            }
        });
    }

    // Load processed resignations
    function loadProcessedTable() {
        var page = $('#resignProcessed').data('page');
        var size = $('#resignProcessed').data('size');
        var search = $('#resignProcessed').data('search');
        var sort = $('#resignProcessed').data('sort');
        var dir = $('#resignProcessed').data('dir');
        var dateRange = $('#approveDateRange').val();
        var department = $('#approveDepartment').val();
        var designation = $('#approveDesignation').val();

        $.ajax({
            url: '/EmployeeResignApproval/GetProcessedResignations',
            type: 'GET',
            data: {
                dateRange: dateRange,
                department: department,
                designation: designation,
                pageNumber: page,
                pageSize: size,
                searchTerm: search,
                sortColumn: sort,
                sortDirection: dir
            },
            success: function (data) {
                var tbody = $('#processed-resignation-body');
                tbody.empty();
                $.each(data.resignations, function (index, item) {
                    var statusBadge = item.status === 'Approved' ? 'badge-phoenix-success' : 'badge-phoenix-danger';
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-1">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                                <div class="d-flex align-items-center position-relative">
                                    <a href="" class="avatar avatar-md me-2">
                                        <img src="${item.profileImage || 'https://placehold.co/400'}" class="rounded-circle" alt="user">
                                    </a>
                                    <a class="text-body-highlight fw-bold stretched-link" href="#!">${item.employeeName}</a>
                                </div>
                            </td>
                            <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.department}</td>
                            <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.position}</td>
                            <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.reason}</td>
                            <td class="processedDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.processedDate}</td>
                            <td class="lastWorkingDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.lastWorkingDay}</td>
                            <td class="status align-middle white-space-nowrap text-end pe-0 ps-4">
                                <span class="badge badge-phoenix ${statusBadge}">${item.status}</span>
                            </td>
                        </tr>
                    `);
                });

                updatePagination('#resignProcessed', data.totalCount, page, size);
                $('#resignProcessed').data('total', data.totalCount);
            },
            error: function () {
                console.error('Error loading processed resignations');
            }
        });
    }

    // Update pagination controls
    function updatePagination(tableId, totalCount, page, size) {
        var totalPages = Math.ceil(totalCount / size);
        var pagination = $(tableId + ' .pagination');
        pagination.empty();

        for (var i = 1; i <= totalPages; i++) {
            var activeClass = i === page ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><button class="page-link">${i}</button></li>`);
        }

        $(tableId + ' [data-list-info]').text(`Showing ${(page - 1) * size + 1} to ${Math.min(page * size, totalCount)} of ${totalCount} entries`);
    }
   
    // Load resignation details for modal
    function loadResignationDetails(id) {
        $.ajax({
            url: '/EmployeeResignApproval/GetResignationDetails',
            type: 'GET',
            data: { id: id },
            success: function (data) {
                $('#modalDepartment').val(data.department);
                $('#modalPosition').val(data.position);
                $('#modalEmployeeId').val(data.employeeId);
                $('#modalNoticeDate').val(data.noticeDate);
                $('#modalYearsOfService').val(data.yearsOfService);
                $('#modalLastWorkingDay').val(data.lastWorkingDay);
                $('#modalNoticePeriod').val(data.noticePeriod);
                $('#modalCurrentSalary').val(data.currentSalary);
                $('#modalPendingDues').val(data.pendingDues);
                $('#modalResignationReason').val(data.reason);
                $('#modalHRComments').val('');
                $('#modalHandoverStatus').val(data.handoverStatus || 'pending');
                $('#assetReturnCheck').prop('checked', data.assetReturned || false);
                $('#clearanceCheck').prop('checked', data.clearanceCompleted || false);
                $('#documentsCheck').prop('checked', data.documentsPrepared || false);

                $('#resignation_approval_modal [data-action]').data('resignation-id', id);
                $('#resignation_approval_modal').modal('show');
            },
            error: function () {
                console.error('Error loading resignation details');
            }
        });
    }

    // Process resignation action
    function processResignation(id, action) {
        var data = {
            id: id,
            action: action,
            hrComments: $('#modalHRComments').val(),
            handoverStatus: $('#modalHandoverStatus').val(),
            assetReturned: $('#assetReturnCheck').is(':checked'),
            clearanceCompleted: $('#clearanceCheck').is(':checked'),
            documentsPrepared: $('#documentsCheck').is(':checked')
        };

        $.ajax({
            url: '/EmployeeResignApproval/ProcessResignation',
            type: 'POST',
            data: data,
            success: function (response) {
                if (response.success) {
                    $('#resignation_approval_modal').modal('hide');
                    loadPendingTable();
                    loadProcessedTable();
                    if (action === 'approve') {
                        $('#final_settlement_modal').modal('show');
                    }
                } else {
                    alert(response.message);
                }
            },
            error: function () {
                console.error('Error processing resignation');
            }
        });
    }
});