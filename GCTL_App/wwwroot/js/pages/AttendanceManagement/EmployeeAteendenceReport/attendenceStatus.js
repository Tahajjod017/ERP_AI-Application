//function parseTime(timeStr) {
//    const [hh, mm] = timeStr.split(':').map(Number);
//    return new Date(0, 0, 0, hh, mm);
//}

//function parseDuration(durationStr) {
//    const hMatch = durationStr.match(/(\d+)h/);
//    const mMatch = durationStr.match(/(\d+)m/);
//    const hours = hMatch ? parseInt(hMatch[1]) : 0;
//    const minutes = mMatch ? parseInt(mMatch[1]) : 0;
//    return hours * 60 + minutes;
//}

//function formatTime(date) {
//    return date.toTimeString().substring(0, 5); // "HH:mm"
//}

//function updateProgressBarWithStart(totalMins, offsetMins, durationMins, barId) {
//    const left = (offsetMins / totalMins) * 100;
//    const width = (durationMins / totalMins) * 100;

//    $(`#${barId}`).css({
//        left: `${left}%`,
//        width: `${width}%`
//    });
//}

//function updateAttendanceUI(data) {
//    // Display summary values
//    $('#totalWorkingHours').text(data.totalWorkingHours || '00h 00m');
//    $('#productiveHours').text(data.productionTime || '00h 00m');
//    $('#breakHours').text(data.break || '00h 00m');
//    $('#overtime').text(data.overtime || '00h 00m');

//    const totalMins = parseDuration(data.totalWorkingHours);
//    const prodMins = parseDuration(data.productionTime);
//    const breakMins = parseDuration(data.break);
//    const overtimeMins = parseDuration(data.overtime);
//    alert(overtimeMins)

//    // Progress bar logic
//    updateProgressBarWithStart(totalMins, 0, prodMins, 'progressProductive');
//    updateProgressBarWithStart(totalMins, prodMins, breakMins, 'progressBreak');
//    updateProgressBarWithStart(totalMins, prodMins + breakMins, overtimeMins, 'progressOvertime');

//    // Generate timeline labels (from check-in  to end)
//    const timeline = $('#timelineLabels');
//    timeline.empty();

//    const checkIn = parseTime(data.checkInShiftTime || '08:00');
//    //const start = new Date(checkIn.getTime() - 60 * 60000); // +1h
//    const start = new Date(checkIn.getTime()); // +1h
//    const end = new Date(checkIn.getTime() + totalMins * 60000);

//    let current = new Date(start);
//    while (current <= end) {
//        const span = $('<span>').addClass('fs-10').text(formatTime(current));
//        timeline.append(span);
//        current.setMinutes(current.getMinutes() + 60);
//    }
//}



//$(document).ready(function () {
//    const userId = 123; // replace with your actual user ID
//    $.get(`/EmployeesAttendance/GetEmployeeAttendanceData`, function (data) {
//        if (data) updateAttendanceUI(data);
//    });
//});





//// Function to parse time (HH:mm) into a Date object
//function parseTime(timeStr) {
//    const [hh, mm] = timeStr.split(':').map(Number);
//    return new Date(0, 0, 0, hh, mm); // Returns a date object for the given time
//}

//// Function to parse duration string (Xh Xm) into total minutes
//// Function to parse duration string (Xh Xm) into total minutes
//function parseDuration(durationStr) {
//    if (typeof durationStr !== 'string') {
//        console.warn('Invalid duration string:', durationStr); // Log if the durationStr is invalid
//        return 0; // Return 0 if the duration string is not valid
//    }

//    const hMatch = durationStr.match(/(\d+)h/);
//    const mMatch = durationStr.match(/(\d+)m/);

//    const hours = hMatch ? parseInt(hMatch[1]) : 0;
//    const minutes = mMatch ? parseInt(mMatch[1]) : 0;

//    return hours * 60 + minutes; // Returns total minutes
//}


//// Function to format a Date object into "HH:mm" string
//function formatTime(date) {
//    return date.toTimeString().substring(0, 5); // "HH:mm"
//}

