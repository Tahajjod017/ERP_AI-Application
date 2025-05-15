$(document).ready(function () {

  


    let choicesInstance; // Store Choices.js instance

    // Ensure the dropdown is visible
    $("#empCodePass1").show();

    // Remove duplicate hidden input ID to avoid conflicts
    $('input[type="hidden"][id="empCodePass1"]').attr('id', 'empCodePassHidden');

    // Function to fetch and populate the employee list
    function getAllemp() {
        $.ajax({
            url: '/EmployeePersonal/GetEmployeeList',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                //console.log('Data received:', data);
                const $dropdown = $("#empCodePass1");

                // Destroy previous Choices instance if it exists
                if (choicesInstance) {
                    choicesInstance.destroy();
                }

                // Clear and populate dropdown
                $dropdown.empty().append(`<option value="" selected>Select Employee</option>`);

                if (Array.isArray(data) && data.length > 0) {
                    data.forEach(employee => {
                        $dropdown.append(`
                        <option value="${employee.employeeID}">
                            ${employee.fullName} (${employee.employeeCode})
                        </option>
                    `);
                    });

                    //console.log('Dropdown populated with ' + data.length + ' employees');
                } else {
                    //console.log('No employees found or invalid data format:', data);
                }

                // Initialize Choices.js
                choicesInstance = new Choices("#empCodePass1", {
                    searchEnabled: true,
                    itemSelectText: 'Select',
                    removeItemButton: true,
                    allowHTML: false
                });

                // Check session storage AFTER initializing Choices.js
                var paramValue = sessionStorage.getItem("tabParameter");
                if (paramValue) {
                    applySessionData();
                }
            },
            error: function (error) {
                console.error("Error fetching employee list:", error);
            }
        });
    }




    
    

    // Function to handle session data
    function applySessionData() {
   
        var paramValue = sessionStorage.getItem("tabParameter");
        //console.log("Received Parameter on page load:", paramValue);

        if (paramValue) {
            //console.log('Making AJAX request for empCode:', paramValue);
            $.ajax({
                url: '/EmployeePersonal/getEmpInfo',
                type: 'GET',
                dataType: 'json',
                data: { empCode: paramValue },
                success: function (response) {
                    //console.log('Received Employee Data:', response);

                    if (response) {
                        $("#NidPass1").val(response.nid || '');
                        $("#BirthCertiPass1").val(response.birthCertificateNo || '');
                        $("#empIdMain1").val(response.employeeCode);

                        // Set dropdown value using Choices.js API
                        if (response.employeeID && choicesInstance) {
                            choicesInstance.setChoiceByValue(response.employeeID.toString());

                            // Trigger change event to load additional data
                            $("#empCodePass1").val(response.employeeID).trigger('change');
                        }
                        sessionStorage.removeItem("tabParameter");
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                }
            });
        } else {
            //console.log('No empCode found in sessionStorage111');
            $("#NidPass1, #BirthCertiPass1, #empCodePass1, #empIdMain1").val('');
        }
    }







    // Dropdown change event handler
    $("#empCodePass1").on('change', function () {
        var selectedEmpID = $(this).val();
        if (selectedEmpID && selectedEmpID !== 'Select Employee') {
            $.ajax({
                url: '/EmployeePersonal/getEmpInfoById',
                type: 'GET',
                dataType: 'json',
                data: { empId: selectedEmpID },
                success: function (response) {
                    ////console.log('Selected Employee Data:', response);
                    $("#NidPass1").val(response.nid || '');
                    $("#BirthCertiPass1").val(response.birthCertificateNo || '');
                    $("#empIdMain1").val(response.employeeCode);
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching employee details:', error);
                }
            });
        } else {
            $("#NidPass1, #BirthCertiPass1").val('');
        }
    });

    // Load employee data on page load
    getAllemp();


    //function getAllemp() {
    //    $.ajax({
    //        url: '/EmployeePersonal/GetEmployeeList',
    //        type: 'GET',
    //        dataType: 'json',
    //        success: function (data) {
    //            ////console.log('Data received:', data);
    //            const $dropdown = $("#empCodePass1");
    //            $dropdown.empty();
    //            $dropdown.append(`<option value="">Select Employee</option>`);

    //            if (Array.isArray(data) && data.length > 0) {
    //                data.forEach(employee => {
    //                    $dropdown.append(`
    //                        <option value="${employee.employeeID}">
    //                            ${employee.fullName} (${employee.employeeCode})
    //                        </option>
    //                    `);

                       
    //                });
                    

    //                ////console.log('Dropdown populated with ' + data.length + ' employees');
    //            } else {
    //                ////console.log('No employees found or data is not in expected format:', data);
    //            }
    //        },
    //        error: function (error) {
    //            console.error("Error fetching employee list:", error);
    //        }
    //    });
    //}



    



    function initializeSelectPicker(elementId) {
        var $element = $('#' + elementId);

        // Destroy if exists
        if ($element.data('selectpicker')) {
            $element.selectpicker('destroy');
        }

        // Initialize with options
        $element.selectpicker({
            liveSearch: true,
            enableSelectedText: true,
            liveSearchPlaceholder: 'Search...',
            size: 10,
            selectedTextFormat: 'count',
            actionsBox: true,
            iconBase: 'fa',
            showTick: true,
            tickIcon: 'fa-check',
            container: 'body'
        });

        // Refresh to ensure proper rendering
        $element.selectpicker('refresh');
    }




    $("#empCodePass1").on('change', function () {
        var empId = $(this).val();
        getOffficalFormData(empId);
    })

    $("#office").on('change', function () {
        var officeId = $(this).val();
        populateBranch(officeId);
    })

    $("#department").on('change', function () {
        var deptId = $(this).val();
        populateDesignation(deptId);
    })




    function populateDesignation(deptId) {
        $.ajax({
            url: '/designation/getbydept',
            type: 'GET',
            dataType: 'json',
            data: { deptId: deptId },
            success: function (response) {
                ////console.log('Received designa Data:', response);
                // Add code to populate the fields with the response data
                if (response.success) {

                    var dropdown = $('#designation');
                    // Clear existing options except the first one (Select...)
                    dropdown.find('option:not(:first)').remove();

                    // Add new options from the data
                    $.each(response.data, function (index, item) {
                        dropdown.append($('<option></option>')
                            .attr('value', item.id)
                            .text(item.name));
                    });

                } else {

                }

              //  sessionStorage.removeItem("tabParameter");
            },
            error: function (xhr, status, error) {
                console.error('Error fetching data:', error);
                ////console.log('XHR Response Text:', xhr.responseText);
            }
        });
    }



    function populateBranch(officeId) {
        $.ajax({
            url: '/branch/getbyofc',
            type: 'GET',
            dataType: 'json',
            data: { officeId: officeId },
            success: function (response) {
                ////console.log('Received Branch Data:', response);
                // Add code to populate the fields with the response data
                if (response.success) {

                    var dropdown = $('#branch');
                    // Clear existing options except the first one (Select...)
                    dropdown.find('option:not(:first)').remove();

                    // Add new options from the data
                    $.each(response.data, function (index, item) {
                        dropdown.append($('<option></option>')
                            .attr('value', item.id)
                            .text(item.name));
                    });

                } else {

                }

              //  sessionStorage.removeItem("tabParameter");
            },
            error: function (xhr, status, error) {
                console.error('Error fetching data:', error);
                ////console.log('XHR Response Text:', xhr.responseText);
            }
        });
    }

    function getOffficalFormData(empId) {
        if (empId) {
            ////console.log('Making AJAX request for empCode:', empId);

            

            $.ajax({
                url: '/Employeeofficial/getEmpOfficialInfo',
                type: 'GET',
                dataType: 'json',
                data: { empId: empId },
                success: function (response) {
                    
                    ////console.log('Received Employee official Data:', response);
                    // Add code to populate the fields with the response data
                    if (response.success) {
                        populateOfficialForm(response.data)
                    } else {

                    }

                  //  sessionStorage.removeItem("tabParameter");
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                    ////console.log('XHR Response Text:', xhr.responseText);
                }
            });
        } else {
            ////console.log('No empCode found in sessionStorage222');


        }
    }




    function populateOfficialForm(employeeData) {
        console.log('777', employeeData)

        $("#empOfficialInfoId").val(employeeData.employeeOfficeInfoID);
        $("#employeeId").val(employeeData.employeeID);

        // Set dropdown values
        $("#office").val(employeeData.officeID);
        $("#branch").val(employeeData.branchID);
        $("#department").val(employeeData.departmentID);
        $("#designation").val(employeeData.designationID);
        $("#empType").val(employeeData.employeeTypeID);
        $("#empNature").val(employeeData.employmentNatureID);
        $("#gradeNo").val(employeeData.gradeID);
        $("#curency").val(employeeData.currencyID);
        $("#paymentPeriod").val(employeeData.paymentPeriodID);
        $("#modeOfPayment").val(employeeData.paymentModeID);
        $("#employeeStatus").val(employeeData.employmentStatusId);
        $("#salaryTypeoffi").val(employeeData.salaryTypeID);
        $("#time").val("Days"); // Assuming "Month" is the default for time

        // Set text input values
        $("#grossSalary").val(employeeData.salary);
        $("#bankPercent").val(employeeData.modeOfPayInBank);
        $("#appointmentLetterNo1").val(employeeData.appointmentLetterNo);
        $("#probationPeriod").val(employeeData.provisionPeriodDays);
        $("#confirmationLetterNo").val(employeeData.confirmationLetterNo);

        // Set date inputs
        // Note: If using flatpickr, you might need to use flatpickr API instead
        $("#appointmentLetterDate").val(employeeData.appointmentLetterDate);
        $("#joiningDate").val(employeeData.joiningDate);
        $("#confirmationDate").val(employeeData.confirmationDate);
        $("#contractEndDate").val(employeeData.contractEndDate);
        $("#immediateSupervisor").val(employeeData.immediateSupervisorId).trigger('change');
        $("#time").val("Days");



        // If using flatpickr for dates, uncomment this section and modify as needed:
        /*
        if (typeof flatpickr !== 'undefined') {
            flatpickr("#appointmentLetterDate").setDate(employeeData.appointmentLetterDate);
            flatpickr("#joiningDate").setDate(employeeData.joiningDate);
            flatpickr("#confirmationDate").setDate(employeeData.confirmationDate);
            flatpickr("#contractEndDate").setDate(employeeData.contractEndDate);
        }
        */

        // Trigger change events to update any dependent fields
       // $("select").trigger("change");
    }




    $("#btnSaveOfficial").click(function () {
        saveEmployeeOfficialInfo();
    });


   
    function saveEmployeeOfficialInfo() {
        // Create view model from form data
        const employeeOfficialViewModel = {
            // Make sure property names match EXACTLY with C# model (case-sensitive)
            EmpOfficialInfoId: $("#empOfficialInfoId").val() ? parseInt($("#empOfficialInfoId").val()) : 0,
            EmployeeId: $("#empCodePass1").val() ? parseInt($("#empCodePass1").val()) : 0,
            OfficeId: $("#office").val() ? parseInt($("#office").val()) : 0,
            BranchId: $("#branch").val() ? parseInt($("#branch").val()) : 0,
            DepartmentId: $("#department").val() ? parseInt($("#department").val()) : 0,
            DesignationId: $("#designation").val() ? parseInt($("#designation").val()) : 0,
            EmployeeTypeId: $("#empType").val() ? parseInt($("#empType").val()) : 0,
            EmploymentNatureId: $("#empNature").val() ? parseInt($("#empNature").val()) : 0,
            GradeId: $("#gradeNo").val() ? parseInt($("#gradeNo").val()) : 0,
            GrossSalary: $("#grossSalary").val() ? parseFloat($("#grossSalary").val()) : 0,
            CurrencyId: $("#currency").val() ? parseInt($("#currency").val()) : 0,
            PaymentPeriodId: $("#paymentPeriod").val() ? parseInt($("#paymentPeriod").val()) : 0,
            ModeOfPaymentId: $("#modeOfPayment").val() ? parseInt($("#modeOfPayment").val()) : 0,
            BankPaymentPercentage: $("#bankPercent").val() ? parseFloat($("#bankPercent").val()) : null,
            EmployeeStatusId: $("#employeeStatus").val() ? parseInt($("#employeeStatus").val()) : 0,

            // Joining information
            AppointmentLetterNo: $("#appointmentLetterNo1").val(),
            AppointmentLetterDate: $("#appointmentLetterDate").val(),
            JoiningDate: $("#joiningDate").val(),
            ProbationPeriod: $("#probationPeriod").val() ? parseInt($("#probationPeriod").val()) : null,
            ProbationTimeUnit: $("#time").val(),
            ConfirmationDate: $("#confirmationDate").val(),
            ConfirmationLetterNo: $("#confirmationLetterNo").val(),
            ContractEndDate: $("#contractEndDate").val(),
            empCode: $("#empIdMain1").val(),
            SalaryType: $("#salaryTypeoffi").val() ? parseInt($("#salaryTypeoffi").val()) : 0,
            ImmediateSupervisor: $("#immediateSupervisor").val() ? parseInt($("#immediateSupervisor").val()) : 0

        };

        var is = $("#immediateSupervisor").val();

        alert(is);

        ////console.log("Submitting data:", employeeOfficialViewModel); // Debug log
        // Reset previous validation error messages and styles
        $(".invalid-feedback").remove();
        $(".form-control").removeClass("is-invalid");

        var isValid = true;

        // Check if any required fields are missing or invalid
        if (!employeeOfficialViewModel.EmployeeId) {
            isValid = false;
            $("#empCodePass1").addClass("is-invalid");
            $("#empCodePass1").after('<div class="invalid-feedback">Employee ID is required.</div>');
        }

        if (!employeeOfficialViewModel.OfficeId) {
            isValid = false;
            $("#office").addClass("is-invalid");
            $("#office").after('<div class="invalid-feedback">Office is required.</div>');
        }

        if (!employeeOfficialViewModel.GrossSalary) {
            isValid = false;
            $("#grossSalary").addClass("is-invalid");
            $("#grossSalary").after('<div class="invalid-feedback">Gross Salary is required.</div>');
        }

        if (!employeeOfficialViewModel.JoiningDate) {
            isValid = false;
            $("#joiningDate").addClass("is-invalid");
            $("#joiningDate").after('<div class="invalid-feedback">Joining Date is required.</div>');
        }

        if (!employeeOfficialViewModel.ProbationPeriod && employeeOfficialViewModel.ProbationTimeUnit) {
            isValid = false;
            $("#probationPeriod").addClass("is-invalid");
            $("#probationPeriod").after('<div class="invalid-feedback">Please enter a valid probation period.</div>');
        }

        if (!isValid) {
            return; // Exit if validation fails
        }
        

        // Send AJAX request to save data
        $.ajax({
            url: "/employee/official/save",
            type: "POST",
            data: JSON.stringify(employeeOfficialViewModel),
            contentType: "application/json",
            dataType: "json",
            success: function (response) {
                ////console.log("Response:", response); // Debug log
                if (response.success) {


                    var paramValue = response.data.data;
                    //debugger

                   // sessionStorage.removeItem("tabParameter");
                    sessionStorage.setItem("tabParameter", paramValue);
                    sessionStorage.setItem("tabParameter1", paramValue);



                    // Get current tab dynamically inside the event
                    var currentTab = $("#btnSaveOfficial").closest(".tab-pane"); // Get current tab pane
                    var currentTabId = currentTab.attr("id"); // Get current tab ID

                    // Move to the next tab
                    var $currentTabLink = $('a[href="#' + currentTabId + '"]'); // Get current tab link
                    var $nextTabLink = $currentTabLink.parent().next().find(".nav-link"); // Get next tab link

                    if ($nextTabLink.length) {
                        $nextTabLink.tab("show"); // Show next tab
                    } else {
                        window.location.href = "/employeeadditional/index";
                    }


                    // Update hidden field with the returned ID if necessary
                    if (response.data && response.data.empOfficialInfoId) {
                        $("#empOfficialInfoId").val(response.data.empOfficialInfoId);
                    }
                } else {
                   toastr.warning(response.message)
                }
            },
            error: function (xhr, status, error) {
                ////console.log("Error details:", xhr.responseText); // Debug log
                
            }
        });
    }



    function isValidEmail(email) {
        const emailRegex = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        return emailRegex.test(email);
    }

    function showNotification(type, message) {
        // You can implement this based on your notification system
        // Example using Bootstrap toast or alert
        const alertHtml = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;

        // Add the alert before the form
        $("#myTabContent").before(alertHtml);

        // Auto-dismiss after 5 seconds
        setTimeout(function () {
            $(".alert").alert('close');
        }, 5000);
    }

    // Additional event handlers for "Add new" dropdowns
    setupAddNewForms();

    function setupAddNewForms() {
        // Department form
        $("#deptSave").click(function () {
            const deptId = $("#deptId").val();
            const deptName = $("#deptName").val();

            if (deptId && deptName) {
                saveDepartment(deptId, deptName);
            } else {
                alert("Please enter both Department ID and Name");
            }
        });

        $("#deptCancel").click(function () {
            clearAndCloseDropdown("deptForm");
        });

        // Designation form
        $("#desigSave").click(function () {
            const desigId = $("#desigId").val();
            const desigName = $("#desigName").val();

            if (desigId && desigName) {
                saveDesignation(desigId, desigName);
            } else {
                alert("Please enter both Designation ID and Name");
            }
        });

        $("#desigCancel").click(function () {
            clearAndCloseDropdown("desigForm");
        });

        // Employee Type form
        $("#empTypeSave").click(function () {
            const empTypeId = $("#empTypeId").val();
            const empTypeName = $("#empTypeName").val();

            if (empTypeId && empTypeName) {
                saveEmployeeType(empTypeId, empTypeName);
            } else {
                alert("Please enter both Employee Type ID and Name");
            }
        });

        $("#empTypeCancel").click(function () {
            clearAndCloseDropdown("empTypeForm");
        });

        // Grade form
        $("#gradeSave").click(function () {
            const gradeId = $("#gradeId").val();
            const gradeName = $("#gradeName").val();

            if (gradeId && gradeName) {
                saveGrade(gradeId, gradeName);
            } else {
                alert("Please enter both Grade ID and Name");
            }
        });

        $("#gradeCancel").click(function () {
            clearAndCloseDropdown("gradeForm");
        });
    }

    function clearAndCloseDropdown(formSelector) {
        $(`form[action="${formSelector}"] input`).val("");
        $(".dropdown-menu").removeClass("show");
    }

    // Functions to save dropdown items
    function saveDepartment(id, name) {
        $.ajax({
            url: "/department/save", // Replace with your API endpoint
            type: "POST",
            data: { id: id, name: name },
            success: function (response) {
                ////console.log(response)
                // Add new option to department dropdown
                $("#department").append(new Option(name, id));
                // Select the newly added option
                $("#department").val(id);
                // Close dropdown
                clearAndCloseDropdown("deptForm");
            },
            error: function (xhr, status, error) {
                alert("Error saving department: " + error);
            }
        });
    }

    function saveDesignation(id, name) {
        $.ajax({
            url: "/designation/save", // Replace with your API endpoint
            type: "POST",
            data: { id: id, name: name },
            success: function (response) {
                ////console.log(response)
                $("#designation").append(new Option(name, id));
                $("#designation").val(id);
                clearAndCloseDropdown("desigForm");
            },
            error: function (xhr, status, error) {
                alert("Error saving designation: " + error);
            }
        });
    }

    function saveEmployeeType(id, name) {
        $.ajax({
            url: "/employeeType/save", // Replace with your API endpoint
            type: "POST",
            data: { id: id, name: name },
            success: function (response) {
                ////console.log(response)

                $("#empType").append(new Option(name, id));
                $("#empType").val(id);
                clearAndCloseDropdown("empTypeForm");
            },
            error: function (xhr, status, error) {
                alert("Error saving employee type: " + error);
            }
        });
    }

    function saveGrade(id, name) {
        $.ajax({
            url: "/grade/save", // Replace with your API endpoint
            type: "POST",
            data: { id: id, name: name },
            success: function (response) {
                ////console.log(response)

                $("#gradeNo").append(new Option(name, id));
                $("#gradeNo").val(id);
                clearAndCloseDropdown("gradeForm");
            },
            error: function (xhr, status, error) {
                alert("Error saving grade: " + error);
            }
        });
    }

  





    function populateDropdown(url, dropdownId, valueField, textField) {
        $.ajax({
            url: url,
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                ////console.log('1',dropdownId,data)
                var dropdown = $('#' + dropdownId);
                // Clear existing options except the first one (Select...)
                dropdown.find('option:not(:first)').remove();

                // Add new options from the data
                $.each(data.data, function (index, item) {
                    dropdown.append($('<option></option>')
                        .attr('value', item[valueField])
                        .text(item[textField]));
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading ' + dropdownId + ': ' + error);
            }
        });
    }

    populateDropdown('/designation/get', 'designation', 'id', 'name');

    populateDropdown('/department/get', 'department', 'id', 'name');

    populateDropdown('/employeeType/get', 'empType', 'id', 'name');

    populateDropdown('/grade/get', 'gradeNo', 'id', 'name');

    populateDropdown('/office/get', 'office', 'id', 'name');

    populateDropdown('/branch/get', 'branch', 'id', 'name');

    populateDropdown('/empNature/get', 'empNature', 'id', 'name');

    populateDropdown('/curency/get', 'currency', 'id', 'name');

    populateDropdown('/paymentPeriod/get', 'paymentPeriod', 'id', 'name');

    populateDropdown('/paymentMode/get', 'modeOfPayment', 'id', 'name');

    populateDropdown('/empStatus/get', 'employeeStatus', 'id', 'name');

    populateDropdown('/salaryTypeOffi/get', 'salaryTypeoffi', 'id', 'name');
    


    

    
  
    loadTimeOptions();



    function loadTimeOptions() {
        $.ajax({
            url: '/common/timeOptions',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                //populateDropdown('#time', data, 'Select Time Unit');
                //// Set Month as default selected
                //$("#time option").each(function () {
                //    if ($(this).text() === "Month") {
                //        $(this).prop("selected", true);
                //    }
                //});
                

                ////console.log('1yyyyyy',  data)
                var dropdown = $('#time');
                ////console.log('1yyyyyy', dropdown)
                // Clear existing options except the first one (Select...)
                dropdown.find('option:not(:first)').remove();

                // Add new options from the data
                $.each(data, function (index, item) {
                    dropdown.append($('<option></option>')
                        .attr('value', item.id)
                        .text(item.name));
                });



            },
            error: function (error) {
                console.error('Error loading time options:', error);
                // Fallback to some default data for testing
                const fallbackData = [
                    { id: "Days", name: "Days" },
                    { id: "Month", name: "Month", isSelected: "selected" },
                    { id: "Year", name: "Year" }
                ];
                populateDropdown('#time', fallbackData, 'Select Time Unit');
                // Set Month as default selected
                $("#time option").each(function () {
                    if ($(this).text() === "Month") {
                        $(this).prop("selected", true);
                    }
                });
            }
        });
    }





});

