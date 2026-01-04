$(function () {
    const ids = {
        note: "#aNote",
        date: '#aDate',
        file: '#aFile',
        contactNumber: "#ContectPersonId",
        contactEmail: "#ContectPersonEmailId",
        leadID: '#leadID2',
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
    // global variable
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
                return { leadId: $(ids.leadID).val(), search: params.term || '', page: params.page || 1 };
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
                return { leadId: $(ids.leadID).val(), search: params.term || '', page: params.page || 1 };
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
    $('#wonNo').on('click', function () {
        saveActivityFunction(this);
        secialButtonState = "won";
    });
    $('#wonYes').on('click', function () {
        debugger;
        //open modal
        $("#customerModalActionName").text("Create")


        $.get('/CreateJobs/IndexModal', function (html) {
            $('.create-job-modal-body').html(html);
            // Load script if needed
            $.getScript('/js/pages/FieldServices/CreateJob.js')
                .done(() => {
                    debugger;
                    if (typeof initCreateJobModal === "function") {
                        initCreateJobModal();
                    }
                    debugger;
                    const modalEl = document.getElementById('createJobModalToggle');
                    modalEl.setAttribute("data-bs-backdrop", "static");
                    modalEl.setAttribute("data-bs-keyboard", "false");
                    // Now open modal
                    bootstrap.Modal.getOrCreateInstance(modalEl).show();
                    const customerId2 = $('#CustomerId2').val();
                    if (typeof LoadMainPageData === "function") {
                        LoadMainPageData(customerId2);
                    }
                });


        });
        //saveActivityFunction(this);
        //secialButtonState = "won";
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
            const id = $("#leadID2").val();
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
                                renderActivity(item, activityDate, 'activity-list');
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

    //==============================
    // hide closed Date Div
    //================================
    function hideClosedDate() {
        $(ids.closingDateDiv).addClass('d-none');
        $(ids.closingDateResult).text("");
    }

    // =============================
    // make disabled state
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
        const id = $("#leadID2").val();
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
                    return;
                } else {
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
                            renderActivity(item, activityDate, 'upcoming-activity');
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
    // Render Activity - PHOENIX DESIGN
    // ==============================
    function renderActivity(value, activityDate, containerId) {
        const container = document.getElementById(containerId);
        if (!container) return;

        const isUpcoming = containerId === 'upcoming-activity';
        const activityType = value.leadActivityName || 'Note';
        const iconClass = getTimelineIconClass(activityType);
        const badgeClass = getTimelineBadgeClass(activityType);
        const pulseClass = isUpcoming ? 'timeline-icon-pulse' : '';

        // Build phone numbers HTML
        let phoneHtml = '';
        if (value.phoneNumber && value.phoneNumber.trim() !== "") {
            const phones = value.phoneNumber.split(',').map(p => p.trim()).filter(Boolean);
            if (phones.length > 0) {
                phoneHtml = phones.map(phone =>
                    `<span class="timeline-meta-item">
                        <i class="fa fa-phone"></i>
                        <span>${phone}</span>
                    </span>`
                ).join('');
            }
        }

        // Build email HTML
        let emailHtml = '';
        if (value.emailAddress && value.emailAddress.trim() !== "") {
            const emails = value.emailAddress.split(',').map(e => e.trim()).filter(Boolean);
            if (emails.length > 0) {
                emailHtml = emails.map(email =>
                    `<span class="timeline-meta-item">
                        <i class="fa fa-envelope"></i>
                        <span>${email}</span>
                    </span>`
                ).join('');
            }
        }
        let taskStatus = value.isDone === true ? "Completed" : "Incomplete";
        // Build description with show more/less
        let descriptionHtml = '';
        if (value.activityNote && value.activityNote.trim() !== "") {
            const noteLength = value.activityNote.length;
            const shouldCollapse = noteLength > 150;

            descriptionHtml = `
                <div class="timeline-activity-description">
                    <div class="timeline-description-text ${shouldCollapse ? 'collapsed' : ''}" 
                         data-activity-id="${value.leadDetailID}">
                        <p>${escapeHtml(value.activityNote)}</p>
                    </div>
                    ${shouldCollapse ? `
                        <button class="timeline-show-more-btn" 
                                onclick="toggleActivityDescription(${value.leadDetailID})">
                            <span>Read More</span>
                            <i class="fa fa-chevron-down"></i>
                        </button>
                    ` : ''}
                </div>
            `;
        }

        const activityHtml = `
            <div class="timeline-activity-item" data-activity-id="${value.leadDetailID}">
                <div class="timeline-activity-icon ${iconClass} ${pulseClass}">
                    <i class="fa ${getActivityIcon(activityType)}"></i>
                </div>
                <div class="timeline-activity-content">
                    <div class="timeline-activity-header">
                        <h5 class="timeline-activity-title">${escapeHtml(activityType)}</h5>
                        <span class="timeline-activity-badge ${badgeClass}">${activityType}</span>
                    </div>
                    <div class="timeline-activity-meta">
                        <span class="timeline-meta-item">
                            <i class="fa fa-calendar"></i>
                            <span>${activityDate}</span>
                        </span>
                        <span class="timeline-meta-item">
                            <i class="fa fa-user"></i>
                            <span>${escapeHtml(value.createdByName || 'Unknown')}</span>
                        </span>
                        ${phoneHtml}
                        ${emailHtml}
                    </div>
                    ${descriptionHtml}
                    <div class="timeline-activity-footer">
                        <span class="timeline-activity-status ${isUpcoming ? 'timeline-status-upcoming' : value.isDone ? 'timeline-status-completed' : 'timeline-status-incompleted'}">
                            <i class="fa ${isUpcoming ? 'fa-regular fa-clock' : 'fa-check-circle'}"></i>
                            ${isUpcoming ? 'Upcoming' : taskStatus}
                        </span>
                        <div class="timeline-activity-actions">
                            ${isUpcoming ? `
                                <button class="timeline-action-btn" onclick="editActivity(${value.leadDetailID})">
                                    <i class="fa fa-pencil"></i> Comment
                                </button>
                                <button class="timeline-action-btn" onclick="noResponseActivity(${value.leadDetailID})">
                                    <i class="fa fa-pencil"></i> No Response
                                </button>
                                <button class="timeline-action-btn" onclick="editActivity(${value.leadDetailID})">
                                    <i class="fa fa-pencil"></i> Edit
                                </button>
                                <button class="timeline-action-btn" onclick="completeActivity(${value.leadDetailID})">
                                    <i class="fa fa-check"></i> Complete
                                </button>
                            ` : `
                                <button class="timeline-action-btn" onclick="viewActivity(${value.leadDetailID})">
                                    <i class="fa fa-eye"></i> View
                                </button>
                            `}
                        </div>
                    </div>
                </div>
            </div>
        `;

        container.insertAdjacentHTML('beforeend', activityHtml);
    }

    // ==============================
    // Helper Functions for Timeline
    // ==============================

    // Helper function for activity icons (FA 4.7)
    function getActivityIcon(activityType) {
        const iconMap = {
            'Call': 'fa-phone',
            'Meeting': 'fa-users',
            'Offline Meeting': 'fa-users',
            'Online Meeting': 'fa-video-camera',
            'Email': 'fa-envelope',
            'Task': 'fa-tasks',
            'Note': 'fa-sticky-note',
            'Attachment': 'fa-paperclip',
            'Quatation': 'fa-file-text',
            'Rev. Quatation': 'fa-file-text-o',
            'Won': 'fa-trophy',
            'Lost': 'fa-times-circle'
        };
        return iconMap[activityType] || 'fa-circle';
    }

    // Helper function for icon color classes
    function getTimelineIconClass(activityType) {
        const iconClassMap = {
            'Call': 'timeline-icon-call',
            'Meeting': 'timeline-icon-meeting',
            'Offline Meeting': 'timeline-icon-meeting',
            'Online Meeting': 'timeline-icon-meeting',
            'Email': 'timeline-icon-email',
            'Task': 'timeline-icon-task',
            'Note': 'timeline-icon-note',
            'Attachment': 'timeline-icon-note',
            'Quatation': 'timeline-icon-task',
            'Rev. Quatation': 'timeline-icon-task',
            'Won': 'timeline-icon-success',
            'Lost': 'timeline-icon-lost',
            'Cancel': 'timeline-icon-cancel'
        };
        return iconClassMap[activityType] || 'timeline-icon-default';
    }

    // Helper function for badge classes
    function getTimelineBadgeClass(activityType) {
        const badgeClassMap = {
            'Call': 'timeline-badge-call',
            'Meeting': 'timeline-badge-meeting',
            'Offline Meeting': 'timeline-badge-meeting',
            'Online Meeting': 'timeline-badge-meeting',
            'Email': 'timeline-badge-email',
            'Task': 'timeline-badge-task',
            'Note': 'timeline-badge-note',
            'Attachment': 'timeline-badge-note',
            'Quatation': 'timeline-badge-task',
            'Rev. Quatation': 'timeline-badge-task',
            'Won': 'timeline-badge-success',
            'Lost': 'timeline-badge-lost',
            'Cancel': 'timeline-badge-cancel'
        };
        return badgeClassMap[activityType] || 'timeline-badge-default';
    }

    // Helper function to escape HTML
    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    // Toggle description expand/collapse
    function toggleActivityDescription(activityId) {
        const description = document.querySelector(`.timeline-description-text[data-activity-id="${activityId}"]`);
        const button = description.parentElement.querySelector('.timeline-show-more-btn');

        if (description && button) {
            description.classList.toggle('collapsed');
            const isCollapsed = description.classList.contains('collapsed');

            button.querySelector('span').textContent = isCollapsed ? 'Read More' : 'Read Less';
            button.querySelector('i').className = isCollapsed ? 'fa fa-chevron-down' : 'fa fa-chevron-up';
        }
    }

    // ==============================
    // Render Attachment Activity (if needed)
    // ==============================
    function renderAttachmentActivity(value, activityDate) {
        // Add your attachment rendering logic here if needed
        return '';
    }

    // ==============================
    // Preview File Function
    // ==============================
    function previewFile(fileUrl) {
        let ext = fileUrl.split('.').pop().toLowerCase();
        let container = document.getElementById("filePreviewContainer");
        container.innerHTML = "";

        if (["jpg", "jpeg", "png", "gif", "bmp", "webp"].includes(ext)) {
            container.innerHTML = `<img src="${fileUrl}" class="img-fluid" alt="preview">`;
        }
        else if (ext === "pdf") {
            container.innerHTML = `<iframe src="${fileUrl}" style="width:100%;height:500px" frameborder="0"></iframe>`;
        }
        else {
            window.open(fileUrl, "_blank");
            return;
        }

        let modal = new bootstrap.Modal(document.getElementById('filePreviewModal'));
        modal.show();
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
        $(".option-btn").css("border", "");
        $("#optionBtnDiv").css("border", "");
        $("input, textarea, select").css("border", "");
        $("input[type='file']").css("border", "");
        $(".errorShow").text("");
    }

    // ==============================
    // Initial load
    // ==============================
    updateActivate(1, "reset");
    updateUpcomingActivate();

    //#region Edit Modal Button
    $(document).on("click", "#editModalBtn", function () {
        $("#leadModalTitle").text("Edit");
        const leadId = $("#leadID2").val();
        const firstModalEl = document.getElementById('createLeadModalToggle');
        const firstModal = bootstrap.Modal.getOrCreateInstance(firstModalEl);
        firstModal.hide();

        $.get('/CreateLead/IndexModal', function (html) {
            $('.create-lead-modal-body').html(html);

            $.getScript('/js/pages/crm/createlead_modal.js')
                .done(function () {
                    if (typeof initCreateLeadModal === "function") {
                        initCreateLeadModal();
                    }

                    const modalEl = document.getElementById('createLeadModalToggle');
                    modalEl.setAttribute("data-bs-backdrop", "static");
                    modalEl.setAttribute("data-bs-keyboard", "false");

                    bootstrap.Modal.getOrCreateInstance(modalEl).show();

                    if (typeof loadEditData === "function") {
                        loadEditData(leadId);
                    }
                });
        });
    });
    //#endregion

    // ======================
    // employee
    // ======================
    const selectEl = document.getElementById('leadOwnerId');
    let debounceTimer;
    let loading3 = false;
    let currentPage3 = 1;
    let lastSearch3 = '';
    let hasMore = true;

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

    async function handleScroll(e) {
        const dropdownList = e.target;
        if (!loading3 && hasMore && dropdownList.scrollTop + dropdownList.clientHeight >= dropdownList.scrollHeight - 10) {
            currentPage3++;
            const data = await fetchOptions(lastSearch3, currentPage3);

            if (data.items.length > 0) {
                choices.setChoices(data.items, 'value', 'label', false);
            }
        }
    }

    // ==============================
    // update lead information
    // ==============================
    $("#editBtn").on("click", function (e) {
        e.preventDefault();
        const data = {
            LeadID: $("#leadID2").val(),
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

            if (el.closest('.choices').length > 0) {
                target = el.closest('.choices').find('.choices__inner');
            }

            if (value === '' || value === null) {
                target.css('border', '1px solid red');
                isValid = false;
            } else {
                target.css('border', '1px solid #ccc');
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

    // ==========================
    // Restore Lead
    // ===========================
    $(document).on("click", ids.restoreBtn, async function () {
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

    // keyboard shortcut for note input field
    $("#aNote").on("keydown", function (e) {
        if (e.ctrlKey && e.key === "s") {
            e.preventDefault();
            let textarea = this;
            let before = textarea.value.substring(0, textarea.selectionStart);
            let after = textarea.value.substring(textarea.selectionEnd);
            textarea.value = before + "Communication start from phone." + after;
        }
    });

    // OPEN FIRST MODAL (Create Lead)
    $(document).on("click", "#openCreateLeadModal", function () {
        $("#leadModalTitle").text("Create");
        $.get('/CreateLead/IndexModal', function (html) {
            $('.create-lead-modal-body').html(html);

            $.getScript('/js/pages/crm/createlead_modal.js')
                .done(() => {
                    if (typeof initCreateLeadModal === "function") {
                        initCreateLeadModal();
                    }
                });

            const modalEl = document.getElementById('createLeadModalToggle');
            const modalInstance = bootstrap.Modal.getOrCreateInstance(modalEl, {
                backdrop: 'static',
                keyboard: false
            });
            modalInstance.show();
        });
    });

    // OPEN SECOND MODAL (Customer) from inside first modal
    $(document).on("click", "#openCustomerModal", function (e) {
        e.preventDefault();
        $("#customerModalActionName").text("Create")
        const firstModalEl = document.getElementById('createLeadModalToggle');
        const firstModal = bootstrap.Modal.getOrCreateInstance(firstModalEl);

        $.get('/Customers/IndexModal', function (html) {
            $('.customer-modal-content').html(html);

            if (typeof initCustomerModal !== 'function') {
                $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                    .done(() => initCustomerModal && initCustomerModal());
            } else {
                initCustomerModal();
            }

            const secondModal = bootstrap.Modal.getOrCreateInstance('#openCustomerModalToggle', {
                backdrop: 'static',
                focus: true,
                keyboard: false
            });

            secondModal.show();

            $('#openCustomerModalToggle').one('hidden.bs.modal', function () {
                firstModalEl.removeAttribute("inert");
                firstModal.show();
            });
        });

        firstModal.hide();
    });

    //#region Modal Edit Customer Button
    $(document).on("click", "#editCustomerBtn2", function (e) {
        e.preventDefault();
        $("#customerModalActionName").text("Edit")

        $.get('/Customers/IndexModal', function (html) {
            $('.customer-modal-content').html(html);

            if (typeof initCustomerModal !== 'function') {
                $.getScript('/js/pages/CRM/Customer/customer.bundle.js')
                    .done(() => initCustomerModal && initCustomerModal());
            } else {
                initCustomerModal();
            }

            const id = $('#CustomerId2').val();
            loadCustomerData(id);

            const secondModal = bootstrap.Modal.getOrCreateInstance('#openCustomerModalToggle', {
                backdrop: 'static',
                focus: true,
                keyboard: false
            });

            secondModal.show();
        });
    });
    //#endregion

    // ==============================
    // Make Functions Globally Accessible
    // ==============================
    window.toggleActivityDescription = toggleActivityDescription;
    window.previewFile = previewFile;
    window.editActivity = editActivity;
    window.noResponseActivity = noResponseActivity;
    window.completeActivity = completeActivity;
    window.viewActivity = viewActivity;

    // ==============================
    // Activity Action Functions (Add your logic)
    // ==============================
    function editActivity(activityId) {
        $.ajax({
            url: '/LeadDetails/GetActivityInfo',
            type: 'GET',
            data: { detailsId: activityId },
            success: function (res) {
                console.log(res);
                debugger;
                if (!res.success || !res.data || res.data.length === 0) {
                    toastr.error("No activity data found.");
                    return;
                }

                let d = res.data[0];

                const activityName = d.leadActivityName?.trim();
                const activityNote = d.activityNote || "";
                const emailAddress = d.emailAddress || "";
                const phoneNumber = d.phoneNumber || "";
                const fileLink = d.fileLink || null;
                const date = d.activityDateTime || "";

                // Step 1: Activate the correct activity type button
                const $targetBtn = $(`.option-btn[data-usefor="${activityName}"]`);
                if ($targetBtn.length === 0) {
                    toastr.error("Activity type not supported for editing.");
                    return;
                }
                $targetBtn.trigger('click');

                // Step 2: Fill simple fields
              
                $(ids.note).val(activityNote);

                // date time load
                if (date && date.trim() !== "") {
                    const backendDate = new Date(date);

                    if (!isNaN(backendDate.getTime())) {
                        // Format as DD/MM/YYYY HH:MM (AM/PM) – matches most Flatpickr setups
                        const day = String(backendDate.getDate()).padStart(2, '0');
                        const month = String(backendDate.getMonth() + 1).padStart(2, '0');
                        const year = backendDate.getFullYear();
                        const hours = backendDate.getHours();
                        const mins = String(backendDate.getMinutes()).padStart(2, '0');
                        const ampm = hours >= 12 ? 'PM' : 'AM';
                        const displayHours = hours % 12 || 12;

                        const formattedDate = `${day}/${month}/${year} ${displayHours}:${mins} ${ampm}`;

                        $(ids.date).val(formattedDate);

                        // Important: If using Flatpickr, force update
                        const fpInstance = $(ids.date)[0]._flatpickr;
                        if (fpInstance) {
                            fpInstance.setDate(formattedDate, true);
                        }
                    } else {
                        console.warn("Invalid date from backend:", date);
                    }
                }

                // Handle file preview
                if (fileLink) {
                    const fileName = fileLink.split('/').pop().split('?')[0];
                    const filePreviewHtml = `
                    <div class="mt-2 p-2 border rounded bg-light existing-file-preview">
                        <small>Current file: 
                            <a href="${fileLink}" target="_blank">${fileName}</a>
                        </small><br>
                        <small class="text-muted">Upload new file to replace</small>
                    </div>`;
                    $('#file-field .existing-file-preview').remove(); // clear old
                    $('#file-field').append(filePreviewHtml);
                    $('#file-field').data('existing-file', fileLink);
                } else {
                    $('#file-field .existing-file-preview').remove();
                    $('#file-field').removeData('existing-file');
                }

                // Step 3: Handle Select2 AJAX fields (Contact Phone - multiple)
                const phoneSelect = $('#ContectPersonId');
                const emailSelect = $('#ContectPersonEmailId');

                // Clear previous selections
                phoneSelect.val(null).trigger('change');
                emailSelect.val(null).trigger('change');

                if (phoneNumber) {
                    const phoneIds = phoneNumber.split(',').map(p => p.trim()).filter(Boolean);

                    // For each ID, we need to fetch its text/label via the same AJAX endpoint
                    let phoneLoaded = 0;
                    phoneIds.forEach(id => {
                        $.ajax({
                            url: "/LeadDetails/GetContactNumberList",
                            data: { leadId: $(ids.leadID).val(), search: id }, // search by exact ID to get single
                            dataType: 'json',
                            success: function (data) {
                                console.log(data);
                                if (data.items && data.items.length > 0) {
                                    const item = data.items[0];
                                    const option = new Option(item.label, item.value, true, true);
                                    phoneSelect.append(option).trigger('change');
                                }
                                phoneLoaded++;
                                if (phoneLoaded === phoneIds.length) {
                                    phoneSelect.trigger('change'); // final refresh
                                }
                            }
                        });
                    });
                }

                // Step 4: Handle Email (seems single select from your code)
                if (emailAddress) {
                    $.ajax({
                        url: "/LeadDetails/GetContactEmailList",
                        data: { leadId: $(ids.leadID).val(), search: emailAddress },
                        dataType: 'json',
                        success: function (data) {
                            if (data.items && data.items.length > 0) {
                                const item = data.items[0];
                                const option = new Option(item.label, item.value, true, true);
                                emailSelect.append(option).trigger('change');
                            }
                        }
                    });
                }

                // Step 5: Switch button to Update mode
                $('#addLActivity')
                    .text('Update Activity')
                    .removeClass('btn-primary')
                    .addClass('btn-success')
                    .data('edit-mode', true)
                    .data('edit-activity-id', activityId);

                toastr.success("Activity loaded for editing. Modify and click 'Update Activity'.");

                // Scroll to form
                $('html, body').animate({
                    //scrollTop: $("#activity-form-section").offset().top - 100
                }, 500);
            },
            error: function (err) {
                console.error(err);
                toastr.error("Failed to load activity for editing.");
            }
        });
    }
    async function noResponseActivity(activityId) {
        debugger
        const confirmed = await customToaster.confirm("Do you want to confirm 'No Response' Button?");
            if (confirmed) customToaster.success("Confirmed! Next process...");
            else customToaster.error("Cancelled!");
        
        $.ajax({
            url: '/LeadDetails/NoResponse',
            type: 'GET',
            data: { detailsId: activityId },
            success: function (res) {
                
            },
            error: function (err) {
                console.error(err);
                toastr.error("Failed to load activity for editing.");
            }
        });
    }
    //function editActivity(activityId) {
    //    console.log('Edit activity:', activityId);
    //    // Add your edit logic here
    //    toastr.info('Edit functionality - ID: ' + activityId);
    //    $.ajax({
    //        url: '/LeadDetails/GetActivityInfo', // Controller/Action
    //        type: 'GET',
    //        data: { detailsId: activityId },
    //        success: function (res) {

    //            if (!res.success) {
    //                alert("No data found");
    //                return;
    //            }

    //            if (res.data && res.data.length > 0) {
    //                let d = res.data[0]; // because data is List<>

    //                $("#LeadActivityName").text(d.leadActivityName);
    //                $("#ActivityNote").text(d.activityNote);
    //                $("#EmailAddress").text(d.emailAddress);
    //                $("#PhoneNumber").text(d.phoneNumber);

    //                if (d.fileLink) {
    //                    $("#FileLink")
    //                        .attr("href", d.fileLink)
    //                        .show();
    //                } else {
    //                    $("#FileLink").hide();
    //                }
    //            }
    //        },
    //        error: function (err) {
    //            console.error(err);
    //            alert("Something went wrong!");
    //        }
    //    });
    //}

    //function completeActivity(activityId) {
    //    console.log('Complete activity:', activityId);
    //    // Add your complete logic here
    //    toastr.info('Complete functionality - ID: ' + activityId);
    //}

    function viewActivity(activityId) {
        console.log('View activity:', activityId);
        // Add your view logic here
        toastr.info('View functionality - ID: ' + activityId);
    }

    function completeActivity(activityId) {
        $.ajax({
            url: '/LeadDetails/Complete',
            method: 'POST',
            data: { LeadDetailID: activityId },
            success: function (response) {
                debugger
                if (response.success) {
                    customToaster.success(response.message);

                    resetAndReload();
                    resetAndReloadUpcoming();

                } else {
                    toastr.error(response.message || "Failed to complete activity");
                }
            },
            error: function () {
                toastr.error("An error occurred while completing the activity");
            }
        });
    }
    

});

// Close Window Function
window.closeWindow = function () {
    const modalEl = document.getElementById('customerModal');
    const modal = bootstrap.Modal.getInstance(modalEl);
    if (modal) modal.hide();
};

function rescheduleModal(taksName) {

}