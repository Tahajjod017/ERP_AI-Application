$(document).ready(function () {

   

    $("#PayRollEmpSave").on("click", function (e) {
        e.preventDefault();

        var formData = new FormData();

        // ====== Dropdowns / Inputs ======
        formData.append("OrganizationID", $("select[name='OrganizationID']").val());
        formData.append("HealthInsurance", $("input[name='HealthInsurance']").val());
        formData.append("FastivalBonusRate", $("select[name='FastivalBonusRate']").val());
        formData.append("FastivalBonusOnSalaryTypeID", $("select[name='FastivalBonusOnSalaryTypeID']").val());
        formData.append("YearlyEndBonusTypeID", $("select[name='YearlyEndBonusTypeID']").val());
        formData.append("ProvidentFundEmployeeContrebution", $("select[name='ProvidentFundEmployeeContrebution']").val());
        formData.append("ProvidentFundOrganizationContrebution", $("select[name='ProvidentFundOrganizationContrebution']").val());
        formData.append("ProvidentFundOnSalaryTypeID", $("select[name='ProvidentFundOnSalaryTypeID']").val());
        formData.append("ProvidentFundMinimumServiceYear", $("select[name='ProvidentFundMinimumServiceYear']").val());
        formData.append("PerformanceBonus", $("select[name='PerformanceBonus']").val());
        formData.append("FastivalBonusMinimumServiceInMonth", $("select[name='FastivalBonusMinimumServiceInMonth']").val());
        // ====== Checkboxes (bool?) ======
        formData.append("IsHealthInsuranceEnabled", $("#IsHealthInsuranceEnabled").is(":checked"));
        formData.append("IsPerformanceBonusEnabled", $("#IsPerformanceBonusEnabled").is(":checked"));
        formData.append("IsFastivalBonusEnabled", $("#IsFastivalBonusEnabled").is(":checked"));
        formData.append("IsProvidentFundEnabled", $("#IsProvidentFundEnabled").is(":checked"));
        formData.append("IsYearEndBonusEnabled", $("#IsYearEndBonusEnabled").is(":checked"));
     
        $.ajax({
            url: '/EmployeeBenefits/Create',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (res) {
             
                if (res.success) {
                    toastr.success(res.message); // "Save Successfully"
                    resetPayRollForm();
                } else {
                    toastr.error(res.message); 
                }
            },
            error: function (xhr, status, error) {
                console.error("Error:", error);
                toastr.error(res.message); 
            }
        });
    });

    // for RESET Button

    function resetPayRollForm() {

        const branchSelect = document.getElementById('OrganizationID');
        const branchInstance = coreui.MultiSelect.getInstance(branchSelect);
        if (branchInstance) {
            branchInstance.deselectAll();
        }
        choiceManager.resetChoice('FastivalBonusMinimumServiceInMonth','PerformanceBonus','FastivalBonusRate', 'FastivalBonusOnSalaryTypeID', 'YearlyEndBonusTypeID', 'ProvidentFundOnSalaryTypeID', 'ProvidentFundOrganizationContrebution','ProvidentFundOnSalaryTypeID','ProvidentFundMinimumServiceYear')
        // Reset all input fields
        $("input[name='HealthInsurance']").val("");
       
        // Reset all checkboxes to unchecked
        $("#IsHealthInsuranceEnabled").prop('checked', false);
        $("#IsPerformanceBonusEnabled").prop('checked', false);
        $("#IsFastivalBonusEnabled").prop('checked', false);
        $("#IsProvidentFundEnabled").prop('checked', false);
        $("#IsYearEndBonusEnabled").prop('checked', false);
        console.log("Form reset successfully");
    }

    $("#ResetBtn").on("click", function (e) {
        e.preventDefault();
        resetPayRollForm();
        
    });

    // Table Datum

    //#region Get By Id Data
    $(document).on('click', '#EditEmpBenefits', function () {
        const employeeBenefitID = $(this).data('id');

        $.ajax({
            url: '/EmployeeBenefits/GetByIdEmpBenefits',
            type: 'GET',
            data: { employeeBenefitID },
            dataType: 'json',
            success: function (data) {
                console.log("Get By ID:", data);
              
            },
            error: function (xhr, status, error) {
                console.error("Error fetching Employee Benefits:", error);
            }
        });
    });

    //#endregion

    //#region Delete Soft Leave Request
    $(document).on('click', '#SoftDeletePayRollEmpBenefitsDelete-singleDelBtn', function () {
        var id = $(this).data('id');
        if (id) {
            showDeleteModal(function () {
                $.ajax({
                    url: '/EmployeeBenefits/SoftDeletePayRollEmpRequest',
                    method: 'POST',
                    data: { ids: [id] },
                    success: function (response) {

                        if (response.success) {
                            toastr.success(response.message);
                            loadTableData();
                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function () {
                        toastr.error("Error occurred while deleting.");
                    }
                });
            });
        } else {
            toastr.error("Invalid action.");
        }
    });
    //#endregion

    // #region  Data Table for Peresonal
    var currentPage = 1;
    var pageSize = 5;

    $('#payRollEmp-pageSizeSelect').on('change', function () {
        var selectedSize = $(this).val();

        if (selectedSize) {
            pageSize = parseInt(selectedSize, 10);
            currentPage = 1;
            loadTableData();
        }
    });

    $(document).ready(function () {
        loadTableData();

        $("#payRollEmp-searchInput").on("input", function () {
            currentPage = 1;
            loadTableData();
        });

        $("#payRollEmp-prevPageBtn").on('click', function () {
            if (currentPage > 1) {
                currentPage--;
                loadTableData();
            }
        });

        $("#payRollEmp-nextPageBtn").on('click', function () {
            currentPage++;
            loadTableData();
        });
    });
    let currentSortColumn = '';
    let currentSortOrder = '';

    $('th.sort').on('click', function () {
        const column = $(this).data('sort');
        debugger
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

    $(document).on("change", "#StatusIDFilterDD,#LeaveTypeIDFilterDD", function () {

        currentPage = 1;
        loadTableData();
    });

    $('#OrganizationIDD').on('changed.coreui.multi-select', function () {
        currentPage = 1;
        loadTableData(); // Make AJAX call or reload the table
    });

    // Filtering according to formdate to ToDate
 
    function loadTableData(currentSortColumn, currentSortOrder) {
        var searchTerm = $("#payRollEmp-searchInput").val();
        const organizationId = $('#OrganizationIDD').val();
    
        $.ajax({
            url: '/EmployeeBenefits/GetAllTableListAsync',
            method: 'GET',
            traditional: true,
            data: {
                pageNumber: currentPage,
                pageSize: pageSize,
                searchTerm: searchTerm,
                currentSortColumn: currentSortColumn,
                currentSortOrder: currentSortOrder,
                organizationId: organizationId,
            },
            success: function (response) {



                console.log("Datassssss", response);
                var tableBody = $("#PayRollEmployeeBnefits-body");
                tableBody.empty();
                var totalItems = response.paginationInfo.totalItems;

                if (response.data.length > 0) {
                    response.data.forEach(function (item, index) {

                        if (currentSortOrder === 'asc') {
                            rowIndex = (currentPage - 1) * pageSize + index + 1;
                        } else {
                            rowIndex = totalItems - ((currentPage - 1) * pageSize + index);
                        }
                    
                        tableBody.append(`
                      <tr class="hover-actions-trigger btn-reveal-trigger position-static">
                            <td class="companyName align-middle white-space-nowrap ps-5 fw-semibold text-body py-1">
                                <span>${item.organizationName}</span>
                            </td>
                            <td class="helthInsurance align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.healthInsurance} tk</td>
                            <td class="providantFundEC align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">-%</td>
                            <td class="providantFundOC align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">-%</td>
                            <td class="providantFundT align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">-Years</td>
                            <td class="providantFundS align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.providentFundOnSalaryTypeName}</td>
                            <td class="FestivalBonusR align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.fastivalBonusRate}</td>
                            <td class="FestivalBonusS align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.fastivalBonusOnSalaryTypeName}</td>
                            <td class="performanceBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1"> ${item.performanceBonus}%</td>
                            <td class="yearEndBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.yearlyEndBonusTypeName}</td>
                            <td class="align-middle white-space-nowrap text-end pe-3">
                              <div class="d-flex justify-content-end align-items-center">
                                  <a href="#" class="nav-item mx-2" title="Edit" data-bs-toggle="modal" data-bs-target="#edit_benefits" id="EditEmpBenefits" data-id="${item.employeeBenefitID}">
                                      <i class="fas fa-edit text-black"></i>
                                  </a>

                                  <a href="#" title="Delete" data-id="${item.employeeBenefitID}"
                                     class="btn btn-outline-light btn-icon"
                                     id="SoftDeletePayRollEmpBenefitsDelete-singleDelBtn">
                                      <i class="far fa-trash-alt text-black"></i>
                                  </a>
                              </div>
                           </td>

                        </tr>
                   `);
                    });
                } else {
                    tableBody.append('<tr><td colspan="10" class="text-center">No data available</td></tr>');
                }

                var paginationInfo = response.paginationInfo;

                $("#payRollEmp-paginationInfo").text(`Showing ${paginationInfo.startItem} to ${paginationInfo.endItem} Items of ${paginationInfo.totalItems}`);
                $("#payRollEmp-totalCount").text(`(${paginationInfo.totalItems})`);

                updatePagination(paginationInfo.pageNumbers, paginationInfo.currentPage, paginationInfo.totalPages);
            }
        });
    }

    function updatePagination(pageNumbers, currentPage, totalPages) {
        const paginationLinks = $("#payRollEmp-paginationLinks");
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
        $("#payRollEmp-prevPageBtn").prop('disabled', currentPage === 1);
        $("#payRollEmp-nextPageBtn").prop('disabled', currentPage === totalPages);
    }

    $(document).on('click', '.page-btn', function () {
        const page = $(this).data('page');
        currentPage = page;
        loadTableData();
    });
    //#endregion
    //




});
