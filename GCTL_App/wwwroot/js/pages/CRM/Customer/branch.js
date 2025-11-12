window.initBranchForm = function (root) {
    root = root || document;

    try {
        // Ensure global itiMap exists
        window.itiMap = window.itiMap || {};

        function initPhoneFields() {
            const phoneIds = ['#BPhone', '#bOtherPhone'];

            phoneIds.forEach(selector => {
                const input = document.querySelector(selector);
                if (!input || window.itiMap[selector]) return; // skip if already initialized

                const iti = window.intlTelInput(input, {
                    separateDialCode: true,
                    initialCountry: 'bd',
                    preferredCountries: ['bd', 'in', 'us'],
                    utilsScript: "js/utils.js" // load utils for formatting/validation
                });

                window.itiMap[selector] = iti;
            });
        }

        // ✅ Call the function once DOM is ready
        document.addEventListener("DOMContentLoaded", initPhoneFields);

    } catch (error) {
        console.error("Error initializing phone fields:", error);
    }



    initPhoneFields();


    const customerSelect = root.querySelector('#BCustomerID');

    if (customerSelect && !customerSelect.dataset.select2Initialized) {
        let dropdownParent = $(customerSelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

        $(customerSelect).select2({
            placeholder: 'Select Customer',
            dropdownParent: dropdownParent,
            ajax: {
                url: '/CreateJobs/GetCompnayCustomers',
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

    const countrySelect = root.querySelector("#BCountryID");
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
    const branchTypeSelect = root.querySelector("#BOrganizationTypeID");
    if (branchTypeSelect && !branchTypeSelect.dataset.listenerAttached) {
        branchTypeSelect.dataset.listenerAttached = true;
        let dropdownParent = $(branchTypeSelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

        $(branchTypeSelect).select2({
            placeholder: 'Select Type',
            dropdownParent: dropdownParent,
            ajax: {
                url: '/Customers/GetBranchTypesList',
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
   

    const fields = ["BCustomerID", "BName","BFirstName", "BPhone"];

    const saveBtn = root.querySelector("#bsave");
    if (saveBtn && !saveBtn.dataset.listenerAttached) {
        saveBtn.dataset.listenerAttached = true;
        if (saveBtn) {
            saveBtn.addEventListener("click", async function (e) {
                e.preventDefault();
                const form = this.closest("form");
                if (!form) return;

                debugger;

                const formData = new FormData(form);
                const jsonData = {};
                formData.forEach((value, key) => {
                    jsonData[key] = value === "" ? null : value;
                });
                // get contacts
                const contacts = [];
                document.querySelectorAll("#branch-cotact-field .bcontact-item").forEach(row => {
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

                jsonData.BContactInformations = contacts;
                console.log(jsonData);

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

            // Determine if field is a Select2 field
            const isSelect2 = field.classList.contains("select2-hidden-accessible");
            let value = field.value?.trim();

            if (!value) {
                isValid = false;

                if (isSelect2) {
                    // Apply red border to Select2 box
                    const select2Box = field.nextElementSibling?.querySelector(".select2-selection");
                    if (select2Box) {
                        select2Box.classList.add("is-invalid");
                        select2Box.style.borderColor = "red";
                    }
                } else {
                    field.classList.add("is-invalid");
                    field.style.border = "2px solid red";
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

            // Automatically remove red border when user types or selects
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

        return isValid;
    }
    // === Load existing contacts or initialize empty contact section ===
    function loadExistingContacts(contactList = []) {
        const rootHtmlDiv = $("#branch-cotact-field");
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
    <div class="row align-items-center gap-2 mx-2 mb-2 bcontact-item p-2">
        <!-- Index number -->
        <div class="col-auto text-center align-self-center fw-bold fs-6">
            ${index + 1}
        </div>

        <!-- Input fields -->
        <div class="col">
            <div class="row g-2">
                <input type="number" name="BContactInformations[${index}].Id" value="${c.id ?? 0}" hidden>

                <div class="col">
                    <label class="form-label">First Name</label>
                    <input type="text" name="BContactInformations[${index}].FirstName" class="form-control" value="${c.firstName ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Last Name</label>
                    <input type="text" name="BContactInformations[${index}].LastName" class="form-control" value="${c.lastName ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Designation</label>
                    <input type="text" name="BContactInformations[${index}].Designation" class="form-control" value="${c.designation ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Phone 1</label>
                    <input type="text" name="BContactInformations[${index}].Phone" class="form-control" value="${c.phone ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Phone 2</label>
                    <input type="text" name="BContactInformations[${index}].OtherPhone" class="form-control" value="${c.otherPhone ?? ''}">
                </div>
                <div class="col">
                    <label class="form-label">Email</label>
                    <input type="email" name="BContactInformations[${index}].Email" class="form-control" value="${c.email ?? ''}">
                </div>
            </div>
        </div>

        <!-- Delete button -->
        <div class="col-auto text-center">
            <button type="button" class="btn btn-sm btn-danger bremove-contact" title="Remove Contact"  data-contact-id="${c.id ?? 0}">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    </div>`;
    }


    // === Event Binding ===
    const contactInit = document.querySelector("#branchContact-btn");
    if (contactInit && !contactInit.dataset.listenerAttached) {
        contactInit.dataset.listenerAttached = true;

        // Add Contact
        $("#branchContact-btn").on("click", function (e) {
            e.preventDefault();
            const rootHtmlDiv = $("#branch-cotact-field");
            const index = rootHtmlDiv.children(".bcontact-item").length;
            rootHtmlDiv.append(getContactRowHtml(index));
        });

        // Delete Contact (delegated)
        $(document).on("click", ".bremove-contact", async function () {
            const item = $(this).closest(".bcontact-item").remove();
            const contactIdAttr = $(this).data("contact-id");
            const contactId = contactIdAttr ? parseInt(contactIdAttr, 10) : 0;

            if (contactId === 0) {
                item.remove();
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
                headers: {
                    "Accept": "application/json",
                },
                body: formData
            });
            const data = await response.json();
            if (data.success) {
                $item.remove();
                reIndexContacts();
                customToaster.success("Succed");
            } else {
                customToaster.error("Not succed")
            }
        });
    }
    //#endregion


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
