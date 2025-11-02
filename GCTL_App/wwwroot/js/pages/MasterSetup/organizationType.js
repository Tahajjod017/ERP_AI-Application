(function ($) {
    $.organizationType = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#organizationType-form',
            saveBtn: '#organizationType-saveBtn',
            editBtn: '#organizationType-editBtn',
            resetBtn: '#organizationType-resetBtn',
            bulkDelBtn: '#organizationType-bulkDelBtn',
            singleDeleteBtn: '#organizationType-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#organizationType-saveBtn').on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#organizationType-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    Id: $('#Id').val(),
                    TypeName: $('#TypeName').val(),
                    UseFor: $('#UseFor').val(),
                }

                validateName();

                var id = $('#organizationType-form #Id').val();
                var url = '';
                if (id > 0) {
                    url = '/OrganizationTypes/Update';
                } else {
                    url = '/OrganizationTypes/Create';
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
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    },
                    error: function (err) {
                        console.log(err);
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    }
                });
            });


            $(document).on('click', '#organizationType-edit', function (e) {
                e.preventDefault();
                debugger;
                var id = $(this).data('id');

                $.ajax({
                    url: '/OrganizationTypes/GetById',
                    method: 'POST',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            debugger;
                            var data = response.data;
                            $('#organizationType-form #Id').val(data.id);
                            $('#organizationType-form #TypeName').val(data.typeName);
                            $('#organizationType-form #UseFor').val(data.useFor);

                            $('#organizationType-form #organizationType-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });


            //#region useFor init
            $('#UseFor').select2({
                placeholder: "Select Type",
                width: '100%',
                allowClear: true
            });
            //#endregion


            $("#organizationType-delSel").on('click', function () {
                debugger
                var selectedItems = $(".organizationType-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/OrganizationTypes/SoftDelete',
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

            $(document).on('click', '#organizationType-single-delete', function () {
                debugger;
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/OrganizationTypes/SoftDelete',
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




            $('#organizationType-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#organizationType-form')[0].reset();
                $('#Id').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#organizationType-form #organizationType-saveBtn').text('Save');
                $("#organizationType-check-all").prop('checked', false);
                $('.organizationType-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#organizationType-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#TypeName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#TypeName').val().trim();

                if (name === '') {
                    $('#TypeName').css('border', '1px solid red');
                } else {
                    $('#TypeName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#TypeName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/OrganizationTypes/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="TypeName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="TypeName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#organizationType-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.organizationType-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.organizationType-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.organizationType-selectItem');
                const checkedItems = $('.organizationType-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#organizationType-check-all').prop('checked', allChecked);
                $('#organizationType-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#organizationType-bulkSelectActions').removeClass('d-none');
                    $('#organizationType-searchBox').addClass('d-none');
                    $('.organizationType-bulkDelete').addClass('disabled');
                    $('.organizationType-bulkEdit').addClass('disabled');
                } else {
                    $('#organizationType-bulkSelectActions').addClass('d-none');
                    $('#organizationType-searchBox').removeClass('d-none');
                    $('.organizationType-bulkDelete').removeClass('disabled');
                    $('.organizationType-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#organizationType-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#organizationType-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#organizationType-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#organizationType-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'TypeName';
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
            var searchTerm = $("#organizationType-searchInput").val();

            $.ajax({
                url: '/OrganizationTypes/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#organizationType-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input organizationType-selectItem" data-id="${item.id}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.typeName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 organizationType-bulkDelete" href="#!" id="organizationType-edit" data-id="${item.id}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 organizationType-bulkEdit" href="#!" id="organizationType-single-delete" data-id="${item.id}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#organizationType-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#organizationType-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#organizationType-paginationLinks");
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
            $("#organizationType-prevPageBtn").prop('disabled', currentPage === 1);
            $("#organizationType-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




