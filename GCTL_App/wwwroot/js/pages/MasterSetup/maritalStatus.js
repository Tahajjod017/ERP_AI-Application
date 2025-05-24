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


            $('#maritalStatus-saveBtn').on('click', function (e) {
                e.preventDefault();

                var token = $('#maritalStatus-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    MaritalStatusID: $('#MaritalStatusID').val(),
                    MaritalStatusName: $('#MaritalStatusName').val(),
                }

                validateName();

                var id = $('#maritalStatus-form #MaritalStatusID').val();
                var url = '';
                if (id > 0) {
                    url = '/MaritalStatus/Update';
                } else {
                    url = '/MaritalStatus/Create';
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


            $(document).on('click', '#maritalStatus-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/MaritalStatus/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#maritalStatus-form #MaritalStatusID').val(data.maritalStatusID);
                            $('#maritalStatus-form #MaritalStatusName').val(data.maritalStatusName);

                            $('#maritalStatus-form #maritalStatus-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#maritalStatus-delSel").on('click', function () {
                var selectedItems = $(".maritalStatus-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/MaritalStatus/SoftDelete',
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

            $(document).on('click', '#maritalStatus-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/MaritalStatus/SoftDelete',
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




            $('#maritalStatus-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#maritalStatus-form')[0].reset();
                $('#MaritalStatusID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#maritalStatus-form #maritalStatus-saveBtn').text('Save');
                $("#maritalStatus-check-all").prop('checked', false);
                $('.maritalStatus-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
            }


            $('#MaritalStatusName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#MaritalStatusName').val().trim();

                if (name === '') {
                    $('#MaritalStatusName').css('border', '1px solid red');
                } else {
                    $('#MaritalStatusName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#MaritalStatusName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/MaritalStatus/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="MaritalStatusName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="MaritalStatusName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#maritalStatus-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.maritalStatus-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.maritalStatus-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.maritalStatus-selectItem');
                const checkedItems = $('.maritalStatus-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#maritalStatus-check-all').prop('checked', allChecked);
                $('#maritalStatus-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#maritalStatus-bulkSelectActions').removeClass('d-none');
                    $('#maritalStatus-searchBox').addClass('d-none');
                    $('.maritalStatus-bulkDelete').addClass('disabled');
                    $('.maritalStatus-bulkEdit').addClass('disabled');
                } else {
                    $('#maritalStatus-bulkSelectActions').addClass('d-none');
                    $('#maritalStatus-searchBox').removeClass('d-none');
                    $('.maritalStatus-bulkDelete').removeClass('disabled');
                    $('.maritalStatus-bulkEdit').removeClass('disabled');
                }
            }




        });


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

            $("#maritalStatus-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#maritalStatus-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#maritalStatus-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'MaritalStatusName';
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
            var searchTerm = $("#maritalStatus-searchInput").val();

            $.ajax({
                url: '/MaritalStatus/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#maritalStatus-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input maritalStatus-selectItem" data-id="${item.maritalStatusID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.maritalStatusName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 maritalStatus-bulkDelete" href="#!" id="maritalStatus-edit" data-id="${item.maritalStatusID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 maritalStatus-bulkEdit" href="#!" id="maritalStatus-single-delete" data-id="${item.maritalStatusID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#maritalStatus-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#maritalStatus-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#maritalStatus-paginationLinks");
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
            $("#maritalStatus-prevPageBtn").prop('disabled', currentPage === 1);
            $("#maritalStatus-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        function goToPage(page) {
            currentPage = page;
            loadTableData();
        }


    }
}(jQuery));




