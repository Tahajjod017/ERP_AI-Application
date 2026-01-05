// wwwroot/js/pages/Purchase/rfq.js
// This file can contain additional RFQ-specific JavaScript functions
// Most logic is already in the view above, but you can add modular functions here

class RfqManager {
    constructor() {
        this.currentAlternativeId = null;
        this.alternativeRfqs = [];
    }

    // Add any additional methods here
}

// Initialize when document is ready
$(document).ready(function () {
    window.rfqManager = new RfqManager();
});


$(function () {
    // Initialize Select2
    $('.searchableSelect').select2({
        width: '100%',
        allowClear: true,
        placeholder: 'Select an option'
    });

    // Get next RFQ number
    getNextRfqNumber();

    // Initialize drag and drop
    initializeSortable();

    // Calculate initial totals
    calculateTotals();
});

function getNextRfqNumber() {
    $.ajax({
        url: '/Rfq/GetNextRfqNumber',
        method: 'GET',
        success: function (data) {
            $('#RfqNumber').val(data);
        }
    });
}

function onVendorChange() {
    const vendorId = $('#SelectedVendorId').val();
    if (vendorId) {
        $('#sendEmailBtn').prop('disabled', false);
    } else {
        $('#sendEmailBtn').prop('disabled', true);
    }
}

function initializeSortable() {
    const tbody = document.getElementById('productTableBody');
    if (!tbody) return;

    tbody.addEventListener('dragover', function (e) {
        e.preventDefault();
        const dragging = document.querySelector('.item-row.dragging');
        if (!dragging) return;

        const afterElement = getDragAfterElement(tbody, e.clientY);
        if (afterElement == null) {
            tbody.appendChild(dragging);
        } else {
            tbody.insertBefore(dragging, afterElement);
        }
    });

    document.querySelectorAll('.item-row').forEach(row => {
        row.draggable = true;
        row.addEventListener('dragstart', function () {
            this.classList.add('dragging');
        });
        row.addEventListener('dragend', function () {
            this.classList.remove('dragging');
            renumberRows();
        });
    });
}

function getDragAfterElement(container, y) {
    const draggableElements = [...container.querySelectorAll('.item-row:not(.dragging)')];
    return draggableElements.reduce((closest, child) => {
        const box = child.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;
        if (offset < 0 && offset > closest.offset) {
            return { offset: offset, element: child };
        } else {
            return closest;
        }
    }, { offset: Number.NEGATIVE_INFINITY }).element;
}

function renumberRows() {
    $('#productTableBody tr.item-row').each(function (index) {
        $(this).find('input, select, textarea').each(function () {
            const name = $(this).attr('name');
            if (name && name.includes('Items')) {
                $(this).attr('name', name.replace(/Items\[\d+\]/, `Items[${index}]`));
            }
        });
    });
}

function addProductLine() {
    const index = $('#productTableBody tr').length;
    const html = `
                        <tr class="item-row" draggable="true">
                            <td class="drag-handle"><i class="fas fa-grip-vertical"></i></td>
                            <td>
                                        <select name="Items[${index}].ProductId" class="form-select product-select" asp-for="ViewBag.Products as SelectList" onchange="onProductSelect(this)">
                                    <option value="">-- Select Product --</option>
        
                                </select>
                                <input type="hidden" name="Items[${index}].ProductName" class="product-name" />
                            </td>
                            <td><input type="text" name="Items[${index}].Description" class="form-control" placeholder="Description" /></td>
                            <td><input type="number" name="Items[${index}].Quantity" class="form-control text-center calc quantity" value="1.00" step="0.01" /></td>
                            <td>
                                <select name="Items[${index}].Uom" class="form-select">
                                    <option value="Units">Units</option>
                                    <option value="kg">kg</option>
                                    <option value="Dozens">Dozens</option>
                                    <option value="Hours">Hours</option>
                                </select>
                            </td>
                            <td><input type="number" name="Items[${index}].UnitPrice" class="form-control text-end calc unit-price" value="0.00" step="0.01" /></td>
                            <td>
                                <select name="Items[${index}].TaxRate" class="form-select calc tax-select">
                                    <option value="0.15">Purchase Tax 15%</option>
                                    <option value="0">No Tax</option>
                                </select>
                            </td>
                            <td class="text-end subtotal">$ 0.00</td>
                            <td class="text-center">
                                <button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteRow(this)">
                                    <i class="far fa-trash-alt"></i>
                                </button>
                            </td>
                        </tr>
                    `;
    $('#productTableBody').append(html);
    initializeSortable();
}

