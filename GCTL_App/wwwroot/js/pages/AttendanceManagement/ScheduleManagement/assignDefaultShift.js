(function ($) {
    $.assigndefaultshift = function (options) {
        // Default options
        var settings = $.extend({
            baseUrl: '/',
            addform: '#assignDefaultShift-addForm',
            updateform: '#assignDefaultShift-addForm',
            saveBtn: '#assignDefaultShift-saveBtn',
            resetBtn: '#assignDefaultShift-resetBtn',
        }, options);

        var gridUrl = settings.baseUrl + "/GetAll";
        var createUrl = settings.baseUrl + "/Create";
        var updateUrl = settings.baseUrl + "/Update";
        $(() => {

            $(settings.saveBtn).on('click', function (e) {
                e.preventDefault();

                var token = $('#assignDefaultShift-addForm input[name="__RequestVerificationToken"]').val();

                var formData = {
                    __RequestVerificationToken: token,
                    OrganizationIDs: $('#OrganizationIDs').val(),
                    DepartmentIDs: $('#DepartmentIDs').val(),
                    EmployeeIDs: $('#EmployeeIDs').val(),
                    ShiftID: $('#ShiftID').val()
                }

                $.ajax({
                    url: createUrl,
                    type: 'POST',
                    data: formData,
                    success: function (data) {
                        if (data.isSuccess) {
                            clear();
                            toastr.success(data.message);
                        } else {
                            toastr.info(data.message);
                        }
                    },
                    error: function (err) {
                        console.log(err);
                    }
                })
            });



            $(settings.resetBtn).on('click', function () {
                clear();
            });

            function clear() {
                $(settings.addform)[0].reset();
                $('#DefaultShiftID').val('0');
                $('.text-danger').hide();
                $('.form-control').removeClass('is-invalid');
                $('.form-control').each(function () {
                    if ($(this).css('border-color') === 'rgb(255, 0, 0)') {
                        $(this).css('border-color', '#ccc');
                    }
                });
                $(settings.addform).find(settings.saveBtn).text('Save');
                $("#assignDefaultShift-check-all").prop('checked', false);
                $('.assignDefaultShift-selectItem').prop('checked', false);
                loadTableData();
                toggleBulkActions();
            }



            document.addEventListener('changed.coreui.multi-select', function (event) {
                const target = event.target;

                if (target.id === 'OrganizationIDs' || target.id === 'DepartmentIDs') {
                    loadFilteredEmployees();
                }
            });

            function loadFilteredEmployees() {
                var orgIds = $('#OrganizationIDs').val() || [];
                var deptIds = $('#DepartmentIDs').val() || [];

                if (!Array.isArray(orgIds)) orgIds = [orgIds];
                if (!Array.isArray(deptIds)) deptIds = [deptIds];

                $.ajax({
                    url: '/AssignDefaultShift/GetEmployeesByFilters',
                    type: 'GET',
                    data: {
                        organizationIds: orgIds.join(','),
                        departmentIds: deptIds.join(',')
                    },
                    success: function (data) {
                        updateEmployeeDropdown(data);
                    },
                    error: function (xhr, status, error) {
                        console.error('Error loading employees:', error);
                    }
                });
            }

            function updateEmployeeDropdown(data) {
                var $empSelect = $('#EmployeeIDs');
                $empSelect.empty();

                if (!Array.isArray(data) || data.length === 0) {
                    $empSelect.append('<option disabled>No employees found</option>');
                    refreshCoreUIMultiSelect();
                    return;
                }

                // Group employees by department name
                const grouped = {};
                data.forEach(emp => {
                    const dept = emp.departmentName || 'No Department';
                    if (!grouped[dept]) grouped[dept] = [];
                    grouped[dept].push(emp);
                });

                // Append optgroups and options
                Object.entries(grouped).forEach(([dept, employees]) => {
                    const $optgroup = $('<optgroup>').attr('label', dept);
                    employees.forEach(emp => {
                        $('<option>')
                            .val(emp.employeeID)
                            .text(emp.employeeName)
                            .appendTo($optgroup);
                    });
                    $empSelect.append($optgroup);
                });
                refreshCoreUIMultiSelect();
            }


            function refreshCoreUIMultiSelect() {
                const empSelect = document.getElementById('EmployeeIDs');

                // Dispose existing CoreUI MultiSelect instance
                const existingInstance = coreui.MultiSelect.getInstance(empSelect);
                if (existingInstance) {
                    existingInstance.dispose();
                }

                // Remove previously generated UI dropdown manually
                const generatedDropdown = empSelect.nextElementSibling;
                if (generatedDropdown && generatedDropdown.classList.contains('form-multi-select')) {
                    generatedDropdown.remove();
                }

                // Reinitialize CoreUI MultiSelect
                coreui.MultiSelect.getOrCreateInstance(empSelect);
            }



            $(document).ready(function () {
                $('#assignDefaultShift-check-all').on('change', function () {
                    var isChecked = $(this).prop('checked');
                    $('.assignDefaultShift-selectItem').prop('checked', isChecked);

                    toggleBulkActions();
                });

                $(document).on('change', '.assignDefaultShift-selectItem', function () {
                    toggleBulkActions();
                });
            });



            function toggleBulkActions() {
                const allItems = $('.assignDefaultShift-selectItem');
                const checkedItems = $('.assignDefaultShift-selectItem:checked');

                const allChecked = allItems.length === checkedItems.length;
                const someChecked = checkedItems.length > 0 && !allChecked;

                $('#assignDefaultShift-check-all').prop('checked', allChecked);
                $('#assignDefaultShift-check-all').prop('indeterminate', someChecked);

                if (checkedItems.length > 1) {
                    $('#assignDefaultShift-bulkSelectActions').removeClass('d-none');
                    $('#assignDefaultShift-searchBox').addClass('d-none');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkDelete').addClass('disabled');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkEdit').addClass('disabled');
                } else {
                    $('#assignDefaultShift-bulkSelectActions').addClass('d-none');
                    $('#assignDefaultShift-searchBox').removeClass('d-none');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkDelete').removeClass('disabled');
                    $('#assignDefaultShift-tBody .assignDefaultShift-bulkEdit').removeClass('disabled');
                }
            }



        });


        var currentPage = 1;
        var pageSize = 5;

        $('#assignDefaultShift-pageSizeSelect').on('change', function () {
            var selectedSize = $(this).val();

            if (selectedSize) {
                pageSize = parseInt(selectedSize, 10);
                currentPage = 1;
                loadTableData();
            }
        });


        $(document).ready(function () {
            loadTableData();

            $("#assignDefaultShift-searchInput").on("input", function () {
                currentPage = 1;
                loadTableData();
            });

            $("#assignDefaultShift-prevPageBtn").on('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    loadTableData();
                }
            });

            $("#assignDefaultShift-nextPageBtn").on('click', function () {
                currentPage++;
                loadTableData();
            });
        });


        let currentSortColumn = 'DefaultShiftID';
        let currentSortOrder = 'desc';

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
            var searchTerm = $("#assignDefaultShift-searchInput").val();
            $.ajax({
                url: gridUrl,
                method: 'GET',
                data: {
                    pageNumber: currentPage,
                    pageSize: pageSize,
                    searchTerm: searchTerm,
                    sortColumn: sortColumn,
                    sortOrder: sortOrder,
                },
                success: function (response) {
                    var tableBody = $("#assignDefaultShift-tBody");
                    tableBody.empty();
                    var totalItems = response.paginationInfo.totalItems;

                    if (response.data.length > 0) {
                        response.data.forEach(function (item, index) {
                            tableBody.append(`
                                <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                                    <td class="text-center text-middle align-middle" style="width: 5%;">
                                        <input type="checkbox" class="form-check-input assignDefaultShift-selectItem" data-id="${item.defaultShiftID}" />
                                    </td>
                                    <td class="align-middle white-space-nowrap fw-semibold text-body-emphasis ps-0 py-2">
                                        <h5>${item.organizationName}</h5>
                                    </td>
                                    <td class="align-middle white-space-nowrap ps-0 fw-semibold text-body py-1">
                                        <span>${item.departmentName}</span>
                                    </td>
                                    <td class="white-space-nowrap align-middle ps-0">${item.employeeName ?? '-'}</td>
                                    <td class="white-space-nowrap align-middle ps-0">${item.shiftName ?? '-'}</td>
                                    <td class="white-space-nowrap align-middle ps-0">
                                        <div class="btn-reveal-trigger position-static">
                                            <a href="#!" class="nav-item mx-2 assignDefaultShift-bulkEdit" id="assignDefaultShift-editBtn" data-id="${item.defaultShiftID}"><i class="fas fa-edit text-black"></i></a>
                                            <a href="#!" class="nav-item mx-2 assignDefaultShift-bulkDelete" id="assignDefaultShift-singleDelBtn" data-id="${item.defaultShiftID}"><i class="far fa-trash-alt text-black"></i></a>
                                        </div>
                                    </td>
                                </tr>
                            `);
                        });
                    } else {
                        tableBody.append('<tr><td colspan="6" class="text-center">No data available</td></tr>');
                    }

                    var paginationInfo = response.paginationInfo;

                    $("#assignDefaultShift-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                    $("#assignDefaultShift-totalCount").text(`(${paginationInfo.totalItems})`);

                    updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
                }
            });
        }

        function updatePagination(pageNumbers, currentPage, totalPages) {
            const paginationLinks = $("#assignDefaultShift-paginationLinks");
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
            $("#assignDefaultShift-prevPageBtn").prop('disabled', currentPage === 1);
            $("#assignDefaultShift-nextPageBtn").prop('disabled', currentPage === totalPages);
        }

        $(document).on('click', '.page-btn', function () {
            const page = $(this).data('page');
            currentPage = page;
            loadTableData();
        });
    }
}(jQuery));