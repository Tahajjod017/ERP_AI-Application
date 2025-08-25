//const { ajax } = require("jquery"); // Commented out as jQuery is assumed to be globally available

//const { toArray } = require("lodash");

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
        countryName: 'cCountry',
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
        countryName: 'bCountry',
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
        branchName: 'wName',
        firstName: 'wFirstName',
        lastName: 'wLastName',
        fullAddress: 'wFullAddress',
        additionalAddress: 'wAdditionalAddress',
        street: 'wStreet',
        city: 'wCity',
        state: 'wState',
        countryName: 'wCountry',
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
        countryName: 'countryPerson',
        countryCode: 'countryCodePerson',
        postalCode: 'postalCodePerson',
        phone: 'pPhone',
        otherPhone: 'pOtherPhone',
        email: 'emailPerson'
    },
    shipping: {
        primaryID: 'shipingID',
        customerID: 'personCustomerID',
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
        `#phone`, `#phone1`, `#phone2`, `#${idMap.person.phone}`, `#${idMap.person.otherPhone}`, `#${idMap.branch.phone}`, `#${idMap.branch.otherPhone}`, `#${idMap.warehouse.phone}`, `#${idMap.warehouse.otherPhone}`, `#phone5`, `#phone6`,
        `#phonePersonIndex`, `#otherPhonePersonIndex`, `#phone5Index`, `#phone6Index`,
        `#${idMap.company.phone}`, `#${idMap.company.otherPhone}`
    ];

    phoneIds.forEach(selector => {
        const input = document.querySelector(selector);
        if (!input) {
            return;
        }

        // Prevent double initialization
        if (itiMap[selector]) {
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

    //console.log("Phone fields initialized:", Object.keys(itiMap));
}

// Initialize once when DOM ready
document.addEventListener("DOMContentLoaded", function () {
    initPhoneFields();
});

$(document).ready(function () {
    let companyList = []
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
            
        } else {
        }
    });

    let customers = [];

    async function getCustomerList() {
        try {
            customers = await new Promise((resolve, reject) => {
                $.ajax({
                    url: '/CreateLead/GetCustomerList',
                    method: 'GET',
                    success: function (data) {
                        resolve(data);
                    },
                    error: function (xhr) {
                        toastr.error('Failed to load Contact Name');
                        reject(xhr);
                    }
                });
            });
        } catch (err) {
            console.error(err);
            customers = []; // fallback
        }
    }

    // Usage
    async function initCutomer() {
        await getCustomerList(); // wait for AJAX to complete
    }

    initCutomer();


    function getCustomerInfo(customerId) {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/GetCustomerInfo',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(customerId),
                success: function (response) {
                    resolve(response);

                    // Re-initialize phone fields
                    initPhoneFields();

                    // Trigger validation
                },
                error: function (xhr) {
                    toastr.error('Failed to load Contact Name');
                }
            });
        })
       
    }

    function showSuggestions(query) {
        const $list = $('#customerList');
        const $noResults = $('#noResults');
        $list.empty();
        $list.hide();
        $noResults.hide();

        if (!query) return;
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
                $list.append(`
  <button type="button" 
          class="list-group-item list-group-item-action customerName-item" 
          data-id="${item.customerId}" 
          data-type="${item.type}">
      ${item.fullName} &nbsp; ${item.phone ? `${item.phone}` : ""} &nbsp; 
      ${item.email ? `${item.email}` : ""} </button>`);
            });
        } else {
            $noResults.show();
        }
    }
    function showSuggestions2(query) {
        const $list = $('#personCustomerList');
        const $noResults = $('#personNoResults');
        $list.empty();
        $list.hide();
        $noResults.hide();

        if (!query) return;
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
                $list.append(`
  <button type="button" 
          class="list-group-item list-group-item-action customerName-item2" 
          data-id="${item.customerId}" 
          data-type="${item.type}">
      ${item.fullName} &nbsp; ${item.phone ? `${item.phone}` : ""} &nbsp; 
      ${item.email ? `${item.email}` : ""} </button>`);
            });
        } else {
            $noResults.show();
        }
    }
    function showCompanySuggestions(query) {
        const $list = $('#bCustomerList');
        const $noResults = $('#bNoResults');
        $list.empty();
        $list.hide();
        $noResults.hide();

        console.log(query);

        if (!query) return;
        const q = String(query ?? '')
            .toLowerCase()
            .trim();

        const filtered = companyList.filter(item =>
            String(item.text ?? '').toLowerCase().includes(q)
        );
        console.log(filtered);
        if (filtered.length > 0) {
            $list.show();
            filtered.forEach(item => {
                $list.append(`
  <button type="button" 
          class="list-group-item list-group-item-action companyName-item" 
          data-id="${item.value}"">
      ${item.text} </button>`);
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

    $('#personContactNameSearch').on('input', function () {
        const query = $(this).val();
        $('#personSearchResults').show();
        $('#removeContactNameBtn').toggle(!!query);
        showSuggestions2(query);
    });

    function setDataDesktop(id) {
        getCustomerInfo(id).then(response => {
            document.getElementById("customerInfoContainer").style.display = "block";

            $('#ContactNameSearch').val(response.customer.firstName + " " + response.customer.lastName);
            //$('#customerType').val(response.customer.addressTypeName);

            const ids = idMapIndex.person;
            $("#" + ids.primaryID).val(response.customer.individualAddressID);
            $("#" + ids.firstName).val(response.customer.firstName);
            $("#" + ids.lastName).val(response.customer.lastName);
            $("#" + ids.fullAddress).val(response.customer.fullAddress);
            $("#" + ids.street).val(response.customer.street);
            $("#" + ids.city).val(response.customer.city);
            $("#" + ids.additionalAddress).val(response.customer.additionaladdress);
            $("#" + ids.state).val(response.customer.state);
            $("#" + ids.postalCode).val(response.customer.postalCode);
            $("#" + ids.countryName).val(response.customer.countryName);
            $("#" + ids.countryCode).val(response.customer.countryCode);
            $("#" + ids.latitude).val(response.customer.latitude);
            $("#" + ids.longitude).val(response.customer.longitude);
            $("#" + ids.phone).val(response.customer.phone);
            $("#" + ids.otherPhone).val(response.customer.otherPhone);
            $("#" + ids.email).val(response.customer.email);

            $('#searchResults').hide();
            $('#removeContactNameBtn').show();
        });
    }

    $(document).on('click', '.customerName-item', function () {

        const selected = $(this).text();
        const customerId = $(this).data("id");
        const customerType = $(this).data("type");
        $('#ContactNameSearch').val(selected);
        $('#customerID').val(customerId);
        $('#customerType').val(customerType);
        getCustomerInfo(customerId).then(response => {
            document.getElementById("customerInfoContainer").style.display = "block";

            $('#ContactNameSearch').val(response.customer.firstName + " " + response.customer.lastName);
            $('#customerID').val(response.customer.individualAddressID);
            $('#customerType').val(response.customer.addressTypeName);

            const ids = idMapIndex.person;
            $("#" + ids.primaryID).val(response.customer.individualAddressID);
            $("#" + ids.firstName).val(response.customer.firstName);
            $("#" + ids.lastName).val(response.customer.lastName);
            $("#" + ids.fullAddress).val(response.customer.fullAddress);
            $("#" + ids.street).val(response.customer.street);
            $("#" + ids.city).val(response.customer.city);
            $("#" + ids.additionalAddress).val(response.customer.additionaladdress);
            $("#" + ids.state).val(response.customer.state);
            $("#" + ids.postalCode).val(response.customer.postalCode);
            $("#" + ids.countryName).val(response.customer.countryName);
            $("#" + ids.countryCode).val(response.customer.countryCode);
            $("#" + ids.latitude).val(response.customer.latitude);
            $("#" + ids.longitude).val(response.customer.longitude);
            $("#" + ids.phone).val(response.customer.phone);
            $("#" + ids.otherPhone).val(response.customer.otherPhone);
            $("#" + ids.email).val(response.customer.email);

            $('#searchResults').hide();
            $('#removeContactNameBtn').show();
        });


    });
    $(document).on('click', '.customerName-item2', function () {
        debugger;
        const selected = $(this).text();
        const customerId = $(this).data("id");
        $('#personContactNameSearch').val(selected);
        $('#personCustomerID').val(customerId);
        getCustomerInfo(customerId).then(response => {
            ids = idMap.shipping;
            console.log("response: ", response);
            $('#personContactNameSearch').val(response.customer.firstName + " " + response.customer.lastName);
            $('#personCustomerID').val(response.customer.individualAddressID);
            $('#personSearchResults').hide();
        });

    });
    $(document).on('click', '.companyName-item', function () {
        debugger;
        const selected = $(this).text();
        const customerId = $(this).data("id");
        $('#bCompanySearch').val(selected);
        $('#bCId').val(customerId);
        $('#bSearchResults').hide();
        console.log(selected, customerID);
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
    });

    $("#company-tab").on("click", async function () {
        await companyTabWork();
        await mapInit();
    });


   


    $('#addNewContactNameBtn2').on('click', async function () {
        await personTabWork();
        $('#addCustomerModal').modal('show');
        await mapInit();
    });

    $("#person-tab").on("click",async function () {
        $('#addCustomerModal').modal('show');
        await personTabWork();
        await mapInit();
    });
    $("#addCustommerTab").on("click",async function () {
        await personTabWork();
        await mapInit();
    });

    $("#addBranchTab").on("click", async function () {
        targetTab = 'branch';
        await mapInit();
        console.log(targetTab);
    });
    $("#addWarehouseTab").on("click", async function () {
        targetTab = 'warehouse';
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
    });

  
    function initAutocomplete() {
        const ids = idMap[targetTab] || {};
        const input = document.getElementById(ids.fullAddress);

        if (!input) {
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
                $("#" + ids.companyName).val(company_name);

                const service = new google.maps.places.PlacesService(document.createElement('div'));
                service.getDetails({
                    placeId: place.place_id,
                    fields: ['formatted_phone_number']
                }, (details, status) => {
                    if (status === google.maps.places.PlacesServiceStatus.OK) {
                        const phone = details.formatted_phone_number || '';
                        $("#" + ids.phone).val(phone);
                    }
                });
            }
            debugger;
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
                getCountryList().then(() => {
                    choiceManager.setChoiceValue(id, response.countryId);
                });
            },
            error: function (xhr) {
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
                    toastr.error('Error setting country');
                }
            });
        })
        
    }
    
    function getCompanyList() {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getCompanyList',
                method: 'POST',
                contentType: 'application/json',
                success: function (response) {
                    companyList = response;
                    resolve(200);
                },
                error: function (xhr) {
                    toastr.error('Error setting country');
                }
            });
        })
        
    }

    async function initCompany() {
        await getCompanyList();
    }

    initCompany();

    $("#bCompanySearch").on("input",async function () {
        showCompanySuggestions($(this).val());
        console.log("clicked on company search btn");
    })
    getCountryList();
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

    function clearTabData(ids) {
        Object.entries(ids).forEach(([key, value]) => {
            var field = $("#" + value);
            if (field.attr('type') == 'number') field.val(0)
            else field.val("")
            
        });
    }

    async function savePerson(e) {
        
        e.preventDefault();
        
        const ids = idMap[targetTab]
        const data = {};
        
        data['PrimaryID'] = $("#" + ids.primaryID).val() ? parseInt($("#" + ids.primaryID).val(), 10) : 0;
        data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
        data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

        Object.entries(ids).slice(3).forEach(([key, value]) => {
            data[titleizeKeys(key)] = $("#" + value).val() || "";
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
                        getCustomerList();
                        getCustomerInfo(response.id);
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
        })
            
        
    };
    async function saveCompany(e) {
        
        e.preventDefault();
        console.log(targetTab);
        const ids = idMap[targetTab]
        const data = {};
      

        Object.entries(ids).slice(3).forEach(([key, value]) => {
            debugger;
            if (key === 'primaryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'latitude' || key === 'longitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            }else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
            
        });
        console.log("data", data);
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertCompany',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        getCustomerList();
                        getCustomerInfo(response.id);
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
        })
            
        
    };
    async function saveBranch(e) {
        
        e.preventDefault();
        console.log(targetTab);
        const ids = idMap[targetTab]
        const data = {};
        
        //data['PrimaryID'] = 
        //data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
        //data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

        Object.entries(ids).slice(3).forEach(([key, value]) => {
            debugger;
            if (key === 'primaryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'latitude' || key === 'longitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            }else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
            
        });
        debugger;
        console.log("data", data);
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertBranch',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        getCustomerList();
                        getCustomerInfo(response.id);
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
        })
            
        
    };
    async function saveWarehouse(e) {
        
        e.preventDefault();
        console.log(targetTab);
        const ids = idMap[targetTab]
        const data = {};
        
        //data['PrimaryID'] = 
        //data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
        //data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

        Object.entries(ids).slice(3).forEach(([key, value]) => {
            debugger;
            if (key === 'primaryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'latitude' || key === 'longitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            }else {
                data[titleizeKeys(key)] = $("#" + value).val() || "";
            }
            
        });
        debugger;
        console.log("data", data);
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/CreateLead/InsertBranch',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        getCustomerList();
                        getCustomerInfo(response.id);
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
        })
            
        
    };
    async function saveShippingAddress(e) {
        
        e.preventDefault();
        
        const ids = idMap[targetTab]
        const data = {};
        data['PrimaryID'] = $("#" + ids.primaryID).val() ? parseInt($("#" + ids.primaryID).val(), 10) : 0;
        data['CustomerID'] = $("#" + ids.customerID).val() ? parseInt($("#" + ids.customerID).val(), 10) : 0;
        data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
        data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

        Object.entries(ids).slice(4).forEach(([key, value]) => {
            data[titleizeKeys(key)] = $("#" + value).val() || "";
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
                        toastr.error(response.message || "Failed to save person");
                        resolve(response);
                    }
                },
                error: function (xhr) {
                    toastr.error('Error saving person');
                    reject(xhr);
                }
            });
        })
            
        
    };


    $("#goToShippingTab").on("click", async function (e) {

        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await savePerson(e);
                targetTab = 'shipping'
                if (result.success == true) {
                    clearTabData(idMap.person);
                    removeClassList = {
                        'addCustommerTab': 'active',
                        'person': 'active show'
                    }
                    addClassList = {
                        'shippingAddressTab': "active",
                        "shippingAddressTabContent": "active show",

                    }
                    activeDeactiveClass(removeClassList, addClassList)
                }
                setDataDesktop(result.id);
                await getCustomerList(); 
                var cusomer = await customers.find(c => c.customerId == result.id);
                $('#personContactNameSearch').val(cusomer.fullName)
                $('#personCustomerID').val(cusomer.customerId);
                console.log(cusomer.customerId);
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
    $("#personSaveAndExit").on("click", async function (e) {
        console.log(targetTab);
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await savePerson(e);
                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
    $("#companySaveAndExit").on("click", async function (e) {
        try {
            if (true) {
                $(this).prop("disabled", true);
                var result = await saveCompany(e);
                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
    $("#branchSaveAndExit").on("click", async function (e) {
        try {
            if (true) {
                $(this).prop("disabled", true);
                var result = await saveBranch(e);
                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
    $("#saveWarehouse").on("click", async function (e) {
        try {
            if (true) {
                $(this).prop("disabled", true);
                var result = await saveWarehouse(e);
                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
        
        

        // go to shipping tab
        

    $("#shippingAddressSaveBtn").on("click", async function (e) {
        console.log(targetTab);
        if (await fieldValidation()) {
            $(this).prop("disabled", true);
            var result = await saveShippingAddress(e);   
            if (result.success == true) {
                setDataDesktop($('#personCustomerID').val());
                $('#addCustomerModal').modal('hide');
                clearTabData(idMap.shipping);
                targetTab = 'index';
            }
            $(this).prop("disabled", false);
        }
        $(this).prop("disabled", false);
        
    });


    $("#indexSaveBtn").on("click", async function (e) {
        e.preventDefault();
        if (await fieldValidation()) {
            const data = {
                IsIndividualCustomer: $("#customerType").val() === "billing",
                LeadName: $("#" + idMapIndex.indexBase.leadName).val() || "",
                LeadStatusID: parseInt($("#" + idMapIndex.indexBase.leadStatusID).val()) || 0,
                LeadSourceID: parseInt($("#" + idMapIndex.indexBase.leadSourceID).val()) || 0,
                LeadOwnerID: parseInt($("#" + idMapIndex.indexBase.leadOwnerID).val()) || 0,
                ApproximateDealValue: parseFloat($("#" + idMapIndex.indexBase.approximateDealValue).val()) || 0,
                ProbabilityPercentage: parseFloat($("#" + idMapIndex.indexBase.probabilityPercentage).val()) || 0,
                CustomerId: parseInt($("#" + idMapIndex.person.primaryID).val()) || 0,
            };

            console.log(data);

            $.ajax({
                url: '/CreateLead/CreateLeadData',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        getCustomerList();
                        clearTabData(idMapIndex.indexBase);
                        clearTabData(idMapIndex.person);
                        window.location.href = "/crm/Index";
                    } else {
                        toastr.error(response.message || "Failed to create lead");
                    }
                },
                error: function (xhr) {
                    toastr.error("Error creating lead");
                }
            });
        } else {

        }
    });

    function targetListForValidation() {
        if (targetTab === 'person') {
            return [
                idMap.person.firstName,
                idMap.person.phone,
            ]
            
        } else if (targetTab === "shipping") {
            return [
                idMap.shipping.firstName,
                idMap.shipping.phone
            ];
        }else if (targetTab === "company") {
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
            ].filter(Boolean);
        } else {
            return [];
        }
    }

    async function uniquenPhoneCheck() {
        let isValid = true;
        const targetedField = targetTab === 'person'
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

        let isValid = await uniquenPhoneCheck();

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
           
        } else {
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
            });
        });
    }

    runtimeValidationCheck();


    $("#sameAsShippingBtn").on("click", async function () {
        let customerId = $('#personCustomerID').val();
        console.log("customerId ", customerId)
        let clickBtnValue = $(this).is(":checked");
        if (clickBtnValue == true) {
            let ids = idMap.shipping;
            $.ajax({
                url: '/CreateLead/GetCustomerInfo',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(customerId),
                success: function (response) {
                    console.log("response: ", response);
                    $('#personContactNameSearch').val(response.customer.firstName + " " + response.customer.lastName);
                    $('#personCustomerID').val(response.customer.individualAddressID);

                    $("#" + ids.firstName).val(response.customer.firstName);
                    $("#" + ids.lastName).val(response.customer.lastName);
                    $("#" + ids.fullAddress).val(response.customer.fullAddress);
                    $("#" + ids.street).val(response.customer.street);
                    $("#" + ids.city).val(response.customer.city);
                    $("#" + ids.additionalAddress).val(response.customer.additionaladdress);
                    $("#" + ids.state).val(response.customer.state);
                    $("#" + ids.postalCode).val(response.customer.postalCode);
                    $("#" + ids.countryName).val(response.customer.countryName);
                    $("#" + ids.countryCode).val(response.customer.countryCode);
                    $("#" + ids.latitude).val(response.customer.latitude);
                    $("#" + ids.longitude).val(response.customer.longitude);
                    $("#" + ids.phone).val(response.customer.phone);
                    $("#" + ids.otherPhone).val(response.customer.otherPhone);
                    $("#" + ids.email).val(response.customer.email);
                    // Trigger validation
                },
                error: function (xhr) {
                    toastr.error('Failed to load Contact Name');
                }
            });
            // set country
            let selectedItem = $("#countryPerson").siblings(".choices__list").find(".choices__item--selectable[aria-selected='true']");
            let dataValue = selectedItem.attr("data-value");
            if ($(`#${idMap.person.country}`).data('value') != "")
                choiceManager.setChoiceValue(idMap.shipping.country, dataValue);
        }
    })
});