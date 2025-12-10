$(document).ready(function () {
    console.log('CreatePurchaseOrder.js loaded');


    loadProducts();


    loadVendors();
    //loadShippingAddresses();

    var dta = $('#getLocationId').val();
    showDev(dta, 'ss');

    loadShippingAddresses(() => {
        $('#shippingSelect').val(dta).trigger('change');
    });


    updateGrandTotal();








    const dropzoneElement = document.getElementById('dropzone-multiple');
    const simpleFileUpload = document.getElementById('simple-file-upload');

    if (dropzoneElement && typeof Dropzone !== 'undefined') {
        try {
            // Check if dropzone is already initialized
            if (!dropzoneElement.dropzone) {
                new Dropzone('#dropzone-multiple', {
                    url: '/CreatePurchaseOrder/upload-attachments',
                    paramName: 'file',
                    maxFilesize: 10, // MB
                    acceptedFiles: '.pdf,.doc,.docx,.jpg,.png,.txt',
                    addRemoveLinks: true,
                    dictDefaultMessage: 'Drop files here or click to upload',
                    dictRemoveFile: 'Remove',
                    dictFileTooBig: 'File is too big ({{filesize}}MB). Max filesize: {{maxFilesize}}MB.',
                    dictInvalidFileType: 'You can\'t upload files of this type.',
                    init: function () {
                        this.on('addedfile', function (file) {
                            console.log('File added:', file.name);
                        });
                        this.on('success', function (file, response) {
                            console.log('File uploaded successfully:', response);
                            file.serverId = response.uniqueFileName; // Store server file ID
                        });
                        this.on('error', function (file, errorMessage) {
                            console.error('File upload error:', errorMessage);
                        });
                        this.on('removedfile', function (file) {
                            console.log('File removed:', file.name);
                            // Optionally send delete request to server
                            if (file.serverId) {
                                // Add delete API call here if needed
                            }
                        });
                    }
                });
                console.log('Dropzone initialized successfully');
            }
        } catch (e) {
            console.log('Dropzone initialization failed, showing fallback:', e.message);
            // Show fallback file input if dropzone fails
            if (simpleFileUpload) {
                dropzoneElement.style.display = 'none';
                simpleFileUpload.style.display = 'block';
            }
        }
    } else {
        console.log('Dropzone not available, showing fallback file input');
        // Show fallback if Dropzone library is not loaded
        if (simpleFileUpload && dropzoneElement) {
            dropzoneElement.style.display = 'none';
            simpleFileUpload.style.display = 'block';
        }
    }




    $('#purchaseOrderForm').on('submit', function (e) {
        e.preventDefault();
        console.log('Form submission started');
        let uploadedFiles = [];
        if (dropzoneElement && dropzoneElement.dropzone) {
            dropzoneElement.dropzone.files.forEach(function (file) {
                if (file.serverId) {
                    uploadedFiles.push(file.serverId);
                }
            });
        }
        const billToElement = $('#billTo');
        if (!billToElement.length || !billToElement.text().trim()) {
            toastr.info('Please select a vendor before submitting.');
            return;
        }
        const poNumber = $('input[name="POID"]').val();
        if (!poNumber || !poNumber.trim()) {
            toastr.info('Please enter a PO Number.');
            return;
        }
        const poDate = $('input[name="billDate"]').val();
        if (!poDate) {
            toastr.info('Please select a PO Date.');
            return;
        }
        const itemRows = $('#item-container tbody tr');
        if (itemRows.length === 0) {
            toastr.info('Please add at least one item.');
            return;
        }
        const formData = new FormData();
        formData.append('PurchaseOderId', $('#purchaseOderId').val());
        formData.append('SupperId', $('#vendorSelect').val());
        formData.append('ToLocationId', $('#shippingSelect').val());
        formData.append('PoDate', poDate);
        formData.append('PoNumber', poNumber);
        formData.append('DueDate', $('input[name="dueDate"]').val() || '');
        formData.append('WorkOrderDate', $('input[name="workOrderDate"]').val() || '');
        formData.append('OtherReference', $('input[name="otherReference"]').val() || '');
        formData.append('WorkOrderNo', $('input[name="workOrderNo"]').val() || '');
        formData.append('Note', $('textarea[name="note"]').val() || '');
        formData.append('Terms', $('textarea[name="terms"]').val() || '');

        formData.append('TotalAmount', parseFloat($('#TotalAmount').text()) || 0);
        formData.append('TaxRate', parseFloat($('#TaxRate').val()) || 0);
        formData.append('TaxAmount', parseFloat($('#TaxAmount').text()) || 0);
        formData.append('GrandTotalAmount', parseFloat($('#GrandTotalAmount').text()) || 0);

        formData.append('PaymentMethodID', $('#PaymentMethodID').val() || "");
        formData.append('BankAccountInfoID', $('#BankAccountInfoID').val() || "");
        formData.append('MobileBankingType', $('#MobileBankingType').val() || "");

        formData.append('CheckNumber', $('#CheckNumber').val() || "");
        formData.append('CheckDate', $('#CheckDate').val() || "");

        formData.append('PaidAmount', parseFloat($('#PaidAmount').val()) || 0);
        formData.append('DueAmount', parseFloat($('#DueAmount').val()) || 0);

        formData.append('BillingInfo.Name', $('#billTo').text().replace('Bill To: ', ''));
        formData.append('BillingInfo.Address', $('#billAddress').text() || '');
        formData.append('BillingInfo.TaxNumber', $('#billTax').text().replace('Tax Number: ', '') || '');
        formData.append('BillingInfo.Phone', $('#billPhone').text() || '');
        formData.append('BillingInfo.Email', $('#billEmail').text() || '');
        if ($('#shippingCheckbox').is(':checked')) {
            formData.append('ShippingInfo.Address', $('#shipAddress').text() || '');
            formData.append('ShippingInfo.Contact', $('#shipContact').text().replace('Contact Person: ', '') || '');
            formData.append('ShippingInfo.Phone', $('#shipPhone').text() || '');
            formData.append('ShippingInfo.Email', $('#shipEmail').text() || '');
        }
        let hasValidItems = false;
        itemRows.each(function (index) {
            const $row = $(this);
            const quantity = parseFloat($row.find('.quantity').val()) || 0;
            const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
            const purchasOrderItemID = parseFloat($row.find('.purchasOrderItemID').val()) || 0;
            const productId = $row.find('.product-id').text().trim() || $row.find('.product-select').val();
            const itemName = $row.find('td').eq(0).text().trim() || $row.find('.product-select option:selected').text();
            if (quantity > 0 && unitPrice > 0 && productId) {
                formData.append(`Items[${index}].SerialNumber`, productId);
                formData.append(`Items[${index}].ItemName`, itemName);
                formData.append(`Items[${index}].Unit`, $row.find('.unit').text().trim());
                formData.append(`Items[${index}].Quantity`, quantity);
                formData.append(`Items[${index}].UnitPrice`, unitPrice);
                formData.append(`Items[${index}].TotalAmount`, parseFloat($row.find('.total-amount').text()) || 0);
                formData.append(`Items[${index}].PurchasOrderItemID`, purchasOrderItemID);
                hasValidItems = true;
            }
        });
        if (!hasValidItems) {
            toastr.info('Please ensure all items have valid quantity and unit price.');
            return;
        }
        uploadedFiles.forEach((fileId, index) => {
            formData.append(`Attachments[${index}]`, fileId);
        });
        showDev(formData, 'Data being sent:');
        const $submitBtn = $(this).find('button[type="submit"]');
        const originalBtnText = $submitBtn.text();
        $submitBtn.text('Saving...').prop('disabled', true);
        $.ajax({
            url: '/ManualPO/save',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                showDev(result ,'Server response:');
                if (result.success) {
                    toastr.success(result.message || 'Purchase Order saved successfully!');
                    window.location.href = `/PurchaseWaitingList/Index`;
                    //if (result.purchaseId) {
                    //    window.location.href = `/PurchaseWaitingList/Index`;
                    //} else {
                    //    window.location.reload();
                    //}
                } else {
                    toastr.info('Failed to save Purchase Order: ' + (result.message || 'Unknown error'));
                }
            },
            error: function (xhr, status, error) {
                console.error('AJAX Error:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });
                let errorMessage = 'Failed to save Purchase Order. ';
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    errorMessage += xhr.responseJSON.message;
                } else {
                    errorMessage += 'Please try again.';
                }
                toastr.info(errorMessage);
            },
            complete: function () {
                $submitBtn.text(originalBtnText).prop('disabled', false);
            }
        });
    });






    //$('#purchaseOrderForm').on('submit', function (e) {
    //    e.preventDefault();
    //    console.log('Form submission started');


    //    let uploadedFiles = [];
    //    if (dropzoneElement && dropzoneElement.dropzone) {
    //        dropzoneElement.dropzone.files.forEach(function (file) {
    //            if (file.serverId) {
    //                uploadedFiles.push(file.serverId);
    //            }
    //        });
    //    }




    //    const billToElement = $('#billTo');
    //    if (!billToElement.length || !billToElement.text().trim()) {
    //        toastr.info('Please select a vendor before submitting.');
    //        return;
    //    }

    //    const poNumber = $('input[name="POID"]').val();
    //    if (!poNumber || !poNumber.trim()) {
    //        toastr.info('Please enter a PO Number.');
    //        return;
    //    }

    //    const poDate = $('input[name="billDate"]').val();
    //    if (!poDate) {
    //        toastr.info('Please select a PO Date.');
    //        return;
    //    }


    //    const itemRows = $('#item-container tbody tr');
    //    if (itemRows.length === 0) {
    //        toastr.info('Please add at least one item.');
    //        return;
    //    }



    //    const data = {
    //        poDate: poDate,
    //        poNumber: poNumber,
    //        purchaseOderId: $('#purchaseOderId').val(),
    //        supperId: $('#vendorSelect').val(),
    //        toLocationId: $('#shippingSelect').val(),


    //        dueDate: $('input[name="dueDate"]').val() || '',
    //        workOrderDate: $('input[name="workOrderDate"]').val() || '',
    //        otherReference: $('input[name="otherReference"]').val() || '',
    //        workOrderNo: $('input[name="workOrderNo"]').val() || '',
    //        note: $('textarea[name="note"]').val() || '',
    //        terms: $('textarea[name="terms"]').val() || '',
    //        TaxRate: parseFloat($('input[name="TaxRate"]').val()) || 0,
    //        billingInfo: {
    //            name: $('#billTo').text().replace('Bill To: ', ''),
    //            address: $('#billAddress').text() || '',
    //            taxNumber: $('#billTax').text().replace('Tax Number: ', '') || '',
    //            phone: $('#billPhone').text() || '',
    //            email: $('#billEmail').text() || ''
    //        },
    //        shippingInfo: $('#shippingCheckbox').is(':checked') ? {
    //            address: $('#shipAddress').text() || '',
    //            contact: $('#shipContact').text().replace('Contact Person: ', '') || '',
    //            phone: $('#shipPhone').text() || '',
    //            email: $('#shipEmail').text() || ''
    //        } : null,
    //        items: [],
    //        attachments: uploadedFiles
    //    };

    //    // Collect items data
    //    let hasValidItems = false;





    //    itemRows.each(function (index) {
    //        const $row = $(this);
    //        const quantity = parseFloat($row.find('.quantity').val()) || 0;
    //        const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
    //        const purchasOrderItemID = parseFloat($row.find('.purchasOrderItemID').val()) || 0;
    //        const productId = $row.find('.product-id').text().trim() || $row.find('.product-select').val();
    //        const itemName = $row.find('td').eq(0).text().trim() || $row.find('.product-select option:selected').text();

    //        if (quantity > 0 && unitPrice > 0 && productId) {
    //            const item = {
    //                serialNumber: productId,
    //                itemName: itemName,
    //                unit: $row.find('.unit').text().trim(),
    //                quantity: quantity,
    //                unitPrice: unitPrice,
    //                totalAmount: parseFloat($row.find('.total-amount').text()) || 0,
    //                purchasOrderItemID: purchasOrderItemID
    //            };
    //            data.items.push(item);
    //            hasValidItems = true;
    //        }
    //    });

    //    if (!hasValidItems) {
    //        toastr.info('Please ensure all items have valid quantity and unit price.');
    //        return;
    //    }

    //    showDev(data, 'Data being sent:');


    //    const $submitBtn = $(this).find('button[type="submit"]');
    //    const originalBtnText = $submitBtn.text();
    //    $submitBtn.text('Saving...').prop('disabled', true);


    //    $.ajax({
    //        url: '/ManualPO/save',
    //        type: 'POST',
    //        contentType: 'application/json',
    //        dataType: 'json',
    //        data: JSON.stringify(data),
    //        success: function (result) {
    //            console.log('Server response:', result);

    //            if (result.success) {
    //                toastr.success('Purchase Order saved successfully!');
    //                if (result.purchaseId) {
    //                    window.location.href = `/PurchaseWaitingList/Index`;
    //                } else {
    //                    window.location.reload();
    //                }
    //            } else {
    //                toastr.info('Failed to save Purchase Order: ' + (result.message || 'Unknown error'));
    //            }




    //        },
    //        error: function (xhr, status, error) {
    //            console.error('AJAX Error:', {
    //                status: xhr.status,
    //                statusText: xhr.statusText,
    //                responseText: xhr.responseText,
    //                error: error
    //            });

    //            let errorMessage = 'Failed to save Purchase Order. ';
    //            if (xhr.responseJSON && xhr.responseJSON.message) {
    //                errorMessage += xhr.responseJSON.message;
    //            } else {
    //                errorMessage += 'Please try again.';
    //            }
    //            toastr.info(errorMessage);
    //        },
    //        complete: function () {

    //            $submitBtn.text(originalBtnText).prop('disabled', false);
    //        }
    //    });




    //});




    // #region Added by Md. Rakib Hasan

    // #region Toggle Acconts on change Payment Type
    $('#PaymentMethodID').on('change', function () {
        var id = $(this).val();

        if (id == '2') {
            $('#mobileBankingDiv').addClass('d-none');
            $('#chequeDiv').addClass('d-none');
            $('#AccountsDiv').removeClass('d-none');
        } else if (id == '4') {
            $('#mobileBankingDiv').addClass('d-none');
            $('#AccountsDiv').removeClass('d-none');
            $('#chequeDiv').removeClass('d-none');
        } else if (id == '6') {
            $('#chequeDiv').addClass('d-none');
            $('#AccountsDiv').addClass('d-none');
            $('#mobileBankingDiv').removeClass('d-none');
        } else {
            $('#chequeDiv').addClass('d-none');
            $('#AccountsDiv').addClass('d-none');
            $('#mobileBankingDiv').addClass('d-none');
            $('#BankAccountInfoID').val('');
            $('#CheckNumber').val('');
            document.querySelector("#CheckDate")._flatpickr.clear();
            $('#MobileBankingType').val('');
        }
    });
    // #endregion


    // #region Calculate Due Amount
    $(document).on('input', '#PaidAmount', function () {
        const grandTotalAmount = parseFloat($('#GrandTotalAmount').text()) || 0;
        const paidAmt = parseFloat($('#PaidAmount').val()) || 0;
        const dueAmt = grandTotalAmount - paidAmt;

        $('#DueAmount').val(dueAmt.toFixed(2));
    });
    // #endregion


    // #region flatpicker DatePicker
    flatpickr(".datetimepicker", {
        altInput: true,
        altFormat: "d/m/Y",
        dateFormat: "Y-m-d",
        monthSelectorType: "dropdown",
        disableMobile: true,
        allowInput: true
    });
    // #endregion


    // #region Initialize Choices DD
    //function initBankAccDD() {
    //    bankAccDD = new Choices('#Accounts', {
    //        removeItemButton: true,
    //        shouldSort: false,
    //        placeholderValue: 'Select Account...',
    //        searchEnabled: true,
    //        searchChoices: true,
    //        searchFloor: 1,
    //        searchResultLimit: 10,
    //        searchFields: ['label', 'value']
    //    });
    //}
    //initBankAccDD();
    // #endregion

    // #endregion



});


