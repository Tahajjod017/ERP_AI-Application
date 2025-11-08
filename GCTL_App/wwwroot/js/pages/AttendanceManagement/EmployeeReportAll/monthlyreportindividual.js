// Function to fetch and populate attendance data dynamically
function fetchAttendanceData() {
    var employeeId = 123; // Example employeeId (this can be dynamic)
    var monthyear = '2025-06'; // Example month and year (this can be dynamic)

    $.ajax({
        url: '/MonthlyIndividualReport/SomeAction2',  // Update with your actual API endpoint
        type: 'GET',
        data: {
            monthyear: monthyear,
            employeeId: employeeId
        },
        success: function (data) {
            // Populate the table with dynamic dates
            populateAttendanceTable(data, monthyear);
        },
        error: function (xhr, status, error) {
            console.error('Error fetching data: ', error);
        }
    });
}

// Function to populate the table with the fetched attendance data
function populateAttendanceTable(data, monthyear) {
    var tableBody = $('#attendanceTableBody');
    var tableHeader = $('thead tr');

    // First, clear any existing table rows and headers
    tableBody.empty();
    tableHeader.empty();

    // Dynamically generate the date columns based on the month and year
    var daysInMonth = new Date(monthyear.split('-')[0], monthyear.split('-')[1], 0).getDate(); // Get number of days in the month

    // Add the "Date" header
    tableHeader.append('<th>Date</th>');

    // Loop through each day of the month and add it to the table header
    for (var i = 1; i <= daysInMonth; i++) {
        tableHeader.append('<th>' + (i < 10 ? '0' + i : i) + '</th>');
    }

    // Generate rows for check-in, check-out, etc.
    var rows = [
        { label: 'Check in at', key: 'CheckInTime' },
        { label: 'Check out at', key: 'CheckOutTime' },
        { label: 'Break', key: 'Break' },
        { label: 'Late', key: 'LateHour' },
        { label: 'Early Leave', key: 'EarlyHour' },
        { label: 'Over Time', key: 'OvertimeHour' },
        { label: 'Production Hours', key: 'WorkingHour' },
        { label: 'Status', key: 'Status' }
    ];

    // Loop through each row (check-in, check-out, etc.) and populate it
    rows.forEach(function (row) {
        var tableRow = '<tr><th>' + row.label + '</th>';

        // Loop through each day and check if the data exists for that day
        for (var i = 1; i <= daysInMonth; i++) {
            var attendanceData = data.find(function (d) {
                return parseInt(d.Date) === i;
            });

            if (attendanceData) {
                // Add specific data based on the key (e.g., CheckInTime, Status)
                if (attendanceData[row.key]) {
                    tableRow += '<td>' + attendanceData[row.key] + '</td>';
                } else {
                    tableRow += '<td>-</td>';
                }
            } else {
                tableRow += '<td>-</td>';
            }
        }
        tableRow += '</tr>';
        tableBody.append(tableRow);
    });
}

// Call the fetchAttendanceData function when the page loads
$(document).ready(function () {
    fetchAttendanceData();
});
//// ===== Example JSON (আপনার API/ফাইল থেকে একই কাঠামো দিন) =====
//const attendanceJSON = {
//    month: "2025-10",              // YYYY-MM (শুধু মাস জানলেই হেডার ডে-কাউন্ট ঠিক হবে)
//    days: 31,                      // চাইলে বাদ; না দিলে মাস অনুযায়ী অটো হিসাব হবে
//    data: {
//        // day: { fields... } — "-" না থাকলে ফিল্ডই বাদ দিতে পারেন
//        "01": { in: "08:45 AM", out: "05:15 PM", break: "25 Min", late: "15 Min", early: "12 Min", ot: "55 Min", prod: "8.41 H", status: "P" },
//        "02": { in: "08:45 AM", out: "05:15 PM", break: "25 Min", late: "50 Min", early: "12 Min", ot: "55 Min", prod: "8.41 H", status: "P" },
//        "03": { status: "W" },
//        "04": { in: "08:45 AM", out: "05:15 PM", break: "25 Min", late: "5 Min", early: "12 Min", ot: "55 Min", prod: "8.41 H", status: "P" },
//        "05": { in: "12:15 PM", out: "06:25 PM", break: "-", late: "4.12 H", early: "-", ot: "-", prod: "4.20 H", status: "A" },
//        // ... আপনার পুরো মাসের দিনগুলো এখানে যোগ করুন ...
//    }
//};

