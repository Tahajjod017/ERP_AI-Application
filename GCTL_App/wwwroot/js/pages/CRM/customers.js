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
}

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