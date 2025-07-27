
$(document).ready(function () {

    $('.one').on('changed.coreui.multi-select', function (event) {
        const target = event.target;

        if (target && target.id === 'OrganizationID') {
            const selectedOrgId = $(target).val();
            if (selectedOrgId) {
                loadDepartmentsByCompany(selectedOrgId);
                loadEmplooyeesByCompany(selectedOrgId);
                currentPage = 1;
                loadTableData();
            } else {
                currentPage = 1;
                loadTableData();
            }
        }
    });

    function loadDepartmentsByCompany(organizationId) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/LeaveRequest/GetDepartmentByCompany',
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
    function loadDepartmentsByCompany(organizationId) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/LeaveRequest/GetDepartmentByCompany',
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
    function loadEmplooyeesByCompany(organizationId) {
        $.ajax({
            url: '/LeaveRequest/GetEmployeeByCompany',
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
    document.querySelector('.two').addEventListener('changed.coreui.multi-select', function (event) {
        const target = event.target;

        if (target.id === 'DepartmentIDs') {
            loadFilteredEmployees();
            currentPage = 1;
            loadTableData();
        }
    });
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



    function loadFilteredEmployees(employeeIDs = []) {
        var deptIds = $('#DepartmentIDs').val() || [];

        if (!Array.isArray(deptIds)) deptIds = [deptIds];

        $.ajax({
            url: '/LeaveRequest/GetEmployeeByDepartment',
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
    //
    //
    //Get Employee according to LoginID
    GetAllEmpoyee();
    function GetAllEmpoyee() {
        $.ajax({
            url: '/LeaveRequest/GetEmployee',
            type: 'GET',
            success: function (data) {

                choiceManager.populateDropdown('EmployeeID', data);
                choiceManager.populateDropdown('EmployeeIDEdit', data);
                if (data.length === 1) {
                    var firstData = data[0];
                    choiceManager.setChoiceValue('EmployeeID', firstData.id);
                }

            },
            error: function () {
                toastr.error('Failed to retrieve employee data.');
            }
        });
    }
    initializeDatepickerDMY('TransferDate');


    
    //#region Save Data
  $(document).on('click', '#EmpTransferButton', function (e) {
      e.preventDefault();

      const data = {
          EmployeeID: $('#EmployeeID').val(),
          FromOrganizationID: $('#FromOrganizationID').val(),
          ToOrganizationID: $('#ToOrganizationID').val(),
          FromOrganizationBranchID: $('#FromOrganizationBranchID').val(),
          ToOrganizationBranchID: $('#ToOrganizationBranchID').val(),
          TransferDate: $('#TransferDate').val(),
          TransferNote: $('#TransferNote').val(),
          ToDesignationID: $('#ToDesignationID').val(),
          FromDesignationID: $('#FromDesignationID').val(),
          ToDepartmentID: $('#ToDepartmentID').val(),
          FromDepartmentID: $('#FromDepartmentID').val()
      };
     
      $.ajax({
          url: '/EmployeeTransferManagement/SaveEmplopyeeTransfer', // 🔁 Change to your correct controller/action
          type: 'POST',
          data: data,
          success: function (response) {
              if (response.success) {
                  toastr.success(response.message);
                  resetForm();
                  var applyModalEl = document.getElementById('apply_leave');
                  var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                  if (!applyModal) {
                      applyModal = new bootstrap.Modal(applyModalEl);
                  }
                  applyModal.hide();
              } else {
                  toastr.error(response.message);
              }
          },
          error: function (xhr, status, error) {
              console.error("AJAX Error:", error);
              toastr.error(response.message);
          }
      });
  });

    function resetForm() {
        $('#employeeTransferForm,#employeeTransferFormEdit')[0].reset();
        choiceManager.resetChoice('EmployeeID', 'FromOrganizationID', 'ToOrganizationID', 'FromOrganizationBranchID', 'ToOrganizationBranchID', 'FromDepartmentID', 'FromDesignationID', 'ToDepartmentID', 'ToDesignationID');
        choiceManager.resetChoice('EmployeeIDEdit', 'FromOrganizationIDEdit', 'ToOrganizationIDEdit', 'FromOrganizationBranchIDEdit', 'ToOrganizationBranchIDEdit');
        loadTableData();
    }

    $('#EmpTransferButtonReset,#EmpTransferButtonResetEdit').on('click', function () {
        resetForm();
    });
    //#endregion

    //#region GetBy EmpoyeeTransfer
    $(document).on('click', '#EmpTransferButtonEdit', function (e) {
        e.preventDefault();

        let transferId = $(this).data('id');
        console.log('GetByID' + transferId);
        $.ajax({
            url: '/EmployeeTransferManagement/GetEmployeeTransferByIdAsync', 
            type: 'GET',
            data: { employeeTransferID: transferId },
            success: function (response) {
                if (!response.success) {
                    toastr.warning(response.message || 'Record not found.');
                    return;
                }
                const data = response.data;
                choiceManager.setChoiceValue('EmployeeIDEdit', data.employeeIDEdit);
                choiceManager.setChoiceValue('FromOrganizationIDEdit', data.fromOrganizationIDEdit);
                choiceManager.setChoiceValue('FromOrganizationBranchIDEdit', data.fromOrganizationBranchIDEdit);
                choiceManager.setChoiceValue('ToOrganizationIDEdit', data.toOrganizationIDEdit);
                choiceManager.setChoiceValue('ToOrganizationBranchIDEdit', data.toOrganizationBranchIDEdit);
                $('#TransferDateEdit').val(data.transferDateEdit);
                $('#TransferNoteEdit').val(data.transferNoteEdit);
                $('#EmployeeTransferID').val(data.employeeTransferID);
                initializeDatepickerDMY('TransferDateEdit');
            },
            error: function () {
                toastr.error('Failed to load data.');
            }
        });
    });
    $(document).on('click', '#EmpTransferButtonUpdateSubmit', function (e) {
        e.preventDefault();
        const payload = {
            EmployeeTransferID: parseInt($('#EmployeeTransferID').val()) || 0, 
            EmployeeIDEdit: parseInt($('#EmployeeIDEdit').val()) || null,
            FromOrganizationIDEdit: parseInt($('#FromOrganizationIDEdit').val()) || null,
            FromOrganizationBranchIDEdit: parseInt($('#FromOrganizationBranchIDEdit').val()) || null,
            ToOrganizationIDEdit: parseInt($('#ToOrganizationIDEdit').val()) || null,
            ToOrganizationBranchIDEdit: parseInt($('#ToOrganizationBranchIDEdit').val()) || null,
            TransferDateEdit: $('#TransferDateEdit').val() || null, 
            TransferNoteEdit: $('#TransferNoteEdit').val() || ""
        };


        $.ajax({
            url: '/EmployeeTransferManagement/UpdateEmployeeTransferAsync', 
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (response)
            {
              
                if (response.success) {
                    toastr.success(response.message || 'Updated successfully');
                    resetForm();
                    var applyModalEl = document.getElementById('edit_leaves');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                  
                } else {
                    toastr.warning(response.message || 'Update failed');
                }
            },
            error: function () {
               
                toastr.error('An error occurred while updating.');
            }
        });
    });

   
    //#endregion


    //#region Delete Soft Leave Request
    $(document).on('click', '#leaveRequestDelete-singleDelBtn', function () {
        var id = $(this).data('id');

        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/EmployeeTransferManagement/SoftDeleteEmpTransfer',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {

                        if (response.success)
                        {
                            toastr.success(response.message);
                            loadTableData();
                        } else
                        {
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
    //#endregion

    //region Get EmpOrganizationWithBranch according to employee
    $(document).on('change', '#EmployeeID,#EmployeeEdit', function (e) {
        e.preventDefault();
        var employeeID = $(this).val();
        if (!employeeID) {
            // Clear dependent fields
            choiceManager.setChoiceValue('FromOrganizationID', '');
            choiceManager.setChoiceValue('FromOrganizationBranchID', '');
            choiceManager.setChoiceValue('FromOrganizationIDEdit', '');
            choiceManager.setChoiceValue('FromOrganizationBranchIDEdit', '');
            choiceManager.setChoiceValue('FromDepartmentID', '');
            choiceManager.setChoiceValue('FromDesignationID', '');
           
                 

            return;
        }

        $.ajax({
            url: '/EmployeeTransferManagement/GetEmpOrganizationBranchId',
            type: 'GET',
            data: { employeeID: employeeID },
            success: function (response) {
                if (!response || !response.success || !response.data)
                {
                    toastr.warning('Employee data not found.');
                    return;
                }
                const data = response.data;
                choiceManager.setChoiceValue('FromOrganizationID', data.fromOrganizationID);
                choiceManager.setChoiceValue('FromOrganizationBranchID', data.fromOrganizationBranchID);
                choiceManager.setChoiceValue('FromOrganizationIDEdit', data.fromOrganizationID);
                choiceManager.setChoiceValue('FromOrganizationBranchIDEdit', data.fromOrganizationBranchID);
                choiceManager.setChoiceValue('FromDepartmentID', data.fromDepartmentID);
                choiceManager.setChoiceValue('FromDesignationID', data.fromDesignationID);
                
            },
            error: function () {
                toastr.error('Failed to fetch organization and branch information.');
            }
        });
    });

    //organizationBranch according  Organization
    $(document).on('change', '#ToOrganizationID,#ToOrganizationIDEdit', function (e) {
        e.preventDefault();

        var toOrganizationID = $(this).val();

        if (!toOrganizationID) {
            // Clear the branch selection if organization is deselected
            choiceManager.setChoiceValue('ToOrganizationBranchID', '');
            choiceManager.setChoiceValue('ToOrganizationBranchIDEdit', '');
            return;
        }

        $.ajax({
            url: '/EmployeeTransferManagement/GetEmpBranchId',
            type: 'GET',
            data: { toOrganizationID: toOrganizationID },
            success: function (response) {
                if (!response || !response.success || !response.data) {
                    toastr.warning('Branch data not found.');
                    choiceManager.setChoiceValue('ToOrganizationBranchID', '');
                    choiceManager.setChoiceValue('ToOrganizationBranchIDEdit', '');
                    return;
                }

                const data = response.data;

                // ✅ Set the Branch dropdown value
                choiceManager.setChoiceValue('ToOrganizationBranchID', data.id);
            },
            error: function () {
                toastr.error('Failed to fetch branch information.');
            }
        });
    });

    //#endregion

   
});




// #region 🟣 Get Employee Avatar HTML (Initial or Image)
function getAvatarHtml(employee) {
    if (employee.employeeImage && employee.employeeImage !== '') {
        return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
    } else {
        const initial = employee.employeeName.charAt(0).toUpperCase();
        return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
    }
}
// #endregion


// #region  Data Table for Peresonal
var currentPage = 1;
var pageSize = 5;

$('#leaveRequest-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});

$(document).ready(function () {
    loadTableData();

    $("#leaveRequest-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#leaveRequest-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#leaveRequest-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});
let currentSortColumn = '';
let currentSortOrder = '';

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

$(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

    currentPage = 1;
    loadTableData();
});

$('#EmployeeIDs').on('changed.coreui.multi-select', function () {
    currentPage = 1;
    loadTableData(); // Make AJAX call or reload the table
});

// Filtering according to formdate to ToDate
initializeGlobalDateRangePicker(
    'basic-daterange',
    'basic-daterange_fromHidden',
    'basic-daterange_toHidden',
    function () {
        currentPage = 1;
        loadTableData();
    }
);
function loadTableData(currentSortColumn, currentSortOrder) {
    var searchTerm = $("#leaveRequest-searchInput").val();
    const organizationId = $('#OrganizationID').val();
    const departmentIds = $('#DepartmentIDs').val() || [];
    const employeeIds = $('#EmployeeIDs').val() || [];
    const fromDate = $('#basic-daterange_fromHidden').val();
    const toDate = $('#basic-daterange_toHidden').val();
    console.log("Dept: " + departmentIds + " | Emp: " + employeeIds + " | Org: " + organizationId);
    console.log("From: " + fromDate + " | To: " + toDate);

    $.ajax({
        url: '/EmployeeTransferManagement/GetAllTableListAsync',
        method: 'GET',
        traditional: true,
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            currentSortColumn: currentSortColumn,
            currentSortOrder: currentSortOrder,
            organizationId: organizationId,
            departmentIds: departmentIds,
            employeeIds: employeeIds,
            fromDate: fromDate,
            toDate: toDate
        },
        success: function (response) {



            console.log("Datassssss", response);
            var tableBody = $("#leaveRequest-tBody");
            tableBody.empty();
            var totalItems = response.paginationInfo.totalItems;

            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {

                    if (currentSortOrder === 'asc') {
                        rowIndex = (currentPage - 1) * pageSize + index + 1;
                    } else {
                        rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
                    }

                 
                    const avatar = getAvatarHtml(item);
                    tableBody.append(`
                       <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        
                        <td class="fs-9 align-middle py-0">
                          <div class="form-check mb-0 fs-8">
                            <input class="form-check-input" data-id="${item.employeeTransferID}" type="checkbox" />
                          </div>
                        </td>
  
                        
                        <td class="approveByEmployee align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                          <div class="d-flex align-items-center file-name-icon">
                            <div class="avatar avatar-m avatar-bordered me-2">
                             ${avatar}
                            </div>
                            <div class="ms-1">
                              <h6 class="fw-bold">${item.employeeName}</h6>
                              <span class="fs-12 fw-normal ">${item.employeeDepartment || 'HRM'}</span>
                            </div>
                          </div>
                        </td>
                        
                     
                         <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.fromOrganizationName || ''}</td>
                          <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.fromOrganizationBranchName || ''}</td>

                         <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.toOrganizationName || ''}</td>
                          <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.toOrganizationBranchName || ''}</td>
                        <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.transferDate || ''}</td>
                       
                        
                     <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="EmpTransferButtonEdit"
                               data-id="${item.employeeTransferID}"
                               class="btn btn-outline-light btn-icon me-1" 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves">
                               <i class="fas fa-edit text-black"></i>
                    </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.employeeTransferID}"
                              class="btn btn-outline-light btn-icon"  
                              id="leaveRequestDelete-singleDelBtn" >
                              <i class="far fa-trash-alt text-black"></i>
                            </a>
                          </div>
                    </td>

  
                      </tr>
                   `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#leaveRequest-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#leaveRequest-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#leaveRequest-paginationLinks");
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
    $("#leaveRequest-prevPageBtn").prop('disabled', currentPage === 1);
    $("#leaveRequest-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});
//#endregion
