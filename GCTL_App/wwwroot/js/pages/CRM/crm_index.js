$('document').ready(function () {

    $("#dataSearch").on("input change", function () {
        loadProcessedTable();
    })
    $("#pageElementSize").on("input change", function () {
        loadProcessedTable();
    })

    function updatePaginationApprove(totalCount, page, size) {
        debugger;
        var totalPages = Math.ceil(totalCount / size);
        var pagination = $('#pageNumber');
        pagination.empty();

        for (var i = 1; i <= totalPages; i++) {
            var activeClass = i === page ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><button class="page-link">${i}</button></li>`);
        }

        $('#totalApprove').text(`Showing ${(page - 1) * size + 1} to ${Math.min(page * size, totalCount)} of ${totalCount} entries`);
    }
    $('#pageNumber').on('click', '.page-link', function (e) {
        e.preventDefault();
        var selectedPage = parseInt($(this).text()); // get page number from button text
        $('#pageNumber').data('page', selectedPage); // store selected page
        loadProcessedTable(); // reload table with new page
    });

    function loadProcessedTable() {
        var page = $('#pageNumber').data('page');
        //var size = $('#resignProcessed').data('size');
        var size = $('#pageElementSize').val();
        var search = $('#dataSearch').val();
        var sort = $('#resignProcessed').data('sort');
        var dir = $('#resignProcessed').data('dir');
        var dateRange = $('#dateRange').val();
        var customerType = $('#customerType').val();
        console.log(`page: ${page}, size: ${size}, search: ${search}, sort: ${sort}, dir: ${sort}, dateRange: ${dateRange}, customerType: ${customerType}`)

        $.ajax({
            url: '/CRM/GetAllLead',
            type: 'GET',
            data: {
                dateRange: dateRange,
                customerType: customerType,
                pageNumber: page,
                pageSize: size,
                searchTerm: search,
                sortColumn: sort,
                sortDirection: dir
            },
            success: function (data) {
                debugger;
                showDev(data, 'Approve Table')
                console.log(data);
                var tbody = $('#processed-resignation-body');
                tbody.empty();
                $.each(data.result.leads, function (index, item) {
                    var statusBadge = item.status === 'Approved' ? 'badge-phoenix-success' : 'badge-phoenix-danger';
                    tbody.append(`
                    <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        <td class="fs-9 align-middle py-1">
                            <div class="form-check mb-0 fs-8">
                                <input class="form-check-input" type="checkbox" />
                            </div>
                        </td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="0">${item.leadName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1">${item.leadStatus}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.leadSourceName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.leadOwnerName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.approximateDealValue}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="5">${item.probabilityPercentage}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="6">${item.email}</td>
                        <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="7">${item.phone}</td>
                        <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="8">${item.contactName}</td>
                        <td class="status align-middle white-space-nowrap pe-0 ps-2" data-column="9">
                            <span class="badge badge-phoenix ${statusBadge} fs-9">${item.status}</span>
                        </td>
                    </tr>
                `);
                });

                //DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('resignProcessed'), 'resignProcessed');

                DynamicTableDrag.refreshTableSettings('resignProcessed');
                updatePaginationApprove(data.result.totalCount, data.result.pageNumber, data.result.pageSize)

                $('#resignProcessed').data('total', data.result.totalCount);
            },
            error: function () {
                console.error('Error loading processed resignations');
            }
        });
    }
    loadProcessedTable();


    $('.sort').on('click', function () {
        var tableId = $(this).closest('table').attr('id');
        var column = $(this).data('sort');
        var currentSort = $('#' + tableId).data('sort');
        var direction = (currentSort === column && $('#' + tableId).data('dir') === 'asc') ? 'desc' : 'asc';

        $('#' + tableId).data('sort', column).data('dir', direction).data('page', 1);
        if (tableId === 'resignPending') {
            loadPendingTable();
        } else {
            loadProcessedTable();
        }
    });


});