$(document).ready(function () {
    showDev("EmployeeTerminationEntry.js", 'init');

    let currentPage = 1;
    let pageSize = 10;
    let sortColumn = 'tEmpName';
    let sortDirection = 'asc';
    let fromDate = '';
    let toDate = '';
    let currentEditingId = null;

    //#region On change

    $('#company').on('change', function () {
        const companyId = $(this).val();
        const url = $(this).data('url');

        if (companyId) {
            $.ajax({
                url: url,
                type: 'GET',
                data: { companyId: companyId },
                success: function (data) {
                    showDev(data, '1')
                    choiceManager.populateDropdown('employeeId', data)
                },
                error: function () {
                    alert('Failed to load employees.');
                }
            });
        } else {
            $('#employeeId').empty().append('<option value="">Select Employee</option>');
        }
    });

    $('#editCompany').on('change', function () {
        const companyId = $(this).val();
        const url = $(this).data('url');

        if (companyId) {
            $.ajax({
                url: url,
                type: 'GET',
                data: { companyId: companyId },
                success: function (data) {
                    showDev(data, '2')
                    choiceManager.populateDropdown('editEmployeeId', data)
                },
                error: function () {
                    alert('Failed to load employees.');
                }
            });
        } else {
            $('#employeeId').empty().append('<option value="">Select Employee</option>');
        }
    });

    //#endregion

    //#region Initialize flatpickr for date range picker

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

    //#endregion

    //#region Initialize date pickers for modal forms

    $("#noticeDate, #editNoticeDate").flatpickr({
        dateFormat: "d/m/Y",
        disableMobile: true
    });

    $("#terminationDate, #editTerminationDate").flatpickr({
        dateFormat: "d/m/Y",
        disableMobile: true
    });

    //#endregion

    //#region table data Function to fetch and render
    function loadTableData() {
        $.ajax({
            url: '/EmployeeTermination/GetTerminations',
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
                const tbody = $('#termination-body');
                tbody.empty();

                showDev(response, 'table data')
                // Render table rows
                data.forEach(item => {
                    const row = `
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-1">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" data-bulk-select-row='${JSON.stringify(item)}' />
                                </div>
                            </td>
                            <td class="tEmpName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" data-column="0">
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
                            <td class="tEmpDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1">${item.department || item.Department}</td>
                            <td class="terminationType align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.terminationType || item.TerminationType}</td>
                            <td class="terminationReason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.reason || item.Reason}</td>
                            <td class="termNoticeDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.noticeDate || item.NoticeDate}</td>
                            <td class="terminationDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="5">${item.terminationDate || item.TerminationDate}</td>
                            <td class="align-middle white-space-nowrap text-end pe-0 ps-4" data-column="6">
                                <div class="btn-reveal-trigger position-static">
                                    <a href="#" class="nav-item mx-2 edit-termination11" data-id="${item.id || item.Id}" data-bs-toggle="modal" data-bs-target="#edit_termination">
                                        <i class="fas fa-edit text-success"></i>
                                    </a>
                                    <a href="#" class="nav-item mx-2 delete-termination" data-id="${item.id || item.Id}" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                        <i class="fas fa-trash text-danger"></i>
                                    </a>
                                </div>
                            </td>
                        </tr>`;
                    tbody.append(row);
                });

                showDev("table Loaded");

                // Update pagination info
                const start = (currentPage - 1) * pageSize + 1;
                const end = Math.min(currentPage * pageSize, totalRecords);
                $('[data-list-info]').text(`Showing ${start} to ${end} of ${totalRecords} entries`);

                // Render pagination
                renderPagination(totalRecords);
            },
            error: function () {
                toastr.error('Failed to load termination data');
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
    $('#terminateTBL th.sort').on('click', function () {
        const column = $(this).data('sort');
        if (sortColumn === column) {
            sortDirection = sortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            sortColumn = column;
            sortDirection = 'asc';
        }
        loadTableData();
    });

    //#endregion

    //#region Handle Add Termination Form Submit

    $('#new_termination form').on('submit', function (e) {
        e.preventDefault();

        const formData = new FormData(this);

        $.ajax({
            url: '/EmployeeTermination/CreateTermination',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    clearTerminationForm()
                    hideModal('new_termination');
                    loadTableData(); // Refresh the table
                } else {
                    toastr.warning(response.message);
                    showDev(response.message, '3')
                }
            },
            error: function (ex) {
                toastr.error('Failed to add termination');
                showDev(ex.message, '4');
            }
        });
    });

    //#endregion

    //#region Handle Edit button click

    $(document).on('click', '.edit-termination11', function (e) {
        e.preventDefault();
        const terminationId = $(this).data('id');
        currentEditingId = terminationId;

        // Get termination data
        $.ajax({
            url: '/EmployeeTermination/GetTermination',
            type: 'GET',
            data: { id: terminationId },
            success: function (response) {
                showDev(response, 'edit data populate')
                if (response.success) {
                    removeTerminationFormReadonly();
                    const data = response.data;
                    $('#duplicateMessage').text('');
                    populateEditModal(data)
                } else {
                    const data = response.data;
                    if (data == null) {
                        toastr.warning(response.message);
                        clearTerminationForm();
                    } else {
                        toastr.warning(response.message);
                        $('#duplicateMessage').text(response.message);
                        populateEditModal(data)
                        makeTerminationFormReadonly()
                    }
                }
            },
            error: function (ex) {
                toastr.error('Failed to load termination data');
                showDev(ex.message, '5');
            }
        });
    });

    function populateEditModal(data) {
        choiceManager.setChoiceValue('editCompany', data.companyId || data.CompanyId);
        setTimeout(function () {
            choiceManager.setChoiceValue('editEmployeeId', data.employeeId || data.EmployeeId);
        }, 500);
        choiceManager.setChoiceValue('editTerminationTypeId', data.terminationTypeId || data.TerminationTypeId);
        flatpickrHelper.setDate('editNoticeDate', data.noticeDate || data.NoticeDate)
        flatpickrHelper.setDate('editTerminationDate', data.terminationDate || data.TerminationDate)
        $('#edit_termination textarea').val(data.reason || data.Reason);
    }

    function removeTerminationFormReadonly() {
        choiceManager.enableChoice('editCompany')
        choiceManager.enableChoice('editEmployeeId')
        choiceManager.enableChoice('editTerminationTypeId')
        flatpickrHelper.enable('editNoticeDate', 'editTerminationDate');
        $('#edit_termination textarea').prop('readonly', false);
    }

    function makeTerminationFormReadonly() {
        choiceManager.disableChoice('editCompany')
        choiceManager.disableChoice('editEmployeeId')
        choiceManager.disableChoice('editTerminationTypeId')
        flatpickrHelper.disable('editNoticeDate', 'editTerminationDate');
        $('#edit_termination textarea').prop('readonly', true);
    }

    // Handle Edit Termination Form Submit
    $('#edit_termination form').on('submit', function (e) {
        e.preventDefault();

        if (!currentEditingId) {
            toastr.error('No termination selected for editing');
            return;
        }

        const formData = new FormData();
        formData.append('terminationId', currentEditingId);
        formData.append('CompanyId', $('#editCompany').val());
        formData.append('EmployeeId', $('#editEmployeeId').val());
        formData.append('TerminationTypeId', $('#editTerminationTypeId').val());
        formData.append('NoticeDate', $('#editNoticeDate').val());
        formData.append('ResignationDate', $('#editTerminationDate').val());
        formData.append('Reason', $('#edit_termination textarea').val());

        $.ajax({
            url: '/EmployeeTermination/UpdateTermination',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    hideModal('edit_termination');
                    currentEditingId = null;
                    loadTableData(); // Refresh the table
                } else {
                    toastr.warning(response.message);
                }
            },
            error: function (ex) {
                toastr.error('Failed to update termination');
                showDev(ex.message, '6')
            }
        });
    });

    //#endregion

    //#region Handle Delete button click

    $(document).on('click', '.delete-termination', function (e) {
        e.preventDefault();
        const terminationId = $(this).data('id');
        currentEditingId = terminationId; // Store for deletion
    });

    // Handle Delete confirmation
    $('#delete_modal .btn-danger').on('click', function (e) {
        e.preventDefault();

        if (!currentEditingId) {
            toastr.error('No termination selected for deletion');
            return;
        }

        $.ajax({
            url: '/EmployeeTermination/DeleteTermination',
            type: 'POST',
            data: { ids: currentEditingId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    hideModal('delete_modal');
                    currentEditingId = null;
                    loadTableData();
                    showDev(response, '6')
                } else {
                    hideModal('delete_modal');
                    $('#notDelete_modal').modal('show');
                    toastr.warning(response.message);
                    showDev(response, '7')
                }
            },
            error: function () {
                toastr.error('Failed to delete termination');
            }
        });
    });

    //#endregion

    //#region clear form

    function clearTerminationForm() {
        const form = document.querySelector('#new_termination form');

        if (!form) return;

        // Reset all input/select/textarea fields
        form.reset();

        // Clear Choices.js dropdowns if used
        const choiceDropdowns = form.querySelectorAll('.choiceDD');
        choiceDropdowns.forEach(dd => {
            if (dd._choices) {
                dd._choices.clearStore();
                dd._choices.setChoices([], 'value', 'label', true);
            }
        });

        // Clear Flatpickr instances
        const dateInputs = form.querySelectorAll('.datetimepicker');
        dateInputs.forEach(input => {
            if (input._flatpickr) {
                input._flatpickr.clear();
            }
        });
    }

    //#endregion

    //#region close modal click

    $('#ndCloseBtn').on('click', function () {
        $('#notDelete_modal').modal('hide');
        showDev('click close ', '8')
    });

    //#endregion

    loadTableData();
});