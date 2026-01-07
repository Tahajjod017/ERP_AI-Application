
// Global state
let selectedSubRfqs = new Map(); // parentId -> Set of subRfqIds
let comparisonSelection = new Set(); // All selected sub-RFQs across all parents

$(document).ready(function () {
    // Initialize state from checkboxes
    initializeSelectionState();
    updateUI();
});

function initializeSelectionState() {
    selectedSubRfqs.clear();
    comparisonSelection.clear();

    $('.sub-rfq-checkbox-input:checked').each(function () {
        const subId = parseInt($(this).data('sub-id'));
        const parentId = parseInt($(this).data('parent-id'));

        addToSelection(subId, parentId);
    });
}

function addToSelection(subId, parentId) {
    if (!selectedSubRfqs.has(parentId)) {
        selectedSubRfqs.set(parentId, new Set());
    }
    selectedSubRfqs.get(parentId).add(subId);
    comparisonSelection.add(subId);

    // Update parent checkbox if all sub-RFQs are selected
    updateParentCheckbox(parentId);
}

function removeFromSelection(subId, parentId) {
    if (selectedSubRfqs.has(parentId)) {
        selectedSubRfqs.get(parentId).delete(subId);
        if (selectedSubRfqs.get(parentId).size === 0) {
            selectedSubRfqs.delete(parentId);
        }
    }
    comparisonSelection.delete(subId);

    // Update parent checkbox
    updateParentCheckbox(parentId);
}

function updateParentCheckbox(parentId) {
    const $parentCard = $(`[data-parent-id="${parentId}"]`);
    const subCount = $parentCard.find('.sub-rfq-checkbox-input').length;
    const selectedCount = selectedSubRfqs.get(parentId)?.size || 0;

    const $parentCheckbox = $parentCard.find('.parent-checkbox-input');
    const $selectAllCheckbox = $parentCard.find('.select-all-sub');

    // Update parent checkbox
    if (selectedCount === 0) {
        $parentCheckbox.prop('checked', false);
        $parentCheckbox.prop('indeterminate', false);
    } else if (selectedCount === subCount) {
        $parentCheckbox.prop('checked', true);
        $parentCheckbox.prop('indeterminate', false);
    } else {
        $parentCheckbox.prop('checked', false);
        $parentCheckbox.prop('indeterminate', true);
    }

    // Update select-all checkbox
    $selectAllCheckbox.prop('checked', selectedCount === subCount);
}

// Toggle parent expansion
function toggleParent(parentId) {
    const $parentCard = $(`[data-parent-id="${parentId}"]`);
    const $header = $parentCard.find('.parent-header');
    const $container = $parentCard.find('.sub-rfq-container');
    const $icon = $header.find('.expand-icon i');

    if ($header.hasClass('expanded')) {
        $header.removeClass('expanded');
        $container.slideUp(300);
        $icon.removeClass('fa-chevron-down').addClass('fa-chevron-right');
    } else {
        $header.addClass('expanded');
        $container.slideDown(300);
        $icon.removeClass('fa-chevron-right').addClass('fa-chevron-down');
    }
}

// Parent checkbox change
function onParentCheckboxChange(parentId, isChecked) {
    const $parentCard = $(`[data-parent-id="${parentId}"]`);
    const $checkboxes = $parentCard.find('.sub-rfq-checkbox-input');

    if (isChecked) {
        // Select all sub-RFQs
        $checkboxes.prop('checked', true);
        $checkboxes.each(function () {
            const subId = parseInt($(this).data('sub-id'));
            addToSelection(subId, parentId);
        });
    } else {
        // Deselect all sub-RFQs
        $checkboxes.prop('checked', false);
        const subIds = Array.from(selectedSubRfqs.get(parentId) || []);
        subIds.forEach(subId => removeFromSelection(subId, parentId));
    }

    updateUI();
}

// Sub-RFQ checkbox change
function onSubRfqCheckboxChange(subId, parentId, isChecked) {
    if (isChecked) {
        addToSelection(subId, parentId);
    } else {
        removeFromSelection(subId, parentId);
    }

    updateUI();
}

