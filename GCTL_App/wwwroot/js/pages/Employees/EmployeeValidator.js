
////#region Validation

//function isValidEmail(email) {
//    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
//    return regex.test(email);
//}

//function getLabelText(inputId) {
//    var $label = $('label[for="' + inputId + '"]');
//    return $label.length ? $label.text().trim() : 'This field';
//}

//function showError(inputId, message) {
    
//    const $input = $("#" + inputId);

//    if ($input.is("input")) {
//        $input.addClass("is-invalid"); // <-- add this line 'test' +

//        if ($input.next(".text-danger").length === 0) {
//            const $error = $('<span class="text-danger">' + message + '</span>').css({
//                position: 'absolute',
//                fontSize: '0.8rem',
//                zIndex: 10,
//                //marginTop: '2px',
//                left: 0
//            });
//            $input.after($error);
//        }
//        attachInputValidationHandler($input);
//    }



   
//}

//function removeError(inputId) {
//    const $input = $("#" + inputId);
//    $input.removeClass("is-invalid"); // <-- remove the class

//    $input.next(".text-danger").remove();
//}

//function clearErrors() {
//    $(".text-danger").remove();
//}

//// New function to validate fields
//function validateFields(fields) {
//    clearErrors();
//    let valid = true;

   

//    const foundFields = [];
//    const missingFields = [];


//    fields.forEach(fieldId1 => {
//        const $field = $("#" + fieldId1);
//        if ($field.length === 0) {
//            console.warn(`Element with ID '${fieldId1}' not found.`);
//            missingFields.push(fieldId1);

//        } else {

//            foundFields.push(fieldId1);

//        }
//    });

    
//    foundFields.forEach(fieldId => {
       
//        const $field = $("#" + fieldId);
//        if ($field.length === 0) {
//            console.warn(`Element with ID '${fieldId}' not found.`);
//            valid = false;
//            return;
//        }
        
//        const value = $field.val().trim();
//        const labelText = getLabelText(fieldId);

//        if (!value) {
//            showError(fieldId, labelText + " is required.");
//            valid = false;
//        }
//    });

//    return valid;
//}


//function attachInputValidationHandler(selectors) {
//    $(selectors).on("input", function () {
//        const id = $(this).attr("id");
//        const value = $(this).val().trim();

//        if (value !== "") {
//            if (id === "personalEmail" && !isValidEmail(value)) {
//                return; // still invalid email, keep error
//            }
//            removeError(id);
//        }
//    });
//}

////#endregion







//$(function () {
//    // Function to get the label text for an input
//    function getLabelText($input) {
//        var inputId = $input.attr('id');
//        if (inputId) {
//            var $label = $('label[for="' + inputId + '"]');
//            return $label.length ? $label.text() : 'This field';
//        }
//        return 'This field';
//    }

//    // Show error for input
//    function showError($input) {
//        if ($input.hasClass('validChk-error')) {
//            return; // Error already shown
//        }
//        var labelText = getLabelText($input);
//        $input.addClass('validChk-error');
//        var $msg = $('<span class="validChk-error-message"></span>').text(labelText + ' is required');
//        $input.closest('.form-floating').append($msg);
//    }

//    // Remove error for input
//    function removeError($input) {
//        if (!$input.hasClass('validChk-error')) {
//            return; // No error to remove
//        }
//        $input.removeClass('validChk-error');
//        $input.closest('.form-floating').find('.validChk-error-message').remove();
//    }

//    // On form submit, validate all validChk inputs
//    $(document).on('submit', 'form', function (e) {
//        var $form = $(this);
//        var isValid = true;

//        $form.find('.validChk').each(function () {
//            var $input = $(this);
//            if ($input.val().trim() === '') {
//                showError($input);
//                isValid = false;
//            } else {
//                removeError($input);
//            }
//        });

//        if (!isValid) {
//            e.preventDefault(); // Prevent form submission
//        }
//    });

//    // On input, remove error if valid
//    $(document).on('input', '.validChk', function () {
//        var $input = $(this);
//        if ($input.val().trim() !== '') {
//            removeError($input);
//        }
//    });
//});