//// ===== Utility: মাসের দিনের সংখ্যা বের করা =====
//function daysInMonth(yyyy, mm) {
//    return new Date(yyyy, mm, 0).getDate(); // mm: 1-12
//}

//// ===== Badge helper =====
//function badge(content, type) {
//    if (!content || content === "-") return "-";
//    const c = (type === "danger") ? "text-bg-danger"
//        : (type === "warning") ? "text-bg-warning"
//            : "text-bg-success";
//    return `<span class="badge ${c}">${content}</span>`;
//}

//// ===== Status badge color =====
//function statusBadge(st) {
//    if (!st) return "-";
//    const map = { "P": "text-bg-success", "A": "text-bg-danger", "W": "text-bg-warning", "L": "text-bg-warning" };
//    const cls = map[st] || "text-bg-secondary";
//    return `<span class="badge ${cls}">${st}</span>`;
//}

//// ===== Main Render Function =====
//function renderAttendanceTable(containerId, json) {
//    const container = document.getElementById(containerId);
//    if (!container) return;

//    // Parse year & month
//    const [Y, M] = (json.month || "2025-01").split("-").map(n => parseInt(n, 10));
//    const totalDays = json.days || daysInMonth(Y, M);

//    // Table parts
//    const cols = [];
//    for (let d = 1; d <= totalDays; d++) {
//        cols.push(String(d).padStart(2, "0"));
//    }

//    // Rows to render (label, key, renderer)
//    const rows = [
//        { label: "Check in at", key: "in", render: v => v || "-" },
//        { label: "Check out at", key: "out", render: v => v || "-" },
//        { label: "Break", key: "break", render: v => v || "-" },
//        { label: "Late", key: "late", render: v => v || "-" },
//        { label: "Early Leave", key: "early", render: v => v || "-" },
//        { label: "Over Time", key: "ot", render: v => v || "-" },
//        { label: "Production Hours", key: "prod", render: v => (v ? badge(v, v.includes("H") ? "success" : "success") : "-") },
//        { label: "Status", key: "status", render: v => statusBadge(v) }
//    ];

//    // Build thead
//    let thead = `
//    <thead>
//      <tr>
//        <th>Date</th>
//        ${cols.map(d => `<th>${d}</th>`).join("")}
//      </tr>
//    </thead>
//  `;

//    // Build tbody
//    const tbodyRows = rows.map(r => {
//        const tds = cols.map(d => {
//            const cell = (json.data && json.data[d]) ? json.data[d][r.key] : "-";
//            return `<td class="fs-10 fw-normal">${r.render(cell)}</td>`;
//        }).join("");
//        return `<tr><th>${r.label}</th>${tds}</tr>`;
//    }).join("");

//    const table = `
//    <div class="table-responsive">
//      <table class="table table-bordered text-center p-2">
//        ${thead}
//        <tbody>
//          ${tbodyRows}
//        </tbody>
//      </table>
//    </div>
//  `;

//    container.innerHTML = table;
//}

//// ===== Render with inline JSON =====
//renderAttendanceTable("individualEmployeemonthlyAttendanceTable", attendanceJSON);

// ======= EXAMPLE JSON (আপনার API থেকেও একই স্কিমা দিন) =======
const attendanceJSON = {
    month: "2025-10",  // YYYY-MM
    // চাইলে 'days' না দিলেও মাস অনুযায়ী অটো হবে
    // days: 31,
    data: {
        "01": { in: "08:45 AM", out: "05:15 PM", break: "25 Min", late: "15 Min", early: "12 Min", ot: "55 Min", prod: "8.41 H", status: "P" },
        "02": { in: "08:45 AM", out: "05:15 PM", break: "25 Min", late: "50 Min", early: "12 Min", ot: "55 Min", prod: "8.41 H", status: "P" },
        "03": { status: "W" },
        "04": { in: "08:45 AM", out: "05:15 PM", break: "25 Min", late: "5 Min", early: "12 Min", ot: "55 Min", prod: "8.41 H", status: "P" },
        "05": { in: "12:15 PM", out: "06:25 PM", break: "-", late: "4.12 H", early: "-", ot: "-", prod: "4.20 H", status: "A" },
        // ... "06" .. "31"
    }
};

