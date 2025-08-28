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
    demoField: {
        customerID:'iCID',
        latitude: 'iLatitude',
        longitude: 'iLongitude',
        firstName: 'iFirstName',
        lastName: 'iLastName',
        fullAddress: 'iFullAddress',
        street: 'iStreet',
        city: 'iCity',
        state: 'iState',
        additionalAddress: 'iAdditionalAddress',
        countryID: 'iCountry',
        countryCode: 'iCountryCode',
        postalCode: 'iPostalCode',
        phone: 'iPhone',
        otherPhone: 'iOtherPhone',
        email: 'iEmail'
    },
    
};

function initPhoneFields() {
    const phoneIds = [
        `#${idMapIndex.demoField.phone}`, `#${idMapIndex.demoField.otherPhone}`, `#${idMap.person.phone}`, `#${idMap.person.otherPhone}`, `#${idMap.branch.phone}`, `#${idMap.branch.otherPhone}`, `#${idMap.warehouse.phone}`, `#${idMap.warehouse.otherPhone}`, `#${idMap.shipping.phone}`, `#${idMap.shipping.otherPhone}`, `#phone5`, `#phone6`,

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
    let allCustomerList = []

    const list = document.getElementById("customerList");
    const list2 = document.getElementById("searchResults");
    const list3 = document.getElementById("no-results");

    //if (list.children.length === 0 || list3.children.length === 0) {
    //    list2.style.display = "none";
    //} else {
    //    list2.style.display = "block";
    //}

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
    let personSearchList = [];

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

                    // Trigger validation if needed
                },
                error: function (xhr) {
                    toastr.error('Failed to load Contact Name');
                    reject(xhr);
                }
            });
        });
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
  //  function showSuggestions2(query) {
  //      const $list = $('#personCustomerList');
  //      const $noResults = $('#personNoResults');
  //      $list.empty();
  //      $list.hide();
  //      $noResults.hide();

  //      if (!query) return;
  //      const q = String(query ?? '')
  //          .toLowerCase()
  //          .trim()
  //          .replace(/ /g, "")
  //          .replace(/-/g, "");

  //      const filtered = customers.filter(item =>
  //          String(item.fullName ?? '').toLowerCase().includes(q) ||
  //          String(item.phone ?? '').toLowerCase().includes(q) ||
  //          String(item.email ?? '').toLowerCase().includes(q)
  //      );
  //      if (filtered.length > 0) {
  //          $list.show();
  //          filtered.forEach(item => {
  //              $list.append(`
  //<button type="button" 
  //        class="list-group-item list-group-item-action customerName-item2" 
  //        data-id="${item.customerId}" 
  //        data-type="${item.type}">
  //    ${item.fullName} &nbsp; ${item.phone ? `${item.phone}` : ""} &nbsp; 
  //    ${item.email ? `${item.email}` : ""} </button>`);
  //          });
  //      } else {
  //          $noResults.show();
  //      }
  //  }

  //  function showCompanySuggestions(listDiv, noResutlDiv) {
  //      const $listDiv = $('#' + listDiv);
  //      const $noResultsDiv = $('#' + noResutlDiv);
  //      $listDiv.empty();
  //      $listDiv.hide();
  //      $noResultsDiv.hide();

  //      if (!query) return;
  //      const q = String(query ?? '')
  //          .toLowerCase()
  //          .trim();

  //      const filtered = companyList.filter(item =>
  //          String(item.text ?? '').toLowerCase().includes(q)
  //      );
  //      if (filtered.length > 0) {
  //          $listDiv.show();
  //          filtered.forEach(item => {
  //              $listDiv.append(`
  //<button type="button" 
  //        class="list-group-item list-group-item-action companyName-item" 
  //        data-id="${item.value}"">
  //    ${item.text} </button>`);
  //          });
  //      } else {
  //          $noResultsDiv.show();
  //      }
  //  }
    function showCompanySuggestions(listDiv, noResutlDiv) {
        const $listDiv = $(listDiv);
        const $noResultsDiv = $(noResutlDiv);
        $listDiv.empty();
        $listDiv.hide();
        $noResultsDiv.hide();

        if (companyList.length > 0) {
            $listDiv.show();
            companyList.forEach(item => {
                $listDiv.append(`
  <button type="button" 
          class="list-group-item list-group-item-action companyName-item" 
          data-id="${item.id}" data-text="${item.name}">
      ${item.name} ${item.phone} ${item.email}</button>`);
            });
        } else {
            $noResultsDiv.show();
        }
    }

    function showPersonSuggestions(listDiv, noResutlDiv) {
        const $listDiv = $(listDiv);
        const $noResultsDiv = $(noResutlDiv);
        $listDiv.empty();
        $listDiv.hide();
        $noResultsDiv.hide();

        if (personSearchList.length > 0) {
            $listDiv.show();
            personSearchList.forEach(item => {
                $listDiv.append(`
  <button type="button" 
          class="list-group-item list-group-item-action personName-item" 
          data-id="${item.id}" data-text="${item.name}">
      ${item.name} ${item.phone} ${item.email}</button>`);
            });
        } else {
            $noResultsDiv.show();
        }
    }
    function showAllCustomerSuggestions(listDiv, noResutlDiv) {
        const $listDiv = $(listDiv);
        const $noResultsDiv = $(noResutlDiv);
        $listDiv.empty();
        $listDiv.hide();
        $noResultsDiv.hide();

        if (allCustomerList.length > 0) {
            $listDiv.show();
            allCustomerList.forEach(item => {
                $listDiv.append(`
  <button type="button" 
          class="list-group-item list-group-item-action customerName-item" 
          data-id="${item.id}" data-text="${item.name}">
      ${item.name} ${item.phone} ${item.email}</button>`);
            });
        } else {
            $noResultsDiv.show();
        }
    }



    //$('#personContactNameSearch').on('input', function () {
    //    const query = $(this).val();
    //    $('#personSearchResults').show();
    //    $('#removeContactNameBtn').toggle(!!query);
    //    showSuggestions2(query);
    //});

    function setDataDesktop(id) {

        $('#customerList').hide();
        $('#noResults').hide();

        getCustomerInfo(id).then(response => {
            document.getElementById("customerInfoContainer").style.display = "block";

            let title = "";
            if (response.customer.addressTypeName === "billing") {
                title = "Person Information"
            } else if (response.customer.addressTypeName === "company") {
                title = "Company Information"
            }

            $("#iInformationTitle").text(title);
            $('#ContactNameSearch').val(response.customer.fullName);


            const ids = idMapIndex.demoField;
            $("#" + ids.customerID).val(response.customer.individualAddressID);
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

            $('#searchResults').hide();
            $('#removeContactNameBtn').show();
        });
    }

    $(document).on('click', '.customerName-item', function () {
        const selected = $(this).text().trim();
        const customerId = $(this).data("id");
        const customerType = $(this).data("type");
        $('#ContactNameSearch').val(selected);
        $('#customerID').val(customerId);
        $('#customerType').val(customerType);
        getCustomerInfo(customerId).then(response => {
            document.getElementById("customerInfoContainer").style.display = "block";
            let title = "";
            if (response.customer.addressTypeName === "billing") {
                title = "Person Information"
            } else if (response.customer.addressTypeName === "company") {
                title = "Company Information"
            }

            $("#iInformationTitle").text(title);
            $('#ContactNameSearch').val(response.customer.fullName);
       

            const ids = idMapIndex.demoField;
            $("#" + ids.customerID).val(response.customer.customerAddressID);
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

            $('#customerList').hide();
            $('#noResults').hide();
        });


    });
    $(document).on('click', '.customerName-item2', function () {
        const selected = $(this).text().trim();
        const customerId = $(this).data("id");
        $('#personContactNameSearch').val(selected);
        $('#personCustomerID').val(customerId);
        $('#personSearchResults').hide();
        getCustomerInfo(customerId).then(response => {
            ids = idMap.shipping;
            $('#personContactNameSearch').val(response.customer.fullName);
            $('#personCustomerID').val(response.customer.individualAddressID);
           
        });

    });
    $(document).on('click', '.companyName-item', function () {
        const selected = $(this).data("text").trim();
        const customerId = $(this).data("id");
        let tabSymble = targetTab == "branch" ? "b" : targetTab == "warehouse" ? 'w' : '';
        $('#' + tabSymble + 'CompanySearch').val(selected);
        $('#' + tabSymble + 'CId').val(customerId);
        $('#' + tabSymble + 'CustomerList').hide();
        $('#' + tabSymble + 'NoResults').hide();
    });
