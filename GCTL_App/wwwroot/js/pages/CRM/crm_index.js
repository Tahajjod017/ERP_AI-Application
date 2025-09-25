$('document').ready(function () {
    let ids = {
        leadID: "#leadID",
        leadName: "#leadName",
        leadStatusID: '#leadStatusID',
        filterLeadStatus: '#LeadStatus',
        filterLeadSource: "#leadSource",
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

    $("#pageElementSize, #dateRange2, #customerType, #LeadStatus").on("change", function () {
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
        var leadSourceID = $(ids.filterLeadStatus).val();
        showDev(leadSourceID);
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
                sortDirection: dir,
                leadStatus: leadSourceID
            },
            success: function (data) {
                var tbody = $('#processed-resignation-body');
                tbody.empty();
                let itemsPerPage = parseInt($('#pageElementSize').val()) || 10;

                let page = parseInt($('#pageNumber').data('page')) || 1;

                let pageOffset = (page - 1) * itemsPerPage;

                $.each(data.result.leads, function (index, item) {
                    let itemSL = pageOffset + index + 1; 
                    let statusBadge = getStatusBadgeClass(item.status);
                    tbody.append(`
                    <tr class="hover-actions-trigger btn-reveal-trigger position-static">

                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="0">${itemSL}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="1"><a class="fw-bold cursor-pointer" href="/LeadDetails/Index/${item.leadId}">${item.leadName}</a></td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="2">${item.leadStatus}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="3">${item.leadSourceName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="4">${item.leadOwnerName}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="5">${item.approximateDealValue}</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="6">${item.probabilityPercentage} %</td>
                        <td class="department align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="7">${item.email}</td>
                        <td class="position align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="8">${item.phone}</td>
                        <td class="reason align-middle white-space-nowrap ps-4 fw-semibold text-body py-1" data-column="9">${item.contactName}</td>
                        <td class="status align-middle white-space-nowrap pe-0 ps-2" data-column="10">
                           <span class="badge badge-phoenix ${statusBadge} fs-9">${item.leadStatus}</span>
                        </td>
                        <td class="status align-middle white-space-nowrap pe-0 ps-2 d-flex justify-content-center" data-column="11">
                            <a href="#!" class="btn btn-outline-light btn-icon addShift-bulkEdit me-2"  id="editModalBtn" data-id="${item.leadId}"><i class="fas fa-edit text-black"></i></a>
                        </td>
                    </tr>
                `);
                });
                DynamicTableDrag.refreshTableSettings('mytable');
                
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




    // ==============================
    // update lead information
    // ==============================

    $("#editBtn").on("click", function (e) {
        e.preventDefault();
        debugger;
        const data = {
            LeadID: $("#leadID").val(),
            LeadName: $("#leadName").val() || "",
            LeadStatusID: parseInt($("#leadStatusID").val()) || 0,
            LeadSourceID: parseInt($("#leadSourceID").val()) || 0,
            LeadOwnerID: parseInt($("#leadOwnerId").val()) || 0,
            PriorityID: parseInt($("#leadPriorityID").val()) || 0,
            ApproximateDealValue: parseFloat($("#approximateDealValue").val()) || 0,
            ProbabilityPercentage: parseFloat($("#probabilityPercentage").val()) || 0,
            LeadDescription: $("#descriptionText").val(),
            ServiceTypeIds: $("#serviceTypes").val(),
        };
        showDev(data);
        if (validation()) {
        $.ajax({
            url: '/CRM/EditLeadData',
            method: 'POST',
            data: JSON.stringify(data),
            contentType: "application/json; charset=utf-8",

            success: function (response) {

                if (response.success) {
                    loadProcessedTable();
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
        }
    });


    // ===============
    // lead validation
    // =================
    function validation() {
        let requiredField = [
            '#leadOwnerId',
            '#leadSourceID',
            '#leadStatusID',
            '#leadName',
            '#leadPriorityID'
        ];
        let isValid = true;

        requiredField.forEach(function (selector) {
            let el = $(selector);
            let value = el.val() ? el.val().trim() : '';
            let target = el;

            // Special case for Choices.js (hidden select)
            if (el.closest('.choices').length > 0) {
                target = el.closest('.choices').find('.choices__inner');
            }

            if (value === '' || value === null) {
                target.css('border', '1px solid red');
                isValid = false;
            } else {
                target.css('border', '1px solid #ccc'); // reset valid field
            }
        });

        return isValid;
    }

    //function validation() {
    //    let requiredField = [
    //        ids.leadName,
    //        ids.leadPriorityID,
    //        ids.leadSourceID,
    //        ids.leadStatusID,
    //        ids.leadOwnerId
    //    ];

    //    let isValid = true;

    //    requiredField.forEach(function (selector) {
    //        let el = $(selector);
    //        let value = el.val() ? el.val().trim() : '';
    //        let target = el;

    //        // Special case for Choices.js (hidden select)
    //        if (el.closest('.choices').length > 0) {
    //            target = el.closest('.choices').find('.choices__inner');
    //        }

    //        if (value === '' || value === null) {
    //            target.css('border', '1px solid red');
    //            isValid = false;
    //        } else {
    //            target.css('border', '1px solid #ccc'); // reset valid field
    //        }
    //    });

    //    return isValid;
    //}

    // #region Choice with Pagination + Infinite Scroll (server-side search only)
    const selectEl = document.getElementById('leadOwnerId');
    let debounceTimer;
    let loading = false;
    let currentPage = 1;
    let lastSearch = '';
    let hasMore = true;

    const choices = new Choices(selectEl, {
        searchEnabled: true,
        placeholder: true,
        placeholderValue: 'Select Organization...',
        searchPlaceholderValue: 'Type to search...',
        noChoicesText: 'Type 3 or more characters...',
        searchResultLimit: -1, // disable local limiting
        shouldSort: false,
        duplicateItemsAllowed: false,
        itemSelectText: '',
        removeItemButton: true,

        // 🚨 disable client-side filtering (server handles search)
        searchChoices: false,
        fuseOptions: false,
        searchFn: () => true
    });

    // Fetch data from server
    async function fetchOptions(search, page = 1, pageSize = 50) {
        loading = true;
        try {
            const res = await fetch(`/CRM/SearchEmployee?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`);
            const data = await res.json();
            hasMore = data.hasMore;
            return data;
        } catch (error) {
            console.error("Error fetching organizations:", error);
            return { items: [], hasMore: false };
        } finally {
            loading = false;
        }
    }

    // Handle debounce on search
    selectEl.addEventListener('search', function (e) {
        const searchTerm = e.detail.value;
        clearTimeout(debounceTimer);

        if (searchTerm.length < 1) {
            choices.clearChoices();
            return;
        }

        debounceTimer = setTimeout(async () => {
            currentPage = 1;
            lastSearch = searchTerm;
            const data = await fetchOptions(searchTerm, currentPage);

            choices.clearChoices();
            if (data.items.length > 0) {
                // replace with new results
                choices.setChoices(data.items, 'value', 'label', true);
            }
        }, 500); // debounce delay
    });

    // Scroll handler
    async function handleScroll(e) {
        const dropdownList = e.target;
        if (!loading && hasMore && dropdownList.scrollTop + dropdownList.clientHeight >= dropdownList.scrollHeight - 10) {
            currentPage++;
            const data = await fetchOptions(lastSearch, currentPage);

            if (data.items.length > 0) {
                // append results, keep existing
                choices.setChoices(data.items, 'value', 'label', false);
            }
        }
    }

    // Reattach scroll listener when dropdown opens
    choices.passedElement.element.addEventListener('showDropdown', () => {
        const dropdownList = document.querySelector('.choices__list--dropdown .choices__list[role="listbox"]');
        if (dropdownList) {
            dropdownList.removeEventListener('scroll', handleScroll);
            dropdownList.addEventListener('scroll', handleScroll);
        }
    });
    // #endregion
});