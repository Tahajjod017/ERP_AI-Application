(function ($) {
    $.assignSpiralPattern = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            saveBtn: '#assignSpiralPattern-saveBtn',
            saveForm: '#assignSpiralPattern-form',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        $(() => {


            // #region Save
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                var form = $(settings.saveForm);
                var formData = form.serialize();

                $.ajax({
                    url: form.attr('action'),
                    type: 'POST',
                    data: formData,
                    success: function (response) {
                        if (response.isSuccess === true) {
                            toastr.success(response.message);
                        } else {
                            const allFields = ["OrganizationID", "SpiralPatternTypeID", "SpiralPatternID", "StartDate", "EndDate"];

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


            // #region OrganizationID on change
            $('#OrganizationID').on('change', function (e) {
                e.preventDefault();

                var organizationId = $(this).val();
                loadDepartmentsByOrganization(organizationId);
                loadEmpByOrg(organizationId);
                loadSpiralPatternsByOrgPatternType(organizationId, null);
            });
            // #endregion


            // #region loadDepartmentsByOrganization
            function loadDepartmentsByOrganization(organizationId) {
                $.ajax({
                    url: '/AssignSpiralPattern/GetDepartmentByOrganization',
                    type: 'GET',
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
                    url: '/AssignSpiralPattern/GetEmployeeByOrganization',
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


            // #region OrganizationID on change
            $('#SpiralPatternTypeID').on('change', function (e) {
                e.preventDefault();

                var orgId = $('#OrganizationID').val();
                var typeId = $('#SpiralPatternTypeID').val();

                loadSpiralPatternsByOrgPatternType(orgId, typeId);
            });
            // #endregion


            // #region loadSpiralPatternByOrg
            function loadSpiralPatternsByOrgPatternType(orgId, typeId) {
                $.ajax({
                    url: '/AssignSpiralPattern/GetSpiralPatternsByOrgPatternType',
                    type: 'GET',
                    data: {
                        orgId: orgId,
                        typeId : typeId
                    },
                    success: function (spiralPatterns) {
                        const select = document.getElementById('SpiralPatternID');

                        // Destroy existing Choices instance
                        if (spiralPatternDD) {
                            spiralPatternDD.destroy();
                            spiralPatternDD = null;
                        }

                        // Clear old options
                        select.innerHTML = '<option value="">Select Spiral Pattern...</option>';

                        // Group data
                        const grouped = {};
                        spiralPatterns.forEach(sp => {
                            const group = sp.groupName || 'No Spiral Pattern';
                            if (!grouped[group]) {
                                grouped[group] = [];
                            }
                            grouped[group].push(sp);
                        });

                        // Build optgroups
                        for (const group in grouped) {
                            const optgroup = document.createElement('optgroup');
                            optgroup.label = group;

                            grouped[group].forEach(sp => {
                                const option = document.createElement('option');
                                option.value = sp.id;
                                option.text = sp.name;
                                optgroup.appendChild(option);
                            });

                            select.appendChild(optgroup);
                        }

                        // Re-initialize Choices.js
                        spiralPatternDD = new Choices(select, {
                            removeItemButton: true,
                            shouldSort: false,
                            placeholderValue: 'Select Spiral Pattern...'
                        });
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading departments:', error);
                    }
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


            function initSpiralPatternTypeDD() {
                spiralPatternTypeDD = new Choices('#SpiralPatternTypeID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Pattern Type...'
                });
            }
            document.addEventListener('DOMContentLoaded', initSpiralPatternTypeDD);
            initSpiralPatternTypeDD();

            
            function initSpiralPatternDD() {
                spiralPatternDD = new Choices('#SpiralPatternID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Spiral Pattern...'
                });
            }
            document.addEventListener('DOMContentLoaded', initSpiralPatternDD);
            initSpiralPatternDD();
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
        });

    }
}(jQuery));
