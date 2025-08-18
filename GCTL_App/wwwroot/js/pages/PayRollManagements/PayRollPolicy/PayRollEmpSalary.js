$(document).ready(function () {
    let today = new Date();
    today.setMonth(today.getMonth() -1)
    $('#year-month-picker-1').flatpickr({
        mode: "single",
        dateFormat: "F Y",
        altFormat: "F Y",
        defaultDate: today,
        plugins: [
            new monthSelectPlugin({
                shorthand: true,
                dateFormat: "F Y",
                altFormat: "F Y",
                theme: "light"
            })
        ]
    });




});