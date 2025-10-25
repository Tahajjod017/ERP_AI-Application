(function ($) {
    $.addjournal = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#addJournals-form',
            updateform: '#addJournals-Updateform',
            saveBtn: '#addJournals-saveBtn',
            editBtn: '#addJournals-editBtn',
            resetBtn: '#addJournals-resetBtn',
            bulkDelBtn: '#addJournals-bulkDelBtn',
            singleDeleteBtn: '#addJournals-singleDelBtn',
        }, options);

        var getAllUrl = settings.baseUrl + "/GetAll";
        var getByIdUrl = settings.baseUrl + "/GetById";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var generateNextCodeUrl = settings.baseUrl + "/GenerateThreeDigitCodeAsync";

        $(() => {


            // #region Initialize Choices.js on load
            generateThreeDigitCodeAsync();
            initializeChoicesDropdowns();
            initMainToSubAccCascade();
            initSubToTrxAccCascade();
            // #endregion

            //$.fn.addJournalInit({
            //    addform: settings.addform,
            //    updateform: settings.updateform,
            //    saveBtn: settings.saveBtn,
            //    editBtn: settings.editBtn,
            //    resetBtn: settings.resetBtn,
            //    bulkDelBtn: settings.bulkDelBtn,
            //    singleDeleteBtn: settings.singleDeleteBtn,
            //    getAllUrl: getAllUrl,
            //    getByIdUrl: getByIdUrl,
            //    createUrl: createUrl,
            //    updateUrl: updateUrl,
            //    deleteUrl: deleteUrl,
            //    checkNameUniqueUrl: checkNameUniqueUrl,
            //    checkCodeUniqueUrl: checkCodeUniqueUrl,
            //    generateNextCodeUrl: generateNextCodeUrl
            //});


            // #region Save
            $(settings.saveBtn).on('click', async function (e) {
                e.preventDefault();

                const saveBtn = $(settings.saveBtn);
                saveBtn.prop('disabled', true).html('<i class="fa fa-spinner fa-spin"></i> Saving...');

                const token = $('#addJournals-form input[name="__RequestVerificationToken"]').val();
                const formData = {
                    __RequestVerificationToken: token,
                    JournalID: $('#JournalID').val(),
                    JournalCode: $('#JournalCode').val(),
                    JournalTypeID: $('#JournalTypeID').val(),
                    PostingRuleID: $('#PostingRuleID').val(),
                    FinancialYearID: $('#FinancialYearID').val(),
                    JournalDate: $('#JournalDate').val(),
                    Note: $('#Note').val(),
                    CreateJournalDetailsVMs: []
                };

                // Collect all CreateJournalDetailsVMs data
                $('#addJournals-form .CreateJournalDetailsVMs').each(function () {
                    const details = {
                        JournalDetailID: $(this).find('.JournalDetailID').val(),
                        MainAccountID: $(this).find('.mainAccDD').val(),
                        SubAccID: $(this).find('.subAccDD').val(),
                        TransactionAccountID: $(this).find('.trxAccDD').val(),
                        Description: $(this).find('.description').val(),
                        Debit: $(this).find('.debitAmount').val(),
                        Credit: $(this).find('.creditAmount').val(),
                    };
                    formData.CreateJournalDetailsVMs.push(details);
                });

                console.log(formData);

                const id = $(settings.addform).find('#JournalID').val();
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

                    const allFields = ["JournalTypeID", "JournalCode", "PostingRuleID", "FinancialYearID", "JournalDate", "CreateJournalDetailsVMs", "Debit", "Credit"];
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


            // #region Clear
            $(settings.resetBtn).on('click', function (e) {
                e.preventDefault();

                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#JournalID').val('0');
                resetValidation(["JournalTypeID", "JournalCode", "PostingRuleID", "FinancialYearID", "JournalDate", "CreateJournalDetailsVMs", "Debit", "Credit"]);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#addJournals-check-all").prop('checked', false);
                $('.addJournals-selectItem').prop('checked', false);

                initializeChoicesDropdowns();
                generateThreeDigitCodeAsync();
                //loadTableData();
                toggleBulkActions();
                $('#addJournals-check-all').prop('checked', false).prop('indeterminate', false);
            }
            // #endregion


            // #region Add Details and Remove Details
            $(document).on('click', '.detailsAddRowBtn', function (e) {
                e.preventDefault();

                const $tableBody = $('#journalDetails-TBody');
                const $lastRow = $tableBody.find('tr:last');
                const newIndex = $tableBody.find('tr').length; // next index

                // Clone the last row
                const $newRow = $lastRow.clone();

                // Clear inputs/selects/textareas
                $newRow.find('input, textarea, select').val('');

                // Update name/id for correct model binding
                $newRow.find('select, input, textarea').each(function () {
                    const name = $(this).attr('name');
                    const id = $(this).attr('id');

                    if (name) {
                        $(this).attr('name', name.replace(/\[\d+\]/, `[${newIndex}]`));
                    }
                    if (id) {
                        $(this).attr('id', id.replace(/\_\d+__/, `_${newIndex}__`));
                    }
                });

                // Remove old add button
                $lastRow.find('.detailsAddRowBtn').remove();

                // Append new row
                $tableBody.append($newRow);

                // Reindex after adding (ensures model binding consistency)
                reindexJournalDetails();
            });


            // DELETE ROW
            $(document).on('click', '.detailsRemoveRowBtn', function (e) {
                e.preventDefault();

                const $tableBody = $('#journalDetails-TBody');
                const $rows = $tableBody.find('tr');
                const rowCount = $rows.length;

                if (rowCount > 1) {
                    $(this).closest('tr').remove();
                    reindexJournalDetails(); // Reindex after deletion
                } else {
                    alert('At least one row is required.');
                }
            });


            // REINDEX FUNCTION (core fix)
            function reindexJournalDetails() {
                const $tableBody = $('#journalDetails-TBody');
                const $rows = $tableBody.find('tr');

                $rows.each(function (i, row) {
                    $(row).find('select, input, textarea').each(function () {
                        const name = $(this).attr('name');
                        const id = $(this).attr('id');

                        if (name) {
                            $(this).attr('name', name.replace(/\[\d+\]/, `[${i}]`));
                        }

                        if (id) {
                            $(this).attr('id', id.replace(/\_\d+__/, `_${i}__`));
                        }
                    });
                });

                // Ensure Add button exists only on last row
                $rows.find('.detailsAddRowBtn').remove();
                $rows.last().find('td:last').append(`
                    <button class="btn btn-outline-light btn-icon detailsAddRowBtn">
                        <i class="fa-regular fa-square-plus"></i>
                    </button>
                `);
            }
            // #endregion


            // #region calculateJournalTotal
            function calculateJournalTotals() {
                let totalDebit = 0;
                let totalCredit = 0;

                $('.debitAmount').each(function () {
                    const value = parseFloat($(this).val()) || 0;

                    totalDebit += value;
                });

                $('.creditAmount').each(function () {
                    const value = parseFloat($(this).val()) || 0;

                    totalCredit += value;
                });

                // Update totals
                $('#totalDebitAmount').text(totalDebit.toFixed(2));
                $('#totalCreditAmount').text(totalCredit.toFixed(2));

                // Calculate and show difference
                const diff = (totalDebit - totalCredit).toFixed(2);
                $('#creditAmountDifference')
                    .text(diff)
                    .toggleClass('text-danger', diff != 0)
                    .toggleClass('text-success', diff == 0);
            }

            // 🔁 Recalculate totals whenever debit or credit changes
            $(document).on('input', '.debitAmount, .creditAmount', calculateJournalTotals);

            // 🔁 Also recalc totals whenever a row is added or deleted
            $(document).on('click', '.detailsAddRowBtn, .detailsRemoveRowBtn', function () {
                setTimeout(calculateJournalTotals, 100); // small delay to allow DOM update
            });

            // Run once on page load
            calculateJournalTotals();
            // #endregion


            // #region Move to next input/textarea on Enter
            $(document).on('keydown', 'textarea, input.debitAmount, input.creditAmount', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault(); // prevent form submit

                    const $inputs = $('#journalDetails-TBody')
                        .find('textarea, input.debitAmount, input.creditAmount')
                        .filter(':visible'); // only visible fields

                    const index = $inputs.index(this);

                    // Move to next field if available
                    if (index > -1 && index < $inputs.length - 1) {
                        $inputs.eq(index + 1).focus();
                    } else {
                        // If it's the last field, optionally trigger Add Row button
                        const $addBtn = $('.detailsAddRowBtn');
                        if ($addBtn.length) {
                            $addBtn.trigger('click');
                            setTimeout(() => {
                                $('#journalDetails-TBody')
                                    .find('textarea, input.debitAmount, input.creditAmount')
                                    .last()
                                    .focus();
                            }, 100);
                        }
                    }
                }
            });

            //$(document).on('keydown', 'textarea, input.debitAmount, input.creditAmount', function (e) {
            //    if (e.key === 'Enter') {
            //        e.preventDefault(); // prevent form submit

            //        const $inputs = $('#journalDetails-TBody')
            //            .find('textarea, input.debitAmount, input.creditAmount')
            //            .filter(':visible:not([disabled]):not([readonly])'); // only visible & enabled fields

            //        const index = $inputs.index(this);

            //        // Move focus to the next input/textarea if available
            //        if (index > -1 && index < $inputs.length - 1) {
            //            $inputs.eq(index + 1).focus();
            //        } else {
            //            // If this is the last field in the last row → do nothing
            //            $(this).blur(); // optional: remove focus
            //        }
            //    }
            //});
            // #endregion


            // #region generateThreeDigitCodeAsync
            function generateThreeDigitCodeAsync() {

                $.ajax({
                    url: generateNextCodeUrl,
                    type: 'GET',
                    success: function (result) {
                        $('#JournalCode').val(result);
                    },
                    error: function () {
                        console.error('Error generating next code!');
                    }
                })
            }
            // #endregion


            $('#PostingRuleID').on('change', function () {
                const id = $(this).val();

                $.ajax({
                    url: '/AddJournal/GetMainAccByScenarioTypeId',
                    type: 'GET',
                    data: { scenarioTypeId : id },
                    success: function (data) {

                    },
                    error: function () {


                    }
                })
            });


            


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


            // #region Initialize Choices DD
            function initJournalTypeDD() {
                journalTypeDD = new Choices('#JournalTypeID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Journal Type...'
                });
            }
            document.addEventListener('DOMContentLoaded', initJournalTypeDD);
            initJournalTypeDD();

            function initScenarioTypeDD() {
                scenarioTypeDD = new Choices('#PostingRuleID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Scenario Type...'
                });
            }
            document.addEventListener('DOMContentLoaded', initScenarioTypeDD);
            initScenarioTypeDD();

            function initTransactionYearDD() {
                transactionYearDD = new Choices('#FinancialYearID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Transaction Year...'
                });
            }
            document.addEventListener('DOMContentLoaded', initTransactionYearDD);
            initTransactionYearDD();

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


            // #region Joournal Type on change
            $('#JournalTypeID').on('change', function () {
                var selectedValue = $(this).val();
                if (selectedValue === '2') {
                    $('#correctionJournalDiv').removeClass('d-none');
                } else {
                    $('#correctionJournalDiv').addClass('d-none');
                }
            });
            // #endregion


            // #region toggle table checkbox
            $(document).ready(function () {
                $('#addJournals-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.addJournals-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.addJournals-selectItem', function () {
                    toggleBulkActions();
                });
            });

            function toggleBulkActions() {
                const allItems = $('.addJournals-selectItem');
                const checkedItems = $('.addJournals-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#addJournals-check-all').prop('checked', allChecked);
                $('#addJournals-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#addJournals-bulkSelectActions').removeClass('d-none');
                    $('#addJournals-searchBox').addClass('d-none');
                    $('.addJournals-bulkDelete').addClass('disabled');
                    $('.addJournals-bulkEdit').addClass('disabled');
                } else {
                    $('#addJournals-bulkSelectActions').addClass('d-none');
                    $('#addJournals-searchBox').removeClass('d-none');
                    $('.addJournals-bulkDelete').removeClass('disabled');
                    $('.addJournals-bulkEdit').removeClass('disabled');
                }
            }
            // #endregion


            // #region flatpicker DatePicker
            flatpickr(".datetimepicker", {
                altInput: true,
                altFormat: "d/m/Y",
                dateFormat: "Y-m-d",
                monthSelectorType: "dropdown",
                disableMobile: true,
                allowInput: true
            });
            // #endregion

        });

    }
}(jQuery));