//#region Back up


//var empId = $("#empIdMain1").val();



//sessionStorage.setItem("tabParameter", 'EMP001');

//var paramValue = sessionStorage.getItem("tabParameter");
//////console.log("Received Parameter on page load:", paramValue);

//if (paramValue) {
//    ////console.log('Making AJAX request for empCode:', paramValue);  // Debugging

//    $.ajax({
//        url: '/EmployeePersonal/getEmpInfo', // Ensure the URL matches the controller route
//        type: 'GET',
//        dataType: 'json',
//        data: { empCode: paramValue },
//        success: function (response) {
//            ////console.log('Received Employee Data:', response);
//            sessionStorage.removeItem("tabParameter");
//        },
//        error: function (xhr, status, error) {
//            console.error('Error fetching data:', error);
//            ////console.log('XHR Response Text:', xhr.responseText);  // Useful for debugging errors
//        }
//    });

//} else {
//    ////console.log('No empCode found in sessionStorage'); // Debugging
//}

//getAllemp();

//function getAllemp() {
//    $.ajax({
//        url: '/EmployeePersonal/GetEmployeeList', // Replace with your endpoint URL
//        type: 'GET',
//        dataType: 'json', // Expect JSON response
//        success: function (data) {
//            var $dropdown = $("#empCodePass1"); // Target the select element
//            $dropdown.empty(); // Clear existing options

