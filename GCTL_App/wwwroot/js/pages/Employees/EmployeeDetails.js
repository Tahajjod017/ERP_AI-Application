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
               
                populateBaseInfo(response.data)
                
            },
            error: function (xhr, status, error) {
                console.error("Error fetching employee data:", error);
                alert("Failed to load employee data. Please try again later.");
            }
        });
    }

    //#endregion

    
});

//#region Populate all
function populateEmployeeData(employeeData) {
    populateProfile(employeeData.profile);
    populateSupervisor(employeeData.supervisor);
    populatePassport(employeeData.passport);
    populateEmergencyContacts(employeeData.emergencyContacts);
    populateBio(employeeData.bio);
    populateBankInfo(employeeData.bankInfo);
    populateFamilyInfo(employeeData.familyInfo);
    populateEducationInfo(employeeData.educationInfo);
    populateTrainingInfo(employeeData.trainingInfo);
    populateExperienceInfo(employeeData.experienceInfo);
}



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



function populateEmergencyContacts(contacts) {
    if (contacts.length > 0) {
        $("#primaryContactName").text(contacts[0].name);
        $("#primaryContactRelation").text(contacts[0].relation);
        $("#primaryContactNumber").text(contacts[0].number);
    }
    if (contacts.length > 1) {
        $("#secondaryContactName").text(contacts[1].name);
        $("#secondaryContactRelation").text(contacts[1].relation);
        $("#secondaryContactNumber").text(contacts[1].number);
    }
}

function populateBio(bio) {
}

function populateBankInfo(bank) {
    $("#bankName").text(bank.name);
    $("#bankBranch").text(bank.branch);
    $("#bankAccountNo").text(bank.accountNo);
    $("#bankSwiftCode").text(bank.swiftCode);
    $("#bankIfscCode").text(bank.ifscCode);
}

function populateFamilyInfo(familyInfo) {
    const familyTableBody = $("#familyTableBody");
    familyTableBody.empty();
    familyInfo.forEach((family, index) => {
        familyTableBody.append(`
            <tr>
                <th id="familyName${index + 1}">${family.name}</th>
                <td id="familyContact${index + 1}">${family.contact}</td>
                <td id="familyEmail${index + 1}">${family.email}</td>
                <td id="familyRelation${index + 1}">${family.relation}</td>
            </tr>
        `);
    });
}

function populateEducationInfo(educationInfo) {
    const educationTableBody = $("#educationTableBody");
    educationTableBody.empty();
    educationInfo.forEach((edu, index) => {
        educationTableBody.append(`
            <tr>
                <th id="eduTitle${index + 1}">${edu.title}</th>
                <td id="eduMajor${index + 1}">${edu.major}</td>
                <td id="eduInstitute${index + 1}">${edu.institute}</td>
                <td id="eduResult${index + 1}">${edu.result}</td>
                <td id="eduYear${index + 1}">${edu.year}</td>
                <td id="eduDuration${index + 1}">${edu.duration}</td>
            </tr>
        `);
    });
}

function populateTrainingInfo(trainingInfo) {
    const trainingTableBody = $("#trainingTableBody");
    trainingTableBody.empty();
    trainingInfo.forEach((train, index) => {
        trainingTableBody.append(`
            <tr>
                <th id="trainTitle${index + 1}">${train.title}</th>
                <td id="trainTopic${index + 1}">${train.topic}</td>
                <td id="trainInstitute${index + 1}">${train.institute}</td>
                <td id="trainYear${index + 1}">${train.year}</td>
                <td id="trainDuration${index + 1}">${train.duration}</td>
            </tr>
        `);
    });
}

function populateExperienceInfo(experienceInfo) {
    const experienceTableBody = $("#experienceTableBody");
    experienceTableBody.empty();
    experienceInfo.forEach((exp, index) => {
        experienceTableBody.append(`
            <tr>
                <th id="expOrg${index + 1}">${exp.organization}</th>
                <td id="expTitle${index + 1}">${exp.jobTitle}</td>
                <td id="expDuration${index + 1}">${exp.duration}</td>
            </tr>
        `);
    });
}


//#endregion


//#region Conststants for Employee Data

const profileInfo = {
    image: "../../../assets/img/users/user-13.jpg",
    name: "Nazib Uddin",
    role: "Designer",
    experience: "5+ years Experience",
    employeeId: "CLT-0024",
    department: "UI/UX Design",
    joinDate: "1st Jan 2023",
    phone: "+880 01723 259 315",
    email: "perralt12@example.com",
    gender: "Male",
    dateOfBirth: "24th July 2000",
    address: "1861 Bayonne Ave, Manchester, NJ, 08759"
};

const supervisorInfo = {
    image: "../../../assets/img/users/user-13.jpg",
    name: "Jueal Rana"
};

const passportInfo = {
    number: "QRET4566FGRT",
    expiryDate: "15 May 2029",
    nationality: "Bangladeshi",
    religion: "Islam",
    maritalStatus: "Yes",
    spouseEmployment: "No",
    numberOfChildren: "2"
};

const emergencyContacts = [
    {
        name: "A. K. Azad",
        relation: "Father",
        number: "01989 2685 598"
    },
    {
        name: "Rabia Khatun",
        relation: "Spouse",
        number: "01989 7774 787"
    }
];

const bioInfo = "As an award-winning designer, I deliver exceptional quality work and bring value to your brand!...";

const bankInfo = {
    name: "Islami Bank",
    branch: "Mirput 2",
    accountNo: "20059800034",
    swiftCode: "WS345",
    ifscCode: "Ms45"
};

const familyInfo = [
    {
        name: "A. K. Azad",
        contact: "0176 023 102",
        email: "aka@mail.com",
        relation: "Father"
    },
    {
        name: "Rabia Khatun",
        contact: "0176 023 102",
        email: "rabia@mail.com",
        relation: "Spouse"
    }
];

const educationInfo = [
    {
        title: "Bachelor of Science (BSc)",
        major: "Computer Science & Engineering",
        institute: "Dhaka International University",
        result: "CGPA:3.94 (out of 4)", year: "2018",
        duration: "4"
    },
    {
        title: "Diploma",
        major: "Computer Technology",
        institute: "Jhenaidah Govt Polytechnic Institute",
        result: "CGPA:3.79 (out of 4)",
        year: "2014",
        duration: "4"
    }
];

const trainingInfo = [
    {
        title: "Data Science Masters Pro",
        topic: "Python,NLP, LLM, CV, ML, DL",
        institute: "Inuron",
        year: "2023",
        duration: "1 year"
    },
    {
        title: "Professional Diploma In Games Development",
        topic: "2D Physics,3D,AR,VR.",
        institute: "Bangladesh Computer Council",
        year: "2018",
        duration: "1 year"
    }
];

const experienceInfo = [
    {
        organization: "Google",
        jobTitle: "UI/UX Designer",
        duration: "Jan 2013 - Present"
    },
    {
        organization: "Facebook",
        jobTitle: "Graphics Designer",
        duration: "Dec 2012 - Jan 2015"
    }
];

// Main Employee Data (combining all above)
const employeeData = {
    profile: profileInfo,
    supervisor: supervisorInfo,
    passport: passportInfo,
    emergencyContacts: emergencyContacts,
    bio: bioInfo,
    bankInfo: bankInfo,
    familyInfo: familyInfo,
    educationInfo: educationInfo,
    trainingInfo: trainingInfo,
    experienceInfo: experienceInfo
};

//#endregion