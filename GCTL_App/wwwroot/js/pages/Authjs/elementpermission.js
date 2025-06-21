$('#companySelection').on('change', function () {
    var companyId = $(this).val();
    $.get('/RolePermission/GetElementRolesByCompany', { companyId }, function (data) {
        var dropdown = $('#roleSelection');
        dropdown.empty().append('<option value="">Select a Role</option>');
        data.forEach(r => dropdown.append(`<option value="${r.id}">${r.name}</option>`));
    });
});

$('#pageSelection').on('change', function () {
    var pageId = $(this).val();
    $.get('/RolePermission/GetElementsByPage', { pageId }, function (data) {
        //var dropdown = $('#elementSelection');
        //dropdown.empty();
        //data.forEach(el => dropdown.append(`<option value="${el.key}">${el.name}</option>`));
        choiceManager.populateDropdown('elementSelection',data);
    });
});


//$('#permissions-form').on('submit', function (e) {
//    e.preventDefault();

//    var a = choiceManager.getChoiceValue('elementSelection');
//    toastr.success(a)
//});


$('#permissions-form').on('submit', function (e) {
    e.preventDefault();
    var model = {
        RoleId: $('#roleSelection').val(),
        PageId: $('#pageSelection').val(),
        ElementKeys: $('#elementSelection').val()  // Ensure this is an array if the backend expects it
    };

    $.ajax({
        url: '/RolePermission/SaveElementPermissions',
        method: 'POST',
        contentType: 'application/json',  // Tell the server this is JSON
        data: JSON.stringify(model),  // Serialize the model object into JSON
        success: function (res) {
            if (res.success) toastr.success(res.message);
            else toastr.error(res.message);
        },
        error: function (xhr, status, error) {
            toastr.error("An error occurred while saving permissions.");
        }
    });
});
