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
                    loadEmployeeTrainingData(selectedEmployeeId);

                    


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

    function loadEmployeeTrainingData(selectedEmployeeId) {
        
        if (!selectedEmployeeId || selectedEmployeeId === 0) {
            selectedEmployeeId = $('#EmployeePersonalId').val();
        }


        $.ajax({
            url: '/EmployeeTraining/GetEmployeeData', // Adjust the URL to your API endpoint
            type: 'GET',
            data: { id: selectedEmployeeId },
            success: function (data) {
                

                var employee = Array.isArray(data) ? data[0] : data;

                $('#PersonalEmail').val(employee.personalEmail);
                $('#PersonalPhone').val(employee.personalPhone);
                choiceManager.setChoiceValue('EmployeePersonalId', employee.employeePersonalId)

                const tableBody = $('#employeeTrainingTable tbody');
                tableBody.empty();

                if (employee.isActive) {
                    PopulateTable(data);
                }

            },
            error: function (xhr, status, error) {
                console.error('Error fetching employee training data:', error);
            }
        });
    }
    //#endregion

    let deleteId = null;

    //#region Populate Table
    function PopulateTable(data) {
        console.log('data for table ', data);
        const tableBody = $('#employeeTrainingTable tbody');
        tableBody.empty();
        
        if (data && data.length > 0) {
            data.forEach(function (item) {
                const row = `<tr data-id="${item.employeeTranningInfoID}">
                <td>${item.tranningTitle || ''}</td>
                <td>${item.topicCovered || ''}</td>
                <td>${item.instituteName || ''}</td>
                <td>${item.countryID || ''}</td>
                <td>${item.locationName || ''}</td>
                <td>${item.trainingYearID || ''}</td>
                <td>${item.yearDuration || ''}</td>
                <td class="align-middle white-space-nowrap ">
                    <div class="btn-reveal-trigger position-static g-3">
                        <button class="nav-item me-2 btn-edit" data-id="${item.employeeTranningInfoID}" data-bs-toggle="modal" data-bs-target="#edit_traning">
                            <i class="fas fa-edit"></i>
                        </button>
                        <button class="nav-item me-2 btn-delete" data-id="${item.employeeTranningInfoID}" data-bs-toggle="modal" data-bs-target="#delete_modal">
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
                EditTrainingRecord(id);
            });

           


            $('.btn-delete').click(function (e) {
                e.preventDefault()
                deleteId = $(this).data('id'); // Store the id temporarily
            });


        } else {
            tableBody.append('<tr><td colspan="8">No training records found.</td></tr>');
        }
    }

    //#endregion

    //#region Delete Model

    $('#confirmDeleteBtn').click(function (e) {
        e.preventDefault()
        if (deleteId) {
            
            DeleteTrainingRecord(deleteId);





        }
    });

    //#endregion

    //#region Edit record function
    function EditTrainingRecord(id) {

        $.ajax({
            url: '/EmployeeTraining/GetEmployeeTrainingData',
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
        $('#editEmployeeTranningInfoID').val(record.employeeTranningInfoID);
        $('#editTranningTitle').val(record.tranningTitle);
        $('#editTopicCovered').val(record.topicCovered);
        $('#editInstituteName').val(record.instituteName);
        $('#editYearDuration').val(record.yearDuration);
        $('#editLocationName').val(record.locationName);

        choiceManager.setChoiceValue('editCountryID', record.countryID)
        choiceManager.setChoiceValue('editTrainingYearID', record.trainingYearID)

        
    }

    //#endregion

    //#region Delete record function
    function DeleteTrainingRecord(id) {
       
        $.ajax({
            url: '/EmployeeTraining/Delete',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Training record deleted');

                    loadEmployeeTrainingData()

                } else {
                    toastr.error(response.message || 'Failed to delete training record');
                }
            },
            error: function () {
                toastr.error('An error occurred while deleting the training record');
            }
        });
        
    }

    //#endregion

    //#region clear form

    function clearForm() {

        $('#EmployeeTranningInfoID').val('');
        $('#TranningTitle').val('');
        $('#TopicCovered').val('');
        $('#InstituteName').val('');
        $('#YearDuration').val('');
        $('#LocationName').val('');

        choiceManager.clearChoice('CountryID', 'TrainingYearID')

    }

    //#endregion

    //#region Form Submission

    $('form').on('submit', function (e) {
        e.preventDefault();

        const fields = ["EmployeePersonalId"];

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
                    toastr.success(response.message || 'Employee Training saved successfully.');
                    loadEmployeeTrainingData(response.data)
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
                    toastr.error('Failed to save employee training. Please try again.');
                }
            }
        });
    });

    //#endregion

    //#region Edit Button Click

    $(document).on('click', '#btnEditSubmit', function (e) {
        e.preventDefault();
        let formData = {
            employeeTranningInfoID: $('#editEmployeeTranningInfoID').val(),
            tranningTitle: $('#editTranningTitle').val(),
            topicCovered: $('#editTopicCovered').val(),
            instituteName: $('#editInstituteName').val(),
            yearDuration: $('#editYearDuration').val(),
            locationName: $('#editLocationName').val(),

            //countryID: $('#editCountryID').val(),
            //trainingYearID: $('#editTrainingYearID').val(),

            countryID: choiceManager.getChoiceValue('editCountryID'),
            trainingYearID: choiceManager.getChoiceValue('editTrainingYearID')
        }


        $.ajax({
            type: "POST",
            url: "/EmployeeTraining/Update",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    $('#edit_traning').modal('hide');
                    toastr.success(response.message || "Employee Training info updated successfully!");
                    loadEmployeeTrainingData()
                } else {
                    toastr.warning(response.message || "Failed to update Employee Training info. Try again!");
                }
            },
            error: function () {
                toastr.error("Error updating Employee Training info.");
            }
        });
    });

    //#endregion

});