(function ($) {
    $.designation = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#designation-form',
            saveBtn: '#designation-saveBtn',
            editBtn: '#designation-editBtn',
            resetBtn: '#designation-resetBtn',
            bulkDelBtn: '#designation-bulkDelBtn',
            singleDeleteBtn: '#designation-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#designation-saveBtn').on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#designation-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    DesignationID: $('#DesignationID').val(),
                    DesignationName: $('#DesignationName').val(),
                }

                validateName();

                var id = $('#designation-form #DesignationID').val();
                var url = '';
                if (id > 0) {
                    url = '/Designations/Update';
                } else {
                    url = '/Designations/Create';
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


            $(document).on('click', '#designation-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/Designations/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#designation-form #DesignationID').val(data.designationID);
                            $('#designation-form #DesignationName').val(data.designationName);

                            $('#designation-form #designation-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#designation-delSel").on('click', function () {
                var selectedItems = $(".designation-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/Designations/SoftDelete',
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

            $(document).on('click', '#designation-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/Designations/SoftDelete',
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




            $('#designation-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#designation-form')[0].reset();
                $('#DesignationID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#designation-form #designation-saveBtn').text('Save');
                $("#designation-check-all").prop('checked', false);
                $('.designation-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#designation-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#DesignationName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#DesignationName').val().trim();

                if (name === '') {
                    $('#DesignationName').css('border', '1px solid red');
                } else {
                    $('#DesignationName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#DesignationName').on('input', function () {
                    var name = $(this).val();
                    var id = $('#DepartmentID').val();

                    $.ajax({
                        url: '/Designations/CheckNameUnique',
                        type: 'POST',
                        data: { id: id, name: name },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="DesignationName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="DesignationName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }






            $(document).ready(function () {
                $('#designation-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.designation-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.designation-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.designation-selectItem');
                const checkedItems = $('.designation-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#designation-check-all').prop('checked', allChecked);
                $('#designation-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#designation-bulkSelectActions').removeClass('d-none');
                    $('#designation-searchBox').addClass('d-none');
                    $('.designation-bulkDelete').addClass('disabled');
                    $('.designation-bulkEdit').addClass('disabled');
                } else {
                    $('#designation-bulkSelectActions').addClass('d-none');
                    $('#designation-searchBox').removeClass('d-none');
                    $('.designation-bulkDelete').removeClass('disabled');
                    $('.designation-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#designation-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#designation-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#designation-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#designation-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'DesignationName';
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
            var searchTerm = $("#designation-searchInput").val();

            $.ajax({
                url: '/Designations/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#designation-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input designation-selectItem" data-id="${item.designationID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0" style="width: 10%;">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.designationName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 designation-bulkDelete" href="#!" id="designation-edit" data-id="${item.designationID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 designation-bulkEdit" href="#!" id="designation-single-delete" data-id="${item.designationID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#designation-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#designation-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#designation-paginationLinks");
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
            $("#designation-prevPageBtn").prop('disabled', currentPage === 1);
            $("#designation-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });


        $(document).ready(function () {
            $('.designationSelectTwo').select2({
                placeholder: "Select Department",
                allowClear: true,
                dropdownAutoWidth: true,
                width: '100%'
            });

            $('.designationSelectTwo').next('.select2-container').css({
                'width': '100%',
                'height': '100%'
            });

            $('.designationSelectTwo').next('.select2-container').find('.select2-selection').css({
                'height': '50px',
                'line-height': '100%'
            });
        });



    }
}(jQuery));




