$(document).ready(function () {
    showDev('PurchaseReceived.js loaded need authorize');

    // Initialize vendors and shipping addresses
    loadVendors(() => {
        const vendorId = $('#getVendorId').val();
        if (vendorId) {
            $('#vendorSelect').val(vendorId).trigger('change');
            selectVendorFromDropdown();
        }
    });

    loadShippingAddresses(() => {
        const locationId = $('#getLocationId').val();
        if (locationId) {
            toggleShippingDropdown();
            $('#shippingSelect').val(locationId).trigger('change');
        }
    });

    // Form submission
    $('#purchaseOrderForm').on('submit', function (e) {
        e.preventDefault();

        const data = collectPurchaseReceivedData();
        if (!data) return;

        submitPurchaseReceived(data, $(this).find('button[type="submit"]'));
    });



    
    $(document).on('input', '.quantity, .unit-price', function () {
        updateTotals();
    });

    $(document).on('input', '#taxRate', function () {
        updateTotals();
    });


    $(document).on('input', '#paidAmt', function () {
        showDev()
        updateDue();
    });

   
    updateTotals();


    function calculateRowTotal($row) {
        const quantity = parseFloat($row.find('.quantity').val()) || 0;
        const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
        const total = quantity * unitPrice;

        $row.find('.total-price').val(total.toFixed(2));
        return total;
    }

    function updateTotals() {
        let subtotal = 0;

        $('#item-container tbody tr').each(function () {
            const rowTotal = calculateRowTotal($(this));
            subtotal += rowTotal;
        });

        const taxRate = parseFloat($('#taxRate').val()) || 0;
        const taxAmount = (subtotal * taxRate) / 100;
        const grandTotalAmount = subtotal + taxAmount;

        $('#TotalAmount').text(subtotal.toFixed(2));
        $('#TaxAmount').text(taxAmount.toFixed(2));
        $('#GrandTotalAmount').text(grandTotalAmount.toFixed(2));
    }
    
    function updateDue() {
        const grandTotalAmount = parseFloat($('#GrandTotalAmount').text()) || 0;
        const paidAmt = parseFloat($('#paidAmt').val()) || 0;
        const dueAmt = grandTotalAmount - paidAmt;

        $('#dueAmt').val(dueAmt.toFixed(2));
    }
    



    $('#shippingSelect').on('change', function () {
        const selectedId = $(this).val();

        if (selectedId) {
            $.ajax({
                url: '/PurchaseReceived/GetReqValue',
                type: 'GET',
                data: { id: selectedId },
                success: function (response) {

                    showDev(response);
                  
                    $('.req-quantity').val(0);
                    $('.dis-quantity').val(0);


                    $('#item-container tbody tr').each(function () {
                        const $row = $(this);
                        const productId = parseInt($row.find('th').first().text().trim());

                        // Find matching item from response
                        const matchedItem = response.find(x => x.productId === productId);

                        if (matchedItem) {
                            $row.find('.req-quantity').val(matchedItem.reqQty ?? '');
                            $row.find('.dis-quantity').val(matchedItem.disQty ?? '');
                            $row.find('.requisitionItemID').val(matchedItem.reqItemId ?? '');
                        }
                    });

                    updateTotals();

                },
                error: function (xhr, status, error) {
                    console.error("Ajax Error:", error);
                }
            });
        }
    });




    



});

//#region Data Collection & Validation

