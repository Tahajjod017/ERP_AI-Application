(function ($) {
    $.offdayroster = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#rosterInOffDay-form',
            saveBtn: '#rosterInOffDay-saveBtn',
            resetBtn: '#rosterInOffDay-resetBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        $(() => {


            loadTableData();

            // #region Save
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                //const token = $('#rosterInOfficeDays-form input[name="__RequestVerificationToken"]').val();

                const formData = {
                    //__RequestVerificationToken: token,
                    RosterInOffDayID: $('#RosterInOffDayID').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    DepartmentIDs: $('#DepartmentIDs').val(),
                    EmployeeIDs: $('#EmployeeIDs').val(),
                    ShiftID: $('#ShiftID').val(),
                    DayDate: $('#DayDate').val().split(','),
                    CompensationTypeID: $('#CompensationTypeID').val()
                };

                const id = $('#RosterInOffDayID').val();
                const isEdit = id > 0;
                const url = isEdit ? updateUrl : createUrl;

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        const allFields = ["OrganizationID", "ShiftID", "DayDate", "CompensationTypeID"];

                        allFields.forEach(function (fieldId) {
                            validateField(fieldId, response);
                        });

                        if (response.isSuccess) {
                            toastr.success(response.message);
                            clear();
                            loadTableData();
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



        });



        function loadTableData() {
            $.ajax({
                url: '/OffDayRoster/GetRosterData',
                type: 'GET',
                success: function (response) {
                    if (response.success) {
                        buildRosterTable(response.rosterList, response.uniqueDates);
                    } else {
                        alert("Failed to get data.");
                    }
                },
                error: function () {
                    alert('Failed to load roster data.');
                }
            });

            function buildRosterTable(rosterList, uniqueDates) {
                let thead = '<tr><th>Employee Name</th>';

                uniqueDates.forEach(date => {
                    const d = new Date(date);
                    const displayDate = d.toLocaleDateString('en-GB'); // Format: dd/mm/yyyy
                    const dayName = d.toLocaleDateString('en-GB', { weekday: 'short' }); // e.g., Mon, Tue

                    thead += `<th>${dayName}<br/>${displayDate}</th>`;
                });

                thead += '</tr>';
                $('#rosterTableHead').html(thead);

                let tbody = '';
                rosterList.forEach(emp => {
                    tbody += `<tr>
                    <td>${emp.employeeName}<br/>${emp.departmentName}<br/>${emp.organizationName}</td>`;

                    uniqueDates.forEach(date => {
                        debugger
                        const shift = emp.shiftsPerDay[date];

                        if (shift) {
                            tbody += `<td>${shift.shiftName}<br/><small>${shift.timeRange}</small></td>`;
                        } else {
                            tbody += `<td>-</td>`;
                        }
                    });

                    tbody += '</tr>';
                });

                $('#rosterTableBody').html(tbody);
            }
        }

    }
}(jQuery));