//


$(document).ready(function () {


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

    GetLeavePolicyIsCountAsync();
    function GetLeavePolicyIsCountAsync() {
        $.ajax({
            url: '/LeaveRequest/GetLeavePolicyIsCountAsync',
            type: 'GET',
            success: function (data) {

                if (data.length > 0 && (data[0].isWeekendCountedAsLeave || data[0].isHolidayCountedAsLeave)) {
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


    // Get LeaveDays according to LeaveType
    $('#LeaveTypeID').on('change', function () {
        var selectedId = $(this).val();
        if (selectedId) {
            $.ajax({
                url: '/LeaveRequest/GetLeaveDays',
                type: 'GET',
                data: { leaveTypeId: selectedId },
                success: function (data) {
                    if (data && data.leaveDays !== null) {
                        $('#LeaveDays').val(data.leaveDays);
                    } else {
                        $('#LeaveDays').val('0');
                    }
                },
                error: function () {
                    toastr.error('Failed to fetch leave days.');
                    $('#LeaveDays').val('0');
                }
            });
        } else {
            $('#LeaveDays').val('');
        }
    });

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

    flatpickr("#ToDateFromDateCombined", {
        dateFormat: "Y-m-d", // yyyy-mm-dd
        onChange: function (selectedDates, dateStr) {
            // Set the same date to both FromDate and ToDate
            $('#FromDate').val(dateStr);
            $('#ToDate').val(dateStr);
        }
    });

    // Also initialize flatpickr for other date fields
    flatpickr("#FromDate", { dateFormat: "Y-m-d" });
    flatpickr("#ToDate", { dateFormat: "Y-m-d" });
    //

    //Calculate Days
    function calculateDays() {
        let fromDate = flatpickrHelper.getDate('FromDate');
        let toDate = flatpickrHelper.getDate('ToDate');

        console.log("FromDate:", fromDate);
        console.log("ToDate:", toDate);

        // Convert string to Date if needed
        if (typeof fromDate === 'string') fromDate = new Date(fromDate);
        if (typeof toDate === 'string') toDate = new Date(toDate);

        // Validate both are valid Date objects
        if (!(fromDate instanceof Date) || isNaN(fromDate) ||
            !(toDate instanceof Date) || isNaN(toDate)) {
            document.getElementById('TotalAppliedDays').value = '';
            return;
        }

        // Ensure ToDate >= FromDate
        if (toDate < fromDate) {
            toastr.warning("To Date must be greater than or equal to From Date");
            document.getElementById('TotalAppliedDays').value = '';
            flatpickrHelper.clearDate('ToDate')
            document.getElementById('ToDate').value = '';
            return;
        }

        // Normalize time to midnight
        fromDate.setHours(0, 0, 0, 0);
        toDate.setHours(0, 0, 0, 0);

        const timeDiff = toDate.getTime() - fromDate.getTime();
        const dayDiff = Math.floor(timeDiff / (1000 * 60 * 60 * 24)) + 1; // Inclusive

        document.getElementById('TotalAppliedDays').value = dayDiff;
    }

    // Bind change events
    document.getElementById('FromDate').addEventListener('change', calculateDays);
    document.getElementById('ToDate').addEventListener('change', calculateDays);


    //

    $(document).on('change', '#FromDate, #ToDate', function (e) {
        e.preventDefault();

        let fromDate = flatpickrHelper.getDate('FromDate');
        let toDate = flatpickrHelper.getDate('ToDate');
        console.log("SubFromDate", fromDate);
        console.log("SubToDate", toDate);

        if (!fromDate || !toDate) return;

        $.ajax({
            url: '/LeaveApprovalDeclineRoute/SubsequentLeaveCount',
            type: 'GET',
            data: {
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (data) {
                if (data && data.totalSubsequentDays > 0) {
                    $('#SubsequentHolydayDays').val(data.totalSubsequentDays);
                } else if (!data.isHolidayCountedAsLeave && !data.isWeekendCountedAsLeave) {
                    $('#SubsequentHolydayDays').val("Not Applicable");
                } else {
                    $('#SubsequentHolydayDays').val("0");
                }
            }
            ,
            error: function () {
                toastr.error('Failed to fetch subsequent.');
            }
        });
    });

    //

    


    //


    //
    // Handle form submit
    $(document).on('click', '#ApplyLeaveSubmitButtonApproval', function (e) {
        e.preventDefault();

        const formdata = {
            LeaveApplicationID: $('#LeaveApplicationID').val(),
            FromDateEdit: flatpickrHelper.getDate('FromDate'),
            ToDateEdit: flatpickrHelper.getDate('ToDate'),
            IsFullDayEdit: $('input[name="IsFullDayEdit"]:checked').val() === "true"

        };

        $.ajax({
            type: 'POST',
            url: '/LeaveApprovalDeclineRoute/UpdateRequestAsync',
            data: JSON.stringify(formdata), // Send data as JSON string
            contentType: 'application/json; charset=utf-8', // Tell server it's JSON
            dataType: 'json',
            success: function (response) {
                console.log("Response:", response);
                if (response.success) {
                    toastr.success(response.message);
                    resetForm(); // Reset after successful save
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
            url: '/LeaveApprovalDeclineRoute/GetLeaveRequestByIdAsync',
            type: 'GET',
            data: { leaveApplicationID: leaveApplicationID },
            success: function (data) {
          
                console.log("Data GetBy LeaveRequest", data);
                if (data && Object.keys(data).length > 0) {

                    // Set hidden ID
                    $('#LeaveApplicationID').val(data.leaveApplicationID);

                    // EmployeeIDEdit

                    choiceManager.setChoiceValue('EmployeeIDEdit', data.employeeIDEdit);
                    choiceManager.setChoiceValue('LeaveTypeIDEdit', data.leaveTypeIDEdit);
                    flatpickrHelper.setDate('ToDateFromDateCombinedEdit', data.fromDateEdit);
                    // LeaveDaysEdit
                    $('input[name="LeaveDaysEdit"]').val(data.leaveDaysEdit);

                    // IsFullDayEdit (radio buttons)
                    $('input[name="IsFullDayEdit"][value="' + data.isFullDayEdit + '"]').prop('checked', true).trigger('change');

                    // FromDateEdit
                    $('input[name="FromDateEdit"]').val(data.fromDateEdit);

                    // ToDateEdit
                    $('input[name="ToDateEdit"]').val(data.toDateEdit);

                    // ToDateFromDateCombinedEdit
                    $('#ToDateFromDateCombinedEdit').val(data.fromDateEdit);
                    $('#TotalAppliedDays').val(data.period);
                    // PartialFromTimeEdit
                    $('input[name="PartialFromTimeEdit"]').val(data.partialFromTimeEdit);

                    // PartialToTimeEdit
                    $('input[name="PartialToTimeEdit"]').val(data.partialToTimeEdit);

                    // ReasonEdit
                    $('textarea[name="ReasonEdit"]').val(data.reasonEdit);

                    // Optionally toggle the sections based on IsFullDayEdit
                    if (data.isFullDayEdit === true) {
                        $('#FullDayDivEdit').removeClass('d-none');
                        $('#PartialDayDivEdit').addClass('d-none');

                        $('#PartialDayRadioWrapper').addClass('d-none');
                        $('#FullDayRadioWrapper').removeClass('d-none');
                    } else {
                        $('#FullDayDivEdit').addClass('d-none');
                        $('#PartialDayDivEdit').removeClass('d-none');

                        $('#FullDayRadioWrapper').addClass('d-none');
                        $('#PartialDayRadioWrapper').removeClass('d-none');
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


});



// Approval Table 


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


function getAvatarHtml(employee) {
    if (employee.employeeImage && employee.employeeImage !== '') {
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
        url: '/LeaveApprovalDeclineRoute/GetAllTableListAsync',
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
            var tableBody = $("#LeaveapprovalDeclineListTable_Tbody");
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
                        <td class="leaveTotalDay align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.period} ${unitLabel}</td>
                      
                          <td class="leaveTotal align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">3(Remaining)</td>
                    

                        <td class="dptStatus align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <a href="#" class="nav-item mx-2" data-bs-toggle="tooltip" data-bs-placement="top" title="Approved">
                                    <i class="fas fa-check-square text-success"></i>
                                </a>

                                <a href="#" class="nav-item mx-2" data-bs-toggle="tooltip" data-bs-placement="top" title="Declined">
                                    <i class="far fa-window-close text-danger"></i>
                                </a>
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


