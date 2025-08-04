
$(document).ready(function () {

    //#region Auto suggest

    let nationalities = [];



    $.ajax({
        url: '/CreateLead/GetNationalities',
        method: 'GET',
        success: function (data) {
            nationalities = data;
        },
        error: function () {
            alert('Failed to load Contact Name');
        }
    });



    function showSuggestions(query) {
        const $list = $('#contactNameList');
        const $noResults = $('#noResults');
        $list.empty();
        $noResults.hide();

        if (!query) return;

        const filtered = nationalities.filter(item =>
            item.toLowerCase().includes(query.toLowerCase())
        );

        if (filtered.length > 0) {
            filtered.forEach(item => {
                $list.append(`<button type="button" class="list-group-item list-group-item-action contractName-item">${item}</button>`);
            });
        } else {
            $noResults.show();
        }
    }

    $('#ContactNameSearch').on('input', function () {
        const query = $(this).val();
        $('#searchResults').show();
        $('#removeContactNameBtn').toggle(!!query);
        showSuggestions(query);
    });

    $(document).on('click', '.contractName-item', function () {
        const selected = $(this).text();
        $('#ContactNameSearch').val(selected);
        $('#searchResults').hide();
        $('#removeContactNameBtn').show();
    });

    $('#removeContactNameBtn').on('click', function () {
        $('#ContactNameSearch').val('');
        $('#contactNameList').empty();
        $('#noResults').hide();
        $('#searchResults').hide();
        $(this).hide();
    });
    let targetTab = 'company';

    $('#addNewContactNameBtn').on('click', function () {
        targetTab = 'company'; // set desired tab
        $('#addNationalityModal').modal('show');
        $('#increment-tab').removeClass('active');
        $('#increment').removeClass('active show');
        $('#promotion-tab').addClass('active');
        $('#promotion').addClass('active show');
    });

    $('#addNewContactNameBtn2').on('click', function () {
        targetTab = 'person';
        $('#addNationalityModal').modal('show');
        $('#promotion-tab').removeClass('active');
        $('#promotion').removeClass('active show');
        $('#increment-tab').addClass('active');
        $('#increment').addClass('active show');
    });

    $("#closeModal").on('click', function () {
        $('#addNationalityModal').modal('hide');
    });
    $("#closeModal1").on('click', function () {
        $('#addNationalityModal').modal('hide');
    });


    // Call initMap when the script is loaded
    //window.initMap = initMap;
    //console.log("initMap is running2");



//$('#confirmAddNationalityBtn').on('click', function () {
//    const newNationality = $('#newNationalityName').val().trim();
//    if (newNationality && !nationalities.includes(newNationality)) {
//        nationalities.push(newNationality);
//        $('#ContactNameSearch').val(newNationality);
//        $('#searchResults').hide();
//        $('#removeContactNameBtn').show();
//        $('#addNationalityModal').modal('hide');
//    }
//});

// Optional: hide suggestion list when clicking outside
$(document).on('click', function (e) {
    if (!$(e.target).closest('#ContactNameSearch, #searchResults').length) {
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
        url: '/CreateLead/SaveNationality', // <-- Update with your actual route
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(newNationality),
        success: function (response) {
            if (response.success) {
                nationalities.push(newNationality);
                $('#ContactNameSearch').val(newNationality);
                $('#searchResults').hide();
                $('#removeContactNameBtn').show();
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