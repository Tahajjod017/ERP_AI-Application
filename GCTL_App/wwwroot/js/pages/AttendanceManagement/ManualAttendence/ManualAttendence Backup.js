$(document).ready(function () {
    toastr.info("Manual Attendance Page Loaded");

    let currentPage = 1;
    let pageSize = 10;
    let totalRecords = 0;

    

    //#region Function to convert time to 24-hour format for sorting (client-side for timeline)
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

    //#endregion

    //#region Function to sort punch data by time (client-side for timeline)
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

    //#endregion

    //#region render timeline with scroll
    function renderHorizontalTimeline(data, containerId, newIndex = -1, inputRect = null) {
        try {
            
            const container = document.getElementById(containerId);
            if (!container) {
                console.warn(`Container with ID "${containerId}" not found.`);
                return;
            }

            if (!Array.isArray(data) || data.length === 0) {
                console.warn('Timeline data is empty or invalid:', data);
                container.innerHTML = '<div class="timeline-empty">No timeline data available.</div>';
                return;
            }

            container.innerHTML = '';

            data.forEach((item, index) => {
                const iconClass = item.notPunched ? 'timeline-icon not-punched' : 'timeline-icon';
                const timeDisplay = item.time ? formatTimeForDisplay(item.time) : '<span class="text-danger">N/A</span>';
                const deleteBtn = item.deletable
                    ? `<div class="timeline-delete-btn" data-index="${index}" data-type="horizontal">
                        <i class="fas fa-times"></i>
                    </div>`
                    : '';
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

                if (inputRect) {
                    const newItem = items[newIndex];
                    if (newItem) {
                        const containerRect = container.getBoundingClientRect();
                        const offsetX = inputRect.left - containerRect.left;
                        newItem.style.setProperty('--start-x', `${offsetX}px`);
                    }
                }
            }
        } catch (e) {
            console.error('Error rendering horizontal timeline:', e);
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

    function initHorizontalScroll(containerId) {
        const container = document.getElementById(containerId);
        container.addEventListener('wheel', function (e) {
            e.preventDefault();
            container.scrollLeft += e.deltaY;
        });
    }

    //#endregion

    //#region Fetch punch data from controller
    function fetchPunchData(employeeId, attendanceDate) {
        return $.ajax({
            url: "/ManualAttendence/GetPunchData",
            method: "GET",
            data: { employeeId: employeeId, attendanceDate: attendanceDate },
            success: function (response) {
                return response.data || [];
            },
            error: function (xhr) {
                console.error("Failed to fetch punch data:", xhr.responseText);
                toastr.error("Failed to load punch data");
                return [];
            }
        });
    }
    //#endregion

    //#region Save punch data to controller
    function savePunchData(employeeId, attendanceDate, punchData) {
        $.ajax({
            url: "/ManualAttendence/SavePunchData",
            method: "POST",
            data: { employeeId: employeeId, attendanceDate: attendanceDate, punchData: punchData },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || "Punch data saved successfully");

                    var applyModalEl = document.getElementById('edit_leaves_horizontal');
                    var applyModal = bootstrap.Modal.getInstance(applyModalEl);
                    if (!applyModal) {
                        applyModal = new bootstrap.Modal(applyModalEl);
                    }
                    applyModal.hide();
                   
                    GetLoadData(currentPage, pageSize);
                }
                else {
                    toastr.warning("Punch data saved with warnings: " + response.message);
                }
            },
            error: function (xhr) {
                console.error("Failed to save punch data:", xhr.responseText);
                toastr.error("Failed to save punch data");
            }
        });
    }
    //#endregion

    //#region Add or delete entry

    
    function deleteTimeEntry(index, employeeId, attendanceDate, timelineType = 'both') {
        const horizontalItems = document.querySelectorAll('.timeline-item-horizontal');
        const verticalItems = document.querySelectorAll('.timeline-item');

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

        setTimeout(() => {
            punchData.splice(index, 1);
           // savePunchData(employeeId, attendanceDate, punchData);
            if (timelineType === 'both' || timelineType === 'horizontal') {
                renderHorizontalTimeline(punchData, 'timelineContainer');
                initHorizontalScroll('timelineContainer');
            }
            if (timelineType === 'both' || timelineType === 'vertical') {
                renderVerticalTimeline(punchData, 'verticalTimelineContainer');
            }
        }, 400);
    }

   
    function addTimeEntry(timeValue, employeeId, attendanceDate, timelineType = 'both', inputElement = null) {
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

        //savePunchData(employeeId, attendanceDate, punchData);

        if (timelineType === 'both' || timelineType === 'horizontal') {
            renderHorizontalTimeline(punchData, 'timelineContainer', newIndex, inputRect);
            initHorizontalScroll('timelineContainer');
        }
        if (timelineType === 'both' || timelineType === 'vertical') {
            renderVerticalTimeline(punchData, 'verticalTimelineContainer', newIndex, inputRect);
        }
    }

    //#endregion

    //#region Event delegation for delete buttons
    document.addEventListener('click', function (e) {
        if (e.target.closest('.timeline-delete-btn')) {
            const deleteBtn = e.target.closest('.timeline-delete-btn');
            const index = parseInt(deleteBtn.getAttribute('data-index'));
            const type = deleteBtn.getAttribute('data-type');
            const employeeId = document.getElementById('empName').value;
            const attendanceDate = document.getElementById('Date').value;
            if (punchData[index] && punchData[index].deletable) {
                deleteTimeEntry(index, employeeId, attendanceDate, type);
            }
        }
    });

    //#endregion

    //#region Handle + button clicks for horizontal timeline

    document.getElementById('btnAddTime').addEventListener('click', function () {
        const timeInput = document.getElementById('time');
        const timeValue = timeInput.value.trim();
        const employeeId = document.getElementById('empName').value;
        const attendanceDate = document.getElementById('Date').value;
        if (timeValue) {
            addTimeEntry(timeValue, employeeId, attendanceDate, 'horizontal', timeInput);
            timeInput.value = '';
        } else {
            alert('Please enter a valid time');
        }
    });

    // Handle + button clicks for vertical timeline
    document.getElementById('btnAddTime2').addEventListener('click', function () {
        const timeInput = document.getElementById('time2');
        const timeValue = timeInput.value.trim();
        const employeeId = document.getElementById('empName2').value;
        const attendanceDate = document.getElementById('Date2').value;
        if (timeValue) {
            addTimeEntry(timeValue, employeeId, attendanceDate, 'vertical', timeInput);
            timeInput.value = '';
        } else {
            alert('Please enter a valid time');
        }
    });

    //#endregion

    //#region Initialize Flatpickr

    $("#Date").flatpickr({
        defaultDate: new Date(),
        disableMobile: true
    });

    $("#Date2").flatpickr({
        defaultDate: new Date(),
        disableMobile: true
    });

    $(".timepicker-12hr").flatpickr({
        enableTime: true,
        noCalendar: true,
        dateFormat: "H:i",
        time_24hr: true,
        disableMobile: true,
        allowInput: true,
        clickOpens: true,
        defaultDate: null
    });

    //#endregion

    //#region Fetch Table data from controller


    

    function GetLoadData(page = 1, size = pageSize, filters = {}) {
        const formData = new FormData();

        // Keys must match the C# ViewModel property names exactly (case-insensitive)
        formData.append("Page", page);
        formData.append("PageSize", size);
        formData.append("Department", filters.department || '');
        formData.append("PossibleReason", filters.possibleReason || '');
        formData.append("DateRange", filters.dateRange || '');
        formData.append("Search", filters.search || '');
        formData.append("Sort", filters.sort || '');

        console.log("Sending FormData:", Object.fromEntries(formData));

        $.ajax({
            url: "/ManualAttendance/GetAllAttendance",
            method: "POST",
            processData: false,
            contentType: false,
            headers: {
                "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() || ''
            },
            data: formData,
            success: function (response) {
                console.log("Response:", response);
                attendanceData = response.data || [];
                totalRecords = response.totalRecords || 0;
                populateAttendanceTable();
                updatePagination();
                updateSummaryCards(response.summary);
            },
            error: function (xhr, status, error) {
                console.error("AJAX error:", xhr.responseText || error);
                toastr.error(`Failed to load attendance data: ${xhr.statusText || error}`);
            }
        });
    }


    //#endregion

    function updateSummaryCards(summary) {
        document.getElementById('inTimeMissing').textContent = summary.inTimeMissing || 0;
        document.getElementById('bTinOutMissing').textContent = summary.breakTimeMissing || 0;
        document.getElementById('doubleEntry').textContent = summary.doubleEntry || 0;
        document.getElementById('holidayEntry').textContent = summary.absent || 0;
    }

    //#region Get badge
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

    function getOvertimeBadgeClass(overtime) {
        if (overtime === 'No Overtime') {
            return 'badge-phoenix badge-phoenix-secondary';
        } else {
            return 'badge-phoenix badge-phoenix-success';
        }
    }

    //#endregion

    //#region Attendence table with filter

    function populateAttendanceTable() {
        const tbody = document.getElementById('attendance-body');
        tbody.innerHTML = '';
        
        attendanceData.forEach(item => {
          
            const row = document.createElement('tr');
            row.className = 'hover-actions-trigger btn-reveal-trigger position-static';
            row.setAttribute('data-id', item.id);
            row.style.cursor = 'pointer';

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
                    <span class="${getPossibleReasonBadgeClass(item.possibleReason)} fs-10">${item.possibleReason}</span>
                </td>
            `;

            row.querySelector('.edit-attendance').addEventListener('click', function () {
                fetchPunchData(item.employeeId, item.attendanceDate).then(data => {
                   
                    debugger
                    punchData = data.data;
                    renderHorizontalTimeline(punchData, 'timelineContainer');
                    initHorizontalScroll('timelineContainer')
                    //renderVerticalTimeline(punchData, 'verticalTimelineContainer');
                    //document.getElementById('empName').value = item.employeeName + ' (' + item.employeeId +')';
                    //document.getElementById('shiftNameSpan').textContent = data.empData.shiftName;
                    document.getElementById('shiftName').value = data.empData.shiftTime;
                    document.getElementById('empName').value = data.empData.name;
                    document.getElementById('empId').value = data.empData.id;
                    document.getElementById('Date').value = item.attendanceDate;
                });
            });

            tbody.appendChild(row);
        });
    }

    function updatePagination() {
        const paginationContainer = document.querySelector('.pagination');
        paginationContainer.innerHTML = '';
        const totalPages = Math.ceil(totalRecords / pageSize);
        const maxPagesToShow = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
        let endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

        if (endPage - startPage + 1 < maxPagesToShow) {
            startPage = Math.max(1, endPage - maxPagesToShow + 1);
        }

        if (currentPage > 1) {
            paginationContainer.innerHTML += `<li class="page-item"><a class="page-link" href="#" data-page="${currentPage - 1}">Previous</a></li>`;
        }

        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === currentPage ? 'active' : '';
            paginationContainer.innerHTML += `<li class="page-item ${activeClass}"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`;
        }

        if (currentPage < totalPages) {
            paginationContainer.innerHTML += `<li class="page-item"><a class="page-link" href="#" data-page="${currentPage + 1}">Next</a></li>`;
        }

        document.querySelectorAll('.page-link[data-page]').forEach(link => {
            link.addEventListener('click', function (e) {
                e.preventDefault();
                currentPage = parseInt(this.getAttribute('data-page'));
                applyFilters();
            });
        });

        document.querySelector('[data-list-info]').textContent = `Showing ${((currentPage - 1) * pageSize + 1)} to ${Math.min(currentPage * pageSize, totalRecords)} of ${totalRecords} entries`;
    }

    function initializeFilters() {
        const departmentFilter = document.getElementById('departmentFilter');
        const possibleReasonFilter = document.getElementById('possibleReasonFilter');
        const sortFilter = document.getElementById('sortFilter');
        const pageSizeSelect = document.getElementById('pageSizeSelect');

       

        const searchInput = document.getElementById('searchInput');
        const dateRangePicker = document.getElementById('dateRangePicker');

        departmentFilter.addEventListener('change', applyFilters);
        possibleReasonFilter.addEventListener('change', applyFilters);
        sortFilter.addEventListener('change', applyFilters);
        pageSizeSelect.addEventListener('change', function () {
            pageSize = parseInt(this.value);
            currentPage = 1;
            applyFilters();
        });
        searchInput.addEventListener('input', debounce(applyFilters, 300));
        dateRangePicker.addEventListener('change', applyFilters);
    }

    function debounce(func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }

    function applyFilters() {
        
        const departmentValue = choiceManager.getChoiceValue('departmentFilter');
        const possibleReasonValue = choiceManager.getChoiceValue('possibleReasonFilter');
        const sortValue = choiceManager.getChoiceValue('sortFilter');
       

        //const departmentValue = document.getElementById('departmentFilter').value;
        //const possibleReasonValue = document.getElementById('possibleReasonFilter').value;
        //const sortValue = document.getElementById('sortFilter').value;

        const searchValue = document.getElementById('searchInput').value.trim();
        const dateRangeValue = document.getElementById('dateRangePicker').value;

        const filters = {
            department: departmentValue,
            possibleReason: possibleReasonValue,
            sort: sortValue,
            search: searchValue,
            dateRange: dateRangeValue
        };

        GetLoadData(currentPage, pageSize, filters);
    }

    //#endregion

    //#region Handle form submission for saving attendance data

    $('#apply_leave form').on('submit', function (e) {
        e.preventDefault();
        const formData = {
            employeeName: $(this).find('input[name="employeeName"]').val(),
            attendanceDate: $(this).find('input[name="attendanceDate"]').val(),
            actualInTime: $(this).find('input[name="actualInTime"]').val(),
            actualOutTime: $(this).find('input[name="actualOutTime"]').val(),
            breakInTime: $(this).find('input[name="breakInTime"]').val(),
            breakOutTime: $(this).find('input[name="breakOutTime"]').val()
        };

        $.ajax({
            url: "/ManualAttendence/SaveAttendance",
            method: "POST",
            data: formData,
            success: function (response) {
                
                toastr.success("Attendance saved successfully");
                $('#apply_leave').modal('hide');
                GetLoadData(currentPage, pageSize);
            },
            error: function (xhr) {
                console.error("Failed to save attendance:", xhr.responseText);
                toastr.error("Failed to save attendance");
            }
        });
    });

    // Handle form submission for horizontal timeline modal
    $('#edit_leaves_horizontal form').on('submit', function (e) {
        e.preventDefault();
        const employeeId = $(this).find('input[name="empId"]').val();
        const attendanceDate = $(this).find('input[name="attendanceDate"]').val();
        savePunchData(employeeId, attendanceDate, punchData);
        $('#edit_leaves_horizontal').modal('hide');
        GetLoadData(currentPage, pageSize);
    });

    // Handle form submission for vertical timeline modal
    $('#edit_leaves_vertical form').on('submit', function (e) {
        e.preventDefault();
        const employeeId = $(this).find('input[name="empId"]').val();
        const attendanceDate = $(this).find('input[name="attendanceDate"]').val();
        savePunchData(employeeId, attendanceDate, punchData);
        $('#edit_leaves_vertical').modal('hide');
        GetLoadData(currentPage, pageSize);
    });

    //#endregion

    GetLoadData();
    initializeFilters();
});