//            // Add a default "Select Employee" option
//            $dropdown.append('<option selected="selected">Select Employee</option>');

//            // Loop through the data and add options
//            $.each(data, function (key, employee) {
//                $dropdown.append(
//                    $('<option></option>')
//                        .attr('value', employee.EmployeeID) // Use EmployeeID as the value
//                        .text(`${employee.FullName} (${employee.EmployeeCode})`) // Display Name (Code)
//                );


//            });
//        },
//        error: function (error) {
//            console.error("Error fetching employee list:", error);
//        }
//    });
//}






//////$('a[data-bs-toggle="tab"]').on("shown.bs.tab", function (e) {
//////    var paramValue = sessionStorage.getItem("tabParameter");
//////    ////console.log("Received Parameter:", paramValue);

//////    if (paramValue) {
//////        ////console.log('Making AJAX request for empCode:', paramValue);  // Debugging

//////        $.ajax({
//////            url: 'EmployeePersonal/getEmpInfo', // Ensure the URL matches the controller route
//////            type: 'GET',
//////            dataType: 'json',
//////            data: { empCode: paramValue },
//////            success: function (response) {
//////                ////console.log('Received Employee Data:', response);
//////            },
//////            error: function (xhr, status, error) {
//////                console.error('Error fetching data:', error);
//////                ////console.log('XHR Response Text:', xhr.responseText);  // Useful for debugging errors
//////            }
//////        });

