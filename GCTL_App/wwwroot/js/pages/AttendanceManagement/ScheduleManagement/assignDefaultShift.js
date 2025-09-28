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
            
            


            // #region Save
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();
                $(settings.saveBtn).prop('disabled', true);

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
            // #endregion


            // #region assignDefaultShift-confirm-modal
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
            // #endregion


            // #region postDefaultShift
            function postDefaultShift(url, formData) {
                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    beforeSend: function () {
                        showLoadingIndicator();
                    },
                    success: function (response) {
                        if (response.isSuccess === true) {
                            clear();
                            toastr.success(response.message);
                            $(settings.saveBtn).prop('disabled', false);
                        } else {
                            const allFields = ["OrganizationID", "ShiftID"];

                            allFields.forEach(function (fieldId) {
                                validateField(fieldId, response);
                            });
                            toastr.info(response.message);
                        }
                    },
                    error: function (err) {
                        console.error('Save failed:', err);
                    },
                    complete: function () {
                        hideLoadingIndicator();
                    }
                });
            }
            // #endregion


            // #region populateConflictModal
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
            // #endregion


            let suppressDepartmentChange = false;
            // #region On click edit button GetByIdAsync
            $(document).on('click', settings.editBtn, async function (e) {
                e.preventDefault();
                const id = $(this).data('id');

                try {
                    const response = await $.get(getByIdUrl, { id });

                    if (response.isSuccess) {
                        const data = response.data;

                        $('#DefaultShiftID').val(data.defaultShiftID);

                        organizationDD.setChoiceByValue(data.organizationID.toString());

                        suppressDepartmentChange = true; // Disable event temporarily

                        await getDepartmentByOrganization(data.organizationID);
                        $('#DepartmentIDs').val(data.departmentID).each(function () {
                            coreui.MultiSelect.getInstance(this)?.update();
                        });
                        suppressDepartmentChange = false; // Re-enable event

                        await getEmployeesByOrgBraDepId(data.organizationID, [], data.departmentID);
                        $('#EmployeeIDs').val(data.employeeID).each(function () {
                            coreui.MultiSelect.getInstance(this)?.update();
                        });

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
                    suppressDepartmentChange = false; // Always reset on error too
                    console.error("Edit load failed:", error);
                }
            });
            // #endregion


            // #region clear/reset
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();
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

                if (organizationDD) {
                    organizationDD.destroy();
                }

                if (choiceShift) {
                    choiceShift.destroy();
                }

                selectSingleOrg();
                initOrganizationDD();
                initChoices();

                loadTableData();
                toggleBulkActions();
                $('#assignDefaultShift-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            // #region OrganizationID on change
            $('#OrganizationID').on('change', function (e) {
                e.preventDefault();

                var organizationId = $(this).val();
                getBranchesByOrgId(organizationId);
                getDepartmentByOrganization(organizationId);

                // Clear selected employees
                //const employeeSelect = document.getElementById('EmployeeIDs');
                //if (employeeSelect) {
                //    [...employeeSelect.options].forEach(o => o.selected = false);
                //    const keep = new Set([...employeeSelect.options].filter(o => o.selected).map(o => o.value));
                //    [...employeeSelect.options].forEach(o => {
                //        if (!keep.has(o.value)) o.remove();
                //    });

                //    const ms = coreui.MultiSelect.getInstance(employeeSelect);
                //    if (ms) ms.update();
                //}
                const employeeSelect = document.getElementById('EmployeeIDs');
                if (employeeSelect) {
                    // Fully clear previous options and groups
                    while (employeeSelect.firstChild) {
                        employeeSelect.removeChild(employeeSelect.firstChild);
                    }

                    // Reset CoreUI MultiSelect
                    const ms = coreui.MultiSelect.getInstance(employeeSelect);
                    if (ms) ms.update();
                }


                getEmployeesByOrgBraDepId(organizationId, [], []);
                loadShiftsByCompany(organizationId);
            });
            // #endregion


            // #region getBranchesByOrgId
            function getBranchesByOrgId(organizationId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/AssignDefaultShift/GetBranchesByOrgId',
                        type: 'GET',
                        traditional: true,
                        data: { orgId: organizationId },
                        success: function (branches) {
                            const select = $('#BranchIDs');
                            select.empty();

                            if (branches && branches.length > 0) {
                                branches.forEach(branch => {
                                    select.append(
                                        $('<option>', {
                                            value: branch.id,
                                            text: branch.name
                                        })
                                    );
                                });
                            }

                            // CoreUI MultiSelect reinitialization/refresh
                            const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                            if (multiSelectInstance) {
                                multiSelectInstance.update(); // Refresh UI to reflect new options
                            } else {
                                new coreui.MultiSelect(select[0]); // First time init
                            }

                            resolve();
                        },
                        error: function (xhr, status, error) {
                            console.error("Error loading branches:", error);
                            reject(error);
                        }
                    });
                });
            }
            // #endregion


            // #region getDepartmentByOrganization
            function getDepartmentByOrganization(organizationId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/AssignDefaultShift/GetDepartmentByOrganization',
                        type: 'GET',
                        traditional: true,
                        data: { id: organizationId },
                        success: function (departments) {
                            var select = $('#DepartmentIDs');
                            select.empty();

                            const grouped = {};

                            departments.forEach(dep => {
                                const group = dep.groupName || 'No Group';
                                if (!grouped[group]) {
                                    grouped[group] = [];
                                }
                                grouped[group].push(dep);
                            });

                            Object.keys(grouped).forEach(group => {
                                const optgroup = $('<optgroup>').attr('label', group);
                                grouped[group].forEach(dep => {
                                    optgroup.append(
                                        $('<option>').val(dep.id).text(dep.name)
                                    );
                                });
                                select.append(optgroup);
                            });

                            // Get the CoreUI multiselect instance
                            const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                            if (multiSelectInstance) {
                                multiSelectInstance.update(); // Refresh the UI
                            } else {
                                // Reinitialize if not already initialized (in case it's dynamically added)
                                new coreui.MultiSelect(select[0]);
                            }
                            resolve();
                        },
                        error: function (xhr, status, error) {
                            console.error('Error loading departments:', error);
                            reject(error);
                        }
                    });
                })
            }
            // #endregion


            // #region GetEmployeesByOrgBraDepId
            //function getEmployeesByOrgBraDepId(orgId, branchIds = [], depIds = []) {
            //    return new Promise((resolve, reject) => {
            //        $.ajax({
            //            url: '/AssignDefaultShift/GetEmployeesByOrgBraDepId',
            //            type: 'GET',
            //            traditional: true,
            //            data: {
            //                orgId: orgId,
            //                branchIds: branchIds,
            //                depIds: depIds
            //            },
            //            success: function (employees) {
            //                const select = $('#EmployeeIDs');
            //                select.empty();

            //                const grouped = {};

            //                // Group employees by GroupName (DepartmentName)
            //                employees.forEach(emp => {
            //                    const group = emp.groupName || 'No Department';
            //                    if (!grouped[group]) {
            //                        grouped[group] = [];
            //                    }
            //                    grouped[group].push(emp);
            //                });

            //                // Build <optgroup> structure
            //                Object.keys(grouped).forEach(group => {
            //                    const optgroup = $('<optgroup>').attr('label', group);
            //                    grouped[group].forEach(emp => {
            //                        optgroup.append(
            //                            $('<option>').val(emp.id).text(emp.name)
            //                        );
            //                    });
            //                    select.append(optgroup);
            //                });

            //                const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

            //                if (multiSelectInstance) {
            //                    multiSelectInstance.update(); // Refresh UI
            //                } else {
            //                    new coreui.MultiSelect(select[0]); // Init CoreUI multiselect
            //                }
            //                resolve();
            //            },
            //            error: function (xhr, status, error) {
            //                console.error('Error loading employees:', error);
            //                reject (error);
            //            }
            //        });
            //    });
            //}

            //async function getEmployeesByOrgBraDepId(orgId, branchIds = [], depIds = []) {
            //    try {
            //        const url = new URL('/AssignDefaultShift/GetEmployeesByOrgBraDepId', window.location.origin);
            //        const params = new URLSearchParams();

            //        if (orgId) params.append('orgId', orgId);
            //        branchIds.forEach(id => params.append('branchIds', id));
            //        depIds.forEach(id => params.append('depIds', id));

            //        url.search = params.toString();

            //        const response = await fetch(url, {
            //            method: 'GET'
            //        });

            //        if (!response.ok) {
            //            throw new Error(`HTTP error! Status: ${response.status}`);
            //        }

            //        const employees = await response.json();

            //        const select = document.getElementById('EmployeeIDs');
            //        select.innerHTML = '';

            //        const grouped = {};

            //        // Group employees by GroupName (DepartmentName)
            //        employees.forEach(emp => {
            //            const group = emp.groupName || 'No Department';
            //            if (!grouped[group]) {
            //                grouped[group] = [];
            //            }
            //            grouped[group].push(emp);
            //        });

            //        // Build <optgroup> structure
            //        Object.keys(grouped).forEach(group => {
            //            const optgroup = document.createElement('optgroup');
            //            optgroup.label = group;

            //            grouped[group].forEach(emp => {
            //                const option = document.createElement('option');
            //                option.value = emp.id;
            //                option.textContent = emp.name;
            //                optgroup.appendChild(option);
            //            });

            //            select.appendChild(optgroup);
            //        });

            //        const multiSelectInstance = coreui.MultiSelect.getInstance(select);

            //        if (multiSelectInstance) {
            //            multiSelectInstance.update();
            //        } else {
            //            new coreui.MultiSelect(select);
            //        }

            //    } catch (error) {
            //        console.error('Error loading employees:', error);
            //    }
            //}
            // #endregion


            // #region DepartmentIDs on change  
            document.getElementById('DepartmentIDs').addEventListener('changed.coreui.multi-select', function () {
                if (suppressDepartmentChange) return;

                const orgId = $('#OrganizationID').val();
                const selectedOptions = $('#DepartmentIDs').val() || [];
                const depIds = selectedOptions.map(x => parseInt(x));

                getEmployeesByOrgBraDepId(orgId, [], depIds);
            });
            // #endregion


            // #region loadShiftsByCompany
            function loadShiftsByCompany(organizationId, selectedShiftId = null) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/AssignDefaultShift/GetShiftByCompany',
                        type: 'GET',
                        data: { id: organizationId },
                        success: function (shifts) {
                            if (!choiceShift) return resolve();

                            const shiftChoices = shifts.map(shift => ({
                                value: shift.id.toString(),
                                label: shift.name,
                                selected: shift.id.toString() === selectedShiftId?.toString()
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
            // #endregion


            // #region Dropdowns
            function initOrganizationDD() {
                organizationDD = new Choices('#OrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Organization...'
                });
            }
            document.addEventListener('DOMContentLoaded', initOrganizationDD);
            initOrganizationDD();


            function initChoices() {
                choiceShift = new Choices('#ShiftID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Shift...'
                });
            }
            document.addEventListener('DOMContentLoaded', initChoices);
            initChoices();


            function selectSingleOrg() {
                var $select = $('#OrganizationID');

                // Count the number of options excluding the placeholder (empty value)
                var $realOptions = $select.find('option').filter(function () {
                    return $(this).val() !== '';
                });

                if ($realOptions.length === 1) {
                    $realOptions.prop('selected', true);
                    $select.trigger('change');
                }
            }
            // #endregion


            // #region toggleBulkActions
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
            // #endregion


            // #region getEmployeesByOrgBraDepId (with pagination, search, scroll)
            function getEmployeesByOrgBraDepId(orgId, branchIds = [], depIds = []) {
                const selectEl = document.getElementById('EmployeeIDs');
                if (!selectEl) return;

                const ms = coreui.MultiSelect.getOrCreateInstance(selectEl);

                let page = 1;
                let term = '';
                const pageSize = 50;

                let hasMore = true;
                let loading = false;
                let debounce;
                let scrollPosition = 0;

                // Initial fetch trigger on open
                selectEl.addEventListener('shown.coreui.multi-select', () => {
                    if (selectEl.options.length === 0) {
                        page = 1;
                        term = '';
                        hasMore = true;
                        fetchPage({ append: false });
                    }

                    ensureSearchHandler();
                    rebindScroll();
                });

                function ensureSearchHandler() {
                    const wrapper = selectEl.nextElementSibling;
                    const input = wrapper?.querySelector('.form-multi-select-search');
                    const box = wrapper?.querySelector('.form-multi-select-options');
                    if (!input || input.dataset.listenerAttached) return;

                    input.dataset.listenerAttached = '1';
                    input.addEventListener('mousedown', e => e.stopPropagation());

                    input.addEventListener('input', e => {
                        const val = e.target.value.trim();
                        clearTimeout(debounce);

                        if (val.length < 3) {
                            addOptions([], { reset: true });
                            page = 1;
                            term = '';
                            hasMore = false;
                            if (box) box.scrollTop = 0;
                            return;
                        }

                        debounce = setTimeout(() => {
                            term = val;
                            page = 1;
                            hasMore = true;
                            fetchPage({ append: false });
                            if (box) box.scrollTop = 0;
                        }, 1000);
                    });
                }

                function rebindScroll() {
                    const box = selectEl.nextElementSibling?.querySelector('.form-multi-select-options');
                    if (!box || box.dataset.infiniteAttached) return;

                    box.dataset.infiniteAttached = '1';
                    box.addEventListener('scroll', () => {
                        if (box.scrollTop + box.clientHeight >= box.scrollHeight - 10) {
                            if (hasMore && !loading) fetchPage({ append: true });
                        }
                    });
                }

                function addOptions(items, { reset = false } = {}) {
                    const box = selectEl.nextElementSibling?.querySelector('.form-multi-select-options');
                    if (box) scrollPosition = box.scrollTop;

                    if (reset) {
                        const keep = new Set([...selectEl.options].filter(o => o.selected).map(o => o.value));
                        [...selectEl.options].forEach(o => {
                            if (!keep.has(o.value)) o.remove();
                        });
                    }

                    const existing = new Set([...selectEl.options].map(o => String(o.value)));
                    const grouped = {};

                    items.forEach(emp => {
                        const group = emp.group || 'No Department';
                        if (!grouped[group]) grouped[group] = [];
                        grouped[group].push(emp);
                    });

                    Object.keys(grouped).forEach(group => {
                        const optgroup = document.createElement('optgroup');
                        optgroup.label = group;

                        grouped[group].forEach(emp => {
                            const idStr = String(emp.value);
                            if (existing.has(idStr)) return;
                            const opt = document.createElement('option');
                            opt.value = idStr;
                            opt.textContent = emp.label;
                            optgroup.appendChild(opt);
                        });

                        selectEl.appendChild(optgroup);
                    });

                    const wasOpen = !!ms._isShown;
                    ms.update();
                    if (wasOpen) ms.show();
                    ensureSearchHandler();

                    const input = selectEl.nextElementSibling?.querySelector('.form-multi-select-search');
                    if (input && input.value) {
                        input.setSelectionRange(input.value.length, input.value.length);
                    }

                    setTimeout(() => {
                        const box = selectEl.nextElementSibling?.querySelector('.form-multi-select-options');
                        if (box) {
                            box.scrollTop = reset ? 0 : scrollPosition;
                        }
                    }, 10);

                    rebindScroll();
                }

                async function fetchPage({ append }) {
                    if (loading || (!hasMore && append)) return;
                    loading = true;
                    try {
                        const url = new URL('/AssignDefaultShift/GetEmployeesByOrgBraDepId', window.location.origin);
                        const params = new URLSearchParams();

                        if (orgId) params.append('orgId', orgId);
                        branchIds.forEach(id => params.append('branchIds', id));
                        depIds.forEach(id => params.append('depIds', id));
                        if (term) params.append('search', term);
                        params.append('page', page);
                        params.append('pageSize', pageSize);

                        url.search = params.toString();

                        const response = await fetch(url);
                        if (!response.ok) throw new Error(`HTTP ${response.status}`);

                        const data = await response.json();

                        addOptions(data.items, { reset: !append });
                        hasMore = !!data.hasMore;
                        if (append) page++;
                        else page = 2;
                    } catch (e) {
                        console.error('Error fetching employees:', e);
                    } finally {
                        loading = false;
                    }
                }
            }
            // #endregion




        });


        // #region Table
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
                                        <h5>${item.organizationName ?? '-'}</h5>
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0 fw-semibold text-body py-1">
                                        <span>${item.departmentName ?? '-'}</span>
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
        // #endregion
    }
}(jQuery));