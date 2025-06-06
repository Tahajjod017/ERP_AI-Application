$(document).ready(function () {
    // Configuration
    const API_BASE_URL = '/EmployeeList/GetEmployees';
    const ITEMS_PER_PAGE = 3;

    // DOM Elements
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
        search: ''
    };

    // Fetch employee data
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
                search: filters.search
            },
            dataType: 'json'
        }).catch(function (error) {
            console.error('Error fetching employees:', error);
            return { employees: [], total: 0 };
        });
    }

    // Render board view
    function renderBoardView(employees) {
        $boardView.empty();
        $.each(employees, function (index, employee) {
            const card = `
                <div class="col">
                    <div class="card mb-3">
                        <div class="card-body">
                            <div class="row align-items-center g-3">
                                <div class="col-12 col-sm-auto flex-1">
                                    <div class="d-md-flex d-xl-block align-items-center justify-content-between mb-5">
                                        <div class="d-flex align-items-center mb-3 mb-md-0 mb-xl-3">
                                            <div class="avatar avatar-xl me-3">
                                                <img class="rounded-circle" src="${employee.avatar || '../../../assets/img/team/72x72/58.webp'}" alt="" />
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
                                                Joining Date: <span class="fw-semibold text-body-tertiary text-opactity-85 ms-1">${employee.joiningDate}</span>
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

    // Render table view
    function renderTableView(employees) {
        $employeeListTbody.empty(); // Only clear tbody, not the entire table
        $.each(employees, function (index, employee) {
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
                        ${employee.joiningDate}
                    </td>
                    <td class="empStatus align-middle white-space-nowrap fw-bold ps-0 text-body py-0">
                        <span class="badge badge-phoenix badge-phoenix-${employee.status === 'Active' ? 'success' : 'danger'}">${employee.status}</span>
                    </td>
                    <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                        <div class="btn-reveal-trigger position-static g-3">
                            <a href="#" class="nav-item me-2" data-bs-toggle="offcanvas" data-bs-target="#offcanvasRightANE" aria-controls="offcanvasRightANE">
                                <i class="fas fa-edit"></i>
                            </a>
                            <a href="#" class="nav-item me-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                <i class="fas fa-trash"></i>
                            </a>
                        </div>
                    </td>
                </tr>`;
            $employeeListTbody.append(row);
        });
    }

    // Render pagination for table view
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

    // Render pagination for board view
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

    // Load data for table view
    function loadTableData(page = 1) {
        currentTablePage = page;
        fetchEmployees(page, currentFilters).then(function (data) {
            renderTableView(data.employees);
            renderTablePagination(data.total);
            updateViewVisibility();
        });
    }

    // Load data for board view
    function loadBoardData(page = 1) {
        currentBoardPage = page;
        fetchEmployees(page, currentFilters).then(function (data) {
            renderBoardView(data.employees);
            renderBoardPagination(data.total);
            updateViewVisibility();
        });
    }

    // Update view visibility
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
        }
    }

    // Event handlers
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

    // Pagination for table view
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

    // Initial load
    loadTableData();
    loadBoardData();

    // Ensure table header is visible
    $('#employeeListTable').find('thead').show();
});