//////    } else {
//////        ////console.log('No empCode found in sessionStorage'); // Debugging
//////    }
//////});




// Save button click event





//function saveEmployeeData() {
//    // Validate required fields
//    //if (!validateForm()) {
//    //    return false;
//    //}

//    // Collect form data

//    var data = {
//        empOfficialInfoId: $("#empOfficialInfoId").val(),
//        officeId: $("#office").val(),
//        branchId: $("#branch").val(),
//        DepartmentId: $("#department").val(),
//        DesignationId: $("#designation").val(),
//        EmployeeTypeId: $("#empType").val(),
//        EmploymentNatureId: $("#empNature").val(),
//        GradeNoId: $("#gradeNo").val(),
//        GrossSalary: $("#grossSalary").val(),
//        CurrencyId: $("#curency").val(),
//        PaymentPeriodId: $("#paymentPeriod").val(),
//        ModeOfPaymentId: $("#modeOfPayment").val(),
//        BankPercent: $("#bankPercent").val(),
//        EmployeeStatusId: $("#employeeStatus").val(),
//        AppointmentLetterNo: $("#appointmentLetterNo1").val(),
//        AppointmentLetterDate: $("#appointmentLetterDate").val(),
//        JoiningDate: $("#joiningDate").val(),
//        ProbationPeriod: $("#probationPeriod").val(),
//        TimeId: $("#time").val(),
//        ConfirmationDate: $("#confirmationDate").val(),
//        ConfirmationLetterNo: $("#confirmationLetterNo").val(),
//        ContractEndDate: $("#contractEndDate").val(),
//        employeeId: $("#empCodePass1").val(),
//    };


