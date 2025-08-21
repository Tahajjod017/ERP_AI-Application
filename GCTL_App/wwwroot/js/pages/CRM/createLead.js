//const { ajax } = require("jquery"); // Commented out as jQuery is assumed to be globally available

let itiMap = {};
let targetTab = 'index';
const idMap = {
    company: {
        primaryID: 'personID',
        latitude: 'latitudeCompany',
        longitude: 'longitudeCompany',
        firstName: 'firstNameCompnay',
        lastName: 'lastNameCompany',
        fullAddress: 'autocompleteCompany',
        street: 'streetCompany',
        city: 'cityCompany',
        state: 'stateCompany',
        countryName: 'countryCompany',
        countryCode: 'countryCodeCompany',
        postalCode: 'postalCodeCompany',
        phone: 'phone3',
        otherPhone: 'phone4',
        email: 'emailPerson'
    },
    person: {
        primaryID: 'personID',
        latitude: 'latitudePerson',
        longitude: 'longitudePerson',
        firstName: 'firstNamePerson',
        lastName: 'lastNamePerson',
        fullAddress: 'autocompletePerson',
        street: 'streetPerson',
        city: 'cityPerson',
        state: 'statePerson',
        additionalAddress: 'additionalAddressPerson',
        countryName: 'countryPerson',
        countryCode: 'countryCodePerson',
        postalCode: 'postalCodePerson',
        phone: 'phone3',
        otherPhone: 'phone4',
        email: 'emailPerson'
    },
    shipping: {
        primaryID: 'shipingID',
        latitude: 'latitudeShiping',
        longitude: 'longitudeShiping',
        firstName: 'firstNameShiping',
        lastName: 'lastNameShipping',
        fullAddress: 'autocompleteShiping',
        street: 'streetShiping',
        city: 'cityShiping',
        state: 'stateShiping',
        additionalAddress: 'additionalAddressShiping',
        countryName: 'countryShiping',
        countryCode: 'countryCodeShiping',
        postalCode: 'postalCodeShiping',
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
        latitude: 'latitudePersonIndex',
        longitude: 'longitudePersonIndex',
        firstName: 'firstNamePersonIndex',
        lastName: 'lastNamePersonIndex',
        fullAddress: 'autocompletePersonIndex',
        street: 'streetPersonIndex',
        city: 'cityPersonIndex',
        state: 'statePersonIndex',
        additionalAddress: 'additionalAddressPersonIndex',
        countryName: 'countryPersonIndex',
        countryCode: 'countryCodePersonIndex',
        postalCode: 'postalCodePersonIndex',
        phone: 'phonePersonIndex',
        otherPhone: 'otherPhonePersonIndex',
        email: 'emailPersonIndex'
    },
    shipping: {
        primaryID: 'shippingIndexIA_ID',
        latitude: 'latitudeShippingIndex',
        longitude: 'longitudeShippingIndex',
        firstName: 'firstNameShippingIndex',
        lastName: 'lastNameShippingIndex',
        fullAddress: 'autocompleteShippingIndex',
        street: 'streetShippingIndex',
        city: 'cityShippingIndex',
        state: 'stateShippingIndex',
        additionalAddress: 'additionalAddressShippingIndex',
        countryName: 'countryShippingIndex',
        countryCode: 'countryCodeShippingIndex', // Fixed typo
        postalCode: 'postalCodeShippingIndex',
        phone: 'phone5Index',
        otherPhone: 'phone6Index',
        email: 'emailShippingIndex'
    }
};

