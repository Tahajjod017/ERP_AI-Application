$(document).ready(function () {


    $('#submitButton').on('click', function (e) {
        e.preventDefault()
        if (confirm("Are you sure you want to submit the form?")) {
            $('#employeeForm').submit(); 
        }
    });


    //$('#employeeForm').on('submit', function (e) {
    //    if (!confirm("Are you sure you want to submit the formooo?")) {
    //        e.preventDefault();
    //    }
    //});


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



});