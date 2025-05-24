(function ($) {
    $.actiontaken = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#actionTaken-form',
            saveBtn: '#actionTaken-saveBtn',
            editBtn: '#actionTaken-editBtn',
            resetBtn: '#actionTaken-resetBtn',
            bulkDelBtn: '#actionTaken-bulkDelBtn',
            singleDeleteBtn: '#actionTaken-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {





        });





    }
}(jQuery));






$('#department-saveBtn').on('click', function (e) {
    e.preventDefault();

    var token = $('#department-form input[name="__RequestVerificationToken"]').val();

    var formData = {
        __RequestVerificationToken: token,
        DepartmentID: $('#DepartmentID').val(),
        DepartmentName: $('#DepartmentName').val(),
    }

    validateName();

    var id = $('#department-form #DepartmentID').val();
    var url = '';
    if (id > 0) {
        url = '/Departments/Update';
    } else {
        url = '/Departments/Create';
    }

    $.ajax({
        url: url,
        type: 'POST',
        data: formData,
        success: function (data) {
            if (data.isSuccess) {
                toastr.success(data.message);
                clear();
            } else {
                toastr.info(data.message);
            }
        },
        error: function (err) {
            console.log(err);
        }
    });
});


$(document).on('click', '#department-edit', function (e) {
    e.preventDefault();

    var id = $(this).data('id');

    $.ajax({
        url: '/Departments/GetById',
        method: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.isSuccess) {
                var data = response.data;
                $('#department-form #DepartmentID').val(data.departmentID);
                $('#department-form #DepartmentName').val(data.departmentName);

                $('#department-form #department-saveBtn').text('Update');
            } else {
                toastr.warning(response.message);
            }
        }
    });
});



$("#department-delSel").on('click', function () {
    var selectedItems = $(".department-selectItem:checked");
    var selectedIds = [];

    selectedItems.each(function () {
        selectedIds.push($(this).data('id'));
    });

    if (selectedIds.length > 0) {
        showDeleteModal(function () {
            $.ajax({
                url: '/Departments/SoftDelete',
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

$(document).on('click', '#department-single-delete', function () {
    var id = $(this).data('id');

    if (id) {
        showDeleteModal(function () {
            $.ajax({
                url: '/Departments/SoftDelete',
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




$('#department-resetBtn').on('click', function () {
    clear();
})

function clear() {
    $('#department-form')[0].reset();
    $('#DepartmentID').val('0');
    $('.text-danger').hide();
    $('.form-control').removeClass('is-invalid');
    $('.form-control').each(function () {
        if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
            $(this).css('border-color', '#ccc');
        }
    });
    $('#department-form #department-saveBtn').text('Save');
    $("#department-check-all").prop('checked', false);
    $('.department-selectItem').prop('checked', false);
    loadTableData();
    toggleBulkActions();
}


$('#DepartmentName').on('input', function () {
    validateName();
});


function validateName() {
    var name = $('#DepartmentName').val().trim();

    if (name === '') {
        $('#DepartmentName').css('border', '1px solid red');
    } else {
        $('#DepartmentName').css('border', '1px solid #ccc');
    }
}


$(document).ready(function () {
    checkNameUnique();
});

function checkNameUnique() {
    $('#DepartmentName').on('input', function () {
        var value = $(this).val();

        $.ajax({
            url: '/Departments/CheckNameUnique',
            type: 'POST',
            data: { name: value },
            success: function (response) {
                if (response === true) {
                    $('#nameError').hide();
                    $('input[name="DepartmentName"]').removeClass('is-invalid');
                } else {
                    $('#nameError').text(response).show();
                    $('input[name="DepartmentName"]').addClass('is-invalid');
                }
            },
            error: function (xhr, status, error) {
                console.log("Error occurred while checking name uniqueness: " + error);
            }
        });
    });
}





$(document).ready(function () {
    $('#department-check-all').on('change', function () {
        var isChecked = $(this).prop('checked');
        $('.department-selectItem').prop('checked', isChecked);

        toggleBulkActions();
    });

    $(document).on('change', '.department-selectItem', function () {
        toggleBulkActions();
    });
});

function toggleBulkActions() {
    const allItems = $('.department-selectItem');
    const checkedItems = $('.department-selectItem:checked');

    const allChecked = allItems.length === checkedItems.length;
    const someChecked = checkedItems.length > 0 && !allChecked;

    $('#department-check-all').prop('checked', allChecked);
    $('#department-check-all').prop('indeterminate', someChecked);

    if (checkedItems.length > 1) {
        $('#department-bulkSelectActions').removeClass('d-none');
        $('#department-searchBox').addClass('d-none');
        $('.department-bulkDelete').addClass('disabled');
        $('.department-bulkEdit').addClass('disabled');
    } else {
        $('#department-bulkSelectActions').addClass('d-none');
        $('#department-searchBox').removeClass('d-none');
        $('.department-bulkDelete').removeClass('disabled');
        $('.department-bulkEdit').removeClass('disabled');
    }
}


////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                  Pagination Starts
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
var currentPage = 1;
var pageSize = 5;

$('.dropdown-item').on('click', function () {
    var selectedSize = $(this).data("size");
    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
    } else {
        return;
    }

    $('#selectedPageSize').text(selectedSize);
    loadTableData();
})


$(document).ready(function () {
    loadTableData();

    $("#department-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#department-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#department-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});


let currentSortColumn = 'DepartmentName';
let currentSortOrder = 'asc';

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
    var searchTerm = $("#department-searchInput").val();

    $.ajax({
        url: '/Departments/GetAll',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#department-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input department-selectItem" data-id="${item.departmentID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0" style="width: 10%;">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.departmentName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 department-bulkDelete" href="#!" id="department-edit" data-id="${item.departmentID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 department-bulkEdit" href="#!" id="department-single-delete" data-id="${item.departmentID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#department-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#department-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#department-paginationLinks");
    paginationLinks.empty();
    // Window size (number of pages before/after the current page)
    const windowSize = 1;
    // Helper function to generate page button
    const createPageButton = (page) => `
        <li class="page-item ${page === currentPage ? 'active' : ''}">
            <button class="page-link" onclick="goToPage(${page})">${page}</button>
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
    $("#department-prevPageBtn").prop('disabled', currentPage === 1);
    $("#department-nextPageBtn").prop('disabled', currentPage === totalPages);
}

function goToPage(page) {
    currentPage = page;
    loadTableData();
}
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//                                                  Pagination End
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//


//