//// Function to update the progress bars with a start offset and a duration
//function updateProgressBarWithStart(totalMins, offsetMins, durationMins, barId) {
//    const left = (offsetMins / totalMins) * 100; // Calculate starting position of the bar
//    const width = (durationMins / totalMins) * 100; // Calculate width of the bar

//    // Ensure the progress bar is positioned and has the correct width
//    $(`#${barId}`).css({
//        left: `${left}%`, // Position the progress bar according to the offset
//        width: `${width}%` // Set the width of the progress bar based on the duration
//    });
//}

//// Function to update the UI based on the data
//function updateAttendanceUI(data) {
//    // Display summary values
//    $('#totalWorkingHours').text(data.totalWorkingHours || '00h 00m');
//    $('#productiveHours').text(data.productiveHours || '00h 00m');
//    $('#breakHours').text(data.breakHours || '00h 00m');
//    $('#overtime').text(data.overtime || '00h 00m');

//    const totalMins = parseDuration(data.totalWorkingHours);
//    const prodMins = parseDuration(data.productiveHours);
//    const breakMins = parseDuration(data.breakHours);
//    const overtimeMins = parseDuration(data.overtime);

//    // Progress bar logic
//    updateProgressBarWithStart(totalMins, 0, prodMins, 'progressProductive');
//    updateProgressBarWithStart(totalMins, prodMins, breakMins, 'progressBreak');
//    updateProgressBarWithStart(totalMins, prodMins + breakMins, overtimeMins, 'progressOvertime');

//    // Generate timeline labels (from check-in to end)
//    const timeline = $('#timelineLabels');
//    timeline.empty(); // Clear existing labels

//    const checkIn = parseTime(data.checkInShiftTime || '08:00');
//    const start = new Date(checkIn.getTime()); // Starting time is check-in time
//    const end = new Date(checkIn.getTime() + totalMins * 60000); // Add total working minutes to get the end time

//    let current = new Date(start);
//    while (current <= end) {
//        const span = $('<span>').addClass('fs-10').text(formatTime(current)); // Format time labels
//        timeline.append(span); // Append label to timeline
//        current.setMinutes(current.getMinutes() + 60); // Increment by 1 hour for each label
//    }

//    // Handle session timeline (e.g., Worked 5h 30m, Break 1h 00m)
//    const sessionTimeline = $('#sessionTimeline');
//    sessionTimeline.empty();
//    if (data.sessionTimeline && Array.isArray(data.sessionTimeline)) {
//        data.sessionTimeline.forEach(session => {
//            const span = $('<span>').addClass('fs-10').text(session);
//            sessionTimeline.append(span); // Add session data to timeline
//        });
//    } else {
//        sessionTimeline.append('<span class="fs-10">No session data available</span>');
//    }
//}

//$(document).ready(function () {
//    const userId = 123; // replace with your actual user ID
//    $.get(`/EmployeesAttendance/GetEmployeeAttendanceData2?userId=${userId}`, function (data) {
//        if (data) {
//            updateAttendanceUI(data);
//        }
//    });
//});



// Data provided
const data = {
    "value": {
        "sessionTimeline": [
            { "type": "Worked", "duration": "2h 0m" },
            { "type": "Break", "duration": "0.5h 0m" },
            { "type": "Worked", "duration": "3h 0m" },
            { "type": "Break", "duration": "1h 0m" },
            { "type": "Worked", "duration": "2h 0m" },
            { "type": "Ovvertime", "duration": "2h 0m" }
        ]
    }
};
//function updateAttendanceUI(data) {
//    const sessionTimeline = data.value.sessionTimeline;

//    // Call the functions to update the UI with the fetched data
//    updateSessionHours(sessionTimeline);
//    updateProgressBars(sessionTimeline);
//}

// Convert duration from string (e.g., "2h 0m") to total minutes
function convertToMinutes(duration) {
    if (!duration || typeof duration !== "string") return 0;

    const parts = duration.trim().split(' ');
    let hours = 0, minutes = 0;

    if (parts[0]?.includes('h')) {
        hours = parseFloat(parts[0].replace('h', '')) || 0;
    }

    if (parts[1]?.includes('m')) {
        minutes = parseInt(parts[1].replace('m', '')) || 0;
    }

    return (hours * 60) + minutes;
}