function collectPurchaseReceivedData() {
    // Validate vendor
    if (!$('#billTo').text().trim()) {
        toastr.warning('Please select a vendor before submitting.');
        return null;
    }

    // Validate PO Number
    const poNumber = $('input[name="POID"]').val();
    if (!poNumber || !poNumber.trim()) {
        toastr.warning('Please enter a PO Number.');
        return null;
    }

    // Validate Bill Date
    const billDate = $('#billDate').val();
    if (!billDate) {
        toastr.warning('Please select a Bill Date.');
        return null;
    }

    // Validate Bill Date
    const prDate = $('#prDate').val();
    if (!prDate) {
        toastr.warning('Please select a Bill Date.');
        return null;
    }

    // Validate items
    const itemRows = $('#item-container tbody tr');
    if (itemRows.length === 0) {
        toastr.warning('Please add at least one item.');
        return null;
    }

    // Collect items
    const items = [];
    let hasValidItems = false;

    itemRows.each(function () {
        const $row = $(this);

        const quantity = parseFloat($row.find('.quantity').val()) || 0;
        const unitPrice = parseFloat($row.find('.unit-price').val()) || 0;
        const unitDistribute = parseFloat($row.find('.unit-Distribute').val()) || 0;

        const productID = parseInt($row.find('th').first().text().trim()) || 0;
        const purchasOrderItemID = parseInt($row.find('.purchasOrderItemID').val()) || 0;
        const requisitionItemID = parseInt($row.find('.requisitionItemID').val()) || 0;

        const reqQuantity = parseFloat($row.find('.req-quantity').val()) || 0;
        const alrDisQuantity = parseFloat($row.find('.dis-quantity').val()) || 0;

        if (quantity > 0 && unitPrice > 0) {
            items.push({
                productID: productID,
                itemName: $row.find('td').eq(0).text().trim(),
                unit: $row.find('td').eq(1).text().trim(),
                quantity: quantity,
                reqQuantity: reqQuantity,
                alrDisQuantity: alrDisQuantity,
                unitDistribute: unitDistribute,
                unitPrice: unitPrice,
                purchasOrderItemID: purchasOrderItemID,
                requisitionItemID: requisitionItemID
            });

            hasValidItems = true;
        }
    });

    if (!hasValidItems) {
        toastr.warning('Please ensure all items have valid quantity and unit price.');
        return null;
    }

    // Final object
    return {
        
        poid: poNumber,
        vendorBillNO: $('input[name="vendorBill"]').val() || '',
        prid: $('input[name="PRID"]').val() || '',
        note: $('textarea[name="note"]').val() || '',
        termNcondition: $('textarea[name="terms"]').val() || '',
        taxRate: parseFloat($('input[name="taxRate"]').val()) || 0,
        purchaseOrderID: parseInt($('#purchaseOderId').val()) || 0,
        purchaseDate: $('#purchaseDate').val() || null,
        dueDate: $('input[name="dueDate"]').val() || null,
        billDate: billDate,
        prDate: prDate,
        tolocation: parseInt($('#shippingSelect').val()) || null,
        vendorId: parseInt($('#vendorSelect').val()) || null,
        items: items,

        deliveryOrderName: $('#deliveryOrderName').val() || '',
        driverName: $('#driverName').val() || '',
        driverMobileNo: $('#driverMobileNo').val() || '',
        truckNo: $('#truckNo').val() || '',


        paidAmt: $('#paidAmt').val() || '',
        dueAmt: $('#dueAmt').val() || '',
        paymentMethodID: $('#PaymentMethodID').val() || '',
        checkNumber: $('#CheckNumber').val() || '',
        checkDate: $('#CheckDate').val() || '',

        checkNumber: $('#CheckNumber').val() || '',
        checkDate: $('#CheckDate').val() || '',
        totalAmount: parseFloat($('#TotalAmount').text()) || 0,
        grandTotalAmount: parseFloat($('#GrandTotalAmount').text()) || 0,
        bankAccountInfoID: $('#BankAccountInfoID').val(),
        taxAmount: parseFloat($('#TaxAmount').text()) || 0,





       




    };
}





//#endregion

//#region Form Submission

