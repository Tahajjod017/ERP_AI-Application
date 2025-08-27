$(document).ready(function () {


    

    $(document).on('click', '#AllowanceType-saveBtn',function (e) {
        alert('Button clicked');

        e.preventDefault();

        var token = $('#AllowanceType-form input[name="__RequestVerificationToken"]').val();

        var formData = {
            __RequestVerificationToken: token,
            EmployeeAllowanceTypeID: $('#EmployeeAllowanceTypeID').val(),
            OrganizationID: $('#OrganizationID').val(),
            EmployeeAllowanceTypeName: $('#EmployeeAllowanceTypeName').val(),
        }

        var id = $('#AllowanceType-form #EmployeeAllowanceTypeID').val();
        var url = '';
        if (id > 0) {
            url = '/EmpAllowanceOrganization/Update';
        } else {
            url = '/EmpAllowanceOrganization/Create';
        }
        debugger
        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            success: function (res) {
                console.log(res); 
                debugger
                if (res.success) {
                    debugger
                    toastr.success(res.message);
                    resetAllowanceTypeForm();
                } else {
                    toastr.info(res.message);
                }
            },
            error: function (err) {
                console.log(err);
            }
        });
    });

    $(document).on('click', '#EmpAllowanceOrganization-edit', function () {
        var id = $(this).data('id');
        if (!id)
        {
            toastr.error("Id Not Found")
        }
        debugger
        $.ajax({
            url: '/EmpAllowanceOrganization/GetByID',
            type: 'GET',
            data: {id:id},
            success: function (res) {
                if (res.success) {
                    debugger
                    d = res.data;

                    $('#EmployeeAllowanceTypeID').val(d.employeeAllowanceTypeID);
                        choiceManager.setChoiceValue('OrganizationID', d.organizationID);
                    $('#EmployeeAllowanceTypeName').val(d.employeeAllowanceTypeName);
                   
                } else {
                    toastr.error(res);
                }
            }, error: function (err) {
                console.error(err);
            }
        })
    });

    $(document).on('click', '#AllowanceType-resetBtn', function () {

        resetAllowanceTypeForm();
    })
    function resetAllowanceTypeForm() {
        // Reset all text, hidden, and select fields in the form
        $('#AllowanceType-form')[0].reset();

        // Specifically clear fields if needed
        $('#EmployeeAllowanceTypeName').val('');
        $('#EmployeeAllowanceTypeID').val('');

        // Reset your custom choice manager for OrganizationID
        if (typeof choiceManager !== 'undefined' && choiceManager.resetChoice) {
            choiceManager.resetChoice('OrganizationID');
        }

        // Clear validation errors if any
        $('.field-validation-error').text('');
    }


    //

    $("#PayRolltaxSettings-delSel").on('click', function () {
        var selectedItems = $(".EmpAllowanceOrganization-selectItem:checked");
        var selectedIds = [];
        selectedItems.each(function () {
            selectedIds.push($(this).data('id'));
        });

        if (selectedIds.length > 0) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/EmpAllowanceOrganization/SoftDelete',
                    method: 'POST',
                    data: { ids: selectedIds },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $("#EmpAllowanceOrganization-check-all").prop('checked', false);
                            $('.EmpAllowanceOrganization-selectItem').prop('checked', false);
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

    $(document).on('click', '#EmpAllowanceOrganization-single-delete', function () {
        var id = $(this).data('id');

        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/EmpAllowanceOrganization/SoftDelete',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $("#EmpAllowanceOrganization-check-all").prop('checked', false);
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
    //#region Dat Table
    var currentPage = 1;
    var pageSize = 5;

    $('#EmpAllowanceOrganization-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });


    $(document).ready(function () {
        loadTableData();

        $("#EmpAllowanceOrganization-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#EmpAllowanceOrganization-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#EmpAllowanceOrganization-nextPageBtn").on('click', function () {
            currentPage++;
            loadTableData();
        });
    });


    let currentSortColumn = '';
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
        var searchTerm = $("#EmpAllowanceOrganization-searchInput").val();

        $.ajax({
            url: '/EmpAllowanceOrganization/GetAll',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                var tableBody = $("#EmpAllowanceOrganization-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input EmpAllowanceOrganization-selectItem" data-id="${item.employeeAllowanceTypeID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.employeeAllowanceTypeName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 EmpAllowanceOrganization-bulkDelete" href="#!" id="EmpAllowanceOrganization-edit" data-id="${item.employeeAllowanceTypeID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 EmpAllowanceOrganization-Edit" href="#!" id="EmpAllowanceOrganization-single-delete" data-id="${item.employeeAllowanceTypeID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#EmpAllowanceOrganization-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#EmpAllowanceOrganization-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#EmpAllowanceOrganization-paginationLinks");
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
        $("#EmpAllowanceOrganization-prevPageBtn").prop('disabled', currentPage === 1);
        $("#EmpAllowanceOrganization-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endrgion

})