// Function to format time (convert minutes to HH:MM format)
function formatTime(minutes) {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
}

// Function to calculate total working hours, productive hours, break hours, and overtime
function updateSessionHours() {
    let totalWorkingHours = 0;
    let productiveHours = 0;
    let breakHours = 0;
    let overtimeHours = 0;

    // Loop through each session entry and accumulate time for each type
    data.value.sessionTimeline.forEach(session => {
        const durationInMinutes = convertToMinutes(session.duration);

        // Accumulate total time
        totalWorkingHours += durationInMinutes;

        // Accumulate time based on session type
        switch (session.type) {
            case "Worked":
                productiveHours += durationInMinutes;
                break;
            case "Break":
                breakHours += durationInMinutes;
                break;
            case "Ovvertime":
                overtimeHours += durationInMinutes;
                break;
        }
    });

    // Update the HTML with the calculated values
    $('#totalWorkingHours').text(formatTime(totalWorkingHours));
    $('#productiveHours').text(formatTime(productiveHours));
    $('#breakHours').text(formatTime(breakHours));
    $('#overtime').text(formatTime(overtimeHours));
}

// Function to update progress bars and session details
function updateProgressBars() {
    let totalTime = 0;
    let sessionPercentages = [];

    // Loop through each session entry and accumulate time for each type
    data.value.sessionTimeline.forEach(session => {
        const durationInMinutes = convertToMinutes(session.duration);
        totalTime += durationInMinutes;
    });

    // Now calculate the percentage for each session type
    data.value.sessionTimeline.forEach(session => {
        const durationInMinutes = convertToMinutes(session.duration);
        const sessionPercentage = (durationInMinutes / totalTime) * 100;

        // Push session type, duration, and formatted percentage to array
        sessionPercentages.push({
            type: session.type,
            duration: session.duration,
            percentage: sessionPercentage.toFixed(2) + "%"  // format percentage to 2 decimal places
        });
    });

    // Display the progress bars and the session details
    let progressBarsHtml = "";
    sessionPercentages.forEach(session => {
        progressBarsHtml += `
            <div class="progress-bar ${getProgressBarColor(session.type)} rounded me-1" role="progressbar"
                style="width: ${session.percentage}">
                ${session.duration}
            </div>
        `;
    });

    $('#progressBars').html(progressBarsHtml);  // Insert the progress bars into the container

    // Update the timeline labels
    const timelineLabels = $('#timelineLabels');
    let currentTime = 0;
    let currentHour = 6;  // Start at 6:00 AM

    data.value.sessionTimeline.forEach(session => {
        const durationInMinutes = convertToMinutes(session.duration);
        const label = $('<span>').addClass('fs-10').text(`${currentHour}:00`);
        timelineLabels.append(label);

        currentTime += durationInMinutes;
        currentHour = (currentHour + Math.floor(currentTime / 60)) % 24;  // Update hour
    });
}

// Function to determine the progress bar color based on session type
function getProgressBarColor(type) {
    switch (type) {
        case "Worked":
            return "bg-success";  // Green for Work
        case "Break":
            return "bg-warning";  // Yellow for Break
        case "Ovvertime":
            return "bg-info";  // Blue for Overtime
        default:
            return "bg-secondary";  // Default for any unknown session type
    }
}

// Use jQuery's ready function to ensure the DOM is fully loaded
$(document).ready(function () {
    updateSessionHours();
    updateProgressBars();
});

// Use jQuery's ready function to ensure the DOM is fully loaded
//$(document).ready(function () {
//    // Dynamically fetch data from the server
//    const userId = 123; // Replace with your actual user ID or another dynamic value
//    $.get(`/EmployeesAttendance/GetEmployeeAttendanceData2?userId=${userId}`, function (data) {
//        // Check if data is returned and then update the UI
//        if (data) {
//            console.log(data)
//            updateAttendanceUI(data);  // Process the data and update the UI

