$(document).ready(function () {


    //#region employeeChoices with onchange




    var initData = $('#initEmp').val();

    if (initData && initData !== '') {
        setTimeout(function () {
            loadAndSelectEmployee(initData);
        }, 300);
    }

    paginationService.init('EmployeePersonalId', {
        apiUrl: '/EmployeePersonal/SearchEmployees',
        pageSize: 50,
        minSearchLength: 2,
        loadInitial: true,
        placeholder: 'Select Employee',
        searchPlaceholder: 'Type to search...'
    });





    async function loadAndSelectEmployee(employeeId) {
        try {
            // Fetch the specific employee data from server
            const response = await fetch(`/EmployeePersonal/GetEmployeeByIdCC?id=${employeeId}`);
            const employee = await response.json();

            if (employee && employee.value) {
                // Get the Choices instance
                const instance = paginationService.activeInstances['EmployeePersonalId'];

                if (instance && instance.choices) {
                    // Add this specific employee to choices first
                    instance.choices.setChoices([{
                        value: employee.value,
                        label: employee.label,
                        selected: true
                    }], 'value', 'label', false);

                    // Set the value
                    instance.choices.setChoiceByValue(employee.value);

                    // Update the underlying select
                    $('#EmployeePersonalId').val(employee.value).trigger('change');

                    console.log('Employee preselected:', employee.label);
                }
            }
        } catch (error) {
            console.error('Error loading preselected employee:', error);
        }
    }




    $('#EmployeePersonalId').on('change', function (e) {
        const selectedEmployeeId = e.target.value;
        showDev(selectedEmployeeId, 'Selected Employee ID:');
       
        if (selectedEmployeeId) {
            loadEmployeeFamilyData(selectedEmployeeId);
           // TabChange(selectedEmployeeId);
        } else {
            clearForm();
        }
    });

    //#endregion


    //#region Get Last Int from URL

    function getLastIntFromUrl() {
        const parts = window.location.pathname.split('/').filter(Boolean).reverse();
        return parts.find(part => !isNaN(part) && Number.isInteger(Number(part)));
    }

    const lastInt = getLastIntFromUrl();
    showDev(lastInt, 'Last int:');

    if (lastInt) {
        loadEmployeeFamilyData(lastInt);
        // paginationService.setValue('EmployeePersonalId', lastInt);



        setTimeout(function () {
            loadAndSelectEmployee(lastInt);
        }, 300);



        //TabChange(lastInt);
    }

    //#endregion





    //#region Load Data

    function loadEmployeeFamilyData(selectedEmployeeId) {

        if (!selectedEmployeeId || selectedEmployeeId === 0) {
            selectedEmployeeId = $('#EmployeePersonalId').val();
        }

        $.ajax({
            url: '/EmployeeFamily/GetEmployeeData', // Adjust the URL to your API endpoint
            type: 'GET',
            data: { id: selectedEmployeeId },
            success: function (data) {
                var employee = Array.isArray(data) ? data[0] : data;

                $('#PersonalEmail').val(employee.personalEmail);
                $('#PersonalPhone').val(employee.personalPhone);
               // choiceManager.setChoiceValue('EmployeePersonalId', employee.employeePersonalId)

                const tableBody = $('#employeeFamilyTable tbody');
                tableBody.empty();

                if (employee.isActive) {
                    PopulateTable(data);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching employee family data:', error);
            }
        });
    }
    //#endregion

    let deleteId = null;

    //#region Populate Table
    function PopulateTable(data) {

        try {
            console.log('data for table ', data);
            const tableBody = $('#employeeFamilyTable tbody');
            tableBody.empty();

            if (data && data.length > 0) {


                data.forEach(function (item) {
                   
                    const row = `<tr data-id="${item.employeeFamilyInfoID}">
                <td>${item.fullName || ''}</td>
                <td>${item.relationToEmployee || ''}</td>
                <td>${item.occupation || ''}</td>
                <td>${item.contactNumber || ''}</td>
                <td>${item.email || ''}</td>
                <td>${item.address || ''}</td>
                <td class="white-space-nowrap align-middle ps-0">
                    <div class="btn-reveal-trigger position-static g-3">
                        <a class="nav-item mx-2 btn-edit" data-id="${item.employeeFamilyInfoID}" data-bs-toggle="modal" data-bs-target="#edit_family">
                            <i class="fas fa-edit text-black"></i>
                        </a>
                        <a class="nav-item me-2 btn-delete" data-id="${item.employeeFamilyInfoID}" data-bs-toggle="modal" data-bs-target="#delete_modal">
                            <i class="far fa-trash-alt text-black"></i>
                        </a>
                    </div>
                </td>
            </tr>`;
                    tableBody.append(row);
                });

                // Add click handlers for edit buttons
                $('.btn-edit').click(function (e) {
                    e.preventDefault();
                    const id = $(this).data('id');
                    EditFamilyRecord(id);
                });

                $('.btn-delete').click(function (e) {
                    e.preventDefault()
                    deleteId = $(this).data('id'); // Store the id temporarily
                });

            } else {
                tableBody.append('<tr><td colspan="7">No family records found.</td></tr>');
            }
        } catch (e) {

        }
        
    }

    //#endregion

    //#region Delete Model

    $('#confirmDeleteBtn').click(function (e) {
        e.preventDefault()
        if (deleteId) {
            DeleteFamilyRecord(deleteId);
        }
    });

    //#endregion

    //#region Edit record function
    function EditFamilyRecord(id) {
        $.ajax({
            url: '/EmployeeFamily/GetEmployeeFamilyData',
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

    //#region Populate form on edit

    function populateOnform(record) {
        // Populate the form with the record data
        $('#editEmployeeFamilyInfoID').val(record.employeeFamilyInfoID);
        $('#editFullName').val(record.fullName);
        $('#editRelationToEmployee').val(record.relationToEmployee);
        $('#editOccupation').val(record.occupation);
        $('#editContactNumber').val(record.contactNumber);
        $('#editEmail').val(record.email);
        $('#editAddress').val(record.address);
    }

    //#endregion

    //#region Delete record function
    function DeleteFamilyRecord(id) {
        $.ajax({
            url: '/EmployeeFamily/Delete',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Family record deleted');
                    $('#delete_modal').modal('hide');
                    loadEmployeeFamilyData()
                } else {
                    toastr.error(response.message || 'Failed to delete family record');
                }
            },
            error: function () {
                toastr.error('An error occurred while deleting the family record');
            }
        });
    }

    //#endregion

    //#region clear form

    function clearForm() {
        $('#EmployeeFamilyInfoID').val('');
        $('#FullName').val('');
        $('#RelationToEmployee').val('');
        $('#Occupation').val('');
        $('#ContactNumber').val('');
        $('#Email').val('');
        $('#Address').val('');

        const tableBody = $('#employeeFamilyTable tbody');
        tableBody.empty();
    }

    //#endregion

    //#region Form Submission

    //$('form').on('submit', function (e) {
    $('#familyForm').on('submit', function (e) {
        e.preventDefault();

        const fields = ["EmployeePersonalId"];

        if (!validateFields(fields)) {
            return;
        }
        toastr.success('ee')
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
                    toastr.success(response.message || 'Employee Family saved successfully.');
                    loadEmployeeFamilyData(response.data)
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
                    toastr.error('Failed to save employee family. Please try again.');
                }
            }
        });
    });

    //#endregion

    //#region Edit Button Click

    $(document).on('click', '#btnEditSubmit', function (e) {
        e.preventDefault();
        let formData = {
            employeeFamilyInfoID: $('#editEmployeeFamilyInfoID').val(),
            fullName: $('#editFullName').val(),
            relationToEmployee: $('#editRelationToEmployee').val(),
            occupation: $('#editOccupation').val(),
            contactNumber: $('#editContactNumber').val(),
            email: $('#editEmail').val(),
            address: $('#editAddress').val()
        }

        $.ajax({
            type: "POST",
            url: "/EmployeeFamily/Update",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    $('#edit_family').modal('hide');
                    toastr.success(response.message || "Employee Family info updated successfully!");
                    loadEmployeeFamilyData()
                } else {
                    toastr.warning(response.message || "Failed to update Employee Family info. Try again!");
                }
            },
            error: function () {
                toastr.error("Error updating Employee Family info.");
            }
        });
    });

    //#endregion

    //#region Add Button Click (Fix the form submission)

    //$('.btn[type="button"]').not('#btnEditSubmit, #confirmDeleteBtn, #dismisDeleteModal, .btn-close').click(function (e) {
    //    e.preventDefault();

    //    const fields = ["EmployeePersonalId"];

    //    if (!validateFields(fields)) {
    //        return;
    //    }

    //    // Create form data manually
    //    let formData = {
    //        EmployeePersonalId: $('#EmployeePersonalId').val(),
    //        FullName: $('#FullName').val(),
    //        RelationToEmployee: $('#RelationToEmployee').val(),
    //        Occupation: $('#Occupation').val(),
    //        ContactNumber: $('#ContactNumber').val(),
    //        Email: $('#Email').val(),
    //        Address: $('#Address').val(),
    //        PersonalEmail: $('#PersonalEmail').val(),
    //        PersonalPhone: $('#PersonalPhone').val()
    //    };

    //    $.ajax({
    //        url: '/EmployeeFamily/Index',
    //        type: 'POST',
    //        data: formData,
    //        success: function (response) {
    //            if (response.success) {
    //                toastr.success(response.message || 'Employee Family saved successfully.');
    //                loadEmployeeFamilyData(response.data);

    //                // Clear only the family form fields, not employee selection
    //                $('#FullName').val('');
    //                $('#RelationToEmployee').val('');
    //                $('#Occupation').val('');
    //                $('#ContactNumber').val('');
    //                $('#Email').val('');
    //                $('#Address').val('');
    //            } else {
    //                toastr.warning(response.message);
    //                console.error(response.message);
    //            }
    //        },
    //        error: function (xhr) {
    //            if (xhr.status === 400 && xhr.responseJSON) {
    //                var errors = xhr.responseJSON;
    //                for (var field in errors) {
    //                    if (errors.hasOwnProperty(field)) {
    //                        toastr.error(errors[field][0]);
    //                    }
    //                }
    //            } else {
    //                toastr.error('Failed to save employee family. Please try again.');
    //            }
    //        }
    //    });
    //});

    //#endregion

});