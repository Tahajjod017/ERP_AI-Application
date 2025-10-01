

window.choicesInstances = {};

document.addEventListener('DOMContentLoaded', function () {
    const dropdownConfigs = [

        { selector: '#TargetTypeDD', placeholder: 'Select Target', dataKey: 'targetTypeData' },
        { selector: '#ActionNameDD', placeholder: 'Select Action Name', dataKey: 'actionNameData' },
        { selector: '#UserNameDD', placeholder: 'Select User', dataKey: 'userNameData' }
    ];

    dropdownConfigs.forEach(config => {
        const element = document.querySelector(config.selector);
        const data = window[config.dataKey];

        if (element && data) {
            const instance = new Choices(element, {
                removeItemButton: true,
                placeholder: true,
                placeholderValue: config.placeholder,
                searchEnabled: true
            });

            // Set initial choices
            instance.setChoices(
                [{ value: '', label: config.placeholder, selected: true, disabled: true }]
                    .concat(data.map(item => ({
                        value: item.Value,
                        label: item.Text
                    }))),
                'value',
                'label',
                true
            );

            window.choicesInstances[config.selector] = {
                instance: instance,
                placeholder: config.placeholder,
                data: data
            };
        }
    });



});
//





function formatKey(key) {

    return key.replace(/([A-Z])/g, ' $1').replace(/^./, str => str.toUpperCase());
}

// Optional: Render condensed key-values for preview (used in row preview column)
function renderFields(jsonString) {
    try {
        const obj = JSON.parse(jsonString);
        return Object.entries(obj)
            .filter(([k, v]) => v !== null && v !== "")
            .map(([k, v]) => `${formatKey(k)}: ${v}`)
            .join(', ');
    } catch (err) {
        return 'N/A';
    }
}



// On DOM ready, set TaxpayerID from URL

var currentPage = 1;
var pageSize = 5;

$('.dropdown-item').on('click', function () {
    var selectedSize = $(this).data("size");
    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
        currentPage = 1;
    } else {
        return;
    }

    $('#selectedPageSize').text(selectedSize);
    loadTableData();
})



