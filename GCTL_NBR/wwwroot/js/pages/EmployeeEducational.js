//#region Edu Page

//#region Before

// Initialize an array to store education records
let educationRecords = [];
let editIndex = -1;

// Function to clear form fields
function clearForm() {
    $("#empEducationId").val("");
    $("#levelEducation").val("none");
    $("#examDegreeTitle").val("");
    $("#major").val("");
    $("#educationBoard").val("");
    $("#instituteNameEdu").val("");
    $("#educationResult").val("");
    $("#yearPassingEdu").val("");
    $("#durationEdu").val("");
    $("#achiement").val("");
}

// Function to validate form
function validateForm() {
    let isValid = true;
    const requiredFields = [
        "#levelEducation", "#examDegreeTitle", "#major",
        "#educationBoard", "#instituteNameEdu", "#educationResult", "#yearPassingEdu"
    ];

    requiredFields.forEach(field => {
        if ($(field).val() === "" || $(field).val() === "none" || $(field).val() === null) {
            $(field).addClass("is-invalid");
            isValid = false;
        } else {
            $(field).removeClass("is-invalid");
        }
    });

    return isValid;
}

// Function to add education record to table
function addEducationRecord() {
    if (!validateForm()) {
        alert("Please fill all required fields");
        return;
    }

    const record = {

        empEducationId: $("#empEducationId").val(),
        levelEducation: $("#levelEducation").val(),
        examTitle: $("#examDegreeTitle option:selected").text(),
        examTitleValue: $("#examDegreeTitle").val(),
        major: $("#major").val(),
        educationBoard: $("#educationBoard option:selected").text(),
        educationBoardValue: $("#educationBoard").val(),
        instituteName: $("#instituteNameEdu").val(),
        result: $("#educationResult option:selected").text(),
        resultValue: $("#educationResult").val(),
        passingYear: $("#yearPassingEdu").val(),
        duration: $("#durationEdu").val(),
        achievement: $("#achiement").val()
    };

    if (editIndex === -1) {
        // Add new record
        educationRecords.push(record);
    } else {
        // Update existing record
        educationRecords[editIndex] = record;
        editIndex = -1;
        $("#btnAddEdu").text("Add");
    }

    refreshTable();
    clearForm();
}

// Function to refresh the table with current data
function refreshTable() {
    const tableBody = $("#educationTableBody");
    tableBody.empty();

    educationRecords.forEach((record, index) => {
        const row = `
            <tr data-index="${index}">
                <td style="display:none">${record.empEducationId}</td>
                <td style="display:none">${record.levelEducation}</td>
                <td>${record.examTitle}</td>
                <td>${record.major}</td>
                <td>${record.instituteName}</td>
                <td>${record.result}</td>
                <td>${record.passingYear}</td>
                <td>${record.duration}</td>
                <td>${record.achievement}</td>
                <td>
                    <button type="button" class="btn btn-sm btn-primary edit-btne">  <i class="fas fa-edit"></i> </button>
                    <button type="button" class="btn btn-sm btn-danger delete-btne">  <i class="fas fa-trash"></i> </button>

                </td>
            </tr>
        `;
        tableBody.append(row);
    });
}

//#endregion

