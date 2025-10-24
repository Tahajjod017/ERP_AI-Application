(function ($) {
    $.postingrules = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#postingRules-form',
            updateform: '#postingRules-Updateform',
            saveBtn: '#postingRules-saveBtn',
            editBtn: '#postingRules-editBtn',
            resetBtn: '#postingRules-resetBtn',
            bulkDelBtn: '#postingRules-bulkDelBtn',
            singleDeleteBtn: '#postingRules-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/SoftDelete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var generateNextCodeUrl = settings.baseUrl + "/GenerateThreeDigitCodeAsync";

        $(() => {


            // #region Save
            $(settings.saveBtn).on('click', async function (e) {
                e.preventDefault();

                const saveBtn = $(settings.saveBtn);
                saveBtn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                const token = $('#postingRules-form input[name="__RequestVerificationToken"]').val();
                const formData = {
                    __RequestVerificationToken: token,
                    PostingRuleID: $('#PostingRuleID').val(),
                    ScenarioName: $('#ScenarioName').val(),
                    ScenarioCode: $('#ScenarioCode').val(),
                    PostingRuleDetailsVMs: []
                };

                // Collect all PostingRuleDetailsVMs data
                $('#postingRules-form .PostingRuleDetailsVM').each(function () {
                    const details = {
                        PostingRuleDetailID: $(this).find('.PostingRuleDetailID').val(),
                        MainAccountID: $(this).find('.mainAccDD').val(),
                        SubAccID: $(this).find('.subAccDD').val(),
                        TrxAccID: $(this).find('.trxAccDD').val(),
                        DebitCredit: $(this).find('.initDrCr').val()
                    };
                    formData.PostingRuleDetailsVMs.push(details);
                });

                console.log(formData);

                const id = $(settings.addform).find('#PostingRuleID').val();
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

                    const allFields = ['ScenarioName', 'ScenarioCode', 'PostingRuleDetailsVMs', 'MainAccountID', 'SubAccID', 'DebitCredit'];
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


            // #region Edit
            $(document).on('click', settings.editBtn, async function (e) {
                e.preventDefault();

                $('.postingRules-editBtn').prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i>');

                var id = $(this).data('id');
                try {
                    const response = await $.get(getByIdUrl, { id });
                    if (response.isSuccess == true) {
                        const data = response.data;

                        $('#postingRules-form #PostingRuleID').val(data.postingRuleID);
                        $('#postingRules-form #ScenarioName').val(data.scenarioName);
                        $('#postingRules-form #ScenarioCode').val(data.scenarioCode);

                        // Loop through detail rows and populate them
                        for (let index = 0; index < data.postingRuleDetailsVMs.length; index++) {
                            const detail = data.postingRuleDetailsVMs[index];
                            const $row = $('.PostingRuleDetailsVM').eq(index);

                            // 🔹 Hidden field for PostingRuleDetailID
                            $row.find('.PostingRuleDetailID').val(detail.postingRuleDetailID);

                            // MAIN ACCOUNT
                            const $main = $row.find('.mainAccDD');
                            const mainChoices = $main[0]?.choicesInstance;
                            if (mainChoices) {
                                mainChoices.setChoiceByValue(detail.mainAccountID?.toString() || '');
                            } else {
                                $main.val(detail.mainAccountID);
                            }

                            await loadSubAccounts($main, detail.subAccID);

                            const $sub = $row.find('.subAccDD');
                            await loadTrxAccounts($sub, detail.trxAccID);

                            // DEBIT / CREDIT
                            const $drCr = $row.find('.initDrCr');
                            const drCrChoices = $drCr[0]?.choicesInstance;
                            if (drCrChoices) {
                                drCrChoices.setChoiceByValue(detail.trxType || '');
                            } else {
                                $drCr.val(detail.trxType);
                            }
                        }

                        $('#postingRules-form #postingRules-saveBtn').text('Update');
                        $('.postingRules-editBtn').prop('disabled', false).html('<i class="fas fa-edit text-black"></i>');
                        window.scrollTo({ top: 0, behavior: 'smooth' });
                    } else {
                        toastr.warning(response.message);
                        $('.postingRules-editBtn').prop('disabled', false).html('<i class="fas fa-edit text-black"></i>');
                    }
                } catch (error) {
                    console.error("Edit load failed:", error);
                    $('.postingRules-editBtn').prop('disabled', false).html('<i class="fas fa-edit text-black"></i>');
                }
            })
            // #endregion


            // #region Bulk Delete
            $(document).on('click', settings.singleDeleteBtn, function (e) {
                e.preventDefault();

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
                                    clear();
                                } else {
                                    toastr.error(response.message);
                                }
                            },
                            error: function () {
                                toastr.error('An unexpected error occurred.');
                            }
                        });
                    });
                } else {
                    toastr.error('Invalid ID for deletion.');
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
                $('#PostingRuleID').val('0');
                resetValidation(['ScenarioName', 'ScenarioCode', 'PostingRuleDetailsVMs', 'MainAccountID', 'SubAccID', 'DebitCredit']);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#postingRules-check-all").prop('checked', false);
                $('.postingRules-selectItem').prop('checked', false);
                $('.postingRules-ul').empty();

                initializeChoicesDropdowns();
                generateThreeDigitCodeAsync();
                loadTableData();
                toggleBulkActions();
                $('#postingRules-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            // #region Initialize Choices.js on load
            generateThreeDigitCodeAsync();
            initializeChoicesDropdowns();
            initMainToSubAccCascade();
            initSubToTrxAccCascade();
            // #endregion


            // #region Initialize Choices
            function initializeChoicesDropdowns() {
                $('.initChoices').each(function () {
                    const choicesInstance = new Choices(this, {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    this.choicesInstance = choicesInstance;
                });

                $('.initDrCr').each(function () {
                    new Choices(this, {
                        shouldSort: false,
                        searchEnabled: false,
                        itemSelectText: ''
                    });
                });
            }
            // #endregion


            // #region Mirror First MainAccountID to Second
            function syncFirstToSecondMainAccount() {
                const $mainAccountDropdowns = $('.mainAccDD');

                if ($mainAccountDropdowns.length >= 2) {
                    const $firstMain = $mainAccountDropdowns.eq(0);
                    const $secondMain = $mainAccountDropdowns.eq(1);

                    $firstMain.on('change', function () {
                        const selectedValue = $(this).val();

                        const secondInstance = $secondMain[0].choicesInstance;
                        if (secondInstance) {
                            secondInstance.setChoiceByValue(selectedValue);

                            // Trigger change on second to cascade SubAccount
                            $secondMain.trigger('change');
                        }
                    });

                    // Optional: trigger immediately if already set
                    const selectedInitial = $firstMain.val();
                    if (selectedInitial) {
                        const secondInstance = $secondMain[0].choicesInstance;
                        if (secondInstance) {
                            secondInstance.setChoiceByValue(selectedInitial);
                            $secondMain.trigger('change');
                        }
                    }
                }
            }
            // #endregion


            // #region Cascade MainAccount => SubAccount
            function initMainToSubAccCascade() {
                $('.mainAccDD').on('change', function () {
                    const $mainSelect = $(this);

                    // Call the async loader but don’t await here, since event handler is not async.
                    // If you want, you can make event handler async as well:
                    loadSubAccounts($mainSelect);
                });
            }


            async function loadSubAccounts($mainSelect, selectedSubAccId = null) {
                const selectedMainAccId = $mainSelect.val();
                const $row = $mainSelect.closest('.row');
                const $subAccSelect = $row.find('.subAccDD');

                // Destroy existing Choices instance if any
                if ($subAccSelect[0].choicesInstance) {
                    $subAccSelect[0].choicesInstance.destroy();
                }

                $subAccSelect.html('<option value="">Loading...</option>');

                try {
                    // Use fetch or $.ajax wrapped in a Promise
                    const data = await $.ajax({
                        url: '/PostingRules/GetSubAccByMainAccId',
                        method: 'GET',
                        data: { mainAccId: selectedMainAccId }
                    });

                    $subAccSelect.empty();
                    $subAccSelect.append('<option value="">Select Sub Account...</option>');

                    $.each(data, function (i, item) {
                        $subAccSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });

                    // Reinitialize Choices
                    const instance = new Choices($subAccSelect[0], {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    $subAccSelect[0].choicesInstance = instance;

                    if (selectedSubAccId !== null) {
                        // Set the value and update Choices UI
                        instance.setChoiceByValue(selectedSubAccId.toString());
                    }

                    return Promise.resolve();
                } catch (error) {
                    $subAccSelect.html('<option value="">Error loading sub accounts</option>');
                    return Promise.reject(error);
                }
            }
            // #endregion


            // #region Cascade SubAccount => TrxAccount (async)
            function initSubToTrxAccCascade() {
                $('.subAccDD').on('change', function () {
                    const $subAccSelect = $(this);
                    // Call the async loader but don’t await here (event handler not async)
                    loadTrxAccounts($subAccSelect);
                });
            }

            async function loadTrxAccounts($subAccSelect, selectedTrxAccId = null) {
                const $row = $subAccSelect.closest('.row');
                const $mainSelect = $row.find('.mainAccDD');
                const $trxAccSelect = $row.find('.trxAccDD');

                const selectedSubAccId = $subAccSelect.val();
                const selectedMainAccId = $mainSelect.val();

                if (!selectedMainAccId || !selectedSubAccId) {
                    $trxAccSelect.html('<option value="">Select transaction Account...</option>');
                    return Promise.resolve();
                }

                // Destroy existing Choices instance if any
                if ($trxAccSelect[0].choicesInstance) {
                    $trxAccSelect[0].choicesInstance.destroy();
                }

                $trxAccSelect.html('<option value="">Loading...</option>');

                try {
                    const data = await $.ajax({
                        url: '/PostingRules/GetTrxAccByMainAccIdSubAccId',
                        method: 'GET',
                        data: {
                            mainAccId: selectedMainAccId,
                            subAccId: selectedSubAccId
                        }
                    });

                    $trxAccSelect.empty();
                    $trxAccSelect.append('<option value="">Select transaction Account...</option>');

                    $.each(data, function (i, item) {
                        $trxAccSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });

                    // Reinitialize Choices
                    const instance = new Choices($trxAccSelect[0], {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    $trxAccSelect[0].choicesInstance = instance;

                    if (selectedTrxAccId !== null) {
                        instance.setChoiceByValue(selectedTrxAccId.toString());
                    }

                    return Promise.resolve();
                } catch (error) {
                    $trxAccSelect.html('<option value="">Error loading transaction accounts</option>');
                    return Promise.reject(error);
                }
            }
            // #endregion


            // #region checkNameUnique
            var typingTimer;
            var doneTypingInterval = 100; // Wait 500ms after user stops typing

            $('#ScenarioName').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#ScenarioNameError').hide();
                    $('#ScenarioName').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkNameUniqueUrl,
                        type: 'POST',
                        data: { name: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#ScenarioNameError').hide();
                                $('#ScenarioName').removeClass('is-invalid');
                            } else {
                                $('#ScenarioNameError').text(response.message).show();
                                $('#ScenarioName').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking name uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            // Clear timer when user is still typing
            $('#ScenarioName').on('keydown', function () {
                clearTimeout(typingTimer);
            });
            // #endregion


            // #region checkCodeUnique
            $('#ScenarioCode').on('input', function () {
                clearTimeout(typingTimer);
                var value = $(this).val();

                // Clear error immediately when input is empty
                if (!value) {
                    $('#ScenarioCodeError').hide();
                    $('#ScenarioCode').removeClass('is-invalid');
                    return;
                }

                typingTimer = setTimeout(function () {
                    $.ajax({
                        url: checkCodeUniqueUrl,
                        type: 'POST',
                        data: { code: value },
                        success: function (response) {
                            if (response.isSuccess == true || response === true) {
                                $('#ScenarioCodeError').hide();
                                $('#ScenarioCode').removeClass('is-invalid');
                            } else {
                                $('#ScenarioCodeError').text(response.message).show();
                                $('#ScenarioCode').addClass('is-invalid');
                            }
                        },
                        error: function (xhr, status, error) {
                            console.log("Error occurred while checking code uniqueness: " + error);
                        }
                    });
                }, doneTypingInterval);
            });

            $('#ScenarioCode').on('keypress', function (e) {
                var value = $(this).val();
                if (value.length >= 12) {
                    e.preventDefault();
                    $('#ScenarioCodeError').text('Maximum 12 characters allowed.').show();
                    $('#ScenarioCode').addClass('is-invalid');
                }
            });

            // Clear timer when user is still typing
            $('#ScenarioCode').on('keydown', function () {
                clearTimeout(typingTimer);
                $('#ScenarioCode').removeClass('is-invalid');
            });
            // #endregion


            // #region generateThreeDigitCodeAsync
            function generateThreeDigitCodeAsync() {

                $.ajax({
                    url: generateNextCodeUrl,
                    type: 'GET',
                    success: function (result) {
                        $('#ScenarioCode').val(result);
                    },
                    error: function () {
                        console.error('Error generating next code!');
                    }
                })
            }
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#postingRules-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.postingRules-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.postingRules-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.postingRules-selectItem');
                const checkedItems = $('.postingRules-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#postingRules-check-all').prop('checked', allChecked);
                $('#postingRules-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#postingRules-bulkSelectActions').removeClass('d-none');
                    $('#postingRules-searchBox').addClass('d-none');
                    $('.postingRules-bulkDelete').addClass('disabled');
                    $('.postingRules-bulkEdit').addClass('disabled');
                } else {
                    $('#postingRules-bulkSelectActions').addClass('d-none');
                    $('#postingRules-searchBox').removeClass('d-none');
                    $('.postingRules-bulkDelete').removeClass('disabled');
                    $('.postingRules-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


        });



        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#postingRules-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#postingRules-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#postingRules-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#postingRules-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'postingRulesName';
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
            var searchTerm = $("#postingRules-searchInput").val();
            var subAccId = $("#SubAccountID").val();

            $.ajax({
                url: getAllUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                    subAccId: subAccId
                },
                success: function (response) {
                    var tableBody = $("#postingRules-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input postingRules-selectItem" data-id="${item.postingRuleID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.scenarioName}</td>
                                    <td class="align-middle white-space-nowrap ps-0">${item.scenarioCode}</td>
                                    <td class="align-middle white-space-nowrap ps-0">
                                        <div class="d-flex gap-2">
                                            <a href="#!" class="btn btn-outline-light btn-icon postingRules-editBtn" id="postingRules-editBtn" data-id="${item.postingRuleID}">
                                                <i class="fas fa-edit text-black"></i>
                                            </a>
                                            <a href="#!" class="btn btn-outline-light btn-icon postingRules-singleDelBtn" id="postingRules-singleDelBtn" data-id="${item.postingRuleID}">
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

                    $("#postingRules-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#postingRules-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#postingRules-paginationLinks");
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
            $("#postingRules-prevPageBtn").prop('disabled', currentPage === 1);
            $("#postingRules-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    };
}(jQuery));
