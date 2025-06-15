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
                    loadEmployeeContactData(selectedEmployeeId);
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

    function loadEmployeeContactData(selectedEmployeeId) {
        if (!selectedEmployeeId || selectedEmployeeId === 0) {
            selectedEmployeeId = $('#EmployeePersonalId').val();
        }

        $.ajax({
            url: '/EmployeeContact/GetEmployeeData',
            type: 'GET',
            data: { id: selectedEmployeeId },
            success: function (data) {
                var employee = Array.isArray(data) ? data[0] : data;

                $('#PersonalEmail').val(employee.personalEmail);
                $('#PersonalPhone').val(employee.personalPhone);
                choiceManager.setChoiceValue('EmployeePersonalId', employee.employeePersonalId);

                const tableBody = $('#employeeContactTable tbody');
                tableBody.empty();

                if (employee.isActive) {
                    PopulateTable(data);
                }
            },
            error: function (xhr, status, error) {
                console.error('Error fetching employee contact data:', error);
            }
        });
    }
    //#endregion

    let deleteId = null;

    //#region Populate Table
    function PopulateTable(data) {
        console.log('data for table ', data);
        const tableBody = $('#employeeContactTable tbody');
        tableBody.empty();

        if (data && data.length > 0) {
            data.forEach(function (item) {
                const row = `<tr data-id="${item.employeeEmeContactID}">
                    <td>${item.contactName || ''}</td>
                    <td>${item.relationship || ''}</td>
                    <td>${item.contactNumber || ''}</td>
                    <td>${item.contactEmail || ''}</td>
                    <td class="align-middle white-space-nowrap">
                        <div class="btn-reveal-trigger position-static g-3">
                            <button class="nav-item me-2 btn-edit" data-id="${item.employeeEmeContactID}" data-bs-toggle="modal" data-bs-target="#edit_emmergencyContact">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="nav-item me-2 btn-delete" data-id="${item.employeeEmeContactID}" data-bs-toggle="modal" data-bs-target="#delete_modal">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>`;
                tableBody.append(row);
            });

            $('.btn-edit').click(function (e) {
                e.preventDefault();
                const id = $(this).data('id');
                EditContactRecord(id);
            });

            $('.btn-delete').click(function (e) {
                e.preventDefault();
                deleteId = $(this).data('id');
            });
        } else {
            tableBody.append('<tr><td colspan="5">No emergency contact records found.</td></tr>');
        }
    }
    //#endregion

    //#region Delete Model

    $('#confirmDeleteBtn').click(function (e) {
        e.preventDefault();
        if (deleteId) {
            DeleteContactRecord(deleteId);
        }
    });

    //#endregion

    //#region Edit record function
    function EditContactRecord(id) {
        $.ajax({
            url: '/EmployeeContact/GetEmployeeContactData',
            type: 'GET',
            data: { id: id },
            success: function (data) {
                console.log("Received Data:", data);
                populateOnform(data);
            },
            error: function (xhr, status, error) {
                console.error('XHR Status:', status);
                console.error('XHR Response:', xhr.responseText);
                console.error('Error Details:', error);
            }
        });
    }
    //#endregion

    //#region Populate form on edit
    function populateOnform(record) {
        $('#editEmployeeEmeContactID').val(record.employeeEmeContactID);
        $('#editContactName').val(record.contactName);
        $('#editRelationship').val(record.relationship);
        $('#editContactNumber').val(record.contactNumber);
        $('#editContactEmail').val(record.contactEmail);
    }
    //#endregion

    //#region Delete record function
    function DeleteContactRecord(id) {
        $.ajax({
            url: '/EmployeeContact/Delete',
            type: 'POST',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Emergency contact deleted');
                    $('#delete_modal').modal('hide');
                    loadEmployeeContactData();
                } else {
                    toastr.error(response.message || 'Failed to delete emergency contact');
                }
            },
            error: function () {
                toastr.error('An error occurred while deleting the emergency contact');
            }
        });
    }
    //#endregion

    //#region clear form
    function clearForm() {
        $('#EmployeeEmeContactID').val('');
        $('#ContactName').val('');
        $('#Relationship').val('');
        $('#ContactNumber').val('');
        $('#ContactEmail').val('');

        const tableBody = $('#employeeContactTable tbody');
        tableBody.empty();
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
                    toastr.success(response.message || 'Emergency contact saved successfully.');
                    loadEmployeeContactData(response.data);
                    clearForm();
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
                    toastr.error('Failed to save emergency contact. Please try again.');
                }
            }
        });
    });
    //#endregion

    //#region Edit Button Click
    $(document).on('click', '#btnEditSubmit', function (e) {
        e.preventDefault();
        let formData = {
            employeeEmeContactID: $('#editEmployeeEmeContactID').val(),
            contactName: $('#editContactName').val(),
            relationship: $('#editRelationship').val(),
            contactNumber: $('#editContactNumber').val(),
            contactEmail: $('#editContactEmail').val()
        };

        $.ajax({
            type: "POST",
            url: "/EmployeeContact/Update",
            contentType: "application/json",
            data: JSON.stringify(formData),
            success: function (response) {
                if (response.success) {
                    $('#edit_emmergencyContact').modal('hide');
                    toastr.success(response.message || "Emergency contact updated successfully!");
                    loadEmployeeContactData();
                } else {
                    toastr.warning(response.message || "Failed to update emergency contact. Try again!");
                }
            },
            error: function () {
                toastr.error("Error updating emergency contact.");
            }
        });
    });
    //#endregion

    //#region Add Button Click
    $('.btn[type="button"]').not('#btnEditSubmit, #confirmDeleteBtn, #dismisDeleteModal, .btn-close').click(function (e) {
        e.preventDefault();

        const fields = ["EmployeePersonalId"];

        if (!validateFields(fields)) {
            return;
        }

        let formData = {
            EmployeePersonalId: $('#EmployeePersonalId').val(),
            ContactName: $('#ContactName').val(),
            Relationship: $('#Relationship').val(),
            ContactNumber: $('#ContactNumber').val(),
            ContactEmail: $('#ContactEmail').val(),
            PersonalEmail: $('#PersonalEmail').val(),
            PersonalPhone: $('#PersonalPhone').val()
        };

        $.ajax({
            url: '/EmployeeContact/Index',
            type: 'POST',
            data: formData,
            success: function (response) {
                if (response.success) {
                    toastr.success(response.message || 'Emergency contact saved successfully.');
                    loadEmployeeContactData(response.data);
                    $('#ContactName').val('');
                    $('#Relationship').val('');
                    $('#ContactNumber').val('');
                    $('#ContactEmail').val('');
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
                    toastr.error('Failed to save emergency contact. Please try again.');
                }
            }
        });
    });
    //#endregion


   
    initCustomContactAutocomplete();
   
    
    //#region Custom Contact Name Autocomplete
    function initCustomContactAutocomplete() {
        const input = $('#ContactName');
        let dropdown = $('<div class="custom-autocomplete-dropdown" style="display: none;"></div>');
        input.after(dropdown).empty();

        input.on('input', function () {
            const query = $(this).val().trim();
            if (query.trim() >= 2) {
                dropdown.hide().empty();
                return;
            }

            const id = choiceManager.getChoiceValue('EmployeePersonalId')

            $.ajax({
                url: '/EmployeeContact/GetContactSuggestions',
                type: 'GET',
                data: { term: query, id: id },
                success: function (data) {
                    dropdown.empty();
                    if (data && data.length > 0) {
                        data.forEach(item => {
                            const suggestion = $(`<a class="autocomplete-item">${item.label}</a>`);
                            suggestion.data('item', item);
                            suggestion.on('click', function () {
                                input.val(item.value);
                                $('#Relationship').val(item.relationship || '');
                                $('#ContactNumber').val(item.contactNumber || '');
                                $('#ContactEmail').val(item.contactEmail || '');
                                dropdown.hide().empty();
                            });
                            dropdown.append(suggestion);
                        });
                        dropdown.show();
                    } else {
                        dropdown.hide();
                    }
                },
                error: function () {
                    console.error('Error fetching contact suggestions');
                    dropdown.hide().empty();
                }
            });
        });

        input.on('blur', function () {
            setTimeout(() => dropdown.hide().empty(), 200);
        });

        input.on('focus', function () {
            if ($(this).val().trim().length >= 2) {
                $(this).trigger('input');
            }
        });

        // Clear other fields if no suggestion selected (new name)
        input.on('change', function () {
            const value = $(this).val().trim();
            const selectedItem = dropdown.find('.autocomplete-item').filter(function () {
                return $(this).data('item') && $(this).data('item').value === value;
            });

            if (!selectedItem.length) {
                // No matching suggestion, clear other fields
                $('#Relationship').val('');
                $('#ContactNumber').val('');
                $('#ContactEmail').val('');
            }
        });

        // Close dropdown on click outside
        $(document).on('click', function (e) {
            if (!$(e.target).closest('#ContactName, .custom-autocomplete-dropdown').length) {
                dropdown.hide().empty();
            }
        });
    }
    
    //#endregion

});