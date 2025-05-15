
$(document).ready(function () {

    getSKU()

    //#region table

    loadProducts(1, 10, '', 'ProductName', 'asc');

    function loadProducts(page, pageSize, search, sortColumn, sortDirection, category, subCategory, unit) {
        $.ajax({
            url: '/Product/GetProducts',
            type: 'GET',
            data: {
                page: page,
                pageSize: pageSize,
                search: search,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                category: category,
                subCategory: subCategory,
                unit: unit
            },
            success: function (response) {
                populateTable(response.data);
                setupPagination(response.totalRecords, pageSize, page);
            }
        });
    }

    function populateTable(products) {
        console.log(products)
        let tbody = $("#product-summary-table-body");
        tbody.empty();
        $.each(products, function (index, product) {
            let row = `<tr class="position-static">
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.sku}</td>
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.productName}</td>
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.productType}</td>
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.categoryName}</td>
                      
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.modelName}</td>
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.brandName}</td>
                      
                        <td class="align-middle white-space-nowrap ps-0 productId">${product.unitName}</td>
                        
                    </tr>`;
            tbody.append(row);
        });
    }


    //<td class="align-middle white-space-nowrap ps-0 productId">
    //    <div class="row g-3">
    //        <button data-id="${product.productId}" class="btn btn-phoenix-secondary btn-icon me-1 fs-10 text-body px-0" type="button" data-bs-toggle="modal" data-bs-target="#modelForEditProduct"><span class="fas fa-edit"></span></button>
    //        <button data-id="${product.productId}" class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0"><span class="fas fa-trash"></span></button>
    //    </div>


    //</td>



    function setupPagination(totalRecords, pageSize, currentPage) {
        let totalPages = Math.ceil(totalRecords / pageSize);
        let pagination = $(".pagination");
        pagination.empty();
        for (let i = 1; i <= totalPages; i++) {
            let activeClass = (i === currentPage) ? 'active' : '';
            pagination.append(`<li class="page-item ${activeClass}"><a class="page-link" href="#">${i}</a></li>`);
        }
    }

    // Pagination click event
    $(document).on('click', '.page-link', function (e) {
        e.preventDefault();
        let page = parseInt($(this).text());
        loadProducts(page, $("#pageSize").val(), $("#search").val());
    });

    // Page size change
    $("#pageSize").change(function () {
        loadProducts(1, $(this).val(), $("#search").val());
    });

    // Search functionality
    $("#search").on("keyup", function () {
        loadProducts(1, $("#pageSize").val(), $(this).val());
    });

    // Filter event handlers
    $(".filter-dropdown").change(function () {
        loadProducts(1, $("#pageSize").val(), $("#search").val(), null, null,
            $("#category").val(), $("#subCategory").val(), $("#unit").val());
    });

    // Sorting
    $(".sort").click(function () {
        let sortColumn = $(this).data("sort");
        let sortDirection = $(this).hasClass("asc") ? "desc" : "asc";
        $(".sort").removeClass("asc desc");
        $(this).addClass(sortDirection);
        loadProducts(1, $("#pageSize").val(), $("#search").val(), sortColumn, sortDirection);
    });

    //#endregion

    //#region Create Form



    $("#categoryDropdown").on("change", function () {
        let categoryId = $(this).val();
        if (categoryId) {
            loadSubCategories(categoryId);
        } else {
            $("#subCategoryDropdown").empty().append('<option selected="selected">Select Sub Category</option>');
        }
    });


    //loadDropdownData("categories", "#categoryDropdown");
    //loadDropdownData("subcategories", "#subCategoryDropdown");
    //loadDropdownData("units", "#unitDropdown");

    function loadDropdownData(endpoint, dropdownId) {
        $.ajax({
            url: `/${endpoint}/get`, // Adjust API endpoint as needed
            type: "GET",
            dataType: "json",
            success: function (data) {
                let dropdown = $(dropdownId);
                dropdown.empty();
                dropdown.append('<option selected="selected">Select</option>');
                $.each(data, function (index, item) {
                    dropdown.append(`<option value="${item.id}">${item.name}</option>`);
                });
            },
            error: function () {
                console.log(`Failed to load ${endpoint}`);
            }
        });
    }



    function loadSubCategories(categoryId) {
        $.ajax({
            url: '/product/subcategories',
            type: "GET",
            dataType: "json",
            data: { categoryId: categoryId },
            success: function (data) {

                console.log(data)

                let dropdown = $("#subCategoryDropdown");
                dropdown.empty();
                dropdown.append('<option selected="selected">Select Sub Category</option>');
                $.each(data, function (index, item) {
                    dropdown.append(`<option value="${item.subCategoryID}">${item.subCategoryName}</option>`);
                });
            },
            error: function () {
                console.log("Failed to load subcategories");
            }
        });
    }


    $("#saveProductBtn").click(function (e) {
        e.preventDefault();

        // Collect form data
        let productData = {
            CategoryId: parseInt($("#categoryDropdown").val(), 10),
            SubCategoryId: parseInt($("#subCategoryDropdown").val(), 10),
            ProductTypeId: $("#productTypeDropdown").val(), // Assuming this remains a string
            ProductName: $("#productName").val(),
            Model: $("#model").val(),
            Brand: $("#brand").val(),
            SKU: $("#sku").val(),
            UnitId: parseInt($("#unitDropdown").val(), 10)
        };
        
        console.log(productData)

        // Validate required fields
        if (!productData.CategoryId || !productData.SubCategoryId || !productData.ProductName || !productData.SKU || !productData.UnitId) {
            alert("Please fill all required fields.");
            return;
        }

        // Send AJAX POST request
        $.ajax({
            url: "/product/add-product",  // Change based on your actual API route
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(productData),
            success: function (response) {
                alert(response.message);
                console.log("Product saved:", response.product);
                //$("#productForm")[0].reset(); // Reset form after successful save
            },
            error: function (xhr) {
                console.error("Error saving product:", xhr.responseText);
                alert("Error saving product. Please try again.");
            }
        });
    });


    //#endregion

    
    function getSKU() {
        $.ajax({
            url: '/get/SKU',  // Replace with your actual API endpoint
            type: 'GET',
           
            success: function (response) {
                if (response) {
                    $('#sku').val(response); // Update input field
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching SKU:', error);
            }
        });
    }
});

