(function ($) {
    $.offdayroster = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#rosterInOffDay-form',
            saveBtn: '#rosterInOffDay-saveBtn',
            resetBtn: '#rosterInOffDay-resetBtn',
            editShiftModal: '#rosterInOffDay-editShiftModal',
            editShiftSaveBtn: '#EditShiftModal-saveShift',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAllAsync";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var updateEmpShift = settings.baseUrl + "/UpdateEmpShiftAsync";
        $(() => {


            loadTableData();

            // #region Save
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                //const token = $('#rosterInOffDay-form input[name="__RequestVerificationToken"]').val();

                const formData = {
                    //__RequestVerificationToken: token,
                    RosterInHolyDayID: $('#RosterInHolyDayID').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    DepartmentIDs: $('#DepartmentIDs').val(),
                    EmployeeIDs: $('#EmployeeIDs').val(),
                    ShiftID: $('#ShiftID').val(),
                    DayDate: $('#DayDate').val().split(','),
                    CompensationTypeID: $('#CompensationTypeID').val(),
                    //ExchangeDate: $('#ExchangeDate').val().split(',')
                };

                const exchangeDateValue = $('#ExchangeDate').val();
                if (exchangeDateValue) {
                    formData.ExchangeDate = exchangeDateValue.split(',');
                }

                const id = $('#RosterInHolyDayID').val();
                const isEdit = id > 0;
                const url = isEdit ? updateUrl : createUrl;

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        if (response.isSuccess === true) {
                            toastr.success(response.message);
                            clear();
                            loadTableData();
                        } else {
                            const allFields = ["OrganizationID", "ShiftID", "DayDate", "CompensationTypeID"];

                            allFields.forEach(function (fieldId) {
                                validateField(fieldId, response);
                            });
                            toastr.info(response.message);
                        }
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                    }
                });
            });
            // #endregion


            // #region clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();
                clear();
            });

            function clear() {
                $(settings.form)[0].reset();
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

                if (compensationDD) {
                    compensationDD.destroy();
                }

                ['OrganizationID', 'ShiftID', 'DayDate', 'CompensationTypeID'].forEach(function (fieldId) {
                    $('#' + fieldId).removeClass('is-valid is-invalid');
                    $('#' + fieldId + 'Error').hide().text('');
                    $('#' + fieldId).val('');
                });

                $('#DayDate').prop('disabled', true);
                flatpickr("#DayDate").destroy();
                toggleCompensationSelect();

                $('#ExchangeDateDiv').addClass('d-none');

                initOrganizationDD();
                initShiftDD();
                initCompensationDD();
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


            //function initBranchDD() {
            //    branchDD = new Choices('#BranchIDs', {
            //        removeItemButton: true,
            //        shouldSort: false,
            //        placeholderValue: 'Select Branch...'
            //    });
            //}
            //document.addEventListener('DOMContentLoaded', initBranchDD);
            //initBranchDD();


            function initShiftDD() {
                shiftDD = new Choices('#ShiftID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Shift...'
                });
            }
            document.addEventListener('DOMContentLoaded', initShiftDD);
            initShiftDD();


            function initCompensationDD() {
                compensationDD = new Choices('#CompensationTypeID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Compensation...'
                });
            }
            document.addEventListener('DOMContentLoaded', initCompensationDD);
            initCompensationDD();
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
                    url: '/OffDayRoster/GetBranchByOrganization',
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
                    url: '/OffDayRoster/GetDepartmentByOrganization',
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
                    url: '/OffDayRoster/GetEmployeeByOrganization',
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
                        url: '/OffDayRoster/GetShiftByOrganization',
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
                    url: '/OffDayRoster/GetEmployeeByBranch',
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
                    url: '/OffDayRoster/GetEmployeeByDepartment',
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


            // #region GetWeekendByOrganization
            $('#OrganizationID').on('change', function () {
                var selectedId = $(this).val();  // Get the selected OrganizationID

                if (selectedId) {  // Ensure a valid ID is selected (not an empty value)
                    // Perform the AJAX request to get the weekend settings
                    $.ajax({
                        url: '/OffDayRoster/GetWeekendByOrganization',  // Your API endpoint
                        type: 'GET',
                        data: { id: selectedId },  // Pass the selected OrganizationID as a query parameter
                        dataType: 'json',
                        success: function (data) {
                            // Prepare an array to store the enabled dates
                            let enabledDates = [];
                            let disabledDates = [];

                            // Loop through the weekend settings data
                            $.each(data, function (index, item) {
                                // Parse the WeekdayNumbers (comma-separated) into an array of numbers
                                const weekdayNumbers = item.weekdayNumbers.split(',').map(Number);  // [1, 2, 5]

                                // Loop through each month (0-11, where 0 = January and 11 = December)
                                for (let month = 0; month < 12; month++) {
                                    const currentYear = new Date().getFullYear();

                                    // Loop through all days in the month (1 to 31)
                                    for (let day = 1; day <= 31; day++) {
                                        const date = new Date(currentYear, month, day);

                                        // Skip invalid dates (e.g., February 30, April 31, etc.)
                                        if (date.getMonth() !== month) continue;

                                        const weekday = date.getDay(); // 0-Sunday, 1-Monday, ..., 6-Saturday

                                        // If the weekday is in the list of enabled weekdays, add the date to the enabledDates array
                                        if (weekdayNumbers.includes(weekday)) {
                                            // Format the date as 'Y-m-d' (flatpickr date format)
                                            const formattedDate = date.toISOString().split('T')[0];  // 'YYYY-MM-DD'
                                            enabledDates.push(formattedDate);
                                        } else {
                                            // Otherwise, it's disabled, add to disabledDates
                                            const formattedDate = date.toISOString().split('T')[0];  // 'YYYY-MM-DD'
                                            disabledDates.push(formattedDate);
                                        }
                                    }
                                }
                            });

                            $('#DayDate').prop('disabled', false); 

                            flatpickr("#DayDate", {
                                altInput: true,
                                altFormat: "d/m/Y",
                                dateFormat: "Y-m-d",
                                monthSelectorType: "dropdown",
                                disableMobile: true,
                                allowInput: true,
                                mode: "multiple",
                                enable: enabledDates 
                            });

                            flatpickr("#ExchangeDate", {
                                altInput: true,
                                altFormat: "d/m/Y",
                                dateFormat: "Y-m-d",
                                monthSelectorType: "dropdown",
                                disableMobile: true,
                                allowInput: true,
                                mode: "multiple",
                                enable: disabledDates 
                            });
                        },
                        error: function (xhr, status, error) {
                            console.error('Error fetching weekend settings:', error);
                        }
                    });
                } else {
                    $('#DayDate').prop('disabled', true);
                    flatpickr("#DayDate").destroy();
                }
            });
            // #endregion

                        
            function toggleCompensationSelect() {
                const dayDateVal = $('#DayDate').val().trim();
                if (dayDateVal) {
                    $('#CompensationTypeID').prop('disabled', false);
                    compensationDD.enable();
                } else {
                    $('#CompensationTypeID').prop('disabled', true);
                    compensationDD.disable();
                }
            }
            toggleCompensationSelect();

            // Recheck on input change
            $('#DayDate').on('input change blur', function () {
                toggleCompensationSelect();
            });


            // #region Load shift by opening edit shift modal
            $(settings.editShiftModal).on('show.bs.modal', function (e) {
                var btn = $(e.relatedTarget);
                var rosterInHolyDayIdEdit = btn.data('id');
                var shiftId = btn.data('shift-id');
                var organizationId = btn.data('organization-id');
                var depId = btn.data('dep-id');
                var empId = btn.data('emp-id');
                var overrideDate = btn.data('date');

                $('#RosterInHolyDayIdEdit').val(rosterInHolyDayIdEdit);
                $('#OrganizationIdEdit').val(organizationId);
                $('#DepartmentIdEdit').val(depId);
                $('#EmployeeIdEdit').val(empId);
                $('#DayDateEdit').val(overrideDate);

                $.ajax({
                    url: '/OffDayRoster/GetShiftByOrganization',
                    type: 'GET',
                    data: { id: organizationId },
                    success: function (shifts) {
                        const $shiftSelect = $('#ShiftIdEdit');

                        $shiftSelect.empty();

                        // Add default placeholder
                        $shiftSelect.append(`<option value="">Select Shift...</option>`);

                        // Populate shift options
                        shifts.forEach(shift => {
                            const isSelected = shift.id === shiftId;
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


            // #region Edit
            $(settings.editShiftSaveBtn).on('click', function (e) {
                e.preventDefault();

                const $modal = $(settings.editShiftModal);
                //const token = $('#EditShiftModal-form input[name="__RequestVerificationToken"]').val();

                let formData = new FormData();
                formData.append("RosterInHolyDayIdEdit", $('#RosterInHolyDayIdEdit').val());
                formData.append("OrganizationIdEdit", $('#OrganizationIdEdit').val());
                formData.append("DepartmentIdEdit", $('#DepartmentIdEdit').val());
                formData.append("EmployeeIdEdit", $('#EmployeeIdEdit').val());
                formData.append("ShiftIdEdit", $('#ShiftIdEdit').val());
                formData.append("DayDateEdit", $('#DayDateEdit').val());

                const url = updateEmpShift;

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    processData: false,
                    contentType: false,
                    success: function (response) {
                        if (response.isSuccess) {
                            toastr.success(response.message);

                            const modalElement = document.getElementById('rosterInOffDay-editShiftModal');
                            const modalInstance = bootstrap.Modal.getInstance(modalElement) || new bootstrap.Modal(modalElement);
                            modalInstance.hide();

                            loadTableData();
                            //clear();
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



            $('#CompensationTypeID').on('change', function () {
                const selectedValue = $(this).val();
                if (selectedValue === "3") {
                    $('#ExchangeDateDiv').removeClass('d-none');
                    //$('#rosterInOffDay-dayExchangeModal').modal('show');
                }
            });
            


        });

        // #region
        var currentPage = 1;
        var pageSize = 5;
        let currentSortColumn = 'RosterInHolyDayID';
        let currentSortOrder = 'desc';
        let daysToShow = 7;
        let columnStartDate = new Date(); 

        $('#rosterInOffDay-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();
            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });

        $(document).ready(function () {
            loadTableData();

            $("#rosterInOffDay-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#rosterInOffDay-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#rosterInOffDay-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });

            $('#chevron-right').on('click', function () {
                columnStartDate.setDate(columnStartDate.getDate() + parseInt(daysToShow));
                loadTableData();
            });

            $('#chevron-left').on('click', function () {
                columnStartDate.setDate(columnStartDate.getDate() - parseInt(daysToShow));
                loadTableData();
            });
        });

        $('#timeFrame').on('change', function () {
            daysToShow = $(this).val();
            columnStartDate = new Date();
            currentPage = 1;
            loadTableData();
        });

        function loadTableData(sortColumn, sortOrder) {
            var searchTerm = $("#rosterInOffDay-searchInput").val();
            const formattedStartDate = columnStartDate.toISOString();
            $.ajax({
                url: gridUrl,
                type: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    daysToShow: daysToShow,
                    startDate: formattedStartDate
                },
                success: function (response) {
                    if (response.isSuccess) {
                        buildRosterTable(response.data, response.uniqueDates);

                        // 👇 NEW CODE: Set date range label
                        if (response.uniqueDates && response.uniqueDates.length > 0) {
                            const first = new Date(response.uniqueDates[0]);
                            const last = new Date(response.uniqueDates[response.uniqueDates.length - 1]);

                            const format = (date) =>
                                date.toLocaleDateString('en-GB', {
                                    day: '2-digit',
                                    month: 'short',
                                    year: 'numeric'
                                });

                            const rangeText = `${format(first)} - ${format(last)}`;
                            $(".date-range-label").text(rangeText);
                        } else {
                            $(".date-range-label").text("No dates available");
                        }

                        const pageInfo = response.pagination;

                        $('#startItem').text(response.pagination.startItem);
                        $('#endItem').text(response.pagination.endItem);
                        $('#totalItems').text(response.pagination.totalItems);

                        updatePagination(pageInfo.pageNumbers, pageInfo.currentPage, pageInfo.totalPages);
                    } else {
                        console.log("Failed to get data.");
                    }
                },
                error: function () {
                    console.log('Failed to load roster data.');
                }
            });
        }

        function buildRosterTable(rosterList, uniqueDates) {
            let thead = '<tr><th class="align-middle text-center text-uppercase text-nowrap">Employee Name</th>';

            uniqueDates.forEach(date => {
                const d = new Date(date);
                const displayDate = d.toLocaleDateString('en-GB');
                const dayName = d.toLocaleDateString('en-GB', { weekday: 'short' });

                thead += `
                    <th class="align-middle text-center text-uppercase text-nowrap">
                        ${dayName}<br/>${displayDate}
                    </th>`;
            });

            thead += '</tr>';
            $('#rosterTableHead').html(thead);

            let tbody = '';
            rosterList.forEach(emp => {
                tbody += `<tr>
                    <td class="align-middle text-center white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2 sticky-col bg-white z-index-sticky">
                        <div class="d-inline-flex flex-column align-items-center justify-content-center">
                            <h5>${emp.employeeName}<br/></h5>
                            <p class="fs-9 mb-0">${emp.departmentName}<br/></p>
                            <p class="fs-9 mb-0">${emp.organizationName}</p>
                        </div>
                    </td>`;

                uniqueDates.forEach(date => {
                    const shift = emp.shiftsPerDay[date];
                    if (shift) {
                        tbody += `
                        <td class="holiday-cell align-middle text-center">
                            <div class="position-relative badge badge-phoenix-primary shift-block px-4 py-2" style="border-left:5px solid #FF6F6F;">
                                <p class="fs-10 mb-1">${shift.timeRange}</p>
                                <p class="fs-10 mb-1">${shift.shiftName}</p>
                                <a href="#" class="btn btn-light btn-sm px-2 py-1 nav-item mx-2 edit-shift-btn"
                                   data-bs-toggle="modal" id="rosterInOffDay-editBtn"
                                   data-id="${shift.rosterInHolyDayID}" 
                                   data-date="${date}" 
                                   data-shift-id="${shift.shiftID}"
                                   data-organization-id="${emp.organizationID}" 
                                   data-dep-id="${emp.departmentID}" 
                                   data-emp-id="${emp.employeeID}" 
                                   data-bs-target="#rosterInOffDay-editShiftModal">
                                    <i class="fas fa-pen"></i>
                                </a>
                            </div>
                        </td>`;
                    } else {
                        tbody += `<td class="holiday-cell align-middle text-center">-</td>`;
                    }
                });

                tbody += '</tr>';
            });

            $('#rosterTableBody').html(tbody);
        }


        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#rosterInOffDay-paginationLinks");
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

            $("#rosterInOffDay-prevPageBtn").prop('disabled', currentPage === 1);
            $("#rosterInOffDay-nextPageBtn").prop('disabled', currentPage === totalPages);
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