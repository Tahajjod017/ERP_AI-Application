
$(document).ready(function () {
    initSelect2();


    choiceManager.setChoiceValue('priority', 1)


    showDev("new.js: document ready");


    //#region On change

   

    $('#OrganizationId').change(function () {

        var orgId = $(this).val();
        var branchDropdown = $('#OrganizationBranchId');

        branchDropdown.empty();
        branchDropdown.append('<option value="">Select Branch</option>');

        if (orgId !== "") {
            $.ajax({
                url: '/Requisition/GetBranchesByOrganization',
                type: 'GET',
                data: { organizationId: orgId },
                success: function (data) {
                    choiceManager.populateDropdown('OrganizationBranchId', data)
                }
            });
        }
    });
    
    //#endregion 



    //#region Table and realted Filters

    let page = 1;
    let pageSize = 10;
    let search = "";
    let sortColumn = "RequisitionId";
    let sortDirection = "asc";
    let projectId = "";
    let productTypeId = "";
    let fromDate
    let toDate

    function loadRequisitions() {
        $.ajax({
            url: '/Requisition/GetRequisitions',
            data: {
                page: page,
                pageSize: pageSize,
                search: search,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                projectId: projectId,
                productTypeId: productTypeId,
                fromDate: fromDate,
                toDate: toDate,
            },
            success: function (res) {

                showDev(res, 'Nicher Table');

                let rows = '';
                $.each(res.data, function (i, item) {
                    rows += `
                        <tr class="position-static">
                            <td class="align-middle white-space-nowrap ps-0 reqId">#${item.requisitionId}</td>
                            <td class="align-middle white-space-nowrap ps-2 productName">${item.requitionCode}</td>
                        
                            <td class="align-middle white-space-nowrap ps-2 reqDate">${formatDate(item.requisitionDate)}</td>
                     
                            
                            <td class="align-middle white-space-nowrap ps-2 productSubCatagory">${item.requisitionBy}</td>
                         
                            <td class="align-middle white-space-nowrap ps-2 approveQuantity">${item.requisitionItems}</td>
                            <td class="align-middle white-space-nowrap ps-2 approvalStatus">
                                <span class="badge badge-phoenix ${getStatusBadge(item.status)}">${item.status}</span>
                            </td>
                            <td class="align-middle text-end white-space-nowrap pe-2 action">
                                <div class="d-flex g-3">
                                    

                                   
                                    <a href="#" class="nav-item me-2" type="button" data-bs-toggle="modal" data-bs-target="#modelForEditRequisition" data-id="${item.requisitionId}">
                                        <i class="fas fa-edit text-black tblEditBtn"></i>
                                    </a>
                                    <a href="#" class="nav-item me-2 tblDelBtn" data-bs-toggle="modal" data-bs-target="#deleteConfirmModal" data-id="${item.requisitionId}">
                                        <i class="far fa-trash-alt text-black "></i>
                                    </a>



                                </div>
                            </td>
                        </tr>`;
                });
                $("#product-Requisition-history-table").html(rows);



                DynamicTableDrag.refreshTableSettings('NewRequisition');

                // Pagination
                let totalPages = Math.ceil(res.totalRecords / pageSize);
                let paginationHtml = '';
                for (let i = 1; i <= totalPages; i++) {
                    paginationHtml += `<li class="page-item ${i === page ? 'active' : ''}">
                        <a class="page-link" href="#">${i}</a>
                    </li>`;
                }
                $(".pagination").html(paginationHtml);

                // Show total info
                $("[data-list-info]").text(`Showing ${(page - 1) * pageSize + 1} to ${Math.min(page * pageSize, res.totalRecords)} of ${res.totalRecords}`);
            }
        });
    }

    // Date formatting helper
    function formatDate(dateStr) {
        let d = new Date(dateStr);
        return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' });
    }

    // Badge helper
    function getStatusBadge(status) {
        if (typeof status !== "string") return "badge-phoenix-secondary";

        switch (status.toLowerCase()) {
            case "new": return "badge-phoenix-primary";
            case "approved": return "badge-phoenix-success";
            case "rejected": return "badge-phoenix-danger";
            default: return "badge-phoenix-secondary";
        }
    }

    // Init load
    loadRequisitions();

    // Search
    $("#searchInput").on("keyup", function () {
        search = $(this).val();
        page = 1;
        loadRequisitions();
    });

    // Page Size
    $("#pageSizeSelect").on("change", function () {
        pageSize = $(this).val();
        page = 1;
        loadRequisitions();
    });

    // Pagination click
    $(document).on("click", ".pagination .page-link", function (e) {
        e.preventDefault();
        page = parseInt($(this).text());
        loadRequisitions();
    });

    // Filters
    $("#projectFilter, #productTypeFilter").on("change", function () {
        projectId = $("#projectFilter").val();
        productTypeId = $("#productTypeFilter").val();
        page = 1;
        loadRequisitions();
    });

    // Sorting
    $("th.sort").on("click", function () {
        sortColumn = $(this).data("sort");
        sortDirection = sortDirection === "asc" ? "desc" : "asc";
        loadRequisitions();
    });

    


    $('#fromDate').on('change', function () {
        debugger;
        fromDate = $(this).val();
        page = 1;
        loadRequisitions();
    });
    $('#toDate').on('change', function () {
        toDate = $(this).val();
        page = 1;
        loadRequisitions();
    });

    //#endregion

    //#region create and related

    addNewRow();


    $("#createRequisitionForm").on("submit", function (e) {
        e.preventDefault();


        let isValid = true;
        $("select[name^='Products'][name$='.ProductId']").each(function () {

            if ($(this).closest('tr').is(':visible') && !$(this).val()) {
                isValid = false;
                toastr.warning("Please select a product for all rows.");
                return false; // Break the loop
            }
        });

        if (!isValid) return;

        const supervisorSelected = choiceManager.getChoiceValue('SupervisorId'); // $("select[name='SupervisorId']").val();
        const projectSelected = choiceManager.getChoiceValue('ProjectId'); // $("select[name='ProjectId']").val();

        if (supervisorSelected == "" || projectSelected == "") {
            toastr.warning("Please select either a Project Manager or a Project.");
            return;
        }

        const $submitBtn = $(".submitBTN");
        const $resetBtn = $(".resetBTN");

        // Disable buttons and show loading
        $submitBtn.prop("disabled", true).html("Saving...");
        $resetBtn.prop("disabled", true);


        // Show loading modal
        const loadingModal = new bootstrap.Modal(document.getElementById('loadingModal'));
        loadingModal.show();

        $.ajax({
            url: '/Requisition/Create',
            type: "POST",
            data: $(this).serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    $("#createRequisitionForm")[0].reset();
                    $("#productTableBody").empty();
                    addNewRow();
                    loadRequisitions();
                    choiceManager.resetAllChoices();
                    loadingModal.hide();
                } else {
                    toastr.warning(res.message || "Validation failed.");
                    loadingModal.hide();
                }
            },
            error: function () {
                toastr.warning("Something went wrong!");
                loadingModal.hide();
            },
            complete: function () {
                // Re-enable buttons and restore text
                $submitBtn.prop("disabled", false).html("Save");
                $resetBtn.prop("disabled", false);

                // Hide loading modal
                loadingModal.hide();
            }
        });
    });




    // Dependent Product dropdown
    $(document).on("change", "select[id^='ProductType_']", function () {
        const productTypeId = $(this).val();
        const index = $(this).attr("id").split("_")[1];
        const productSelect = $("#Product_" + index);

        showDev(productTypeId, 'typw id');

        if (!productTypeId) {
            productSelect.html('<option value="">Select Product</option>');
            return;
        }

        $.ajax({
            url: '/Requisition/GetProductsByType',
            type: 'GET',
            data: { productTypeId },
            success: function (data) {
                let options = '<option value="">Select Product</option>';
                data.forEach(p => {
                    //options += `<option value="${p.productID}">${p.productName}</option>`;



                    options += `<option value="${p.id}">${p.productName}</option>`;
                });
                productSelect.html(options);
            },
            error: function () {
                productSelect.html('<option value="">Failed to load</option>');
            }
        });
    });


    // Dependent Unit on Product change
    $(document).on("change", "select[id^='Product_']", function () {
        const productId = $(this).val();
        const index = $(this).attr("id").split("_")[1];
        const unitInput = $("#UnitType_" + index);

        if (!productId) {
            unitInput.val('');
            return;
        }

        $.ajax({
            url: '/Requisition/GetUnitByProduct',  // Assume you add a new controller action; adjust URL if needed
            type: 'GET',
            data: { productId: productId },
            success: function (data) {
                unitInput.val(data.unitName || '');  // Assume response has { unitName: '...' }
            },
            error: function () {
                unitInput.val('Failed to load');
            }
        });
    });



   




    //#endregion


    //#region Edit and related

    // === ONLY FOR EDIT MODAL: Cascading Dropdowns (Product Type → Product → Unit) ===

    // When Product Type changes in Edit modal
    $(document).on("change", "#editProductTableBody select.product-type-dd", function () {
        const $typeSelect = $(this);
        const productTypeId = $typeSelect.val();
        const $row = $typeSelect.closest("tr");
        const $productSelect = $row.find("select.product-dd");
        const $unitInput = $row.find("input[readonly][placeholder='Unit will auto-fill']");

        // Reset dependents
        $productSelect.html('<option value="">Select Product</option>');
        $unitInput.val('');

        if (!productTypeId) return;

        $.ajax({
            url: '/Requisition/GetProductsByType',
            type: 'GET',
            data: { productTypeId: productTypeId },
            success: function (data) {
                let options = '<option value="">Select Product</option>';
                $.each(data, function (i, p) {
                    options += `<option value="${p.id}">${p.productName}</option>`;
                });
                $productSelect.html(options);
            },
            error: function () {
                $productSelect.html('<option value="">Failed to load</option>');
            }
        });
    });

    // When Product changes in Edit modal
    $(document).on("change", "#editProductTableBody select.product-dd", function () {
        const productId = $(this).val();
        const $row = $(this).closest("tr");
        const $unitInput = $row.find("input[readonly][placeholder='Unit will auto-fill']");

        $unitInput.val('');

        if (!productId) return;

        $.ajax({
            url: '/Requisition/GetUnitByProduct',
            type: 'GET',
            data: { productId: productId },
            success: function (data) {
                $unitInput.val(data.unitName || 'N/A');
            },
            error: function () {
                $unitInput.val('Error');
            }
        });
    });


    // Edit button click - load data into modal
    $(document).on("click", "a[data-bs-target='#modelForEditRequisition']", function () {
        const id = $(this).data('id');

        $.ajax({
            url: '/Requisition/GetRequisitionById',
            type: "GET",              // or "POST" depending on your backend
            dataType: "json",         // expect JSON response
            data: { id: id },
            headers: { "RequestVerificationToken": $("input[name='__RequestVerificationToken']").val() },
            success: function (data) {
                console.log("Server returned:", data);

                showDev(data, 'ddd')
                openEditRequisition(data.data)
            },
            error: function (xhr, status, error) {
                let message = "Failed to load requisition.";
                if (xhr.responseJSON && xhr.responseJSON.message) {
                    message = xhr.responseJSON.message;
                } else if (error) {
                    message = error;
                }
                console.error("Error fetching requisition:", status, error);
                alert(message);
            }


        });
    });

    // Function to populate edit modal (call this when opening modal)
    function openEditRequisition(requisition) {
        // Set main fields
        $('#edit_RequisitionId').val(requisition.reqId);
       
        choiceManager.setChoiceValue('edit_OrganizationId', requisition.organizationId);
        choiceManager.setChoiceValue('edit_OrganizationBranchId', requisition.organizationBranchId);
        choiceManager.setChoiceValue('edit_RequesterId', requisition.requesterId);
        choiceManager.setChoiceValue('edit_Priority', requisition.priority);
       
        $('#edit_RequisitionNote').val(requisition.requisitionNote);

        // Clear and repopulate product rows
        $('#editProductTableBody').empty();

        if (requisition.products && requisition.products.length > 0) {
            requisition.products.forEach((product, idx) => {
                addNewRowToEditModal();
                let $row = $('#editProductTableBody tr').last();

                $row.find('select[name*="ProductTypeId"]').val(product.productTypeId).trigger('change');

                // Wait until products are loaded
                setTimeout(() => {
                    $row.find('select[name*="ProductId"]').val(product.productId).trigger('change');
                    $row.find('input[name*="Quantity"]').val(product.quantity);
                    $row.find('input[name*="Id"]').val(product.id || 0);
                }, 300); // adjust delay if needed

               



            });
        } else {
            // Add one empty row if none exist
            addNewRowToEditModal();
        }

       
    }


    // Save edit
    window.saveEditRequisition = function () {
        $.ajax({
            url: '/Requisition/Edit',
            type: 'POST',
            data: $('#editRequisitionForm').serialize(),
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    // $('#modelForEditRequisition').modal('hide');
                    hideModal('modelForEditRequisition')
                    loadRequisitions(); // Reload table
                } else {
                    toastr.warning(res.message || "Update failed.");
                }
            },
            error: function () {
                alert("Something went wrong!");
            }
        });
    };

    //#endregion

    //#region Delete

    // Delete button click
    $(document).on("click", ".tblDelBtn", function () {
        //const id = $(this).closest('button').data('id');
        const id = $(this).data('id');

        $('#deleteRequisitionId').val(id);

        showDev(id, 'sss')

    });

    // Confirm delete
    window.confirmDelete = function () {
        const id = $('#deleteRequisitionId').val();
        showDev(id, 'sss')
        $.ajax({
            url: '/Requisition/Delete',
            type: 'POST',
            data: {
                id: id,
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    hideModal('deleteConfirmModal')
                    loadRequisitions(); // Reload table
                } else {
                    toastr.warning(res.message || "Delete failed.");
                }
            },
            error: function () {
                toastr.warning("Something went wrong!");
            }
        });
    };





    //#endregion


    var empInit = $('#empinit').val();
    if (empInit) {
        choiceManager.setChoiceValue('SupervisorId', empInit);
        // $('#SupervisorId').trigger('change');
    }

    

    //#region Preview PDF
    $('#btnExportPDFpreview').on('click', function () {
        customToaster.loading("Generating your PDF, please wait...");
        const productTypeID = $("#productType").val();
        showDev();
        fetch(`/Requisition/GeneratePDF?fromDate=${fromDate}&toDate=${toDate}`, { method: "POST" })
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                // Open in a new tab for preview
                customToaster.success("PDF generated successfully!");
                window.open(url, '_blank');
            })
            .catch(() => {
                customToaster.error("Failed to generate PDF.");
            });
    });
    //#endregion

    //#region download pdf
    $('#btnExportPDFdownload').on('click', function () {
        customToaster.loading("Generating your PDF, please wait...");
        const productTypeID = $("#productType").val();
        fetch(`/Requisition/GeneratePDF?fromDate=${fromDate}&toDate=${toDate}`, { method: "POST" })
            .then(res => res.blob())
            .then(blob => {
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                const now = new Date();
                const formattedDate = now.getFullYear().toString() +
                    String(now.getMonth() + 1).padStart(2, '0') +
                    String(now.getDate()).padStart(2, '0') + "_" +
                    String(now.getHours()).padStart(2, '0') +
                    String(now.getMinutes()).padStart(2, '0') +
                    String(now.getSeconds()).padStart(2, '0');
                a.download = `NewRequisition${formattedDate}.pdf`;
                document.body.appendChild(a);
                a.click();
                a.remove();
                window.URL.revokeObjectURL(url);
                customToaster.success("PDF generated successfully!");
            })
            .catch(() => {
                tcustomToaster.error("Failed to generate PDF.");
            });
    });
    //#endregion

    //#region for generate xl file
    $("#btnExportXL").on("click", async function downloadExcel() {
        const btn = document.getElementById('btnExportXL');
        const msg = document.getElementById('message');

        try {
            customToaster.loading("Generating your Excel File, please wait...");
            const productTypeID = $("#productType").val();
            // Call your API endpoint
            const response = await fetch(`/Requisition/DownloadExcel?fromDate=${fromDate}&toDate=${toDate}`, {
                method: 'GET',
                headers: {
                    'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
                }
            });

            if (!response.ok) {
                customToaster.error("Failed to generate Excel.");
            }

            // Get blob from response
            const blob = await response.blob();

            // Trigger file download
            const url = window.URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'NewRequisition.xlsx';
            document.body.appendChild(a);
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);

            // Show success message
            customToaster.success("Excel generated successfully!");
        } catch (error) {
            customToaster.error("Failed to generate Excel.");
        } finally {
            btn.disabled = false;
        }
    });
    //#endregion



});




