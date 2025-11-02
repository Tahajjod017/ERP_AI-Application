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
        var getJournalDetailsByIdUrl = settings.baseUrl + "/GetJournalDetailsByIdAsync";

        $(() => {


            // #region Initialize Choices.js on load
            generateThreeDigitCodeAsync();
            initMainToSubAccCascade();
            initSubToTrxAccCascade();
            // #endregion


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
                        TrxType: $(this).find('.initDrCr').val(),
                        Amount: $(this).find('.amount').val(),
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

                    const allFields = ["JournalTypeID", "JournalCode", "PostingRuleID", "JournalDate", "CreateJournalDetailsVMs", "TrxType", "Amount"];
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
                resetValidation(["JournalTypeID", "JournalCode", "PostingRuleID", "JournalDate", "CreateJournalDetailsVMs", "TrxType", "Amount"]);
                $('.text-danger').not('.notResetDanger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });

                if (journalTypeDD) {
                    journalTypeDD.destroy();
                }
                initJournalTypeDD();

                if (scenarioTypeDD) {
                    scenarioTypeDD.destroy();
                }
                initScenarioTypeDD();

                $('.mainAccDD').each(function () {
                    if (this.mainAccDD) {
                        this.mainAccDD.destroy();
                    }
                });
                initMainAccDD();

                $('.subAccDD').each(function () {
                    if (this.subAccDD) {
                        this.subAccDD.destroy();
                    }
                });
                initSubAccDD();

                $('.trxAccDD').each(function () {
                    if (this.trxAccDD) {
                        this.trxAccDD.destroy();
                    }
                });
                initTrxAccDD();

                $('.initDrCr').each(function () {
                    if (this.drCrDD) {
                        this.drCrDD.destroy();
                    }
                });
                initDrCrDD();

                // ✅ Explicitly reset totals and show them visibly
                $('#totalDebitAmount').text('0.00');
                $('#totalCreditAmount').text('0.00');
                $('#creditAmountDifference')
                    .text('0.00')
                    .removeClass('text-danger')
                    .addClass('text-success') // 0.00 = balanced, so green
                    .show(); // ensure visible

                $(settings.addform).find(settings.saveBtn).text('Save');

                setTimeout(() => calculateJournalTotals(), 50);
                generateThreeDigitCodeAsync();
                loadTableData();
            }
            // #endregion


            // #region Add Details and Remove Details
            $(document).on('click', '.detailsAddRowBtn', function (e) {
                e.preventDefault();

                const $tableBody = $('#journalDetails-TBody');
                const $lastRow = $tableBody.find('tr:last');
                const newIndex = $tableBody.find('tr').length; // next index

                // 🔥 FIX: Clone the ORIGINAL row structure, not the Choices-transformed one
                const $newRow = $('<tr class="CreateJournalDetailsVMs"></tr>');

                // Build the row structure manually
                $newRow.html(`
                    <input type="hidden" class="JournalDetailID" name="CreateJournalDetailsVMs[${newIndex}].JournalDetailID" value="" />
                    <td>
                        <select class="form-select mainAccDD" name="CreateJournalDetailsVMs[${newIndex}].MainAccountID" id="CreateJournalDetailsVMs_${newIndex}__MainAccountID">
                            <option value="">Loading...</option>
                        </select>
                    </td>
                    <td>
                        <select class="form-select subAccDD" name="CreateJournalDetailsVMs[${newIndex}].SubAccountID" id="CreateJournalDetailsVMs_${newIndex}__SubAccountID">
                            <option value="">Select...</option>
                        </select>
                    </td>
                    <td>
                        <select class="form-select trxAccDD" name="CreateJournalDetailsVMs[${newIndex}].TransactionAccountID" id="CreateJournalDetailsVMs_${newIndex}__TransactionAccountID">
                            <option value="">Select...</option>
                        </select>
                    </td>
                    <td>
                        <select class="form-select initDrCr" name="CreateJournalDetailsVMs[${newIndex}].TrxType" id="CreateJournalDetailsVMs_${newIndex}__TrxType">
                            <option value="">Select...</option>
                            <option value="D">Debit</option>
                            <option value="C">Credit</option>
                        </select>
                        <span id="DebitError" class="text-danger" style="display:none;"></span>
                    </td>
                    <td>
                        <textarea class="form-control no-border-input bg-transparent description" name="CreateJournalDetailsVMs[${newIndex}].Description" id="CreateJournalDetailsVMs_${newIndex}__Description" rows="2"></textarea>
                    </td>
                    <td>
                        <input class="form-control no-border-input bg-transparent amount" name="CreateJournalDetailsVMs[${newIndex}].Amount" id="CreateJournalDetailsVMs_${newIndex}__Amount" type="text" />
                        <span id="AmountError" class="text-danger" style="display:none;"></span>
                    </td>
                    <td>
                        <div class="d-flex justify-content-end align-items-center">
                            <a href="#!" class="btn btn-outline-light btn-icon me-2 detailsRemoveRowBtn">
                                <i class="far fa-trash-alt text-black"></i>
                            </a>
                            <button class="btn btn-outline-light btn-icon detailsAddRowBtn">
                                <i class="fa-regular fa-square-plus text-black"></i>
                            </button>
                        </div>
                    </td>
                `);

                // Remove old add button from last row
                $lastRow.find('.detailsAddRowBtn').remove();

                // Append new row
                $tableBody.append($newRow);

                // 🔥 Get the select elements from the NEW row
                const $mainSelect = $newRow.find('.mainAccDD');
                const $subSelect = $newRow.find('.subAccDD');
                const $trxSelect = $newRow.find('.trxAccDD');
                const $trxTypeSelect = $newRow.find('.initDrCr');

                // 🔥 Load Main Account dropdown dynamically
                loadMainAccounts($mainSelect);

                // Initialize Sub Account
                const subAccDD = new Choices($subSelect[0], {
                    shouldSort: false,
                    searchEnabled: true,
                    itemSelectText: ''
                });
                $subSelect[0].subAccDD = subAccDD;

                // Initialize Transaction Account
                const trxAccDD = new Choices($trxSelect[0], {
                    shouldSort: false,
                    searchEnabled: true,
                    itemSelectText: ''
                });
                $trxSelect[0].trxAccDD = trxAccDD;

                // Initialize Dr/Cr
                const drCrDD = new Choices($trxTypeSelect[0], {
                    shouldSort: false,
                    searchEnabled: false,
                    itemSelectText: ''
                });
                $trxTypeSelect[0].drCrDD = drCrDD;
            });

            //$(document).on('click', '.detailsAddRowBtn', function (e) {
            //    e.preventDefault();

            //    const $tableBody = $('#journalDetails-TBody');
            //    const $lastRow = $tableBody.find('tr:last');
            //    const newIndex = $tableBody.find('tr').length; // next index

            //    // Clone the last row
            //    const $newRow = $lastRow.clone();

            //    // Clear inputs/selects/textareas
            //    $newRow.find('input, textarea, select').val('');

            //    // Update name/id for correct model binding
            //    $newRow.find('select, input, textarea').each(function () {
            //        const name = $(this).attr('name');
            //        const id = $(this).attr('id');

            //        if (name) {
            //            $(this).attr('name', name.replace(/\[\d+\]/, `[${newIndex}]`));
            //        }
            //        if (id) {
            //            $(this).attr('id', id.replace(/\_\d+__/, `_${newIndex}__`));
            //        }
            //    });

            //    // Remove old add button
            //    $lastRow.find('.detailsAddRowBtn').remove();

            //    // Append new row
            //    $tableBody.append($newRow);

            //    // Reindex after adding (ensures model binding consistency)
            //    reindexJournalDetails();
            //});


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
                        <i class="fa-regular fa-square-plus text-black"></i>
                    </button>
                `);
            }
            // #endregion


            // #region calculateJournalTotal
            function calculateJournalTotals() {
                let totalDebit = 0;
                let totalCredit = 0;

                // Loop through all rows to check the TrxType and Amount
                $('tr.CreateJournalDetailsVMs').each(function () {
                    const trxType = $(this).find('.initDrCr').val(); // Get TrxType (Debit or Credit)
                    const amount = parseFloat($(this).find('.amount').val()) || 0; // Get Amount, default to 0 if empty

                    // Check if the TrxType is Debit or Credit and update totals accordingly
                    if (trxType === 'D') {
                        totalDebit += amount;
                    } else if (trxType === 'C') {
                        totalCredit += amount;
                    }
                });

                // Update the total Debit and Credit amounts in the UI
                $('#totalDebitAmount').text(totalDebit.toFixed(2));
                $('#totalCreditAmount').text(totalCredit.toFixed(2));

                // Calculate the difference
                const diff = (totalDebit - totalCredit).toFixed(2);

                // Show the difference and apply styles (red for non-zero difference, green for zero difference)
                $('#creditAmountDifference')
                    .text(diff)
                    .toggleClass('text-danger', diff !== 0) // red if non-zero difference
                    .toggleClass('text-success', diff == 0); // green if zero difference
            }

            // Recalculate totals whenever an input value changes
            $(document).on('input', '.amount, .initDrCr', calculateJournalTotals);

            // Recalculate totals whenever a row is added or deleted
            $(document).on('click', '.detailsAddRowBtn, .detailsRemoveRowBtn', function () {
                setTimeout(calculateJournalTotals, 100); // small delay to ensure DOM update
            });

            // Run once on page load
            calculateJournalTotals();
            // #endregion


            // #region Move to next input/textarea on Enter
            $(document).on('keydown', 'textarea, input.amount', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault(); // prevent form submit

                    const $inputs = $('#journalDetails-TBody')
                        .find('textarea, input.amount')
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
                                    .find('textarea, input.amount')
                                    .last()
                                    .focus();
                            }, 100);
                        }
                    }
                }
            });
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


            // #region on change Posting Rule
            $(document).on('change', '#PostingRuleID', async function (e) {
                e.preventDefault();

                var scenarioTypeId = $(this).val();
                try {
                    const response = await $.get('/AddJournal/GetDataByPostingRuleID', { scenarioTypeId });
                    if (response.isSuccess === true) {
                        const data = response.data;

                        if (!data || !data.getByIdPostingRuleDetailsVMs || data.getByIdPostingRuleDetailsVMs.length === 0) {
                            toastr.warning('No posting rule details found.');
                            return;
                        }

                        const details = data.getByIdPostingRuleDetailsVMs;

                        // Separate Debit and Credit entries
                        const debitDetail = details.find(d => d.debitCredit.toLowerCase() === 'd');
                        const creditDetail = details.find(d => d.debitCredit.toLowerCase() === 'c');

                        // Ensure we have at least two rows in the journal table
                        const $tbody = $('#journalDetails-TBody');
                        while ($tbody.find('tr').length < 2) {
                            $('#detailsAddBtn').trigger('click');
                        }

                        const $rows = $tbody.find('tr');

                        // Helper: populate a row with data
                        async function populateRow($row, detail) {
                            if (!detail) return;

                            const $mainSelect = $row.find('.mainAccDD');
                            const $subSelect = $row.find('.subAccDD');
                            const $trxSelect = $row.find('.trxAccDD');
                            const $trxTypeSelect = $row.find('.initDrCr');

                            // Destroy existing Choices instances if any
                            if ($mainSelect[0]?.mainAccDD) $mainSelect[0].mainAccDD.destroy();
                            if ($subSelect[0]?.subAccDD) $subSelect[0].subAccDD.destroy();
                            if ($trxSelect[0]?.trxAccDD) $trxSelect[0].trxAccDD.destroy();
                            if ($trxTypeSelect[0]?.drCrDD) $trxTypeSelect[0].drCrDD.destroy();

                            // Initialize Main Account Choices
                            const mainAccDD = new Choices($mainSelect[0], {
                                shouldSort: false,
                                searchEnabled: true,
                                itemSelectText: ''
                            });
                            $mainSelect[0].mainAccDD = mainAccDD;

                            // ✅ Step 1: Set Main Account
                            mainAccDD.setChoiceByValue(detail.mainAccountID.toString());

                            // ✅ Step 2: Load Sub Account + select
                            await loadSubAccounts($mainSelect, detail.subAccID);

                            // ✅ Step 3: Load Trx Account + select
                            await loadTrxAccounts($subSelect, detail.trxAccID);

                            // ✅ Step 4: Initialize and set Debit/Credit type
                            const drCrDD = new Choices($trxTypeSelect[0], {
                                shouldSort: false,
                                searchEnabled: false,
                                itemSelectText: ''
                            });
                            $trxTypeSelect[0].drCrDD = drCrDD;
                            drCrDD.setChoiceByValue(detail.debitCredit.toString());
                        }

                        // Populate rows
                        if (debitDetail) await populateRow($rows.eq(0), debitDetail);
                        if (creditDetail) await populateRow($rows.eq(1), creditDetail);

                    } else {
                        toastr.warning(response.message || 'Failed to load posting rule details.');
                    }
                } catch (error) {
                    console.error("Error loading posting rule details:", error);
                    toastr.error('An error occurred while loading posting rule details.');
                }
            });
            // #endregion


            // #region Load and populate Main Account dropdowns dynamically
            async function loadMainAccounts($select, selectedId = null) {
                try {
                    // 1️⃣ Destroy existing Choices instance if any
                    if ($select[0]?.mainAccDD) {
                        $select[0].mainAccDD.destroy();
                        delete $select[0].mainAccDD;
                    }

                    // 2️⃣ Show temporary loading state
                    $select.html('<option value="">Loading...</option>');

                    // 3️⃣ Fetch data from backend
                    const response = await $.get('/AddJournal/GetMainAccount');
                    if (!response.isSuccess || !response.data) {
                        console.warn('Failed to load main accounts');
                        return;
                    }

                    const data = response.data;

                    // 4️⃣ Group data by GroupName (like optgroups)
                    const grouped = {};
                    data.forEach(item => {
                        if (!grouped[item.groupName]) grouped[item.groupName] = [];
                        grouped[item.groupName].push(item);
                    });

                    // 5️⃣ Build option HTML with <optgroup>
                    let optionsHtml = '<option value="">Select...</option>';
                    for (const [group, items] of Object.entries(grouped)) {
                        optionsHtml += `<optgroup label="${group}">`;
                        items.forEach(item => {
                            optionsHtml += `<option value="${item.id}">${item.name}</option>`;
                        });
                        optionsHtml += `</optgroup>`;
                    }

                    $select.html(optionsHtml);

                    // 6️⃣ Initialize Choices.js again
                    const mainAccDD = new Choices($select[0], {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    $select[0].mainAccDD = mainAccDD;

                    // 7️⃣ Pre-select a value if provided
                    if (selectedId) {
                        mainAccDD.setChoiceByValue(selectedId.toString());
                    }
                } catch (error) {
                    console.error('Error loading main accounts:', error);
                    $select.html('<option value="">Error loading main accounts</option>');
                }
            }


            $(document).ready(function () {
                $('.mainAccDD').each(function () {
                    loadMainAccounts($(this)); 
                });
            });
            // #endregion


            // #region Cascade MainAccount => SubAccount
            function initMainToSubAccCascade() {
                // Use delegated event to support dynamically added rows
                $(document).on('change', '.mainAccDD', function () {
                    const $mainSelect = $(this);
                    loadSubAccounts($mainSelect);
                });
            }

            async function loadSubAccounts($mainSelect, selectedSubAccId = null) {
                const selectedMainAccId = $mainSelect.val();
                const $row = $mainSelect.closest('tr');
                const $subAccSelect = $row.find('.subAccDD');

                // Destroy existing Choices instance for sub account if exists
                if ($subAccSelect[0]?.subAccDD) {
                    $subAccSelect[0].subAccDD.destroy();
                    delete $subAccSelect[0].subAccDD;
                }

                // Show loading text
                $subAccSelect.html('<option value="">Loading...</option>');

                if (!selectedMainAccId) {
                    $subAccSelect.html('<option value="">Select Sub Account...</option>');
                    return;
                }

                try {
                    const data = await $.ajax({
                        url: '/AddJournal/GetSubAccByMainAccId',
                        method: 'GET',
                        data: { mainAccId: selectedMainAccId }
                    });

                    // Populate sub account dropdown
                    $subAccSelect.empty().append('<option value="">Select Sub Account...</option>');
                    $.each(data, function (i, item) {
                        $subAccSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });

                    // Reinitialize Choices for this sub account dropdown
                    const subAccDD = new Choices($subAccSelect[0], {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });
                    $subAccSelect[0].subAccDD = subAccDD;

                    // If editing, pre-select the saved value
                    if (selectedSubAccId) {
                        subAccDD.setChoiceByValue(selectedSubAccId.toString());
                    }
                } catch (error) {
                    console.error('Error loading sub accounts:', error);
                    $subAccSelect.html('<option value="">Error loading sub accounts</option>');
                }
            }
            // #endregion


            // #region Cascade SubAccount => TrxAccount (async)
            function initSubToTrxAccCascade() {
                // Use delegated event so it works for dynamically added rows
                $(document).on('change', '.subAccDD', function () {
                    const $subAccSelect = $(this);
                    loadTrxAccounts($subAccSelect);
                });
            }

            async function loadTrxAccounts($subAccSelect, selectedTrxAccId = null) {
                const $row = $subAccSelect.closest('tr');
                const $mainSelect = $row.find('.mainAccDD');
                const $trxAccSelect = $row.find('.trxAccDD');

                const selectedSubAccId = $subAccSelect.val();
                const selectedMainAccId = $mainSelect.val();

                // If either not selected, reset
                if (!selectedMainAccId || !selectedSubAccId) {
                    $trxAccSelect.html('<option value="">Select Transaction Account...</option>');
                    return;
                }

                // Destroy existing Choices instance for trxAccDD if exists
                if ($trxAccSelect[0]?.trxAccDD) {
                    $trxAccSelect[0].trxAccDD.destroy();
                    delete $trxAccSelect[0].trxAccDD;
                }

                // Show loading message
                $trxAccSelect.html('<option value="">Loading...</option>');

                try {
                    const data = await $.ajax({
                        url: '/AddJournal/GetTrxAccByMainAccIdSubAccId',
                        method: 'GET',
                        data: {
                            mainAccId: selectedMainAccId,
                            subAccId: selectedSubAccId
                        }
                    });

                    // Populate dropdown
                    $trxAccSelect.empty().append('<option value="">Select Transaction Account...</option>');

                    $.each(data, function (i, item) {
                        $trxAccSelect.append($('<option>', {
                            value: item.id,
                            text: item.name
                        }));
                    });

                    // Reinitialize Choices
                    const trxAccDD = new Choices($trxAccSelect[0], {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });
                    $trxAccSelect[0].trxAccDD = trxAccDD;

                    // Pre-select if editing
                    if (selectedTrxAccId) {
                        trxAccDD.setChoiceByValue(selectedTrxAccId.toString());
                    }

                } catch (error) {
                    console.error('Error loading transaction accounts:', error);
                    $trxAccSelect.html('<option value="">Error loading transaction accounts</option>');
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
            initJournalTypeDD();


            function initMainAccDD() {
                $('.mainAccDD').each(function () {
                    const mainAccDD = new Choices(this, {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    this.mainAccDD = mainAccDD;
                });
            }
            initMainAccDD();


            function initSubAccDD() {
                $('.subAccDD').each(function () {
                    const subAccDD = new Choices(this, {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    this.subAccDD = subAccDD;
                });
            }
            initSubAccDD();


            function initTrxAccDD() {
                $('.trxAccDD').each(function () {
                    const trxAccDD = new Choices(this, {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    this.trxAccDD = trxAccDD;
                });
            }
            initTrxAccDD();


            function initDrCrDD() {
                $('.initDrCr').each(function () {
                    const drCrDD = new Choices(this, {
                        shouldSort: false,
                        searchEnabled: true,
                        itemSelectText: ''
                    });

                    this.drCrDD = drCrDD;
                });
            }
            initDrCrDD();


            function initScenarioTypeDD() {
                scenarioTypeDD = new Choices('#PostingRuleID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Scenario Type...'
                });
            }
            initScenarioTypeDD();
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


        });


        // #region loadTableData
        var currentPage = 1;
        var pageSize = 5;

        $('#addJournals-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#addJournals-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#addJournals-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#addJournals-nextPageBtn").on('click', function () {
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
            var searchTerm = $("#addJournals-searchInput").val();

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
                    var tableBody = $("#addJournals-tBody");
                    tableBody.empty();
                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            var rowIndex = (currentPage - 1) * pageSize + index + 1;
                            tableBody.append(`
                                <tr class="position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input addJournals-selectItem" data-id="${item.journalID}" />
                                    </td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.journalCode}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.journalType}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.postingRule}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.financialYear}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.journalDate}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.note}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">${item.fileLink}</td>
                                    <td class="align-middle text-start white-space-nowrap ps-0">
                                        <div class="d-flex justify-content-end align-items-center">
                                            <a href="#!" class="btn btn-outline-light btn-icon addJournals-detailsBtn" id="addJournals-detailsBtn" data-id="${item.journalID}">
                                                <i class="far fa-eye text-black"></i>
                                            </a>
                                            <a href="#!" class="btn btn-outline-light btn-icon addJournals-editBtn" id="addJournals-editBtn" data-id="${item.journalID}">
                                                <i class="fas fa-edit text-black"></i>
                                            </a>
                                            <a href="#!" class="btn btn-outline-light btn-icon addJournals-singleDelBtn" id="addJournals-singleDelBtn" data-id="${item.journalID}">
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

                    $("#addJournals-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#addJournals-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                },
                error: function () {
                    console.log("Error! Fetching all data.");
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#addJournals-paginationLinks");
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
            $("#addJournals-prevPageBtn").prop('disabled', currentPage === 1);
            $("#addJournals-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion


        // #region Show Journal Details Modal
        $(document).on('click', '.addJournals-detailsBtn', function () {
            const journalId = $(this).data('id');
            const modal = new bootstrap.Modal(document.getElementById('addJournalDetailsModal'));
            const detailsBody = $("#addJournalsDetails-tBody");

            detailsBody.html(`
                <tr>
                    <td colspan="3" class="text-center text-muted py-3">Loading...</td>
                </tr>
            `);

            modal.show();

            $.ajax({
                url: getJournalDetailsByIdUrl, 
                type: 'GET',
                data: { id: journalId },
                success: function (data) {
                    if (data.getByIdJournalDetailsVMs && data.getByIdJournalDetailsVMs.length > 0) {
                        let rows = data.getByIdJournalDetailsVMs.map(d => `
                    <tr>
                        <td class="align-middle text-start">${d.trxType || '-'}</td>
                        <td class="align-middle text-start">${parseFloat(d.amount || 0).toFixed(2)}</td>
                        <td class="align-middle text-start">${d.description || '-'}</td>
                    </tr>
                `).join('');
                        detailsBody.html(rows);
                    } else {
                        detailsBody.html(`
                    <tr>
                        <td colspan="3" class="text-center text-muted py-3">No details available</td>
                    </tr>
                `);
                    }
                },
                error: function () {
                    detailsBody.html(`
                <tr>
                    <td colspan="3" class="text-center text-danger py-3">Error loading journal details.</td>
                </tr>
            `);
                }
            });
        });
        // #endregion

    }
}(jQuery));