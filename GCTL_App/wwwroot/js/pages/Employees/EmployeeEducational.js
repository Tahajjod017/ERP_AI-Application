$(document).ready(function () {

    //#region employeeChoices with onchange

    let employeeChoices;
    function initEmployeeChoices() {
        employeeChoices = new Choices('#EmployeePersonalId', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Employee'
        });

        const employeeElement = document.getElementById('EmployeePersonalId');
        if (employeeElement) {
            employeeElement.addEventListener('change', function (e) {
                const selectedEmployeeId = e.detail.value || e.target.value;
                if (selectedEmployeeId && selectedEmployeeId !== '') {
                    loadEmployeeEducationalData(selectedEmployeeId);
                    TabChange(selectedEmployeeId) // this function is located in EmployeeTabChange.js
                   
                } else {
                     clearForm();
                }
            });
        }
    }
    document.addEventListener('DOMContentLoaded', initEmployeeChoices);
    initEmployeeChoices();

    //#endregion

    //#region Load Data

    function loadEmployeeEducationalData(selectedEmployeeId) {
    
        if (!selectedEmployeeId || selectedEmployeeId === 0) {
            selectedEmployeeId = $('#EmployeePersonalId').val();
        }

        
        $.ajax({
            url: '/EmployeeEducation/GetEmployeeData', // Adjust the URL to your API endpoint
            type: 'GET',
            data: { id: selectedEmployeeId },
            success: function (data) {
                
               
                var employee = Array.isArray(data) ? data[0] : data;
              
                $('#PersonalEmail').val(employee.personalEmail);
                $('#PersonalPhone').val(employee.personalPhone);
                choiceManager.setChoiceValue('EmployeePersonalId', employee.employeePersonalId)

                const tableBody = $('#educationalTable tbody');
                tableBody.empty();

                if (employee.isActive) {
                    PopulateTable(data);
                }


            },
            error: function (xhr, status, error) {
                console.error('Error fetching employee educational data:', error);
            }
        });
    }
    //#endregion

    let deleteId = null;

    //#region Populate Table
    function PopulateTable(data) {
        console.log('data for table ', data);
        const tableBody = $('#educationalTable tbody');
        tableBody.empty();
        
        if (data && data.length > 0) {
            data.forEach(function (item) {
                const row = `<tr data-id="${item.employeeEducationalInfoID}">
                <td>${item.degreeID || ''}</td>
                <td>${item.majorSubject || ''}</td>
                <td>${item.institutionName || ''}</td>
                <td>${item.resultTypeID || ''}</td>
                <td>${item.passingYearID || ''}</td>
                <td>${item.yearDuration || ''}</td>
                <td>${item.achievement || ''}</td>
                <td class="align-middle white-space-nowrap ">
                    <div class="btn-reveal-trigger position-static g-3">
                        <button class="nav-item me-2 btn-edit" data-id="${item.employeeEducationalInfoID}" data-bs-toggle="modal" data-bs-target="#edit_education">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="nav-item me-2 btn-delete" data-id="${item.employeeEducationalInfoID}" data-bs-toggle="modal" data-bs-target="#delete_modal">
                            <i class="fas fa-trash"></i>
                        </button>
                        
                    </div>
                </td>

                

            </tr>`;
                tableBody.append(row);
            });

            // Add click handlers for edit buttons
            $('.btn-edit').click(function (e) {
                e.preventDefault();
                const id = $(this).data('id');
                EditEducationalRecord(id);
            });

            
         


           

            $('.btn-delete').click(function (e) {
                e.preventDefault()
                deleteId = $(this).data('id'); // Store the id temporarily
            });



        } else {
            tableBody.append('<tr><td colspan="8">No educational records found.</td></tr>');
        }
    }


    //#endregion


    //#region Delete Button

    $('#confirmDeleteBtn').click(function (e) {
        e.preventDefault()
        if (deleteId) {
            debugger
            DeleteEducationalRecord(deleteId);
        }
    });


    //#endregion
   

    //#region Edit record function
    function EditEducationalRecord(id) {


        $.ajax({
            url: '/EmployeeEducation/GetEmployeeEduData',
            type: 'GET',
            data: { id: id },
            success: function (data) {
                console.log("Received Data:", data); // Log data before populating
                populateOnform(data);
            },
            error: function (xhr, status, error) {
                console.error('XHR Status:', status);
                console.error('XHR Response:', xhr.responseText); // Log detailed response
                console.error('Error Details:', error);
            }
        });
        

        
    }

    //#endregion

    //#region Populate from on edit

    function populateOnform(record) {
       
        // Populate the form with the record data
        $('#editEmployeeEducationalInfoID').val(record.employeeEducationalInfoID);
        $('#editInstitutionName').val(record.institutionName);
        $('#editYearDuration').val(record.yearDuration);
        $('#editAchievement').val(record.achievement);
        $('#editMajorSubject').val(record.majorSubject);


        choiceManager.setChoiceValue('editEducationLevelID', record.educationLevelID)
        choiceManager.setChoiceValue('editDegreeID', record.degreeID)
        choiceManager.setChoiceValue('editEducationBoardID', record.educationBoardID)
        choiceManager.setChoiceValue('editResultTypeID', record.resultTypeID)
        choiceManager.setChoiceValue('editPassingYearID', record.passingYearID)
              
    }

    //#endregion

    //#region Edit Button Click

    $(document).on('click', '#btnEditSubmit', function (e) {
        e.preventDefault();
        let formData = {
            employeeEducationalInfoID: $('#editEmployeeEducationalInfoID').val(),
            majorSubject: $('#editMajorSubject').val(),
            institutionName: $('#editInstitutionName').val(),
            yearDuration: $('#editYearDuration').val(),
            achievement: $('#editAchievement').val(),

            //educationLevelID: $('#editEducationLevelID').val(),
            //degreeID: $('#editDegreeID').val(),
            //educationBoardID: $('#editEducationBoardID').val(),
            //resultTypeID: $('#editResultTypeID').val(),
            //passingYearID: $('#editPassingYearID').val(),

            educationLevelID: choiceManager.getChoiceValue('editEducationLevelID'),
            degreeID: choiceManager.getChoiceValue('editDegreeID'),
            educationBoardID: choiceManager.getChoiceValue('editEducationBoardID'),
            resultTypeID: choiceManager.getChoiceValue('editResultTypeID'),
            passingYearID: choiceManager.getChoiceValue('editPassingYearID'),

        };

        $.ajax({
            type: "POST",
            url: "/EmployeeEducation/Update", 
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    $('#edit_education').modal('hide'); 
                    toastr.success(response.message || "Education info updated successfully!");

                    loadEmployeeEducationalData();

                } else {
                    toastr.warning(response.message || "Failed to update education info. Try again!");
                }
            },
            error: function () {
                toastr.error("Error updating education info.");
            }
        });
    });

    //#endregion

    //#region Delete record function
    function DeleteEducationalRecord(id) {
        
            // AJAX call to delete the record
            $.ajax({
                url: '/EmployeeEducation/Delete',
                type: 'POST',
                data: { id: id },
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message || ' Record  deleted');

                        $('#delete_modal').modal('hide');

                        loadEmployeeEducationalData()

                    } else {
                        toastr.error(response.message || 'Failed to delete record');

                        $('#delete_modal').modal('hide');

                    }
                },
                error: function () {
                    toastr.error('An error occurred while deleting the record');
                }
            });
        
    }

    //#endregion

    //#region cleasr

    function clearForm() {
        
       
        $('#EmployeeEducationalInfoID').val('');
       
        $('#MajorSubject').val('');
        $('#InstitutionName').val('');
        $('#YearDuration').val('');
        $('#Achievement').val('');

        choiceManager.clearChoice('EducationLevelID', 'DegreeID', 'EducationBoardID', 'ResultTypeID')
        choiceManager.clearChoice('PassingYearID')

    }

    //#endregion


    //#region Form Submission

    

    $('form').on('submit', function (e) {
        e.preventDefault();

        const fields = ["EmployeePersonalId" ];

        if (!validateFields(fields)) {
            return;
        }


        var form = $(this);
        var formData = new FormData(form[0]);

        $.ajax({
            url: form.attr('action'),
            type: form.attr('method'),
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Employee Education saved successfully.');
                    loadEmployeeEducationalData(response.data)
                    clearForm()
                } else {
                    toastr.warning(response.message);
                    console.error(response.message);
                }
            },
            error: function (xhr) {
                if (xhr.status === 400 && xhr.responseJSON) {
                    var errors = xhr.responseJSON;
                    for (var field in errors) {
                        if (errors.hasOwnProperty(field)) {
                            toastr.error(errors[field][0]);
                        }
                    }
                } else {
                    toastr.error('Failed to save employee allowance. Please try again.');
                }
            }
        });
    });

    //#endregion
    


});