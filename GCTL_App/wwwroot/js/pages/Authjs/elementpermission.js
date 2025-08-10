$('#companySelection').on('change', function () {
    var companyId = $(this).val();
    $.get('/RolePermission/GetElementRolesByCompany', { companyId }, function (data) {
        var dropdown = $('#roleSelection');
        dropdown.empty().append('<option value="">Select a Role</option>');
        data.forEach(r => dropdown.append(`<option value="${r.id}">${r.name}</option>`));
    });
});

$('#pageSelection').on('change', function () {
    var pageId = $(this).val();
    $.get('/RolePermission/GetElementsByPage', { pageId }, function (data) {
        //var dropdown = $('#elementSelection');
        //dropdown.empty();
        //data.forEach(el => dropdown.append(`<option value="${el.key}">${el.name}</option>`));
        choiceManager.populateDropdown('elementSelection',data);
    });
});


//$('#permissions-form').on('submit', function (e) {
//    e.preventDefault();

//    var a = choiceManager.getChoiceValue('elementSelection');
//    toastr.success(a)
//});


$('#permissions-form').on('submit', function (e) {
    e.preventDefault();
    var model = {
        RoleId: $('#roleSelection').val(),
        PageId: $('#pageSelection').val(),
        ElementKeys: $('#elementSelection').val()  // Ensure this is an array if the backend expects it
    };

    $.ajax({
        url: '/RolePermission/SaveElementPermissions',
        method: 'POST',
        contentType: 'application/json',  // Tell the server this is JSON
        data: JSON.stringify(model),  // Serialize the model object into JSON
        success: function (res) {
            if (res.success) toastr.success(res.message);
            else toastr.error(res.message);
        },
        error: function (xhr, status, error) {
            toastr.error("An error occurred while saving permissions.");
        }
    });
});
$(document).on('click', '#elementPermission-singleDelBtn', function () {
    var id = $(this).data('id');

    if (id) {
        showDeleteModal(function () {
            $.ajax({
                url: '/RolePermission/ElementSoftDelete',
                method: 'POST',
                data: { ids: [id] },
                success: function (response) {
                    if (response.isSuccess) {
                        toastr.success(response.message);
                        $("#bloodGroup-check-all").prop('checked', false);
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
//////Element page selection Table list
//////////////////////////////Data Table Initialization//////////////////////////////
var currentPage = 1;
var pageSize = 5;

$('#PageElements-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#PageElements-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#PageElements-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#PageElements-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});

let currentSortColumn = 'HolidayTitle';
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
    var searchTerm = $("#PageElements-searchInput").val();
    $.ajax({
        url: '/RolePermission/ElementPermissionList',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#PageElements-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input addHolidayConfig-selectItem" data-id="${item.id}" />
                            </td>
                            <td class="align-middle text-center white-space-nowrap ps-0">${rowIndex}</td>
                            
                             <td class="align-middle white-space-nowrap ">${item.roleName}</td>
                             <td class="align-middle white-space-nowrap ">${item.pageName}</td>
                             <td class="align-middle white-space-nowrap ">${item.elementKeys}</td>
                             <td class="align-middle white-space-nowrap text-end pe-0">
                          <div class="d-flex justify-content-end align-items-center">
                          <a
                               href="#"
                               title="Edit"
                               id="LeaveRequestEditButton"
                               data-id="${item.id}"
                               class="btn btn-outline-light btn-icon me-1 " 
                               data-bs-toggle="modal" 
                               data-bs-target="#edit_leaves"
                              >
                               <i class="fas fa-edit text-black"></i>
                            </a>
                            <a 
                              href="#" title="Delete"  data-id="${item.id}"
                              class="btn btn-outline-light btn-icon"  
                              id="elementPermission-singleDelBtn" >
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

            $("#PageElements-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#PageElements-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#PageElements-paginationLinks");
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
    $("#PageElements-prevPageBtn").prop('disabled', currentPage === 1);
    $("#PageElements-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});

