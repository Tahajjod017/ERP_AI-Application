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

        $(() => {


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
                };

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

                    const allFields = ['ScenarioName', 'ScenarioCode', 'MainAccountID', 'SubAccountID', 'TrxAccName', 'TrxAccCode'];
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


            // #region Initialize Choices.js on load
            generateThreeDigitCodeAsync();
            initializeChoicesDropdowns();
            initMainToSubAccCascade();
            initSubToTrxAccCascade();

            // Mirror first to second and trigger change
            syncFirstToSecondMainAccount();
            syncFirstToSecondSubAccount();
            syncFirstToSecondTrxAccount();
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


            // #region syncFirstToSecondSubAccount
            function syncFirstToSecondSubAccount() {
                const $subAccDropdowns = $('.subAccDD');

                if ($subAccDropdowns.length >= 2) {
                    const $firstSub = $subAccDropdowns.eq(0);
                    const $secondSub = $subAccDropdowns.eq(1);

                    $firstSub.on('change', function () {
                        const selectedValue = $(this).val();

                        const secondInstance = $secondSub[0].choicesInstance;
                        if (secondInstance) {
                            secondInstance.setChoiceByValue(selectedValue);

                            $secondSub.trigger('change');
                        }
                    });

                    // Optional: apply initial value
                    const selectedInitial = $firstSub.val();
                    if (selectedInitial) {
                        const secondInstance = $secondSub[0].choicesInstance;
                        if (secondInstance) {
                            secondInstance.setChoiceByValue(selectedInitial);
                            $secondSub.trigger('change');
                        }
                    }
                }
            }
            // #endregion


            // #region syncFirstToSecondTrxAccount
            function syncFirstToSecondTrxAccount() {
                const $trxAccDropdowns = $('.trxAccDD');

                if ($trxAccDropdowns.length >= 2) {
                    const $firstTrx = $trxAccDropdowns.eq(0);
                    const $secondTrx = $trxAccDropdowns.eq(1);

                    $firstTrx.on('change', function () {
                        const selectedValue = $(this).val();

                        const secondInstance = $secondTrx[0].choicesInstance;
                        if (secondInstance) {
                            secondInstance.setChoiceByValue(selectedValue);

                            $secondTrx.trigger('change');
                        }
                    });

                    // Optional: apply initial value
                    const selectedInitial = $firstTrx.val();
                    if (selectedInitial) {
                        const secondInstance = $secondTrx[0].choicesInstance;
                        if (secondInstance) {
                            secondInstance.setChoiceByValue(selectedInitial);
                            $secondTrx.trigger('change');
                        }
                    }
                }
            }
            // #endregion


            // #region Cascade MainAccount => SubAccount
            function initMainToSubAccCascade() {
                $('.mainAccDD').on('change', function () {
                    const $mainSelect = $(this);
                    const selectedMainAccId = $mainSelect.val();

                    // Find corresponding sub account dropdown
                    const $row = $mainSelect.closest('.row');
                    const $subAccSelect = $row.find('.subAccDD');

                    // Destroy existing Choices instance if any
                    if ($subAccSelect[0].choicesInstance) {
                        $subAccSelect[0].choicesInstance.destroy();
                    }

                    $subAccSelect.html('<option value="">Loading...</option>');

                    $.ajax({
                        url: '/PostingRules/GetSubAccByMainAccId',
                        method: 'GET',
                        data: { mainAccId: selectedMainAccId },
                        success: function (data) {
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
                        },
                        error: function () {
                            $subAccSelect.html('<option value="">Error loading sub accounts</option>');
                        }
                    });
                });
            }
            // #endregion


            // #region Cascade SubAccount => TrxAccount
            function initSubToTrxAccCascade() {
                $('.subAccDD').on('change', function () {
                    const $subAccSelect = $(this);
                    const $row = $subAccSelect.closest('.row');

                    // Find related mainAccDD and trxAccDD in the same row
                    const $mainSelect = $row.find('.mainAccDD');
                    const $trxAccSelect = $row.find('.trxAccDD');

                    const selectedSubAccId = $subAccSelect.val();
                    const selectedMainAccId = $mainSelect.val();

                    if (!selectedMainAccId || !selectedSubAccId) {
                        $trxAccSelect.html('<option value="">Select transaction Account...</option>');
                        return;
                    }

                    // Destroy existing Choices instance if any
                    if ($trxAccSelect[0].choicesInstance) {
                        $trxAccSelect[0].choicesInstance.destroy();
                    }

                    $trxAccSelect.html('<option value="">Loading...</option>');

                    $.ajax({
                        url: '/PostingRules/GetTrxAccByMainAccIdSubAccId',
                        method: 'GET',
                        data: {
                            mainAccId: selectedMainAccId,
                            subAccId: selectedSubAccId
                        },
                        success: function (data) {
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
                        },
                        error: function () {
                            $trxAccSelect.html('<option value="">Error loading transaction accounts</option>');
                        }
                    });
                });
            }
            // #endregion


            // #region generateThreeDigitCodeAsync
            function generateThreeDigitCodeAsync() {

                $.ajax({
                    url: '/PostingRules/GenerateThreeDigitCodeAsync',
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

        });
    };
}(jQuery));
