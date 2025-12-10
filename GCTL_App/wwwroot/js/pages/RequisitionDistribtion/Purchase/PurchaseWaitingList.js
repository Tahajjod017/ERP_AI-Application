$(document).ready(function () {
    showDev("PurchaseWaitingList.js loaded");

    // Initialize the page
    initializePage();

    //#region download pdf
    $(document).on('click',".btnExportPDFdownload", function () {
        debugger;
        const id = $(this).data("id");
        fetch(`/PurchaseWaitingList/GeneratePDF?id=${id}`, { method: "POST" })
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                const now = new Date();
                const formattedDate = now.getFullYear().toString() +
                    String(now.getMonth() + 1).padStart(2, '0') +
                    String(now.getDate()).padStart(2, '0') + "_" +
                    String(now.getHours()).padStart(2, '0') +
                    String(now.getMinutes()).padStart(2, '0') +
                    String(now.getSeconds()).padStart(2, '0');
                a.download = `PurchaseWaitingList_${formattedDate}.pdf`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
            })
            .catch(() => {
                toastr.error("Error crating PDF");
            });
    });

    //#endregion
});

// Global variables
let currentFilters = {
    searchTerm: '',
    productTypeId: null,
    projectId: null,
    productId: null,
    sortBy: 'reqId',
    sortDirection: 'asc'
};

let currentPagination = {
    currentPage: 1,
    pageSize: 10,
    totalItems: 0,
    totalPages: 0
};

let isLoading = false;

function initializePage() {
    // Load dropdown data first, then load table data
    loadDropdownData().then(() => {
        bindEvents();
        loadPurchaseWaitingList();
    });
}

function loadDropdownData() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/PurchaseWaitingList/GetDropdownData',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    populateDropdowns(response);
                    resolve(response);
                } else {
                    showError(response.message || 'Failed to load dropdown data');
                    reject(response);
                }
            },
            error: function (xhr, status, error) {
                showError('Failed to load dropdown data: ' + error);
                reject(error);
            }
        });
    });
}

function populateDropdowns(data) {
    // Populate Product Type dropdown
    const productTypeSelect = $('#productType');
    productTypeSelect.empty();
    productTypeSelect.append('<option value="">Search by Product Type</option>');
    if (data.productTypes && data.productTypes.length > 0) {
        data.productTypes.forEach(item => {
            productTypeSelect.append(`<option value="${item.id}">${item.name}</option>`);
        });
    }

    // Populate Project dropdown
    const projectSelect = $('#project');
    projectSelect.empty();
    projectSelect.append('<option value="">Search by Project</option>');
    if (data.projects && data.projects.length > 0) {
        data.projects.forEach(item => {
            projectSelect.append(`<option value="${item.id}">${item.name}</option>`);
        });
    }

    // Populate Product dropdown
    const productSelect = $('#product');
    productSelect.empty();
    productSelect.append('<option value="">Search by Product</option>');
    if (data.products && data.products.length > 0) {
        data.products.forEach(item => {
            productSelect.append(`<option value="${item.id}">${item.name}</option>`);
        });
    }
}

function bindEvents() {
    // Search input with debounce
    $('#searchInput').on('keyup', debounce(function () {
        currentFilters.searchTerm = $(this).val();
        currentPagination.currentPage = 1;
        loadPurchaseWaitingList();
    }, 300));

    // Filter dropdowns
    $('#productType').on('change', function () {
        currentFilters.productTypeId = $(this).val() ? parseInt($(this).val()) : null;
        currentPagination.currentPage = 1;
        loadPurchaseWaitingList();
    });

    $('#project').on('change', function () {
        currentFilters.projectId = $(this).val() ? parseInt($(this).val()) : null;
        currentPagination.currentPage = 1;
        loadPurchaseWaitingList();
    });

    $('#product').on('change', function () {
        currentFilters.productId = $(this).val() ? parseInt($(this).val()) : null;
        currentPagination.currentPage = 1;
        loadPurchaseWaitingList();
    });

    // Page size selector
    $('#pageSizeSelect').on('change', function () {
        currentPagination.pageSize = parseInt($(this).val());
        currentPagination.currentPage = 1;
        loadPurchaseWaitingList();
    });

    // Purchase button click event
    $(document).on('click', '[data-req-id]', function () {
        const reqId = $(this).data('req-id');
        handlePurchaseClick(reqId);
    });
}