function submitPurchaseReceived(data, $submitBtn) {
    const originalBtnText = $submitBtn.text();
    $submitBtn.text('Saving...').prop('disabled', true);

    showDev(data, 'fprm fata')

    const formData = new FormData();

    // Add top-level fields
    formData.append('POID', data.poid);
    formData.append('VendorBillNO', data.vendorBillNO);
    formData.append('PRID', data.prid);
    formData.append('Note', data.note);
    formData.append('TermNcondition', data.termNcondition);
    formData.append('PurchaseOrderID', data.purchaseOrderID);
    formData.append('PurchaseDate', data.purchaseDate);
    formData.append('DueDate', data.dueDate);
    formData.append('BillDate', data.billDate);
    formData.append('PRDate', data.prDate);
    formData.append('Tolocation', data.tolocation);
    formData.append('VendorId', data.vendorId);

    formData.append('driverMobileNo', data.driverMobileNo);
    formData.append('driverName', data.driverName);
    formData.append('truckNo', data.truckNo);
    formData.append('deliveryOrderName', data.deliveryOrderName);

    formData.append('TaxRate', data.taxRate);
    formData.append('TaxAmount', data.taxAmount);
    formData.append('PaidAmount', data.paidAmt);
    formData.append('DueAmount', data.dueAmt);
    formData.append('TotalAmount', data.totalAmount);
    formData.append('GrandTotalAmount', data.grandTotalAmount);
    formData.append('CheckNumber', data.checkNumber);
    formData.append('CheckDate', data.checkDate);
    formData.append('BankAccountInfoID', data.bankAccountInfoID);
    formData.append('PaymentMethodID', data.paymentMethodID);

    // Add nested items (array)
    data.items.forEach((item, index) => {
        formData.append(`Items[${index}].ProductID`, item.productID);
        formData.append(`Items[${index}].ItemName`, item.itemName);
        formData.append(`Items[${index}].Unit`, item.unit);
        formData.append(`Items[${index}].Quantity`, item.quantity);
        formData.append(`Items[${index}].ReqQuantity`, item.reqQuantity);
        formData.append(`Items[${index}].AlrDisQuantity`, item.alrDisQuantity);
        formData.append(`Items[${index}].UnitDistribute`, item.unitDistribute);
        formData.append(`Items[${index}].UnitPrice`, item.unitPrice);
        formData.append(`Items[${index}].PurchasOrderItemID`, item.purchasOrderItemID);
        formData.append(`Items[${index}].RequisitionItemID`, item.requisitionItemID);
    });

    $.ajax({
        url: '/PurchaseReceived/save',
        type: 'POST',
        data: formData,
        processData: false, // Required for FormData
        contentType: false, // Required for FormData
        success: function (result) {
            if (result.success) {
                toastr.success('Purchase Received saved successfully!');
                setTimeout(() => {
                    window.location.href = result.redirectUrl || '/PurchaseReceived/Index';
                }, 1000);
            } else {
                toastr.error(result.message || 'Failed to save Purchase Received');
            }
        },
        error: function (xhr) {
            const errorMessage = xhr.responseJSON?.message || 'Failed to save. Please try again.';
            toastr.error(errorMessage);
        },
        complete: function () {
            $submitBtn.text(originalBtnText).prop('disabled', false);
        }
    });
}



//function submitPurchaseReceived(data, $submitBtn) {
//    const originalBtnText = $submitBtn.text();
//    $submitBtn.text('Saving...').prop('disabled', true);

//    showDev(data)

//    $.ajax({
//        url: '/PurchaseReceived/save',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify(data),
//        success: function (result) {
//            if (result.success) {
//                toastr.success('Purchase Received saved successfully!');
//                setTimeout(() => {
//                    window.location.href = result.redirectUrl || '/PurchaseWaitingList/Index';
//                }, 1000);
//            } else {
//                toastr.error(result.message || 'Failed to save Purchase Received');
//            }
//        },
//        error: function (xhr) {
//            const errorMessage = xhr.responseJSON?.message || 'Failed to save. Please try again.';
//            toastr.error(errorMessage);
//        },
//        complete: function () {
//            $submitBtn.text(originalBtnText).prop('disabled', false);
//        }
//    });
//}

//#endregion

//#region Vendor & Shipping Load

function loadVendors(callback) {
    fetch('/PurchaseReceived/vendors', {
        method: 'GET',
        headers: { 'Accept': 'application/json' }
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            return response.json();
        })
        .then(vendors => {
            const vendorSelect = $('#vendorSelect');
            vendorSelect.html('<option value="">Select Vendor</option>');

            vendors.forEach(vendor => {
                vendorSelect.append(`
                <option value="${vendor.id}" 
                    data-name="${vendor.name}"
                    data-address="${vendor.address || ''}"
                    data-tax="${vendor.taxNumber || ''}"
                    data-phone="${vendor.phone || ''}"
                    data-email="${vendor.email || ''}">
                    ${vendor.name}
                </option>
            `);
            });

            vendorSelect.append('<option value="add_new_vendor">+ Add New Vendor</option>');

            if (callback) callback();
        })
        .catch(error => {
            console.error('Error loading vendors:', error);
            toastr.error('Failed to load vendors');
        });
}