function onProductSelect(select) {
    const $row = $(select).closest('tr');
    const productText = $(select).find('option:selected').text();
    const productName = productText.split('-')[0].trim();
    $row.find('.product-name').val(productName);
}

function addSection() {
    const index = $('#productTableBody tr').length;
    const html = `
                        <tr class="item-row" draggable="true">
                            <td class="drag-handle"><i class="fas fa-grip-vertical"></i></td>
                            <td colspan="8">
                                <input type="text" name="Items[${index}].SectionTitle" class="form-control" placeholder="Section title..." />
                                <input type="hidden" name="Items[${index}].IsSection" value="true" />
                            </td>
                        </tr>
                    `;
    $('#productTableBody').append(html);
    initializeSortable();
}

function addNote() {
    const index = $('#productTableBody tr').length;
    const html = `
                        <tr class="item-row" draggable="true">
                            <td class="drag-handle"><i class="fas fa-grip-vertical"></i></td>
                            <td colspan="8">
                                <textarea name="Items[${index}].NoteText" class="form-control" rows="2" placeholder="Add a note..."></textarea>
                                <input type="hidden" name="Items[${index}].IsNote" value="true" />
                            </td>
                        </tr>
                    `;
    $('#productTableBody').append(html);
    initializeSortable();
}

function deleteRow(btn) {
    if (confirm('@Html.SmartLocalize("Delete this line?")')) {
        $(btn).closest('tr').remove();
        renumberRows();
        calculateTotals();
    }
}

// Attach calculation events
$(document).on('input', '.calc', calculateTotals);
$(document).on('change', '.calc', calculateTotals);

function calculateTotals() {
    let untaxed = 0;
    let taxTotal = 0;

    $('#productTableBody tr.item-row').each(function () {
        const $row = $(this);
        const isSection = $row.find('input[name$=".IsSection"]').val() === 'true';
        const isNote = $row.find('input[name$=".IsNote"]').val() === 'true';

        if (!isSection && !isNote) {
            const qty = parseFloat($row.find('.quantity').val()) || 0;
            const price = parseFloat($row.find('.unit-price').val()) || 0;
            const taxRate = parseFloat($row.find('.tax-select').val()) || 0;

            const subtotal = qty * price;
            const taxAmount = subtotal * taxRate;
            const total = subtotal + taxAmount;

            $row.find('.subtotal').text('$ ' + total.toFixed(2));

            untaxed += subtotal;
            taxTotal += taxAmount;
        }
    });

    const total = untaxed + taxTotal;
    $('#untaxedAmount').text('$ ' + untaxed.toFixed(2));
    $('#taxAmount').text('$ ' + taxTotal.toFixed(2));
    $('#grandTotal').text('$ ' + total.toFixed(2));
}

function showTab(tabName) {
    $('.tab').removeClass('active');
    $('.tab-content').hide();

    $(event.target).addClass('active');
    $('#' + tabName + '-tab').show();
}

function toggleStar() {
    const $star = $('.star');
    if ($star.text() === '☆') {
        $star.text('★');
        $star.css('color', '#f0ad4e');
    } else {
        $star.text('☆');
        $star.css('color', '#ccc');
    }
}

function changeStatus(status) {
    $('.status-btn').removeClass('active');
    $(event.target).addClass('active');
    $('#Status').val(status);
}

// Alternative RFQ Functions
$('#createAlternativeBtn').on('click', function () {
    // Populate product preview
    const $previewList = $('#productPreviewList');
    $previewList.empty();

    $('#productTableBody tr.item-row').each(function () {
        const $row = $(this);
        const productName = $row.find('.product-name-display').text() || $row.find('.product-name').val();
        const qty = $row.find('.quantity').val() || 1;
        const price = $row.find('.unit-price').val() || 0;

        if (productName && productName !== '') {
            $previewList.append(`
                                <li>${productName} - Qty: ${qty}, Price: $${parseFloat(price).toFixed(2)}</li>
                            `);
        }
    });

    $('#createAlternativeModal').addClass('active');
});

function closeCreateAlternativeModal() {
    $('#createAlternativeModal').removeClass('active');
}

