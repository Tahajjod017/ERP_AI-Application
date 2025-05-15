$(document).ready(function () {

    

    // First make sure the dropdown is visible and properly initialized
    $("#empCodePass").show();

    // Initialize employee data
    getAllemp();



    // First, let's remove or rename the duplicate ID
    // This code will rename the hidden input to avoid the conflict
    $('input[type="hidden"][id="empCodePass"]').attr('id', 'empCodePassHidden');

    // Now let's make sure we're working with the correct select element
    const $dropdown = $('select[id="empCodePass"]');

    // Debug to verify we found the correct element
    console.log('Found dropdown element:', $dropdown.length > 0, $dropdown.prop('tagName'));

  

    getAllemp();


    //sessionStorage.setItem("tabParameter", 'EMP001');
    var paramValue = sessionStorage.getItem("tabParameter");
    console.log("Received Parameter on page load:", paramValue);
    if (paramValue) {
        console.log('Making AJAX request for empCode:', paramValue);
        $.ajax({
            url: '/EmployeePersonal/getEmpInfo',
            type: 'GET',
            dataType: 'json',
            data: { empCode: paramValue },
            success: function (response) {
                console.log('Received Employee Data555:', response);
                // Add code to populate the fields with the response data
                if (response) {
                    $("#NidPass").val(response.nid || '');
                    $("#BirthCertiPass").val(response.birthCertificateNo || '');
                    $("#empCodePass").val(response.employeeID);
                    $("#empIdMain").val(response.employeeCode);


                    // Set the dropdown to the current employee if applicable
                    //if (response.employeeID) {
                       
                    //}
                } else {
                   
                }

                sessionStorage.removeItem("tabParameter");
            },
            error: function (xhr, status, error) {
                console.error('Error fetching data:', error);
                console.log('XHR Response Text:', xhr.responseText);
            }
        });
    } else {
        console.log('No empCode found in sessionStorage');
       
        $("#NidPass").val('');
        $("#BirthCertiPass").val('');
        $("#empCodePass").val('');
        $("#empIdMain").val('');
    }

    // Add change event handler for the dropdown
    $("#empCodePass").on('change', function () {
        var selectedEmpID = $(this).val();
        if (selectedEmpID && selectedEmpID !== 'Select Employee') {
            $.ajax({
                url: '/EmployeePersonal/getEmpInfoById',
                type: 'GET',
                dataType: 'json',
                data: { empId: selectedEmpID },
                success: function (response) {
                    console.log('1111',response)
                    // Update the fields with the selected employee's data
                    $("#NidPass").val(response.nid || '');
                    $("#BirthCertiPass").val(response.birthCertificateNo || '');
                    $("#empIdMain").val(response.employeeCode);
                   
                },
                error: function (xhr, status, error) {
                    console.error('Error fetching employee details:', error);
                }
            });
        } else {
            // Clear fields if "Select Employee" is chosen
            $("#NidPass").val('');
            $("#BirthCertiPass").val('');
        }
    });

    function getAllemp() {
        $.ajax({
            url: '/EmployeePersonal/GetEmployeeList',
            type: 'GET',
            dataType: 'json',
            success: function (data) {
                console.log('Data received:', data);
                const $dropdown = $("#empCodePass");
                $dropdown.empty();
                $dropdown.append(`<option value="">Select Employee</option>`);

                if (Array.isArray(data) && data.length > 0) {
                    data.forEach(employee => {
                        $dropdown.append(`
                            <option value="${employee.employeeID}">
                                ${employee.fullName} (${employee.employeeCode})
                            </option>
                        `);
                    });
                    console.log('Dropdown populated with ' + data.length + ' employees');
                } else {
                    console.log('No employees found or data is not in expected format:', data);
                }
            },
            error: function (error) {
                console.error("Error fetching employee list:", error);
            }
        });
    }
});




//$(document).ready(function () {
//    // First make sure the dropdown is visible and properly initialized
//    $("#empCodePass").show();

//    // Initialize employee data
//    getAllemp();

   

//    // First, let's remove or rename the duplicate ID
//    // This code will rename the hidden input to avoid the conflict
//    $('input[type="hidden"][id="empCodePass"]').attr('id', 'empCodePassHidden');

//    // Now let's make sure we're working with the correct select element
//    const $dropdown = $('select[id="empCodePass"]');

//    // Debug to verify we found the correct element
//    console.log('Found dropdown element:', $dropdown.length > 0, $dropdown.prop('tagName'));

//    getAllemp();

//    function getAllemp() {
//        $.ajax({
//            url: '/EmployeePersonal/GetEmployeeList',
//            type: 'GET',
//            dataType: 'json',
//            success: function (data) {
//                console.log('Data received:', data);

//                // Make sure we're targeting the SELECT element
//                const $dropdown = $('select[id="empCodePass"]');
//                $dropdown.empty();
//                $dropdown.append(`<option value="">Select Employee</option>`);

//                if (Array.isArray(data) && data.length > 0) {
//                    data.forEach(employee => {
//                        $dropdown.append(`
//                            <option value="${employee.employeeID}">
//                                ${employee.firstName || ''} ${employee.lastName || ''} (${employee.employeeCode || 'N/A'})
//                            </option>
//                        `);
//                    });
//                    console.log('Dropdown populated with ' + data.length + ' employees');

//                    // Re-initialize any Bootstrap selects if necessary
//                    if (typeof $.fn.selectpicker !== 'undefined') {
//                        $dropdown.selectpicker('refresh');
//                    }
//                } else {
//                    console.log('No employees found or data is not in expected format:', data);
//                }
//            },
//            error: function (error) {
//                console.error("Error fetching employee list:", error);
//            }
//        });
//    }
//    // Add change event for dropdown
//    $(document).on('change', '#empCodePass', function () {
//        console.log('Dropdown changed to:', $(this).val());
//        // Your change handler code here
//    });
//});