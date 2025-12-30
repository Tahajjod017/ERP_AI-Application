
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
                if (data.length > 0) {
                    const policy = data[0];
                    if (policy.isWeekendCountedAsLeave || policy.isHolidayCountedAsLeave) {
                        $('#SubsequentHolydayDays').val('');
                    } else {
                        $('#SubsequentHolydayDays').val('Not Applicable');
                    }

                    const today = new Date();
                    today.setHours(0, 0, 0, 0);
                    let minDate = null;
                    let maxDate = null;
                    // Allow or disallow past dates
                    if (policy.isAllowRequestForPastDates === true) {
                        //minDate = today ;
                        const pastDate = new Date(today);
                        pastDate.setDate(today.getDate() + 1);
                        minDate = pastDate;
                    } else {
                        minDate = null
                    }

                    if (policy.isAllowRequestForFutureDays && policy.allowRequestForFutureDays > 0) {
                        const futureDate = new Date(today);
                        futureDate.setDate(today.getDate() + (policy.allowRequestForFutureDays + 1));
                        maxDate = futureDate;
                    }
                    const minDateStr = minDate ? minDate.toISOString().split('T')[0] : null;
                    const maxDateStr = maxDate ? maxDate.toISOString().split('T')[0] : null;
                    console.log("Today:", today.toISOString().split('T')[0]);
                    console.log("Past dates allowed:", policy.isAllowRequestForPastDates);
                    console.log("Final minDate:", minDate ? minDate.toISOString().split('T')[0] : 'null');

                    initializeDatepickerDMY2("FromDate,ToDate", minDateStr, maxDateStr);
                    initializeDatepickerDMY2("FromDateEdit,ToDateEdit", minDateStr, maxDateStr);

                    window.__minDateStr = minDateStr;
                    window.__maxDateStr = maxDateStr;
                    //
                }
            },
            error: function () {
                toastr.error('Failed to retrieve data.');
            }
        });
    }
    // Display  Leave Balance

    $(document).ready(function () {
        $.ajax({
            url: '/LeaveApprovalDeclineRoute/GetLeaveTypeBalancesForEmployeeDisplay',
            type: 'GET',
            success: function (data) {
             
                if (data && data.length > 0) {
                    let container = $('#leaveCardsContainer');
                    container.empty(); // clear previous content if any
                    console.log(data.length);
                    data.forEach(function (item, index) {
                        console.log(index);
                        // Set background color based on leave type (optional logic)
                        let bgColor = "bg-secondary"; // default
                        let iconClass = "ti ti-calendar-event"; // default

                        switch (item.leaveTypeName) {
                            case "Annual Leaves":
                                bgColor = "bg-black-le";
                                iconClass = "ti ti-calendar-event";
                                break;
                            case "Medical Leaves":
                                bgColor = "bg-blue-le";
                                iconClass = "ti ti-vaccine";
                                break;
                            case "Casual Leaves":
                                bgColor = "bg-purple-le";
                                iconClass = "ti ti-hexagon-letter-c";
                                break;
                            default:
                                bgColor = "bg-pink-le";
                                iconClass = "ti ti-hexagonal-prism-plus";
                                break;
                        }

                        // Build the card HTML
                        let card = `
                       <div class="col-xl-2 col-md-6 mb-2" style="padding-right: 4px; padding-left: 4px;">
                            <div class="card ${bgColor}">
                                <div class="card-body">
                                    <div class="d-flex align-items-center justify-content-between">
                                        <div class="text-start">
                                            <p class="mb-1">${item.leaveTypeName}</p>
                                            <h4>${item.totalLeave ?? 0}</h4>
                                        </div>
                                       
                                    </div>
                                    
                                    <span class="badge badge-phoenix badge-phoenix-success">
                                        Remaining Leaves : ${item.remainingDays ?? 0}
                                    </span>
                                </div>
                            </div>
                        </div>
                    `;
                        console.log(data.length);
                        if (data.length - 1 == index) {
                            container.append(
                                `
                       <div class="col-xl-2 col-md-6 mb-2" style="padding-right: 0px; padding-left: 4px;">
                            <div class="card ${bgColor}">
                                <div class="card-body">
                                    <div class="d-flex align-items-center justify-content-between">
                                        <div class="text-start">
                                            <p class="mb-1">${item.leaveTypeName}</p>
                                            <h4>${item.totalLeave ?? 0}</h4>
                                        </div>
                                       
                                    </div>
                                    
                                    <span class="badge badge-phoenix badge-phoenix-success">
                                        Remaining Leaves : ${item.remainingDays ?? 0}
                                    </span>
                                </div>
                            </div>
                        </div>
                    `
                            );
                        } else {
                            container.append(card);
                        }
                    });
                } else {
                    toastr.error('No leave balance data available.');
                }
            },
            error: function () {
                toastr.error('Failed to load leave balance data.');
            }
        });
    });

    //
    // Restriuction FromDate less than ToDate
    /*initializeDatepickerDMY("FromDate, ToDate,ToDateFromDateCombined");*/

    // OnlyTOday  Selected
    initializeDatepickerDMYOnlyToday("ToDateFromDateCombined");
    $("#ToDate").prop("disabled", true);
    $(document).on('change', "#FromDate", function () {
        const fromDate = $(this).val();
        if (fromDate) {
            $("#ToDate").prop("disabled", false); // ✅ enable
            updateDatepickerWithMinDate("ToDate", fromDate, {
                maxDate: window.__maxDateStr // ✅ reuse global maxDate
            });
        }

    });


    $(document).on('change', '#FromDateEdit', function () {
        const fromDate = $(this).val();
        if (fromDate) {

            updateDatepickerWithMinDate("ToDateEdit", fromDate, {
                maxDate: window.__maxDateStr
            });
        }
    });


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
    function handleLeaveChange(employeeIdField, leaveTypeIdField) {
        var leaveTypeID = $(leaveTypeIdField).val();
        var employeeId = choiceManager.getChoiceValue(employeeIdField);
        GetleaveDaysOrAvailble(employeeId, leaveTypeID);
        if (leaveTypeID == 2) {
            $('#sickLeaveUploadDiv').removeClass('d-none');
        } else {
            $('#sickLeaveUploadDiv').addClass('d-none');
            $('#SickLeaveDocument').val(''); // clear file if not sick
        }
    }

    // Bind events for both Add and Edit forms
    $('#EmployeeID,#LeaveTypeID').on('change', () =>
        handleLeaveChange('EmployeeID', '#LeaveTypeID')
    );

    $('#EmployeeIDEdit,#LeaveTypeIDEdit').on('change', () =>
        handleLeaveChange('EmployeeIDedit', '#LeaveTypeIDEdit')
    );

    // Time Picker
    initializeTimePickerById('timepicker-12hr,timepicker-12hrEdit');
    //


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


    //

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



    function GetLeavedaysSubsequent(employeeId, fromDate, toDate) {

        if (!fromDate || !toDate) return;

        $.ajax({
            url: '/LeaveRequest/SubsequentLeaveCount',
            type: 'GET',
            data: {
                employeeId: employeeId,
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
                if (data.isMaximumleaveDaysPerAplication && data.maximumleaveDaysPerAplication) {

                    $('#TotalAppliedDaysValidation').text(data.message);

                } else {

                    $('#TotalAppliedDaysValidation').text('');
                }
                if (data.isMaximumGapDaysBetweenAplications && data.maximumGapDaysBetweenAplications) {

                    $('#GapDaysTotalAppliedDaysValidation').text(data.maxGapdaysMessage);

                } else {
                    $('#GapDaysTotalAppliedDaysValidation').text('');

                }

            }
            ,
            error: function () {
                toastr.error('Failed to fetch subsequent.');
            }
        });
    }

    // LeaveDays Count according to employeeid 
    $(document).on('change', '#EmployeeID,#FromDate, #ToDate', function (e) {
        e.preventDefault();

        let fromDate = flatpickrHelper.getDate('FromDate');
        let toDate = flatpickrHelper.getDate('ToDate');
        var employeeId = $('#EmployeeID').val();

        GetLeavedaysSubsequent(employeeId, fromDate, toDate);
    });

    $(document).on('change', '#EmployeeIDEdit,#FromDateEdit, #ToDateEdit', function (e) {
        e.preventDefault();

        let fromDate = flatpickrHelper.getDate('FromDateEdit');
        let toDate = flatpickrHelper.getDate('ToDateEdit');
        var employeeId = $('#EmployeeIDEdit').val();

        GetLeavedaysSubsequent(employeeId, fromDate, toDate);

    });

    // Save Data Start
    let exceedConfirmed = false; // state flag
    // Handle form submit
    $('body').on('submit', '#LeaveRequestForm', function (e) {
        e.preventDefault();
        
        var $form = $(this);
        var $saveButton = $form.find('#ApplyLeaveSubmitButton'); 
        if (!$form.valid()) {

            return false;
        }
        var available = parseFloat($('#LeaveDays').val()) || 0;
        var applied = parseFloat($('#TotalAppliedDays').val()) || 0;
        var leaveType = $('#LeaveTypeID').val();
        if (!leaveType) {
            toastr.error("Please select a leave type before applying.");
            return false;
        }
        if (applied > available && !exceedConfirmed) {
            const message = `You have ${available} day(s) available, but you tried to apply for ${applied} day(s).
        So, your exceed leave will be deducted from Annual Leave. `;
            //  $('#exceedAnnualLeaveModal').find('.modal-body').text(message);
            $('#DisplayContainer').text(message);
            var modal = new bootstrap.Modal(document.getElementById('exceedAnnualLeaveModal'));

            var employeeId = $('#EmployeeID').val();
            DisplayLeave(employeeId)
            modal.show();

            return false;
        }

        // ✅ Set flag before submission
        $('#IsGroupApplication').val(exceedConfirmed ? 'true' : 'false');

        var url = $form.attr('action');
        var formData = new FormData(this);


        $.ajax({
            type: 'POST',
            url: url,
            data: formData,
            contentType: false,
            processData: false,
            dataType: 'json',
            beforeSend: function ()
            {
                $saveButton.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');
                showLoadingIndicator();
            },
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success(response.message);
                    resetForm(); 
                    //
                    var applyModalEl = document.getElementById('apply_leave');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal)
                    {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                    //
                } else {
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
            }, complete: function () {
                $saveButton.prop('disabled', false).html('Apply Leave');
                hideLoadingIndicator();
            }
        });
    });

    //

    $('#confirmExceedLeaveBtn').on('click', function () {
        exceedConfirmed = true;
        $('#IsGroupApplication').val('true');
        $('#exceedAnnualLeaveModal').modal('hide');
        $('#LeaveRequestForm').submit();
    });

    /// Edit Data


    // Get By data leaveRequest
    $(document).on('click', '#LeaveRequestEditButton', function () {
        var leaveApplicationID = $(this).data('id');

        $.ajax({
            url: '/LeaveRequestRoute/GetLeaveRequestByIdAsync',
            type: 'GET',
            data: { leaveApplicationID: leaveApplicationID },
            success: function (data) {
                if (data && Object.keys(data).length > 0) {
                    const maxDate = window.__maxDateStr || null;
                    const minDate = window.__minDateStr || null;
                    $('#LeaveApplicationID').val(data.leaveApplicationID);
                    choiceManager.setChoiceValue('EmployeeIDEdit', data.employeeIDEdit);
                    choiceManager.setChoiceValue('LeaveTypeIDEdit', data.leaveTypeIDEdit);
                    $('#TotalAppliedDaysTT').val(data.period);
                    $('#SubsequentHolydayDaysTT').val(data.totalSubsequentDays);
                    $('#LeaveDaysEdit').val(data.leaveDaysEdit);
                    $('input[name="IsFullDayEdit"][value="' + data.isFullDayEdit + '"]').prop('checked', true).trigger('change');
                    $('input[name="FromDateEdit"]').val(data.fromDateEdit);
                    $('input[name="ToDateEdit"]').val(data.toDateEdit);
                    $('#ToDateFromDateCombinedEdit').val(data.fromDateEdit);
                    if (!data.isFullDay) {
                        $('input[name="ToDateFromDateCombinedEdit"]').val(data.toDateEdit);
                        $('input[name="PartialFromTimeEdit"]').val(data.partialFromTimeEdit);
                        $('input[name="PartialToTimeEdit"]').val(data.partialToTimeEdit);
                        flatpickrHelper.setDate('FromDateEdit', data.fromDateEdit);
                        flatpickrHelper.setDate('ToDateEdit', data.toDateEdit);
                    }

                    $('textarea[name="ReasonEdit"]').val(data.reasonEdit);
                    initializeDatepickerDMY("FromDateEdit,ToDateEdit,ToDateFromDateCombinedEdit");
                    initializeDatepickerDMY2("FromDateEdit,ToDateEdit", minDate, maxDate);
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
                initializeDatepickerDMYOnlyToday("ToDateFromDateCombinedEdit");
            },

            error: function () {
                toastr.error("Error leave request get by Id.");
            }
        })
    })


    //
    $(document).on('click', '#ApplyLeaveSubmitButtonUpdate', function (e) {
        e.preventDefault();
        var $saveButton = $(this); // 🔑 reference the clicked button
        var model = {
            LeaveApplicationID: parseInt($('#LeaveApplicationID').val()) || 0,
            EmployeeIDEdit: parseInt($('#EmployeeIDEdit').val()) || null,
            LeaveTypeIDEdit: parseInt($('#LeaveTypeIDEdit').val()) || 0,
            LeaveDaysEdit: parseFloat($('#LeaveDaysEdit').val()) || 0,
            IsFullDayEdit: $('input[name="IsFullDayEdit"]:checked').val() === "true",
            FromDateEdit: $('#FromDateEdit').val() || null,
            ToDateEdit: $('#ToDateEdit').val() || null,
            ToDateFromDateCombinedEdit: $('#ToDateFromDateCombinedEdit').val() || null,
            PartialFromTimeEdit: $('#PartialFromTimeEdit').val() || null,
            PartialToTimeEdit: $('#PartialToTimeEdit').val() || null,
            ReasonEdit: $('textarea[name="ReasonEdit"]').val() || null,
            TotalSubsequentDays: parseInt($('#TotalSubsequentDays').val()) || null,
            IsHolidayCountedAsLeave: $('input[name="IsHolidayCountedAsLeave"]:checked').val() === "true",
            IsWeekendCountedAsLeave: $('input[name="IsWeekendCountedAsLeave"]:checked').val() === "true",
            Period: parseFloat($('#TotalAppliedDaysTT').val()) || 0,
        };
   
        //
        $.ajax({
            url: '/LeaveRequestUpdatedRoute/UpdateLeaveRequest',
            type: 'POST',
            data: JSON.stringify(model),
            contentType: 'application/json',
            beforeSend: function () {
                // show spinner
                $saveButton
                    .prop('disabled', true)
                    .html('<i class="fa fa-spinner fa-spin"></i> Updating...');
                showLoadingIndicator();
            },
            success: function (response) {

                if (response.success) {
                    toastr.success(response.message || "Updated successfully.");
                    resetLeaveRequestFormEdit();
                } else {
                    toastr.error(response.message || "Update failed.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Update Error:", error);
                toastr.error("An unexpected error occurred while updating the leave request.");
            }, complete: function () {
                // restore button
                $saveButton.prop('disabled', false).html('Update Apply');
                hideLoadingIndicator();
            }
        });
    });
    //

    function resetLeaveRequestFormEdit() {

        $('#LeaveRequestForm')[0].reset();
        $('#LeaveApplicationID').val('');
        choiceManager.clearChoice('EmployeeIDEdit');
        choiceManager.clearChoice('LeaveTypeIDEdit');
        // Clear readonly fields
        $('#LeaveDaysEdit').val('');
        $('#TotalAppliedDaysTT').val('');
        ['FromDateEdit', 'ToDateEdit', 'ToDateFromDateCombinedEdit'].forEach(id => {
            const input = document.getElementById(id);
            if (input) {
                input.value = '';
                if (input._flatpickr) {
                    input._flatpickr.clear();
                }
            }
        });

        // Clear partial time pickers
        $('#PartialFromTimeEdit').val('');
        $('#PartialToTimeEdit').val('');

        // Clear textarea
        $('textarea[name="ReasonEdit"]').val('');

        // Reset radio buttons and show Full Day section
        $('input[name="IsFullDayEdit"][value="true"]').prop('checked', true);
        $('#FullDayDivEdit').removeClass('d-none');
        $('#PartialDayDivEdit').addClass('d-none');
    }
    //

    function DisplayLeave(employeeId) {

        $.ajax({
            url: '/LeaveRequest/GetLeaveTypeBalancesForEmployeeDisplay',
            type: 'GET',
            data: { employeeId: employeeId },
            success: function (data) {

                if (data && data.length > 0) {

                } else {

                    toastr.error('No leave balance data available.');
                }
            },
            error: function () {
                toastr.error('Failed to load leave balance data.');
            }
        });
    }

    //
    //#region Reset logic function
    $('#ResetButton').on('click', function () {
        resetForm();
    });
    function resetForm() {

        choiceManager.clearChoice('EmployeeID');
        choiceManager.clearChoice('LeaveTypeID');
        $('#Reason').val('');
        $('#FromDate').val('');
        $('#ToDate').val('');
        $('#ToDateFromDateCombined').val('');
        $('#PartialFromTime').val('');
        $('#PartialToTime').val('');
        $('#TotalAppliedDays').val();
        $('#ToDateFromDateCombined').removeClass('is-invalid');
        $('#ToDateFromDateCombinedError').hide().text('');
        $('#FromDate').removeClass('is-invalid');
        $('#FromDateError').hide().text('');
        $('#EmployeeID').removeClass('is-invalid');
        $('#EmployeeIDError').hide().text('');
        $('#PartialFromTime, #PartialToTime').removeClass('is-invalid input-validation-error');
        $('#PartialFromTime, #PartialToTime').siblings('.text-danger').text('');
        loadTableData();
        ['#FromDate', '#ToDate', '#ToDateFromDateCombined', '#PartialFromTime', '#PartialToTime'].forEach(function (id) {
            if ($(id)[0] && $(id)[0]._flatpickr) {
                $(id)[0]._flatpickr.clear();
            }
        });
        exceedConfirmed = false;
        $('#apply_leave').modal('hide');
    }
    //#endregion

    //#region Delete Soft Leave Request
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
    //#endregion


});