function createAlternativeRfq() {
    const vendorId = $('#alternativeVendor').val();
    const mainRfqId = $('#Id').val();

    if (!vendorId) {
        toastr.error('@Html.SmartLocalize("Please select a vendor")');
        return;
    }

    $.ajax({
        url: '/Rfq/CreateAlternativeRfq',
        method: 'POST',
        data: { mainRfqId: mainRfqId, vendorId: vendorId },
        success: function (response) {
            if (response.success) {
                toastr.success('@Html.SmartLocalize("Alternative RFQ created successfully!")');
                closeCreateAlternativeModal();
                // Refresh alternative RFQ list
                location.reload();
            }
        },
        error: function () {
            toastr.error('@Html.SmartLocalize("Failed to create alternative RFQ")');
        }
    });
}

function viewAlternativeRfq(id) {
    $.ajax({
        url: '/Rfq/GetAlternativeRfqDetails',
        method: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                const rfq = response.data;
                $('#viewAltReference').text(rfq.reference);
                $('#viewAltVendor').text(rfq.vendorName);
                $('#viewAltDate').text(new Date(rfq.date).toLocaleDateString());
                $('#viewAltStatus').text(rfq.status);
                $('#viewAltTotal').text('$ ' + rfq.totalAmount.toFixed(2));

                const $table = $('#viewAltProductsTable');
                $table.empty();

                rfq.items.forEach(item => {
                    $table.append(`
                                        <tr>
                                            <td>${item.productName}</td>
                                            <td>${item.qty}</td>
                                            <td>$ ${item.unitPrice.toFixed(2)}</td>
                                            <td>${(item.taxRate * 100).toFixed(0)}%</td>
                                            <td>$ ${item.subtotal.toFixed(2)}</td>
                                        </tr>
                                    `);
                });

                $('#viewAlternativeModal').addClass('active');
            }
        }
    });
}

function closeViewAlternativeModal() {
    $('#viewAlternativeModal').removeClass('active');
}

function editAlternativeRfq(id) {
    $.ajax({
        url: '/Rfq/GetAlternativeRfqDetails',
        method: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                const rfq = response.data;
                $('#editAltReference').text(rfq.reference);
                $('#editAltVendor').val(rfq.vendorId);

                const $table = $('#editAltProductsTable');
                $table.empty();

                rfq.items.forEach((item, index) => {
                    $table.append(`
                                        <tr>
                                            <td>${item.productName}</td>
                                            <td>
                                                <input type="number" class="form-control alt-qty" value="${item.qty}" data-index="${index}" step="0.01" />
                                            </td>
                                            <td>
                                                <input type="number" class="form-control alt-price" value="${item.unitPrice}" data-index="${index}" step="0.01" />
                                            </td>
                                            <td>
                                                <select class="form-select alt-tax" data-index="${index}">
                                                    <option value="0.15" ${item.taxRate === 0.15 ? 'selected' : ''}>15%</option>
                                                    <option value="0" ${item.taxRate === 0 ? 'selected' : ''}>0%</option>
                                                </select>
                                            </td>
                                            <td class="alt-subtotal" data-index="${index}">$ ${item.subtotal.toFixed(2)}</td>
                                            <td>
                                                <button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteAltProduct(this, ${index})">
                                                    <i class="far fa-trash-alt"></i>
                                                </button>
                                            </td>
                                        </tr>
                                    `);
                });

                $('#editAlternativeModal').addClass('active');
            }
        }
    });
}

function closeEditAlternativeModal() {
    $('#editAlternativeModal').removeClass('active');
}

function saveAlternativeRfq() {
    // Implement save alternative RFQ logic
    toastr.success('@Html.SmartLocalize("Alternative RFQ saved successfully!")');
    closeEditAlternativeModal();
    location.reload();
}

function deleteAlternativeRfq(id) {
    if (confirm('@Html.SmartLocalize("Delete this alternative RFQ?")')) {
        // Implement delete alternative RFQ logic
        toastr.success('@Html.SmartLocalize("Alternative RFQ deleted successfully!")');
        location.reload();
    }
}

function useThisAlternative() {
    if (confirm('@Html.SmartLocalize("Use this alternative RFQ as the main RFQ?")')) {
        // Implement logic to replace main RFQ with alternative
        toastr.success('@Html.SmartLocalize("Alternative RFQ applied successfully!")');
        closeViewAlternativeModal();
        location.reload();
    }
}

// Comparison View
function openComparisonView() {
    // Check if there are alternative RFQs
    if ($('#alternativeRFQList .rfq-item').length === 0) {
        toastr.warning('@Html.SmartLocalize("Please create at least one alternative RFQ first")');
        return;
    }

    generateComparisonTable();
    $('#comparisonModal').addClass('active');
}

