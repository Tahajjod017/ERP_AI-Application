
function loadJobs(pageNumber = 1) {
    $.ajax({
        url: '/JobLists/GetJobList',
        type: 'GET',
        data: {
            pageNumber: pageNumber,
            pageSize: 5,
            searchTerm: $('#searchBox').val() || '',
            sortColumn: 'CreateJobID',
            sortOrder: 'asc'
        },
        beforeSend: function () {
            $('#leal-tables-body').html('<tr><td colspan="4" class="text-center">Loading...</td></tr>');
        },
        success: function (response) {
            showDev(response)
            showDev(response.data.length)
            $("#card-title").text(`${response.data.length} Jobs`)
            const jobs = response?.data || [];
            if (jobs.length === 0) {
                $('#leal-tables-body').html('<tr><td colspan="9" class="text-center text-muted py-3">No jobs found</td></tr>');
                return;
            }

            let rows = '';
            $.each(jobs, function (i, job) {
                rows += `
                 <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        <td class="fs-9 align-middle">
                            <div class="form-check mb-0 fs-8">
                                <input class="form-check-input" type="checkbox"/>
                            </div>
                        </td>
                        <td class="name align-middle border-end border-translucent white-space-nowrap ps-0 asp-controller="JobDetails"">
                            <a href="/JobDetails/Index/${job.jobID}">${job.jobTitle}</a>
                        </td>
                                                <td class="email align-middle white-space-nowrap fw-semibold ps-4 border-end border-translucent">${job.customerName}</td>
                        <td class="email align-middle white-space-nowrap fw-semibold border-end border-translucent text-center"><a class="text-body-highlight">${job.jobType}</a></td>
                        <td class="email align-middle white-space-nowrap fw-semibold text-center border-end border-translucent">${job.startDate}</td>
                        <td class="email align-middle white-space-nowrap fw-semibold text-center border-end border-translucent">${job.endDate}</td>
                        <td class="contact align-middle white-space-nowrap text-center border-end border-translucent fw-semibold text-body-highlight"><span class="badge badge-phoenix badge-phoenix-secondary">${job.statusName}</span></td>
                        <td class="email align-middle white-space-nowrap fw-semibold ps-4 border-end border-translucent">${job.jobLocation}</td>

                        <td class="align-middle white-space-nowrap text-end pe-0 ps-4 d-flex justify-content-between">
                            <div> ${job.note}</div>
                            <div class="btn-reveal-trigger position-static">
                                
                                <button class="btn btn-sm dropdown-toggle dropdown-caret-none transition-none btn-reveal fs-10" type="button" data-bs-toggle="dropdown" data-boundary="window" aria-haspopup="true" aria-expanded="false" data-bs-reference="parent"><i class="fa-solid fa-ellipsis-vertical"></i></button>
                                <div class="dropdown-menu dropdown-menu-end py-2">
                                    <a class="dropdown-item" href="#!">View</a><a class="dropdown-item" href="#!">Export</a>
                                    <div class="dropdown-divider"></div><a class="dropdown-item text-danger" href="#!">Remove</a>
                                </div>
                            </div>
                        </td>
                    </tr>`
            });

            $('#leal-tables-body').html(rows);
        },
        error: function () {
            $('#leal-tables-body').html('<tr><td colspan="4" class="text-center text-danger">Error loading data</td></tr>');
        }
    });
}

$(function () {
    loadJobs();
});