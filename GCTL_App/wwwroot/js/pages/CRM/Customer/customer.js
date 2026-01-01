window.itiMap = window.itiMap || {};

window.initCustomerForm = function (root) {
    root = root || document;

    function initPhoneFields() {
        const phoneIds = [
            `#Phone`, `#OtherPhone`
        ];

        phoneIds.forEach(selector => {
            const input = document.querySelector(selector);
            if (!input || window.itiMap[selector]) return; 

            const iti = window.intlTelInput(input, {
                separateDialCode: true,
                initialCountry: 'bd',
                preferredCountries: ['bd', 'in', 'us'],
                utilsScript: "js/utils.js"
            });

            window.itiMap[selector] = iti; 
        });
    }

    initPhoneFields();

    const countrySelect = root.querySelector("#CountryID");
    if (countrySelect && !countrySelect.dataset.listenerAttached) {
        countrySelect.dataset.listenerAttached = true;

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


    window.showClose = function () {
        $("#close-btn").css("display", "block");
    }

    const compnayTypeSelect = root.querySelector("#OrganizationTypeID");
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
                url: '/Customers/GetOrganizationTypesList',
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
                debugger;
                // Convert FormData → JSON
                const formData = new FormData(form);
                const jsonData = {};
                formData.forEach((value, key) => {
                    // Optional: Convert empty string to null for numbers
                    jsonData[key] = value === "" ? null : value;
                });
                // Collect contacts dynamically
                const contacts = [];
                document.querySelectorAll("#root-cotact-field .contact-item").forEach(row => {
                    const contact = {
                        Id: row.querySelector('[name*=".Id"]')?.value || 0,
                        FirstName: row.querySelector('[name*=".FirstName"]')?.value || '',
                        LastName: row.querySelector('[name*=".LastName"]')?.value || '',
                        Designation: row.querySelector('[name*=".Designation"]')?.value || '',
                        Phone: row.querySelector('[name*=".Phone"]')?.value || '',
                        OtherPhone: row.querySelector('[name*=".OtherPhone"]')?.value || '',
                        Email: row.querySelector('[name*=".Email"]')?.value || ''
                    };
                    contacts.push(contact);
                });

                jsonData.ContactInformations = contacts;

                console.log("Customer data (sending):", jsonData);

                try {
                    this.disabled = true;

                    // Run unified validation
                    if (!validateCustomerForm(fields)) {
                        toastr.error("Please fill all required fields.");
                        return;
                    }
                    const response = await fetch(form.action, {
                        method: form.method || "POST",
                        headers: {
                            "Accept": "application/json",
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify(jsonData)
                    });
                    debugger;
                    showDev(response);
                    const data = await response.json();
                    console.log("Server response:", data);
                    if (response.ok && data.success) {
                        debugger;
                        const rootHtmlDiv = $("#root-cotact-field");
                        rootHtmlDiv.empty();
                        resetForm(form)
                        toastr.success(data.message || "Customer saved successfully!");
                        if (typeof loadTableData == 'function') {
                            loadTableData();
                        }
                        if (typeof closeWindow == "function") {
                            closeWindow();
                        }
                        if (typeof selectCustomer == "function") {
                            selectCustomer(data.data.id, data.data.text);
                        }
                    } else {
                        toastr.error(data.message || "Something went wrong!");
                    }
                    
                } catch (error) {
                    console.error("Error during fetch:", error);
                    toastr.error("Network or server error");
                } finally {
                    this.disabled = false;
                    this.textContent = "Save & Exit";
                }
            });
        }

    }
    function validateEmail(email) {
        if (!email) return false;
        // Simple regex for basic email validation
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email.trim());
    }
    function validateCustomerForm(fieldIds) {
        debugger;
        let isValid = true;

        // Validate main fields
        fieldIds.forEach(id => {
            const field = document.getElementById(id);
            if (!field) return;

            const isSelect2 = field.classList.contains("select2-hidden-accessible");
            let value = field.value?.trim();

            if (!value) {
                isValid = false;
                if (isSelect2) {
                    const select2Box = field.nextElementSibling?.querySelector(".select2-selection");
                    if (select2Box) {
                        select2Box.classList.add("is-invalid");
                        select2Box.style.borderColor = "red !importent";
                    }
                } else {
                    field.classList.add("is-invalid");
                    field.style.border = "2px solid red !importent";
                }
            } else {
                if (isSelect2) {
                    const select2Box = field.nextElementSibling?.querySelector(".select2-selection");
                    if (select2Box) {
                        select2Box.classList.remove("is-invalid");
                        select2Box.style.borderColor = "";
                    }
                } else {
                    field.classList.remove("is-invalid");
                    field.style.border = "";
                }
            }

            // Remove red border when user interacts
            if (!field.dataset.listenerAttached) {
                field.dataset.listenerAttached = true;
                if (isSelect2) {
                    $(field).on("change", function () {
                        const select2Box = field.nextElementSibling?.querySelector(".select2-selection");
                        if (field.value?.trim() && select2Box) {
                            select2Box.classList.remove("is-invalid");
                            select2Box.style.borderColor = "";
                        }
                    });
                } else {
                    field.addEventListener("input", () => {
                        if (field.value?.trim()) {
                            field.classList.remove("is-invalid");
                            field.style.border = "";
                        }
                    });
                }
            }
        });

        // Validate contacts
        const contactRows = document.querySelectorAll("#root-cotact-field .contact-item");
        contactRows.forEach((row, index) => {
            const firstName = row.querySelector('[name*=".FirstName"]');
            const phone = row.querySelector('[name*=".Phone"]');
            const email = row.querySelector('[name*=".Email"]');

            const phoneValid = phone?.value?.trim() && (!window.itiMap[`#${phone.id}`] || window.itiMap[`#${phone.id}`].isValidNumber());
            const emailValid = email?.value?.trim() && validateEmail(email.value);

            let rowValid = true;

            // First Name required
            if (!firstName.value?.trim()) {
                rowValid = false;
                firstName.classList.add("is-invalid");
                firstName.style.border = "2px solid red";
            } else {
                firstName.classList.remove("is-invalid");
                firstName.style.border = "";
            }

            // Either phone or email required
            if (!phoneValid && !emailValid) {
                rowValid = false;

                if (phone) {
                    phone.classList.add("is-invalid");
                    phone.style.border = "2px solid red";
                }
                if (email) {
                    email.classList.add("is-invalid");
                    email.style.border = "2px solid red";
                }
            } else {
                if (phone) {
                    phone.classList.remove("is-invalid");
                    phone.style.border = "";
                }
                if (email) {
                    email.classList.remove("is-invalid");
                    email.style.border = "";
                }
            }

            if (!rowValid) isValid = false;
        });

        return isValid;
    }



    // === Load existing contacts or initialize empty contact section ===
    function loadExistingContacts(contactList = []) {
        const rootHtmlDiv = $("#root-cotact-field");
        rootHtmlDiv.empty();

        // If no contactList given, just keep it empty (used for new customers)
        if (!Array.isArray(contactList)) contactList = [];

        contactList.forEach((c, index) => {
            rootHtmlDiv.append(getContactRowHtml(index, c));
        });
    }
    window.loadExistingContacts = loadExistingContacts;

    // === Reusable function to build each contact HTML ===
    function getContactRowHtml(index, c = {}) {
        return `
    <div class="row align-items-center gap-2 mx-2 mb-2 contact-item p-2">
        <!-- Index number -->
        <div class="col-auto text-center align-self-center fw-semibold border rounded fs-7" style="margin-top: 23px!important;">
            ${index + 1}
        </div>

        <!-- Input fields -->
        <div class="col">
            <div class="row g-2">
                <input type="number" name="ContactInformations[${index}].Id" value="${c.id ?? 0}" hidden>

                <div class="col">
                    <label class="form-label">First Name</label>
                    <input type="text" name="ContactInformations[${index}].FirstName" class="form-control required-firstname" value="${c.firstName ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Last Name</label>
                    <input type="text" name="ContactInformations[${index}].LastName" class="form-control" value="${c.lastName ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Designation</label>
                    <input type="text" name="ContactInformations[${index}].Designation" class="form-control" value="${c.designation ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Phone 1</label>
                    <input type="text" name="ContactInformations[${index}].Phone" class="form-control required-contac" value="${c.phone ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Phone 2</label>
                    <input type="text" name="ContactInformations[${index}].OtherPhone" class="form-control" value="${c.otherPhone ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Email</label>
                    <input type="email" name="ContactInformations[${index}].Email" class="form-control required-contac" value="${c.email ?? ''}">
                </div>
            </div>
        </div>

        <!-- Delete button -->
        <div class="col-auto text-center">
            <button type="button" class="btn btn-sm border remove-contact" title="Remove Contact" class="removeBtn"  data-contact-id="${c.id ?? 0}" style="margin-top: 23px!important;">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    </div>`;
    }


    // === Event Binding ===
    const contactInit = document.querySelector("#contact-btn");
    if (contactInit && !contactInit.dataset.listenerAttached) {
        contactInit.dataset.listenerAttached = true;

        // Add Contact
        $("#contact-btn").on("click",function (e) {
            e.preventDefault();
            const rootHtmlDiv = $("#root-cotact-field");
            const index = rootHtmlDiv.children(".contact-item").length;
            rootHtmlDiv.append(getContactRowHtml(index));
        });

        // Delete Contact (delegated)
        $(document).on("click", ".remove-contact", async function () {
            const contactIdAttr = $(this).attr("data-contact-id");
            const contactId = contactIdAttr ? parseInt(contactIdAttr, 10) : 0;

            const $item = $(this).closest(".contact-item");

            // If new contact (id = 0), just remove
            if (contactId === 0) {
                $item.remove();
                reIndexContacts();
                return;
            }

            const confirmed = await customToaster.confirm("Do you want to restart this Lead?");
            if (!confirmed) {
                customToaster.error("Canceled");
                return;
            }

            const formData = new FormData();
            formData.append("contactId", contactId);
            const response = await fetch("/Customers/DeleteContactPerson", {
                method: "POST",
                headers: { "Accept": "application/json" },
                body: formData
            });
            const data = await response.json();

            if (data.success) {
                $item.remove();
                reIndexContacts();
                customToaster.success("Succed");
            } else {
                customToaster.error("Not succed");
            }
        });

    }
    //#endregion


    // #region Google Maps Autocomplete
    const mapApiInit = root.querySelector("#FullAddress");
    if (mapApiInit && !mapApiInit.dataset.listenerAttached) {
        mapApiInit.dataset.listenerAttached = true;
        let dropdownParent = $(mapApiInit).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
            function initAutocomplete() {
                const input = document.getElementById("FullAddress");
                if (!input) return;

                const autocomplete = new google.maps.places.Autocomplete(input, {
                    types: ["establishment"],
                    fields: ["place_id", "name", "formatted_address", "address_components", "geometry"]
                });

                autocomplete.addListener("place_changed", () => {
                    const place = autocomplete.getPlace();
                    let street_number = "", city = "", state = "", countryName = "", postal_code = "", route = "", countryCode = "";

                    for (const component of place.address_components) {
                        if (component.types.includes("street_number")) street_number = component.long_name;
                        if (component.types.includes("route")) route = component.long_name;
                        if (component.types.includes("postal_code")) postal_code = component.long_name;
                        if (component.types.includes("locality")) city = component.long_name;
                        if (component.types.includes("administrative_area_level_1")) state = component.short_name;
                        if (component.types.includes("country")) {
                            countryName = component.long_name;
                            countryCode = component.short_name;
                        }
                    }

                    const lat = place.geometry.location.lat();
                    const lng = place.geometry.location.lng();
                    const fullStreet = `${street_number} ${route}`.trim();

                    $("#CompnayName").val(place.name || "");

                    $("#City").val(city);
                    $("#State").val(state);
                    //$("#").val(countryCode);
                    $("#PostalCode").val(postal_code);
                    $("#Latitude").val(lat);
                    $("#Longitude").val(lng);
                    $("#Street").val(fullStreet);
                });
            }
            //#endregion
            initAutocomplete()
        }
        
    }

};

