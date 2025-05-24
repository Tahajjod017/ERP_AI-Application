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


    //#region ChoiceMin

    window.choicesInstances = {};

    document.addEventListener('DOMContentLoaded', function () {
        const dropdownConfigs = [
           
            { selector: '#MaritalStatus', placeholder: 'Select Merital Status', dataKey: 'maritalStatus' },
           // { selector: '#OccupationID', placeholder: 'Select Taxpayer\'s Profession', dataKey: 'occupationData' },
          //  { selector: '#TaxZoneID', placeholder: 'Select Tax Zone', dataKey: 'taxZoneData' },
         //   { selector: '#TaxCircleID', placeholder: 'Select Circle', dataKey: 'taxCircleData' }
        ];

        dropdownConfigs.forEach(config => {
            const element = document.querySelector(config.selector);
            const data = window[config.dataKey];

            if (element && data) {
                const instance = new Choices(element, {
                    removeItemButton: true,
                    placeholder: true,
                    placeholderValue: config.placeholder,
                    searchEnabled: true
                });

                window.choicesInstances[config.selector] = {
                    instance: instance,
                    placeholder: config.placeholder,
                    data: data
                };
              

                element.addEventListener('change', function () {
                    $(element).valid(); 
                });
                
            }
        });


    //#endregion




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