document.querySelectorAll('.closeModal').forEach(btn => {
    btn.addEventListener('click', () => {

        hideModal('addUserModal')
    });
});

// Load vendors from service
function loadVendors() {
    fetch('/CreatePurchaseOrder/vendors', {
        method: 'GET',
        headers: { 'Accept': 'application/json' }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(vendors => {
            const vendorSelect = document.getElementById('vendorSelect');
            vendorSelect.innerHTML = '<option value="">Select Vendor</option> ';
            vendors.forEach(vendor => {
                const option = document.createElement('option');
                option.value = vendor.id;
                option.textContent = vendor.name;
                option.dataset.name = vendor.name;
                option.dataset.address = vendor.address;
                option.dataset.tax = vendor.taxNumber;
                option.dataset.phone = vendor.phone;
                option.dataset.email = vendor.email;
                vendorSelect.appendChild(option);
            });

            // Add "Add New Vendor" option at the end
            const addNewOption = document.createElement('option');
            addNewOption.value = 'add_new_vendor';
            addNewOption.textContent = '+ Add New Vendor';
            vendorSelect.appendChild(addNewOption);




        })
        .catch(error => {
            console.error('Error loading vendors:', error);
            toastr.info('Failed to load vendors. Please try again later.');
        });
}


function loadShippingAddresses(callback) {
    fetch('/CreatePurchaseOrder/shipping-addresses', { method: 'GET', headers: { 'Accept': 'application/json' } })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            return response.json();
        })
        .then(addresses => {
            const shippingSelect = document.getElementById('shippingSelect');
            shippingSelect.innerHTML = '<option value="">Select Shipping Address</option>';
            addresses.forEach(address => {
                const option = document.createElement('option');
                option.value = address.id;
                option.textContent = address.name;
                option.dataset.address = address.address;
                option.dataset.contact = address.contact;
                option.dataset.phone = address.phone;
                option.dataset.email = address.email;
                shippingSelect.appendChild(option);
            });

            if (callback) callback();
        })
        .catch(error => {
            console.error('Error loading shipping addresses:', error);
            toastr.info('Failed to load shipping addresses. Please try again later.');
        });
}


