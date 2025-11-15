//#region ids
const idMapIndex = {
    demoField: {
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
//#endregion

$(function () {
    //#region customer serach field
    $('#CustomerId').select2({
        placeholder: 'Select Customer',
        width: '100%',
        ajax: {
            url: '/CreateJobs/GetCustomers',
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
        language: {
            noResults: function () {
                return $(
                    `<span>Data not found. Create a <a id="createCustomer" href="#">Customer</a></span>`
                );
            }
        },
        
        width: '100%'
    });
    //#endregion

    //#region PriorityID
    $('#PriorityID').select2({
        placeholder: 'Select Customer',
        width: '100%',
        ajax: {
            url: '/CreateLead/GetPriorityList',
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
                    results: data.items.map(item => ({
                        id: item.value,
                        text: item.label
                    })),
                    pagination: {
                        more: data.hasMore
                    }
                };
            },
            cache: true
        },
        
        width: '100%'
    });
    //#endregion
    //#region LeadStatusID
    $('#LeadStatusID').select2({
        placeholder: 'Select Customer',
        width: '100%',
        ajax: {
            url: '/CreateLead/GetLeadStatusList',
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
                    results: data.items.map(item => ({
                        id: item.value,
                        text: item.label
                    })),
                    pagination: {
                        more: data.hasMore
                    }
                };
            },
            cache: true
        },
        
        width: '100%'
    });
    //#endregion

    //#region LeadSourceID
    $('#LeadSourceID').select2({
        placeholder: 'Select Lead Source',
        width: '100%',
        ajax: {
            url: '/CreateLead/GetLeadSourceList',
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
                    results: data.items.map(item => ({
                        id: item.value,
                        text: item.label
                    })),
                    pagination: {
                        more: data.hasMore
                    }
                };
            },
            cache: true
        },
        
        width: '100%'
    });
    //#endregion

    //#region GetServiceList
    $('#ServiceTypeIds').select2({
        placeholder: 'Select Service Item',
        width: '100%',
        ajax: {
            url: '/CreateLead/GetServiceList',
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
                    results: data.items.map(item => ({
                        id: item.value,
                        text: item.label
                    })),
                    pagination: {
                        more: data.hasMore
                    }
                };
            },
            cache: true
        },
        
        width: '100%'
    });
    //#endregion

    //#region get Lead Owner
    $('#LeadOwnerID').select2({
        placeholder: 'Select Lead Owner',
        width: '100%',
        ajax: {
            url: '/CreateLead/GetLeadOwnerList',
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
                    results: data.items.map(item => ({
                        id: item.value,
                        text: item.label
                    })),
                    pagination: {
                        more: data.hasMore
                    }
                };
            },
            cache: true
        }
    });
    //#endregion

    //#region set Customer Info
    function setDataDesktop(id) {

        $('#customerList').hide();
        $('#noResults').hide();

        getCustomerInfo(id).then(response => {
            document.getElementById("customerInfoContainer").style.display = "block";
            console.log(response);
            let title = "";
            if (response.customer.addressTypeName === "billing") {
                title = "Person Information"
            } else if (response.customer.addressTypeName === "company") {
                title = "Company Information"
            }

            $("#iInformationTitle").text(title);
            $('#ContactNameSearch').val(response.customer.fullName);
            $("#customerID").val(response.customer.customerAddressID);
            const ids = idMapIndex.demoField;
           
            $("#" + ids.firstName).val(response.customer.firstName);
            $("#" + ids.lastName).val(response.customer.lastName);
            $("#" + ids.fullAddress).val(response.customer.fullAddress);
            $("#" + ids.street).val(response.customer.street);
            $("#" + ids.city).val(response.customer.city);
            $("#" + ids.additionalAddress).val(response.customer.additionaladdress);
            $("#" + ids.state).val(response.customer.state);
            $("#" + ids.postalCode).val(response.customer.postalCode);
            choiceManager.setChoiceValue(ids.countryID, response.customer.countryID);
            $("#" + ids.countryCode).val(response.customer.countryCode);
            $("#" + ids.latitude).val(response.customer.latitude);
            $("#" + ids.longitude).val(response.customer.longitude);
            $("#" + ids.phone).val(response.customer.phone);
            $("#" + ids.otherPhone).val(response.customer.otherPhone);
            $("#" + ids.email).val(response.customer.email);

            exExruntimeValidationCheck('#ContactNameSearch');
            $('#searchResults').hide();
            $('#removeContactNameBtn').show();
        });
    }

    //#endregion
    const fields = ["CustomerId", "LeadName", "LeadSourceID", "LeadStatusID", "PriorityID", "LeadOwnerID"];
    //#region create Lead
    $("#indexSaveBtn").on("click", function (e) {
        e.preventDefault();
        let services = $("#serviceTypes").val();

        if (validateFields(fields)) {
            debugger
            const form = this.closest("form");
            if (!form) return;
            const formData = new FormData(form);
            const jsonData = {};
            formData.forEach((value, key) => {
                // Optional: Convert empty string to null for numbers

                jsonData[key] = value === "" ? null : value;
            });
            console.log(jsonData)
            showDev(formData);
            $.ajax({
                url: '/CreateLead/CreateLeadData',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(jsonData),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        document.getElementById("leadForm").reset();
                        window.location.href = "/crm/Index";
                    } else {
                        toastr.error(response.message || "Failed to create lead");
                    }
                },
                error: function (xhr, status, error) {
                    if (xhr.status === 403 && xhr.responseJSON) {
                        toastr.error(xhr.responseJSON.message || "Access denied.", 'Permission Denied');
                    } else {
                        toastr.error("Unexpected error: " + error, 'Server Error');

                    }
                }
            });
        } else {

        }
    });
    //#endregion

    // When you load modal via AJAX
    $(document).on("click", "#createCustomer", function () {
        $.get('/Customers/IndexModal', function (html) {
            $('#customerModalContent').html(html);

            // Initialize newly added modal elements
            $('#customerModalContent [data-init]').each(function () {
                const el = this;
                const key = el.dataset.init;
                if (key && typeof window[key] === "function") {
                    window[key](el);
                    el.dataset.initialized = true; // optional flag
                }
            });

            // Show modal
            var modal = new bootstrap.Modal(document.getElementById('customerModal'));
            modal.show();
        });
    });



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
                        select2Box.style.borderColor = "red !important";
                    }
                } else {
                    field.classList.add("is-invalid");
                    field.style.border = "2px solid red !important";
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
});

