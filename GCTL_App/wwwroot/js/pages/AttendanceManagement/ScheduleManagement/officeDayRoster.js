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
        $(() => {


            $('body').on('click', '#rosterInOfficeDays-resetBtn', function () {
                const orgSelect = document.getElementById('OrganizationID');
                const orgInstance = coreui.MultiSelect.getInstance(orgSelect);
                if (orgInstance) {
                    orgInstance.deselectAll();
                }

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
            });


            //const myMutliSelect = document.getElementById('myMutliSelect')
            //myMutliSelect.addEventListener('changed.coreui.multi-select', event => {
            //    // Get the list of selected options.
            //    const selected = event.value
            //});

            $('body').on('changed.coreui.multi-select', function (event) {
                event.preventDefault();
                event.stopPropagation();
                event.stopImmediatePropagation();
                const target = event.target;
                toastr.info('working');
                if (target && target.id === 'OrganizationID') {
                    const selectedOrgId = $(target).val();
                    if (selectedOrgId) {
                        loadDepartmentsByCompany(selectedOrgId);
                        //loadEmplooyeesByCompany(selectedOrgId);
                        //loadShiftsByCompany(selectedOrgId);
                    } else {
                        clearDepartmentDropdown();
                    }
                }
            });




            function loadDepartmentsByCompany(organizationId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/OfficeDayRoster/GetDepartmentByCompany',
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
                const container = document.querySelector('.two'); 
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