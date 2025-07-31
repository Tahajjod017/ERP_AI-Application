$(document).ready(function () {

    const developmentMode = true; 

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

    function loadPromotionList(page = 1) {
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

    function populatePromotionTable(items) {
        const tbody = $('#promotion-body');
        tbody.empty();

        items.forEach(item => {
            tbody.append(`
            <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                
                <td class="align-middle text-nowrap">${item.employeeName}</td>
                <td class="align-middle text-nowrap">${item.department}</td>
                <td class="align-middle text-nowrap">${item.currentDesignation}</td>
                <td class="align-middle text-nowrap">${item.newDesignation}</td>
                <td class="align-middle text-nowrap">${item.effectiveDate}</td>
                <td class="align-middle text-nowrap">${item.salaryChange}</td>
                <td class="align-middle text-nowrap">${item.status}</td>
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

    //#endregion

    
})