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
    }
}
