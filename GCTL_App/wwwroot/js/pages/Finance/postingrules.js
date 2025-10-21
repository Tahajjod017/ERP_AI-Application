(function ($) {
    $.postingrules = function (options) {
        // Default options
        var settings = $.extend({
            addForm: '#postingRules-form',
            tbody: '#postingRulesDetails-tBody',
            addBtn: '#add-detail',
            selectAllChk: '#postingRulesDetails-check-all'
        }, options);

        $(() => {
            // Add new detail row
            $(settings.addBtn).on('click', function () {
                const index = $(settings.tbody + ' tr').length; // next index

                const newRow = `
                    <tr>
                        <td>
                            <input name="PostingRuleDetailsVMs[${index}].SubDebitAccountID" class="form-control" />
                        </td>
                        <td>
                            <input name="PostingRuleDetailsVMs[${index}].SubCreditAccountID" class="form-control" />
                        </td>
                        <td>
                            <input name="PostingRuleDetailsVMs[${index}].TrxDebitAccountID" class="form-control" />
                        </td>
                        <td>
                            <input name="PostingRuleDetailsVMs[${index}].TrxCreditAccountID" class="form-control" />
                        </td>
                        <td class="text-center text-middle align-middle">
                            <input type="checkbox" name="PostingRuleDetailsVMs[${index}].IsActive" class="form-check-input" />
                        </td>
                        <td class="text-center">
                            <button type="button" class="btn btn-outline-light btn-icon remove-detail">
                                <i class="far fa-trash-alt text-black"></i>
                            </button>
                        </td>
                    </tr>
                `;
                $(settings.tbody).append(newRow);
            });

            // Remove individual row and reindex
            $(document).on('click', '.remove-detail', function () {
                $(this).closest('tr').remove();
                reindexRows();
            });

            // Reindex rows to maintain proper MVC model binding
            function reindexRows() {
                $(settings.tbody + ' tr').each(function (i, row) {
                    $(row).find('input, select').each(function () {
                        const name = $(this).attr('name');
                        if (name) {
                            $(this).attr('name', name.replace(/\[\d+\]/, `[${i}]`));
                        }
                    });
                });
            }

            // Select/unselect all checkboxes
            $(document).on('change', settings.selectAllChk, function () {
                const checked = $(this).is(':checked');
                $(settings.tbody + ' .row-check').prop('checked', checked);
            });
        });
    };
}(jQuery));
