$(function () {
    const jobId = window.location.pathname.split("/").pop();
    const steps = $(".timeline-step");
    let selectedStep = 0;
    const modal = new bootstrap.Modal(document.getElementById("activityModal"));

    // Function to create connecting lines
    function createLines() {
        // Remove existing lines first
        $(".timeline-line").remove();
        
        steps.each(function (index) {
            if (index === 0) return; // Skip first step
            
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

            // Check if both current and previous steps are completed
            if (prevIcon.hasClass("completed") && currIcon.hasClass("completed")) {
                line.addClass("completed");
            }
        });
    }

    // Get job primary data
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
                
                // Initialize timeline after a delay
                setTimeout(function() {
                    createLines();
                }, 300);
            },
            error: function (xhr) {
                console.error('Error:', xhr);
                alert("Job not found!");
            }
        });
    }

    // Update progress bar
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

    // Update step dates
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

    // Initialize
    getPrimaryData();

    // Recreate lines on window resize
    $(window).on("resize", function() {
        createLines();
    });

    // Handle step click
    $(".step-circle").on("click", function () {
        const stepNum = Number($(this).data("step"));
        const stepName = $(this).closest(".timeline-step").find(".step-title").text();
        
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

    // Save activity
    $("#saveActivity").on("click", function () {
        const remarks = $("#activityInput").val().trim();
        
        if (!remarks) {
            alert("Please enter remarks!");
            return;
        }
        
        // Mark all steps up to selected step as completed
        steps.each(function (index) {
            const stepNum = Number($(this).data("step"));
            
            if (stepNum <= selectedStep) {
                $(this).find(".step-circle").removeClass("active").addClass("completed");
            } else if (stepNum === selectedStep + 1) {
                $(this).find(".step-circle").addClass("active");
            }
        });
        
        // Update dates, lines, and progress
        updateStepDates();
        createLines();
        updateProgress();
        
        // Hide modal
        modal.hide();
        
        // Optional: Save to backend
        saveActivityToBackend(selectedStep, remarks);
    });

    // Optional: Function to save activity to backend
    function saveActivityToBackend(step, remarks) {
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
                console.log('Activity saved:', response);
            },
            error: function (xhr) {
                console.error('Error saving activity:', xhr);
            }
        });
    }

    // Initialize feather icons if available
    if (typeof feather !== 'undefined') {
        feather.replace();
    }
});