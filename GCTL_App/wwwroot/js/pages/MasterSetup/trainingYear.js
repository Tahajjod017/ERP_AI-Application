(function ($) {
    $.trainingyear = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#trainingYear-form',
            saveBtn: '#trainingYear-saveBtn',
            editBtn: '#trainingYear-editBtn',
            resetBtn: '#trainingYear-resetBtn',
            bulkDelBtn: '#trainingYear-bulkDelBtn',
            singleDeleteBtn: '#trainingYear-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                var token = $('#trainingYear-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    TrainingYearID: $('#TrainingYearID').val(),
                    TrainingYearName: $('#TrainingYearName').val(),
                }

                validateName();

                var id = $(settings.form).find('#TrainingYearID').val();
                var url = '';
                if (id > 0) {
                    url = updateUrl;
                } else {
                    url = createUrl;
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



            $(document).on('click', settings.editBtn, function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: getByIdUrl,
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $(settings.form).find('#TrainingYearID').val(data.trainingYearID);
                            $(settings.form).find('#TrainingYearName').val(data.trainingYearName);

                            $(settings.form).find(settings.saveBtn).text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });




            $(document).on('click', settings.bulkDelBtn, function () {
                var selectedItems = $(".trainingYear-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
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

            $(document).on('click', settings.singleDeleteBtn, function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
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




            $(settings.resetBtn).on('click', function () {
                clear();
            });

            function clear() {
                $(settings.form)[0].reset();
                $('#TrainingYearID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.form).find(settings.saveBtn).text('Save');
                $("#trainingYear-check-all").prop('checked', false);
                $('.trainingYear-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#trainingYear-check-all').prop('checked', false).prop('indeterminate', false);
            }




            $('#TrainingYearName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#TrainingYearName').val().trim();

                if (name === '') {
                    $('#TrainingYearName').css('border', '1px solid red');
                } else {
                    $('#TrainingYearName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#TrainingYearName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: uniqueNameUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="TrainingYearName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="TrainingYearName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }




            $(document).ready(function () {
                $('#trainingYear-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.trainingYear-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.trainingYear-selectItem', function () {
                    toggleBulkActions();
                });
            });



            function toggleBulkActions() {
                const allItems = $('.trainingYear-selectItem');
                const checkedItems = $('.trainingYear-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#trainingYear-check-all').prop('checked', allChecked);
                $('#trainingYear-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#trainingYear-bulkSelectActions').removeClass('d-none');
                    $('#trainingYear-searchBox').addClass('d-none');
                    $('.trainingYear-bulkDelete').addClass('disabled');
                    $('.trainingYear-bulkEdit').addClass('disabled');
                } else {
                    $('#trainingYear-bulkSelectActions').addClass('d-none');
                    $('#trainingYear-searchBox').removeClass('d-none');
                    $('.trainingYear-bulkDelete').removeClass('disabled');
                    $('.trainingYear-bulkEdit').removeClass('disabled');
                }
            }



        });




        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //                                                  Pagination Starts
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        var currentPage = 1;
        var pageSize = 5;

        $('#trainingYear-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#trainingYear-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#trainingYear-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#trainingYear-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'TrainingYearID';
        let currentSortOrder = 'desc';

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
            var searchTerm = $("#trainingYear-searchInput").val();

            $.ajax({
                url: gridUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#trainingYear-tBody");
                    tableBody.empty();
                    var totalItems = response.paginationInfo.totalItems;

                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex;

                            if (currentSortOrder === 'asc') {
                                rowIndex = (currentPage - 1) * pageSize + index + 1;
                            } else {
                                rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
                            }

                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input trainingYear-selectItem" data-id="${item.trainingYearID}" />
                                    </td>
                                    <td class="text-center text-middle align-middle white-space-nowrap ps-0" style="width: 10%;">${rowIndex}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.trainingYearName}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-3">
                                        <div class="row g-3">
                                            <a class="btn btn-phoenix-secondary btn-icon me-1 fs-10 text-body px-0 trainingYear-bulkEdit" href="#!" id="trainingYear-editBtn" data-id="${item.trainingYearID}"><i class="fas fa-edit"></i></a>
                                            <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 trainingYear-bulkDelete" href="#!" id="trainingYear-singleDelBtn" data-id="${item.trainingYearID}"><span class="fas fa-trash"></span></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#trainingYear-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#trainingYear-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#trainingYear-paginationLinks");
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
            $("#trainingYear-prevPageBtn").prop('disabled', currentPage === 1);
            $("#trainingYear-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));

