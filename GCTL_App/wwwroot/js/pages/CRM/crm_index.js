$('document').ready(function () {
    let typingTimer;
    let delay = 300;

    $("#dataSearch, #pageElementSize, #dateRange2, #customerType").on("change", function () {
        console.log("Hello");
        clearTimeout(typingTimer);
        typingTimer = setTimeout(async function () {
            loadProcessedTable();
        }, delay);
    })


    function updatePaginationApprove(totalCount, page, size) {
        const totalPages = Math.ceil(totalCount / size);
        const pagination = $('#pageNumber');
        pagination.empty();

        const maxVisible = 5; // max page buttons to show
        let startPage = Math.max(1, page - Math.floor(maxVisible / 2));
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);

        // adjust startPage if we're near the end
        startPage = Math.max(1, endPage - maxVisible + 1);

        // Previous button
        const prevDisabled = page === 1 ? 'disabled' : '';
        pagination.append(`<li class="page-item ${prevDisabled}"><button class="page-link" data-page="${page - 1}">&laquo;</button></li>`);

        // Page number buttons
        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === page ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><button class="page-link" data-page="${i}">${i}</button></li>`);
        }

        // Next button
        const nextDisabled = page === totalPages ? 'disabled' : '';
        pagination.append(`<li class="page-item ${nextDisabled}"><button class="page-link" data-page="${page + 1}">&raquo;</button></li>`);

        $('#totalApprove').text(`Showing ${(page - 1) * size + 1} to ${Math.min(page * size, totalCount)} of ${totalCount} entries`);
    }

    // Click handler
    $('#pageNumber').off('click', '.page-link').on('click', '.page-link', function (e) {
        e.preventDefault();
        const selectedPage = parseInt($(this).data('page'));
        const totalPages = Math.ceil($('#resignProcessed').data('total') / $('#pageElementSize').val());
        if (selectedPage >= 1 && selectedPage <= totalPages) {
            $('#pageNumber').data('page', selectedPage);
            loadProcessedTable();
        }
    });

    function loadProcessedTable() {
        let typingTimer;
        let delay = 200;
        console.log("outpu: " , search);
        var page = $('#pageNumber').data('page');
        //var size = $('#resignProcessed').data('size');
        var size = $('#pageElementSize').val();
        var search = $('#dataSearch').val();
        var sort = $('#resignProcessed').data('sort');
        var dir = $('#resignProcessed').data('dir');
        var dateRange = $('#dateRange2').val();
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
                        <td class="fs-9 align-middle py-1  py-2">
                            <div class="form-check mb-0 fs-8">
                                <input class="form-check-input" type="checkbox" />
                            </div>
                        </td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="0"><a class="fw-bold cursor-pointer" href="/LeadDetails/Index/${item.leadId}">${item.leadName}</a></td>
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
                        <td class="status align-middle white-space-nowrap pe-0 ps-2" data-column="10">
                            <button class="btn btn-sm btn-primary"><i class="fa-solid fa-pen-to-square"></i></button> <button class="btn btn-sm btn-danger"> <i class="fa-solid fa-trash"></i> </button>
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