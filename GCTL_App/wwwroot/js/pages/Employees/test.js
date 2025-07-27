$(document).ready(function () {

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


    //#endregion



    //#region FILE


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






});