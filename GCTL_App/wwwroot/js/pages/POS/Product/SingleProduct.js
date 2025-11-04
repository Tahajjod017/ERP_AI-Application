$(document).ready(function () {
    showDev('Product Single Product Page');

    const initializeSelect = () => {
        $('.searchableSelect').select2({
            width: '100%',
            allowClear: true,
            placeholder: 'Select an option',
            language: { noResults: () => 'No results found' },
            escapeMarkup: markup => markup
        });
    };

    initializeSelect(); // ✅ function call after declaration

    $('#openDemoModal').on('click', function () {
        $('#demoModal').modal('show');
    });



    $(document).on('select2:open', () => {
        setTimeout(() => {
            document.querySelector('.select2-search__field').focus();
        }, 0);
    });


    //#region Single Produict



    $('#showAdvancedPricing11').on('click', function () {
        $('#advancedPricing').modal('show');
    });







    //#region Get Sku

    GetSKU();
    function GetSKU() {
        $.ajax({
            url: '/SingleProduct/GetAutoSKU',
            type: 'GET',
            success: function (data) {
                $('#singleSKU').val(data.sku);
            }
        });


    }

    //#endregion



    $('#singleProductForm').on('submit', function (e) {
        e.preventDefault();
        const fd = new FormData(this);
        files.forEach(f => fd.append('ProductImages', f));

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: res => {
                if (res.success) {
                    toastr.success(res.message);
                    showDev(res.model)
                } else {
                    toastr.warning(res.message || 'Error');
                    showDev(res.model)
                }
            },
            error: () => alert('Server error')
        });
    });



    //#endregion


});


