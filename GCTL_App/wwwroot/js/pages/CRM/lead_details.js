$(function () {

    // ==============================
    // Active option buttons
    // ==============================
    $(".option-btn").on('click', function () {
        $(".option-btn").removeClass('active');
        $(this).addClass('active');

        let btnText = $(this).text().trim();
        if (btnText === "Attachment") {
            $('#file-field').show();
        } else {
            $('#file-field').hide();
        }
    });

    // ==============================
    // Config & State
    // ==============================
    const activityListDiv = "#activity-list";
    const options = {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    };

    let currentPage = 1;
    let loading = false;
    let lastSearch = "";
    let noMoreDataDown = false;
    let loadedIds = new Set();

    // ==============================
    // Add Lead Details
    // ==============================
    //$('#addLDetails').on('click', function (e) {
    //    e.preventDefault();
        
    //    const buttonID = $(".option-btn.active").data('id');
    //    const date = $("#lDetailsDate").val();
    //    const text = $("#lDetailsText").val();
    //    const id = $("#leadID").val();
    //    const fileInput = $('#formFile')[0];
    //    const file = fileInput.files[0];

    //    console.log(fileInput);
    //    console.log(file);
    //    const convertedDate = convertToISODateTime(date);

    //    const data = {
    //        LeadID: id,
    //        LeadActivityTypeID: buttonID,
    //        ActivityDateTime: convertedDate,
    //        ActivityNote: text,
    //        File: file
    //    };
    //    showDev(data);


    //    $.ajax({
    //        url: '/LeadDetails/CeateLeadDetail',
    //        method: 'POST',
    //        contentType: false, 
    //        processData: false, 
    //        data: data,
    //        success: function (response) {
    //            toastr.success(response.message);
    //            resetAndReload();
    //            debugger;
    //            $(".option-btn").removeClass("active");

    //            $("#lDetailsDate").val("");
    //            text = $("#lDetailsText").val("");
    //        },
    //        error: function (error) {
    //            toastr.error(error.message || "Error adding lead detail");
    //        }
    //    });
    //});


    $('#addLDetails').on('click', function (e) {
        e.preventDefault();

        const buttonID = $(".option-btn.active").data('id');
        const date = $("#lDetailsDate").val();
        const text = $("#lDetailsText").val();
        const id = $("#leadID").val();
        const fileInput = $('#formFile')[0];
        const file = fileInput.files[0];

        if (!id || !buttonID || !date) {
            toastr.error("Please fill all required fields");
            return;
        }

        const convertedDate = convertToISODateTime(date);

        const formData = new FormData();
        formData.append("LeadID", parseInt(id));
        formData.append("LeadActivityTypeID", parseInt(buttonID));
        formData.append("ActivityDateTime", convertedDate);
        formData.append("ActivityNote", text || "");
        if (file) formData.append("File", file);

        // Debug
        for (let pair of formData.entries()) {
            console.log(pair[0], pair[1]);
        }

        $.ajax({
            url: '/LeadDetails/CeateLeadDetail',
            method: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                toastr.success(response.message);
                resetAndReload();
                $(".option-btn").removeClass("active");
                $("#lDetailsDate").val("");
                $("#lDetailsText").val("");
                $("#formFile").val("");
                $('#file-field').hide();
            },
            error: function (error) {
                toastr.error(error.responseJSON?.message || "Error adding lead detail");
            }
        });
    });

    // ==============================
    // Infinite scroll inside activity-list
    // ==============================
    $('#activity-list').on("scroll", function () {
        console.log("Scrolling")
        const container = $(this);
        const scrollTop = container.scrollTop();
        const innerHeight = container.innerHeight();
        const scrollHeight = container[0].scrollHeight;

        // Debug
        console.log("ScrollTop:", scrollTop, "InnerHeight:", innerHeight, "ScrollHeight:", scrollHeight);

        if (!loading && !noMoreDataDown && Math.ceil(scrollTop + innerHeight) >= scrollHeight) {
            console.log("?? Reached bottom ? loading page", currentPage + 1);
            currentPage++;
            updateActivate(currentPage, "down");
        }
    });

    // ==============================
    // Search with debounce
    // ==============================
    let typingTimer;
    const delay = 500;

    $('#search-activity').on("input", function () {
        clearTimeout(typingTimer);
        typingTimer = setTimeout(function () {
            const search = $('#search-activity').val() || "";
            if (search !== lastSearch) {
                lastSearch = search;
                resetAndReload();
            }
        }, delay);
    });

    // ==============================
    // Tab click handler
    // ==============================
    $("#myTab .nav-link").on('click', function (e) {
        e.preventDefault();
        $("#myTab .nav-link").removeClass('active');
        $(this).addClass('active');
        resetAndReload();
    });

    // ==============================
    // Fetch activity data
    // ==============================
    function updateActivate(page = 1, direction = "down") {
        if (loading) return;
        loading = true;

        const tabName = $("#myTab .nav-link.active").text().trim();
        const search = $("#search-activity").val() || "";
        const id = $("#leadID").val();
        const typeD = tabName === "All Activity" ? "" : tabName;

        $.ajax({
            url: '/LeadDetails/getActivityList',
            method: 'GET',
            contentType: 'application/json',
            data: { id, query: search, page, type: typeD },
            success: function (response) {
                console.log(response);
                if (!response || response.length === 0) {
                    noMoreDataDown = true;
                    return;
                }

                response.forEach(item => {
                    console.log(item.leadDetailID);
                    if (!item.leadDetailID) return; // skip invalid
                    if (!loadedIds.has(item.leadDetailID)) {
                        loadedIds.add(item.leadDetailID);
                        const activityDate = new Date(item.activityDateTime).toLocaleString('en-GB', options);
                        $(activityListDiv).append(renderActivity(item, activityDate));
                    }
                });
            },
            complete: function () { loading = false; },
            error: function (jqXHR, textStatus) {
                toastr.error("Error: " + textStatus);
            }
        });
    }

    // ==============================
    // Render a single activity
    // ==============================
    function renderActivity(value, activityDate) {
        return `
            <div class="border-bottom border-translucent py-4">
                <div class="d-flex">
                    <div class="d-flex bg-primary-subtle rounded-circle flex-center me-3"
                         style="width:25px; height:25px">
                        <span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon}"></span>
                    </div>
                    <div class="flex-1">
                        <div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
                            <div class="flex-1 me-2">
                                <h5 class="text-body-highlight lh-sm">${value.leadActivityName}</h5>
                                <p class="fs-9 mb-0">by<a class="ms-1" href="#!">${value.createdByName}</a></p>
                            </div>
                            <div class="fs-9">
                                <span class="fa-regular fa-calendar-days text-primary me-2"></span>
                                <span class="fw-semibold">${activityDate}</span>
                            </div>
                        </div>
                        <p class="fs-9 mb-0">${value.activityNote}</p>
                    </div>
                </div>
            </div>`;
    }

    // ==============================
    // Convert date to ISO string
    // ==============================
    function convertToISODateTime(dateTimeString) {
        const [datePart, timePart] = dateTimeString.split(' ');
        const [day, month, year] = datePart.split('/').map(Number);
        const fullYear = year < 100 ? 2000 + year : year;
        const [hours, minutes] = timePart.split(':').map(Number);
        const date = new Date(fullYear, month - 1, day, hours, minutes);

        const pad = (num) => String(num).padStart(2, '0');
        const offsetMinutes = date.getTimezoneOffset();
        const offsetSign = offsetMinutes > 0 ? '-' : '+';
        const offsetHours = pad(Math.floor(Math.abs(offsetMinutes) / 60));
        const offsetMins = pad(Math.abs(offsetMinutes) % 60);

        return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}:${pad(date.getSeconds())}${offsetSign}${offsetHours}:${offsetMins}`;
    }

    // ==============================
    // Reset state and reload page 1
    // ==============================
    function resetAndReload() {
        currentPage = 1;
        noMoreDataDown = false;
        loadedIds.clear();
        $(activityListDiv).empty();
        updateActivate(1, "reset");
    }

    // ==============================
    // Initial load
    // ==============================
    updateActivate(1, "reset");
});