function toggleVendorDropdown() {
    const showUserSearchBtn = document.getElementById('showUserSearchBtn');
    const vendorSelect = document.getElementById('vendorSelect');

    showUserSearchBtn.style.display = 'none';
    vendorSelect.style.display = 'block';
}

// Toggle shipping dropdown visibility
function toggleShippingDropdown() {
    const showShippingSearchBtn = document.getElementById('showShippingSearchBtn');
    const shippingSelect = document.getElementById('shippingSelect');

    showShippingSearchBtn.style.display = 'none';
    shippingSelect.style.display = 'block';
}

// Handle vendor selection from select dropdown
function selectVendorFromDropdown() {
    const vendorSelect = document.getElementById('vendorSelect');
    const selectedOption = vendorSelect.selectedOptions[0];
    if (selectedOption.value && selectedOption.value !== 'add_new_vendor') {
        populateVendorInfo({
            userId: selectedOption.value,
            name: selectedOption.dataset.name,
            address: selectedOption.dataset.address,
            tax: selectedOption.dataset.tax,
            phone: selectedOption.dataset.phone,
            email: selectedOption.dataset.email
        });
    } else if (selectedOption.value === 'add_new_vendor') {
        showAddVendorModal();
    }
}

// Handle shipping address selection from select dropdown
function selectShippingFromDropdown() {
    const shippingSelect = document.getElementById('shippingSelect');
    const selectedOption = shippingSelect.selectedOptions[0];
    if (selectedOption.value && selectedOption.value !== 'add_new_shipping') {
        populateShippingInfo({
            addressId: selectedOption.value,
            address: selectedOption.dataset.address,
            contact: selectedOption.dataset.contact,
            phone: selectedOption.dataset.phone,
            email: selectedOption.dataset.email
        });
    } else if (selectedOption.value === 'add_new_shipping') {
        showAddShippingModal();
    }
}

