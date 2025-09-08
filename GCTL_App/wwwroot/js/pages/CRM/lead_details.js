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
    const upcomingListDiv = "#upcoming-activity";
    const options = {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    };

    let currentPage = 1;
    let currentPage2 = 1;
  
    let lastSearch = "";


    let loading = false, noMoreDataDown = false;
    let loading2 = false, noMoreDataDown2 = false;

    let loadedIds = new Set();
    let loadedIds2 = new Set();


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

        $.ajax({
            url: '/LeadDetails/CeateLeadDetail',
            method: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                toastr.success(response.message);
                resetAndReload();
                resetAndReloadUpcoming();
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
    $('#activity-list, #upcoming-activity',).on("scroll", function () {
        const container = $(this);
        const containerName = container.attr('id');
        const scrollTop = container.scrollTop();
        const innerHeight = container.innerHeight();
        const scrollHeight = container[0].scrollHeight;

        if (containerName === 'activity-list') {
            if (!loading && !noMoreDataDown && Math.ceil(scrollTop + innerHeight) >= scrollHeight) {
                currentPage++;
                updateActivate(currentPage, "down");
            }
        } else if (containerName === 'upcoming-activity') {
            if (!loading2 && !noMoreDataDown2 && Math.ceil(scrollTop + innerHeight) >= scrollHeight) {
                currentPage2++;
                updateUpcomingActivate(currentPage2)
            }
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
                debugger;
                $("#activity-label").text(tabName);
                if (!response || response.length === 0) {
                    noMoreDataDown = true;
                    //$("#activity-result-div").addClass("d-none");
                    //$("#activity-label").text("");
                    return;
                } else {
                    $("#all-activity-div").removeClass("d-none");
                    //$("#activity-result-div").removeClass("d-none");
                }
                

                response.forEach(item => {
                    if (!item.leadDetailID) return; // skip invalid
                    if (!loadedIds.has(item.leadDetailID)) {
                        loadedIds.add(item.leadDetailID);
                        const activityDate = new Date(item.activityDateTime).toLocaleString('en-GB', options);

                        if (item.leadActivityName === 'Attachment') {
                            $(activityListDiv).append(renderAttachmentActivity(item, activityDate));
                        } else {
                            $(activityListDiv).append(renderActivity(item, activityDate));
                        }
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
    // Fetch upcoming activity data
    // ==============================
    function updateUpcomingActivate(page = 1, direction = "down") {
        if (loading2) return;
        loading2 = true;
        const id = $("#leadID").val();
        $.ajax({
            url: '/LeadDetails/GetUpcomingActivityList',
            method: 'GET',
            contentType: 'application/json',
            data: { id, page },
            success: function (response) {
                if (!response || response.length === 0) {
                    noMoreDataDown2 = true;
                    return;
                    $("#upcomming-div").addClass("d-none");
                } else {
                    //$("#myTab").css("display", "block");
                    $("#upcomming-div").removeClass("d-none");
                }

                response.forEach(item => {
                    if (!item.leadDetailID) return;
                    if (!loadedIds2.has(item.leadDetailID)) {
                        loadedIds2.add(item.leadDetailID);
                        const activityDate = new Date(item.activityDateTime).toLocaleString('en-GB', options);
                        $('#upcoming-activity').append(renderActivity(item, activityDate));
                    }
                });
            },
            complete: function () { loading2 = false;},
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
            <div class="border-bottom border-translucent py-3 mx-3">
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

    // previewFile function
    function previewFile(fileUrl) {
        let ext = fileUrl.split('.').pop().toLowerCase();
        let container = document.getElementById("filePreviewContainer");
        container.innerHTML = ""; // reset

        if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) {
            // Image preview
            container.innerHTML = `<img src="${fileUrl}" class="img-fluid" alt="preview">`;
        }
        else if (ext === "pdf") {
            container.innerHTML = `<iframe src="${fileUrl}" 
                           style="width:100%;height:500px" frameborder="0"></iframe>`;
        }
        //else if (["xls", "xlsx"].includes(ext)) {
        //    // Excel preview using SheetJS
        //    fetch(fileUrl).then(res => res.arrayBuffer()).then(data => {
        //        let workbook = XLSX.read(data, { type: "array" });
        //        let firstSheet = workbook.SheetNames[0];
        //        let html = XLSX.utils.sheet_to_html(workbook.Sheets[firstSheet]);
        //        container.innerHTML = `<div class="table-responsive" style="max-height:500px;overflow:auto">${html}</div>`;
        //    }).catch(() => {
        //        window.open(fileUrl, "_blank"); // fallback download
        //    });
        //}
        else {
            // Not supported ? force download
            window.open(fileUrl, "_blank");
            return;
        }

        // Show modal
        let modal = new bootstrap.Modal(document.getElementById('filePreviewModal'));
        modal.show();
    }
    // Ensure global access
    window.previewFile = previewFile;
    function renderAttachmentActivity(value, activityDate) {
        return `
        <div class="border-bottom border-translucent py-3 mx-3">
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
                            
                            <p class="fs-9 mb-0">file: 
                                <a href="javascript:void(0)" onclick="previewFile('${value.fileLink}')">
                                    ${value.fileLink}
                                </a>
                            </p>
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

    $("#editBtn").on("click", function (e) {
        e.preventDefault();
        //if (await fieldValidation()) {
            const data = {
                LeadName: $("#leadName").val() || "",
                LeadStatusID: parseInt($("#leadStatusId").val()) || 0,
                LeadSourceID: parseInt($("#leadSourceId").val()) || 0,
                LeadOwnerID: parseInt($("#leadOwnerId").val()) || 0,
                PriorityID: parseInt($("#leadPriorityId").val()) || 0,
                leadID : $("#leadID").val(),
                ApproximateDealValue: parseFloat($("#approximateDealValue").val()) || 0,
                ProbabilityPercentage: parseFloat($("#probabilityPercentage").val()) || 0,
                CustomerId: parseInt($("#customerID").val()) || 0,
                LeadDescription: $("#descriptionText").val(),
                ServiceTypeIds: $("#serviceTypes").val() || [],
        };
        showDev(data);
            $.ajax({
                url: '/LeadDetails/EditLeadData',
                method: 'POST',
                data: JSON.stringify(data),
                success: function (response) {

                    if (response.success) {
                        //toastr.success(response.message);
                        //getCustomerList();
                        //clearTabData(idMapIndex.indexBase);
                        //clearTabData(idMapIndex.demoField);
                        //window.location.href = "/crm/Index";
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
    // update Lead Source value
    // ==============================

    $("#leadSource, #lead-status, #leadPriority").on("change", function () {
        let fieldValue = $(this).val();
        let fieldID = $(this).attr("id");
        let fieldName = fieldID === "leadSource" ? "source" : fieldID == "lead-status" ? "stage" : fieldID === 'leadPriority' ? "priority" : "";
        let leadID = $("#leadID").val();

        $.ajax({
            url: '/LeadDetails/UpdateLeadValue',
            method: 'POST',
            data: { LeadID: leadID, FieldName: fieldName, FieldValue: fieldValue },
            success: function (response) {
                if (response) {
                    toastr.success("Lead source updated successfully");
                }
            },
           
            error: function (jqXHR, textStatus) {
                toastr.error("Error: " + textStatus);
            }

        });
    })

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
    function resetAndReloadUpcoming() {
        currentPage2 = 1;
        noMoreDataDown2 = false;
        loadedIds2.clear()
        $(upcomingListDiv).empty();
        updateUpcomingActivate(currentPage2);
    }

    // ==============================
    // loss and won button work
    // ==============================
    $("#loss, #won").on("click", function (e) {
        let type = $(this).attr('id');
        showDev(type)
        const id = $("#leadID").val();
        showDev(id)
        $.ajax({
            url: '/LeadDetails/IsWon',
            method: 'POST',
            data: { id : id, type : type },
            success: function (response) {
                showDev(response);
                $(this+"modal").hide();
            },
            complete: function () { loading2 = false; },
            error: function (jqXHR, textStatus) {
                toastr.error("Error: " + textStatus);
            }

        });
    });


    // ==============================
    // Initial load
    // ==============================
   
    updateActivate(1, "reset");
    updateUpcomingActivate();
   
});
