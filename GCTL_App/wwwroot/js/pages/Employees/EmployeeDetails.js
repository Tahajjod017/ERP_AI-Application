$(document).ready(function () {

    
    function getEmployeeIdAfterIndex(url) {
        var matches = url.match(/\/index\/(\d+)(?!.*\d)/i); // 'i' makes it case-insensitive
        return matches ? parseInt(matches[1], 10) : null;
    }

    // Example usage
    var currentUrl = window.location.href;
    var employeeId = getEmployeeIdAfterIndex(currentUrl);
   
    

   

    //#region Ajax Call to Fetch Employee Data
    GetProfileInfo(employeeId)
    function GetProfileInfo(empID) {
        $.ajax({
            url: '/EmployeeDetails/BasicDetail',
            type: 'GET',
            data: { empID: empID },
            dataType: 'json',
            success: function (response) {

                console.log(response.data);

                populateBaseInfo(response.data)
                populateBankInfoTable(response.data.bankInfoData);
                populateFamilyInfoTable(response.data.familyInfoData);
                populateEducationInfoTable(response.data.educationInfoData);
                populateTrainingInfoTable(response.data.trainingInfoData);
                populateEmergencyContactInfo(response.data.emergencyContactInfoData);
            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                alert("Failed to load employee data. Please try again later.");
            }
        });
    }

    //#endregion


   
   // populateExperienceInfoTable(experienceInfoData);

});


//#region Populate real


function populateBaseInfo(profile) {
    $("#profileImage").attr("src", profile.image);
    $("#employeeName").text(profile.name);
    $("#employeeRole").text(profile.role);
    $("#employeeExperience").text(profile.experience);
    $("#employeeId").text(profile.employeeId);
    $("#department").text(profile.department);
    $("#joinDate").text(profile.joinDate);
    $("#phoneNumber").text(profile.phone);
    $("#emailAddress").text(profile.email).attr("href", `mailto:${profile.email}`);
    $("#gender").text(profile.gender);
    $("#dateOfBirth").text(profile.dateOfBirth);
    $("#address").html(profile.address.replace(", ", "<br>"));

    $("#supervisorImage").attr("src", profile.supervisorImage);
    $("#supervisorName").text(profile.supervisorName);

    $("#passportNumber").text(profile.number);
    $("#passportExpiryDate").text(profile.expiryDate);
    $("#nationality").text(profile.nationality);
    $("#religion").text(profile.religion);
    $("#maritalStatus").text(profile.maritalStatus);
    $("#spouseEmployment").text(profile.spouseEmployment);
    $("#numberOfChildren").text(profile.numberOfChildren);

    $("#employeeBio").text(profile.bio);
}



// Populate Functions
function populateBankInfoTable(data) {
    let tbody = '';
    data.forEach(item => {
        tbody += `<tr>
                            <td>${item.bankName}</td>
                            <td>${item.branch}</td>
                            <td>${item.accountNo}</td>
                            <td>${item.swiftCode}</td>
                            <td>${item.ifscCode}</td>
                          </tr>`;
    });
    $('#bankTableBody').html(tbody);
}

function populateFamilyInfoTable(data) {
    let tbody = '';
    data.forEach(item => {
        tbody += `<tr>
                            <td>${item.name}</td>
                            <td>${item.contactNo}</td>
                            <td>${item.email}</td>
                            <td>${item.relationship}</td>
                          </tr>`;
    });
    $('#familyTableBody').html(tbody);
}

function populateEducationInfoTable(data) {
    let tbody = '';
    data.forEach(item => {
        tbody += `<tr>
                            <td>${item.examTitle}</td>
                            <td>${item.major}</td>
                            <td>${item.institute}</td>
                            <td>${item.result}</td>
                            <td>${item.passYear}</td>
                            <td>${item.duration}</td>
                          </tr>`;
    });
    $('#educationTableBody').html(tbody);
}

