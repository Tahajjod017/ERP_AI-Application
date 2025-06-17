using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace GCTL.Core.ViewModels.Employee.EmployeePersonal
{
    public class EmployeePersonalGetViewModel: BaseViewModel
    {


        public int EmployeeID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FatherName { get; set; }

        public string MotherName { get; set; }

        public string MobileNumber { get; set; }

        public string Email { get; set; }

        public string BirthCertificateNo { get; set; }

        public string TIN { get; set; }

        public DateOnly? DateOfBirth { get; set; }

        public string AboutEmployee { get; set; }

        public string NID { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string HouseNo { get; set; }

        public string RoadNo { get; set; }

        public string PostalCode { get; set; }

        public string EmployeeImageFileName { get; set; }

        public string EmployeeSignatureFileName { get; set; }

        

        public bool? HasUser { get; set; }

        public int? MaritalStatusID { get; set; }

        public int? GenderID { get; set; }

        public int? BloodGroupID { get; set; }

        public int? NationalityID { get; set; }
        public string? Nationality { get; set; }

        public int? ReligionID { get; set; }

        public int? CountryID { get; set; }

       

        public string EmployeeCode { get; set; }
    }
}
