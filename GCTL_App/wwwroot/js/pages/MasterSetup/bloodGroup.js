(function ($) {
    $.bloodGroup = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#bloodGroup-form',
            saveBtn: '#bloodGroup-saveBtn',
            editBtn: '#bloodGroup-editBtn',
            resetBtn: '#bloodGroup-resetBtn',
            bulkDelBtn: '#bloodGroup-bulkDelBtn',
            singleDeleteBtn: '#bloodGroup-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#bloodGroup-saveBtn').on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#bloodGroup-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    BloodGroupID: $('#BloodGroupID').val(),
                    BloodGroupName: $('#BloodGroupName').val(),
                }

                validateName();

                var id = $('#bloodGroup-form #BloodGroupID').val();
                var url = '';
                if (id > 0) {
                    url = '/BloodGroups/Update';
                } else {
                    url = '/BloodGroups/Create';
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


            $(document).on('click', '#bloodGroup-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/BloodGroups/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#bloodGroup-form #BloodGroupID').val(data.bloodGroupID);
                            $('#bloodGroup-form #BloodGroupName').val(data.bloodGroupName);

                            $('#bloodGroup-form #bloodGroup-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#bloodGroup-delSel").on('click', function () {
                var selectedItems = $(".bloodGroup-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/BloodGroups/SoftDelete',
                            method: 'POST',
                            data: { ids: selectedIds },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    $("#bloodGroup-check-all").prop('checked', false);
                                    $('.bloodGroup-selectItem').prop('checked', false);
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

            $(document).on('click', '#bloodGroup-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/BloodGroups/SoftDelete',
                            method: 'POST',
                            data: { ids: [id] },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    $("#bloodGroup-check-all").prop('checked', false);
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




            $('#bloodGroup-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#bloodGroup-form')[0].reset();
                $('#BloodGroupID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#bloodGroup-form #bloodGroup-saveBtn').text('Save');
                loadTableData();
                toggleBulkActions();
                $('#bloodGroup-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#BloodGroupName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#BloodGroupName').val().trim();

                if (name === '') {
                    $('#BloodGroupName').css('border', '1px solid red');
                } else {
                    $('#BloodGroupName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#BloodGroupName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/BloodGroups/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="BloodGroupName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="BloodGroupName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#bloodGroup-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.bloodGroup-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.bloodGroup-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.bloodGroup-selectItem');
                const checkedItems = $('.bloodGroup-selectItem:checked');

                const total = allItems.length;
                const checkedCount = checkedItems.length;
                const allChecked = total > 0 && checkedCount === total;
                const someChecked = checkedCount > 0 && checkedCount < total;

                $('#bloodGroup-check-all').prop('checked', allChecked).prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#bloodGroup-bulkSelectActions').removeClass('d-none');
                    $('#bloodGroup-searchBox').addClass('d-none');
                    $('.bloodGroup-bulkDelete').addClass('disabled');
                    $('.bloodGroup-bulkEdit').addClass('disabled');
                } else {
                    $('#bloodGroup-bulkSelectActions').addClass('d-none');
                    $('#bloodGroup-searchBox').removeClass('d-none');
                    $('.bloodGroup-bulkDelete').removeClass('disabled');
                    $('.bloodGroup-bulkEdit').removeClass('disabled');
                }
            }



        });

        var currentPage = 1;
        var pageSize = 5;

        $('#bloodGroup-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#bloodGroup-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#bloodGroup-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#bloodGroup-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'BloodGroupName';
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
            var searchTerm = $("#bloodGroup-searchInput").val();

            $.ajax({
                url: '/BloodGroups/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#bloodGroup-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input bloodGroup-selectItem" data-id="${item.bloodGroupID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bloodGroupName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 bloodGroup-bulkDelete" href="#!" id="bloodGroup-edit" data-id="${item.bloodGroupID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 bloodGroup-bulkEdit" href="#!" id="bloodGroup-single-delete" data-id="${item.bloodGroupID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#bloodGroup-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#bloodGroup-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#bloodGroup-paginationLinks");
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
            $("#bloodGroup-prevPageBtn").prop('disabled', currentPage === 1);
            $("#bloodGroup-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });


    }
}(jQuery));

