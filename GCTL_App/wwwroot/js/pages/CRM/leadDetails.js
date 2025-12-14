$(function () {
    const ids = {
        note: "#aNote",
        date: '#aDate',
        file: '#aFile',
        contactNumber : "#ContectPersonId",
        contactEmail : "#ContectPersonEmailId",
        leadID: '#leadID',
        addActiveBtn: '#addLActivity',
        wonConfirmDiv: '#won-fonfirm-div',
        restoreBtn: '#restoreBtn2',

        wonBtn: '.special-btn:first',   // first .special-btn
        lostBtn: '.special-btn:last',   // last .special-btn
        cSpecialBtn: '.special-btn',
        closingDateDiv: '#closingDateDiv',
        closingDateResult: '#closingDateResult',

        successPercentage: '#successPercentage',
        cancelPercentage: '#cancelPercentage',
        lostPercentage: '#lostPercentage',
    }

    //=============================
    // gobal veriable
    //==============================
    let isWon = null;

    // ==============================
    // Active option buttons
    // ==============================
    $(".option-btn").on('click', function () {
        $(".option-btn").removeClass('active');
        $(this).addClass('active');
        let btnText = $(this).data('usefor');
        if (btnText === "Attachment") {
            $('#file-field').show();
            $('#contact-field').show();
            $('#email-field').show();

            $(ids.addActiveBtn).removeClass('d-none');
            $(ids.addActiveBtn).removeAttr('disabled');
            $(ids.wonConfirmDiv).addClass('d-none');
        }
        else if (btnText === "Call") {
            $('#email-field').hide();
            $('#file-field').hide();
            $('#contact-field').show();
        }
        else if (btnText === "Email") {
            $('#contact-field').hide();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Email") {
            $('#contact-field').hide();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Offline Meeting") {
            $('#contact-field').show();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Online Meeting") {
            $('#contact-field').show();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Online Meeting") {
            $('#contact-field').show();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Quatation") {
            $('#contact-field').show();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Rev. Quatation") {
            $('#contact-field').show();
            $('#file-field').hide();
            $('#email-field').show();
        }
        else if (btnText === "Won") {
            let isWonAlready = $(this).data('confirm');
            if (isWonAlready !== 'yes') {
                $('#file-field').hide();
                $('#contact-field').hide();
                $('#dateField').hide();
                $(ids.addActiveBtn).addClass('d-none');
                $(ids.addActiveBtn).prop('disabled', true);
                $(ids.wonConfirmDiv).removeClass('d-none');
            }

        } else if (btnText === "Lost") {
            $('#file-field').hide();
            $('#dateField').hide();
            $('#contact-field').hide();
            $('#email-field').hide();
            $('#note-Field').show();

            $(ids.addActiveBtn).removeClass('d-none');
            $(ids.addActiveBtn).removeAttr('disabled');
            $(ids.wonConfirmDiv).addClass('d-none');
        } else {
            $('#file-field').hide();
            $('#dateField').show();
            $('#note-Field').show();
            $('#contact-field').hide();
            $('#email-field').hide();
            //$('#contact-field').hide();
            $(ids.addActiveBtn).removeClass('d-none');
            $(ids.addActiveBtn).removeAttr('disabled');
            $(ids.wonConfirmDiv).addClass('d-none');
        }
    });

    $("#ContectPersonId").select2({
        placeholder: 'Select Contact Person',
        ajax: {
            url: "/LeadDetails/GetContactNumberList",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { leadId: $(ids.leadID).val(),search: params.term || '', page: params.page || 1 };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: data.items.map(x => ({
                        id: x.value,
                        text: x.label
                    })),
                    pagination: { more: data.hasMore }
                };
            },
            cache: true
        },
        language: {
            noResults: function () {
                return $(
                    `<span>Data not found. Add a <a id="createCustomer" href="#">Contact</a></span>`
                );
            }
        },
        width: '100%',
        multiple: true,
        
    });
    $("#ContectPersonEmailId").select2({
        placeholder: 'Select Contact Email',
        ajax: {
            url: "/LeadDetails/GetContactEmailList",
            dataType: 'json',
            delay: 250,
            data: function (params) {
                return { leadId: $(ids.leadID).val(),search: params.term || '', page: params.page || 1 };
            },
            processResults: function (data, params) {
                params.page = params.page || 1;
                return {
                    results: data.items.map(x => ({
                        id: x.value,
                        text: x.label
                    })),
                    pagination: { more: data.hasMore }
                };
            },
            cache: true
        },
        language: {
            noResults: function () {
                return $(
                    `<span>Data not found. Add a <a id="createCustomer" href="#">Email</a></span>`
                );
            }
        },
        width: '100%',
    });

    // won cancel btn
    $('#won-btn-cancel').on('click', function () {
        $('#dateField').show();

        $(ids.addActiveBtn).removeClass('d-none');
        $(ids.addActiveBtn).removeAttr('disabled');
        $(ids.wonConfirmDiv).addClass('d-none');
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

    let secialButtonState = null;

    //==========================
    // save lead activity function
    //==============================
    async function saveActivityFunction(e) {
        const userFor = $(".option-btn.active").data("usefor");
        const buttonName = (userFor === "Won" || userFor === "Lost")
            ? userFor
            : $(".option-btn.active").text().trim();


        const buttonID = $(".option-btn.active").data('id');
        const leadID = $(ids.leadID).val();
        const note = $(ids.note).val();
        const date = $(ids.date).val();
        const contactNumber = $(ids.contactNumber).val();
        const contactEmail = $(ids.contactEmail).val();
        const fileInput = $(ids.file)[0];
        const file = fileInput ? fileInput.files[0] : null;

        // run validation
        if (!validation(buttonName)) return;

        //let isWonOrLost = $('.special-btn').hasClass('active2');
        //let isWonOrLostText = $('.special-btn.active').text().trim();
        // Show confirmation modal if Won/Lost
        //if (isWon != null) {
        //    const confirmed = await showConfirmationModal();
        //    if (!confirmed) return; // User clicked No, stop execution
        //}

        const formData = new FormData();
        formData.append("LeadID", parseInt(leadID));
        formData.append("LeadActivityTypeID", parseInt(buttonID));
        formData.append("ActivityNote", note || "");
        formData.append("ActivityTypeName", buttonName);
        formData.append("ContactEmail", contactEmail);
        formData.append("ContactNumber", contactNumber);

        if (buttonName !== "Won" && buttonName !== "Lost") {
            const convertedDate = convertToISODateTime(date);
            formData.append("ActivityDateTime", convertedDate);
            if (file) formData.append("File", file);
        }
        let isWonOrLostBtnSelected = $('.special-btn').hasClass('active');
        console.log(formData);
        $.ajax({
            url: '/LeadDetails/SaveLeadActivity',
            method: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: async function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    await updateActivate();
                    resetAndReload();
                    resetAndReloadUpcoming();
                    $(".option-btn").removeClass("active");
                    $(ids.date).val("");
                    $(ids.note).val("");
                    $(ids.file).val("");
                    $('#ContectPersonId').val(null).trigger('change');
                    $('#ContectPersonEmailId').val(null).trigger('change');
                    $('#file-field').hide();
                    $(ids.wonConfirmDiv).addClass("d-none");
                    $(ids.addActiveBtn).removeClass("d-none");

                    
                    if (isWon === true && isWonOrLostBtnSelected) {
                        customToaster.success("Congratulations! Lead won successfully.");
                        makeDisabledState();
                    } if (isWon === false && isWonOrLostBtnSelected) {
                        customToaster.success("Lead marked as Lost successfully.");
                        makeDisabledState();
                    }
                } else {
                    toastr.error(response.message);
                }
               
            },
            error: function (error) {
                toastr.error(error.responseJSON?.message || "Error saving lead activity");
            }
        });
    }

    // ==================
    // save activity
    // ==================
    $('#addLActivity').on('click', function (e) {
        e.preventDefault();
        saveActivityFunction(this);
    });

    //============================
    // yes no Won button work
    // ============================
    $('#wonYes, #wonNo').on('click', function () {
        saveActivityFunction(this);
        secialButtonState = "won";
    });


    //============================
    // validation
    // ============================

    function validation(placeName) {
        clearAllValidationBorders();

        let requiredField = [];
        if (placeName === 'Lost') {
            requiredField = [ids.note];
        } else if (placeName !== 'Won') {
            requiredField = [ids.date, ids.note];
        }

        let isValid = true;

        if (!$('.option-btn').hasClass("active")) {
            $('#optionBtnDiv').css("border", "1px solid red");
            isValid = false;
        } else {
            $('#optionBtnDiv').css("border", "");
        }

        const activeBtn = $(".option-btn.active").text().trim();
        if (activeBtn === "Attachment") {
            requiredField.push(ids.file);
        }

        requiredField.forEach(function (selector) {
            let $el = $(selector);
            let fieldText = $el.val();
            if (!fieldText || fieldText.trim() === "") {
                $el.css("border-color", "red");
                isValid = false;
            } else {
                $el.css("border-color", "");
            }
        });

        return isValid;
    }


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
    function updateActivate(page = 1) {
        return new Promise((resolve, reject) => {
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
                    showLeadCreatorPercentage(response.successPercentage, response.lostPercentage, response.cancelPercentage)


                    if (response.closingDate != null) {
                        showClosedDate(response.closingDate);
                    } else {
                        hideClosedDate();
                    }

                    $("#activity-label").text(tabName);
                    if (!response.activities || response.activities.length === 0) {
                        noMoreDataDown = true;
                    } else {
                        $("#all-activity-div").removeClass("d-none");
                    }
                    $(ids.cSpecialBtn).removeClass('active2');
                    isWon = response.isWon;
                    
                    if (response.isWon == true) {
                        $(ids.wonBtn).addClass('active2');
                        makeDisabledState();
                    }
                    else if (response.isWon == false) {
                        $(ids.lostBtn).addClass('active2');
                        makeDisabledState();
                    }

                    if (response.isWon === true || response.isWon === false) {
                        $(ids.restoreBtn).removeClass('d-none');
                        makeDisabledState();
                    } else {
                        $(ids.restoreBtn).addClass('d-none');
                    }

                    response.activities.forEach(item => {
                        if (!item.leadDetailID) return;
                        if (!loadedIds.has(item.leadDetailID)) {
                            loadedIds.add(item.leadDetailID);
                            const activityDate = new Date(item.activityDateTime).toLocaleString('en-GB', options);

                            if (item.leadActivityName === 'Attachment') {
                                $(activityListDiv).append(renderAttachmentActivity(item, activityDate));
                            } else {
                                renderActivity(item, activityDate, 'activity-list'); // For main activity list
                            }
                        }
                    });
                },
                complete: function () { loading = false; resolve(200); },
                error: function (jqXHR, textStatus) {
                    toastr.error("Error: " + textStatus);
                }
            });
        });
        
    }

    //==============================
    // show showLeadCreatorPercentage
    //================================
    function showLeadCreatorPercentage(success, lost, cancel) {
        $(ids.successPercentage).text(success);
        $(ids.lostPercentage).text(lost);
        $(ids.cancelPercentage).text(cancel);
    }

    //==============================
    // show close Date Div
    //================================
    function showClosedDate(date) {
        const d = new Date(date);
        const pad = n => n.toString().padStart(2, '0');
        const isoLocal = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
        $(ids.closingDateDiv).removeClass('d-none');
        $(ids.closingDateResult).text(showClosedDate(isoLocal));
    }
    //==============================
    // hide closed Date Div
    //================================
    function hideClosedDate(date) {
        $(ids.closingDateDiv).addClass('d-none');
        $(ids.closingDateResult).text("");
    }
    // =============================
    // make disabled statue
    //==========
    function showClosedDate(date) {
        $(ids.closingDateDiv).removeClass('d-none');
        const d = new Date(date);
        const options = {
            year: 'numeric',
            month: 'numeric',
            day: 'numeric',
            hour: 'numeric',
            minute: 'numeric',
            second: 'numeric',
            hour12: true
        };
        $(ids.closingDateResult).text(d.toLocaleString('en-US', options));
    }
    // =============================
    // make disabled statue
    //==============================
    function makeDisabledState() {
        $(ids.addActiveBtn).prop('disabled', true);
        $("#optionBtnDiv .option-btn").prop('disabled', true);
        $(ids.wonBtn).prop('disabled', true);
        $(ids.lostBtn).prop('disabled', true);
        $(ids.note).prop('disabled', true);
        $(ids.date).prop('disabled', true);
        $(ids.file).prop('disabled', true);
    }
    function makeEnableState() {
        $(ids.addActiveBtn).removeAttr('disabled');
        $("#optionBtnDiv .option-btn").removeAttr('disabled');
        $(ids.wonBtn).removeAttr('disabled');
        $(ids.lostBtn).removeAttr('disabled');
        $(ids.note).removeAttr('disabled');
        $(ids.date).removeAttr('disabled');
        $(ids.file).removeAttr('disabled');
    }
    // ==============================
    // Fetch upcoming activity data
    // ==============================
    function updateUpcomingActivate(page = 1) {
        if (loading2) return;
        loading2 = true;
        const id = $("#leadID").val();
        $.ajax({
            url: '/LeadDetails/GetUpcomingActivityList',
            method: 'GET',
            contentType: 'application/json',
            data: { id, page },
            success: function (response) {
                if (!response || response.length === 0 && loadedIds2.size == 0) {
                    $("#upcomming-div").addClass("d-none");
                    return;

                }
                  
                if (!response || response.length === 0) {
                    noMoreDataDown2 = true;
                    //$("#upcomming-div").addClass("d-none");
                    return;

                } else {
                    //$("#myTab").css("display", "block");
                    $("#upcomming-div").removeClass("d-none");
                }
                response.forEach(item => {
                    if (!item.leadDetailID) return;
                    if (!loadedIds2.has(item.leadDetailID)) {
                        loadedIds2.add(item.leadDetailID);
                        const activityDate = new Date(item.activityDateTime).toLocaleString('en-GB', options);
                        if (item.leadActivityName === 'Attachment') {
                            $('#upcoming-activity').append(renderAttachmentActivity(item, activityDate));
                        } else {
                            renderActivity(item, activityDate, 'upcoming-activity'); // For upcoming activities
                        }

                    }
                });
            },
            complete: function () { loading2 = false; },
            error: function (jqXHR, textStatus) {
                toastr.error("Error: " + textStatus);
            }

        });
    }

    // ==============================
    // Render a single activity
    // ==============================
   function updateVerticalLine() {
    const container = document.getElementById('upcoming-activity');
    const line = container.querySelector('.vertical-line');

    if (line) {
        // Total scrollable height including items
        line.style.height = container.scrollHeight + 'px';
    }
}