//#region Select2
function initSelect2(context) {

    $(context).find('.product-type-dd').each(function () {
        if (!$(this).hasClass('select2-hidden-accessible')) {
            $(this).select2({
                width: '100%',
                placeholder: 'Select Product Type',
                allowClear: true
            });
        }
    });

    $(context).find('.product-dd').each(function () {
        if (!$(this).hasClass('select2-hidden-accessible')) {
            $(this).select2({
                width: '100%',
                placeholder: 'Select Product',
                allowClear: true
            });
        }
    });
}
function initSelect2Modal(context) {

    $(context).find('.product-type-dd').each(function () {
        if (!$(this).hasClass('select2-hidden-accessible')) {
            $(this).select2({
                width: '100%',
                placeholder: 'Select Product Type',
                allowClear: true,
                dropdownParent: $('#modelForEditRequisition')

            });
        }
    });

    $(context).find('.product-dd').each(function () {
        if (!$(this).hasClass('select2-hidden-accessible')) {
            $(this).select2({
                width: '100%',
                placeholder: 'Select Product',
                allowClear: true,
                dropdownParent: $('#modelForEditRequisition')

            });
        }
    });
}

//#endregion

//#region Add / Remove Row





//#region Create 


window.addNewRow = function () {

    // Destroy select2 from template if ever initialized
    $('#productRowTemplate')
        .find('.product-type-dd, .product-dd')
        .each(function () {
            if ($(this).hasClass('select2-hidden-accessible')) {
                $(this).select2('destroy');
            }
        });

    const index = $("#productTableBody tr").length;
    let template = $("#productRowTemplate").prop("outerHTML");

    template = template.replace(/INDEX/g, index);
    template = template.replace('id="productRowTemplate"', '');

    let $newRow = $(template);

    // ✅ THIS LINE WAS MISSING (CRITICAL)
    $newRow.find('input[name="Products.Index"]').val(index);

    $("#productTableBody").append($newRow);

    // Init Select2 AFTER append
    initSelect2($newRow);
};



