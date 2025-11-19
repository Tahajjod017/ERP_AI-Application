$(function () {
    const jobId = window.location.pathname.split("/").pop();
    showDev(jobId)

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
});