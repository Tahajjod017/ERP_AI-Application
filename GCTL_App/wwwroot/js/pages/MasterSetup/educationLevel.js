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



            $('#educationLevel-saveBtn').on('click', function (e) {
                e.preventDefault();

                var token = $('#educationLevel-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    EducationLevelID: $('#EducationLevelID').val(),
                    EducationLevelName: $('#EducationLevelName').val(),
                }

                validateName();

                var id = $('#educationLevel-form #EducationLevelID').val();
                var url = '';
                if (id > 0) {
                    url = '/EducationLevels/Update';
                } else {
                    url = '/EducationLevels/Create';
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


            $(document).on('click', '#educationLevel-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/EducationLevels/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#educationLevel-form #EducationLevelID').val(data.educationLevelID);
                            $('#educationLevel-form #EducationLevelName').val(data.educationLevelName);

                            $('#educationLevel-form #educationLevel-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#educationLevel-delSel").on('click', function () {
                var selectedItems = $(".educationLevel-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/EducationLevels/SoftDelete',
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

            $(document).on('click', '#educationLevel-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/EducationLevels/SoftDelete',
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




            $('#educationLevel-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#educationLevel-form')[0].reset();
                $('#EducationLevelID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#educationLevel-form #educationLevel-saveBtn').text('Save');
                $("#educationLevel-check-all").prop('checked', false);
                $('.educationLevel-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
            }


            $('#EducationLevelName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#EducationLevelName').val().trim();

                if (name === '') {
                    $('#EducationLevelName').css('border', '1px solid red');
                } else {
                    $('#EducationLevelName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#EducationLevelName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/EducationLevels/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="EducationLevelName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="EducationLevelName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#educationLevel-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.educationLevel-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.educationLevel-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.educationLevel-selectItem');
                const checkedItems = $('.educationLevel-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#educationLevel-check-all').prop('checked', allChecked);
                $('#educationLevel-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#educationLevel-bulkSelectActions').removeClass('d-none');
                    $('#educationLevel-searchBox').addClass('d-none');
                    $('.educationLevel-bulkDelete').addClass('disabled');
                    $('.educationLevel-bulkEdit').addClass('disabled');
                } else {
                    $('#educationLevel-bulkSelectActions').addClass('d-none');
                    $('#educationLevel-searchBox').removeClass('d-none');
                    $('.educationLevel-bulkDelete').removeClass('disabled');
                    $('.educationLevel-bulkEdit').removeClass('disabled');
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

            $("#educationLevel-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#educationLevel-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#educationLevel-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'EducationLevelName';
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
            var searchTerm = $("#educationLevel-searchInput").val();

            $.ajax({
                url: '/EducationLevels/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#educationLevel-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input educationLevel-selectItem" data-id="${item.educationLevelID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.educationLevelName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 educationLevel-bulkDelete" href="#!" id="educationLevel-edit" data-id="${item.educationLevelID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 educationLevel-bulkEdit" href="#!" id="educationLevel-single-delete" data-id="${item.educationLevelID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#educationLevel-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#educationLevel-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#educationLevel-paginationLinks");
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
            $("#educationLevel-prevPageBtn").prop('disabled', currentPage === 1);
            $("#educationLevel-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        function goToPage(page) {
            currentPage = page;
            loadTableData();
        }



    }
}(jQuery));



