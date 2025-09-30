(function ($) {
    $.addmainaccount = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#addMainAccount-form',
            updateform: '#addMainAccount-Updateform',
            saveBtn: '#addMainAccount-saveBtn',
            editBtn: '#addMainAccount-editBtn',
            resetBtn: '#addMainAccount-resetBtn',
            bulkDelBtn: '#addMainAccount-bulkDelBtn',
            singleDeleteBtn: '#addMainAccount-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var getGroupByClassIdUrl = settings.baseUrl + "/GetAccountGroupByClassId";

        $(() => {


            // #region Save 
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#addMainAccount-form input[name="__RequestVerificationToken"]').val();
                var formData = {
                    __RequestVerificationToken: token,
                    MainAccountID: $('#MainAccountID').val(),
                    ClassID: $('#ClassID').val(),
                    GroupID: $('#GroupID').val(),
                    MainAccountName: $('#MainAccountName').val(),
                    MainAccountCode: $('#MainAccountCode').val(),
                    Description: $('#Description').val(),
                }

                var id = $(settings.addform).find('#MainAccountID').val();
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
                        const allFields = ['ClassID', 'GroupID', 'MainAccountName', 'MainAccountCode'];

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
            $(document).on('click', '#addMainAccount-edit', function (e) {
                e.preventDefault();

                $('.addMainAccount-edit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');

                $.ajax({
                    url: getByIdUrl,
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#addMainAccount-form #MainAccountID').val(data.mainAccountID);
                            $('#addMainAccount-form #MainAccountCode').val(data.mainAccountCode);
                            $('#addMainAccount-form #MainAccountName').val(data.mainAccountName);
                            $('#addMainAccount-form #Description').val(data.description);

                            $('#addMainAccount-form #BaseAccount-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                        $('.addMainAccount-edit').prop('disabled', false).html('<i class="fas fa-edit"></i>');
                    }
                });
            });
            // #endregion


            // #region Delete
            $("#addMainAccount-delSel").on('click', function () {
                var selectedItems = $(".addMainAccount-selectItem:checked");
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
                                    $("#addMainAccount-check-all").prop('checked', false);
                                    $('.addMainAccount-selectItem').prop('checked', false);
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

            $(document).on('click', '#addMainAccount-single-delete', function () {
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
                                    $("#addMainAccount-check-all").prop('checked', false);
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


            // #region GetAccountGroupByClassId
            $('#ClassID').on('change', function (e) {
                e.preventDefault();

                var id = $(this).val();

                $.ajax({
                    url: getGroupByClassIdUrl,
                    type: 'GET',
                    data: { id: id },
                    success: function (result) {
                        $('.chat-thread-tab').empty();

                        if (result.length === 0) {
                            $('.chat-thread-tab').append('<li class="nav-item text-center">No Data Found</li>');
                        } else {
                            result.forEach(function (item) {
                                var listItem = `
                                    <li class="nav-item read border-bottom" role="presentation">
                                        <a class="nav-link d-flex align-items-center justify-content-center p-2" data-bs-toggle="tab" data-chat-thread="data-chat-thread" href="#tab-thread-${item.id}" role="tab" aria-selected="false" data-group-id="${item.id}">
                                            <div class="flex-1 d-sm-none d-xl-block">
                                                <div class="d-flex justify-content-between align-items-center">
                                                    <h5 class="text-body fw-normal name text-nowrap">${item.name}</h5>
                                                </div>
                                                <div class="d-flex justify-content-between">
                                                    <p class="fs-9 mb-0 line-clamp-1 text-body-tertiary text-opacity-85 message">${item.groupName}</p>
                                                </div>
                                            </div>
                                        </a>
                                    </li>
                                `;

                                $('.chat-thread-tab').append(listItem);
                            });

                            $('.chat-thread-tab .nav-link').on('click', function () {
                                // Remove 'active' class from all <a> tags
                                $('.chat-thread-tab .nav-link').removeClass('active');

                                // Add 'active' class to the clicked <a> tag
                                $(this).addClass('active');

                                // Optionally, you can also highlight the parent <li> by adding a custom class
                                $(this).closest('li').siblings().removeClass('selected'); // Remove from all siblings
                                $(this).closest('li').addClass('selected'); // Add to the clicked li

                                // Set the GroupID in the hidden input field
                                var groupId = $(this).data('group-id'); 
                                $('#GroupID').val(groupId); 
                            });
                        }
                    },
                    error: function () {
                        console.error('Something went wrong!');
                    }
                });
            });
            // #endregion


            // #region Clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();

                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#MainAccountID').val('0');
                resetValidation(['MainAccountName', 'MainAccountCode']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#addMainAccount-check-all").prop('checked', false);
                $('.addMainAccount-selectItem').prop('checked', false);

                if (accountClassDD) {
                    accountClassDD.destroy();
                }
                initAccountClassDD();

                loadTableData();
                toggleBulkActions();
                $('#addMainAccount-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            // #region Dropdowns
            function initAccountClassDD() {
                accountClassDD = new Choices('#ClassID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Class...'
                });
            }
            document.addEventListener('DOMContentLoaded', initAccountClassDD);
            initAccountClassDD();
            // #endregion


            // #region checkNameUnique
            var typingTimer;
            var doneTypingInterval = 100; // Wait 500ms after user stops typing

            $('#MainAccountName').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#MainAccountNameError').hide();
                    $('#MainAccountName').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#MainAccountNameError').hide();
                                $('#MainAccountName').removeClass('is-invalid');
                            } else {
                                $('#MainAccountNameError').text(response.message).show();
                                $('#MainAccountName').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#MainAccountName').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region checkCodeUnique
            $('#MainAccountCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#MainAccountCodeError').hide();
                    $('#MainAccountCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#MainAccountCodeError').hide();
                                $('#MainAccountCode').removeClass('is-invalid');
                            } else {
                                $('#MainAccountCodeError').text(response.message).show();
                                $('#MainAccountCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#MainAccountCode').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#addMainAccount-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.addMainAccount-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.addMainAccount-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.addMainAccount-selectItem');
                const checkedItems = $('.addMainAccount-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#addMainAccount-check-all').prop('checked', allChecked);
                $('#addMainAccount-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#addMainAccount-bulkSelectActions').removeClass('d-none');
                    $('#addMainAccount-searchBox').addClass('d-none');
                    $('.addMainAccount-bulkDelete').addClass('disabled');
                    $('.addMainAccount-bulkEdit').addClass('disabled');
                } else {
                    $('#addMainAccount-bulkSelectActions').addClass('d-none');
                    $('#addMainAccount-searchBox').removeClass('d-none');
                    $('.addMainAccount-bulkDelete').removeClass('disabled');
                    $('.addMainAccount-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#addMainAccount-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#addMainAccount-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#addMainAccount-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#addMainAccount-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'addMainAccountName';
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
            var searchTerm = $("#addMainAccount-searchInput").val();

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
                    var tableBody = $("#addMainAccount-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input addMainAccount-selectItem" data-id="${item.mainAccountID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.mainAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.mainAccountCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-2">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon me-2 addMainAccount-edit" id="addMainAccount-edit" data-id="${item.mainAccountID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon addMainAccount-single-delete" id="addMainAccount-single-delete" data-id="${item.mainAccountID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#addMainAccount-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#addMainAccount-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#addMainAccount-paginationLinks");
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
            $("#addMainAccount-prevPageBtn").prop('disabled', currentPage === 1);
            $("#addMainAccount-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));