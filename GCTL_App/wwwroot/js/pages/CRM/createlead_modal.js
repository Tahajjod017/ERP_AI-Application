function initCreateLeadModal() {

    setTimeout(function () {
        const modal = $('#createLeadModalToggle');

        // Helper functions
        function canInitializeSelect2(id) {
            const el = document.getElementById(id);
            if (!el) {
                console.error(`❌ Element #${id} not found`);
                return false;
            }
            if (el.tagName.toUpperCase() !== 'SELECT') {
                console.error(`❌ Element #${id} is <${el.tagName}>, not <SELECT>`);
                return false;
            }
            return true;
        }

        function ensureBranchIsSelect() {
            const branchInput = document.getElementById('BranchId');
            if (!branchInput) return false;

            if (branchInput.tagName === 'INPUT') {

                const parent = branchInput.parentNode;
                const select = document.createElement('select');
                select.id = 'BranchId';
                select.name = 'BranchId';
                select.className = 'form-select';

                const option = document.createElement('option');
                option.value = '';
                option.text = 'Select Branch';
                select.appendChild(option);

                parent.replaceChild(select, branchInput);
                return true;
            }
            return true;
        }

        // 1. Customer
        if (canInitializeSelect2('CustomerId')) {
            $('#CustomerId').select2({
                placeholder: 'Select Customer',
                width: '100%',
                dropdownParent: modal,
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
                dropdownParent: modal,
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
                dropdownParent: modal,
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
                dropdownParent: modal,
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
                dropdownParent: modal,
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
                dropdownParent: modal,
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

        let isBranchRequired = false;

        // Branch initialization - CRITICAL FIX
        function initializeBranchSelect2(customerId) {

            // First ensure it's SELECT
            ensureBranchIsSelect();

            // Wait for DOM update
            setTimeout(function () {

                // Get FRESH reference after conversion
                const branchEl = document.getElementById('BranchId');

                if (!branchEl) {
                    return;
                }

                if (branchEl.tagName !== 'SELECT') {
                    console.error('❌ Still not SELECT:', branchEl.tagName);
                    return;
                }


                // Create FRESH jQuery object
                const $branch = $(branchEl);

                // Destroy old Select2 if exists
                if ($branch.data('select2')) {
                    $branch.select2('destroy');
                }

                // Clear
                $branch.val('');

                // Initialize Select2
                $branch.select2({
                    placeholder: 'Select Branch',
                    width: '100%',
                    dropdownParent: modal,
                    allowClear: true,
                    ajax: {
                        url: '/Customers/GetBranches',
                        dataType: 'json',
                        delay: 250,
                        data: function (params) {
                            return {
                                customerID: customerId,
                                search: params.term || '',
                                page: params.page || 1
                            };
                        },
                        processResults: function (data) {
                            return {
                                results: data.results,
                                pagination: { more: data.pagination.more }
                            };
                        }
                    }
                });

                // Verify
                if ($branch.hasClass('select2-hidden-accessible')) {
                    console.log('✅✅✅ Branch Select2 SUCCESSFULLY initialized!');
                } else {
                    console.error('❌ Select2 initialization failed!');
                }

            }, 200);
        }

        // Customer change
        $('#CustomerId').on('change', function () {
            const customerId = $(this).val();

            if (!customerId) {
                $('#branchContainer').hide();
                isBranchRequired = false;
                return;
            }

            $.ajax({
                url: '/Customers/GetCustomerInfo',
                type: 'POST',
                data: { id: customerId },
                success: function (response) {
                    const $branchContainer = $('#branchContainer');

                    if (!response.isIndividual) {
                        isBranchRequired = true;

                        // Show first
                        $branchContainer.removeClass('d-none').show();

                        // Then initialize with delays
                        setTimeout(function () {
                            initializeBranchSelect2(customerId);
                        }, 150);

                    } else {
                        isBranchRequired = false;

                        const $branch = $('#BranchId');
                        if ($branch.data('select2')) {
                            $branch.select2('destroy');
                        }
                        $branch.val('');
                        $branchContainer.hide();
                    }
                }
            });
        });

        // Validation
        function validateForm() {
            let isValid = true;
            const fields = ['CustomerId', 'LeadName', 'LeadSourceID', 'LeadStatusID', 'PriorityID', 'LeadOwnerID'];

            //if (isBranchRequired && $('#branchContainer').is(':visible')) {
            //    fields.push('BranchId');
            //}

            fields.forEach(id => {
                const $field = $('#' + id);
                if (!$field.length) return;

                const isEmpty = !$field.val() || $field.val().trim() === '';
                const isSelect2 = $field.hasClass('select2-hidden-accessible');

                if (isEmpty) {
                    isValid = false;
                    if (isSelect2) {
                        $field.next('.select2-container').find('.select2-selection').css('border-color', 'red');
                    } else {
                        $field.css('border-color', 'red');
                    }
                } else {
                    if (isSelect2) {
                        $field.next('.select2-container').find('.select2-selection').css('border-color', '');
                    } else {
                        $field.css('border-color', '');
                    }
                }
            });

            if (!isValid) toastr.error('Please fill all required fields');
            return isValid;
        }

        // Submit
        $(document)
            .off('click', '#indexSaveBtn')
            .on('click', '#indexSaveBtn', function (e) {
            e.preventDefault();

            if (!validateForm()) return;

            const form = document.getElementById("leadForm");
            const token = $("input[name='__RequestVerificationToken']").val();
            const formData = new FormData(form);
            const jsonData = {};

            formData.forEach((value, key) => {
                if (key === "__RequestVerificationToken") return;
                if (["CustomerId", "LeadStatusID", "LeadSourceID", "LeadOwnerID", "PriorityID", "BranchId"].includes(key)) {
                    value = parseInt(value) || 0;
                }
                if (["ApproximateDealValue", "ProbabilityPercentage"].includes(key)) {
                    value = parseFloat(value) || 0;
                }
                if (key === "ServiceTypeIds") {
                    if (!jsonData[key]) jsonData[key] = [];
                    jsonData[key].push(parseInt(value));
                    return;
                }
                jsonData[key] = value === "" ? null : value;
            });

            if (!isBranchRequired) jsonData.BranchId = 0;

            $.ajax({
                url: '/CreateLead/CreateLeadData',
                method: 'POST',
                contentType: "application/json",
                headers: { "RequestVerificationToken": token },
                data: JSON.stringify(jsonData),
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        const modal = bootstrap.Modal.getInstance($('#createLeadModalToggle')[0]);
                        if (modal) modal.hide();
                        if (response.data?.id) {
                            window.location.href = `/LeadDetails/Index/${response.data.id}`;
                        }
                    } else {
                        toastr.error(response.message || "Failed");
                    }
                },
                error: function () {
                    toastr.error('Failed to create lead');
                }
            });
        });

    }, 400);
}

window.initCreateLeadModal = initCreateLeadModal;