// Populate vendor info
function populateVendorInfo(data) {
    const userInfo = document.getElementById('userInfo');
    const billTo = document.getElementById('billTo');
    const billAddress = document.getElementById('billAddress');
    const billTax = document.getElementById('billTax');
    const billPhone = document.getElementById('billPhone');
    const billEmail = document.getElementById('billEmail');

    billTo.textContent = `Bill To: ${data.name}`;
    billAddress.textContent = data.address;
    billTax.textContent = `Tax Number: ${data.tax}`;
    billPhone.textContent = data.phone;
    billEmail.textContent = data.email;
    userInfo.style.display = 'block';


    //showShippingSearchBtn.style.display = showShippingSearchBtn.style.display === 'none' && shippingSelect.style.display === 'none' ? 'block' : showShippingSearchBtn.style.display;
    //shippingSelect.style.display = shippingSelect.style.display === 'block' ? 'block' : 'none';
    //shipingInfo.style.display = shipingInfo.style.display === 'block' ? 'block' : 'none';

}

// Populate shipping info
function populateShippingInfo(data) {
    const shipingInfo = document.getElementById('shipingInfo');
    const shipAddress = document.getElementById('shipAddress');
    const shipContact = document.getElementById('shipContact');
    const shipPhone = document.getElementById('shipPhone');
    const shipEmail = document.getElementById('shipEmail');

    shipAddress.textContent = data.address;
    shipContact.textContent = `Contact Person: ${data.contact}`;
    shipPhone.textContent = data.phone;
    shipEmail.textContent = data.email;
    shipingInfo.style.display = 'block';
}

