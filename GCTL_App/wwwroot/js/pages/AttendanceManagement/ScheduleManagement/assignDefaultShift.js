(function ($) {
    $.assigndefaultshift = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#assignDefaultShift-addForm',
            updateform: '#assignDefaultShift-addForm',
            saveBtn: '#assignDefaultShift-saveBtn',
            editBtn: '#assignDefaultShift-editBtn',
            resetBtn: '#assignDefaultShift-resetBtn',
            conModal: '#assignDefaultShift-confirm-modal',
            conMdlClsBtn: '#assignDefaultShift-closeBtn-confirm-modal',
            conMdlCnlBtn: '#assignDefaultShift-cancel-confirm-modal'
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + "/Create";
        var getByIdUrl = settings.baseUrl + '/GetById';
        var updateUrl = settings.baseUrl + "/Update";
        var updateEmpShift = settings.baseUrl + "/UpdateEmpShift";
        var checkConflictsUrl = settings.baseUrl + '/CheckConflicts';
        $(() => {

            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                const token = $('#assignDefaultShift-addForm input[name="__RequestVerificationToken"]').val();

                const formData = {
                    __RequestVerificationToken: token,
                    DefaultShiftID: $('#DefaultShiftID').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    DepartmentIDs: $('#DepartmentIDs').val(),
                    EmployeeIDs: $('#EmployeeIDs').val(),
                    ShiftID: $('#ShiftID').val()
                };

                const id = $('#DefaultShiftID').val();
                const isEdit = id > 0;
                const url = isEdit ? updateEmpShift : createUrl;

                $.ajax({
                    url: checkConflictsUrl,
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        const allFields = ["OrganizationID", "ShiftID"];

                        allFields.forEach(function (fieldId) {
                            validateField(fieldId, response);
                        });

                        if (response.hasConflicts) {
                            populateConflictModal(response.conflicts);
                            $('#assignDefaultShift-confirm-modal').modal('show');
                        } else {
                            postDefaultShift(url, formData);
                        }
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                    }
                });
            });

            $('#assignDefaultShift-confirm-modal .btn-primary').on('click', function () {
                const excludedIds = [];

                $('.conflict-checkbox:not(:checked)').each(function () {
                    const id = $(this).data('id');
                    const parsedId = Number(id);
                    if (!isNaN(parsedId)) {
                        excludedIds.push(parsedId);
                    }
                });

                const formData = {
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    ShiftID: $('#ShiftID').val(),
                    LIP: $('#LIP').val(),
                    LMAC: $('#LMAC').val(),
                    CreatedBy: $('#CreatedBy').val(),
                    ExcludedEmployeeIDs: excludedIds
                };

                postDefaultShift(createUrl, formData);
                $('#assignDefaultShift-confirm-modal').modal('hide');
            });

            function postDefaultShift(url, formData) {
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (data) {
                        if (data.isSuccess) {
                            clear();
                            toastr.success(data.message);
                        } else {
                            toastr.info(data.message);
                        }
                    },
                    error: function (err) {
                        console.error('Save failed:', err);
                    }
                });
            }

            function populateConflictModal(conflicts) {
                const tbody = $('#assignDefaultShift-confirm-modal tbody');
                tbody.empty();

                if (conflicts.length === 0) {
                    tbody.append(`<tr><td colspan="4" class="text-muted text-center">No conflicts to show.</td></tr>`);
                    return;
                }

                conflicts.forEach(item => {
                    tbody.append(`
                        <tr>
                            <td class="text-center text-middle align-middle">${item.defaultShiftID ?? '-'}</td>
                            <td class="text-start text-middle align-middle">${item.employeeID ?? '-'}</td>
                            <td class="text-start text-middle align-middle">${item.departmentID ?? '-'}</td>
                            <td class="text-start text-middle align-middle">${item.shiftID}</td>
                            <td class="text-center text-middle align-middle"><input type="checkbox" class="form-check-input conflict-checkbox" data-id="${item.employeeID}" /></td>
                        </tr>
                    `);
                });
            }



            
            $(document).on('click', settings.editBtn, async function (e) {
                e.preventDefault();
                const id = $(this).data('id');

                try {
                    const response = await $.get(getByIdUrl, { id });

                    if (response.isSuccess) {
                        const data = response.data;

                        setMultiSelectValues('OrganizationID', data.organizationID);

                        // ✅ Wait for department select to be updated
                        await loadDepartmentsByCompany(data.organizationID);

                        // ✅ Now set Department values
                        await setMultiSelectValues('DepartmentIDs', data.departmentID);

                        // ✅ Load employee and shift
                        const employeeIDs = Array.isArray(data.employeeID) ? data.employeeID : String(data.employeeID).split(',');
                        loadFilteredEmployees(employeeIDs);

                        await loadShiftsByCompany(data.organizationID, data.shiftID);
                        if (choiceShift) {
                            choiceShift.removeActiveItems();
                            let selectedIDs = Array.isArray(data.shiftID) ? data.shiftID : String(data.shiftID).split(',');
                            selectedIDs.forEach(id => choiceShift.setChoiceByValue(id.toString()));
                        }

                        $(settings.addform).find(settings.saveBtn).text('Update');
                        window.scrollTo({ top: 0, behavior: 'smooth' });

                    } else {
                        toastr.warning(response.message);
                    }

                } catch (error) {
                    console.error("Edit load failed:", error);
                }
            });



            function setMultiSelectValues(selectId, values) {
                return new Promise(resolve => {
                    const select = document.getElementById(selectId);
                    if (!select) return resolve();

                    const valueArray = Array.isArray(values) ? values.map(v => v.toString()) : [values.toString()];

                    for (const option of select.options) {
                        option.selected = valueArray.includes(option.value);
                    }

                    const multiSelect = coreui.MultiSelect.getInstance(select);
                    if (multiSelect) {
                        multiSelect.update();
                    }

                    // Small timeout to ensure UI is fully refreshed
                    setTimeout(() => resolve(), 50);
                });
            }




            $(settings.resetBtn).on('click', function () {
                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#DefaultShiftID').val('0');

                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#assignDefaultShift-check-all").prop('checked', false);
                $('.assignDefaultShift-selectItem').prop('checked', false);

                // Reset Choices value (must happen BEFORE destroying)
                if (choiceShift) {
                    choiceShift.setChoiceByValue(''); // Clears value
                    choiceShift.destroy(); // Destroys instance
                    choiceShift = null;
                }
                // Reinitialize Choices
                initChoices();

                loadTableData();
                toggleBulkActions();
                $('#assignDefaultShift-check-all').prop('checked', false).prop('indeterminate', false);
            }



            document.querySelector('.two').addEventListener('changed.coreui.multi-select', function (event) {
                const target = event.target;

                if (target.id === 'DepartmentIDs') {
                    loadFilteredEmployees();
                }
            });

            


            function loadFilteredEmployees(employeeIDs = []) {
                var deptIds = $('#DepartmentIDs').val() || [];

                if (!Array.isArray(deptIds)) deptIds = [deptIds];

                $.ajax({
                    url: '/AssignDefaultShift/GetEmployeeByDepartment',
                    type: 'GET',
                    data: {
                        departmentIds: deptIds.join(',')
                    },
                    success: function (data) {
                        updateEmployeeDropdown(data, employeeIDs);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees:', error);
                    }
                });
            }

            function updateEmployeeDropdown(data, employeeIDs = []) {
                var $empSelect = $('#EmployeeIDs');
                $empSelect.empty();

                if (!Array.isArray(data) || data.length === 0) {
                    $empSelect.append('<option disabled>No employees found</option>');
                    refreshCoreUIMultiSelect();
                    return;
                }

                // Group employees by department name
                const grouped = {};
                data.forEach(emp => {
                    const dept = emp.departmentName || 'No Department';
                    if (!grouped[dept]) grouped[dept] = [];
                    grouped[dept].push(emp);
                });

                // Append optgroups and options
                Object.entries(grouped).forEach(([dept, employees]) => {
                    const $optgroup = $('<optgroup>').attr('label', dept);
                    employees.forEach(emp => {
                        const $option = $('<option>')
                            .val(emp.employeeID)
                            .text(emp.employeeName);

                        // Pre-select if employeeID is in employeeIDs
                        if (employeeIDs.includes(emp.employeeID.toString())) {
                            $option.prop('selected', true);
                        }

                        $option.appendTo($optgroup);
                    });
                    $empSelect.append($optgroup);
                });

                // Refresh CoreUI multi-select
                refreshCoreUIMultiSelect();

                // Ensure CoreUI reflects the pre-selected values
                if (employeeIDs.length > 0) {
                    setMultiSelectValues('EmployeeIDs', employeeIDs);
                }
            }

            


            function refreshCoreUIMultiSelect() {
                const empSelect = document.getElementById('EmployeeIDs');

                // Dispose existing CoreUI MultiSelect instance
                const existingInstance = coreui.MultiSelect.getInstance(empSelect);
                if (existingInstance) {
                    existingInstance.dispose();
                }

                // Remove previously generated UI dropdown manually
                const generatedDropdown = empSelect.nextElementSibling;
                if (generatedDropdown && generatedDropdown.classList.contains('form-multi-select')) {
                    generatedDropdown.remove();
                }

                // Reinitialize CoreUI MultiSelect
                coreui.MultiSelect.getOrCreateInstance(empSelect);
            }




            $(document).ready(function () {
                $('#assignDefaultShift-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.assignDefaultShift-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.assignDefaultShift-selectItem', function () {
                    toggleBulkActions();
                });
            });




            function toggleBulkActions() {
                const allItems = $('.assignDefaultShift-selectItem');
                const checkedItems = $('.assignDefaultShift-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#assignDefaultShift-check-all').prop('checked', allChecked);
                $('#assignDefaultShift-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#assignDefaultShift-bulkSelectActions').removeClass('d-none');
                    $('#assignDefaultShift-searchBox').addClass('d-none');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkDelete').addClass('disabled');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkEdit').addClass('disabled');
                } else {
                    $('#assignDefaultShift-bulkSelectActions').addClass('d-none');
                    $('#assignDefaultShift-searchBox').removeClass('d-none');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkDelete').removeClass('disabled');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkEdit').removeClass('disabled');
                }
            }



            
            $('.one').on('changed.coreui.multi-select', function (event) {
                const target = event.target;

                if (target && target.id === 'OrganizationID') {
                    const selectedOrgId = $(target).val();
                    if (selectedOrgId) {
                        loadDepartmentsByCompany(selectedOrgId);
                        loadEmplooyeesByCompany(selectedOrgId);
                        loadShiftsByCompany(selectedOrgId);
                    } else {
                        clearDepartmentDropdown();
                    }
                }
            });




            function loadDepartmentsByCompany(organizationId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/AssignDefaultShift/GetDepartmentByCompany',
                        type: 'GET',
                        data: { id: organizationId },
                        success: function (departments) {
                            recreateDepartmentDropdown(departments);
                            //resolve(); 
                            setTimeout(() => resolve(), 100);
                        },
                        error: function (xhr, status, error) {
                            console.error('Error loading departments:', error);
                            reject(error);
                        }
                    });
                });
            }

            function recreateDepartmentDropdown(departments) {
                const container = document.querySelector('.two'); // The div with class "two"
                const originalSelect = document.getElementById('DepartmentIDs');

                // ✅ Step 1: Dispose existing MultiSelect instance
                const existingInstance = coreui.MultiSelect.getInstance(originalSelect);
                if (existingInstance) {
                    existingInstance.dispose();
                }

                // ✅ Step 2: Store original attributes
                const originalAttributes = {
                    id: originalSelect.id,
                    name: originalSelect.name,
                    className: originalSelect.className,
                    multiple: originalSelect.multiple
                };

                // ✅ Step 3: Remove the entire content and recreate
                container.innerHTML = `
                    <label class="form-label" for="DepartmentIDs">${container.querySelector('label').textContent}</label>
                    <select class="form-multi-select" 
                            id="${originalAttributes.id}" 
                            name="${originalAttributes.name}" 
                            multiple 
                            data-coreui-multiple="true" 
                            data-coreui-selection-type="counter" 
                            data-coreui-search="true">
                    </select>
                `;

                // ✅ Step 4: Get the new select element and populate it
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

                // ✅ Step 5: Initialize MultiSelect
                new coreui.MultiSelect(newSelect, {
                    multiple: true,
                    search: true,
                    selectionType: 'counter'
                });
            }
            function clearDepartmentDropdown() {
                recreateDepartmentDropdown([]);
            }




            function loadEmplooyeesByCompany(organizationId) {
                $.ajax({
                    url: '/AssignDefaultShift/GetEmployeeByCompany',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (data) {
                        updateEmployeeDropdown(data);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees:', error);
                    }
                });
            }




            function loadShiftsByCompany(organizationId, selectedShiftId = null) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/AssignDefaultShift/GetShiftByCompany',
                        type: 'GET',
                        data: { id: organizationId },
                        success: function (shifts) {
                            if (!choiceShift) return resolve();

                            const shiftChoices = shifts.map(shift => ({
                                value: shift.shiftID.toString(),
                                label: shift.shiftName,
                                selected: shift.shiftID.toString() === selectedShiftId?.toString()
                            }));

                            choiceShift.setChoices(shiftChoices, 'value', 'label', true);
                            resolve();
                        },
                        error: function (xhr, status, error) {
                            console.error('Error loading shifts:', error);
                            reject(error);
                        }
                    });
                });
            }




            let choiceShift;
            function initChoices() {
                choiceShift = new Choices('#ShiftID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Shift'
                });
            }
            initChoices();

                        
        });


        var currentPage = 1;
        var pageSize = 5;

        $('#assignDefaultShift-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#assignDefaultShift-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#assignDefaultShift-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#assignDefaultShift-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'DefaultShiftID';
        let currentSortOrder = 'desc';

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


        function loadTableData(sortColumn, sortOrder) {
            var searchTerm = $("#assignDefaultShift-searchInput").val();
            $.ajax({
                url: gridUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                },
                success: function (response) {
                    var tableBody = $("#assignDefaultShift-tBody");
                    tableBody.empty();
                    var totalItems = response.paginationInfo.totalItems;

                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            tableBody.append(`
                                <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input assignDefaultShift-selectItem" data-id="${item.defaultShiftID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-0 py-2">
                                        <h5>${item.organizationName}</h5>
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0 fw-semibold text-body py-1">
                                        <span>${item.departmentName}</span>
                                    </td>
                                    <td class="white-space-nowrap align-middle ps-0">${item.employeeName ?? '-'}</td>
                                    <td class="white-space-nowrap align-middle ps-0">${item.shiftName ?? '-'}</td>
                                    <td class="text-end align-middle white-space-nowrap pe-3">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon assignDefaultShift-bulkEdit" id="assignDefaultShift-editBtn" data-id="${item.defaultShiftID}"><i class="fas fa-edit text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="6" class="text-center">No data available</td></tr>');
                    }
                    //<td class="white-space-nowrap align-middle ps-0">
                    //    <div class="btn-reveal-trigger position-static">
                    //        <a href="#!" class="nav-item mx-2 assignDefaultShift-bulkEdit" id="assignDefaultShift-editBtn" data-id="${item.defaultShiftID}"><i class="fas fa-edit text-black"></i></a>
                    //        <a href="#!" class="nav-item mx-2 assignDefaultShift-bulkDelete" id="assignDefaultShift-singleDelBtn" data-id="${item.defaultShiftID}"><i class="far fa-trash-alt text-black"></i></a>
                    //    </div>
                    //</td>

                    var paginationInfo = response.paginationInfo;

                    $("#assignDefaultShift-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#assignDefaultShift-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#assignDefaultShift-paginationLinks");
            paginationLinks.empty();
            // Window size (number of pages before/after the current page)
            const windowSize = 1;

            const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;

            // Helper function for ellipsis
            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
            // Add "First Page" and ellipsis if needed
            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }
            // Add page number buttons within the window range
            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }
            // Add ellipsis and "Last Page" button if needed
            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }
            // Disable or enable previous/next buttons
            $("#assignDefaultShift-prevPageBtn").prop('disabled', currentPage === 1);
            $("#assignDefaultShift-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));