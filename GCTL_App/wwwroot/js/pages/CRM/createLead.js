
$(document).ready(function () {

//#region Auto suggest

let nationalities = [];



$.ajax({
    url: '/CreateLead/GetNationalities',
    method: 'GET',
    success: function (data) {
        console.log(data);
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

// On input typing
$('#ContactNameSearch').on('input', function () {
    const query = $(this).val();
    $('#searchResults').show();
    $('#removeContactNameBtn').toggle(!!query);
    showSuggestions(query);
});

// On selecting a suggestion
$(document).on('click', '.contractName-item', function () {
    const selected = $(this).text();
    $('#ContactNameSearch').val(selected);
    $('#searchResults').hide();
    $('#removeContactNameBtn').show();
});

// Clear selected value
$('#removeContactNameBtn').on('click', function () {
    $('#ContactNameSearch').val('');
    $('#contactNameList').empty();
    $('#noResults').hide();
    $('#searchResults').hide();
    $(this).hide();
});
    let targetTab = 'company'; // default

    // Button to open Company tab
    $('#addNewContactNameBtn').on('click', function () {
        targetTab = 'company'; // set desired tab
        $('#addNationalityModal').modal('show');
        console.log(targetTab)
        $('#increment-tab').removeClass('active');
        $('#increment').removeClass('active show');
        $('#promotion-tab').addClass('active');
        $('#promotion').addClass('active show');
    });

    // Button to open Person tab
    $('#addNewContactNameBtn2').on('click', function () {
        targetTab = 'person';
        $('#addNationalityModal').modal('show');
        console.log(targetTab);
        $('#promotion-tab').removeClass('active');
        $('#promotion').removeClass('active show');
        $('#increment-tab').addClass('active');
        $('#increment').addClass('active show');
    });
// Show modal when clicking "Click Here"
    // Track which tab to activate
    //let targetTab = 'company'; // default

    //// Button to open Company tab
    //$('#addNewContactNameBtn').on('click', function () {
    //    targetTab = 'company'; // set desired tab
    //    $('#addNationalityModal').modal('show');
    //});

    //// Button to open Person tab
    //$('#addNewContactNameBtn2').on('click', function () {
    //       = 'person';
    //    $('#addNationalityModal').modal('show');
    //});

    //// When modal is fully shown, activate the correct tab
    //$('#addNationalityModal').on('shown.bs.modal', function () {
    //    if (targetTab === 'company') {
    //        $('#increment-tab').removeClass('active');
    //        $('#increment').removeClass('active show');

    //        $('#promotion-tab').addClass('active');
    //        $('#promotion').addClass('active show');
    //    } else {
    //        $('#promotion-tab').removeClass('active');
    //        $('#promotion').removeClass('active show');

    //        $('#increment-tab').addClass('active');
    //        $('#increment').addClass('active show');
    //    }
    //});



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