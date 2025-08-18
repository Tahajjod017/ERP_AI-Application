//const { ajax } = require("jquery"); // Commented out as jQuery is assumed to be globally available

let itiMap = {};
let targetTab = 'index';

function initPhoneFields() {
    const phoneIds = [
        "#phone", "#phone1", "#phone2", "#phone3", "#phone4", "#phone5", "#phone6",
        "#phonePersonIndex", "#otherPhonePersonIndex", "#phone5Index", "#phone6Index"
    ];

    phoneIds.forEach(selector => {
        const input = document.querySelector(selector);
        if (!input) {
            console.warn(`Phone input ${selector} not found`);
            return;
        }

        // Prevent double initialization
        if (itiMap[selector]) {
            console.warn(`Phone input ${selector} already initialized`);
            return;
        }

        const iti = window.intlTelInput(input, {
            separateDialCode: true,
            initialCountry: 'bd',
            preferredCountries: ['bd', 'in', 'us'],
            utilsScript: "js/utils.js"
        });

        itiMap[selector] = iti;
    });

    console.log("Phone fields initialized:", Object.keys(itiMap));
}

// Initialize once when DOM ready
document.addEventListener("DOMContentLoaded", function () {
    initPhoneFields();
});

$(document).ready(function () {
    const list = document.getElementById("customerList");
    const list2 = document.getElementById("searchResults");
    const list3 = document.getElementById("no-results");

    if (list.children.length === 0 || list3.children.length === 0) {
        list2.style.display = "none";
    } else {
        list2.style.display = "block";
    }

    $("#submitBtn").on("click", function (e) {
        e.preventDefault();
        let anyValid = false;
        let lastValidNumber = "";

        for (const selector in itiMap) {
            const iti = itiMap[selector];
            if (iti && iti.isValidNumber()) {
                anyValid = true;
                lastValidNumber = iti.getNumber();
                break; // Stop at first valid input
            }
        }

        if (anyValid) {
            console.log("Valid phone number:", lastValidNumber);
        } else {
            console.log("Invalid phone number!");
        }
    });

    let customers = [];

    function getCustomerList() {
        $.ajax({
            url: '/CreateLead/GetCustomerList',
            method: 'GET',
            success: function (data) {
                customers = data;
                console.log("Customer List:", customers);
            },
            error: function (xhr) {
                console.error("Failed to load customer list:", xhr);
                toastr.error('Failed to load Contact Name');
            }
        });
    }

    // Initialize customer list
    getCustomerList();

    function getCustomerInfo(customerId) {
        console.log(`Fetching customer info for ID: ${customerId}`);
        $.ajax({
            url: '/CreateLead/GetCustomerInfo',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(customerId),
            success: function (response) {
                document.getElementById("customerInfoContainer").style.display = "block";

                $('#ContactNameSearch').val(response.customer.firstName + " " + response.customer.lastName);
                $('#customerID').val(response.customer.individualAddressID);
                $('#customerType').val(response.customer.addressTypeName);

                const ids = idMapIndex.person;
                document.getElementById(ids.primaryID).value = response.customer.individualAddressID;
                document.getElementById(ids.firstName).value = response.customer.firstName;
                document.getElementById(ids.lastName).value = response.customer.lastName;
                document.getElementById(ids.autocomplete).value = response.customer.fullAddress;
                document.getElementById(ids.street).value = response.customer.street;
                document.getElementById(ids.city).value = response.customer.city;
                document.getElementById(ids.additionalAddress).value = response.customer.additionaladdress;
                document.getElementById(ids.state).value = response.customer.state;
                document.getElementById(ids.postal_code).value = response.customer.postalCode;
                document.getElementById(ids.country).value = response.customer.countryName;
                document.getElementById(ids.countryCode).value = response.customer.countryCode;
                document.getElementById(ids.latitude).value = response.customer.latitude;
                document.getElementById(ids.longitude).value = response.customer.longitude;
                document.getElementById(ids.phone).value = response.customer.phone;
                document.getElementById(ids.otherPhone).value = response.customer.otherPhone;
                document.getElementById(ids.email).value = response.customer.email;

                // Re-initialize phone fields
                initPhoneFields();

                // Trigger validation
            },
            error: function (xhr) {
                console.error("Failed to load customer info:", xhr);
                toastr.error('Failed to load Contact Name');
            }
        });
    }

    function showSuggestions(query) {
        const $list = $('#customerList');
        const $noResults = $('#noResults');
        $list.empty();
        $list.hide();
        $noResults.hide();

        if (!query) return;

        const q = String(query ?? '').toLowerCase();
        const filtered = customers.filter(item =>
            String(item.fullName ?? '').toLowerCase().includes(q) ||
            String(item.phone ?? '').toLowerCase().includes(q)
        );
        if (filtered.length > 0) {
            $list.show();
            filtered.forEach(item => {
                $list.append(`<button type="button" class="list-group-item list-group-item-action customerName-item" data-id="${item.customerId}" data-type="${item.type}">${item.fullName} (${item.phone})</button>`);
            });
        } else {
            $noResults.show();
        }
    }

    $('#ContactNameSearch').on('input', function () {
        const query = $(this).val();
        $('#searchResults').show();
        $('#removeContactNameBtn').toggle(!!query);
        showSuggestions(query);
    });

    $(document).on('click', '.customerName-item', function () {
        const selected = $(this).text();
        const customerId = $(this).data("id");
        const customerType = $(this).data("type");
        console.log("Selected customer ID:", customerId);
        $('#ContactNameSearch').val(selected);
        $('#customerID').val(customerId);
        $('#customerType').val(customerType);
        getCustomerInfo(customerId);
        $('#searchResults').hide();
        $('#removeContactNameBtn').show();
    });

    $('#removeContactNameBtn').on('click', function () {
        $('#ContactNameSearch').val('');
        $('#customerList').empty();
        $('#noResults').hide();
        $('#searchResults').hide();
        $(this).hide();
    });

    $('#addNewContactNameBtn').on('click', function () {
        targetTab = 'company';
        $('#addCustomerModal').modal('show');
        // Remove active classes
        $('#person-tab').removeClass('active');
        $('#tab-Personal').removeClass('active show');
        $('#person').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#addBranchTab').removeClass('active');
        $('#addWarehouseTab').removeClass('active');
        // Add active classes
        $('#company-tab').addClass('active');
        $('#addCompanyTab').addClass('active');
        $('#tab-Company').addClass('active show');
        $('#company').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log("Target tab:", targetTab);
    });

    $("#company-tab").on("click", function () {
        targetTab = 'company';
        $('#person-tab').removeClass('active');
        $('#tab-Personal').removeClass('active show');
        $('#person').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#addBranchTab').removeClass('active');
        $('#addWarehouseTab').removeClass('active');
        $('#company-tab').addClass('active');
        $('#addCompanyTab').addClass('active');
        $('#tab-Company').addClass('active show');
        $('#company').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log("Target tab:", targetTab);
    });

    $('#addNewContactNameBtn2').on('click', function () {
        targetTab = 'person';
        $('#addCustomerModal').modal('show');
        $('#company-tab').removeClass('active');
        $('#tab-Company').removeClass('active show');
        $('#company').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#shippingAddressTab').removeClass('active');
        $('#person-tab').addClass('active');
        $('#tab-Personal').addClass('active show');
        $('#addCustommerTab').addClass('active');
        $('#person').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log("Target tab:", targetTab);
    });

    $("#person-tab").on("click", function () {
        targetTab = 'person';
        $('#company-tab').removeClass('active');
        $('#tab-Company').removeClass('active show');
        $('#company').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#shippingAddressTab').removeClass('active');
        $('#person-tab').addClass('active');
        $('#tab-Personal').addClass('active show');
        $('#addCustommerTab').addClass('active');
        $('#person').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log("Target tab:", targetTab);
    });

    $("#shippingAddressTab").on("click", function () {
        targetTab = 'shipping';
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log("Target tab:", targetTab);
    });

    $("#closeModal, #closeModal2").on('click', function () {
        $('#addCustomerModal').modal('hide');
        targetTab = "index";
        console.log("Target tab:", targetTab);
    });

    const idMap = {
        company: {
            primaryID: 'personID',
            firstName: 'firstNameCompnay',
            lastName: 'lastNameCompany',
            autocomplete: 'autocompleteCompany',
            street: 'streetCompany',
            city: 'cityCompany',
            state: 'stateCompany',
            country: 'countryCompany',
            countryCode: 'countryCodeCompany',
            postal_code: 'postalCodeCompany',
            latitude: 'latitudeCompany',
            longitude: 'longitudeCompany',
            phone: 'phone3',
            otherPhone: 'phone4',
            email: 'emailPerson'
        },
        person: {
            primaryID: 'personID',
            firstName: 'firstNamePerson',
            lastName: 'lastNamePerson',
            autocomplete: 'autocompletePerson',
            street: 'streetPerson',
            city: 'cityPerson',
            state: 'statePerson',
            additionalAddress: 'additionalAddressPerson',
            country: 'countryPerson',
            countryCode: 'countryCodePerson',
            postal_code: 'postalCodePerson',
            latitude: 'latitudePerson',
            longitude: 'longitudePerson',
            phone: 'phone3',
            otherPhone: 'phone4',
            email: 'emailPerson'
        },
        shipping: {
            primaryID: 'shipingID',
            firstName: 'firstNameShiping',
            lastName: 'lastNameShipping',
            autocomplete: 'autocompleteShiping',
            street: 'streetShiping',
            city: 'cityShiping',
            state: 'stateShiping',
            additionalAddress: 'additionalAddressShiping',
            country: 'countryShiping',
            countryCode: 'countryCodeShiping',
            postal_code: 'postalCodeShiping',
            latitude: 'latitudeShiping',
            longitude: 'longitudeShiping',
            phone: 'phone5',
            otherPhone: 'phone6',
            email: 'emailShiping'
        }
    };

    const idMapIndex = {
        indexBase: {
            customerName: 'ContactNameSearch',
            leadName: 'leadName',
            leadStatusID: 'leadStatusId',
            leadSourceID: 'leadSourceId',
            leadOwnerID: 'leadOwnerId',
            approximateDealValue: 'approximateDealValue',
            probabilityPercentage: 'probabilityPercentage',
            leadDescription: 'descriptionText'
        },
        person: {
            primaryID: 'personIndexIA_ID',
            primaryType: 'customerType',
            firstName: 'firstNamePersonIndex',
            lastName: 'lastNamePersonIndex',
            autocomplete: 'autocompletePersonIndex',
            street: 'streetPersonIndex',
            city: 'cityPersonIndex',
            state: 'statePersonIndex',
            additionalAddress: 'additionalAddressPersonIndex',
            country: 'countryPersonIndex',
            countryCode: 'countryCodePersonIndex',
            postal_code: 'postalCodePersonIndex',
            latitude: 'latitudePersonIndex',
            longitude: 'longitudePersonIndex',
            phone: 'phonePersonIndex',
            otherPhone: 'otherPhonePersonIndex',
            email: 'emailPersonIndex'
        },
        shipping: {
            primaryID: 'shippingIndexIA_ID',
            firstName: 'firstNameShippingIndex',
            lastName: 'lastNameShippingIndex',
            autocomplete: 'autocompleteShippingIndex',
            street: 'streetShippingIndex',
            city: 'cityShippingIndex',
            state: 'stateShippingIndex',
            additionalAddress: 'additionalAddressShippingIndex',
            country: 'countryShippingIndex',
            countryCode: 'countryCodeShippingIndex', // Fixed typo
            postal_code: 'postalCodeShippingIndex',
            latitude: 'latitudeShippingIndex',
            longitude: 'longitudeShippingIndex',
            phone: 'phone5Index',
            otherPhone: 'phone6Index',
            email: 'emailShippingIndex'
        }
    };

    function initAutocomplete() {
        const ids = idMap[targetTab] || {};
        const input = document.getElementById(ids.autocomplete);

        if (!input) {
            console.warn(`Autocomplete input ${ids.autocomplete} not found for tab ${targetTab}`);
            return;
        }

        const autocomplete = new google.maps.places.Autocomplete(input, {
            types: ["establishment"],
            fields: ["place_id", "name", "formatted_address", "address_components", "geometry"]
        });

        autocomplete.addListener("place_changed", () => {
            const place = autocomplete.getPlace();
            let street_number = "", city = "", state = "", country = "", postal_code = "", route = "", countryCode = "";

            for (const component of place.address_components) {
                if (component.types.includes("street_number")) street_number = component.long_name;
                if (component.types.includes("route")) route = component.long_name;
                if (component.types.includes("postal_code")) postal_code = component.long_name;
                if (component.types.includes("locality")) city = component.long_name;
                if (component.types.includes("administrative_area_level_1")) state = component.short_name;
                if (component.types.includes("country")) country = component.long_name;
                if (component.types.includes("country")) countryCode = component.short_name;
            }

            const lat = place.geometry.location.lat();
            const lng = place.geometry.location.lng();
            const fullStreet = `${street_number} ${route}`.trim();

            if (targetTab === 'company') {
                const company_name = place.name || "";
                document.getElementById("companyName").value = company_name;

                const service = new google.maps.places.PlacesService(document.createElement('div'));
                service.getDetails({
                    placeId: place.place_id,
                    fields: ['formatted_phone_number']
                }, (details, status) => {
                    if (status === google.maps.places.PlacesServiceStatus.OK) {
                        const phone = details.formatted_phone_number || '';
                        document.getElementById("phone1").value = phone;
                    }
                });
            }

            document.getElementById(ids.city).value = city;
            document.getElementById(ids.state).value = state;
            setCountry(ids.country, country);
            document.getElementById(ids.countryCode).value = countryCode;
            document.getElementById(ids.postal_code).value = postal_code;
            document.getElementById(ids.latitude).value = lat;
            document.getElementById(ids.longitude).value = lng;
            document.getElementById(ids.street).value = fullStreet;
        });
    }

    function setCountry(id, countryName) {
        $.ajax({
            url: '/CreateLead/getCountry',
            method: 'GET',
            contentType: 'application/json',
            data: { countryName: countryName },
            success: function (response) {
                console.log("Country response:", response);
                choiceManager.setChoiceValue(id, response.countryId);
            },
            error: function (xhr) {
                console.error("Failed to set country:", xhr);
                toastr.error('Error setting country');
            }
        });
    }

    function uniquenessCheck(text, type, id) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/UniquenessCheck',
                method: 'GET',
                contentType: 'application/json',
                data: { queryText: text, type: type, id: id },
                success: function (response) {
                    console.log("Uniqueness check response:", response);
                    resolve(response.unique);
                },
                error: function (xhr) {
                    console.error("Uniqueness check failed:", xhr);
                    toastr.error('Error checking uniqueness');
                    reject(xhr);
                }
            });
        });
    }

    $(document).on('click', function (e) {
        if (!$(e.target).closest('#ContactNameSearch, #searchResults').length) {
            $('#searchResults').hide();
        }
    });

    function getPhoneNumber(selector) {
        const iti = itiMap[`#${selector}`];
        if (iti && iti.isValidNumber()) {
            return iti.getNumber();
        }
        return "";
    }

    function modalValidation(item) {
        const ids = idMap[item] || {};
        if (item === "shipping") {
            return $(`#${ids.firstName}`).val().trim() !== "" && $(`#${ids.lastName}`).val().trim() !== "";
        }
        return true;
    }

    $("#modalSaveBtn").on("click", function (e) {
        e.preventDefault();
        if (fieldValidation()) {
            const actionTab = (targetTab === "person" || targetTab === "shipping") ? ["person", "shipping"] : ["company"];
            const data = [];

            actionTab.forEach(item => {
                if (modalValidation(item)) {
                    const ids = idMap[item] || {};
                    data.push({
                        TabName: item,
                        PrimaryID: document.getElementById(ids.primaryID)?.value ? parseInt(document.getElementById(ids.primaryID).value, 10) : 0,
                        FirstName: document.getElementById(ids.firstName)?.value || "",
                        LastName: document.getElementById(ids.lastName)?.value || "",
                        FullAddress: document.getElementById(ids.autocomplete)?.value || "",
                        Street: document.getElementById(ids.street)?.value || "",
                        City: document.getElementById(ids.city)?.value || "",
                        State: document.getElementById(ids.state)?.value || "",
                        Additionaladdress: document.getElementById(ids.additionalAddress)?.value || "",
                        PostalCode: document.getElementById(ids.postal_code)?.value || "",
                        CountryName: document.getElementById(ids.country)?.value || "",
                        CountryCode: document.getElementById(ids.countryCode)?.value || "",
                        Latitude: parseFloat(document.getElementById(ids.latitude)?.value) || null,
                        Longitude: parseFloat(document.getElementById(ids.longitude)?.value) || null,
                        Phone: getPhoneNumber(ids.phone),
                        OtherPhone: getPhoneNumber(ids.otherPhone),
                        Email: document.getElementById(ids.email)?.value || ""
                    });
                }
            });

            const dataToSend = { Customers: data };
            console.log("Modal save data:", dataToSend);

            $.ajax({
                url: '/CreateLead/upsertPerson',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(dataToSend),
                success: function (response) {
                    console.log("Upsert person response:", response);
                    targetTab = "index";
                    if (response.success) {
                        $('#addCustomerModal').modal('hide');
                        toastr.success(response.message);
                        getCustomerList();
                        getCustomerInfo(response.result.data);
                    } else {
                        toastr.error(response.message || "Failed to save person");
                    }
                },
                error: function (xhr) {
                    console.error("Failed to save person:", xhr);
                    toastr.error('Error saving person');
                }
            });
        }
    });

    $("#indexSaveBtn").on("click", async function (e) {
        e.preventDefault();
        if (await fieldValidation()) {
            const actionTab = ["person"];
            const data = {
                IsIndividualCustomer: document.getElementById("customerType").value === "billing",
                LeadName: document.getElementById(idMapIndex.indexBase.leadName)?.value || "",
                LeadStatusID: parseInt(document.getElementById(idMapIndex.indexBase.leadStatusID)?.value) || 0,
                LeadSourceID: parseInt(document.getElementById(idMapIndex.indexBase.leadSourceID)?.value) || 0,
                LeadOwnerID: parseInt(document.getElementById(idMapIndex.indexBase.leadOwnerID)?.value) || 0,
                ApproximateDealValue: parseFloat(document.getElementById(idMapIndex.indexBase.approximateDealValue)?.value) || 0,
                ProbabilityPercentage: parseFloat(document.getElementById(idMapIndex.indexBase.probabilityPercentage)?.value) || 0,
                LeadDescription: document.getElementById(idMapIndex.indexBase.leadDescription)?.value || "",
                Customers: []
            };

            actionTab.forEach(item => {
                const ids = idMapIndex[item] || {};
                const customerId = document.getElementById("customerID")?.value || "0";
                data.Customers.push({
                    TabName: item,
                    PrimaryID: parseInt(customerId, 10) || 0,
                    FirstName: document.getElementById(ids.firstName)?.value || "",
                    LastName: document.getElementById(ids.lastName)?.value || "",
                    FullAddress: document.getElementById(ids.autocomplete)?.value || "",
                    Street: document.getElementById(ids.street)?.value || "",
                    City: document.getElementById(ids.city)?.value || "",
                    State: document.getElementById(ids.state)?.value || "",
                    Additionaladdress: document.getElementById(ids.additionalAddress)?.value || "",
                    PostalCode: document.getElementById(ids.postal_code)?.value || "",
                    CountryName: document.getElementById(ids.country)?.value || "",
                    CountryCode: document.getElementById(ids.countryCode)?.value || "",
                    Latitude: parseFloat(document.getElementById(ids.latitude)?.value) || null,
                    Longitude: parseFloat(document.getElementById(ids.longitude)?.value) || null,
                    Phone: getPhoneNumber(ids.phone) || "",
                    OtherPhone: getPhoneNumber(ids.otherPhone) || "",
                    Email: document.getElementById(ids.email)?.value || ""
                });
            });

            console.log("Index save data:", data);

            $.ajax({
                url: '/CreateLead/CreateLead',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    console.log("Create lead response:", response);
                    if (response.success) {
                        toastr.success(response.message);
                    } else {
                        toastr.error(response.message || "Failed to create lead");
                    }
                },
                error: function (xhr) {
                    console.error("Failed to create lead:", xhr);
                    toastr.error("Error creating lead");
                }
            });
        } else {
            console.log("Validation failed");
        }
    });

    function targetListForValidation() {
        if (targetTab === 'person' || targetTab === 'shipping') {
            let ids = idMap.shipping;
            let list = [];

            if ($(`#${ids.firstName}`).val().trim() === "" && $(`#${ids.lastName}`).val().trim() === "") {
                list = [
                    idMap.person.firstName,
                    idMap.person.phone
                ];
                let removeBorderItemList = [
                    idMap.shipping.firstName,
                    idMap.shipping.phone
                ];
                removeBorderItemList.forEach(e => removeValidationOne(`#${e}`));
            } else {
                list = [
                    idMap.person.firstName,
                    idMap.person.phone,
                    idMap.shipping.firstName,
                    idMap.shipping.phone
                ];
            }
            return list;
        } else if (targetTab === "company") {
            return [
                idMap.company.firstName,
                idMap.company.phone
            ];
        } else if (targetTab === "index") {
            return [
                idMapIndex.indexBase.customerName,
                idMapIndex.indexBase.leadName,
                idMapIndex.indexBase.leadStatusID,
                idMapIndex.indexBase.leadSourceID,
                idMapIndex.indexBase.leadOwnerID,
                idMapIndex.person.firstName,
                idMapIndex.person.phone
            ].filter(Boolean);
        } else {
            return [];
        }
    }

    async function runtimeUniquenessCheck() {
        let isValid = true;
        const targetedField = targetTab === 'index'
            ? [[idMapIndex.person.phone, idMapIndex.person.otherPhone, idMapIndex.person.primaryID]]
            : targetTab === 'person'
                ? [[idMap.person.phone, idMap.person.otherPhone, idMap.person.primaryID]]
                : targetTab === 'company'
                    ? [[idMap.company.phone, idMap.company.otherPhone, idMap.company.primaryID]]
                    : [];

        for (const item of targetedField) {
            const [phoneSelector, otherPhoneSelector, idSelector] = item;
            const phone = getPhoneNumber(phoneSelector);
            const otherPhone = getPhoneNumber(otherPhoneSelector);
            const id = $(`#${idSelector}`).val() || "0";

            if (phone) {
                try {
                    const isUnique = await uniquenessCheck(phone, "phone", id);
                    const errorSpan = $(`#${phoneSelector}`).closest(".col-12").find("#errorShow");
                    if (isUnique) {
                        errorSpan.text("");
                    } else {
                        errorSpan.text("This phone number is already used");
                        $(`#${phoneSelector}`).css('border', '1px solid red');
                        isValid = false;
                    }
                } catch (error) {
                    console.error("Uniqueness check failed for phone:", error);
                    isValid = false;
                }
            }

            if (otherPhone) {
                try {
                    const isUnique = await uniquenessCheck(otherPhone, "phone", id);
                    const errorSpan = $(`#${otherPhoneSelector}`).closest(".col-12").find("#errorShow");
                    if (isUnique) {
                        errorSpan.text("");
                    } else {
                        errorSpan.text("This other phone number is already used");
                        $(`#${otherPhoneSelector}`).css('border', '1px solid red');
                        isValid = false;
                    }
                } catch (error) {
                    console.error("Uniqueness check failed for otherPhone:", error);
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    async function fieldValidation() {
        const selectedTab = targetListForValidation();
        console.log("Validating fields for tab:", targetTab, "Fields:", selectedTab);

        let isValid = await runtimeUniquenessCheck();
        console.log("Uniqueness check result:", isValid);

        let errorCount = 0;
        selectedTab.filter(Boolean).forEach(e => {
            let obj = $(`#${e}`);
            const name = obj.val().trim();
            let target;

            if (obj.closest('.choices').length > 0) {
                target = obj.closest('.choices').find('.choices__inner');
            } else {
                target = obj;
            }

            console.log(`Validating field: ${e}, Value: ${name}`);
            if (name === '') {
                target.css('border', '1px solid red');
                errorCount += 1;
                isValid = false;
            } else {
                target.css('border', '1px solid #ccc');
            }
        });

        if (errorCount > 0) {
            console.log(`Validation failed with ${errorCount} errors`);
            toastr.warning(errorCount === 1 ? "This field is required" : "These fields are required");
        } else {
            console.log("All fields validated successfully");
        }

        return isValid;
    }

    function fieldValidationOne(obj) {
        obj = $(obj);
        const name = obj.val().trim();
        let target;

        if (obj.closest('.choices').length > 0) {
            target = obj.closest('.choices').find('.choices__inner');
        } else {
            target = obj;
        }

        if (name === '') {
            target.css('border', '1px solid red');
        } else {
            target.css('border', '1px solid #ccc');
        }
    }

    function removeValidationOne(obj) {
        obj = $(obj);
        const name = obj.val().trim();
        let target;

        if (obj.closest('.choices').length > 0) {
            target = obj.closest('.choices').find('.choices__inner');
        } else {
            target = obj;
        }

        if (name === '') {
            target.css('border', '1px solid #ccc');
        }
    }

    // Runtime validation for input changes
    function runtimeValidationCheck() {
        const selectedTabList = targetListForValidation();
        selectedTabList.filter(Boolean).forEach(e => {
            $(document).on('input change', `#${e}`, function () {
                fieldValidationOne(this);
                console.log(`Validation triggered for ${e}`);
            });
        });
    }

    runtimeValidationCheck();
});