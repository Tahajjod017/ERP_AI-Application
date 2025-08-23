$(document).ready(function () {

    // Initialize table data settings
    $('#terminatePending').data('page', 1).data('size', 10).data('search', '').data('sort', '').data('dir', 'asc');
    $('#terminateProcessed').data('page', 1).data('size', 10).data('search', '').data('sort', '').data('dir', 'asc');

    // Load initial data
    loadPendingTable();
    loadProcessedTable();

    // Search handlers
    $('#pendingSearch').on('input', function () {
        $('#terminatePending').data('search', $(this).val()).data('page', 1);
        loadPendingTable();
    });

    $('#processedSearch').on('input', function () {
        $('#terminateProcessed').data('search', $(this).val()).data('page', 1);
        loadProcessedTable();
    });

    // Filter change handlers
    $('#timepicker2, #processedDepartment, #processedDesignation, #processedTerminationType').on('change', function () {
        $('#terminatePending').data('page', 1);
        loadPendingTable();
    });

    $('#approveDateRange, #approveDepartment, #approveDesignation, #approveTerminationType').on('change', function () {
        $('#terminateProcessed').data('page', 1);
        loadProcessedTable();
    });

    // Modal backdrop fix
    $('#termination_approval_modal').on('hidden.bs.modal', function () {
        $('.modal-backdrop').remove();
        $('body').removeClass('modal-open');
    });

    // Sort handlers
    $('.sort').on('click', function () {
        var tableId = $(this).closest('table').attr('id');
        var column = $(this).data('sort');
        var currentSort = $('#' + tableId).data('sort');
        var direction = (currentSort === column && $('#' + tableId).data('dir') === 'asc') ? 'desc' : 'asc';

        $('#' + tableId).data('sort', column).data('dir', direction).data('page', 1);
        if (tableId === 'terminatePending') {
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
            if (tableId === 'terminatePending') {
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
            if (tableId === 'terminatePending') {
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
        if (tableId === 'terminatePending') {
            loadPendingTable();
        } else {
            loadProcessedTable();
        }
    });

    // Modal click handler
    $(document).on('click', '[data-bs-target="#termination_approval_modal"]', function (e) {
        e.preventDefault();
        loadTerminationDetails($(this).data('termination-id'));
    });

    // Action handlers
    var currentAction = '';
    var currentTerminationId = '';

    $('#termination_approval_modal').on('click', '[data-action]', function () {
        if ($(this).data('action') === 'hold') {
            $('#termination_approval_modal').modal('hide');
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('#final_settlement_modal').modal('show');
        } else {
            currentAction = $(this).data('action');
            currentTerminationId = $(this).data('termination-id');
            $('#confirmation_action').text(currentAction.charAt(0).toUpperCase() + currentAction.slice(1));
            $('#confirmation_modal').modal('show');
        }
    });

    // Confirm action handler
    $('#confirm_action').on('click', function () {
        $('#confirmation_modal').modal('hide');
        processTermination(currentTerminationId, currentAction);
    });

    //#region Table pending terminations
    function loadPendingTable() {
        var page = $('#terminatePending').data('page');
        var size = $('#pageSizeSelectPending').val();
        var search = $('#terminatePending').data('search');
        var sort = $('#terminatePending').data('sort');
        var dir = $('#terminatePending').data('dir');
        var dateRange = $('#timepicker2').val();
        var department = $('#processedDepartment').val();
        var designation = $('#processedDesignation').val();
        var terminationType = $('#processedTerminationType').val();

        $.ajax({
            url: '/EmployeeTerminationApproval/GetPendingTerminations',
            type: 'GET',
            data: {
                dateRange: dateRange,
                department: department,
                designation: designation,
                terminationType: terminationType,
                pageNumber: page,
                pageSize: size,
                searchTerm: search,
                sortColumn: sort,
                sortDirection: dir
            },
            success: function (data) {
                var tbody = $('#pending-termination-body');
                tbody.empty();
                $.each(data.terminations, function (index, item) {
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-1">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" data-column="0">
                               <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${item.profileImage || '/images/default-avatar.png'}"
                                             alt="${item.employeeName}" onerror="this.src='/images/default-avatar.png'" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold mb-0">${item.employeeName}</h6>
                                        <span class="fs-12 fw-normal text-muted">#${item.employeeCode || 'N/A'}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1">${item.department}</td>
                            <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.position}</td>
                            <td class="terminationType align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.terminationType}</td>
                            <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.reason}</td>
                            <td class="noticeDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="5">${item.noticeDate}</td>
                            <td class="terminationDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="6">${item.terminationDate}</td>
                            <td class="decision align-middle white-space-nowrap pe-0 ps-4" data-column="7">
                                <div class="btn-reveal-trigger position-static">
                                    <div class="me-1" data-bs-toggle="modal" data-bs-target="#termination_approval_modal" data-termination-id="${item.id}">
                                        <i class="fas fa-eye me-1"></i>Review
                                    </div>
                                </div>
                            </td>
                        </tr>
                    `);
                });

                // Apply column visibility
                DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('terminatePending'), 'terminatePending');

                updatePaginationPending(data.totalCount, page, size);
                $('#terminatePending').data('total', data.totalCount);
            },
            error: function () {
                console.error('Error loading pending terminations');
            }
        });
    }
    //#endregion

    //#region table processed terminations
    function loadProcessedTable() {
        var page = $('#terminateProcessed').data('page');
        var size = $('#pageSizeSelectApprove').val();
        var search = $('#terminateProcessed').data('search');
        var sort = $('#terminateProcessed').data('sort');
        var dir = $('#terminateProcessed').data('dir');
        var dateRange = $('#approveDateRange').val();
        var department = $('#approveDepartment').val();
        var designation = $('#approveDesignation').val();
        var terminationType = $('#approveTerminationType').val();

        $.ajax({
            url: '/EmployeeTerminationApproval/GetProcessedTerminations',
            type: 'GET',
            data: {
                dateRange: dateRange,
                department: department,
                designation: designation,
                terminationType: terminationType,
                pageNumber: page,
                pageSize: size,
                searchTerm: search,
                sortColumn: sort,
                sortDirection: dir
            },
            success: function (data) {
                showDev(data, 'Approve Table');

                var tbody = $('#processed-termination-body');
                tbody.empty();
                $.each(data.terminations, function (index, item) {
                    var statusBadge = item.status === 'Approved' ? 'badge-phoenix-success' : 'badge-phoenix-danger';
                    tbody.append(`
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-1">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" data-column="0">
                                 <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${item.profileImage || '/images/default-avatar.png'}"
                                             alt="${item.employeeName}" onerror="this.src='/images/default-avatar.png'" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold mb-0">${item.employeeName}</h6>
                                        <span class="fs-12 fw-normal text-muted">#${item.employeeCode || 'N/A'}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1">${item.department}</td>
                            <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.position}</td>
                            <td class="terminationType align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.terminationType}</td>
                            <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.reason}</td>
                            <td class="processedDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="5">${item.processedDate}</td>
                            <td class="terminationDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="6">${item.terminationDate}</td>
                            <td class="status align-middle white-space-nowrap pe-0 ps-2" data-column="7">
                                <span class="badge badge-phoenix ${statusBadge} fs-9">${item.status}</span>
                            </td>
                        </tr>
                    `);
                });

                DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('terminateProcessed'), 'terminateProcessed');

                updatePaginationApprove(data.totalCount, page, size);
                $('#terminateProcessed').data('total', data.totalCount);
            },
            error: function () {
                console.error('Error loading processed terminations');
            }
        });
    }
    //#endregion

    //#region pagination
    function updatePaginationPending(totalCount, page, size) {
        var totalPages = Math.ceil(totalCount / size);
        var pagination = $('#paginationPending');
        pagination.empty();

        for (var i = 1; i <= totalPages; i++) {
            var activeClass = i === page ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><button class="page-link">${i}</button></li>`);
        }

        $('#totalPending').text(`Showing ${(page - 1) * size + 1} to ${Math.min(page * size, totalCount)} of ${totalCount} entries`);
    }

    function updatePaginationApprove(totalCount, page, size) {
        var totalPages = Math.ceil(totalCount / size);
        var pagination = $('#paginationApprove');
        pagination.empty();

        for (var i = 1; i <= totalPages; i++) {
            var activeClass = i === page ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><button class="page-link">${i}</button></li>`);
        }

        $('#totalApprove').text(`Showing ${(page - 1) * size + 1} to ${Math.min(page * size, totalCount)} of ${totalCount} entries`);
    }
    //#endregion

    //#region get by id for modal
    function loadTerminationDetails(id) {
        $.ajax({
            url: '/EmployeeTerminationApproval/GetTerminationDetails',
            type: 'GET',
            data: { id: id },
            success: function (data) {
                $('#modalDepartment').val(data.department);
                $('#modalPosition').val(data.position);
                $('#modalEmployeeId').val(data.employeeName);
                $('#modalTerminationType').val(data.terminationType);
                $('#modalNoticeDate').val(data.noticeDate);
                $('#modalYearsOfService').val(data.yearsOfService);
                $('#modalTerminationDate').val(data.terminationDate);
                $('#modalCurrentSalary').val(data.currentSalary);
                $('#modalPendingDues').val(data.pendingDues);
                $('#modalTerminationReason').val(data.reason);
                $('#modalHRComments').val('');

                $('#termination_approval_modal [data-action]').data('termination-id', id);
                $('#termination_approval_modal').modal('show');
            },
            error: function () {
                console.error('Error loading termination details');
            }
        });
    }
    //#endregion

    //#region Approve or decline button 
    function processTermination(id, action) {
        var data = {
            id: id,
            action: action,
            hrComments: $('#modalHRComments').val(),
            handoverStatus: 'pending',
            assetReturned: false,
            clearanceCompleted: false,
            documentsPrepared: false
        };
        showDev(data, 'data sent');

        $.ajax({
            url: '/EmployeeTerminationApproval/ProcessTermination',
            type: 'POST',
            data: data,
            success: function (response) {
                if (response.success) {
                    $('#termination_approval_modal').modal('hide');

                    $('.modal-backdrop').remove();
                    $('body').removeClass('modal-open');

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
                console.error('Error processing termination');
            }
        });
    }
    //#endregion

});