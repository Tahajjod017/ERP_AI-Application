// CREATE CUSTOMER CODE
// This handles customer creation (Person, Company, Branch, Warehouse, Shipping Address)

let itiMap = {};
let targetTab = 'index';

const idMap = {
    company: {
        primaryID: 'cId',
        latitude: 'cLatitude',
        longitude: 'cLongitude',
        companyName: 'cName',
        firstName: 'cFirstName',
        lastName: 'cLastName',
        fullAddress: 'cFullAddress',
        additionalAddress: 'cAdditionalAddress',
        street: 'cStreet',
        city: 'cCity',
        state: 'cState',
        countryID: 'cCountry',
        countryCode: 'cCountryCode',
        postalCode: 'cPostalCode',
        phone: 'cPhone',
        otherPhone: 'cOtherPhone',
        email: 'cEmail'
    },
    branch: {
        primaryID: 'bId',
        latitude: 'bLatitude',
        longitude: 'bLongitude',
        companyID: 'bCId',
        branchName: 'bName',
        firstName: 'bFirstName',
        lastName: 'bLastName',
        fullAddress: 'bFullAddress',
        additionalAddress: 'bAdditionalAddress',
        street: 'bStreet',
        city: 'bCity',
        state: 'bState',
        countryID: 'bCountry',
        countryCode: 'bCountryCode',
        postalCode: 'bPostalCode',
        phone: 'bPhone',
        otherPhone: 'bOtherPhone',
        email: 'bEmail'
    },
    warehouse: {
        primaryID: 'wId',
        latitude: 'wLatitude',
        longitude: 'wLongitude',
        companyID: 'wCId',
        wareHouseName: 'wName',
        firstName: 'wFirstName',
        lastName: 'wLastName',
        fullAddress: 'wFullAddress',
        additionalAddress: 'wAdditionalAddress',
        street: 'wStreet',
        city: 'wCity',
        state: 'wState',
        countryID: 'wCountry',
        countryCode: 'wCountryCode',
        postalCode: 'wPostalCode',
        phone: 'wPhone',
        otherPhone: 'wOtherPhone',
        email: 'wEmail'
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
        countryID: 'countryPerson',
        countryCode: 'countryCodePerson',
        postalCode: 'postalCodePerson',
        phone: 'pPhone',
        otherPhone: 'pOtherPhone',
        email: 'emailPerson'
    },
    shipping: {
        primaryID: 'sId',
        latitude: 'sLatitude',
        longitude: 'sLongitude',
        personID: 'sCId',
        firstName: 'sFirstName',
        lastName: 'sLastName',
        fullAddress: 'sFullAddress',
        additionalAddress: 'sAdditionalAddress',
        street: 'sStreet',
        city: 'sCity',
        state: 'sState',
        countryID: 'sCountry',
        countryCode: 'sCountryCode',
        postalCode: 'sPostalCode',
        phone: 'sPhone',
        otherPhone: 'sOtherPhone',
        email: 'sEmail'
    }
};

// Initialize phone fields with international phone input
function initPhoneFields() {
    const phoneIds = [
        `#${idMap.person.phone}`, `#${idMap.person.otherPhone}`,
        `#${idMap.branch.phone}`, `#${idMap.branch.otherPhone}`,
        `#${idMap.warehouse.phone}`, `#${idMap.warehouse.otherPhone}`,
        `#${idMap.shipping.phone}`, `#${idMap.shipping.otherPhone}`,
        `#${idMap.company.phone}`, `#${idMap.company.otherPhone}`
    ];

    phoneIds.forEach(selector => {
        const input = document.querySelector(selector);
        if (!input || itiMap[selector]) return;

        const iti = window.intlTelInput(input, {
            separateDialCode: true,
            initialCountry: 'bd',
            preferredCountries: ['bd', 'in', 'us'],
            utilsScript: "js/utils.js"
        });

        itiMap[selector] = iti;
    });
}

