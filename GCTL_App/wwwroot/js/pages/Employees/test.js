$(document).ready(function () {

    //#region FILE


    const currentFilters = {
        department: '',
        status: '',
        sort: '',
        search: '',
        sortColumn: '',
        sortDirection: '',
        company: ''
    };



    //$('#testLoginPage').show();

    $('#btnExport').on('click', function () {

        GenaratePDF(8);
    });

    $('#btnExportXL').on('click', function () {

        //toastr.info("Processing");
        const filterData = {
            department: currentFilters.department,
            status: currentFilters.status,
            sort: currentFilters.sort,
            search: currentFilters.search,
            sortColumn: currentFilters.sortColumn,
            sortDirection: currentFilters.sortDirection,
            company: currentFilters.company
        };


        GenarateXL(filterData);
    });

    $('#btnExportPDFdownload').on('click', function () {

        //toastr.info("Processing to download PDF");
        const filterData = {
            department: currentFilters.department,
            status: currentFilters.status,
            sort: currentFilters.sort,
            search: currentFilters.search,
            sortColumn: currentFilters.sortColumn,
            sortDirection: currentFilters.sortDirection,
            company: currentFilters.company
        };


        GenaratePDFdownload(filterData);
    });

    $('#btnExportPDFpreview').on('click', function () {

        //toastr.info("Processing to preview PDF");
        const filterData = {
            department: currentFilters.department,
            status: currentFilters.status,
            sort: currentFilters.sort,
            search: currentFilters.search,
            sortColumn: currentFilters.sortColumn,
            sortDirection: currentFilters.sortDirection,
            company: currentFilters.company
        };


        GenaratePDFpreview(filterData);
    });


   


    function GenaratePDFdownload(filterData) {
        //toastr.info("Generating PDF for download");
        console.log(filterData);
    }




    //#region PDF Preview

    function GenaratePDFpreview(filterData) {
        //toastr.info("Generating PDF for preview");
        console.log(filterData);

        const formData = new FormData();
        formData.append("Department", filterData.department || "");
        formData.append("Status", filterData.status || "");
        formData.append("Sort", filterData.sort || "");
        formData.append("Search", filterData.search || "");
        formData.append("SortColumn", filterData.sortColumn || "");
        formData.append("SortDirection", filterData.sortDirection || "");
        formData.append("Company", filterData.company || "");

        $.ajax({
            url: '/EmployeeReport/GenerateEmployeePdfPreview',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            xhrFields: {
                responseType: 'blob'
            },
            success: function (response, status, xhr) {
                const blob = new Blob([response], { type: 'application/pdf' });
                const url = window.URL.createObjectURL(blob);

                // Open PDF in a new tab for preview
                window.open(url, '_blank');

                //toastr.success("PDF preview generated successfully");
            },
            error: function (xhr, status, error) {
                console.error("Error generating PDF preview:", error);
                //toastr.error("Failed to generate PDF preview");
            }
        });
    }

    //#endregion


    //#endregion


    showDev('page load test')


    // Enhanced sample data and sorting functionality
    const sampleData = [
        {
            employeeName: "John Doe",
            department: "HR",
            position: "Manager",
            terminationType: "Voluntary",
            reason: "Resignation",
            processedDate: "2025-08-01",
            terminationDate: "2025-08-15",
            status: "Processed"
        },
        {
            employeeName: "Jane Smith",
            department: "IT",
            position: "Developer",
            terminationType: "Involuntary",
            reason: "Performance",
            processedDate: "2025-08-10",
            terminationDate: "2025-08-20",
            status: "Pending"
        },
        {
            employeeName: "Mike Johnson",
            department: "Finance",
            position: "Analyst",
            terminationType: "Voluntary",
            reason: "Better Opportunity",
            processedDate: "2025-08-12",
            terminationDate: "2025-08-25",
            status: "Approved"
        }
    ];

    let ascending = true;

    // Enhanced populate table function
    function populateTable() {
        const tbody = document.querySelector('#table2 tbody');
        tbody.innerHTML = ''; // Clear existing rows

        sampleData.forEach(rowData => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td data-column="0">${rowData.employeeName}</td>
                <td data-column="1">${rowData.department}</td>
                <td data-column="2">${rowData.position}</td>
                <td data-column="3">${rowData.terminationType}</td>
                <td data-column="4">${rowData.reason}</td>
                <td data-column="5">${rowData.processedDate}</td>
                <td data-column="6">${rowData.terminationDate}</td>
                <td data-column="7">${rowData.status}</td>
            `;
            tbody.appendChild(row);
        });

        console.log('Table populated with sample data');

        // Apply column settings to newly populated data
        DynamicTableDrag.refreshTableSettings('table2');
    }

    function populateTable3() {
        const tbody = document.querySelector('#table3 tbody');
        tbody.innerHTML = ''; // Clear existing rows

        sampleData.forEach(rowData => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td >${rowData.employeeName}</td>
                <td >${rowData.department}</td>
                <td >${rowData.position}</td>
                <td >${rowData.terminationType}</td>
                <td >${rowData.reason}</td>
                <td >${rowData.processedDate}</td>
                <td >${rowData.terminationDate}</td>
                <td >${rowData.status}</td>
            `;
            tbody.appendChild(row);
        });

        console.log('Table populated with sample data');

        // Apply column settings to newly populated data
        DynamicTableDrag.refreshTableSettings('table3');
    }


    $('#table3 thead th').on('click', function () {
        const columnIndex = $(this).index();
        sampleData.sort((a, b) => {
            const valA = Object.values(a)[columnIndex];
            const valB = Object.values(b)[columnIndex];
            // Date check
            const isDate = /^\d{4}-\d{2}-\d{2}$/;
            if (isDate.test(valA)) {
                return ascending
                    ? new Date(valA) - new Date(valB)
                    : new Date(valB) - new Date(valA);
            }
            return ascending
                ? valA.localeCompare(valB)
                : valB.localeCompare(valA);
        });
        ascending = !ascending;
        populateTable3(); // This will now preserve column settings
    });

       
        setTimeout(() => {
            populateTable();
            populateTable3();
        }, 100);

        
        $('#table2 thead th').on('click', function () {
            const columnIndex = $(this).index();
            sampleData.sort((a, b) => {
                const valA = Object.values(a)[columnIndex];
                const valB = Object.values(b)[columnIndex];
                // Date check
                const isDate = /^\d{4}-\d{2}-\d{2}$/;
                if (isDate.test(valA)) {
                    return ascending
                        ? new Date(valA) - new Date(valB)
                        : new Date(valB) - new Date(valA);
                }
                return ascending
                    ? valA.localeCompare(valB)
                    : valB.localeCompare(valA);
            });
            ascending = !ascending;
            populateTable(); // This will now preserve column settings
        });
  

   


    console.log('All tables populated successfully with same termination data');
});