
$(document).ready(function () {

    let choicesInstance;

    // First make sure the dropdown is visible and properly initialized
    $("#empCodePass4").show();

   
    $('input[type="hidden"][id="empCodePass4"]').attr('id', 'empCodePassHidden');






    getAllemp();


    function getAllemp() {
        $.ajax({
            url: '/EmployeePersonal/GetEmployeeList',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                //console.log('Data received:', data);
                const $dropdown = $("#empCodePass4");

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
                choicesInstance = new Choices("#empCodePass4", {
                    searchEnabled: true,
                    itemSelectText: 'Select',
                    removeItemButton: true,
                    allowHTML: false
                });

                //debugger

                // Check session storage AFTER initializing Choices.js
                var paramValue = sessionStorage.getItem("tabParameter4");
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
        var paramValue = sessionStorage.getItem("tabParameter4");
        //console.log("Received Parameter on page load:", paramValue);

        if (paramValue) {
            //console.log('Making AJAX request for empCode:', paramValue);
            $.ajax({
                url: '/EmployeePersonal/getEmpInfo',
                type: 'GET',
                dataType: 'json',
                data: { empCode: paramValue },
                success: function (response) {
                    //console.log('Received Employee Data1111:', response);

                    if (response) {
                        $("#NidPass4").val(response.nid || '');
                        $("#BirthCertiPass4").val(response.birthCertificateNo || '');
                        $("#empIdMain4").val(response.employeeCode);

                        // Set dropdown value using Choices.js API
                        if (response.employeeID && choicesInstance) {
                            choicesInstance.setChoiceByValue(response.employeeID.toString());

                            // Trigger change event to load additional data
                            $("#empCodePass4").val(response.employeeID).trigger('change');
                        }

                        sessionStorage.removeItem("tabParameter4");
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                }
            });
        } else {
            //console.log('No empCode found in sessionStorage111');
            $("#NidPass4, #BirthCertiPass4, #empCodePass4, #empIdMain4").val('');
        }
    }









    $("#empCodePass4").on('change', function () {
        var selectedEmpID = $(this).val();
        if (selectedEmpID && selectedEmpID !== 'Select Employee') {
            $.ajax({
                url: '/EmployeePersonal/getEmpInfoById',
                type: 'GET',
                dataType: 'json',
                data: { empId: selectedEmpID },
                success: function (response) {
                    //console.log('1111', response)
                    // Update the fields with the selected employee's data
                    $("#NidPass4").val(response.nid || '');
                    $("#BirthCertiPass4").val(response.birthCertificateNo || '');
                    $("#empIdMain4").val(response.employeeCode);

                },
                error: function (xhr, status, error) {
                    console.error('Error fetching employee details:', error);
                }
            });
        } else {
            // Clear fields if "Select Employee" is chosen
            $("#NidPass4").val('');
            $("#BirthCertiPass4").val('');
        }
    });



 



   // $("#empCodePass4").show();


    //function getAllemp() {
    //    $.ajax({
    //        url: '/EmployeePersonal/GetEmployeeList',
    //        type: 'GET',
    //        dataType: 'json',
    //        success: function (data) {
    //            //console.log('Data received:', data);
    //            const $dropdown = $("#empCodePass4");

    //            // Destroy previous Choices instance if it exists
    //            if (choicesInstance) {
    //                choicesInstance.destroy();
    //            }

    //            // Clear and populate dropdown
    //            $dropdown.empty().append(`<option value="" selected >Select Employee</option>`);

    //            if (Array.isArray(data) && data.length > 0) {
    //                data.forEach(employee => {
    //                    $dropdown.append(`
    //                         <option value="${employee.employeeID}">
    //                             ${employee.fullName} (${employee.employeeCode})
    //                         </option>
    //                     `);
    //                });

    //                //console.log('Dropdown populated with ' + data.length + ' employees');
    //            } else {
    //                //console.log('No employees found or invalid data format:', data);
    //            }

    //            // Initialize Choices.js with safe settings
    //            choicesInstance = new Choices("#empCodePass4", {
    //                searchEnabled: true,
    //                itemSelectText: 'Select',
    //                removeItemButton: true,
    //                allowHTML: false // Explicitly set to avoid future issues
    //            });

    //        },
    //        error: function (error) {
    //            console.error("Error fetching employee list:", error);
    //        }
    //    });
    //}




    //function getAllemp() {
    //    $.ajax({
    //        url: '/EmployeePersonal/GetEmployeeList',
    //        type: 'GET',
    //        dataType: 'json',
    //        success: function (data) {
    //            //console.log('Data received:', data);
    //            const $dropdown = $("#empCodePass4");
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
    //                //console.log('Dropdown populated with ' + data.length + ' employees');
    //            } else {
    //                //console.log('No employees found or data is not in expected format:', data);
    //            }
    //        },
    //        error: function (error) {
    //            console.error("Error fetching employee list:", error);
    //        }
    //    });
    //}


    // Initialize an array to store training records

    let trainingRecords = [];
    let editIndex = -1;

    // Function to clear form fields
    function clearForm() {
        $("#empTrainingInfoId").val("");
        $("#trainingTitle").val("");
        $("#country").val("");
        $("#topicsCovered").val("");
        $("#yearPassing").val("");
        $("#instituteNameTrain").val("");
        $("#duration").val("");
        $("#location").val("");
    }

    // Function to validate form
    function validateForm() {
        let isValid = true;
        const requiredFields = [
            "#trainingTitle", "#country", "#topicsCovered",
            "#yearPassing", "#instituteNameTrain"
        ];

        requiredFields.forEach(field => {
            if ($(field).val() === "" || $(field).val() === null) {
                $(field).addClass("is-invalid");
                isValid = false;
            } else {
                $(field).removeClass("is-invalid");
            }
        });

        return isValid;
    }

    // Function to add training record to table
    function addTrainingRecord() {
        if (!validateForm()) {
            alert("Please fill all required fields");
            return; // Fixed: Uncommented the return statement
        }

        const record = {
            empTrainingInfoId: parseInt($("#empTrainingInfoId").val()) || 0,
            trainingTitle: $("#trainingTitle").val(),
            country: parseInt($("#country").val()) || 1,
            countryName: $("#country option:selected").text(), // Added: Get country name
            topicsCovered: $("#topicsCovered").val(),
            year: parseInt($("#yearPassing").val()) || 0,
            instituteName: $("#instituteNameTrain").val(),
            duration: $("#duration").val(),
            location: $("#location").val()
        };

        if (editIndex === -1) {
            // Add new record
            trainingRecords.push(record);
        } else {
            // Update existing record
            trainingRecords[editIndex] = record;
            editIndex = -1;
            $("#btnAddTraining").text("Add");
        }

        refreshTable();
        clearForm();
    }

    // Function to refresh the table with current data
    function refreshTable() {
        const tableBody = $("#trainingTable");
        tableBody.empty();

        trainingRecords.forEach((record, index) => {
            const row = `
                <tr>
                    <td style="display:none">${record.empTrainingInfoId}</td>
                    <td>${record.trainingTitle}</td>
                    <td>${record.topicsCovered}</td>
                    <td>${record.instituteName}</td>
                    <td>${record.countryName || record.country}</td> <!-- Fixed: Display country name instead of ID -->
                    <td>${record.location}</td>
                    <td>${record.year}</td>
                    <td>${record.duration}</td>
                    <td>
                        <button type="button" class="btn btn-sm btn-primary edit-btnt" data-index="${index}"><i class="fas fa-edit"></i></button>
                        <button type="button" class="btn btn-sm btn-danger delete-btnt" data-index="${index}"><i class="fas fa-trash"></i></button>
                    </td>
                </tr>
            `;
            tableBody.append(row);
        });
    }

    // Event handler for edit button
    $(document).on("click", ".edit-btnt", function (e) {
        e.preventDefault(); // Prevent default button behavior
        const index = $(this).data("index");
        editIndex = index;
        const record = trainingRecords[index];

        // Populate form with record data
        $("#empTrainingInfoId").val(record.empTrainingInfoId);
        $("#trainingTitle").val(record.trainingTitle);
        $("#country").val(record.country);
        $("#topicsCovered").val(record.topicsCovered);
        $("#yearPassing").val(record.year);
        $("#instituteNameTrain").val(record.instituteName);
        $("#duration").val(record.duration);
        $("#location").val(record.location);

        // Change button text to indicate edit mode
        $("#btnAddTraining").text("Update");

        // Scroll back to form
        $('html, body').animate({
            scrollTop: $("#trainingTitle").offset().top - 100
        }, 500);
    });

    // Event handler for delete button
    $(document).on("click", ".delete-btnt", function (e) {
        e.preventDefault(); // Prevent default button behavior
        const index = $(this).data("index");

        // Added: Confirmation dialog before deleting
        if (confirm("Are you sure you want to delete this training record?")) {
            trainingRecords.splice(index, 1);
            refreshTable();
        }
    });

    // Add button click handler
    $("#btnAddTraining").click(function (e) {
        e.preventDefault(); // Prevent default button behavior
        addTrainingRecord();
    });

    // Save button click handler
    $("#btnSaveTraining").click(function (e) {
        e.preventDefault(); // Prevent default button behavior
        if (trainingRecords.length === 0) {
            alert("Please add at least one training record");
            return;
        }

        // Prepare data to send to controller
        const trainingData = {
            trainingRecords: trainingRecords,
            EmpCode: $("#empIdMain4").val(),
            EmployeeId: parseInt($("#empCodePass4").val()) || 0,
        };

        // Send data to controller via AJAX
        $.ajax({
            url: "/training/save",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(trainingData),
            success: function (response) {
                alert("Training information saved successfully!");
                // Clear the table after successful save
                trainingRecords = [];
                refreshTable();

                window.location.href = "/employeesetup/index";
            },
            error: function (xhr, status, error) {
                alert("Error saving training information: " + error);
            }
        });
    });

    // Initialize employee data listener
    $("#empCodePass4").on('change', function () {
        var empId = $(this).val();
        trainingRecords = [];
        getTrainFormData(empId);
    });

    function getTrainFormData(empId) {
        if (empId) {
            $.ajax({
                url: '/EmployeeTraining/getEmpTrainingInfo',
                type: 'GET',
                dataType: 'json',
                data: { empId: empId },
                success: function (response) {
                    if (response.success) {
                        populateTrainingTable(response.data);
                    }
                    sessionStorage.removeItem("tabParameter");
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                }
            });
        }
    }

    function populateTrainingTable(data) {
        if (!data || data.length === 0) {
            trainingRecords = []; // Clear any existing records
            refreshTable();
            return;
        }

        console.log('4444445rtert',data)

        // Clear the existing records array
        trainingRecords = [];

        // Loop through fetched data and add it to records array
        data.forEach((record) => {
            trainingRecords.push({
                empTrainingInfoId: record.employeeTranningID || 0,
                trainingTitle: record.tranningTitle || '',
                country: record.countryID || 0,
                countryName: record.countryName || '', // Added: Store country name if available
                topicsCovered: record.topicCovered || '',
                year: record.tranningYear || 0,
                instituteName: record.instituteName || '',
                duration: record.durationYears || '',
                location: record.tranningLocation || ''
            });
        });

        // Refresh the table to display new records
        refreshTable();
    }
});





//$(document).ready(function () {



//    // Initialize an array to store training records
//    let trainingRecords = [];
//    let editIndex = -1;

//    // Function to clear form fields
//    function clearForm() {
//        $("#empTrainingInfoId").val("");
//        $("#trainingTitle").val("");
//        $("#country").val("");
//        $("#topicsCovered").val("");
//        $("#yearPassing").val("");
//        $("#instituteNameTrain").val("");
//        $("#duration").val("");
//        $("#location").val("");
//    }

//    // Function to validate form
//    function validateForm() {
//        let isValid = true;
//        const requiredFields = [
//            "#trainingTitle", "#country", "#topicsCovered",
//            "#yearPassing", "#instituteNameTrain"
//        ];

//        requiredFields.forEach(field => {
//            if ($(field).val() === "" || $(field).val() === null) {
//                $(field).addClass("is-invalid");
//                isValid = false;
//            } else {
//                $(field).removeClass("is-invalid");
//            }
//        });

//        return isValid;
//    }

//    // Function to add training record to table
//    function addTrainingRecord() {
//        if (!validateForm()) {
//            alert("Please fill all required fields");
//         //   return;
//        }

//        const record = {
//            empTrainingInfoId: parseInt($("#empTrainingInfoId").val()) || 0,
//            trainingTitle: $("#trainingTitle").val(),
//            country: parseInt($("#country").val()) || 0,
//            topicsCovered: $("#topicsCovered").val(),
//            year: parseInt($("#yearPassing").val()) || 0,
//            instituteName: $("#instituteNameTrain").val(),
//            duration: $("#duration").val(),
//            location: $("#location").val()
//        };

//        //console.log(record)

//        if (editIndex === -1) {
//            // Add new record
//            trainingRecords.push(record);
//        } else {
//            // Update existing record
//            trainingRecords[editIndex] = record;
//            editIndex = -1;
//            $("#btnAddTraining").text("Add");
//        }

//        refreshTable();
//        clearForm();
//    }

//    // Function to refresh the table with current data
//    function refreshTable() {
//        const tableBody = $("#trainingTable");
//        tableBody.empty();

//        trainingRecords.forEach((record, index) => {
//            const row = `
//                <tr>
//                    <td style="display:none">${record.empTrainingInfoId}</td>
//                    <td>${record.trainingTitle}</td>
//                    <td>${record.topicsCovered}</td>
//                    <td>${record.instituteName}</td>
//                    <td>${record.country}</td>
//                    <td>${record.location}</td>
//                    <td>${record.year}</td>
//                    <td>${record.duration}</td>
//                    <td>
//                        <button type="button" class="btn btn-sm btn-primary edit-btnt" data-index="${index}">Edit</button>
//                        <button type="button" class="btn btn-sm btn-danger delete-btnt" data-index="${index}">Delete</button>
//                    </td>
//                </tr>
//            `;
//            tableBody.append(row);
//        });

//        // Save to local storage
//       // saveToLocalStorage();
//    }

//    // Event handler for edit button
//    $(document).on("click", ".edit-btnt", function (e) {
//        e.preventDefault(); // Prevent default button behavior
//        const index = $(this).data("index");
//        editIndex = index;
//        const record = trainingRecords[index];

//        // Populate form with record data
//        $("#empTrainingInfoId").val(record.empTrainingInfoId);
//        $("#trainingTitle").val(record.trainingTitle);
//        $("#country").val(record.country);
//        $("#topicsCovered").val(record.topicsCovered);
//        $("#yearPassing").val(record.year);
//        $("#instituteNameTrain").val(record.instituteName);
//        $("#duration").val(record.duration);
//        $("#location").val(record.location);

//        // Change button text to indicate edit mode
//        $("#btnAddTraining").text("Update");

//        // Scroll back to form
//        $('html, body').animate({
//            scrollTop: $("#trainingTitle").offset().top - 100
//        }, 500);
//    });

//    // Event handler for delete button
//    $(document).on("click", ".delete-btnt", function (e) {
//        e.preventDefault(); // Prevent default button behavior
//        const index = $(this).data("index");
//        //if (confirm("Are you sure you want to delete this training record?")) {
//            trainingRecords.splice(index, 1);
//            refreshTable();
//       // }
//    });

//    // Add button click handler
//    $("#btnAddTraining").click(function (e) {
//        e.preventDefault(); // Prevent default button behavior
//        addTrainingRecord();
//    });

//    // Save button click handler
//    $("#btnSaveTraining").click(function (e) {
//        e.preventDefault(); // Prevent default button behavior
//        if (trainingRecords.length === 0) {
//            alert("Please add at least one training record");
//            return;
//        }

//        // Prepare data to send to controller
//        const trainingData = {
//            trainingRecords: trainingRecords,
//            EmpCode: $("#empIdMain4").val(),
//            EmployeeId: parseInt($("#empCodePass4").val()) || 0,
//        };
//        //console.log('2222222222233',trainingData)

//        // Send data to controller via AJAX
//        $.ajax({
//            url: "/training/save", // Replace with your actual controller endpoint
//            type: "POST",
//            contentType: "application/json",
//            data: JSON.stringify(trainingData),
//            success: function (response) {
//                //console.log(response)
//                alert("Training information saved successfully!");
//                // Optionally clear the table after successful save
//                 trainingRecords = [];
//                 refreshTable();
//            },
//            error: function (xhr, status, error) {
//                alert("Error saving training information: " + error);
//            }
//        });
//    });




//    // Initialize employee data listener
//    $("#empCodePass4").on('change', function () {
//        var empId = $(this).val();
//        getTrainFormData(empId);
//    });


//    function getTrainFormData(empId) {
//        if (empId) {
//            //console.log('Making AJAX request for empCode:', empId);
//            $.ajax({
//                url: '/EmployeeTraining/getEmpTrainingInfo',
//                type: 'GET',
//                dataType: 'json',
//                data: { empId: empId },
//                success: function (response) {
//                    //console.log('Received Employee Training Data:', response);
//                    if (response.success) {
//                        populateTrainingTable(response.data);
//                    }
//                    sessionStorage.removeItem("tabParameter");
//                },
//                error: function (xhr, status, error) {
//                    console.error('Error fetching data:', error);
//                    //console.log('XHR Response Text:', xhr.responseText);
//                }
//            });
//        } else {
//            //console.log('No empCode found in sessionStorage');
//        }
//    }

//    function populateTrainingTable(data) {
//        //console.log('Populating Training Table:', data);

//        if (!data || data.length === 0) {
//            //console.log("No education data found.");
//            trainingRecords = []; // Clear any existing records
//            refreshTable();
//            return;
//        }

//        // Clear the existing educationRecords array
//        trainingRecords = [];

//        // Loop through fetched data and add it to educationRecords array
//        data.forEach((record) => {
//            trainingRecords.push({

//                empTrainingInfoId: record.empTrainingInfoId || '',
//                trainingTitle: record.trainingTitle || '',
//                country: record.countryID || '',
//                topicsCovered: record.topicCovered || '',
//                year: record.tranningYear || '',
//                instituteName: record.instituteName || '',
//                duration: record.durationYears || '',
//                location: record.tranningLocation || ''
               
//            });
//        });

//        // Refresh the table to display new records
//        refreshTable();
//    }



//    // Store the training records in local storage to persist between page refreshes
//    //function saveToLocalStorage() {
//    //    localStorage.setItem('trainingRecords', JSON.stringify(trainingRecords));
//    //}

//    //// Load training records from local storage
//    //function loadFromLocalStorage() {
//    //    const saved = localStorage.getItem('trainingRecords');
//    //    if (saved) {
//    //        trainingRecords = JSON.parse(saved);
//    //        refreshTable();
//    //    }
//    //}

//    //// Initialize by loading any saved data
//    //loadFromLocalStorage();
//});