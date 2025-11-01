window.dataTable = function (root) {
    root = root || document;

    var currentPage = 1;
    var pageSize = 5;
    let currentSortColumn = 'CreatedAt';
    let currentSortOrder = 'desc';
    function loadTableData(sortColumn, sortOrder) {
        var searchTerm = $("#gender-searchInput").val();

        $.ajax({
            url: '/Customers/GetAll',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                showDev(response);
                var tableBody = $("#gender-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input gender-selectItem" data-id="${item.id}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.name}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.type}</td>
                            <td class="align-middle white-space-nowrap ps-0"><a href="#" type="button" class="branchListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#offcanvasBottom" aria-controls="offcanvasBottom">${item.totalBranch}</a></td>
                            <td class="align-middle white-space-nowrap ps-0">${item.totalWarehouse}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 customer-edit" href="#!" id="customer-edit" data-id="${item.id}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 gender-bulkEdit" href="#!" id="customer-single-delete" data-id="${item.id}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#gender-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#gender-totalCount").text(`(${paginationInfo.totalItems})`);

                //updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });


    }
    loadTableData(currentSortColumn, currentSortOrder);

    $(document).on("click", ".branchListBtn", function () {
        let id = $(this).data("id");
        var tableBody = $("#offcanvas-body");
        tableBody.empty();

        $.ajax({
            url: '/Customers/GetBranchList',
            method: 'POST',
            data: { id: id },
            success: function (response) {
                showDev(response)// make sure response is an array

                if (response && response.length > 0) {
                    response.forEach(function (item, index) {
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input gender-selectItem" data-id="${item.bid}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${index + 1}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bFirstName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bLastName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bPhone}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.bFullAddress}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 branch-edit" href="#!" id="customer-edit" data-cid="${item.bCustomerID}" data-bid="${item.bid}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 gender-bulkEdit" href="#!" id="customer-single-delete" data-id="${item.bid}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="6" class="text-center">No data available</td></tr>');
                }
            },
            error: function (res) {
                console.error("Error fetching branches:", res);
            }
        });
    });

    $(document).on("click",".customer-edit", function () {
        let id = $(this).data("id");
        $.ajax({
            url: '/Customers/GetCustoerInfo',
            method: 'POST',
            data: {
                id: id,
            },
            success: function (response) {
                showDev(response)
                const form = document.querySelector("#customerForm"); // form is DOM element

                setFormValues(form, response);
            },
            error: function (res) {
                showDev(res)
            }
        });
    })

    $(document).on("click",".branch-edit", function () {
        let bid = $(this).data("bid");
        let cid = $(this).data("cid");
        $.ajax({
            url: '/Customers/GetBranchInfo',
            method: 'POST',
            data: {
                customerID: cid,
                branchId: bid,
            },
            success: function (response) {
                showDev(response)
                const form = document.querySelector("#branchForm"); // form is DOM element

                setFormValues(form, response);
            },
            error: function (res) {
                showDev(res)
            }
        });
    })


    function setFormValues(form, jsonData) {
        Object.keys(jsonData).forEach(key => {
            const capitalizedKey = key.charAt(0).toUpperCase() + key.slice(1);
            const input = form.querySelector(`[name="${capitalizedKey}"]`);
            if (!input) return; // skip if input not found

            const value = jsonData[key] ?? ""; // fallback to empty string if null

            // Handle different input types
            if (input.type === "checkbox") {
                input.checked = !!value;
            } else if (input.type === "radio") {
                const radio = form.querySelector(`input[name="${key}"][value="${value}"]`);
                if (radio) radio.checked = true;
            } else if ($(input).hasClass("select2-hidden-accessible")) {
                // Handle select2 dropdowns
                $(input).val(value).trigger('change');
            } else {
                input.value = value;
            }
        });
    }

};