function loadPurchaseWaitingList() {
    if (isLoading) return;

    isLoading = true;
    showLoadingState();

    const requestData = {
        searchTerm: currentFilters.searchTerm,
        productTypeId: currentFilters.productTypeId,
        projectId: currentFilters.projectId,
        productId: currentFilters.productId,
        sortBy: currentFilters.sortBy,
        sortDirection: currentFilters.sortDirection,
        page: currentPagination.currentPage,
        pageSize: currentPagination.pageSize
    };

    $.ajax({
        url: '/PurchaseWaitingList/GetPurchaseWaitingList',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(requestData),
        success: function (response) {
            isLoading = false;
            hideLoadingState();

            if (response.success) {
                // Update pagination info
                currentPagination = {
                    ...currentPagination,
                    ...response.pagination
                };
                
                renderTable(response.data);
                renderPagination(response.pagination);
                updateShowingInfo(response.pagination);
            } else {
                showError(response.message || 'Failed to load purchase waiting list');
                renderEmptyTable('Error loading data');
            }
        },
        error: function (xhr, status, error) {
            isLoading = false;
            hideLoadingState();
            showError('Failed to load purchase waiting list: ' + error);
            renderEmptyTable('Error loading data');
        }
    });
}

function showLoadingState() {
    const tbody = $('#product-Requisition-history-table');
    tbody.empty();
    tbody.append(`
        <tr>
            <td colspan="8" class="text-center py-4">
                <div class="d-flex flex-column align-items-center">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                    <p class="text-muted mt-2">Loading purchase requests...</p>
                </div>
            </td>
        </tr>
    `);
}

function hideLoadingState() {
    // Loading state will be replaced by renderTable or renderEmptyTable
}

function renderTable(data) {
    const tbody = $('#product-Requisition-history-table');
    tbody.empty();

    if (!data || data.length === 0) {
        renderEmptyTable();
        return;
    }

    showDev( data , 'datd');
    //console.log('ddddd',data)
    console.log(data)

    data.forEach(item => {
        const row = `
            <tr class="position-static">
                <td class="align-middle white-space-nowrap ps-0 reqId">${item.poId}</td>
                <td class="align-middle white-space-nowrap ps-0 reqId">${item.reqId}</td>
                <td class="align-middle white-space-nowrap ps-2 purpose">${item.projectName}</td>
                <td class="align-middle white-space-nowrap ps-2 purpose">${item.requestedBy}</td>
                <td class="align-middle white-space-nowrap ps-2 type">${item.requestDateFormatted}</td>
                <td class="align-middle white-space-nowrap ps-2 productName">${item.productType}</td>
                <td class="align-middle white-space-nowrap ps-2 productInStock">${item.productName}</td>
                <td class="align-middle white-space-nowrap ps-2 productInStock">
                       <div class = "badge badge-phoenix badge-phoenix-success">${item.status}</div> 
                </td>
                <td class="align-middle white-space-nowrap ps-2 productInStock">${item.quantity}</td>
                <td class="align-middle white-space-nowrap ps-2 pdfExp">
                    <button class="btn btn-secondary btnExportPDFdownload" data-id="${item.purchaseId}"><i class="fa-solid fa-file-pdf"></i></button>
                </td>



                <td class="align-middle text-end white-space-nowrap pe-2 action">
                    <div class="d-flex g-3 justify-content-end">
                       <button class="btn btn-phoenix-secondary fs-9 text-info purchase-btn"
                                title="Purchase" data-bs-toggle="modal" 
                                data-bs-target="#productDistribution"
                                data-req-id="${item.purchaseId}"
                                ${item.priority === '1' ? 'disabled' : ''}
                                >
                            Purchase
                        </button> 
                        

                        <!-- <button class="btn btn-phoenix-secondary fs-9 text-info purchase-btn1"
                                title="Purchase" data-bs-toggle="modal" 
                                data-bs-target="#productDistribution"
                                data-req-id="${item.purchaseId}">
                            Purchase
                        </button> -->
                    </div>
                </td>
            </tr>
        `;
        tbody.append(row);
    });
    DynamicTableDrag.refreshTableSettings('PurchaseWaitingList');


}

