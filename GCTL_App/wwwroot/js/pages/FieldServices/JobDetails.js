$(function () {
    const jobId = window.location.pathname.split("/").pop();

    let getPrimaryData = () => {
        $.ajax({
            url: `/JobDetails/GetJobInfo?jobId=${jobId}`,
            type: 'GET',
            success: function (data) {
                console.log(data); // full job object
                $('#jobTitle').text(data.jobTitle);
                $('#customerName').text(data.customerName);
                $('#startDate').text(data.startDate);
                $('#endDate').text(data.endDate);
                $('#statusName').text(data.statusName);
                $('#jobLocation').text(data.jobLocation);
                $('#note').text(data.note);
            },
            error: function (xhr) {
                alert("Job not found!");
            }
        });
    }
    getPrimaryData();


    // design state line
    const steps = $(".timeline-step");
    let selectedStep = 0;
    const modal = new bootstrap.Modal(document.getElementById("activityModal"));

    function createLines() {
        steps.each(function (index) {
            debugger;
            if (index === 0) return;

            const prevIcon = $(steps[index - 1]).find(".step-icon");
            const currIcon = $(this).find(".step-icon");

            const line = $('<div class="step-line"></div>');
            $(".timeline-container").append(line);

            const containerLeft = $(".timeline-container").offset().left;
            const startX = prevIcon.offset().left + prevIcon.outerWidth() / 2 - containerLeft;
            const endX = currIcon.offset().left + currIcon.outerWidth() / 2 - containerLeft;
            const width = endX - startX; // exactly from previous center to current center

            line.css({ left: startX + "px", width: width + "px" });
        })
    }

    $(window).on("load resize", createLines)


    $(".step-icon").on("click", function () {
        const stepNum = Number($(this).closest(".timeline-step").data("step"));
        const maxCompleted = $(".step-icon.completed").length;

        if (stepNum > maxCompleted + 1) {
            alert("Please complete previous steps first!");
            return;
        }

        selectedStep = stepNum;
        $("#modalStepInfo").text("You clicked step " + selectedStep);
        $("#activityInput").val();
        modal.show();
    });

    $("#saveActivity").on("click", function () {
        steps.each(function (index) {
            const stepNum = Number($(this).data("step"));
            if (stepNum <= selectedStep) {
                $(this).find(".step-icon").addClass("completed");
                if (index > 0) {
                    $(".step-line").eq(index - 1).addClass("completed");
                }
            }
        });
        modal.hide();
    });
});