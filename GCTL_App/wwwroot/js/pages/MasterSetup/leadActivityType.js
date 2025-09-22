(function ($) {
    $.leadActivityType = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#leadActivityType-form',
            saveBtn: '#leadActivityType-saveBtn',
            editBtn: '#leadActivityType-editBtn',
            resetBtn: '#leadActivityType-resetBtn',
            bulkDelBtn: '#leadActivityType-bulkDelBtn',
            singleDeleteBtn: '#leadActivityType-singleDelBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {

            $('#leadActivityType-saveBtn').on('click', function (e) {
                e.preventDefault();
                debugger;
                var token = $('#leadActivityType-form input[name="__RequestVerificationToken"]').val();

                var formData = {
                    
                    __RequestVerificationToken: token,
                    LeadActivityTypeID: $('#LeadActivityTypeID').val(),
                    LeadActivityName: $('#LeadActivityName').val(),
                    LeadActivityIcon: $('#LeadActivityIcon').val(),
                    UseFor: $('#UseFor').val(),
                }
                showDev(formData);
                validateName();

                var id = $('#leadActivityType-form #LeadActivityTypeID').val();
                var url = '';
                if (id > 0) {
                    url = '/LeadActivityTypes/Update';
                } else {
                    url = '/LeadActivityTypes/Create';
                }

                $.ajax({
                    url: url,
                    type: 'POST',
                    data: formData,
                    success: function (data) {
                        if (data.isSuccess) {
                            toastr.success(data.message);
                            clear();
                            choiceManager.resetAllChoices();
                        } else {
                            toastr.info(data.message);
                        }
                    },
                    error: function (err) {
                        console.log(err);
                    }
                });
            });


            $(document).on('click', '#leadActivityType-edit', function (e) {
                e.preventDefault();

                var id = $(this).data('id');

                $.ajax({
                    url: '/LeadActivityTypes/GetById',
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            debugger;
                            var data = response.data;
                            $('#leadActivityType-form #LeadActivityTypeID').val(data.leadActivityTypeID);
                            $('#leadActivityType-form #LeadActivityName').val(data.leadActivityName);
                            $('#leadActivityType-form #LeadActivityIcon').val(data.leadActivityIcon);
                            //$('#leadActivityType-form #UseFor').val(data.useFor);

                            choiceManager.setChoiceValue('UseFor', data.useFor)
                            //$('#leadActivityType-form #UseFor').val(data.useFor);
                            
                            $('#leadActivityType-form #leadActivityType-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                    }
                });
            });



            $("#leadActivityType-delSel").on('click', function () {
                var selectedItems = $(".leadActivityType-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });
                
                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/LeadActivityTypes/SoftDelete',
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

            $(document).on('click', '#leadActivityType-single-delete', function () {
                debugger;
                var id = $(this).data('id');
                showDev(id)
                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: '/LeadActivityTypes/SoftDelete',
                            method: 'POST',
                            data: { Ids: [id] },
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




            $('#leadActivityType-resetBtn').on('click', function () {
                clear();
            })

            function clear() {
                $('#leadActivityType-form')[0].reset();
                $('#LeadActivityTypeID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $('#leadActivityType-form #leadActivityType-saveBtn').text('Save');
                $("#leadActivityType-check-all").prop('checked', false);
                $('.leadActivityType-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
                $('#leadActivityType-check-all').prop('checked', false).prop('indeterminate', false);
            }


            $('#LeadActivityName').on('input', function () {
                validateName();
            });


            function validateName() {
                var name = $('#LeadActivityName').val().trim();

                if (name === '') {
                    $('#LeadActivityName').css('border', '1px solid red');
                } else {
                    $('#LeadActivityName').css('border', '1px solid #ccc');
                }
            }


            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#LeadActivityName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: '/LeadActivityTypes/CheckNameUnique',
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response === true) {
                                $('#nameError').hide();
                                $('input[name="LeadActivityName"]').removeClass('is-invalid');
                            } else {
                                $('#nameError').text(response).show();
                                $('input[name="LeadActivityName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }





            $(document).ready(function () {
                $('#leadActivityType-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.leadActivityType-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.leadActivityType-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.leadActivityType-selectItem');
                const checkedItems = $('.leadActivityType-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#leadActivityType-check-all').prop('checked', allChecked);
                $('#leadActivityType-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#leadActivityType-bulkSelectActions').removeClass('d-none');
                    $('#leadActivityType-searchBox').addClass('d-none');
                    $('.leadActivityType-bulkDelete').addClass('disabled');
                    $('.leadActivityType-bulkEdit').addClass('disabled');
                } else {
                    $('#leadActivityType-bulkSelectActions').addClass('d-none');
                    $('#leadActivityType-searchBox').removeClass('d-none');
                    $('.leadActivityType-bulkDelete').removeClass('disabled');
                    $('.leadActivityType-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#leadActivityType-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#leadActivityType-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#leadActivityType-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#leadActivityType-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'LeadActivityName';
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
            var searchTerm = $("#leadActivityType-searchInput").val();

            $.ajax({
                url: '/LeadActivityTypes/GetAll',
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    showDev(response);
                    var tableBody = $("#leadActivityType-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input leadActivityType-selectItem" data-id="${item.leadActivityTypeID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.leadActivityName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.leadActivityIcon}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.useFor}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 leadActivityType-bulkDelete" href="#!" id="leadActivityType-edit" data-id="${item.leadActivityTypeID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 leadActivityType-bulkEdit" href="#!" id="leadActivityType-single-delete" data-id="${item.leadActivityTypeID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#leadActivityType-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#leadActivityType-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#leadActivityType-paginationLinks");
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
            $("#leadActivityType-prevPageBtn").prop('disabled', currentPage === 1);
            $("#leadActivityType-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));