$(document).ready(function () {
    loadTableData();

    // Handle search input change
    $("#searchInput").on("input", function () {
        currentPage = 1;  // Reset to first page when searching
        loadTableData();
    });

    // Handle pagination
    $("#prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});

// Declare sortColumn and sortOrder globally so they are accessible
var currentSortColumn = '';

let currentSortOrder = '';

$('th.sort').on('click', function () {
   
    const column = $(this).data('sort');
   
    if (currentSortColumn === column) {
        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortOrder = 'asc';
    }

    loadTableData(currentSortColumn, currentSortOrder);
});

// Whenever any filter changes, reload the table
$('#FromDate, #ToDate, #TargetTypeDD, #ActionNameDD, #UserNameDD').on('change', function () {
    loadTableData(currentSortColumn, currentSortOrder);
});


function loadTableData(currentSortColumn, currentSortOrder) {
    var searchTerm = $("#searchInput").val();
    var fromDate = $('#FromDate').val();
    var toDate = $('#ToDate').val();
    var tergetType = $("#TargetTypeDD").val();

    var actionName = $('#ActionNameDD').val();
    var createdBy = $('#UserNameDD').val();
    console.log("From Date:", fromDate);
    console.log("To Date:", toDate);
    $.ajax({
        url: '/ActionLog/ActionLogDataTable',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            currentSortColumn: currentSortColumn,
            currentSortOrder: currentSortOrder,
            fromDate: fromDate,
            toDate: toDate,
            tergetType: tergetType,
            actionName: actionName,
            createdBy: createdBy
        },
        success: function (response) {
            var tableBody = $("#project-summary-table-body");
            console.log("data:Dataata Tablee", response);


            tableBody.empty();
            if (response.data.length > 0) {


                response.data.forEach(function (item) {
                    const isDataUpdated = item.actionName === 'Data Updated';
                    const isNotLoginLogout = item.actionName !== 'Login' && item.actionName !== 'Logout';


                    tableBody.append(`
                    <tr class="position-static">

                        <td class="align-middle white-space-nowrap ps-0 text-center">${item.actionLogID}</td>
                        <td class="align-middle white-space-nowrap ps-0">${item.employeeUserName}</td>
                        <td class="align-middle white-space-nowrap ps-0">${item.userEmail}</td>
                        <td class="align-middle white-space-nowrap ps-0">${item.targetType}</td>
                        <td class="align-middle white-space-nowrap ps-0">${item.actionName}</td>

                           <td class="align-middle white-space-nowrap ps-0">
                        <button class="btn btn-info btn-sm view-changes showinfo" data-id="${item.actionLogID}" type="button" ${isDataUpdated ? '' : 'disabled'}>
                            View
                        </button>
                    </td>
                               <td class="align-middle white-space-nowrap ps-0">
                                                <button class="btn btn-info btn-sm view-changes1 showinfo"
                              data-id="${item.actionLogID}" type="button" ${isNotLoginLogout ? '' : 'disabled'}>  View
                           </button>
                               </td>


                    </tr>
                `);
                });


            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            var paginationInfo = response.paginationInfo;

            $("#paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#totalCount").text(`(${paginationInfo.totalItems})`);

            // Update pagination buttons
            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        }
    });
}


//

$(document).on("change", "#FromDate,#ToDate,#TargetTypeDD,#ActionNameDD,#UserNameDD", function () {

    currentPage = 1;
    loadTableData();
});

//

$(document).on('click', '.view-changes1', function () {
    const actionLogId = $(this).data('id');

    $.ajax({
        url: `/ActionLog/GetActionLogDetails1`,
        method: 'GET',
        data: { actionLogId: actionLogId },
        success: function (response) {

            console.log("Dataaaaaaaaaaaaaaaaaaaaa", response);
            const beforeData = response.actionBefore;
            const afterData = response.actionAfter;
            //$('#userName').text(response.employeeUserName);
            $('#userEmail').text(response.userEmail);
            $('#userName').text(`${response.employeeUserName} (${response.employeeID})`);
            const createdDate = new Date(response.createdAt);
            const day = String(createdDate.getDate()).padStart(2, '0');
            const month = String(createdDate.getMonth() + 1).padStart(2, '0');
            const year = createdDate.getFullYear();
            let hours = createdDate.getHours();
            const minutes = String(createdDate.getMinutes()).padStart(2, '0');
            const ampm = hours >= 12 ? 'PM' : 'AM';
            hours = hours % 12;
            hours = hours ? hours : 12;
            const formattedTime = `${String(hours).padStart(2, '0')}:${minutes} ${ampm}`;
            const formattedDate = `${day}/${month}/${year} ${formattedTime}`;
            $('#createdAt').text(formattedDate);
            $('#TargetID').text(`${response?.targetID || ''}(Primary Key)`);
            $('#TargetType').text(`${response.targetType} (Page Title)` );
            $('#ActionName').text(response.actionName);
            renderKeyValueTable(beforeData, afterData);
            $('#actionDetailModal').modal('show');
        },
        error: function () {
            $('#actionDetailsTableContainer').html('<div class="alert alert-danger">Failed to load change details.</div>');
        }
    });
});



function renderKeyValueTable(actionBefore, actionAfter) {
    let html = `
                    <table class="table table-bordered table-striped table-hover">
                        <thead class="table-light">
                            <tr>
                                   <th style="text-align: center;">Field</th>
                                   <th style="text-align: center;">Before</th>
                                   <th style="text-align: center;">After</th>

                            </tr>
                        </thead>
                        <tbody>`;


    const allKeys = new Set([
        ...Object.keys(actionBefore || {}),
        ...Object.keys(actionAfter || {})
    ]);


    allKeys.forEach(key => {
        const beforeValue = actionBefore?.[key] ?? '';
        const afterValue = actionAfter?.[key] ?? '';

        html += `
                        <tr>
                      <td style="text-align: center;"><strong>${key}</strong></td>
                     <td style="text-align: center;">${beforeValue}</td>
                     <td style="text-align: center;">${afterValue}</td>

                        </tr>`;
    });

    html += ` </tbody>
                       </table>`;

    $("#actionDetailsTableContainer").html(html);
}

//Comparison

function renderChangedFieldsTable(data) {
    let table = `
                    <table class="table table-bordered table-sm">
                        <thead>
                            <tr>
                                   <th class="text-center">Field</th>
                                   <th class="text-center">Before</th>
                                   <th class="text-center">After</th>

                            </tr>
                        </thead>
                        <tbody>`;
    let hasRows = false;
    data.forEach(item => {

        table += `
                        <tr>
                                <td class="text-center">${item.field}</td>
                                <td class="text-center">${item.before ?? ''}</td>
                                <td class="text-center">${item.after ?? ''}</td>

                        </tr>`;
    });

    table += `</tbody></table>`;
    return table;
}

$(document).on("click", ".view-changes", function () {
    const actionLogId = $(this).data("id");

    $.ajax({
        url: `/ActionLog/GetActionLogDetails`,
        method: 'GET',
        data: { actionLogId: actionLogId },
        success: function (response) {
            //

            console.log("Dataaaa", response);
            $('#userName').text(`${response.employeeUserName || ''} (${response.employeeID || ''})`);

            // $('#userName').text(response.employeeUserName);
            $('#userEmail').text(response.userEmail || '');

            const createdDate = new Date(response.createdAt);

            // Convert and format
            const day = String(createdDate.getDate()).padStart(2, '0');
            const month = String(createdDate.getMonth() + 1).padStart(2, '0');
            const year = createdDate.getFullYear();

            let hours = createdDate.getHours();
            const minutes = String(createdDate.getMinutes()).padStart(2, '0');
            const ampm = hours >= 12 ? 'PM' : 'AM';
            hours = hours % 12;
            hours = hours ? hours : 12;

            const formattedTime = `${String(hours).padStart(2, '0')}:${minutes} ${ampm}`;
            const formattedDate = `${day}/${month}/${year} ${formattedTime}`;



            $('#createdAt').text(formattedDate);
            $('#TargetID').text(response.targetID);
            $('#TargetType').text(response.targetType);  //ActionName
            $('#ActionName').text(response.actionName);
            //
            const changesTable = renderChangedFieldsTable(response.changedValues);
            $("#actionDetailsTableContainer").html(changesTable);

            $("#actionDetailModal").modal("show");
        },
        error: function () {
            alert("Failed to load changed fields.");
        }
    });
});


//


//


function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#paginationLinks");
    paginationLinks.empty();
    // Window size (number of pages before/after the current page)
    const windowSize = 1;
    // Helper function to generate page button
    const createPageButton = (page) => `
                    <li class="page-item ${page === currentPage ? 'active' : ''}">
                        <button class="page-link" onclick="goToPage(${page})">${page}</button>
                    </li>
                `;
    // Helper function for ellipsis
    const addEllipsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
    // Add "First Page" and ellipsis if needed
    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addEllipsis());
    }
    // Add page number buttons within the window range
    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);
    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }
    // Add ellipsis and "Last Page" button if needed
    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addEllipsis(), createPageButton(totalPages));
    }
    // Disable or enable previous/next buttons
    $("#prevPageBtn").prop('disabled', currentPage === 1);
    $("#nextPageBtn").prop('disabled', currentPage === totalPages);
}

function goToPage(page) {
    currentPage = page;
    loadTableData();
}


