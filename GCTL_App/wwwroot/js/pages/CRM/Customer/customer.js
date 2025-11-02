window.itiMap = window.itiMap || {};

window.initCustomerForm = function (root) {
    root = root || document;

    function initPhoneFields() {
        const phoneIds = [
            `#Phone`, `#OtherPhone`
        ];

        phoneIds.forEach(selector => {
            const input = document.querySelector(selector);
            if (!input || window.itiMap[selector]) return; // use window.itiMap safely

            const iti = window.intlTelInput(input, {
                separateDialCode: true,
                initialCountry: 'bd',
                preferredCountries: ['bd', 'in', 'us'],
                utilsScript: "js/utils.js"
            });

            window.itiMap[selector] = iti; // store instance
        });
    }

    initPhoneFields();

    const countrySelect = root.querySelector("#CountryID");
    if (countrySelect && !countrySelect.dataset.listenerAttached) {
        countrySelect.dataset.listenerAttached = true;
        // 🔥 Smart dropdown parent: modal if inside one, else body
        let dropdownParent = $(countrySelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

        $(countrySelect).select2({
            placeholder: 'Select Country',
            dropdownParent: dropdownParent,
            ajax: {
                url: '/CreateJobs/GetCountryList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: data.results,
                        pagination: { more: data.pagination.more }
                    };
                },
                cache: true
            },
            width: '100%',
        });
    }

    const compnayTypeSelect = root.querySelector("#CompanyType");
    if (compnayTypeSelect && !compnayTypeSelect.dataset.listenerAttached) {
        compnayTypeSelect.dataset.listenerAttached = true;
        let dropdownParent = $(compnayTypeSelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

        $(compnayTypeSelect).select2({
            placeholder: 'Select Type',
            dropdownParent: dropdownParent,
            ajax: {
                url: '/CreateJobs/GetCountryList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: data.results,
                        pagination: { more: data.pagination.more }
                    };
                },
                cache: true
            },
            width: '100%',
        });
    }

    const fields = ["FirstName", "Phone"];

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
                    if (validateFields(fields)) {
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

    function validateFields(fieldIds) {
        let isValid = true;

        fieldIds.forEach(id => {
            const field = document.getElementById(id);
            if (!field) return;

            const value = field.value?.trim();

            if (!value) {
                field.classList.add("is-invalid");
                field.style.border = "2px solid red";
                isValid = false;
            } else {
                field.classList.remove("is-invalid");
                field.style.border = "";
            }

            // Automatically remove red border when user types
            if (!field.dataset.listenerAttached) {
                field.dataset.listenerAttached = true;
                field.addEventListener("input", () => {
                    if (field.value.trim()) {
                        field.classList.remove("is-invalid");
                        field.style.border = "";
                    }
                });
            }
        });

        return isValid;
    }
    function setFormValues(form, jsonData) {
        Object.keys(jsonData).forEach(key => {
            const input = form.querySelector(`[name="${key}"]`);
            if (!input) return; // skip if input not found

            const value = jsonData[key] ?? ""; // fallback to empty string if null

            // Handle different input types
            if (input.type === "checkbox") {
                input.checked = !!value;
            } else if (input.type === "radio") {
                const radio = form.querySelector(`input[name="${key}"][value="${value}"]`);
                if (radio) radio.checked = true;
            } else {
                input.value = value;
            }
        });
    }

    const contactInit = root.querySelector("#contact-btn");
    if (contactInit && !contactInit.dataset.listenerAttached) {
        contactInit.dataset.listenerAttached = true;
        let dropdownParent = $(contactInit).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }
        $("#contact-btn").on("click", function (e) {
            e.preventDefault();

            let rootHtmlDiv = $("#root-cotact-field");
            const html = `
        <div class="row gap-2 mx-2">
            <div class="col p-0 mb-2">
                <label class="form-label">Name</label>
                <input type="text" class="form-control" placeholder="">
            </div>
            <div class="col p-0 mb-2">
                <label class="form-label">Designation</label>
                <input type="text" class="form-control" placeholder="">
            </div>
            <div class="col p-0 mb-2">
                <label class="form-label">Phone 1</label>
                <input type="text" class="form-control" placeholder="">
            </div>
            <div class="col p-0 mb-2">
                <label class="form-label">Phone 2</label>
                <input type="text" class="form-control" placeholder="">
            </div>
            <div class="col p-0 mb-2">
                <label class="form-label">Email</label>
                <input type="email" class="form-control" placeholder="">
            </div>
        </div>
    `;

            rootHtmlDiv.append(html);
        });
    }
};