//    const employeeData = {
//        // Official Information
//        empOfficialInfoId: $("#empOfficialInfoId").val(),
//        employeeId: $("#empCodePass1").val(),
//        office: $("#office").val(),
//        branch: $("#branch").val(),
//        department: $("#department").val(),
//        designation: $("#designation").val(),
//        employeeType: $("#empType").val(),
//        employmentNature: $("#empNature").val(),
//        gradeNo: $("#gradeNo").val(),
//        grossSalary: $("#grossSalary").val(),
//        currency: $("#curency").val(),
//        paymentPeriod: $("#paymentPeriod").val(),
//        modeOfPayment: $("#modeOfPayment").val(),
//        bankPaymentPercent: $("#bankPercent").val(),
//        //immediateSupervisor: $("#immSupervisor").val(),
//        //headOfDepartment: $("#hod").val(),
//        //officialPhone: $("#officialPhone").val(),
//        //officialEmail: $("#officialEmail").val(),
//        //attendanceId: $("#attendenceId").val(),
//        employeeStatus: $("#employeeStatus").val(),

//        // Joining Information
//        appointmentLetterNo1: $("#appointmentLetterNo1").val(),
//        appointmentLetterDate: $("#appointmentLetterDate").val(),
//        joiningDate: $("#joiningDate").val(),
//       // appointmentLetterNo2: $("#appointmentLetterNo2").val(),
//        probationPeriod: $("#probationPeriod").val(),
//        probationTimeUnit: $("#time").val(),
//        confirmationDate: $("#confirmationDate").val(),
//        confirmationLetterNo: $("#confirmationLetterNo").val(),
//        contractEndDate: $("#contractEndDate").val()
//    };


