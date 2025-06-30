//$(document).ready(function () {

//    $('#logoUpload').on('change', function (event) {
//        var reader = new FileReader();
//        reader.onload = function (e) {
//            // Display the selected image in the logo preview
//            $('#logoPreview').attr('src', e.target.result);
//        };
//        // Read the file as a data URL (base64 encoded)
//        if (event.target.files[0]) {
//            reader.readAsDataURL(event.target.files[0]);
//        }
//    });


//    $('#faviconUpload').on('change', function (event) {
//        var reader = new FileReader();
//        reader.onload = function (e) {
//            // Display the selected image in the favicon preview
//            $('#faviconPreview').attr('src', e.target.result);
//        };
//        // Read the file as a data URL (base64 encoded)
//        if (event.target.files[0]) {
//            reader.readAsDataURL(event.target.files[0]);
//        }
//    });
//});

$(document).ready(function () {
    // When the logo input changes (image is selected)
    $('#logoUpload').on('change', function (event) {
        var file = event.target.files[0];

        // Check if a file is selected
        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var img = new Image();
                img.onload = function () {
                    // Validate image size (160px x 50px)
                    if (img.width === 160 && img.height === 50) {
                        // If valid size, show the image preview
                        $('#logoPreview').attr('src', e.target.result);
                        $('#logoPreview').show(); // Show the preview image
                        $('#logoWarning').hide(); // Hide the warning message
                        $('#clearLogo').show(); // Show the clear logo button
                    } else {
                        // If invalid size, hide preview and show warning
                        $('#logoPreview').hide();
                        $('#logoWarning').text('Invalid image size. Recommended size is 160px x 50px.').show();
                        $('#clearLogo').hide();
                    }
                };
                img.src = e.target.result; // Set the image source to the selected file
            };
            reader.readAsDataURL(file); // Read the file as a data URL
        }
    });

    // When the favicon input changes (image is selected)
    $('#faviconUpload').on('change', function (event) {
        var file = event.target.files[0];

        // Check if a file is selected
        if (file) {
            var reader = new FileReader();
            reader.onload = function (e) {
                var img = new Image();
                img.onload = function () {
                    // Validate image size (128px x 128px)
                    if (img.width === 128 && img.height === 128) {
                        // If valid size, show the image preview
                        $('#faviconPreview').attr('src', e.target.result);
                        $('#faviconPreview').show(); // Show the preview image
                        $('#faviconWarning').hide(); // Hide the warning message
                    } else {
                        // If invalid size, hide preview and show warning
                        $('#faviconPreview').hide();
                        $('#faviconWarning').text('Invalid image size. Recommended size is 128px x 128px.').show();
                    }
                };
                img.src = e.target.result; // Set the image source to the selected file
            };
            reader.readAsDataURL(file); // Read the file as a data URL
        }
    });
});

