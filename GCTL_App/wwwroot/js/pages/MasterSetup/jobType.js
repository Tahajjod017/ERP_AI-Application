(function ($) {
    $.jobType = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#jobType-form',
            saveBtn: '#jobType-saveBtn',
            editBtn: '#jobType-editBtn',
            resetBtn: '#jobType-resetBtn',
            bulkDelBtn: '#jobType-bulkDelBtn',
            singleDeleteBtn: '#jobType-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#jobType-saveBtn').on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#jobType-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    JobTypeID: $('#JobTypeID').val(),
                    JobTypeName: $('#JobTypeName').val(),
                }

                validateName();

                var id = $('#jobType-form #JobTypeID').val();
                var url = '';
                if (id > 0) {
                    url = '/JobTypes/Update';
                } else {
                    url = '/JobTypes/Create';
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


            $(document).on('click', '#jobType-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/JobTypes/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#jobType-form #JobTypeID').val(data.jobTypeID);
                            $('#jobType-form #JobTypeName').val(data.jobTypeName);

                            $('#jobType-form #jobType-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#jobType-delSel").on('click', function () {
                var selectedItems = $(".jobType-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/JobTypes/SoftDelete',
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

            $(document).on('click', '#jobType-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/JobTypes/SoftDelete',
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




            $('#jobType-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#jobType-form')[0].reset();
                $('#JobTypeID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#jobType-form #jobType-saveBtn').text('Save');
                $("#jobType-check-all").prop('checked', false);
                $('.jobType-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#jobType-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#JobTypeName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#JobTypeName').val().trim();

                if (name === '') {
                    $('#JobTypeName').css('border', '1px solid red');
                } else {
                    $('#JobTypeName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#JobTypeName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/JobTypes/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="JobTypeName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="JobTypeName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#jobType-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.jobType-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.jobType-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.jobType-selectItem');
                const checkedItems = $('.jobType-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#jobType-check-all').prop('checked', allChecked);
                $('#jobType-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#jobType-bulkSelectActions').removeClass('d-none');
                    $('#jobType-searchBox').addClass('d-none');
                    $('.jobType-bulkDelete').addClass('disabled');
                    $('.jobType-bulkEdit').addClass('disabled');
                } else {
                    $('#jobType-bulkSelectActions').addClass('d-none');
                    $('#jobType-searchBox').removeClass('d-none');
                    $('.jobType-bulkDelete').removeClass('disabled');
                    $('.jobType-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#jobType-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#jobType-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#jobType-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#jobType-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'JobTypeName';
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
            var searchTerm = $("#jobType-searchInput").val();

            $.ajax({
                url: '/JobTypes/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#jobType-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input jobType-selectItem" data-id="${item.jobTypeID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.jobTypeName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 jobType-bulkDelete" href="#!" id="jobType-edit" data-id="${item.jobTypeID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 jobType-bulkEdit" href="#!" id="jobType-single-delete" data-id="${item.jobTypeID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#jobType-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#jobType-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#jobType-paginationLinks");
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
            $("#jobType-prevPageBtn").prop('disabled', currentPage === 1);
            $("#jobType-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




