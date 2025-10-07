

$(document).ready(function () {


    //#region Validation

    function isValidEmail(email) {
        const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return regex.test(email);
    }

    function showError(inputId, message) {
        const $input = $("#" + inputId);
        if ($input.next(".text-danger").length === 0) {
            const $error = $('<span class="text-danger">' + message + '</span>').css({
                position: 'absolute',
                fontSize: '0.8rem',
                zIndex: 10,
                marginTop: '2px',
                left: 0
            });
            $input.after($error);
        }
    }


    function removeError(inputId) {
        const $input = $("#" + inputId);
        $input.next(".text-danger").remove();
    }

    function clearErrors() {
        $(".text-danger").remove();
    }


    function attachInputValidationHandler(selectors) {
        $(selectors).on("input", function () {
            const id = $(this).attr("id");
            const value = $(this).val().trim();

            if (value !== "") {
                if (id === "personalEmail" && !isValidEmail(value)) {
                    return; // still invalid email, keep error
                }
                removeError(id);
            }
        });
    }

    //$("#firstName, #lastName, #personalMobile, #personalEmail").on("input", function () {
    //    const id = $(this).attr("id");
    //    const value = $(this).val().trim();

    //    if (value !== "") {
    //        if (id === "personalEmail" && !isValidEmail(value)) {
    //            return; // still invalid email, keep error
    //        }
    //        removeError(id);
    //    }
    //});


    attachInputValidationHandler("#firstName, #lastName, #personalMobile, #personalEmail");
    

    //#endregion


    //#region Submit 


    $('#submitButton').on('click', function (e) {
        e.preventDefault();

        const enteredNationality = $('#nationalitySearch').val().trim();

        if (enteredNationality && !nationalities.includes(enteredNationality)) {
            $('#newNationalityName').val(enteredNationality);
            $('#addNationalityModal').modal('show');
        }

        clearErrors();
        let valid = true;

        const firstName = $("#firstName").val().trim();
        const lastName = $("#lastName").val().trim();
        const email = $("#personalEmail").val().trim();
        const mobile = $("#personalMobile").val().trim();

        if (!firstName) {
            showError("firstName", "First Name is required.");
            toastr.warning("firstName", "First Name is required.");
            valid = false;
        }

        if (!lastName) {
            showError("lastName", "Last Name is required.");
            toastr.warning("lastName", "Last Name is required.");
            valid = false;
        }

        if (!email) {
            showError("personalEmail", "Email is required.");
            toastr.warning("personalEmail", "Email is required.");
            valid = false;
        } else if (!isValidEmail(email)) {
            showError("personalEmail", "Invalid email format.");
            valid = false;
        }

        if (!mobile) {
            showError("personalMobile", "Mobile number is required.");
            toastr.warning("personalMobile", "Mobile number is required.");
            valid = false;
        }

        if (valid) {
            $('#employeeForm').submit();
        }

      
           
        
    });

    //#endregion

    //$('#employeeForm').on('submit', function (e) {
    //    if (!confirm("Are you sure you want to submit the formooo?")) {
    //        e.preventDefault();
    //    }
    //});

    //#region tostr


    const toastrElement = document.getElementById('toastr-data');
    if (toastrElement) {
        const message = toastrElement.getAttribute('data-message');
        const type = toastrElement.getAttribute('data-type');

        if (message && message.trim() !== '') {
            switch (type) {
                case 'success':
                    toastr.success(message);
                    break;
                case 'error':
                    toastr.error(message);
                    break;
                case 'info':
                    toastr.info(message);
                    break;
                case 'warning':
                    toastr.warning(message);
                    break;
                default:
                    toastr.info(message);
                    break;
            }
        }
    }

    //#endregion

    //#region Choice Min Js


   


    let maritalChoices;
    function initMaritalChoices() {
        maritalChoices = new Choices('#MaritalStatus', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Marital Status'
        });
    }
    document.addEventListener('DOMContentLoaded', initMaritalChoices);
    initMaritalChoices();


    let ReligionChoices;
    function initReligionChoices() {
        ReligionChoices = new Choices('#Religion', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Religion'
        });
    }
    document.addEventListener('DOMContentLoaded', initReligionChoices);
    initReligionChoices();

    let GenderChoices;
    function initGenderChoices() {
        GenderChoices = new Choices('#Gender', {
            removeItemButton: true,
            shouldSort: false,
            placeholderValue: 'Select Gender'
        });
    }
    document.addEventListener('DOMContentLoaded', initReligionChoices);
    initGenderChoices();




    //#endregion

    //#region Image Perview

    $(function () {
        function setupImagePreview($fileInput, $previewImg, $closeBtn) {
            $fileInput.on('change', function () {
                const file = this.files[0];
                if (file && file.type.startsWith('image/')) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        $previewImg
                            .attr('src', e.target.result)
                            .css('visibility', 'visible');
                        $closeBtn.show();
                    };
                    reader.readAsDataURL(file);
                } else {
                    // optional: warn if not an image
                    alert('Please select a valid image file.');
                    this.value = '';
                }
            });

            $closeBtn.on('click', function () {
                $fileInput.val('');
                $previewImg
                    .attr('src', '')
                    .css('visibility', 'hidden');
                $closeBtn.hide();
            });
        }

        // wire up both fields
        setupImagePreview(
            $('#epImageUpload'),
            $('#epImagePreview'),
            $('#epCloseBtn')
        );
        setupImagePreview(
            $('#esImageUpload'),
            $('#esImagePreview'),
            $('#esCloseBtn')
        );
    });

    //#endregion

    //#region Auto suggest

    let nationalities = [];



    $.ajax({
        url: '/EmployeePersonal/GetNationalities', 
        method: 'GET',
        success: function (data) {
            nationalities = data; 
        },
        error: function () {
            alert('Failed to load nationalities');
        }
    });



    function showSuggestions(query) {
        const $list = $('#nationalityList');
        const $noResults = $('#noResults');
        $list.empty();
        $noResults.hide();

        if (!query) return;

        const filtered = nationalities.filter(item =>
            item.toLowerCase().includes(query.toLowerCase())
        );

        if (filtered.length > 0) {
            filtered.forEach(item => {
                $list.append(`<button type="button" class="list-group-item list-group-item-action nationality-item">${item}</button>`);
            });
        } else {
            $noResults.show();
        }
    }

    // On input typing
    $('#nationalitySearch').on('input', function () {
        const query = $(this).val();
        $('#searchResults').show();
        $('#removeNationalityBtn').toggle(!!query);
        showSuggestions(query);
    });

    // On selecting a suggestion
    $(document).on('click', '.nationality-item', function () {
        const selected = $(this).text();
        $('#nationalitySearch').val(selected);
        $('#searchResults').hide();
        $('#removeNationalityBtn').show();
    });

    // Clear selected value
    $('#removeNationalityBtn').on('click', function () {
        $('#nationalitySearch').val('');
        $('#nationalityList').empty();
        $('#noResults').hide();
        $('#searchResults').hide();
        $(this).hide();
    });

    // Show modal when clicking "Click Here"
    $('#addNewNationalityBtn').on('click', function () {
        const newValue = $('#nationalitySearch').val();
        $('#newNationalityName').val(newValue);
        $('#addNationalityModal').modal('show');
    });

    
    //$('#confirmAddNationalityBtn').on('click', function () {
    //    const newNationality = $('#newNationalityName').val().trim();
    //    if (newNationality && !nationalities.includes(newNationality)) {
    //        nationalities.push(newNationality);
    //        $('#nationalitySearch').val(newNationality);
    //        $('#searchResults').hide();
    //        $('#removeNationalityBtn').show();
    //        $('#addNationalityModal').modal('hide');
    //    }
    //});

    // Optional: hide suggestion list when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#nationalitySearch, #searchResults').length) {
            $('#searchResults').hide();
        }
    });


    //#endregion

    //#region Save Nationality

    $('#confirmAddNationalityBtn').on('click', function () {
        const newNationality = $('#newNationalityName').val().trim();

        if (!newNationality) {
            alert('Please enter a nationality name.');
            return;
        }

        $.ajax({
            url: '/EmployeePersonal/SaveNationality', // <-- Update with your actual route
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(newNationality),
            success: function (response) {
                if (response.success) {
                    nationalities.push(newNationality);
                    $('#nationalitySearch').val(newNationality);
                    $('#searchResults').hide();
                    $('#removeNationalityBtn').show();
                    $('#addNationalityModal').modal('hide');
                }
            },
            error: function (xhr) {
                alert('Error saving nationality: ' + xhr.responseText);
            }
        });
    });


    //#endregion 

});