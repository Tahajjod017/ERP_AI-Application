window.initCommonFields = function (root) {
    root = root || document;
    

    root.querySelectorAll('.countryField').forEach((el) => {
        if (el.dataset.select2Initialized) return;
        el.dataset.select2Initialized = true;

        let dropdownParent = $(el).closest('.modal');
        if (dropdownParent.length === 0) dropdownParent = $(document.body);

        

        $(el).select2({
            placeholder: 'Select Country',
            dropdownParent: dropdownParent,
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
        });
    });

    
};
window.initFormsForTabs = function () {
    const tabs = document.querySelectorAll('a[data-bs-toggle="tab"]');

    tabs.forEach(tab => {
        tab.addEventListener('shown.bs.tab', (e) => {
            const targetId = e.target.getAttribute('data-bs-target'); // e.g., "#customer"
            const tabContent = document.querySelector(targetId);

            if (!tabContent) return;

            // Initialize common fields (e.g., country select)
            if (window.initCommonFields) window.initCommonFields(tabContent);

            // Initialize tab-specific form
            const funcName = tabContent.dataset.init;
            if (funcName && typeof window[funcName] === 'function') {
                window[funcName](tabContent);
            }
        });
    });

    // Initialize the first active tab immediately
    const firstActiveTab = document.querySelector('.tab-pane.show.active');
    if (firstActiveTab) {
        if (window.initCommonFields) window.initCommonFields(firstActiveTab);
        const funcName = firstActiveTab.dataset.init;
        if (funcName && typeof window[funcName] === 'function') {
            window[funcName](firstActiveTab);
        }
    }
};
