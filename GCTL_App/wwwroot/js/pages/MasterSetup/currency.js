(function ($) {
    $.currency = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#currency-form',
            saveBtn: '#currency-saveBtn',
            editBtn: '#currency-editBtn',
            resetBtn: '#currency-resetBtn',
            bulkDelBtn: '#currency-bulkDelBtn',
            singleDeleteBtn: '#currency-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#currency-saveBtn').on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#currency-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    CurrencyID: $('#CurrencyID').val(),
                    CurrencyCode: $('#CurrencyCode').val(),
                    Symbol: $('#Symbol').val(),
                    CurrencyName: $('#CurrencyName').val(),
                }

                validateName();
                validateSymbol();

                var id = $('#currency-form #CurrencyID').val();
                var url = '';
                if (id > 0) {
                    url = '/Currencys/Update';
                } else {
                    url = '/Currencys/Create';
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


            $(document).on('click', '#currency-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/Currencys/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#currency-form #CurrencyID').val(data.currencyID);
                            $('#currency-form #CurrencyName').val(data.currencyName);
                            $('#currency-form #CurrencyCode').val(data.currencyCode);
                            $('#currency-form #Symbol').val(data.symbol);

                            $('#currency-form #currency-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#currency-delSel").on('click', function () {
                var selectedItems = $(".currency-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/Currencys/SoftDelete',
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

            $(document).on('click', '#currency-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/Currencys/SoftDelete',
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




            $('#currency-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#currency-form')[0].reset();
                $('#CurrencyID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#currency-form #currency-saveBtn').text('Save');
                $("#currency-check-all").prop('checked', false);
                $('.currency-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#currency-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#CurrencyName').on('input', function () {
                validateName();
            });
            $('#Symbol').on('input', function () {
                validateSymbol();
            });


            function validateName() {
                var name = $('#CurrencyName').val().trim();

                if (name === '') {
                    $('#CurrencyName').css('border', '1px solid red');
                } else {
                    $('#CurrencyName').css('border', '1px solid #ccc');
                }
            }

            function validateSymbol() {
                var name = $('#Symbol').val().trim();

                if (name === '') {
                    $('#Symbol').css('border', '1px solid red');
                } else {
                    $('#Symbol').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#CurrencyName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/Currencys/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="CurrencyName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="CurrencyName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#currency-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.currency-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.currency-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.currency-selectItem');
                const checkedItems = $('.currency-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#currency-check-all').prop('checked', allChecked);
                $('#currency-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#currency-bulkSelectActions').removeClass('d-none');
                    $('#currency-searchBox').addClass('d-none');
                    $('.currency-bulkDelete').addClass('disabled');
                    $('.currency-bulkEdit').addClass('disabled');
                } else {
                    $('#currency-bulkSelectActions').addClass('d-none');
                    $('#currency-searchBox').removeClass('d-none');
                    $('.currency-bulkDelete').removeClass('disabled');
                    $('.currency-bulkEdit').removeClass('disabled');
                }
            }




        });

        var currentPage = 1;
        var pageSize = 5;

        $('#currency-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#currency-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#currency-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#currency-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'CurrencyName';
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
            var searchTerm = $("#currency-searchInput").val();

            $.ajax({
                url: '/Currencys/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#currency-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input currency-selectItem" data-id="${item.currencyID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.currencyCode}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.currencyName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.symbol}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 currency-bulkDelete" href="#!" id="currency-edit" data-id="${item.currencyID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 currency-bulkEdit" href="#!" id="currency-single-delete" data-id="${item.currencyID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#currency-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#currency-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#currency-paginationLinks");
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
            $("#currency-prevPageBtn").prop('disabled', currentPage === 1);
            $("#currency-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




