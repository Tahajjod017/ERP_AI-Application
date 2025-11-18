window.dataTable = function (root) {
    root = root || document;
    // Import the function from file1.js
    //import { loadExistingContacts } from './customer.js';

    //#region GetAll customer Data
    var currentPage = 1;
    var pageSize = 5;
    let currentSortColumn = 'CreatedAt';
    let currentSortOrder = 'desc';
    window.loadTableData = function (sortColumn, sortOrder) {
        var searchTerm = $("#customer-searchInput").val();

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
                var tableBody = $("#customer-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle  py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input customer-selectItem" data-id="${item.id}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap p-0 py-1">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.name}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.type}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.organizationTypeName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1"><a href="#" type="button" class="branchListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#branchOffcanvasBottom" aria-controls="branchOffcanvasBottom">${item.totalBranch}</a></td>
                            <td class="align-middle white-space-nowrap ps-0 py-1"><a href="#" type="button" class="warehouseListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#warehouseOffcanvasBottom" aria-controls="warehouseOffcanvasBottom">${item.totalWarehouse}</a></td>
                            <td class="align-middle white-space-nowrap ps-0 py-1"><a href="#" type="button" class="shippingListBtn" data-id="${item.id}" data-bs-toggle="offcanvas" data-bs-target="#shippingOffcanvasBottom" aria-controls="shippingOffcanvasBottom">${item.totalShipping}</a></td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.totalContactPerson}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
                                <div class="row g-3  py-1">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 customer-edit" href="#!" id="customer-edit" data-id="${item.id}"><i class="fas fa-edit"></i></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#customer-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#customer-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination("#customer-paginationLinks", "#customer-prevPageBtn", "#customer-nextPageBtn", paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });


    }

    loadTableData();

    //#endregion

    //#region customer edit

    
    $(document).on("click", ".customer-edit", function () {
        let id = $(this).data("id");
        loadCustomerData(id);
        changeTab("#customer-tab", "#customer");
        GoToTop();
    });
    //#endregion

    //#region GetAll branch Data
    var bcurrentPage = 1;
    var bpageSize = 5;
    let bcurrentSortColumn = 'CreatedAt';
    let bcurrentSortOrder = 'desc';
    window.loadWBranchTableData = function (customerID, sortColumn, sortOrder) {
        var searchTerm = $("#branch-searchInput").val();

        $.ajax({
            url: '/Customers/GetBranchList',
            method: 'GET',
            data: {
                customerID: customerID,
                pageNumber: bcurrentPage,
                pageSize: bpageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                showDev(response);
                var tableBody = $("#branchOffcanvas-body");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input branch-selectItem" data-id="${item.bid}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0 py-1">${index + 1}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bOrganizationTypeName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bFirstName} ${item.bLastName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bPhone}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bEmail}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bFullAddress}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.bTotalContactPerson}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 branch-edit" href="#!" data-cid="${item.bCustomerID}" data-bid="${item.bid}"><i class="fas fa-edit"></i></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#branch-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#branch-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination("#branch-paginationLinks", "#branch-prevPageBtn", "#branch-nextPageBtn", paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }
    //#endregion

    //#region branch edit
    $(document).on("click", ".branch-edit", function () {
        let bid = $(this).data("bid");
        let cid = $(this).data("cid");
        loadBranchData(bid, cid);
        changeTab("#branch-tab", "#branch");
        hideOffcanvas('branchOffcanvasBottom');
    });
    //#endregion

    

    //#region shipping TableDataGet
    $(document).on("click", ".branchListBtn", function () {
        var customerID = $(this).data("id");
        loadWBranchTableData(customerID, scurrentSortColumn, scurrentSortOrder);
    })
    //#endregion



    //#region warehouse edit
    $(document).on("click", ".warehouse-edit", function () {
        let wid = $(this).data("wid");
        let cid = $(this).data("cid");

        $.ajax({
            url: '/Customers/GetWarehouseInfo',
            method: 'POST',
            data: {
                customerID: cid,
                warehouseId: wid,
            },
            success: function (response) {
                changeTab("#warehouse-tab", "#warehouse");
                hideOffcanvas('warehouseOffcanvasBottom');

                const form = document.querySelector("#WarehouseForm");
                select2ScrollingDataSet('#WCountryID', response.wCountryID, response.wCountryName)
                select2ScrollingDataSet("#WCustomerID", response.wCustomerID, response.wCustomerName)
                setFormValues(form, response);
            },
            error: function (res) {
                showDev(res);
            }
        });
    });
    //#endregion

    //#region warehouse edit
    $(document).on("click", ".shipping-edit", function () {
        let sid = $(this).data("sid");
        let cid = $(this).data("cid");

        $.ajax({
            url: '/Customers/GetShippingInfo',
            method: 'POST',
            data: {
                customerID: cid,
                shippingId: sid,
            },
            success: function (response) {
                showDev(response);
                changeTab("#shipping-tab", "#shipping");
                hideOffcanvas('shippingOffcanvasBottom');

                const form = document.querySelector("#ShippingForm");
                select2ScrollingDataSet('#SCountryID', response.sCountryID, response.sCountryName)
                select2ScrollingDataSet("#SCustomerID", response.sCustomerID, response.sCustomerName)
                setFormValues(form, response);
            },
            error: function (res) {
                showDev(res);
            }
        });
    });
    //#endregion

    //#region Save Form Value
    
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

    //#region go to Top 
    function GoToTop() {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });

    }
    //#endregion

    //#region Update Pagination
    function updatePagination(paginationLink, prevPageBtn, nextPageBtn, pageNumbers, currentPage, totalPages) {
        const paginationLinks = $(paginationLink);
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
        $(prevPageBtn).prop('disabled', currentPage === 1);
        $(nextPageBtn).prop('disabled', currentPage === totalPages);
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
    function loadWarehouseTableData(customerID, sortColumn, sortOrder) {
        var searchTerm = $("#warehouse-searchInput").val();
        
        $.ajax({
            url: '/Customers/GetWarehouseList',
            method: 'GET',
            data: {
                customerID: customerID,
                pageNumber: wcurrentPage,
                pageSize: wpageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                showDev(response);
                var tableBody = $("#warehouseOffcanvas-body");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input warehouse-selectItem" data-id="${item.wid}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0 py-1">${index + 1}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wCustomerName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wFirstName} ${item.wLastName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wPhone}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wEmail}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.wFullAddress}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 warehouse-edit" href="#!" data-cid="${item.wCustomerID}" data-wid="${item.wid}"><i class="fas fa-edit"></i></a>
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

                updatePagination("#warehouse-paginationLinks", "#warehouse-prevPageBtn", "#warehosue-nextPageBtn", paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }
    //#endregion

    //#region shipping Table DataGet
    $(document).on("click", ".warehouseListBtn", function () {
        var customerID = $(this).data("id");
        loadWarehouseTableData(customerID, wcurrentSortColumn, wcurrentSortOrder);
    })
    //#endregion

    //#region GetAll shipping Data
    var scurrentPage = 1;
    var spageSize = 5;
    let scurrentSortColumn = 'CreatedAt';
    let scurrentSortOrder = 'desc';
    function loadWShippingTableData(customerID, sortColumn, sortOrder) {
        var searchTerm = $("#warehouse-searchInput").val();
        
        $.ajax({
            url: '/Customers/GetShippingList',
            method: 'GET',
            data: {
                customerID: customerID,
                pageNumber: scurrentPage,
                pageSize: spageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                showDev(response);
                var tableBody = $("#shippingOffcanvas-body");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle py-1" style="width: 5%;">
                                <input type="checkbox" class="form-check-input shipping-selectItem" data-id="${item.sid}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0 py-1">${index + 1}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sFullAddress}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sFirstName} ${item.sLastName}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sPhone}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sEmail}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sStreet}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sCity}</td>
                            <td class="align-middle white-space-nowrap ps-0 py-1">${item.sAdditionaladdress}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2 py-1">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 shipping-edit" href="#!" data-cid="${item.sCustomerID}" data-sid="${item.sid}"><i class="fas fa-edit"></i></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                let paginationInfo = response.paginationInfo;

                $("#shipping-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#shipping-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination("#shipping-paginationLinks", "#shipping-prevPageBtn", "#shipping-nextPageBtn", paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });
    }
    //#endregion

    //#region shipping TableDataGet
    $(document).on("click", ".shippingListBtn", function () {
        var customerID = $(this).data("id");
        loadWShippingTableData(customerID, scurrentSortColumn, scurrentSortOrder);
    })
    //#endregion

    return { loadTableData };
};


