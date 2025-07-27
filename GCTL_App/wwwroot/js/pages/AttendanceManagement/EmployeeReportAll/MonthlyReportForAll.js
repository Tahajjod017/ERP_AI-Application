function getOrganizationId() {
    $.ajax({
        url: '/MonthlyReportForAll/GetOrganizationId', // Adjust the URL to your update endpoint
        type: 'GET',
        success: function (data) {
            // Assuming the response has 'value' and 'text' properties
            const simplified = data.map(role => ({
                value: role.value,
                label: role.text
            }));

            // Populating the dropdown with simplified data
            choiceManager.populateDropdown('organizationID', simplified);

          
        },
        error: function (xhr, status, error) {
            console.error("Error fetching organization data:", error);
        }
    });
}