//    ////console.log('1',data)

//    // AJAX call to save data
//    $.ajax({
//        url: "/employee/official/save", // Replace with your API endpoint
//        type: "POST",
//        contentType: "application/json",
//        data: JSON.stringify(employeeData),
//        beforeSend: function () {
//            // Show loading indicator
//            $("#btnSave").prop("disabled", true).html('<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Saving...');
//        },
//        success: function (response) {
//            ////console.log(response)
//            // Handle success
//            showNotification("success", "Employee information saved successfully!");
//            // Optional: Reset form or redirect
//           //  resetForm();
//            // window.location.href = "employeeList.html";




//        },
//        error: function (xhr, status, error) {
//            // Handle error
//            showNotification("danger", "Error saving employee data: " + (xhr.responseJSON?.message || error));
//        },
//        complete: function () {
//            // Re-enable button
//            $("#btnSave").prop("disabled", false).html('Save');
//        }
//    });
//}

//function validateForm() {
//    let isValid = true;

//    // Clear previous validation messages
//    $(".validation-error").remove();

//    // Check required fields (assuming all select elements with required attribute are mandatory)
//    $("select[required], input[required]").each(function () {
//        const $this = $(this);

//        if ($this.val() === "" || $this.val() === "Select Office" ||
//            $this.val() === "Select Branch" || $this.val() === "Select Department" ||
//            $this.val() === "Select Designation" || $this.val() === "Select Employee Type" ||
//            $this.val() === "Select Employment Nature" || $this.val() === "Select Grade No" ||
//            $this.val() === "Select Employee Status") {

//            isValid = false;
//            const fieldName = $this.siblings("label").text().trim();

//            // Add validation message after the parent div
//            $this.closest(".form-floating").after(
//                `<div class="validation-error text-danger mt-1">Please select/enter ${fieldName}</div>`
//            );

//            // Highlight the field
//            $this.addClass("is-invalid");
//        } else {
//            $this.removeClass("is-invalid");
//        }
//    });

//    // Email validation for official email
//    const email = $("#officialEmail").val();
//    if (email && !isValidEmail(email)) {
//        isValid = false;
//        $("#officialEmail").addClass("is-invalid");
//        $("#officialEmail").closest(".form-floating").after(
//            `<div class="validation-error text-danger mt-1">Please enter a valid email address</div>`
//        );
//    }

//    return isValid;
//}




//// Initialize date pickers
//function initializeDatePickers() {
//    $(".datetimepicker").flatpickr({
//        dateFormat: "Y-m-d",
//        disableMobile: true
//    });
//}

//// Load Office dropdown
//function loadOffices() {
//    $.ajax({
//        url: '/common/offices',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#office', data, 'Select Office');
//        },
//        error: function (error) {
//            console.error('Error loading offices:', error);
//            // Fallback to some default data for testing
//            const fallbackData = [
//                { id: "1", name: "NBR" },
//                { id: "2", name: "NBR-BD" }
//            ];
//            populateDropdown('#office', fallbackData, 'Select Office');
//        }
//    });
//}

//// Load Branch dropdown
//function loadBranches() {
//    $.ajax({
//        url: '/common/branches',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#branch', data, 'Select Branch');
//        },
//        error: function (error) {
//            console.error('Error loading branches:', error);
//            // Fallback to some default data for testing
//            const fallbackData = [
//                { id: "1", name: "Main office" },
//                { id: "2", name: "Branch office" }
//            ];
//            populateDropdown('#branch', fallbackData, 'Select Branch');
//        }
//    });
//}

//// Load Department dropdown
//function loadDepartments(officeId, branchId) {
//    // Clear the current dropdown
//    $('#department').empty().append('<option selected disabled>Select Department</option>');

//    if (!officeId || !branchId) return;

//    $.ajax({
//        url: '/common/departments',
//        type: 'GET',
//        data: { officeId: officeId, branchId: branchId },
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#department', data);
//        },
//        error: function (error) {
//            console.error('Error loading departments:', error);
//        }
//    });
//}

//// Load Designation dropdown
//function loadDesignations() {
//    $.ajax({
//        url: '/common/designations',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#designation', data, 'Select Designation');
//        },
//        error: function (error) {
//            console.error('Error loading designations:', error);
//        }
//    });
//}

//// Load Employee Types dropdown
//function loadEmployeeTypes() {
//    $.ajax({
//        url: '/common/employeeTypes',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#empType', data, 'Select Employee Type');
//        },
//        error: function (error) {
//            console.error('Error loading employee types:', error);
//        }
//    });
//}