window.removeRow = function (btn) {
    $(btn).closest("tr").remove();

    $("#productTableBody tr").each(function (index) {

        // Destroy select2 before renaming
        $(this).find('.product-type-dd, .product-dd').select2('destroy');

        $(this).find('input, select').each(function () {
            let name = $(this).attr('name');
            if (name) {
                $(this).attr('name', name.replace(/\[\d+\]/, `[${index}]`));
            }

            let id = $(this).attr('id');
            if (id) {
                $(this).attr('id', id.replace(/_\d+/, `_${index}`));
            }
        });

        $(this).find('input[name="Products.Index"]').val(index);

        // Re-init
        initSelect2(this);
    });
};



//#endregion

//#region edit

// Add new row in Edit Modal
window.addNewRowToEditModal = function () {
    const index = $("#editProductTableBody tr").length;
    let template = $("#editProductRowTemplate").prop("outerHTML");
    template = template.replace(/INDEX/g, index);
    template = template.replace('id="editProductRowTemplate"', '');
    let $newRow = $(template);

    // Set hidden index for model binding
    $newRow.find('input[name="Products.Index"]').val(index);

    $("#editProductTableBody").append($newRow);
    initSelect2Modal($newRow); // Reuse your existing initSelect2 function
};

// Remove row in Edit Modal + reindex
window.removeEditRow = function (btn) {
    $(btn).closest("tr").remove();

    $("#editProductTableBody tr").each(function (index) {
        $(this).find('.product-type-dd, .product-dd').select2('destroy');

        $(this).find('input, select').each(function () {
            let name = $(this).attr('name');
            if (name) {
                $(this).attr('name', name.replace(/\[\d+\]/, `[${index}]`));
            }
            let id = $(this).attr('id');
            if (id) {
                $(this).attr('id', id.replace(/_\d+/, `_${index}`));
            }
        });

        $(this).find('input[name="Products.Index"]').val(index);
        initSelect2Modal(this);
    });
};



//#endregion


//#endregion