// Toggle all sub-RFQs in a parent
function toggleAllSubRfqs(parentId, isChecked) {
    const $parentCard = $(`[data-parent-id="${parentId}"]`);
    const $checkboxes = $parentCard.find('.sub-rfq-checkbox-input');

    $checkboxes.prop('checked', isChecked);

    if (isChecked) {
        $checkboxes.each(function () {
            const subId = parseInt($(this).data('sub-id'));
            addToSelection(subId, parentId);
        });
    } else {
        const subIds = Array.from(selectedSubRfqs.get(parentId) || []);
        subIds.forEach(subId => removeFromSelection(subId, parentId));
    }

    updateUI();
}

// Update UI based on selection
function updateUI() {
    // Update comparison bar
    updateComparisonBar();

    // Update parent-level compare sections
    updateParentCompareSections();

    // Update bulk actions bar
    updateBulkActionsBar();
}

function updateComparisonBar() {
    const totalSelected = comparisonSelection.size;
    const $comparisonBar = $('#comparisonBar');
    const $compareButton = $('#compareButton');
    const $comparisonCount = $('#comparisonCount');
    const $selectedItemsList = $('#selectedItemsList');

    $comparisonCount.text(totalSelected);

    if (totalSelected > 0) {
        $comparisonBar.addClass('active');
        $compareButton.prop('disabled', false);

        // Update selected items list
        $selectedItemsList.empty();
        comparisonSelection.forEach(subId => {
            const $row = $(`[data-sub-id="${subId}"]`);
            const reference = $row.find('td:nth-child(2) strong').text();
            const vendor = $row.find('td:nth-child(3)').text();

            $selectedItemsList.append(`
    <div class="selected-item">
        ${reference} (${vendor})
        <button type="button" class="remove-selected" onclick="removeFromComparison(${subId})">
            ×
        </button>
    </div>
    `);
        });
    } else {
        $comparisonBar.removeClass('active');
        $compareButton.prop('disabled', true);
        $selectedItemsList.empty();
    }
}

function updateParentCompareSections() {
    selectedSubRfqs.forEach((subIds, parentId) => {
        const $compareSection = $(`#compareSection-${parentId}`);
        const $selectedCount = $(`#selectedCount-${parentId}`);
        const selectedCount = subIds.size;

        if (selectedCount >= 2) {
            $compareSection.show();
            $selectedCount.text(`${selectedCount} sub-RFQs selected`);
        } else {
            $compareSection.hide();
        }
    });

    // Hide sections for parents with no selections
    $('.parent-compare-section').each(function () {
        const parentId = parseInt($(this).attr('id').replace('compareSection-', ''));
        if (!selectedSubRfqs.has(parentId) || selectedSubRfqs.get(parentId).size < 2) {
            $(this).hide();
        }
    });
}

function updateBulkActionsBar() {
    const $bulkActions = $('#bulkActions');
    const $bulkSelectedCount = $('#bulkSelectedCount');
    const totalSelected = comparisonSelection.size;

    if (totalSelected > 0) {
        $bulkActions.addClass('active');
        $bulkSelectedCount.text(`${totalSelected} selected`);
    } else {
        $bulkActions.removeClass('active');
    }
}

// Remove single item from comparison
function removeFromComparison(subId) {
    const $checkbox = $(`[data-sub-id="${subId}"] .sub-rfq-checkbox-input`);
    const parentId = parseInt($checkbox.data('parent-id'));

    $checkbox.prop('checked', false);
    removeFromSelection(subId, parentId);
    updateUI();
}

// Clear all selection
function clearSelection() {
    $('.sub-rfq-checkbox-input, .parent-checkbox-input, .select-all-sub').prop('checked', false);
    selectedSubRfqs.clear();
    comparisonSelection.clear();
    updateUI();
}