//// Load Employment Nature dropdown
//function loadEmploymentNatures() {
//    $.ajax({
//        url: '/common/employmentNatures',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#empNature', data, 'Select Employment Nature');
//        },
//        error: function (error) {
//            console.error('Error loading employment natures:', error);
//            // Fallback to some default data for testing
//            const fallbackData = [
//                { id: "1", name: "Full Time" },
//                { id: "2", name: "Part Time" },
//                { id: "3", name: "Contractual" }
//            ];
//            populateDropdown('#empNature', fallbackData, 'Select Employment Nature');
//        }
//    });
//}

//// Load Grade Numbers dropdown
//function loadGradeNumbers() {
//    $.ajax({
//        url: '/common/gradeNumbers',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#gradeNo', data, 'Select Grade No');
//        },
//        error: function (error) {
//            console.error('Error loading grade numbers:', error);
//        }
//    });
//}

//// Load Currencies dropdown
//function loadCurrencies() {
//    $.ajax({
//        url: '/common/currencies',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#curency', data, 'Currency');
//        },
//        error: function (error) {
//            console.error('Error loading currencies:', error);
//            // Fallback to some default data for testing
//            const fallbackData = [
//                { id: "1", name: "BDT" },
//                { id: "2", name: "USD" }
//            ];
//            populateDropdown('#curency', fallbackData, 'Currency');
//        }
//    });
//}

//// Load Payment Periods dropdown
//function loadPaymentPeriods() {
//    $.ajax({
//        url: '/common/paymentPeriods',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#paymentPeriod', data, 'Select Payment Period');
//            // Set Monthly as default selected
//            $("#paymentPeriod option").each(function () {
//                if ($(this).text() === "Monthly") {
//                    $(this).prop("selected", true);
//                }
//            });
//        },
//        error: function (error) {
//            console.error('Error loading payment periods:', error);
//            // Fallback to some default data for testing
//            const fallbackData = [
//                { id: "Daily", name: "Daily" },
//                { id: "Weekly", name: "Weekly" },
//                { id: "Monthly", name: "Monthly", isSelected: "selected" },
//                { id: "Quarterly", name: "Quarterly" },
//                { id: "Half-Yearly", name: "Half-Yearly" },
//                { id: "Yearly", name: "Yearly" }
//            ];
//            populateDropdown('#paymentPeriod', fallbackData, 'Select Payment Period');
//            // Set Monthly as default selected
//            $("#paymentPeriod option").each(function () {
//                if ($(this).text() === "Monthly") {
//                    $(this).prop("selected", true);
//                }
//            });
//        }
//    });
//}

//// Load Payment Modes dropdown
//function loadPaymentModes() {
//    $.ajax({
//        url: '/common/paymentModes',
//        type: 'GET',
//        dataType: 'json',
//        success: function (data) {
//            populateDropdown('#modeOfPayment', data, 'Payment Mode');
//        },
//        error: function (error) {
//            console.error('Error loading payment modes:', error);
//            // Fallback to some default data for testing
//            const fallbackData = [
//                { id: "1", name: "Cash" },
//                { id: "2", name: "Card" },
//                { id: "3", name: "BT" }
//            ];
//            populateDropdown('#modeOfPayment', fallbackData, 'Payment Mode');
//        }
//    });
//}



//// Helper function to populate dropdowns
//function populateDropdown(selector, data, defaultText) {
//    const dropdown = $(selector);
//    dropdown.empty();

//    if (defaultText) {
//        dropdown.append(`<option selected disabled>${defaultText}</option>`);
//    }

//    $.each(data, function (key, entry) {
//        const option = $("<option></option>")
//            .attr("value", entry.id)
//            .text(entry.name);

//        if (entry.isSelected === "selected") {
//            option.attr("selected", "selected");
//        }

//        dropdown.append(option);
//    });
//}

//// Set up dynamic dropdowns with dependencies
//function setupDynamicDropdowns() {
//    // Load departments when office and branch are selected
//    $('#office, #branch').on('change', function () {
//        const officeId = $('#office').val();
//        const branchId = $('#branch').val();
//        if (officeId && branchId) {
//            loadDepartments(officeId, branchId);
//        }
//    });

//    // Initial loading of other dropdowns
//    loadDesignations();
//    loadEmployeeTypes();
//    loadGradeNumbers();
//}

//// Set up "Add New" forms
//function setupAddNewForms() {
//    // Department form
//    $('#deptCancel').on('click', function () {
//        $('#deptId, #deptName').val('');
//        $(this).closest('.dropdown-menu').removeClass('show');
//    });

//    $('#deptSave').on('click', function () {
//        const deptId = $('#deptId').val();
//        const deptName = $('#deptName').val();

//        if (!deptId || !deptName) {
//            alert('Please enter both Department ID and Name');
//            return;
//        }

//        $.ajax({
//            url: '/common/departments',
//            type: 'POST',
//            data: { id: deptId, name: deptName },
//            success: function (result) {
//                // Add the new option to the dropdown
//                $('#department').append(new Option(deptName, deptId, true, true));

//                // Clear and close the form
//                $('#deptId, #deptName').val('');
//                $('.dropdown-menu.show').removeClass('show');

//                alert('Department added successfully!');
//            },
//            error: function (error) {
//                console.error('Error adding department:', error);
//                alert('Failed to add department. Please try again.');
//            }
//        });
//    });

//    // Designation form
//    $('#desigCancel').on('click', function () {
//        $('#desigId, #desigName').val('');
//        $(this).closest('.dropdown-menu').removeClass('show');
//    });

//    $('#desigSave').on('click', function () {
//        const desigId = $('#desigId').val();
//        const desigName = $('#desigName').val();