document.addEventListener("DOMContentLoaded", function () {
    initPhoneFields();
});

$(document).ready(function () {
    let companyList = [];
    let personSearchList = [];

    // Show company suggestions
    function showCompanySuggestions(listDiv, noResultDiv) {
        const $listDiv = $(listDiv);
        const $noResultsDiv = $(noResultDiv);
        $listDiv.empty().hide();
        $noResultsDiv.hide();

        if (companyList.length > 0) {
            $listDiv.show();
            companyList.forEach(item => {
                $listDiv.append(`
                    <button type="button" 
                        class="list-group-item list-group-item-action companyName-item" 
                        data-id="${item.id}" data-text="${item.name}">
                        ${item.name} ${item.phone} ${item.email}
                    </button>
                `);
            });
        } else {
            $noResultsDiv.show();
        }
    }

    // Show person suggestions
    function showPersonSuggestions(listDiv, noResultDiv) {
        const $listDiv = $(listDiv);
        const $noResultsDiv = $(noResultDiv);
        $listDiv.empty().hide();
        $noResultsDiv.hide();

        if (personSearchList.length > 0) {
            $listDiv.show();
            personSearchList.forEach(item => {
                $listDiv.append(`
                    <button type="button" 
                        class="list-group-item list-group-item-action personName-item" 
                        data-id="${item.id}" data-text="${item.name}">
                        ${item.name} ${item.phone} ${item.email}
                    </button>
                `);
            });
        } else {
            $noResultsDiv.show();
        }
    }

    // Company selection handler
    $(document).on('click', '.companyName-item', function () {
        const selected = $(this).data("text").trim();
        const customerId = $(this).data("id");
        let tabSymbol = targetTab == "branch" ? "b" : targetTab == "warehouse" ? 'w' : '';

        $('#' + tabSymbol + 'CompanySearch').val(selected);
        $('#' + tabSymbol + 'CId').val(customerId);
        $('#' + tabSymbol + 'CustomerList').hide();
        $('#' + tabSymbol + 'NoResults').hide();
        exExruntimeValidationCheck('#' + tabSymbol + 'CompanySearch');
    });

    // Person selection handler
    $(document).on('click', '.personName-item', function () {
        const selected = $(this).data("text").trim();
        const customerId = $(this).data("id");

        $('#personSearch').val(selected);
        $('#sCId').val(customerId);
        $('#sCustomerList').hide();
        $('#sNoResults').hide();
        exExruntimeValidationCheck('#personSearch');
    });

    // Search navigation
    function searchNavigation(inputSelector, listSelector, itemSelector) {
        let activeIndex = -1;

        $(document).on("keydown", inputSelector, function (e) {
            const $items = $(listSelector + " " + itemSelector);
            if ($items.length === 0) return;

            if (e.key === "ArrowDown") {
                e.preventDefault();
                activeIndex = (activeIndex + 1) % $items.length;
                updateActiveItem($items, activeIndex);
            } else if (e.key === "ArrowUp") {
                e.preventDefault();
                activeIndex = (activeIndex - 1 + $items.length) % $items.length;
                updateActiveItem($items, activeIndex);
            } else if (e.key === "Enter") {
                e.preventDefault();
                if (activeIndex >= 0) $items.eq(activeIndex).trigger("click");
            }
        });

        $(document).on("input", inputSelector, function () {
            activeIndex = -1;
        });
    }

    function updateActiveItem($items, index) {
        $items.removeClass("active");
        if (index >= 0) {
            $items.eq(index).addClass("active");
            $items.eq(index)[0].scrollIntoView({ block: "nearest" });
        }
    }

    searchNavigation("#wCompanySearch", "#wCustomerList", ".companyName-item");
    searchNavigation("#bCompanySearch", "#bCustomerList", ".companyName-item");
    searchNavigation("#personSearch", "#sCustomerList", ".personName-item");

    // Map initialization
    function mapInit() {
        return new Promise((resolve) => {
            setTimeout(() => {
                initAutocomplete();
                resolve();
            }, 300);
        });
    }

    // Tab management
    function activeDeactiveClass(removeClass, addClass) {
        Object.entries(removeClass).forEach(([key, value]) => {
            $('#' + key).removeClass(value);
        });
        Object.entries(addClass).forEach(([key, value]) => {
            $('#' + key).addClass(value);
        });
    }

    function companyTabWork() {
        targetTab = 'company';
        let removeClassList = {
            'person-tab': "active",
            "tab-Personal": "active show",
            'person': 'active show',
            "shippingAddressTabContent": "active show",
            "addBranchTabContent": "active show",
            "addWarehouseTabContent": "active show",
            'addBranchTab': 'active',
            'addWarehouseTab': 'active',
        };
        let addClassList = {
            'company-tab': "active",
            'addCompanyTab': "active",
            "tab-Company": "active show",
            "company": "active show",
            "shippingAddressTab": "active",
        };
        activeDeactiveClass(removeClassList, addClassList);
    }

    function personTabWork() {
        targetTab = 'person';
        let removeClassList = {
            'company-tab': "active",
            "tab-Company": "active show",
            "company": "active show",
            "addBranchTabContent": "active show",
            "addWarehouseTabContent": "active show",
            "shippingAddressTabContent": "active show",
            "shippingAddressTab": "active",
        };
        let addClassList = {
            'person-tab': "active",
            "tab-Personal": "active show",
            'addCustommerTab': 'active',
            'person': 'active show'
        };
        activeDeactiveClass(removeClassList, addClassList);
    }

    // Modal open buttons
    $('#addNewContactNameBtn').on('click', async function () {
        $('#addCustomerModal').modal('show');
        await companyTabWork();
        await mapInit();
    });

    $("#company-tab, #addCompanyTab").on("click", async function () {
        targetTab = "company";
        await companyTabWork();
        await mapInit();
    });

    $('#addNewContactNameBtn2').on('click', async function () {
        await personTabWork();
        $('#addCustomerModal').modal('show');
        await mapInit();
    });

    $("#person-tab, #addCustommerTab").on("click", async function () {
        await personTabWork();
        $('#addCustomerModal').modal('show');
        await mapInit();
    });

    $("#addBranchTab").on("click", async function () {
        targetTab = 'branch';
        await mapInit();
    });

    $("#addWarehouseTab").on("click", async function () {
        targetTab = 'warehouse';
        await mapInit();
    });

    $("#shippingAddressTab").on("click", async function () {
        targetTab = 'shipping';
        await mapInit();
    });

    $("#closeModal, #closeModal2").on('click', function () {
        $('#addCustomerModal').modal('hide');
        targetTab = "index";
    });

    // Google Maps Autocomplete
    function initAutocomplete() {
        const ids = idMap[targetTab] || {};
        const input = document.getElementById(ids.fullAddress);
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

            if (targetTab === 'company') {
                $("#" + ids.companyName).val(place.name || "");
            }

            $("#" + ids.city).val(city);
            $("#" + ids.state).val(state);
            setCountry(ids.countryID, countryName);
            $("#" + ids.countryCode).val(countryCode);
            $("#" + ids.postalCode).val(postal_code);
            $("#" + ids.latitude).val(lat);
            $("#" + ids.longitude).val(lng);
            $("#" + ids.street).val(fullStreet);
        });
    }

    // Country management
    function getCountryList(id) {
        return new Promise((resolve) => {
            $.ajax({
                url: '/CreateLead/getCountry',
                method: 'GET',
                contentType: 'application/json',
                success: function (response) {
                    choiceManager.populateDropdown(id, response);
                    resolve(200);
                },
                error: function () {
                    toastr.error('Error setting country');
                }
            });
        });
    }

    function setCountry(id, countryName) {
        $.ajax({
            url: '/CreateLead/addCountry',
            method: 'GET',
            contentType: 'application/json',
            data: { countryName: countryName },
            success: function (response) {
                getCountryList(id).then(() => {
                    choiceManager.setChoiceValue(id, response.countryId);
                });
            },
            error: function () {
                toastr.error('Error setting country');
            }
        });
    }

    // Company and person search
    function getCompanyList(id) {
        return new Promise((resolve) => {
            $.ajax({
                url: '/CreateLead/getCompanyList',
                method: 'POST',
                contentType: 'application/json',
                success: function (response) {
                    companyList = response;
                    choiceManager.populateDropdown(id, response);
                    resolve(200);
                },
                error: function () {
                    toastr.error('Error fetching company list');
                }
            });
        });
    }

    function getCompnayList(query) {
        return new Promise((resolve) => {
            $.ajax({
                url: '/CreateLead/getCompnayList',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(query),
                success: function (response) {
                    companyList = response;
                    resolve(200);
                },
                error: function () {
                    toastr.error('Error fetching company list');
                }
            });
        });
    }

    function getPersonList(query) {
        return new Promise((resolve) => {
            $.ajax({
                url: '/CreateLead/getPersonList',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(query),
                success: function (response) {
                    personSearchList = response;
                    resolve(200);
                },
                error: function () {
                    toastr.error('Error fetching person list');
                }
            });
        });
    }

    async function initCompany() {
        await getCompanyList();
    }
    initCompany();

    // Search handlers
    function cSearchInitial(searchFieldSelector, listShowSelector, noResultSelector) {
        let typingTimer;
        let delay = 200;

        $(searchFieldSelector).on("input", async function () {
            let query = $(this).val();
            clearTimeout(typingTimer);

            typingTimer = setTimeout(async function () {
                try {
                    if (query.length === 0) {
                        $(listShowSelector).hide();
                        $(noResultSelector).hide();
                    } else {
                        $(listShowSelector).show();
                        await getCompnayList(query);
                        await showCompanySuggestions(listShowSelector, noResultSelector);
                        $(noResultSelector).show();
                    }
                } catch (err) {
                    console.error("Error in search:", err);
                }
            }, delay);
        });
    }

    function pSearchInitial(searchFieldSelector, listShowSelector, noResultSelector) {
        let typingTimer;
        let delay = 200;

        $(searchFieldSelector).on("input", async function () {
            let query = $(this).val();
            clearTimeout(typingTimer);

            typingTimer = setTimeout(async function () {
                try {
                    if (query.length === 0) {
                        $(listShowSelector).hide();
                        $(noResultSelector).hide();
                    } else {
                        $(listShowSelector).show();
                        await getPersonList(query);
                        await showPersonSuggestions(listShowSelector, noResultSelector);
                        $(noResultSelector).show();
                    }
                } catch (err) {
                    console.error("Error in search:", err);
                }
            }, delay);
        });
    }

    cSearchInitial('#bCompanySearch', '#bCustomerList', '#bNoResults');
    cSearchInitial('#wCompanySearch', '#wCustomerList', '#wNoResults');
    pSearchInitial('#personSearch', '#sCustomerList', '#sNoResults');

    getCountryList();

    // Uniqueness check
    function uniquenessCheck(text, type, id) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/UniquenessCheck',
                method: 'GET',
                contentType: 'application/json',
                data: { queryText: text, type: type, id: id },
                success: function (response) {
                    resolve(response.unique);
                },
                error: function (xhr) {
                    toastr.error('Error checking uniqueness');
                    reject(xhr);
                }
            });
        });
    }

    // Helper functions
    function getPhoneNumber(selector) {
        const iti = itiMap[`#${selector}`];
        if (iti && iti.isValidNumber()) {
            return iti.getNumber();
        }
        return "";
    }

    function titleizeKeys(key) {
        return key.charAt(0).toUpperCase() + key.slice(1);
    }

    function clearTabData(ids) {
        Object.entries(ids).forEach(([key, value]) => {
            var field = $("#" + value);
            if (field.attr('type') == 'number') field.val(0);
            else field.val("");
        });
    }

    // SAVE FUNCTIONS

    // Save Person
    async function savePerson(e) {
        e.preventDefault();
        const ids = idMap.person;
        const data = {};

        Object.entries(ids).forEach(([key, value]) => {
            if (key === 'primaryID' || key == 'countryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'longitude' || key === 'latitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            } else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
        });

        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertPerson',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        resolve(response);
                    } else {
                        toastr.error(response.message || "Failed to save person");
                        resolve(response);
                    }
                },
                error: function (xhr) {
                    toastr.error('Error saving person');
                    reject(xhr);
                }
            });
        });
    }

    // Save Company
    async function saveCompany(e) {
        e.preventDefault();
        const ids = idMap.company;
        const data = {};

        Object.entries(ids).forEach(([key, value]) => {
            if (key === 'primaryID' || key == 'countryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'longitude' || key === 'latitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            } else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
        });

        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertCompany',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        resolve(response);
                    } else {
                        toastr.error(response.message || "Failed to save company");
                        resolve(response);
                    }
                },
                error: function (xhr) {
                    toastr.error('Error saving company');
                    reject(xhr);
                }
            });
        });
    }

    // Save Branch
    async function saveBranch(e) {
        e.preventDefault();
        const ids = idMap.branch;
        const data = {};

        Object.entries(ids).forEach(([key, value]) => {
            if (key === 'primaryID' || key == 'countryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'longitude' || key === 'latitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            } else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
        });

        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertBranch',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        resolve(response);
                    } else {
                        toastr.error(response.message || "Failed to save branch");
                        resolve(response);
                    }
                },
                error: function (xhr) {
                    toastr.error('Error saving branch');
                    reject(xhr);
                }
            });
        });
    }

    // Save Warehouse
    async function saveWarehouse(e) {
        e.preventDefault();
        const ids = idMap.warehouse;
        const data = {};

        Object.entries(ids).forEach(([key, value]) => {
            if (key === 'primaryID' || key == 'countryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'longitude' || key === 'latitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            } else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
        });

        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertWarehouse',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        resolve(response);
                    } else {
                        toastr.error(response.message || "Failed to save warehouse");
                        resolve(response);
                    }
                },
                error: function (xhr) {
                    toastr.error('Error saving warehouse');
                    reject(xhr);
                }
            });
        });
    }

    // Save Shipping Address
    async function saveShippingAddress(e) {
        e.preventDefault();
        const ids = idMap.shipping;
        const data = {};

        Object.entries(ids).forEach(([key, value]) => {
            if (key === 'primaryID' || key == 'countryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'longitude' || key === 'latitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            } else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
        });

        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertShippingAddress',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        resolve(response);
                    } else {
                        toastr.error(response.message || "Failed to save shipping address");
                        resolve(response);
                    }
                },
                error: function (xhr) {
                    toastr.error('Error saving shipping address');
                    reject(xhr);
                }
            });
        });
    }

    // BUTTON CLICK HANDLERS

    // Person: Go to Shipping Tab
    $("#goToShippingTab").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await savePerson(e);
                targetTab = 'shipping';

                if (result.success == true) {
                    clearTabData(idMap.person);
                    let removeClassList = {
                        'addCustommerTab': 'active',
                        'person': 'active show'
                    };
                    let addClassList = {
                        'shippingAddressTab': "active",
                        "shippingAddressTabContent": "active show"
                    };
                    activeDeactiveClass(removeClassList, addClassList);
                    await mapInit();

                    $('#personSearch').val(result.name);
                    $('#' + idMap.shipping.personID).val(result.id);
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Person: Save and Exit
    $("#personSaveAndExit").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await savePerson(e);

                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    clearTabData(idMap.person);
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Company: Save and Exit
    $("#companySaveAndExit").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveCompany(e);

                if (result.success == true) {
                    clearTabData(idMap.company);
                    getCompanyList();
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Company: Save and Go to Branch Tab
    $("#saveAndGoBranchTab").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveCompany(e);
                targetTab = 'branch';

                if (result.success == true) {
                    clearTabData(idMap.company);
                    let removeClassList = {
                        'addCompanyTab': 'active',
                        'company': 'active show'
                    };
                    let addClassList = {
                        'addBranchTab': "active",
                        "addBranchTabContent": "active show"
                    };
                    await mapInit();
                    activeDeactiveClass(removeClassList, addClassList);
                    await getCompanyList();

                    $('#bCompanySearch').val(result.name);
                    $('#' + idMap.branch.companyID).val(result.id);
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Branch: Save and Exit
    $("#branchSaveAndExit").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveBranch(e);

                if (result.success == true) {
                    clearTabData(idMap.branch);
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    $('#bCompanySearch').val("");
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Branch: Save and Go to Warehouse Tab
    $("#saveAndgoWarehouseTab").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveBranch(e);
                targetTab = 'warehouse';

                if (result.success == true) {
                    clearTabData(idMap.branch);
                    let removeClassList = {
                        'addBranchTab': 'active',
                        'addBranchTabContent': 'active show'
                    };
                    let addClassList = {
                        'addWarehouseTab': "active",
                        "addWarehouseTabContent": "active show"
                    };
                    await mapInit();
                    activeDeactiveClass(removeClassList, addClassList);
                    $('#bCompanySearch').val("");
                    await getCompanyList();

                    $('#wCompanySearch').val(result.name);
                    $('#' + idMap.warehouse.companyID).val(result.id);
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Warehouse: Save and Exit
    $("#saveWarehouse").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveWarehouse(e);

                if (result.success == true) {
                    clearTabData(idMap.warehouse);
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    $('#wCompanySearch').val("");
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Shipping Address: Save
    $("#shippingAddressSaveBtn").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveShippingAddress(e);

                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    clearTabData(idMap.shipping);
                    $("#personSearch").val("");
                    targetTab = 'index';
                }
                $(this).prop("disabled", false);
            }
        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed");
        }
    });

    // Same as Shipping Button
    $("#sameAsShippingBtn").on("click", async function () {
        let customerId = $('#' + idMap.shipping.personID).val();
        let clickBtnValue = $(this).is(":checked");

        if (clickBtnValue == true) {
            let ids = idMap.shipping;
            $.ajax({
                url: '/CreateLead/GetCustomerInfo',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(customerId),
                success: function (response) {
                    $('#personContactNameSearch').val(response.customer.fullName);
                    $('#personCustomerID').val(response.customer.customerAddressID);
                    $("#" + ids.firstName).val(response.customer.firstName);
                    $("#" + ids.lastName).val(response.customer.lastName);
                    $("#" + ids.fullAddress).val(response.customer.fullAddress);
                    $("#" + ids.street).val(response.customer.street);
                    $("#" + ids.city).val(response.customer.city);
                    $("#" + ids.additionalAddress).val(response.customer.additionaladdress);
                    $("#" + ids.state).val(response.customer.state);
                    $("#" + ids.postalCode).val(response.customer.postalCode);
                    $("#" + ids.countryID).val(response.customer.countryID);
                    $("#" + ids.countryCode).val(response.customer.countryCode);
                    $("#" + ids.latitude).val(response.customer.latitude);
                    $("#" + ids.longitude).val(response.customer.longitude);
                    $("#" + ids.phone).val(response.customer.phone);
                    $("#" + ids.otherPhone).val(response.customer.otherPhone);
                    $("#" + ids.email).val(response.customer.email);
                },
                error: function (xhr) {
                    toastr.error('Failed to load Contact Name');
                }
            });
        }
    });

    // VALIDATION FUNCTIONS
    function exExruntimeValidationCheck(inputEl) {
        let $idBank = $(inputEl).siblings('.idBank');
        let fieldValue = parseInt($idBank.val()) || 0;

        if (fieldValue === 0) {
            $(inputEl).css('border', '1px solid red');
        } else {
            $(inputEl).css('border', '1px solid #ccc');
        }
    }

    $("#bCompanySearch, #wCompanySearch, #personSearch").on("change", function () {
        let $idBank = $(this).siblings('.idBank');
        $idBank.val(0);
        let fieldValue = parseInt($idBank.val()) || 0;
        $(this).css('border', fieldValue === 0 ? '1px solid red' : '1px solid #ccc');
    });

    function targetListForValidation() {
        if (targetTab === 'person') {
            return [idMap.person.firstName, idMap.person.phone];
        } else if (targetTab === "shipping") {
            return [idMap.shipping.firstName, idMap.shipping.phone];
        } else if (targetTab === "company") {
            return [idMap.company.companyName, idMap.company.firstName, idMap.company.phone];
        } else if (targetTab === "branch") {
            return [idMap.branch.branchName, idMap.branch.firstName, idMap.branch.phone];
        } else if (targetTab === "warehouse") {
            return [idMap.warehouse.wareHouseName, idMap.warehouse.firstName, idMap.warehouse.phone];
        }
        return [];
    }

    function exListForValidation() {
        return targetTab === "shipping" ? 'personSearch'
            : targetTab === 'branch' ? 'bCompanySearch'
                : targetTab === "warehouse" ? 'wCompanySearch'
                    : "";
    }

    function extraFieldIdValidation() {
        return new Promise((resolve) => {
            let targetField = exListForValidation();
            if (targetField != "") {
                let hiddenFieldVal = $("#" + targetField).siblings(".idBank");
                let fieldValue = parseInt(hiddenFieldVal.val()) || 0;

                if (fieldValue === 0) {
                    $("#" + targetField).css('border', '1px solid red');
                    resolve(false);
                } else {
                    $("#" + targetField).css('border', '1px solid #ccc');
                    resolve(true);
                }
            }
            resolve(true);
        });
    }

    async function uniquenPhoneCheck() {
        let isValid = true;
        const targetedField = targetTab === 'person' ? [[idMap.person.phone, idMap.person.otherPhone, idMap.person.primaryID, idMap.person.email]]
            : targetTab === 'company' ? [[idMap.company.phone, idMap.company.otherPhone, idMap.company.primaryID, idMap.company.email]]
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
                $(`#${selector}`).css('border', '1px solid #ccc');
                errorSpan.text("");
            }

            if (phone) {
                try {
                    const isUnique = await uniquenessCheck(phone, "phone", id);
                    if (isUnique) {
                        errorDeactive(phoneSelector);
                    } else {
                        errorActive(phoneSelector, "This Phone Number Already Used");
                    }
                } catch (error) {
                    isValid = false;
                }
            }

            if (otherPhone) {
                try {
                    const isUnique = await uniquenessCheck(otherPhone, "phone", id);
                    if (isUnique) {
                        errorDeactive(otherPhoneSelector);
                    } else {
                        errorActive(otherPhoneSelector, "This Other Phone Number Already Used");
                    }
                } catch (error) {
                    isValid = false;
                }
            }

            if (email) {
                try {
                    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
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
        runtimeValidationCheck();
        let isValid = true;
        isValid = await uniquenPhoneCheck();
        isValid = isValid === false ? false : await extraFieldIdValidation();
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

            if (name === '') {
                target.css('border', '1px solid red');
                errorCount += 1;
            } else {
                target.css('border', '1px solid #ccc');
            }
        });

        if (errorCount > 0) {
            isValid = false;
            toastr.warning(errorCount === 1 ? "This field is required" : "These fields are required");
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

    function runtimeValidationCheck() {
        const selectedTabList = targetListForValidation();
        selectedTabList.filter(Boolean).forEach(e => {
            $(document).on('input change', `#${e}`, function () {
                fieldValidationOne(this);
            });
        });
    }
});