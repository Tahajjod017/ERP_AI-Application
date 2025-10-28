window.initBranchForm = function (root) {
    root = root || document;
    debugger
    //const countrySelect = root.querySelectorAll(".branchCustomer");
    //if (countrySelect && !countrySelect.dataset.listenerAttached) {
    //    countrySelect.dataset.listenerAttached = true;
    //    // 🔥 Smart dropdown parent: modal if inside one, else body
    //    let dropdownParent = $(countrySelect).closest('.modal');
    //    if (dropdownParent.length === 0) {
    //        dropdownParent = $(document.body);
    //    }

    //    $(countrySelect).select2({
    //        placeholder: 'Select Country',
    //        dropdownParent: dropdownParent,
    //        ajax: {
    //            url: '/CreateLead/GetPriorityList',
    //            dataType: 'json',
    //            delay: 250,
    //            data: function (params) {
    //                return { search: params.term || '', page: params.page || 1 };
    //            },
    //            processResults: function (data, params) {
    //                params.page = params.page || 1;
    //                return {
    //                    results: data.items.map(i => ({ id: i.value, text: i.label })),
    //                    pagination: { more: data.hasMore }
    //                };
    //            },
    //            cache: true
    //        },
    //        width: '100%',
    //    });
    //}\



    const initializeSelect = () => {
        $('.searchableSelect').select2({
            width: '100%',
            allowClear: true,
            placeholder: 'Select an option',
            language: { noResults: () => 'No results found' },
            escapeMarkup: markup => markup
        });
    };

    initializeSelect();


    const customerSelects = root.querySelectorAll('.branchCustomer');
    customerSelects.forEach(select => {
        if (!select.dataset.select2Initialized) {
            let dropdownParent = $(select).closest('.modal');
            if (!dropdownParent.length) dropdownParent = $(document.body);

            $(select).select2({
                placeholder: 'Select Customer',
                dropdownParent: dropdownParent,
                ajax: {
                    url: '/CreateJobs/GetCustomers',
                    dataType: 'json',
                    delay: 250,
                    data: params => ({ search: params.term || '', page: params.page || 1 }),
                    processResults: (data, params) => ({
                        results: data.results,
                        pagination: { more: data.pagination.more }
                    }),
                    cache: true
                },
                width: '100%'
            });

            select.dataset.select2Initialized = true;
        }
    });

    const saveBtn = root.querySelector("#saveAndExit");
    if (saveBtn && !saveBtn.dataset.listenerAttached) {
        saveBtn.dataset.listenerAttached = true;
        if (saveBtn) {
            saveBtn.addEventListener("click", async function () {
                const form = this.closest("form");
                if (!form) return;

                // Convert FormData → JSON
                const formData = new FormData(form);
                const jsonData = {};
                formData.forEach((value, key) => {
                    // Optional: Convert empty string to null for numbers
                    jsonData[key] = value === "" ? null : value;
                });
                console.log("Customer data (sending):", jsonData);

                try {
                    this.disabled = true;
                    this.textContent = "Saving...";

                    const response = await fetch(form.action, {
                        method: form.method || "POST",
                        headers: {
                            "Accept": "application/json",
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify(jsonData)
                    });

                    const data = await response.json();
                    console.log("Server response:", data);

                    if (response.ok && data.success) {
                        alert(data.message || "Customer saved successfully!");
                    } else {
                        alert(data.message || "Something went wrong!");
                    }
                } catch (error) {
                    console.error("Error during fetch:", error);
                    alert("Network or server error");
                } finally {
                    this.disabled = false;
                    this.textContent = "Save & Exit";
                }
            });
        }
    }
};
