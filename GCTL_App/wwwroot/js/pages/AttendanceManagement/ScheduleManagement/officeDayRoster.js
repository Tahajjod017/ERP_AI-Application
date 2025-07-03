(function ($) {
    $.officedayroster = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#rosterInOfficeDays-form',
            saveBtn: '#rosterInOfficeDays-saveBtn',
            resetBtn: '#rosterInOfficeDays-resetBtn',
        }, options);

        var getAll = settings.baseUrl + "/GetAll";
        var getAllPaging = settings.baseUrl + "/GetAllPaging";
        var createUrl = settings.baseUrl + "/Create";

        let organizationDD;
        let shiftDD;
        $(() => {



            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                const token = $('#rosterInOfficeDays-form input[name="__RequestVerificationToken"]').val();

                const formData = {
                    __RequestVerificationToken: token,
                    RosterInOfficeDayID: $('#RosterInOfficeDayID').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    DepartmentIDs: $('#DepartmentIDs').val(),
                    EmployeeIDs: $('#EmployeeIDs').val(),
                    ShiftID: $('#ShiftID').val(),
                    StartDate: $('#StartDate').val(),
                    EndDate: $('#EndDate').val()
                };

                const id = $('#RosterInOfficeDayID').val();
                const isEdit = id > 0;
                const url = isEdit ? updateEmpShift : createUrl;

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.success(response.message);
                        } else {
                            toastr.info(response.message);
                        }
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                    }
                });
            });
            
            

            $('#rosterInOfficeDays-resetBtn').on('click', function (e) {
                e.preventDefault();
                clear();
            });

            function clear() {
                $('#rosterInOfficeDays-form')[0].reset();
                //$('#BankID').val('').trigger('change');

                const deptSelect = document.getElementById('DepartmentIDs');
                const deptInstance = coreui.MultiSelect.getInstance(deptSelect);
                if (deptInstance) {
                    deptInstance.deselectAll();
                }

                const empSelect = document.getElementById('EmployeeIDs');
                const empInstance = coreui.MultiSelect.getInstance(empSelect);
                if (empInstance) {
                    empInstance.deselectAll();
                }


                if (organizationDD) {
                    organizationDD.destroy();
                }

                if (shiftDD) {
                    shiftDD.destroy();
                }

                initOrganizationDD();
                initShiftDD();
            }

            
            function initOrganizationDD() {
                organizationDD = new Choices('#OrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Organization...'
                });
            }
            document.addEventListener('DOMContentLoaded', initOrganizationDD);
            initOrganizationDD();

            
            function initShiftDD() {
                shiftDD = new Choices('#ShiftID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Shift...'
                });
            }
            document.addEventListener('DOMContentLoaded', initShiftDD);
            initShiftDD();

            

            const departmentIds = document.getElementById('DepartmentIDs')
            departmentIds.addEventListener('changed.coreui.multi-select', event => {
                debugger
                // Get the list of selected options.
                const selected = event.value

                
            });


            $('#OrganizationID').on('change', function (e) {
                e.preventDefault();

                var organizationId = $(this).val();  
                loadDepartmentsByCompany(organizationId);
                loadEmpByOrg(organizationId);
                loadShiftByOrg(organizationId);
            });




            function loadDepartmentsByCompany(organizationId) {
                $.ajax({
                    url: '/OfficeDayRoster/GetDepartmentByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (departments) {
                        recreateDepartmentDropdown(departments);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading departments:', error);
                    }
                });
            }


            function loadEmpByOrg(organizationId) {
                $.ajax({
                    url: '/OfficeDayRoster/GetEmployeeByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (emp) {
                        recreateEmpDD(emp);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees: ', error);
                    }
                });
            }




            function loadShiftByOrg(organizationId, selectedShiftId = null) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/OfficeDayRoster/GetShiftByOrganization',
                        type: 'GET',
                        data: { id: organizationId },
                        success: function (shifts) {
                            if (!shiftDD) return resolve();

                            const shiftChoices = shifts.map(shift => ({
                                value: shift.shiftID.toString(),
                                label: shift.shiftName,
                                selected: shift.shiftID.toString() === selectedShiftId?.toString()
                            }));

                            shiftDD.setChoices([
                                { value: '', label: 'Select Shift...', disabled: true, selected: !selectedShiftId },
                                ...shiftChoices
                            ], 'value', 'label', true);

                            resolve();
                        },
                        error: function (xhr, status, error) {
                            console.error('Error loading shifts:', error);
                            reject(error);
                        }
                    });
                });
            }





            function recreateDepartmentDropdown(departments) {
                const container = document.querySelector('.department'); // The div with class "two"
                const originalSelect = document.getElementById('DepartmentIDs');

                // Dispose existing MultiSelect instance
                const existingInstance = coreui.MultiSelect.getInstance(originalSelect);
                if (existingInstance) {
                    existingInstance.dispose();
                }

                // Store original attributes
                const originalAttributes = {
                    id: originalSelect.id,
                    name: originalSelect.name,
                    className: originalSelect.className,
                    multiple: originalSelect.multiple
                };

                // Remove the entire content and recreate
                container.innerHTML = `
                    <label class="form-label" asp-for="DepartmentIDs">Department Name</label>
                    <select class="form-multi-select" 
                            id="${originalAttributes.id}" 
                            name="${originalAttributes.name}" 
                            multiple 
                            data-coreui-multiple="true" 
                            data-coreui-selection-type="counter" 
                            data-coreui-search="true"
                            data-coreui-placeholder="Select Department...">
                    </select>
                `;

                // Get the new select element and populate it
                const newSelect = container.querySelector('select');

                if (!departments || departments.length === 0) {
                    const option = new Option('No departments found', '', false, false);
                    option.disabled = true;
                    newSelect.appendChild(option);
                } else {
                    departments.forEach(dep => {
                        const option = new Option(dep.departmentName, dep.departmentID, false, false);
                        newSelect.appendChild(option);
                    });
                }

                new coreui.MultiSelect(newSelect, {
                    multiple: true,
                    search: true,
                    selectionType: 'counter'
                });


                
                document.getElementById('DepartmentIDs')
                    .addEventListener('changed.coreui.multi-select', function (event) {
                        const orgId = $('#OrganizationID').val();

                        const selected = event.value || []; // array of {text, value}
                        const departmentIds = selected.map(x => parseInt(x.value));

                        loadEmployeesByFilter(orgId, departmentIds);
                    });
            }

            function loadEmployeesByFilter(organizationId, departmentIds = []) {
                console.log('departmentIds:', departmentIds);
                $.ajax({
                    url: '/OfficeDayRoster/GetEmployeeByDepartment',
                    type: 'GET',
                    traditional: true, 
                    data: {
                        orgId: organizationId,
                        depIds: departmentIds
                    },
                    success: function (data) {
                        recreateEmpDD(data);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees:', error);
                    }
                });
            }





            function recreateEmpDD(data, employeeIDs = []) {
                const container = document.querySelector('.employee');
                const originalSelect = document.getElementById('EmployeeIDs');

                const existing = coreui.MultiSelect.getInstance(originalSelect);
                if (existing) {
                    existing.dispose();
                }

                // Store original attributes
                const originalAttributes = {
                    id: originalSelect.id,
                    name: originalSelect.name,
                    className: originalSelect.className,
                    multiple: originalSelect.multiple
                };

                // Replace container HTML with a new label + select
                container.innerHTML = `
                    <label class="form-label" asp-for="EmployeeIDs">${container.querySelector('label').textContent}</label>
                    <select class="${originalAttributes.className}" 
                            id="${originalAttributes.id}" 
                            name="${originalAttributes.name}" 
                            multiple 
                            data-coreui-multiple="true" 
                            data-coreui-selection-type="counter" 
                            data-coreui-search="true"
                            data-coreui-placeholder="Select Employee...">
                    </select>
                `;

                // Get the new select element
                const newSelect = container.querySelector('select');

                // Group and populate employees
                if (!Array.isArray(data) || data.length === 0) {
                    const opt = new Option('No employees found', '', false, false);
                    opt.disabled = true;
                    newSelect.appendChild(opt);
                } else {
                    const grouped = {};

                    data.forEach(emp => {
                        const dept = emp.departmentName || 'No Department';
                        if (!grouped[dept]) grouped[dept] = [];
                        grouped[dept].push(emp);
                    });

                    Object.entries(grouped).forEach(([dept, employees]) => {
                        const optgroup = document.createElement('optgroup');
                        optgroup.label = dept;

                        employees.forEach(emp => {
                            const option = new Option(emp.employeeName, emp.employeeID, false, false);
                            if (employeeIDs.includes(emp.employeeID.toString())) {
                                option.selected = true;
                            }
                            optgroup.appendChild(option);
                        });

                        newSelect.appendChild(optgroup);
                    });
                }

                new coreui.MultiSelect(newSelect, {
                    multiple: true,
                    search: true,
                    selectionType: 'counter'
                });
            }







            $('#timeFrame').on('change', toggleTables);

            function toggleTables() {
                var selectedValue = $('#timeFrame').val();
                $('#day7, #day14').hide();
                $('#' + selectedValue).show();
            }
            toggleTables();





            //$(document).ready(function () {
            //    $('#basic-daterange').dateRangePicker({
            //        format: 'DD/MM/YYYY',
            //        separator: ' to ',
            //        language: 'en',
            //        autoClose: true,
            //        getValue: function () {
            //            return $(this).val();
            //        },
            //        setValue: function (s) {
            //            $(this).val(s);
            //        }
            //    }).bind('datepicker-change', function (event, obj) {
            //        const start = moment(obj.date1).format("YYYY-MM-DD");
            //        const end = moment(obj.date2).format("YYYY-MM-DD");
            //        $('#StartDate').val(start);
            //        $('#EndDate').val(end);
            //    });
            //});
        });





        

        var currentPage = 1;
        var pageSize = 5;
        let currentSortColumn = 'DefaultShiftID';
        let currentSortOrder = 'desc';

        // 🔁 On page size change
        $('#rosterInOfficeDays-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();
            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });

        // 🔁 On document ready
        $(document).ready(function () {
            loadTableData();

            $("#rosterInOfficeDays-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#rosterInOfficeDays-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#rosterInOfficeDays-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });

        // 🔁 Sorting
        $('th.sort').on('click', function () {
            const column = $(this).data('sort');
            if (currentSortColumn === column) {
                currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
            } else {
                currentSortColumn = column;
                currentSortOrder = 'asc';
            }

            loadTableData(currentSortColumn, currentSortOrder);
            updateSortingIndicator(column, currentSortOrder);
        });

        // 🔁 Sorting icon
        function updateSortingIndicator() {
            $('th.sort').each(function () {
                const $th = $(this);
                const column = $th.data('sort');
                $th.find('.sort-icon').remove();

                if (column === currentSortColumn) {
                    const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
                    $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
                } else {
                    $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
                }
            });
        }

        // 🔄 Trigger on time frame change
        $('#timeFrame').on('change', function () {
            const days = parseInt($(this).val(), 10);
            loadTableData(currentSortColumn, currentSortOrder, days);
        });

        // 🔁 Load data function
        function loadTableData(sortColumn = currentSortColumn, sortOrder = currentSortOrder, daysToShow = 7) {
            var searchTerm = $("#rosterInOfficeDays-searchInput").val();

            $.ajax({
                url: getAllPaging,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    daysToShow: daysToShow
                },
                success: function (response) {
                    const headers = response.headers;
                    const result = response.result;
                    const data = result.data;
                    const paginationInfo = result.paginationInfo;

                    // ✅ Build table headers
                    let headerRow = `<th class="align-middle text-uppercase text-nowrap">Employee Name</th>`;
                    headers.forEach(h => {
                        headerRow += `
                    <th class="align-middle px-3 text-uppercase text-nowrap">
                        <p class="weekDay">${h.day}</p>
                        <p class="date">${h.date}</p>
                    </th>`;
                    });
                    $('table thead tr').html(headerRow);

                    // ✅ Group data by EmployeeID
                    const grouped = {};
                    data.forEach(item => {
                        const empId = item.employeeID;
                        if (!grouped[empId]) {
                            grouped[empId] = {
                                name: item.employeeName,
                                designation: item.departmentName,
                                shifts: {}
                            };
                        }

                        const shiftDateKey = new Date(item.startDate).toISOString().split('T')[0];
                        grouped[empId].shifts[shiftDateKey] = {
                            timeRange: item.timeRange,
                            shiftName: item.shiftName
                        };
                    });

                    // ✅ Build tbody
                    let bodyHtml = '';
                    for (const empId in grouped) {
                        const emp = grouped[empId];
                        bodyHtml += `
                        <tr>
                            <td class="empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="ms-1">
                                        <h5>${emp.name}</h5>
                                        <p class="fs-9">${emp.designation}</p>
                                    </div>
                                </div>
                            </td>`;

                        headers.forEach(h => {
                            const dateKey = new Date(h.date).toISOString().split('T')[0];
                            const shift = emp.shifts[dateKey];

                            if (shift) {
                                bodyHtml += `
                            <td class="startTime">
                                <div class="badge badge-phoenix-primary shift-block px-4">
                                    <p class="fs-10">${shift.timeRange}</p>
                                    <p class="fs-10">${shift.shiftName}</p>
                                    <div class="add-shift-btn2">
                                        <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#editShiftModal">
                                            <i class="fas fa-edit text-success"></i>
                                        </a>
                                        <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                            <i class="fas fa-trash text-danger"></i>
                                        </a>
                                    </div>
                                </div>
                            </td>`;
                                } else {
                                    bodyHtml += `
                            <td class="shift-cell">
                                <button class="btn add-shift-btn" data-bs-toggle="modal" data-bs-target="#addShiftModal">
                                    <i class="fa fa-plus" aria-hidden="true"></i>
                                </button>
                            </td>`;
                                }
                        });

                        bodyHtml += `</tr>`;
                    }

                    $('table tbody').html(bodyHtml);

                    // ✅ Pagination info
                    $("#startItem").text(paginationInfo.startItem);
                    $("#endItem").text(paginationInfo.endItem);
                    $("#totalItems").text(paginationInfo.totalItems);

                    // ✅ Update pagination controls
                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function (err) {
                    console.error("Error loading data:", err);
                }
            });
        }


        // 🔁 Pagination logic
        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#rosterInOfficeDays-paginationLinks");
            paginationLinks.empty();
            const windowSize = 1;

            const createPageButton = (page) => `
        <li class="page-item ${page === currentPage ? 'active' : ''}">
            <button class="page-link page-btn" data-page="${page}">${page}</button>
        </li>`;

            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }

            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }

            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }

            $("#rosterInOfficeDays-prevPageBtn").prop('disabled', currentPage === 1);
            $("#rosterInOfficeDays-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        // 🔁 Page button click
        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData(currentSortColumn, currentSortOrder);
        });

    }
}(jQuery));