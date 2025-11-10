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
                        Phone: row.querySelector('[name*=".Phone"')?.value || '',
                        OtherPhone: row.querySelector('[name*=".OtherPhone"')?.value || '',
                        Email: row.querySelector('[name*=".Email"')?.value || ''
                    };
                    contacts.push(contact);
                });

                jsonData.ContactInformations = contacts;

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
                            toastr.success(data.message || "Customer saved successfully!");
                            resetForm(form)
                            
                        } else {
                            toastr.error(data.message || "Something went wrong!");
                        }
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
        <div class="col-auto text-center align-self-center fw-bold fs-6">
            ${index + 1}
        </div>

        <!-- Input fields -->
        <div class="col">
            <div class="row g-2">
                <input type="number" name="ContactInformations[${index}].Id" value="${c.id ?? 0}" hidden>

                <div class="col">
                    <label class="form-label">First Name</label>
                    <input type="text" name="ContactInformations[${index}].FirstName" class="form-control" value="${c.firstName ?? ''}">
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
                    <input type="text" name="ContactInformations[${index}].Phone" class="form-control" value="${c.phone ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Phone 2</label>
                    <input type="text" name="ContactInformations[${index}].OtherPhone" class="form-control" value="${c.otherPhone ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Email</label>
                    <input type="email" name="ContactInformations[${index}].Email" class="form-control" value="${c.email ?? ''}">
                </div>
            </div>
        </div>

        <!-- Delete button -->
        <div class="col-auto text-center">
            <button type="button" class="btn btn-sm btn-danger remove-contact" title="Remove Contact" class="removeBtn">
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
        $("#contact-btn").on("click", function (e) {
            e.preventDefault();
            const rootHtmlDiv = $("#root-cotact-field");
            const index = rootHtmlDiv.children(".contact-item").length;
            rootHtmlDiv.append(getContactRowHtml(index));
        });

        // Delete Contact (delegated)
        $(document).on("click", ".remove-contact", function () {
            debugger;
            $(this).closest(".contact-item").remove();
            customToaster.success("Hello");
            // Re-index remaining contacts
            $("#root-cotact-field .contact-item").each(function (i) {
                $(this).find(".fw-bold").text(i + 1);
                $(this).find("input").each(function () {
                    const nameAttr = $(this).attr("name");
                    const newName = nameAttr.replace(/\[\d+\]/, `[${i}]`);
                    $(this).attr("name", newName);
                });
            });
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


//function getAllContactData() {
//    const rootDiv = document.querySelector("#root-cotact-field");
//    const contactRows = rootDiv.querySelectorAll(".row"); // each dynamically added contact row

//    const contacts = [];

//    document.querySelectorAll("#root-cotact-field .contact-item").forEach(row => {
//        const contact = {
//            Id: row.querySelector('[name*=".Id"]')?.value || 0,
//            FirstName: row.querySelector('[name*=".FirstName"]')?.value || '',
//            LastName: row.querySelector('[name*=".LastName"]')?.value || '',
//            Designation: row.querySelector('[name*=".Designation"]')?.value || '',
//            Phone: row.querySelector('[name*=".Phone"]')?.value || '',
//            OtherPhone: row.querySelector('[name*=".OtherPhone"]')?.value || '',
//            Email: row.querySelector('[name*=".Email"]')?.value || ''
//        };
//        contacts.push(contact);
//    });
//    console.log("All contacts:", contacts);

//    // Optionally show in page
//    const outputDiv = document.getElementById("outputContacts");
//    if (outputDiv) {
//        outputDiv.textContent = JSON.stringify(contacts, null, 2);
//    }

//    return contacts;
//}
