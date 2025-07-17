//function initializeDatepickerDMY(dateId) {
//    flatpickr(`#${dateId}`, {
//        dateFormat: "Y-m-d",       // for backend
//        altInput: true,            // enables display input
//        altFormat: "d/m/Y",        // what the user sees
//        allowInput: true,
//        onReady: function (selectedDates, dateStr, instance) {
//            instance.input.placeholder = "dd/mm/yyyy";
//        }
//    });
//}

function initializeDatepickerDMY(dateIds) {
    dateIds.split(',').forEach(function (id) {
        const trimmedId = id.trim(); // remove any leading/trailing spaces
        flatpickr(`#${trimmedId}`, {
            dateFormat: "Y-m-d",       // for backend
            altInput: true,            // enables display input
            altFormat: "d/m/Y",        // what the user sees
            allowInput: true,
            onReady: function (selectedDates, dateStr, instance) {
                instance.input.placeholder = "dd/mm/yyyy";
            }
        });
    });
}

// OnlyTOday  Selected
function initializeDatepickerDMYOnlyToday(dateIds) {
    const today = new Date();

    dateIds.split(',').forEach(function (id) {
        const trimmedId = id.trim();
        const inputElement = document.getElementById(trimmedId);

        if (!inputElement) return;

        flatpickr(`#${trimmedId}`, {
            dateFormat: "Y-m-d",
            altInput: true,
            altFormat: "d/m/Y",
            allowInput: false,
            clickOpens: false,
            minDate: today,
            maxDate: today,
            defaultDate: today,
            onReady: function (selectedDates, dateStr, instance) {
                instance.input.placeholder = "dd/mm/yyyy";
            }
        });
    });
}

//
function initializeDatepickerDMY2(dateIds, minDate = null, maxDate = null) {
   
    dateIds.split(',').forEach(function (id) {
        const trimmedId = id.trim();
        flatpickr(`#${trimmedId}`, {
            dateFormat: "Y-m-d",
            altInput: true,
            altFormat: "d/m/Y",
            allowInput: true,
            minDate: minDate || undefined,
            maxDate: maxDate || undefined,
            onReady: function (selectedDates, dateStr, instance) {
                instance.input.placeholder = "dd/mm/yyyy";
            }
        });
    });
}


// Hide Date less than Today date
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
        maxDate: options.maxDate || null,
        onReady: function (selectedDates, dateStr, instance) {
            instance.input.placeholder = "dd/mm/yyyy";
        }
    };

    flatpickr(`#${dateId}`, { ...defaultOptions, ...options });
}


function updateDatepickerWithMinDateTotalDays(dateId, minDate, options = {}, displayDaysId = null, baseDateId = null) {
    const input = document.getElementById(dateId);
    if (input && input._flatpickr) {
        input._flatpickr.destroy();
    }

    const defaultOptions = {
        dateFormat: "Y-m-d",
        altInput: true,
        altFormat: "d/m/Y",
        allowInput: true,
        minDate: minDate,
        onReady: function (selectedDates, dateStr, instance) {
            instance.input.placeholder = "dd/mm/yyyy";
        },
        onChange: function (selectedDates, dateStr, instance) {
            if (selectedDates.length && displayDaysId && baseDateId) {
                const selectedDate = selectedDates[0];
                const baseDateVal = document.getElementById(baseDateId).value;
                if (baseDateVal) {
                    const baseDate = new Date(baseDateVal);
                    const daysDiff = Math.ceil((selectedDate - baseDate) / (1000 * 60 * 60 * 24)) + 1;

                    // Display in span
                    document.getElementById(displayDaysId).innerText = daysDiff;

                    // Set in hidden field if exists
                    const hiddenInput = document.getElementById(displayDaysId);
                    if (hiddenInput) {
                        hiddenInput.value = daysDiff;
                    }
                }
            }
        }
    };

    flatpickr(`#${dateId}`, { ...defaultOptions, ...options });
}

// Date range Global   Pair calender 

function initializeGlobalDateRangePicker(pickerId, fromHiddenId, toHiddenId, onChangeCallback) {

    $(`#${pickerId}`).dateRangePicker({
        format: 'DD/MM/YYYY',
        separator: ' to ',
        language: 'en',
        autoClose: true,
        getValue: function () {
            return $(this).val();
        },
        setValue: function (s) {
            $(this).val(s);
        }
    })
        .bind('datepicker-change datepicker-apply', function (event, obj) {
            const start = moment(obj.date1).format("YYYY-MM-DD");
            const end = moment(obj.date2).format("YYYY-MM-DD");

            $(`#${fromHiddenId}`).val(start);
            $(`#${toHiddenId}`).val(end);

            if (typeof onChangeCallback === 'function') {
                onChangeCallback();
            }
        });
}

