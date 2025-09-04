(function ($) {
    $.employeeshiftview = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
        }, options);

        var getAll = settings.baseUrl + "/GetAll";
        $(() => {


            // #region Dropdowns
            function initOrganizationDD() {
                organizationDD = new Choices('#OrganizationID', {
                    removeItemButton: true,
                    shouldSort: false,
                    placeholderValue: 'Select Organization...'
                });
            }
            document.addEventListener('DOMContentLoaded', initOrganizationDD);
            initOrganizationDD();
            // #endregion

        });
        

        // #region Table With Pagination
        var currentPage = 1;
        var pageSize = 5;
        let currentSortColumn = 'EmployeeID';
        let currentSortOrder = 'desc';

        $('#employeeShiftView-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();
            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });

        $(document).ready(function () {
            loadTableData();

            $("#employeeShiftView-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#employeeShiftView-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#employeeShiftView-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });

        $('#timeFrame').on('change', function () {
            const days = parseInt($(this).val(), 10);
            loadTableData(currentSortColumn, currentSortOrder, days);
        });


        let currentStartDate = new Date(); // defaults to today
        function getDaysToShow() {
            return parseInt($("#timeFrame").val(), 10);
        }


        function generateDateHeaders(daysCount = 7, startDate = new Date()) {
            const headers = [];
            for (let i = 0; i < daysCount; i++) {
                const date = new Date(startDate);
                date.setDate(startDate.getDate() + i);
                headers.push({
                    day: date.toLocaleDateString(undefined, { weekday: 'short' }),
                    date: date.toISOString().split('T')[0]
                });
            }
            return headers;
        }


        function renderEmployeeTable(employees, holidays, leaves) {
            const daysToShow = getDaysToShow();
            const headers = generateDateHeaders(daysToShow, currentStartDate); // Dates to display

            // 🔷 1. Render header row
            let headerHtml = `<th class="sort align-middle text-center text-uppercase text-nowrap">Employee Name</th>`;
            headers.forEach(h => {
                headerHtml += `<th class="align-middle text-center text-uppercase text-nowrap">${h.day}<br>${h.date}</th>`;
            });
            $('table.leads-table thead tr').html(headerHtml);

            // 🔷 2. Create global holiday map: key = orgID_branchID_date
            const holidayMap = {};
            holidays.forEach(h => {
                const startDateStr = h.startDate.split("T")[0]; // "2025-09-05"
                const endDateStr = h.endDate.split("T")[0];     // "2025-09-06"

                const start = new Date(Date.UTC(
                    parseInt(startDateStr.slice(0, 4)),         // year
                    parseInt(startDateStr.slice(5, 7)) - 1,     // month (0-based)
                    parseInt(startDateStr.slice(8, 10))         // day
                ));

                const end = new Date(Date.UTC(
                    parseInt(endDateStr.slice(0, 4)),
                    parseInt(endDateStr.slice(5, 7)) - 1,
                    parseInt(endDateStr.slice(8, 10))
                ));

                const title = h.holidayTitle || "Holiday";

                for (let d = new Date(start); d <= end; d.setUTCDate(d.getUTCDate() + 1)) {
                    const isoDate = d.toISOString().split("T")[0];
                    const key = `${h.organizationID || ''}_${h.organizationBranchID || ''}_${isoDate}`;
                    holidayMap[key] = title;
                }
            });

            // 🔷 2b. Create global leave map: key = employeeID_date
            const leaveMap = {};
            leaves.forEach(leave => {
                const empId = leave.employeeID;

                const fromDate = new Date(leave.fromDate.split("T")[0]);
                const toDate = new Date(leave.toDate.split("T")[0]);

                for (let d = new Date(fromDate); d <= toDate; d.setDate(d.getDate() + 1)) {
                    const isoDate = d.toISOString().split("T")[0];
                    const key = `${empId}_${isoDate}`;
                    leaveMap[key] = {
                        leaveType: leave.leaveTypeName || 'Leave',
                        isFullDay: leave.isFullDay,
                        partialFromTime: leave.partialFromTime,
                        partialToTime: leave.partialToTime
                    };
                }
            });

            // 🔷 3. Render body rows
            let bodyHtml = '';
            employees.forEach(emp => {
                bodyHtml += '<tr>';

                // Employee column
                bodyHtml += `
                <td class="align-middle text-center white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2 sticky-col bg-white z-index-sticky">
                    <div class="d-inline-flex flex-column align-items-center justify-content-center">
                        <h5>${emp.employeeName}</h5>
                        <p class="fs-9 mb-0">${emp.departmentName}</p>
                        <p class="fs-9 mb-0">${emp.organizationName}</p>
                    </div>
                </td>`;

                // Assigned shifts, weekends, holidays, leaves
                const shiftMap = parseShiftSchedule(emp.assignedDates);
                const weekendDays = parseWeekdayNumbers(emp.weekdayNumbers || '');

                headers.forEach(h => {
                    const dateStr = h.date;
                    const dateObj = new Date(dateStr);
                    const weekdayNumber = dateObj.getDay(); // Sunday = 0, Saturday = 6
                    const isWeekend = weekendDays.includes(weekdayNumber);
                    const shift = shiftMap[dateStr];

                    // Get holiday title using orgID + branchID + date
                    const holidayKey = `${emp.organizationID || ''}_${emp.organizationBranchID || ''}_${dateStr}`;
                    const holidayTitle = holidayMap[holidayKey];

                    const leaveKey = `${emp.employeeID}_${dateStr}`;
                    const leaveData = leaveMap[leaveKey];

                    if (holidayTitle) {
                        // 🟥 Holiday cell
                        bodyHtml += `
                        <td class="align-middle text-center">
                            <div class="shift-cell position-relative badge badge-phoenix-danger holiday-block px-4 py-2" style="border-left:5px solid #FC0808;">
                                <p class="fs-10 mb-0 p-1 bg-light text-info">${holidayTitle}</p>
                            </div>
                        </td>`;
                    } else if (leaveData) {
                        // 🟦 Leave cell
                        const timeRange = !leaveData.isFullDay && leaveData.partialFromTime && leaveData.partialToTime
                            ? `${leaveData.partialFromTime} - ${leaveData.partialToTime}`
                            : '';

                        bodyHtml += `
                        <td class="align-middle text-center">
                            <div class="shift-cell position-relative badge badge-phoenix-warning holiday-block px-4 py-2" style="border-left:5px solid #FFC107;">
                                <p class="fs-10 mb-0 p-1 bg-light text-info">${leaveData.leaveType}</p>
                                ${timeRange ? `<p class="fs-10 mb-0 text-muted">${timeRange}</p>` : ''}
                            </div>
                        </td>`;
                    } else if (shift) {
                        // 🟩 Assigned Shift cell
                        const badgeClass = isWeekend ? 'badge-phoenix-danger' : 'badge-phoenix-primary';
                        const leftColor = isWeekend ? '#FF6F6F' : '#A1F1A1';
                        bodyHtml += `
                        <td class="align-middle text-center">
                            <div class="shift-cell position-relative badge ${badgeClass} shift-block px-4 py-2" style="border-left:5px solid ${leftColor};">
                                <p class="fs-10 mb-1">${shift.timeRange}</p>
                                <p class="fs-10 mb-1">${shift.shiftName}</p>
                            </div>
                        </td>`;
                    } else if (isWeekend) {
                        // 🟧 Weekend cell
                        bodyHtml += `
                        <td class="align-middle text-center">
                            <div class="shift-cell position-relative badge badge-phoenix-danger shift-block px-4 py-2" style="border-left:5px solid #FF6F6F;">
                                <p class="fs-10 mb-1">Weekend</p>
                            </div>
                        </td>`;
                    } else {
                        // 🟨 Default shift (fallback)
                        bodyHtml += `
                        <td class="align-middle text-center">
                            <div class="shift-cell position-relative badge badge-phoenix-primary shift-block px-4 py-2" style="border-left:5px solid #A1F1A1;">
                                <p class="fs-10 mb-1">${emp.shiftName || ''}</p>
                                <p class="fs-10 mb-1">${emp.startTime || ''} - ${emp.endTime || ''}</p>
                            </div>
                        </td>`;
                    }
                });

                bodyHtml += '</tr>';
            });

            $('table.leads-table tbody').html(bodyHtml);

            // 🔷 4. Update date range label
            const firstDate = headers[0].date;
            const lastDate = headers[headers.length - 1].date;
            $(".date-range-label").text(`${firstDate} - ${lastDate}`);
        }


        function parseShiftSchedule(assignedDatesStr) {
            const shiftMap = {};
            if (!assignedDatesStr) return shiftMap;

            assignedDatesStr.split(',').forEach(entry => {
                const [date, shiftName, timeRange] = entry.trim().split('|');
                if (date && shiftName && timeRange) {
                    shiftMap[date] = { shiftName, timeRange };
                }
            });
            return shiftMap;
        }


        function parseWeekdayNumbers(weekdayNumberStr) {
            if (!weekdayNumberStr) return [];
            return weekdayNumberStr.split(',').map(n => parseInt(n.trim(), 10)).filter(n => !isNaN(n));
        }


        function loadTableData(daysToShow = getDaysToShow(), startDate = currentStartDate) {
            var searchTerm = $("#employeeShiftView-searchInput").val();
            $.ajax({
                //url: settings.baseUrl + '/GetEmployeesPaged',
                url: getAll,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    daysToShow: daysToShow,
                    startDate: startDate
                },
                success: function (response) {
                    const employees = response.items;  
                    const holidays = response.holidays;
                    const leaves = response.leaveApplications;
                    const totalCount = response.totalCount;
                    renderEmployeeTable(employees, holidays, leaves);

                    // Calculate pagination info
                    const pagination = response.separatePaginationInfo;

                    // ✅ Update pagination info display
                    $("#startItem").text(response.startItem);
                    $("#endItem").text(response.endItem);
                    $("#totalItems").text(response.totalItems);

                    // ✅ Update pagination buttons
                    updatePagination(response.pageNumbers, response.currentPage, response.totalPages);
                },
                error: function (xhr, status, error) {
                    console.error('Error loading employee data:', error);
                }
            });
        }



        $("#chevron-left").on("click", function () {
            const days = getDaysToShow();
            currentStartDate.setDate(currentStartDate.getDate() - days); // Go back one page
            loadTableData(days, currentStartDate);
        });

        $("#chevron-right").on("click", function () {
            const days = getDaysToShow();
            currentStartDate.setDate(currentStartDate.getDate() + days); // Go forward one page
            loadTableData(days, currentStartDate);
        });



        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#employeeShiftView-paginationLinks");
            paginationLinks.empty();
            const windowSize = 1;

            const createPageButton = (page) => `
            <li class="page-item ${page === currentPage ? 'active' : ''}">
                <button class="page-link page-btn" data-page="${page}">${page}</button>
            </li>`;

            const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';

            if (currentPage > windowSize + 1) {
                paginationLinks.append(createPageButton(1), addEllipsis());
            }

            const startPage = Math.max(1, currentPage - windowSize);
            const endPage = Math.min(totalPages, currentPage + windowSize);
            for (let i = startPage; i <= endPage; i++) {
                paginationLinks.append(createPageButton(i));
            }

            if (currentPage < totalPages - windowSize) {
                paginationLinks.append(addEllipsis(), createPageButton(totalPages));
            }

            $("#employeeShiftView-prevPageBtn").prop('disabled', currentPage === 1);
            $("#employeeShiftView-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        // 🔁 Page button click
        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
        // #endregion
    }
}(jQuery));
