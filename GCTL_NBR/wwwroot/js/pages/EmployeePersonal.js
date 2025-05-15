

$(document).ready(function () {


   


    NextEmployeeCode();
    //#region Submit Form

    $("#btnSavePersonal").click(function () {



        var formData = new FormData();
        var isValid = true;

        // Reset all error messages and remove previous invalid classes
        $(".invalid-feedback").remove();
        $(".form-control").removeClass("is-invalid");

        // Employee ID (readonly, no validation needed)
        formData.append("EmployeeCode", $("#employeeCodePersonal").val() || '');
        formData.append("Office", $("#officePersonal").val() || '');

        // First Name validation
        var firstName = $("#firstNamep").val().trim();
        if (firstName === "") {
            isValid = false;
            $("#firstNamep").addClass("is-invalid");
            $("#firstNamep").after('<div class="invalid-feedback">First name is required.</div>');
        } else {
            formData.append("FirstName", firstName);
        }

        // Last Name validation
        var lastName = $("#lastNamep").val().trim();
        if (lastName === "") {
            isValid = false;
            $("#lastNamep").addClass("is-invalid");
            $("#lastNamep").after('<div class="invalid-feedback">Last name is required.</div>');
        } else {
            formData.append("LastName", lastName);
        }

        // Gender validation
        var gender = $("#genderPersonal").val();
        if (gender === "Select Gender") {
            isValid = false;
            $("#genderPersonal").addClass("is-invalid");
            $("#genderPersonal").after('<div class="invalid-feedback">Please select a gender.</div>');
        } else {
            formData.append("Gender", gender);
        }

        // Marital Status validation
        var maritalStatus = $("#maritalStatusPersonal").val();
        if (maritalStatus === "Select Marital Status") {
            isValid = false;
            $("#maritalStatusPersonal").addClass("is-invalid");
            $("#maritalStatusPersonal").after('<div class="invalid-feedback">Please select a marital status.</div>');
        } else {
            formData.append("MaritalStatus", maritalStatus);
        }

        // Mobile validation
        var mobile = $("#mobilep").val().trim();
        if (mobile === "") {
            isValid = false;
            $("#mobilep").addClass("is-invalid");
            $("#mobilep").after('<div class="invalid-feedback">Mobile number is required.</div>');
        } else {
            formData.append("PersonalMobile", mobile);
        }

        // Email validation (simple regex)
        var email = $("#emailp").val().trim();
        var emailPattern = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,6}$/;
        if (email === "") {
            isValid = false;
            $("#emailp").addClass("is-invalid");
            $("#emailp").after('<div class="invalid-feedback">Email is required.</div>');
        } else if (!emailPattern.test(email)) {
            isValid = false;
            $("#emailp").addClass("is-invalid");
            $("#emailp").after('<div class="invalid-feedback">Please enter a valid email.</div>');
        } else {
            formData.append("PersonalEmail", email);
        }



        if (isValid) {
            var formData = new FormData();

            formData.append("EmployeeCode", $("#employeeCodePersonal").val() || '');
            formData.append("Office", $("#officePersonal").val() || '');
            formData.append("Branch", $("#branchPersonal").val() || '');
            formData.append("EmployeeID", $("#employeeIdp").val() || '');
            formData.append("FirstName", $("#firstNamep").val() || '');
            formData.append("LastName", $("#lastNamep").val() || ''); // Fixed
            formData.append("FatherName", $("#fatherNamep").val() || '');
            formData.append("MotherName", $("#motherNamep").val() || '');
            formData.append("DateOfBirth", $("#floatingInputReportDatep").val() || '');
            formData.append("BirthCertificateNo", $("#bcnp").val() || '');
            formData.append("BirthPlace", $("#bPlacep").val() || '');
            formData.append("Gender", $("#genderPersonal").val() || '');
            formData.append("BloodGroup", $("#bloodGroupPersonal").val() || '');
            formData.append("Nationality", $("#nationalityPersonal").val() || '');
            formData.append("NationalID", $("#nationalIDPersonal").val() || ''); // Fixed
            formData.append("Religion", $("#religionPersonal").val() || '');
            formData.append("MaritalStatus", $("#maritalStatusPersonal").val() || '');
            formData.append("CardNo", $("#cardNop").val() || '');
            formData.append("PersonalMobile", $("#mobilep").val() || '');
            formData.append("PersonalEmail", $("#emailp").val() || '');
            formData.append("TINNo", $("#tinp").val() || '');
            formData.append("OtherActivities", $("#floatingProjectOverview").val() || '');

            // Handling file uploads
            var epImage = $("#epImageUpload")[0].files[0];
            var esImage = $("#esImageUpload")[0].files[0];

            if (epImage) {
                formData.append("EmployeePicture", epImage);
            }
            if (esImage) {
                formData.append("Signature", esImage);
            }

            console.log(formData);

            $.ajax({
                url: "/EmployeePersonal/SaveEmployeeInfo",
                type: "POST",
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    console.log('1', response);
                    if (response.success) {
                        var paramValue = response.data.data;


                        sessionStorage.removeItem("tabParameter");
                        sessionStorage.setItem("tabParameter", paramValue);

                        toastr.success(response.message)

                        // Get current tab dynamically inside the event
                        var currentTab = $("#btnSavePersonal").closest(".tab-pane"); // Get current tab pane
                        var currentTabId = currentTab.attr("id"); // Get current tab ID

                        // Move to the next tab
                        var $currentTabLink = $('a[href="#' + currentTabId + '"]'); // Get current tab link
                        var $nextTabLink = $currentTabLink.parent().next().find(".nav-link"); // Get next tab link

                        if ($nextTabLink.length) {
                            $nextTabLink.tab("show"); // Show next tab
                        } else {
                            window.location.href = "/employeeofficial/index";
                        }
                    }


                    else {
                        toastr.error(response.message)
                    }






                },
                error: function (xhr, status, error) {
                    alert("Error saving data: " + error);
                }
            });
        } else {
            console.log("Form validation failed.");
        }



        
    });



    //#endregion

    //#region Reset Form
    function resetForm() {
        $("#empPersonalInfo input").val(""); // Clear all input fields
        $("#empPersonalInfo select").prop("selectedIndex", 0); // Reset all select elements
        $("#floatingInputReportDate").val(""); // Clear date input
        $("#floatingProjectOverview").val(""); // Clear textarea input
        $("#epImageUpload").val(""); // Clear file input for employee picture
        $("#esImageUpload").val(""); // Clear file input for signature
        resetImagePreview(['#epImagePreview', '#esImagePreview']);
    }
    function resetImagePreview(previewElements) {
        previewElements.forEach(function (element) {
            $(element).attr('src', '').css('visibility', 'hidden'); // Clear src and hide the element
        });
    }

   

    //#endregion


    //#region Function to handle image preview
    //function previewImage(input, previewElement) {
    //    const file = input.files[0]; // Get the first file selected

    //    // If a file is selected
    //    if (file) {
    //        const reader = new FileReader();

    //        // Set up the callback function that will run once the file is read
    //        reader.onload = function (e) {
    //            $(previewElement).attr('src', e.target.result).css('visibility', 'visible');
    //        };

    //        reader.readAsDataURL(file); // Read the image file as a data URL (base64 string)
    //    }
    //}

    //// Event listeners for file inputs
    //$('#epImageUpload').on('change', function () {
    //    previewImage(this, '#epImagePreview');
    //});

    //$('#esImageUpload').on('change', function () {
    //    previewImage(this, '#esImagePreview');
    //});


    function previewImage(input, previewElement, closeButton) {
        const file = input.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $(previewElement).attr('src', e.target.result).css('visibility', 'visible');
                $(closeButton).css('visibility', 'visible');
            };
            reader.readAsDataURL(file);
        }
    }

    function removeImage(previewElement, inputElement, closeButton) {
        $(previewElement).attr('src', '').css('visibility', 'hidden');
        $(closeButton).css('visibility', 'hidden');
        $(inputElement).val('');
    }

    // Bind events
    $('#epImageUpload').on('change', function () {
        previewImage(this, '#epImagePreview', '#epCloseBtn');
    });

    $('#epCloseBtn').on('click', function () {
        removeImage('#epImagePreview', '#epImageUpload', '#epCloseBtn');
    });

    $('#esImageUpload').on('change', function () {
        previewImage(this, '#esImagePreview', '#esCloseBtn');
    });

    $('#esCloseBtn').on('click', function () {
        removeImage('#esImagePreview', '#esImageUpload', '#esCloseBtn');
    });


    //#endregion

    //#region Next Employee Code

    // Get the next employee code when the page loads
   

    function NextEmployeeCode() {
        $.ajax({
            url: '/EmployeePersonal/getNextEmpId',
            type: 'GET', // Assuming it’s a GET request
            success: function (data) {
                console.log('Next Employee Code:', data);
                $("#employeeIdp").val(data);
            },
            error: function (xhr, status, error) {
                console.error('Error fetching Employee Code:', error);
            }
        });
    }

    //#endregion

    //#region DropDown Data Load

    // Load the dropdown data when the page loads
    populateDropdown('/common/getGender', 'genderPersonal', 'id', 'name');
    populateDropdown('/common/getMaritalStatus', 'maritalStatusPersonal', 'id', 'name');
    populateDropdown('/common/getNationality', 'nationalityPersonal', 'id', 'name');
    populateDropdown('/common/getBloodGroup', 'bloodGroupPersonal', 'id', 'name');
    populateDropdown('/common/getReligion', 'religionPersonal', 'id', 'name');

    function populateDropdown(url, dropdownId, idKey, nameKey) {
        // Perform the AJAX request
        $.ajax({
            url: url, // Endpoint to fetch the data
            type: 'GET',
            dataType: 'json', // Assuming JSON response
            success: function (response) {
                console.log('Dropdown data:', dropdownId, response);
                // Find the dropdown by its ID and clear existing options
                const $dropdown = $(`#${dropdownId}`);
                $dropdown.empty().append('<option selected="selected">Select Option</option>');

                // Loop through the response to populate the dropdown
                response.data.forEach(item => {
                    $dropdown.append(`<option value="${item[idKey]}">${item[nameKey]}</option>`);
                });
            },
            error: function (xhr, status, error) {
                console.error('Error fetching data:', error);
            }
        });
    }



    //#endregion
    
});



   




