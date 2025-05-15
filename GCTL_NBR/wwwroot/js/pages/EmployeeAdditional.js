
$(document).ready(function () {


    // First make sure the dropdown is visible and properly initialized
    $("#empCodePass2").show();

 
    let choicesInstance;





    $('input[type="hidden"][id="empCodePass2"]').attr('id', 'empCodePassHidden');

    //// First, let's remove or rename the duplicate ID
    //// This code will rename the hidden input to avoid the conflict

    //// Now let's make sure we're working with the correct select element
    //const $dropdown = $('select[id="empCodePass2"]');

    //// Debug to verify we found the correct element
    ////console.log('Found dropdown element:', $dropdown.length > 0, $dropdown.prop('tagName'));



    getAllemp();


    function getAllemp() {
        $.ajax({
            url: '/EmployeePersonal/GetEmployeeList',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                //console.log('Data received:', data);
                const $dropdown = $("#empCodePass2");

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
                choicesInstance = new Choices("#empCodePass2", {
                    searchEnabled: true,
                    itemSelectText: 'Select',
                    removeItemButton: true,
                    allowHTML: false
                });

                //debugger

                // Check session storage AFTER initializing Choices.js
                var paramValue = sessionStorage.getItem("tabParameter1");
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
        //debugger
        var paramValue = sessionStorage.getItem("tabParameter1");
        //console.log("Received Parameter on page load:", paramValue);

        if (paramValue) {
            //console.log('Making AJAX request for empCode:', paramValue);
            $.ajax({
                url: '/EmployeePersonal/getEmpInfo',
                type: 'GET',
                dataType: 'json',
                data: { empCode: paramValue },
                success: function (response) {
                    console.log('Received Employee Data1111:', response);

                    if (response) {
                        $("#NidPass2").val(response.nid || '');
                        $("#BirthCertiPass2").val(response.birthCertificateNo || '');
                        $("#empIdMain2").val(response.employeeCode);

                        // Set dropdown value using Choices.js API
                        if (response.employeeID && choicesInstance) {
                            choicesInstance.setChoiceByValue(response.employeeID.toString());

                            // Trigger change event to load additional data
                            $("#empCodePass2").val(response.employeeID).trigger('change');
                        }
                        sessionStorage.removeItem("tabParameter1");
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                }
            });
        } else {
            //console.log('No empCode found in sessionStorage111');
            $("#NidPass2, #BirthCertiPass2, #empCodePass2, #empIdMain2").val('');
        }
    }




    $("#empCodePass2").on('change', function () {
        var selectedEmpID = $(this).val();
        if (selectedEmpID && selectedEmpID !== 'Select Employee') {
            $.ajax({
                url: '/EmployeePersonal/getEmpInfoById',
                type: 'GET',
                dataType: 'json',
                data: { empId: selectedEmpID },
                success: function (response) {
                    //console.log('Additional', response)
                    // Update the fields with the selected employee's data
                    $("#NidPass2").val(response.nid || '');
                    $("#BirthCertiPass2").val(response.birthCertificateNo || '');
                    $("#empIdMain2").val(response.employeeCode);

                },
                error: function (xhr, status, error) {
                    console.error('Error fetching employee details:', error);
                }
            });
        } else {
            // Clear fields if "Select Employee" is chosen
            $("#NidPass2").val('');
            $("#BirthCertiPass2").val('');
        }
    });




   






    $("#btnSaveAdditional").click(function () {


        $(".invalid-feedback").remove();
        $(".form-control").removeClass("is-invalid");

        var isValid = true;


        var employeeData1 = {
            EmpAdditionalId: parseInt($("#empAdditionalId").val()) || 0,
            EmpCode: $("#empIdMain2").val(),
            EmployeeId: parseInt($("#empCodePass2").val()) || 0,
            PassportName: $("#passportName").val(),
            PassportNo: $("#passportNo").val(),
            PlaceOfIssuePassport: $("#placeOfIssuePassport").val(),
            DateIssuePassport: $("#dateIssuePassport").val() ? new Date($("#dateIssuePassport").val()).toISOString().split('T')[0] : null,
            DateExpiryPassport: $("#dateExperyPassport").val() ? new Date($("#dateExperyPassport").val()).toISOString().split('T')[0] : null,
            BankName: parseInt($("#bankForm").val()) || 0,
            BranchName: parseInt($("#branchForm").val()) || 0,
            Address: $("#address").val(),
            AcName: $("#acName").val(),
            AcNumber: $("#acNumber").val(),
            AtmCardName: $("#atmCardName").val(),
            BkashNumber: $("#bkashNumber").val(),
            RocketNumber: $("#rocketNumber").val(),
            LicenseNo: $("#licenseNo").val(),
            LicenseType: $("#licenseType").val(),
            DateIssueLicense: $("#dateIssueLicense").val() ? new Date($("#dateIssueLicense").val()).toISOString().split('T')[0] : null,
            DateExpiryLicense: $("#dateExpiryLicense").val() ? new Date($("#dateExpiryLicense").val()).toISOString().split('T')[0] : null,
            SymbolVehicle: $("#SymbolVehicle").val(),
            PlaceOfIssueLicense: $("#placeOfIssueLicense").val(),
            WorkPermitNumber: $("#workPermitNumber").val(),
            WorkPermitType: $("#workPermitType").val(),
            WpEffectiveDate: $("#wpEffectiveDate").val() ? new Date($("#wpEffectiveDate").val()).toISOString().split('T')[0] : null,
            WpExpiryDate: $("#wpExpiryDate").val() ? new Date($("#wpExpiryDate").val()).toISOString().split('T')[0] : null
        };


        // Validation Checks
        if (!employeeData1.EmployeeId) {
            isValid = false;
            $("#empCodePass2").addClass("is-invalid").after('<div class="invalid-feedback">Employee ID is required.</div>');
        }

        if (!employeeData1.PassportNo) {
            isValid = false;
            $("#passportNo").addClass("is-invalid").after('<div class="invalid-feedback">Passport Number is required.</div>');
        }

        if (!employeeData1.DateIssuePassport && employeeData1.DateExpiryPassport) {
            isValid = false;
            $("#dateIssuePassport").addClass("is-invalid").after('<div class="invalid-feedback">Passport issue date is required if expiry date is provided.</div>');
        }

        if (!employeeData1.BankName) {
            isValid = false;
            $("#bankForm").addClass("is-invalid").after('<div class="invalid-feedback">Bank Name is required.</div>');
        }

        if (!employeeData1.BranchName) {
            isValid = false;
            $("#branchForm").addClass("is-invalid").after('<div class="invalid-feedback">Branch Name is required.</div>');
        }

        if (!employeeData1.AcNumber) {
            isValid = false;
            $("#acNumber").addClass("is-invalid").after('<div class="invalid-feedback">Account Number is required.</div>');
        }

        if (!isValid) {
            return; // Stop execution if validation fails
        }


        //console.log("Sending data:", employeeData1);

        $.ajax({
            url: "/Employee/SaveEmployeeAdditional",
            type: "POST",
            data: JSON.stringify(employeeData1),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                if (response.success) {
                    //console.log('1', response);
                 //debugger
                    var paramValue = response.data;


                    sessionStorage.removeItem("tabParameter");
                    sessionStorage.setItem("tabParameter", paramValue);
                    sessionStorage.setItem("tabParameter3", paramValue);



                    // Get current tab dynamically inside the event
                    var currentTab = $("#btnSaveAdditional").closest(".tab-pane"); // Get current tab pane
                    var currentTabId = currentTab.attr("id"); // Get current tab ID

                    // Move to the next tab
                    var $currentTabLink = $('a[href="#' + currentTabId + '"]'); // Get current tab link
                    var $nextTabLink = $currentTabLink.parent().next().find(".nav-link"); // Get next tab link

                    if ($nextTabLink.length) {
                        $nextTabLink.tab("show"); // Show next tab
                    } else {
                        window.location.href = "/employeeeducational/index";
                    }

                } else {
                    toastr.warning(response.message);
                    //alert("Error: " + response.message);
                }
            },
            error: function () {
                alert("An error occurred while saving data.");
            }
        });
    });

    // Function to reset the form
    function resetaddiForm() {
        alert()
        // Clear all input fields
        $("input").val("");

        // Reset all dropdowns to the first option
        $("select").each(function () {
            $(this).prop("selectedIndex", 0);
        });

        // Reset Flatpickr datepickers
        $(".datetimepicker").each(function () {
            if ($(this)._flatpickr) {
                $(this)._flatpickr.clear();
            }
        });
    }

    $("#empCodePass2").on('change', function () {
        var empId = $(this).val();
        resetForm();

         getAdditionalFormData(empId);
       
    })

    $("#bankForm").on('change', function () {
        var bankId = $(this).val();
        populateBankBranch(bankId);
    })


    function getAdditionalFormData(empId) {
        if (empId) {
            //console.log('Making AJAX request for empCode:', empId);
            $.ajax({
                url: '/Employeeadditional/getEmpadditionakInfo',
                type: 'GET',
                dataType: 'json',
                data: { empId: empId },
                success: function (response) {

                    //console.log('Received Employee Addition Data:', response);
                    // Add code to populate the fields with the response data
                    if (response.success) {
                        populateAddiForm(response.data)
                    } else {

                    }

                    sessionStorage.removeItem("tabParameter");
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                    //console.log('XHR Response Text:', xhr.responseText);
                }
            });
        } else {
            //console.log('No empCode found in sessionStorage');


        }
    }

    function populateAddiForm(data) {
        //console.log('55555555555555555', data)
        $('#empAdditionalId').val(data.employeeAdditinalInfoID);
        $('#passportName').val(data.pasportName);
        $('#passportNo').val(data.enterPasportNo);
        $('#placeOfIssuePassport').val(data.pasportPlaceOfIssue);
        $('#dateIssuePassport').val(data.pasportDateIssued);
        $('#dateExperyPassport').val(data.passportExpiryDate);

        $('#bankForm').val(data.salaryBankID);
        $('#branchForm').val(data.salaryBankBranchID);
        $('#acName').val(data.bankAccountName);
        $('#acNumber').val(data.bankAccountNumber);
        $('#atmCardName').val(data.atmCardNumber);
        $('#bkashNumber').val(data.bKashAccountNumber);
        $('#rocketNumber').val(data.roketAccountNumber);

        $('#licenseNo').val(data.drivingLicenceNo);
        $('#licenseType').val(data.drivingLicenceType);
        $('#dateIssueLicense').val(data.drivingLicenceIssueDate);
        $('#dateExpiryLicense').val(data.drivingLicenceExpiryDate);
        $('#SymbolVehicle').val(data.symbolOfVehicleClass);
        $('#placeOfIssueLicense').val(data.drivingLicencePlaceOfIssue);

        $('#workPermitNumber').val(data.workPermaitNumber);
        $('#workPermitType').val(data.workPermitType);
        $('#wpEffectiveDate').val(data.workPermitEffectiveDate);
        $('#wpExpiryDate').val(data.workPermitExpiryDate);
    }



    $("#btnSaveAdditional1").click(function () {
        var formData = {
            PassportName: $("#passportName").val(),
            PassportNo: $("#passportNo").val(),
            PlaceOfIssuePassport: $("#placeOfIssuePassport").val(),
            DateIssuePassport: $("#dateIssuePassport").val(),
            DateExpiryPassport: $("#dateExperyPassport").val(),

            BankId: $("#bankForm").val(),
            BranchId: $("#branchForm").val(),
            Address: $("#address").val(),
            AcName: $("#acName").val(),
            AcNumber: $("#acNumber").val(),
            AtmCardName: $("#atmCardName").val(),
            BkashNumber: $("#bkashNumber").val(),
            RocketNumber: $("#rocketNumber").val(),

            LicenseNo: $("#licenseNo").val(),
            LicenseType: $("#licenseType").val(),
            DateIssueLicense: $("#dateIssueLicense").val(),
            DateExpiryLicense: $("#dateExpiryLicense").val(),
            SymbolVehicle: $("#SymbolVehicle").val(),
            PlaceOfIssueLicense: $("#placeOfIssueLicense").val(),

            WorkPermitNumber: $("#workPermitNumber").val(),
            WorkPermitType: $("#workPermitType").val(),
            WpEffectiveDate: $("#wpEffectiveDate").val(),
            WpExpiryDate: $("#wpExpiryDate").val(),

            empCode: $("#empIdMain2").val(),
            empAdditionalId: $("#empAdditionalId").val()
        };
        //console.log(formData)
        $.ajax({
            url: "/AdditionalInfo/Save",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                //console.log('11', response)
                if (response) {

                    alert("Data saved successfully!");
                    resetForm()
                }
            },
            error: function (xhr, status, error) {
                alert("Error saving data: " + error);
            }
        });
    });

    

    function resetForm() {
        $('#myTabContent input[type="text"], #myTabContent input[type="number"]').val(''); // Clear text and number inputs
        $('#myTabContent select').prop('selectedIndex', 0); // Reset dropdowns to default

        // Check if Flatpickr is initialized before trying to clear
        $('.datetimepicker').each(function () {
            if ($(this).hasClass('flatpickr-input')) {
                this._flatpickr.clear(); // Clear Flatpickr date fields
            }
        });
    }

    //#region populate by id

    function populateBankBranch(bankId) {
        if (bankId) {
            $.ajax({
                url: '/Branch/GetAllByBank', // Change this URL to match your backend route for fetching branches
                type: 'GET',
                data: { bankId: bankId },
                success: function (response) {
                    //console.log('122', response)
                    // Empty current dropdown
                    $('#branchForm').empty();
                    $('#branchForm').append('<option selected="selected">Select Branch</option>');

                    // Populate dropdown with branch options
                    $.each(response.data, function (index, branch) {
                        $('#branchForm').append(`<option value="${branch.id}">${branch.name}</option>`);
                    });
                },
                error: function () {
                    // alert("Error fetching branch data.");
                }
            });
        }
    }


    //#endregion


    //#region Bank and Branch
    // Function to add a new bank
    $('#btnBankSave').click(function () {
        var bankId = $('#bankId').val();
        var bankName = $('#bankName').val();

        if (bankName.trim() === '') {
            alert("Bank Name cannot be empty!");
            return;
        }

        $.ajax({
            url: '/Bank/Add', // Change this URL to match your backend route
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ BankId: bankId, BankName: bankName }),
            success: function (response) {
                alert("Bank Added Successfully!");

                // Append the new bank to the dropdown
                $('#bankForm').append(`<option value="${response.bankId}">${response.bankName}</option>`);

                // Clear input fields after adding
                $('#bankId').val('');
                $('#bankName').val('');
            },
            error: function () {
                alert("Error adding bank.");
            }
        });
    });

    // Function to add a new branch
    $('#btnBranchSave').click(function () {
        var branchId = $('#branchId').val();
        var branchName = $('#branchName').val();

        if (branchName.trim() === '') {
            alert("Branch Name cannot be empty!");
            return;
        }

        $.ajax({
            url: '/Branch/Add', // Change this URL to match your backend route
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ BranchId: branchId, BranchName: branchName }),
            success: function (response) {
                alert("Branch Added Successfully!");

                // Append the new branch to the dropdown
                $('#branchForm').append(`<option value="${response.branchId}">${response.branchName}</option>`);

                // Clear input fields after adding
                $('#branchId').val('');
                $('#branchName').val('');
            },
            error: function () {
                alert("Error adding branch.");
            }
        });
    });
    //#endregion

    //#region Function to populate the Bank and Branch dropdown
    function populateBankDropdown() {
        $.ajax({
            url: '/Bank/GetAll', // Change this URL to match your backend route for fetching banks
            type: 'GET',
            success: function (response) {
                //console.log('1', response)
                // Empty current dropdown
                $('#bankForm').empty();
                $('#bankForm').append('<option selected="selected">Select Bank</option>');

                // Populate dropdown with bank options
                $.each(response.data, function (index, bank) {
                    $('#bankForm').append(`<option value="${bank.id}">${bank.name}</option>`);
                });
            },
            error: function () {
                alert("Error fetching bank data.");
            }
        });
    }

    // Function to populate the Branch dropdown
    function populateBranchDropdown() {
        $.ajax({
            url: '/Branch/GetAll', // Change this URL to match your backend route for fetching branches
            type: 'GET',
            success: function (response) {
                //console.log('122', response)
                // Empty current dropdown
                $('#branchForm').empty();
                $('#branchForm').append('<option selected="selected">Select Branch</option>');

                // Populate dropdown with branch options
                $.each(response.data, function (index, branch) {
                    $('#branchForm').append(`<option value="${branch.id}">${branch.name}</option>`);
                });
            },
            error: function () {
               // alert("Error fetching branch data.");
            }
        });
    }


    // Function to populate the Branch dropdown
    function populateLicenseDropdown() {
        $.ajax({
            url: '/License/GetAll', // Change this URL to match your backend route for fetching branches
            type: 'GET',
            success: function (response) {

                //console.log('55555555555555555', response)
                // Empty current dropdown
                $('#licenseType').empty();
                $('#licenseType').append('<option selected="selected">Select License</option>');

                // Populate dropdown with branch options
                $.each(response.data, function (index, license) {
                    $('#licenseType').append(`<option value="${license.id}">${license.name}</option>`);
                });
            },
            error: function () {
                alert("Error fetching branch data.");
            }
        });
    }



    //#endregion

    // Call functions to populate dropdowns on page load
    populateBankDropdown();
    populateBranchDropdown();
    populateLicenseDropdown();

});




 


