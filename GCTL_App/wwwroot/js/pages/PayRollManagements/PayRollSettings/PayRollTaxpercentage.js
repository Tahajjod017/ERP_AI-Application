(function ($) {
    $.payrolltaxpercentage = function (options) {
        // Default options
        
       
        $(() => {

            $('#PayRolltaxSettings-saveBtn').on('click', function (e) {
                e.preventDefault();

                var token = $('#PayRolltaxSettings-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    PSettingID: $('#PSettingID').val(),
                    OrganizationID: $('#OrganizationID').val(),
                    TaxPercentage: $('#TaxPercentage').val(),
                }

                validateName();

                var id = $('#PayRolltaxSettings-form #PSettingID').val();
                var url = '';
                if (id > 0) {
                    url = '/PayRollTaxPercentageSettigns/Update';
                } else {
                    url = '/PayRollTaxPercentageSettigns/Create';
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


            $(document).on('click', '#PayRolltaxSettings-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');
                $.ajax({
                    url: '/PayRollTaxPercentageSettigns/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            debugger
                            $('#PSettingID').val(data.pSettingID);
                            $('#TaxPercentage').val(data.taxPercentage);
                            choiceManager.setChoiceValue('OrganizationID', data.organizationID);
                            $('#PayRolltaxSettings-form #PayRolltaxSettings-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#PayRolltaxSettings-delSel").on('click', function () {
                var selectedItems = $(".PayRolltaxSettings-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/PayRollTaxPercentageSettigns/SoftDelete',
                            method: 'POST',
                            data: { ids: selectedIds },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    $("#PayRolltaxSettings-check-all").prop('checked', false);
                                    $('.PayRolltaxSettings-selectItem').prop('checked', false);
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

            $(document).on('click', '#PayRolltaxSettings-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/PayRollTaxPercentageSettigns/SoftDelete',
                            method: 'POST',
                            data: { ids: [id] },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    $("#PayRolltaxSettings-check-all").prop('checked', false);
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




            $('#PayRolltaxSettings-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#PayRolltaxSettings-form')[0].reset();
               
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#PayRolltaxSettings-form #PayRolltaxSettings-saveBtn').text('Save');

                loadTableData();
                toggleBulkActions();

                choiceManager.resetChoice('OrganizationID')

            }


            $('#OrganizationID').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#OrganizationID').val().trim();

                if (name === '') {
                    $('#OrganizationID').css('border', '1px solid red');
                } else {
                    $('#OrganizationID').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#OrganizationID').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/PayRollTaxPercentageSettigns/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="OrganizationID"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="OrganizationID"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#PayRolltaxSettings-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.PayRolltaxSettings-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.PayRolltaxSettings-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.PayRolltaxSettings-selectItem');
                const checkedItems = $('.PayRolltaxSettings-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#PayRolltaxSettings-check-all').prop('checked', allChecked);
                $('#bloodGroup-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#PayRolltaxSettings-bulkSelectActions').removeClass('d-none');
                    $('#PayRolltaxSettings-searchBox').addClass('d-none');
                    $('.PayRolltaxSettings-bulkDelete').addClass('disabled');
                    $('.PayRolltaxSettings-bulkEdit').addClass('disabled');
                } else {
                    $('#PayRolltaxSettings-bulkSelectActions').addClass('d-none');
                    $('#PayRolltaxSettings-searchBox').removeClass('d-none');
                    $('.PayRolltaxSettings-bulkDelete').removeClass('disabled');
                    $('.PayRolltaxSettings-bulkEdit').removeClass('disabled');
                }
            }



        });

        //#region Dat Table
        var currentPage = 1;
        var pageSize = 5;

        $('#PayRolltaxSettings-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#PayRolltaxSettings-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#PayRolltaxSettings-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#PayRolltaxSettings-nextPageBtn").on('click', function () {
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
            var searchTerm = $("#PayRolltaxSettings-searchInput").val();

            $.ajax({
                url: '/PayRollTaxPercentageSettigns/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#PayRolltaxSettings-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input PayRolltaxSettings-selectItem" data-id="${item.pSettingID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.organizationName}</td>
                             <td class="align-middle white-space-nowrap ps-0">${item.taxPercentage}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 PayRolltaxSettings-bulkDelete" href="#!" id="PayRolltaxSettings-edit" data-id="${item.pSettingID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 PayRolltaxSettings-bulkEdit" href="#!" id="PayRolltaxSettings-single-delete" data-id="${item.pSettingID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#PayRolltaxSettings-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#PayRolltaxSettings-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#PayRolltaxSettings-paginationLinks");
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
            $("#PayRolltaxSettings-prevPageBtn").prop('disabled', currentPage === 1);
            $("#PayRolltaxSettings-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        //#endrgion
        


    }
}(jQuery));

