$(document).ready(function () {

    const developmentMode = false; 

    if (developmentMode) {
        toastr.info("Welcome to the Promotion List!");
    }


    //#region Promotion Table Load

    let currentSortColumn = '';
    let currentSortDirection = 'asc';
    let currentPage = 1;
    loadPromotionList(1)

    // Auto-reload on filter change
    $('#searchInput').on('input', () => loadPromotionList(1));
    $('#departmentFilter, #promotionStatusFilter, #dateRangePicker, #pageSizeSelect').on('change', () => loadPromotionList(1));


    $(document).on('click', 'th.sort', function () {
        const clickedColumn = $(this).data('sort');

        if (currentSortColumn === clickedColumn) {
            currentSortDirection = currentSortDirection === 'asc' ? 'desc' : 'asc';
        } else {
            currentSortColumn = clickedColumn;
            currentSortDirection = 'asc';
        }

        loadPromotionList(1);
    });

    

    //#endregion

    
})



function loadPromotionList(page = 1) {
    debugger
    currentPage = page;

    const formData = new FormData();
    formData.append("searchTerm", $('#searchInput').val());
    formData.append("departmentId", $('#departmentFilter').val());
    formData.append("status", $('#promotionStatusFilter').val());
    formData.append("dateRange", $('#dateRangePicker').val());
    formData.append("pageSize", $('#pageSizeSelect').val());
    formData.append("pageNumber", page);
    formData.append("sortColumn", currentSortColumn);
    formData.append("sortDirection", currentSortDirection);

    $.ajax({
        url: '/PromotionList/GetPromotionList',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (res) {
            if (res.success) {
                populatePromotionTable(res.data.items);
                updatePromotionPagination(res.data.pagination);
                updatePromotionResultCount(res.data.pagination);
            } else {
                $('#promotion-body').html('<tr><td colspan="8">No data found.</td></tr>');
            }
        },
        error: function () {
            alert("Failed to load promotion list.");
        }
    });
}





function populatePromotionTable(items) {
    const tbody = $('#promotion-body');
    tbody.empty();

    items.forEach(item => {
        tbody.append(`
           

            <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            
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
                            <td class="currentPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <p class="fs-14 fw-medium d-flex align-items-center mb-0">${item.department}</p>
                            </td>
                             <td class="currentPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">
                                <p class="fs-14 fw-medium d-flex align-items-center mb-0">${item.currentDesignation}</p>
                            </td>


                            <td class="proposedPosition align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.newDesignation}</td>
                       
                            <td class="effectiveDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.effectiveDate}</td>
                            <td class="proposedSalary align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.salaryChange}</td>
                            <td class="effectiveDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-0">${item.status}</td>
                            
                        </tr>


        `);
    });
}

function updatePromotionPagination(pagination) {
    const paginationUl = $('.pagination');
    paginationUl.empty();

    for (let i = 1; i <= pagination.totalPages; i++) {
        paginationUl.append(`
            <li class="page-item ${i === pagination.currentPage ? 'active' : ''}">
                <a class="page-link" href="#" onclick="loadPromotionList(${i})">${i}</a>
            </li>
        `);
    }
}

function updatePromotionResultCount(pagination) {
    const infoText = `Showing ${pagination.startRecord} - ${pagination.endRecord} of ${pagination.totalRecords} results`;
    $('[data-list-info]').text(infoText);
}