(function ($) {
    $.officedayroster = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#rosterInOfficeDays-form',
            saveBtn: '#rosterInOfficeDays-saveBtn',
            resetBtn: '#rosterInOfficeDays-resetBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        let organizationDD;
        let shiftDD;
        $(() => {



            $(document).ready(function () {
                $('#basic-daterange').dateRangePicker({
                    format: 'DD/MM/YYYY',
                    separator: ' to ',
                    language: 'en',
                    autoClose: true,
                    getValue: function () {
                        return $(this).val();
                    },
                    setValue: function (s) {
                        $(this).val(s);
                    }
                }).bind('datepicker-change', function (event, obj)
                {
                    const start = moment(obj.date1).format("YYYY-MM-DD");
                    const end = moment(obj.date2).format("YYYY-MM-DD");
                    console.log("FromDatePairDate" + start);
                    console.log("ToDatePairDate" + end);
                    $('#basic-daterange_fromHidden').val(start);
                    $('#basic-daterange_toHidden').val(end);
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



        });
    }
}(jQuery));