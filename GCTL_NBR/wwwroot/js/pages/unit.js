$(document).ready(function () {
    // Handle form submission via AJAX
    $("form").on("submit", function (e) {
        e.preventDefault();

        var formData = {
            unitId: $("#unitId").val() || 0, // Use hidden input for edit mode
            unitName: $("#unitName").val(),
            unitSymbol: $("#unitSymbol").val()
        };

        $.ajax({
            type: "POST",
            url: "/unit/save",
            data: JSON.stringify(formData),
            contentType: "application/json",
            success: function () {
                //toastr.success("Unit saved successfully!");
                resetForm();
                loadUnits();
            },
            error: function () {
                //toastr.warning("Error saving unit. Please try again.");
            }
        });
    });

    function resetForm() {
        $("#unitId").val('');
        $("#unitName").val('');
        $("#unitSymbol").val('');
    }

    $(document).on("click", ".btn-phoenix-primary", function (e) {
        e.preventDefault();
        resetForm();
    });

    let currentPage = 1;
    let pageSize = $("#pageSize").val();
    let searchTerm = "";
    let sortColumn = "unitId";
    let sortOrder = "asc";

    function loadUnits() {
        $.ajax({
            url: "/units/get",
            type: "GET",
            data: { page: currentPage, pageSize, search: searchTerm, sortColumn, sortOrder },
            success: function (response) {
                $("#unitTableBody").empty();
                $("#totalResults").text(`Total Results: ${response.totalRecords}`);

                response.data.forEach(unit => {
                    $("#unitTableBody").append(`
                        <tr>
                            <td>${unit.unitID}</td>
                            <td>${unit.unitName}</td>
                            <td>${unit.unitSymbol}</td>
                            <td>
                                <button class="btn btn-sm btn-primary edit" data-id="${unit.unitID}" data-name="${unit.unitName}" data-symbol="${unit.unitSymbol}"><i class="fas fa-edit"></i></button>
                                <button class="btn btn-sm btn-danger delete" data-id="${unit.unitID}"><i class="fas fa-trash"></i></button>
                            </td>
                        </tr>
                    `);
                });

                setupPagination(response.totalRecords);
            },
            error: function () {
                //toastr.warning("Error loading data.");
            }
        });
    }

    function setupPagination(totalRecords) {
        let totalPages = Math.ceil(totalRecords / pageSize);
        $("#paginationControls").empty();
        for (let i = 1; i <= totalPages; i++) {
            $("#paginationControls").append(`
                <li class="page-item ${i === currentPage ? 'active' : ''}">
                    <a class="page-link" href="#" data-page="${i}">${i}</a>
                </li>
            `);
        }
    }

    $(document).on("click", ".page-link", function (e) {
        e.preventDefault();
        currentPage = $(this).data("page");
        loadUnits();
    });

    $("#searchInput").on("keyup", function () {
        searchTerm = $(this).val();
        currentPage = 1;
        loadUnits();
    });

    $("#pageSize").on("change", function () {
        pageSize = $(this).val();
        currentPage = 1;
        loadUnits();
    });

    $(".sortable").on("click", function () {
        let column = $(this).data("column");
        sortOrder = (sortColumn === column) ? (sortOrder === "asc" ? "desc" : "asc") : "asc";
        sortColumn = column;
        loadUnits();
    });

    // Handle edit
    $(document).on("click", ".edit", function () {
        $("#unitId").val($(this).data("id"));
        $("#unitName").val($(this).data("name"));
        $("#unitSymbol").val($(this).data("symbol"));
    });

    // Handle delete
    $(document).on("click", ".delete", function () {
        if (confirm("Are you sure you want to delete this unit?")) {
            let unitId = $(this).data("id");
            $.ajax({
                type: "POST",
                url: "/units/delete",
                data: { unitId: unitId },
                success: function () {
                    //toastr.success("Unit deleted successfully!");
                    loadUnits();
                },
                error: function () {
                    //toastr.warning("Error deleting unit.");
                }
            });
        }
    });

    loadUnits();
});