function closeComparisonModal() {
    $('#comparisonModal').removeClass('active');
}

function generateComparisonTable() {
    // This would be populated with real data from server
    const container = $('#comparisonTableContainer');
    container.html(`
                        <table class="comparison-table">
                            <thead>
                                <tr>
                                    <th>@Html.SmartLocalize("Product")</th>
                                    <th>@Html.SmartLocalize("Main RFQ")<br><small>RFQ00001</small></th>
                                    <th>@Html.SmartLocalize("Azure Interior")<br><small>ALT0001</small></th>
                                    <th>@Html.SmartLocalize("Deco Addict")<br><small>ALT0002</small></th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td>[FURN_5800] Cable Management Box</td>
                                    <td class="offer-cell">
                                        <div class="offer-price">$ 90.00</div>
                                        <div class="offer-details">Qty: 2</div>
                                        <button class="btn btn-sm btn-phoenix-primary" onclick="selectOffer('product1', 'main')">Select</button>
                                    </td>
                                    <td class="offer-cell best-offer">
                                        <div class="offer-price">$ 85.00</div>
                                        <div class="offer-details">Qty: 2</div>
                                        <button class="btn btn-sm btn-phoenix-success" onclick="selectOffer('product1', 'alt1')">Select</button>
                                    </td>
                                    <td class="offer-cell">
                                        <div class="offer-price">$ 88.00</div>
                                        <div class="offer-details">Qty: 2</div>
                                        <button class="btn btn-sm btn-phoenix-primary" onclick="selectOffer('product1', 'alt2')">Select</button>
                                    </td>
                                </tr>
                                <tr>
                                    <td>[FURN_7800] Office Desk</td>
                                    <td class="offer-cell">
                                        <div class="offer-price">$ 450.00</div>
                                        <div class="offer-details">Qty: 1</div>
                                        <button class="btn btn-sm btn-phoenix-primary" onclick="selectOffer('product2', 'main')">Select</button>
                                    </td>
                                    <td class="offer-cell">
                                        <div class="offer-price">$ 420.00</div>
                                        <div class="offer-details">Qty: 1</div>
                                        <button class="btn btn-sm btn-phoenix-primary" onclick="selectOffer('product2', 'alt1')">Select</button>
                                    </td>
                                    <td class="offer-cell best-offer">
                                        <div class="offer-price">$ 410.00</div>
                                        <div class="offer-details">Qty: 1</div>
                                        <button class="btn btn-sm btn-phoenix-success" onclick="selectOffer('product2', 'alt2')">Select</button>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    `);
}

function selectOffer(productId, rfqId) {
    toastr.info('Selected product from RFQ:');
}

function selectBestOffers() {
    toastr.success('@Html.SmartLocalize("Best offers selected automatically!")');
    closeComparisonModal();
}

// Email Functions
$('#sendEmailBtn').on('click', function () {
    const vendorId = $('#SelectedVendorId').val();
    if (!vendorId) {
        toastr.error('@Html.SmartLocalize("Please select a vendor first")');
        return;
    }
    $('#emailModal').addClass('active');
});

function closeEmailModal() {
    $('#emailModal').removeClass('active');
}

$('#sendEmail').on('click', function () {
    $.ajax({
        url: '/Rfq/SendByEmail',
        method: 'POST',
        data: { rfqId: $('#Id').val() },
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                closeEmailModal();
                changeStatus('RFQ_SENT');
            }
        }
    });
});

// Action Functions
function saveRFQ() {
    const formData = $('#rfqForm').serialize();
    $.ajax({
        url: '/Rfq/Save',
        method: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                if (response.id) {
                    $('#Id').val(response.id);
                }
            }
        }
    });
}

function discardRFQ() {
    if (confirm('@Html.SmartLocalize("Discard changes?")')) {
        location.reload();
    }
}

function printRFQ() {
    window.print();
}

function confirmOrder() {
    if (confirm('@Html.SmartLocalize("Confirm this order?")')) {
        $.ajax({
            url: '/Rfq/ConfirmOrder',
            method: 'POST',
            data: { rfqId: $('#Id').val() },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    changeStatus('PURCHASE_ORDER');
                }
            }
        });
    }
}

function cancelRFQ() {
    if (confirm('@Html.SmartLocalize("Cancel this RFQ?")')) {
        $.ajax({
            url: '/Rfq/CancelRfq',
            method: 'POST',
            data: { rfqId: $('#Id').val() },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    location.href = '/Rfq/Index';
                }
            }
        });
    }
}