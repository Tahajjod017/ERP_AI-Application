(function ($) {
    $.secondTab = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#secondTab-form',
            updateform: '#secondTab-Updateform',
            saveBtn: '#secondTab-saveBtn',
            editBtn: '#secondTab-editBtn',
            resetBtn: '#secondTab-resetBtn',
            bulkDelBtn: '#secondTab-bulkDelBtn',
            singleDeleteBtn: '#secondTab-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";

        $(() => {




            // #region Save 
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#secondTab-form input[name="__RequestVerificationToken"]').val();
                var formData = {
                    __RequestVerificationToken: token,
                    ClassID: $('#ClassID').val(),
                    BaseAccountID: $('#BaseAccountID').val(),
                    ClassName: $('#ClassName').val(),
                    ClassCode: $('#ClassCode').val(),
                    Description: $('#Description').val(),
                }

                var id = $(settings.addform).find('#ClassID').val();
                var url = '';
                var type = '';
                if (id > 0) {
                    url = updateUrl;
                    type = 'POST'
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
                        const allFields = ['BaseAccountID', 'ClassName', 'ClassCode'];

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
            $(document).on('click', '#secondTab-edit', function (e) {
                e.preventDefault();

                $('.secondTab-edit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');

                $.ajax({
                    url: getByIdUrl,
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#secondTab-form #ClassID').val(data.classID);
                            baseAccountDD.setChoiceByValue(data.baseAccountID.toString());
                            $('#secondTab-form #ClassCode').val(data.classCode);
                            $('#secondTab-form #ClassName').val(data.className);
                            $('#secondTab-form #Description').val(data.description);

                            $('#secondTab-form #BaseAccount-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                        $('.secondTab-edit').prop('disabled', false).html('<i class="fas fa-edit"></i>');
                    }
                });
            });
            // #endregion


            // #region Delete
            $("#secondTab-delSel").on('click', function () {
                var selectedItems = $(".secondTab-selectItem:checked");
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
                                    $("#secondTab-check-all").prop('checked', false);
                                    $('.secondTab-selectItem').prop('checked', false);
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

            $(document).on('click', '#secondTab-single-delete', function () {
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
                                    $("#secondTab-check-all").prop('checked', false);
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
                $('#ClassID').val('0');
                resetValidation(['BaseAccountID', 'ClassName', 'ClassCode']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#secondTab-check-all").prop('checked', false);
                $('.secondTab-selectItem').prop('checked', false);

                if (baseAccountDD) {
                    baseAccountDD.destroy();
                }

                initBaseAccountDD();

                loadTableData();
                toggleBulkActions();
                $('#secondTab-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            var typingTimer;
            var doneTypingInterval = 100; // Wait 500ms after user stops typing
            // #region checkNameUnique
            $('#ClassName').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#ClassNameError').hide();
                    $('#ClassName').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#ClassNameError').hide();
                                $('#ClassName').removeClass('is-invalid');
                            } else {
                                $('#ClassNameError').text(response.message).show();
                                $('#ClassName').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#ClassName').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region checkCodeUnique
            $('#ClassCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#ClassCodeError').hide();
                    $('#ClassCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#ClassCodeError').hide();
                                $('#ClassCode').removeClass('is-invalid');
                            } else {
                                $('#ClassCodeError').text(response.message).show();
                                $('#ClassCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#ClassCode').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region Dropdowns
            function initBaseAccountDD() {
                baseAccountDD = new Choices('#BaseAccountID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Base Account...'
                });
            }
            document.addEventListener('DOMContentLoaded', initBaseAccountDD);
            initBaseAccountDD();
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#secondTab-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.secondTab-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.secondTab-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.secondTab-selectItem');
                const checkedItems = $('.secondTab-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#secondTab-check-all').prop('checked', allChecked);
                $('#secondTab-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#secondTab-bulkSelectActions').removeClass('d-none');
                    $('#secondTab-searchBox').addClass('d-none');
                    $('.secondTab-bulkDelete').addClass('disabled');
                    $('.secondTab-bulkEdit').addClass('disabled');
                } else {
                    $('#secondTab-bulkSelectActions').addClass('d-none');
                    $('#secondTab-searchBox').removeClass('d-none');
                    $('.secondTab-bulkDelete').removeClass('disabled');
                    $('.secondTab-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#secondTab-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#secondTab-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#secondTab-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#secondTab-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'secondTabName';
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
            var searchTerm = $("#secondTab-searchInput").val();

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
                    var tableBody = $("#secondTab-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input secondTab-selectItem" data-id="${item.classID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.baseAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.className}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.classCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-2">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon me-2 secondTab-edit" id="secondTab-edit" data-id="${item.classID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon secondTab-single-delete" id="secondTab-single-delete" data-id="${item.classID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#secondTab-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#secondTab-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#secondTab-paginationLinks");
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
            $("#secondTab-prevPageBtn").prop('disabled', currentPage === 1);
            $("#secondTab-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));