(function ($) {
    $.assignSpiralPattern = function (options) {
        // #region Default options
        var settings = $.extend({
            baseUrl: '/',
            saveForm: '#assignSpiralPattern-form',
            editForm: '#assignSpiralPattern-editForm',
            saveBtn: '#assignSpiralPattern-saveBtn',
            editBtn: '#assignSpiralPattern-editBtn',
            updateBtn: '#assignSpiralPatter-updateBtn',
            bulkDelBtn: '#assignSpiralPattern-delSel',
            singleDelBtn: '#assignSpiralPattern-singleDelBtn',
            resetBtn: '#assignSpiralPattern-resetBtn',
            editModal: '#assignSpiralPatternEditModal',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAllAsync";
        var editUrl = settings.baseUrl + "/GetByIdAsync";
        var deleteUrl = settings.baseUrl + "/Delete";
        // #endregion


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
                            clear();
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


            // #region On click edit button GetByIdAsync
            $(document).on('click', settings.editBtn, function (e) {
                e.preventDefault();
                const id = $(this).data('id');

                $.ajax({
                    url: editUrl,
                    type: 'GET',
                    data: { id: id },
                    success: function (result) {
                        const modal = $(settings.editModal);
                        modal.modal('show')

                        $('#SpiralPatternAssignListID').val(result.spiralPatternAssignListID);
                        editOrgSelecetChoices.setChoiceByValue(result.editOrganizationID.toString());

                        // ✅ DEPARTMENT MULTI SELECT
                        $('#EditDepartmentID').val(result.editDepartmentID).each(function () {
                            coreui.MultiSelect.getInstance(this)?.update();
                        });

                        // ✅ EMPLOYEEE MULTI SELECT
                        $('#EditEmployeeID').val(result.editEmployeeID).each(function () {
                            coreui.MultiSelect.getInstance(this)?.update();
                        });

                        editPatternTypeChoices.setChoiceByValue(result.editSpiralPatternTypeID.toString());
                        editSpiralPatternChoices.setChoiceByValue(result.editSpiralPatternID.toString());

                        $('#EditStartDate')[0]._flatpickr.setDate(result.editStartDate, true);
                        $('#EditEndDate')[0]._flatpickr.setDate(result.editEndDate, true);
                    },
                    error: function () {
                        console.log('Something went wrong!');
                    }
                });
            });
            // #endregion


            // #region Delete
            $(document).on('click', settings.bulkDelBtn, function () {
                var selectedItems = $(".assignSpiralPattern-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
                            method: 'POST',
                            data: { ids: selectedIds },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    clear();
                                } else {
                                    toastr.error(response.message);
                                }
                            },
                            error: function () {
                                toastr.error("Error occurred while deleting.");
                            }
                        });
                    });
                } else {
                    toastr.info("Please select at least one item to delete.");
                }
            });

            $(document).on('click', settings.singleDelBtn, function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
                            method: 'POST',
                            data: { ids: [id] },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    clear();
                                } else {
                                    toastr.error(response.message);
                                }
                            },
                            error: function () {
                                toastr.error("Error occurred while deleting.");
                            }
                        });
                    });
                } else {
                    toastr.error("Invalid action.");
                }
            });
            // #endregion


            // #region toggleBulkActions
            $(document).ready(function () {
                $('#assignSpiralPattern-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.assignSpiralPattern-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.assignSpiralPattern-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.assignSpiralPattern-selectItem');
                const checkedItems = $('.assignSpiralPattern-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#assignSpiralPattern-check-all').prop('checked', allChecked);
                $('#assignSpiralPattern-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#assignSpiralPattern-bulkSelectActions').removeClass('d-none');
                    $('#assignSpiralPattern-searchBox').addClass('d-none');
                    $('.assignSpiralPattern-bulkDelete').addClass('disabled');
                    $('.assignSpiralPattern-bulkEdit').addClass('disabled');
                } else {
                    $('#assignSpiralPattern-bulkSelectActions').addClass('d-none');
                    $('#assignSpiralPattern-searchBox').removeClass('d-none');
                    $('.assignSpiralPattern-bulkDelete').removeClass('disabled');
                    $('.assignSpiralPattern-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


            // #region clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();
                clear();
            });


            function clear() {
                $(settings.saveForm)[0].reset();

                const deptSelect = document.getElementById('DepartmentIDs');
                const empSelect = document.getElementById('EmployeeIDs');

                const deptInstance = coreui.MultiSelect.getInstance(deptSelect);
                const empInstance = coreui.MultiSelect.getInstance(empSelect);

                if (deptInstance) {
                    deptInstance.deselectAll();
                }

                if (empInstance) {
                    empInstance.deselectAll();
                }

                if (organizationDD) {
                    organizationDD.destroy();
                }

                if (spiralPatternTypeDD) {
                    spiralPatternTypeDD.destroy();
                }

                if (spiralPatternDD) {
                    spiralPatternDD.destroy();
                }

                resetValidation(['OrganizationID', 'SpiralPatternTypeID', 'SpiralPatternID', 'StartDate', 'EndDate']);

                initOrganizationDD();
                initSpiralPatternTypeDD();
                initSpiralPatternDD();
                toggleBulkActions();
                $('#assignSpiralPattern-check-all').prop('checked', false).prop('indeterminate', false);
                loadTableData();
            }
            // #endregion


            // #region OrganizationID on change
            $('#OrganizationID').on('change', function (e) {
                e.preventDefault();

                var organizationId = $(this).val();
                loadDepartmentsByOrganization(organizationId);
                getEmployeesByOrgBraDepId(organizationId, [], []);
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


            // #region SpiralPatternTypeID on change
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


            // #region DepartmentIDs on change
            document.getElementById('DepartmentIDs').addEventListener('changed.coreui.multi-select', function (event) {
                const orgId = $('#OrganizationID').val();

                const selected = event.value || []; // array of {text, value}
                const depIds = selected.map(x => parseInt(x.value));

                getEmployeesByOrgBraDepId(orgId, depIds);
            });
            // #endregion


            // #region GetEmployeesByOrgBraDepId
            function getEmployeesByOrgBraDepId(orgId, depIds = []) {
                $.ajax({
                    url: '/AssignSpiralPattern/GetEmployeesByOrgBraDepId',
                    type: 'GET',
                    traditional: true,
                    data: {
                        orgId: orgId,
                        depIds: depIds
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


            editOrgSelecetChoices = new Choices('#EditOrganizationID', {
                removeItemButton: true,
                shouldSort: false,
                placeholderValue: 'Select Organization...'
            });

            editPatternTypeChoices = new Choices('#EditSpiralPatternTypeID', {
                removeItemButton: true,
                shouldSort: false,
                placeholderValue: 'Select Spiral Patern Type...'
            });

            editSpiralPatternChoices = new Choices('#EditSpiralPatternID', {
                removeItemButton: true,
                shouldSort: false,
                placeholderValue: 'Select Spiral Patern...'
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


            // #region on click spiral-pattern-link
            $(document).on("click", ".spiral-pattern-link", function () {
                var spiralPatternID = $(this).data("id");
                var spiralPatternTypeID = $(this).data("typeid");

                // Clear all table bodies
                $("#weeklyTblBody, #fortnightlyTblBody, #monthlyTblBody").empty();

                // Hide all tables
                $("#weekly, #fortnightly, #monthly").hide();

                // Fetch data from backend
                $.ajax({
                    url: '/AssignSpiralPattern/GetSpiralPatternDetails',
                    type: 'GET',
                    data: {
                        id: spiralPatternID,
                        typeId: spiralPatternTypeID
                    },
                    success: function (response) {
                        if (!response || !response.length) return;

                        // Decide which table to show based on type
                        if (spiralPatternTypeID === 1) {
                            $("#weekly").show();
                            renderSpiralWeeklyPatternTable(response);
                        }
                        else if (spiralPatternTypeID === 2) {
                            $("#fortnightly").show();
                            renderSpiralBioWeeklyPatternTable(response);
                        }
                        else if (spiralPatternTypeID === 3) {
                            $("#monthly").show();
                            renderSpiralMonthlyPatternTable(response);
                        }
                    }
                });
            });
            // #endregion


            // #region renderSpiralWeeklyPatternTable
            function renderSpiralWeeklyPatternTable(response) {
                var $tableBody = $('#weeklyTblBody');
                $tableBody.empty(); // Clear existing rows

                if (response.length === 0) {
                    $tableBody.append('<tr><td colspan="8" class="text-center">No Data Found</td></tr>');
                    return;
                }

                response.forEach(function (pattern) {
                    var row = '<tr class="hover-actions-trigger btn-reveal-trigger position-static">';
                    row += `<td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                    <h5>${pattern.spiralPatternName}</h5></ br>
                    <p class="fs-9 mb-0">${pattern.organizationName}</p>

                </td>`;

                    // Loop for 7 days (0 = Saturday, 6 = Friday)
                    for (var day = 0; day < 7; day++) {
                        var shiftDetail = pattern.spiralWeeklyPatternDetailsListVMs.find(d => d.dayOfWeek === day);
                        if (shiftDetail && shiftDetail.shiftID > 0) {
                            row += `<td class="startTime align-middle text-center">
                            <div class="position-relative badge badge-phoenix-primary px-4 py-2 day-block" style="border-left:5px solid #A1F1A1;">
                                <p class="my-2 fs-10">${shiftDetail.shiftName}</p>
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                            </div>
                        </td>`;
                        } else {
                            row += `<td class="startTime align-middle text-center">
                            <p class="my-2 fs-10">-</p>
                        </td>`;
                        }
                    }

                    row += '</tr>';
                    $tableBody.append(row);
                });
            }
            // #endregion


            // #region renderSpiralBioWeeklyPatternTable
            function renderSpiralBioWeeklyPatternTable(response) {
                var $tableBody = $('#fortnightlyTblBody');
                $tableBody.empty(); // Clear existing rows

                if (response.length === 0) {
                    $tableBody.append('<tr><td colspan="15" class="text-center">No Data Found</td></tr>');
                    return;
                }

                response.forEach(function (pattern) {
                    var row = '<tr class="hover-actions-trigger btn-reveal-trigger position-static">';
                    row += `<td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                    <h5>${pattern.spiralBioWeeklyPatternName}</h5></ br>
                    <p class="fs-9 mb-0">${pattern.organizationName}</p>

                </td>`;

                    // Loop for 7 days (0 = Saturday, 6 = Friday)
                    for (var day = 0; day < 14; day++) {
                        var shiftDetail = pattern.spiralBioWeeklyPatternDetailsListVMs.find(d => d.dayOfMonth === day);
                        if (shiftDetail && shiftDetail.shiftID) {
                            // Example: you need to format Shift Time here (hardcoded now)
                            row += `<td class="startTime align-middle text-center">
                            <div class="position-relative badge badge-phoenix-primary px-4 py-2 day-block" style="border-left:5px solid #A1F1A1;">
                                <p class="my-2 fs-10">${shiftDetail.shiftName}</p>
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                            </div>
                        </td>`;
                        } else {
                            row += `<td class="startTime align-middle text-center">
                            <p class="my-2 fs-10">-</p>
                        </td>`;
                        }
                    }

                    row += '</tr>';
                    $tableBody.append(row);
                });
            }
            // #endregion


            // #region renderSpiralMonthlyPatternTable
            function renderSpiralMonthlyPatternTable(response) {
                var $tableBody = $('#monthlyTblBody');
                $tableBody.empty(); // Clear existing rows

                if (response.length === 0) {
                    $tableBody.append('<tr><td colspan="30" class="text-center">No Data Found</td></tr>');
                    return;
                }

                response.forEach(function (pattern) {
                    var row = '<tr class="hover-actions-trigger btn-reveal-trigger position-static">';
                    row += `<td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2">
                    <h5>${pattern.spiralMonthlyPatternName}</h5></ br>
                    <p class="fs-9 mb-0">${pattern.organizationName}</p>

                </td>`;

                    // Loop for 7 days (0 = Saturday, 6 = Friday)
                    for (var day = 0; day < 30; day++) {
                        var shiftDetail = pattern.spiralMonthlyPatternDetailsListVMs.find(d => d.dayOfMonth === day);
                        if (shiftDetail && shiftDetail.shiftID) {
                            // Example: you need to format Shift Time here (hardcoded now)
                            row += `<td class="startTime align-middle text-center">
                            <div class="position-relative badge badge-phoenix-primary px-4 py-2 day-block" style="border-left:5px solid #A1F1A1;">
                                <p class="my-2 fs-10">${shiftDetail.shiftName}</p>
                                <p class="my-2 fs-10">${shiftDetail.shiftTime}</p>
                            </div>
                        </td>`;
                        } else {
                            row += `<td class="startTime align-middle text-center">
                            <p class="my-2 fs-10">-</p>
                        </td>`;
                        }
                    }

                    row += '</tr>';
                    $tableBody.append(row);
                });
            }
            // #endregion
        });


        // #region Table with Pagination
        var currentPage = 1;
        var pageSize = 5;

        $('#assignSpiralPattern-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#assignSpiralPattern-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#assignSpiralPattern-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#assignSpiralPattern-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'ShiftID';
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
            var searchTerm = $("#assignSpiralPattern-searchInput").val();
            var organizationID = $("#assignSpiralPattern-dd-search").val();
            $.ajax({
                url: gridUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    organizationID: organizationID
                },
                success: function (response) {
                    var tableBody = $("#assignSpiralPattern-tBody");
                    tableBody.empty();

                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            tableBody.append(`
                                <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input assignSpiralPattern-selectItem" data-id="${item.spiralPatternAssignListID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-2 ps-3">
                                        <h5>${item.organizationName}</h5>
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-2 ps-3">
                                        <h5>${item.departmentName}</h5>
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-2 ps-3">
                                        <h5>${item.employeeName}</h5>
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-1 ps-3">
                                        <span>${item.spiralPatternTypeName}</span>
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-1 ps-3">
                                        <a href="#" class="nav-item mx-2 text-decoration-none spiral-pattern-link" 
                                            data-bs-toggle="modal" 
                                            data-bs-target="#spiral_pattern_details"
                                            data-id="${item.spiralPatternID}" 
                                            data-typeid="${item.spiralPatternTypeID}">
                                            <span>${item.spiralPatternName}</span>
                                        </a>
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-1 ps-3">${item.startDate}</td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body py-1 ps-3">${item.endDate}</td>
                                    <td class="text-end align-middle white-space-nowrap py-1 ps-3">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon assignSpiralPattern-bulkEdit me-2"
                                                id="assignSpiralPattern-editBtn"
                                                data-id="${item.spiralPatternAssignListID}" >
                                                <i class="fas fa-edit text-black"></i>
                                            </a>
                                            <a href="#!" class="btn btn-outline-light btn-icon assignSpiralPattern-bulkDelete"
                                                id="assignSpiralPattern-singleDelBtn"
                                                data-id="${item.spiralPatternAssignListID}" >
                                                <i class="far fa-trash-alt text-black"></i>
                                            </a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="9" class="text-center">No data available</td></tr>');
                    }

                    var separatePaginationInfo = response.separatePaginationInfo;

                    $("#assignSpiralPattern-paginationInfo").text(`Showing ${separatePaginationInfo.startItem} to ${separatePaginationInfo.endItem} Items of ${separatePaginationInfo.totalItems}`);
                    $("#assignSpiralPattern-totalCount").text(`(${separatePaginationInfo.totalItems})`);

                    updatePagination(separatePaginationInfo.pageNumbers, separatePaginationInfo.currentPage, separatePaginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#assignSpiralPattern-paginationLinks");
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
            $("#assignSpiralPattern-prevPageBtn").prop('disabled', currentPage === 1);
            $("#assignSpiralPattern-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));