// Document ready function to initialize everything
$(document).ready(function () {


    let choicesInstance;

    // First make sure the dropdown is visible and properly initialized
    $("#empCodePass3").show();

   



    // First, let's remove or rename the duplicate ID
    // This code will rename the hidden input to avoid the conflict
    $('input[type="hidden"][id="empCodePass3"]').attr('id', 'empCodePassHidden');

    // Now let's make sure we're working with the correct select element
    const $dropdown = $('select[id="empCodePass3"]');

    // Debug to verify we found the correct element
    //console.log('Found dropdown element:', $dropdown.length > 0, $dropdown.prop('tagName'));

    getAllemp();


    function getAllemp() {
        $.ajax({
            url: '/EmployeePersonal/GetEmployeeList',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                ////console.log('Data received:', data);
                const $dropdown = $("#empCodePass3");

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

                    ////console.log('Dropdown populated with ' + data.length + ' employees');
                } else {
                    ////console.log('No employees found or invalid data format:', data);
                }

                // Initialize Choices.js
                choicesInstance = new Choices("#empCodePass3", {
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
        ////console.log("Received Parameter on page load:", paramValue);

        if (paramValue) {
            ////console.log('Making AJAX request for empCode:', paramValue);
            $.ajax({
                url: '/EmployeePersonal/getEmpInfo',
                type: 'GET',
                dataType: 'json',
                data: { empCode: paramValue },
                success: function (response) {
                    //console.log('Received Employee Data1111:', response);

                    if (response) {
                        $("#NidPass3").val(response.nid || '');
                        $("#BirthCertiPass3").val(response.birthCertificateNo || '');
                        $("#empIdMain3").val(response.employeeCode);

                        // Set dropdown value using Choices.js API
                        if (response.employeeID && choicesInstance) {
                            choicesInstance.setChoiceByValue(response.employeeID.toString());

                            // Trigger change event to load additional data
                            $("#empCodePass3").val(response.employeeID).trigger('change');
                        }
                        sessionStorage.removeItem("tabParameter1");
                    }
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching data:', error);
                }
            });
        } else {
            ////console.log('No empCode found in sessionStorage111');
            $("#NidPass3, #BirthCertiPass3, #empCodePass3, #empIdMain3").val('');
        }
    }


    


    ////sessionStorage.setItem("tabParameter", 'EMP001');
    //var paramValue = sessionStorage.getItem("tabParameter3");
    ////console.log("Received Parameter on page load:", paramValue);
    //if (paramValue) {
    //    //console.log('Making AJAX request for empCode:', paramValue);
    //    $.ajax({
    //        url: '/EmployeePersonal/getEmpInfo',
    //        type: 'GET',
    //        dataType: 'json',
    //        data: { empCode: paramValue },
    //        success: function (response) {
    //            //console.log('Received Employee Data555:', response);
    //            // Add code to populate the fields with the response data
    //            if (response) {
    //                $("#NidPass3").val(response.nid || '');
    //                $("#BirthCertiPass3").val(response.birthCertificateNo || '');
    //                $("#empCodePass3").val(response.employeeID);
    //                $("#empIdMain3").val(response.employeeCode);


    //                // Set the dropdown to the current employee if applicable
    //                //if (response.employeeID) {

    //                //}
    //            } else {

    //            }

    //            sessionStorage.removeItem("tabParameter");
    //        },
    //        error: function (xhr, status, error) {
    //            console.error('Error fetching data:', error);
    //            //console.log('XHR Response Text:', xhr.responseText);
    //        }
    //    });
    //} else {
    //    //console.log('No empCode found in sessionStorage');

    //    $("#NidPass3").val('');
    //    $("#BirthCertiPass3").val('');
    //    $("#empCodePass3").val('');
    //    $("#empIdMain3").val('');
    //}

    // Add change event handler for the dropdown


    $("#empCodePass3").on('change', function () {
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
                    $("#NidPass3").val(response.nid || '');
                    $("#BirthCertiPass3").val(response.birthCertificateNo || '');
                    $("#empIdMain3").val(response.employeeCode);

                },
                error: function (xhr, status, error) {
                    console.error('Error fetching employee details:', error);
                }
            });
        } else {
            // Clear fields if "Select Employee" is chosen
            $("#NidPass3").val('');
            $("#BirthCertiPass3").val('');
        }
    });




   



    $("#empCodePass3").show();


    //function getAllemp() {
    //    $.ajax({
    //        url: '/EmployeePersonal/GetEmployeeList',
    //        type: 'GET',
    //        dataType: 'json',
    //        success: function (data) {
    //            //console.log('Data received:', data);
    //            const $dropdown = $("#empCodePass3");

    //            // Destroy previous Choices instance if it exists
    //            if (choicesInstance) {
    //                choicesInstance.destroy();
    //            }

    //            // Clear and populate dropdown
    //            $dropdown.empty().append(`<option value="" selected >Select Employee</option>`);

    //            if (Array.isArray(data) && data.length > 0) {
    //                data.forEach(employee => {
    //                    $dropdown.append(`
    //                            <option value="${employee.employeeID}">
    //                                ${employee.fullName} (${employee.employeeCode})
    //                            </option>
    //                        `);
    //                });

    //                //console.log('Dropdown populated with ' + data.length + ' employees');
    //            } else {
    //                //console.log('No employees found or invalid data format:', data);
    //            }

    //            // Initialize Choices.js with safe settings
    //            choicesInstance = new Choices("#empCodePass3", {
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
    //            const $dropdown = $("#empCodePass3");
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




    // Add button click handler


    $("#btnAddEdu").click(function (e) {
        e.preventDefault(); // Prevent default button behavior
        addEducationRecord();
    });

    // Save button click handler
    $("#btnSaveEdu").click(function (e) {
        e.preventDefault(); // Prevent default button behavior
        if (educationRecords.length === 0) {
            alert("Please add at least one education record");
            return;
        }

        // Prepare data to send to controller
        const educationData = {
            educationRecords: educationRecords,
            EmpEducationId: parseInt($("#empEducationId").val()) || 0,
            EmpCode: $("#empIdMain3").val(),
            EmployeeId: parseInt($("#empCodePass3").val()) || 0,
        };


        //console.log('2222', educationData)

        // Send data to controller via AJAX
        $.ajax({
            url: "/EmployeeEducational/SaveEducationDetails",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(educationData),
            success: function (response) {
                //console.log(response);
                //alert("Education information saved successfully!");
                // Clear the records after successful save
                educationRecords = [];
                refreshTable();

                //debugger
                var paramValue = response.data;


                sessionStorage.removeItem("tabParameter");
                sessionStorage.setItem("tabParameter", paramValue);
                sessionStorage.setItem("tabParameter4", paramValue);



                // Get current tab dynamically inside the event
                var currentTab = $("#btnSaveEdu").closest(".tab-pane"); // Get current tab pane
                var currentTabId = currentTab.attr("id"); // Get current tab ID

                // Move to the next tab
                var $currentTabLink = $('a[href="#' + currentTabId + '"]'); // Get current tab link
                var $nextTabLink = $currentTabLink.parent().next().find(".nav-link"); // Get next tab link

                if ($nextTabLink.length) {
                    $nextTabLink.tab("show"); // Show next tab
                } else {
                    window.location.href = "/employeetraining/index";
                }




            },
            error: function (xhr, status, error) {
                alert("Error saving education information: " + error);
            }
        });
    });

    // Using event delegation for edit and delete buttons
    // This attaches the handlers only once to the document
    $(document).on("click", ".edit-btne", function (e) {
        e.preventDefault();
        const index = $(this).closest('tr').data("index");
        editIndex = index;
        const record = educationRecords[index];

        // Populate form with record data
        $("#empEducationId").val(record.empEducationId);
        $("#levelEducation").val(record.levelEducation);
        $("#examDegreeTitle").val(record.examTitleValue);
        $("#major").val(record.major);
        $("#educationBoard").val(record.educationBoardValue);
        $("#instituteNameEdu").val(record.instituteName);
        $("#educationResult").val(record.result);
        $("#yearPassingEdu").val(record.passingYear);
        $("#durationEdu").val(record.duration);
        $("#achiement").val(record.achievement);

        // Change button text to indicate edit mode
        $("#btnAddEdu").text("Update");

        // Scroll back to form
        $('html, body').animate({
            scrollTop: $("#levelEducation").offset().top - 100
        }, 500);
    });

    $(document).on("click", ".delete-btne", function (e) {
        e.preventDefault();
        const index = $(this).closest('tr').data("index");

        // If we're currently editing this record, cancel the edit
        if (editIndex === index) {
            editIndex = -1;
            $("#btnAddEdu").text("Add");
            clearForm();
        }

        educationRecords.splice(index, 1);
        refreshTable();
    });

    // Initialize employee data listener
    $("#empCodePass3").on('change', function () {
        var empId = $(this).val();
        clearForm();
        educationRecords = [];
        getEduFormData(empId);
    });



    loadEducationLevels();
    loadExamDegreeTitles();
    loadEducationBoards();
    loadEducationResults();
    loadYearsOfPassing();


});

