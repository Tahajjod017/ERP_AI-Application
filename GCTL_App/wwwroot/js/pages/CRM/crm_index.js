$('document').ready(function () {

    $("#dataSearch").on("input change", function () {
        loadProcessedTable();
    })
    $("#pageElementSize").on("input change", function () {
        loadProcessedTable();
    })
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
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1">${item.email}</td>
                        <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.phone}</td>
                        <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.contactName}</td>
                        <td class="processedDate align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.companyName}</td>
                        <td class="status align-middle white-space-nowrap pe-0 ps-2" data-column="6">
                            <span class="badge badge-phoenix ${statusBadge} fs-9">${item.status}</span>
                        </td>
                    </tr>
                `);
                });

                DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('resignProcessed'), 'resignProcessed');


                updatePaginationApprove(data.totalCount, page, size)

                $('#resignProcessed').data('total', data.totalCount);
            },
            error: function () {
                console.error('Error loading processed resignations');
            }
        });
    }
    loadProcessedTable();
});