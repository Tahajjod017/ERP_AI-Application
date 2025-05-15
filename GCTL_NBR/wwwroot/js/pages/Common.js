

$(document).ready(function () {
    $('#empCodePass1').selectpicker({
        liveSearch: true,
        //multipleSeparator: ', ',
        //maxOptions: 5,
        //maxOptionsText: 'You can only select {n} items',
        enableSelectedText: true,
        liveSearchPlaceholder: 'Search...',
        size: 10,  // Displays up to 10 items before scrolling appears.
        selectedTextFormat: 'count',  // Display only the count of selected items.
        //dropupAuto: false,  // Prevents the dropdown from auto-opening upwards.
        actionsBox: true,  // Enables "Select All" and "Deselect All" buttons.
        iconBase: 'fa',      // Uses Font Awesome icons
        showTick: true,
        tickIcon: 'fa-check',
        //hideDisabled: true,  // Hides the disabled options in the dropdown.
        container: 'body',  // Appends the dropdown to the body element.
    });
    $('#empCodePass1').selectpicker('deselectAll');
});