// Call this after rendering activities
    //function renderActivity(value, activityDate) {
    //    const container = document.getElementById('upcoming-activity');

    //    // ------------------------
    //    // BUILD PHONE HTML FIRST
    //    // ------------------------
    //    debugger;
    //    let phoneHtml = "";
    //    if (value.phoneNumber && value.phoneNumber.trim() !== "") {
    //        phoneHtml = value.phoneNumber
    //            .split(',')
    //            .map(e => `<p class="fs-9 mb-0">${e.trim()}</p>`)
    //            .join("");
    //    }

    //    // ------------------------
    //    // BUILD EMAIL HTML FIRST
    //    // ------------------------
    //    let emailHtml = "";
    //    if (value.emailAddress && value.emailAddress.trim() !== "") {
    //        emailHtml = value.emailAddress
    //            .split(',')
    //            .map(e => `<p class="fs-9 mb-0">${e.trim()}</p>`)
    //            .join("");
    //    }

    //    const div = document.createElement('div');
    //    div.className = 'activity-item border-bottom border-translucent';
    //    div.innerHTML = `
    //    <div class="d-flex">
    //        <div class="d-flex bg-primary-subtle rounded-circle flex-center me-3 mt-2">
    //            <span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon}"></span>
    //        </div>

    //        <div class="flex-1 card p-2">
    //            <div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
    //                <div class="flex-1 me-2">
    //                    <h5 class="text-body-highlight lh-sm">${value.leadActivityName}</h5>
    //                    <p class="fs-9 mb-0">by <a class="ms-1" href="#!">${value.createdByName}</a></p>
    //                </div>
    //                <div class="fs-9">
    //                    <span class="fa-regular fa-calendar-days text-primary me-2"></span>
    //                    <span class="fw-semibold">${activityDate}</span>
    //                </div>
    //            </div>

    //            ${phoneHtml}
    //            ${emailHtml}
    //            ${value.activityNote ? `<p class="fs-9 mb-0">${value.activityNote}</p>` : ""}
    //        </div>
    //    </div>
    //`;

    //    container.appendChild(div);
    //    updateVerticalLine();
    //}


