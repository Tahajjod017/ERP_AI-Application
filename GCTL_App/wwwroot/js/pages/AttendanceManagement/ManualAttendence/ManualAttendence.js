$(document).ready(function () {
    toastr.info("Manual Attendance Page Loaded");


    // Example JSON punch data
    const punchData = [
        { time: '09:45 AM', label: 'in time', icon: 'fas fa-fingerprint' },
        { time: '01:00 PM', label: 'break in', icon: 'fas fa-fingerprint' },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true },
        { time: '', label: 'break out', icon: 'fas fa-times', notPunched: true },
        { time: '06:10 PM', label: 'out time', icon: 'fas fa-fingerprint' }
    ];

    function renderTimeline(data) {
        const container = document.getElementById('timelineContainer');
        container.innerHTML = ''; // Clear previous

        data.forEach(item => {
            const iconClass = item.notPunched ? 'timeline-icon not-punched' : 'timeline-icon';
            const html = `
                <div class="timeline-item-horizontal">
                    <div class="${iconClass}">
                        <i class="${item.icon}"></i>
                    </div>
                    <div class="timeline-label">${item.label}</div>
                    <div class="timeline-time">${item.time || '<span class="text-danger">N/A</span>'}</div>
                </div>
            `;
            container.insertAdjacentHTML('beforeend', html);
        });
    }

    // Call this when modal opens (or data updates)
  //  renderTimeline(punchData);


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
                 <a href="#" class="nav-item mx-0 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves" data-id="${item.id}">
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
                <span class="${getPossibleReasonBadgeClass(item.possibleReason)} fs-10">${item.possibleReason}</span>
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