// Toggle shipping section based on checkbox
function toggleShippingSection() {
    const shippingCheckbox = document.getElementById('shippingCheckbox');
    const showShippingSearchBtn = document.getElementById('showShippingSearchBtn');
    const shippingSelect = document.getElementById('shippingSelect');
    const shipingInfo = document.getElementById('shipingInfo');

    if (shippingCheckbox.checked) {
        showShippingSearchBtn.style.display = showShippingSearchBtn.style.display === 'none' && shippingSelect.style.display === 'none' ? 'block' : showShippingSearchBtn.style.display;
        shippingSelect.style.display = shippingSelect.style.display === 'block' ? 'block' : 'none';
        shipingInfo.style.display = shipingInfo.style.display === 'block' ? 'block' : 'none';
    } else {
        showShippingSearchBtn.style.display = 'none';
        shippingSelect.style.display = 'none';
        shipingInfo.style.display = 'none';
    }
}

// Show add vendor modal
function showAddVendorModal() {
    const modal = new bootstrap.Modal(document.getElementById('addUserModal'));
    modal.show();
}

// Show add shipping modal
function showAddShippingModal() {
    // $('#addShippingModal').modal('show');
    const modal = new bootstrap.Modal(document.getElementById('addShippingModal'));
    modal.show();
}




