(function ($) {
    $.thirdTab = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#thirdTab-form',
            updateform: '#thirdTab-Updateform',
            saveBtn: '#thirdTab-saveBtn',
            editBtn: '#thirdTab-editBtn',
            resetBtn: '#thirdTab-resetBtn',
            bulkDelBtn: '#thirdTab-bulkDelBtn',
            singleDeleteBtn: '#thirdTab-singleDelBtn',
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

                var token = $('#thirdTab-form input[name="__RequestVerificationToken"]').val();
                var formData = {
                    __RequestVerificationToken: token,
                    GroupID: $('#GroupID').val(),
                    ClassID: $('#ClassID').val(),
                    GroupName: $('#GroupName').val(),
                    GroupCode: $('#GroupCode').val(),
                    Description: $('#Description').val(),
                }

                var id = $(settings.addform).find('#GroupID').val();
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
                        const allFields = ['ClassID', 'GroupName', 'GroupCode'];

                        allFields.forEach(function (fieldId) {
                            validateField(fieldId, response);
                        });

                        if (response.isSuccess == true) {
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
            $(document).on('click', '#thirdTab-edit', function (e) {
                e.preventDefault();

                $('.thirdTab-edit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');

                $.ajax({
                    url: getByIdUrl,
                    method: 'GET',
                    data: { id: id },
                    success: function (response) {
                        if (response.isSuccess) {
                            var data = response.data;
                            $('#thirdTab-form #GroupID').val(data.groupID);
                            classDD.setChoiceByValue(data.classID.toString());
                            $('#thirdTab-form #GroupName').val(data.groupName);
                            $('#thirdTab-form #GroupCode').val(data.groupCode);
                            $('#thirdTab-form #Description').val(data.description);

                            $('#thirdTab-form #thirdTab-saveBtn').text('Update');
                        } else {
                            toastr.warning(response.message);
                        }
                        $('.thirdTab-edit').prop('disabled', false).html('<i class="fas fa-edit"></i>');
                    }
                });
            });
            // #endregion


            // #region Delete
            $("#thirdTab-delSel").on('click', function () {
                var selectedItems = $(".thirdTab-selectItem:checked");
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
                                    $("#thirdTab-check-all").prop('checked', false);
                                    $('.thirdTab-selectItem').prop('checked', false);
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

            $(document).on('click', '#thirdTab-single-delete', function () {
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
                                    $("#thirdTab-check-all").prop('checked', false);
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
                $('#GroupID').val('0');
                resetValidation(['ClassID', 'GroupName', 'GroupCode']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#thirdTab-check-all").prop('checked', false);
                $('.thirdTab-selectItem').prop('checked', false);

                loadTableData();
                toggleBulkActions();
                $('#thirdTab-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            var typingTimer;
            var doneTypingInterval = 100; // Wait 500ms after user stops typing
            // #region checkNameUnique
            $(document).ready(function () {
                checkNameUnique();
            });

            function checkNameUnique() {
                $('#GroupNameName').on('input', function () {
                    var value = $(this).val();

                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess === true) {
                                $('#GroupNameError').hide();
                                $('input[name="GroupName"]').removeClass('is-invalid');
                            } else {
                                $('#GroupNameError').text(response.message).show();
                                $('input[name="GroupName"]').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                });
            }
            // #endregion


            // #region checkCodeUnique
            $('#GroupCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#GroupCodeError').hide();
                    $('#GroupCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#GroupCodeError').hide();
                                $('#GroupCode').removeClass('is-invalid');
                            } else {
                                $('#GroupCodeError').text(response.message).show();
                                $('#GroupCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#GroupCode').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region Dropdowns
            function initClassDD() {
                classDD = new Choices('#ClassID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select First Layer Account...'
                });
            }
            document.addEventListener('DOMContentLoaded', initClassDD);
            initClassDD();
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#thirdTab-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.thirdTab-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.thirdTab-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.thirdTab-selectItem');
                const checkedItems = $('.thirdTab-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#thirdTab-check-all').prop('checked', allChecked);
                $('#thirdTab-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#thirdTab-bulkSelectActions').removeClass('d-none');
                    $('#thirdTab-searchBox').addClass('d-none');
                    $('.thirdTab-bulkDelete').addClass('disabled');
                    $('.thirdTab-bulkEdit').addClass('disabled');
                } else {
                    $('#thirdTab-bulkSelectActions').addClass('d-none');
                    $('#thirdTab-searchBox').removeClass('d-none');
                    $('.thirdTab-bulkDelete').removeClass('disabled');
                    $('.thirdTab-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#thirdTab-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#thirdTab-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#thirdTab-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#thirdTab-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'thirdTabName';
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
            var searchTerm = $("#thirdTab-searchInput").val();

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
                    var tableBody = $("#thirdTab-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input thirdTab-selectItem" data-id="${item.groupID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.baseAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.className}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.groupName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.groupCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-2">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon me-2 thirdTab-edit" id="thirdTab-edit" data-id="${item.groupID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon thirdTab-single-delete" id="thirdTab-single-delete" data-id="${item.groupID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#thirdTab-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#thirdTab-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#thirdTab-paginationLinks");
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
            $("#thirdTab-prevPageBtn").prop('disabled', currentPage === 1);
            $("#thirdTab-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));