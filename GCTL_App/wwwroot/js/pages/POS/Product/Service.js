let page = 1;
let pageSize = 5;
let search = "";
let totalRecords = 0;
let totalPages = 0;

$(document).ready(function () {
    loadServiceList();

    // Search
    $("#serviceSearch").keyup(function () {
        search = $(this).val();
        page = 1;
        loadServiceList();
    });

    // Page Size (your dropdown: 5,10,20,50,100)
    $("#serviceShow").change(function () {
        pageSize = parseInt($(this).val()) || 5;
        page = 1;
        loadServiceList();
    });

    // Prev / Next Buttons
    $("#prevBtn").click(function () {
        if (page > 1) {
            page--;
            loadServiceList();
        }
    });

    $("#nextBtn").click(function () {
        if (page < totalPages) {
            page++;
            loadServiceList();
        }
    });

    // Form Submit
    $("#serviceForm").on("submit", function (e) {
        e.preventDefault();
        saveService();
    });
});

function saveService() {
    
    // Step 1: Get all rate fields
    const nameSer = $("#serviceSelectService").val();
    const hourly = $("#serviceHourlyRate").val();
    const daily = $("#serviceDailyRate").val();
    const perJob = $("#servicePerJobRate").val();
    const perMeter = $("#servicePerMeterRate").val();

    // Step 2: Check if Service Name is empty
    if (!$("#serviceSelectService").val()) {
        alert("Service Name is required!");
        $("#serviceSelectService").focus();
        return;
    }

    // Step 3: Validate all rate fields are valid positive numbers (allow empty = 0)
    const isValidNumber = (val) => val === "" || (!isNaN(val) && parseFloat(val) > 0);

    if (!isValidNumber(hourly)) {
        alert("Hourly Rate must be a valid positive number!");
        $("#serviceHourlyRate").focus();
        return;
    }
    if (!isValidNumber(daily)) {
        alert("Daily Rate must be a valid positive number!");
        $("#serviceDailyRate").focus();
        return;
    }
    if (!isValidNumber(perJob)) {
        alert("Per Job Rate must be a valid positive number!");
        $("#servicePerJobRate").focus();
        return;
    }
    if (!isValidNumber(perMeter)) {
        alert("Per Meter Rate must be a valid positive number!");
        $("#servicePerMeterRate").focus();
        return;
    }

    // Step 4: Optional: Convert empty to 0 (cleaner for backend)
    if (hourly === "") $("#serviceHourlyRate").val("0");
    if (daily === "") $("#serviceDailyRate").val("0");
    if (perJob === "") $("#servicePerJobRate").val("0");
    if (perMeter === "") $("#servicePerMeterRate").val("0");

    // Step 5: Now submit safely
    $.ajax({
        url: "/SingleProduct/AddService",
        type: "POST",
        data: $("#serviceForm").serialize(),
        success: function (response) {
            // If backend returns JSON (recommended)
            if (response && response.success === false) {
                toastr.error("Error: " + (response.message || "Failed to save service"));
                return;
            }

            toastr.success(response.message || "Service added successfully!");
            $("#serviceForm")[0].reset();
            loadServiceList();
        },
        error: function (xhr) {
            const msg = xhr.responseText || "Server error";
            alert("Submit failed: " + msg);
        }
    });
}
function loadServiceList() {
    $.ajax({
        url: "/SingleProduct/GetServiceList",
        type: "GET",
        data: { page: page, pageSize: pageSize, search: search },
        success: function (res) {
            totalRecords = res.total || 0;
            totalPages = Math.ceil(totalRecords / pageSize) || 1;

            // === Render Table Rows ===
            $("#serTable-body").empty();
            if (res.data && res.data.length > 0) {
                $.each(res.data, function (i, item) {
                    $("#serTable-body").append(`
                        <tr>
                            <td class="white-space-nowrap ps-0">
                                <div class="form-check mb-0 fs-8">
                                    <input class="form-check-input" type="checkbox" />
                                </div>
                            </td>
                            <td class="ps-5">${item.serviceSelectService || ''}</td>
                            <td>${item.serviceHourlyRate || 0}</td>
                            <td>${item.serviceDailyRate || 0}</td>
                            <td>${item.servicePerJobRate || 0}</td>
                            <td>${item.servicePerMeterRate || 0}</td>
                            <td class="text-end pe-0 ps-4">
                                <!-- You can add Edit/Delete buttons here later -->
                            </td>
                        </tr>
                    `);
                });
            } else {
                $("#serTable-body").append(`
                    <tr><td colspan="7" class="text-center py-4 text-muted">No services found</td></tr>
                `);
            }

            // === Showing X to Y of Z ===
            let start = totalRecords === 0 ? 0 : (page - 1) * pageSize + 1;
            let end = Math.min(page * pageSize, totalRecords);
            $("#showingText").text(`Showing ${start} to ${end} of ${totalRecords}`);

            // === Disable Prev/Next ===
            $("#prevBtn").prop("disabled", page === 1);
            $("#nextBtn").prop("disabled", page >= totalPages);

            // === Render Page Numbers (This is what you wanted!) ===
            renderPagination();
        }
    });
}

// This function creates page numbers: 1 2 3 4 ...
function renderPagination() {
    const pagination = $(".pagination");
    pagination.empty();

    const maxVisible = 5;
    let startPage = Math.max(1, page - Math.floor(maxVisible / 2));
    let endPage = Math.min(totalPages, startPage + maxVisible - 1);

    if (endPage - startPage + 1 < maxVisible) {
        startPage = Math.max(1, endPage - maxVisible + 1);
    }

    // First page
    if (startPage > 1) {
        pagination.append(`<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`);
        if (startPage > 2) {
            pagination.append(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
        }
    }

    // Page numbers
    for (let i = startPage; i <= endPage; i++) {
        const active = i === page ? 'active' : '';
        pagination.append(`
            <li class="page-item ${active}">
                <a class="page-link" href="#" data-page="${i}">${i}</a>
            </li>
        `);
    }

    // Last page
    if (endPage < totalPages) {
        if (endPage < totalPages - 1) {
            pagination.append(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
        }
        pagination.append(`<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`);
    }

    // Click handler for page numbers
    pagination.find("a[data-page]").click(function (e) {
        e.preventDefault();
        page = parseInt($(this).data("page"));
        loadServiceList();
    });
}