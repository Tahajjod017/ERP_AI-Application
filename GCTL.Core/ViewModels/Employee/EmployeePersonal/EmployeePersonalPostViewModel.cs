using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Core.ViewModels.Employee.EmployeePersonal
{
    public class EmployeePersonalPostViewModel : BaseViewModel
    {

        [Required]
        public string FirstName { get; set; }

        public string? LastName { get; set; }


        public string? PersonalMobile { get; set; }

        public string? PersonalEmail { get; set; }
        public string? Gender { get; set; }

        public string? TinNo { get; set; }

        public string? FatherName { get; set; }

        public string? MotherName { get; set; }

        public string? Religion { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string? BirthCertificateNo { get; set; }

        public string? BloodGroup { get; set; }

        public string? Nationality { get; set; }

        public string? NationalId { get; set; }

        public string? MaritalStatus { get; set; }

        public string? AboutEmployee { get; set; }

        public int? Country { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? HouseNo { get; set; }

        public string? RoadNo { get; set; }

        public string? PostalCode { get; set; }

        public IFormFile? EmployeePicture { get; set; }

        public IFormFile? Signature { get; set; }
        public int? EmployeeId { get; set; }
        public string? EmployeeCode { get; set; }

    }

}