//#region TooTip Modal
//
let hideTooltipTimer;
// 1. Create the tooltip only once and append to <body>
let $tooltip = $('<div class="custom-tooltip-box"></div>').css({
    position: 'fixed',
    top: '0px',
    left: '1258px',
    zIndex: 9999999,
    backgroundColor: 'rgb(255 247 209)',
    border: '1px solid #ccc',
    padding: '10px',
    minWidth: '250px',
    maxWidth: '400px',
    maxHeight: '300px',
    overflowY: 'auto',
    boxShadow: '0 3px 8px rgba(0,0,0,0.15)',
    display: 'none',
    fontSize: '13px',
    borderRadius: '4px'
});
$('body').append($tooltip);

// 2. Show tooltip on hover
$(document).on('mouseenter', '.custom-tooltip-container', function () {
    const $container = $(this);
    const $button = $container.find('.info-button');
    const leaveApplicationID = $button.data('id2');
    const offset = $button.offset();

    clearTimeout(hideTooltipTimer);

    // Show loading state
    $tooltip.html('<div style="text-align: center; color: #666;">Loading...</div>').css({
        top: offset.top + 25,
        left: offset.left - 100
    }).fadeIn(200);

    $.ajax({
        url: '/LeaveRequest/GetByPersonLeaveStepVM',
        type: 'GET',
        data: { leaveApplicationID: leaveApplicationID },
        dataType: 'json',
        success: function (data) {
            const steps = Array.isArray(data) ? data : [data];
            let html = '';

            if (steps.length > 0) {
                steps.forEach((item, index) => {
                    var approverStep = item.approverStep ?? '';
                    const statusName = item.statusName ?? '';
                    const author = item.approvarPerson ?? '';
                    const statusDescription = item.approvarNote ?? '';
                    const approvedOrDeclineDate = item.approvedOrDeclineDate ?? '';
                    var total = item.approverStepTotal ?? '';
                    if (approverStep > total)
                    {
                        approverStep = total;
                    }

                    html += `
                <div class="timeline-item" style="margin-bottom:1px>
                    <div class="timeline-item position-relative">
                        <div class="row g-md-3">
                            <div class="col-12 col-md-auto d-flex">
                                <div class="timeline-item-date order-1 order-md-0 me-md-4">
                                    <p class="fs-10 fw-semibold text-body-tertiary text-opacity-85 text-end">
                                    
                                        ${approverStep} of ${total}
                                    </p>
                                </div>

                                <div class="timeline-item-bar position-md-relative me-3 me-md-0">
                                    <div class="icon-item icon-item-sm rounded-7 shadow-none bg-primary-subtle">
                                        <span class="fa-solid far fa-file-alt text-primary-dark fs-10"></span>
                                    </div>
                                    <span class="timeline-bar border-end border-dashed"></span>
                                </div>
                            </div>
                            <div class="col">
                                <div class="timeline-item-content ps-6 ps-md-3">
                                    <h5 class="fs-9 lh-sm">${statusName}</h5>
                                    <p class="fs-9 mb-0">by <a class="fw-semibold" href="#!">${author}</a></p>
                                    <h5 class="fs-9 lh-sm">${approvedOrDeclineDate}</h5>
                                    <p class="fs-9 text-body-secondary">${statusDescription}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div> `;
                });
            } else {
                html = '<div class="text-muted" style="color: #999;">No approval steps found</div>';
            }

            $tooltip.html(html);
        }
        ,
        error: function () {
            $tooltip.html('<div class="text-danger" style="color: #d32f2f;">Error loading data</div>');
        }
    });
});

