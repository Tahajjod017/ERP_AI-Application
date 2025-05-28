(function ($) {
    $.addshift = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            form: '#actionTaken-form',
            saveBtn: '#actionTaken-saveBtn',
            editBtn: '#actionTaken-editBtn',
            resetBtn: '#actionTaken-resetBtn',
            bulkDelBtn: '#actionTaken-bulkDelBtn',
            singleDeleteBtn: '#actionTaken-singleDelBtn',
            companyIds: '#companySelect',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + '/Create';
        var updateUrl = settings.baseUrl + '/Update';
        var deleteUrl = settings.baseUrl + '/Delete';
        var getByIdUrl = settings.baseUrl + '/GetById';
        var uniqueNameUrl = settings.baseUrl + '/CheckNameUnique';
        $(() => {



        });



    }
}(jQuery));