// Date range Global   Pair calender  end

// Time Picker

function initializeTimePickerById(elementClassList) {
    const classes = elementClassList.split(',');
    classes.forEach(className => {
        flatpickr(`.${className.trim()}`, {
            enableTime: true,
            noCalendar: true,
            dateFormat: "H:i",
            time_24hr: true,
            disableMobile: true,
            allowInput: true,
            clickOpens: true,
            defaultDate: null
        });
    });
}


//
//$(".timepicker-12hr,.timepicker-12hrEdit").flatpickr({
//    enableTime: true,       // ✅ Enables time selection (hours & minutes)
//    noCalendar: true,       // ✅ Hides the calendar view, showing only the time picker
//    dateFormat: "H:i",      // h = 12-hour, H = 24-hour, i = minutes, K = AM/PM
//    time_24hr: true,        // ✅ Uses 24-hour time format (00:00–23:59 instead of 12-hour AM/PM)
//    disableMobile: true,    // ✅ Prevents the native mobile date/time picker
//    allowInput: true,        // optional: lets user leave it blank
//    clickOpens: true,        // opens on click only
//    defaultDate: null,       // explicitly prevents pre-filling
//    //// ✅ Sets the default time to show when the picker opens
//    //defaultHour: 9,         // default hour (0–23)
//    //defaultMinute: 30,      // default minute (0–59)
//    //minuteIncrement: 5,
//    //minTime: "09:00",       // ✅ Restricts the minimum allowed time
//    //maxTime: "18:00",       // ✅ Restricts the maximum allowed time
//    //enableSeconds: true,    // ✅ Whether seconds can be selected (you’ll also need to update dateFormat)
//    //allowInput: false,      // ✅ Disables manual typing into the input field
//    //// ✅ Disables the entire picker
//    //// Can be used to toggle state from JS: instance.set('disable', true/false)
//    //disable: [function (date) {
//    //    return false; // no disable by default
//    //}],
//    //// ✅ Hook that runs when a date/time is selected
//    //onChange: function (selectedDates, dateStr, instance) {
//    //    console.log("Time selected:", dateStr);
//    //}
//});

//

//$(document).ready(function () {
//    $('#basic-daterange').dateRangePicker({
//        format: 'DD/MM/YYYY',
//        separator: ' to ',
//        language: 'en',
//        autoClose: true,
//        getValue: function () {
//            return $(this).val();
//        },
//        setValue: function (s) {
//            $(this).val(s);
//        }
//    }).bind('datepicker-change', function (event, obj)
//    {

//        const start = moment(obj.date1).format("YYYY-MM-DD");
//        const end = moment(obj.date2).format("YYYY-MM-DD");
//        console.log("FromDatePairDate" + start);
//        console.log("ToDatePairDate" + end);
//        $('#basic-daterange_fromHidden').val(start);
//        $('#basic-daterange_toHidden').val(end);
//        currentPage = 1;
//        loadTableData();
//    });
//});



//$(document).ready(function () {
//    $('#basic-daterange').dateRangePicker({
//        format: 'DD/MM/YYYY',
//        separator: ' to ',
//        language: 'en',
//        autoClose: true,
//        getValue: function () {
//            return $(this).val();
//        },
//        setValue: function (s) {
//            $(this).val(s);
//        }
//    })
//        // Fires when a new range is selected
//        .bind('datepicker-change', function (event, obj) {
//            const start = moment(obj.date1).format("YYYY-MM-DD");
//            const end = moment(obj.date2).format("YYYY-MM-DD");

//            $('#basic-daterange_fromHidden').val(start);
//            $('#basic-daterange_toHidden').val(end);

//            currentPage = 1;
//            loadTableData(); // ✅ refresh data immediately
//        })
//        // Fires even if the same range is selected again
//        .bind('datepicker-apply', function (event, obj) {
//            const start = moment(obj.date1).format("YYYY-MM-DD");
//            const end = moment(obj.date2).format("YYYY-MM-DD");

//            $('#basic-daterange_fromHidden').val(start);
//            $('#basic-daterange_toHidden').val(end);

//            currentPage = 1;
//            loadTableData(); // ✅ refresh data immediately again
//        });
//});