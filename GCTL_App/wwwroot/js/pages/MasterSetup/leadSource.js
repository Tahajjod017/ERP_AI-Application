(function ($) {
    $.leadSource = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#leadSource-form',
            saveBtn: '#leadSource-saveBtn',
            editBtn: '#leadSource-editBtn',
            resetBtn: '#leadSource-resetBtn',
            bulkDelBtn: '#leadSource-bulkDelBtn',
            singleDeleteBtn: '#leadSource-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#leadSource-saveBtn').on('click', function (e) {
                e.preventDefault();

                var token = $('#leadSource-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    LeadSourceID: $('#LeadSourceID').val(),
                    LeadSourceName: $('#LeadSourceName').val(),
                }

                validateName();

                var id = $('#leadSource-form #LeadSourceID').val();
                var url = '';
                if (id > 0) {
                    url = '/LeadSources/Update';
                } else {
                    url = '/LeadSources/Create';
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


            $(document).on('click', '#leadSource-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');


                $.ajax({
                    url: '/LeadSources/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#leadSource-form #LeadSourceID').val(data.leadSourceID);
                            $('#leadSource-form #LeadSourceName').val(data.leadSourceName);

                            $('#leadSource-form #leadSource-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#leadSource-delSel").on('click', function () {
                var selectedItems = $(".leadSource-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/LeadSources/SoftDelete',
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

            $(document).on('click', '#leadSource-single-delete', function () {
                var id = $(this).data('id');


                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/LeadSources/SoftDelete',
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




            $('#leadSource-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#leadSource-form')[0].reset();
                $('#LeadSourceID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#leadSource-form #leadSource-saveBtn').text('Save');
                $("#leadSource-check-all").prop('checked', false);
                $('.leadSource-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#leadSource-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#LeadSourceName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#LeadSourceName').val().trim();

                if (name === '') {
                    $('#LeadSourceName').css('border', '1px solid red');
                } else {
                    $('#LeadSourceName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#LeadSourceName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/LeadSources/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="LeadSourceName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="LeadSourceName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#leadSource-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.leadSource-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.leadSource-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.leadSource-selectItem');
                const checkedItems = $('.leadSource-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#leadSource-check-all').prop('checked', allChecked);
                $('#leadSource-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#leadSource-bulkSelectActions').removeClass('d-none');
                    $('#leadSource-searchBox').addClass('d-none');
                    $('.leadSource-bulkDelete').addClass('disabled');
                    $('.leadSource-bulkEdit').addClass('disabled');
                } else {
                    $('#leadSource-bulkSelectActions').addClass('d-none');
                    $('#leadSource-searchBox').removeClass('d-none');
                    $('.leadSource-bulkDelete').removeClass('disabled');
                    $('.leadSource-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#leadSource-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#leadSource-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#leadSource-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#leadSource-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'leadSourceName';
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
            var searchTerm = $("#leadSource-searchInput").val();

            $.ajax({
                url: '/LeadSources/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#leadSource-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input leadSource-selectItem" data-id="${item.leadSourceID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.leadSourceName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 leadSource-bulkDelete" href="#!" id="leadSource-edit" data-id="${item.leadSourceID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 leadSource-bulkEdit" href="#!" id="leadSource-single-delete" data-id="${item.leadSourceID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#leadSource-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#leadSource-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#leadSource-paginationLinks");
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
            $("#leadSource-prevPageBtn").prop('disabled', currentPage === 1);
            $("#leadSource-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));