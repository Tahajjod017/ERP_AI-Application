$(document).ready(function () {
    // Configuration
    const API_BASE_URL = '/EmployeeList/GetEmployees';

    let urle = '';

    const ITEMS_PER_PAGE = 3;

    //#region  Page Load

    //#region DOM Elements
    const $boardView = $('#boardView');
    const $listView = $('#listView');
    const $employeeListTbody = $('#employeeListTbody'); // Target tbody specifically
    const $departmentFilter = $('#departmentFilter');
    const $statusFilter = $('#statusFilter');
    const $sortFilter = $('#sortFilter');
    const $pageSizeData = $('#pageSize');
    const $searchInput = $('.search-input');
    const $listViewBtn = $('#listViewBtn');
    const $boardViewBtn = $('#boardViewBtn');
    const $tablePaginationContainer = $('.tblPagination'); // Table view pagination
   // const $boardPaginationContainer = $('<ul class="mb-0 pagination board-pagination d-flex justify-content-end"></ul>'); // New board view pagination
    const $boardPaginationContainer = $('.paginationBoard'); // New board view pagination

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

        test = $('#pageSize').val();

        //console.log('test', test)

        return $.ajax({
            url: API_BASE_URL,
            method: 'GET',
            data: {
                page: page,
                limit:  ITEMS_PER_PAGE,
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
            urle = employee.url;
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
                                <i class="fas fa-edit text-black tblEditBtn"></i>
                            </a>
                            <a href="#" class="nav-item me-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                <i class="far fa-trash-alt text-black tblDelBtn"></i>
                            </a>

                           
                        </div>
                    </td>
                </tr>`;
            $employeeListTbody.append(row);
        });
    }
    //#endregion

    //#region  pagination

    function renderTablePagination(totalItems) {
        const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
        $tablePaginationContainer.empty();

        let paginationHtml = '<li class="page-item"><button class="page-link" data-page="1">« First</button></li>';

        const startPage = Math.max(1, currentTablePage - 2);
        const endPage = Math.min(totalPages, startPage + 4);

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
            <li class="page-item ${i === currentTablePage ? 'active' : ''}">
                <button class="page-link" data-page="${i}">${i}</button>
            </li>`;
        }

        paginationHtml += `<li class="page-item"><button class="page-link" data-page="${totalPages}">Last »</button></li>`;

        $tablePaginationContainer.append(paginationHtml);
    }

    //#endregion

    //#region Render pagination for board view

    function renderBoardPagination(totalItems) {
        const totalPages = Math.ceil(totalItems / ITEMS_PER_PAGE);
        $boardPaginationContainer.empty();

        let paginationHtml = '<li class="page-item"><button class="page-link" data-page="1">« First</button></li>';

        const startPage = Math.max(1, currentBoardPage - 2);
        const endPage = Math.min(totalPages, startPage + 4);

        for (let i = startPage; i <= endPage; i++) {
            paginationHtml += `
            <li class="page-item ${i === currentBoardPage ? 'active' : ''}">
                <button class="page-link" data-page="${i}">${i}</button>
            </li>`;
        }

        paginationHtml += `<li class="page-item"><button class="page-link" data-page="${totalPages}">Last »</button></li>`;

        $boardPaginationContainer.append(paginationHtml);
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

    //$pageSizeData.on('change', function () {
    //    currentFilters.department = $(this).val();
    //    loadTableData(1);
    //    loadBoardData(1);
    //});

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

    //#endregion

    //#region Edit Form

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

        toastr.info('Form is Ready To Modify')
    }

   



    //#endregion

    //#region Populate employee data functions ANd edit and delete


    //#region Personal info

    //#region Populare Persomal

    function PopulatePersonalData(employee) {
        console.log('Employee Personal Data:', employee);

        var a = employee.firstName + ' ' + employee.lastName;
        $('#empNameEdit').text(a);

        $('#eduEmployeePersonalId').val(employee.employeeID || '');
        $('#trnEmployeePersonalId').val(employee.employeeID || '');


        $('#personalEmployeeCode').val(employee.employeeCode || '');
        $('#personalFirstName').val(employee.firstName || '');
        $('#personalLastName').val(employee.lastName || '');
        $('#personalPersonalMobile').val(employee.mobileNumber || '');
        $('#personalPersonalEmail').val(employee.email || '');
        $('#personalTinNo').val(employee.tin || '');
        $('#personalFatherName').val(employee.fatherName || '');
        $('#personalMotherName').val(employee.motherName || '');
        $('#personalBirthCertificateNo').val(employee.birthCertificateNo || '');
        $('#personalNationalId').val(employee.nid || '');
        $('#personalAboutEmployee').val(employee.aboutEmployee || '');
        $('#personalState').val(employee.state || '');
        $('#personalCity').val(employee.city || '');
        $('#personalHouseNo').val(employee.houseNo || '');
        $('#personalRoadNo').val(employee.roadNo || '');
        $('#personalPostalCode').val(employee.postalCode || '');

        $('#personalNationality').val(employee.nationality || '');

        $('#personalEmployeeID').val(employee.employeeID || '');
        
        $('#personalDateOfBirth').val(employee.dateOfBirth || '');
        flatpickrHelper.setDate('personalDateOfBirth', (employee.dateOfBirth || ''))

       

        choiceManager.setChoiceValue('personalReligion', employee.religionID || '');
        choiceManager.setChoiceValue('personalBloodGroup', employee.bloodGroupID || '');
        choiceManager.setChoiceValue('personalMaritalStatus', employee.maritalStatusID || '');
        choiceManager.setChoiceValue('personalCountry', employee.countryID || '');
        choiceManager.setChoiceValue('personalGender', employee.genderID || '');



        // For image preview
        if (employee.employeeImageFileName) {
            var imgFile = urle + 'images/' + employee.employeeImageFileName
            $('#epImagePreview').attr('src', imgFile).css('visibility', 'visible');
            $('#epCloseBtn').show();
        } else {
            $('#epImagePreview').css('visibility', 'hidden');
            $('#epCloseBtn').hide();
        }

        if (employee.employeeSignatureFileName) {
            var sigFile = urle + 'signatures/' + employee.employeeSignatureFileName
            $('#esImagePreview').attr('src', sigFile).css('visibility', 'visible');
            $('#esCloseBtn').show();
        } else {
            $('#esImagePreview').css('visibility', 'hidden');
            $('#esCloseBtn').hide();
        }
    }

    //#endregion

    //#region Submit

    $('#personalSubmitButton').click(function (e) {
        e.preventDefault(); // prevent form default submission

        clearErrors(); // clear previous errors
        let valid = true;

        const enteredNationality = $('#personalNationality').val().trim();
        if (enteredNationality && !nationalities.includes(enteredNationality)) {
            $('#newNationalityName').val(enteredNationality);
            $('#addNationalityModal').modal('show');
        }

        const firstName = $("#personalFirstName").val().trim();
        const lastName = $("#personalLastName").val().trim();
        const email = $("#personalPersonalEmail").val().trim();
        const mobile = $("#personalPersonalMobile").val().trim();

        if (!firstName) {
            showError("personalFirstName", "First Name is required.");
            valid = false;
        }

        if (!lastName) {
            showError("personalLastName", "Last Name is required.");
            valid = false;
        }

        if (!email) {
            showError("personalPersonalEmail", "Email is required.");
            valid = false;
        } else if (!isValidEmail(email)) {
            showError("personalPersonalEmail", "Invalid email format.");
            valid = false;
        }

        if (!mobile) {
            showError("personalPersonalMobile", "Mobile number is required.");
            valid = false;
        }

        if (!valid) return; // stop if validation fails

        var formData = new FormData();

        formData.append('EmployeeId', $('#personalEmployeeID').val() || '');
        formData.append('EmployeeCode', $('#personalEmployeeCode').val() || '');
        formData.append('FirstName', firstName);
        formData.append('LastName', lastName);
        formData.append('PersonalMobile', mobile);
        formData.append('PersonalEmail', email);
        formData.append('Gender', $('#personalGender').val() || '');
        formData.append('TinNo', $('#personalTinNo').val() || '');
        formData.append('FatherName', $('#personalFatherName').val() || '');
        formData.append('MotherName', $('#personalMotherName').val() || '');
        formData.append('Religion', $('#personalReligion').val() || '');
        formData.append('DateOfBirth', $('#personalDateOfBirth').val() || '');
        formData.append('BirthCertificateNo', $('#personalBirthCertificateNo').val() || '');
        formData.append('BloodGroup', $('#personalBloodGroup').val() || '');
        formData.append('Nationality', $('#personalNationality').val() || '');
        formData.append('NationalId', $('#personalNationalId').val() || '');
        formData.append('MaritalStatus', $('#personalMaritalStatus').val() || '');
        formData.append('AboutEmployee', $('#personalAboutEmployee').val() || '');
        formData.append('Country', $('#personalCountry').val() || '');
        formData.append('State', $('#personalState').val() || '');
        formData.append('City', $('#personalCity').val() || '');
        formData.append('HouseNo', $('#personalHouseNo').val() || '');
        formData.append('RoadNo', $('#personalRoadNo').val() || '');
        formData.append('PostalCode', $('#personalPostalCode').val() || '');

        var employeePic = $('#personalEmployeePicture')[0].files[0];
        if (employeePic) {
            formData.append('EmployeePicture', employeePic);
        }

        var signaturePic = $('#personalSignature')[0].files[0];
        if (signaturePic) {
            formData.append('Signature', signaturePic);
        }

        $.ajax({
            url: '/EmployeePersonal/SubmitFromEdit', // your API endpoint
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                toastr.success(response.message || 'Personal data saved successfully!');
                console.log('Save success:', response);
            },
            error: function (xhr) {
                toastr.error('Failed to save personal data.');
                console.log('Save error:', xhr.responseText);
            }
        });
    });

    // Supporting functions
    function clearErrors() {
        $('.error-text').remove(); // Assuming you append errors with this class
    }

    function showError(elementId, message) {
        const element = $('#' + elementId);
        element.after('<span class="error-text text-danger">' + message + '</span>');
    }

    function isValidEmail(email) {
        var re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }


    //#endregion

    //#region Image Perview

    $(function () {
        function setupImagePreview($fileInput, $previewImg, $closeBtn) {
            $fileInput.on('change', function () {
                const file = this.files[0];
                if (file && file.type.startsWith('image/')) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        $previewImg
                            .attr('src', e.target.result)
                            .css('visibility', 'visible');
                        $closeBtn.show();
                    };
                    reader.readAsDataURL(file);
                } else {
                    // optional: warn if not an image
                    alert('Please select a valid image file.');
                    this.value = '';
                }
            });

            $closeBtn.on('click', function () {
                $fileInput.val('');
                $previewImg
                    .attr('src', '')
                    .css('visibility', 'hidden');
                $closeBtn.hide();
            });
        }

        // wire up both fields
        setupImagePreview(
            $('#personalEmployeePicture'),
            $('#epImagePreview'),
            $('#epCloseBtn')
        );
        setupImagePreview(
            $('#personalSignature'),
            $('#esImagePreview'),
            $('#esCloseBtn')
        );
    });

    //#endregion

    //#region Auto suggest

    let nationalities = [];

    $.ajax({
        url: '/EmployeePersonal/GetNationalities',
        method: 'GET',
        success: function (data) {
            nationalities = data;
        },
        error: function () {
            alert('Failed to load nationalities');
        }
    });

    function showSuggestions(query) {
        const $list = $('#nationalityList');
        const $noResults = $('#noResults');
        const $clickHereBtn = $('#clickHereBtn');
        $list.empty();
        $noResults.hide();
        $clickHereBtn.hide();

        if (!query) {
            $('#searchResults').hide();
            return;
        }

        // Show partial matches while typing (normal behavior)
        const filtered = nationalities.filter(item =>
            item.toLowerCase().includes(query.toLowerCase())
        );

        if (filtered.length > 0) {
            filtered.forEach(item => {
                $list.append(`<button type="button" class="list-group-item list-group-item-action nationality-item">${item}</button>`);
            });
            $noResults.hide();
            $clickHereBtn.hide();
        } else {
            // No match found at all
            $noResults.show();
            $clickHereBtn.show();
        }
    }

    $('#personalNationality').on('input keyup', function () {
        const query = $(this).val().trim();

        if (query) {
            $('#searchResults').show();
            $('#removeNationalityBtn').show();
            showSuggestions(query);
        } else {
            $('#searchResults').hide();
            $('#removeNationalityBtn').hide();
            $('#noResults').hide();
            $('#clickHereBtn').hide();
        }
    });

    // Check for exact match when user clicks outside (blur)
    $('#personalNationality').on('blur', function () {
        const query = $(this).val().trim();

        if (!query) {
            $('#searchResults').hide();
            $('#noResults').hide();
            $('#clickHereBtn').hide();
            return;
        }

        // Check if the input exactly matches any nationality
        const exactMatch = nationalities.find(item =>
            item.toLowerCase() === query.toLowerCase()
        );

        if (!exactMatch) {
            // No exact match found - show "No results" and keep search results visible
            $('#searchResults').show(); // Keep the dropdown visible
            $('#noResults').show();
            $('#clickHereBtn').show();
            $('#nationalityList').empty(); // Clear suggestions but keep dropdown
        } else {
            // Exact match found - hide "No results"
            $('#noResults').hide();
            $('#clickHereBtn').hide();
            $('#searchResults').hide(); // Hide dropdown for exact match
        }
    });

    // Removed the problematic change event handler - not needed anymore

    // On selecting a suggestion - Alternative approach
    $(document).on('mousedown', '.nationality-item', function (e) {
        e.preventDefault(); // Prevent blur event from firing first
        const selected = $(this).text();
        $('#personalNationality').val(selected);
        $('#searchResults').hide();
        $('#removeNationalityBtn').show();
        $('#noResults').hide();
        $('#clickHereBtn').hide();
        $('#nationalityList').empty();

        // Focus back to input to prevent any issues
        $('#personalNationality').focus();
    });

    // Clear selected value
    $('#removeNationalityBtn').on('click', function () {
        $('#personalNationality').val('');
        $('#nationalityList').empty();
        $('#noResults').hide();
        $('#clickHereBtn').hide();
        $('#searchResults').hide();
        $(this).hide();
    });

    // Show modal when clicking "Click Here"
    $('#addNewNationalityBtn').on('click', function () {
        const newValue = $('#personalNationality').val();
        $('#newNationalityName').val(newValue);
        $('#addNationalityModal').modal('show');
    });

    // Optional: hide suggestion list when clicking outside (but keep "No results" visible)
    $(document).on('click', function (e) {
        // Don't hide if clicking on nationality item, input field, search results, or click here button
        if (!$(e.target).closest('#personalNationality, #searchResults, #clickHereBtn, .nationality-item').length) {
            // Only hide if there's no "No results" showing
            if (!$('#noResults').is(':visible')) {
                $('#searchResults').hide();
            }
        }
    });

    //#endregion

    //#region Save Nationality

    $('#confirmAddNationalityBtn').on('click', function () {
        const newNationality = $('#newNationalityName').val().trim();

        if (!newNationality) {
            alert('Please enter a nationality name.');
            return;
        }

        $.ajax({
            url: '/EmployeePersonal/SaveNationality', // <-- Update with your actual route
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(newNationality),
            success: function (response) {
                if (response.success) {
                    nationalities.push(newNationality);
                    $('#nationalitySearch').val(newNationality);
                    $('#searchResults').hide();
                    $('#removeNationalityBtn').show();
                    $('#addNationalityModal').modal('hide');
                }
            },
            error: function (xhr) {
                alert('Error saving nationality: ' + xhr.responseText);
            }
        });
    });


    //#endregion

    //#endregion

    //#region official info

    //#region populate oficial
    function PopulateOfficialData(employee) {
        console.log('Employee Official data:', employee);
        
        // Dropdowns
        choiceManager.setChoiceValue('officialOrganizationID', employee.organizationID || '');
        choiceManager.setChoiceValue('officialOrganizationBranchID', employee.organizationBranchID || '');
        choiceManager.setChoiceValue('officialEmployeeTypeID', employee.employeeTypeID || '');
        choiceManager.setChoiceValue('officialDepartmentID', employee.departmentID || '');
        choiceManager.setChoiceValue('officialDesignationID', employee.designationID || '');
        choiceManager.setChoiceValue('officialEmploymentNatureID', employee.employmentNatureID || '');
        choiceManager.setChoiceValue('officialSeniorSupervisorId', employee.seniorSupervisorId || '');
        choiceManager.setChoiceValue('officialImmediateSupervisorId', employee.immediateSupervisorId || '');
        choiceManager.setChoiceValue('officialHeadOfDepartmentId', employee.headOfDepartmentId || '');
        choiceManager.setChoiceValue('officialEmploymentStatusId', employee.employmentStatusId || '');
        choiceManager.setChoiceValue('officialProvisionPeriodTtimeTypeID', employee.provisionPeriodTtimeTypeID || '');

        // Text inputs
        $('#officialOfficePhone').val(employee.officePhone || '');
        $('#officialOfficeEmail').val(employee.officeEmail || '');
        $('#officialAttendanceId').val(employee.attendanceId || '');
        $('#officialAppointmentLetterNo').val(employee.appointmentLetterNo || '');
        $('#officialConfirmationLetterNo').val(employee.confirmationLetterNo || '');
        $('#officialProvisionPeriod').val(employee.provisionPeriod || '');

        $('#officialEmployeeOfficeID').val(employee.employeeOfficeInfoID || '');

        // Date inputs (if using datepicker/flatpickr, format might be required)
        flatpickrHelper.setDate('#officialAppointmentLetterIssueDate' , (employee.appointmentLetterIssueDate || ''));
        flatpickrHelper.setDate('#officialJoiningDate' ,(employee.joiningDate || ''));
        flatpickrHelper.setDate('#officialProvisionPeriodStartDate' , (employee.provisionPeriodStartDate || ''));
        flatpickrHelper.setDate('#officialConfirmationDate' , (employee.confirmationDate || ''));
        flatpickrHelper.setDate('#officialContractEndDate' , (employee.contractEndDate || ''));
    }

    //#endregion

    //#region Submit
    $('#officialSubmitBtn').on('click', function (e) {
        e.preventDefault();

        if (!validateOfficialForm()) {
            const firstError = $('.validation-error').first();
            if (firstError.length) {
                $('html, body').animate({
                    scrollTop: firstError.offset().top - 100
                }, 500);
            }
            return;
        }

        var formData = new FormData();

        formData.append('EmployeePersonalId', $('#EmployeePersonalId').val());
        formData.append('EmployeeOfficeInfoID', $('#officialEmployeeOfficeID').val());

        formData.append('OrganizationID', choiceManager.getChoiceValue('officialOrganizationID') || '');
        formData.append('OrganizationBranchID', choiceManager.getChoiceValue('officialOrganizationBranchID') || '');
        formData.append('EmployeeTypeID', choiceManager.getChoiceValue('officialEmployeeTypeID') || '');
        formData.append('DepartmentID', choiceManager.getChoiceValue('officialDepartmentID') || '');
        formData.append('DesignationID', choiceManager.getChoiceValue('officialDesignationID') || '');
        formData.append('EmploymentNatureID', choiceManager.getChoiceValue('officialEmploymentNatureID') || '');
        formData.append('SeniorSupervisorId', choiceManager.getChoiceValue('officialSeniorSupervisorId') || '');
        formData.append('ImmediateSupervisorId', choiceManager.getChoiceValue('officialImmediateSupervisorId') || '');
        formData.append('HeadOfDepartmentId', choiceManager.getChoiceValue('officialHeadOfDepartmentId') || '');
        formData.append('EmploymentStatusId', choiceManager.getChoiceValue('officialEmploymentStatusId') || '');
        formData.append('ProvisionPeriodTtimeTypeID', choiceManager.getChoiceValue('officialProvisionPeriodTtimeTypeID') || '');

        formData.append('OfficePhone', $('#officialOfficePhone').val());
        formData.append('OfficeEmail', $('#officialOfficeEmail').val());
        formData.append('AttendanceId', $('#officialAttendanceId').val());
        formData.append('AppointmentLetterNo', $('#officialAppointmentLetterNo').val());
        formData.append('ConfirmationLetterNo', $('#officialConfirmationLetterNo').val());
        formData.append('ProvisionPeriod', $('#officialProvisionPeriod').val());

        formData.append('AppointmentLetterIssueDate', flatpickrHelper.getDate('#officialAppointmentLetterIssueDate') || '');
        formData.append('JoiningDate', flatpickrHelper.getDate('#officialJoiningDate') || '');
        formData.append('ProvisionPeriodStartDate', flatpickrHelper.getDate('#officialProvisionPeriodStartDate') || '');
        formData.append('ConfirmationDate', flatpickrHelper.getDate('#officialConfirmationDate') || '');
        formData.append('ContractEndDate', flatpickrHelper.getDate('#officialContractEndDate') || '');

        console.log('Official data saved before:', formData);

        const $submitBtn = $('#officialSubmitBtn');
        const originalText = $submitBtn.text();
        $submitBtn.prop('disabled', true).text('Saving...');

        $.ajax({
            url: '/EmployeeOfficial/SubmitFromEdit',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                    console.log('Official data saved successfully:', response);
                if (response.success) {
                    toastr.success(response.message);
                } else {
                    toastr.warning(response.message);
                }
            },
            error: function (xhr) {
                console.error('Error:', xhr);
                if (xhr.responseJSON && xhr.responseJSON.errors) {
                    Object.keys(xhr.responseJSON.errors).forEach(key => {
                        const errors = xhr.responseJSON.errors[key];
                        if (errors.length > 0) {
                            showError(key, errors[0]);
                        }
                    });
                } else {
                    alert('Something went wrong! Please try again.');
                }
            },
            complete: function () {
                $submitBtn.prop('disabled', false).text(originalText);
            }
        });
    });
    //#endregion

    //#region Validation
    function isValidEmail(email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    function showError(fieldName, message) {
        let $input = $("input[name='" + fieldName + "']");
        if ($input.length === 0) {
            $input = $("select[name='" + fieldName + "']");
        }
        if ($input.length === 0) {
            $input = $("#" + fieldName);
        }

        if ($input.length === 0) {
            console.error("Could not find input for field: " + fieldName);
            return;
        }

        removeError(fieldName);

        $input.addClass('border-danger').css('border-color', '#dc3545');

        const $error = $('<span class="text-danger d-block mt-1 validation-error" data-field="' + fieldName + '" style="padding:2px 4px; font-size:12px;">' + message + '</span>');

        const $container = $input.closest('.form-floating, .flatpickr-input-container, .col-sm-12, .col-md-4');
        if ($container.length > 0) {
            $container.after($error);
        } else {
            $input.after($error);
        }
    }

    function removeError(fieldName) {
        const $input = $("input[name='" + fieldName + "'], select[name='" + fieldName + "'], #" + fieldName);
        $input.removeClass('border-danger').css('border-color', '');
        $(".validation-error[data-field='" + fieldName + "']").remove();
    }

    function clearErrors() {
        $(".validation-error").remove();
    }

    function validateOfficialForm() {
        clearErrors();
        let isValid = true;

        const requiredInputs = [
            { name: "OfficePhone", label: "Office Phone" },
            { name: "OfficeEmail", label: "Office Email", type: "email" },
            { name: "AttendanceId", label: "Attendance ID" },
            { name: "JoiningDate", label: "Joining Date" },
            { name: "AppointmentLetterNo", label: "Appointment Letter No" },
            { name: "ConfirmationLetterNo", label: "Confirmation Letter No" }
        ];

        requiredInputs.forEach(field => {
            const $input = $("input[name='" + field.name + "']");
            if ($input.length === 0) return;

            const value = $input.val().trim();
            if (!value) {
                showError(field.name, field.label + " is required.");
                isValid = false;
            } else if (field.type === "email" && !isValidEmail(value)) {
                showError(field.name, "Invalid email format.");
                isValid = false;
            }
        });

        const probationPeriod = $('#officialProvisionPeriod').val().trim();
        const probationStartDate = $('#officialProvisionPeriodStartDate').val().trim();
        const timeUnit = $('#officialProvisionPeriodTtimeTypeID').val();

        if (probationPeriod && (!probationStartDate || !timeUnit)) {
            if (!probationStartDate) {
                showError("ProvisionPeriodStartDate", "Probation start date is required when probation period is specified.");
                isValid = false;
            }
            if (!timeUnit) {
                showError("ProvisionPeriodTtimeTypeID", "Time unit is required when probation period is specified.");
                isValid = false;
            }
        }

        return isValid;
    }
    //#endregion

    //#endregion

    //#region Additional info

    //#region Populate Additional
    function PopulateAdditionalData(employee) {
        console.log('Employee Additional data:', employee);

       
        // Passport Information
        $("#additionalEmployeeAdditionalInfoID").val(employee.employeeAdditionalInfoID || '');
        $("#additionalEmployeePersonalID").val(employee.employeePersonalId || '');
        $("#additionalPasportName").val(employee.pasportName || '');
        $("#additionalPasportNo").val(employee.pasportNo || '');
        $("#additionalPasportPlaceOfIssue").val(employee.pasportPlaceOfIssue || '');
      
        // Driving License Information
        $("#additionalDrivingLicenceNo").val(employee.drivingLicenceNo || '');    

        $("#additionalSymbolOfVehicleClass").val(employee.symbolOfVehicleClass || '');
        $("#additionalDrivingLicencePlaceOfIssue").val(employee.drivingLicencePlaceOfIssue || '');

        // Work Permit Information
        $("#additionalWorkPermaitNumber").val(employee.workPermaitNumber || '');
        $("#additionalWorkPermitType").val(employee.workPermitType || '');

        // Apply formatting to your date fields
        flatpickrHelper.setDate('additionalPasportIssueDate', employee.pasportIssueDate);
        flatpickrHelper.setDate('additionalPasportExpireDate', employee.pasportExpireDate);
        flatpickrHelper.setDate('additionalDrivingLicenceIssueDate', employee.drivingLicenceIssueDate);
        flatpickrHelper.setDate('additionalDrivingLicenceExpireDate', employee.drivingLicenceExpireDate);
        flatpickrHelper.setDate('additionalWorkPermitEffectiveDate', employee.workPermitEffectiveDate);
        flatpickrHelper.setDate('additionalWorkPermitExpireDate', employee.workPermitExpireDate);
        flatpickrHelper.setDate('additionalVisaExpireDate', employee.visaExpireDate);

        choiceManager.setChoiceValue('additionalLicenceTypeID', employee.licenceTypeID || '')

    }
    //#endregion

    //#region submit

    $('#additionalSubmitBtn').on('click', function (e) {
        e.preventDefault();
       

        const fields = ["additionalPasportName", "additionalPasportNo", "additionalDrivingLicenceNo", "additionalSymbolOfVehicleClass"];

        if (!validateFields(fields)) {
            return;
        }
        

        SubmitAdditionalData();
    });

    function SubmitAdditionalData() {
        var formData = new FormData();

        formData.append('EmployeeAdditionalInfoID', $("#additionalEmployeeAdditionalInfoID").val() || 0);
        formData.append('EmployeePersonalId', $("#additionalEmployeePersonalID").val() || 0);
        formData.append('PasportName', $("#additionalPasportName").val());
        formData.append('PasportNo', $("#additionalPasportNo").val());
        formData.append('PasportPlaceOfIssue', $("#additionalPasportPlaceOfIssue").val());
        formData.append('DrivingLicenceNo', $("#additionalDrivingLicenceNo").val());
        formData.append('SymbolOfVehicleClass', $("#additionalSymbolOfVehicleClass").val());
        formData.append('DrivingLicencePlaceOfIssue', $("#additionalDrivingLicencePlaceOfIssue").val());
        formData.append('WorkPermaitNumber', $("#additionalWorkPermaitNumber").val());
        formData.append('WorkPermitType', $("#additionalWorkPermitType").val());

        formData.append('LicenceTypeID', choiceManager.getChoiceValue('additionalLicenceTypeID'));
        formData.append('PasportIssueDate', flatpickrHelper.getDate('additionalPasportIssueDate'));
        formData.append('PasportExpireDate', flatpickrHelper.getDate('additionalPasportExpireDate'));
        formData.append('DrivingLicenceIssueDate', flatpickrHelper.getDate('additionalDrivingLicenceIssueDate'));
        formData.append('DrivingLicenceExpireDate', flatpickrHelper.getDate('additionalDrivingLicenceExpireDate'));
        formData.append('WorkPermitEffectiveDate', flatpickrHelper.getDate('additionalWorkPermitEffectiveDate'));
        formData.append('WorkPermitExpireDate', flatpickrHelper.getDate('additionalWorkPermitExpireDate'));
        formData.append('VisaExpireDate', flatpickrHelper.getDate('additionalVisaExpireDate'));

        console.log('FormData prepared for submission:' , formData);
        

        $.ajax({
            url: '/EmployeeAdditional/SubmitFromEdit', 
            type: 'POST',
            data: formData,
            processData: false, 
            contentType: false, 
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Additional data saved successfully.');
                } else {
                    toastr.warning(response.message || 'Something went wrong.');
               }                            
            },
            error: function (xhr, status, error) {
                toastr.error('Error saving additional data: ' + error);
            }
        });
    }


    //#endregion


    //#endregion

    //#region Educational info

    //#region Populate Table
    function PopulateEducationalData(employee) {
        console.log('Employee Educational data:', employee);

        // Clear the existing table rows first
        $('#educationalTable tbody').empty();

        var chkEduData = Array.isArray(employee) ? employee[0] : employee;

        if (chkEduData.isActive) {
            if (employee && employee.length > 0) {
                employee.forEach(function (edu, index) {
                    var row = `
                <tr>
                    <td>${edu.degreeID ?? ''}</td>
                    <td>${edu.majorSubject ?? ''}</td>
                    <td>${edu.institutionName ?? ''}</td>
                    <td>${(edu.resultTypeID ?? '').replace(/\r\n/g, '').trim()}</td>
                    <td>${edu.passingYearID ?? ''}</td>
                    <td>${edu.yearDuration ?? ''}</td>
                    <td>${edu.achievement ?? ''}</td>
                    <td>
                        <a class="nav-item me-2 editEducationBtn" data-id="${edu.employeeEducationalInfoID}"><i class="fas fa-edit text-black"></i></a>
                        <a class="nav-item me-2 deleteEducationBtn" data-id="${edu.employeeEducationalInfoID}"><i class="far fa-trash-alt text-black"></i></a>
                    </td>
                </tr>
            `;
                    $('#educationalTable tbody').append(row);
                });
            } else {
                var emptyRow = `
            <tr>
                <td colspan="8" class="text-center">No educational records found.</td>
            </tr>
        `;
                $('#educationalTable tbody').append(emptyRow);
            }
        }

        
    }
    //#endregion

    //#region Edit Click to populate

    $(document).on('click', '.editEducationBtn', function () {
        var id = $(this).data('id');

        $.ajax({
            url: '/EmployeeEducation/GetEmployeeEduData', 
            type: 'GET',
            data: { id: id },
            success: function (edu) {
               
                $("#eduEmployeeEducationalInfoID").val(edu.employeeEducationalInfoID);
                $("#eduEmployeePersonalId").val(edu.employeePersonalId);
                $('#eduMajorSubject').val(edu.majorSubject);
                $('#eduInstitutionName').val(edu.institutionName);
                $('#eduYearDuration').val(edu.yearDuration);
                $('#eduAchievement').val(edu.achievement);

                choiceManager.setChoiceValue('eduEducationLevelID', edu.educationLevelID || '');
                choiceManager.setChoiceValue('eduDegreeID', edu.degreeID || '');
                choiceManager.setChoiceValue('eduEducationBoardID', edu.educationBoardID || '');
                choiceManager.setChoiceValue('eduResultTypeID', edu.resultTypeID || '');
                choiceManager.setChoiceValue('eduPassingYearID', edu.passingYearID || '');
            },
            error: function () {
                toastr.error('Failed to fetch educational info for editing.');
            }
        });
    });

    //#endregion

    //#region submit edit form

    $('#educationSubmitBtn').on('click', function (e) {
        e.preventDefault();
        SubmitEducationalData();
    });

    function SubmitEducationalData() {
        var formData = new FormData();

        formData.append('EmployeeEducationalInfoID', $("#eduEmployeeEducationalInfoID").val() || 0);
        formData.append('EmployeePersonalId', $("#eduEmployeePersonalId").val());
        formData.append('MajorSubject', $('#eduMajorSubject').val());
        formData.append('InstitutionName', $('#eduInstitutionName').val());
        formData.append('YearDuration', $('#eduYearDuration').val());
        formData.append('Achievement', $('#eduAchievement').val());

        formData.append('EducationLevelID', choiceManager.getChoiceValue('eduEducationLevelID'));
        formData.append('DegreeID', choiceManager.getChoiceValue('eduDegreeID'));
        formData.append('EducationBoardID', choiceManager.getChoiceValue('eduEducationBoardID'));
        formData.append('ResultTypeID', choiceManager.getChoiceValue('eduResultTypeID'));
        formData.append('PassingYearID', choiceManager.getChoiceValue('eduPassingYearID'));

        $.ajax({
            url: '/EmployeeEducation/SubmitFromEdit', // Change URL accordingly
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    
                    toastr.success(response.message || 'Educational info saved successfully');
                    PopulateEducationalData(response.data); // Reload table with new data
                    ClearEducationalForm();
                } else {
                    toastr.warning(response.message || 'Something went wrong');
                }
            },
            error: function (xhr) {
                toastr.error('Failed to save educational info');
            }
        });
    }


    //#endregion

    //#region Delete Edu

    let deleteId = 0;

    $(document).on('click', '.deleteEducationBtn', function () {
        deleteId = $(this).data('id');
        $('#confirmDeleteModalEdu').modal('show');
    });

    $('#confirmDeleteBtnEdu').on('click', function () {
        $.ajax({
            url: '/EmployeeEducation/DeleteFromEdit', // Your delete backend route
            type: 'POST',
            data: { id: deleteId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Educational info deleted successfully.');
                    PopulateEducationalData(response.data); // Refresh table
                    $('#confirmDeleteModalEdu').modal('hide');
                    ClearEducationalForm();
                } else {
                    toastr.warning(response.message || 'Educational info Not deleted .');
                }
                
            },
            error: function () {
                toastr.error('Failed to delete educational info.');
            }
        });
    });


    //#endregion

    //#region Clear Form

    function ClearEducationalForm() {
       
        $("#eduEmployeeEducationalInfoID").val('');
       // $("#eduEmployeePersonalId").val('');
        $('#eduMajorSubject').val('');
        $('#eduInstitutionName').val('');
        $('#eduYearDuration').val('');
        $('#eduAchievement').val('');

        // Clear with choiceManager (if it manages these)
        choiceManager.clearChoice('eduEducationLevelID');
        choiceManager.clearChoice('eduDegreeID');
        choiceManager.clearChoice('eduEducationBoardID');
        choiceManager.clearChoice('eduResultTypeID');
        choiceManager.clearChoice('eduPassingYearID');

    }


    //#endregion

    //#endregion

    //#region Training info

    //#region Populate Table
    function PopulateTrainingData(employeeTrainingList) {
        console.log('Employee Training data:', employeeTrainingList);



        const tbody = $("#employeeTrainingTable tbody");
        tbody.empty(); // Clear table before adding rows

        var chkTrnData = Array.isArray(employeeTrainingList) ? employeeTrainingList[0] : employeeTrainingList;

        if (chkTrnData.isActive) {
            if (employeeTrainingList && employeeTrainingList.length > 0) {
                employeeTrainingList.forEach(function (item, index) {
                    var row = `
                <tr>
                    <td>${item.tranningTitle || ''}</td>
                    <td>${item.topicCovered || ''}</td>
                    <td>${item.instituteName || ''}</td>
                    <td>${item.countryID || ''}</td>
                    <td>${item.locationName || ''}</td>
                    <td>${item.trainingYearID || ''}</td>
                    <td>${item.yearDuration || ''}</td>
                    <td>
                        <a class="nav-item me-2 editTrainingBtn" data-id="${item.employeeTranningInfoID}"><i class="fas fa-edit text-black"></i></a>
                        <a class="nav-item me-2 deleteTrainingBtn" data-id="${item.employeeTranningInfoID}"><i class="far fa-trash-alt text-black"></i></a>
                    </td>
                </tr>
            `;
                    tbody.append(row);
                });
            } else {
                var emptyRow = `
            <tr>
                <td colspan="8" class="text-center">No training records found.</td>
            </tr>
        `;
                tbody.append(emptyRow);
            }
        }

        
    }
    //#endregion

    //#region Edit Click to populate

    $(document).on('click', '.editTrainingBtn', function () {
        var id = $(this).data('id');

        $.ajax({
            url: '/EmployeeTraining/GetEmployeeTrainingData',
            type: 'GET',
            data: { id: id },
            success: function (training) {
                $("#trnEmployeeTranningInfoID").val(training.employeeTranningInfoID);
                $("#trnEmployeePersonalId").val(training.employeePersonalId);
                $('#trnTranningTitle').val(training.tranningTitle);
                $('#trnTopicCovered').val(training.topicCovered);
                $('#trnInstituteName').val(training.instituteName);
                $('#trnLocationName').val(training.locationName);
                $('#trnYearDuration').val(training.yearDuration);

                choiceManager.setChoiceValue('trnCountryID', training.countryID || '');
                choiceManager.setChoiceValue('trnTrainingYearID', training.trainingYearID || '');
            },
            error: function () {
                toastr.error('Failed to fetch training info for editing.');
            }
        });
    });
    //#endregion

    //#region Submit edit form

    $('#trainingSubmitBtn').on('click', function (e) {
        e.preventDefault();
        SubmitTrainingData();
    });

    function SubmitTrainingData() {
        var formData = new FormData();

        formData.append('EmployeeTranningInfoID', $("#trnEmployeeTranningInfoID").val() || 0);
        formData.append('EmployeePersonalId', $("#trnEmployeePersonalId").val());
        formData.append('TranningTitle', $('#trnTranningTitle').val());
        formData.append('TopicCovered', $('#trnTopicCovered').val());
        formData.append('InstituteName', $('#trnInstituteName').val());
        formData.append('LocationName', $('#trnLocationName').val());
        formData.append('YearDuration', $('#trnYearDuration').val());
        formData.append('CountryID', choiceManager.getChoiceValue('trnCountryID'));
        formData.append('TrainingYearID', choiceManager.getChoiceValue('trnTrainingYearID'));

        $.ajax({
            url: '/EmployeeTraining/SubmitFromEdit',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Training info saved successfully');
                    PopulateTrainingData(response.data);
                    ClearTrainingForm();
                } else {
                    toastr.warning(response.message || 'Something went wrong');
                }
            },
            error: function () {
                toastr.error('Failed to save training info');
            }
        });
    }
    //#endregion

    //#region Delete Training

    let deleteTrainingId = 0;

    $(document).on('click', '.deleteTrainingBtn', function () {
        deleteTrainingId = $(this).data('id');
        $('#confirmDeleteModalTrn').modal('show');
    });

    $('#confirmDeleteBtnTrn').on('click', function () {
        $.ajax({
            url: '/EmployeeTraining/DeleteFromEdit',
            type: 'POST',
            data: { id: deleteTrainingId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Training info deleted successfully.');
                    PopulateTrainingData(response.data);
                    $('#confirmDeleteModalTrn').modal('hide');
                    ClearTrainingForm();
                } else {
                    toastr.warning(response.message || 'Training info not deleted.');
                }
            },
            error: function () {
                toastr.error('Failed to delete training info.');
            }
        });
    });
    //#endregion

    //#region Clear Form
    function ClearTrainingForm() {
        $("#trnEmployeeTranningInfoID").val('');
        $('#trnTranningTitle').val('');
        $('#trnTopicCovered').val('');
        $('#trnInstituteName').val('');
        $('#trnLocationName').val('');
        $('#trnYearDuration').val('');

        choiceManager.clearChoice('trnCountryID');
        choiceManager.clearChoice('trnTrainingYearID');
    }
    //#endregion

    //#endregion


    //#region Contact info

    //#region Populate Table
    function PopulateContactData(employee) {
        console.log('Employee Contact data:', employee);

        const tbody = $('#employeeContactTable tbody');
        tbody.empty(); // Clear existing rows

        var chkConData = Array.isArray(employee) ? employee[0] : employee;

        if (chkConData.isActive) {
            if (employee && employee.length > 0) {
                employee.forEach(function (contact, index) {
                    var row = `
                <tr>
                    <td>${contact.contactName || ''}</td>
                    <td>${contact.relationship || ''}</td>
                    <td>${contact.contactNumber || ''}</td>
                    <td>${contact.contactEmail || ''}</td>
                    <td>
                        <a class="nav-item me-2 editContactBtn" data-id="${contact.employeeEmeContactID}"><i class="fas fa-edit text-black"></i></a>
                        <a class="nav-item me-2 deleteContactBtn" data-id="${contact.employeeEmeContactID}"><i class="far fa-trash-alt text-black"></i></a>
                    </td>
                </tr>
                `;
                        tbody.append(row);
                    });
                } else {
                    var emptyRow = `
                <tr>
                    <td colspan="5" class="text-center">No emergency contact records found.</td>
                </tr>
            `;
                tbody.append(emptyRow);
            }
        }
        
    }
    //#endregion

    //#region Edit Click to populate
    $(document).on('click', '.editContactBtn', function () {
        var id = $(this).data('id');
        $.ajax({
            url: '/EmployeeContact/GetEmployeeContactData',
            type: 'GET',
            data: { id: id },
            success: function (contact) {
                $("#emergencyEmployeeEmeContactID").val(contact.employeeEmeContactID);
                $("#emergencyEmployeePersonalId").val(contact.employeePersonalId);
                $('#emergencyContactName').val(contact.contactName);
                $('#emergencyRelationship').val(contact.relationship);
                $('#emergencyContactNumber').val(contact.contactNumber);
                $('#emergencyContactEmail').val(contact.contactEmail);
            },
            error: function () {
                toastr.error('Failed to fetch emergency contact info for editing.');
            }
        });
    });
    //#endregion

    //#region Submit edit form
    $('#emergencySubmitBtn').on('click', function (e) {
        e.preventDefault();
        SubmitContactData();
    });

    function SubmitContactData() {
        var formData = new FormData();
        formData.append('EmployeeEmeContactID', $("#emergencyEmployeeEmeContactID").val() || 0);
        formData.append('EmployeePersonalId', $("#emergencyEmployeePersonalId").val());
        formData.append('ContactName', $('#emergencyContactName').val());
        formData.append('Relationship', $('#emergencyRelationship').val());
        formData.append('ContactNumber', $('#emergencyContactNumber').val());
        formData.append('ContactEmail', $('#emergencyContactEmail').val());

        $.ajax({
            url: '/EmployeeContact/SubmitFromEdit',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Emergency contact info saved successfully');
                    PopulateContactData(response.data);
                    ClearContactForm();
                } else {
                    toastr.warning(response.message || 'Something went wrong');
                }
            },
            error: function () {
                toastr.error('Failed to save emergency contact info');
            }
        });
    }
    //#endregion

    //#region Delete Contact
    let deleteContactId = 0;

    $(document).on('click', '.deleteContactBtn', function () {
        deleteContactId = $(this).data('id');
        $('#confirmDeleteModalCon').modal('show');
    });

    $('#confirmDeleteBtnCon').on('click', function () {
        $.ajax({
            url: '/EmployeeContact/DeleteFromEdit',
            type: 'POST',
            data: { id: deleteContactId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Emergency contact info deleted successfully.');
                    PopulateContactData(response.data);
                    $('#confirmDeleteModalCon').modal('hide');
                    ClearContactForm();
                } else {
                    toastr.warning(response.message || 'Emergency contact info not deleted.');
                }
            },
            error: function () {
                toastr.error('Failed to delete emergency contact info.');
            }
        });
    });
    //#endregion

    //#region Clear Form
    function ClearContactForm() {
        $("#emergencyEmployeeEmeContactID").val('');
      //  $("#emergencyEmployeePersonalId").val('');
        $('#emergencyContactName').val('');
        $('#emergencyRelationship').val('');
        $('#emergencyContactNumber').val('');
        $('#emergencyContactEmail').val('');
    }
    //#endregion

    //#endregion


    //#region Family info

    //#region Populate Table
    function PopulateFamilyData(employee) {
        console.log('Employee Family data:', employee);

        const tbody = $("#employeeFamilyTable tbody");
        tbody.empty(); // Clear existing rows

        var chkFamData = Array.isArray(employee) ? employee[0] : employee;

        if (chkFamData.isActive) {
            if (employee && employee.length > 0) {
                employee.forEach(function (item, index) {
                    var row = `
                <tr>
                    <td>${item.fullName || ''}</td>
                    <td>${item.relationToEmployee || ''}</td>
                    <td>${item.occupation || ''}</td>
                    <td>${item.contactNumber || ''}</td>
                    <td>${item.email || ''}</td>
                    <td>${item.address || ''}</td>
                    <td>
                        <a class="nav-item me-2 editFamilyBtn" data-id="${item.employeeFamilyInfoID}"><i class="fas fa-edit text-black"></i></a>
                        <a class="nav-item me-2 deleteFamilyBtn" data-id="${item.employeeFamilyInfoID}"><i class="far fa-trash-alt text-black"></i></a>
                    </td>
                </tr>
            `;
                    tbody.append(row);
                });
            } else {
                var emptyRow = `
            <tr>
                <td colspan="7" class="text-center">No family records found.</td>
            </tr>
        `;
                tbody.append(emptyRow);
            }
        }

        
    }
    //#endregion

    //#region Edit Click to populate
    $(document).on('click', '.editFamilyBtn', function () {
        var id = $(this).data('id');
        $.ajax({
            url: '/EmployeeFamily/GetEmployeeFamilyData',
            type: 'GET',
            data: { id: id },
            success: function (family) {
                $("#familyEmployeeFamilyInfoID").val(family.employeeFamilyInfoID);
                $("#familyEmployeePersonalId").val(family.employeePersonalId);
                $('#familyFullName').val(family.fullName);
                $('#familyRelationToEmployee').val(family.relationToEmployee);
                $('#familyOccupation').val(family.occupation);
                $('#familyContactNumber').val(family.contactNumber);
                $('#familyEmail').val(family.email);
                $('#familyAddress').val(family.address);
            },
            error: function () {
                toastr.error('Failed to fetch family info for editing.');
            }
        });
    });
    //#endregion

    //#region Submit edit form
    $('#familySubmitBtn').on('click', function (e) {
        e.preventDefault();
        SubmitFamilyData();
    });

    function SubmitFamilyData() {
        var formData = new FormData();
        formData.append('EmployeeFamilyInfoID', $("#familyEmployeeFamilyInfoID").val() || 0);
        formData.append('EmployeePersonalId', $("#familyEmployeePersonalId").val());
        formData.append('FullName', $('#familyFullName').val());
        formData.append('RelationToEmployee', $('#familyRelationToEmployee').val());
        formData.append('Occupation', $('#familyOccupation').val());
        formData.append('ContactNumber', $('#familyContactNumber').val());
        formData.append('Email', $('#familyEmail').val());
        formData.append('Address', $('#familyAddress').val());

        $.ajax({
            url: '/EmployeeFamily/SubmitFromEdit',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Family info saved successfully');
                    PopulateFamilyData(response.data);
                    ClearFamilyForm();
                } else {
                    toastr.warning(response.message || 'Something went wrong');
                }
            },
            error: function () {
                toastr.error('Failed to save family info');
            }
        });
    }
    //#endregion

    //#region Delete Family
    let deleteFamilyId = 0;

    $(document).on('click', '.deleteFamilyBtn', function () {
        deleteFamilyId = $(this).data('id');
        $('#confirmDeleteModalFam').modal('show');
    });

    $('#confirmDeleteBtnFam').on('click', function () {
        $.ajax({
            url: '/EmployeeFamily/DeleteFromEdit',
            type: 'POST',
            data: { id: deleteFamilyId },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Family info deleted successfully.');
                    PopulateFamilyData(response.data);
                    $('#confirmDeleteModalFam').modal('hide');
                    ClearFamilyForm();
                } else {
                    toastr.warning(response.message || 'Family info not deleted.');
                }
            },
            error: function () {
                toastr.error('Failed to delete family info.');
            }
        });
    });
    //#endregion

    //#region Clear Form
    function ClearFamilyForm() {
        $("#familyEmployeeFamilyInfoID").val('');
      //  $("#familyEmployeePersonalId").val('');
        $('#familyFullName').val('');
        $('#familyRelationToEmployee').val('');
        $('#familyOccupation').val('');
        $('#familyContactNumber').val('');
        $('#familyEmail').val('');
        $('#familyAddress').val('');
    }
    //#endregion

    //#endregion

     

    //#region Salary info




    function PopulateSalaryData(employee) {
        console.log('Employee Salary data:', employee);

        if (!employee) return;


        $('#salaryBankName').val(employee.bankName || '');
        $('#salaryBranchName').val(employee.branchName || '');
        $('#salaryAddress').val(employee.address || '');
        $('#salaryAccountName').val(employee.accountName || '');
        $('#salaryAccountNo').val(employee.accountNo || '');
        $('#salaryATMCardNo').val(employee.atmCardNo || '');
        $('#salaryRoutingNo').val(employee.routingNo || '');
        $('#salarySWIFTCode').val(employee.swiftCode || '');
        $('#salaryIFSCCode').val(employee.ifscCode || '');
        $('#salarybKashAccountNo').val(employee.bKashAccountNo || '');
        $('#salaryRoketAccountNo').val(employee.roketAccountNo || '');
        $('#salaryNagodAccountNo').val(employee.nagodAccountNo || '');
        $('#salarySalary').val(employee.salary || '');


        choiceManager.setChoiceValue('salaryGradeID', employee.gradeID || '');
        choiceManager.setChoiceValue('salaryCurrencyID', employee.currencyID || '');
        choiceManager.setChoiceValue('salaryPaymenPeriodTypeID', employee.paymenPeriodTypeID || '');



        if (employee.paymentModeIds) {
            const paymentModes = Array.isArray(employee.paymentModeIds)
                ? employee.paymentModeIds
                : employee.paymentModeIds.split(',');
            $('#salaryPaymentModeIds').val(paymentModes).trigger('change');


            if (paymentModes.length > 1) {
                $('#multyPayment').show();
                $('#salaryPrimaryPaymentModeId').val(employee.primaryPaymentModeId || '').trigger('change');
                $('#salaryPrimaryPaymentPercent').val(employee.primaryPaymentPercent || '');
                $('#salarySecondaryPaymentModeId').val(employee.secondaryPaymentModeId || '').trigger('change');
            } else {
                $('#multyPayment').hide();
            }
        }


        $('.choiceDD').trigger('change');
    }


    //#endregion

    //#region Benifit info
    function PopulateBenefitData(employee) {
        console.log('Employee Benefit data:', employee);

        if (!employee) return;


        $('#benifitHealthInsurance').val(employee.healthInsurance || '');
        $('#benifitPerformanceBonus').val(employee.performanceBonus || '');


        $('#benifitIsHealthInsuranceEnabled').prop('checked', employee.isHealthInsuranceEnabled || false);
        $('#benifitIsPerformanceBonusEnabled').prop('checked', employee.isPerformanceBonusEnabled || false);
        $('#benifitIsYearlyEndBonusTypeIDEnabled').prop('checked', employee.isYearlyEndBonusTypeIDEnabled || false);
        $('#benifitIsFastivalBonusPercentageEnabled').prop('checked', employee.isFastivalBonusPercentageEnabled || false);
        $('#benifitIsProvidantFundEnabled').prop('checked', employee.isProvidantFundEnabled || false);
        $('#benifitIsBenifitEnabled').prop('checked', employee.isBenifitEnabled || false).trigger('change');

        choiceManager.setChoiceValue('benifitProvidantFundEmployeePercentage', employee.providantFundEmployeePercentage || '');
        choiceManager.setChoiceValue('benifitProvidantFundOrganizationPercentage', employee.providantFundOrganizationPercentage || '');
        choiceManager.setChoiceValue('benifitServiceYearID', employee.serviceYearID || '');
        choiceManager.setChoiceValue('benifitFastivalBonusPercentage', employee.fastivalBonusPercentage || '');
        choiceManager.setChoiceValue('benifitYearlyEndBonusTypeID', employee.yearlyEndBonusTypeID || '');



        toggleBenefitFields();
    }

    function toggleBenefitFields() {
        const healthEnabled = $('#benifitIsHealthInsuranceEnabled').is(':checked');
        $('#benifitHealthInsurance').prop('disabled', !healthEnabled);

        const performanceEnabled = $('#benifitIsPerformanceBonusEnabled').is(':checked');
        $('#benifitPerformanceBonus').prop('disabled', !performanceEnabled);

        const yearlyBonusEnabled = $('#benifitIsYearlyEndBonusTypeIDEnabled').is(':checked');
        $('#benifitYearlyEndBonusTypeID').prop('disabled', !yearlyBonusEnabled);

        const festivalEnabled = $('#benifitIsFastivalBonusPercentageEnabled').is(':checked');
        $('#benifitFastivalBonusPercentage').prop('disabled', !festivalEnabled);

        const pfEnabled = $('#benifitIsProvidantFundEnabled').is(':checked');
        $('#benifitProvidantFundEmployeePercentage').prop('disabled', !pfEnabled);
        $('#benifitProvidantFundOrganizationPercentage').prop('disabled', !pfEnabled);
        $('#benifitServiceYearID').prop('disabled', !pfEnabled);
    }



    //#endregion

    //#region Allow info
    function PopulateAllowanceData(employee) {
        console.log('Employee Allowance data:', employee);

        if (!employee) return;

        $('#allowMobileAllowance').val(employee.mobileAllowance || '');
        $('#allowInternetAllowance').val(employee.internetAllowance || '');

        if (employee.mobileAllowanceEffectiveFromStr) {
            $('#allowMobileAllowanceEffectiveFromStr').val(employee.mobileAllowanceEffectiveFromStr);
        }

        if (employee.internetAllowanceEffectiveFromStr) {
            $('#allowInternetAllowanceEffectiveFromStr').val(employee.internetAllowanceEffectiveFromStr);
        }


        $('#allowIsMobileInternetAllowanceEnabled').prop('checked', employee.isMobileAllowanceEnabled || false);
        $('#allowIsInternetAllowanceEnabled').prop('checked', employee.isInternetAllowanceEnabled || false);
        $('#allowIsHouseRentAllowancePercentageEnabled').prop('checked', employee.isHouseRentAllowancePercentageEnabled || false);
        $('#allowIsMedicalAllowancePercentageEnabled').prop('checked', employee.isMedicalAllowancePercentageEnabled || false);
        $('#allowIsConveyanceAllowancePercentageEnabled').prop('checked', employee.isConveyanceAllowancePercentageEnabled || false);

        $('#allowIsEmployeeAllowanceEnabled').prop('checked', employee.isEmployeeAllowanceEnabled || false).trigger('change');


        choiceManager.setChoiceValue('allowHouseRentAllowancePercentage', employee.houseRentAllowancePercentage || '');
        choiceManager.setChoiceValue('allowMedicalAllowancePercentage', employee.medicalAllowancePercentage || '');
        choiceManager.setChoiceValue('allowConveyanceAllowancePercentage', employee.conveyanceAllowancePercentage || '');



        toggleAllowanceFields();
    }


    function toggleAllowanceFields() {
        const mobileEnabled = $('#allowIsMobileInternetAllowanceEnabled').is(':checked');
        $('#allowMobileAllowance').prop('disabled', !mobileEnabled);
        $('#allowMobileAllowanceEffectiveFromStr').prop('disabled', !mobileEnabled);

        const internetEnabled = $('#allowIsInternetAllowanceEnabled').is(':checked');
        $('#allowInternetAllowance').prop('disabled', !internetEnabled);
        $('#allowInternetAllowanceEffectiveFromStr').prop('disabled', !internetEnabled);

        const houseRentEnabled = $('#allowIsHouseRentAllowancePercentageEnabled').is(':checked');
        $('#allowHouseRentAllowancePercentage').prop('disabled', !houseRentEnabled);

        const medicalEnabled = $('#allowIsMedicalAllowancePercentageEnabled').is(':checked');
        $('#allowMedicalAllowancePercentage').prop('disabled', !medicalEnabled);

        const conveyanceEnabled = $('#allowIsConveyanceAllowancePercentageEnabled').is(':checked');
        $('#allowConveyanceAllowancePercentage').prop('disabled', !conveyanceEnabled);
    }


    //#endregion

    

    $('#benifitIsHealthInsuranceEnabled').on('change', toggleBenefitFields);
    $('#benifitIsPerformanceBonusEnabled').on('change', toggleBenefitFields);
    $('#benifitIsYearlyEndBonusTypeIDEnabled').on('change', toggleBenefitFields);
    $('#benifitIsFastivalBonusPercentageEnabled').on('change', toggleBenefitFields);
    $('#benifitIsProvidantFundEnabled').on('change', toggleBenefitFields);

    // Allowance toggles
    $('#allowIsMobileInternetAllowanceEnabled').on('change', toggleAllowanceFields);
    $('#allowIsInternetAllowanceEnabled').on('change', toggleAllowanceFields);
    $('#allowIsHouseRentAllowancePercentageEnabled').on('change', toggleAllowanceFields);
    $('#allowIsMedicalAllowancePercentageEnabled').on('change', toggleAllowanceFields);
    $('#allowIsConveyanceAllowancePercentageEnabled').on('change', toggleAllowanceFields);

       


    //#endregion

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