$(document).ready(function () {
    toastr.info("Manual Attendance Page Losssaded");

    $("#Date").flatpickr({
        defaultDate: new Date(),
        disableMobile: true
    });

    //#region TimeLine
    
    let punchData = [
        { time: '09:45 AM', label: 'in time', icon: 'fas fa-fingerprint', deletable: false },
        { time: '01:00 PM', label: 'break in', icon: 'fas fa-fingerprint', deletable: false },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true, deletable: false },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true, deletable: false },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true, deletable: false },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true, deletable: false },
        { time: '06:10 PM', label: 'out time', icon: 'fas fa-fingerprint', deletable: false }
    ];

    // Function to convert time to 24-hour format for sorting
    function convertTo24Hour(timeStr) {
        if (!timeStr || typeof timeStr !== 'string' || timeStr.trim() === '') {
            return '00:00';
        }
        if (timeStr.includes('AM') || timeStr.includes('PM')) {
            const [time, modifier] = timeStr.split(' ');
            if (!time) return '00:00';
            let [hours, minutes] = time.split(':');
            if (!hours || !minutes) return '00:00';
            hours = parseInt(hours, 10);
            if (isNaN(hours)) return '00:00';
            if (hours === 12) {
                hours = 0;
            }
            if (modifier === 'PM') {
                hours += 12;
            }
            return `${hours.toString().padStart(2, '0')}:${minutes}`;
        } else {
            const [hours, minutes] = timeStr.split(':');
            if (!hours || !minutes || isNaN(parseInt(hours)) || isNaN(parseInt(minutes))) {
                return '00:00';
            }
            return `${hours.padStart(2, '0')}:${minutes}`;
        }
    }

    // Function to format time for display
    function formatTimeForDisplay(timeStr) {
        if (!timeStr || typeof timeStr !== 'string' || timeStr.trim() === '') {
            return '';
        }
        if (timeStr.includes('AM') || timeStr.includes('PM')) {
            return timeStr;
        }
        const [hours, minutes] = timeStr.split(':');
        if (!hours || !minutes) return '';
        const hour24 = parseInt(hours, 10);
        if (isNaN(hour24)) return '';
        if (hour24 === 0) {
            return `12:${minutes} AM`;
        } else if (hour24 < 12) {
            return `${hour24}:${minutes} AM`;
        } else if (hour24 === 12) {
            return `12:${minutes} PM`;
        } else {
            return `${hour24 - 12}:${minutes} PM`;
        }
    }

    // Function to sort punch data by time
    function sortPunchDataByTime(data) {
        return data.sort((a, b) => {
            if (!a.time && !b.time) return 0;
            if (!a.time) return 1;
            if (!b.time) return -1;
            const timeA = convertTo24Hour(a.time);
            const timeB = convertTo24Hour(b.time);
            return timeA.localeCompare(timeB);
        });
    }

    function renderHorizontalTimeline(data, containerId, newIndex = -1, inputRect = null) {
        const container = document.getElementById(containerId);
        container.innerHTML = '';

        data.forEach((item, index) => {
            const iconClass = item.notPunched ? 'timeline-icon not-punched' : 'timeline-icon';
            const timeDisplay = item.time ? formatTimeForDisplay(item.time) : '<span class="text-danger">N/A</span>';
            const deleteBtn = item.deletable ? `<div class="timeline-delete-btn" data-index="${index}" data-type="horizontal"><i class="fas fa-times"></i></div>` : '';
            const deletableClass = item.deletable ? 'deletable' : '';
            const animationClass = index === newIndex ? 'slide-from-input' : '';

            const html = `
            <div class="timeline-item-horizontal ${deletableClass} ${animationClass}" style="position: relative;">
                ${deleteBtn}
                <div class="${iconClass}">
                    <i class="${item.icon}"></i>
                </div>
                <div class="timeline-label">${item.label}</div>
                <div class="timeline-time">${timeDisplay}</div>
            </div>
        `;
            container.insertAdjacentHTML('beforeend', html);
        });

        // Apply shift animations for existing items
        if (newIndex !== -1) {
            const items = container.querySelectorAll('.timeline-item-horizontal');
            items.forEach((item, idx) => {
                if (idx < newIndex) {
                    item.classList.add('shift-left');
                } else if (idx > newIndex) {
                    item.classList.add('shift-right');
                }
                setTimeout(() => {
                    item.classList.remove('shift-left', 'shift-right');
                }, 400);
            });

            // Set initial position of new item to input field
            if (newIndex !== -1 && inputRect) {
                const newItem = items[newIndex];
                if (newItem) {
                    const containerRect = container.getBoundingClientRect();
                    const offsetX = inputRect.left - containerRect.left;
                    newItem.style.setProperty('--start-x', `${offsetX}px`);
                }
            }
        }
    }

    function renderVerticalTimeline(data, containerId, newIndex = -1, inputRect = null) {
        const container = document.getElementById(containerId);
        container.innerHTML = '';

        data.forEach((item, index) => {
            const iconClass = item.notPunched ? 'timeline-icon not-punched' : 'timeline-icon';
            const timeDisplay = item.time ? formatTimeForDisplay(item.time) : '<span class="not-punched-text">Not Punched</span>';
            const deleteBtn = item.deletable ? `<div class="timeline-delete-btn" data-index="${index}" data-type="vertical"><i class="fas fa-times"></i></div>` : '';
            const deletableClass = item.deletable ? 'deletable' : '';
            const animationClass = index === newIndex ? 'slide-from-input' : '';

            const html = `
            <div class="timeline-item ${deletableClass} ${animationClass}" style="position: relative;">
                ${deleteBtn}
                <div class="${iconClass}">
                    <i class="${item.icon}"></i>
                </div>
                <div class="timeline-content">
                    <div class="timeline-label">${item.label}</div>
                    <div class="timeline-time">${timeDisplay}</div>
                </div>
            </div>
        `;
            container.insertAdjacentHTML('beforeend', html);
        });

        // Apply shift animations for existing items
        if (newIndex !== -1) {
            const items = container.querySelectorAll('.timeline-item');
            items.forEach((item, idx) => {
                if (idx < newIndex) {
                    item.classList.add('shift-up');
                } else if (idx > newIndex) {
                    item.classList.add('shift-down');
                }
                setTimeout(() => {
                    item.classList.remove('shift-up', 'shift-down');
                }, 400);
            });

            // Set initial position of new item to input field
            if (newIndex !== -1 && inputRect) {
                const newItem = items[newIndex];
                if (newItem) {
                    const containerRect = container.getBoundingClientRect();
                    const offsetY = inputRect.top - containerRect.top;
                    newItem.style.setProperty('--start-y', `${offsetY}px`);
                }
            }
        }
    }

    // Function to delete time entry with animation
    function deleteTimeEntry(index, timelineType = 'both') {
        
            const horizontalItems = document.querySelectorAll('.timeline-item-horizontal');
            const verticalItems = document.querySelectorAll('.timeline-item');

            // Apply fly-out animation to the deleted item
            if (timelineType === 'both' || timelineType === 'horizontal') {
                if (horizontalItems[index]) {
                    horizontalItems[index].classList.add('fly-out');
                }
            }
            if (timelineType === 'both' || timelineType === 'vertical') {
                if (verticalItems[index]) {
                    verticalItems[index].classList.add('fly-out');
                }
            }

            // Apply smooth shift animations to remaining items
            setTimeout(() => {
                punchData.splice(index, 1);
                if (timelineType === 'both' || timelineType === 'horizontal') {
                    const horizontalItems = document.querySelectorAll('.timeline-item-horizontal');
                    horizontalItems.forEach((item, idx) => {
                        if (idx < index) {
                            item.classList.add('shift-right');
                        } else if (idx >= index) {
                            item.classList.add('shift-left');
                        }
                    });
                    renderHorizontalTimeline(punchData, 'timelineContainer');
                    initHorizontalScroll('timelineContainer');
                    setTimeout(() => {
                        horizontalItems.forEach(item => {
                            item.classList.remove('shift-left', 'shift-right');
                        });
                    }, 600); // Match smooth shift duration
                }
                if (timelineType === 'both' || timelineType === 'vertical') {
                    const verticalItems = document.querySelectorAll('.timeline-item');
                    verticalItems.forEach((item, idx) => {
                        if (idx < index) {
                            item.classList.add('shift-down');
                        } else if (idx >= index) {
                            item.classList.add('shift-up');
                        }
                    });
                    renderVerticalTimeline(punchData, 'verticalTimelineContainer');
                    setTimeout(() => {
                        verticalItems.forEach(item => {
                            item.classList.remove('shift-up', 'shift-down');
                        });
                    }, 600); // Match smooth shift duration
                }
            }, 400); // Match fly-out animation duration
       
    }

    // Function to add new time entry with animation
    function addTimeEntry(timeValue, timelineType = 'both', inputElement = null) {
        if (!timeValue || typeof timeValue !== 'string' || timeValue.trim() === '') {
            alert('Please enter a valid time');
            return;
        }

        const newEntry = {
            time: timeValue,
            label: 'manual entry',
            icon: 'fas fa-plus',
            deletable: true
        };

        punchData.push(newEntry);
        punchData = sortPunchDataByTime(punchData);
        const newIndex = punchData.findIndex(item => item === newEntry);
        const inputRect = inputElement ? inputElement.getBoundingClientRect() : null;

        if (timelineType === 'both' || timelineType === 'horizontal') {
            renderHorizontalTimeline(punchData, 'timelineContainer', newIndex, inputRect);
            initHorizontalScroll('timelineContainer');
        }
        if (timelineType === 'both' || timelineType === 'vertical') {
            renderVerticalTimeline(punchData, 'verticalTimelineContainer', newIndex, inputRect);
        }
    }

    // Initialize horizontal scroll
    function initHorizontalScroll(containerId) {
        const container = document.getElementById(containerId);
        container.addEventListener('wheel', function (e) {
            e.preventDefault();
            container.scrollLeft += e.deltaY;
        });
    }

    // Event delegation for delete buttons
    document.addEventListener('click', function (e) {
        if (e.target.closest('.timeline-delete-btn')) {
            const deleteBtn = e.target.closest('.timeline-delete-btn');
            const index = parseInt(deleteBtn.getAttribute('data-index'));
            const type = deleteBtn.getAttribute('data-type');
            if (punchData[index] && punchData[index].deletable) {
                deleteTimeEntry(index, type);
            }
        }
    });

    // Handle + button clicks for horizontal timeline
    document.getElementById('btnAddTime').addEventListener('click', function () {
        const timeInput = document.getElementById('time');
        const timeValue = timeInput.value.trim();
        if (timeValue) {
            addTimeEntry(timeValue, 'horizontal', timeInput);
            timeInput.value = '';
        } else {
            alert('Please enter a valid time');
        }
    });

    // Handle + button clicks for vertical timeline
    document.getElementById('btnAddTime2').addEventListener('click', function () {
        const timeInput = document.getElementById('time2');
        const timeValue = timeInput.value.trim();
        if (timeValue) {
            addTimeEntry(timeValue, 'vertical', timeInput);
            timeInput.value = '';
        } else {
            alert('Please enter a valid time');
        }
    });

    // Initial render
    renderHorizontalTimeline(punchData, 'timelineContainer');
    renderVerticalTimeline(punchData, 'verticalTimelineContainer');
    initHorizontalScroll('timelineContainer');


    //#endregion TimeLine
    



    $(".timepicker-12hr").flatpickr({
        enableTime: true,       // ✅ Enables time selection (hours & minutes)
        noCalendar: true,       // ✅ Hides the calendar view, showing only the time picker
        dateFormat: "H:i",      // h = 12-hour, H = 24-hour, i = minutes, K = AM/PM
        time_24hr: true,        // ✅ Uses 24-hour time format (00:00–23:59 instead of 12-hour AM/PM)
        disableMobile: true,    // ✅ Prevents the native mobile date/time picker
        allowInput: true,        // optional: lets user leave it blank
        clickOpens: true,        // opens on click only
        defaultDate: null,       // explicitly prevents pre-filling
        //// ✅ Sets the default time to show when the picker opens
        //defaultHour: 9,         // default hour (0–23)
        //defaultMinute: 30,      // default minute (0–59)
        //minuteIncrement: 5,
        //minTime: "09:00",       // ✅ Restricts the minimum allowed time
        //maxTime: "18:00",       // ✅ Restricts the maximum allowed time
        //enableSeconds: true,    // ✅ Whether seconds can be selected (you’ll also need to update dateFormat)
        //allowInput: false,      // ✅ Disables manual typing into the input field
        //// ✅ Disables the entire picker
        //// Can be used to toggle state from JS: instance.set('disable', true/false)
        //disable: [function (date) {
        //    return false; // no disable by default
        //}],
        //// ✅ Hook that runs when a date/time is selected
        //onChange: function (selectedDates, dateStr, instance) {
        //    console.log("Time selected:", dateStr);
        //}
    });

    const attendanceData = [
        {
            id: 1,
            employeeName: "Faruk Hasan",
            employeeRole: "Admin",
            department: "IT",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //01.jpg",
            attendanceDate: "20 Jul 2025",
            scheduleTime: "09:00 AM - 06:00 PM",
            actualInTime: "09:45 AM",
            actualOutTime: "Not Punched",
            breakInTime: "01:00 PM",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 3,
            possibleReason: "Break In/Out Missing"
        },
        {
            id: 2,
            employeeName: "Aminul Islam",
            employeeRole: "Manager",
            department: "HR",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //02.jpg",
            attendanceDate: "19 Jul 2025",
            scheduleTime: "08:30 AM - 05:30 PM",
            actualInTime: "Not Punched",
            actualOutTime: "05:30 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 1,
            possibleReason: "In Time Missing"
        },
        {
            id: 3,
            employeeName: "Sarah Ahmed",
            employeeRole: "Developer",
            department: "IT",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //03.jpg",
            attendanceDate: "18 Jul 2025",
            scheduleTime: "09:30 AM - 06:30 PM",
            actualInTime: "09:30 AM",
            actualOutTime: "Not Punched",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 1,
            possibleReason: "Out Time Missing"
        },
        {
            id: 4,
            employeeName: "Rafiq Khan",
            employeeRole: "Designer",
            department: "Marketing",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //04.jpg",
            attendanceDate: "17 Jul 2025",
            scheduleTime: "10:00 AM - 07:00 PM",
            actualInTime: "10:30 AM",
            actualOutTime: "07:00 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 3,
            possibleReason: "Break In/Out Missing"
        },
        //{
        //    id: 5,
        //    employeeName: "Nasir Uddin",
        //    employeeRole: "Accountant",
        //    department: "Finance",
        //    employeeImage: "https://placehold.co/300x200?text=Placeholder", //05.jpg",
        //    attendanceDate: "16 Jul 2025",
        //    scheduleTime: "09:00 AM - 06:00 PM",
        //    actualInTime: "09:00 AM",
        //    actualOutTime: "07:30 PM",
        //    breakInTime: "01:00 PM",
        //    breakOutTime: "01:30 PM",
        //    overtime: "1.5 Hours",
        //    biometricHits: 4,
        //    possibleReason: "Complete Record"
        //},
        {
            id: 6,
            employeeName: "Rashida Khatun",
            employeeRole: "Executive",
            department: "Operations",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //06.jpg",
            attendanceDate: "15 Jul 2025",
            scheduleTime: "08:00 AM - 05:00 PM",
            actualInTime: "Not Punched",
            actualOutTime: "Not Punched",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 4,
            possibleReason: "Multiple Missing"
        },
        {
            id: 7,
            employeeName: "Tina Rahman",
            employeeRole: "Analyst",
            department: "Finance",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //07.jpg",
            attendanceDate: "14 Jul 2025",
            scheduleTime: "09:00 AM - 06:00 PM",
            actualInTime: "09:00 AM",
            actualOutTime: "09:10 AM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 5,
            possibleReason: "Over Punching"
        },
        {
            id: 8,
            employeeName: "Amir Hossain",
            employeeRole: "Consultant",
            department: "HR",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //08.jpg",
            attendanceDate: "13 Jul 2025",
            scheduleTime: "09:00 AM - 06:00 PM",
            actualInTime: "Not Punched",
            actualOutTime: "06:30 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "0.5 Hours",
            biometricHits: 1,
            possibleReason: "In Time Missing"
        },
        {
            id: 9,
            employeeName: "Fatima Begum",
            employeeRole: "Coordinator",
            department: "Operations",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //09.jpg",
            attendanceDate: "12 Jul 2025",
            scheduleTime: "09:00 AM - 06:00 PM",
            actualInTime: "05:00 PM",
            actualOutTime: "01:00 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            overtime: "No Overtime",
            biometricHits: 2,
            possibleReason: "Over Punching"
        },
        {
            id: 10,
            employeeName: "Priya Das",
            employeeRole: "Developer",
            department: "IT",
            employeeImage: "https://placehold.co/300x200?text=Placeholder", //10.jpg",
            attendanceDate: "11 Jul 2025",
            scheduleTime: "09:00 AM - 06:00 PM",
            actualInTime: "09:00 AM",
            actualOutTime: "10:00 AM",
            breakInTime: "10:05 AM",
            breakOutTime: "10:15 AM",
            overtime: "No Overtime",
            biometricHits: 5,
            possibleReason: "Over Punching"
        }
    ];

    populateAttendanceTable();
    initializeFilters();

    // Function to get badge class for possible reason
    function getPossibleReasonBadgeClass(reason) {
        switch (reason) {
            case 'Complete Record':
                return 'badge-phoenix badge-phoenix-success';
            case 'In Time Missing':
            case 'Out Time Missing':
                return 'badge-phoenix badge-phoenix-warning';
            case 'Break In Missing':
            case 'Break Out Missing':
                return 'badge-phoenix badge-phoenix-info';
            case 'Multiple Missing':
            case 'Over Punching':
                return 'badge-phoenix badge-phoenix-danger';
            default:
                return 'badge-phoenix badge-phoenix-secondary';
        }
    }

    // Function to get overtime badge class
    function getOvertimeBadgeClass(overtime) {
        if (overtime === 'No Overtime') {
            return 'badge-phoenix badge-phoenix-secondary';
        } else {
            return 'badge-phoenix badge-phoenix-success';
        }
    }

   
    //function populateAttendanceTable() {
    //    const tbody = document.getElementById('attendance-body');
    //    tbody.innerHTML = '';

    //    attendanceData.forEach(item => {
    //        const row = `
    //                    <tr class="hover-actions-trigger btn-reveal-trigger position-static">
    //                        <td class="fs-9 align-middle py-0">
    //                            <div class="form-check mb-0 fs-8">
    //                                <input class="form-check-input" type="checkbox" data-bulk-select-row='${JSON.stringify(item)}' />
    //                            </div>
    //                        </td>
    //                        <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
    //                            <div class="d-flex align-items-center file-name-icon">
    //                                <div class="avatar avatar-m avatar-bordered me-4">
    //                                    <img class="rounded-circle" src="${item.employeeImage}" alt="${item.employeeName}" style="width: 40px; height: 40px;" />
    //                                </div>
    //                                <div class="ms-1">
    //                                    <h6 class="fw-bold mb-0">${item.employeeName}</h6>
    //                                    <span class="fs-12 fw-normal text-muted">${item.employeeRole}</span>
    //                                </div>
    //                            </div>
    //                        </td>
    //                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.department}</td>
    //                        <td class="attendanceDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.attendanceDate}</td>
    //                        <td class="scheduleTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.scheduleTime}</td>
    //                        <td class="overtime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
    //                            <span class="${getOvertimeBadgeClass(item.overtime)} fs-9">${item.overtime}</span>
    //                        </td>
    //                        <td class="biometricHits align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
    //                            <span class="badge text-bg-primary fs-9">${item.biometricHits}</span>
    //                        </td>
    //                        <td class="possibleReason align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
    //                            <span class="${getPossibleReasonBadgeClass(item.possibleReason)} fs-9">${item.possibleReason}</span>
    //                        </td>
    //                        <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
    //                            <div class="btn-reveal-trigger position-static">
    //                                <a href="#" class="nav-item mx-2 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves" data-id="${item.id}">
    //                                    <i class="fas fa-info-circle text-success"></i>
    //                                </a>

    //                            </div>
    //                        </td>
    //                    </tr>
    //                `;
    //        tbody.innerHTML += row;
    //    });
    //}



    function populateAttendanceTable() {
        const tbody = document.getElementById('attendance-body');
        tbody.innerHTML = '';

        attendanceData.forEach(item => {
            const row = document.createElement('tr');
            row.className = 'hover-actions-trigger btn-reveal-trigger position-static';
            row.setAttribute('data-id', item.id); // Optional: store ID here
            row.style.cursor = 'pointer';

            
            //row.addEventListener('click', () => {
            //    const modalTrigger = document.querySelector('#edit_leaves');
            //    const modal = new bootstrap.Modal(modalTrigger);
            //    modal.show();
                
            //});

            row.innerHTML = `
              <td class="fs-9 align-middle py-0">
                <div class="form-check mb-0 fs-8">
                  <input class="form-check-input" type="checkbox" data-bulk-select-row='${JSON.stringify(item)}' />
                </div>
              </td>
              <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                 <a href="#" class="nav-item mx-0 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves_horizontal" data-id="${item.id}">
                <div class="d-flex align-items-center file-name-icon">
                  <div class="avatar avatar-m avatar-bordered me-4 mb-2">
                    <img class="rounded-circle" src="${item.employeeImage}" alt="${item.employeeName}" style="width: 40px; height: 40px;" />
                  </div>
                  <div class="ms-1">
                    <h6 class="fw-bold mb-0 text-primary">${item.employeeName}</h6>
                    <span class="fs-12 fw-normal text-muted">${item.employeeRole}</span>
                  </div>
                </div>
                </a>
              </td>
              <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.department}</td>
              <td class="attendanceDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.attendanceDate}</td>
              <td class="scheduleTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.scheduleTime}</td>
              <td class="overtime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                <span class="${getOvertimeBadgeClass(item.overtime)} fs-10">${item.overtime}</span>
              </td>
              <td class="biometricHits align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                <span class="badge-phoenix badge-phoenix-primary fs-10">${item.biometricHits}</span>
              </td>
              <td class="possibleReason align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
              <a href="#" class="nav-item mx-0 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves_vertical" data-id="${item.id}">
                <span class="${getPossibleReasonBadgeClass(item.possibleReason)} fs-10">${item.possibleReason}</span>
                </a>
              </td>

                

            `;

            tbody.appendChild(row);
        });
    }

    
    function initializeFilters() {
        const departmentFilter = document.getElementById('departmentFilter');
        const possibleReasonFilter = document.getElementById('possibleReasonFilter');
        const sortFilter = document.getElementById('sortFilter');

        // Add event listeners for filters
        departmentFilter.addEventListener('change', applyFilters);
        possibleReasonFilter.addEventListener('change', applyFilters);
        sortFilter.addEventListener('change', applyFilters);
    }

    // Apply filters to the table
    function applyFilters() {
        const departmentValue = document.getElementById('departmentFilter').value;
        const possibleReasonValue = document.getElementById('possibleReasonFilter').value;
        const sortValue = document.getElementById('sortFilter').value;

        let filteredData = [...attendanceData];

        // Apply department filter
        if (departmentValue) {
            filteredData = filteredData.filter(item => item.department === departmentValue);
        }

        // Apply possible reason filter
        if (possibleReasonValue) {
            filteredData = filteredData.filter(item => item.possibleReason === possibleReasonValue);
        }

        // Apply sorting
        if (sortValue) {
            switch (sortValue) {
                case 'Latest':
                    filteredData.sort((a, b) => new Date(b.attendanceDate) - new Date(a.attendanceDate));
                    break;
                case 'Oldest':
                    filteredData.sort((a, b) => new Date(a.attendanceDate) - new Date(b.attendanceDate));
                    break;
                case 'Employee Name':
                    filteredData.sort((a, b) => a.employeeName.localeCompare(b.employeeName));
                    break;
                case 'Department':
                    filteredData.sort((a, b) => a.department.localeCompare(b.department));
                    break;
            }
        }

        // Re-populate table with filtered data
        populateFilteredTable(filteredData);
    }

    // Function to populate table with filtered data
    function populateFilteredTable(data) {
        const tbody = document.getElementById('attendance-body');
        tbody.innerHTML = '';

        data.forEach(item => {
            const row = `
                        <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="fs-9 align-middle py-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" data-bulk-select-row='${JSON.stringify(item)}' />
                                </div>
                            </td>
                            <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                                <div class="d-flex align-items-center file-name-icon">
                                    <div class="avatar avatar-m avatar-bordered me-4">
                                        <img class="rounded-circle" src="${item.employeeImage}" alt="${item.employeeName}" style="width: 40px; height: 40px;" />
                                    </div>
                                    <div class="ms-1">
                                        <h6 class="fw-bold mb-0">${item.employeeName}</h6>
                                        <span class="fs-12 fw-normal text-muted">${item.employeeRole}</span>
                                    </div>
                                </div>
                            </td>
                            <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.department}</td>
                            <td class="attendanceDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.attendanceDate}</td>
                            <td class="scheduleTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.scheduleTime}</td>
                            <td class="overtime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <span class="${getOvertimeBadgeClass(item.overtime)}">${item.overtime}</span>
                            </td>
                            <td class="biometricHits align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <span class="badge-phoenix badge-phoenix-primary">${item.biometricHits}</span>
                            </td>
                            <td class="possibleReason align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <span class="${getPossibleReasonBadgeClass(item.possibleReason)}">${item.possibleReason}</span>
                            </td>
                            <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                                <div class="btn-reveal-trigger position-static">
                                    <a href="#" class="nav-item mx-2 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves" data-id="${item.id}">
                                        <i class="fas fa-info-circle text-success"></i>
                                    </a>
                                   
                                </div>
                            </td>
                        </tr>
                    `;
            tbody.innerHTML += row;
        });
    }
});