function saveNewVendor() {
    const vendorData = {
        name: document.getElementById('userName')?.value || '',
        companyName: document.getElementById('companyName')?.value || '',
        email: document.getElementById('userEmail')?.value || '',
        phone: document.getElementById('userPhone')?.value || ''
    };

    fetch('/CreatePurchaseOrder/vendors', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(vendorData)
    })
        //.then(response => {

        //    if (!response.ok) {
        //        throw new Error(`HTTP error! Status: ${response.status}`);
        //    }
        //    return response.json();
        //})
        //.then(() => {
        //    $('#addUserModal').modal('hide');
        //    loadVendors();
        //    document.getElementById('showUserSearchBtn').style.display = 'none';
        //    document.getElementById('vendorSelect').style.display = 'block';
        //    toastr.success('Vendor added successfully!');
        //})
        //.catch(error => {
        //    console.error('Error saving vendor:', error);
        //    toastr.info('Failed to save vendor. Please try again.');
        //});

        .then(response => {
            if (!response.ok) {
                return response.json().then(err => {
                    throw new Error(err.message || 'Unknown error');
                });
            }
            return response.json();
        })
        .then(data => {
            toastr.success(data.message || 'Vendor added successfully!');
            // rest of your success logic
        })
        .catch(error => {
            console.error('Error saving vendor:', error);
            toastr.info(error.message || 'Failed to save vendor. Please try again.');
        });

}