//        }
//    }, 'json');
//});

///punch ativity

// Data provided (Punch In/Punch Out events)
const timelineData = [
    { "type": "Punch In", "time": "10:00 AM", "description": "Punch In " },
    { "type": "Punch Out", "time": "11:30 AM", "description": "Punch Out " },
    /*{ "type": "Break", "time": "1:00 PM - 2:00 PM", "description": "Break Time" },*/
    { "type": "Punch In", "time": "2:10 PM", "description": "Punch In " },
    { "type": "Punch Out", "time": "7:30 PM", "description": "Punch Out " }
];

// Function to generate the timeline dynamically
function generateTimeline() {
    const timelineContainer = document.getElementById('timelineContainer');

    // Loop through the timeline data and create timeline items
    timelineData.forEach(item => {
        // Create the timeline item container
        const timelineItem = document.createElement('div');
        timelineItem.classList.add('timeline-item', 'position-relative');

        // Create the row that contains the item
        const row = document.createElement('div');
        row.classList.add('row', 'g-md-3');

        // Create the timeline bar and icon
        const timelineBar = document.createElement('div');
        timelineBar.classList.add('col-12', 'col-md-auto', 'd-flex');
        const timelineItemBar = document.createElement('div');
        timelineItemBar.classList.add('timeline-item-bar', 'position-md-relative', 'me-3', 'me-md-0');

        const iconItem = document.createElement('div');
        iconItem.classList.add('icon-item', 'icon-item-sm', 'rounded-7', 'shadow-none', 'bg-primary-subtle');

        const icon = document.createElement('span');
        icon.classList.add('fa-solid', 'fa-mobile-retro', 'text-primary-dark', 'fs-10');  // Default icon for now
        iconItem.appendChild(icon);
        timelineItemBar.appendChild(iconItem);

        // Add timeline bar separator line
        const timelineBarLine = document.createElement('span');
        timelineBarLine.classList.add('timeline-bar', 'border-end', 'border-dashed');
        timelineItemBar.appendChild(timelineBarLine);

        timelineBar.appendChild(timelineItemBar);
        row.appendChild(timelineBar);

        // Create the content area for the timeline item
        const contentCol = document.createElement('div');
        contentCol.classList.add('col');

        const content = document.createElement('div');
        content.classList.add('timeline-item-content', 'ps-6', 'ps-md-3');

        const title = document.createElement('h5');
        title.classList.add('fs-9', 'lh-sm');
        title.textContent = item.description;
        content.appendChild(title);

        const timeParagraph = document.createElement('p');
        timeParagraph.classList.add('fs-9');
        timeParagraph.innerHTML = `<i class="" aria-hidden="true"></i> ${item.time}`;
        content.appendChild(timeParagraph);

        contentCol.appendChild(content);
        row.appendChild(contentCol);

        timelineItem.appendChild(row);
        timelineContainer.appendChild(timelineItem);
    });
}

// Call the function to generate the timeline when the page is ready
$(document).ready(function () {
    generateTimeline();
});



///piechart

// Function to update the attendance status pie chart
function updateAttendancePieChart(present, absent, leave, late, earlyLeave) {
    var dom = document.getElementById('employee-attendance-status-piechart');
    var myChart = echarts.init(dom, null, {
        renderer: 'canvas',
        useDirtyRect: false
    });

    var option = {
        tooltip: {
            trigger: 'item'
        },
        legend: {
            top: '5%',
            left: 'center'
        },
        series: [
            {
                name: 'Monthly Report',
                type: 'pie',
                radius: ['40%', '70%'],
                center: ['50%', '70%'],
                startAngle: 180,
                endAngle: 360,
                data: [
                    { value: present, name: 'Present' },
                    { value: absent, name: 'Absent' },
                    { value: leave, name: 'Leave' },
                    {
                        value: late, name: 'Late',
                        itemStyle: {
                            color: 'red'  // Set color 
                        }
                    },
                    { value: earlyLeave, name: 'Early Leave' }
                ]
            }
        ]
    };

    // Set the options for the pie chart
    myChart.setOption(option);

    // Resize the chart when the window is resized
    window.addEventListener('resize', myChart.resize);
}

