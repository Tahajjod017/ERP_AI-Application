//const { ajax } = require("jquery");

let itiMap = {};

document.addEventListener("DOMContentLoaded", function () {

    const phoneIds = ["#phone", "#phone1", "#phone2", "#phone3", "#phone4", "#phone5", "#phone6", "#phonePersonIndex", "#otherPhonePersonIndex", "#phone5Index", "#phone6Index"];

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

    let customers = [];



    function getCustomerList() {
        $.ajax({
            url: '/CreateLead/GetCustomerList',
            method: 'GET',
            success: function (data) {
                customers = data;
                console.log(customers);
            },
            error: function (e) {
                alert('Failed to load Contact Name' + e.text);
            }
        });

    }

    // run getCustomerList funciton intialization
    getCustomerList();

    // getCustomerInformation while click on any customer item
    customerInfoContainer = $("#customerInfoContainer");
    customerInfoContainerHtml = `
                <h5 class="mb-2 ms-2 text-body-highlight my-3 fs-8">Custommer Information</h5>
                <div class="border-top pb-0 mb-3"></div>
                <div class="row p-0">
                    <div class="col-md-6 col-sm-12">
                        <div class="row">
                            <div>
                                <input type="number" id="personIndexIA_ID" hidden>
                                <input type="text" name="name" value="" id="countryCodePersonIndex" hidden />
                            </div>
                            <div class="col-md-12">
                                <div class="mb-3 form-floating">
                                    <input type="text" id="autocompletePersonIndex" class="form-control" placeholder="Customer name" required>
                                    <label class="form-label req">Full Address</label>
                                    <span class="text-danger small"></span>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-6 pe-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="streetPersonIndex" class="form-control" placeholder="Street" required>
                                            <label class="form-label req">Street</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                    <div class="col-md-6 ps-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="cityPersonIndex" class="form-control" placeholder="City" required>
                                            <label class="form-label req">City</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-6 pe-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="additionalAddressPersonIndex" class="form-control" placeholder="" required>
                                            <label class="form-label req">Additional Address</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                    <div class="col-md-6 ps-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="statePersonIndex" class="form-control" placeholder="" required>
                                            <label class="form-label req">State/Province</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-6 pe-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input id="postalCodePersonIndex" type="text" class="form-control" placeholder="" required>
                                            <label class="form-label req">Zip/Postal Code</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                    <div class="col-md-6 ps-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input id="countryPersonIndex" type="text" class="form-control" placeholder="" required>
                                            <label class="form-label req">Country</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-6 pe-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="latitudePersonIndex" class="form-control" placeholder="" required>
                                            <label class="form-label req">Latitude</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                    <div class="col-md-6 ps-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="longitudePersonIndex" class="form-control" placeholder="" required>
                                            <label class="form-label req">Longitude</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6 col-sm-12">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-6 pe-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="firstNamePersonIndex" class="form-control" placeholder="" required>
                                            <label class="form-label req">First Name</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                    <div class="col-md-6 ps-1 col-sm-6">
                                        <div class="mb-3 form-floating">
                                            <input type="text" id="lastNamePersonIndex" class="form-control" placeholder="" required>
                                            <label class="form-label req">Last Name</label>
                                            <span class="text-danger small"></span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <div class="col-12 mb-3">
                                <input class="form-control" id="phonePersonIndex" type="tel" placeholder="Phone Number" />
                            </div>
                            <div class="col-12 mb-3">
                                <input class="form-control" id="otherPhonePersonIndex" type="tel" placeholder="Other Phone" />
                            </div>

                            <div class="col-12">
                                <div class="mb-3 form-floating">
                                    <input type="text" id="emailPersonIndex" class="form-control" placeholder="" required>
                                    <label class="form-label req">Email</label>
                                    <span class="text-danger small"></span>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
    `

    // later show it
    //customerInfoContainer.show();
    function getCustomerInfo(customerId) {
        console.log(`customerId: ${customerId}`);
        $.ajax({
            url: '/CreateLead/GetCustomerInfo',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(customerId),
            success: function (response) {
                console.log(response);
                
                $(document).ready(function () {
                    customerInfoContainer.empty();
                    customerInfoContainer.append(customerInfoContainerHtml);
                });

                setTimeout(() => {
                    $('#ContactNameSearch').val(response.customer.firstName + " " + response.customer.lastName);
                    $('#customerID').val(response.customer.individualAddressID);
                    $('#customerType').val(response.customer.addressTypeName);

                    document.getElementById("personIndexIA_ID").value = response.customer.individualAddressID;
                    document.getElementById("firstNamePersonIndex").value = response.customer.firstName;
                    document.getElementById("lastNamePersonIndex").value = response.customer.lastName;
                    document.getElementById("autocompletePersonIndex").value = response.customer.fullAddress;
                    document.getElementById("streetPersonIndex").value = response.customer.street;
                    document.getElementById("cityPersonIndex").value = response.customer.city;
                    document.getElementById("additionalAddressPersonIndex").value = response.customer.additionaladdress;
                    document.getElementById("statePersonIndex").value = response.customer.state;
                    document.getElementById("postalCodePersonIndex").value = response.customer.postalCode;
                    document.getElementById("countryPersonIndex").value = response.customer.countryName;
                    document.getElementById("countryCodePersonIndex").value = response.customer.countryCode;
                    document.getElementById("latitudePersonIndex").value = response.customer.latitude;
                    document.getElementById("longitudePersonIndex").value = response.customer.longitude;
                    document.getElementById("phonePersonIndex").value = response.customer.phone;
                    document.getElementById("otherPhonePersonIndex").value = response.customer.otherPhone;
                    document.getElementById("emailPersonIndex").value = response.customer.email;
                }, 300);
                




                //// shipping
                //if (response.shipping.firstName) {
                //    document.getElementById("shippingIndexIA_ID").value = response.shipping.individualAddressID;
                //    document.getElementById("firstNameShippingIndex").value = response.shipping.firstName;
                //    document.getElementById("lastNameShippingIndex").value = response.shipping.lastName;
                //    document.getElementById("autocompleteShippingIndex").value = response.shipping.fullAddress;
                //    document.getElementById("streetShippingIndex").value = response.shipping.street;
                //    document.getElementById("cityShippingIndex").value = response.shipping.city;
                //    document.getElementById("additionalAddressShippingIndex").value = response.shipping.additionaladdress;
                //    document.getElementById("stateShippingIndex").value = response.shipping.state;
                //    document.getElementById("postalCodeShippingIndex").value = response.shipping.postalCode;
                //    document.getElementById("countryShippingIndex").value = response.shipping.countryName;
                //    document.getElementById("countryCodeShipingIndex").value = response.shipping.countryCode;
                //    document.getElementById("latitudeShippingIndex").value = response.shipping.latitude;
                //    document.getElementById("longitudeShippingIndex").value = response.shipping.longitude;
                //    document.getElementById("phone5Index").value = response.shipping.phone;
                //    document.getElementById("phone6Index").value = response.shipping.otherPhone;
                //    document.getElementById("emailShippingIndex").value = response.shipping.email;

                //}
                //check validataion again
                
            },
            error: function () {
                alert('Failed to load Contact Name');
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

        const filtered = customers.filter(item =>
            item.fullName.toLowerCase().includes(query.toLowerCase())
        );

        if (filtered.length > 0) {
            $list.show();
            filtered.forEach(item => {
                $list.append(`<button type="button" class="list-group-item list-group-item-action customerName-item" data-id="${item.customerId}" data-type="${item.type}">${item.fullName}</button>`);
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
        console.log(customerId);
        $('#ContactNameSearch').val(selected);
        $('#customerID').val(customerId);
        $('#customerType').val(customerType);
        getCustomerInfo(customerId);
        //setTimeout(() => {
        //    fieldValidation();
        //}, 300)
        
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
    let targetTab = 'index';
    console.log(targetTab);

    $('#addNewContactNameBtn').on('click', function () {
        targetTab = 'company';
        $('#addCustomerModal').modal('show');
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

        console.log(targetTab);
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

        console.log(targetTab);
    });

    $('#addNewContactNameBtn2').on('click', function () {
        targetTab = 'person';
        $('#addCustomerModal').modal('show');
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


        console.log(targetTab);
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
    //shipping tab code
    $("#shippingAddressTab").on("click", function () {
        targetTab = 'shipping';
        setTimeout(() => {
            initAutocomplete();
        }, 300);
        console.log(targetTab);
    });

    $("#closeModal").on('click', function () {
        $('#addCustomerModal').modal('hide');
        targetTab = "index";
        console.log(targetTab);
    });
    $("#closeModal2").on('click', function () {
        $('#addCustomerModal').modal('hide');
        targetTab = "index";
        console.log(targetTab);
    });
    let autocomplete;
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
            additionalAddress: 'additionalAddressPerson',
            country: 'countryPerson',
            countryCode: 'countryCodePerson',
            postal_code: 'postalCodePerson',
            latitude: 'latitudePerson',
            longitude: 'longitudePerson',
            phone: 'phone3',
            otherPhone: 'phone4',
            email: 'emailPerson',
        },
        shipping: {
            primaryID: 'shipingID',
            firstName: 'firstNameShiping',
            lastName: 'lastNameShipping',
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


    const idMapIndex = {

        indexBase: {
            customerName: 'ContactNameSearch',
            leadName: 'leadName',
            leadStatusID: 'leadStatusId',
            leadSourceID: 'leadSourceId',
            leadOwnerID: 'leadOwnerId',
            approximateDealValue: 'approximateDealValue',
            probabilityPercentage: 'probabilityPercentage',
            leadDescription: 'descriptionText',
        },
        company: {
            primaryID: 'companyID',
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
        // index person
        person: {
            primaryID: 'personIndexIA_ID',
            primaryType: 'customerType',
            firstName: 'firstNamePersonIndex',
            lastName: 'lastNamePersonIndex',
            autocomplete: 'autocompletePersonIndex', // full address
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
            email: 'emailPersonIndex',
        },
        shipping: {
            primaryID: 'shippingIndexIA_ID',
            firstName: 'firstNameShippingIndex',
            lastName: 'lastNameShippingIndex',
            autocomplete: 'autocompleteShippingIndex', // full address
            street: 'streetShippingIndex',
            city: 'cityShippingIndex',
            state: 'stateShippingIndex',
            additionalAddress: 'additionalAddressShippingIndex',
            country: 'countryShippingIndex',
            countryCode: 'countryCodeShipingIndex',
            postal_code: 'postalCodeShippingIndex',
            latitude: 'latitudeShippingIndex',
            longitude: 'longitudeShippingIndex',
            phone: 'phone5Index',
            otherPhone: 'phone6Index',
            email: 'emailShippingIndex',
        }
    };
    function initAutocomplete() {
        debugger;
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
            if (place.address_components.component) {
            }
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
            //setCountry(ids.country, country);
            debugger;
            //document.getElementById(ids.country).value = country;
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
            method: 'get',
            contentType: 'application/json',
            data: { countryName : countryName },
            success: function (response) {
                debugger;
                showDev(response, 'dd')

                choiceManager.setChoiceValue(id, response.countryId)

            },
            error: function (xhr) {
                console.log(xhr);
                showDev(xhr)
                alert('Error saving person');
            }
        });
    }


    //$(document).on('shown.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
    //    const newTab = $(e.target).data('bs-target'); // e.g. "#company" or "#person"
    //    if (newTab === '#company') {
    //        targetTab = 'company'
    //    } else if (newTab === '#person') {
    //        targetTab = 'person';
    //    }
    //    console.log('Activated tab:', newTab);
    //    setTimeout(() => {
    //        initAutocomplete();
    //    }, 300);
    //    console.log(targetTab);
    //});

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


    function modalValidation(item) {
        debugger;
        let validationStatus = true;
        const ids = idMap[item] || {};
        if (item == "shipping") {
            validationStatus = $(`#${ids.firstName}`).val() === "" || ids.lastName === "" ? false : true;
        }
        return validationStatus;
    }
    // save data
    $("#modalSaveBtn").on("click", function (e) {
        debugger;
        e.preventDefault();
        
        if (fieldValidation()) {
            const actionTab =
                (targetTab === "person" || targetTab === "shipping") ? ["person", "shipping"] : ["company"];
            //console.log(ids.phone);
            var data = []


            actionTab.forEach(item => {
                if (modalValidation(item)) {
                    const ids = idMap[item] || {};
                    data.push({
                        TabName: item,
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
                        CountryName: document.getElementById(ids.country).value,
                        CountryCode: document.getElementById(ids.countryCode).value,
                        Latitude: parseFloat(document.getElementById(ids.latitude).value) || null,
                        Longitude: parseFloat(document.getElementById(ids.longitude).value) || null,
                        Phone: getPhoneNumber(`#${ids.phone}`),
                        OtherPhone: getPhoneNumber(`#${ids.otherPhone}`),
                        Email: document.getElementById(ids.email).value,
                    });
                }
               
            });
            var dataToSend = { Customers: data };

            console.log(data);
            $.ajax({
                url: '/CreateLead/upsertPerson',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(dataToSend),
                success: function (response) {
                    debugger;
                    console.log(response);
                    if (response.success) {
                        $('#addCustomerModal').modal('hide');
                        toastr.success(response.message);
                        getCustomerList();
                        getCustomerInfo(response.result.data);
                    }
                },
                error: function (xhr) {
                    console.log(xhr);
                    alert('Error saving person');
                }
            });
        };

    });

    $("#indexSaveBtn").on("click", function (e) {
        e.preventDefault();
        debugger;
        console.log(document.getElementById("personIndexIA_ID").value);
        if (fieldValidation()) {
            const actionTab =
                (targetTab === "index") ? ["person"] : ["company"];
            //console.log(ids.phone);
            var data = {
                IsIndividualCustomer: document.getElementById("customerType").value === "billing" ? true : false,
                LeadStatusID: document.getElementById(idMapIndex.indexBase.leadStatusID).value,
                LeadSourceID: document.getElementById(idMapIndex.indexBase.leadSourceID).value,
                LeadOwnerID: document.getElementById(idMapIndex.indexBase.leadOwnerID).value,
                ApproximateDealValue: document.getElementById(idMapIndex.indexBase.approximateDealValue).value
                    ? parseFloat(document.getElementById(idMapIndex.indexBase.approximateDealValue).value)
                    : 0,
                ProbabilityPercentage: document.getElementById(idMapIndex.indexBase.probabilityPercentage).value
                    ? parseFloat(document.getElementById(idMapIndex.indexBase.probabilityPercentage).value)
                    : 0,
                LeadDescription: document.getElementById("descriptionText").value,
                Customers: []
            };

            actionTab.forEach(item => {
                const ids = idMapIndex[item] || {};
                $(".customerName-item").data("id");
                data.Customers.push({
                    TabName: item,
                    PrimaryID: document.getElementById(ids.primaryID).value,
                    FirstName: document.getElementById(ids.firstName).value,
                    LastName: document.getElementById(ids.lastName).value,
                    FullAddress: document.getElementById(ids.autocomplete).value,
                    Street: document.getElementById(ids.street).value,
                    City: document.getElementById(ids.city).value,
                    State: document.getElementById(ids.state).value,
                    Additionaladdress: document.getElementById(ids.additionalAddress).value,
                    PostalCode: document.getElementById(ids.postal_code).value,
                    CountryName: document.getElementById(ids.country).value,
                    CountryCode: document.getElementById(ids.countryCode).value,
                    Latitude: parseFloat(document.getElementById(ids.latitude).value) || null,
                    Longitude: parseFloat(document.getElementById(ids.longitude).value) || null,
                    Phone: getPhoneNumber(`#${ids.phone}`),
                    OtherPhone: getPhoneNumber(`#${ids.otherPhone}`),
                    Email: document.getElementById(ids.email).value,
                });
            });
            var dataToSend = { Customers: data };

            console.log(data);
            $.ajax({
                url: '/CreateLead/CreateLead',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
                success: function (response) {
                    console.log(response);
                    if (response.success) {
                        $('#addCustomerModal').modal('hide');
                        toastr.success(response.message);
                    }
                },
                error: function (xhr) {
                    console.log(xhr);
                    alert('Error saving person');
                }
            });
        }
    });

    function targetListForValidation() {
        if (targetTab === 'person' || targetTab === 'shipping') {
            let ids = idMap.shipping;
            let list = [];

            // Check if shipping fields are empty
            if ($(`#${ids.firstName}`).val() === "" && $(`#${ids.lastName}`).val() === "") {
                list = [
                    idMap.person.firstName,
                    idMap.person.lastName,
                    idMap.person.phone
                ];
                let removeBorderItemList = [
                    idMap.shipping.firstName,
                    idMap.shipping.lastName,
                    idMap.shipping.phone]
                removeBorderItemList.forEach(e => removeValidationOne(`#${e}`));
            } else {
                list = [
                    idMap.person.firstName,
                    idMap.person.lastName,
                    idMap.person.phone,
                    idMap.shipping.firstName,
                    idMap.shipping.lastName,
                    idMap.shipping.phone
                ];
            }

            return list;
        }
        else if (targetTab === "company") {
            return [idMap.company.firstName];
        }
        else if (targetTab === "index") {
            return [
                idMapIndex.indexBase.customerName,
                idMapIndex.indexBase.leadName,
                idMapIndex.indexBase.leadStatusID,
                idMapIndex.indexBase.leadSourceID,
                idMapIndex.indexBase.leadOwnerID,

                idMapIndex.person.firstName,
                idMapIndex.person.lastName,
                idMapIndex.person.autocomplete,
                idMapIndex.person.postal_code,
                idMapIndex.person.country,
                idMapIndex.person.phone,
                idMapIndex.person.email,
            ];
        }
        else {
            return [];
        }
    }


    // runtime validation check
    function runtimeValidationCheck() {
        const selectedTabList = targetListForValidation();

        selectedTabList
            .filter(Boolean)
            .forEach(e => {
                $(document).on('input change', `#${e}`, function () {
                    fieldValidationOne(this);
                    console.log("Validation triggered");
                });
            });
    }

    //function validateNamevalidateNameOne(obj) {
    //    var name = $(obj).val().trim();

    //    if (name === '') {
    //        $(obj).css('border', '1px solid red');
    //    } else {
    //        $(obj).css('border', '1px solid #ccc');
    //    }
    //}


    // check validation when click on submit btn
    function fieldValidation() {
        
        const selectedTab = targetListForValidation();

        let isValid = true;
        const errorMessages = [];
        let errorCount = 0;
        runtimeValidationCheck();
        selectedTab.filter(Boolean).forEach(e => {
            let obj = $(`#${e}`);
            const name = obj.val().trim();
            let target;

            if (obj.closest('.choices').length > 0) {
                target = obj.closest('.choices').find('.choices__inner');
            } else {
                target = obj;
            }

            console.log(`Current Tab Value: ${name}`);
            if (name === '') {
                target.css('border', '1px solid red');
                errorCount += 1;
                isValid = false;
            } else {
                target.css('border', '1px solid #ccc');
            }
        });

        if (errorCount > 0) {
            if (errorCount == 1) {
                toastr.warning("This field is required");
                
                
            }
            else if (errorCount > 1) {
                toastr.warning("These fields are required");
            }
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


    // //check phoneNumber is unique or not
    //function checkNameUnique() {
    //    debugger;
    //    const selectedTab = targetTab === 'person' || targetTab === 'shipping'
    //        ? [
    //            {
    //                phone: idMap.person.phone,
    //                phoneWarningDivID: "phonePersonError",
    //                otherPhone: idMap.person.otherPhone,
    //                otherPhoneWarningDivID: "otherPhonePersonError"
    //            },
    //            {
    //                phone: idMap.shipping.phone,
    //                otherPhone: idMap.shipping.otherPhone
    //            }
    //        ]
    //        : [
    //            {
    //                phone: idMap.company.phone
    //            }
    //        ];

    //    selectedTab.forEach(ids => {
    //        // Attach input listener to phone
    //        $(document).on('input', `#${ids.phone}`, function () {
    //            let phoneVal = $(`#${ids.phone}`).val();
    //            let otherPhoneVal = $(`#${ids.otherPhone}`).val();

    //            if (phoneVal && otherPhoneVal && phoneVal === otherPhoneVal) {
    //                $(`#${ids.phoneWarningDivID}`).text("Phone and Other Phone fields cannot be the same");
    //                toastr.warning("Phone and Other Phone fields cannot be the same");
    //            } else {
    //                $(`#${ids.phoneWarningDivID}`).text("");
    //            }
    //        });

    //        // Attach input listener to other phone
    //        $(document).on('input', `#${ids.otherPhone}`, function () {
    //            let phoneVal = $(`#${ids.phone}`).val();
    //            let otherPhoneVal = $(`#${ids.otherPhone}`).val();

    //            if (phoneVal && otherPhoneVal && otherPhoneVal === phoneVal) {
    //                $(`#${ids.otherPhoneWarningDivID}`).text("Phone and Other Phone fields cannot be the same");
    //                toastr.warning("Phone and Other Phone fields cannot be the same");
    //            } else {
    //                $(`#${ids.otherPhoneWarningDivID}`).text("");
    //            }
    //        });
    //    });
    //}






    //$('#confirmAddNationalityBtn').on('click', function () {
    //    const newNationality = $('#newNationalityName').val().trim();

    //    if (!newNationality) {
    //        alert('Please enter a nationality name.');
    //        return;
    //    }

    //    $.ajax({
    //        url: '/CreateLead/SaveNationality', // <-- Update with your actual route
    //        method: 'POST',
    //        contentType: 'application/json',
    //        data: JSON.stringify(newNationality),
    //        success: function (response) {
    //            if (response.success) {
    //                customers.push(newNationality);
    //                $('#ContactNameSearch').val(newNationality);
    //                $('#searchResults').hide();
    //                $('#removeContactNameBtn').show();
    //                $('#addCustomerModal').modal('hide');
    //            }
    //        },
    //        error: function (xhr) {
    //            alert('Error saving nationality: ' + xhr.responseText);
    //        }
    //    });
    //});


    //#endregion 

});