// 3. Hide tooltip on mouse leave from container
$(document).on('mouseleave', '.custom-tooltip-container', function () {
    hideTooltipTimer = setTimeout(() => {
        $tooltip.fadeOut(200);
    }, 300);
});

// 4. Handle tooltip hover to prevent hiding when mouse moves to tooltip
$tooltip.on('mouseenter', function () {
    clearTimeout(hideTooltipTimer);
}).on('mouseleave', function () {
    hideTooltipTimer = setTimeout(() => {
        $tooltip.fadeOut(200);
    }, 300);
});

// 5. Optional: Hide tooltip when clicking elsewhere
$(document).on('click', function (e) {
    if (!$(e.target).closest('.custom-tooltip-container, .custom-tooltip-box').length) {
        clearTimeout(hideTooltipTimer);
        $tooltip.fadeOut(200);
    }
});

//#endregion



// #region 🔵 Get Badge Class Based on Status
function getBadgeClass(status) {
    if (!status || status.trim() === '') return 'text-bg-success';

    switch (status.trim().toUpperCase()) {
        case 'DECLINED':
            return 'badge-phoenix badge-phoenix-danger';
        case 'APPROVED':
            return 'badge-phoenix badge-phoenix-success';
        case 'PENDING':
        case 'WAITING FOR APPROVAL':
            return 'badge-phoenix badge-phoenix-warning';
        case 'NEW':
            return 'badge-phoenix text-bg-success';
        case 'ONGOING':
            return 'badge-phoenix badge-phoenix-primary';
        default:
            return 'text-bg-success';
    }
}
// #endregion

