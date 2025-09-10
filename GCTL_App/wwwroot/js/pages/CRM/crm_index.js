$('document').ready(function () {
    let typingTimer;
    let delay = 300;

    $("#dataSearch, #pageElementSize, #dateRange2, #customerType").on("change", function () {
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

    // ====================
    // generate color
    // ====================


    const statusColors = {
        "contacted": "badge-phoenix-info",       // green
        "new": "badge-phoenix-primary",             // blue
        "not contacted": "badge-phoenix-warning", // gray
        "nurturing": "badge-phoenix-secondary",       // yellow
        "qualified": "badge-phoenix-success",          // light blue
        "unqualified": "badge-phoenix-danger"       // red
    };

    function getStatusBadgeClass(status) {
        return statusColors[status.trim().toLowerCase()] || "badge-secondary";
    }
    function loadProcessedTable() {
        let typingTimer;
        let delay = 200;
        var page = $('#pageNumber').data('page');
        //var size = $('#resignProcessed').data('size');
        var size = $('#pageElementSize').val();
        var search = $('#dataSearch').val();
        var sort = $('#resignProcessed').data('sort');
        var dir = $('#resignProcessed').data('dir');
        var dateRange = $('#dateRange2').val();
        var customerType = $('#customerType').val();


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
                var tbody = $('#processed-resignation-body');
                tbody.empty();
                $.each(data.result.leads, function (index, item) {
                    let statusBadge = getStatusBadgeClass(item.status);
                    //var statusBadge = item.status === 'Approved' ? 'badge-phoenix-success' : 'badge-phoenix-danger';
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
                           <span class="badge badge-phoenix ${statusBadge} fs-9">${item.leadStatus}</span>
                        </td>
                        <td class="status align-middle white-space-nowrap pe-0 ps-2 d-flex justify-content-center" data-column="10">
                            <a href="#!" class="btn btn-outline-light btn-icon addShift-bulkEdit me-2"  id="editModalBtn" data-id="${item.leadId}"><i class="fas fa-edit text-black"></i></a>
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


    // ====================
    // Edit Button work
    // ======================
    $(document).on("click", "#editModalBtn", function (e) {
        // Bootstrap 5 way to open modal
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
                $("#descriptionText").val(response.leadDescription);
                // multiselect edit field read
                $('#serviceTypes').val(response.serviceIds).each(function () {
                    coreui.MultiSelect.getInstance(this)?.update();
                });
                showDev(response);
                //// ========== Lead Owner single select with search ==========
                //const $owner = $("#ownerID");
                //$owner.empty(); // clear previous options

                //// Add the lead owner from response
                //$owner.append(
                //    $("<option>", {
                //        value: response.leadOwnerId,
                //        text: response.leadOwnerName,
                //        selected: true
                //    })
                //);

                //// Initialize CoreUI MultiSelect for single select with search
                //let ms = coreui.MultiSelect.getInstance($owner[0]);
                //if (!ms) {
                //    ms = new coreui.MultiSelect($owner[0], {
                //        search: true,       // enable search
                //        selectionType: 'single', // single select
                //        placeholder: 'Select Lead Owner...'
                //    });
                //} else {
                //    ms.update(); // refresh if already initialized
                //}
            },
            error: function (xhr) {
                toastr.error("Error creating lead");
            }
        });
    });

    //$("#editBtn").on("click", function (e) {
    //    e.preventDefault();
    //    showDev("Edit Button clicked");
    //    console.log("clicked");
    //})

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




    // ==============================
    // update lead information
    // ==============================

    $("#editBtn").on("click", function (e) {
        e.preventDefault();
        //if (await fieldValidation()) {
        const data = {
            LeadID: $("#leadID").val(),
            LeadName: $("#leadName").val() || "",
            LeadStatusID: parseInt($("#leadStatusID").val()) || 0,
            LeadSourceID: parseInt($("#leadSourceID").val()) || 0,
            LeadOwnerID: parseInt($("#ownerID").val()) || 0,
            PriorityID: parseInt($("#leadPriorityID").val()) || 0,
            ApproximateDealValue: parseFloat($("#approximateDealValue").val()) || 0,
            ProbabilityPercentage: parseFloat($("#probabilityPercentage").val()) || 0,
            LeadDescription: $("#descriptionText").val(),
            ServiceTypeIds: $("#serviceTypes").val(),
        };
        showDev(data);
        $.ajax({
            url: '/CRM/EditLeadData',
            method: 'POST',
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",

            success: function (response) {

                if (response.success) {
                    toastr.success(response.message);
                    // HIDE modal
                    var myModalEl = document.getElementById('editModal');
                    var modal = bootstrap.Modal.getInstance(myModalEl);
                    modal.hide();
                } else {
                    toastr.error(response.message || "Failed to create lead");
                }
            },
            error: function (xhr) {
                toastr.error("Error creating lead");
            }
        });
        //}
    })

    // ============
    // get data
    // ===========
    let ownerList = [];
    let currentPage = 1;
    let loadItem = new Set();
    let loading = false;
    let noMoreDataDown = false;
    let selectedIndex = -1;
    let typedValue = ""; 

    async function getOwnerList(page = 1) {
        debugger;
        if (loading || noMoreDataDown) return;
        loading = true;
        
        try {
            let query = $("#queryText").val().trim();
            const response = await $.ajax({
                url: '/CRM/GetOwnerList',
                method: 'POST',
                data: { query: query, page: page },
            });

            if (page === 1) {
                $("#result-show-div").empty();
                loadItem.clear();
                selectedIndex = -1;
            }

            const results = response.result;

            if (!results || results.length === 0) {
                noMoreDataDown = true;
            } else {
                results.forEach(item => {
                    if (!loadItem.has(item.id)) {
                        $("#result-show-div").append(`<button type="button"
                            class="list-group-item list-group-item-action item"
                            data-id="${item.id}">
                            ${item.name}</button>`);
                        loadItem.add(item.id);
                    }
                })
            }
        } catch (e) {

        } finally {
            loading = false;
        }
        
    }
    // =============
    // search owner
        // ==============
    $("#queryText").on("input click", async function () {
        currentPage = 1;
        loading = false;
        noMoreDataDown = false;
        loadItem.clear();

        $("#result-show-div").remove();

        const resultDiv = `<div class="list-group bg-white p-0 border position-absolute" id="result-show-div" style="width:100%;overflow-y:auto; z-index:10012;max-height:210px;scrollbar-width: none;"></div>`;
        $(this).after(resultDiv);

        $("#result-show-div").off('scroll').on('scroll', function () {
            const container = $(this);
            if (!loading && !noMoreDataDown &&
                Math.ceil(container.scrollTop() + container.innerHeight()) >= container[0].scrollHeight) {
                currentPage++;
                getOwnerList(currentPage);
            }
        });

        await getOwnerList(currentPage); // fetch first page
    });

    // ===============
    // arrow key
    // ===================
    function checkLoadMoreKeyboard() {
        const container = $("#result-show-div");
        const items = container.find(".item");
        if (selectedIndex < 0 || selectedIndex >= items.length) return;

        const selectedItem = items.eq(selectedIndex);

        // Use container's scrollTop relative to container
        const containerTop = container.scrollTop();
        const containerBottom = containerTop + container.innerHeight();

        // Use selectedItem's position relative to container
        const itemTop = selectedItem.position().top + containerTop;
        const itemBottom = itemTop + selectedItem.outerHeight();

        // Reset flag so we can load more
        if (!loading && itemBottom > containerBottom - 20) {
            noMoreDataDown = false; // reset
            currentPage++;
            getOwnerList(currentPage);
        }
    }



    // ============
    // keyboard event
    // ===================


    $("#queryText").on("keydown", function (e) {
        const items = $("#result-show-div .item");
        if (items.length === 0) return;

        if (e.key === "ArrowDown") {
            selectedIndex = Math.min(selectedIndex + 1, items.length - 1);
            items.removeClass("active").eq(selectedIndex).addClass("active")[0].scrollIntoView({ block: "nearest" });

            // Show nearest suggestion in input
            $(this).val(items.eq(selectedIndex).text().trim());
            checkLoadMoreKeyboard();
            e.preventDefault();
            setTimeout(checkLoadMoreKeyboard, 50);
        }
             else if (e.key === "ArrowUp") {
                selectedIndex = Math.max(selectedIndex - 1, 0);
                items.removeClass("active").eq(selectedIndex)
                    .addClass("active")[0]
                .scrollIntoView({ block: "nearest" }); // <<< add this
            // Show nearest suggestion in input
            $(this).val(items.eq(selectedIndex).text().trim());
                e.preventDefault();
            } else if (e.key === "Tab") {
            if (selectedIndex >= 0) {
                const selectedName = items.eq(selectedIndex).text();
                $(this).val(selectedName);
                $("#result-show-div").remove();
                e.preventDefault();
            }
        }
    });

    //    // BIND AFTER WORK 

    //    $('#result-show-div').on('scroll', function () {
    //        currentPage = 1;
    //        onMoreDataDown = false;
    //        debugger;
    //        const container = $(this);
    //        const scrollTop = container.scrollTop();
    //        const innerHeight = container.innerHeight();
    //        const scrollHeight = container[0].scrollHeight;

    //        if (!loading && !noMoreDataDown && Math.ceil(scrollTop + innerHeight) >= scrollHeight) {
    //            currentPage++;
    //            getOwnerList(currentPage);
    //        }
    //    });


    //    await getOwnerList(currentPage);
    //})


    // ==============
    // scroll event
    // ==============


    // ========================
    // response when did search
    // ========================
    //$('#search-activity').on("input", function () {
    //    clearTimeout(typingTimer);
    //    typingTimer = setTimeout(function () {
    //        const search = $('#search-activity').val() || "";
    //        if (search !== lastSearch) {
    //            lastSearch = search;
    //            resetAndReload();
    //        }
    //    }, delay);
    //});
});