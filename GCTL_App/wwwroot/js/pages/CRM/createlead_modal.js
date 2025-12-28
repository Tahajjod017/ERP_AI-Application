function initCreateLeadModal() {
    const modal = $('#createLeadModalToggle');
    const modalContentArea = $('.create-lead-modal-body'); // তোমার custom class যেখানে form load হয়

    // Helper to check if element is valid <select>
    function canInitializeSelect2(id) {
        const el = document.getElementById(id);
        if (!el) {
            console.error(`Element #${id} not found`);
            return false;
        }
        if (el.tagName.toUpperCase() !== 'SELECT') {
            console.error(`Element #${id} is <${el.tagName}>, not <SELECT>`);
            return false;
        }
        return true;
    }

    // 1. Customer Select2
    if (canInitializeSelect2('CustomerId')) {
        $('#CustomerId').select2({
            placeholder: 'Select Customer',
            width: '100%',
            dropdownParent: modalContentArea,
            allowClear: true,
            ajax: {
                url: '/CreateJobs/GetCustomers',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data) {
                    return { results: data.results, pagination: { more: data.pagination.more } };
                }
            },
            language: {
                noResults: function () {
                    return $('<span>Data not found. <a href="#" id="openCustomerModal" class="text-primary">Create Customer</a></span>');
                }
            }
        });
    }

    // 2. Lead Source
    if (canInitializeSelect2('LeadSourceID')) {
        $('#LeadSourceID').select2({
            placeholder: 'Select Lead Source',
            width: '100%',
            dropdownParent: modalContentArea,
            allowClear: true,
            ajax: {
                url: '/CreateLead/GetLeadSourceList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data) {
                    return {
                        results: data.items.map(item => ({ id: item.value, text: item.label })),
                        pagination: { more: data.hasMore }
                    };
                }
            }
        });
    }

    // 3. Lead Status
    if (canInitializeSelect2('LeadStatusID')) {
        $('#LeadStatusID').select2({
            placeholder: 'Select Lead Status',
            width: '100%',
            dropdownParent: modalContentArea,
            allowClear: true,
            ajax: {
                url: '/CreateLead/GetLeadStatusList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data) {
                    return {
                        results: data.items.map(item => ({ id: item.value, text: item.label })),
                        pagination: { more: data.hasMore }
                    };
                }
            }
        });
    }

    // 4. Priority
    if (canInitializeSelect2('PriorityID')) {
        $('#PriorityID').select2({
            placeholder: 'Select Priority',
            width: '100%',
            dropdownParent: modalContentArea,
            allowClear: true,
            ajax: {
                url: '/CreateLead/GetPriorityList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data) {
                    return {
                        results: data.items.map(item => ({ id: item.value, text: item.label })),
                        pagination: { more: data.hasMore }
                    };
                }
            }
        });
    }

    // 5. Lead Owner
    if (canInitializeSelect2('LeadOwnerID')) {
        $('#LeadOwnerID').select2({
            placeholder: 'Select Lead Owner',
            width: '100%',
            dropdownParent: modalContentArea,
            allowClear: true,
            ajax: {
                url: '/CreateLead/GetLeadOwnerList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data) {
                    return {
                        results: data.items.map(item => ({ id: item.value, text: item.label })),
                        pagination: { more: data.hasMore }
                    };
                }
            }
        });
    }

    // 6. Service Types
    if (canInitializeSelect2('ServiceTypeIds')) {
        $('#ServiceTypeIds').select2({
            placeholder: 'Select Service Types',
            width: '100%',
            dropdownParent: modalContentArea,
            allowClear: true,
            multiple: true,
            ajax: {
                url: '/CreateLead/GetServiceList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data) {
                    return {
                        results: data.items.map(item => ({ id: item.value, text: item.label })),
                        pagination: { more: data.hasMore }
                    };
                }
            }
        });
    }

    // ========================
    // Branch Select2 - Only when company customer selected
    // ========================
    function initializeBranchSelect2(customerId) {
        if (!customerId) return;

        const $branch = $('#BranchId');

        // Destroy previous instance if exists
        if ($branch.hasClass('select2-hidden-accessible')) {
            $branch.select2('destroy');
        }

        // Clear options and add empty one
        $branch.empty().append('<option></option>');

        // Show the container
        $('#branchContainer').removeClass('d-none');

        // Initialize Select2
        $branch.select2({
            placeholder: 'Select a branch',
            allowClear: true,
            width: '100%',
            dropdownParent: modalContentArea,
            minimumInputLength: 0,
            ajax: {
                url: '/Customers/GetBranches',
                dataType: 'json',
                delay: 250,
                cache: true,
                data: function (params) {
                    return {
                        customerId: customerId, // camelCase - তোমার controller এ customerId হলে
                        search: params.term || '',
                        page: params.page || 1
                    };
                },
                processResults: function (data) {
                    console.log('Branches data:', data);
                    return {
                        results: data.results || [],
                        pagination: { more: data.pagination?.more || false }
                    };
                }
            }
        });

        console.log('Branch Select2 initialized inside modal');
    }

    // Customer change handler
    //$('#CustomerId').on('change', function () {
    //    const customerId = $(this).val();



        //const $branch = $('#BranchId');
        //const $container = $('#branchContainer');

        //// Cleanup previous Select2
        //if ($branch.hasClass('select2-hidden-accessible')) {
        //    $branch.select2('destroy');
        //}
        //$branch.empty().append('<option></option>');
        //$container.addClass('d-none');

        //if (!customerId) return;

        //const token = $('input[name="__RequestVerificationToken"]').val();

        //$.ajax({
        //    url: '/Customers/GetCustomerInfo',
        //    type: 'POST',
        //    data: { id: customerId },
        //    headers: { "RequestVerificationToken": token },
        //    success: function (response) {
        //        if (!response.isIndividual) {
        //            $container.removeClass('d-none');
        //            setTimeout(() => {
        //                initializeBranchSelect2(customerId);
        //            }, 100);
        //        }
        //    },
        //    error: function () {
        //        toastr.error('Failed to load customer information');
        //    }
        //});
    //});


    $('#CustomerId').on('change', function () {
        
        const customerId = $(this).val();
        if (Number(customerId) > 0) {
            $("#editCustomerBtn").removeClass("d-none");
        } else {
            $("#editCustomerBtn").addClass("d-none");
        }
        const $branchContainer = $('#branchContainer');
        const $branch = $('#BranchId1');
        showDev(customerId)
        // Reset branch dropdown
        $branch.empty().append('<option value="">Select Branch</option>');
        $branchContainer.addClass('d-none');

        if (!customerId) return;

        $.ajax({
            url: '/Customers/GetBranchsInfo',
            type: 'GET',
            data: { id: customerId },
           
            success: function (response) {
                showDev(response)

                // Expecting response = [{ id: 1, name: "Dhaka" }, ...]
                if (response && response.length > 0) {

                    $.each(response, function (i, item) {
                        $branch.append(
                            $('<option>', {
                                value: item.id,
                                text: item.name
                            })
                        );
                    });

                    // Show branch dropdown
                    $branchContainer.removeClass('d-none');

                    initBranchSelect222()
                }
            },
            error: function () {
                toastr.error('Failed to load customer branches');
            }
        });
    });

    //#region leadEditData into Lead fields
    window.loadEditData = function (leadId) {
        $.ajax({
            url: '/CRM/GetLeadInfo',
            method: 'POST',
            data: { id: leadId },
            success: function (response) {
                console.log(response)
                //updateEmployee();
                debugger;
                $("#LeadID").val(response.leadID);
                $("#LeadName").val(response.leadName);
                $("#ApproximateDealValue").val(response.approximateDealValue);
                $("#ProbabilityPercentage").val(response.probability);
                $("#completionValue2").text(response.probability + "%");
                $("#LeadDescription").val(response.leadDescription);
                $("#queryText").val(response.leadOwnerName);
                $("#selectedID").val(response.leadOwnerId);
                // multiselect edit field read
                if (response.services && response.services.length > 0) {
                    response.services.forEach(item => {
                        const option = new Option(item.text, item.value, true, true);
                        $('#ServiceTypeIds').append(option);
                    });

                    $('#ServiceTypeIds').trigger('change');
                }
                setSelect2EditValue("#CustomerId", response.customerId, response.customerName);
                setSelect2EditValue("#BranchId1", response.branchId, response.branchName);
                setSelect2EditValue("#LeadSourceID", response.leadSourceID, response.leadSourceName);
                setSelect2EditValue("#PriorityID", response.priorityID, response.priorityName);
                setSelect2EditValue("#LeadStatusID", response.leadStatusID, response.leadStatusName);
                setSelect2EditValue("#LeadOwnerID", response.leadOwnerId, response.leadOwnerName);
                // employee add
                const currentOwnerId = response.leadOwnerId;
                const currentOwnerName = response.leadOwnerName;
                if (currentOwnerId && currentOwnerName) {
                    choices.setChoices(
                        [{ value: currentOwnerId, label: currentOwnerName, selected: true }],
                        'value',
                        'label',
                        false // false = append (don?t clear)
                    );
                }
            },
            error: function (xhr) {
                toastr.error("Error creating lead");
            }
        });
    }
    //#endregion

    //#region select2 auto functon
    function setSelect2EditValue(selector, id, text) {
        if (!id) return;

        let option = new Option(text, id, true, true);
        $(selector).append(option).trigger('change');
    }

    //#endregion

    //#region branch edit modal option
    $('#BranchId1').on('change', function () {
        
        const customerId = $(this).val();
        if (Number(customerId) > 0) {
            $("#editBranchBtn").removeClass("d-none");
        } else {
            $("#editBranchBtn").addClass("d-none");
        }
        const $branchContainer = $('#branchContainer');
        const $branch = $('#BranchId1');
    });
    //#endregion

    //#region EditCustomer From modal 
    // has to be store modal into current page
    $(document).on("click", "#editCustomerBtn", function (e) {
        e.preventDefault();
        $("#customerModalActionName").text("Edit")
        const firstModalEl = document.getElementById('createLeadModalToggle');
        const firstModal = bootstrap.Modal.getOrCreateInstance(firstModalEl);

        // Load customer modal content
        $.get('/Customers/IndexModal', function (html) {

            $('.customer-modal-content').html(html);

            // Load script if needed
            if (typeof initCustomerModal !== 'function') {
                $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                    .done(() => initCustomerModal && initCustomerModal());
            } else {
                initCustomerModal();
            }
            // show data
            debugger
            const id = $('#CustomerId').val();
            loadCustomerData(id);
            //changeTab("#customer-tab", "#customer");
            // Show second modal on top
            const secondModal = bootstrap.Modal.getOrCreateInstance('#openCustomerModalToggle', {
                backdrop: 'static',
                focus: true,
                keyboard: false
            });

            secondModal.show();

            // When second modal closes ? restore first modal
            $('#openCustomerModalToggle').one('hidden.bs.modal', function () {
                firstModalEl.removeAttribute("inert");
                firstModal.show();
            });

        });

        // Hide first modal visually now
        firstModal.hide();
    });
    //#endregion

    //#region editBranchModalWork
    $(document).on("click", "#editBranchBtn", function (e) {
        e.preventDefault();
        $("#customerModalActionName").text("Edit")
        const firstModalEl = document.getElementById('createLeadModalToggle');
        const firstModal = bootstrap.Modal.getOrCreateInstance(firstModalEl);

        // Load customer modal content
        $.get('/Customers/IndexModal', function (html) {

            $('.customer-modal-content').html(html);

            // Load script if needed
            if (typeof initCustomerModal !== 'function') {
                $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                    .done(() => initCustomerModal && initCustomerModal());
            } else {
                initCustomerModal();
            }
            // show data
            debugger
            const id = $('#CustomerId').val();
            loadCustomerData(id);
            //changeTab("#customer-tab", "#customer");
            // Show second modal on top
            const secondModal = bootstrap.Modal.getOrCreateInstance('#openCustomerModalToggle', {
                backdrop: 'static',
                focus: true,
                keyboard: false
            });

            secondModal.show();

            // When second modal closes ? restore first modal
            $('#openCustomerModalToggle').one('hidden.bs.modal', function () {
                firstModalEl.removeAttribute("inert");
                firstModal.show();
            });

        });

        // Hide first modal visually now
        firstModal.hide();
    });
    //#endregion
    function initBranchSelect222() {
        const $branch = $('#BranchId1');

        // Destroy if already initialized (VERY IMPORTANT)
        if ($branch.hasClass('select2-hidden-accessible')) {
            $branch.select2('destroy');
        }

        $branch.select2({
            placeholder: 'Select Branch',
            width: '100%',
            dropdownParent: $('#createLeadModalToggle'), // 🔥 IMPORTANT
            allowClear: true
        });
    }




    // ========================
    // Form Validation
    // ========================
    function validateForm() {
        debugger;
        let isValid = true;
        let fields = ['CustomerId', 'LeadName', 'LeadSourceID', 'LeadStatusID', 'PriorityID', 'LeadOwnerID'];

        //if ($('#branchContainer').is(':visible')) {
        //    fields.push('BranchId1');
        //}

        fields.forEach(id => {
            const $field = $('#' + id);
            if (!$field.length) return;

            const value = $field.val();
            const isEmpty = !value || (Array.isArray(value) ? value.length === 0 : String(value).trim() === '');

            if (isEmpty) {
                isValid = false;
                const $container = $field.next('.select2-container');
                if ($container.length) {
                    $container.find('.select2-selection').css('border-color', 'red');
                } else {
                    $field.css('border-color', 'red');
                }
            } else {
                const $container = $field.next('.select2-container');
                if ($container.length) {
                    $container.find('.select2-selection').css('border-color', '');
                } else {
                    $field.css('border-color', '');
                }
            }
        });

        if (!isValid) {
            toastr.error('Please fill all required fields');
        }
        return isValid;
    }

    // ========================
    // Form Submit
    // ========================
    $(document).off('click', '#indexSaveBtn').on('click', '#indexSaveBtn', function (e) {
        e.preventDefault();
        if (!validateForm()) return;
        debugger;
        let leadId = $("#LeadID").val();
        const form = document.getElementById('leadForm');
        const token = $("input[name='__RequestVerificationToken']").val();
        const formData = new FormData(form);
        const jsonData = {};
        formData.forEach((value, key) => {
            if (key === "__RequestVerificationToken") return;

            if (["LeadID", "CustomerId", "LeadStatusID", "LeadSourceID", "LeadOwnerID", "PriorityID"].includes(key)) {
                jsonData[key] = parseInt(value) || 0;
            } else if (["ApproximateDealValue", "ProbabilityPercentage"].includes(key)) {
                jsonData[key] = parseFloat(value) || 0;
            } else if (key === "ServiceTypeIds") {
                if (!jsonData[key]) jsonData[key] = [];
                jsonData[key].push(parseInt(value));
            } else {
                jsonData[key] = value === "" ? null : value;
            }
        });

        if ($('#branchContainer').is(':hidden')) {
            jsonData.BranchId = 0;
        }
        console.log(jsonData);
        const actionUrl = Number(leadId) === 0 ? '/CreateLead/CreateLeadData' : '/CreateLead/Edit';

        $.ajax({
            url: actionUrl,
            method: 'POST',
            contentType: 'application/json',
            headers: { "RequestVerificationToken": token },
            data: JSON.stringify(jsonData),
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    const bsModal = bootstrap.Modal.getInstance(modal[0]);
                    if (bsModal) bsModal.hide();
                    if (response.data?.id) {
                        window.location.href = `/LeadDetails/Index/${response.data.id}`;
                    }
                } else {
                    toastr.error(response.message || 'Failed to create lead');
                }
            },
            error: function () {
                toastr.error('Failed to create lead');
            }
        });
    });
}

// Global access
window.initCreateLeadModal = initCreateLeadModal;