// Call the function with dynamic data
// Example dynamic data
updateAttendancePieChart(65, 20, 2, 5, 8);

// You can call this function again with new data to update the chart dynamically
// updateAttendancePieChart(newPresent, newAbsent, newLeave, newLate, newEarlyLeave);

//bar compare
// Function to render the "You vs Best_Emp" bar chart using dynamic JSON data
function renderAttendanceCompareChart(data) {
    var dom = document.getElementById('employee-attendance-status-compare-barchart');
    var myChart = echarts.init(dom, null, {
        renderer: 'canvas',
        useDirtyRect: false
    });

    var option = {
        title: {
            text: 'You vs Emp benchmark',
            subtext: 'Previous Month'
        },
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            data: ['You', 'Emp benchmark']
        },
        toolbox: {
            show: true,
            feature: {
                magicType: { show: true, type: ['line', 'bar'] },
                restore: { show: true },
                saveAsImage: { show: true }
            }
        },
        calculable: true,
        xAxis: [
            {
                type: 'category',
                data: ['Present', 'Absent', 'Early Leave', 'Late Leave']
            }
        ],
        yAxis: [
            {
                type: 'value'
            }
        ],
        series: [
            {
                name: 'You',
                type: 'bar',
                data: data.you,
                itemStyle: {
                    color: '#ef5c08',
                },
            },
            {
                name: 'Emp benchmark',
                type: 'bar',
                data: data.bestEmp,
                itemStyle: {
                    color: '#48cd00',
                },
            }
        ]
    };

    // Set the chart options
    myChart.setOption(option);

    // Resize the chart on window resize
    window.addEventListener('resize', myChart.resize);
}

// Example of passing dynamic data as a JSON object
var jsonData = {
    "you": [16, 3, 3, 2],      // Data for "You"
    "bestEmp": [20, 2, 1, 1]   // Data for ""
};

// Call the function with the dynamic data (this can be done on the fly)
renderAttendanceCompareChart(jsonData);

// linechart1

