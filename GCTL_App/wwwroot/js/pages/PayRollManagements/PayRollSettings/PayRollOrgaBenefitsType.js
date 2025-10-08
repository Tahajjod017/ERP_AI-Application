$(document).ready(function () {

    $(document).on('click', '#OrganizationBenefitsType-saveBtn', function (e) {
        e.preventDefault();
        var token = $('#OrganizationBenefitsType-form input[name="__RequestVerificationToken"]').val();
        var formData = {
            __RequestVerificationToken: token,
            BenefitTypeID: $('#BenefitTypeID').val(),
            OrganizatonIDs: $('#OrganizatonIDs').val(),
            BenefitTypeName: $('#BenefitTypeName').val(),
            IsApplyOnGrossSalary: $('#IsApplyOnGrossSalary').is(':checked'),
        }
        var id = $('#OrganizationBenefitsType-form #BenefitTypeID').val();
        var url = '';
        if (id > 0) {
            url = '/OrganizationBenefitsType/Update';
        } else {
            url = '/OrganizationBenefitsType/Save';
        }
        debugger
        $.ajax({
            url: url,
            type: 'POST',
            data: formData,
            traditional: true,
            dataType: 'json',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            success: function (res) {
                console.log(res);
                $('#nameError').hide();
                if (res.success) {
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



    //
    $(document).on('click', '#OrganizationBenefitsType-edit', function () {

        debugger
        var id = $(this).data('id');
        if (!id) {
            toastr.error("Id Not Found")
        }
        $.ajax({
            url: '/OrganizationBenefitsType/GetByID',
            type: 'GET',
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    d = res.data;
                    debugger
                    $('#BenefitTypeID').val(d.benefitTypeID);
                    $('#OrganizationIDs').val(d.organizatonID).each(function () {
                        coreui.MultiSelect.getInstance(this)?.update();
                    });

                    $('#BenefitTypeName').val(d.benefitTypeName);
                    $('#IsApplyOnGrossSalary').prop('checked', d.isApplyOnGrossSalary === true);
                    console.log("TTT" + res.data);
                } else {
                    toastr.error(res);
                }
            }, error: function (err) {
                console.error(err);
            }
        })
    });

    $(document).on('click', '#OrganizationBenefitsType-resetBtn', function () {

        resetAllowanceTypeForm();
    })
    function resetAllowanceTypeForm() {
       
        $('#OrganizationBenefitsType-form')[0].reset();
        $('#BenefitTypeName').val('');
        $('#BenefitTypeID').val('');
        const orgSelect = document.getElementById('OrganizationIDs');
        const orgInstance = coreui.MultiSelect.getInstance(orgSelect);
        if (orgInstance) {
            orgInstance.deselectAll();
        }
        loadTableData();
        
        $('.field-validation-error').text('');
    }


    //

    $("#PayRolltaxSettings-delSel").on('click', function () {
        var selectedItems = $(".OrganizationBenefitsType-selectItem:checked");
        var selectedIds = [];
        selectedItems.each(function () {
            selectedIds.push($(this).data('id'));
        });

        if (selectedIds.length > 0) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/OrganizationBenefitsType/SoftDelete',
                    method: 'POST',
                    data: { ids: selectedIds },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $("#OrganizationBenefitsType-check-all").prop('checked', false);
                            $('.OrganizationBenefitsType-selectItem').prop('checked', false);
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

    $(document).on('click', '#OrganizationBenefitsType-single-delete', function () {
        var id = $(this).data('id');

        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/OrganizationBenefitsType/SoftDelete',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);
                            $("#OrganizationBenefitsType-check-all").prop('checked', false);
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

    $('#OrganizationBenefitsType-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });


    $(document).ready(function () {
        loadTableData();

        $("#OrganizationBenefitsType-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#OrganizationBenefitsType-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#OrganizationBenefitsType-nextPageBtn").on('click', function () {
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
        var searchTerm = $("#OrganizationBenefitsType-searchInput").val();

        $.ajax({
            url: '/OrganizationBenefitsType/GetAll',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                var tableBody = $("#OrganizationBenefitsType-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input OrganizationBenefitsType-selectItem" data-id="${item.benefitTypeID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.benefitTypeName}</td>
                             <td class="align-middle white-space-nowrap ps-5">${item.isApplyOnGrossSalary}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 OrganizationBenefitsType-bulkDelete" href="#!" id="OrganizationBenefitsType-edit" data-id="${item.benefitTypeID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 OrganizationBenefitsType-Edit" href="#!" id="OrganizationBenefitsType-single-delete" data-id="${item.benefitTypeID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#OrganizationBenefitsType-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#OrganizationBenefitsType-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#OrganizationBenefitsType-paginationLinks");
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
        $("#OrganizationBenefitsType-prevPageBtn").prop('disabled', currentPage === 1);
        $("#OrganizationBenefitsType-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endrgion

})