$(document).on('click', '.purchase-btn', function () {
    const purchaseId = $(this).data('req-id');
    window.location.href = `/CreatePurchaseOrder/index/${purchaseId}`;
});


function renderEmptyTable(message = null) {
    const tbody = $('#product-Requisition-history-table');
    tbody.empty();

    const defaultMessage = message || 'No purchase requests found';
    const subMessage = message ? 'Please try again later' : 'Try adjusting your search criteria or filters';

    tbody.append(`
        <tr>
            <td colspan="8" class="text-center py-4">
                <div class="d-flex flex-column align-items-center">
                    <i class="fas fa-search fa-3x text-muted mb-3"></i>
                    <h5 class="text-muted">${defaultMessage}</h5>
                    <p class="text-muted">${subMessage}</p>
                </div>
            </td>
        </tr>
    `);
}

function renderPagination(pagination) {
    if (pagination.totalPages <= 1) {
        $('.pagination-container').hide();
        return;
    }

    $('.pagination-container').show();

    const paginationContainer = $('.pagination ul');

    // remove only old page numbers, keep prev/next
    paginationContainer.find('li').not('[data-list-pagination="prev"], [data-list-pagination="next"]').remove();


  



    const startPage = Math.max(1, pagination.currentPage - 2);
    const endPage = Math.min(pagination.totalPages, pagination.currentPage + 2);

    // Add first page and ellipsis if needed
    if (startPage > 1) {
        paginationContainer.append(`
            <li class="page-item">
                <a class="page-link" href="#" onclick="changePage(1)">1</a>
            </li>
        `);
        if (startPage > 2) {
            paginationContainer.append(`
                <li class="page-item disabled">
                    <a class="page-link" href="#">...</a>
                </li>
            `);
        }
    }

    //for (let i = startPage; i <= endPage; i++) {
    //    const activeClass = i === pagination.currentPage ? 'active' : '';
    //    paginationContainer.append(`
    //        <li class="page-item ${activeClass}">
    //            <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
    //        </li>
    //    `);
    //}

    for (let i = startPage; i <= endPage; i++) {
        const activeClass = i === pagination.currentPage ? 'active' : '';
        const pageItem = `
        <li class="page-item ${activeClass}" data-page="${i}">
            <a class="page-link" href="#" onclick="changePage(${i})">${i}</a>
        </li>
    `;
        paginationContainer.find('[data-list-pagination="next"]').before(pageItem);
    }

    // Add last page and ellipsis if needed
    if (endPage < pagination.totalPages) {
        if (endPage < pagination.totalPages - 1) {
            paginationContainer.append(`
                <li class="page-item disabled">
                    <a class="page-link" href="#">...</a>
                </li>
            `);
        }
        paginationContainer.append(`
            <li class="page-item">
                <a class="page-link" href="#" onclick="changePage(${pagination.totalPages})">${pagination.totalPages}</a>
            </li>
        `);
    }

    // Update navigation buttons
    const prevBtn = $('[data-list-pagination="prev"]');
    const nextBtn = $('[data-list-pagination="next"]');

    if (pagination.hasPrevious) {
        prevBtn.removeClass('disabled').attr('onclick', `changePage(${pagination.currentPage - 1})`);
    } else {
        prevBtn.addClass('disabled').removeAttr('onclick');
    }

    if (pagination.hasNext) {
        nextBtn.removeClass('disabled').attr('onclick', `changePage(${pagination.currentPage + 1})`);
    } else {
        nextBtn.addClass('disabled').removeAttr('onclick');
    }
}

function updateShowingInfo(pagination) {
    $('#showDown2').text(`Showing ${pagination.startItem} to ${pagination.endItem} of ${pagination.totalItems}`);
}

function changePage(page) {
    if (page >= 1 && page <= currentPagination.totalPages && page !== currentPagination.currentPage) {
        currentPagination.currentPage = page;
        loadPurchaseWaitingList();
    }
}