// Function to render the "Yearly Attendance Status" bar chart with dynamic JSON data
function renderAttendanceBarChart(data) {
    var dom = document.getElementById('yearly-employee-attendance-status-linechart1');
    var myChart = echarts.init(dom, null, {
        renderer: 'canvas',
        useDirtyRect: false
    });

    const posList = [
        'left', 'right', 'top', 'bottom', 'inside', 'insideTop', 'insideLeft', 'insideRight',
        'insideBottom', 'insideTopLeft', 'insideTopRight', 'insideBottomLeft', 'insideBottomRight'
    ];

    const app = {
        configParameters: {
            rotate: { min: -90, max: 90 },
            align: { options: { left: 'left', center: 'center', right: 'right' } },
            verticalAlign: { options: { top: 'top', middle: 'middle', bottom: 'bottom' } },
            position: posList.reduce((map, pos) => { map[pos] = pos; return map; }, {}),
            distance: { min: 0, max: 100 }
        },
        config: {
            rotate: 90,
            align: 'left',
            verticalAlign: 'middle',
            position: 'insideBottom',
            distance: 15,
            onChange: function () {
                const labelOption = {
                    rotate: app.config.rotate,
                    align: app.config.align,
                    verticalAlign: app.config.verticalAlign,
                    position: app.config.position,
                    distance: app.config.distance
                };
                myChart.setOption({
                    series: [
                        { label: labelOption },
                        { label: labelOption },
                        { label: labelOption },
                        { label: labelOption }
                    ]
                });
            }
        }
    };

    const labelOption = {
        show: true,
        position: app.config.position,
        distance: app.config.distance,
        align: app.config.align,
        verticalAlign: app.config.verticalAlign,
        rotate: app.config.rotate,
        formatter: '{c}',
        fontSize: 7,
        rich: { name: {} }
    };

    const option = {
        title: { text: 'Attendance Status', subtext: 'Previous Year' },
        tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
        grid: { left: '3%', right: '3%', bottom: '3%', containLabel: true },
        legend: { data: ['Present', 'Absent', 'Late Entry', 'Early Leave', 'Casual Leave', 'Medical Leave'] },
        toolbox: {
            show: true,
            orient: 'vertical',
            left: 'right',
            top: 'center',
            feature: {
                mark: { show: true },
                dataView: { show: true, readOnly: false },
                magicType: { show: true, type: ['line', 'bar', 'stack'] },
                restore: { show: true },
                saveAsImage: { show: true }
            }
        },
        xAxis: [
            {
                type: 'category',
                axisTick: { show: false },
                data: data.months // Dynamic months data from JSON
            }
        ],
        yAxis: [{ type: 'value' }],
        series: [
            {
                name: 'Present',
                type: 'bar',
                barGap: 0,
                label: labelOption,
                emphasis: { focus: 'series' },
                data: data.present // Dynamic data for "Present"
            },
            {
                name: 'Absent',
                type: 'bar',
                label: labelOption,
                emphasis: { focus: 'series' },
                data: data.absent // Dynamic data for "Absent"
            },
            {
                name: 'Late Entry',
                type: 'bar',
                label: labelOption,
                itemStyle: {
                    color: 'red'  // Set color 
                },
                emphasis: { focus: 'series' },
                data: data.lateEntry // Dynamic data for "Late Entry"
            },
            {
                name: 'Early Leave',
                type: 'bar',
                label: labelOption,
                emphasis: { focus: 'series' },
                data: data.earlyLeave // Dynamic data for "Early Leave"
            },
            {
                name: 'Casual Leave',
                type: 'bar',
                label: labelOption,
                emphasis: { focus: 'series' },
                data: data.casualLeave // Dynamic data for "Casual Leave"
            },
            {
                name: 'Medical Leave',
                type: 'bar',
                label: labelOption,
                emphasis: { focus: 'series' },
                data: data.medicalLeave // Dynamic data for "Medical Leave"
            }
        ]
    };

    // Set the chart options
    myChart.setOption(option);

    // Resize the chart on window resize
    window.addEventListener('resize', myChart.resize);
}

// Example of passing dynamic data as a JSON object
var jsonData = {
    "months": ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
    "present": [20, 21, 19, 21, 18, 20, 21, 19, 21, 18, 20, 21],
    "absent": [3, 2, 1, 3, 2, 1, 1, 1, 2, 3, 1, 1],
    "lateEntry": [2, 1, 2, 1, 3, 1, 2, 2, 1, 2, 1, 2],
    "earlyLeave": [3, 2, 1, 3, 2, 1, 0, 1, 2, 3, 1, 1],
    "casualLeave": [1, 0, 1, 1, 1, 1, 0, 1, 2, 1, 2, 1],
    "medicalLeave": [0, 0, 1, 3, 0, 1, 0, 0, 2, 0, 2, 1]
};

// Call the function with the dynamic data (this can be done on the fly)
renderAttendanceBarChart(jsonData);

//linechart
// Function to render the "Yearly Attendance Status" line chart with dynamic JSON data
function renderAttendanceLineChart(data) {
    var dom = document.getElementById('yearly-employee-attendance-status-linechart');
    var myChart = echarts.init(dom, null, {
        renderer: 'canvas',
        useDirtyRect: false
    });

    var option = {
        title: {
            text: 'Attendance Status',
            subtext: 'Previous Year'
        },
        tooltip: {
            trigger: 'axis'
        },
        legend: {
            data: ['Present', 'Absent', 'Late Entry', 'Early Leave', 'Casual Leave', 'Medical Leave']
        },
        grid: {
            left: '3%',
            right: '4%',
            bottom: '4%',
            containLabel: true
        },
        toolbox: {
            feature: {
                saveAsImage: {}
            }
        },
        xAxis: {
            type: 'category',
            boundaryGap: false,
            data: data.months // Data for the months
        },
        yAxis: {
            type: 'value'
        },
        series: [
            {
                name: 'Casual Leave',
                type: 'line',
                stack: 'Total',
                data: data.casualLeave
            },
            {
                name: 'Medical Leave',
                type: 'line',
                stack: 'Total',
                data: data.medicalLeave
            },
            {
                name: 'Early Leave',
                type: 'line',
                stack: 'Total',
                data: data.earlyLeave
            },
            {
                name: 'Late Entry',
                type: 'line',
                stack: 'Total',
                data: data.lateEntry
            },
            {
                name: 'Absent',
                type: 'line',
                stack: 'Total',
                data: data.absent
            },
            {
                name: 'Present',
                type: 'line',
                stack: 'Total',
                data: data.present
            }
        ]
    };

    // Set the chart options
    myChart.setOption(option);

    // Resize the chart on window resize
    window.addEventListener('resize', myChart.resize);
}

