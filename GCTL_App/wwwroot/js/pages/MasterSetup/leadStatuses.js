(function ($) {
    $.leadStatus = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#leadStatus-form',
            saveBtn: '#leadStatus-saveBtn',
            editBtn: '#leadStatus-editBtn',
            resetBtn: '#leadStatus-resetBtn',
            bulkDelBtn: '#leadStatus-bulkDelBtn',
            singleDeleteBtn: '#leadStatus-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#leadStatus-saveBtn').on('click', function (e) {
                e.preventDefault();
                debugger;
                var token = $('#leadStatus-form input[name="__RequestVerificationToken"]').val();
                const isSpecial = $("#IsSpecial").is(":checked");
                showDev(isSpecial);
                var formData = {
                    __RequestVerificationToken: token,
                    LeadStatusID: $('#LeadStatusID').val(),
                    LeadStatusName: $('#LeadStatusName').val(),
                    IsSpecial: isSpecial,
                }

                validateName();

                var id = $('#leadStatus-form #LeadStatusID').val();
                var url = '';
                if (id > 0) {
                    url = '/LeadStatuses/Update';
                } else {
                    url = '/LeadStatuses/Create';
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


            $(document).on('click', '#leadStatus-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');
                

                $.ajax({
                    url: '/LeadStatuses/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#leadStatus-form #LeadStatusID').val(data.leadStatusID);
                            $('#leadStatus-form #LeadStatusName').val(data.leadStatusName);
                            if (data.isSpecial == true) {
                                $("#IsSpecial").prop("checked", true);
                            }
                            $("#")
                            $('#leadStatus-form #leadStatus-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#leadStatus-delSel").on('click', function () {
                var selectedItems = $(".leadStatus-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/LeadStatuses/SoftDelete',
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

            $(document).on('click', '#leadStatus-single-delete', function () {
                var id = $(this).data('id');


                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/LeadStatuses/SoftDelete',
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




            $('#leadStatus-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#leadStatus-form')[0].reset();
                $('#LeadStatusID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#leadStatus-form #leadStatus-saveBtn').text('Save');
                $("#leadStatus-check-all").prop('checked', false);
                $('.leadStatus-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#leadStatus-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#LeadStatusName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#LeadStatusName').val().trim();

                if (name === '') {
                    $('#LeadStatusName').css('border', '1px solid red');
                } else {
                    $('#LeadStatusName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#LeadStatusName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/LeadStatuses/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="LeadStatusName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="LeadStatusName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#leadStatus-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.leadStatus-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.leadStatus-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.leadStatus-selectItem');
                const checkedItems = $('.leadStatus-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#leadStatus-check-all').prop('checked', allChecked);
                $('#leadStatus-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#leadStatus-bulkSelectActions').removeClass('d-none');
                    $('#leadStatus-searchBox').addClass('d-none');
                    $('.leadStatus-bulkDelete').addClass('disabled');
                    $('.leadStatus-bulkEdit').addClass('disabled');
                } else {
                    $('#leadStatus-bulkSelectActions').addClass('d-none');
                    $('#leadStatus-searchBox').removeClass('d-none');
                    $('.leadStatus-bulkDelete').removeClass('disabled');
                    $('.leadStatus-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#leadStatus-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#leadStatus-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#leadStatus-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#leadStatus-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'LeadStatusName';
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
            var searchTerm = $("#leadStatus-searchInput").val();

            $.ajax({
                url: '/LeadStatuses/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#leadStatus-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input leadStatus-selectItem" data-id="${item.leadStatusID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.leadStatusName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.isSpecial}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 leadStatus-bulkDelete" href="#!" id="leadStatus-edit" data-id="${item.leadStatusID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 leadStatus-bulkEdit" href="#!" id="leadStatus-single-delete" data-id="${item.leadStatusID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#leadStatus-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#leadStatus-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#leadStatus-paginationLinks");
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
            $("#leadStatus-prevPageBtn").prop('disabled', currentPage === 1);
            $("#leadStatus-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




