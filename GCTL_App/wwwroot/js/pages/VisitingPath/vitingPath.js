
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
let currentSortColumn = "";
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

function formatDate(dateString) {
    const date = new Date(dateString);

    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0'); // Months are 0-based
    const year = date.getFullYear();

    let hours = date.getHours();
    const minutes = String(date.getMinutes()).padStart(2, '0');

    const ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12 || 12; // Convert to 12-hour format and show 12 instead of 0

    return `${day}/${month}/${year} ${hours}:${minutes} ${ampm}`;
}
function loadTableData(currentSortColumn, currentSortOrder) {
    var searchTerm = $("#searchInput").val();
    

    $.ajax({
        url: '/VisitingPath/GetAll',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            currentSortColumn: currentSortColumn,
            currentSortOrder: currentSortOrder,
           

        },
        success: function (response) {

 
            console.log(response);


            //$("#userVisitList").empty();

            //            // Loop through users
            //            response.data.forEach(function (user) {
            //                var userHtml = `
            //                    <li>
            //                        <strong class="text-bg-success">👤 User: ${user.userId}</strong>
            //                        <ul>
            //                `;

            //                user.visits.forEach(function (visit) {
            //                    userHtml += `
            //                        <li>
            //                            📄 <strong>${visit.path}</strong> – ${visit.durationInSeconds} seconds
            //                            <br /><small>Visited:${formatDate(visit.visitTime)}</small>
            //                        </li>
            //                    `;
            //                });

            //                userHtml += `
            //                        </ul>
            //                       </li>`;

            //                // Append to the container
            //                $("#userVisitList").append(userHtml);
            //            });


            //
            $("#userVisitList").empty();

            response.data.forEach(function (user) {
                var userHtml = `
        <div class="col-12 mb-4">
            <div class="card shadow-sm border-primary">
                <div class="card-header bg-primary text-white d-flex justify-content-between align-items-center">
                    <span><i class="fas fa-user me-2"></i>User ID: <strong>${user.userId}</strong></span>
                    <span class="badge bg-light text-dark">${user.visits.length} Visits</span>
                </div>
                <div class="card-body">
                    <ul class="list-group list-group-flush">
    `;

                user.visits.forEach(function (visit) {
                    userHtml += `
            <li class="list-group-item">
                <div class="d-flex justify-content-between">
                    <div>
                        <i class="fas fa-map-marker-alt text-success me-2"></i>
                        <strong>${visit.path}</strong>
                        <div class="text-muted small">Visited: ${formatDate(visit.visitTime)}</div>
                    </div>
                    <div class="text-end">
                        <span class="badge bg-secondary">${visit.durationInSeconds} sec</span>
                    </div>
                </div>
            </li>
        `;
                });

                userHtml += `
                    </ul>
                </div>
            </div>
        </div>
    `;

                $("#userVisitList").append(userHtml);
            });

            //

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
