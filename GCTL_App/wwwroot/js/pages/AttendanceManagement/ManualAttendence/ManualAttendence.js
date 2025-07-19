$(document).ready(function () {
    toastr.info("Manual Attendance Page Loaded");

    const attendanceData = [
        {
            id: 1,
            employeeName: "Faruk Hasan",
            employeeRole: "Admin",
            department: "IT",
            employeeImage: "../../assets/img/users/user-01.jpg",
            attendanceDate: "20 Jul 2025",
            expectedInTime: "09:00 AM",
            expectedOutTime: "06:00 PM",
            actualInTime: "09:45 AM",
            actualOutTime: "Not Punched",
            breakInTime: "01:00 PM",
            breakOutTime: "Not Punched",
            status: "Missing Break Out",
            priority: "High",
            statusClass: "badge text-bg-warning",
            priorityClass: "badge text-bg-danger"
        },
        {
            id: 2,
            employeeName: "Aminul Islam",
            employeeRole: "Manager",
            department: "HR",
            employeeImage: "../../assets/img/users/user-02.jpg",
            attendanceDate: "19 Jul 2025",
            expectedInTime: "08:30 AM",
            expectedOutTime: "05:30 PM",
            actualInTime: "Not Punched",
            actualOutTime: "05:30 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "Partial Entry",
            priority: "Medium",
            statusClass: "badge text-bg-info",
            priorityClass: "badge text-bg-warning"
        },
        {
            id: 3,
            employeeName: "Sarah Ahmed",
            employeeRole: "Developer",
            department: "IT",
            employeeImage: "../../assets/img/users/user-03.jpg",
            attendanceDate: "18 Jul 2025",
            expectedInTime: "09:30 AM",
            expectedOutTime: "06:30 PM",
            actualInTime: "09:30 AM",
            actualOutTime: "Not Punched",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "Missing Entry",
            priority: "High",
            statusClass: "badge text-bg-warning",
            priorityClass: "badge text-bg-danger"
        },
        {
            id: 4,
            employeeName: "Rafiq Khan",
            employeeRole: "Designer",
            department: "Marketing",
            employeeImage: "../../assets/img/users/user-04.jpg",
            attendanceDate: "17 Jul 2025",
            expectedInTime: "10:00 AM",
            expectedOutTime: "07:00 PM",
            actualInTime: "10:30 AM",
            actualOutTime: "07:00 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "Missing Break",
            priority: "Low",
            statusClass: "badge text-bg-secondary",
            priorityClass: "badge text-bg-success"
        },
        {
            id: 5,
            employeeName: "Nasir Uddin",
            employeeRole: "Accountant",
            department: "Finance",
            employeeImage: "../../assets/img/users/user-05.jpg",
            attendanceDate: "16 Jul 2025",
            expectedInTime: "09:00 AM",
            expectedOutTime: "06:00 PM",
            actualInTime: "09:00 AM",
            actualOutTime: "04:30 PM",
            breakInTime: "01:00 PM",
            breakOutTime: "01:30 PM",
            status: "Early Exit",
            priority: "Medium",
            statusClass: "badge text-bg-primary",
            priorityClass: "badge text-bg-warning"
        },
        {
            id: 6,
            employeeName: "Rashida Khatun",
            employeeRole: "Executive",
            department: "Operations",
            employeeImage: "../../assets/img/users/user-06.jpg",
            attendanceDate: "15 Jul 2025",
            expectedInTime: "08:00 AM",
            expectedOutTime: "05:00 PM",
            actualInTime: "Not Punched",
            actualOutTime: "Not Punched",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "No Entry",
            priority: "High",
            statusClass: "badge text-bg-danger",
            priorityClass: "badge text-bg-danger"
        },
        {
            id: 7,
            employeeName: "Tina Rahman",
            employeeRole: "Analyst",
            department: "Finance",
            employeeImage: "../../assets/img/users/user-07.jpg",
            attendanceDate: "14 Jul 2025",
            expectedInTime: "09:00 AM",
            expectedOutTime: "06:00 PM",
            actualInTime: "09:00 AM",
            actualOutTime: "09:10 AM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "Double Entry",
            priority: "High",
            statusClass: "badge text-bg-danger",
            priorityClass: "badge text-bg-danger"
        },
        {
            id: 8,
            employeeName: "Amir Hossain",
            employeeRole: "Consultant",
            department: "HR",
            employeeImage: "../../assets/img/users/user-08.jpg",
            attendanceDate: "13 Jul 2025",
            expectedInTime: "09:00 AM",
            expectedOutTime: "06:00 PM",
            actualInTime: "Not Punched",
            actualOutTime: "06:30 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "Missing Entry",
            priority: "Medium",
            statusClass: "badge text-bg-warning",
            priorityClass: "badge text-bg-warning"
        },
        {
            id: 9,
            employeeName: "Fatima Begum",
            employeeRole: "Coordinator",
            department: "Operations",
            employeeImage: "../../assets/img/users/user-09.jpg",
            attendanceDate: "12 Jul 2025",
            expectedInTime: "09:00 AM",
            expectedOutTime: "06:00 PM",
            actualInTime: "05:00 PM",
            actualOutTime: "01:00 PM",
            breakInTime: "Not Punched",
            breakOutTime: "Not Punched",
            status: "Partial Entry",
            priority: "High",
            statusClass: "badge text-bg-info",
            priorityClass: "badge text-bg-danger"
        },
        {
            id: 10,
            employeeName: "Priya Das",
            employeeRole: "Developer",
            department: "IT",
            employeeImage: "../../assets/img/users/user-10.jpg",
            attendanceDate: "11 Jul 2025",
            expectedInTime: "09:00 AM",
            expectedOutTime: "06:00 PM",
            actualInTime: "09:00 AM",
            actualOutTime: "10:00 AM",
            breakInTime: "10:05 AM",
            breakOutTime: "10:15 AM",
            status: "Over Punching",
            priority: "High",
            statusClass: "badge text-bg-danger",
            priorityClass: "badge text-bg-danger"
        }
    ];

    populateAttendanceTable();
    initializeFilters();
   // initializeEditModal();

    // Function to populate the table
    function populateAttendanceTable() {
        const tbody = document.getElementById('attendance-body');
        tbody.innerHTML = '';

        attendanceData.forEach(item => {
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
                                    <img class="rounded-circle" src="${item.employeeImage}" alt="${item.employeeName}" />
                                </div>
                                <div class="ms-1">
                                    <h6 class="fw-bold">${item.employeeName}</h6>
                                    <span class="fs-12 fw-normal">${item.employeeRole}</span>
                                </div>
                            </div>
                        </td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.department}</td>
                        <td class="attendanceDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.attendanceDate}</td>
                        <td class="expectedInTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.expectedInTime}</td>
                        <td class="expectedOutTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.expectedOutTime}</td>
                        <td class="actualTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                            <div class="fs-12">
                                <div>In: ${item.actualInTime}</div>
                                <div>Out: ${item.actualOutTime}</div>
                                <div>Break In: ${item.breakInTime}</div>
                                <div>Break Out: ${item.breakOutTime}</div>
                            </div>
                        </td>
                        <td class="status align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                            <span class="${item.statusClass}">${item.status}</span>
                        </td>
                        <td class="priority align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                            <span class="${item.priorityClass}">${item.priority}</span>
                        </td>
                        <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                            <div class="btn-reveal-trigger position-static">
                                <a href="#" class="nav-item mx-2 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves" data-id="${item.id}">
                                    <i class="fas fa-edit text-success"></i>
                                </a>
                               
                            </div>
                        </td>
                    </tr>
                `;
            tbody.innerHTML += row;
        });
    }

    // Initialize table when page loads
    document.addEventListener('DOMContentLoaded', function () {
        populateAttendanceTable();
        initializeFilters();
        initializeEditModal();
    });

    // Initialize filter functionality
    function initializeFilters() {
        const departmentFilter = document.getElementById('departmentFilter');
        const statusFilter = document.getElementById('statusFilter');
        const priorityFilter = document.getElementById('priorityFilter');
        const sortFilter = document.getElementById('sortFilter');
        const dateRangePicker = document.getElementById('dateRangePicker');

        // Add event listeners for filters
        departmentFilter.addEventListener('change', applyFilters);
        statusFilter.addEventListener('change', applyFilters);
        priorityFilter.addEventListener('change', applyFilters);
        sortFilter.addEventListener('change', applyFilters);
        dateRangePicker.addEventListener('change', applyFilters);
    }

    // Apply filters to the table
    function applyFilters() {
        const departmentValue = document.getElementById('departmentFilter').value;
        const statusValue = document.getElementById('statusFilter').value;
        const priorityValue = document.getElementById('priorityFilter').value;
        const sortValue = document.getElementById('sortFilter').value;

        let filteredData = [...attendanceData];

        // Apply department filter
        if (departmentValue) {
            filteredData = filteredData.filter(item => item.department === departmentValue);
        }

        // Apply status filter
        if (statusValue) {
            filteredData = filteredData.filter(item => item.status === statusValue);
        }

        // Apply priority filter
        if (priorityValue) {
            filteredData = filteredData.filter(item => item.priority === priorityValue);
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
                case 'Priority':
                    const priorityOrder = { 'High': 3, 'Medium': 2, 'Low': 1 };
                    filteredData.sort((a, b) => priorityOrder[b.priority] - priorityOrder[a.priority]);
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
                                    <img class="rounded-circle" src="${item.employeeImage}" alt="${item.employeeName}" />
                                </div>
                                <div class="ms-1">
                                    <h6 class="fw-bold">${item.employeeName}</h6>
                                    <span class="fs-12 fw-normal">${item.employeeRole}</span>
                                </div>
                            </div>
                        </td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.department}</td>
                        <td class="attendanceDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.attendanceDate}</td>
                        <td class="expectedInTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.expectedInTime}</td>
                        <td class="expectedOutTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.expectedOutTime}</td>
                        <td class="actualTime align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                            <div class="fs-12">
                                <div>In: ${item.actualInTime}</div>
                                <div>Out: ${item.actualOutTime}</div>
                                <div>Break In: ${item.breakInTime}</div>
                                <div>Break Out: ${item.breakOutTime}</div>
                            </div>
                        </td>
                        <td class="status align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                            <span class="${item.statusClass}">${item.status}</span>
                        </td>
                        <td class="priority align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                            <span class="${item.priorityClass}">${item.priority}</span>
                        </td>
                        <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                            <div class="btn-reveal-trigger position-static">
                                <a href="#" class="nav-item mx-2 edit-attendance" data-bs-toggle="modal" data-bs-target="#edit_leaves" data-id="${item.id}">
                                    <i class="fas fa-edit text-success"></i>
                                </a>
                                <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#approve_attendance" data-id="${item.id}">
                                    <i class="fas fa-check text-primary"></i>
                                </a>
                                <a href="#" class="nav-item mx-2" data-bs-toggle="modal" data-bs-target="#delete_modal" data-id="${item.id}">
                                    <i class="fas fa-trash text-danger"></i>
                                </a>
                            </div>
                        </td>
                    </tr>
                `;
            tbody.innerHTML += row;
        });
    }

    // Initialize edit modal functionality
//    function initializeEditModal() {
//        $('.edit-attendance').on('click', function () {
//            const id = $(this).data('id');
//            const item = attendanceData.find(item => item.id === id);

//            // Populate the edit modal fields
//            const modal = $('#edit_leaves');
//            modal.find('input[name="employeeName"]').val(item.employeeName);
//            modal.find('input[name="attendanceDate"]').val(item.attendanceDate);
//            modal.find('input[name="actualInTime"]').val(item.actualInTime === "Not Punched" ? "" : item.actualInTime);
//            modal.find('input[name="actualOutTime"]').val(item.actualOutTime === "Not Punched" ? "" : item.actualOutTime);
//            modal.find('input[name="breakInTime"]').val(item.breakInTime === "Not Punched" ? "" : item.breakInTime);
//            modal.find('input[name="breakOutTime"]').val(item.breakOutTime === "Not Punched" ? "" : item.breakOutTime);

//            // Update status based on inputs
//            modal.find('form').off('submit').on('submit', function (e) {
//                e.preventDefault();

//                // Update attendance data
//                item.actualInTime = modal.find('input[name="actualInTime"]').val() || "Not Punched";
//                item.actualOutTime = modal.find('input[name="actualOutTime"]').val() || "Not Punched";
//                item.breakInTime = modal.find('input[name="breakInTime"]').val() || "Not Punched";
//                item.breakOutTime = modal.find('input[name="breakOutTime"]').val() || "Not Punched";

//                // Update status based on punch data
//                if (item.actualInTime === "Not Punched" && item.actualOutTime === "Not Punched") {
//                    item.status = "No Entry";
//                    item.statusClass = "badge text-bg-danger";
//                } else if (item.actualInTime === "Not Punched" || item.actualOutTime === "Not Punched") {
//                    item.status = "Partial Entry";
//                    item.statusClass = "badge text-bg-info";
//                } else if (item.breakInTime === "Not Punched" || item.breakOutTime === "Not Punched") {
//                    item.status = "Missing Break";
//                    item.statusClass = "badge text-bg-warning";
//                } else {
//                    item.status = "Complete";
//                    item.statusClass = "badge text-bg-success";
//                }

//                // Close modal and refresh table
//                modal.modal('hide');
//                populateAttendanceTable();
//                toastr.success("Attendance updated successfully");
//            });
//        });
    //    }



});