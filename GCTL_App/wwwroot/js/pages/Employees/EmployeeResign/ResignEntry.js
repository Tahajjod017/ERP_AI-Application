$(document).ready(function () {
    showDev("EmployeeResignEntry.js", 'init');

    let currentPage = 1;
    let pageSize = 10;
    let sortColumn = 'rEmpName';
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
                    showDev(data)
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

    $("#resignationDate, #editResignationDate").flatpickr({
        dateFormat: "d/m/Y",
        disableMobile: true
    });

    //#endregion

<<<<<<< Updated upstream
    //#region Function to fetch and render table data
=======
    //#region  table data Function to fetch and

    function approvalStagesTemplate(data) {
        showDev(data, "tumi reality mene neo");
        const steps = Array.isArray(data.stageDetails) ? data.stageDetails : [data.stageDetails];
        let html = `
        <div class="p-2" style="min-width: 300px;">
            <h6 class="fw-bold mb-3 text-center border-bottom pb-2">📋 Approval Process <span>${data.approvalDetails}</span></h6>
            <div class="approval-steps">
    `;

        steps.forEach((step, index) => {
            // Determine status color and icon
            let statusClass = '';
            let statusIcon = '';

            switch (step.statusName.toLowerCase()) {
                case 'approved':
                    statusClass = 'text-success';
                    statusIcon = '✅';
                    break;
                case 'rejected':
                    statusClass = 'text-danger';
                    statusIcon = '❌';
                    break;
                case 'in progress':
                    statusClass = 'text-warning';
                    statusIcon = '⏳';
                    break;
                case 'pending':
                    statusClass = 'text-info';
                    statusIcon = '📋';
                    break;
                case 'not started':
                    statusClass = 'text-secondary';
                    statusIcon = '⏸️';
                    break;
                default:
                    statusClass = 'text-muted';
                    statusIcon = '📄';
            }

            html += `
            <div class="approval-step p-2  ${index === steps.length - 1 ? '' : 'border-bottom-0'}">
                <div class="d-flex justify-content-between align-items-start mb-1">
                    <span class="fw-bold">${statusIcon} ${step.approverStep}</span>
                    <span class="badge ${statusClass}">${step.statusName}</span>
                </div>
                <div class="step-details">
                    <p class="mb-1"><strong>Approver:</strong> ${step.approvarPerson}</p>
                    <p class="mb-1"><strong>Date:</strong> ${step.approvedOrDeclineDate}</p>
                    <p class="mb-0"><strong>Notes:</strong> <em>${step.approvarNote}</em></p>
                </div>
            </div>
            <hr/>
        `;
        });

        html += `
         </div>
            <div class="progress mt-3" style="height: 8px;">
                <div class="progress-bar" role="progressbar" 
                     style="width: ${(steps.filter(s => s.statusName.toLowerCase() === 'approved').length / steps.length) * 100}%"
                     aria-valuenow="${(steps.filter(s => s.statusName.toLowerCase() === 'approved').length / steps.length) * 100}" 
                     aria-valuemin="0" aria-valuemax="100">
                </div>
            </div>
        </div>
    `;

        return html;
    }

    showDev('tooltip')
  
        UniversalTooltipService.registerTemplate('approvalStagesTemplate', approvalStagesTemplate);
        
   
      
  

>>>>>>> Stashed changes
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
                            <td class="fs-9 align-middle py-1">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" data-bulk-select-row='${JSON.stringify(item)}' />
                                </div>
                            </td>
                            <td class="rEmpName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1" data-column="0">
<<<<<<< Updated upstream
                                <div class="d-flex align-items-center position-relative">
                                    <a href="" class="avatar avatar-md me-2">
                                        <img src="${item.image || item.Image}" class="rounded-circle" alt="user">
                                    </a>
                                    <a class="text-body-highlight fw-bold stretched-link" href="#!">${item.rEmpName || item.REmpName}</a>
=======
                                

                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${item.image || '~/img/No-Image-Placeholder.svg.png'}"
                                             alt="${item.rEmpName}"  />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold mb-0">${item.rEmpName}

                                            <div class="universal-tooltip-container position-relative d-inline-block">
                                                <i class="fa-solid fa-circle-info universal-tooltip-trigger"
                                                   data-tooltip-url="/EmployeeResign/GetDetails"
                                                   data-tooltip-id="${item.resigId}"
                                                   data-tooltip-data-key="id"
                                                   data-tooltip-template="approvalStagesTemplate"
                                                   style="cursor: pointer; font-size: 12px; color: #007bff; margin-left: 5px;"></i>
                                            </div>

                                        </h6>
                                        <span class="fs-12 fw-normal text-muted">#${item.rEmpCode || 'N/A'}</span>
                                    </div>
>>>>>>> Stashed changes
                                </div>
                            </td>
                            <td class="rEmpDept align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1">${item.rEmpDept || item.REmpDept}</td>
                            <td class="resignResons align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.resignResons || item.ResignResons}</td>
                            <td class="resNoticeDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.resNoticeDate || item.ResNoticeDate}</td>
                            <td class="resinDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.resinDate || item.ResinDate}</td>
                            <td class="align-middle white-space-nowrap text-end pe-0 ps-4" data-column="5">
                                <div class="btn-reveal-trigger position-static">
                                    <a href="#" class="nav-item mx-2 edit-resignation11" data-id="${item.resigId || item.ResigId}" data-bs-toggle="modal" data-bs-target="#edit_resignation">
                                        <i class="fas fa-edit text-success"></i>
                                    </a>
                                    <a href="#" class="nav-item mx-2 delete-resignation" data-id="${item.resigId || item.ResigId}" data-bs-toggle="modal" data-bs-target="#delete_modal">
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

    //#endregion

    //#region Handle Add Resignation Form Submit

    $('#new_resignation form').on('submit', function (e) {
        e.preventDefault();

        const formData = new FormData(this);

        $.ajax({
            url: '/EmployeeResign/CreateResignation',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    clearResignationForm()


                    hideModal('new_resignation');

                    loadTableData(); // Refresh the table
                } else {
                    toastr.warning(response.message);
                    showDev(response.message)
                }
            },
            error: function (ex) {
                toastr.error('Failed to add resignation');
                showDev(ex.message);
            }
        });
    });

    //#endregion

    //#region Handle Edit button click

    $(document).on('click', '.edit-resignation11', function (e) {
        e.preventDefault();
        const resignationId = $(this).data('id');
        currentEditingId = resignationId;

        // Get resignation data
        $.ajax({
            url: '/EmployeeResign/GetResignation',
            type: 'GET',
            data: { id: resignationId },
            success: function (response) {
                if (response.success) {
                    const data = response.data;
                    choiceManager.setChoiceValue('editCompany', data.companyId || data.CompanyId);
                    choiceManager.setChoiceValue('editEmployeeId', data.employeeId || data.EmployeeId);
                    flatpickrHelper.setDate('editNoticeDate', data.resNoticeDate || data.ResNoticeDate )
                    flatpickrHelper.setDate('editResignationDate', data.resinDate || data.ResinDate )
                    $('#edit_resignation textarea').val(data.resignResons || data.ResignResons);

                 

                } else {
                    toastr.error(response.message);
                }
            },
            error: function (ex) {
                toastr.error('Failed to load resignation data');
                showDev(ex.message);
            }
        });
    });

    // Handle Edit Resignation Form Submit
    $('#edit_resignation form').on('submit', function (e) {
        e.preventDefault();

        if (!currentEditingId) {
            toastr.error('No resignation selected for editing');
            return;
        }

        const formData = new FormData();
        formData.append('resignationId', currentEditingId);
        formData.append('CompanyId', $('#editCompany').val());
        formData.append('EmployeeId', $('#editEmployeeId').val());
        formData.append('NoticeDate', $('#editNoticeDate').val());
        formData.append('ResignationDate', $('#editResignationDate').val());
        formData.append('Reason', $('#edit_resignation textarea').val());

        $.ajax({
            url: '/EmployeeResign/UpdateResignation',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    
                    hideModal('edit_resignation');

                    currentEditingId = null;
                    loadTableData(); // Refresh the table
                } else {
                    toastr.warning(response.message);
                }
            },
            error: function (ex) {
                toastr.error('Failed to update resignation');
                showDev(ex.message)
            }
        });
    });

    //#endregion

    //#region Handle Delete button click

    $(document).on('click', '.delete-resignation', function (e) {
        e.preventDefault();
        const resignationId = $(this).data('id');
        currentEditingId = resignationId; // Store for deletion
    });

    // Handle Delete confirmation
    $('#delete_modal .btn-danger').on('click', function (e) {
        e.preventDefault();

        if (!currentEditingId) {
            toastr.error('No resignation selected for deletion');
            return;
        }

        $.ajax({
            url: '/EmployeeResign/DeleteResignation',
            type: 'POST',
            data: { id: currentEditingId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    $('#delete_modal').modal('hide');
                    currentEditingId = null;
                    loadTableData(); // Refresh the table
                } else {
                    toastr.error(response.message);
                }
            },
            error: function () {
                toastr.error('Failed to delete resignation');
            }
        });
    });

    //#endregion

    //#region clear form

    function clearResignationForm() {
        const form = document.querySelector('#new_resignation form');

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

   


    loadTableData();
});