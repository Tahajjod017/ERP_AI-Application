(function ($) {
    $.employmentNature = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#employmentNature-form',
            saveBtn: '#employmentNature-saveBtn',
            editBtn: '#employmentNature-editBtn',
            resetBtn: '#employmentNature-resetBtn',
            bulkDelBtn: '#employmentNature-bulkDelBtn',
            singleDeleteBtn: '#employmentNature-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#employmentNature-saveBtn').on('click', function (e) {
                e.preventDefault();

                var token = $('#employmentNature-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    EmploymentNatureID: $('#EmploymentNatureID').val(),
                    EmploymentNatureName: $('#EmploymentNatureName').val(),
                }

                validateName();

                var id = $('#employmentNature-form #EmploymentNatureID').val();
                var url = '';
                if (id > 0) {
                    url = '/EmploymentNatures/Update';
                } else {
                    url = '/EmploymentNatures/Create';
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


            $(document).on('click', '#employmentNature-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/EmploymentNatures/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#employmentNature-form #EmploymentNatureID').val(data.employmentNatureID);
                            $('#employmentNature-form #EmploymentNatureName').val(data.employmentNatureName);

                            $('#employmentNature-form #employmentNature-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#employmentNature-delSel").on('click', function () {
                var selectedItems = $(".employmentNature-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/EmploymentNatures/SoftDelete',
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

            $(document).on('click', '#employmentNature-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/EmploymentNatures/SoftDelete',
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




            $('#employmentNature-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#employmentNature-form')[0].reset();
                $('#EmploymentNatureID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#employmentNature-form #employmentNature-saveBtn').text('Save');
                $("#employmentNature-check-all").prop('checked', false);
                $('.employmentNature-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#employmentNature-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#EmploymentNatureName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#EmploymentNatureName').val().trim();

                if (name === '') {
                    $('#EmploymentNatureName').css('border', '1px solid red');
                } else {
                    $('#EmploymentNatureName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#EmploymentNatureName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/EmploymentNatures/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="EmploymentNatureName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="EmploymentNatureName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#employmentNature-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.employmentNature-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.employmentNature-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.employmentNature-selectItem');
                const checkedItems = $('.employmentNature-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#employmentNature-check-all').prop('checked', allChecked);
                $('#employmentNature-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#employmentNature-bulkSelectActions').removeClass('d-none');
                    $('#employmentNature-searchBox').addClass('d-none');
                    $('.employmentNature-bulkDelete').addClass('disabled');
                    $('.employmentNature-bulkEdit').addClass('disabled');
                } else {
                    $('#employmentNature-bulkSelectActions').addClass('d-none');
                    $('#employmentNature-searchBox').removeClass('d-none');
                    $('.employmentNature-bulkDelete').removeClass('disabled');
                    $('.employmentNature-bulkEdit').removeClass('disabled');
                }
            }




        });


        var currentPage = 1;
        var pageSize = 5;

        $('#employmentNature-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#employmentNature-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#employmentNature-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#employmentNature-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'EmploymentNatureName';
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
            var searchTerm = $("#employmentNature-searchInput").val();

            $.ajax({
                url: '/EmploymentNatures/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#employmentNature-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input employmentNature-selectItem" data-id="${item.employmentNatureID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.employmentNatureName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 employmentNature-bulkDelete" href="#!" id="employmentNature-edit" data-id="${item.employmentNatureID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 employmentNature-bulkEdit" href="#!" id="employmentNature-single-delete" data-id="${item.employmentNatureID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#employmentNature-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#employmentNature-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#employmentNature-paginationLinks");
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
            $("#employmentNature-prevPageBtn").prop('disabled', currentPage === 1);
            $("#employmentNature-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




