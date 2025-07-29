$(document).ready(function () {
    toastr.info("Increment List Loaded");

    let currentPage = 1;

    
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

    function populateTable(items) {
        const tbody = $('#incrementList-body');
        tbody.empty();

        items.forEach(item => {
            tbody.append(`
                <tr>
                    <td><input class="form-check-input" type="checkbox" /></td>
                    <td>${item.employeeName}</td>
                    <td>${item.department}</td>
                    <td>${item.currentSalary}</td>
                    <td>${item.incrementAmount}</td>
                    <td>${item.newSalary}</td>
                    <td>${item.effectiveDate}</td>
                    <td>${item.incrementType}</td>
                    <td>${item.status}</td>
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