// #region 🟡 Get Status Text Based on Approver Steps & Timing


function getStatusText(item) {
    const rawStatus = item.statusName?.trim().toUpperCase();
     if (rawStatus === 'DECLINED') {
        return 'DECLINED';
    }
     else if (rawStatus === 'APPROVED' && item.isFinalApproved)
     {
        return 'APPROVED';
    }
     else if (rawStatus === 'APPROVED' && !item.isFinalApproved)
     {
        return 'ONGOING';
    }
    const isNewStatus = !rawStatus;
    if (isNewStatus && item.applicationDate) {
        const applicationDate = new Date(item.applicationDate);
        const now = new Date();
        const hoursPassed = (now - applicationDate) / (1000 * 60 * 60); 

        if (hoursPassed >= 24)
        {
            return 'WAITING FOR APPROVAL';
        }
        return 'NEW';
    }

    return rawStatus;
}

// #endregion

// #region 🟠 Check Whether to Show Info Icon
function shouldShowInfoIcon(item) {
    const status = getStatusText(item)?.trim().toUpperCase();
    return !(status === 'NEW' || status === 'WAITING FOR APPROVAL');
}
// #endregion

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
    debugger
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
    var leaveTypeID = $('#LeaveTypeIDFilterDD').val();
    var statusID = $('#StatusIDFilterDD').val();
    const organizationId = $('#OrganizationID').val();
    const departmentIds = $('#DepartmentIDs').val() || [];
    const employeeIds = $('#EmployeeIDs').val() || [];
    const fromDate = $('#basic-daterange_fromHidden').val(); 
    const toDate = $('#basic-daterange_toHidden').val();    
    console.log("Dept: " + departmentIds + " | Emp: " + employeeIds + " | Org: " + organizationId);
    console.log("From: " + fromDate + " | To: " + toDate);

    $.ajax({
        url: '/LeaveRequestRoute/GetAllTableListAsync',
        method: 'GET',
        traditional: true,
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            currentSortColumn: currentSortColumn,
            currentSortOrder: currentSortOrder,
            leaveTypeID: leaveTypeID,
            statusID: statusID,
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
                    let status = item.statusName; // Assuming this is your status value
                    let isDisabled = status && (status.toUpperCase() === 'APPROVED' || status.toUpperCase() === 'DECLINED');
                    const isFullDay = item.isFullDay;
                    // pick the right label and pluralize
                    const unitLabel = isFullDay
                        ? (item.period > 1 ? 'Days' : 'Day')
                        : (item.period > 1 ? 'Hours' : 'Hour');
                    
                    const avatar = getAvatarHtml(item);
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
                        <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.period} ${unitLabel}</td>
                        <td class="dptStatus align-middle white-space-nowrap ps-5 fw-semibold text-body py-0">
                          <span class="badge ${getBadgeClass(getStatusText(item))}">${getStatusText(item)} </span>
                           ${shouldShowInfoIcon(item) ? `
        <div class="custom-tooltip-container position-relative d-inline-block">
            <i class="fa-solid fa-circle-info info-button"
               data-id2="${item.leaveApplicationID}"
               style="cursor: pointer; font-size: 14px; color: #007bff;"></i>
        </div>` : ''}
                        </td>
                        <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.applicationDateForTable}</td>
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
                              class="btn btn-outline-light btn-icon d-none"  
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
