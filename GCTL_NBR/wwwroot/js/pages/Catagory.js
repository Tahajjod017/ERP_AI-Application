$(document).ready(function () {




        //$("#resetCata").click(function () {
        //    console.log("Reset button clicked!");
        //    // Add your reset logic here
        //});

        //$("#saveCata").click(function () {
        //    console.log("Save button clicked!");
        //    // Add your save logic here
        //});
    


    // Handle form submission via AJAX
    $("#saveCata").click( function (e) {
        e.preventDefault();

        var formData = {
            categoryId: $("#categoryId").val() || 0, // Use hidden input for edit mode
            categoryName: $("#categoryName").val(),
            categorySymbol: $("#categorySymbol").val()
        };

        $.ajax({
            type: "POST",
            url: "/catagory/save",
            data: JSON.stringify(formData),
            contentType: "application/json",
            success: function () {
                toastr.success("Category saved successfully!");
                resetForm();
                loadCategories();
            },
            error: function () {
                toastr.warning("Error saving category. Please try again.");
            }
        });
    });

    function resetForm() {
        $("#categoryId").val('');
        $("#categoryName").val('');
        $("#categorySymbol").val('');
    }

    $("#resetCata").click( function (e) {
        e.preventDefault();
        resetForm();
    });

    let currentPage = 1;
    let pageSize = $("#pageSize").val();
    let searchTerm = "";
    let sortColumn = "categoryId";
    let sortOrder = "asc";

    function loadCategories() {
        $.ajax({
            url: "/Categories/get",
            type: "GET",
            data: { page: currentPage, pageSize, search: searchTerm, sortColumn, sortOrder },
            success: function (response) {
                $("#categoryTableBody").empty();
                $("#totalResults").text(`Total Results: ${response.totalRecords}`);

                response.data.forEach(category => {
                    $("#categoryTableBody").append(`
                        <tr>
                            <td>${category.categoryID}</td>
                            <td>${category.categoryName}</td>
                            <td>${category.categorySymbol}</td>
                            <td>
                                <button class="btn btn-sm btn-primary edit" data-id="${category.categoryID}" data-name="${category.categoryName}" data-symbol="${category.categorySymbol}"><i class="fas fa-edit"></i></button>
                                <button class="btn btn-sm btn-danger delete" data-id="${category.categoryID}"><i class="fas fa-trash"></i></button>
                            </td>
                        </tr>
                    `);
                });

                setupPagination(response.totalRecords);
            },
            error: function () {
                toastr.warning("Error loading data.");
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
        loadCategories();
    });

    $("#searchInput").on("keyup", function () {
        searchTerm = $(this).val();
        currentPage = 1;
        loadCategories();
    });

    $("#pageSize").on("change", function () {
        pageSize = $(this).val();
        currentPage = 1;
        loadCategories();
    });

    $(".sortable").on("click", function () {
        let column = $(this).data("column");
        sortOrder = (sortColumn === column) ? (sortOrder === "asc" ? "desc" : "asc") : "asc";
        sortColumn = column;
        loadCategories();
    });

    // Handle edit
    $(document).on("click", ".edit", function () {
        $("#categoryId").val($(this).data("id"));
        $("#categoryName").val($(this).data("name"));
        $("#categorySymbol").val($(this).data("symbol"));
    });

    // Handle delete
    $(document).on("click", ".delete", function () {
        if (confirm("Are you sure you want to delete this category?")) {
            let categoryId = $(this).data("id");
            $.ajax({
                type: "POST",
                url: "/Categories/delete", // Match the route defined in the controller
                data: { categoryId: categoryId }, // Send the ID as part of the request body
                success: function () {
                    toastr.success("Category deleted successfully!");
                    loadCategories(); // Refresh the list of categories
                },
                error: function () {
                    toastr.warning("Error deleting category.");
                }
            });
        }
    });

    loadCategories();
});
