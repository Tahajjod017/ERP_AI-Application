$(document).ready(function () {

    
    $("select[name='OrganizationID']").on("changed.coreui.multi-select", function () {
        validateOrganization();
    });

    function validateOrganization()
    {
        var orgSelect = $("select[name='OrganizationID']");
        var selectedValues = orgSelect.val();
        var errorSpan = $("#OrganizationID-error");
        if (!selectedValues || selectedValues.length === 0) {
            errorSpan.text("Please select an organization.");
            orgSelect.css('border', '1px solid red');
            return false;
        } else {
            errorSpan.text("");
            orgSelect.css('border', '1px solid #ccc');
            return true;
        }
    }


    $("#PayRollEmpSave").on("click", function (e) {
        e.preventDefault();
        if (!validateOrganization())
        {
            return;
        }
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
                    toastr.success(res.message); 
                    resetPayRollForm();
                    loadTableData();
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

    

    //
    $(document).on('click', '#PayRollEmpUpdate', function (e) {
        e.preventDefault();

        const formData = new FormData();

        formData.append("EmployeeBenefitIDEdit", $("#EmployeeBenefitIDEdit").val());
        formData.append("OrganizationIDEdit", $("#OrganizationIDEdit").val());
        formData.append("HealthInsuranceEdit", $("#HealthInsuranceEdit").val());
        formData.append("IsHealthInsuranceEnabledEdit", $("#IsHealthInsuranceEnabledEdit").is(":checked"));
        formData.append("IsPerformanceBonusEnabledEdit", $("#IsPerformanceBonusEnabledEdit").is(":checked"));
        formData.append("IsFastivalBonusEnabledEdit", $("#IsFastivalBonusEnabledEdit").is(":checked"));
        formData.append("IsProvidentFundEnabledEdit", $("#IsProvidentFundEnabledEdit").is(":checked"));
        formData.append("IsYearEndBonusEnabledEdit", $("#IsYearEndBonusEnabledEdit").is(":checked"));
        formData.append("PerformanceBonusEdit", $("#PerformanceBonusEdit").val());
        formData.append("FastivalBonusRateEdit", $("#FastivalBonusRateEdit").val());
        formData.append("FastivalBonusOnSalaryTypeIDEdit", $("#FastivalBonusOnSalaryTypeIDEdit").val());
        formData.append("FastivalBonusMinimumServiceInMonthEdit", $("#FastivalBonusMinimumServiceInMonthEdit").val());
        formData.append("ProvidentFundEmployeeContrebutionEdit", $("#ProvidentFundEmployeeContrebutionEdit").val());
        formData.append("ProvidentFundOrganizationContrebutionEdit", $("#ProvidentFundOrganizationContrebutionEdit").val());
        formData.append("ProvidentFundOnSalaryTypeIDEdit", $("#ProvidentFundOnSalaryTypeIDEdit").val());
        formData.append("ProvidentFundMinimumServiceYearEdit", $("#ProvidentFundMinimumServiceYearEdit").val());
        formData.append("YearlyEndBonusTypeIDEdit", $("#YearlyEndBonusTypeIDEdit").val());

        // Validation for organization selection
        if (!$("#OrganizationIDEdit").val()) {
            toastr.error("Please select an organization.");
            return;
        }

        $.ajax({
            url: '/EmployeeBenefits/Update',
            type: 'POST',
            data: formData,
            processData: false,  // Required for FormData
            contentType: false,  // Required for FormData
            success: function (res) {
                if (res.success) {
                    toastr.success(res.message || "Employee benefits updated successfully.");
                    resetPayRollEditForm();
                    loadTableData();
                } else {
                    toastr.error(res.message || "Failed to update employee benefits.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error updating Employee Benefits:", error);
                toastr.error("An error occurred while updating employee benefits.");
            }
        });
    });

    //

    //
    //#region Get By Id Data
    $(document).on('click', '#EditEmpBenefits', function () {
        const employeeBenefitID = $(this).data('id');

        $.ajax({
            url: '/EmployeeBenefits/GetByIdEmpBenefits',
            type: 'GET',
            data: { employeeBenefitID: employeeBenefitID },
            dataType: 'json',
            success: function (res) {
                if (res.success && res.data) {
                    var data = res.data;

                    $("#EmployeeBenefitIDEdit").val(data.employeeBenefitID);
                    $('#OrganizationIDEdit').val(data.organizationIDEdit).each(function () {
                        coreui.MultiSelect.getInstance(this)?.update();
                    });
                    $("#HealthInsuranceEdit").val(data.healthInsurance);
                    $("#IsHealthInsuranceEnabledEdit").prop('checked', data.isHealthInsuranceEnabled);
                    $("#IsPerformanceBonusEnabledEdit").prop('checked', data.isPerformanceBonusEnabled);
                    $("#IsFastivalBonusEnabledEdit").prop('checked', data.isFastivalBonusEnabled);
                    $("#IsProvidentFundEnabledEdit").prop('checked', data.isProvidentFundEnabled);
                    $("#IsYearEndBonusEnabledEdit").prop('checked', data.isYearEndBonusEnabled);
                    setFormattedChoiceValue('PerformanceBonusEdit', data.performanceBonus);
                    setFormattedChoiceValue('FastivalBonusRateEdit', data.fastivalBonusRate)
                    choiceManager.setChoiceValue('FastivalBonusOnSalaryTypeIDEdit', data.fastivalBonusOnSalaryTypeID)
                    choiceManager.setChoiceValue('FastivalBonusMinimumServiceInMonthEdit', data.fastivalBonusMinimumServiceInMonth)
                    setFormattedChoiceValue('ProvidentFundEmployeeContrebutionEdit', data.providentFundEmployeeContrebution)
                    setFormattedChoiceValue('ProvidentFundOrganizationContrebutionEdit', data.providentFundOrganizationContrebution)
                    choiceManager.setChoiceValue('ProvidentFundOnSalaryTypeIDEdit', data.providentFundOnSalaryTypeID)
                    choiceManager.setChoiceValue('YearlyEndBonusTypeIDEdit', data.yearlyEndBonusTypeID)
                    choiceManager.setChoiceValue('ProvidentFundMinimumServiceYearEdit', data.providentFundMinimumServiceYear)
                    console.log("Form populated with employee benefits data.");
                } else {
                    toastr.error(res.message || "Failed to load employee benefits.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error fetching Employee Benefits:", error);
                toastr.error("An error occurred while fetching employee benefits.");
            }
        });

    });

    function setFormattedChoiceValue(fieldName, value) {
        const formattedValue = Number(value).toFixed(2);
        choiceManager.setChoiceValue(fieldName, formattedValue);
    }

    //#endregion
    // for RESET Button

    function resetPayRollForm() {

        const branchSelect = document.getElementById('OrganizationID');
        const branchInstance = coreui.MultiSelect.getInstance(branchSelect);
        if (branchInstance) {
            branchInstance.deselectAll();
        }
        choiceManager.resetChoice('ProvidentFundEmployeeContrebution',
            'FastivalBonusMinimumServiceInMonth',
            'PerformanceBonus', 'FastivalBonusRate',
            'FastivalBonusOnSalaryTypeID', 'YearlyEndBonusTypeID',
            'ProvidentFundOnSalaryTypeID', 'ProvidentFundOrganizationContrebution',
            'ProvidentFundOnSalaryTypeID', 'ProvidentFundMinimumServiceYear')
        // Reset all input fields
        $("input[name='HealthInsurance']").val("");
       
        // Reset all checkboxes to unchecked
        $("#IsHealthInsuranceEnabled").prop('checked', false);
        $("#IsPerformanceBonusEnabled").prop('checked', false);
        $("#IsFastivalBonusEnabled").prop('checked', false);
        $("#IsProvidentFundEnabled").prop('checked', false);
        $("#IsYearEndBonusEnabled").prop('checked', false);

        $("#OrganizationID-error").text("");
        $("select[name='OrganizationID']").css('border', '1px solid #ccc');
        console.log("Form reset successfully");
    }

    $("#ResetBtn").on("click", function (e) {
        e.preventDefault();
        resetPayRollForm();
        resetPayRollEditForm();
    });
    function resetPayRollEditForm() {
        // Deselect all organizations
        const branchSelect = document.getElementById('OrganizationIDEdit');
        const branchInstance = coreui.MultiSelect.getInstance(branchSelect);
        if (branchInstance) {
            branchInstance.deselectAll();
        }

        // Reset all Choice dropdowns
        choiceManager.resetChoice(
            'PerformanceBonusEdit',
            'FastivalBonusRateEdit',
            'FastivalBonusOnSalaryTypeIDEdit',
            'FastivalBonusMinimumServiceInMonthEdit',
            'YearlyEndBonusTypeIDEdit',
            'ProvidentFundOnSalaryTypeIDEdit',
            'ProvidentFundEmployeeContrebutionEdit',
            'ProvidentFundOrganizationContrebutionEdit',
            'ProvidentFundMinimumServiceYearEdit'
        );

        // Reset input fields
        $("#HealthInsuranceEdit").val("");

        // Reset checkboxes
        $("#IsHealthInsuranceEnabledEdit").prop('checked', false);
        $("#IsPerformanceBonusEnabledEdit").prop('checked', false);
        $("#IsFastivalBonusEnabledEdit").prop('checked', false);
        $("#IsProvidentFundEnabledEdit").prop('checked', false);
        $("#IsYearEndBonusEnabledEdit").prop('checked', false);

        // Reset validation error
        $("#OrganizationIDEdit-error").text("");
        $("select[name='OrganizationIDEdit']").css('border', '1px solid #ccc');

        console.log("Edit form reset successfully");
    }

    // Bind reset button
    $("#ResetBtnEdit").on("click", function (e) {
        e.preventDefault();
        resetPayRollEditForm();
    });

    // Table Datum

   

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
                            <td class="helthInsurance align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.healthInsurance || ''} tk</td>
                            <td class="providantFundEC align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.providentFundEmployeeContrebution}%</td>
                            <td class="providantFundOC align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.providentFundOrganizationContrebution}%</td>
                            <td class="providantFundT align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.providentFundMinimumServiceYear} Years</td>
                            <td class="providantFundS align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.providentFundOnSalaryTypeName || ''}</td>
                            <td class="FestivalBonusR align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.fastivalBonusRate || ''}</td>
                            <td class="FestivalBonusS align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.fastivalBonusOnSalaryTypeName || ''}</td>
                            <td class="performanceBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1"> ${item.performanceBonus || ''}%</td>
                            <td class="yearEndBonus align-middle white-space-nowrap ps-4 fw-semibold text-body py-1">${item.yearlyEndBonusTypeName || ''}</td>
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
