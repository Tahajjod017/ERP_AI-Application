window.initWarehouseForm = function (root) {
    root = root || document;

        //if (window.initCommonFields) initCommonFields(root);
    // If root is document, skip dataset check
    if (root !== document && root.dataset.warehouseInitialized) return;
    if (root !== document) root.dataset.warehouseInitialized = true;

    if (window.initCommonFields) window.initCommonFields(root);


    const customerSelect = root.querySelector('#CustomerID');
    //alert("customer")
    console.log(root)
    if (customerSelect && !customerSelect.dataset.select2Initialized) {
        debugger;
        // 🔥 Smart dropdown parent: modal if inside one, else body
        let dropdownParent = $(customerSelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

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
    }
    //#endregion
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
