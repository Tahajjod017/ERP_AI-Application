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

            const orgId = $('#OrganizationID');
            const dayDate = $('#DayDate');

            // Function to toggle input based on selection
            function toggleDayDate() {
                const hasValue = orgId.val() !== "";
                dayDate.prop('disabled', !hasValue);
            }

            // Run on page load
            toggleDayDate();

            let dayDatePicker = null;
            let exchangeDatePicker = null;


            // #region Save
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');
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
                            const allFields = ["OrganizationID", "DayDate", "ShiftID", "CompensationTypeID"];

                            allFields.forEach(function (fieldId) {
                                validateField(fieldId, response);
                            });
                            toastr.info(response.message);
                        }
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    },
                    error: function (err) {
                        console.error('Conflict check failed:', err);
                        $(settings.saveBtn).prop('disabled', false).html('Save');
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

                if (deptInstance) {
                    deptInstance.deselectAll();
                }

                if (branchInstance) {
                    branchInstance.deselectAll();
                }

                const empSelect = document.getElementById('EmployeeIDs');
                const empInstance = coreui.MultiSelect.getInstance(empSelect);
                if (empInstance) {
                    empInstance.deselectAll();
                }


                if (organizationDD) {
                    organizationDD.destroy();
                }
                selectSingleOrg();
                initOrganizationDD();

                if (shiftDD) {
                    shiftDD.destroy();
                }
                initShiftDD();

                if (compensationDD) {
                    compensationDD.destroy();
                }
                initCompensationDD();

                ['OrganizationID', 'DayDate', 'ShiftID', 'CompensationTypeID'].forEach(function (fieldId) {
                    $('#' + fieldId).removeClass('is-valid is-invalid');
                    $('#' + fieldId + 'Error').hide().text('');
                    $('#' + fieldId).val('');
                });
                //dayDatePicker.clear();
                //exchangeDatePicker.clear();
                $('#DayDate').prop('disabled', true);
                flatpickr("#DayDate").destroy();

                $('#ExchangeDateDiv').addClass('d-none');

                toggleCompensationSelect();
            }
            // #endregion


            // #region Dropdown
            function initOrganizationDD() {
                organizationDD = new Choices('#OrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Organization...'
                });
                selectSingleOrg();
            }
            function selectSingleOrg() {
                var $select = $('#OrganizationID');

                // Count the number of options excluding the placeholder (empty value)
                var $realOptions = $select.find('option').filter(function () {
                    return $(this).val() != '';
                });

                if ($realOptions.length == 1) {
                    $realOptions.prop('selected', true);
                    $select.trigger('change');
                }
            }
            $(document).ready(function () {
                initOrganizationDD();
            });


            function initShiftDD() {
                shiftDD = new Choices('#ShiftID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Shift...'
                });
            }
            initShiftDD();


            function initCompensationDD() {
                compensationDD = new Choices('#CompensationTypeID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Compensation...'
                });
            }
            initCompensationDD();
            // #endregion


            // #region OrganizationID on change
            $('#OrganizationID').on('change', function (e) {
                e.preventDefault();

                var orgId = $(this).val();

                getEmployeesByOrgDatesBraDepId(orgId, [], [], []);
                loadBranchByOrganization(orgId);
                loadDepartmentsByOrganization(orgId);
                loadShiftByOrg(orgId);
            });
            // #endregion


            // #region DayDate on change
            $('#DayDate').on('change', function () {
                var dates = $(this).val().split(',');
                var orgId = $('#OrganizationID').val();
                getEmployeesByOrgDatesBraDepId(orgId, dates, [], []);
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
                            new coreui.MultiSelect(select[0]);
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading departments:', error);
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
            document.getElementById('BranchIDs').addEventListener('changed.coreui.multi-select', function (event) {
                const orgId = $('#OrganizationID').val();
                var dates = $('#DayDate').val().split(',');
                const selected = event.value || []; // array of {text, value}
                const branchIds = selected.map(x => parseInt(x.value));

                getEmployeesByOrgDatesBraDepId(orgId, dates, branchIds, null);
            });
            // #endregion


            // #region DepartmentIDs on change
            document.getElementById('DepartmentIDs').addEventListener('changed.coreui.multi-select', function (event) {
                const orgId = $('#OrganizationID').val();
                var dates = $('#DayDate').val().split(',');
                const branchIds = $('#BranchIDs').val();

                const selected = event.value || []; // array of {text, value}
                const deptIds = selected.map(x => parseInt(x.value));

                getEmployeesByOrgDatesBraDepId(orgId, dates, branchIds, deptIds);
            });
            // #endregion


            // #region getEmployeesByOrgDatesBraDepId (with pagination, search, scroll)
            //function getEmployeesByOrgDatesBraDepId(orgId, dates = [], branchIds = [], depIds = []) {
            //    const selectEl = document.getElementById('EmployeeIDs');
            //    if (!selectEl) return;

            //    const ms = coreui.MultiSelect.getOrCreateInstance(selectEl);

            //    let page = 1;
            //    let term = '';
            //    const pageSize = 50;

            //    let hasMore = true;
            //    let loading = false;
            //    let debounce;
            //    let scrollPosition = 0;

            //    // Initial fetch trigger on open
            //    selectEl.addEventListener('shown.coreui.multi-select', () => {
            //        if (selectEl.options.length === 0) {
            //            page = 1;
            //            term = '';
            //            hasMore = true;
            //            fetchPage({ append: false });
            //        }

            //        ensureSearchHandler();
            //        rebindScroll();
            //    });

            //    function ensureSearchHandler() {
            //        const wrapper = selectEl.nextElementSibling;
            //        const input = wrapper?.querySelector('.form-multi-select-search');
            //        const box = wrapper?.querySelector('.form-multi-select-options');
            //        if (!input || input.dataset.listenerAttached) return;

            //        input.dataset.listenerAttached = '1';
            //        input.addEventListener('mousedown', e => e.stopPropagation());

            //        input.addEventListener('input', e => {
            //            const val = e.target.value.trim();
            //            clearTimeout(debounce);

            //            if (val.length < 3) {
            //                addOptions([], { reset: true });
            //                page = 1;
            //                term = '';
            //                hasMore = false;
            //                if (box) box.scrollTop = 0;
            //                return;
            //            }

            //            debounce = setTimeout(() => {
            //                term = val;
            //                page = 1;
            //                hasMore = true;
            //                fetchPage({ append: false });
            //                if (box) box.scrollTop = 0;
            //            }, 1000);
            //        });
            //    }

            //    function rebindScroll() {
            //        const box = selectEl.nextElementSibling?.querySelector('.form-multi-select-options');
            //        if (!box || box.dataset.infiniteAttached) return;

            //        box.dataset.infiniteAttached = '1';
            //        box.addEventListener('scroll', () => {
            //            if (box.scrollTop + box.clientHeight >= box.scrollHeight - 10) {
            //                if (hasMore && !loading) fetchPage({ append: true });
            //            }
            //        });
            //    }

            //    function addOptions(items, { reset = false } = {}) {
            //        const box = selectEl.nextElementSibling?.querySelector('.form-multi-select-options');
            //        if (box) scrollPosition = box.scrollTop;

            //        if (reset) {
            //            const keep = new Set([...selectEl.options].filter(o => o.selected).map(o => o.value));
            //            [...selectEl.options].forEach(o => {
            //                if (!keep.has(o.value)) o.remove();
            //            });
            //        }

            //        const existing = new Set([...selectEl.options].map(o => String(o.value)));
            //        const grouped = {};

            //        items.forEach(emp => {
            //            const group = emp.group || 'No Department';
            //            if (!grouped[group]) grouped[group] = [];
            //            grouped[group].push(emp);
            //        });

            //        Object.keys(grouped).forEach(group => {
            //            const optgroup = document.createElement('optgroup');
            //            optgroup.label = group;

            //            grouped[group].forEach(emp => {
            //                const idStr = String(emp.value);
            //                if (existing.has(idStr)) return;
            //                const opt = document.createElement('option');
            //                opt.value = idStr;
            //                opt.textContent = emp.label;
            //                optgroup.appendChild(opt);
            //            });

            //            selectEl.appendChild(optgroup);
            //        });

            //        const wasOpen = !!ms._isShown;
            //        ms.update();
            //        if (wasOpen) ms.show();
            //        ensureSearchHandler();

            //        const input = selectEl.nextElementSibling?.querySelector('.form-multi-select-search');
            //        if (input && input.value) {
            //            input.setSelectionRange(input.value.length, input.value.length);
            //        }

            //        setTimeout(() => {
            //            const box = selectEl.nextElementSibling?.querySelector('.form-multi-select-options');
            //            if (box) {
            //                box.scrollTop = reset ? 0 : scrollPosition;
            //            }
            //        }, 10);

            //        rebindScroll();
            //    }

            //    async function fetchPage({ append }) {
            //        if (loading || (!hasMore && append)) return;
            //        loading = true;
            //        try {
            //            const url = new URL('/OffDayRoster/GetEmployeesByOrgDatesBraDepId', window.location.origin);
            //            const params = new URLSearchParams();

            //            if (orgId) params.append('orgId', orgId);
            //            dates.forEach(id => params.append('dates', id));
            //            branchIds.forEach(id => params.append('branchIds', id));
            //            depIds.forEach(id => params.append('depIds', id));
            //            if (term) params.append('search', term);
            //            params.append('page', page);
            //            params.append('pageSize', pageSize);

            //            url.search = params.toString();

            //            const response = await fetch(url);
            //            if (!response.ok) throw new Error(`HTTP ${response.status}`);

            //            const data = await response.json();

            //            addOptions(data.items, { reset: !append });
            //            hasMore = !!data.hasMore;
            //            if (append) page++;
            //            else page = 2;
            //        } catch (e) {
            //            console.error('Error fetching employees:', e);
            //        } finally {
            //            loading = false;
            //        }
            //    }
            //}
            // #endregion


            // #region GetEmployeesByOrgDatesBraDepId
            function getEmployeesByOrgDatesBraDepId(orgId, dates = [], branchIds = [], deptIds = []) {
                $.ajax({
                    url: '/OffDayRoster/GetEmployeesByOrgDatesBraDepId',
                    type: 'GET',
                    traditional: true,
                    data: {
                        orgId: orgId,
                        dates: dates,
                        branchIds: branchIds,
                        deptIds: deptIds
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


            // #region GetWeekendByOrganization and Weekdays
            $('#OrganizationID').on('change', function () {
                const selectedId = $(this).val();

                if (!selectedId) {
                    $('#DayDate, #ExchangeDate').prop('disabled', true).each(function () {
                        this._flatpickr?.clear();
                    });
                    return;
                }

                $.getJSON('/OffDayRoster/GetWeekendByOrganization', { id: selectedId }, function (data) {
                    //const holidayDates = new Set();
                    const holidayDates = new Map();
                    const weekendDays = new Set();

                    data.forEach(item => {
                        if (item.totalDays) {
                            item.totalDays.split(', ').forEach((d, i) => {
                                const [day, month, year] = d.split('/');
                                const isoDate = `20${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
                                const title = item.holidayTitle?.split(', ')[i] || "Holiday"; // fallback
                                holidayDates.set(isoDate, title);
                            });
                        }

                        // Handle weekend days
                        if (item.weekdayNumbers) {
                            item.weekdayNumbers.split(',').map(Number).forEach(wd => weekendDays.add(wd));
                        }
                    });

                    const currentYear = new Date().getFullYear();
                    const dayDateEnabled = [];
                    const exchangeDateEnabled = [];

                    // Generate both lists by iterating through the full year
                    for (let month = 0; month < 12; month++) {
                        for (let day = 1; day <= 31; day++) {
                            const date = new Date(currentYear, month, day);
                            if (date.getMonth() !== month) continue; // skip invalid days

                            //const iso = date.toISOString().split('T')[0];
                            const iso = date.toLocaleDateString('en-CA');
                            const weekday = date.getDay();

                            if (holidayDates.has(iso) || weekendDays.has(weekday)) {
                                dayDateEnabled.push(iso);          // Enable in DayDate
                            } else {
                                exchangeDateEnabled.push(iso);     // Enable in ExchangeDate
                            }
                        }
                    }

                    // Enable both fields
                    $('#DayDate, #ExchangeDate').prop('disabled', false);

                    dayDatePicker = flatpickr("#DayDate", {
                        altInput: true,
                        altFormat: "d/m/Y",
                        dateFormat: "Y-m-d",
                        //mode: "multiple",
                        disableMobile: true,
                        allowInput: true,
                        // enable: dayDateEnabled, // optional
                        //onChange: function (selectedDates) {
                        //    if (selectedDates.length) {
                        //        exchangeDatePicker.set('minDate', selectedDates[0]);
                        //    }
                        //},
                        onChange: function (selectedDates) {
                            if (selectedDates.length) {
                                const nextDay = new Date(selectedDates[0]);
                                nextDay.setDate(nextDay.getDate() + 1); // Add one day
                                exchangeDatePicker.set('minDate', nextDay);
                            }
                        },
                        onDayCreate: function (dObj, dStr, fp, dayElem) {
                            const date = dayElem.dateObj;
                            const iso = date.toLocaleDateString('en-CA');
                            const weekday = date.getDay();
                            if (holidayDates.has(iso)) {
                                dayElem.classList.add("flatpickr-holiday");
                                dayElem.setAttribute("title", holidayDates.get(iso));
                            } else if (weekendDays.has(weekday)) {
                                dayElem.classList.add("flatpickr-weekend");
                                dayElem.setAttribute("title", "Weekend");
                            }
                        }
                    });

                    exchangeDatePicker = flatpickr("#ExchangeDate", {
                        altInput: true,
                        altFormat: "d/m/Y",
                        dateFormat: "Y-m-d",
                        //mode: "multiple",
                        disableMobile: true,
                        allowInput: true,
                        enable: exchangeDateEnabled,
                        onChange: function (selectedDates) {
                            if (selectedDates.length) {
                                // Set maxDate on DayDate
                                dayDatePicker.set('maxDate', selectedDates[0]);
                            }
                        },
                        onDayCreate: function (dObj, dStr, fp, dayElem) {
                            const date = dayElem.dateObj;
                            const iso = date.toLocaleDateString('en-CA');
                            const weekday = date.getDay();
                            if (holidayDates.has(iso)) {
                                dayElem.classList.add("flatpickr-holiday");
                                dayElem.setAttribute("title", holidayDates.get(iso));
                            } else if (weekendDays.has(weekday)) {
                                dayElem.classList.add("flatpickr-weekend");
                                dayElem.setAttribute("title", "Weekend");
                            }
                        }
                    });

                });
            });


            //$('#OrganizationID').on('change', function () {
            //    const selectedId = $(this).val();

            //    if (!selectedId) {
            //        $('#DayDate, #ExchangeDate').prop('disabled', true).each(function () {
            //            this._flatpickr?.clear();
            //        });
            //        return;
            //    }

            //    $.getJSON('/OffDayRoster/GetWeekendByOrganization', { id: selectedId }, function (data) {
            //        const holidayDates = new Set();
            //        const weekendDays = new Set();

            //        data.forEach(item => {
            //            // Handle holiday (TotalDays)
            //            if (item.totalDays) {
            //                item.totalDays.split(', ').forEach(d => {
            //                    const [day, month, year] = d.split('/');
            //                    const isoDate = `20${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
            //                    holidayDates.add(isoDate);
            //                });
            //            }

            //            // Handle weekend days
            //            if (item.weekdayNumbers) {
            //                item.weekdayNumbers.split(',').map(Number).forEach(wd => weekendDays.add(wd));
            //            }
            //        });

            //        const currentYear = new Date().getFullYear();
            //        const dayDateEnabled = [];
            //        const exchangeDateEnabled = [];

            //        // Generate both lists by iterating through the full year
            //        for (let month = 0; month < 12; month++) {
            //            for (let day = 1; day <= 31; day++) {
            //                const date = new Date(currentYear, month, day);
            //                if (date.getMonth() !== month) continue; // skip invalid days

            //                //const iso = date.toISOString().split('T')[0];
            //                const iso = date.toLocaleDateString('en-CA');
            //                const weekday = date.getDay();

            //                if (holidayDates.has(iso) || weekendDays.has(weekday)) {
            //                    dayDateEnabled.push(iso);          // Enable in DayDate
            //                } else {
            //                    exchangeDateEnabled.push(iso);     // Enable in ExchangeDate
            //                }
            //            }
            //        }

            //        // Enable both fields
            //        $('#DayDate, #ExchangeDate').prop('disabled', false);

            //        // Init DayDate flatpickr
            //        flatpickr("#DayDate", {
            //            altInput: true,
            //            altFormat: "d/m/Y",
            //            dateFormat: "Y-m-d",
            //            mode: "multiple",
            //            disableMobile: true,
            //            allowInput: true,
            //            enable: dayDateEnabled
            //        });

            //        // Init ExchangeDate flatpickr
            //        flatpickr("#ExchangeDate", {
            //            altInput: true,
            //            altFormat: "d/m/Y",
            //            dateFormat: "Y-m-d",
            //            mode: "multiple",
            //            disableMobile: true,
            //            allowInput: true,
            //            enable: exchangeDateEnabled
            //        });
            //    });
            //});
            // #endregion


            // #region toggleCompensationSelect
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
            // #endregion


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


            // #region CompensationTypeID on change
            $('#CompensationTypeID').on('change', function () {
                const selectedValue = $(this).val();
                if (selectedValue === "3") {
                    $('#ExchangeDateDiv').removeClass('d-none');
                    //$('#rosterInOffDay-dayExchangeModal').modal('show');
                } else {
                    $('#ExchangeDateDiv').addClass('d-none');
                }
            });
            // #endregion
            

        });

        // #region loadTableData
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

                        // NEW CODE: Set date range label
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

            if (!rosterList || rosterList.length === 0) {
                const totalColumns = (uniqueDates.length + 1); // Employee Name + dates
                $('#rosterTableBody').html(`
                    <tr>
                        <td colspan="${totalColumns}" class="text-center py-4 fw-semibold text-muted">
                            No data available
                        </td>
                    </tr>
                `);
                return;
            }

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