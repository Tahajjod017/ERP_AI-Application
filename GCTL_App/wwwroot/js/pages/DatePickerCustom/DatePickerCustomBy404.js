function initializeDatepickerDMY(dateId) {
    flatpickr(`#${dateId}`, {
        dateFormat: "Y-m-d",       // for backend
        altInput: true,            // enables display input
        altFormat: "d/m/Y",        // what the user sees
        allowInput: true,
        onReady: function (selectedDates, dateStr, instance) {
            instance.input.placeholder = "dd/mm/yyyy";
        }
    });
}
function updateDatepickerWithMinDate(dateId, minDate, options = {}) {
    // Destroy existing flatpickr instance if any
    const input = document.getElementById(dateId);
    if (input && input._flatpickr) {
        input._flatpickr.destroy();
    }
    console.log(dateId, minDate);
    const defaultOptions = {
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat: "d/m/Y",
        allowInput: true,
        minDate: minDate,
        onReady: function (selectedDates, dateStr, instance) {
            instance.input.placeholder = "dd/mm/yyyy";
        }
    };

    flatpickr(`#${dateId}`, { ...defaultOptions, ...options });
}

