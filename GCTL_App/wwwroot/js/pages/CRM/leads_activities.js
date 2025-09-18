$('document').ready(function () {
    let ids = {
        leadID: "#leadID",
        leadName: "#leadName",
        leadStatusID: '#leadStatusID',
        leadSourceID: '#leadSourceID',
        leadPriorityID: '#leadPriorityID',
        approximateDealValue: '#approximateDealValue',
        probabilityPercentage: '#probabilityPercentage',
        completionValue: 'completionValue',
        descriptionText: 'descriptionText',
        queryText: '#queryText',
        selectedID: '#selectedID',
        leadOwnerId: '#leadOwnerId',
        itemPerPage: '#pageElementSize',
    }

    let typingTimer;
    let delay = 300;
    let currentIndex = 1;
    // Use input for #dataSearch (text field) 
    $("#dataSearch").on("input", function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(function () {
            loadProcessedTable();
        }, delay);
    });

    $("#pageElementSize, #dateRange2, #customerType").on("change", function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(async function () {
            loadProcessedTable();
        }, delay);
    })


    function updatePaginationApprove(totalCount, page, size) {
        debugger;
        const totalPages = Math.ceil(totalCount / size);
        const pagination = $('#pageNumber');
        pagination.empty();

        const maxVisible = 5;
        let startPage = Math.max(1, page - Math.floor(maxVisible / 2));
        let endPage = Math.min(totalPages, startPage + maxVisible - 1);

        startPage = Math.max(1, endPage - maxVisible + 1);
        const prevDisabled = page === 1 ? 'disabled' : '';
        pagination.append(`<li class="page-item ${prevDisabled}"><button class="page-link" data-page="${page - 1}">&laquo;</button></li>`);

        for (let i = startPage; i <= endPage; i++) {
            const activeClass = i === page ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><button class="page-link" data-page="${i}">${i}</button></li>`);
        }

        const nextDisabled = page === totalPages ? 'disabled' : '';
        pagination.append(`<li class="page-item ${nextDisabled}"><button class="page-link" data-page="${page + 1}">&raquo;</button></li>`);

        $('#totalApprove').text(`Showing ${(page - 1) * size + 1} to ${Math.min(page * size, totalCount)} of ${totalCount} entries`);
    }

    $('#pageNumber').off('click', '.page-link').on('click', '.page-link', function (e) {
        e.preventDefault();
        const selectedPage = parseInt($(this).data('page'));
        const totalPages = Math.ceil($('#resignProcessed').data('total') / $('#pageElementSize').val());
        if (selectedPage >= 1 && selectedPage <= totalPages) {
            $('#pageNumber').data('page', selectedPage);
            loadProcessedTable();
        }
    });

    // ====================
    // generate color
    // ====================


    const statusColors = {
        "contacted": "badge-phoenix-info",
        "new": "badge-phoenix-primary",
        "not contacted": "badge-phoenix-warning",
        "nurturing": "badge-phoenix-secondary",
        "qualified": "badge-phoenix-success",
        "unqualified": "badge-phoenix-danger"
    };

    function getStatusBadgeClass(status) {
        return statusColors[status.trim().toLowerCase()] || "badge-secondary";
    }
    function loadProcessedTable() {
        debugger;
        var page = $('#pageNumber').data('page');
        //var size = $('#resignProcessed').data('size');
        var size = $('#pageElementSize').val();
        var search = $('#dataSearch').val();
        var sort = $('#resignProcessed').data('sort');
        var dir = $('#resignProcessed').data('dir');
        var dateRange = $('#dateRange2').val();

        $.ajax({
            url: '/LeadsActivities/GetUpcomingActivities',
            type: 'POST',
            data: {
                dateRange: dateRange,
                pageNumber: page,
                itemPerPage: size,
                search: search,
                sortColumn: sort,
                sortDirection: dir
            },
            success: function (response) {
                
                showDev(response)
                debugger;
                var tbody = $('#processed-resignation-body');
                tbody.empty();
                let itemsPerPage = parseInt($('#pageElementSize').val()) || 10;

                let page = parseInt($('#pageNumber').data('page')) || 1;


                let pageOffset = (page - 1) * itemsPerPage;

                $.each(response.data, function (index, item) {
                    const dt = new Date(item.activityDateTime);

                    // Format Date (dd-mm-yyyy)
                    const datePart = dt.toLocaleDateString('en-GB').replace(/\//g, '-');

                    // Format Time (hh:mm AM/PM)
                    const time = dt.toLocaleTimeString('en-US', { hour: 'numeric', minute: '2-digit' });
                    let itemSL = pageOffset + index + 1;
                    //let statusBadge = getStatusBadgeClass(item.status);
                    tbody.append(`
                    <tr class="hover-actions-trigger btn-reveal-trigger position-static">

                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="0">${itemSL}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.customerName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.leadName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.leadActivityType}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${datePart} ${time}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.activityNote}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.activityNote}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.leadOwner}</td>
           
                        <td class="status align-middle white-space-nowrap pe-0 ps-2 d-flex justify-content-center" data-column="11">
                            <a href="/LeadDetails/Index/${item.leadID}" class="btn btn-outline-secondary btn-icon addShift-bulkEdit me-2"  id="editModalBtn" data-id="${item.leadId}"><i class="fa-solid fa-arrow-right"></i></a>
                        </td>
                    </tr>
                `);
                });

                //DynamicTable.applyColumnVisibilityToNewRows(document.getElementById('resignProcessed'), 'resignProcessed');

                DynamicTableDrag.refreshTableSettings('LeadsActivities');
                updatePaginationApprove(response.totalSearchItem, page, size);


                $('#LeadsActivities').data('total', response.totalSearchItem);

            },
            error: function () {
                console.error('Error loading processed resignations');
            }
        });
    }
    loadProcessedTable();


    // ====================
    // Edit Button work
    // ======================
    $(document).on("click", "#editModalBtn", function (e) {
        var myModal = new bootstrap.Modal(document.getElementById('editModal'), {
            keyboard: false
        });
        myModal.show();

        let leadID = $(this).data('id');
        $.ajax({
            url: '/CRM/GetLeadInfo',
            method: 'POST',
            data: { id: leadID },
            success: function (response) {
                $("#leadID").val(response.leadID);
                $("#leadName").val(response.leadName);
                $("#leadStatusID").val(response.leadStatusID);
                $("#leadSourceID").val(response.leadSourceID);
                $("#leadPriorityID").val(response.priorityID);
                $("#approximateDealValue").val(response.approximateDealValue);
                $("#probabilityPercentage").val(response.probability);
                $("#completionValue").text(response.probability + '%');
                $("#descriptionText").val(response.leadDescription);
                $("#queryText").val(response.leadOwnerName);
                // $("#selectedID").val(response.leadOwnerId);

                choiceManager.setChoiceValue('leadOwnerId', response.leadOwnerId)
                // multiselect edit field read
                $('#serviceTypes').val(response.serviceIds).each(function () {
                    coreui.MultiSelect.getInstance(this)?.update();
                });



                // employee add
                const currentOwnerId = response.leadOwnerId;
                const currentOwnerName = response.leadOwnerName;

                if (currentOwnerId && currentOwnerName) {
                    choices.setChoices(
                        [{ value: currentOwnerId, label: currentOwnerName, selected: true }],
                        'value',
                        'label',
                        false // false = append (don’t clear)
                    );
                }
            },
            error: function (xhr) {
                toastr.error("Error creating lead");
            }
        });
    });


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