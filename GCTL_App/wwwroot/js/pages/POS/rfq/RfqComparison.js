
// Negotiate Button Functionality
document.getElementById('negotiateBtn').addEventListener('click', function () {
    const negotiateColumns = document.querySelectorAll('.negotiate-column');
    const isHidden = negotiateColumns[0].classList.contains('d-none');

    if (isHidden) {
        // Show negotiate columns
        negotiateColumns.forEach(col => col.classList.remove('d-none'));
        this.innerHTML = '<i class="bi bi-check-circle"></i> Negotiate (On)';
        this.classList.remove('btn-warning');
        this.classList.add('btn-success');
    } else {
        // Hide negotiate columns
        negotiateColumns.forEach(col => col.classList.add('d-none'));
        this.innerHTML = '<i class="bi bi-currency-exchange"></i> Negotiate';
        this.classList.remove('btn-success');
        this.classList.add('btn-warning');
    }
});

// Auto-calculate negotiate totals
document.querySelectorAll('input[type="number"]').forEach(input => {
    input.addEventListener('input', function () {
        const row = this.closest('tr');
        const priceInput = row.querySelectorAll('input[type="number"]')[0];
        const totalInput = row.querySelectorAll('input[type="number"]')[1];
        const qty = parseInt(row.querySelector('td:nth-child(2)').textContent);

        if (this === priceInput && totalInput) {
            const price = parseFloat(this.value) || 0;
            totalInput.value = (price * qty).toFixed(2);
        }
    });
});

// Update lowest prices automatically (for demo)
function updateLowestPrices() {
    // Clear all lowest price highlights
    document.querySelectorAll('.lowest-price').forEach(row => {
        row.classList.remove('lowest-price');
    });

    // For each product row (assuming same order in all tables)
    const vendorTables = document.querySelectorAll('.vendor-table');
    const productCount = 4; // We have 4 products

    for (let i = 0; i < productCount; i++) {
        let lowestPrice = Infinity;
        let lowestVendorIndex = -1;

        // Find lowest price for this product across vendors
        vendorTables.forEach((table, vendorIndex) => {
            const rows = table.querySelectorAll('tbody tr');
            if (rows[i]) {
                const priceCell = rows[i].querySelector('td:nth-child(1)');
                if (priceCell) {
                    const price = parseFloat(priceCell.textContent.replace('$', '').trim());
                    if (price < lowestPrice) {
                        lowestPrice = price;
                        lowestVendorIndex = vendorIndex;
                    }
                }
            }
        });

        // Highlight lowest price row
        if (lowestVendorIndex !== -1) {
            const lowestTable = vendorTables[lowestVendorIndex];
            const rows = lowestTable.querySelectorAll('tbody tr');
            if (rows[i]) {
                rows[i].classList.add('lowest-price');
            }
        }
    }
}

// Initial highlight of lowest prices
updateLowestPrices();
