// wwwroot/js/pages/Purchase/rfq.js
// RFQ-specific JavaScript functions

class RfqManager {
    constructor() {
        this.currentAlternativeId = null;
        this.alternativeRfqs = [];
    }
}

// Initialize when document is ready
$(document).ready(function () {
    window.rfqManager = new RfqManager();

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
    $('#sendEmailBtn').prop('disabled', !vendorId);
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
                <select name="Items[${index}].ProductId" class="form-select product-select" onchange="onProductSelect(this)">
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
            <td colspan="7">
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
            <td colspan="7">
                <textarea name="Items[${index}].NoteText" class="form-control" rows="2" placeholder="Add a note..."></textarea>
                <input type="hidden" name="Items[${index}].IsNote" value="true" />
            </td>
        </tr>
    `;
    $('#productTableBody').append(html);
    initializeSortable();
}

function deleteRow(btn) {
    if (confirm('Delete this line?')) {
        $(btn).closest('tr').remove();
        renumberRows();
        calculateTotals();
    }
}

$(document).on('input change', '.calc', calculateTotals);

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
            const subtotal = qty * price;
            $row.find('.subtotal').text('$ ' + subtotal.toFixed(2));

            untaxed += subtotal;
            taxTotal += subtotal * 0.15; // assuming 15% tax for demo
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
        $star.text('★').css('color', '#f0ad4e');
    } else {
        $star.text('☆').css('color', '#ccc');
    }
}

// Modal helper functions (Bootstrap 5 compatible)
function showModal(modalId) {
    const modalEl = document.getElementById(modalId);
    if (modalEl) {
        let modal = bootstrap.Modal.getInstance(modalEl);
        if (!modal) {
            modal = new bootstrap.Modal(modalEl);
        }
        modal.show();
    }
}

function hideModal(modalId) {
    const modalEl = document.getElementById(modalId);
    if (modalEl) {
        const modal = bootstrap.Modal.getInstance(modalEl);
        if (modal) modal.hide();
    }
}

// Alternative RFQ Functions
$('#createAlternativeBtn').on('click', function () {
    const $previewList = $('#productPreviewList');
    $previewList.empty();

    $('#productTableBody tr.item-row').each(function () {
        const $row = $(this);
        const productName = $row.find('.product-name-display').text() || $row.find('.product-name').val() || '';
        const qty = $row.find('.quantity').val() || 1;
        const price = $row.find('.unit-price').val() || 0;

        if (productName) {
            $previewList.append(`<li>${productName} - Qty: ${qty}, Price: $${parseFloat(price).toFixed(2)}</li>`);
        }
    });

    showModal('createAlternativeModal');
});

function createAlternativeRfq() {
    const vendorId = $('#alternativeVendor').val();
    const mainRfqId = $('#Id').val();

    if (!vendorId) {
        toastr.error('Please select a vendor');
        return;
    }

    $.ajax({
        url: '/Rfq/CreateAlternativeRfq',
        method: 'POST',
        data: { mainRfqId: mainRfqId, vendorId: vendorId },
        success: function (response) {
            if (response.success) {
                toastr.success('Alternative RFQ created successfully!');
                hideModal('createAlternativeModal');
                location.reload();
            }
        },
        error: function () {
            toastr.error('Failed to create alternative RFQ');
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
                $('#viewAltReference').text(rfq.reference || rfq.Reference);
                $('#viewAltVendor').text(rfq.vendorName || rfq.VendorName);
                $('#viewAltDate').text(new Date(rfq.date || rfq.Date).toLocaleDateString());
                $('#viewAltStatus').text(rfq.status || 'Draft');
                $('#viewAltTotal').text('$ ' + (rfq.totalAmount || rfq.TotalAmount).toFixed(2));

                const $table = $('#viewAltProductsTable');
                $table.empty();
                (rfq.items || rfq.Items).forEach(item => {
                    $table.append(`
                        <tr>
                            <td>${item.productName || item.ProductName}</td>
                            <td>${item.qty || item.Quantity}</td>
                            <td>$ ${(item.unitPrice || item.UnitPrice).toFixed(2)}</td>
                            <td>15%</td>
                            <td>$ ${(item.subtotal || item.Subtotal).toFixed(2)}</td>
                        </tr>
                    `);
                });

                showModal('viewAlternativeModal');
            }
        }
    });
}

function editAlternativeRfq(id) {
    $.ajax({
        url: '/Rfq/GetAlternativeRfqDetails',
        method: 'GET',
        data: { id: id },
        success: function (response) {
            if (response.success) {
                const rfq = response.data;
                $('#editAltReference').text(rfq.reference || rfq.Reference);
                $('#editAltVendor').val(rfq.vendorId || rfq.VendorId);

                const $table = $('#editAltProductsTable');
                $table.empty();
                (rfq.items || rfq.Items).forEach((item, index) => {
                    $table.append(`
                        <tr>
                            <td>${item.productName || item.ProductName}</td>
                            <td><input type="number" class="form-control alt-qty" value="${item.qty || item.Quantity}" data-index="${index}" step="0.01" /></td>
                            <td><input type="number" class="form-control alt-price" value="${(item.unitPrice || item.UnitPrice).toFixed(2)}" data-index="${index}" step="0.01" /></td>
                            <td><select class="form-select alt-tax" data-index="${index}"><option value="0.15" selected>15%</option><option value="0">0%</option></select></td>
                            <td class="alt-subtotal" data-index="${index}">$ ${(item.subtotal || item.Subtotal).toFixed(2)}</td>
                            <td><button type="button" class="btn btn-sm btn-outline-danger" onclick="deleteAltProduct(this, ${index})"><i class="far fa-trash-alt"></i></button></td>
                        </tr>
                    `);
                });

                showModal('editAlternativeModal');
            }
        }
    });
}

function openComparisonView() {
    if ($('#alternativeRFQList .rfq-item').length === 0) {
        toastr.warning('Please create at least one alternative RFQ first');
        return;
    }
    generateComparisonTable();
    showModal('comparisonModal');
}

// Email Functions
$('#sendEmailBtn').on('click', function () {
    const vendorId = $('#SelectedVendorId').val();
    if (!vendorId) {
        toastr.error('Please select a vendor first');
        return;
    }
    showModal('emailModal');
});

$('#sendEmail').on('click', function () {
    $.ajax({
        url: '/Rfq/SendByEmail',
        method: 'POST',
        data: { rfqId: $('#Id').val() },
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                hideModal('emailModal');
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
                if (response.id) $('#Id').val(response.id);
            }
        }
    });
}

function discardRFQ() {
    if (confirm('Discard changes?')) location.reload();
}

function printRFQ() {
    window.print();
}

function confirmOrder() {
    if (confirm('Confirm this order?')) {
        $.ajax({
            url: '/Rfq/ConfirmOrder',
            method: 'POST',
            data: { rfqId: $('#Id').val() },
            success: function (response) {
                if (response.success) toastr.success(response.message);
            }
        });
    }
}

function cancelRFQ() {
    if (confirm('Cancel this RFQ?')) {
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

// Placeholder functions (you can expand later)
function generateComparisonTable() {
    // Keep your existing hardcoded table or make it dynamic later
    $('#comparisonTableContainer').html(`your existing comparison table HTML here`);
}

function closeCreateAlternativeModal() { hideModal('createAlternativeModal'); }
function closeViewAlternativeModal() { hideModal('viewAlternativeModal'); }
function closeEditAlternativeModal() { hideModal('editAlternativeModal'); }
function closeComparisonModal() { hideModal('comparisonModal'); }
function closeEmailModal() { hideModal('emailModal'); }