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

function initCreateLeadModal() {
    //#region customer serach field
    $('#CustomerId2').select2({
        placeholder: 'Select Customer',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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
                    `<span>
                    Data not found. 
                    <a href="#" id="openCustomerModal" class="text-primary">Create Customer</a>
                </span>`
                );
            }
        }
    });



    //#region customer serach field
    $('#BranchId2').select2({
        placeholder: 'Select Customer',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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

    $('#CustomerId2').on('change', function () {
        var customerId = $(this).val();
        setCustomerDetails(customerId);
    });
    function setCustomerDetails(customerId) {
        $.ajax({
            url: '/Customers/GetCustomerInfo',
            type: 'POST',
            data: { id: customerId },
            dataType: 'json',
            success: function (response) {
                viewCustomerInfo(response)
                //showDev(response)
                if (!response.isIndividual) {
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
    $('#PriorityID2').select2({
        placeholder: 'Select Customer',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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
    $('#LeadStatusID2').select2({
        placeholder: 'Select Customer',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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
    $('#LeadSourceID2').select2({
        placeholder: 'Select Lead Source',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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
    $('#ServiceTypeIds2').select2({
        placeholder: 'Select Service Item',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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
    $('#LeadOwnerID2').select2({
        placeholder: 'Select Lead Owner',
        width: '100%',
        dropdownParent: $('#createLeadModalToggle'),
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
};

window.closeWindow = function () {
    const modalEl = document.getElementById('customerModal');
    const modal = bootstrap.Modal.getInstance(modalEl); // get existing instance
    if (modal) modal.hide();
};