//#region reset Form
function resetForm(form) {
    if (!form) return;

    form.reset();
    $(form).find("select").val(null).trigger("change");
    $(form).find("input[type=checkbox], input[type=radio]").prop("checked", false);
}
//#endregion

//#region reindexing problem
function reIndexContacts() {
    $("#root-cotact-field .contact-item").each(function (i) {
        // Update serial number
        $(this).find(".fw-bold").text(i + 1);

        // Update input names
        $(this).find("input").each(function () {
            const nameAttr = $(this).attr("name");
            const newName = nameAttr.replace(/\[\d+\]/, `[${i}]`);
            $(this).attr("name", newName);
        });
    });
}
//#endregion

window.loadCustomerData = function (id) {
    $.ajax({
        url: '/Customers/GetCustomerInfo',
        method: 'POST',
        data: {
            id: id,
        },
        success: function (response) {
            const rootHtmlDiv = $("#root-cotact-field");
            rootHtmlDiv.empty();
            const form = document.querySelector("#customerForm");
            select2ScrollingDataSet('#CountryID', response.countryID, response.countryName)
            select2ScrollingDataSet('#OrganizationTypeID', response.organizationTypeID, response.organizationTypeName)
            setFormValues(form, response);
            if (response.contactInformations?.length > 0) {
                loadExistingContacts(response.contactInformations);
            }
        },
        error: function (res) {
            showDev(res)
        }
    });
}
window.setFormValues = function (form, jsonData) {

    Object.keys(jsonData).forEach(key => {
        const capitalizedKey = key.charAt(0).toUpperCase() + key.slice(1);
        const input = form.querySelector(`[name="${capitalizedKey}"]`);
        if (!input) return;

        const value = jsonData[key] ?? "";

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
//#region select2ScrollingDataSet
window.select2ScrollingDataSet = function (fieldId, id, text) {
    const countrySelect = $(fieldId);
    if (id && text) {
        let newOption = new Option(text, id, true, true);
        countrySelect.append(newOption).trigger('change');
    } else {
        countrySelect.val('').trigger('change'); // clear if no country
    }

}
//#endregion