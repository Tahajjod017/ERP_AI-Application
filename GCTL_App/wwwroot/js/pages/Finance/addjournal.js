(function ($) {
    $.addjournal = function (options) {
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
        var deleteUrl = settings.baseUrl + "/Delete";
        var checkNameUniqueUrl = settings.baseUrl + "/CheckNameUnique";
        var checkCodeUniqueUrl = settings.baseUrl + "/CheckCodeUnique";
        var generateNextCodeUrl = settings.baseUrl + "/GenerateThreeDigitCodeAsync";

        $(() => {


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

            $('#JournalTypeID').on('change', function () {
                var selectedValue = $(this).val();
                if (selectedValue === '2') {
                    $('#correctionJournalDiv').removeClass('d-none');
                } else {
                    $('#correctionJournalDiv').addClass('d-none');
                }
            });


        });

    }
}(jQuery));