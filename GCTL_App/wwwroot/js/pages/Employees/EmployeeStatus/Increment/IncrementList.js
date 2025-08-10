$(document).ready(function () {
   

    let currentPage = 1;
    let currentSortColumn = "effectiveDate"; // Default sort column
    let currentSortDirection = "desc";       // Default sort direction

    
    $('#searchInput, #departmentFilter, #incrementTypeFilter, #dateRangePicker, #pageSizeSelect').on('change keyup', function () {
        loadIncrementList(1);
    });


    loadIncrementList();

    //#region Table Load 
    function loadIncrementList(page = 1) {
        currentPage = page;

        const formData = new FormData();
        formData.append("searchTerm", $('#searchInput').val());
        formData.append("departmentId", $('#departmentFilter').val());
        formData.append("incrementType", $('#incrementTypeFilter').val());
        formData.append("dateRange", $('#dateRangePicker').val());
        formData.append("pageSize", $('#pageSizeSelect').val());
        formData.append("pageNumber", page);
        formData.append("sortColumn", currentSortColumn);
        formData.append("sortDirection", currentSortDirection);


        $.ajax({
            url: '/IncrementList/GetIncrementList', // Update controller route if needed
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
                if (res.success) {
                    populateTable(res.data.items);
                    updatePagination(res.data.pagination);
                    updateResultCount(res.data.pagination);
                } else {
                    $('#incrementList-body').html('<tr><td colspan="9">No data found.</td></tr>');
                }
            },
            error: function () {
                alert("Failed to load increment list.");
            }
        });
    }


    $(document).on('click', 'th.sort', function () {
        const clickedColumn = $(this).data('sort');

        if (currentSortColumn === clickedColumn) {
            // Toggle direction
            currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = clickedColumn;
            currentSortDirection = 'asc'; // Reset to ascending
        }

        loadIncrementList(1); // Reload data with new sort
    });


    function populateTable(items) {
        const tbody = $('#incrementList-body');
        tbody.empty();

        items.forEach(item => {
            tbody.append(`
                <tr class= "hover-actions-trigger btn-reveal-trigger position-static">
                    
                    <td class="employeeName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-4 py-1">
                        <div class="d-flex align-items-center file-name-icon">
                            <div class="avatar avatar-m avatar-bordered me-4">
                                <img class="rounded-circle" src="${item.avatarUrl}" alt="" />
                            </div>
                            <div class="ms-1">
                                <h6 class="fw-bold">${item.employeeName}</h6>
                                <span class="fs-12 fw-normal">${item.department}</span>
                            </div>
                        </div>
                    </td>

                    
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.department}</td>
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.currentSalary}</td>
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.incrementAmount}</td>
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.newSalary}</td>
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.effectiveDate}</td>
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.incrementType}</td>
                    <td class="align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.status}</td>
                </tr>
            `);
        });
    }

    function updatePagination(pagination) {
        const paginationUl = $('.pagination');
        paginationUl.empty();

        for (let i = 1; i <= pagination.totalPages; i++) {
            paginationUl.append(`
                <li class="page-item ${i === pagination.currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" onclick="loadIncrementList(${i})">${i}</a>
                </li>
            `);
        }
    }

    function updateResultCount(pagination) {
        const infoText = `Showing ${pagination.startRecord} - ${pagination.endRecord} of ${pagination.totalRecords} results`;
        $('[data-list-info]').text(infoText);
    }

    
   
    //#endregion


})