function sortTable(column) {
    if (currentFilters.sortBy === column) {
        currentFilters.sortDirection = currentFilters.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
        currentFilters.sortBy = column;
        currentFilters.sortDirection = 'asc';
    }

    updateSortIcons(column);
    currentPagination.currentPage = 1;
    loadPurchaseWaitingList();
}

function updateSortIcons(activeColumn) {
    // Reset all sort icons
    $('[id^="sort-"]').removeClass('fa-sort-up fa-sort-down').addClass('fa-sort');

    // Set active column icon
    const iconClass = currentFilters.sortDirection === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
    $(`#sort-${activeColumn}`).removeClass('fa-sort').addClass(iconClass);
}

function handlePurchaseClick(reqId) {
    $.ajax({
        url: '/PurchaseWaitingList/GetPurchaseRequest',
        type: 'GET',
        data: { reqId: reqId },
        success: function (response) {
            if (response.success) {
                console.log('Purchase request details:', response.data);

                // Map the controller data to PDF template structure
                const pdfData = {
                    poNumber: response.data.poNumber,
                    status: response.data.status,
                    poDate: response.data.poDate,
                    dueDate: response.data.dueDate,
                    workOrder: response.data.workOrder,
                    reference: response.data.reference,
                    vendorDetails: response.data.vendorDetails,
                    shipToDetails: response.data.shipToDetails,
                    items: response.data.items,
                    summary: response.data.summary,
                    notes: response.data.notes,
                    terms: response.data.terms,
                    barcodeText: response.data.barcodeText,
                    footer: response.data.footer,
                    // Additional fields for compatibility
                    reqId: response.data.reqId,
                    projectName: response.data.projectName,
                    productName: response.data.productName,
                    productType: response.data.productType,
                    quantity: response.data.quantity,
                    estimatedCost: response.data.estimatedCost
                };

                generatePOpdf(pdfData);
            } else {
                showError(response.message || 'Failed to load purchase request details');
            }
        },
        error: function (xhr, status, error) {
            showError('Failed to load purchase request details: ' + error);
        }
    });
}

function populatePurchaseModal(data) {
    // Example modal population - adjust based on your modal structure
    $('#purchaseReqId').text(data.reqId);
    $('#purchaseProject').text(data.projectName);
    $('#purchaseProduct').text(data.productName);
    $('#purchaseQuantity').text(data.quantity);
    $('#purchaseEstimatedCost').text('$' + data.estimatedCost.toLocaleString());
    // Add more fields as needed
}

function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

function showError(message) {
    console.error(message);

    // You can customize this to show error messages in your preferred way
    // Example using toast notification (if you have a toast library)
    // toastr.error(message);

    // Or show in a div
    // $('#errorContainer').html(`<div class="alert alert-danger">${message}</div>`);

    // For now, just show an alert
    alert('Error: ' + message);
}

function showSuccess(message) {
    console.log('Success:', message);

    // Example using toast notification
    // toastr.success(message);

    // For now, just log to console
}

function showDev(message) {
    console.log('DEV:', message);
}

// Global functions for HTML onclick events
window.changePage = changePage;
window.sortTable = sortTable;




//#region PDF

//#region PDF

