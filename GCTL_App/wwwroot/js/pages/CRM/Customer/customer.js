if (!window.customerJSLoaded) {
    window.customerJSLoaded = true;
    $(function () {
        //#region priorityID
        $('#CountryID').select2({
            placeholder: 'Select Country',
            ajax: {
                url: '/CreateLead/GetPriorityList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return {
                        search: params.term || '',
                        page: params.page || 1
                    };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;

                    return {
                        results: data.items.map(item => ({
                            id: item.value,
                            text: item.label
                        })),
                        pagination: {
                            more: data.hasMore
                        }
                    };
                },
                cache: true
            },
            language: {
                noResults: function () {
                    return $(
                        `<span>Data not found. Create a <a id="createCustomer" href="#">Customer</a></span>`
                    );
                }
            },
            width: '100%'
        });
        //#endregion
    });
}