// Example: after rendering all activities
// activities.forEach(act => renderActivity(act, act.activityDate));


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
    function renderActivity(value, activityDate, containerId) {
        const container = document.getElementById(containerId);

        // Make sure vertical line exists for this container
        if (!container.querySelector('.vertical-line')) {
            const line = document.createElement('div');
            line.className = 'vertical-line';
            line.style.position = 'absolute';
            line.style.left = '51px';
            line.style.top = '0';
            line.style.width = '3px';
            line.style.backgroundColor = '#0d6efd';
            line.style.zIndex = '0';
            container.appendChild(line);
        }

        const div = document.createElement('div');
        div.className = 'activity-item';
        div.innerHTML = `
        <div class="d-flex">
            <div class="d-flex bg-primary-subtle rounded-circle flex-center me-3 mt-3">
                <span class="fa-solid text-primary-dark fs-9 ${value.leadActivityIcon}"></span>
            </div>
            <div class="flex-1 card p-3">
                <div class="d-flex justify-content-between flex-column flex-xl-row mb-2 mb-sm-0">
                    <div class="flex-1 me-2">
                        <h5 class="text-body-highlight lh-sm">${value.leadActivityName}</h5>
                        <p class="fs-9 mb-0">by <a class="ms-1" href="#!">${value.createdByName}</a></p>
                    </div>
                    <div class="fs-9">
                        <span class="fa-regular fa-calendar-days text-primary me-2"></span>
                        <span class="fw-semibold">${activityDate}</span>
                    </div>
                </div>
                ${value.phoneNumber
                ? value.phoneNumber.split(',').map(e =>
                    `<p class="fs-9 mb-0 position-relative" style="z-index:1;">${e.trim()}</p>`
                ).join("")
                : ''
            }

${value.emailAddress
                ? value.emailAddress.split(',').map(e =>
                    `<p class="fs-9 mb-0 position-relative" style="z-index:1;">${e.trim()}</p>`
                ).join("")
                : ''
}
                ${value.activityNote ? `<p class="fs-9 mb-0">${value.activityNote}</p>` : ''}
            </div>
        </div>
    `;

        container.appendChild(div);

        // Update vertical line height dynamically
        const line = container.querySelector('.vertical-line');
        line.style.height = container.scrollHeight + 'px';
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
    // update Lead Source value
    // ==============================

    $("#leadSource, #lead-status, #leadPriority, #probabilityPercentage").on("change", function () {

        let fieldValue = $(this).val();
        let fieldID = $(this).attr("id");
        let fieldName = fieldID === "leadSource" ? "source" : fieldID == "lead-status" ? "stage" : fieldID === 'leadPriority' ? "priority" : fieldID == 'probabilityPercentage' ? 'probability' : "";
        let leadID = $(ids.leadID).val();

        $.ajax({
            url: '/LeadDetails/UpdateLeadValue',
            method: 'POST',
            data: { LeadID: leadID, FieldName: fieldName, FieldValue: fieldValue },
            success: function (response) {
                if (response) {
                    toastr.success(`${fieldName} updated successfully`);
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
        loadedIds2.clear();
        $(upcomingListDiv).empty();
        updateUpcomingActivate(currentPage2);
    }


    // ==============================
    // validation load
    // ==============================
    function clearAllValidationBorders() {
        // Clear option buttons and container
        $(".option-btn").css("border", "");
        $("#optionBtnDiv").css("border", "");

        // Clear all input, textarea, select fields
        $("input, textarea, select").css("border", "");

        // Clear file inputs
        $("input[type='file']").css("border", "");

        // Clear error messages if you have spans
        $(".errorShow").text("");
    }




    // ==============================
    // Initial load
    // ==============================
    updateActivate(1, "reset");
    updateUpcomingActivate();   

    // ====================
    // Edit Button work
    // ======================
    //$(document).on("click", "#editModalBtn", function (e) {
    //    var myModal = new bootstrap.Modal(document.getElementById('editModal'), {
    //        keyboard: false,
    //        backdrop: 'static',
    //    });
    //    myModal.show();

    //    let leadID = $(ids.leadID).val();
    //    $.ajax({
    //        url: '/CRM/GetLeadInfo',
    //        method: 'POST',
    //        data: { id: leadID },
    //        success: function (response) {
    //            //updateEmployee();
    //            $("#leadID").val(response.leadID);
    //            $("#leadName").val(response.leadName);
    //            $("#leadStatusID").val(response.leadStatusID);
    //            $("#leadSourceID").val(response.leadSourceID);
    //            $("#leadPriorityID").val(response.priorityID);
    //            $("#approximateDealValue").val(response.approximateDealValue);
    //            $("#probabilityPercentage2").val(response.probability);
    //            $("#completionValue2").text(response.probability + "%");
    //            $("#descriptionText").val(response.leadDescription);
    //            $("#queryText").val(response.leadOwnerName);
    //            $("#selectedID").val(response.leadOwnerId);
    //            // multiselect edit field read
    //            $('#serviceTypes').val(response.serviceIds).each(function () {
    //                coreui.MultiSelect.getInstance(this)?.update();
    //            });

    //            // employee add
    //            const currentOwnerId = response.leadOwnerId;
    //            const currentOwnerName = response.leadOwnerName;
    //            if (currentOwnerId && currentOwnerName) {
    //                choices.setChoices(
    //                    [{ value: currentOwnerId, label: currentOwnerName, selected: true }],
    //                    'value',
    //                    'label',
    //                    false // false = append (don?t clear)
    //                );
    //            }
    //        },
    //        error: function (xhr) {
    //            toastr.error("Error creating lead");
    //        }
    //    });
    //});

    // ======================
    // employee
    // ======================
    // #region Choice with Pagination + Infinite Scroll (server-side search only)
    const selectEl = document.getElementById('leadOwnerId');
    let debounceTimer;
    let loading3 = false;
    let currentPage3 = 1;
    let lastSearch3 = '';
    let hasMore = true;

    //const choices = new Choices(selectEl, {
    //    searchEnabled: true,
    //    placeholder: true,
    //    placeholderValue: 'Select Organization...',
    //    searchPlaceholderValue: 'Type to search...',
    //    noChoicesText: 'Type 3 or more characters...',
    //    searchResultLimit: -1, // disable local limiting
    //    shouldSort: false,
    //    duplicateItemsAllowed: false,
    //    itemSelectText: '',
    //    removeItemButton: true,

    //    // ?? disable client-side filtering (server handles search)
    //    searchChoices: false,
    //    fuseOptions: false,
    //    searchFn: () => true
    //});

    // Fetch data from server
    async function fetchOptions(search, page = 1, pageSize = 50) {
        loading3 = true;
        try {
            const res = await fetch(`/CRM/SearchEmployee?search=${encodeURIComponent(search)}&page=${page}&pageSize=${pageSize}`);
            const data = await res.json();
            hasMore = data.hasMore;
            return data;
        } catch (error) {
            console.error("Error fetching organizations:", error);
            return { items: [], hasMore: false };
        } finally {
            loading3 = false;
        }
    }

    // Handle debounce on search
    //selectEl.addEventListener('search', function (e) {
    //    const searchTerm = e.detail.value;
    //    clearTimeout(debounceTimer);

    //    if (searchTerm.length < 1) {
    //        choices.clearChoices();
    //        return;
    //    }

    //    debounceTimer = setTimeout(async () => {
    //        currentPage3 = 1;
    //        lastSearch3 = searchTerm;
    //        const data = await fetchOptions(searchTerm, currentPage3);

    //        choices.clearChoices();
    //        if (data.items.length > 0) {
    //            // replace with new results
    //            choices.setChoices(data.items, 'value', 'label', true);
    //        }
    //    }, 500); // debounce delay
    //});

    // Scroll handler
    async function handleScroll(e) {
        const dropdownList = e.target;
        if (!loading3 && hasMore && dropdownList.scrollTop + dropdownList.clientHeight >= dropdownList.scrollHeight - 10) {
            currentPage3++;
            const data = await fetchOptions(lastSearch3, currentPage3);

            if (data.items.length > 0) {
                // append results, keep existing
                choices.setChoices(data.items, 'value', 'label', false);
            }
        }
    }

    //// Reattach scroll listener when dropdown opens
    //choices.passedElement.element.addEventListener('showDropdown', () => {
    //    const dropdownList = document.querySelector('.choices__list--dropdown .choices__list[role="listbox"]');
    //    if (dropdownList) {
    //        dropdownList.removeEventListener('scroll', handleScroll);
    //        dropdownList.addEventListener('scroll', handleScroll);
    //    }
    //});
    // #endregion

    // ==============================
    // update lead information
    // ==============================

    $("#editBtn").on("click", function (e) {
        e.preventDefault();
        const data = {
            LeadID: $("#leadID").val(),
            LeadName: $("#leadName").val() || "",
            LeadStatusID: parseInt($("#leadStatusID").val()) || 0,
            LeadSourceID: parseInt($("#leadSourceID").val()) || 0,
            LeadOwnerID: parseInt($("#leadOwnerId").val()) || 0,
            PriorityID: parseInt($("#leadPriorityID").val()) || 0,
            ApproximateDealValue: parseFloat($("#approximateDealValue").val()) || 0,
            ProbabilityPercentage: parseFloat($("#probabilityPercentage2").val()) || 0,
            LeadDescription: $("#descriptionText").val(),
            ServiceTypeIds: $("#serviceTypes").val(),
        };
        if (validation2()) {
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
        }
    })


    // ===============
    // lead validation2
    // =================

    function validation2() {
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

    // ==========================
    // option button on click clear all validation
    // ===========================
    $('.option-btn').on('click', function () {
        clearAllValidationBorders();
    })

    $(document).on("click", ids.restoreBtn,async function () {
 
            const confirmed = await customToaster.confirm("Do you want to restart this Lead?");
            if (confirmed) {
                let leadId = $(ids.leadID).val();
                $.ajax({
                    url: '/LeadDetails/RestoreLead',
                    method: 'POST',
                    data: { id: leadId },
                    success: function (response) {
                        if (response.success) {
                            customToaster.success(response.message);
                            resetAndReloadUpcoming();
                            resetAndReload();
                            makeEnableState();

                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function (xhr) {
                        toastr.error("Error restoring lead");
                    }
                });
            }
            else customToaster.error("Cancelled!");
        });


    // When you load modal via AJAX
    $(document).on("click", "#createCustomer", function () {
        $.get('/Customers/IndexModal', function (html) {
            $('#customerModalContent').html(html);

            // Initialize newly added modal elements
            $('#customerModalContent [data-init]').each(function () {
                const el = this;
                if (typeof showClose == "function") {
                    showClose();
                }
                if (typeof loadCustomerData == "function") {
                    const id = $("#CustomerId2").val();
                    loadCustomerData(id);
                }
                if (typeof loadCustomerData == "function") {
                    const cid = $("#CustomerId2").val();
                    const bid = $("#BranchId").val();
                    loadBranchData(bid, cid);
                }
                const key = el.dataset.init;
                if (key && typeof window[key] === "function") {
                    window[key](el);
                    el.dataset.initialized = true; // optional flag
                }
            });

            // Show modal
            var modal = new bootstrap.Modal(document.getElementById('customerModal'));
            modal.show();
        });
    });


    // keyboard shorcurt for note input field
    $("#aNote").on("keydown", function (e) {
        // Check if Ctrl + V is pressed
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault(); // Stop normal paste

            let textarea = this;
            let before = textarea.value.substring(0, textarea.selectionStart);
            let after = textarea.value.substring(textarea.selectionEnd);

            // Insert text at cursor position
            textarea.value = before + "Communication start from phone." + after;
        }
    });


    // OPEN FIRST MODAL (Create Lead)
    $(document).on("click", "#openCreateLeadModal", function () {

        $.get('/CreateLead/IndexModal', function (html) {

            $('.create-lead-modal-body').html(html);

            // Load script if needed
            $.getScript('/js/pages/crm/createlead_modal.js')
                .done(() => {
                    if (typeof initCreateLeadModal === "function") {
                        initCreateLeadModal();
                    }
                });

            const modalEl = document.getElementById('createLeadModalToggle');
            modalEl.setAttribute("data-bs-backdrop", "static");
            modalEl.setAttribute("data-bs-keyboard", "false");

            // Now open modal
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });
    });


    // OPEN SECOND MODAL (Customer) from inside first modal
    $(document).on("click", "#openCustomerModal", function (e) {
        e.preventDefault();

        const firstModalEl = document.getElementById('createLeadModalToggle');
        const firstModal = bootstrap.Modal.getOrCreateInstance(firstModalEl);

        // Load customer modal content
        $.get('/Customers/IndexModal', function (html) {

            $('.customer-modal-content').html(html);

            // Load script if needed
            if (typeof initCustomerModal !== 'function') {
                $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                    .done(() => initCustomerModal && initCustomerModal());
            } else {
                initCustomerModal();
            }

            // Make first modal non-interactive but NOT aria-hidden
            firstModalEl.setAttribute("inert", "");

            // Show second modal on top
            const secondModal = bootstrap.Modal.getOrCreateInstance('#openCustomerModalToggle', {
                backdrop: 'static',
                focus: true,
                keyboard: false
            });

            secondModal.show();

            // When second modal closes ? restore first modal
            $('#openCustomerModalToggle').one('hidden.bs.modal', function () {
                firstModalEl.removeAttribute("inert");
                firstModal.show();
            });

        });

        // Hide first modal visually now
        firstModal.hide();
    });

});

window.closeWindow = function () {
    debugger;
    const modalEl = document.getElementById('customerModal');
    const modal = bootstrap.Modal.getInstance(modalEl); // get existing instance
    if (modal) modal.hide();
};