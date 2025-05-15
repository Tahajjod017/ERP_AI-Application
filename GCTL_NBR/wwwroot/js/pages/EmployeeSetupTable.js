$(document).ready(function () {


    //#region data tablw

    loadEmployees(1);

    function loadEmployees(page) {
        let search = $("#empTblSearch").val();
        let pageSize = $("#pageSizeDropdown").val() || 10;
        let sortColumn = $(".sort.active").data("sort") || "empId";
        let sortDirection = $(".sort.active").hasClass("asc") ? "asc" : "desc";
        let designation = $("#designationDropdown").val()

        $.ajax({
            url: "/Employees/GetEmployeeTable",
            type: "GET",
            data: {
                page: page,
                pageSize: pageSize,
                search: search,
                sortColumn: sortColumn,
                sortDirection: sortDirection,
                designation: designation
            },
            success: function (response) {
                if (response) {

                    populateTable(response.data, response.totalEmpCount);
                    setupPagination(response.totalRecords, page, pageSize);
                    updateResultsInfo(response.data.length, response.totalRecords);

                    //if (response.totalEmpCount > 0) {
                    //    $("#displayOrderBtn").show();
                    //}

                    
                }
                
            }
        });
    }

    
    let displayOrderChanged = false;

    function populateTable(employees, designation) {

        console.log('employees', employees);

        let tableBody = $("#employeeSetup-summary-table-body");
        tableBody.empty();


        // Hide or show the Display Order column header based on designation
        if (designation != 0) {
            $("table thead th:nth-child(10)").show(); // Show Display Order header (10th column)
        } else {
            $("table thead th:nth-child(10)").hide(); // Hide Display Order header (10th column)
        }




        // Start building the table dynamically
        $.each(employees, function (index, employee) {
            console.log('employee', employee);
            let row = `
        <tr data-employee-id="${employee.employeeID}">
            <td class="align-middle">${employee.employeeCode}</td>
            <td class="align-middle white-space-nowrap ps-0 empName">
                <a class="fw-bold fs-8" type="button" data-bs-toggle="offcanvas" 
                   data-bs-target="#offcanvasRightVEI" aria-controls="offcanvasRightVEI">${employee.employeeName}</a>
            </td>
            <td class="align-middle">${employee.genderName}</td>
            <td class="align-middle">${employee.dob}</td>
            <td class="align-middle">${employee.mobile}</td>
            <td class="align-middle">${employee.email}</td>
            <td class="align-middle">${employee.departmentName}</td>
            <td class="align-middle">${employee.designationName}</td>
            <td class="align-middle">${employee.address}</td>
        `;
            // Add "DISPLAY ORDER" column conditionally
            if (designation != 0) {
                row += `
            <td class="align-middle">
                 <select class="form-select displayDropdown" data-original-value="${employee.displayOrder || ''}">
                     <option value="" selected="selected">Select Display Order</option>
                 </select>
            </td>`;
            }
            row += `
        <td class="align-middle">
            
        </td>
        </tr>`;
            tableBody.append(row);

            if (designation != 0) {
                let displayDropdown = $(".displayDropdown").last();
                for (let i = 1; i <= designation; i++) {
                    displayDropdown.append($('<option>', {
                        value: i,
                        text: 'Position ' + i
                    }));
                }
                // Try to set the value to employee.displayOrder
                if (employee.displayOrder) {
                    displayDropdown.val(employee.displayOrder);
                } else {
                    displayDropdown.val("");
                }
                $("#displayOrderBtn").show();
            } else {
                $("#displayOrderBtn").hide();
            }
            
        });

        // Add event listener for display order changes
        $(".displayDropdown").on("change", function () {
            let newValue = $(this).val();
            let originalValue = $(this).data("original-value");

            if (newValue !== originalValue) {
                displayOrderChanged = true;

                // If a position is selected that's already in use, update others
                if (newValue !== "") {
                    updateOtherPositions($(this), newValue);
                }
            }
        });
    }

    // Function to update other positions when one is changed
    function updateOtherPositions(changedDropdown, newPosition) {
        let currentRow = changedDropdown.closest("tr");
        let employeeId = currentRow.data("employee-id");
        let maxPosition = parseInt($(".displayDropdown option").last().val());

        // Find all other dropdowns with the same value and adjust them
        $(".displayDropdown").each(function () {
            let thisRow = $(this).closest("tr");
            let thisEmployeeId = thisRow.data("employee-id");

            // Skip the dropdown that was just changed
            if (thisEmployeeId === employeeId) {
                return;
            }

            let currentValue = parseInt($(this).val());

            // If this dropdown has the same value as the new position
            if (currentValue === parseInt(newPosition)) {
                // Check if the next position is available
                let nextPosition = currentValue + 1;

                // If next position exceeds maximum, set to empty (default)
                if (nextPosition > maxPosition) {
                    $(this).val("");
                    $(this).trigger("change");
                } else {
                    $(this).val(nextPosition);
                    $(this).trigger("change"); // This will trigger another updateOtherPositions call
                }
            }
        });
    }

   
    // Add event handler for the display order button
    $("#displayOrderBtn").on("click", function () {
        // Collect ALL employee IDs and their display orders, including default values
        let employeeDisplayOrders = [];
        $("#employeeSetup-summary-table-body tr").each(function () {
            let employeeId = $(this).data("employee-id");
            let displayOrder = $(this).find(".displayDropdown").val();

            // Push all employees regardless of whether they have a display order
            employeeDisplayOrders.push({
                employeeId: employeeId,
                displayOrder: displayOrder || 0 // Send empty string if display order is null/undefined
            });
        });

        console.log("Employee Display Orders:", employeeDisplayOrders);

        // Send to controller
        $.ajax({
            url: "/Employee/UpdateDisplayOrders",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(employeeDisplayOrders),
            success: function (response) {
                if (response.success) {
                    toastr.success("Display orders updated successfully.");
                    displayOrderChanged = false;

                    // Update original values
                    $(".displayDropdown").each(function () {
                        $(this).data("original-value", $(this).val());
                    });
                } else {
                    toastr.warning(response.message);
                }
            },
            error: function () {
                toastr.warning("An error occurred while updating display orders.");
            }
        });
    });



    // Update the designation dropdown change handler
    $("#designationDropdown").on("change", function () {
        if (displayOrderChanged) {
            // Show confirmation alert
            if (confirm("You have unsaved changes to display orders. Do you want to continue without saving?")) {
                loadEmployees(1);
                displayOrderChanged = false;
            } else {
                // Revert dropdown change
                $(this).val($(this).data("previous-value"));
                return;
            }
        } else {
            loadEmployees(1);
        }

        // Store current value for potential revert
        $(this).data("previous-value", $(this).val());
    });


    function updateResultsInfo(shownRecords, totalRecords) {
        $("#showing-results").text(shownRecords);
        $("#total-results").text(totalRecords);
    }

    function setupPagination(totalRecords, currentPage, pageSize) {
        let totalPages = Math.ceil(totalRecords / pageSize);
        let pagination = $(".pagination");
        pagination.empty();

        // Determine which page numbers to show
        let startPage = Math.max(1, currentPage - 2);
        let endPage = Math.min(totalPages, currentPage + 2);

        // Adjust to show 5 pages if possible
        if (endPage - startPage < 4) {
            if (startPage === 1) {
                endPage = Math.min(5, totalPages);
            } else if (endPage === totalPages) {
                startPage = Math.max(1, totalPages - 4);
            }
        }

        // First page
        if (startPage > 1) {
            pagination.append(`<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`);
            if (startPage > 2) {
                pagination.append(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
            }
        }

        // Middle pages
        for (let i = startPage; i <= endPage; i++) {
            let activeClass = i === currentPage ? "active" : "";
            pagination.append(`<li class="page-item ${activeClass}"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`);
        }

        // Last page
        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                pagination.append(`<li class="page-item disabled"><span class="page-link">...</span></li>`);
            }
            pagination.append(`<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`);
        }

        // Set up event handlers for pagination
        $(".page-link[data-page]").click(function (e) {
            e.preventDefault();
            let page = parseInt($(this).data("page"));
            loadEmployees(page);
        });

        // Previous and Next buttons
        $("[data-list-pagination='prev']").off("click").on("click", function (e) {
            e.preventDefault();
            if (currentPage > 1) {
                loadEmployees(currentPage - 1);
            }
        });

        $("[data-list-pagination='next']").off("click").on("click", function (e) {
            e.preventDefault();
            if (currentPage < totalPages) {
                loadEmployees(currentPage + 1);
            }
        });
    }

    // Event handlers
    $("#empTblSearch").on("keyup", function () {
        loadEmployees(1);
    });

    $("#pageSizeDropdown").on("change", function () {
        loadEmployees(1);
    });

    //$("#designationDropdown").on("change", function () {
    //    loadEmployees(1);
        
    //});

    $(document).on("click", ".sort", function () {
        $(".sort").removeClass("active");
        $(this).addClass("active");
        $(this).toggleClass("asc desc");
        loadEmployees(1);
    });

    $(document).on("click", ".delete-btn", function () {
        let empId = $(this).data("id");
        if (confirm("Are you sure you want to delete this employee?")) {
            $.ajax({
                url: `/Employees/Delete/${empId}`,
                type: "POST",
                success: function () {
                    loadEmployees(1);
                }
            });
        }
    });

    //#endregion


    //#region profile Table

    // Add this event listener after your table is populated
    $(document).on('click', '.empName a', function () {
        let employeeCode = $(this).closest('tr').find('td:first').text(); // Assuming first column has employee code

        //// Make AJAX call to get employee details
        //$.ajax({
        //    url: '/employeeSetup/getEmployeeDetails',
        //    type: 'GET',
        //    data: { employeeCode: employeeCode },
        //    beforeSend: function () {
        //        // Optional: Show loading indicator
        //       // $('#offcanvasRightVEI').find('.card-body').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
        //    },
        //    success: function (data) {
        //        // Populate the offcanvas with employee data
        //        console.log('525635', data);

        //        //// Header information
        //        //$('#offcanvasRightVEI .card-body h3').text(data.employeeName);
        //        //$('#offcanvasRightVEI .card-body .fs-8').text('(' + data.designationName + ')');


        //        //// Correct way to update text while preserving the Feather icons
        //        //$('#offcanvasRightVEI [data-feather="phone"]').parent().html('<span data-feather="phone"></span> ' + data.mobile);
        //        //$('#offcanvasRightVEI [data-feather="mail"]').parent().html('<span data-feather="mail"></span> ' + data.email);
        //        //$('#offcanvasRightVEI [data-feather="home"]').parent().html('<span data-feather="home"></span> ' + data.address);


        //        // Header information

        //        $('#employee-name').text(data.employeeName);
        //        $('#employee-designation').text('(' + data.designationName + ')');

        //        // Contact information
        //        $('#phone-text').text(data.mobile);
        //        $('#email-text').text(data.email);
        //        $('#address-text').text(data.address);


        //        // Profile picture if available
        //        if (data.profilePicture) {
        //            $('#offcanvasRightVEI .rounded-circle').attr('src', data.employeeCode);
        //        }

        //        // Personal information table
        //        $('#offcanvasRightVEI td:contains("Office Name") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Branch") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Employee ID") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("First Name") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Last Name") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Father\'s Name") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Mother\'s Name") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Date Of Birth") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Birth Certificate") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Place Of Birth") + td + td h5').text(data.employeeCode);
        //        $('#offcanvasRightVEI td:contains("Gender") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("Blood Group") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("Nationality") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("National ID") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("Religion") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("Marrital Status") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("Card No") + td + td h5').text(data.genderName);
        //        $('#offcanvasRightVEI td:contains("TIN No") + td + td h5').text(data.genderName);

        //        // Re-initialize Feather icons if needed
        //        if (typeof feather !== 'undefined') {
        //            feather.replace();
        //        }
        //    },
        //    error: function (xhr, status, error) {
        //        console.error("Error fetching employee data:", error);
        //        $('#offcanvasRightVEI').find('.card-body').html('<div class="alert alert-danger">Error loading employee data</div>');
        //    }
        //});



        // Make AJAX call to get employee details


        $.ajax({
            url: '/employeeSetup/Personal/getEmployeeDetails',
            type: 'GET',
            data: { employeeCode: employeeCode },
            beforeSend: function () {
                // Optional: Show loading indicator
                // $('#offcanvasRightVEI').find('.card-body').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
            },
            success: function (data) {
                // Populate the offcanvas with employee data
                console.log('525635', data);

                var fullName = data.firstName + ' ' + data.lastName

                // Header information

                $('#employee-name').text(fullName);
               

                // Contact information
                $('#phone-text').text(data.mobile);
                $('#email-text').text(data.email);
                $('#address-text').text(data.birthPlace);


                populatePersonalDetails(data)


                // Profile picture if available
                if (data.employeeImageFileName) {

                    const profileImageUrl = `https://localhost:7171/uploads/employee/photo/${data.employeeImageFileName}`;
                   

                    // Use jQuery to set the image `src` attribute
                    $('#ProfileImages').attr('src', profileImageUrl);


                   // $('#offcanvasRightVEI .rounded-circle').attr('src', `https://localhost:7171/uploads/employee/photo/${data.profilePicture}`);
                }

                // Personal information table
               

                // Re-initialize Feather icons if needed
                if (typeof feather !== 'undefined') {
                    feather.replace();
                }
            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                $('#offcanvasRightVEI').find('.card-body').html('<div class="alert alert-danger">Error loading employee data</div>');
            }
        });



        // Make AJAX call to get employee details
        $.ajax({
            url: '/employeeSetup/official/getEmployeeDetails',
            type: 'GET',
            data: { employeeCode: employeeCode },
            beforeSend: function () {
                // Optional: Show loading indicator
                // $('#offcanvasRightVEI').find('.card-body').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
            },
            success: function (data) {
                // Populate the offcanvas with employee data
               //  console.log('525635', data);
               
               

                clearOfficialDetails();

                if (data) {
                    populateOfficialDetails(data);
                    $('#employee-designation').text('(' + data.designationName + ')');
                }
                else {

                    $('#employee-designation').text('N/A');

                    var $targetElement = $('#offiWarning');

                    $targetElement.empty();

                    // Check if the element exists and is empty
                    if ($targetElement.length && $targetElement.html().trim() === '') {
                        // Append "No Data Available" message
                        $targetElement.append('<div style="color: red; font-weight: bold;">No Data Available</div>');
                    }
                }
               
            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                $('#offcanvasRightVEI').find('.card-body').html('<div class="alert alert-danger">Error loading employee data</div>');
            }
        });


        // Make AJAX call to get employee details
        $.ajax({
            url: '/employeeSetup/additional/getEmployeeDetails',
            type: 'GET',
            data: { employeeCode: employeeCode },
            beforeSend: function () {
                // Optional: Show loading indicator
                // $('#offcanvasRightVEI').find('.card-body').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
            },
            success: function (data) {
                // Populate the offcanvas with employee data
                // console.log('525635', data);

                clearAdditionalEmployeeInfo();

                if (data) {
                    populateAdditionalEmployeeInfo(data);

                }
                else {
                    var $targetElement = $('#addiWarning');

                    $targetElement.empty();

                    // Check if the element exists and is empty
                    if ($targetElement.length && $targetElement.html().trim() === '') {
                        // Append "No Data Available" message
                        $targetElement.append('<div style="color: red; font-weight: bold;">No Data Available</div>');
                    }
                }


            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                $('#offcanvasRightVEI').find('.card-body').html('<div class="alert alert-danger">Error loading employee data</div>');
            }
        });


        // Make AJAX call to get employee details
        $.ajax({
            url: '/employeeSetup/education/getEmployeeDetails',
            type: 'GET',
            data: { employeeCode: employeeCode },
            beforeSend: function () {
                // Optional: Show loading indicator
                // $('#offcanvasRightVEI').find('.card-body').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
            },
            success: function (data) {
                // Populate the offcanvas with employee data
               // console.log('525635', data);

              populateEmployeeEducationInfo(data);


            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                $('#offcanvasRightVEI').find('.card-body').html('<div class="alert alert-danger">Error loading employee data</div>');
            }
        });


        // Make AJAX call to get employee details
        $.ajax({
            url: '/employeeSetup/training/getEmployeeDetails',
            type: 'GET',
            data: { employeeCode: employeeCode },
            beforeSend: function () {
                // Optional: Show loading indicator
                // $('#offcanvasRightVEI').find('.card-body').html('<div class="text-center"><div class="spinner-border" role="status"></div></div>');
            },
            success: function (data) {
                // Populate the offcanvas with employee data
               // console.log('5256351', data);

               populateEmployeeTrainingInfo(data);


            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                $('#offcanvasRightVEI').find('.card-body').html('<div class="alert alert-danger">Error loading employee data</div>');
            }
        });




    });


    function populatePersonalDetails(employee) {
        $("#proTblEmployeeID").text(employee.employeeID || "");
        $("#proTblEmployeeCode").text(employee.employeeCode || "");
        $("#proTblFirstName").text(employee.firstName || "");
        $("#proTblLastName").text(employee.lastName || "");
        $("#proTblFatherName").text(employee.fatherName || "");
        $("#proTblMotherName").text(employee.motherName || "");
        $("#proTblDOB").text(employee.dob || "");
        $("#proTblBirthCertificateNo").text(employee.birthCertificateNo || "");
        $("#proTblBirthPlace").text(employee.birthPlace || "");
        $("#proTblGenderID").text(employee.genderID === 2 ? "Male" : "Female");
        $("#proTblBloodGroupID").text(employee.bloodGroupName || "");
        $("#proTblNationalityID").text(employee.nationalityName || "");
        $("#proTblNID").text(employee.nid || "");
        $("#proTblReligionID").text(employee.religionName || "");
        $("#proTblMaritalStatusID").text(employee.maritalStatusName || "");
        $("#proTblCardNo").text(employee.cardNo || "");
        $("#proTblMobile").text(employee.mobile || "");
        $("#proTblEmail").text(employee.email || "");
        $("#proTblTIN").text(employee.tin || "");
    }

    function populateOfficialDetails(employee) {
        // Populate office and employment details
        $("#proTblOfficeID").text(employee.officeName || "");
        $("#proTblBranchID").text(employee.branchName || "");
        $("#proTblEmployeeOfficeId").text(employee.employeeOfficeId || "");
        $("#proTblDepartmentID").text(employee.departmentName || "");
        $("#proTblDesignationID").text(employee.designationName || "");
        $("#proTblEmployeeTypeID").text(employee.employeeTypeName || "");
        $("#proTblEmploymentNatureID").text(employee.employmentNatureName || "");
        $("#proTblGradeID").text(employee.gradeName || "");
        $("#proTblPaymentPeriodID").text(employee.paymentPeriodName || "");
        $("#proTblPaymentModeID").text(employee.paymentModeName || "");
        $("#proTblModeOfPayInBank").text(employee.modeOfPayInBank || "");
        $("#proTblImmediateSupervisorId").text(employee.immediateSupervisorId || "");
        $("#proTblEmploymentStatusId").text(employee.employmentStatusName || "");
        $("#proTblAppointmentLetterNo").text(employee.appointmentLetterNo || "");
        $("#proTblAppointmentLetterDate").text(employee.appointmentLetterDate || "");
        $("#proTblJoiningDate").text(employee.joiningDate || "");
        $("#proTblProvisionPeriodDays").text(employee.provisionPeriodDays || "");
        $("#proTblConfirmationDate").text(employee.confirmationDate || "");
        $("#proTblConfirmationLetterNo").text(employee.confirmationLetterNo || "");
        $("#proTblContractEndDate").text(employee.contractEndDate || "");
    }


    function populateAdditionalEmployeeInfo(info) {
        $("#proTblPasportName").text(info.pasportName || "");
        $("#proTblEnterPasportNo").text(info.enterPasportNo || "");
        $("#proTblPasportPlaceOfIssue").text(info.pasportPlaceOfIssue || "");
        $("#proTblPasportDateIssued").text(info.pasportDateIssued || "");
        $("#proTblPassportExpiryDate").text(info.passportExpiryDate || "");
        $("#proTblSalaryBankID").text(info.salaryBankID || "");
        $("#proTblSalaryBankBranchID").text(info.salaryBankBranchID || "");
        $("#proTblBankAccountName").text(info.bankAccountName || "");
        $("#proTblBankAccountNumber").text(info.bankAccountNumber || "");
    }


    function clearPersonalDetails() {
        $("#proTblEmployeeID, #proTblEmployeeCode, #proTblFirstName, #proTblLastName, #proTblFatherName, #proTblMotherName, #proTblDOB, #proTblBirthCertificateNo, #proTblBirthPlace, #proTblGenderID, #proTblBloodGroupID, #proTblNationalityID, #proTblNID, #proTblReligionID, #proTblMaritalStatusID, #proTblCardNo, #proTblMobile, #proTblEmail, #proTblTIN").text("");
    }

    function clearOfficialDetails() {
        $("#proTblOfficeID, #proTblBranchID, #proTblEmployeeOfficeId, #proTblDepartmentID, #proTblDesignationID, #proTblEmployeeTypeID, #proTblEmploymentNatureID, #proTblGradeID, #proTblPaymentPeriodID, #proTblPaymentModeID, #proTblModeOfPayInBank, #proTblImmediateSupervisorId, #proTblEmploymentStatusId, #proTblAppointmentLetterNo, #proTblAppointmentLetterDate, #proTblJoiningDate, #proTblProvisionPeriodDays, #proTblConfirmationDate, #proTblConfirmationLetterNo, #proTblContractEndDate").text("");
    }

    function clearAdditionalEmployeeInfo() {
        $("#proTblPasportName, #proTblEnterPasportNo, #proTblPasportPlaceOfIssue, #proTblPasportDateIssued, #proTblPassportExpiryDate, #proTblSalaryBankID, #proTblSalaryBankBranchID, #proTblBankAccountName, #proTblBankAccountNumber").text("");
    }




    //populateEducationEmployeeInfo(data){

    //}


    function populateEmployeeEducationInfo(educationList) {
        let tbody = $("#proTbleducationTable tbody");
        tbody.empty();

        if (educationList.length === 0) {
            tbody.append("<tr><td colspan='8'>No educational information available</td></tr>");
        } else {
            educationList.forEach(edu => {
                let row = `<tr>
                <td>${edu.instituteName || 'NA'}</td>
                <td>${edu.degreeName || 'NA'}</td>
                <td>${edu.yearOfPassing || 'NA'}</td>
                <td>${edu.result || 'NA'}</td>
                <td>${edu.majorGroup || 'NA'}</td>
                <td>${edu.educationBoardName || 'NA'}</td>
                <td>${edu.durationYears || 'NA'}</td>
                <td>${edu.achievement || 'NA'}</td>
            </tr>`;
                tbody.append(row);
            });
        }
    }




    //function populateTrainingEmployeeInfo(data) {

    //}


    function populateEmployeeTrainingInfo(trainingList) {
        let tbody = $("#proTbltrainingTable tbody");
        tbody.empty();

        if (trainingList.length === 0) {
            tbody.append("<tr><td colspan='6'>No training information available</td></tr>");
        } else {
            trainingList.forEach(training => {
                let row = `<tr>
                <td>${training.instituteName || 'NA'}</td>
                <td>${training.durationYears || 'NA'}</td>
                <td>${training.topicCovered || 'NA'}</td>
                <td>${training.countryName || 'NA'}</td>
                <td>${training.tranningYear || 'NA'}</td>
                <td>${training.tranningLocation || 'NA'}</td>
            </tr>`;
                tbody.append(row);
            });
        }
    }



    //#endregion


});
