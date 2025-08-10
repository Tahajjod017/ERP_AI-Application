let itiMap = {};

document.addEventListener("DOMContentLoaded", function () {

    const phoneIds = ["#phone", "#phone1", "#phone2", "#phone3", "#phone4", "#phone5", "#phone6"];

    phoneIds.forEach(selector => {
        const input = document.querySelector(selector);
        if (!input) return;

        const iti = window.intlTelInput(input, {
            separateDialCode: true,
            initialCountry: 'bd',
            preferredCountries: ['bd', 'in', 'us'],
            utilsScript: "js/utils.js"
        });

        itiMap[selector] = iti;
    });

    console.log("Phone fields initialized", itiMap);
});


$(document).ready(function () {

    const list = document.getElementById("contactNameList");
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
                break; // if you want to stop at first valid input
            }
        }

        if (anyValid) {
            console.log(lastValidNumber);
        } else {
            console.log('Invalid number!');
        }
    });



    //#region Auto suggest

    let nationalities = [];



    $.ajax({
        url: '/CreateLead/GetNationalities',
        method: 'GET',
        success: function (data) {
            nationalities = data;
        },
        error: function () {
            alert('Failed to load Contact Name');
        }
    });



    function showSuggestions(query) {
        const $list = $('#contactNameList');
        const $noResults = $('#noResults');
        $list.empty();
        $noResults.hide();

        if (!query) return;

        const filtered = nationalities.filter(item =>
            item.toLowerCase().includes(query.toLowerCase())
        );

        if (filtered.length > 0) {
            filtered.forEach(item => {
                $list.append(`<button type="button" class="list-group-item list-group-item-action contractName-item">${item}</button>`);
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

    $(document).on('click', '.contractName-item', function () {
        const selected = $(this).text();
        $('#ContactNameSearch').val(selected);
        $('#searchResults').hide();
        $('#removeContactNameBtn').show();
    });

    $('#removeContactNameBtn').on('click', function () {
        $('#ContactNameSearch').val('');
        $('#contactNameList').empty();
        $('#noResults').hide();
        $('#searchResults').hide();
        $(this).hide();
    });
    let targetTab = 'company';

    $('#addNewContactNameBtn').on('click', function () {
        targetTab = 'company';
        $('#addNationalityModal').modal('show');
        //remove
        
        $('#person-tab').removeClass('active');
        $('#tab-Personal').removeClass('active show');
        $('#person').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#addBranchTab').removeClass('active');
        $('#addWarehouseTab').removeClass('active');
        //active
        $('#company-tab').addClass('active');
        $('#addCompanyTab').addClass('active');
        $('#tab-Company').addClass('active show');
        $('#company').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
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
        //active
        $('#company-tab').addClass('active');
        $('#addCompanyTab').addClass('active');
        $('#tab-Company').addClass('active show');
        $('#company').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
    });

    $('#addNewContactNameBtn2').on('click', function () {
        targetTab = 'person';
        $('#addNationalityModal').modal('show');
        //remove
        $('#company-tab').removeClass('active');
        $('#tab-Company').removeClass('active show');
        $('#company').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#shippingAddressTab').removeClass('active');
        //active
        $('#person-tab').addClass('active');
        $('#tab-Personal').addClass('active show');
        $('#addCustommerTab').addClass('active');
        $('#person').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
    });

    $("#person-tab").on("click", function () {
        targetTab = 'person';
        //remove
        $('#company-tab').removeClass('active');
        $('#tab-Company').removeClass('active show');
        $('#company').removeClass('active show');
        $('#addBranchTabContent').removeClass('active show');
        $('#addWarehouseTabContent').removeClass('active show');
        $('#shippingAddressTabContent').removeClass('active show');
        $('#shippingAddressTab').removeClass('active');
        //active
        $('#person-tab').addClass('active');
        $('#tab-Personal').addClass('active show');
        $('#addCustommerTab').addClass('active');
        $('#person').addClass('active show');
        setTimeout(() => {
            initAutocomplete();
        }, 300);
    });
    //shiping tab code
    $("#shippingAddressTab").on("click", function () {
        console.log("shipping tag clicked");
        targetTab = 'shiping';
        setTimeout(() => {
            initAutocomplete();
        }, 300);
    });

    $("#closeModal").on('click', function () {
        $('#addNationalityModal').modal('hide');
    });
    $("#closeModal2").on('click', function () {
        $('#addNationalityModal').modal('hide');
    });
    let autocomplete;
    const idMap = {
        company: {
            primaryID: 'personID',
            firstName: 'firstNamePerson',
            lastName: 'lastNamePerson',

            autocomplete: 'autocompleteCompany',
            street: 'streetCompany',
            city: 'cityCompany',
            state: 'stateCompany',
            country: 'countryCompany',
            countryCode: 'countryCodeCompany',
            postal_code: 'postalCodeCompany',
            latitude: 'latitudeCompany',
            longitude: 'longitudeCompany',

            Phone: 'phone3',
            otherPhone: 'phone4',
            email: 'emailPerson',
        },
        person: {
            primaryID: 'personID',
            firstName: 'firstNamePerson',
            lastName: 'lastNamePerson',
            autocomplete: 'autocompletePerson', // full address
            street: 'streetPerson',
            city: 'cityPerson',
            state: 'statePerson',
            additionalAddress : 'additionalAddressPerson',
            country: 'countryPerson',
            countryCode: 'countryCodePerson',
            postal_code: 'postalCodePerson',
            latitude: 'latitudePerson',
            longitude: 'longitudePerson',
            phone: 'phone3',
            otherPhone: 'phone4',
            email: 'emailPerson',
        },
        shiping: {
            primaryID: 'shipingID',
            firstName: 'firstNameShiping',
            lastName: 'lastNamePerson',
            autocomplete: 'autocompleteShiping', // full address
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
            email: 'emailShiping',
        }
    };
    function initAutocomplete() {
        const ids = idMap[targetTab] || {};
        const input = document.getElementById(ids.autocomplete);;

        if (!input) return;

        autocomplete = new google.maps.places.Autocomplete(input, {
            //types: ["address"],
            types: ["establishment"],
            fields: ["place_id", "name", "formatted_address", "address_components", "geometry"],
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

            // e.g. "#company" or "#person"
            console.log(`terget tab: ${targetTab}`);
            if (targetTab === 'company') {
                const company_name = place.name || "";
                document.getElementById("companyName").value = company_name

                // get mobile number
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
            document.getElementById(ids.country).value = country;
            document.getElementById(ids.countryCode).value = countryCode;
            document.getElementById(ids.postal_code).value = postal_code;
            document.getElementById(ids.latitude).value = lat;
            document.getElementById(ids.longitude).value = lng;
            document.getElementById(ids.street).value = fullStreet;
        });
    }
    $(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
        const newTab = $(e.target).data('bs-target'); // e.g. "#company" or "#person"
        if (newTab === '#company') {
            targetTab = 'company'
        } else if (newTab === '#person'){
            targetTab = 'person';
        }
        console.log('Activated tab:', newTab);
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log(targetTab);
    });
    
    // Optional: hide suggestion list when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#ContactNameSearch, #searchResults').length) {
            $('#searchResults').hide();
        }
    });

    function getPhoneNumber(selector) {
        console.log(selector);
        const iti = itiMap[selector];
        if (iti && iti.isValidNumber()) {
            anyValid = true;
            lastValidNumber = iti.getNumber();
            return lastValidNumber;
        }
        return null;
    }

    // save data
    $("#modalSaveBtn").on("click", function (e) {
        debugger;
        e.preventDefault();
        if (targetTab === 'company') {
            console.log("company tab open");
        } else if (targetTab === 'person') {
            console.log("Person tab open");
        }
        
        const ids = idMap[targetTab] || {};
        console.log(ids.phone);
        var data = {
            TabName : targetTab,
            PrimaryID: document.getElementById(ids.primaryID).value
                ? parseInt(document.getElementById(ids.primaryID).value, 10)
                : 0,
            FirstName: document.getElementById(ids.firstName).value,
            LastName: document.getElementById(ids.lastName).value,
            FullAddress: document.getElementById(ids.autocomplete).value,
            Street: document.getElementById(ids.street).value,
            City: document.getElementById(ids.city).value,
            State: document.getElementById(ids.state).value,
            Additionaladdress: document.getElementById(ids.additionalAddress).value,
            PostalCode: document.getElementById(ids.postal_code).value,
            //  CountryID: 1,
            CountryName: document.getElementById(ids.country).value,
            CountryCode: document.getElementById(ids.countryCode).value,
            Latitude: parseFloat(document.getElementById(ids.latitude).value) || null,
            Longitude: parseFloat(document.getElementById(ids.longitude).value) || null,
            Phone: getPhoneNumber(`#${ids.phone}`),
            OtherPhone: getPhoneNumber(`#${ids.otherPhone}`),
            Email: document.getElementById(ids.email).value,
        }
        console.log(data);
        $.ajax({
            url: '/CreateLead/createPerson',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(data),
            success: function (response) {
                console.log(response);
                if (response.success) {
                    alert("Saved Successfully");
                    console.log(response.success);
                }
            },
            error: function (xhr) {
                console.log(xhr);
                alert('Error saving person: ' + xhr.responseText);
            }
        });
    });



    //document.addEventListener("DOMContentLoaded", function () {
    //    const phoneIds = ["#phone", "#phone1", "#phone2", "#phone3", "#phone4"];

    //    phoneIds.forEach(selector => {
    //        const input = document.querySelector(selector);
    //        if (!input) return;

    //        const iti = window.intlTelInput(input, {
    //            utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js",
    //            separateDialCode: true,
    //            initialCountry: "bd",
    //            preferredCountries: ["bd", "in", "us"]
    //        });

    //        input.addEventListener("blur", function () {
    //            console.log(`${selector} Phone:`, iti.getNumber());
    //        });
    //    });
    //});


    //document.addEventListener("DOMContentLoaded", function () {
    //    const phoneIds = ["#phone", "#phone1", "#phone2", "#phone3", "#phone4"];

    //    phoneIds.forEach(selector => {
    //        const input = document.querySelector(selector);
    //        if (!input) return;

    //        const iti = window.intlTelInput(input, {
    //            utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js",
    //            separateDialCode: true,
    //            initialCountry: "bd",
    //            preferredCountries: ["bd", "in", "us"]
    //        });

    //        input.addEventListener("blur", function () {
    //            console.log(`${selector} Phone:`, iti.getNumber());
    //        });
    //    });
    //});
    // phone field value insert
    //let itiMap = {};
    //document.addEventListener("DOMContentLoaded", function () {
    //    debugger;
    //    console.log("phone running");
    //    const phoneIds = ["#phone", "#phone1", "#phone2", "#phone3", "#phone4"];
    //    debugger;
    //    phoneIds.forEach(selector => {
    //        const input = document.querySelector(selector);
    //        if (!input) return;

    //        const iti = window.intlTelInput(input, {
    //            utilsScript: "https://cdn.jsdelivr.net/npm/intl-tel-input@25.3.1/build/js/utils.js",
    //            separateDialCode: true,
    //            initialCountry: "bd",
    //            preferredCountries: ["bd", "in", "us"]
    //        });
    //        itiMap[selector] = iti;
    //        let phoneInit = itiMap["#phone"];
    //        input.addEventListener("blur", function () {
    //            console.log(`${selector} Phone:`, phoneInit.getNumber());
    //        });
    //    });

    //    console.log("Phone fields initialized", itiMap);
    //});

    // submit button
    //$("#submitBtn").on('click', function (e) {
    //    debugger
    //    e.preventDefault();
    //    console.log("Phone fields initialized", itiMap);

    //    const iti = itiMap["#phone"];
    //    if (iti) {
    //        const phoneNumber = iti.getNumber();
    //        console.log(`Phone Number: ${phoneNumber}`);
    //    }
        
    //});


//#endregion

//#region Save Nationality

$('#confirmAddNationalityBtn').on('click', function () {
    const newNationality = $('#newNationalityName').val().trim();

    if (!newNationality) {
        alert('Please enter a nationality name.');
        return;
    }

    $.ajax({
        url: '/CreateLead/SaveNationality', // <-- Update with your actual route
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(newNationality),
        success: function (response) {
            if (response.success) {
                nationalities.push(newNationality);
                $('#ContactNameSearch').val(newNationality);
                $('#searchResults').hide();
                $('#removeContactNameBtn').show();
                $('#addNationalityModal').modal('hide');
            }
        },
        error: function (xhr) {
            alert('Error saving nationality: ' + xhr.responseText);
        }
    });
});


    //#endregion 

});