function loadShippingAddresses(callback) {
    fetch('/PurchaseReceived/shipping-addresses', {
        method: 'GET',
        headers: { 'Accept': 'application/json' }
    })
        .then(response => {
            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);
            return response.json();
        })
        .then(addresses => {
            const shippingSelect = $('#shippingSelect');
            shippingSelect.html('<option value="">Select Shipping Address</option>');

            addresses.forEach(address => {
                shippingSelect.append(`
                <option value="${address.id}"
                    data-address="${address.address || ''}"
                    data-contact="${address.contact || ''}"
                    data-phone="${address.phone || ''}"
                    data-email="${address.email || ''}">
                    ${address.name}
                </option>
            `);
            });

            if (callback) callback();
        })
        .catch(error => {
            console.error('Error loading shipping addresses:', error);
            toastr.error('Failed to load shipping addresses');
        });
}

//#endregion

//#region Toggle Functions

function toggleVendorDropdown() {
    $('#showUserSearchBtn').hide();
    $('#vendorSelect').show();
}

function toggleShippingDropdown() {
    $('#showShippingSearchBtn').hide();
    $('#shippingSelect').show();
}

//#endregion

//#region Select & Populate

function selectVendorFromDropdown() {
    const selectedOption = $('#vendorSelect').find(':selected');
    const value = selectedOption.val();

    if (value === 'add_new_vendor') {
        showAddVendorModal();
    } else if (value) {
        populateVendorInfo({
            name: selectedOption.data('name'),
            address: selectedOption.data('address'),
            tax: selectedOption.data('tax'),
            phone: selectedOption.data('phone'),
            email: selectedOption.data('email')
        });
    }
}

function selectShippingFromDropdown() {
    const selectedOption = $('#shippingSelect').find(':selected');
    const value = selectedOption.val();

    if (value) {
        populateShippingInfo({
            address: selectedOption.data('address'),
            contact: selectedOption.data('contact'),
            phone: selectedOption.data('phone'),
            email: selectedOption.data('email')
        });
    }
}

function populateVendorInfo(data) {
    $('#billTo').text(`Bill To: ${data.name}`);
    $('#billAddress').text(data.address || '');
    $('#billTax').text(`Tax Number: ${data.tax || ''}`);
    $('#billPhone').text(data.phone || '');
    $('#billEmail').text(data.email || '');
    $('#userInfo').show();
}

function populateShippingInfo(data) {
    $('#shipAddress').text(data.address || '');
    $('#shipContact').text(`Contact Person: ${data.contact || ''}`);
    $('#shipPhone').text(data.phone || '');
    $('#shipEmail').text(data.email || '');
    $('#shipingInfo').show();
}

//#endregion

//#region Modal Functions

function showAddVendorModal() {
    const modal = new bootstrap.Modal(document.getElementById('addUserModal'));
    modal.show();
}

function saveNewVendor() {
    const vendorData = {
        name: $('#userName').val() || '',
        companyName: $('#companyName').val() || '',
        email: $('#userEmail').val() || '',
        phone: $('#userPhone').val() || ''
    };

    fetch('/PurchaseReceived/vendors', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(vendorData)
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => {
                    throw new Error(err.message || 'Failed to save vendor');
                });
            }
            return response.json();
        })
        .then(data => {
            toastr.success(data.message || 'Vendor added successfully!');
            bootstrap.Modal.getInstance(document.getElementById('addUserModal')).hide();
            loadVendors();
            toggleVendorDropdown();
        })
        .catch(error => {
            console.error('Error saving vendor:', error);
            toastr.error(error.message || 'Failed to save vendor');
        });
}

//#endregion