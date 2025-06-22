using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeDetails
{
    public class EmployeeBasicDetailsViewModel
    {
    
        // Profile Information
        public string Image { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Experience { get; set; }
        public int EmployeeId { get; set; }
        public string Department { get; set; }
        public DateOnly? JoinDate { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string Address { get; set; }

        // Supervisor Information
        public string SupervisorName { get; set; }
        public string SupervisorImage { get; set; }

        // Passport Information
        public string PassportNumber { get; set; }
        public DateTime? PassportExpiryDate { get; set; }
        public string Nationality { get; set; }
        public string Religion { get; set; }
        public string MaritalStatus { get; set; }
        public string SpouseEmployment { get; set; }
        public string NumberOfChildren { get; set; }
        public string Bio { get; set; }

        public List<BankInfoDetailsViewModel> bankInfoData { get; set; }
        public List<FamilyInfoDetailsViewModel> familyInfoData { get; set; }
        public List<EducationInfoDetailsViewModel> educationInfoData { get; set; }
        public List<TrainingInfoDetailsViewModel> trainingInfoData { get; set; }
        public List<ExperienceInfoDetailsViewModel> experienceInfoData { get; set; }
        public List<EmergencyContactDetailsViewModel> emergencyContactInfoData { get; set; }
    }

    public class BankInfoDetailsViewModel
    {
        public string bankName { get; set; }
        public string branch { get; set; }
        public string accountNo { get; set; }
        public string swiftCode { get; set; }
        public string ifscCode { get; set; }
    }

    public class FamilyInfoDetailsViewModel
    {
        public string name { get; set; }
        public string contactNo { get; set; }
        public string email { get; set; }
        public string relationship { get; set; }
    }

    public class EducationInfoDetailsViewModel
    {
        public string examTitle { get; set; }
        public string major { get; set; }
        public string institute { get; set; }
        public string result { get; set; }
        public string passYear { get; set; }
        public string duration { get; set; }
    }

    public class TrainingInfoDetailsViewModel
    {
        public string trainingTitle { get; set; }
        public string topic { get; set; }
        public string institute { get; set; }
        public string year { get; set; }
        public string duration { get; set; }
    }

    public class ExperienceInfoDetailsViewModel
    {
        public string organization { get; set; }
        public string jobTitle { get; set; }
        public string timeDuration { get; set; }
    
    
    }

    public class EmergencyContactDetailsViewModel
    {
        public string name { get; set; }
        public string contactNo { get; set; }
        public string email { get; set; }
        public string relationship { get; set; }
    }

}
