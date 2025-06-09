$(document).ready(function () {
    // Configuration
    const API_BASE_URL = '/EmployeeList/GetEmployees';
    

    const ITEMS_PER_PAGE = 3;

    //#region DOM Elements
    const $boardView = $('#boardView');
    const $listView = $('#listView');
    const $employeeListTbody = $('#employeeListTbody'); // Target tbody specifically
    const $departmentFilter = $('#departmentFilter');
    const $statusFilter = $('#statusFilter');
    const $sortFilter = $('#sortFilter');
    const $searchInput = $('.search-input');
    const $listViewBtn = $('#listViewBtn');
    const $boardViewBtn = $('#boardViewBtn');
    const $tablePaginationContainer = $('.pagination'); // Table view pagination
    const $boardPaginationContainer = $('<ul class="mb-0 pagination board-pagination"></ul>'); // New board view pagination

    let currentTablePage = 1;
    let currentBoardPage = 1;
    let currentFilters = {
        department: '',
        status: '',
        sort: '',
        search: '',
        sortColumn: 'joiningDate', // Default sort column
        sortDirection: 'desc' // Default sort direction
    };

    //#endregion

    //#region Fetch employee data
    function fetchEmployees(page = 1, filters = currentFilters) {
        return $.ajax({
            url: API_BASE_URL,
            method: 'GET',
            data: {
                page: page,
                limit: ITEMS_PER_PAGE,
                department: filters.department,
                status: filters.status,
                sort: filters.sort,
                search: filters.search,
                sortColumn: filters.sortColumn,
                sortDirection: filters.sortDirection
            },
            dataType: 'json'
        }).catch(function (error) {
            console.error('Error fetching employees:', error);
            return { employees: [], total: 0 };
        });
    }

    //#endregion

    //#region Generate avatar HTML (image or initial-based) And format date
    function getAvatarHtml(employee) {
        if (employee.avatar && employee.avatar !== '') {
            return `<img class="rounded-circle" src="${employee.avatar}" alt="${employee.name}" />`;
        } else {
            const initial = employee.name.charAt(0).toUpperCase();
            return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
        }
    }

    function GetdateFileter(dateString) {
        if (!dateString) return '-';
        const date = new Date(dateString);
        const options = { year: 'numeric', month: '2-digit', day: '2-digit' };
        return date.toLocaleDateString('en-US', options).replace(/\//g, '-');

    }

    //#endregion


    //#region Render board view
    function renderBoardView(employees) {
        $boardView.empty();
        $.each(employees, function (index, employee) {
            const avatarHtml = getAvatarHtml(employee);
            const dateFileter = GetdateFileter(employee.joiningDate)
            const card = `
                <div class="col">
                    <div class="card mb-3">
                        <div class="card-body">
                            <div class="row align-items-center g-3">
                                <div class="col-12 col-sm-auto flex-1">
                                    <div class="d-md-flex d-xl-block align-items-center justify-content-between mb-5">
                                        <div class="d-flex align-items-center mb-3 mb-md-0 mb-xl-3">
                                            <div class="avatar avatar-xl me-3">
                                                ${avatarHtml}
                                            </div>
                                            <div>
                                                <h5>${employee.name}</h5>
                                                <span class="badge badge-phoenix badge-phoenix-${employee.status === 'Active' ? 'success' : 'danger'} me-2">${employee.status}</span>
                                            </div>
                                        </div>
                                        <div class="d-flex align-items-center mt-2">
                                            <p class="mb-0 fw-bold fs-9">
                                                Designation: <span class="fw-semibold text-body-tertiary text-opactity-85 ms-1">${employee.department}</span>
                                            </p>
                                        </div>
                                        <div class="d-flex align-items-center mt-2">
                                            <p class="mb-0 fw-bold fs-9">
                                                Joining Date: <span class="fw-semibold text-body-tertiary text-opactity-85 ms-1">${dateFileter}</span>
                                            </p>
                                        </div>
                                        <div class="d-flex align-items-center mt-2">
                                            <p class="mb-0 fw-bold fs-9">
                                                Email: <span class="fw-semibold text-body-tertiary text-opactity-85 ms-1">${employee.email}</span>
                                            </p>
                                        </div>
                                        <div class="d-flex align-items-center mt-2">
                                            <p class="mb-0 fw-bold fs-9">
                                                Phone: <span class="fw-semibold text-body-tertiary text-opactity-85 ms-1">${employee.phone}</span>
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>`;
            $boardView.append(card);
        });
    }
    //#endregion

    //#region Render table view
    function renderTableView(employees) {
        $employeeListTbody.empty();
        $.each(employees, function (index, employee) {
            debugger
            const avatarHtml = getAvatarHtml(employee);
            const dateFileter = GetdateFileter(employee.joiningDate)

            const row = `
                <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                    <td class="fs-9 align-middle py-2">
                        <div class="form-check mb-0 fs-8">
                            <input class="form-check-input" type="checkbox"
                                   data-bulk-select-row='{"empID":"${employee.id}","empName":"${employee.name}","empEmail":"${employee.email}","empPhone":"${employee.phone}","empDesignation":"${employee.department}","empJointinDate":"${employee.joiningDate}","empStatus":"${employee.status}"}' />
                        </div>
                    </td>
                    <td class="empID align-middle white-space-nowrap fw-semibold text-body-highlight ps-0 py-0">
                        <a class="fw-bold text-primary" href="#!">${employee.id}</a>
                    </td>
                    <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-0">
                        <div class="d-flex align-items-center position-relative">
                            
                            <a class="text-body-highlight fw-bold stretched-link" href="#!">${employee.name}</a>
                        </div>
                    </td>
                    <td class="empEmail align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                        ${employee.email}
                    </td>
                    <td class="empPhone align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                        ${employee.phone}
                    </td>
                    <td class="empDesignation align-middle white-space-nowrap fw-bold ps-4 text-body py-0">
                        ${employee.department}
                    </td>
                    <td class="empJointinDate align-middle white-space-nowrap fw-bold ps-4 text-body py-0">
                        ${dateFileter}
                    </td>
                    <td class="empStatus align-middle white-space-nowrap fw-bold ps-0 text-body py-0">
                        <span class="badge badge-phoenix badge-phoenix-${employee.status === 'Active' ? 'success' : 'danger'}">${employee.status}</span>
                    </td>
                    <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                        <div class="btn-reveal-trigger position-static g-3">
                            <a href="#" class="nav-item me-2" data-bs-toggle="offcanvas" data-bs-target="#offcanvasRightANE" aria-controls="offcanvasRightANE">
                                <i class="fas fa-edit tblEditBtn"></i>
                            </a>
                            <a href="#" class="nav-item me-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                <i class="fas fa-trash tblDelBtn"></i>
                            </a>

                           
                        </div>
                    </td>
                </tr>`;
            $employeeListTbody.append(row);
        });
    }
    //#endregion

    //#region Render pagination for table view
    function renderTablePagination(totalItems) {
        const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
        $tablePaginationContainer.empty();

        for (let i = 1; i <= totalPages; i++) {
            const pageItem = `
                <li class="page-item ${i === currentTablePage ? 'active' : ''}">
                    <button class="page-link" data-page="${i}">${i}</button>
                </li>`;
            $tablePaginationContainer.append(pageItem);
        }
    }

    //#endregion

    //#region Render pagination for board view
    function renderBoardPagination(totalItems) {
        const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
        $boardPaginationContainer.empty();

        for (let i = 1; i <= totalPages; i++) {
            const pageItem = `
                <li class="page-item ${i === currentBoardPage ? 'active' : ''}">
                    <button class="page-link" data-page="${i}">${i}</button>
                </li>`;
            $boardPaginationContainer.append(pageItem);
        }
    }

    //#endregion

    //#region Load data for table view
    function loadTableData(page = 1) {
        currentTablePage = page;
        fetchEmployees(page, currentFilters).then(function (data) {
            renderTableView(data.employees);
            renderTablePagination(data.total);
            updateViewVisibility();
        });
    }


    //#endregion

    //#region Load data for board view
    function loadBoardData(page = 1) {
        currentBoardPage = page;
        fetchEmployees(page, currentFilters).then(function (data) {
            renderBoardView(data.employees);
            renderBoardPagination(data.total);
            updateViewVisibility();
        });
    }

    //#endregion

    //#region Update view visibility
    function updateViewVisibility() {
        if ($boardViewBtn.hasClass('active')) {
            $boardView.addClass('visible').removeClass('hidden');
            $listView.addClass('hidden').removeClass('visible');
            $tablePaginationContainer.parent().hide();
            $boardPaginationContainer.insertAfter($boardView).show();
        } else {
            $listView.addClass('visible').removeClass('hidden');
            $boardView.addClass('hidden').removeClass('visible');
            $boardPaginationContainer.hide();
            $tablePaginationContainer.parent().show();
            // Initialize sort indicators
            $('#employeeListTable th.sort').removeClass('sort-asc sort-desc');
            const $sortHeader = $(`#employeeListTable th[data-sort="${currentFilters.sortColumn}"]`);
            $sortHeader.addClass(currentFilters.sortDirection === 'asc' ? 'sort-asc' : 'sort-desc');
        }
        // Ensure table header is visible
        $('#employeeListTable').find('thead').show();
    }

    //#endregion

    //#region Event handlers
    $departmentFilter.on('change', function () {
        currentFilters.department = $(this).val();
        loadTableData(1);
        loadBoardData(1);
    });

    $statusFilter.on('change', function () {
        currentFilters.status = $(this).val();
        loadTableData(1);
        loadBoardData(1);
    });

    $sortFilter.on('change', function () {
        currentFilters.sort = $(this).val();
        loadTableData(1);
        loadBoardData(1);
    });

    $searchInput.on('input', function () {
        currentFilters.search = $(this).val();
        loadTableData(1);
        loadBoardData(1);
    });

    $listViewBtn.on('click', function (e) {
        e.preventDefault();
        $listViewBtn.addClass('active');
        $boardViewBtn.removeClass('active');
        loadTableData(currentTablePage);
    });

    $boardViewBtn.on('click', function (e) {
        e.preventDefault();
        $boardViewBtn.addClass('active');
        $listViewBtn.removeClass('active');
        loadBoardData(currentBoardPage);
    });

    //#endregion

    //#region Pagination for table view
    $tablePaginationContainer.on('click', '.page-link', function (e) {
        e.preventDefault();
        const page = $(this).data('page');
        if (page) {
            loadTableData(page);
        } else if ($(this).parent().is('[data-list-pagination="prev"]') && currentTablePage > 1) {
            loadTableData(currentTablePage - 1);
        } else if ($(this).parent().is('[data-list-pagination="next"]')) {
            loadTableData(currentTablePage + 1);
        }
    });

    

    // Pagination for board view
    $boardPaginationContainer.on('click', '.page-link', function (e) {
        e.preventDefault();
        const page = $(this).data('page');
        if (page) {
            loadBoardData(page);
        } else if ($(this).parent().is('[data-list-pagination="prev"]') && currentBoardPage > 1) {
            loadBoardData(currentBoardPage - 1);
        } else if ($(this).parent().is('[data-list-pagination="next"]')) {
            loadBoardData(currentBoardPage + 1);
        }
    });
    //#endregion

    //#region Column sorting
    $('#employeeListTable th.sort').on('click', function () {
        const column = $(this).data('sort');
        if (column) {
            // Toggle sort direction if same column, else default to asc
            if (currentFilters.sortColumn === column) {
                currentFilters.sortDirection = currentFilters.sortDirection === 'asc' ? 'desc' : 'asc';
            } else {
                currentFilters.sortColumn = column;
                currentFilters.sortDirection = 'asc';
            }
            // Update sort indicators
            $('#employeeListTable th.sort').removeClass('sort-asc sort-desc');
            $(this).addClass(currentFilters.sortDirection === 'asc' ? 'sort-asc' : 'sort-desc');
            // Reload table data
            loadTableData(1);
        }
    });

    //#endregion


    //#region Edit button click



    $('#employeeListTbody').on('click', '.tblEditBtn', function (e) {
        e.preventDefault();
        const employeeId = $(this).closest('tr').find('.empID a').text();

        fetchAllEmployeeData(employeeId);
    });

    function fetchEmployeeSection(url) {
        return $.ajax({
            url: url,
            method: 'GET',
            dataType: 'json'
        });
    }

    function fetchAllEmployeeData(employeeId) {
        Promise.allSettled([
            fetchEmployeeSection(`GetEmployeePersonal/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeOfficial/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeAdditional/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeContact/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeEducational/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeFamily/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeSalary/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeTraining/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeAllowance/${employeeId}`),
            fetchEmployeeSection(`GetEmployeeBenefit/${employeeId}`)
        ])
            .then(function (results) {
                const [
                    personal, official, additional, contact,
                    educational, family, salary, training,
                    allowance, benefit
                ] = results;

                if (personal.status === 'fulfilled') PopulatePersonalData(personal.value);
                if (official.status === 'fulfilled') PopulateOfficialData(official.value);
                if (additional.status === 'fulfilled') PopulateAdditionalData(additional.value);
                if (contact.status === 'fulfilled') PopulateContactData(contact.value);
                if (educational.status === 'fulfilled') PopulateEducationalData(educational.value);
                if (family.status === 'fulfilled') PopulateFamilyData(family.value);
                if (salary.status === 'fulfilled') PopulateSalaryData(salary.value);
                if (training.status === 'fulfilled') PopulateTrainingData(training.value);
                if (allowance.status === 'fulfilled') PopulateAllowanceData(allowance.value);
                if (benefit.status === 'fulfilled') PopulateBenefitData(benefit.value);

                // Optional: Show warning for failed ones
                results.forEach((r, i) => {
                    if (r.status === 'rejected') {
                        console.warn(`Section ${i + 1} failed`, r.reason);
                    }
                });
            });
    }

   



    //#endregion

    //#region Populate employee data functions
    function PopulatePersonalData(employee) {
        console.log('Employee Personal data:', employee);
    }

    function PopulateOfficialData(employee) {
        console.log('Employee Official data:', employee);
    }

    function PopulateAdditionalData(employee) {
        console.log('Employee Additional data:', employee);
    }

    function PopulateContactData(employee) {
        console.log('Employee Contact data:', employee);
    }

    function PopulateEducationalData(employee) {
        console.log('Employee Educational data:', employee);
    }

    function PopulateFamilyData(employee) {
        console.log('Employee Family data:', employee);
    }

    function PopulateSalaryData(employee) {
        console.log('Employee Salary data:', employee);
    }

    function PopulateTrainingData(employee) {
        console.log('Employee Training data:', employee);
    }

    function PopulateAllowanceData(employee) {
        console.log('Employee Allowance data:', employee);
    }

    function PopulateBenefitData(employee) {
        console.log('Employee Benefit data:', employee);
    }


        //$.ajax({
        //    url: `${GetEmpById_URL}/${employeeId}`,
        //    method: 'GET',
        //    dataType: 'json',
        //    success: function (employee) {

        //        console.log('Employee data:', employee);

        //        $('#offcanvasRightANE').offcanvas('show');
        //    },

        //    error: function (error) {
        //        console.error('Error fetching employee:', error);
        //        alert('Failed to load employee data.');
        //    }
        //});
    

    //#endregion

    //#region Delete button click

    $('#employeeListTbody').on('click', '.tblDelBtn', function (e) {
        e.preventDefault();
        const employeeId = $(this).closest('tr').find('.empID a').text();
        // Set employee ID in delete modal
        $('#delete_modal').find('input[name="employeeId"]').val(employeeId);
        // Show delete modal
        $('#delete_modal').modal('show');
    });

    //#endregion


    // Initial load
    loadTableData();
    loadBoardData();

    // Ensure table header is visible
    $('#employeeListTable').find('thead').show();
});