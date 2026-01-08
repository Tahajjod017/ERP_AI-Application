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

const employees = [
    //{
    //    name: 'Faruk Hasan',
    //    role: 'Finance Executive',
    //    attendance: ['P', 'P', 'A', 'W', 'P', 'P', 'P', 'P', 'P', 'P', 'M', 'W', 'P', 'H', 'P', 'A', 'P', 'P', 'W', 'P', 'A', 'P', 'P', 'P', 'C', 'W', 'P', 'P', 'P', 'P', 'P']
    //},
    //{
    //    name: 'Faruk Alam',
    //    role: 'IT Executive',
    //    attendance: ['P', 'P', 'W', 'P', 'P', 'A', 'P', 'A', 'P', 'P', 'M', 'W', 'P', 'H', 'P', 'P', 'P', 'W', 'P', 'P', 'A', 'P', 'P', 'W', 'P', 'P', 'C', 'P', 'P', 'P', 'P']
    //},
    // Add more employee data as needed
];

// Function to determine the number of days in a given month and year
function getDaysInMonth(month, year) {
    return new Date(year, month, 0).getDate();
}

// Function to generate the table dynamically
function generateTable() {
    const organizationID = document.getElementById('organizationID').value;
    const departmentID = document.getElementById('departmentID').value;
    //const monthID = document.getElementById('MonthID').value;
    //const yearID = document.getElementById('YearID').value;

   // if (!monthID || !yearID) return;  // Ensure both month and year are selected

    const daysInMonth = getDaysInMonth(1, 1);  // Get the number of days in the selected month
    const tbody = document.getElementById('empMonthlyAll-tbody');
    const thead = document.querySelector('thead tr');

    // Clear existing rows and headers
    tbody.innerHTML = '';
    thead.innerHTML = `<th>Employee</th>`;  // Reset the header row

    // Add day columns to the header
    for (let i = 1; i <= daysInMonth; i++) {
        const th = document.createElement('th');
        th.textContent = String(i).padStart(2, '0');  // Add leading zero for single digits
        thead.appendChild(th);
    }

    // Generate rows for each employee
    employees.forEach(employee => {
        const row = document.createElement('tr');

        // Create the employee cell
        const empCell = document.createElement('td');
        empCell.className = 'empName align-middle white-space-nowrap fw-semibold text-body-emphasis ps-2 py-1';
        empCell.innerHTML = `
                <div class="d-flex align-items-center file-name-icon">
                    <div class="avatar avatar-m avatar-bordered me-2">
                        <img class="rounded-circle" src="../../assets/img/users/user-01.jpg" alt="" />
                    </div>
                    <div class="ms-1">
                        <h6 class="fw-bold">${employee.name}</h6>
                        <span class="fs-12 fw-normal">${employee.role}</span>
                    </div>
                </div>
            `;
        row.appendChild(empCell);

        // Generate attendance data for the days
        for (let i = 0; i < daysInMonth; i++) {
            const status = employee.attendance[i] || '';  // Handle if less data is available than days
            const statusCell = document.createElement('td');
            // Set the background color based on the status
            switch (status) {
                case 'P': statusCell.className = 'bg-success'; break;
                case 'A': statusCell.className = 'bg-danger'; break;
                case 'W': statusCell.className = 'bg-warning'; break;
                case 'M': statusCell.className = 'bg-primary'; break;
                case 'H': statusCell.className = 'bg-warning-subtle'; break;
                case 'C': statusCell.className = 'bg-danger-subtle'; break;
                default: statusCell.className = ''; break;
            }
            statusCell.textContent = status;
            row.appendChild(statusCell);
        }

        // Append the row to the table body
        tbody.appendChild(row);
    });
}

// Listen for changes in the dropdowns
document.getElementById('organizationID').addEventListener('change', generateTable);
document.getElementById('departmentID').addEventListener('change', generateTable);
//document.getElementById('MonthID').addEventListener('change', generateTable);
//document.getElementById('YearID').addEventListener('change', generateTable);


// Initial call to populate the table when the page loads
generateTable();

// Assuming you have the monthSelectPlugin library included in your project




