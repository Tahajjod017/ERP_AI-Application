$(function () {
    var ids = {
        title: "#teamTitle"
    }
    async function GetTeamDetails() {
        const path = window.location.pathname;
        const segments = path.split('/');
        const id = segments[segments.length - 1];

        console.log(id); // "2"

        // If you want it as a number
        const numericId = parseInt(id, 10);
        console.log(numericId); // 2

        try {
            const response = await fetch(`/TeamDetails/GetTeamDetails?id=${numericId}`);
            if (!response.ok) throw new Error('Network response was not ok');

            const teamDeatils = await response.json();
            showDev(teamDeatils);
            setHTML(teamDeatils);
        } catch (error) {
            console.error("Error fetching next index: ", error);
        }
    }
    function setHTML(result) {
        $(ids.title).text(result.teamName);

        const container = document.getElementById('membersDiv');
        if (!container) {
            console.error("Container with id 'teamsDiv' not found in DOM");
            return;
        }

        // Clear previous content
        container.innerHTML = '';

        result.memberDetails.forEach(member => {
            const memberHtml = `<div class="col-sm-12 col-md-4 col-xl-4 col-xl-3">
                <div class="card card-body">
                    <div class="row align-items-center g-3 text-center text-xxl-start">
                        <input type="hidden" class="TeamID" value="${result.teamID}" />
                        <input type="hidden" class="EmployeeID" value="${member.employeeID}" />

                        <div class="col-12 col-xxl-auto"> 
                            <div class="avatar avatar-5xl d-flex align-items-center">
                                <img class="rounded-circle" src="${member.profileImage}" alt="Employee Photo" style="width:80px; height:80px; object-fit:cover;" />
                            </div>
                        </div>
                        <div class="col-12 col-sm-auto flex-1">
                            <h3 class="fw-bolder mb-2">${member.teamMemberName}</h3>
                            <p class="mb-0">${member.designation}</p>
                            <p class="mb-0">${member.mobileNumber}</p>
                        </div>
                        <button type="button" class="btn btn-outline-primary btn-sm teamLeaderAssignBtn" ${member.isTeamHead == true ? "disabled" : ""}>Assign as Team Leader</button>
                    </div>
                </div>
            </div>`

            container.innerHTML += memberHtml;

        });
    }

    

    $(document).on('click', ".teamLeaderAssignBtn", async function () {
        try {
            const card = $(this).closest(".card");

            const teamID = card.find('.TeamID').val();
            const employeeID = card.find('.EmployeeID').val();

            const confirmed = await customToaster.confirm("Do you want to restart this Lead?");

            if (confirmed) {

                const response = await fetch('/TeamDetails/SetTeamHead', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify({ TeamID: teamID, EmployeeID: employeeID })
                });

                if (!response.ok) throw console.log("Network response was not ok");

                const result = await response.json();
                toastr.success(result.message);
                GetTeamDetails();
            }
            else customToaster.error("Cancelled!");
        } catch (error) {
            console.error("Error assigning leader:", error);
        }
       
    });

    GetTeamDetails();
});