;

    $(document).on('click', '.personName-item', function () {
        const selected = $(this).data("text").trim();
        const customerId = $(this).data("id");

        $('#personSearch').val(selected);
        $('#sCId').val(customerId);
        $('#sCustomerList').hide();
        $('#sNoResults').hide();
    });

    // Handle arrow keys + enter

    function searchNavigation(inputSelector, listSelector, itemSelector) {
        let activeIndex = -1;
        //let activeIndex = 0;
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
            activeIndex = - 1;
            //activeIndex = 0;
        });
    }
    


    searchNavigation("#wCompanySearch", "#wCustomerList", ".companyName-item");
    searchNavigation("#bCompanySearch", "#bCustomerList", ".companyName-item");
    searchNavigation("#personSearch", "#sCustomerList", ".personName-item");

    //let activeIndex = -1;
    //$(document).on("keydown", "#wCompanySearch", function (e) {
    //    const $items = $("#wCustomerList .companyName-item");
    //    if ($items.length === 0) return;

    //    if (e.key === "ArrowDown") {
    //        e.preventDefault();
    //        activeIndex = (activeIndex + 1) % $items.length;
    //        updateActiveItem($items, activeIndex);
    //    } else if (e.key === "ArrowUp") {
    //        e.preventDefault();
    //        activeIndex = (activeIndex - 1 + $items.length) % $items.length;
    //        updateActiveItem($items, activeIndex);
    //    } else if (e.key === "Enter") {
    //        e.preventDefault();
    //        if (activeIndex >= 0) $items.eq(activeIndex).trigger("click");
    //    }
    //});
    //// Reset index when typing
    //$(document).on("input", "#wCompanySearch", function () {
    //    activeIndex = -1;
    //});

    // Reset index when typing
    //let bActiveIndex = -1;
    //$(document).on("keydown", "#bCompanySearch", function (e) {
    //    const $items = $("#bCustomerList .companyName-item");
    //    if ($items.length === 0) return;

    //    if (e.key === "ArrowDown") {
    //        e.preventDefault();
    //        bActiveIndex = (bActiveIndex + 1) % $items.length;
    //        updateActiveItem($items, bActiveIndex);
    //    } else if (e.key === "ArrowUp") {
    //        e.preventDefault();
    //        bActiveIndex = (bActiveIndex - 1 + $items.length) % $items.length;
    //        updateActiveItem($items, bActiveIndex);
    //    } else if (e.key === "Enter") {
    //        e.preventDefault();
    //        if (bActiveIndex >= 0) $items.eq(bActiveIndex).trigger("click");
    //    }
    //});

    //$(document).on("input", "#bCompanySearch", function () {
    //    bActiveIndex = -1;
    //});

    // Highlight helper
    function updateActiveItem($items, index) {
        $items.removeClass("active");
        if (index >= 0) {
            $items.eq(index).addClass("active");
            $items.eq(index)[0].scrollIntoView({ block: "nearest" });
        }
    }

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
        targetTab = "company";
        await companyTabWork();
        await mapInit();
    });
    $("#addCompanyTab").on("click", async function () {
        targetTab = "company";
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


    function getCountryList(id) {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getCountry',
                method: 'GET',
                contentType: 'application/json',
                success: function (response) {
                    showDev(response,"contry");

                    choiceManager.populateDropdown(id, response)
                    
                    resolve(200);
                },
                error: function (xhr) {
                    toastr.error('Error setting country');
                }
            });
        })
        
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
            error: function (xhr) {
                toastr.error('Error setting country');
            }
        });
    }
    function getCompanyList(id) {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getCompanyList',
                method: 'POST',
                contentType: 'application/json',
                success: function (response) {
                    companyList = response;
                    choiceManager.populateDropdown(id, response);
                    resolve(200);
                },
                error: function (xhr) {
                    toastr.error('Error setting country');
                }
            });
        })
        
    }
    
    function getAllCustomerList(query) {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getAllCustomerList',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(query),
                success: function (response) {
                    allCustomerList = response;
                    resolve(200);
                },
                error: function (xhr) {
                    toastr.error('Error setting country');
                }
            });
        })
        
    }
    function getPersonList(query) {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getPersonList',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(query),
                success: function (response) {
                    personSearchList = response;
                    resolve(200);
                },
                error: function (xhr) {
                    toastr.error('Error setting country');
                }
            });
        })
        
    }
    function getCompnayList(query) {
        return new Promise((resolve, reject) => {   
            $.ajax({
                url: '/CreateLead/getCompnayList',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(query),
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

    //$("#bCompanySearch").on("input", async function () {
    //    $('#bCustomerList').show();
    //    $('#bNoResults').show();
    //    showCompanySuggestions($(this).val(), 'bCustomerList','bNoResults');
    //})
    //$("#wCompanySearch").on("input", async function () {
    //    try {
    //        $('#wCustomerList').show();
    //        $('#wNoResults').show();
    //        await showCompanySuggestions($(this).val(), 'wCustomerList', 'wNoResults');
    //    } catch (err) {
    //        console.error("Error in showCompanySuggestions:", err);
    //    }
    //});

    // 1: search input box id
    // 2: search result show div id
    // 3: if result found then noResult div id
    //$('#ContactNameSearch').on('input', function () {
    //    const query = $(this).val();
    //    $('#customerList').show();
    //    $('#noResults').show();
    //    $('#removeContactNameBtn').toggle(!!query);
        //showSuggestions(query);
    //});
    iSerachInitial('#ContactNameSearch', '#customerList', '#noResults')
    cSerachInitial('#bCompanySearch', '#bCustomerList', '#bNoResults')
    cSerachInitial('#wCompanySearch', '#wCustomerList', '#wNoResults')
    pSerachInitial('#personSearch', '#sCustomerList', '#sNoResults')
    function iSerachInitial(searchFieldSelector, listShowSelector, noResultSelector) {
        let typingTimer;
        let delay = 200;

        $(searchFieldSelector).on("input", async function () {
            let query = $(this).val();
            clearTimeout(typingTimer);
            typingTimer = setTimeout(async function () {
                console.log("User stoppedTyping value: ", query);
                try {
                    if (query.length === 0) {
                        $(listShowSelector).hide();
                        $(noResultSelector).hide();
                    } else {
                        $(listShowSelector).show();
                        await getAllCustomerList(query);
                        await showAllCustomerSuggestions(listShowSelector, noResultSelector);
                        $(noResultSelector).show();
                    }
                } catch (err) {
                    console.error("Error in showCompanySuggestions:", err);
                }
            }, delay)


        });

    } 
    function pSerachInitial(searchFieldSelector, listShowSelector, noResultSelector) {
        let typingTimer;
        let delay = 200;

        $(searchFieldSelector).on("input", async function () {
            let query = $(this).val();
            clearTimeout(typingTimer);
            typingTimer = setTimeout(async function () {
                console.log("User stoppedTyping value: ", query);
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
                    console.error("Error in showCompanySuggestions:", err);
                }
            }, delay)


        });

    } 
    function cSerachInitial(searchFieldSelector, listShowSelector, noResultSelector) {
        let typingTimer;
        let delay = 200;

        $(searchFieldSelector).on("input", async function () {
            let query = $(this).val();
            clearTimeout(typingTimer);

            typingTimer = setTimeout(async function () {
                console.log("User stoppedTyping value: ", query);
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
                    console.error("Error in showCompanySuggestions:", err);
                }
            }, delay)


        });

    } 
   
    


    //let typingTimer;
    //let delay = 500;
    //$("#personSearch").on("input", async function () {
    //    let query = $(this).val();
    //    clearTimeout(typingTimer);
    //    typingTimer = setTimeout(async function () {
    //        console.log("User stoppedTyping value: ", query);
    //        try {
    //            if (query.length === 0) {
    //                $('#sCustomerList').hide();
    //                $('#sNoResults').hide();
    //            } else {
    //                $('#sCustomerList').show();
    //                await getPersonList(query);
    //                await showPersonSuggestions('sCustomerList', 'sNoResults');
    //                $('#sNoResults').show();
    //            }
    //        } catch (err) {
    //            console.error("Error in showCompanySuggestions:", err);
    //        }
    //    }, delay)

        
    //});

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

    //function modalValidation(item) {
    //    const ids = idMap[item] || {};
    //    if (item === "shipping") {
    //        return $(`#${ids.firstName}`).val().trim() !== "" && $(`#${ids.lastName}`).val().trim() !== "";
    //    }
    //    return true;
    //}

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

    async function savePerson(e) {
        
        e.preventDefault();
        
        const ids = idMap[targetTab]
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

        const ids = idMap[targetTab]
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

        const ids = idMap[targetTab]
        const data = {};
        
        //data['PrimaryID'] = 
        //data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
        //data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

        Object.entries(ids).forEach(([key, value]) => {
            if (key === 'primaryID' || key == 'countryID') {
                data[titleizeKeys(key)] = $("#" + value).val() ? parseInt($("#" + value).val(), 10) : 0;
            } else if (key === 'longitude' || key === 'latitude') {
                data[titleizeKeys(key)] = parseFloat($("#" + value).val()) || null;
            } else if (key === 'phone' || key === 'otherPhone') {
                data[titleizeKeys(key)] = getPhoneNumber(value);
            }else {
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

        const ids = idMap[targetTab]
        const data = {};
        
        //data['PrimaryID'] = 
        //data['Latitude'] = parseFloat($("#" + ids.latitude).val()) || null;
        //data['Longitude'] = parseFloat($("#" + ids.longitude).val()) || null;

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
        e.preventDefault();
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
                await mapInit();
                setDataDesktop(result.id);
                await getCustomerList(); 
                var customer = await customers.find(c => c.customerId == result.id);
                $('#personSearch').val(customer.fullName)
                $('#' + idMap.shipping.personID).val(customer.customerId);

            };
            $(this).prop("disabled", false);

        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed")
        }
     
    });
    $("#personSaveAndExit").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await savePerson(e);
                if (result.success == true) {
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                    clearTabData(idMap.person);
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
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
                    setDataDesktop(result.id);
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
    });

    $("#saveAndGoBranchTab").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveCompany(e);

                targetTab = 'branch'
                if (result.success == true) {
                    clearTabData(idMap.company);
                    removeClassList = {
                        'addCompanyTab': 'active',
                        'company': 'active show'
                    }
                    addClassList = {
                        'addBranchTab': "active",
                        "addBranchTabContent": "active show",

                    }
                    await mapInit();
                    activeDeactiveClass(removeClassList, addClassList)
                }
                setDataDesktop(result.id);
                await getCompanyList();
                //var customer = await .find(c => c.customerId == result.id);
                //choiceManager.populateDropdown(idMap.branch.companyID, result);
                $('#bCompanySearch').val(result.name);
                $('#' + idMap.branch.companyID).val(result.id);

            };
            $(this).prop("disabled", false);

        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed")
        }

    });


    


    $("#branchSaveAndExit").on("click", async function (e) {
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveBranch(e);
                if (result.success == true) {
                    clearTabData(idMap.branch);
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                    $('#bCompanySearch').val("");
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });

    $("#saveAndgoWarehouseTab").on("click", async function (e) {
        e.preventDefault();
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveBranch(e);

                targetTab = 'warehouse'
                if (result.success == true) {
                    clearTabData(idMap.branch);
                    removeClassList = {
                        'addBranchTab': 'active',
                        'addBranchTabContent': 'active show'
                    }
                    addClassList = {
                        'addWarehouseTab': "active",
                        "addWarehouseTabContent": "active show",

                    }
                    await mapInit();
                    activeDeactiveClass(removeClassList, addClassList)
                    $('#bCompanySearch').val("");
                }
                setDataDesktop(result.id);
                await getCompanyList();
                //var customer = await .find(c => c.customerId == result.id);
                //choiceManager.populateDropdown(idMap.branch.companyID, result);
                $('#wCompanySearch').val(result.name);
                $('#' + idMap.warehouse.companyID).val(result.id);

            };
            $(this).prop("disabled", false);

        } catch (e) {
            $(this).prop("disabled", false);
            toastr.error("Save Failed")
        }

    });

    $("#saveWarehouse").on("click", async function (e) {
        try {
            if (await fieldValidation()) {
                $(this).prop("disabled", true);
                var result = await saveWarehouse(e);
                if (result.success == true) {
                    clearTabData(idMap.warehouse);
                    $('#addCustomerModal').modal('hide');
                    targetTab = 'index';
                    setDataDesktop(result.id);
                    $('#wCompanySearch').val("");
                }
                
            };
            $(this).prop("disabled", false);
        } catch (e) {
            toastr.error("Save Failed")
        }
     
    });
        
        

        // go to shipping tab
        

    $("#shippingAddressSaveBtn").on("click", async function (e) {

        if (await fieldValidation()) {
            $(this).prop("disabled", true);
            var result = await saveShippingAddress(e);   
            if (result.success == true) {
                setDataDesktop($('#' + idMap.shipping.personID).val());
                $('#addCustomerModal').modal('hide');
                clearTabData(idMap.shipping);
                $("#personSearch").val("");
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
                LeadName: $("#" + idMapIndex.indexBase.leadName).val() || "",
                LeadStatusID: parseInt($("#" + idMapIndex.indexBase.leadStatusID).val()) || 0,
                LeadSourceID: parseInt($("#" + idMapIndex.indexBase.leadSourceID).val()) || 0,
                LeadOwnerID: parseInt($("#" + idMapIndex.indexBase.leadOwnerID).val()) || 0,
                ApproximateDealValue: parseFloat($("#" + idMapIndex.indexBase.approximateDealValue).val()) || 0,
                ProbabilityPercentage: parseFloat($("#" + idMapIndex.indexBase.probabilityPercentage).val()) || 0,
                CustomerId: parseInt($("#" + idMapIndex.demoField.customerID).val()) || 0,
                LeadDescription: $("#" + idMapIndex.indexBase.leadDescription).val(),
            };

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
                        clearTabData(idMapIndex.demoField);
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
                idMap.company.companyName,
                idMap.company.firstName,
                idMap.company.phone
            ];
        } else if (targetTab === "branch") {
            return [
                idMap.branch.branchName,
                idMap.branch.firstName,
                idMap.branch.phone
            ];
        } else if (targetTab === "warehouse") {
            return [
                idMap.warehouse.wareHouseName,
                idMap.warehouse.firstName,
                idMap.warehouse.phone
            ];
        } else if (targetTab === "index") {
            return [
                idMapIndex.indexBase.leadName,
                idMapIndex.indexBase.leadStatusID,
                idMapIndex.indexBase.leadSourceID,
                idMapIndex.indexBase.leadOwnerID,   
            ].filter(Boolean);
        } else {
            return [];
        }
    }
    function exListForValidation() {
       return targetTab === "shipping" ? 'personSearch'
            : targetTab === 'branch' ? 'bCompanySearch'
                : targetTab === "warehouse" ? 'wCompanySearch'
                    : targetTab === "index" ? idMapIndex.indexBase.customerName
                        : "";
    }
    function extraFieldIdValidation() {
        return new Promise((resove, reject) => {
            let tergetField = exListForValidation();

            if (tergetField != "") {
                let hiddenFieldVal = $("#" + tergetField).siblings(".idBank")

                let fieldValue = parseInt(hiddenFieldVal.val()) || 0;

                if (fieldValue === 0) {
                    $("#" + tergetField).css('border', '1px solid red')
                    
                    resove(false);
                }
                else {
                    $("#" + tergetField).css('border', '1px solid #ccc')
                    resove(true);
                }
            }
            resove(true);
        });

        
    }

    async function uniquenPhoneCheck() {
        let isValid = true;
        const targetedField = targetTab === 'person' ? [[idMap.person.phone, idMap.person.otherPhone, idMap.person.primaryID, idMap.person.email]]
            //: targetTab === 'shipping' ? [[idMap.shipping.phone, idMap.shipping.otherPhone, idMap.shipping.primaryID, idMap.shipping.email]]
                : targetTab === 'company' ? [[idMap.company.phone, idMap.company.otherPhone, idMap.company.primaryID, idMap.company.email]]
                   // : targetTab === 'branch' ? [[idMap.branch.phone, idMap.branch.otherPhone, idMap.branch.primaryID, idMap.branch.email]]
                      //  : targetTab === 'warehouse' ? [[idMap.warehouse.phone, idMap.warehouse.otherPhone, idMap.warehouse.primaryID, idMap.warehouse.email]]
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
            function messageDeactive(selector) {
                const errorSpan = $(`#${selector}`).closest(".col-12").find("#errorShow");
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
            } else {
                messageDeactive(phoneSelector);
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
        runtimeValidationCheck();
        exRuntimeValidationCheck();
        let isValid = true;
        isValid = await uniquenPhoneCheck();
        isValid = isValid === false ? false :  await extraFieldIdValidation();

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

    function idFieldValidationOne(fieldSelector) {


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

    //function removeValidationOne(obj) {
    //    obj = $(obj);
    //    const name = obj.val().trim();
    //    let target;

    //    if (obj.closest('.choices').length > 0) {
    //        target = obj.closest('.choices').find('.choices__inner');
    //    } else {
    //        target = obj;
    //    }

    //    if (name === '') {
    //        target.css('border', '1px solid #ccc');
    //    }
    //}

    // Runtime validation for input changes
    function runtimeValidationCheck() {
        const selectedTabList = targetListForValidation();
        selectedTabList.filter(Boolean).forEach(e => {
            $(document).on('input change', `#${e}`, function () {
                fieldValidationOne(this);
            });
        });
    }
    function exRuntimeValidationCheck() {
        const selectedTab = exListForValidation();
        if (!selectedTab) return; // exit if no valid ID

        // Bind **once** using delegated event
        $(document).off('input change', `#${selectedTab}`); // remove old handlers first

        $(document).on('input change', `#${selectedTab}`, function () {
            let $idBank = $(this).siblings('.idBank'); // the related hidden field
            let fieldValue = parseInt($idBank.val()) || 0;

            if (fieldValue === 0) {
                $(this).css('border', '1px solid red');
            } else {
                $(this).css('border', '1px solid #ccc');
            }
        });
    }


    //runtimeValidationCheck();


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
                    $('#personCustomerID').val(response.customer.individualAddressID);

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