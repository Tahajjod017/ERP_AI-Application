$(document).ready(function () {
    console.log('CreatePurchaseOrder.js loaded');
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
            toastr.warning('Please select a vendor before submitting.');
            return;
        }

        const poNumber = $('input[name="POID"]').val();
        if (!poNumber || !poNumber.trim()) {
            toastr.warning('Please enter a PO Number.');
            return;
        }

        const poDate = $('input[name="billDate"]').val();
        if (!poDate) {
            toastr.warning('Please select a PO Date.');
            return;
        }

       
        const itemRows = $('#item-container tbody tr');
        if (itemRows.length === 0) {
            toastr.warning('Please add at least one item.');
            return;
        }

        
        
        const data = {
            poDate: poDate,
            poNumber: poNumber,
            purchaseOderId: $('#purchaseOderId').val(),
            supperId: $('#vendorSelect').val(),
            toLocationId: $('#shippingSelect').val(),
           
           
            dueDate: $('input[name="dueDate"]').val() || '',
            workOrderDate: $('input[name="workOrderDate"]').val() || '',
            otherReference: $('input[name="otherReference"]').val() || '',
            workOrderNo: $('input[name="workOrderNo"]').val() || '',
            note: $('textarea[name="note"]').val() || '',
            terms: $('textarea[name="terms"]').val() || '',
            taxRate: parseFloat($('input[name="taxRate"]').val()) || 0,
            billingInfo: {
                name: $('#billTo').text().replace('Bill To: ', ''),
                address: $('#billAddress').text() || '',
                taxNumber: $('#billTax').text().replace('Tax Number: ', '') || '',
                phone: $('#billPhone').text() || '',
                email: $('#billEmail').text() || ''
            },
            shippingInfo: $('#shippingCheckbox').is(':checked') ? {
                address: $('#shipAddress').text() || '',
                contact: $('#shipContact').text().replace('Contact Person: ', '') || '',
                phone: $('#shipPhone').text() || '',
                email: $('#shipEmail').text() || ''
            } : null,
            items: [],
            attachments: uploadedFiles 
        };

        // Collect items data
        let hasValidItems = false;
        itemRows.each(function (index) {
            const $row = $(this);
            const quantity = parseFloat($row.find('.quantity').val()) || 0;
            const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
            const purchasOrderItemID = parseFloat($row.find('.purchasOrderItemID').val()) || 0;

          
            debugger
            if (quantity > 0 && unitPrice > 0) {
                const item = {
                    serialNumber: $row.find('th').first().text().trim(),
                    itemName: $row.find('td').eq(0).text().trim(),
                    unit: $row.find('td').eq(1).text().trim(),
                    quantity: quantity,
                    unitPrice: unitPrice,
                    totalAmount: parseFloat($row.find('.total-amount').text()) || 0,
                    purchasOrderItemID: purchasOrderItemID
                };
                data.items.push(item);
                hasValidItems = true;
            }
        });

        if (!hasValidItems) {
            toastr.warning('Please ensure all items have valid quantity and unit price.');
            return;
        }

        showDev(data ,'Data being sent:' );

       
        const $submitBtn = $(this).find('button[type="submit"]');
        const originalBtnText = $submitBtn.text();
        $submitBtn.text('Saving...').prop('disabled', true);

      
        $.ajax({
            url: '/CreatePurchaseOrder/save',
            type: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: JSON.stringify(data),
            success: function (result) {
                console.log('Server response:', result);

                if (result.success) {
                    toastr.success('Purchase Order saved successfully!');
                    if (result.purchaseId) {
                        window.location.href = `/PurchaseWaitingList/Index`;
                    } else {
                        window.location.reload();
                    }
                } else {
                    toastr.warning('Failed to save Purchase Order: ' + (result.message || 'Unknown error'));
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
                toastr.warning(errorMessage);
            },
            complete: function () {
             
                $submitBtn.text(originalBtnText).prop('disabled', false);
            }
        });




    });





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
            toastr.warning('Failed to load vendors. Please try again later.');
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
            toastr.warning('Failed to load shipping addresses. Please try again later.');
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
        //    toastr.warning('Failed to save vendor. Please try again.');
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
            toastr.warning(error.message || 'Failed to save vendor. Please try again.');
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
            toastr.warning('Failed to save shipping address. Please try again.');
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
    const taxRate = parseFloat(document.getElementById('taxRate').value) || 0;
    const taxAmount = (subtotal * (taxRate / 100)).toFixed(2);
    const grandTotal = (subtotal + parseFloat(taxAmount)).toFixed(2);
    document.getElementById('grandTotal').textContent = grandTotal;
}



