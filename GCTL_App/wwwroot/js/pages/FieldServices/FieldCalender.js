//document.addEventListener("DOMContentLoaded", function () {
//    var calendarEl = document.getElementById("calendar");

//    var calendar = new FullCalendar.Calendar(calendarEl, {
//        height: "70vh",
//        initialView: "dayGridMonth",
//        selectable: true,
//        editable: true,
//        headerToolbar: {
//            right: 'today,prev,next',
//            center: 'title',
//            left: 'dayGridMonth,timeGridWeek,timeGridDay'
//        },
//        buttonText: {
//            dayGridMonth: 'Month',
//            timeGridWeek: 'Week',
//            timeGridDay: 'Day',
//            today: 'Today'
//        },
//        events: "/Caledar/GetEvents",
//        dateClick: function (info) {
//            $.get('/CreateJobs/IndexModal', function (html) {
//                $('#customerModalContent').html(html);

//                // Initialize auto-init items if needed
//                $('#customerModalContent [data-init]').each(function () {
//                    const el = this;
//                    const key = el.dataset.init;
//                    if (key && typeof window[key] === "function") {
//                        window[key](el);
//                        el.dataset.initialized = true;
//                    }
//                });

//                // Show the modal
//                var modal = new bootstrap.Modal(document.getElementById('customerModal'));
//                modal.show();
//            });
//            //if (title) {
//            //    $.ajax({
//            //        url: "/Calendar/addEvent",
//            //        type: "POST",
//            //        data: { title: title, date: info.dateStr },
//            //        success: function () {
//            //            calendar.refetchEvents();
//            //        },
//            //        error: function () {
//            //            alert("Event saved locally"),
//            //                calendar.addEvent({
//            //                    title: title,
//            //                    start: info.dateStr
//            //                });
//            //        }
//            //    });
//            //}
//        }
//    });
//    calendar.render();

//})




document.addEventListener("DOMContentLoaded", function () {
    var calendarEl = document.getElementById("calendar");

    var calendar = new FullCalendar.Calendar(calendarEl, {
        height: "70vh",
        initialView: "dayGridMonth",
        selectable: true,
        editable: true,
        headerToolbar: {
            right: 'today,prev,next',
            center: 'title',
            left: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        buttonText: {
            dayGridMonth: 'Month',
            timeGridWeek: 'Week',
            timeGridDay: 'Day',
            today: 'Today'
        },
        events: function (fetchInfo, successCallback, failureCallback) {
            $.ajax({
                url: '/JobLists/GetCalenderJobList',
                dataType: 'json',
                data: {
                    start: fetchInfo.startStr,
                    end: fetchInfo.endStr,
                    searchTerm: ''
                },
                success: function (response) {
                    // Map CommonReturnViewModel.Data to FullCalendar format
                    if (response.success) {
                        successCallback(response.data || []);
                    } else {
                        successCallback([]);
                    }
                },
                error: function () {
                    failureCallback();
                }
            });
        },
        eventDidMount: function (info) {
            $(info.el).tooltip({
                title: info.event.title + "\nCustomer: " + info.event.extendedProps.customer,
                placement: 'top',
                trigger: 'hover',
                container: 'body'
            });
        },
        eventClick: function (info) {
            const jobId = info.event.id;
            $.get('/CreateJobs/IndexModal?jobId=' + jobId, function (html) {
                $('#customerModalContent').html(html);
                var modal = new bootstrap.Modal(document.getElementById('customerModal'));
                modal.show();
            });
        }
    });

    calendar.render();
});
