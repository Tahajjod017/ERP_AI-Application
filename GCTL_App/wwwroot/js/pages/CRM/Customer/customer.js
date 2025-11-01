window.initCustomerForm = function (root) {
    root = root || document;

    //if (window.initCommonFields) initCommonFields(root);



    //const countrySelect = root.querySelector('#CountryID');
    ////alert("customer")

    ////const form = root.querySelector("#customerForm");
    //const saveBtn = root.querySelector("#saveAndExit");


    const countrySelect = root.querySelector("#CountryID");
    if (countrySelect && !countrySelect.dataset.listenerAttached) {
        countrySelect.dataset.listenerAttached = true;
        // 🔥 Smart dropdown parent: modal if inside one, else body
        let dropdownParent = $(countrySelect).closest('.modal');
        if (dropdownParent.length === 0) {
            dropdownParent = $(document.body);
        }

        $(countrySelect).select2({
            placeholder: 'Select Country',
            dropdownParent: dropdownParent,
            ajax: {
                url: '/CreateJobs/GetCountryList',
                dataType: 'json',
                delay: 250,
                data: function (params) {
                    return { search: params.term || '', page: params.page || 1 };
                },
                processResults: function (data, params) {
                    params.page = params.page || 1;
                    return {
                        results: data.results,
                        pagination: { more: data.pagination.more }
                    };
                },
                cache: true
            },
            width: '100%',
        });
    }
    const fields = ["FirstName", "Phone"];

    const saveBtn = root.querySelector("#saveAndExit");
    if (saveBtn && !saveBtn.dataset.listenerAttached) {
        saveBtn.dataset.listenerAttached = true;
        if (saveBtn) {
            saveBtn.addEventListener("click", async function () {
                const form = this.closest("form");
                if (!form) return;

                // Convert FormData → JSON
                const formData = new FormData(form);
                const jsonData = {};
                formData.forEach((value, key) => {
                    // Optional: Convert empty string to null for numbers
                    jsonData[key] = value === "" ? null : value;
                });
                console.log("Customer data (sending):", jsonData);

                try {
                    this.disabled = true;
                    this.textContent = "Saving...";
                    if (validateFields(fields)) {
                        const response = await fetch(form.action, {
                            method: form.method || "POST",
                            headers: {
                                "Accept": "application/json",
                                "Content-Type": "application/json"
                            },
                            body: JSON.stringify(jsonData)
                        });

                        const data = await response.json();
                        console.log("Server response:", data);
                        if (response.ok && data.success) {
                            alert(data.message || "Customer saved successfully!");
                        } else {
                            alert(data.message || "Something went wrong!");
                        }
                    }
                    
                } catch (error) {
                    console.error("Error during fetch:", error);
                    alert("Network or server error");
                } finally {
                    this.disabled = false;
                    this.textContent = "Save & Exit";
                }
            });
        }

    }

    function validateFields(fieldIds) {
        let isValid = true;

        fieldIds.forEach(id => {
            const field = document.getElementById(id);
            if (!field) return;

            const value = field.value?.trim();

            if (!value) {
                field.classList.add("is-invalid");
                field.style.border = "2px solid red";
                isValid = false;
            } else {
                field.classList.remove("is-invalid");
                field.style.border = "";
            }

            // Automatically remove red border when user types
            if (!field.dataset.listenerAttached) {
                field.dataset.listenerAttached = true;
                field.addEventListener("input", () => {
                    if (field.value.trim()) {
                        field.classList.remove("is-invalid");
                        field.style.border = "";
                    }
                });
            }
        });

        return isValid;
    }
    var currentPage = 1;
    var pageSize = 5;
    let currentSortColumn = 'CustomerName';
    let currentSortOrder = 'asc';
    function loadTableData(currentSortColumn, currentSortOrder) {
        var searchTerm = $("#gender-searchInput").val();

        $.ajax({
            url: '/Customers/GetAll',
            method: 'GET',
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                sortColumn: sortColumn,
                sortOrder: sortOrder
            },
            success: function (response) {
                showDev(response);
                var tableBody = $("#gender-tBody");
                tableBody.empty();
                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {
                        var rowIndex = (currentPage - 1) * pageSize + index + 1;
                        tableBody.append(`
                        <tr class="position-static">
                            <td class="text-center text-middle align-middle" style="width: 5%;">
                                <input type="checkbox" class="form-check-input gender-selectItem" data-id="${item.genderID}" />
                            </td>
                            <td class="text-center text-middle align-middle white-space-nowrap ps-0">${rowIndex}</td>
                            <td class="align-middle white-space-nowrap ps-0">${item.genderName}</td>
                            <td class="align-middle text-end white-space-nowrap pe-2">
                                <div class="row g-3">
                                    <a class="btn btn-phoenix-primary btn-icon me-1 fs-10 text-body px-0 gender-bulkDelete" href="#!" id="gender-edit" data-id="${item.genderID}"><i class="fas fa-edit"></i></a>
                                    <a class="btn btn-phoenix-secondary btn-icon fs-10 text-danger px-0 gender-bulkEdit" href="#!" id="gender-single-delete" data-id="${item.genderID}"><span class="fas fa-trash"></span></a>
                                </div>
                            </td>
                        </tr>
                    `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="7" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#gender-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#gender-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            },
            error: function () {
                console.log("Error! Fetching all data.");
            }
        });

        
    }
    loadTableData(currentSortColumn, currentSortOrder);
};
