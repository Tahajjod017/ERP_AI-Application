(function ($) {
    $.baseAccount = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#baseAccount-form',
            updateform: '#baseAccount-Updateform',
            saveBtn: '#baseAccount-saveBtn',
            editBtn: '#baseAccount-editBtn',
            resetBtn: '#baseAccount-resetBtn',
            bulkDelBtn: '#baseAccount-bulkDelBtn',
            singleDeleteBtn: '#baseAccount-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";

        $(() => {


            // #region Frontend Validataion
            //$(settings.saveBtn).on('click', function () {
            //    debugger
            //    $(settings.addform).find('input[required], select[required], textarea[required]').each(function () {
            //        debugger
            //        if (!$(settings.addform).valid()) {
            //            $(settings.addform).addClass('is-invalid');
            //        } else {
            //            $(settings.addform).removeClass('is-invalid');
            //        }
            //    });
            //});
            //// Remove red border when user fixes input
            //$('input[required], select[required], textarea[required]').on('keyup change', function () {
            //    if ($(settings.addform).valid()) {
            //        $(settings.addform).removeClass('is-invalid');
            //    }
            //});
            // #endregion


            // #region Save 
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                //if (!$(settings.addform).valid()) {
                //    $(settings.saveBtn).prop('disabled', false).html('Save');
                //    return; 
                //}

                var token = $('#baseAccount-form input[name="__RequestVerificationToken"]').val();
                var formData = {
                    __RequestVerificationToken: token,
                    BaseAccountID: $('#BaseAccountID').val(),
                    BaseAccountName: $('#BaseAccountName').val(),
                    BaseAccountCode: $('#BaseAccountCode').val(),
                    Description: $('#Description').val(),
                }

                var id = $(settings.addform).find('#BaseAccountID').val();
                var url = '';
                var type = '';
                if (id > 0) {
                    url = updateUrl;
                    type = 'PUT'
                } else {
                    url = createUrl;
                    type = 'POST'
                }

                $.ajax({
                    url: url,
                    type: type,
                    data: formData,
                    beforeSend: function () {
                        showLoadingIndicator();
                    },
                    success: function (response) {
                        const allFields = ['BaseAccountName', 'BaseAccountCode'];

                        allFields.forEach(function (fieldId) {
                            validateField(fieldId, response);
                        });

                        if (response.isSuccess) {
                            clear();
                            toastr.success(response.message);
                        } else {
                            toastr.info(response.message);
                        }
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    },
                    error: function (err) {
                        console.log(err);
                        $(settings.saveBtn).prop('disabled', false).html('Save');
                    },
                    complete: function () {
                        hideLoadingIndicator();
                    }
                });
            });
            // #endregion


            // #region Edit
            $(document).on('click', '#baseAccount-edit', function (e) {
                e.preventDefault();

                $('.baseAccount-edit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');

                $.ajax({
                    url: getByIdUrl,
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#baseAccount-form #BaseAccountID').val(data.baseAccountID);
                            $('#baseAccount-form #BaseAccountCode').val(data.baseAccountCode);
                            $('#baseAccount-form #BaseAccountName').val(data.baseAccountName);
                            $('#baseAccount-form #Description').val(data.description);

                            $('#baseAccount-form #BaseAccount-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                        $('.baseAccount-edit').prop('disabled', false).html('<i class="fas fa-edit"></i>');
                    }
                });
            });
            // #endregion


            // #region Delete
            $("#baseAccount-delSel").on('click', function () {
                var selectedItems = $(".baseAccount-selectItem:checked");
                var selectedIds = [];

                selectedItems.each(function () {
                    selectedIds.push($(this).data('id'));
                });

                if (selectedIds.length > 0) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
                            method: 'DELETE',
                            data: { ids: selectedIds },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    $("#baseAccount-check-all").prop('checked', false);
                                    $('.baseAccount-selectItem').prop('checked', false);
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

            $(document).on('click', '#baseAccount-single-delete', function () {
                var id = $(this).data('id');

                if (id) {
                    showDeleteModal(function () {
                        $.ajax({
                            url: deleteUrl,
                            method: 'DELETE',
                            data: { ids: [id] },
                            success: function (response) {
                                if (response.isSuccess) {
                                    toastr.success(response.message);
                                    $("#baseAccount-check-all").prop('checked', false);
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
            // #endregion


            // #region Clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();

                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#BaseAccountID').val('0');
                resetValidation(['BaseAccountName', 'BaseAccountCode']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#baseAccount-check-all").prop('checked', false);
                $('.baseAccount-selectItem').prop('checked', false);
                
                loadTableData();
                toggleBulkActions();
                $('#baseAccount-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            // #region checkNameUnique
            var typingTimer;
            var doneTypingInterval = 500; // Wait 500ms after user stops typing

            $('#BaseAccountName').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#BaseAccountNameError').hide();
                    $('#BaseAccountName').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#BaseAccountNameError').hide();
                                $('#BaseAccountName').removeClass('is-invalid');
                            } else {
                                $('#BaseAccountNameError').text(response.message).show();
                                $('#BaseAccountName').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#BaseAccountName').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region checkCodeUnique
            $('#BaseAccountCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#BaseAccountCodeError').hide();
                    $('#BaseAccountCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#BaseAccountCodeError').hide();
                                $('#BaseAccountCode').removeClass('is-invalid');
                            } else {
                                $('#BaseAccountCodeError').text(response.message).show();
                                $('#BaseAccountCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#BaseAccountCode').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#baseAccount-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.baseAccount-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.baseAccount-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.baseAccount-selectItem');
                const checkedItems = $('.baseAccount-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#baseAccount-check-all').prop('checked', allChecked);
                $('#baseAccount-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#baseAccount-bulkSelectActions').removeClass('d-none');
                    $('#baseAccount-searchBox').addClass('d-none');
                    $('.baseAccount-bulkDelete').addClass('disabled');
                    $('.baseAccount-bulkEdit').addClass('disabled');
                } else {
                    $('#baseAccount-bulkSelectActions').addClass('d-none');
                    $('#baseAccount-searchBox').removeClass('d-none');
                    $('.baseAccount-bulkDelete').removeClass('disabled');
                    $('.baseAccount-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#baseAccount-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#baseAccount-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#baseAccount-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#baseAccount-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'baseAccountName';
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
            var searchTerm = $("#baseAccount-searchInput").val();

            $.ajax({
                url: getAllUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder
                },
                success: function (response) {
                    var tableBody = $("#baseAccount-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input baseAccount-selectItem" data-id="${item.baseAccountID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.baseAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.baseAccountCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-2">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon me-2 baseAccount-edit" id="baseAccount-edit" data-id="${item.baseAccountID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon baseAccount-single-delete" id="baseAccount-single-delete" data-id="${item.baseAccountID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#baseAccount-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#baseAccount-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#baseAccount-paginationLinks");
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
            $("#baseAccount-prevPageBtn").prop('disabled', currentPage === 1);
            $("#baseAccount-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));