window.initCustomerForm = function (root) {
    root = root || document;

    const countrySelect = root.querySelector('#CountryID');
    if (countrySelect && !countrySelect.dataset.select2Initialized) {
        $(countrySelect).select2({
            placeholder: 'Select Country',
            ajax: {
                url: '/CreateLead/GetPriorityList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: data.items.map(i => ({ id: i.value, text: i.label })),
                        pagination: { more: data.hasMore }
                    };
                },
                cache: true
            },
            width: '100%',
            language: {
                noResults: function () {
                    return $('<span>Data not found. Create a <a id="createCustomer" href="#">Customer</a></span>');
                }
            }
        });
        showDev("Hello")
        countrySelect.dataset.select2Initialized = true;
    }
};
