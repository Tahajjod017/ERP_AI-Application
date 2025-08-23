(function ($) {
    $.educationBoard = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#educationBoard-form',
            saveBtn: '#educationBoard-saveBtn',
            editBtn: '#educationBoard-editBtn',
            resetBtn: '#educationBoard-resetBtn',
            bulkDelBtn: '#educationBoard-bulkDelBtn',
            singleDeleteBtn: '#educationBoard-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#educationBoard-saveBtn').on('click', function (e) {
                e.preventDefault();

                var token = $('#educationBoard-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    EducationBoardID: $('#EducationBoardID').val(),
                    EducationBoardName: $('#EducationBoardName').val(),
                }

                validateName();

                var id = $('#educationBoard-form #EducationBoardID').val();
                var url = '';
                if (id > 0) {
                    url = '/EducationBoards/Update';
                } else {
                    url = '/EducationBoards/Create';
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


            $(document).on('click', '#educationBoard-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/EducationBoards/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#educationBoard-form #EducationBoardID').val(data.educationBoardID);
                            $('#educationBoard-form #EducationBoardName').val(data.educationBoardName);

                            $('#educationBoard-form #educationBoard-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#educationBoard-delSel").on('click', function () {
                var selectedItems = $(".educationBoard-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/EducationBoards/SoftDelete',
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

            $(document).on('click', '#educationBoard-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/EducationBoards/SoftDelete',
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




            $('#educationBoard-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#educationBoard-form')[0].reset();
                $('#EducationBoardID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#educationBoard-form #educationBoard-saveBtn').text('Save');
                $("#educationBoard-check-all").prop('checked', false);
                $('.educationBoard-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#educationBoard-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#EducationBoardName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#EducationBoardName').val().trim();

                if (name === '') {
                    $('#EducationBoardName').css('border', '1px solid red');
                } else {
                    $('#EducationBoardName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#EducationBoardName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/EducationBoards/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="EducationBoardName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="EducationBoardName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#educationBoard-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.educationBoard-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.educationBoard-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.educationBoard-selectItem');
                const checkedItems = $('.educationBoard-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#educationBoard-check-all').prop('checked', allChecked);
                $('#educationBoard-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#educationBoard-bulkSelectActions').removeClass('d-none');
                    $('#educationBoard-searchBox').addClass('d-none');
                    $('.educationBoard-bulkDelete').addClass('disabled');
                    $('.educationBoard-bulkEdit').addClass('disabled');
                } else {
                    $('#educationBoard-bulkSelectActions').addClass('d-none');
                    $('#educationBoard-searchBox').removeClass('d-none');
                    $('.educationBoard-bulkDelete').removeClass('disabled');
                    $('.educationBoard-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#educationBoard-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#educationBoard-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#educationBoard-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#educationBoard-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'EducationBoardName';
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
            var searchTerm = $("#educationBoard-searchInput").val();

            $.ajax({
                url: '/EducationBoards/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#educationBoard-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input educationBoard-selectItem" data-id="${item.educationBoardID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0" style="width: 10%;">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.educationBoardName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 educationBoard-bulkDelete" href="#!" id="educationBoard-edit" data-id="${item.educationBoardID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 educationBoard-bulkEdit" href="#!" id="educationBoard-single-delete" data-id="${item.educationBoardID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#educationBoard-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#educationBoard-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#educationBoard-paginationLinks");
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
            $("#educationBoard-prevPageBtn").prop('disabled', currentPage === 1);
            $("#educationBoard-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




