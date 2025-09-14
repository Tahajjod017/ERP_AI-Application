


$(function () {
    const ids = {
        note : "#aNote",
        date : '#aDate',
        file: '#aFile',
        leadID: '#leadID',
        addActiveBtn: '#addLActivity',
        wonConfirmDiv: '#won-fonfirm-div',

    }
    // ==============================
    // Active option buttons
    // ==============================
    $(".option-btn").on('click', function () {
        debugger;
        showDev("clicked")
        $(".option-btn").removeClass('active');
        $(this).addClass('active');

        let btnText = $(this).text().trim();
        if (btnText === "Attachment") {
            $('#file-field').show();

            $(ids.addActiveBtn).removeClass('d-none');
            $(ids.addActiveBtn).removeAttr('disabled');
            $(ids.wonConfirmDiv).addClass('d-none');
        } else if (btnText === "Won") {
            let isWonAlready = $(this).data('confirm');
            showDev(isWonAlready);
            if (isWonAlready !== 'yes') {
                $('#file-field').hide();
                $('#dateField').hide();
                $(ids.addActiveBtn).addClass('d-none');
                $(ids.addActiveBtn).prop('disabled', true);
                $(ids.wonConfirmDiv).removeClass('d-none');
            }
            
        }else if (btnText === "Lost") {
            $('#file-field').hide();
            $('#dateField').hide();
            $('#note-Field').show();

            $(ids.addActiveBtn).removeClass('d-none');
            $(ids.addActiveBtn).removeAttr('disabled');
            $(ids.wonConfirmDiv).addClass('d-none');
        }else {
            $('#file-field').hide();
            $('#dateField').show();
            $('#note-Field').show();

            $(ids.addActiveBtn).removeClass('d-none');
            $(ids.addActiveBtn).removeAttr('disabled');
            $(ids.wonConfirmDiv).addClass('d-none');
        }
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

    //==========================
    // save lead activity function
    //==============================
    async function saveActivityFunction() {
        const buttonName = $(".option-btn.active").text().trim();
        const buttonID = $(".option-btn.active").data('id');
        const leadID = $(ids.leadID).val();
        const note = $(ids.note).val();
        const date = $(ids.date).val();
        const fileInput = $(ids.file)[0];
        const file = fileInput ? fileInput.files[0] : null;

        // run validation
        if (!validation(buttonName)) return;

        let isWonOrLost = $('.special-btn').hasClass('active2');
        // Show confirmation modal if Won/Lost
        if (isWonOrLost) {
            const confirmed = await showConfirmationModal();
            if (!confirmed) return; // user clicked No, stop execution
        }

        const formData = new FormData();
        formData.append("LeadID", parseInt(leadID));
        formData.append("LeadActivityTypeID", parseInt(buttonID));
        formData.append("ActivityNote", note || "");
        formData.append("ActivityTypeName", buttonName);

        if (buttonName !== "Won" && buttonName !== "Lost") {
            const convertedDate = convertToISODateTime(date);
            formData.append("ActivityDateTime", convertedDate);
            if (file) formData.append("File", file);
        }

        $.ajax({
            url: '/LeadDetails/SaveLeadActivity',
            method: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message);
                    location.reload();
                } else {
                    toastr.error(response.message);
                }
                resetAndReload();
                resetAndReloadUpcoming();
                $(".option-btn").removeClass("active");
                $(ids.date).val("");
                $(ids.note).val("");
                $(ids.file).val("");
                $('#file-field').hide();

                if (buttonName === "Won") {
                    $("#transferDiv").css("display", "block");
                } else if (buttonName === "Lost") {
                    $("#transferDiv").css("display", "none");
                }
            },
            error: function (error) {
                toastr.error(error.responseJSON?.message || "Error saving lead activity");
            }
        });
    }
    // Helper: show Bootstrap 5.1 modal and return a Promise
    function showConfirmationModal() {
        return new Promise((resolve) => {
            const modalEl = document.getElementById('confirmModal');
            const bsModal = new bootstrap.Modal(modalEl);
            bsModal.show();

            // Yes button
            $('#modalYesBtn').off('click').one('click', () => {
                bsModal.hide();
                resolve(true);
            });

            // No button
            $('#modalNoBtn').off('click').one('click', () => {
                bsModal.hide();
                resolve(false);
            });
        });
    }

    // ==================
    // save activity
    // =================
    $('#addLActivity').on('click', function (e) {
        e.preventDefault();
        saveActivityFunction(this);
    });

    //============================
    // yes no Won button work
    // ============================
    $('#wonYes, #wonNo').on('click', function () {
        showDev("yes no clicked");
        saveActivityFunction(this);
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
                    $("#upcomming-div").addClass("d-none");
                    return;
                   
                } else {
                    //$("#myTab").css("display", "block");
                    $("#upcomming-div").removeClass("d-none");
                }
                showDev(response)
                response.forEach(item => {
                    if (!item.leadDetailID) return;
                    if (!loadedIds2.has(item.leadDetailID)) {
                        loadedIds2.add(item.leadDetailID);
                        const activityDate = new Date(item.activityDateTime).toLocaleString('en-GB', options);
                        if (item.leadActivityName === 'Attachment') {
                            $('#upcoming-activity').append(renderAttachmentActivity(item, activityDate));
                        } else {
                            $('#upcoming-activity').append(renderActivity(item, activityDate));
                        }
                       
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
                        ${value.activityNote ? `<p class="fs-9 mb-0">${value.activityNote}</p>` : ''}
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
        showDev(fieldID);
        let fieldName = fieldID === "leadSource" ? "source" : fieldID == "lead-status" ? "stage" : fieldID === 'leadPriority' ? "priority" :  fieldID == 'probabilityPercentage' ? 'probability': "";
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
        loadedIds2.clear()
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
    $(document).on("click", "#editModalBtn", function (e) {
        var myModal = new bootstrap.Modal(document.getElementById('editModal'), {
            keyboard: false
        });
        myModal.show();

        let leadID = $(ids.leadID).val();
        $.ajax({
            url: '/CRM/GetLeadInfo',
            method: 'POST',
            data: { id: leadID },
            success: function (response) {
                //showDev(response)
                //updateEmployee();
                $("#leadID").val(response.leadID);
                $("#leadName").val(response.leadName);
                $("#leadStatusID").val(response.leadStatusID);
                $("#leadSourceID").val(response.leadSourceID);
                $("#leadPriorityID").val(response.priorityID);
                $("#approximateDealValue").val(response.approximateDealValue);
                $("#probabilityPercentage2").val(response.probability);
                $("#completionValue2").text(response.probability + "%");
                $("#descriptionText").val(response.leadDescription);
                $("#queryText").val(response.leadOwnerName);
                $("#selectedID").val(response.leadOwnerId);
                // multiselect edit field read
                $('#serviceTypes').val(response.serviceIds).each(function () {
                    coreui.MultiSelect.getInstance(this)?.update();
                });
             
                // employee add
                const currentOwnerId = response.leadOwnerId;
                const currentOwnerName = response.leadOwnerName;
                showDev(currentOwnerId)
                showDev(currentOwnerName)
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

        // ?? disable client-side filtering (server handles search)
        searchChoices: false,
        fuseOptions: false,
        searchFn: () => true
    });

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
    selectEl.addEventListener('search', function (e) {
        const searchTerm = e.detail.value;
        clearTimeout(debounceTimer);

        if (searchTerm.length < 1) {
            choices.clearChoices();
            return;
        }

        debounceTimer = setTimeout(async () => {
            currentPage3 = 1;
            lastSearch3 = searchTerm;
            const data = await fetchOptions(searchTerm, currentPage3);

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
        if (!loading3 && hasMore && dropdownList.scrollTop + dropdownList.clientHeight >= dropdownList.scrollHeight - 10) {
            currentPage3++;
            const data = await fetchOptions(lastSearch3, currentPage3);

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
        //showDev(data);
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
    // option button on click
    // ===========================
    $('.option-btn').on('click', function () {
        clearAllValidationBorders();
        showDev('called');
    })
});