function getEduFormData(empId) {
    if (empId) {
        //console.log('Making AJAX request for empCode:', empId);
        $.ajax({
            url: '/EmployeeEducational/getEmpEducationInfo',
            type: 'GET',
            dataType: 'json',
            data: { empId: empId },
            success: function (response) {
                //console.log('Received Employee official Data:', response);
                if (response.success) {
                    populateEducationalTable(response.data);
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

function populateEducationalTable(data) {
    console.log('Populating Education Table:', data);

    if (!data || data.length === 0) {
        //console.log("No education data found.");
        educationRecords = []; // Clear any existing records
        refreshTable();
        return;
    }

    // Clear the existing educationRecords array
    educationRecords = [];

    // Loop through fetched data and add it to educationRecords array
    data.forEach((record) => {
        educationRecords.push({
            empEducationId: record.employeeEducationID || '',
            levelEducation: record.educationLevelID,
            examTitle: record.degreeName || '',
            examTitleValue: record.degreeID || '',
            major: record.majorGroup || '',
            educationBoard: record.educationBoardName || '',
            educationBoardValue: record.educationBoardID || '',
            instituteName: record.instituteName || '',
            result: record.result || '',
            resultValue: record.resultValue || '',
            passingYear: record.yearOfPassing || '',
            duration: record.durationYears || '',
            achievement: record.achievement || ''
        });
    });

    // Refresh the table to display new records
    refreshTable();
}

//#endregion Edu Page




//#region Load all dropdowns when page loads


    // Function to fetch and populate education levels
    function loadEducationLevels() {
        $.ajax({
            url: '/common/educationLevels',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var select = $('#levelEducation');
                select.empty();
                select.append('<option value="none" selected disabled>Select Education Level</option>');
                $.each(data, function (index, item) {
                    select.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading education levels:', error);
            }
        });
    }

    // Function to fetch and populate exam/degree titles
    function loadExamDegreeTitles() {
        $.ajax({
            url: '/common/examDegreeTitles',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var select = $('#examDegreeTitle');
                select.empty();
                select.append('<option value="" selected disabled>Select your exam/degree title</option>');
                $.each(data, function (index, item) {
                    select.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading exam/degree titles:', error);
            }
        });
    }

    // Function to fetch and populate education boards
    function loadEducationBoards() {
        $.ajax({
            url: '/common/educationBoards',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var select = $('#educationBoard');
                select.empty();
                select.append('<option value="" selected disabled>Select your education board</option>');
                $.each(data, function (index, item) {
                    select.append('<option value="' + item.id + '">' + item.name + '</option>');
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading education boards:', error);
            }
        });
    }

    // Function to fetch and populate education results/statuses
    function loadEducationResults() {
        $.ajax({
            url: '/common/educationStatuses',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                var select = $('#educationResult');
                select.empty();
                select.append('<option value="" selected disabled>Select your result</option>');
                $.each(data, function (index, item) {
                    select.append('<option value="' + item.value + '">' + item.text + '</option>');
                });
            },
            error: function (xhr, status, error) {
                console.error('Error loading education results:', error);
            }
        });
    }

    // Function to populate year of passing dropdown
    function loadYearsOfPassing() {
        var select = $('#yearPassingEdu');
        select.empty();
        select.append('<option value="" selected disabled>Select your year of passing</option>');

        var currentYear = new Date().getFullYear();
        for (var year = currentYear; year >= currentYear - 50; year--) {
            select.append('<option value="' + year + '">' + year + '</option>');
        }
    }

//#endregion





    //#region Backup


// Store the education records in local storage to persist between page refreshes
//function saveToLocalStorage() {
//    localStorage.setItem('educationRecords', JSON.stringify(educationRecords));
//}

//// Load education records from local storage
//function loadFromLocalStorage() {
//    const saved = localStorage.getItem('educationRecords');
//    if (saved) {
//        educationRecords = JSON.parse(saved);
//        refreshTable();
//    }
//}

//// Save to local storage when records change
//$("#btnAdd, #btnSave").click(saveToLocalStorage);
//$(document).on("click", ".delete-btn", saveToLocalStorage);

//// Initialize by loading any saved data
//loadFromLocalStorage();

//// Initialize table
//refreshTable();




//$(document).ready(function () {
//    let educationData = [];

//    // Function to populate form fields when editing
//    function populateForm(data) {
//        $('#levelEducation').val(data.levelEducation);
//        $('#examDegreeTitle').val(data.examDegreeTitle);
//        $('#major').val(data.major);
//        $('#educationBoard').val(data.educationBoard);
//        $('#instituteNameEdu').val(data.instituteName);
//        $('#educationResult').val(data.educationResult);
//        $('#yearPassingEdu').val(data.yearPassing);
//        $('#durationEdu').val(data.duration);
//        $('#achiement').val(data.achievement);
//        $('#btnAdd').data('editIndex', data.index); // Store the index for editing
//        $('#btnAdd').text('Update');
//    }

//    // Function to clear form fields
//    function clearForm() {
//        $('#levelEducation, #examDegreeTitle, #major, #educationBoard, #instituteNameEdu, #educationResult, #yearPassingEdu, #durationEdu, #achiement').val('');
//        $('#btnAdd').removeData('editIndex');
//        $('#btnAdd').text('Add');
//    }

//    // Function to render table
//    function renderTable() {
//        $('#educationTableBody').empty();
//        $.each(educationData, function (index, item) {
//            $('#educationTableBody').append(`
//                <tr>
//                    <td>${item.examDegreeTitle}</td>
//                    <td>${item.major}</td>
//                    <td>${item.instituteName}</td>
//                    <td>${item.educationResult}</td>
//                    <td>${item.yearPassing}</td>
//                    <td>${item.duration}</td>
//                    <td>${item.achievement}</td>
//                    <td>
//                        <button id="btnTabEdit" class="btn btn-sm btn-warning btnEdit" data-index="${index}">Edit</button>
//                        <button id="btnTabDel" class="btn btn-sm btn-danger btnDelete" data-index="${index}">Delete</button>
//                    </td>
//                </tr>
//            `);
//        });
//    }

//    // Add or Update button click event
//    $('#btnAdd').click(function () {
//        let educationEntry = {
//            levelEducation: $('#levelEducation').val(),
//            examDegreeTitle: $('#examDegreeTitle').val(),
//            major: $('#major').val(),
//            educationBoard: $('#educationBoard').val(),
//            instituteName: $('#instituteNameEdu').val(),
//            educationResult: $('#educationResult').val(),
//            yearPassing: $('#yearPassingEdu').val(),
//            duration: $('#durationEdu').val(),
//            achievement: $('#achiement').val(),
//        };

//        let editIndex = $(this).data('editIndex');
//        if (editIndex !== undefined) {
//            // Update existing entry
//            educationData[editIndex] = educationEntry;
//        } else {
//            // Add new entry
//            educationData.push(educationEntry);
//        }

//        renderTable();
//        clearForm();
//    });


//    $('#btnTabEdit').click(function () {
//        let index = $(this).data('index');
//        let data = educationData[index];
//        data.index = index; // Store index for editing
//        populateForm(data);
//    })

//    $('#btnTabDel').click(function () {
//        let index = $(this).data('index');
//        let data = educationData[index];
//        data.index = index; // Store index for editing
//        populateForm(data);
//    })

//    //// Edit button click event
//    //$(document).on('click', '.btnEdit', function () {
//    //    let index = $(this).data('index');
//    //    educationData.splice(index, 1);
//    //    renderTable();
//    //});

//    //// Delete button click event
//    //$(document).on('click', '.btnDelete', function () {
//    //    let index = $(this).data('index');
//    //    educationData.splice(index, 1);
//    //    renderTable();
//    //});

//    // Save table data and send to backend
//    $("#btnSave").click(function () {

//        //$("#educationTableBody tr").each(function () {
//        //    var row = $(this).find("td");
//        //    var rowData = {
//        //        examTitle: row.eq(0).text(),
//        //        major: row.eq(1).text(),
//        //        institute: row.eq(2).text(),
//        //        result: row.eq(3).text(),
//        //        passYear: row.eq(4).text(),
//        //        duration: row.eq(5).text(),
//        //        achievement: row.eq(6).text()
//        //    };
//        //    educationData.push(rowData);
//        //});

//        //if (educationData.length === 0) {
//        //    alert("Please add at least one educational record.");
//        //    return;
//        //}

//        // AJAX request to send data to the backend
//        $.ajax({
//            url: "/EmployeeEducational/SaveEducationDetails",
//            type: "POST",
//            contentType: "application/json",
//            data: JSON.stringify({ educationRecords: educationData }),
//            success: function (response) {
//                //console.log(response)
//                if (response) {
//                    alert("Education details saved successfully!");
//                }
//            },
//            error: function (error) {
//                console.error("Error saving data:", error);
//                alert("Failed to save data.");
//            }
//        });
//    });

//});





//$(document).ready(function () {
//    let editingRow = null; // Store the row being edited

//    $("#btnAdd").click(function () {
//        // Get input values
//        var examTitle = $("#exam-degree-title").val();
//        var major = $("input[placeholder='Concentration/ Major/Group']").val();
//        var institute = $("input[placeholder='Institute Name']").val();
//        var result = $("#education-result").val();
//        var passYear = $("#year-of-passing").val();
//        var duration = $("input[placeholder='Duration']").val();
//        var achievement = $("input[placeholder='Achievement']").val();

//        // Validate required fields
//        if (!examTitle || !major || !institute || !result || !passYear) {
//            alert("Please fill all required fields.");
//            return;
//        }

//        if (editingRow) {
//            // Update existing row
//            $(editingRow).html(`
//                <td>${examTitle}</td>
//                <td>${major}</td>
//                <td>${institute}</td>
//                <td>${result}</td>
//                <td>${passYear}</td>
//                <td>${duration}</td>
//                <td>${achievement}</td>
//                <td>
//                    <button class="btn btn-warning btn-sm edit-row">Edit</button>
//                    <button class="btn btn-danger btn-sm remove-row">Remove</button>
//                </td>
//            `);

//            editingRow = null; // Reset editing row
//            $("#btnAdd").text("Add"); // Change button text back to Add
//        } else {
//            // Append new row to table
//            var newRow = `
//                <tr>
//                    <td>${examTitle}</td>
//                    <td>${major}</td>
//                    <td>${institute}</td>
//                    <td>${result}</td>
//                    <td>${passYear}</td>
//                    <td>${duration}</td>
//                    <td>${achievement}</td>
//                    <td>
//                        <button class="btn btn-warning btn-sm edit-row">Edit</button>
//                        <button class="btn btn-danger btn-sm remove-row">Remove</button>
//                    </td>
//                </tr>`;
//            $("#educationTableBody").append(newRow);
//        }

//        // Reset form fields
//        $("#educationInfo input, #educationInfo select").val("");
//    });

//    // Remove row from table
//    $(document).on("click", ".remove-row", function () {
//        $(this).closest("tr").remove();
//    });

//    // Edit row in table
//    $(document).on("click", ".edit-row", function () {
//        editingRow = $(this).closest("tr"); // Store reference to the row

//        // Populate form with row data
//      //  $("#levelEducation").val(editingRow.find("td:eq(0)").text());
//        $("#exam-degree-title").val(editingRow.find("td:eq(0)").text());
//        $("input[placeholder='Concentration/ Major/Group']").val(editingRow.find("td:eq(1)").text());
//        $("input[placeholder='Institute Name']").val(editingRow.find("td:eq(2)").text());
//        $("#education-result").val(editingRow.find("td:eq(3)").text());
//        $("#year-of-passing").val(editingRow.find("td:eq(4)").text());
//        $("input[placeholder='Duration']").val(editingRow.find("td:eq(5)").text());
//        $("input[placeholder='Achievement']").val(editingRow.find("td:eq(6)").text());

//        $("#btnAdd").text("Update"); // Change button text to Update
//    });

//    // Save table data and send to backend
//    $("#btnSave").click(function () {
//        var educationData = [];
//        $("#educationTableBody tr").each(function () {
//            var row = $(this).find("td");
//            var rowData = {
//                examTitle: row.eq(0).text(),
//                major: row.eq(1).text(),
//                institute: row.eq(2).text(),
//                result: row.eq(3).text(),
//                passYear: row.eq(4).text(),
//                duration: row.eq(5).text(),
//                achievement: row.eq(6).text()
//            };
//            educationData.push(rowData);
//        });

//        if (educationData.length === 0) {
//            alert("Please add at least one educational record.");
//            return;
//        }

//        // AJAX request to send data to the backend
//        $.ajax({
//            url: "/EmployeeEducational/SaveEducationDetails",
//            type: "POST",
//            contentType: "application/json",
//            data: JSON.stringify({ educationRecords: educationData }),
//            success: function (response) {
//                //console.log(response)
//                if (response) {
//                    alert("Education details saved successfully!");
//                }
//            },
//            error: function (error) {
//                console.error("Error saving data:", error);
//                alert("Failed to save data.");
//            }
//        });
//    });
//});


//#endregion