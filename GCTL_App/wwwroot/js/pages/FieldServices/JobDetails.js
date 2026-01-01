$(function () {
    const jobId = window.location.pathname.split("/").pop();
    const steps = $(".timeline-step");
    let selectedStep = 0;
    let isJobRunning = false; // Track if job is currently running
    const modal = new bootstrap.Modal(document.getElementById("activityModal"));

    // ========== AVATAR HELPER FUNCTIONS ==========

    function getInitials(name) {
        if (!name) return '?';
        const parts = name.trim().split(' ');
        if (parts.length >= 2) {
            return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
        }
        return name.substring(0, 2).toUpperCase();
    }

    function createAvatar(name, imageUrl, size = 'medium') {
        const sizeClass = size === 'large' ? 'customer-avatar' :
            size === 'small' ? 'team-avatar' : 'user-avatar';
        const placeholderClass = size === 'large' ? 'customer-avatar-placeholder' :
            size === 'small' ? 'team-avatar-placeholder' : 'user-avatar-placeholder';

        if (imageUrl && imageUrl !== '' && imageUrl !== null && imageUrl !== 'null') {
            return `
                <div class="${sizeClass}">
                    <img src="${imageUrl}" alt="${name}" 
                         onerror="this.parentElement.innerHTML='<div class=\\'${placeholderClass}\\'>${getInitials(name)}</div>';">
                </div>
            `;
        } else {
            return `
                <div class="${sizeClass}">
                    <div class="${placeholderClass}">${getInitials(name)}</div>
                </div>
            `;
        }
    }

    function loadTeamMembers(teamMembers) {
        const container = $('#teamMembers');

        if (!teamMembers || teamMembers.length === 0) {
            container.html('<span class="text-muted">No team members assigned</span>');
            return;
        }

        container.empty();

        teamMembers.forEach(function (member) {
            const avatar = createAvatar(
                member.name || member.fullName || 'Member',
                member.imageUrl || member.image || member.avatar || null,
                'small'
            );

            const memberElement = $(`
                <div class="d-inline-block position-relative me-2 mb-2" 
                     title="${member.name || member.fullName || 'Team Member'}" 
                     data-bs-toggle="tooltip">
                    ${avatar}
                </div>
            `);

            container.append(memberElement);
        });

        const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }

    // ========== TIMELINE FUNCTIONS ==========

    function createLines() {
        $(".timeline-line").remove();

        steps.each(function (index) {
            if (index === 0) return;

            const prevIcon = $(steps[index - 1]).find(".step-circle");
            const currIcon = $(this).find(".step-circle");
            const line = $('<div class="timeline-line"></div>');

            $("#timelineContainer").append(line);

            const containerLeft = $("#timelineContainer").offset().left;
            const startX = prevIcon.offset().left + prevIcon.outerWidth() / 2 - containerLeft;
            const endX = currIcon.offset().left + currIcon.outerWidth() / 2 - containerLeft;
            const width = endX - startX;

            line.css({
                left: startX + "px",
                width: width + "px"
            });

            if (prevIcon.hasClass("completed") && currIcon.hasClass("completed")) {
                line.addClass("completed");
            }
        });
    }

    function updateProgress() {
        const totalSteps = 6;
        const completedSteps = $(".step-circle.completed").length;
        const percentage = Math.round((completedSteps / totalSteps) * 100);

        const progressBar = $("#progressBar");
        const progressPercentage = $("#progressPercentage");

        progressBar.css('width', percentage + '%');
        progressBar.text(percentage + '% Complete');
        progressPercentage.text(percentage + '%');
    }

    function updateStepDates() {
        const today = new Date();
        const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
            'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];

        steps.each(function (index) {
            const circle = $(this).find(".step-circle");
            const dateElement = $(this).find(".step-date");

            if (circle.hasClass("completed")) {
                const date = new Date(today);
                date.setDate(date.getDate() - (steps.length - index - 1));
                const formatted = months[date.getMonth()] + ' ' +
                    date.getDate() + ', ' + date.getFullYear();
                dateElement.text(formatted);
            } else {
                dateElement.text('Pending');
            }
        });
    }

    // ========== DATA LOADING ==========

    function getPrimaryData() {
        $.ajax({
            url: `/JobDetails/GetJobInfo?jobId=${jobId}`,
            type: 'GET',
            success: function (data) {
                console.log('Job Data:', data);

                // Populate job details
                $('#jobTitle').text(data.jobTitle || 'N/A');
                $('#customerName').text(data.customerName || 'N/A');
                $('#startDate').text(data.startDate || 'N/A');
                $('#endDate').text(data.endDate || 'N/A');
                $('#statusName').text(data.statusName || 'N/A');
                $('#jobLocation').text(data.jobLocation || 'N/A');
                $('#note').text(data.note || 'N/A');
                $('#jobType').text(data.jobType || 'N/A');

                // Handle InTime logic - Show/Hide timeline
                if (data.inTime === true) {
                    $('#timelineWrapper').show();

                    // Auto-enable schedule step if start date is set
                    if (data.startDate && data.startDate !== 'N/A') {
                        const scheduleStep = $('[data-step="1"]').find('.step-circle');
                        scheduleStep.removeClass('active').addClass('completed');
                    }
                } else {
                    $('#timelineWrapper').hide();
                }

                // Set customer avatar
                if ($('#customerAvatarContainer').length) {
                    const customerAvatar = createAvatar(
                        data.customerName || 'Customer',
                        data.customerImage || data.customerAvatar || null,
                        'large'
                    );
                    $('#customerAvatarContainer').html(customerAvatar);
                }

                // Set company details
                if (data.companyName) $('#companyName').text(data.companyName);
                if (data.customerLocation) $('#customerLocation').text(data.customerLocation);
                if (data.customerPhone) $('#customerPhone').text(data.customerPhone);
                if (data.customerEmail) $('#customerEmail').text(data.customerEmail);

                // Load team members
                if (data.teamMembers && Array.isArray(data.teamMembers)) {
                    loadTeamMembers(data.teamMembers);
                }

                // Initialize timeline
                setTimeout(function () {
                    createLines();
                    updateProgress();
                }, 300);
            },
            error: function (xhr) {
                console.error('Error:', xhr);
                alert("Job not found or error loading data!");

                if ($('#customerAvatarContainer').length) {
                    const defaultAvatar = createAvatar('Unknown', null, 'large');
                    $('#customerAvatarContainer').html(defaultAvatar);
                }
            }
        });
    }

    // ========== EVENT HANDLERS ==========

    // **FIXED: Handle step click - Step 3 check করার আগেই**
    $(".step-circle").on("click", function () {
        const stepNum = Number($(this).data("step"));
        const stepName = $(this).closest(".timeline-step").find(".step-title").text();

        console.log(`Step ${stepNum} clicked: ${stepName}`);

        // **✅ IMPORTANT: Step 3 এর জন্য first priority - toggle করতে হবে**
        if (stepNum === 3) {
            handleStartPauseToggle();
            return; // এখানেই return করে দিচ্ছি, নিচের check গুলো ignore
        }

        // অন্য steps এর জন্য normal flow
        // Check if step is already completed
        if ($(this).hasClass("completed")) {
            alert("This step is already completed!");
            return;
        }

        // Get max completed step number
        const maxCompleted = $(".step-circle.completed").length;

        // Can only complete the next step in sequence
        if (stepNum > maxCompleted + 1) {
            alert("Please complete previous steps first!");
            return;
        }

        selectedStep = stepNum;
        $("#modalStepInfo").html(`<strong>Step ${selectedStep}: ${stepName}</strong>`);
        $("#activityInput").val("");
        modal.show();
    });

    // **Handle Start/Pause Toggle**
    function handleStartPauseToggle() {
        const step3Circle = $('[data-step="3"]').find('.step-circle');
        const step3Title = $('[data-step="3"]').find('.step-title');
        const step3Icon = step3Circle.find('i');

        console.log('=== handleStartPauseToggle ===');
        console.log('isJobRunning:', isJobRunning);
        console.log('Has completed class:', step3Circle.hasClass('completed'));

        if (!isJobRunning) {
            // Job is NOT running - START it
            const confirmStart = confirm("Start the job activity?");
            if (confirmStart) {
                console.log('Starting job...');
                saveJobTeamActivity('start', function (success) {
                    if (success) {
                        isJobRunning = true;

                        // Mark as completed
                        step3Circle.removeClass('active').addClass('completed');

                        // Update title and icon
                        step3Title.text('Running');
                        step3Icon.removeClass('fa-play-circle').addClass('fa-pause-circle');

                        updateStepDates();
                        createLines();
                        updateProgress();

                        console.log('✓ Job started successfully');
                        alert('Job started successfully!');
                    }
                });
            }
        } else {
            // Job IS running - PAUSE it
            const confirmPause = confirm("Pause the job activity?");
            if (confirmPause) {
                console.log('Pausing job...');
                saveJobTeamActivity('pause', function (success) {
                    if (success) {
                        isJobRunning = false;

                        // Keep completed class, just update text/icon
                        step3Title.text('Paused');
                        step3Icon.removeClass('fa-pause-circle').addClass('fa-play-circle');

                        // Activate next step
                        $('[data-step="4"]').find('.step-circle').addClass('active');

                        updateStepDates();
                        createLines();
                        updateProgress();

                        console.log('✓ Job paused successfully');
                        alert('Job paused successfully! Click again to resume.');
                    }
                });
            }
        }
    }

    // Complete a step (for non-Start/Push steps)
    function completeStep(stepNum, remarks) {
        steps.each(function () {
            const currentStepNum = Number($(this).data("step"));

            if (currentStepNum <= stepNum) {
                $(this).find(".step-circle").removeClass("active").addClass("completed");
            } else if (currentStepNum === stepNum + 1) {
                $(this).find(".step-circle").addClass("active");
            }
        });

        updateStepDates();
        createLines();
        updateProgress();

        // Save normal activity
        saveActivityToBackend(stepNum, remarks);
    }

    // Save activity
    $("#saveActivity").on("click", function () {
        const remarks = $("#activityInput").val().trim();

        if (!remarks) {
            alert("Please enter remarks!");
            return;
        }

        completeStep(selectedStep, remarks);
        modal.hide();
    });

    // Recreate lines on window resize
    $(window).on("resize", function () {
        if ($("#timelineContainer").is(":visible")) {
            createLines();
        }
    });

    // ========== BACKEND COMMUNICATION ==========

    // Save activity to backend
    function saveActivityToBackend(step, remarks) {
        console.log('Saving activity:', { jobId, step, remarks });

        $.ajax({
            url: '/JobDetails/SaveActivity',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                jobID: parseInt(jobId),
                activityStep: step,
                remarks: remarks
            }),
            success: function (response) {
                console.log('✓ Activity saved successfully:', response);
            },
            error: function (xhr, status, error) {
                console.error('✗ Error saving activity:', {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    error: error,
                    response: xhr.responseText
                });
                alert('Failed to save activity. Please try again.');
            }
        });
    }

    // Save JobTeamActivity
    function saveJobTeamActivity(type, callback) {
        console.log('=== AJAX REQUEST ===');
        console.log('URL: /JobDetails/SaveJobTeamActivity');
        console.log('Type:', type);
        console.log('JobID:', jobId);

        $.ajax({
            url: '/JobDetails/SaveJobTeamActivity',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                jobID: parseInt(jobId),
                activityType: type
            }),
            beforeSend: function (xhr) {
                console.log('Sending request...');
            },
            success: function (response) {
                console.log('✓ SUCCESS Response:', response);
                if (callback) callback(true);
            },
            error: function (xhr, status, error) {
                console.error('✗ ERROR Response:');
                console.error('Status:', xhr.status);
                console.error('Status Text:', xhr.statusText);
                console.error('Response Text:', xhr.responseText);
                console.error('Error:', error);

                try {
                    const errorData = JSON.parse(xhr.responseText);
                    console.error('Parsed Error:', errorData);
                    alert('Error: ' + (errorData.message || 'Failed to save activity'));
                } catch (e) {
                    alert('Failed to save activity. Please check console for details.');
                }

                if (callback) callback(false);
            },
            complete: function () {
                console.log('Request completed');
            }
        });
    }

    // ========== INITIALIZATION ==========

    getPrimaryData();

    if (typeof feather !== 'undefined') {
        feather.replace();
    }

    console.log('Job Details page initialized for Job ID:', jobId);
});