//        if (!desigId || !desigName) {
//            alert('Please enter both Designation ID and Name');
//            return;
//        }

//        $.ajax({
//            url: '/common/designations',
//            type: 'POST',
//            data: { id: desigId, name: desigName },
//            success: function (result) {
//                // Add the new option to the dropdown
//                $('#designation').append(new Option(desigName, desigId, true, true));

//                // Clear and close the form
//                $('#desigId, #desigName').val('');
//                $('.dropdown-menu.show').removeClass('show');

//                alert('Designation added successfully!');
//            },
//            error: function (error) {
//                console.error('Error adding designation:', error);
//                alert('Failed to add designation. Please try again.');
//            }
//        });
//    });

//    // Employee Type form
//    $('#empTypeCancel').on('click', function () {
//        $('#empTypeId, #empTypeName').val('');
//        $(this).closest('.dropdown-menu').removeClass('show');
//    });

//    $('#empTypeSave').on('click', function () {
//        const empTypeId = $('#empTypeId').val();
//        const empTypeName = $('#empTypeName').val();

//        if (!empTypeId || !empTypeName) {
//            alert('Please enter both Employee Type ID and Name');
//            return;
//        }

//        $.ajax({
//            url: '/common/employeeTypes',
//            type: 'POST',
//            data: { id: empTypeId, name: empTypeName },
//            success: function (result) {
//                // Add the new option to the dropdown
//                $('#empType').append(new Option(empTypeName, empTypeId, true, true));

//                // Clear and close the form
//                $('#empTypeId, #empTypeName').val('');
//                $('.dropdown-menu.show').removeClass('show');

//                alert('Employee Type added successfully!');
//            },
//            error: function (error) {
//                console.error('Error adding employee type:', error);
//                alert('Failed to add employee type. Please try again.');
//            }
//        });
//    });

//    // Grade form
//    $('#gradeCancel').on('click', function () {
//        $('#gradeId, #gradeName').val('');
//        $(this).closest('.dropdown-menu').removeClass('show');
//    });

//    $('#gradeSave').on('click', function () {
//        const gradeId = $('#gradeId').val();
//        const gradeName = $('#gradeName').val();

//        if (!gradeId || !gradeName) {
//            alert('Please enter both Grade ID and Name');
//            return;
//        }

//        $.ajax({
//            url: '/common/grades',
//            type: 'POST',
//            data: { id: gradeId, name: gradeName },
//            success: function (result) {
//                // Add the new option to the dropdown
//                $('#gradeNo').append(new Option(gradeName, gradeId, true, true));

//                // Clear and close the form
//                $('#gradeId, #gradeName').val('');
//                $('.dropdown-menu.show').removeClass('show');

//                alert('Grade added successfully!');
//            },
//            error: function (error) {
//                console.error('Error adding grade:', error);
//                alert('Failed to add grade. Please try again.');
//            }
//        });
//    });
//}

//// Save employee official information
//function saveEmployeeOfficial() {
//    // Get form data
//    const employeeOfficialData = {
//        officeId: $('#office').val(),
//        branchId: $('#branch').val(),
//        departmentId: $('#department').val(),
//        designationId: $('#designation').val(),
//        employeeTypeId: $('#empType').val(),
//        employmentNatureId: $('#empNature').val(),
//        gradeId: $('#gradeNo').val(),
//        grossSalary: $('#grossSalary').val(),
//        currencyId: $('#curency').val(),
//        paymentPeriod: $('#paymentPeriod').val(),
//        modeOfPaymentId: $('#modeOfPayment').val(),
//        bankPercent: $('#bankPercent').val(),
//        immediateSupervisorId: $('#immSupervisor').val(),
//        hodId: $('#hod').val(),
//        officialPhone: $('#officialPhone').val(),
//        officialEmail: $('#officialEmail').val(),
//        attendanceId: $('#attendenceId').val(),
//        employeeStatusId: $('#employeeStatus').val(),

//        // Joining information
//        appointmentLetterNo1: $('#appointmentLetterNo1').val(),
//        appointmentLetterDate: $('#appointmentLetterDate').val(),
//        joiningDate: $('#joiningDate').val(),
//        appointmentLetterNo2: $('#appointmentLetterNo2').val(),
//        probationPeriod: $('#probationPeriod').val(),
//        timeUnitId: $('#time').val(),
//        confirmationDate: $('#confirmationDate').val(),
//        confirmationLetterNo: $('#confirmationLetterNo').val(),
//        contractEndDate: $('#contractEndDate').val()
//    };

//    // Validate required fields
//    if (!validateRequiredFields()) {
//        return false;
//    }

//    // Send data to the server
//    $.ajax({
//        url: '/employee/saveOfficialInfo',
//        type: 'POST',
//        contentType: 'application/json',
//        data: JSON.stringify(employeeOfficialData),
//        success: function (response) {
//            alert('Employee official information saved successfully!');
//        },
//        error: function (error) {
//            console.error('Error saving employee information:', error);
//            alert('Failed to save employee information. Please try again.');
//        }
//    });
//}

//// Validate required fields
//function validateRequiredFields() {
//    let isValid = true;
//    const requiredFields = [
//        'office', 'branch', 'department', 'designation',
//        'empType', 'empNature', 'gradeNo', 'employeeStatus',
//        'joiningDate'
//    ];

//    requiredFields.forEach(field => {
//        const element = $('#' + field);
//        if (!element.val()) {
//            isValid = false;
//            element.addClass('is-invalid');
//        } else {
//            element.removeClass('is-invalid');
//        }
//    });

//    if (!isValid) {
//        alert('Please fill all required fields');
//    }

//    return isValid;
//}


//#endregion
