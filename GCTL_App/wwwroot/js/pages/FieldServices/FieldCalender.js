document.addEventListener("DOMContentLoaded", function () {
    var calendarEl = document.getElementById("calendar");

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: "dayGridMonth",
        selectable: true,
        editable: true,
        events: "/Caledar/GetEvents",
        dateClick: function (info) {
            var title = prompt("Enter Event Title:");
            if (title) {
                $.ajax({
                    url: "/Calendar/addEvent",
                    type: "POST",
                    data: { title: title, date: info.dateStr },
                    success: function () {
                        calendar.refetchEvents();
                    },
                    error: function () {
                        alert("Event saved locally"),
                            calendar.addEvent({
                                title: title,
                                start: info.dateStr
                            });
                    }
                });
            }
        }
    });
    calendar.render();

})