function generatePOpdf(purchaseData) {
    const { jsPDF } = window.jspdf;
    if (!jsPDF || !html2canvas) {
        showError('Required libraries (jsPDF or html2canvas) are not loaded');
        return;
    }

    // Populate the PDF template
    populatePDFTemplate(purchaseData);

    const pdfTemplate = document.getElementById('pdfTemplate');
    const originalDisplay = pdfTemplate.style.display;

    // Important: Set proper dimensions and make visible
    pdfTemplate.style.display = 'block';
    pdfTemplate.style.position = 'absolute';
    pdfTemplate.style.left = '0';
    pdfTemplate.style.top = '0';
    pdfTemplate.style.width = '210mm';
    pdfTemplate.style.height = '297mm';
    pdfTemplate.style.overflow = 'visible';

    // Add a small delay to ensure DOM is rendered
    setTimeout(() => {
        html2canvas(pdfTemplate, {
            scale: 2,
            useCORS: true,
            logging: true,
            backgroundColor: '#ffffff',
            width: pdfTemplate.scrollWidth,
            height: pdfTemplate.scrollHeight,
            scrollX: 0,
            scrollY: 0,
            windowWidth: pdfTemplate.scrollWidth,
            windowHeight: pdfTemplate.scrollHeight
        }).then(canvas => {
            const imgData = canvas.toDataURL('image/png');
            const pdf = new jsPDF('p', 'mm', 'a4');
            const imgWidth = 210;
            const imgHeight = (canvas.height * imgWidth) / canvas.width;

            pdf.addImage(imgData, 'PNG', 0, 0, imgWidth, imgHeight);

            const poId = purchaseData.poNumber || 'PO';
            pdf.save(`PurchaseOrder_${poId}.pdf`);

            // Restore original state
            pdfTemplate.style.display = originalDisplay;
            pdfTemplate.style.position = '';
            pdfTemplate.style.left = '';
            pdfTemplate.style.top = '';

            showSuccess(`Purchase Order PDF generated successfully for ${poId}`);
        }).catch(error => {
            pdfTemplate.style.display = originalDisplay;
            pdfTemplate.style.position = '';
            pdfTemplate.style.left = '';
            pdfTemplate.style.top = '';
            showError('Failed to generate PDF: ' + error.message);
            console.error('PDF generation error:', error);
        });
    }, 500);
}

