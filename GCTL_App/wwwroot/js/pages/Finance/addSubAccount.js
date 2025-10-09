(function ($) {
    $.addsubaccount = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#addSubAccount-form',
            updateform: '#addSubAccount-Updateform',
            saveBtn: '#addSubAccount-saveBtn',
            editBtn: '#addSubAccount-editBtn',
            resetBtn: '#addSubAccount-resetBtn',
            bulkDelBtn: '#addSubAccount-bulkDelBtn',
            singleDeleteBtn: '#addSubAccount-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var getGroupByClassIdUrl = settings.baseUrl + "/GetMainAccByClassId";
        var generateNextCodeUrl = settings.baseUrl + "/GenerateNextCodeAsync";

        $(() => {


            


            // #region Save 
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#addSubAccount-form input[name="__RequestVerificationToken"]').val();
                var formData = {
                    __RequestVerificationToken: token,
                    SubAccountID: $('#SubAccountID').val(),
                    ClassID: $('#ClassID').val(),
                    MainAccountID: $('#MainAccountID').val(),
                    SubAccountName: $('#SubAccountName').val(),
                    SubAccountCode: $('#SubAccountCode').val(),
                    Description: $('#Description').val(),
                }

                var id = $(settings.addform).find('#SubAccountID').val();
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
                        const allFields = ['ClassID', 'MainAccountID', 'SubAccountName', 'SubAccountCode'];

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
            $(document).on('click', settings.editBtn, async function (e) {
                e.preventDefault();

                $('.addSubAccount-edit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');
                try {
                    const response = await $.get(getByIdUrl, { id });
                    if (response.isSuccess == true) {
                        const data = response.data;

                        $('#addSubAccount-form #SubAccountID').val(data.subAccountID);
                        $('#addSubAccount-form #SubAccountCode').val(data.subAccountCode);
                        $('#addSubAccount-form #SubAccountName').val(data.subAccountName);
                        $('#addSubAccount-form #Description').val(data.description);
                        accountClassDD.setChoiceByValue(data.classID.toString());

                        await getMainAccByClassId(data.classID);  // Wait for group options to load

                        await getMainAccByClassId(data.classID, data.mainAccountID, true);

                        $('#addSubAccount-form #addSubAccount-saveBtn').text('Update');
                        window.scrollTo({ top: 0, behavior: 'smooth' });
                    } else {
                        toastr.warning(response.message);
                    }
                } catch (error) {
                    console.error("Edit load failed:", error);
                }
            })
            // #endregion


            // #region Delete
            $("#addSubAccount-delSel").on('click', function () {
                var selectedItems = $(".addSubAccount-selectItem:checked");
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
                                    $("#addSubAccount-check-all").prop('checked', false);
                                    $('.addSubAccount-selectItem').prop('checked', false);
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

            $(document).on('click', '#addSubAccount-single-delete', function () {
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
                                    $("#addSubAccount-check-all").prop('checked', false);
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


            // #region getMainAccByClassId
            $('#ClassID').on('change', function (e) {
                e.preventDefault();

                var classId = $('#ClassID').val();

                getMainAccByClassId(classId);
            });

            function getMainAccByClassId(classId, mainAccountId = null, skipGenerateNextCode = false) {
                $.ajax({
                    url: getGroupByClassIdUrl,
                    type: 'GET',
                    data: {
                        classId: classId
                    },
                    success: function (result) {
                        $('.addSubAccount-ul').empty();

                        $('.addSubAccount-ul')
                            .append(`<div class="row">
                            <div class="col-md-12">
                                <label class="form-label"><strong>Select a Main Account</strong><span style="color:red;">*&nbsp;</span></label>
                            </div>
                            <span asp-validation-for="MainAccountID" id="MainAccountIDError" class="text-danger" style="display:none;"></span>
                        </div>`);

                        if (result.length === 0) {
                            $('.addSubAccount-ul').append('<li class="nav-item text-center">No Data Found</li>');
                        } else {
                            result.forEach(function (item) {
                                var listItem = `
                        <li class="nav-item read border-bottom" role="presentation">
                            <a class="nav-link d-flex align-items-center justify-content-center p-2" href="#tab-thread-${item.id}" role="tab" aria-selected="false" data-mainaccount-id="${item.id}">
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

                                $('.addSubAccount-ul').append(listItem);
                            });

                            // Click handler
                            $('.addSubAccount-ul .nav-link').on('click', function () {
                                $('.addSubAccount-ul .nav-link').removeClass('active');
                                $(this).addClass('active');

                                $(this).closest('li').siblings().removeClass('selected');
                                $(this).closest('li').addClass('selected');

                                var mainAccountID = $(this).data('mainaccount-id');
                                $('#MainAccountID').val(mainAccountID);
                                if (!skipGenerateNextCode) {
                                    generateNextCode(mainAccountID);
                                }
                                loadTableData();
                            });

                            // 🔽 If editing, auto-select the mainaccount
                            if (mainAccountId) {
                                const target = $(`.addSubAccount-ul .nav-link[data-mainaccount-id="${mainAccountId}"]`);
                                if (target.length) {
                                    target.trigger('click'); // Triggers the same click handler
                                }
                            }
                        }
                    },
                    error: function () {
                        console.error('Something went wrong!');
                    }
                });
            }
            // #endregion


            // #region generateNextCode
            function generateNextCode(mainAccountID) {

                $.ajax({
                    url: generateNextCodeUrl,
                    type: 'GET',
                    data: { mainAccId: mainAccountID },
                    success: function (result) {
                        $('#SubAccountCode').val(result);
                    },
                    error: function () {
                        console.error('Error generating next code!');
                    }
                })
            }
            // #endregion


            // #region Clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();

                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#SubAccountID').val('0');
                resetValidation(['ClassID', 'MainAccountID', 'SubAccountName', 'SubAccountCode']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#addSubAccount-check-all").prop('checked', false);
                $('.addSubAccount-selectItem').prop('checked', false);
                $('.addSubAccount-ul').empty();

                if (accountClassDD) {
                    accountClassDD.destroy();
                }
                initAccountClassDD();

                loadTableData();
                toggleBulkActions();
                $('#addSubAccount-check-all').prop('checked', false).prop('indeterminate', false);
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

            $('#SubAccountName').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#SubAccountNameError').hide();
                    $('#SubAccountName').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#SubAccountNameError').hide();
                                $('#SubAccountName').removeClass('is-invalid');
                            } else {
                                $('#SubAccountNameError').text(response.message).show();
                                $('#SubAccountName').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#SubAccountName').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region checkCodeUnique
            $('#SubAccountCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#SubAccountCodeError').hide();
                    $('#SubAccountCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#SubAccountCodeError').hide();
                                $('#SubAccountCode').removeClass('is-invalid');
                            } else {
                                $('#SubAccountCodeError').text(response.message).show();
                                $('#SubAccountCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            $('#SubAccountCode').on('keypress', function (e) {
                var value = $(this).val();
                if (value.length >= 8) {
                    e.preventDefault();
                    $('#SubAccountCodeError').text('Maximum 8 characters allowed.').show();
                    $('#SubAccountCode').addClass('is-invalid');
                }
            });

            // Clear timer when user is still typing
            $('#SubAccountCode').on('keydown', function () {
                clearTimeout(typingTimer);
                $('#SubAccountCode').removeClass('is-invalid');
            });
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#addSubAccount-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.addSubAccount-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.addSubAccount-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.addSubAccount-selectItem');
                const checkedItems = $('.addSubAccount-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#addSubAccount-check-all').prop('checked', allChecked);
                $('#addSubAccount-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#addSubAccount-bulkSelectActions').removeClass('d-none');
                    $('#addSubAccount-searchBox').addClass('d-none');
                    $('.addSubAccount-bulkDelete').addClass('disabled');
                    $('.addSubAccount-bulkEdit').addClass('disabled');
                } else {
                    $('#addSubAccount-bulkSelectActions').addClass('d-none');
                    $('#addSubAccount-searchBox').removeClass('d-none');
                    $('.addSubAccount-bulkDelete').removeClass('disabled');
                    $('.addSubAccount-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


            
        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#addSubAccount-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#addSubAccount-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#addSubAccount-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#addSubAccount-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'addSubAccountName';
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
            var searchTerm = $("#addSubAccount-searchInput").val();
            var mainAccId = $("#MainAccountID").val();

            $.ajax({
                url: getAllUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    mainAccId: mainAccId
                },
                success: function (response) {
                    var tableBody = $("#addSubAccount-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input addSubAccount-selectItem" data-id="${item.subAccountID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.className}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.mainAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.subAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.subAccountCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-2">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon me-2 addSubAccount-editBtn" id="addSubAccount-editBtn" data-id="${item.subAccountID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon addSubAccount-single-deleteBtn" id="addSubAccount-single-delete" data-id="${item.subAccountID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#addSubAccount-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#addSubAccount-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#addSubAccount-paginationLinks");
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
            $("#addSubAccount-prevPageBtn").prop('disabled', currentPage === 1);
            $("#addSubAccount-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));