// Save new shipping address
function saveNewShipping() {
    const form = document.getElementById('addShippingForm');
    const formData = new FormData(form);
    const shippingData = {
        contact: formData.get('shippingName'),
        email: formData.get('shippingEmail'),
        phone: formData.get('shippingPhone'),
        address: formData.get('shippingAddress')
    };

    fetch('/CreatePurchaseOrder/shipping-addresses', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(shippingData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(() => {
            $('#addShippingModal').modal('hide');
            loadShippingAddresses();
            document.getElementById('showShippingSearchBtn').style.display = 'none';
            document.getElementById('shippingSelect').style.display = 'block';
            toastr.success('Shipping address added successfully!');
        })
        .catch(error => {
            console.error('Error saving shipping address:', error);
            toastr.info('Failed to save shipping address. Please try again.');
        });
}

// Update row total calculation
function updateRowTotal(element, index) {
    const row = element.closest('tr');
    const quantity = parseFloat(row.querySelector('.quantity').value) || 0;
    const unitPrice = parseFloat(row.querySelector('.unit-price').value) || 0;
    const total = (quantity * unitPrice).toFixed(2);
    row.querySelector('.total-amount').textContent = total;
    updateGrandTotal();
}

// Update grand total calculation
function updateGrandTotal() {
    let subtotal = 0;
    document.querySelectorAll('.total-amount').forEach(cell => {
        subtotal += parseFloat(cell.textContent) || 0;
    });
    const taxRate = parseFloat(document.getElementById('TaxRate').value) || 0;
    const taxAmount = (subtotal * (taxRate / 100)).toFixed(2);
    const grandTotal = (subtotal + parseFloat(taxAmount)).toFixed(2);
    document.getElementById('TotalAmount').textContent = subtotal;
    document.getElementById('TaxAmount').textContent = taxAmount;
    document.getElementById('GrandTotalAmount').textContent = grandTotal;
}



//#region Manual Page

// Load products for dropdown
function loadProducts() {
    fetch('/ManualPO/GetProducts', {
        method: 'GET',
        headers: { 'Accept': 'application/json' }
    })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(products => {
            window.products = products; // Store products globally for easy access
            updateProductDropdowns(); // Initialize dropdowns after loading products
        })
        .catch(error => {
            console.error('Error loading products:', error);
            toastr.info('Failed to load products. Please try again later.');
        });
}

// Update all product dropdowns to disable selected products
function updateProductDropdowns() {
    const tbody = document.getElementById('item-table-body');
    const rows = tbody.querySelectorAll('tr');
    const selectedProductIds = [];

    // Collect all selected product IDs
    rows.forEach(row => {
        const productId = row.querySelector('.product-id').textContent.trim() ||
            row.querySelector('.product-select')?.value;
        if (productId && productId !== '') {
            selectedProductIds.push(productId);
        }
    });

    // Update each dropdown
    rows.forEach(row => {
        const select = row.querySelector('.product-select');
        if (select) {
            const currentValue = select.value;
            select.innerHTML = '<option value="">Select Product</option>';
            window.products.forEach(product => {
                const option = document.createElement('option');
                option.value = product.id;
                option.textContent = product.name;
                option.dataset.unit = product.unit;
                if (selectedProductIds.includes(product.id) && product.id !== currentValue) {
                    option.disabled = true;
                }
                select.appendChild(option);
            });
            select.value = currentValue; // Restore the selected value
        }
    });
}

// Add new item row
function addNewItemRow() {
    const tbody = document.getElementById('item-table-body');
    const rowCount = tbody.querySelectorAll('tr').length;
    const newRow = document.createElement('tr');

    // Create product dropdown
    let productOptions = '<select class="form-control product-select" name="Items[' + rowCount + '].ProductID" onchange="updateItemDetails(this, ' + rowCount + ')">';
    productOptions += '<option value="">Select Product</option>';
    window.products.forEach(product => {
        productOptions += `<option value="${product.id}" data-unit="${product.unit}">${product.name}</option>`;
    });
    productOptions += '</select>';

    newRow.innerHTML = `
        <th class="product-id"></th>
        <td>${productOptions}</td>
        <td class="unit"></td>
        <td>
            <input type="number" class="form-control quantity" name="Items[${rowCount}].Quantity" value="0" step="1" min="0" onchange="updateRowTotal(this, ${rowCount})" />
        </td>
        <td>
            <input type="number" class="form-control unit-price" name="Items[${rowCount}].UnitPrice" value="0" step="0.01" min="0" onchange="updateRowTotal(this, ${rowCount})" />
        </td>
        <td class="total-amount">0.00</td>
        <td>
            <button type="button" class="btn btn-danger btn-sm" onclick="removeItemRow(this)">Remove</button>
        </td>
        <td class="d-none">
            <input type="hidden" class="purchasOrderItemID" name="Items[${rowCount}].PurchasOrderItemID" value="0" />
        </td>
    `;
    tbody.appendChild(newRow);
    updateProductDropdowns(); // Update dropdowns to disable selected products
}

// Update item details when product is selected
function updateItemDetails(select, index) {
    const row = select.closest('tr');
    const selectedOption = select.selectedOptions[0];
    const productId = selectedOption.value;

    // Check for duplicate product
    const tbody = document.getElementById('item-table-body');
    const rows = tbody.querySelectorAll('tr');
    let isDuplicate = false;
    rows.forEach((otherRow, otherIndex) => {
        if (otherRow !== row) {
            const otherProductId = otherRow.querySelector('.product-id').textContent.trim() ||
                otherRow.querySelector('.product-select')?.value;
            if (otherProductId === productId && productId !== '') {
                isDuplicate = true;
            }
        }
    });

    if (isDuplicate) {
        toastr.info('This product is already selected. Please choose a different product.');
        select.value = ''; // Reset the dropdown
        row.querySelector('.product-id').textContent = '';
        row.querySelector('.unit').textContent = '';
        updateRowTotal(select, index);
        updateProductDropdowns();
        return;
    }

    const unit = selectedOption.dataset.unit || '';
    row.querySelector('.product-id').textContent = productId;
    row.querySelector('.unit').textContent = unit;
    updateRowTotal(select, index);
    updateProductDropdowns(); // Update dropdowns to disable this product in other rows
}

// Remove item row
function removeItemRow(button) {
    const row = button.closest('tr');
    row.remove();
    updateGrandTotal();
    updateProductDropdowns(); // Update dropdowns to re-enable the removed product's option
}



//#endregion