// Update the populatePDFTemplate function to ensure exact HTML structure
function populatePDFTemplate(data) {
    // Clear and rebuild the items table to ensure proper structure
    const itemsBody = document.getElementById('itemsBody');
    itemsBody.innerHTML = '';

    if (data.items && Array.isArray(data.items)) {
        data.items.forEach(item => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td><span class="item-id">${item.id || 'ITM-001'}</span></td>
                <td>${item.description || 'Product Description'}</td>
                <td>${item.unit || 'Unit'}</td>
                <td class="text-center">${item.quantity || 0}</td>
                <td class="text-right">${item.price ? '৳' + Number(item.price).toLocaleString() : '৳0'}</td>
                <td class="text-right">${item.total ? '৳' + Number(item.total).toLocaleString() : '৳0'}</td>
            `;
            itemsBody.appendChild(row);
        });
    } else {
        // Default items matching your original HTML
        itemsBody.innerHTML = `
            <tr>
                <td><span class="item-id">ITM-001</span></td>
                <td>Dell OptiPlex 7090</td>
                <td>PC</td>
                <td class="text-center">5</td>
                <td class="text-right">৳85,000</td>
                <td class="text-right">৳425,000</td>
            </tr>
            <tr>
                <td><span class="item-id">ITM-002</span></td>
                <td>HP LaserJet M404dn</td>
                <td>PC</td>
                <td class="text-center">2</td>
                <td class="text-right">৳35,000</td>
                <td class="text-right">৳70,000</td>
            </tr>
        `;
    }

    // Update all other fields
    document.getElementById('poNumber').textContent = data.poNumber || 'PO-2025-0001';
    document.querySelector('.status-badge').textContent = data.status || 'DRAFT';
    document.getElementById('poDate').textContent = data.poDate || 'Sept 23, 2025';
    document.getElementById('dueDate').textContent = data.dueDate || 'Oct 15, 2025';
    document.getElementById('workOrder').textContent = data.workOrder || 'WO-2025-045';
    document.getElementById('reference').textContent = data.reference || 'REF-IT-EQUIPMENT';

    // Vendor Details
    const vendorDetails = document.getElementById('vendorDetails');
    if (data.vendorDetails) {
        vendorDetails.innerHTML = data.vendorDetails;
    } else {
        vendorDetails.innerHTML = `
            <strong>ABC Supplies Ltd.</strong><br>
            123 Business Street<br>
            Dhaka-1212, Bangladesh<br>
            <strong>Contact:</strong> John Doe<br>
            <strong>Phone:</strong> +88 01234-567890
        `;
    }

    // Ship To Details
    const shipToDetails = document.getElementById('shipToDetails');
    if (data.shipToDetails) {
        shipToDetails.innerHTML = data.shipToDetails;
    } else {
        shipToDetails.innerHTML = `
            <strong>GCTL InfoSys - Warehouse</strong><br>
            House-42 (5th Floor), Sector-4<br>
            Uttara, Dhaka-1230<br>
            <strong>Contact:</strong> Warehouse Manager
        `;
    }

    // Summary Table
    const summaryBody = document.getElementById('summaryBody');
    if (data.summary) {
        summaryBody.innerHTML = `
            <tr>
                <td>Subtotal:</td>
                <td class="text-right">${data.summary.subtotal || '৳495,000'}</td>
            </tr>
            <tr>
                <td>Tax (15%):</td>
                <td class="text-right">${data.summary.tax || '৳74,250'}</td>
            </tr>
            <tr class="grand-total">
                <td>Grand Total:</td>
                <td class="text-right">${data.summary.grandTotal || '৳569,250'}</td>
            </tr>
        `;
    } else {
        summaryBody.innerHTML = `
            <tr>
                <td>Subtotal:</td>
                <td class="text-right">৳495,000</td>
            </tr>
            <tr>
                <td>Tax (15%):</td>
                <td class="text-right">৳74,250</td>
            </tr>
            <tr class="grand-total">
                <td>Grand Total:</td>
                <td class="text-right">৳569,250</td>
            </tr>
        `;
    }

    document.getElementById('notes').textContent = data.notes || 'Please deliver items in original packaging. Setup required for desktops.';
    document.getElementById('terms').textContent = data.terms || 'Payment Net 30 | 1yr Warranty | Free Dhaka delivery';

    const barcodeImg = document.getElementById('barcodeImg');
    const barcodeText = data.barcodeText || data.poNumber || 'PO-2025-0001';
    barcodeImg.src = `https://bwipjs-api.metafloor.com/?bcid=code128&text=${encodeURIComponent(barcodeText)}&scale=2&height=50`;

    document.getElementById('footer').textContent = data.footer || `Generated on ${new Date().toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })} | Page 1 of 1`;
}



// Helper function to populate the PDF template with static data from controller
function populatePDFTemplate(data) {
    // PO Number - Use static data from controller
    document.getElementById('poNumber').textContent = data.poNumber || 'PO-2025-0001';

    // Status - Use static data from controller
    document.querySelector('.status-badge').textContent = data.status || 'DRAFT';

    // Dates - Use static data from controller
    document.getElementById('poDate').textContent = data.poDate || 'Sept 23, 2025';
    document.getElementById('dueDate').textContent = data.dueDate || 'Oct 15, 2025';

    // Work Order - Use static data from controller
    document.getElementById('workOrder').textContent = data.workOrder || 'WO-2025-045';

    // Reference - Use static data from controller
    document.getElementById('reference').textContent = data.reference || 'REF-IT-EQUIPMENT';

    // Vendor Information - Use static data from controller
    document.getElementById('vendorDetails').innerHTML = data.vendorDetails || `
        <strong>ABC Supplies Ltd.</strong><br>
        123 Business Street<br>
        Dhaka-1212, Bangladesh<br>
        <strong>Contact:</strong> John Doe<br>
        <strong>Phone:</strong> +88 01234-567890
    `;

    // Ship To Information - Use static data from controller
    document.getElementById('shipToDetails').innerHTML = data.shipToDetails || `
        <strong>GCTL InfoSys - Warehouse</strong><br>
        House-42 (5th Floor), Sector-4<br>
        Uttara, Dhaka-1230<br>
        <strong>Contact:</strong> Warehouse Manager
    `;

    // Items Table - Use static data from controller (multiple items support)
    const itemsBody = document.getElementById('itemsBody');
    if (data.items && Array.isArray(data.items)) {
        itemsBody.innerHTML = data.items.map(item => `
            <tr>
                <td><span class="item-id">${item.id || 'ITM-001'}</span></td>
                <td>${item.description || 'Dell OptiPlex 7090'}</td>
                <td>${item.unit || 'PC'}</td>
                <td class="text-center">${item.quantity || 5}</td>
                <td class="text-right">${item.price ? '৳' + Number(item.price).toLocaleString() : '৳85,000'}</td>
                <td class="text-right">${item.total ? '৳' + Number(item.total).toLocaleString() : '৳425,000'}</td>
            </tr>
        `).join('');
    } else {
        // Default items if no data provided
        itemsBody.innerHTML = `
            <tr>
                <td><span class="item-id">ITM-001</span></td>
                <td>Dell OptiPlex 7090</td>
                <td>PC</td>
                <td class="text-center">5</td>
                <td class="text-right">৳85,000</td>
                <td class="text-right">৳425,000</td>
            </tr>
            <tr>
                <td><span class="item-id">ITM-002</span></td>
                <td>HP LaserJet M404dn</td>
                <td>PC</td>
                <td class="text-center">2</td>
                <td class="text-right">৳35,000</td>
                <td class="text-right">৳70,000</td>
            </tr>
        `;
    }

    // Summary Table - Use static data from controller or calculate from items
    const summaryBody = document.getElementById('summaryBody');
    if (data.summary) {
        summaryBody.innerHTML = `
            <tr>
                <td>Subtotal:</td>
                <td class="text-right">${data.summary.subtotal || '৳495,000'}</td>
            </tr>
            <tr>
                <td>Tax (15%):</td>
                <td class="text-right">${data.summary.tax || '৳74,250'}</td>
            </tr>
            <tr class="grand-total">
                <td>Grand Total:</td>
                <td class="text-right">${data.summary.grandTotal || '৳569,250'}</td>
            </tr>
        `;
    } else {
        // Default summary
        summaryBody.innerHTML = `
            <tr>
                <td>Subtotal:</td>
                <td class="text-right">৳495,000</td>
            </tr>
            <tr>
                <td>Tax (15%):</td>
                <td class="text-right">৳74,250</td>
            </tr>
            <tr class="grand-total">
                <td>Grand Total:</td>
                <td class="text-right">৳569,250</td>
            </tr>
        `;
    }

    // Notes - Use static data from controller
    document.getElementById('notes').textContent = data.notes || 'Please deliver items in original packaging. Setup required for desktops.';

    // Terms - Use static data from controller
    document.getElementById('terms').textContent = data.terms || 'Payment Net 30 | 1yr Warranty | Free Dhaka delivery';

    // Barcode - Use static data from controller
    const barcodeImg = document.getElementById('barcodeImg');
    const barcodeText = data.barcodeText || data.poNumber || 'PO-2025-0001';
    barcodeImg.src = `https://bwipjs-api.metafloor.com/?bcid=code128&text=${encodeURIComponent(barcodeText)}&scale=2&height=50`;

    // Footer - Use static data from controller
    document.getElementById('footer').textContent = data.footer || `Generated on Sept 23, 2025 | Page 1 of 1`;
}

// Update handlePurchaseClick to generate PDF with static data
function handlePurchaseClick(reqId) {
    $.ajax({
        url: '/PurchaseWaitingList/GetPurchaseRequest',
        type: 'GET',
        data: { reqId: reqId },
        success: function (response) {
            if (response.success) {
                console.log('Purchase request details:', response.data);

                // Ensure the response data matches the static structure from your HTML
                const pdfData = {
                    poNumber: response.data.poNumber || 'PO-2025-0001',
                    status: response.data.status || 'DRAFT',
                    poDate: response.data.poDate || 'Sept 23, 2025',
                    dueDate: response.data.dueDate || 'Oct 15, 2025',
                    workOrder: response.data.workOrder || 'WO-2025-045',
                    reference: response.data.reference || 'REF-IT-EQUIPMENT',
                    vendorDetails: response.data.vendorDetails || null,
                    shipToDetails: response.data.shipToDetails || null,
                    items: response.data.items || null,
                    summary: response.data.summary || null,
                    notes: response.data.notes || null,
                    terms: response.data.terms || null,
                    barcodeText: response.data.barcodeText || null,
                    footer: response.data.footer || null
                };

                generatePOpdf(pdfData);
            } else {
                showError(response.message || 'Failed to load purchase request details');
            }
        },
        error: function (xhr, status, error) {
            showError('Failed to load purchase request details: ' + error);
        }
    });
}

// Update the purchase button click event to prevent redirection
$(document).on('click', '.purchase-btn1', function (e) {
    e.preventDefault(); // Prevent default behavior
    const purchaseId = $(this).data('req-id');
    handlePurchaseClick(purchaseId); // Call the updated function
});

//#endregion