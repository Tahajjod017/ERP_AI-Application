r currentPage = 1;
var pageSize = 5;

$('.dropdown-item').on('click', function () {
    var selectedSize = $(this).data("size");
    if (selectedSize) {
        pageSize = parseInt(selectedSize, 10);
    } else {
        return;
    }

    $('#selectedPageSize').text(selectedSize);
    loadTableData();
})


$(document).ready(function () {
   // loadTableData();

    // Handle search input change
    $("#searchInput").on("input", function () {
        currentPage = 1;  // Reset to first page when searching
        //loadTableData();
    });

    // Handle pagination
    $("#prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            //loadTableData();
        }
    });

    $("#nextPageBtn").on('click', function () {
        currentPage++;
        //loadTableData();
    });
});

// Declare sortColumn and sortOrder globally so they are accessible
let currentSortColumn = 'fullName';
let currentSortOrder = 'asc';

$('th.sort').on('click', function () {
    const column = $(this).data('sort');

    if (currentSortColumn === column) {
        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortOrder = 'asc';
    }

    //loadTableData(currentSortColumn, currentSortOrder);
});

function loadTableData(sortColumn, sortOrder) {
    var searchTerm = $("#searchInput").val();
    debugger
    $.ajax({
        url: '/GeneralTaxPayerData/GeneralTaxPayerDataTable',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            debugger
            console.log("Dabbbbbta:" ,response)
            var tableBody = $("#project-summary-table-body");
            tableBody.empty();
            if (response.data.length > 0) {
                response.data.forEach(function (item) {
                    tableBody.append(`

                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="selectItem" data-bank-id="${item.taxpayerID}" />
                            </td>
                            <td class="align-middle white-space-nowrap ps-0">${item.fullName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.occupationIDName}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.tin}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.taxCircleIDName}</td>
                           
                            <td class="align-middle white-space-nowrap ps-0">
                                <a class="btn btn-outline-danger btn-sm p-2" href="#!" id="viewBank-single-delete" data-bank-id="${item.taxpayerID}"><span class="fas fa-trash"></span></a>
                                <a class="btn btn-outline-primary btn-sm p-2" href="#!" id="viewBank-edit" data-bank-id="${item.taxpayerID}"><span class="fas fa-pencil"></span></a>
                            </td>
                        </tr>
                    `);
                });
            } else {
                tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
            }

            // Get pagination info from the response
            var paginationInfo = response.paginationInfo;

            // Update pagination info text to show range of items
            $("#paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#totalCount").text(`(${paginationInfo.totalItems})`);

            // Update pagination buttons
            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        }
    });
}

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
    // Helper function for elLIPsis
    const addElLIPsis = () => '<li class="page-item disabled"><span class="page-link">...</span></li>';
    // Add "First Page" and elLIPsis if needed
    if (currentPage > windowSize + 1) {
        paginationLinks.append(createPageButton(1), addElLIPsis());
    }
    // Add page number buttons within the window range
    const startPage = Math.max(1, currentPage - windowSize);
    const endPage = Math.min(totalPages, currentPage + windowSize);
    for (let i = startPage; i <= endPage; i++) {
        paginationLinks.append(createPageButton(i));
    }
    // Add elLIPsis and "Last Page" button if needed
    if (currentPage < totalPages - windowSize) {
        paginationLinks.append(addElLIPsis(), createPageButton(totalPages));
    }
    // Disable or enable previous/next buttons
    $("#prevPageBtn").prop('disabled', currentPage === 1);
    $("#nextPageBtn").prop('disabled', currentPage === totalPages);
}

function goToPage(page) {
    currentPage = page;
    //loadTableData();
}


