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

    initializeSelect();


    //#region On change add sku

    $("#singleCategory").change(function () {
        var catId = $(this).val();   // get selected category id
        if (catId) {
            $.ajax({
                url: '/SingleProduct/GetSKUByCategory',   // controller action
                type: 'GET',
                data: { categoryId: catId },
                success: function (data) {
                    // populate SKU field
                    $("#singleSKU").val(data);
                },
                error: function () {
                    alert("Error fetching SKU");
                }
            });
        } else {
            $("#singleSKU").val(""); // clear if no category selected
        }
    });



    //#endregion 



    //#region Single Product

    $(document).on('select2:open', () => {
        setTimeout(() => {
            document.querySelector('.select2-search__field').focus();
        }, 0);
    });


    $('#advancedPricing').on('shown.bs.modal', function () {
        $('.searchableSelect1').select2({
            dropdownParent: $('#advancedPricing')
        });
    });


    //#region Get SKU
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

    //#region Advanced Pricing Modal Logic

    // Load data into modal when it opens
    $('#advancedPricing').on('show.bs.modal', function () {
        loadAdvancedPricingData();
    });

    // Save advanced pricing data to hidden fields
    $('#saveAdvancedPricing').on('click', function () {
        saveAdvancedPricingData();
    });

    function loadAdvancedPricingData() {
        const hiddenData = $('#advancedPricingData').val();

        showDev(hiddenData, 'get data');
        

        if (hiddenData) {
            try {
                const data = JSON.parse(hiddenData);

                // Load special pricing data
                $('#specialPrice').val(data.SpecialPrice || '');
                $('#customerGroup').val(data.CustomerGroup || '');
                $('#specialPriceStartDate').val(data.SpecialPriceStartDate || '');
                $('#specialPriceEndDate').val(data.SpecialPriceEndDate || '');

                // Load tier pricing data
                const tbody = $('#tierPricingBody');
                tbody.empty();
                tierPriceIndex = 0;

                if (data.TierPrices && data.TierPrices.length > 0) {
                    data.TierPrices.forEach(tier => {
                        addTierPriceRow(tier);
                    });
                }
            } catch (e) {
                console.error('Error loading advanced pricing data:', e);
            }
        }
    }

    function saveAdvancedPricingData() {
        const data = {
            SpecialPrice: $('#specialPrice').val() || null,
            CustomerGroup: $('#customerGroup').val() || null,
            SpecialPriceStartDate: $('#specialPriceStartDate').val() || null,
            SpecialPriceEndDate: $('#specialPriceEndDate').val() || null,
            TierPrices: []
        };

        

        // Collect tier pricing data
        $('#tierPricingBody tr').each(function () {
            const row = $(this);

            //const tierData = {
            //    CustomerGroup: row.find('select[name*="CustomerGroup"]').val(),
            //    MinQuantity: parseInt(row.find('input[name*="MinQuantity"]').val()) || 0,
            //    MaxQuantity: parseInt(row.find('input[name*="MaxQuantity"]').val()) || null,
            //    PriceType: row.find('select[name*="PriceType"]').val(),
            //    Value: parseFloat(row.find('input[name*="Value"]').val()) || 0
            //};
            debugger
            const tierData = {
               
                CustomerGroup: row.find('.tier-customer-group').val(),
                MinQuantity: parseInt(row.find('.tier-min-qty').val()) || 0,
                MaxQuantity: parseInt(row.find('.tier-max-qty').val()) || null,
                PriceType: row.find('.tier-price-typ').val(),
                Value: parseFloat(row.find('.tier-value').val()) || 0
            };
            data.TierPrices.push(tierData);
        });

        showDev(data, 'Set data 11');
        // Save to hidden field
        $('#advancedPricingData').val(JSON.stringify(data));

        toastr.success('Advanced pricing saved successfully!');
    }

    //#endregion

    //#region Form Submit
    $('#singleProductForm').on('submit', function (e) {
        e.preventDefault();
        const fd = new FormData(this);

        // Add image files
        files.forEach(f => fd.append('ProductImages', f));

        // Add advanced pricing data from hidden field
        const advancedPricingData = $('#advancedPricingData').val();
        showDev(advancedPricingData, 'get data when submit');
        if (advancedPricingData) {
            try {
                const data = JSON.parse(advancedPricingData);

                // Add special pricing fields
                fd.append('SpecialPrice', data.SpecialPrice || '');
                fd.append('CustomerGroup', data.CustomerGroup || '');
                fd.append('SpecialPriceStartDate', data.SpecialPriceStartDate || '');
                fd.append('SpecialPriceEndDate', data.SpecialPriceEndDate || '');

                // Add tier pricing fields
                if (data.TierPrices && data.TierPrices.length > 0) {
                    data.TierPrices.forEach((tier, index) => {
                        fd.append(`TierPrices[${index}].CustomerGroup`, tier.CustomerGroup);
                        fd.append(`TierPrices[${index}].MinQuantity`, tier.MinQuantity);
                        fd.append(`TierPrices[${index}].MaxQuantity`, tier.MaxQuantity || '');
                        fd.append(`TierPrices[${index}].PriceType`, tier.PriceType);
                        fd.append(`TierPrices[${index}].Value`, tier.Value);
                    });
                }
            } catch (e) {
                console.error('Error parsing advanced pricing data:', e);
            }
        }

        $.ajax({
            url: $(this).attr('action'),
            type: 'POST',
            data: fd,
            processData: false,
            contentType: false,
            success: res => {
                if (res.success) {
                    toastr.success(res.message);
                    showDev(res.model);
                    // Optionally redirect or reset form
                    // window.location.href = res.redirectUrl;
                } else {
                    toastr.warning(res.message || 'Error');
                    showDev(res.model);
                }
            },
            error: () => alert('Server error')
        });
    });
    //#endregion

    //#endregion

});
















//$(document).ready(function () {
//    showDev('Product Single Product Page');

//    const initializeSelect = () => {
//        $('.searchableSelect').select2({
//            width: '100%',
//            allowClear: true,
//            placeholder: 'Select an option',
//            language: { noResults: () => 'No results found' },
//            escapeMarkup: markup => markup
//        });
//    };

//    initializeSelect(); // ✅ function call after declaration

//    $('#openDemoModal').on('click', function () {
//        $('#demoModal').modal('show');
//    });



//    $(document).on('select2:open', () => {
//        setTimeout(() => {
//            document.querySelector('.select2-search__field').focus();
//        }, 0);
//    });


//    //#region Single Produict



//    $('#showAdvancedPricing11').on('click', function () {
//        $('#advancedPricing').modal('show');
//    });







//    //#region Get Sku

//    GetSKU();
//    function GetSKU() {
//        $.ajax({
//            url: '/SingleProduct/GetAutoSKU',
//            type: 'GET',
//            success: function (data) {
//                $('#singleSKU').val(data.sku);
//            }
//        });


//    }

//    //#endregion



//    $('#singleProductForm').on('submit', function (e) {
//        e.preventDefault();
//        const fd = new FormData(this);
//        files.forEach(f => fd.append('ProductImages', f));

//        $.ajax({
//            url: $(this).attr('action'),
//            type: 'POST',
//            data: fd,
//            processData: false,
//            contentType: false,
//            success: res => {
//                if (res.success) {
//                    toastr.success(res.message);
//                    showDev(res.model)
//                } else {
//                    toastr.warning(res.message || 'Error');
//                    showDev(res.model)
//                }
//            },
//            error: () => alert('Server error')
//        });
//    });



//    //#endregion


//});


