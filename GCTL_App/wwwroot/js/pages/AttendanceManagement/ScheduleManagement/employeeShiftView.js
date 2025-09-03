(function ($) {
    $.employeeshiftview = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        $(() => {


            

        });

        // #region Table With Pagination
        var currentPage = 1;
        var pageSize = 5;
        let currentSortColumn = 'DefaultShiftID';
        let currentSortOrder = 'desc';

        $('#rosterInOfficeDays-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();
            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });

        $(document).ready(function () {
            loadTableData();

            $("#rosterInOfficeDays-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#rosterInOfficeDays-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#rosterInOfficeDays-nextPageBtn").on('click', function () {
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


        function renderEmployeeTable(employees) {
            const daysToShow = getDaysToShow();
            const headers = generateDateHeaders(daysToShow, currentStartDate);  // Number of days for columns
            //const paginationInfo = result.paginationInfo;

            // Render header
            let headerHtml = `<th class="sort align-middle text-center text-uppercase text-nowrap">Employee Name</th>`;
            headers.forEach(h => {
                headerHtml += `<th class="align-middle text-center text-uppercase text-nowrap">${h.day}<br>${h.date}</th>`;
            });
            $('table.leads-table thead tr').html(headerHtml);

            // Render rows
            let bodyHtml = '';
            employees.forEach(emp => {
                bodyHtml += '<tr>';

                // Employee info column
                bodyHtml += `
                <td class="align-middle text-center white-space-nowrap fw-semibold text-body-emphasis ps-2 py-2 sticky-col bg-white z-index-sticky">
                    <div class="d-inline-flex flex-column align-items-center justify-content-center">
                        <h5>${emp.employeeName}</h5>
                        <p class="fs-9 mb-0">${emp.departmentName}</p>
                        <p class="fs-9 mb-0">${emp.organizationName}</p>
                    </div>
                </td>`;

                // Parse and show shifts
                const shiftMap = parseShiftSchedule(emp.assignedDates);
                const weekendDays = parseWeekdayNumbers(emp.weekdayNumber);
                const holidayMap = {};
                (emp.holidayDates || "").split(',').forEach((date, i) => {
                    const title = (emp.holidayTitle || "").split(',')[i] || "Holiday";
                    holidayMap[date.trim()] = title.trim();
                });
                const leaveMap = {};
                (emp.leaveDates || "").split(',').forEach((date, i) => {
                    const title = (emp.leaveTypeName || "").split(',')[i] || "Leave";
                    leaveMap[date.trim()] = title.trim();
                });
                headers.forEach(h => {
                    const dateObj = new Date(h.date);
                    const weekdayNumber = dateObj.getDay(); // Sunday = 0, Saturday = 6
                    const shift = shiftMap[h.date];
                    const holidayTitle = holidayMap[h.date];
                    const leaveTypeName = leaveMap[h.date];
                    const isWeekend = weekendDays.includes(weekdayNumber);
                    if (holidayTitle) {
                        bodyHtml += `
                        <td class="holiday-cell align-middle text-center">
                            <div class="position-relative badge badge-phoenix-danger holiday-block px-4 py-2" style="border-left:5px solid #FC0808;">
                                <p class="fs-10 mb-0 p-1 bg-light text-info">${holidayTitle}</p>
                                <a href="#" class="btn btn-info btn-sm px-2 py-1 nav-item mx-2 edit-shift-btn" data-bs-toggle="modal"
                                    id="rosterInOfficeDays-editBtn"
                                    data-id="${emp.rosterInOfficeDayID || ''}" 
                                    data-date="${h.date}" 
                                    data-shift-id="${emp.shiftID || ''}"
                                    data-organization-id="${emp.organizationID}" 
                                    data-dep-id="${emp.departmentID}" 
                                    data-emp-id="${emp.employeeID}" 
                                    data-bs-target="#rosterInOfficeDays-editShiftModal">
                                    <i class="fas fa-pen"></i>
                                </a>
                            </div>
                        </td>`;
                    } else if (leaveTypeName) {
                        bodyHtml += `
                        <td class="holiday-cell align-middle text-center">
                            <div class="position-relative badge badge-phoenix-danger holiday-block px-4 py-2" style="border-left:5px solid #FC0808;">
                                <p class="fs-10 mb-0 p-1 bg-light text-info">${leaveTypeName}</p>
                                <a href="#" class="btn btn-info btn-sm px-2 py-1 nav-item mx-2 edit-shift-btn" data-bs-toggle="modal"
                                    id="rosterInOfficeDays-editBtn"
                                    data-id="${emp.rosterInOfficeDayID || ''}" 
                                    data-date="${h.date}" 
                                    data-shift-id="${emp.shiftID || ''}"
                                    data-organization-id="${emp.organizationID}" 
                                    data-dep-id="${emp.departmentID}" 
                                    data-emp-id="${emp.employeeID}" 
                                    data-bs-target="#rosterInOfficeDays-editShiftModal">
                                    <i class="fas fa-pen"></i>
                                </a>
                            </div>
                        </td>`;
                    } else if (shift) {
                        const badgeClass = isWeekend ? 'badge-phoenix-danger' : 'badge-phoenix-primary';
                        const leftColor = isWeekend ? '#FF6F6F' : '#A1F1A1';
                        bodyHtml += `
                        <td class="startTime align-middle text-center">
                            <div class="position-relative badge ${badgeClass} shift-block px-4 py-2" style="border-left:5px solid ${leftColor};">
                                <p class="fs-10 mb-1">${shift.timeRange}</p>
                                <p class="fs-10 mb-1">${shift.shiftName}</p>
                                <a href="#" class="btn btn-info btn-sm px-2 py-1 nav-item mx-2 edit-shift-btn" data-bs-toggle="modal" id="rosterInOfficeDays-editBtn"
                                    data-id="${emp.rosterInOfficeDayID}" 
                                    data-date="${h.date}" 
                                    data-shift-id="${emp.shiftID}"
                                    data-organization-id="${emp.organizationID}" 
                                    data-dep-id="${emp.departmentID}" 
                                    data-emp-id="${emp.employeeID}" 
                                    data-bs-target="#rosterInOfficeDays-editShiftModal">
                                    <i class="fas fa-pen"></i>
                                </a>
                            </div>
                        </td>`;
                    } else {
                        bodyHtml += `
                        <td class="shift-cell align-middle text-center">
                            <a href="#" class="btn btn-outline-success add-shift-btn" data-bs-toggle="modal" data-bs-target="#addShiftModal"
                                data-id="${emp.rosterInOfficeDayID}" 
                                data-date="${h.date}" 
                                data-organization-id="${emp.organizationID}" 
                                data-dep-id="${emp.departmentID}" 
                                data-emp-id="${emp.employeeID}" >
                                <i class="fa fa-plus"></i>
                            </a>
                        </td>`;
                    }
                });

                bodyHtml += '</tr>';
            });

            $('table.leads-table tbody').html(bodyHtml);

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
            var searchTerm = $("#rosterInOfficeDays-searchInput").val();
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
                    const employees = response.data;  // Your controller returns JSON { data = ..., totalCount = ... }
                    const totalCount = response.totalCount;
                    renderEmployeeTable(employees);

                    // Calculate pagination info
                    const pagination = response.separatePaginationInfo;

                    // ✅ Update pagination info display
                    $("#startItem").text(pagination.startItem);
                    $("#endItem").text(pagination.endItem);
                    $("#totalItems").text(pagination.totalItems);

                    // ✅ Update pagination buttons
                    updatePagination(pagination.pageNumbers, pagination.currentPage, pagination.totalPages);
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
            const paginationLinks = $("#rosterInOfficeDays-paginationLinks");
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

            $("#rosterInOfficeDays-prevPageBtn").prop('disabled', currentPage === 1);
            $("#rosterInOfficeDays-nextPageBtn").prop('disabled', currentPage === totalPages);
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
