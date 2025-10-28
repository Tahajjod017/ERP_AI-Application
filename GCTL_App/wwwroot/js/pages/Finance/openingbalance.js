(function ($) {
    $.openingbalance = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#openingBalance-form',
            updateform: '#openingBalance-Updateform',
            saveBtn: '#openingBalance-saveBtn',
            editBtn: '#openingBalance-editBtn',
            resetBtn: '#openingBalance-resetBtn',
            bulkDelBtn: '#openingBalance-bulkDelBtn',
            singleDeleteBtn: '#openingBalance-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var generateNextCodeUrl = settings.baseUrl + "/GenerateThreeDigitCodeAsync";

        $(() => {


            // #region Save
            $(settings.saveBtn).on('click', async function (e) {
                e.preventDefault();

                const saveBtn = $(settings.saveBtn);
                saveBtn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                const token = $('#openingBalance-form input[name="__RequestVerificationToken"]').val();
                const formData = {
                    __RequestVerificationToken: token,
                    OpeningBalanceID: $('#OpeningBalanceID').val(),
                    MainAccountID: $('#MainAccountID').val(),
                    SubAccountID: $('#SubAccountID').val(),
                    TrxAccID: $('#TrxAccID').val(),
                    OpeningBalanceCode: $('#OpeningBalanceCode').val(),
                    Amount: $('#Amount').val(),
                    TrxType: $('#TrxType').val(),
                    Description: $('#Description').val(),
                };

                const id = $(settings.addform).find('#OpeningBalanceID').val();
                const url = id > 0 ? updateUrl : createUrl;
                const type = 'POST';

                try {
                    showLoadingIndicator();

                    // Await the jQuery ajax call
                    const response = await $.ajax({
                        url: url,
                        type: type,
                        data: formData,
                    });

                    const allFields = ["MainAccountID", "SubAccountID", "TrxAccID", "OpeningBalanceCode", "TrxType", "Amount"];
                    allFields.forEach(fieldId => validateField(fieldId, response));

                    if (response.isSuccess) {
                        clear();

                        toastr.success(response.message);
                    } else {
                        toastr.info(response.message);
                    }
                } catch (err) {
                    console.error(err);
                    toastr.error('An unexpected error occurred.');
                } finally {
                    hideLoadingIndicator();
                    saveBtn.prop('disabled', false).html('Save');
                }
            });
            // #endregion


            // #region Cascade MainAccount => SubAccount
            $('#MainAccountID').on('change', function () {
                const mainAccId = $(this).val();

                getSubAccByMainAccId(mainAccId);
            });

            function getSubAccByMainAccId(mainAccId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/OpeningBalance/GetSubAccByMainAccId',
                        type: 'GET',
                        data: {
                            mainAccId: mainAccId,
                        },
                        success: function (results) {
                            if (!subAccDD) return resolve();

                            const select = document.getElementById('SubAccountID');

                            // Destroy existing Choices instance
                            if (subAccDD) {
                                subAccDD.destroy();
                            }

                            // Clear old options
                            select.innerHTML = '<option value="">Select Sub Account...</option>';

                            // Group data
                            const grouped = {};
                            results.forEach(sp => {
                                const group = sp.groupName || 'No Sub Account Found';
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
                            subAccDD = new Choices(select, {
                                removeItemButton: true,
                                shouldSort: false,
                                placeholderValue: 'Select Sub Account...'
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


            // #region Cascade SubAccount => TrxAccount (async)
            $('#SubAccountID').on('change', function () {
                const subAccId = $(this).val();
                const mainAccId = $('#MainAccountID').val();

                getTrxAccByMainAccId(mainAccId, subAccId);
            });

            function getTrxAccByMainAccId(mainAccId, subAccId) {
                return new Promise((resolve, reject) => {
                    $.ajax({
                        url: '/OpeningBalance/GetTrxAccByMainAccIdSubAccId',
                        type: 'GET',
                        data: {
                            mainAccId: mainAccId,
                            subAccId: subAccId,
                        },
                        success: function (results) {
                            if (!trxAccDD) return resolve();

                            const select = document.getElementById('TrxAccID');

                            // Destroy existing Choices instance
                            if (trxAccDD) {
                                trxAccDD.destroy();
                            }

                            // Clear old options
                            select.innerHTML = '<option value="">Select Transaction Account...</option>';

                            // Group data
                            const grouped = {};
                            results.forEach(sp => {
                                const group = sp.groupName || 'No Transaction Account Found';
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
                            trxAccDD = new Choices(select, {
                                removeItemButton: true,
                                shouldSort: false,
                                placeholderValue: 'Select Transaction Account...'
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


            // #region Clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();

                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#JournalID').val('0');
                resetValidation(["MainAccountID", "SubAccountID", "TrxAccID", "OpeningBalanceCode", "Amount", "TrxType"]);
                $('.text-danger').not('.notResetDanger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });

                if (mainAccDD) {
                    mainAccDD.destroy();
                }
                initMainAccountDD();


                if (subAccDD) {
                    subAccDD.destroy();
                }
                initSubAccountDD();


                if (trxAccDD) {
                    trxAccDD.destroy();
                }
                initTrxAccountDD();


                if (trxTypeDD) {
                    trxTypeDD.destroy();
                }
                initTrxTypeDD();

                $(settings.addform).find(settings.saveBtn).text('Save');
                generateThreeDigitCodeAsync();
                loadTableData();
            }
            // #endregion


            // #region Initialize Choices DD
            function initMainAccountDD() {
                mainAccDD = new Choices('#MainAccountID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Main Account...'
                });
            }
            initMainAccountDD();


            function initSubAccountDD() {
                subAccDD = new Choices('#SubAccountID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Sub Account...'
                });
            }
            initSubAccountDD();


            function initTrxAccountDD() {
                trxAccDD = new Choices('#TrxAccID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Transaction Account...'
                });
            }
            initTrxAccountDD();


            function initTrxTypeDD() {
                trxTypeDD = new Choices('#TrxType', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Transaction Type...'
                });
            }
            initTrxTypeDD();
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#openingBalance-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.openingBalance-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.openingBalance-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.openingBalance-selectItem');
                const checkedItems = $('.openingBalance-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#openingBalance-check-all').prop('checked', allChecked);
                $('#openingBalance-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#openingBalance-bulkSelectActions').removeClass('d-none');
                    $('#openingBalance-searchBox').addClass('d-none');
                    $('.openingBalance-bulkDelete').addClass('disabled');
                    $('.openingBalance-bulkEdit').addClass('disabled');
                } else {
                    $('#openingBalance-bulkSelectActions').addClass('d-none');
                    $('#openingBalance-searchBox').removeClass('d-none');
                    $('.openingBalance-bulkDelete').removeClass('disabled');
                    $('.openingBalance-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


            // #region generateThreeDigitCodeAsync
            generateThreeDigitCodeAsync();
            function generateThreeDigitCodeAsync() {

                $.ajax({
                    url: generateNextCodeUrl,
                    type: 'GET',
                    success: function (result) {
                        $('#OpeningBalanceCode').val(result);
                    },
                    error: function () {
                        console.error('Error generating next code!');
                    }
                })
            }
            // #endregion


        });


        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#openingBalance-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#openingBalance-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#openingBalance-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#openingBalance-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'JournalID';
        let currentSortOrder = 'desc';

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
            var searchTerm = $("#openingBalance-searchInput").val();

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
                    var tableBody = $("#openingBalance-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input openingBalance-selectItem" data-id="${item.openingBalanceID}" />
                                    </td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.openingBalanceCode}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.trxAccName}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.amount}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.trxType}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.description}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">
                                        <div class="d-flex gap-2">
                                            <a href="#!" class="btn btn-outline-light btn-icon openingBalance-editBtn" id="openingBalance-editBtn" data-id="${item.openingBalanceID}">
                                                <i class="fas fa-edit text-black"></i>
                                            </a>
                                            <a href="#!" class="btn btn-outline-light btn-icon openingBalance-singleDelBtn" id="openingBalance-singleDelBtn" data-id="${item.openingBalanceID}">
                                                <i class="far fa-trash-alt text-black"></i>
                                            </a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="8" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#openingBalance-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#openingBalance-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#openingBalance-paginationLinks");
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
            $("#openingBalance-prevPageBtn").prop('disabled', currentPage === 1);
            $("#openingBalance-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion

    }
}(jQuery));