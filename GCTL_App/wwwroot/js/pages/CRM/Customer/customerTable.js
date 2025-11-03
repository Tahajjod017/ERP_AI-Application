window.dataTable = function (root) {
    root = root || document;

    //#region GetAll Data
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
                            <td class="text-center text-middle align-middle  py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input gender-selectItem" data-id="${item.id}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap p-0 py-1">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.name}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.type}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1"><a href="#" type="button" class="branchListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#branchOffcanvasBottom" aria-controls="branchOffcanvasBottom">${item.totalBranch}</a></td>
                            <td class="align-middle white-space-nowrap ps-0 py-1"><a href="#" type="button" class="warehouseListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#warehouseOffcanvasBottom" aria-controls="warehouseOffcanvasBottom">${item.totalWarehouse}</a></td>
                            <td class="align-middle white-space-nowrap ps-0 py-1"><a href="#" type="button" class="branchListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#shippingOffcanvasBottom" aria-controls="shippingOffcanvasBottom">${item.totalShipping}</a></td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
                                <div class="row g-3  py-1">
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

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });


    }
    loadTableData(currentSortColumn, currentSortOrder);

    //#endregion

    //#region get All Branch
    $(document).on("click", ".branchListBtn", function () {
        let id = $(this).data("id");
        var tableBody = $("#branchOffcanvas-body");
        tableBody.empty();

        $.ajax({
            url: '/Customers/GetBranchList',
            method: 'POST',
            data: { id: id },
            success: function (response) {
                if (response && response.length > 0) {
                    response.forEach(function (item, index) {
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input gender-selectItem" data-id="${item.bid}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0 py-1">${index + 1}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bFirstName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bLastName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bPhone}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bFullAddress}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
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
    //#endregion

    //#region customer edit
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
                const form = document.querySelector("#customerForm");

                select2ScrollingDataSet('#CountryID', response.countryID, response.countryName)
                setFormValues(form, response);
            },
            error: function (res) {
                showDev(res)
            }
        });
    })
    //#endregion

    //#region select2ScrollingDataSet
    function select2ScrollingDataSet(fieldId, id, text) {
        const countrySelect = $(fieldId);
        if (id && text) {
            let newOption = new Option(text, id, true, true);
            countrySelect.append(newOption).trigger('change');
        } else {
            countrySelect.val('').trigger('change'); // clear if no country
        }

    }
    //#endregion

    //#region branch edit
    $(document).on("click", ".branch-edit", function () {
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
                changeTab("#branch-tab", "#branch");
                hideOffcanvas('branchOffcanvasBottom');

                const form = document.querySelector("#branchForm");
                select2ScrollingDataSet('#BCountryID', response.bCountryID, response.bCountryName)
                select2ScrollingDataSet("#BCustomerID", response.bCustomerID, response.bCustomerName)
                setFormValues(form, response);
            },
            error: function (res) {
                showDev(res);
            }
        });
    });
    //#endregion

    //#region Save Form Value
    function setFormValues(form, jsonData) {
        
        Object.keys(jsonData).forEach(key => {
            const capitalizedKey = key.charAt(0).toUpperCase() + key.slice(1);
            const input = form.querySelector(`[name="${capitalizedKey}"]`);
            if (!input) return;

            const value = jsonData[key] ?? "";

            if (input.type === "checkbox") {
                input.checked = !!value;
            } else if (input.type === "radio") {
                const radio = form.querySelector(`input[name="${key}"][value="${value}"]`);
                if (radio) radio.checked = true;
            } else {
                input.value = value;
            }
        });
    }
    //#endregion

    //#region change Tag
    function changeTab(activeTab, activetTabItem) {
        $(".nav-link").removeClass("active");
        $(".tab-pane").removeClass("active show");
        $(activeTab).addClass("active");
        $(activetTabItem).addClass("active show");
    }

    function hideOffcanvas(name) {
        const offcanvasEl = document.getElementById(name);
        const offcanvas = bootstrap.Offcanvas.getInstance(offcanvasEl) || new bootstrap.Offcanvas(offcanvasEl);

        offcanvasEl.addEventListener('hidden.bs.offcanvas', function handler() {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
            offcanvasEl.removeEventListener('hidden.bs.offcanvas', handler);
        });

        offcanvas.hide();
    }
    //#endregion

    //#region Update Pagination
    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#gender-paginationLinks");
        paginationLinks.empty();
        const windowSize = 1;
        const createPageButton = (page) => `
                <li class="page-item ${page === currentPage ? 'active' : ''}">
                    <button class="page-link page-btn" data-page="${page}">${page}</button>
                </li>
            `;
        const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
        if (currentPage > windowSize + 1) {
            paginationLinks.append(createPageButton(1), addEllipsis());
        }
        const startPage = Math.max(1, currentPage - windowSize);
        const endPage = Math.min(totalPages, currentPage + windowSize);
        for (let i = startPage; i <= endPage; i++) {
            paginationLinks.append(createPageButton(i));
        }
        if (currentPage < totalPages - windowSize) {
            paginationLinks.append(addEllipsis(), createPageButton(totalPages));
        }
        $("#gender-prevPageBtn").prop('disabled', currentPage === 1);
        $("#gender-nextPageBtn").prop('disabled', currentPage === totalPages);
    }
    //#endregion

    //#region go to next page
    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endregion

    //#region GetAll Warehouse Data
    var wcurrentPage = 1;
    var wpageSize = 5;
    let wcurrentSortColumn = 'CreatedAt';
    let wcurrentSortOrder = 'desc';
    function loadWarehouseTableData(sortColumn, sortOrder) {
        var searchTerm = $("#warehouse-searchInput").val();

        $.ajax({
            url: '/Customers/GetWarehouseList',
            method: 'GET',
            data: {
                pageNumber: wcurrentPage,
                pageSize: wpageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                showDev(response);
                var tableBody = $("#warehouse-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input gender-selectItem" data-id="${item.wid}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0 py-1">${index + 1}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wFirstName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wLastName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wPhone}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wFullAddress}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 branch-edit" href="#!" id="customer-edit" data-cid="${item.wCustomerID}" data-bid="${item.wid}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 gender-bulkEdit" href="#!" id="customer-single-delete" data-id="${item.wid}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#warehouse-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#warehouse-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });


    }
    //#endregion

    //#region warehouse TableDataGet
    $(".warehouseListBtn").on("click", function () {
        debugger
        loadWarehouseTableData(wcurrentSortColumn, wcurrentSortOrder);
    })
    //#endregion
};