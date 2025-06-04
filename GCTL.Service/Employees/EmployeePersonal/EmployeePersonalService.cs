using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeePersonal;

using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using GCTL.Core.ServiceExtensions;
using System.Text.RegularExpressions;


namespace GCTL.Service.Employees.EmployeePersonal
{
    public class EmployeePersonalService : IEmployeePersonalService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeePersonalRepository;

        public EmployeePersonalService(IGenericRepository<GCTL.Data.Models.Employees> employeePersonalRepository)
        {
            _employeePersonalRepository = employeePersonalRepository;
        }

        #region Save Method
        public async Task<CommonReturnViewModel> SaveEmployeePersonalInfo(EmployeePersonalPostViewModel model)
        {
            var result = new CommonReturnViewModel();

            


            try
            {
                if (model == null)
                {
                    result.Success = false;
                    result.Message = "Invalid input data.";
                    return result;
                }

                


                GCTL.Data.Models.Employees employee;

                string EmployeeImageFileName = "";
                string EmployeeSignatureFileName = "";

                if (model.EmployeePicture != null)
                {
                     EmployeeImageFileName = await SaveFileAsync(model.EmployeePicture, "uploads/employee/images");
                }

                if (model.Signature != null)
                {
                     EmployeeSignatureFileName = await SaveFileAsync(model.Signature, "uploads/employee/signatures");
                }
                
                


                if (model.EmployeeId == 0 || model.EmployeeId == null)
                {
                    // New employee
                    employee = new GCTL.Data.Models.Employees
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        FatherName = model.FatherName,
                        MotherName = model.MotherName,
                        MobileNumber = model.PersonalMobile,
                        Email = model.PersonalEmail,
                        TIN = model.TinNo,

                        DateOfBirth =  model.DateOfBirth.ToDateOnly(),
                        AboutEmployee = model.AboutEmployee,
                        NID = model.NationalId,
                        State = model.State,
                        City = model.City,
                        HouseNo = model.HouseNo,
                        RoadNo = model.RoadNo,
                        PostalCode = model.PostalCode,
                        EmployeeImageFileName = EmployeeImageFileName,
                        EmployeeSignatureFileName = EmployeeSignatureFileName,
                        MaritalStatusID = Convert.ToInt32(model.MaritalStatus),
                        GenderID = Convert.ToInt32(model.Gender),
                        BloodGroupID = Convert.ToInt32(model.BloodGroup),
                        NationalityID = Convert.ToInt32(model.NationalId),
                        ReligionID = Convert.ToInt32(model.Religion),
                        CountryID = Convert.ToInt32(model.Country),
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy
                    };


                    if (employee.BloodGroupID == 0) employee.BloodGroupID = null;
                    if (employee.GenderID == 0) employee.GenderID = null;
                    if (employee.MaritalStatusID == 0) employee.MaritalStatusID = null;
                    if (employee.NationalityID == 0) employee.NationalityID = null;
                    if (employee.ReligionID == 0) employee.ReligionID = null;
                    if (employee.CountryID == 0) employee.CountryID = null;



                    await _employeePersonalRepository.AddAsync(employee);
                }
                else
                {
                    DateTime? nullableDateTime = DateTime.Now;
                    DateOnly? dateOnly = nullableDateTime.ToDateOnly();

                    employee = await _employeePersonalRepository.AllActive().FirstOrDefaultAsync(e=>e.EmployeeID == model.EmployeeId);

                    if (employee == null)
                    {
                        result.Success = false;
                        result.Message = "Employee not found.";
                        return result;
                    }

                    employee.FirstName = model.FirstName;
                    employee.LastName = model.LastName;
                    employee.FatherName = model.FatherName;
                    employee.MotherName = model.MotherName;
                    employee.MobileNumber = model.PersonalMobile;
                    employee.Email = model.PersonalEmail;
                    employee.TIN = model.TinNo;
                    employee.DateOfBirth = model.DateOfBirth.ToDateOnly();
                    employee.AboutEmployee = model.AboutEmployee;
                    employee.NID = model.NationalId;
                    employee.State = model.State;
                    employee.City = model.City;
                    employee.HouseNo = model.HouseNo;
                    employee.RoadNo = model.RoadNo;
                    employee.PostalCode = model.PostalCode;
                    employee.EmployeeImageFileName = EmployeeImageFileName;
                    employee.EmployeeSignatureFileName = EmployeeSignatureFileName;
                    employee.MaritalStatusID = Convert.ToInt32(model.MaritalStatus);
                    employee.GenderID = Convert.ToInt32(model.Gender);
                    employee.BloodGroupID = Convert.ToInt32(model.BloodGroup);
                    employee.NationalityID = Convert.ToInt32(model.NationalId);
                    employee.ReligionID = Convert.ToInt32(model.Religion);
                    employee.CountryID = Convert.ToInt32(model.Country);
                    employee.UpdatedAt = DateTime.UtcNow;
                    employee.UpdatedBy = model.UpdatedBy;

                    await _employeePersonalRepository.UpdateAsync(employee);
                }

               

                result.Success = true;
                result.Message = "Employee personal information saved successfully.";
                result.Data = employee.EmployeeID;
                return result;
            }
            catch (Exception ex)
            {
                // Ideally log the error (e.g., with ILogger)
                result.Success = false;
                result.Message = $"An error occurred: {ex.Message}";
                return result;
            }
        }


        private async Task<string> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path or full URL depending on preference
            //return Path.Combine(folderName, fileName).Replace("\\", "/");
            return fileName;
        }



        #endregion


        #region Validation


        public async Task<CommonReturnViewModel> CheckValidEmployeeInfo(EmployeePersonalPostViewModel model)
        {
            var result = new CommonReturnViewModel();


            result.Success = true;


            if (
                
                string.IsNullOrEmpty(model.FirstName) ||
                string.IsNullOrEmpty(model.LastName) ||
               
                string.IsNullOrEmpty(model.PersonalMobile) ||
                string.IsNullOrEmpty(model.PersonalEmail)
               

               )
            {
                result.Success = false;
                result.Message = "Please fill out the form.";
            }
            else if (!IsValidEmail(model.PersonalEmail))
            {
                result.Success = false;
                result.Message = "Please enter a valid email address.";
            }
            //else if(model.EmployeePicture.Length > 1048576) // 1MB
            //{
            //    result.Success = false;
            //    result.Message = "Employee Picture size should not exceed 1MB.";
            //}
            //else if (model.Signature.Length > 1048576) // 1MB
            //{
            //    result.Success = false;
            //    result.Message = "Signature size should not exceed 1MB.";
            //}
            //else if(model.EmployeePicture.ContentType != "image/jpeg" && model.EmployeePicture.ContentType != "image/png")
            //{
            //    result.Success = false;
            //    result.Message = "Employee Picture should be in JPEG or PNG format.";
            //}
            //else if (model.Signature.ContentType != "image/jpeg" && model.Signature.ContentType != "image/png")
            //{
            //    result.Success = false;
            //    result.Message = "Signature should be in JPEG or PNG format.";
            //}
            else if (CheckDuplicateFields(model, out string duplicateMessage))
            {
                result.Success = false;
                result.Message = duplicateMessage;
            }
            else
            {
                result.Success = true;
                result.Message = "Employee information is valid.";
            }

            return result;

        }

        private bool CheckDuplicateFields(EmployeePersonalPostViewModel model, out string duplicateMessage)
        {
            List<string> duplicateFields = new List<string>();

            var allEmployees = _employeePersonalRepository.AllActive();

            if (model.EmployeeId != null || model.EmployeeId != 0)
            {
                allEmployees = allEmployees.Where(e => e.EmployeeID != model.EmployeeId);
            }

            if (!string.IsNullOrWhiteSpace(model.PersonalEmail) &&
                allEmployees.Any(e => e.Email == model.PersonalEmail))
            {
                duplicateFields.Add("Personal UserEmail");
            }

            if (!string.IsNullOrWhiteSpace(model.PersonalMobile) &&
                allEmployees.Any(e => e.MobileNumber == model.PersonalMobile))
            {
                duplicateFields.Add("Mobile Number");
            }

            if (!string.IsNullOrWhiteSpace(model.TinNo) &&
                allEmployees.Any(e => e.TIN == model.TinNo))
            {
                duplicateFields.Add("TIN");
            }

            if (!string.IsNullOrWhiteSpace(model.NationalId) &&
                allEmployees.Any(e => e.NID == model.NationalId))
            {
                duplicateFields.Add("National ID");
            }

            if (!string.IsNullOrWhiteSpace(model.BirthCertificateNo) &&
                allEmployees.Any(e => e.BirthCertificateNo == model.BirthCertificateNo))
            {
                duplicateFields.Add("Birth Certificate No");
            }

            if (duplicateFields.Any())
            {
                duplicateMessage = $"{string.Join(", ", duplicateFields)} already exist.";
                return true;
            }

            duplicateMessage = null;
            return false;
        }


        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[\w\.-]+@[\w\.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }

        #endregion
    }
}
