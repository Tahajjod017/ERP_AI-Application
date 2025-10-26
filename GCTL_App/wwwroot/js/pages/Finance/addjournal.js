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

                    const allFields = ["JournalTypeID", "JournalCode", "PostingRuleID", "FinancialYearID", "JournalDate", "CreateJournalDetailsVMs", "TrxType", "Amount"];
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
                resetValidation(["JournalTypeID", "JournalCode", "PostingRuleID", "FinancialYearID", "JournalDate", "CreateJournalDetailsVMs", "TrxType", "Amount"]);
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');

                generateThreeDigitCodeAsync();
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
                <option value="Debit">Debit</option>
                <option value="Credit">Credit</option>
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
            <a href="#!" class="btn btn-outline-light btn-icon detailsRemoveRowBtn">
                <i class="far fa-trash-alt text-black"></i>
            </a>
            <button class="btn btn-outline-light btn-icon detailsAddRowBtn">
                <i class="fa-regular fa-square-plus text-black"></i>
            </button>
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


            // #region Edit
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
                        const debitDetail = details.find(d => d.debitCredit.toLowerCase() === 'debit');
                        const creditDetail = details.find(d => d.debitCredit.toLowerCase() === 'credit');

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


            // Load and populate Main Account dropdowns dynamically
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

            function initTransactionYearDD() {
                transactionYearDD = new Choices('#FinancialYearID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Transaction Year...'
                });
            }
            initTransactionYearDD();
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

        });

    }
}(jQuery));