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


    //#region customer serach field
    $('#BranchId').select2({
        placeholder: 'Select Customer',
        width: '100%',
        ajax: {
            url: '/Customers/GetBranches',
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return {
                    customerID: $("#CustomerId").val() ?? 0,
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

    $('#CustomerId').on('change', function () {
        // Get the selected value (customer ID)
        var customerId = $(this).val();
        setCustomerDetails(customerId);
        // You can use this ID for further AJAX calls or processing
    });
    function setCustomerDetails(customerId) {
        $.ajax({
            url: '/Customers/GetCustomerInfo', // replace with your endpoint
            type: 'POST',
            data: { id: customerId },
            dataType: 'json',
            success: function (response) {
                viewCustomerInfo(response)
                //showDev(response)
                if (!response.isIndividual)
                    { 
                        $("#BranchId").val(null).trigger('change');
                        $('#branchContainer').show();
                    }
                else
                    $('#branchContainer').hide();


                // do something with response
            },
            error: function (xhr, status, error) {
                console.error('Error:', error);
            }
        });
    }


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
    function viewCustomerInfo(response) {
        showDev(response)
        document.getElementById("customerInfoContainer").style.display = "block";
        let title = "Customer Info";
        //if (response.customer.addressTypeName === "billing") {
        //    title = "Person Information"
        //} else if (response.customer.addressTypeName === "company") {
        //    title = "Company Information"
        //}
        $("#informationTitle").text(title);
           
        $("#iFirstName").val(response.firstName);
        $("#iLastName").val(response.lastName);
        $("#iFullAddress").val(response.fullAddress);
        $("#iStreet").val(response.street);
        $("#iCity").val(response.city);
        $("#iAdditionaladdres").val(response.additionaladdres);
        $("#iState").val(response.state);
        $("#iPostalCode").val(response.postalCode);
            //choiceManager.setChoiceValue(ids.countryID, response.customer.countryID);
        $("#iLatitude").val(response.latitude);
        $("#iLongitude").val(response.longitude);
        $("#iPhone").val(response.phone);
        $("#iOtherPhone").val(response.otherPhone);
        $("#iEmail").val(response.email);
    }

    //#endregion
    const fields = ["CustomerId", "LeadName", "LeadSourceID", "LeadStatusID", "PriorityID", "LeadOwnerID"];
    //#region create Lead
    $("#indexSaveBtn").on("click", function (e) {
        e.preventDefault();

        if (validateFields(fields)) {
            const form = document.getElementById("leadForm");
            const token = document.querySelector("input[name='__RequestVerificationToken']").value;

            const formData = new FormData(form);
            const jsonData = {};

            formData.forEach((value, key) => {
                if (key === "__RequestVerificationToken") return; // skip token

                // convert numbers
                if (["CustomerId", "LeadStatusID", "LeadSourceID", "LeadOwnerID", "PriorityID"].includes(key)) {
                    value = parseInt(value) || 0;
                }
                if (["ApproximateDealValue", "ProbabilityPercentage"].includes(key)) {
                    value = parseFloat(value) || 0;
                }

                // handle multi-select ServiceTypeIds
                if (key === "ServiceTypeIds") {
                    if (!jsonData[key]) jsonData[key] = [];
                    jsonData[key].push(parseInt(value));
                    return;
                }

                jsonData[key] = value === "" ? null : value;
            });

            $.ajax({
                url: '/CreateLead/CreateLeadData',
                method: 'POST',
                contentType: "application/json; charset=utf-8",
                headers: {
                    "RequestVerificationToken": token
                },
                data: JSON.stringify(jsonData),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        form.reset();
                        window.location.href = `/LeadDetails/Index/${response.data.id}`;
                    } else {
                        toastr.error(response.message || "Failed to create lead");
                    }
                }
            });
        }
    });
    //#endregion

    // initial customer
    window.selectCustomer = (id, text) => {
        debugger;
        debugger;
        const $select = $('#CustomerId');
        const newOption = new Option(text, id, true, true);
        $select.append(newOption).trigger('change');
        $select.select2('close');
        setCustomerDetails(id);
    };

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
                if (typeof showClose == "function") {
                    showClose();
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
                // field invalid
                if (isSelect2) {
                    const select2Box = field.nextElementSibling?.querySelector(".select2-selection");
                    if (select2Box) {
                        select2Box.classList.remove("is-valid");
                        select2Box.classList.add("is-invalid");
                    }
                } else {
                    field.classList.add("is-invalid");
                }
            } else {
                // field valid
                if (isSelect2) {
                    const select2Box = field.nextElementSibling?.querySelector(".select2-selection");
                    if (select2Box) {
                        select2Box.classList.remove("is-invalid");
                    }
                } else {
                    field.classList.remove("is-invalid");
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

window.closeWindow = function () {
    const modalEl = document.getElementById('customerModal');
    const modal = bootstrap.Modal.getInstance(modalEl); // get existing instance
    if (modal) modal.hide();
};
