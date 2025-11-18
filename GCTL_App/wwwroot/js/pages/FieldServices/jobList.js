
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

            const jobs = response?.data || [];
            if (jobs.length === 0) {
                $('#leal-tables-body').html('<tr><td colspan="6" class="text-center text-muted py-3">No jobs found</td></tr>');
                return;
            }

            let rows = '';
            $.each(jobs, function (i, job) {
                rows += `
                 <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                        <td class="fs-9 align-middle">
                            <div class="form-check mb-0 fs-8">
                                <input class="form-check-input" type="checkbox" data-bulk-select-row='{"customer":{"avatar":"/team/32.webp","name":"Anthoney Michael","designation":"VP Accounting","status":{"label":"new lead","type":"badge-phoenix-primary"}},"email":"anth125@gmail.com","phone":"+1-202-555-0126","contact":"Ally Aagaard","company":"Google.inc","date":"Jan 01, 12:56 PM"}' />
                            </div>
                        </td>
                        <td class="name align-middle white-space-nowrap ps-0">
                            <div class="d-flex align-items-center">

                                <div>
                                    <a class="fs-8 fw-bold text-decoration-none">${job.startDate}</a>
                                    <div class="d-flex align-items-center">
                                        <p class="mb-0 text-body-highlight fw-semibold fs-9 me-2">11:00 AM - 01:30 PM</p>
                                    </div>
                                </div>
                            </div>
                        </td>
                        <td class="contact align-middle white-space-nowrap ps-4 border-end border-translucent fw-semibold text-body-highlight"><span class="badge badge-phoenix badge-phoenix-secondary">${job.statusName}</span></td>
                        <td class="email align-middle white-space-nowrap fw-semibold ps-4 border-end border-translucent"><a class="text-body-highlight" href="mailto:anth125@gmail.com">${job.jobType}</a></td>
                        <td class="phone align-middle white-space-nowrap fw-semibold ps-4 border-end border-translucent">
                            <div class="avatar-group mb-4">
                                <div class="avatar avatar-s ">
                                    <img class="rounded-circle " src="../../assets/img/team/30.webp" alt="" />
                                </div>
                                <div class="avatar avatar-s ">
                                    <img class="rounded-circle " src="../../assets/img/team/57.webp" alt="" />
                                </div>
                                <div class="avatar avatar-s ">
                                    <img class="rounded-circle " src="../../assets/img/team/25.webp" alt="" />
                                </div>
                                <div class="avatar avatar-s ">
                                    <div class="avatar-name rounded-circle "><span>+3</span></div>
                                </div>
                            </div>
                        </td>
                        <td class="date align-middle white-space-nowrap text-body-tertiary text-opacity-85 ps-4 text-body-tertiary">
                            <div class="d-flex align-items-center">

                                <div>
                                    <a class="fs-8 text-black text-decoration-none">Mr. Abdullah al mamun</a>
                                    <div class="d-flex align-items-center">
                                        <p class="mb-0 text-body-highlight fw-semibold fs-9 me-2">#32/4, Savar, Dhaka-1211</p>
                                    </div>
                                </div>
                            </div>
                        </td>
                        <td class="align-middle white-space-nowrap text-end pe-0 ps-4">
                            <div class="btn-reveal-trigger position-static">
                                <button class="btn btn-sm dropdown-toggle dropdown-caret-none transition-none btn-reveal fs-10" type="button" data-bs-toggle="dropdown" data-boundary="window" aria-haspopup="true" aria-expanded="false" data-bs-reference="parent"><span class="fas fa-ellipsis-h fs-10"></span></button>
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