// ======= UTILITIES =======
function daysInMonth(y, m) { return new Date(y, m, 0).getDate(); } // m:1-12
function pad2(n) { return String(n).padStart(2, "0"); }
function badge(content, tone = "success") {
    if (!content || content === "-") return "-";
    const map = { success: "text-bg-success", warning: "text-bg-warning", danger: "text-bg-danger", secondary: "text-bg-secondary" };
    const cls = map[tone] || map.success;
    return `<span class="badge ${cls}">${content}</span>`;
}
function statusTone(s) {
    if (s === "P") return "success";
    if (s === "A") return "danger";
    if (s === "W" || s === "L") return "warning";
    return "secondary";
}

// ======= CORE RENDER (SAME DESIGN) =======
function renderMonthlyAttendanceSameDesign(containerId, payload) {
    const root = document.getElementById(containerId);
    if (!root) return;

    const [Y, M] = (payload.month || "2025-01").split("-").map(n => parseInt(n, 10));
    const totalDays = payload.days || daysInMonth(Y, M);
    const days = Array.from({ length: totalDays }, (_, i) => pad2(i + 1));

    // THEAD: Date + 01..NN
    const thead = `
    <thead>
      <tr>
        <th>Date</th>
        ${days.map(d => `<th>${d}</th>`).join("")}
      </tr>
    </thead>
  `;

    // VALUE GETTER: যদি status 'A'/'W' হয় এবং ফিল্ড মিসিং থাকে, '-' দেখাবে (আপনার উদাহরণ মতো)
    function getCell(day, key) {
        const rec = payload.data?.[day] || {};
        const st = rec.status;
        let val = rec[key];
        if ((st === "A" || st === "W") && (val === undefined || val === null)) return "-";
        return (val !== undefined && val !== null && val !== "") ? val : "-";
    }

    // ROWS (একই অর্ডার + একই ক্লাস)
    const rows = [
        {
            label: "Check in at",
            build: d => `<td class="fs-10 fw-normal">${getCell(d, "in")}</td>`
        },
        {
            label: "Check out at",
            build: d => `<td class="fs-10 fw-normal">${getCell(d, "out")}</td>`
        },
        {
            label: "Break",
            build: d => `<td class="fs-10 fw-normal">${getCell(d, "break")}</td>`
        },
        {
            label: "Late",
            build: d => `<td class="fs-10 fw-normal">${getCell(d, "late")}</td>`
        },
        {
            label: "Early Leave",
            build: d => `<td class="fs-10 fw-normal">${getCell(d, "early")}</td>`
        },
        {
            label: "Over Time",
            build: d => `<td class="fs-10 fw-normal">${getCell(d, "ot")}</td>`
        },
        {
            label: "Production Hours",
            build: d => {
                const p = getCell(d, "prod");
                // উদাহরণ অনুযায়ী: সাধারণত 'H' থাকলে সবুজ, ছোট হলে লাল দেখিয়েছেন
                const tone = (p !== "-" && /(\d+(\.\d+)?)\s*H/i.test(p) && parseFloat(RegExp.$1) < 5) ? "danger" : "success";
                return p === "-" ? `<td>-</td>` : `<td>${badge(p, tone)}</td>`;
            }
        },
        {
            label: "Status",
            build: d => {
                const s = (attendanceJSON.data?.[d]?.status) || "-";
                return s === "-" ? `<td>-</td>` : `<td>${badge(s, statusTone(s))}</td>`;
            }
        }
    ];

    const tbodyRows = rows.map(row => {
        const tds = days.map(d => row.build(d)).join("");
        return `<tr><th>${row.label}</th>${tds}</tr>`;
    }).join("");

    // FINAL TABLE (আপনার একই ক্লাস: table-bordered text-center p-2)
    const table = `
    <div class="table-responsive">
      <table class=" table-bordered text-center p-2">
        ${thead}
        <tbody>
          ${tbodyRows}
        </tbody>
      </table>
    </div>
  `;

    root.innerHTML = table;
}

// ======= RENDER NOW =======
renderMonthlyAttendanceSameDesign("individualEmployeemonthlyAttendanceTable", attendanceJSON);

/* ======= FETCH EXAMPLE (আপনার এন্ডপয়েন্ট দিন)
fetch('/api/attendance?empId=123&month=2025-10')
  .then(r => r.json())
  .then(data => renderMonthlyAttendanceSameDesign('individualEmployeemonthlyAttendanceTable', data))
  .catch(console.error);
*/