function populateTrainingInfoTable(data) {
    let tbody = '';
    data.forEach(item => {
        tbody += `<tr>
                            <td>${item.trainingTitle}</td>
                            <td>${item.topic}</td>
                            <td>${item.institute}</td>
                            <td>${item.year}</td>
                            <td>${item.duration}</td>
                          </tr>`;
    });
    $('#trainingTableBody').html(tbody);
}

function populateExperienceInfoTable(data) {
    let tbody = '';
    data.forEach(item => {
        tbody += `<tr>
                            <td>${item.organization}</td>
                            <td>${item.jobTitle}</td>
                            <td>${item.timeDuration}</td>
                          </tr>`;
    });
    $('#experienceTableBody').html(tbody);
}

// Populate Emergency Contact Info Card
function populateEmergencyContactInfo(data) {
    // Primary Contact (first contact)
    if (data && data.length > 0) {
        $('#primaryContactName').text(data[0].name || '-');
        $('#primaryContactRelation').text(data[0].relationship || '-');
        $('#primaryContactNumber').text(data[0].contactNo || '-');
    } else {
        $('#primaryContactName').text('-');
        $('#primaryContactRelation').text('-');
        $('#primaryContactNumber').text('-');
    }

    // Secondary Contact (second contact)
    if (data && data.length > 1) {
        $('#secondaryContactName').text(data[1].name || '-');
        $('#secondaryContactRelation').text(data[1].relationship || '-');
        $('#secondaryContactNumber').text(data[1].contactNo || '-');
    } else {
        $('#secondaryContactName').text('-');
        $('#secondaryContactRelation').text('-');
        $('#secondaryContactNumber').text('-');
    }
}

//#endregion




//#region Conststants for Employee Data

const bankInfoData = [
    { bankName: 'Bank Asia', branch: 'Gulshan', accountNo: '123456789', swiftCode: 'BASIBDDH', ifscCode: 'BASI0001234' },
    { bankName: 'City Bank', branch: 'Dhanmondi', accountNo: '987654321', swiftCode: 'CIBLBDDH', ifscCode: 'CIBL0005678' },
    { bankName: 'City Bank', branch: 'Dhanmondi', accountNo: '987654321', swiftCode: 'CIBLBDDH', ifscCode: 'CIBL0005678' }
];

const familyInfoData = [
    { name: 'John Doe', contactNo: '01711111111', email: 'john@example.com', relationship: 'Father' },
    { name: 'John Doe', contactNo: '01711111111', email: 'john@example.com', relationship: 'Father' },
    { name: 'Jane Doe', contactNo: '01822222222', email: 'jane@example.com', relationship: 'Mother' }
];

const educationInfoData = [
    { examTitle: 'BSc in CSE', major: 'Computer Science', institute: 'BUET', result: '3.90', passYear: '2020', duration: '4 Years' },
    { examTitle: 'BSc in CSE', major: 'Computer Science', institute: 'BUET', result: '3.90', passYear: '2020', duration: '4 Years' },
    { examTitle: 'HSC', major: 'Science', institute: 'Dhaka College', result: '5.00', passYear: '2016', duration: '2 Years' }
];

const trainingInfoData = [
    { trainingTitle: 'Web Development', topic: 'React, Node.js', institute: 'Programming Hero', year: '2021', duration: '6 Months' },
    { trainingTitle: 'Web Development', topic: 'React, Node.js', institute: 'Programming Hero', year: '2021', duration: '6 Months' },
    { trainingTitle: 'AI Basics', topic: 'Machine Learning', institute: 'Coursera', year: '2022', duration: '3 Months' }
];

const experienceInfoData = [
    { organization: 'Tech Solutions Ltd.', jobTitle: 'Software Engineer', timeDuration: '2 Years' },
    { organization: 'Tech Solutions Ltd.', jobTitle: 'Software Engineer', timeDuration: '2 Years' },
    { organization: 'Innovative IT', jobTitle: 'Junior Developer', timeDuration: '1 Year' }
];

//#endregion