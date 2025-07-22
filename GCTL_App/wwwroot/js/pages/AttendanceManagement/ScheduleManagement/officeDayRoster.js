(function ($) {
    $.officedayroster = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#rosterInOfficeDays-form',
            saveBtn: '#rosterInOfficeDays-saveBtn',
            resetBtn: '#rosterInOfficeDays-resetBtn',
            delBtn: '#rosterInOfficeDays-deleteBtn',
            delModal: '#rosterInOfficeDays-delModal',
            modalDelBtn: '#rosterInOfficeDays-delModal-delBtn',
            addShiftModal: '#addShiftModal',
            editShiftModal: '#rosterInOfficeDays-editShiftModal',
            editShiftSaveBtn: '#EditShiftModal-saveShift',
        }, options);

        var getAll = settings.baseUrl + "/GetAll";
        var getAllSp = settings.baseUrl + "/GetAllFromStoredProc";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var updateEmpShift = settings.baseUrl + "/UpdateEmpShiftAsync";
        var deleteUrl = settings.baseUrl + "/Delete";
        let organizationDD;
        let shiftDD;
        let selectedId = null;
        let selectedDate = null;
        $(() => {

            

            // #region Save
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
                const url = isEdit ? updateUrl : createUrl;

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        const allFields = ["OrganizationID", "ShiftID", "StartDate", "EndDate"];

                        allFields.forEach(function (fieldId) {
                            validateField(fieldId, response);
                        });

                        if (response.isSuccess) {
                            toastr.success(response.message);
                            loadTableData();
                            clear();
                        } else {
                            toastr.info(response.message);
                        }
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                    }
                });
            });
            // #endregion


                        
            // #region Edit
            $(settings.editShiftSaveBtn).on('click', function (e) {
                e.preventDefault();
                const $modal = $(settings.editShiftModal);
                const token = $('#EditShiftModal-form input[name="__RequestVerificationToken"]').val();

                const rosterInOfficeDayID = $modal.data('roster-id');
                const overrideDate = $modal.data('override-date');

                const formData = {
                    __RequestVerificationToken: token,
                    RosterInOfficeDayID: rosterInOfficeDayID,
                    OverrideDate: overrideDate,
                    ShiftID: $('#EditShiftModal-ShiftID').val(),
                };

                if (rosterInOfficeDayID == 0 || rosterInOfficeDayID == null) return toastr.info('Something went wrong!');

                const url = '/OfficeDayRosterRoute/UpdateEmpShiftAsync';

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.success(response.message);

                            const modalElement = document.getElementById('rosterInOfficeDays-editShiftModal');
                            const modalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
                            modalInstance.hide();

                            loadTableData();
                            clear();
                        } else {
                            toastr.info(response.message);
                        }
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                    }
                });
            });
            // #endregion



            // #region Delete
            $(document).on('click', settings.delBtn, function () {
                selectedId = $(this).data('id');
                selectedDate = $(this).data('date');
            });

            $(settings.modalDelBtn).on('click', function () {
                if (selectedId && selectedDate) {
                    $.ajax({
                        url: deleteUrl,
                        method: 'POST',
                        data: {
                            id: selectedId,
                            overrideDate: selectedDate
                        },
                        success: function (response) {
                            if (response.isSuccess) {
                                toastr.success(response.message);
                                
                                const modalElement = document.getElementById('rosterInOfficeDays-delModal');
                                const modalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
                                modalInstance.hide();
                                loadTableData();
                                clear(); 
                            } else {
                                toastr.error(response.message);
                            }
                        },
                        error: function () {
                            toastr.error("Error occurred while deleting.");
                        }
                    });
                } else {
                    toastr.error("Invalid action.");
                }
            });
            // #endregion
            
            

            // #region clear
            $('#rosterInOfficeDays-resetBtn').on('click', function (e) {
                e.preventDefault();
                clear();
            });

            function clear() {
                $('#rosterInOfficeDays-form')[0].reset();
                //$('#BankID').val('').trigger('change');

                const branchSelect = document.getElementById('BranchIDs');
                const deptSelect = document.getElementById('DepartmentIDs');

                const deptInstance = coreui.MultiSelect.getInstance(deptSelect);
                const branchInstance = coreui.MultiSelect.getInstance(branchSelect);

                if (branchInstance) {
                    branchInstance.deselectAll();
                }

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

                ['OrganizationID', 'ShiftID', 'StartDate', 'EndDate'].forEach(function (fieldId) {
                    $('#' + fieldId).removeClass('is-valid is-invalid');
                    $('#' + fieldId + 'Error').hide().text('');
                    $('#' + fieldId).val('');
                });

                initOrganizationDD();
                initShiftDD();
            }
            // #endregion



            // #region Dropdown
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
                // Get the list of selected options.
                const selected = event.value
            });
            // #endregion



            // #region OrganizationID on change
            $('#OrganizationID').on('change', function (e) {
                e.preventDefault();

                var organizationId = $(this).val(); 
                loadBranchByOrganization(organizationId);
                loadDepartmentsByOrganization(organizationId);
                loadEmpByOrg(organizationId);
                loadShiftByOrg(organizationId);
            });
            // #endregion



            // #region loadBranchByOrganization
            function loadBranchByOrganization(organizationId) {
                $.ajax({
                    url: '/OfficeDayRoster/GerBranchByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (branch) {
                        var select = $('#BranchIDs');
                        select.empty();

                        //select.append('')
                        $.each(branch, function (index, b) {
                            console.log(`${b.id} ${b.name}`)
                            select.append(
                                $('<option>').val(b.id).text(b.name)
                            );
                        });

                        // Get the CoreUI multiselect instance
                        const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                        if (multiSelectInstance) {
                            multiSelectInstance.update(); // Refresh the UI
                        } else {
                            // Reinitialize if not already initialized (in case it's dynamically added)
                            new coreui.MultiSelect(select[0]);
                        }
                    },
                    error: function () {
                        console.error('Failed to load branch!');
                    }
                })
            }
            // #endregion



            // #region loadDepartmentsByOrganization
            function loadDepartmentsByOrganization(organizationId) {
                $.ajax({
                    url: '/OfficeDayRoster/GetDepartmentByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (departments) {
                        var select = $('#DepartmentIDs');
                        select.empty();

                        //select.append('')
                        $.each(departments, function (index, d) {
                            console.log(`${d.id} ${d.name}`)
                            select.append(
                                $('<option>').val(d.id).text(d.name)
                            );
                        });

                        // Get the CoreUI multiselect instance
                        const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                        if (multiSelectInstance) {
                            multiSelectInstance.update(); // Refresh the UI
                        } else {
                            // Reinitialize if not already initialized (in case it's dynamically added)
                            new coreui.MultiSelect(select[0]);
                        }

                        //document.getElementById('DepartmentIDs')
                        //    .addEventListener('changed.coreui.multi-select', function (event) {
                        //        const orgId = $('#OrganizationID').val();

                        //        const selected = event.value || []; // array of {text, value}
                        //        const departmentIds = selected.map(x => parseInt(x.value));

                        //        loadEmployeesByFilter(orgId, departmentIds);
                        //    });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading departments:', error);
                    }
                });
            }
            // #endregion



            // #region loadEmpByOrg
            function loadEmpByOrg(organizationId) {
                $.ajax({
                    url: '/OfficeDayRoster/GetEmployeeByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (employees) {
                        const select = $('#EmployeeIDs');
                        select.empty();

                        const grouped = {};

                        // Group employees by GroupName (DepartmentName)
                        employees.forEach(emp => {
                            const group = emp.groupName || 'No Department';
                            if (!grouped[group]) {
                                grouped[group] = [];
                            }
                            grouped[group].push(emp);
                        });

                        // Build <optgroup> structure
                        Object.keys(grouped).forEach(group => {
                            const optgroup = $('<optgroup>').attr('label', group);
                            grouped[group].forEach(emp => {
                                optgroup.append(
                                    $('<option>').val(emp.id).text(emp.name)
                                );
                            });
                            select.append(optgroup);
                        });

                        const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                        if (multiSelectInstance) {
                            multiSelectInstance.update(); // Refresh UI
                        } else {
                            new coreui.MultiSelect(select[0]); // Init CoreUI multiselect
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees: ', error);
                    }
                });
            }
            // #endregion



            // #region loadShiftByOrg
            function loadShiftByOrg(organizationId, selectedShiftId = null) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/OfficeDayRoster/GetShiftByOrganization',
                        type: 'GET',
                        data: { id: organizationId },
                        success: function (shifts) {
                            if (!shiftDD) return resolve();

                            const shiftChoices = shifts.map(shift => ({
                                value: shift.id.toString(),
                                label: shift.name,
                                selected: shift.id.toString() === selectedShiftId?.toString()
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
            // #endregion



            // #region BranchID on change
            document.getElementById('BranchIDs')
                .addEventListener('changed.coreui.multi-select', function (event) {
                    const orgId = $('#OrganizationID').val();

                    const selected = event.value || []; // array of {text, value}
                    const branchIds = selected.map(x => parseInt(x.value));

                    loadEmpByOrgBranchId(orgId, branchIds);
                });
            // #endregion



            // #region loadEmpByOrgBranchId/GetEmployeeByBranch
            function loadEmpByOrgBranchId(organizationId, branchIds = []) {
                $.ajax({
                    url: '/OfficeDayRoster/GetEmployeeByBranch',
                    type: 'GET',
                    traditional: true,
                    data: {
                        orgId: organizationId,
                        ids: branchIds
                    },
                    success: function (employees) {
                        const select = $('#EmployeeIDs');
                        select.empty();

                        const grouped = {};

                        // Group employees by GroupName (DepartmentName)
                        employees.forEach(emp => {
                            const group = emp.groupName || 'No Department';
                            if (!grouped[group]) {
                                grouped[group] = [];
                            }
                            grouped[group].push(emp);
                        });

                        // Build <optgroup> structure
                        Object.keys(grouped).forEach(group => {
                            const optgroup = $('<optgroup>').attr('label', group);
                            grouped[group].forEach(emp => {
                                optgroup.append(
                                    $('<option>').val(emp.id).text(emp.name)
                                );
                            });
                            select.append(optgroup);
                        });

                        const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                        if (multiSelectInstance) {
                            multiSelectInstance.update(); // Refresh UI
                        } else {
                            new coreui.MultiSelect(select[0]); // Init CoreUI multiselect
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees:', error);
                    }
                });
            }
            // #endregion



            // #region DepartmentIDs on change
            document.getElementById('DepartmentIDs')
                .addEventListener('changed.coreui.multi-select', function (event) {
                    const orgId = $('#OrganizationID').val();
                    const branchIds = $('#BranchIDs').val();

                    const selected = event.value || []; // array of {text, value}
                    const ids = selected.map(x => parseInt(x.value));

                    loadEmployeesByBranch(orgId, branchIds, ids);
                });
            // #endregion



            // #region loadEmployeesByBranch/GetEmployeeByDepartment
            function loadEmployeesByBranch(orgId, branchIds = [], ids = []) {
                $.ajax({
                    url: '/OfficeDayRoster/GetEmployeeByDepartment',
                    type: 'GET',
                    traditional: true,
                    data: {
                        orgId: orgId,
                        branchIds: branchIds,
                        depIds: ids
                    },
                    success: function (employees) {
                        const select = $('#EmployeeIDs');
                        select.empty();

                        const grouped = {};

                        // Group employees by GroupName (DepartmentName)
                        employees.forEach(emp => {
                            const group = emp.groupName || 'No Department';
                            if (!grouped[group]) {
                                grouped[group] = [];
                            }
                            grouped[group].push(emp);
                        });

                        // Build <optgroup> structure
                        Object.keys(grouped).forEach(group => {
                            const optgroup = $('<optgroup>').attr('label', group);
                            grouped[group].forEach(emp => {
                                optgroup.append(
                                    $('<option>').val(emp.id).text(emp.name)
                                );
                            });
                            select.append(optgroup);
                        });

                        const multiSelectInstance = coreui.MultiSelect.getInstance(select[0]);

                        if (multiSelectInstance) {
                            multiSelectInstance.update(); // Refresh UI
                        } else {
                            new coreui.MultiSelect(select[0]); // Init CoreUI multiselect
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees:', error);
                    }
                });
            }
            // #endregion



            // #region Load shift by opening edit shift modal
            $(settings.addShiftModal).on('show.bs.modal', function (e) {
                var btn = $(e.relatedTarget);
                var organizationId = btn.data('organization-id');

                $.ajax({
                    url: '/OfficeDayRoster/GetShiftByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (shifts) {
                        const $shiftSelect = $('#AddShiftModal-ShiftID');

                        $shiftSelect.empty();

                        // Add default placeholder
                        $shiftSelect.append(`<option value="">Select Shift...</option>`);

                        // Populate shift options
                        shifts.forEach(shift => {
                            const isSelected = shift.id;
                            $shiftSelect.append(
                                `<option value="${shift.id}" ${isSelected ? 'selected' : ''}>
                                    ${shift.name}
                                </option>`
                            );
                        });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading shifts:', error);
                    }
                });
            });
            // #endregion



            // #region Load shift by opening edit shift modal
            $(settings.editShiftModal).on('show.bs.modal', function (e) {
                var btn = $(e.relatedTarget);
                var shiftId = btn.data('shift-id');
                var organizationId = btn.data('organization-id');
                var rosterInOfficeDayID = btn.data('id');
                var overrideDate = btn.data('date');

                const $modal = $(this);
                $modal.data('roster-id', rosterInOfficeDayID);
                $modal.data('override-date', overrideDate);

                $.ajax({
                    url: '/OfficeDayRoster/GetShiftByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (shifts) {
                        const $shiftSelect = $('#EditShiftModal-ShiftID');
                        const selectedShiftId = $shiftSelect.data('selected');

                        $shiftSelect.empty();

                        // Add default placeholder
                        $shiftSelect.append(`<option value="">Select Shift...</option>`);

                        // Populate shift options
                        shifts.forEach(shift => {
                            const isSelected = shift.id;
                            $shiftSelect.append(
                                `<option value="${shift.id}" ${isSelected ? 'selected' : ''}>
                                    ${shift.name}
                                </option>`
                            );
                        });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading shifts:', error);
                    }
                });
            });
            // #endregion



            // #region flatpicker DatePicker
            flatpickr(".datetimepicker", {
                altInput: true,
                altFormat: "d/m/Y",
                dateFormat: "Y-m-d",
                monthSelectorType: "dropdown",
                disableMobile: true,
                allowInput: true 
            });
            // #endregion

                                   
            
            // #region For Range Date
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
            // #endregion
        });


        
        // #region Table With Pagination
        var currentPage = 1;
        var pageSize = 5;
        let currentSortColumn = 'DefaultShiftID';
        let currentSortOrder = 'desc';

        $('#rosterInOfficeDays-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();
            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });

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

        $('#timeFrame').on('change', function () {
            const days = parseInt($(this).val(), 10);
            loadTableData(currentSortColumn, currentSortOrder, days);
        });


        let currentStartDate = new Date(); // defaults to today
        function getDaysToShow() {
            return parseInt($("#timeFrame").val(), 10);
        }


        function generateDateHeaders(daysCount = 7, startDate = new Date()) {
            const headers = [];
            for (let i = 0; i < daysCount; i++) {
                const date = new Date(startDate);
                date.setDate(startDate.getDate() + i);
                headers.push({
                    day: date.toLocaleDateString(undefined, { weekday: 'short' }),
                    date: date.toISOString().split('T')[0]
                });
            }
            return headers;
        }


        function renderEmployeeTable(employees) {
            const daysToShow = getDaysToShow(); 
            const headers = generateDateHeaders(daysToShow, currentStartDate);  // Number of days for columns
            //const paginationInfo = result.paginationInfo;

            // Render header
            let headerHtml = `<th class="align-middle text-center text-uppercase text-nowrap">Employee Name</th>`;
            headers.forEach(h => {
                headerHtml += `<th class="align-middle text-center text-uppercase text-nowrap">${h.day}<br>${h.date}</th>`;
            });
            $('table.leads-table thead tr').html(headerHtml);

            // Render rows
            let bodyHtml = '';
            employees.forEach(emp => {
                bodyHtml += '<tr>';

                // Employee info column
                bodyHtml += `
                <td class="align-middle text-center white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2 sticky-col bg-white z-index-sticky">
                    <div class="d-inline-flex flex-column align-items-center justify-content-center">
                        <h5>${emp.employeeName}</h5>
                        <p class="fs-9 mb-0">${emp.departmentName}</p>
                        <p class="fs-9 mb-0">${emp.organizationName}</p>
                    </div>
                </td>`;

                // Parse and show shifts
                const shiftMap = parseShiftSchedule(emp.assignedDates);
                const weekendDays = parseWeekdayNumbers(emp.weekdayNumber);
                const holidayMap = {};
                (emp.holidayDates || "").split(',').forEach((date, i) => {
                    const title = (emp.holidayTitle || "").split(',')[i] || "Holiday";
                    holidayMap[date.trim()] = title.trim();
                });
                headers.forEach(h => {
                    const dateObj = new Date(h.date);
                    const weekdayNumber = dateObj.getDay(); // Sunday = 0, Saturday = 6
                    const shift = shiftMap[h.date];
                    const holidayTitle = holidayMap[h.date];
                    const isWeekend = weekendDays.includes(weekdayNumber);
                    if (holidayTitle) {
                        bodyHtml += `
                        <td class="holiday-cell align-middle text-center">
                            <div class="position-relative badge badge-phoenix-danger holiday-block px-4 py-2" style="border-left:5px solid #FC0808;">
                                <p class="fs-10 mb-1">${shift?.timeRange || ''}</p>
                                <p class="fs-10 mb-1">${shift?.shiftName || ''}</p>
                                <p class="fs-10 mb-0 p-1 bg-light text-info">${holidayTitle}</p>
                                <a href="#" class="btn btn-info btn-sm px-2 py-1 nav-item mx-2 edit-shift-btn" data-bs-toggle="modal"
                                    id="rosterInOfficeDays-editBtn"
                                    data-id="${shift?.rosterInOfficeDayId || ''}" 
                                    data-date="${h.date}" 
                                    data-shift-id="${shift?.shiftID || ''}"
                                    data-organization-id="${emp.organizationID}" 
                                    data-bs-target="#rosterInOfficeDays-editShiftModal">
                                    <i class="fas fa-pen"></i>
                                </a>
                            </div>
                        </td>`;
                    } else if (shift) {
                        const badgeClass = isWeekend ? 'badge-phoenix-danger' : 'badge-phoenix-primary';
                        const leftColor = isWeekend ? '#FF6F6F' : '#A1F1A1';
                        bodyHtml += `
                        <td class="startTime align-middle text-center">
                            <div class="position-relative badge ${badgeClass} shift-block px-4 py-2" style="border-left:5px solid ${leftColor};">
                                <p class="fs-10 mb-1">${shift.timeRange}</p>
                                <p class="fs-10 mb-1">${shift.shiftName}</p>
                                <a href="#" class="btn btn-info btn-sm px-2 py-1 nav-item mx-2 edit-shift-btn" data-bs-toggle="modal" id="rosterInOfficeDays-editBtn"
                                    data-id="${shift.rosterInOfficeDayId}" 
                                    data-date="${h.date}" 
                                    data-shift-id="${shift.shiftID}"
                                    data-organization-id="${emp.organizationID}" 
                                    data-bs-target="#rosterInOfficeDays-editShiftModal">
                                    <i class="fas fa-pen"></i>
                                </a>
                            </div>
                        </td>`;
                    } else {
                        bodyHtml += `
                        <td class="shift-cell align-middle text-center">
                            <a href="#" class="btn btn-outline-success add-shift-btn" data-bs-toggle="modal" data-bs-target="#addShiftModal"
                                data-organization-id="${emp.organizationID}">
                                <i class="fa fa-plus"></i>
                            </a>
                        </td>`;
                    }
                });

                bodyHtml += '</tr>';
            });

            $('table.leads-table tbody').html(bodyHtml);

            const firstDate = headers[0].date;
            const lastDate = headers[headers.length - 1].date;
            $(".date-range-label").text(`${firstDate} - ${lastDate}`);
        }


        function parseShiftSchedule(assignedDatesStr) {
            const shiftMap = {};
            if (!assignedDatesStr) return shiftMap;

            assignedDatesStr.split(',').forEach(entry => {
                const [date, shiftName, timeRange] = entry.trim().split('|');
                if (date && shiftName && timeRange) {
                    shiftMap[date] = { shiftName, timeRange };
                }
            });
            return shiftMap;
        }


        function parseWeekdayNumbers(weekdayNumberStr) {
            if (!weekdayNumberStr) return [];
            return weekdayNumberStr.split(',').map(n => parseInt(n.trim(), 10)).filter(n => !isNaN(n));
        }


        function loadTableData(daysToShow = getDaysToShow(), startDate = currentStartDate) {
            var searchTerm = $("#rosterInOfficeDays-searchInput").val();
            $.ajax({
                //url: settings.baseUrl + '/GetEmployeesPaged',
                url: settings.baseUrl + '/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    daysToShow: daysToShow,
                    startDate: startDate
                },
                success: function (response) {
                    const employees = response.data;  // Your controller returns JSON { data = ..., totalCount = ... }
                    const totalCount = response.totalCount;
                    renderEmployeeTable(employees);

                    // Calculate pagination info
                    const pagination = response.separatePaginationInfo;

                    // ✅ Update pagination info display
                    $("#startItem").text(pagination.startItem);
                    $("#endItem").text(pagination.endItem);
                    $("#totalItems").text(pagination.totalItems);

                    // ✅ Update pagination buttons
                    updatePagination(pagination.pageNumbers, pagination.currentPage, pagination.totalPages);
                },
                error: function (xhr, status, error) {
                    console.error('Error loading employee data:', error);
                }
            });
        }



        $("#chevron-left").on("click", function () {
            const days = getDaysToShow();
            currentStartDate.setDate(currentStartDate.getDate() - days); // Go back one page
            loadTableData(currentSortColumn, currentSortOrder, days, currentStartDate);
        });

        $("#chevron-right").on("click", function () {
            const days = getDaysToShow();
            currentStartDate.setDate(currentStartDate.getDate() + days); // Go forward one page
            loadTableData(currentSortColumn, currentSortOrder, days, currentStartDate);
        });



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
        // #endregion
    }
}(jQuery));