// Example of passing dynamic data as a JSON object
var jsonData = {
    "months": ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'],
    "casualLeave": [1, 2, 0, 2, 1, 1, 1, 0, 2, 0, 1, 1],
    "medicalLeave": [1, 1, 0, 2, 0, 1, 0, 0, 1, 4, 0, 1],
    "earlyLeave": [4, 2, 1, 3, 2, 3, 2, 3, 2, 3, 2, 1],
    "lateEntry": [2, 2, 2, 2, 2, 1, 1, 1, 2, 3, 1, 1],
    "absent": [3, 2, 1, 3, 2, 1, 0, 1, 2, 3, 1, 1],
    "present": [20, 21, 19, 21, 18, 20, 21, 19, 21, 18, 20, 21]
};

// Call the function with the dynamic data (this can be done on the fly)
renderAttendanceLineChart(jsonData);


var currentPage = 1;
var pageSize = 5;

$('#attendanceStatus-pageSizeSelect').on('change', function () {
    var selectedSize = $(this).val();

    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
        loadTableData();
    }
});


$(document).ready(function () {
    loadTableData();

    $("#attendanceStatus-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#attendanceStatus-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#attendanceStatus-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});


let currentSortColumn = 'BloodGroupName';
let currentSortOrder = 'asc';

$('th.sort').on('click', function () {
    const column = $(this).data('sort');

    if (currentSortColumn === column) {
        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortOrder = 'asc';
    }

    loadTableData(currentSortColumn, currentSortOrder);
    updateSortingIndicator(column, currentSortOrder);
});


function updateSortingIndicator() {
    $('th.sort').each(function () {
        const $th = $(this);
        const column = $th.data('sort');
        $th.find('.sort-icon').remove();

        if (column === currentSortColumn) {
            const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
        } else {
            $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
        }
    });
}

function loadTableData(sortColumn, sortOrder) {
    var searchTerm = $("#attendanceStatus-searchInput").val();

    $.ajax({
        url: '/EmployeesAttendance/GetAlls',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var tableBody = $("#attendanceStatus-tBody");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item, index) {
                    var rowIndex = (currentPage - 1) * pageSize + index + 1;
                    tableBody.append(`
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0">${item.attendanceDate}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.checkInTime}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.checkOutTime}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.break}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.lateHour}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.overtimeHour}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.workingHours}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.shiftName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.statusName}</td>
                            
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#attendanceStatus-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#attendanceStatus-totalCount").text(`(${paginationInfo.totalItems})`);

            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        },
        error: function () {
            console.log("Error! Fetching all data.");
        }
    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#attendanceStatus-paginationLinks");
    paginationLinks.empty();
    // Window size (number of pages before/after the current page)
    const windowSize = 1;
    const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;
    // Helper function for ellipsis
    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
    // Add "First Page" and ellipsis if needed
    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addEllipsis());
    }
    // Add page number buttons within the window range
    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);
    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }
    // Add ellipsis and "Last Page" button if needed
    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    }
    // Disable or enable previous/next buttons
    $("#attendanceStatus-prevPageBtn").prop('disabled', currentPage === 1);
    $("#attendanceStatus-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});