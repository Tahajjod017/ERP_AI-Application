

$(document).ready(function () {


   


    //


    //

    $('.one').on('changed.coreui.multi-select', function (event) {
        const target = event.target;

        if (target && target.id === 'OrganizationID') {
            const selectedOrgId = $(target).val();
            if (selectedOrgId) {
                loadDepartmentsByCompany(selectedOrgId);
                loadEmplooyeesByCompany(selectedOrgId);
               
            } else {
               
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
            url: '/AssignDefaultShift/GetEmployeeByDepartment',
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

   
    initializeDatepickerDMY("FromDate, ToDate,oDateFromDateCombined");
    
    $(document).on('change', "#FromDate", function ()
    {
        updateDatepickerWithMinDate("ToDate", $("#FromDate").val());

    })

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
               
                if (data.length === 1)
                {
                    var firstData = data[0];
                    choiceManager.setChoiceValue('EmployeeID', firstData.id); 
                }
                
            },
            error: function ()
            {
                toastr.error('Failed to retrieve employee data.');
            }
        });
    }

    GetLeavePolicyIsCountAsync();
    function GetLeavePolicyIsCountAsync() {
        $.ajax({
            url: '/LeaveRequest/GetLeavePolicyIsCountAsync',
            type: 'GET',
            success: function (data) {
               
                if (data.length > 0 && (data[0].isWeekendCountedAsLeave  || data[0].isHolidayCountedAsLeave )) {
                    $('#SubsequentHolydayDays').val('');
                } else {
                    $('#SubsequentHolydayDays').val('Not Applicable');
                }
                
            },
            error: function () {
                toastr.error('Failed to retrieve data.');
            }
        })
    }
   

    function GetleaveDaysOrAvailble(employeeId, leaveTypeID) {
        if (leaveTypeID && employeeId) {
            $.ajax({
                url: '/LeaveRequest/GetLeaveDays',
                type: 'GET',
                data: { employeeId: employeeId, leaveTypeId: leaveTypeID },
                success: function (data) {
                    if (data && data.leaveDays !== null) {
                        $('#LeaveDays').val(data.leaveDays);
                        $('#LeaveDaysEdit').val(data.leaveDays);
                    } else {
                        $('#LeaveDays').val('0');
                        $('#LeaveDaysEdit').val('0');
                    }
                },
                error: function () {
                    toastr.error('Failed to fetch leave days.');
                    $('#LeaveDays').val('0');
                    $('#LeaveDaysEdit').val('0');
                }
            });
        } else {
            $('#LeaveDays').val('');
            $('#LeaveDaysEdit').val('');
        }
    }

    //

 

    //
    function handleLeaveChange(employeeIdField, leaveTypeIdField) {
        var leaveTypeID = $(leaveTypeIdField).val();
        var employeeId = choiceManager.getChoiceValue(employeeIdField);
        GetleaveDaysOrAvailble(employeeId, leaveTypeID);
    }

    // Bind events for both Add and Edit forms
    $('#EmployeeID,#LeaveTypeID').on('change', () =>
        handleLeaveChange('EmployeeID', '#LeaveTypeID')
    );

    $('#EmployeeIDEdit,#LeaveTypeIDEdit').on('change', () =>
        handleLeaveChange('EmployeeIDedit', '#LeaveTypeIDEdit')
    );
  
   
    toggleTimeDateValidation();

    $('#PartialFromTime, #PartialToTime').on('input change', function () {
        var $this = $(this);
        var val = $this.val().trim();
        if (val !== "") {
            $this.removeClass('is-invalid input-validation-error');
            $this.siblings('.text-danger').text('');
            $this.valid(); // Trigger re-validation
        }
    });

   

    function toggleTimeDateValidation() {
        if ($('#IsFullDay').is(':checked')) {
            // Enable required for FromDate and ToDate
            $('#FromDate').attr('required', 'required');
            $('#ToDate').attr('required', 'required');

            // Disable required for Partial times
            $('#PartialFromTime').removeAttr('required');
            $('#PartialToTime').removeAttr('required');
            $('#ToDateFromDateCombined').removeAttr('required');
        } else {
            // Enable required for Partial times
            $('#PartialFromTime').attr('required', 'required');
            $('#PartialToTime').attr('required', 'required');
            $('#ToDateFromDateCombined').attr('required', 'required');
            // Disable required for full-day fields
            $('#FromDate').removeAttr('required');
            $('#ToDate').removeAttr('required');
        }
    }

  
    $('#IsFullDay').on('change', function () {
     
        toggleTimeDateValidation();
    });
    //

      
    //



   
   
    function GetLeavedaysSubsequent(fromDate, toDate) {
    
        if (!fromDate || !toDate) return;

        $.ajax({
            url: '/LeaveRequest/SubsequentLeaveCount',
            type: 'GET',
            data: {
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (data) {
                
                if (!data) return;
                if (data && data.totalSubsequentDays > 0) {
                    $('#SubsequentHolydayDays').val(data.totalSubsequentDays);
                    $('#SubsequentHolydayDaysTT').val(data.totalSubsequentDays);
                } else if (!data.isHolidayCountedAsLeave && !data.isWeekendCountedAsLeave) {
                    $('#SubsequentHolydayDays').val("Not Applicable");
                    $('#SubsequentHolydayDaysTT').val("Not Applicable");
                } else {
                    $('#SubsequentHolydayDays').val("0");
                    $('#SubsequentHolydayDaysTT').val("0");
                }
                if (typeof data.totalDays !== 'undefined') {
                    $('#TotalAppliedDays').val(data.totalDays);
                    $('#TotalAppliedDaysTT').val(data.totalDays);
                    
                }
            }
            ,
            error: function () {
                toastr.error('Failed to fetch subsequent.');
            }
        });
    }

 
    $(document).on('change', '#FromDate, #ToDate', function (e) {
        e.preventDefault();

        let fromDate = flatpickrHelper.getDate('FromDate');
        let toDate = flatpickrHelper.getDate('ToDate');
        GetLeavedaysSubsequent(fromDate, toDate);
    });
    //
    $(document).on('change', '#FromDateEdit, #ToDateEdit', function (e) {
        e.preventDefault();

        let fromDate = flatpickrHelper.getDate('FromDateEdit');
        let toDate = flatpickrHelper.getDate('ToDateEdit');
        GetLeavedaysSubsequent(fromDate, toDate);

    });


   

    //

    // Handle form submit
    $('body').on('submit', '#LeaveRequestForm', function (e) {
        e.preventDefault();
       
        var $form = $(this);

        if (!$form.valid()) {

            return false;
        }
        var available = parseFloat($('#LeaveDays').val()) || 0;
        var applied = parseFloat($('#TotalAppliedDays').val()) || 0;

       

        if (applied > available) {
            toastr.error(`You have ${available} day(s) available, but you tried to apply for ${applied}.`);

            $('#exceedAnnualLeaveModal .modal-body').text(
                `You have ${available} day(s) available, but you tried to apply for ${applied}.`
            );

            var modal = new bootstrap.Modal(document.getElementById('exceedAnnualLeaveModal'));
            modal.show();

            return false;
        }



        var url = $form.attr('action');
        var formData = new FormData(this);
        
      
        $.ajax({
            type: 'POST',
            url: url,
            data: formData,
            contentType: false,
            processData: false,
            dataType: 'json',
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success(response.message);
                    resetForm(); // Reset after successful save
                } else {
                    // Show server-side validation errors
                    if (response.errors && response.errors.length > 0) {
                        response.errors.forEach(function (error) {
                            toastr.error(error);
                        });
                    } else {
                        toastr.error(response.message);
                    }
                }
            },
            error: function () {
                toastr.error("An unexpected error occurred.");
            }
        });
    });

   

    // Reset button click
    $('#ResetButton').on('click', function () {
        resetForm();
    });

    // Reset logic function
    function resetForm() {

        choiceManager.clearChoice('EmployeeID');
        choiceManager.clearChoice('LeaveTypeID');

        // Reset text inputs and textareas
        $('#Reason').val('');
        $('#FromDate').val('');
        $('#ToDate').val('');
        $('#ToDateFromDateCombined').val('');
        $('#PartialFromTime').val('');
        $('#PartialToTime').val('');
        $('#TotalAppliedDays').val();
        // Reset validation states
        $('#ToDateFromDateCombined').removeClass('is-invalid');
        $('#ToDateFromDateCombinedError').hide().text('');
        $('#FromDate').removeClass('is-invalid');
        $('#FromDateError').hide().text('');
        $('#EmployeeID').removeClass('is-invalid');
        $('#EmployeeIDError').hide().text('');
        $('#PartialFromTime, #PartialToTime').removeClass('is-invalid input-validation-error');
        $('#PartialFromTime, #PartialToTime').siblings('.text-danger').text('');
        loadTableData();
        // Reset flatpickr instances
        ['#FromDate', '#ToDate', '#ToDateFromDateCombined', '#PartialFromTime', '#PartialToTime'].forEach(function (id) {
            if ($(id)[0] && $(id)[0]._flatpickr) {
                $(id)[0]._flatpickr.clear();
            }
        });

    }

    // Delete Soft Leave Request

    //
    $(document).on('click', '#leaveRequestDelete-singleDelBtn', function () {
        var id = $(this).data('id');
        
        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/LeaveRequestRoute/SofteDeleteLeaveRequest',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {
                    
                        if (response.success) {
                            toastr.success(response.message);
                            loadTableData();
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
    //

    // Get By data leaveRequest
    $(document).on('click', '#LeaveRequestEditButton', function () {
        var leaveApplicationID = $(this).data('id');
     
        $.ajax({
            url: '/LeaveRequestRoute/GetLeaveRequestByIdAsync',
            type: 'GET',
            data: { leaveApplicationID: leaveApplicationID },
            success: function (data) {
              
                console.log("Data GetBy LeaveRequest", data);
                if (data && Object.keys(data).length > 0) {

                    // Set hidden ID
                    $('#LeaveApplicationID').val(data.leaveApplicationID);
                    choiceManager.setChoiceValue('EmployeeIDEdit', data.employeeIDEdit);
                    choiceManager.setChoiceValue('LeaveTypeIDEdit', data.leaveTypeIDEdit);
                    flatpickrHelper.setDate('ToDateFromDateCombinedEdit', data.fromDateEdit);
                    $('#TotalAppliedDaysTT').val(data.period);
                    $('#SubsequentHolydayDaysTT').val(data.totalSubsequentDays);
                    $('#LeaveDaysEdit').val(data.leaveDaysEdit);
                    $('input[name="IsFullDayEdit"][value="' + data.isFullDayEdit + '"]').prop('checked', true).trigger('change');
                    $('input[name="FromDateEdit"]').val(data.fromDateEdit);
                    $('input[name="ToDateEdit"]').val(data.toDateEdit);
                    $('#ToDateFromDateCombinedEdit').val(data.fromDateEdit);
                    $('input[name="ToDateFromDateCombinedEdit"]').val(data.toDateEdit);
                    $('input[name="PartialFromTimeEdit"]').val(data.partialFromTimeEdit);
                    $('input[name="PartialToTimeEdit"]').val(data.partialToTimeEdit);
                    $('textarea[name="ReasonEdit"]').val(data.reasonEdit);

                    // Optionally toggle the sections based on IsFullDayEdit
                    if (data.isFullDayEdit === true) {
                        $('#FullDayDivEdit').removeClass('d-none');
                        $('#PartialDayDivEdit').addClass('d-none');
                    } else {
                        $('#FullDayDivEdit').addClass('d-none');
                        $('#PartialDayDivEdit').removeClass('d-none');
                    }
                   
                    
                    if (data.totalSubsequentDays > 0) {
                        $('#SubsequentHolydayDays').val(data.totalSubsequentDays);
                    } else if (!data.isHolidayCountedAsLeave && !data.isWeekendCountedAsLeave) {
                        $('#SubsequentHolydayDays').val("Not Applicable");
                    } else {
                        $('#SubsequentHolydayDays').val("0");
                    }
                }
            },

            error: function () {
                toastr.error("Error leave request get by Id.");
            }
        })
    })


    //



   
    
    //


});




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



function getBadgeClass(status) {
    if (!status || status.trim() === '') return 'text-bg-success';

    switch (status.trim().toUpperCase()) {
        case 'DECLINEED':
            return 'badge-phoenix badge-phoenix-danger';
        case 'APPROVED':
            return 'badge-phoenix badge-phoenix-success';
        case 'PENDING':
            return 'badge-phoenix-warning';
        default:
            return 'text-bg-success';
    }
}

function getAvatarHtml(employee) {
    if (employee.employeeImage && employee.employeeImage !== '')
    {
        return `<img class="rounded-circle" src="${employee.employeeImage}" alt="${employee.employeeName}" />`;
    } else {
        const initial = employee.employeeName.charAt(0).toUpperCase();
        return `<div class="avatar-initial rounded-circle bg-primary text-white d-flex align-items-center justify-content-center" style="height: 100%;">${initial}</div>`;
    }
}

$(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

    currentPage = 1;
    loadTableData();
});

function loadTableData(currentSortColumn, currentSortOrder) {
    var searchTerm = $("#leaveRequest-searchInput").val();
    var leaveTypeID = $('#LeaveTypeIDFilterDD').val();
    var statusID = $('#StatusIDFilterDD').val();
   
  
    $.ajax({
        url: '/LeaveRequestRoute/GetAllTableListAsync',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            currentSortColumn: currentSortColumn,
            currentSortOrder: currentSortOrder,
            leaveTypeID: leaveTypeID,
            statusID: statusID
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
                    //
           
                    //
                    let status = item.statusName; // Assuming this is your status value
                    let isDisabled = status && (status.toUpperCase() === 'APPROVED' || status.toUpperCase() === 'DECLINEED');
                    //
                    const isFullDay = item.isFullDay;
                    // pick the right label and pluralize
                    const unitLabel = isFullDay
                        ? (item.period > 1 ? 'Days' : 'Day')
                        : (item.period > 1 ? 'Hours' : 'Hour');
                    //
                    //

                    const avatar = getAvatarHtml(item);

                    //
                    tableBody.append(`
                       <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        
                        <td class="fs-9 align-middle py-0">
                          <div class="form-check mb-0 fs-8">
                            <input class="form-check-input" data-id="${item.leaveApplicationID}" type="checkbox" />
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
                        
                        <td class="hdDescription align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                          <div class="d-flex align-items-center">
                            <p class="fs-14 fw-medium d-flex align-items-center mb-0">${item.leaveType}</p>
                            <span href="#" class="ms-2" data-bs-toggle="tooltip" data-bs-placement="right"
                              data-bs-title="I am currently experiencing a fever and design & Development">
                              <i class="ti ti-info-circle text-info"></i>
                          </span>
                          </div>
                        </td>

                        <td class="leaveFrom align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.fromDate}</td>
                        <td class="leaveTo align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.toDate}</td>
                        <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.period}</td>
                       <td class="leaveTotal align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">  ${unitLabel}</td>
                        
                        <td class="dptStatus align-middle white-space-nowrap ps-5 fw-semibold text-body py-0">
                          <span class="badge ${getBadgeClass(item.statusName)}">${item.statusName || 'NEW'}</span>
                        </td>
                        
                     <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                         <a
                               href="#"
                               title="Edit"
                               id="LeaveRequestEditButton"
                               data-id="${item.leaveApplicationID}"
                               class="btn btn-outline-light btn-icon me-1 ${isDisabled ? 'disabled' : ''}" 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
                               ${isDisabled ? 'aria-disabled="true" tabindex="-1"' : ''}>
                               <i class="fas fa-edit text-black"></i>
                    </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.leaveApplicationID}"
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



