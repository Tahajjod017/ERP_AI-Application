$(document).ready(function () {
    let currentPage = 1;
    let pageSize = $("#pageSize").val();
    let searchTerm = "";
    let sortColumn = "subCategoryId";
    let sortOrder = "asc";
    let selectedCategoryId = "";

    // Load categories into dropdown
    function loadCategories() {
        $.ajax({
            url: "/categories/getall",
            type: "GET",
            success: function (response) {
                $("#categoryId").empty();
                $("#categoryId").append('<option value="" selected>Select Category</option>');

                $("#categoryFilter").empty();
                $("#categoryFilter").append('<option value="">All Categories</option>');

                response.forEach(category => {
                    $("#categoryId").append(`<option value="${category.categoryID}">${category.categoryName}</option>`);
                    $("#categoryFilter").append(`<option value="${category.categoryID}">${category.categoryName}</option>`);
                });
            },
            error: function () {
                //toastr.warning("Error loading categories.");
            }
        });
    }

    // Handle form submission via AJAX
    $("#subCategoryForm").on("submit", function (e) {
        e.preventDefault();

        if (!$("#categoryId").val()) {
            //toastr.warning("Please select a category");
            return;
        }

        if (!$("#subCategoryName").val()) {
            //toastr.warning("Please enter a sub-category name");
            return;
        }

        var formData = {
            subCategoryId: $("#subCategoryId").val() || null,
            categoryId: parseInt($("#categoryId").val()),
            subCategoryName: $("#subCategoryName").val()
        };

        $.ajax({
            type: "POST",
            url: "/subcatagory/save",
            data: JSON.stringify(formData),
            contentType: "application/json",
            success: function (response) {
                if (response.success) {
                    //toastr.success(response.message);
                    resetForm();
                    loadSubCategories();
                } else {
                    //toastr.warning(response.message);
                }
            },
            error: function () {
                //toastr.warning("Error saving sub-category. Please try again.");
            }
        });
    });

    function resetForm() {
        $("#subCategoryId").val('');
        $("#subCategoryName").val('');
        $("#categoryId").val('');
    }

    $("#resetBtn").on("click", function (e) {
        e.preventDefault();
        resetForm();
    });

    function loadSubCategories() {
        $.ajax({
            url: "/subcategories/get",
            type: "GET",
            data: {
                page: currentPage,
                pageSize,
                search: searchTerm,
                sortColumn,
                sortOrder,
                categoryId: selectedCategoryId || null
            },
            success: function (response) {
                $("#subCategoryTableBody").empty();
                $("#totalResults").text(`Total Results: ${response.totalRecords}`);

                response.data.forEach(subCategory => {
                    $("#subCategoryTableBody").append(`
                        <tr>
                            <td>${subCategory.subCategoryID}</td>
                            <td>${subCategory.categoryName}</td>
                            <td>${subCategory.subCategoryName}</td>
                            <td>
                                <button class="btn btn-sm btn-primary edit" 
                                    data-id="${subCategory.subCategoryID}" 
                                    data-category-id="${subCategory.categoryID}" 
                                    data-name="${subCategory.subCategoryName}">
                                    <i class="fas fa-edit"></i>
                                </button>
                                <button class="btn btn-sm btn-danger delete" data-id="${subCategory.subCategoryID}">
                                    <i class="fas fa-trash"></i>
                                </button>
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
        loadSubCategories();
    });

    $("#searchInput").on("keyup", function () {
        searchTerm = $(this).val();
        currentPage = 1;
        loadSubCategories();
    });

    $("#pageSize").on("change", function () {
        pageSize = $(this).val();
        currentPage = 1;
        loadSubCategories();
    });

    $("#categoryFilter").on("change", function () {
        selectedCategoryId = $(this).val();
        currentPage = 1;
        loadSubCategories();
    });

    $(".sortable").on("click", function () {
        let column = $(this).data("column");
        sortOrder = (sortColumn === column) ? (sortOrder === "asc" ? "desc" : "asc") : "asc";
        sortColumn = column;
        loadSubCategories();
    });

    // Handle edit
    $(document).on("click", ".edit", function () {
        $("#subCategoryId").val($(this).data("id"));
        $("#categoryId").val($(this).data("category-id"));
        $("#subCategoryName").val($(this).data("name"));
    });

    // Handle delete
    $(document).on("click", ".delete", function () {
        if (confirm("Are you sure you want to delete this sub-category?")) {
            let subCategoryId = $(this).data("id");
            $.ajax({
                type: "POST",
                url: "/subcategories/delete",
                data: { subCategoryId: subCategoryId },
                success: function (response) {
                    if (response.success) {
                        //toastr.success(response.message);
                        loadSubCategories();
                    } else {
                        //toastr.warning(response.message);
                    }
                },
                error: function () {
                    //toastr.warning("Error deleting sub-category.");
                }
            });
        }
    });

    // Initial loads
    loadCategories();
    loadSubCategories();
});
