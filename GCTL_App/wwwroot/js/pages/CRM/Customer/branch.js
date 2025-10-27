window.initBranchForm = function (root) {
    root = root || document;

    // If root is document, skip dataset check
    if (root !== document && root.dataset.branchInitialized) return;
    if (root !== document) root.dataset.branchInitialized = true;


    const countrySelect = root.querySelector('#customerID');
    const customerSelect = root.querySelector('#CustomerID');
    //alert("customer")

    if (customerSelect && !customerSelect.dataset.select2Initialized) {
        // 🔥 Smart dropdown parent: modal if inside one, else body
        let dropdownParent = $(customerSelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

        //#region customer serach field
        $(customerSelect).select2({
            placeholder: 'Select Customer',
            dropdownParent: dropdownParent,
            ajax: {
                url: '/CreateJobs/GetCustomers',
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

                        results: data.results,
                        pagination: {
                            more: data.pagination.more
                        }
                    };
                },
                cache: true
            },
            

            width: '100%'
        });
    }
    //#endregion

    const saveBtn = root.querySelector("#saveAndExit");
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

};
