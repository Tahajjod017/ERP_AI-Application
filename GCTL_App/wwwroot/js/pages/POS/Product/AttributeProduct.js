
$(function () {

   
    

    /* ==============================================================
       2. ATTRIBUTE CHECKBOX SHOW / HIDE
       ============================================================== */
    $('.attr-checkbox').on('change', function () {
        const target = $(this).data('target');
        $(`#${target}`).toggle(this.checked);
    });

    /* ==============================================================
       3. DESCRIPTION WORD COUNTER
       ============================================================== */
    $('#attrDescription').on('input', function () {
        const words = $(this).val().trim().split(/\s+/).filter(w => w.length).length;
        $('#attrWordCounter').text(`${words} / 60 words`);
        $(this).toggleClass('is-invalid', words > 60);
    });

    window.clearAttrDescription = function () {
        $('#attrDescription').val('').removeClass('is-invalid');
        $('#attrWordCounter').text('0 / 60 words');
        if (typeof tinymce !== 'undefined' && tinymce.get('attrDescription')) {
            tinymce.get('attrDescription').setContent('');
        }
    };

    /* ==============================================================
       4. FORM SUBMIT – AJAX + FILES + JSON
       ============================================================== */
    $('#attributeForm').on('submit', function (e) {
        e.preventDefault();

        /* ---- 4.1 Description validation ---- */
        const descWords = $('#attrDescription').val().trim().split(/\s+/).filter(w => w.length).length;
        if (descWords > 60) {
            alert('Description cannot exceed 60 words.');
            return;
        }

        /* ---- 4.2 Collect selected attribute values ---- */
        const selectedAttrs = {};
        $('.attr-section').each(function () {
            const $sec = $(this);
            const attrName = $sec.find('h4').text().split(' (')[0].trim();


            //const values = $sec.find('input[type=checkbox]:checked').map(function () { return this.value; }).get();


            const values = $sec.find('input[type=checkbox]:checked').map(function ()
            {
                const $cb = $(this);
                    return {
                        atdId: $cb.data('id'),      // <-- AttributeID
                        name: $cb.val()             // <-- Display name
                    };
                }).get();

            if (values.length) selectedAttrs[attrName] = values;
        });

        /* ---- 4.3 Populate hidden fields (for normal binding) ---- */
        attrUploadedNames = attrFiles.map(f => f.name);
        $('#attrHiddenImageNames').val(JSON.stringify(attrUploadedNames));
        $('#attrHiddenSelectedValues').val(JSON.stringify(selectedAttrs));

        /* ---- 4.4 Build FormData ---- */
        //const formData = new FormData(this);

        const formData = new FormData(this);
      //  $.each(attrFiles, (i, file) => formData.append('AttrProductImages', file));
        formData.append('AttrSelectedValuesJson', JSON.stringify(selectedAttrs));

        // Files
        $.each(attrFiles, (i, file) => formData.append('AttrProductImages', file));

        // JSON of selected attributes (controller reads it as string)
        formData.append('AttrSelectedValuesJson', JSON.stringify(selectedAttrs));

        //const $submit = $('button[type=submit]');
        //const oldText = $submit.text();
        //$submit.prop('disabled', true).text('Saving...');

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            cache: false,
            success: function (res) {
                alert('Product saved successfully!');
                attrResetAll();               // <-- reset after success
            },
            error: function (xhr) {
                const msg = xhr.responseText || 'Server error';
                alert('Error: ' + msg);
            },
            complete: function () {
                $submit.prop('disabled', false).text(oldText);
            }
        });
    });

    /* ==============================================================
       5. RESET EVERYTHING (prefixed function)
       ============================================================== */
    window.attrResetAll = function () {
        // Reset native form
        $('form')[0].reset();

        // Reset selects
        $('select').prop('selectedIndex', 0);

        // Reset TinyMCE
        if (typeof tinymce !== 'undefined' && tinymce.get('attrDescription')) {
            tinymce.get('attrDescription').setContent('');
        }

        // Word counter
        $('#attrWordCounter').text('0 / 60 words');

        // Clear upload zone
        attrFiles = [];
        attrUploadedNames = [];
        $attrPreviewContainer.empty();
        $attrUploadComplete.removeClass('show');
        $attrUploadWrapper.removeClass('has-images');

        // Hide attribute sections
        $('.attr-checkbox').prop('checked', false);
        $('.attr-section').hide();

        // Clear hidden fields
        $('#attrHiddenImageNames').val('');
        $('#attrHiddenSelectedValues').val('');

        // Clear date pickers (flatpickr example)
        $('.datetimepicker').each(function () {
            if (this._flatpickr) this._flatpickr.clear();
        });
    };

    /* ==============================================================
       6. OPTIONAL: Add a Reset button next to Save
       ============================================================== */
    //const $resetBtn = $('<button type="button" class="btn btn-secondary ms-2">Reset All</button>');
    //$resetBtn.on('click', attrResetAll);
    //$('button[type=submit]').after($resetBtn);
});
