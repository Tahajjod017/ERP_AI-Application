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

    $("#actionTaken-searchInput").on("input", function () {
        currentPage = 1;
        loadTableData();
    });

    $("#actionTaken-prevPageBtn").on('click', function () {
        if (currentPage > 1) {
            currentPage--;
            loadTableData();
        }
    });

    $("#actionTaken-nextPageBtn").on('click', function () {
        currentPage++;
        loadTableData();
    });
});


let currentSortColumn = 'BankIssuedLetterID';
let currentSortOrder = 'desc';

$('th.sort').on('click', function () {
    const column = $(this).data('sort');

    if (currentSortColumn === column) {
        currentSortOrder = currentSortOrder === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortColumn = column;
        currentSortOrder = 'asc';
    }

    loadTableData(currentSortColumn, currentSortOrder);
    updateSortingIndicator(column, currentSortOrder);
});


function updateSortingIndicator() {
    $('th.sort').each(function () {
        const $th = $(this);
        const column = $th.data('sort');
        $th.find('.sort-icon').remove();

        if (column === currentSortColumn) {
            const iconClass = currentSortOrder === 'asc' ? 'fa-sort-up' : 'fa-sort-down';
            $th.append(`<span class="sort-icon ms-2"><i class="fas ${iconClass} small text-muted"></i></span>`);
        } else {
            $th.append(`<span class="sort-icon ms-2"><i class="fas fa-sort small text-muted"></i></span>`);
        }
    });
}


function loadTableData(sortColumn, sortOrder) {
    var searchTerm = $("#actionTaken-searchInput").val();

    $.ajax({
        url: '/VisitingPath/GetAll',
        method: 'GET',
        data: {
            pageNumber: currentPage,
            pageSize: pageSize,
            searchTerm: searchTerm,
            sortColumn: sortColumn,
            sortOrder: sortOrder
        },
        success: function (response) {
            var paginationInfo = response.paginationInfo;

            

            // Clear the current list
            $("#userVisitList").empty();

            // Loop through users
            response.data.forEach(function (user) {
                var userHtml = `
                    <li>
                        <strong class="text-bg-success">👤 User: ${user.userId}</strong>
                        <ul>
                `;

                user.visits.forEach(function (visit) {
                    userHtml += `
                        <li>
                            📄 <strong>${visit.path}</strong> – ${visit.durationInSeconds} seconds
                            <br /><small>Visited: ${new Date(visit.visitTime).toLocaleString()}</small>
                        </li>
                    `;
                });

                userHtml += `
                        </ul>
                       </li>`;

                // Append to the container
                $("#userVisitList").append(userHtml);
            });
            var paginationInfo = response.paginationInfo;

            $("#paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
            $("#totalCount").text(`(${paginationInfo.totalItems})`);
            updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
        }

    });
}

function updatePagination(pageNumbers, currentPage, totalPages) {
    const paginationLinks = $("#actionTaken-paginationLinks");
    paginationLinks.empty();
    // Window size (number of pages before/after the current page)
    const windowSize = 1;

    const createPageButton = (page) => `
                        <li class="page-item ${page === currentPage ? 'active' : ''}">
                            <button class="page-link page-btn" data-page="${page}">${page}</button>
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
    $("#actionTaken-prevPageBtn").prop('disabled', currentPage === 1);
    $("#actionTaken-nextPageBtn").prop('disabled', currentPage === totalPages);
}

$(document).on('click', '.page-btn', function () {
    const page = $(this).data('page');
    currentPage = page;
    loadTableData();
});
