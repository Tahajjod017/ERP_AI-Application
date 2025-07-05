//


$(document).ready(function () {

    $(document).ready(function () {
        $.ajax({
            url: '/LeaveApprovalDeclineRoute/GetLeaveTypeBalancesForEmployeeDisplay',
            type: 'GET',
            success: function (data) {
                //debugger
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

    //
    // Get LeaveDays according to LeaveType
    $('#EmployeeIDEdit,#LeaveTypeIDEdit').on('change', function () {
  
        var employeeId = $('#EmployeeIDEdit').val();
        var leaveTypeID = $('#LeaveTypeIDEdit').val();
      
        if (leaveTypeID && employeeId) {
            $.ajax({
                url: '/LeaveRequest/GetLeaveDays',
                type: 'GET',
                data: { employeeId: employeeId, leaveTypeID: leaveTypeID },
                success: function (data) {
                    if (data && data.leaveDays !== null) {
                        $('#AvailableLeaveDays').val(data.leaveDays);
                    } else {
                        $('#AvailableLeaveDays').val('0');
                    }
                },
                error: function () {
                    toastr.error('Failed to fetch leave days.');
                    $('#AvailableLeaveDays').val('0');
                }
            });
        } else {
            $('#AvailableLeaveDays').val('');
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

    $(document).on('change', '#FromDate, #ToDate', function (e) {
        e.preventDefault();

        let fromDate = flatpickrHelper.getDate('FromDate');
        let toDate = flatpickrHelper.getDate('ToDate');
        console.log("SubFromDate", fromDate);
        console.log("SubToDate", toDate);

        if (!fromDate || !toDate) return;

        if (new Date(toDate) < new Date(fromDate)) {
            toastr.warning('To Date cannot be earlier than From Date.');
            $('#ToDate').val(''); // Clear invalid ToDate
            $('#TotalAppliedDays').val('');
            $('#SubsequentHolydayDays').val('');
            return;
        }

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
                if (typeof data.totalDays !== 'undefined') {
                    $('#TotalAppliedDays').val(data.totalDays);
                  

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
    // Handle form submit
    $(document).on('click', '#ApplyLeaveSubmitButtonApproval', function (e) {
        e.preventDefault();

        var available = parseFloat($('#AvailableLeaveDays').val()) || 0;
        var applied = parseFloat($('#TotalAppliedDays').val()) || 0;

        if (applied > available) {
            toastr.error(`You only have ${available} day(s) available, but you tried to apply for ${applied}.`);
            return false;
        }

        const isFullDay = $('input[name="IsFullDayEdit"]:checked').val() === "true";
        const approvalStatus = $('input[name="ApprovalStatus"]:checked').val();
        const isApproved = approvalStatus === "true"; 
        const formdata = {
            LeaveApplicationID: $('#LeaveApplicationID').val(),
            FromDateEdit: flatpickrHelper.getDate('FromDate'),
            ToDateEdit: flatpickrHelper.getDate('ToDate'),
            EmployeeIDEdit: choiceManager.getChoiceValue('EmployeeIDEdit'),
            LeaveTypeIDEdit: choiceManager.getChoiceValue('LeaveTypeIDEdit'),
            IsFullDayEdit: isFullDay,
            Approved: isApproved,
            Declined: !isApproved,
            ApprovalNote: $('#ApprovalNote').val(),
            TotalAppliedDays: $('#TotalAppliedDays').val()
        };
        if (!isFullDay) {
            formdata.PartialFromTimeEdit = $('#PartialFromTimeEdit').val();
            formdata.PartialToTimeEdit = $('#PartialToTimeEdit').val();
        }
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
                    $('input[name="LeaveDaysEdit"]').val(data.leaveDaysEdit);
                    $('input[name="IsFullDayEdit"][value="' + data.isFullDayEdit + '"]').prop('checked', true).trigger('change');
                    flatpickrHelper.setDate('FromDate', data.fromDateEdit)
                    $('input[name="ToDateEdit"]').val(data.toDateEdit);
                    $('#ToDateFromDateCombinedEdit').val(data.fromDateEdit);
                    $('#TotalAppliedDays').val(data.period);
                    $('input[name="PartialFromTimeEdit"]').val(data.partialFromTimeEdit);
                    $('input[name="PartialToTimeEdit"]').val(data.partialToTimeEdit);
                    $('textarea[name="ReasonEdit"]').val(data.reasonEdit);
                    $('#AvailableLeaveDays').val(data.availableLeaveDays)
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



//<td class="dptStatus align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
//    <a href="#" class="nav-item mx-2" data-bs-toggle="tooltip" data-bs-placement="top" title="Approved">
//        <i class="fas fa-check-square text-success"></i>
//    </a>

//    <a href="#" class="nav-item mx-2" data-bs-toggle="tooltip" data-bs-placement="top" title="Declined">
//        <i class="far fa-window-close text-danger"></i>
//    </a>
//</td>


//<td class="align-middle white-space-nowrap text-end pe-0">
//    <div class="d-flex justify-content-end align-items-center">
//        <a
//            href="#"
//            title="Edit"
//            id="LeaveRequestEditButton"
//            data-id="${item.leaveApplicationID}"
//            class="btn btn-outline-light btn-icon me-1 ${isDisabled ? 'disabled' : ''}"
//            data-bs-toggle="modal"
//            data-bs-target="#edit_leaves"
//            ${isDisabled ? 'aria-disabled="true" tabindex="-1"' : ''}>
//            <i class="fas fa-edit text-black"></i>
//        </a>
//        <a
//            href="#" title="Delete" data-id="${item.leaveApplicationID}"
//            class="btn btn-outline-light btn-icon"
//            id="leaveRequestDelete-singleDelBtn" >
//            <i class="far fa-trash-alt text-black"></i>
//        </a>
//    </div>
//</td>

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

// Filtering according to formdate to ToDate


$(document).ready(function () {
    initializeGlobalDateRangePicker(
        'basic-daterange_aboveTable',                   // visible input
        'basic-daterange_fromHidden_aboveTable',        // hidden FromDate
        'basic-daterange_toHidden_aboveTable',          // hidden ToDate
        function () {
            currentPage = 1;
            loadTableData(); // Your reload logic here
        }
    );
});

//

function loadTableData(currentSortColumn, currentSortOrder) {
    var searchTerm = $("#leaveRequest-searchInput").val();
    var leaveTypeID = $('#LeaveTypeIDFilterDD').val();
    var statusID = $('#StatusIDFilterDD').val();
    const fromDate = $('#basic-daterange_fromHidden_aboveTable').val();
    const toDate = $('#basic-daterange_toHidden_aboveTable').val();

    console.log("From: " + fromDate + " | To: " + toDate);

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
            statusID: statusID,
            fromDate: fromDate,
            toDate: toDate
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

                          <td class="leaveTotal align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.availableLeaveDays}</td>
                    
                     <td class="align-middle white-space-nowrap text-end pe-0 ps-5">
                      <div class="d-flex  align-items-center">
                      <a href="#"
                         title="View"
                         id="LeaveRequestEditButton"
                         data-id="${item.leaveApplicationID}"
                         class="btn btn-outline-light btn-icon d-flex align-items-center justify-content-center"
                         data-bs-toggle="modal"
                         data-bs-target="#edit_leaves">
                          <i class="fas fa-eye text-primary"></i>
                      </a>
                     </div>
                    </td>


                      

                     <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex  align-items-center">
                        
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



// for Below Table 

$(document).ready(function () {
    var attendPage = 1;
    var attendPageSize = 5;
    let attendSortColumn = '';
    let attendSortOrder = '';

    loadAttendanceTable();

    $('#attendance-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();
        if (selectedSize) {
            attendPageSize = parseInt(selectedSize, 10);
            attendPage = 1;
            loadAttendanceTable();
        }
    });

    $("#attendance-searchInput").on("input", function () {
        attendPage = 1;
        loadAttendanceTable();
    });

    $("#attendance-prevPageBtn").on('click', function () {
        if (attendPage > 1) {
            attendPage--;
            loadAttendanceTable();
        }
    });

    $("#attendance-nextPageBtn").on('click', function () {
        attendPage++;
        loadAttendanceTable();
    });

    $('th.attend-sort').on('click', function () {
        const column = $(this).data('sort');

        if (attendSortColumn === column) {
            attendSortOrder = attendSortOrder === 'asc' ? 'desc' : 'asc';
        } else {
            attendSortColumn = column;
            attendSortOrder = 'asc';
        }

        loadAttendanceTable(attendSortColumn, attendSortOrder);
        updateAttendSortIcons(column, attendSortOrder);
    });

    $(document).on("change", "#attendanceStatusFilter,#attendanceTypeFilter", function () {
        attendPage = 1;
        loadAttendanceTable();
    });

    $(document).ready(function () {
        initializeGlobalDateRangePicker(
            'basic-daterange_belowTable',                    // visible input
            'basic-daterange_fromHidden_belowTable',         // hidden FromDate
            'basic-daterange_toHidden_belowTable',           // hidden ToDate
            function () {
                attendPage = 1;
                loadAttendanceTable(); // You can change this function name if needed
            }
        );
    });



    $(document).on('click', '.attend-page-btn', function () {
        const page = $(this).data('page');
        attendPage = page;
        loadAttendanceTable();
    });

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
    function loadAttendanceTable(sortCol, sortOrder) {
        var keyword = $("#attendance-searchInput").val();
        var typeID = $('#attendanceTypeFilter').val();
        var statusID = $('#attendanceStatusFilter').val();
        var fromDate = $('#basic-daterange_fromHidden_belowTable').val();
        var toDate = $('#basic-daterange_toHidden_belowTable').val();


        $.ajax({
            url: '/LeaveApprovalDeclineRoute/GetAllTableBelowAsync',
            method: 'GET',
            data: {
                pageNumber: attendPage,
                pageSize: attendPageSize,
                searchTerm: keyword,
                currentSortColumn: sortCol,
                currentSortOrder: sortOrder,
                attendanceTypeID: typeID,
                attendanceStatusID: statusID,
                fromDate: fromDate,
                toDate: toDate
            },
            success: function (response) {
                var tbody = $("#attendanceRequestTableBody");
                tbody.empty();
                var total = response.paginationInfo.totalItems;

                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        let rowIndex = sortOrder === 'asc'
                            ? (attendPage - 1) * attendPageSize + index + 1
                            : total - ((attendPage - 1) * attendPageSize + index);

                        
                        const isFullDay = item.isFullDay;
                        // pick the right label and pluralize
                        const unitLabel = isFullDay
                            ? (item.period > 1 ? 'Days' : 'Day')
                            : (item.period > 1 ? 'Hours' : 'Hour');
                        let avatar = renderAttendAvatar(item);

                        tbody.append(`
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
                          <span class="badge ${getBadgeClass(item.statusName)}">${item.statusName || 'NEW'}</span>
                        </td>
                    

                       

                     <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex  align-items-center">
                         
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
                    tbody.append('<tr><td colspan="9" class="text-center">No data available</td></tr>');
                }

                let pageInfo = response.paginationInfo;
                $("#attendance-paginationInfo").text(`Showing ${pageInfo.startItem} to ${pageInfo.endItem} of ${pageInfo.totalItems}`);
                $("#attendance-totalCount").text(`(${pageInfo.totalItems})`);

                updateAttendPagination(pageInfo.pageNumbers, pageInfo.currentPage, pageInfo.totalPages);
            }
        });
    }

    function updateAttendSortIcons() {
        $('th.attend-sort').each(function () {
            const $th = $(this);
            const col = $th.data('sort');
            $th.find('.sort-icon').remove();

            const iconClass = col === attendSortColumn
                ? (attendSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down')
                : 'fa-sort';

            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
        });
    }

    function updateAttendPagination(pageNumbers, currentPage, totalPages) {
        const pager = $("#attendance-paginationLinks");
        pager.empty();
        const win = 1;

        const pageBtn = (p) => `<li class="page-item ${p === currentPage ? 'active' : ''}">
            <button class="page-link attend-page-btn" data-page="${p}">${p}</button>
        </li>`;

        const ellipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

        if (currentPage > win + 1) {
            pager.append(pageBtn(1), ellipsis());
        }

        const start = Math.max(1, currentPage - win);
        const end = Math.min(totalPages, currentPage + win);
        for (let i = start; i <= end; i++) {
            pager.append(pageBtn(i));
        }

        if (currentPage < totalPages - win) {
            pager.append(ellipsis(), pageBtn(totalPages));
        }

        $("#attendance-prevPageBtn").prop('disabled', currentPage === 1);
        $("#attendance-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    function renderAttendAvatar(user) {
        if (user.employeeImage && user.employeeImage !== '') {
            return `<img src="${user.employeeImage}" class="rounded-circle" alt="${user.employeeName}" />`;
        } else {
            const first = user.employeeName?.charAt(0).toUpperCase() || '?';
            return `<div class="avatar-placeholder bg-info text-white rounded-circle d-flex align-items-center justify-content-center">${first}</div>`;
        }
    }
});



//