function initPhoneFields() {
    const phoneIds = [
        `#phone`, `#phone1`, `#phone2`, `#phone3`, `#phone4`, `#phone5`, `#phone6`,
        `#phonePersonIndex`, `#otherPhonePersonIndex`, `#phone5Index`, `#phone6Index`
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
                $("#" + ids.primaryID).val(response.customer.individualAddressID);
                $("#" + ids.firstName).val(response.customer.firstName);
                $("#" + ids.lastName).val(response.customer.lastName);
                $("#" + ids.full).val(response.customer.fullAddress);
                $("#" + ids.street).val(response.customer.street);
                $("#" + ids.city).val(response.customer.city);
                $("#" + ids.additionalAddress).val(response.customer.additionaladdress);
                $("#" + ids.state).val(response.customer.state);
                $("#" + ids.postal_code).val(response.customer.postalCode);
                $("#" + ids.country).val(response.customer.countryName);
                $("#" + ids.countryCode).val(response.customer.countryCode);
                $("#" + ids.latitude).val(response.customer.latitude);
                $("#" + ids.longitude).val(response.customer.longitude);
                $("#" + ids.phone).val(response.customer.phone);
                $("#" + ids.otherPhone).val(response.customer.otherPhone);
                $("#" + ids.email).val(response.customer.email);


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
        console.log(customers);
        const q = String(query ?? '')
            .toLowerCase()
            .trim()
            .replace(/ /g, "")
            .replace(/-/g, "");

        const filtered = customers.filter(item =>
            String(item.fullName ?? '').toLowerCase().includes(q) ||
            String(item.phone ?? '').toLowerCase().includes(q) ||
            String(item.email ?? '').toLowerCase().includes(q)
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

    function mapInit() {
        return new Promise((resolve, reject) => {
            setTimeout(() => {
                initAutocomplete();
                resolve();
            }, 300);
        })
    }

    function activeDeactiveClass(addClass, removeClassList) {
        Object.entries(addClass).forEach(([key, value]) => {
            $('#' + key).removeClass(value);
        });
        Object.entries(removeClassList).forEach(([key, value]) => {
            $('#' + key).addClass(value);
        });
    }


    function companyTabWork() {
        targetTab = 'company';
        removeClassList = {
            'person-tab': "active",
            "tab-Personal": "active show",
            //'addCustommerTab': 'active',
            'person': 'active show',
            "shippingAddressTabContent": "active show",
            "addBranchTabContent": "active show",
            "addWarehouseTabContent": "active show",
            'addBranchTab': 'active',
            'addWarehouseTab': 'active',
        }
        addClassList = {
            'company-tab': "active",
            'addCompanyTab': "active",
            "tab-Company": "active show",
            "company": "active show",

            "shippingAddressTab": "active",
        }
        activeDeactiveClass(removeClassList, addClassList)
    }
    function personTabWork() {
        targetTab = 'person';
        removeClassList = {
            'company-tab': "active",
            "tab-Company": "active show",
            "company": "active show",
            "addBranchTabContent": "active show",
            "addWarehouseTabContent": "active show",
            "shippingAddressTabContent": "active show",
            "shippingAddressTab": "active",
        }
        addClassList = {
            'person-tab': "active",
            "tab-Personal": "active show",
            'addCustommerTab': 'active',
            'person': 'active show'
        }
        activeDeactiveClass(removeClassList, addClassList)
    }

    $('#addNewContactNameBtn').on('click', async function () {

        $('#addCustomerModal').modal('show');

        await companyTabWork();
        await mapInit();
        console.log(targetTab);
    });

    $("#company-tab").on("click", async function () {
        await companyTabWork();
        await mapInit();
        console.log(targetTab);
    });


   


    $('#addNewContactNameBtn2').on('click', async function () {
        await personTabWork();
        $('#addCustomerModal').modal('show');
        await mapInit();
        console.log(targetTab);
    });

    $("#person-tab").on("click",async function () {
        $('#addCustomerModal').modal('show');
        await personTabWork();
        await mapInit();
        console.log(targetTab);
    });
    $("#addCustommerTab").on("click",async function () {
        await personTabWork();
        await mapInit();
        console.log(targetTab);
    });

    $("#shippingAddressTab").on("click", async function () {
        targetTab = 'shipping';
        await mapInit();
        console.log(targetTab);
    });

    $("#closeModal, #closeModal2").on('click', function () {
        $('#addCustomerModal').modal('hide');
        targetTab = "index";
        console.log(targetTab);
    });

  
    function initAutocomplete() {
        const ids = idMap[targetTab] || {};
        const input = document.getElementById(ids.fullAddress);

        if (!input) {
            console.warn(`Autocomplete input ${ids.fullAddress} not found for tab ${targetTab}`);
            return;
        }

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
                if (component.types.includes("country")) countryName = component.long_name;
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

            $("#" + ids.city).val(city);
            $("#" + ids.state).val(state);
            setCountry(ids.countryName, countryName); // keep as is, since it's your own function
            $("#" + ids.countryCode).val(countryCode);
            $("#" + ids.postalCode).val(postal_code);
            $("#" + ids.latitude).val(lat);
            $("#" + ids.longitude).val(lng);
            $("#" + ids.street).val(fullStreet);

        });
    }

    function setCountry(id, countryName) {
        $.ajax({
            url: '/CreateLead/addCountry',
            method: 'GET',
            contentType: 'application/json',
            data: { countryName: countryName },
            success: function (response) {
                console.log("Country response:", response);
                getCountryList().then(() => {
                    choiceManager.setChoiceValue(id, response.countryId);
                });
            },
            error: function (xhr) {
                console.error("Failed to set country:", xhr);
                toastr.error('Error setting country');
            }
        });
    }
    function getCountryList() {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getCountry',
                method: 'GET',
                contentType: 'application/json',
                success: function (response) {
                    console.log("Country List:", response);
                    document.querySelectorAll('.choiceDD').forEach(function (select) {
                        select.innerHTML = '';
                        select.append(new Option('Select Country', ''));

                        response.forEach(item => {
                            select.append(new Option(item.text, item.value));
                        });
                        if (select.choices) {
                            select.choices.setChoices(
                                data.map(i => ({ value: i.countryId, label: i.countryName })),
                                'value',
                                'label',
                                true
                            );
                        }
                    });
                    resolve(200);
                },
                error: function (xhr) {
                    console.error("Failed to set country:", xhr);
                    toastr.error('Error setting country');
                }
            });
        })
        
    }
    getCountryList();
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

    function titleizeKeys(key) {
        return key.charAt(0).toUpperCase() + key.slice(1);
    }

    $("#personSaveBtn").on("click", async function (e) {
        e.preventDefault();
        console.log(targetTab);
        
       
        if (await fieldValidation()) {
            const ids = idMap[targetTab]
            const data = {};
            data['PrimaryID'] = $("#" + ids.primaryID).val() ? parseInt($("#" + ids.primaryID).val(), 10) : 0;
            data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
            data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

            Object.entries(ids).slice(3).forEach(([key, value]) => {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            });

            console.log(data);

            $.ajax({
                url: '/CreateLead/InsertPerson',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    console.log("Upsert person response:", response);
                    if (response.success) {
                        setTimeout(() => {
                            $('#addCustomerModal').modal('hide');
                            toastr.success(response.message);
                            getCustomerList();
                            getCustomerInfo(response.result.data);
                        }, 400);
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
                IsIndividualCustomer: $("#customerType").val() === "billing",
                LeadName: $("#" + idMapIndex.indexBase.leadName).val() || "",
                LeadStatusID: parseInt($("#" + idMapIndex.indexBase.leadStatusID).val()) || 0,
                LeadSourceID: parseInt($("#" + idMapIndex.indexBase.leadSourceID).val()) || 0,
                LeadOwnerID: parseInt($("#" + idMapIndex.indexBase.leadOwnerID).val()) || 0,
                ApproximateDealValue: parseFloat($("#" + idMapIndex.indexBase.approximateDealValue).val()) || 0,
                ProbabilityPercentage: parseFloat($("#" + idMapIndex.indexBase.probabilityPercentage).val()) || 0,
                LeadDescription: $("#" + idMapIndex.indexBase.leadDescription).val() || "",

                Customers: []
            };

            actionTab.forEach(item => {
                const ids = idMapIndex[item] || {};
                const customerId = document.getElementById("customerID")?.value || "0";
                data.Customers.push({
                    TabName: item,
                    PrimaryID: parseInt(customerId, 10) || 0,
                    FirstName: $("#" + ids.firstName).val() || "",
                    LastName: $("#" + ids.lastName).val() || "",
                    FullAddress: $("#" + ids.fullAddress).val() || "",
                    Street: $("#" + ids.street).val() || "",
                    City: $("#" + ids.city).val() || "",
                    State: $("#" + ids.state).val() || "",
                    Additionaladdress: $("#" + ids.additionalAddress).val() || "",
                    PostalCode: $("#" + ids.postal_code).val() || "",
                    CountryName: $("#" + ids.country).val() || "",
                    CountryCode: $("#" + ids.countryCode).val() || "",
                    Latitude: parseFloat($("#" + ids.latitude).val()) || null,
                    Longitude: parseFloat($("#" + ids.longitude).val()) || null,
                    Phone: getPhoneNumber(ids.phone) || "",
                    OtherPhone: getPhoneNumber(ids.otherPhone) || "",
                    Email: $("#" + ids.email).val() || ""
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
                        getCustomerList();
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

    async function uniquenPhoneCheck() {
        let isValid = true;
        const targetedField = targetTab === 'index'
            ? [[idMapIndex.person.phone, idMapIndex.person.otherPhone, idMapIndex.person.primaryID, idMapIndex.person.email]]
            : targetTab === 'person'
                ? [[idMap.person.phone, idMap.person.otherPhone, idMap.person.primaryID, idMap.person.email]]
                : targetTab === 'company'
                    ? [[idMap.company.phone, idMap.company.otherPhone, idMap.company.primaryID, idMap.company.email]]
                    : [];

        for (const item of targetedField) {
            const [phoneSelector, otherPhoneSelector, idSelector, emailSelector] = item;
            const phone = getPhoneNumber(phoneSelector);
            const otherPhone = getPhoneNumber(otherPhoneSelector);
            const email = $("#" + emailSelector).val();
            const id = $(`#${idSelector}`).val() || "0";

            function errorActive(selector, errorMsg) {
                const errorSpan = $(`#${selector}`).closest(".col-12").find("#errorShow");
                errorSpan.text(errorMsg);
                $(`#${selector}`).css('border', '1px solid red');
                isValid = false;
            }
            function errorDeactive(selector) {
                const errorSpan = $(`#${selector}`).closest(".col-12").find("#errorShow");
                $(`#${selector}`).css('border', '1px solid #ccc')
                errorSpan.text("");
            }
            function samePhoneNumberCheck() {
                if (phone && otherPhone) {
                    if (phone === otherPhone) {
                        toastr.error("Phone Number and Other Number field value is same");
                    }

                }
            }

            if (phone) {
                try {
                    const isUnique = await uniquenessCheck(phone, "phone", id);
                    if (isUnique) {
                        errorDeactive(phoneSelector);
                        samePhoneNumberCheck();
                    } else {
                        errorActive(phoneSelector, "This Phone Number Already Used");
                    }
                } catch (error) {
                    console.error("Uniqueness check failed for phone:", error);
                    isValid = false;
                }
            }


            if (otherPhone) {
                try {
                    const isUnique = await uniquenessCheck(otherPhone, "phone", id);
                    if (isUnique) {
                        errorDeactive(otherPhoneSelector);
                        samePhoneNumberCheck();
                    } else {
                        errorActive(otherPhoneSelector, "This Other Phone Number Already Used");
                    }
                } catch (error) {
                    console.error("Uniqueness check failed for phone:", error);
                    isValid = false;
                }
            } else {
                errorDeactive(otherPhoneSelector);
            }
            

            if (email) {
                try {
                    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/; // simple email regex
                    let val = regex.test(email);
                    if (regex.test(email)) {
                        const isUnique = await uniquenessCheck(email, "email", id);
                        if (isUnique) {
                            errorDeactive(emailSelector);
                        } else {
                            errorActive(emailSelector, "This Email Already Used");
                        }
                    } else {
                        errorActive(emailSelector, "Please enter a valid email address");
                    }
                } catch (error) {
                    console.error("Uniqueness check failed for email:", error);
                    isValid = false;
                }
            } else {
                errorDeactive(emailSelector);
            }
        }

        return isValid;
    }

    async function fieldValidation() {
        const selectedTab = targetListForValidation();
        console.log("Validating fields for tab:", targetTab, "Fields:", selectedTab);

        let isValid = await uniquenPhoneCheck();
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
               
            } else {
                target.css('border', '1px solid #ccc');
            }
        });

        if (errorCount > 0) {
            isValid = false;
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


    $("#sameAsShippingBtn").on("click", async function () {
        let clickBtnValue = $(this).is(":checked");
        if (clickBtnValue == true) {
            for (const item in idMap.person) {
                $(`#${idMap.shipping[item]}`).val($(`#${idMap.person[item]}`).val() || "")
                let result = $(`#${idMap.person[item]}`).val();

            }
            // set country
            let selectedItem = $("#countryPerson").siblings(".choices__list").find(".choices__item--selectable[aria-selected='true']");
            let dataValue = selectedItem.attr("data-value");
            if ($(`#${idMap.person.country}`).data('value') != "")
                choiceManager.setChoiceValue(idMap.shipping.country, dataValue);
        }
    })
});