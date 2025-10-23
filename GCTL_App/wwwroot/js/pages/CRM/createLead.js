//#region ids
const idMapIndex = {
    indexBase: {
        customerName: 'ContactNameSearch',
        customerID: 'customerID',
        leadName: 'leadName',
        leadStatusID: 'leadStatusId',
        leadSourceID: 'leadSourceId',
        leadOwnerID: 'leadOwnerId',
        priorityID: 'priorityID',
        approximateDealValue: 'approximateDealValue',
        probabilityPercentage: 'probabilityPercentage',
        leadDescription: 'descriptionText',
        ServiceTypeIds: 'serviceTypes'
    },
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
    $('#customerID').select2({
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

    //#region get Lead Owner
    $('#leadOwnerId').select2({
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

    //#region create Lead
    $("#indexSaveBtn").on("click", async function (e) {
        e.preventDefault();
        let services = $("#serviceTypes").val();

        if (await fieldValidation()) {

            const data = {
                LeadName: $("#" + idMapIndex.indexBase.leadName).val() || "",
                LeadStatusID: parseInt($("#" + idMapIndex.indexBase.leadStatusID).val()) || 0,
                LeadSourceID: parseInt($("#" + idMapIndex.indexBase.leadSourceID).val()) || 0,
                LeadOwnerID: parseInt($("#" + idMapIndex.indexBase.leadOwnerID).val()) || 0,
                PriorityID: parseInt($("#" + idMapIndex.indexBase.priorityID).val()) || 0,
                ApproximateDealValue: parseFloat($("#" + idMapIndex.indexBase.approximateDealValue).val()) || 0,
                ProbabilityPercentage: parseFloat($("#" + idMapIndex.indexBase.probabilityPercentage).val()) || 0,
                CustomerId: parseInt($("#" + idMapIndex.indexBase.customerID).val()) || 0,
                LeadDescription: $("#" + idMapIndex.indexBase.leadDescription).val(),
                ServiceTypeIds: $("#" + idMapIndex.indexBase.ServiceTypeIds).val() || [],
            };
            showDev(data);
            $.ajax({
                url: '/CreateLead/CreateLeadData',
                method: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(data),
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

    //#region show customer modal
    $(document).on("click","#createCustomer", function () {
        $.get('/Customers/IndexModal', function (html) {
            $('#customerModalContent').html(html);
            var modal = new bootstrap.Modal(document.getElementById('customerModal'));
            modal.show();
        });
    });
    //#endregion
});