$(function () {
    let links = {
        generatePDF: '/LeadsActivities/GeneratePDF',
    }

    let ids = {
        leadID: "#leadID",
        leadName: "#leadName",
        leadStatusID: '#leadStatusID',
        leadSourceID: '#leadSourceID',
        ActivityTypeID: '#ActivityType',
        leadPriorityID: '#leadPriorityID',
        approximateDealValue: '#approximateDealValue',
        probabilityPercentage: '#probabilityPercentage',
        completionValue: 'completionValue',
        descriptionText: 'descriptionText',
        queryText: '#queryText',
        selectedID: '#selectedID',
        leadOwnerId: '#leadOwnerId',
        itemPerPage: '#pageElementSize',
        pdfDownloadBtn: '#downloadPDF',
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

    $("#pageElementSize, #dateRange2, #customerType, #LeadStatus, #ActivityType").on("change", function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(async function () {
            loadProcessedTable();
        }, delay);
    })


    function updatePaginationApprove(totalCount, page, size) {
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
        debugger;
        const selectedPage = parseInt($(this).data('page'));
        const totalPages = Math.ceil($('#LeadsActivities').data('total') / $('#pageElementSize').val());
        if (selectedPage >= 1 && selectedPage <= totalPages) {
            $('#pageNumber').data('page', selectedPage);
            loadProcessedTable();
        }
    });

    // ====================
    // generate color
    // ====================



    const statusColors = {
        "pdf": "badge-phoenix-info",
        "jpg": "badge-phoenix-primary",
        "xl": "badge-phoenix-warning",
        "nurturing": "badge-phoenix-secondary",
        "qualified": "badge-phoenix-success",
        "unqualified": "badge-phoenix-danger"
    };
    function getStatusBadgeClass(status) {
        return statusColors[status.trim().toLowerCase()] || "badge-phoenix-secondary";
    }

    // =========================
    // preview
    //============================
    function previewFile(fileUrl) {
        let ext = fileUrl.split('.').pop().toLowerCase();
        let container = document.getElementById("filePreviewContainer");
        container.innerHTML = ""; // reset

        if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) {
            // Image preview
            container.innerHTML = `<img src="${fileUrl}" class="img-fluid" alt="preview">`;
        }
        //else if (ext === "pdf") {
        //    container.innerHTML = `<iframe src="${fileUrl}"
        //                   style="width:100%;height:500px" frameborder="0"></iframe>`;
            //}
        if (ext === "pdf") {
            window.open(fileUrl, "_blank");
            return; 
        }

        else {
            // Not supported ? force download
            window.open(fileUrl, "_blank");
            return;
        }

        // Show modal
        let modal = new bootstrap.Modal(document.getElementById('filePreviewModal'));
        modal.show();
    }
    window.previewFile = previewFile;
    function loadProcessedTable() {
        debugger;
        var page = $('#pageNumber').data('page');
        var size = $('#pageElementSize').val();
        var search = $('#dataSearch').val();
        var sort = $('#resignProcessed').data('sort');
        var dir = $('#resignProcessed').data('dir');
        var dateRange = $('#dateRange2').val();
        var customerID = $('#customerType').val();
        var statusID = $('#LeadStatus').val();
        var ActivityType = $('#ActivityType').val();
        showDev(ActivityType);
        $.ajax({
            url: '/LeadsActivities/GetUpcomingActivities',
            type: 'POST',
            data: {
                DateRange: dateRange,
                PageNumber: page,
                ItemPerPage: size,
                Search: search,
                SortColumn: sort,
                SortDirection: dir,
                CustomerTypeID: customerID,
                LeadStatusID: statusID,
                ActivityTypeID: ActivityType,
            },
            success: function (response) {
                
                var tbody = $('#processed-resignation-body');
                tbody.empty();
                let itemsPerPage = parseInt($('#pageElementSize').val()) || 10;
                let page = parseInt($('#pageNumber').data('page')) || 1;
                


                let pageOffset = (page - 1) * itemsPerPage;

                $.each(response.data, function (index, item) {
                    let fileCell = "-"; // default
                    if (item.file && item.file.length > 0) {
                        const extension = item.file.split('.').pop();
                        let statusBadge = getStatusBadgeClass(extension);
                        fileCell = `<a href="javascript:void(0)" 
                       onclick="previewFile('${item.file}')" 
                       class="badge badge-phoenix ${statusBadge} fs-9">
                       ${extension}
                    </a>`;
                    }
                    
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
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">
                            ${item.activityNote.length > 25 ? item.activityNote.substring(0, 25) + "..." : item.activityNote}
                        </td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${fileCell}</td>
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

    $(ids.pdfDownloadBtn).on('click', function () {
        fetch(links.generatePDF, { method: "POST" })
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = "report.pdf";
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
            })
            .catch(() => {
                toastr.error("Error crating PDF");
            });
    });
});