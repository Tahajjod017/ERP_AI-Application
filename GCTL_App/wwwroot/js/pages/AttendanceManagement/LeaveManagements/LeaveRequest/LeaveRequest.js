
$(document).ready(function () {

    //

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
    

    

    

    //

    toggleTimeDateValidation();

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

   

    //
    // Handle form submit
    $('body').on('submit', '#LeaveRequestForm', function (e) {
        e.preventDefault();

        var $form = $(this);
        if (!$form.valid()) {
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

        // Reset validation states
        $('#ToDateFromDateCombined').removeClass('is-invalid');
        $('#ToDateFromDateCombinedError').hide().text('');
        $('#FromDate').removeClass('is-invalid');
        $('#FromDateError').hide().text('');
        $('#EmployeeID').removeClass('is-invalid');
        $('#EmployeeIDError').hide().text('');
        $('#PartialFromTime, #PartialToTime').removeClass('is-invalid input-validation-error');
        $('#PartialFromTime, #PartialToTime').siblings('.text-danger').text('');

        // Reset flatpickr instances
        ['#FromDate', '#ToDate', '#ToDateFromDateCombined', '#PartialFromTime', '#PartialToTime'].forEach(function (id) {
            if ($(id)[0] && $(id)[0]._flatpickr) {
                $(id)[0]._flatpickr.clear();
            }
        });

    }




});




//var currentPage = 1;
//var pageSize = 5;

//$('#leaveRequest-pageSizeSelect').on('change', function () {
//    var selectedSize = $(this).val();

//    if (selectedSize) {
//        pageSize = parseInt(selectedSize, 10);
//        currentPage = 1;
//        loadTableData();
//    }
//});


//$(document).ready(function () {
//    loadTableData();

//    $("#leaveRequest-searchInput").on("input", function () {
//        currentPage = 1;
//        loadTableData();
//    });

//    $("#leaveRequest-prevPageBtn").on('click', function () {
//        if (currentPage > 1) {
//            currentPage--;
//            loadTableData();
//        }
//    });

//    $("#leaveRequest-nextPageBtn").on('click', function () {
//        currentPage++;
//        loadTableData();
//    });
//});


//let currentSortColumn = 'BankIssuedLetterID';
//let currentSortOrder = 'desc';

//$('th.sort').on('click', function () {
//    const column = $(this).data('sort');

//    if (currentSortColumn === column) {
//        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
//    } else {
//        currentSortColumn = column;
//        currentSortOrder = 'asc';
//    }

//    loadTableData(currentSortColumn, currentSortOrder);
//    updateSortingIndicator(column, currentSortOrder);
//});


//function updateSortingIndicator() {
//    $('th.sort').each(function () {
//        const $th = $(this);
//        const column = $th.data('sort');
//        $th.find('.sort-icon').remove();

//        if (column === currentSortColumn) {
//            const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
//            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
//        } else {
//            $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
//        }
//    });
//}


//function loadTableData(sortColumn, sortOrder) {
//    var searchTerm = $("#leaveRequest-searchInput").val();

//    $.ajax({
//        url: gridUrl,
//        method: 'GET',
//        data: {
//            pageNumber: currentPage,
//            pageSize: pageSize,
//            searchTerm: searchTerm,
//            sortColumn: sortColumn,
//            sortOrder: sortOrder
//        },
//        success: function (response) {
//            var tableBody = $("#leaveRequest-tBody");
//            tableBody.empty();
//            var totalItems = response.paginationInfo.totalItems;

//            if (response.data.length > 0) {
//                response.data.forEach(function (item, index) {
//                    var rowIndex;

//                    if (currentSortOrder === 'asc') {
//                        rowIndex = (currentPage - 1) * pageSize + index + 1;
//                    } else {
//                        rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
//                    }

//                    tableBody.append(`
//                                <tr class="position-static">
//                                    <td class="text-center text-middle align-middle" style="width: 5%;">
//                                        <input type="checkbox" class="form-check-input leaveRequest-selectItem" data-id="${item.leaveRequestID}" />
//                                    </td>
//                                    <td class="text-center text-middle align-middle white-space-nowrap ps-0" style="width: 10%;">${rowIndex}</td>
//                                    <td class="align-middle white-space-nowrap ps-0">${item.leaveRequestName}</td>
//                                    <td class="align-middle text-end white-space-nowrap pe-3">
//                                        <div class="row g-3">
//                                            <a class="btn btn-phoenix-secondary btn-icon me-1 fs-10 text-body px-0 leaveRequest-bulkEdit" href="#!" id="leaveRequest-editBtn" data-id="${item.leaveRequestID}"><i class="fas fa-edit"></i></a>
//                                            <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 leaveRequest-bulkDelete" href="#!" id="leaveRequest-singleDelBtn" data-id="${item.leaveRequestID}"><span class="fas fa-trash"></span></a>
//                                        </div>
//                                    </td>
//                                </tr>`);
//                });
//            } else {
//                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
//            }

//            var paginationInfo = response.paginationInfo;

//            $("#leaveRequest-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
//            $("#leaveRequest-totalCount").text(`(${paginationInfo.totalItems})`);

//            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
//        }
//    });
//}

//function updatePagination(pageNumbers, currentPage, totalPages) {
//    const paginationLinks = $("#leaveRequest-paginationLinks");
//    paginationLinks.empty();
//    // Window size (number of pages before/after the current page)
//    const windowSize = 1;

//    const createPageButton = (page) => `
//                <li class="page-item ${page === currentPage ? 'active' : ''}">
//                    <button class="page-link page-btn" data-page="${page}">${page}</button>
//                </li>
//            `;

//    // Helper function for ellipsis
//    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
//    // Add "First Page" and ellipsis if needed
//    if (currentPage > windowSize + 1) {
//        paginationLinks.append(createPageButton(1), addEllipsis());
//    }
//    // Add page number buttons within the window range
//    const startPage = Math.max(1, currentPage - windowSize);
//    const endPage = Math.min(totalPages, currentPage + windowSize);
//    for (let i = startPage; i <= endPage; i++) {
//        paginationLinks.append(createPageButton(i));
//    }
//    // Add ellipsis and "Last Page" button if needed
//    if (currentPage < totalPages - windowSize) {
//        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
//    }
//    // Disable or enable previous/next buttons
//    $("#leaveRequest-prevPageBtn").prop('disabled', currentPage === 1);
//    $("#leaveRequest-nextPageBtn").prop('disabled', currentPage === totalPages);
//}

//$(document).on('click', '.page-btn', function () {
//    const page = $(this).data('page');
//    currentPage = page;
//    loadTableData();
//});



