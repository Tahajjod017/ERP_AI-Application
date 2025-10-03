(function ($) {
    $.transactionaccount = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#trxAccount-form',
            updateform: '#trxAccount-Updateform',
            saveBtn: '#trxAccount-saveBtn',
            editBtn: '#trxAccount-editBtn',
            resetBtn: '#trxAccount-resetBtn',
            bulkDelBtn: '#trxAccount-bulkDelBtn',
            singleDeleteBtn: '#trxAccount-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var getGroupByClassIdUrl = settings.baseUrl + "/GetAccountGroupByClassId";
        var getMainAccByClassIdGroupIdUrl = settings.baseUrl + "/GetMainAccByClassIdGroupId";
        var getSubAccByClassIdGroupIdMainAccIdUrl = settings.baseUrl + "/GetSubAccByClassIdGroupIdMainAccId";
        var generateNextCodeUrl = settings.baseUrl + "/GenerateNextCodeAsync";

        $(() => {





            // #region Save 
            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                $(settings.saveBtn).prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                var token = $('#trxAccount-form input[name="__RequestVerificationToken"]').val();
                var formData = {
                    __RequestVerificationToken: token,
                    TrxAccID: $('#TrxAccID').val(),
                    SubAccountID: $('#SubAccountID').val(),
                    ClassID: $('#ClassID').val(),
                    GroupID: $('#GroupID').val(),
                    MainAccountID: $('#MainAccountID').val(),
                    TrxAccName: $('#TrxAccName').val(),
                    TrxAccCode: $('#TrxAccCode').val(),
                    Description: $('#Description').val(),
                }

                var id = $(settings.addform).find('#TrxAccID').val();
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
                        const allFields = ['ClassID', 'GroupID', 'MainAccountID', 'SubAccountID', 'TrxAccName', 'TrxAccCode'];

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

                $('.trxAccount-edit').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');
                try {
                    const response = await $.get(getByIdUrl, { id });
                    if (response.isSuccess == true) {
                        const data = response.data;

                        $('#trxAccount-form #TrxAccID').val(data.trxAccID);
                        $('#trxAccount-form #TrxAccCode').val(data.trxAccCode);
                        $('#trxAccount-form #TrxAccName').val(data.trxAccName);
                        $('#trxAccount-form #Description').val(data.description);
                        accountClassDD.setChoiceByValue(data.classID.toString());

                        await getAccountGroupByClassId(data.classID);  // Wait for group options to load
                        await accountGroupDD.setChoiceByValue(data.groupID.toString());  // Now set group
                        await mainAccDD.setChoiceByValue(data.mainAccountID.toString());  // Now set group

                        getSubAccByClassIdGroupIdMainAccId(data.classID, data.groupID, data.mainAccountID, data.subAccountID);

                        $('#trxAccount-form #trxAccount-saveBtn').text('Update');
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
            $("#trxAccount-delSel").on('click', function () {
                var selectedItems = $(".trxAccount-selectItem:checked");
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
                                    $("#trxAccount-check-all").prop('checked', false);
                                    $('.trxAccount-selectItem').prop('checked', false);
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

            $(document).on('click', '#trxAccount-single-delete', function () {
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
                                    $("#trxAccount-check-all").prop('checked', false);
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

                var classId = $(this).val();

                getAccountGroupByClassId(classId);
            });


            function getAccountGroupByClassId(classId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: getGroupByClassIdUrl,
                        type: 'GET',
                        data: {
                            classId: classId,
                        },
                        success: function (results) {
                            if (!accountGroupDD) return resolve();

                            const select = document.getElementById('GroupID');

                            // Destroy existing Choices instance
                            if (accountGroupDD) {
                                accountGroupDD.destroy();
                                accountGroupDD = null;
                            }

                            // Clear old options
                            select.innerHTML = '<option value="">Select Group...</option>';

                            // Group data
                            const grouped = {};
                            results.forEach(sp => {
                                const group = sp.groupName || 'No Group Found';
                                if (!grouped[group]) {
                                    grouped[group] = [];
                                }
                                grouped[group].push(sp);
                            });

                            // Build optgroups
                            for (const group in grouped) {
                                const optgroup = document.createElement('optgroup');
                                optgroup.label = group;

                                grouped[group].forEach(sp => {
                                    const option = document.createElement('option');
                                    option.value = sp.id;
                                    option.text = sp.name;
                                    optgroup.appendChild(option);
                                });

                                select.appendChild(optgroup);
                            }

                            // Re-initialize Choices.js
                            accountGroupDD = new Choices(select, {
                                removeItemButton: true,
                                shouldSort: false,
                                placeholderValue: 'Select Group...'
                            });

                            resolve();
                        },
                        error: function (xhr, status, error) {
                            console.error('Error loading data:', error);
                            reject(error);
                        }
                    });
                })
            }
            // #endregion


            // #region GetMainAccByClassIdGroupId
            $('#GroupID').on('change', function (e) {
                e.preventDefault();

                var classId = $('#ClassID').val();
                var groupId = $(this).val();

                getMainAccByClassIdGroupId(classId, groupId);
            });


            function getMainAccByClassIdGroupId(classId, groupId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: getMainAccByClassIdGroupIdUrl,
                        type: 'GET',
                        data: {
                            classId: classId,
                            groupId: groupId
                        },
                        success: function (results) {
                            if (!mainAccDD) return resolve();

                            const select = document.getElementById('MainAccountID');

                            // Destroy existing Choices instance
                            if (mainAccDD) {
                                mainAccDD.destroy();
                                mainAccDD = null;
                            }

                            // Clear old options
                            select.innerHTML = '<option value="">Select Main Account...</option>';

                            // Group data
                            const grouped = {};
                            results.forEach(sp => {
                                const group = sp.groupName || 'No Main Account Found';
                                if (!grouped[group]) {
                                    grouped[group] = [];
                                }
                                grouped[group].push(sp);
                            });

                            // Build optgroups
                            for (const group in grouped) {
                                const optgroup = document.createElement('optgroup');
                                optgroup.label = group;

                                grouped[group].forEach(sp => {
                                    const option = document.createElement('option');
                                    option.value = sp.id;
                                    option.text = sp.name;
                                    optgroup.appendChild(option);
                                });

                                select.appendChild(optgroup);
                            }

                            // Re-initialize Choices.js
                            mainAccDD = new Choices(select, {
                                removeItemButton: true,
                                shouldSort: false,
                                placeholderValue: 'Select Main Account...'
                            });

                            resolve();
                        },
                        error: function (xhr, status, error) {
                            console.error('Error loading data:', error);
                            reject(error);
                        }
                    });
                })
            }
            // #endregion


            // #region GetSubAccByClassIdGroupIdMainAccId
            $('#MainAccountID').on('change', function (e) {
                e.preventDefault();

                var classId = $('#ClassID').val();
                var groupId = $('#GroupID').val();
                var mainAccId = $(this).val();

                getSubAccByClassIdGroupIdMainAccId(classId, groupId, mainAccId);
            });

            function getSubAccByClassIdGroupIdMainAccId(classId, groupId = null, mainAccId = null, subAccountId = null, ) {
                $.ajax({
                    url: getSubAccByClassIdGroupIdMainAccIdUrl,
                    type: 'GET',
                    data: {
                        classId: classId,
                        groupId: groupId,
                        mainAccId: mainAccId
                    },
                    success: function (result) {
                        $('.chat-thread-tab').empty();

                        $('.chat-thread-tab')
                            .append(`<div class="row">
                            <div class="col-md-12">
                                <label class="form-label"><strong>Select a Sub Account</strong><span style="color:red;">*&nbsp;</span></label>
                            </div>
                            <span asp-validation-for="SubAccountID" id="SubAccountIDError" class="text-danger" style="display:none;"></span>
                        </div>`);

                        if (result.length === 0) {
                            $('.chat-thread-tab').append('<li class="nav-item text-center">No Data Found</li>');
                        } else {
                            result.forEach(function (item) {
                                var listItem = `
                                    <li class="nav-item read border-bottom" role="presentation">
                                        <a class="nav-link d-flex align-items-center justify-content-center p-2" href="#tab-thread-${item.id}" role="tab" aria-selected="false" data-subaccount-id="${item.id}">
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

                            // Click handler
                            $('.chat-thread-tab .nav-link').on('click', function () {
                                $('.chat-thread-tab .nav-link').removeClass('active');
                                $(this).addClass('active');

                                $(this).closest('li').siblings().removeClass('selected');
                                $(this).closest('li').addClass('selected');

                                var subAccountId = $(this).data('subaccount-id');
                                $('#SubAccountID').val(subAccountId);
                                generateNextCode(subAccountId);
                            });

                            // 🔽 If editing, auto-select the group
                            if (subAccountId) {
                                const target = $(`.chat-thread-tab .nav-link[data-subaccount-id="${subAccountId}"]`);
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
            function generateNextCode(subAccountId) {

                $.ajax({
                    url: generateNextCodeUrl,
                    type: 'GET',
                    data: { mainAccId: subAccountId },
                    success: function (result) {
                        $('#TrxAccCode').val(result);
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
                $('#TrxAccID').val('0');
                resetValidation(['ClassID', 'GroupID', 'MainAccountID', 'SubAccountID', 'TrxAccName', 'TrxAccCode']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#trxAccount-check-all").prop('checked', false);
                $('.trxAccount-selectItem').prop('checked', false);
                $('.chat-thread-tab').empty();

                if (accountClassDD) {
                    accountClassDD.destroy();
                }
                initAccountClassDD();

                if (accountGroupDD) {
                    accountGroupDD.destroy();
                }
                initAccountGroupDD();

                if (mainAccDD) {
                    mainAccDD.destroy();
                }
                initMainAccDD();

                loadTableData();
                toggleBulkActions();
                $('#trxAccount-check-all').prop('checked', false).prop('indeterminate', false);
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


            function initAccountGroupDD() {
                accountGroupDD = new Choices('#GroupID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Group...'
                });
            }
            document.addEventListener('DOMContentLoaded', initAccountGroupDD);
            initAccountGroupDD();


            function initMainAccDD() {
                mainAccDD = new Choices('#MainAccountID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Main Account...'
                });
            }
            document.addEventListener('DOMContentLoaded', initMainAccDD);
            initMainAccDD();
            // #endregion


            // #region checkNameUnique
            var typingTimer;
            var doneTypingInterval = 100; // Wait 500ms after user stops typing

            $('#TrxAccName').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#TrxAccNameError').hide();
                    $('#TrxAccName').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#TrxAccNameError').hide();
                                $('#TrxAccName').removeClass('is-invalid');
                            } else {
                                $('#TrxAccNameError').text(response.message).show();
                                $('#TrxAccName').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#TrxAccName').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region checkCodeUnique
            $('#TrxAccCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#TrxAccCodeError').hide();
                    $('#TrxAccCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#TrxAccCodeError').hide();
                                $('#TrxAccCode').removeClass('is-invalid');
                            } else {
                                $('#TrxAccCodeError').text(response.message).show();
                                $('#TrxAccCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            $('#TrxAccCode').on('keypress', function (e) {
                var value = $(this).val();
                if (value.length >= 12) {
                    e.preventDefault();
                    $('#TrxAccCodeError').text('Maximum 12 characters allowed.').show();
                    $('#TrxAccCode').addClass('is-invalid');
                }
            });

            // Clear timer when user is still typing
            $('#TrxAccCode').on('keydown', function () {
                clearTimeout(typingTimer);
                $('#TrxAccCode').removeClass('is-invalid');
            });
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#trxAccount-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.trxAccount-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.trxAccount-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.trxAccount-selectItem');
                const checkedItems = $('.trxAccount-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#trxAccount-check-all').prop('checked', allChecked);
                $('#trxAccount-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#trxAccount-bulkSelectActions').removeClass('d-none');
                    $('#trxAccount-searchBox').addClass('d-none');
                    $('.trxAccount-bulkDelete').addClass('disabled');
                    $('.trxAccount-bulkEdit').addClass('disabled');
                } else {
                    $('#trxAccount-bulkSelectActions').addClass('d-none');
                    $('#trxAccount-searchBox').removeClass('d-none');
                    $('.trxAccount-bulkDelete').removeClass('disabled');
                    $('.trxAccount-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion



        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#trxAccount-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#trxAccount-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#trxAccount-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#trxAccount-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'trxAccountName';
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
            var searchTerm = $("#trxAccount-searchInput").val();

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
                    var tableBody = $("#trxAccount-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input trxAccount-selectItem" data-id="${item.trxAccID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.className}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.groupName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.mainAccountName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.trxAccName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.trxAccCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-end white-space-nowrap pe-2">
                                        <div class="row g-3">
                                            <a href="#!" class="btn btn-outline-light btn-icon me-2 trxAccount-editBtn" id="trxAccount-editBtn" data-id="${item.trxAccID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="btn btn-outline-light btn-icon trxAccount-single-deleteBtn" id="trxAccount-single-delete" data-id="${item.trxAccID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="8" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#trxAccount-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#trxAccount-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#trxAccount-paginationLinks");
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
            $("#trxAccount-prevPageBtn").prop('disabled', currentPage === 1);
            $("#trxAccount-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));