// Compare selected sub-RFQs from same parent
function compareSelectedSubRfqs(parentId) {
    const subIds = Array.from(selectedSubRfqs.get(parentId) || []);
    if (subIds.length < 2) {
        toastr.warning('@Html.SmartLocalize("Please select at least 2 sub-RFQs to compare")');
        return;
    }

    // For now, just show an alert. You'll implement the comparison page later.
    alert(`Comparing ${subIds.length} sub-RFQs from parent RFQ ${parentId}: ${subIds.join(', ')}`);

    // This will go to your comparison page:
    // window.location.href = `/Rfq/Compare?subRfqIds=${subIds.join(',')}`;
}

// Go to comparison page (for cross-parent comparison)
function goToComparisonPage() {
    if (comparisonSelection.size < 2) {
        toastr.warning('@Html.SmartLocalize("Please select at least 2 RFQs to compare")');
        return;
    }

    const subIds = Array.from(comparisonSelection);

    // Get data for comparison
    $.ajax({
        url: '/RfqList/GetComparisonData',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(subIds),
        success: function (response) {
            if (response.success) {
                // Store in sessionStorage for the comparison page
                sessionStorage.setItem('comparisonData', JSON.stringify(response.data));

                // Navigate to comparison page (you'll create this later)
                // window.location.href = '/Rfq/Compare';
                alert(`Ready to compare ${subIds.length} RFQs! (Comparison page will be implemented)`);
            }
        }
    });
}

// Filter functions
function applyFilters() {
    const searchTerm = $('#searchTerm').val();
    const statusFilter = $('#statusFilter').val();
    const fromDate = $('#fromDate').val();
    const toDate = $('#toDate').val();

    // In real app, submit form or make AJAX call
    console.log('Applying filters:', { searchTerm, statusFilter, fromDate, toDate });

    // For now, just show loading
    toastr.info('@Html.SmartLocalize("Applying filters...")');

    // Simulate API call
    setTimeout(() => {
        toastr.success('@Html.SmartLocalize("Filters applied")');
    }, 500);
}

function clearFilters() {
    $('#searchTerm').val('');
    $('#statusFilter').val('');
    $('#fromDate').val('');
    $('#toDate').val('');
    applyFilters();
}

// Action functions
function viewRFQ(parentId, event) {
    if (event) event.stopPropagation();
    window.location.href = `/Rfq/View/${parentId}`;
}

function editRFQ(parentId, event) {
    if (event) event.stopPropagation();
    window.location.href = `/Rfq/Edit/${parentId}`;
}

function createAlternative(parentId, event) {
    if (event) event.stopPropagation();
    window.location.href = `/Rfq/CreateAlternative/${parentId}`;
}

function viewSubRFQ(subId, event) {
    if (event) event.stopPropagation();
    alert(`View sub-RFQ ${subId}`);
    // window.location.href = `/Rfq/ViewSub/${subId}`;
}

function editSubRFQ(subId, event) {
    if (event) event.stopPropagation();
    alert(`Edit sub-RFQ ${subId}`);
    // window.location.href = `/Rfq/EditSub/${subId}`;
}

function useAsMain(subId, event) {
    if (event) event.stopPropagation();
    if (confirm('@Html.SmartLocalize("Use this alternative as the main RFQ?")')) {
        toastr.success('@Html.SmartLocalize("Alternative RFQ set as main")');
    }
}

function deleteSubRFQ(subId, event) {
    if (event) event.stopPropagation();
    if (confirm('@Html.SmartLocalize("Delete this alternative RFQ?")')) {
        toastr.success('@Html.SmartLocalize("Alternative RFQ deleted")');
    }
}

// Bulk actions
function bulkDelete() {
    const count = comparisonSelection.size;
    if (count === 0) return;

    if (confirm(`Delete ${count} selected RFQs?`)) {
        toastr.success(`${count} RFQs deleted (simulated)`);
        clearSelection();
    }
}

function bulkExport() {
    const count = comparisonSelection.size;
    if (count === 0) return;

    toastr.success(`Exporting ${count} RFQs... (simulated)`);
}

