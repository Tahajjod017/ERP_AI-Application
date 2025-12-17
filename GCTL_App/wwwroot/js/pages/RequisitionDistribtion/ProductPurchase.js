$(document).ready(function () {
    // Initialize page
    loadProductPurchases();
    initializeEventHandlers();
});

// Load product purchases data
function loadProductPurchases(page = 1) {
    const pageSize = parseInt($('#pageSizeSelect').val()) || 10;
    const searchTerm = $('#searchInput').val().toLowerCase();
    const productType = $('#productTypeFilter').val();
    const sortBy = $('#ProductPurchase').data('sortBy') || 'reqId';
    const sortOrder = $('#ProductPurchase').data('sortOrder') || 'asc';

    $.ajax({
        url: '/ProductPurchase/GetProductPurchases',
        type: 'GET',
        data: {
            page: page,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortBy: sortBy,
            sortOrder: sortOrder,
            productType: productType
        },
        success: function (response) {
            showDev(response, 'response');
            if (response.success) {
                populateTable(response.data);
                updatePagination(response.totalCount, response.totalPages, page);
            } else {
                showError('Failed to load data: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            showError('Error loading data: ' + error);
        }
    });
}

// Populate table with data
function populateTable(data) {
    showDev(data ,'ddd')
    const tbody = $('#product-Requisition-history-table');
    tbody.empty();
    if (data && data.length > 0) {
        data.forEach(function (item) {
            const row = createTableRow(item);
            tbody.append(row);
        });
        
            DynamicTableDrag.refreshTableSettings('ProductPurchase');
       
    } else {
        tbody.append('<tr><td colspan="7" class="text-center">No data found</td></tr>');
    }
}
// Create table row
function createTableRow(item) {
    return `
        <tr class="position-static" data-id="${item.purchaseId}" data-product-id="${item.purchaseId}">
            <td class="fs-9 align-middle py-0" data-column="0">
                        <div class="form-check mb-0 fs-8">
                            <input class="form-check-input product-checkbox" type="checkbox" value="${item.purchaseId}" />
                        </div>
                    </td>
            <td class="align-middle white-space-nowrap ps-0  reqId" data-column="1">${item.purchaseId}</td>
            <td class="align-middle white-space-nowrap ps-2 purpose" data-column="2">${item.poid}</td>
            <td class="align-middle white-space-nowrap ps-2 type" data-column="3">${item.suppiler}</td>
            <td class="align-middle white-space-nowrap ps-2 productName" data-column="4">${item.poDate}</td>
            
            <td class="align-middle white-space-nowrap ps-2 productInStock" data-column="5">${item.note}</td>
            <td class="align-middle white-space-nowrap ps-2 productInStock" data-column="6">${item.product}</td>
            <td class="align-middle white-space-nowrap ps-2 productInStock" data-column="7">${item.quentity}</td>
            <td class="align-middle white-space-nowrap ps-2 productInStock" data-column="8">${item.price}</td>
            <td class="align-middle text-end white-space-nowrap pe-2 action" data-column="9">
               

                <div class="d-flex g-3 justify-content-end">
                       <button class="btn btn-phoenix-secondary fs-9 text-info purchase-btn"
                                title="Purchase" 
                                data-req-id="${item.purchaseId}"
                                ${item.status === true ? 'disabled' : ''}
                                >
                             ${item.status === true ? 'Already Purchased' : 'Add Purchase'}
                        </button>


                </div>
            </td>
        </tr>
    `;


    //<button class="btn btn-phoenix-secondary btn-icon me-2 fs-8 text-success approve-btn"
    //    title="Approved" data-bs-toggle="modal" data-bs-target="#productPurchase" data-id="${item.purchaseId}">
    //    <span class="far fa-check-square"></span>
    //</button>

    //<button class="btn btn-phoenix-secondary btn-icon fs-8 text-danger decline-btn"
    //    data-bs-toggle="tooltip" data-bs-placement="top" title="Decline" data-id="${item.id}">
    //    <span class="far fa-window-close"></span>
    //</button>
}


$(document).on('click', '.purchase-btn', function () {
    const purchaseId = $(this).data('req-id');
    window.location.href = `/PurchaseReceived/index/${purchaseId}`;
});

// Initialize event handlers
function initializeEventHandlers() {
    // Debounced search
    let searchTimeout;
    $('#searchInput').on('keyup', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function () {
            loadProductPurchases(1);
        }, 300);
    });

    // Product type filter
    $('#productTypeFilter').on('change', function () {
        loadProductPurchases(1);
    });

    // Page size change
    $('#pageSizeSelect').on('change', function () {
        loadProductPurchases(1);
    });

    // Manual purchase form submission
    $('#manualPurchase .btn-primary').on('click', function () {
        submitPurchaseForm();
    });

    // Approve button click
    $(document).on('click', '.approve-btn', function () {
        const id = $(this).data('id');
        const row = $(this).closest('tr');
        const productName = row.find('.productName').text();
        //const productId = row.find('.productId').text();
        const productId = row.data('product-id');
        const quantity = row.find('.productInStock').text();
        const reqId = row.find('.reqId').text();
        showDev(productId)

        populateApprovalModal(id, productId, quantity, reqId);
    });

    // Decline button click
    $(document).on('click', '.decline-btn', function () {
        const id = $(this).data('id');
        declinePurchase(id);
    });

    // Select all checkbox
    $('input[data-bulk-select]').on('change', function () {
        const isChecked = $(this).prop('checked');
        $('tbody input[type="checkbox"]').prop('checked', isChecked);
    });

    // Sorting
    $('#ProductPurchase .sort').on('click', function () {
        const sortBy = $(this).data('sort');
        const currentSortBy = $('#ProductPurchase').data('sortBy');
        const currentSortOrder = $('#ProductPurchase').data('sortOrder') || 'asc';
        const newSortOrder = (sortBy === currentSortBy && currentSortOrder === 'asc') ? 'desc' : 'asc';

        $('#ProductPurchase').data('sortBy', sortBy);
        $('#ProductPurchase').data('sortOrder', newSortOrder);

        // Update sort indicators
        $('#ProductPurchase .sort').removeClass('sort-asc sort-desc');
        $(this).addClass(newSortOrder === 'asc' ? 'sort-asc' : 'sort-desc');

        loadProductPurchases(1);
    });
}

// Update pagination
function updatePagination(totalCount, totalPages, currentPage) {
    const pageSize = parseInt($('#pageSizeSelect').val()) || 10;
    $('#showDown2').text(`Showing ${(currentPage - 1) * pageSize + 1} to ${Math.min(currentPage * pageSize, totalCount)} of ${totalCount}`);

    const pagination = $('.pagination');
    pagination.empty();

    const maxPagesToShow = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
    let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

    if (endPage - startPage + 1 < maxPagesToShow) {
        startPage = Math.max(1, endPage - maxPagesToShow + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
        const isActive = i === currentPage ? 'active' : '';
        pagination.append(`<li class="page-item ${isActive}"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`);
    }

    // Enable/disable prev/next buttons
    $('.page-link[data-list-pagination="prev"]').prop('disabled', currentPage === 1);
    $('.page-link[data-list-pagination="next"]').prop('disabled', currentPage === totalPages);
}

// Pagination click handlers
$(document).on('click', '.page-link[data-page]', function (e) {
    e.preventDefault();
    const page = $(this).data('page');
    loadProductPurchases(page);
});

$(document).on('click', '[data-list-pagination]', function (e) {
    e.preventDefault();
    const currentPage = parseInt($('.pagination .active .page-link').data('page')) || 1;
    const totalPages = parseInt($('.pagination .page-item:last .page-link').data('page')) || 1;
    const action = $(this).data('list-pagination');
    const newPage = action === 'prev' ? currentPage - 1 : currentPage + 1;

    if (newPage >= 1 && newPage <= totalPages) {
        loadProductPurchases(newPage);
    }
});

// Submit purchase form
function submitPurchaseForm() {
    const formData = {
        ProductID: $('#manualPurchase #productName').val(),
        Quantity: parseFloat($('#manualPurchase #quantity').val()),
        SupplierID: $('#manualPurchase #supplierName').val(),
        PurchaseBy: $('#manualPurchase #purchaserName').val(),
        CreatedBy: $('#manualPurchase #receiverName').val(),
        WarehouseID: $('#manualPurchase #warehouseName').val(),
        UnitPrice: parseFloat($('#manualPurchase #unitPrice').val()),
    };

    if (!validatePurchaseForm(formData)) {
        return;
    }

    $.ajax({
        url: '/ProductPurchase/CreatePurchase',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                showSuccess('Purchase created successfully!');
                $('#manualPurchase').modal('hide');
                clearPurchaseForm();
                loadProductPurchases();
            } else {
                showError('Failed to create purchase: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            showError('Error creating purchase: ' + error);
        }
    });
}

// Validate purchase form
function validatePurchaseForm(formData) {
    if (!formData.ProductID) {
        showError('Please select a product');
        return false;
    }
    if (!formData.Quantity || formData.Quantity <= 0) {
        showError('Please enter a valid quantity');
        return false;
    }
    if (!formData.Quantity || formData.Quantity <= 0) {
        showError('Please enter a valid quantity');
        return false;
    }
    if (!formData.SupplierID) {
        showError('Please select a supplier');
        return false;
    }
    if (!formData.PurchaseBy) {
        showError('Please select a purchaser');
        return false;
    }
    return true;
}

// Clear purchase form
function clearPurchaseForm() {
    $('#manualPurchase form')[0].reset();
    $('#manualPurchase select').val('').trigger('change');
}

// Populate approval modal
function populateApprovalModal(id, productId, quantity, reqId) {
    $('#productPurchase').data('purchase-id', id);
    const productSelect = $('#productName1');
    //productSelect.find('option').each(function () {
    //    if ($(this).text() === productName) {
    //        $(this).prop('selected', true);
    //        productSelect.trigger('change');
    //        return false;
    //    }
    //});

    showDev(productSelect)

    choiceManager.setChoiceValue("productName1", productId )
    $('#quantity1').val(quantity);
    $('#productPurchase .modal-title').text(`Approve Purchase - ${reqId}`);
    $('#productPurchase .btn-primary').text('Approve').removeClass('btn-primary').addClass('btn-success approve-final-btn');
}

// Handle final approval
$(document).on('click', '.approve-final-btn', function () {
    const purchaseId = $('#productPurchase').data('purchase-id');
    const formData = {
        id: purchaseId,
        ProductID: $('#productName1').val(),
        Quantity: parseFloat($('#quantity1').val()),
        SupplierID: $('#supplierName1').val(),
        PurchaseBy: $('#purchaserName1').val(),
        ReceivedBy: $('#receiverName1').val(),
        WarehouseID: $('#warehouseName1').val(),
        UnitPrice: parseFloat($('#unitPrice1').val())
    };

    approvePurchaseWithData(formData);
});

// Decline purchase
function declinePurchase(id) {
    if (confirm('Are you sure you want to decline this purchase?')) {
        $.ajax({
            url: '/ProductPurchase/DeclinePurchase',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    showSuccess('Purchase declined successfully!');
                    loadProductPurchases();
                } else {
                    showError('Failed to decline purchase: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                showError('Error declining purchase: ' + error);
            }
        });
    }
}

// Approve purchase
function approvePurchaseWithData(formData) {
    if (!validatePurchaseForm(formData)) {
        return;
    }

    $.ajax({
        url: '/ProductPurchase/ApprovePurchase',
        type: 'POST',
        data: formData,
        success: function (response) {
            if (response.success) {
                showSuccess('Purchase approved successfully!');
                //$('#productPurchase').modal('hide');
                resetApprovalModal();
                loadProductPurchases();
            } else {
                showError('Failed to approve purchase: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            showError('Error approving purchase: ' + error);
        }
    });
}

// Reset approval modal
function resetApprovalModal() {
    $('#productPurchase .modal-title').text('Product Purchase Information');
    $('#productPurchase .btn-success').text('Okay').removeClass('btn-success approve-final-btn').addClass('btn-primary');
    $('#productPurchase form')[0].reset();
    choiceManager.resetAllChoices();
}

// Utility functions
function showSuccess(message) {
    toastr.success(message); // Replace with your notification system
}

function showError(message) {
    showDev(message); // Replace with your notification system
}

// Initialize tooltips
function initializeTooltips() {
    $('[data-bs-toggle="tooltip"]').tooltip();
}

$(document).ready(function () {
    initializeTooltips();
});

// Export functions
window.ProductPurchase = {
    refresh: loadProductPurchases,
    load: loadProductPurchases
};