using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeEducational
{
    public class EmployeeEducationalService : IEmployeeEducationalService
    {

        private readonly IGenericRepository<EmployeeEducationalInfo> _employeeEducationalRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;

       
     
        private readonly IGenericRepository<GCTL.Data.Models.EducationLevels> _educationLevelsRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Degree> _degreeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EducationBoard> _educationBoardRepository;
        private readonly IGenericRepository<GCTL.Data.Models.ResultTypes> _resultTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.PassingYears> _passingYearRepository;


        public EmployeeEducationalService(IGenericRepository<EmployeeEducationalInfo> employeeEducationalRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EducationLevels> educationLevelsRepository, IGenericRepository<Degree> degreeRepository, IGenericRepository<EducationBoard> educationBoardRepository, IGenericRepository<ResultTypes> resultTypeRepository, IGenericRepository<PassingYears> passingYearRepository)
        {
            _employeeEducationalRepository = employeeEducationalRepository;
            _employeeRepository = employeeRepository;
            _educationLevelsRepository = educationLevelsRepository;
            _degreeRepository = degreeRepository;
            _educationBoardRepository = educationBoardRepository;
            _resultTypeRepository = resultTypeRepository;
            _passingYearRepository = passingYearRepository;
        }

        public async Task<CommonReturnViewModel> DeleteAsync(int id)
        {
            var existingInfo = await _employeeEducationalRepository.AllActive().Where(e => e.EmployeeEducationalInfoID == id).FirstOrDefaultAsync();
            if (existingInfo != null)
            {
                await _employeeEducationalRepository.DeleteAsync(id);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Educational information deleted successfully.",
                   // Data = existingInfo.EmployeeID
                };
            }
            else
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee Educational information not found.",
                    Data = null
                };

            }
        }

        public async Task<List<EmployeeEducationGetViewModel>> GetEmployeeAdditionalByIdAsync(int id)
        {
            var employee = await ( from ea in _employeeEducationalRepository.AllActive()
                                        // Left join with Employees
                                    join emp in _employeeRepository.AllActive()
                                        on ea.EmployeeID equals emp.EmployeeID into empGroup
                                    from emp in empGroup.DefaultIfEmpty()
                                        // Left join with EducationLevels
                                    join ed in _educationLevelsRepository.AllActive()
                                        on ea.EducationLevelID equals ed.EducationLevelID into edGroup
                                    from ed in edGroup.DefaultIfEmpty()
                                        // Left join with Degree
                                    join d in _degreeRepository.AllActive()
                                        on ea.DegreeID equals d.DegreeID into dGroup
                                    from d in dGroup.DefaultIfEmpty()
                                        // Left join with EducationBoard
                                    join eb in _educationBoardRepository.AllActive()
                                        on ea.EducationBoardID equals eb.EducationBoardID into ebGroup
                                    from eb in ebGroup.DefaultIfEmpty()
                                        // Left join with ResultTypes
                                    join rt in _resultTypeRepository.AllActive()
                                        on ea.ResultTypeID equals rt.ResultTypeID into rtGroup
                                    from rt in rtGroup.DefaultIfEmpty()
                                        // Left join with PassingYears
                                    join py in _passingYearRepository.AllActive()
                                        on ea.PassingYearID equals py.PassingYearID into pyGroup
                                    from py in pyGroup.DefaultIfEmpty()
                                    where ea.EmployeeID == id
                                    select new EmployeeEducationGetViewModel


                                    {
                                        EmployeeEducationalInfoID = ea.EmployeeEducationalInfoID,
                                        EmployeePersonalId = emp.EmployeeID,
                                        InstitutionName = ea.InstitutionName,
                                        EducationLevelID = ed.EducationLevelName,
                                        DegreeID = d.DegreeName,
                                        MajorSubject = ea.MajorSubject,
                                        EducationBoardID = eb.EducationBoardName,
                                        ResultTypeID = rt.ResultTypeName,
                                        PassingYearID = py.PassingYearName,
                                        YearDuration = ea.YearDuration,
                                        Achievement = ea.Achievement,
                                        PersonalEmail = emp.Email ?? "N/A",
                                        PersonalPhone = emp.MobileNumber ?? "N/A",
                                        IsActive = true,



                                    }).ToListAsync();


            if (employee != null && employee.Count() != 0)
            {
                return employee;
            }
            else
            {
                var emp = await _employeeRepository.AllActive().Where(e => e.EmployeeID == id).Select(m => new EmployeeEducationGetViewModel
                {
                    EmployeePersonalId = (int)m.EmployeeID,

                    PersonalEmail = m.Email ?? "N/A",
                    PersonalPhone = m.MobileNumber ?? "N/A",
                    IsActive = false,

                }).ToListAsync();

                return emp;
            }

        }

        public async Task<EmployeeEducationalPostViewModel> GetEmployeeEduData(int id)
        {
            var data = await _employeeEducationalRepository.AllActive().Where(e => e.EmployeeEducationalInfoID == id).Select(e => new EmployeeEducationalPostViewModel()
            {
                EmployeeEducationalInfoID = id,
                EmployeePersonalId = (int)e.EmployeeID,
                InstitutionName = e.InstitutionName,
                EducationLevelID = e.EducationLevelID,
                DegreeID = e.DegreeID,
                MajorSubject = e.MajorSubject,
                EducationBoardID = e.EducationBoardID,
                ResultTypeID = e.ResultTypeID,
                PassingYearID = e.PassingYearID,
                YearDuration = e.YearDuration,
                Achievement = e.Achievement
            }).FirstOrDefaultAsync();


            return data ?? new EmployeeEducationalPostViewModel
            {
                EmployeeEducationalInfoID = 0,
                EmployeePersonalId = 0,
                InstitutionName = string.Empty,
                EducationLevelID = null,
                DegreeID = null,
                MajorSubject = string.Empty,
                EducationBoardID = null,
                ResultTypeID = null,
                PassingYearID = null,
                YearDuration = string.Empty,
                Achievement = string.Empty
            };


        }

        public async Task<CommonReturnViewModel> SaveAsync(EmployeeEducationalPostViewModel model)
        {
            if (model == null)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = "Model cannot be null."
                };
            }


            var missingFields = new List<string>();

            string Check(object val, string displayName) => CheckRequiredField(val, displayName);

           
            missingFields.AddRange(new[]
            {
                Check(model.EmployeePersonalId, "Employee"),
                Check(model.InstitutionName, "Institution Name"),
                //Check(model.PasportNo, "Passport Number"),
                //Check(model.PasportPlaceOfIssue, "Passport Place of Issue"),
                //Check(model.PasportIssueDate, "Passport Issue Date"),
                //Check(model.PasportExpireDate, "Passport Expire Date"),
                //Check(model.DrivingLicenceNo, "Driving Licence Number"),
                //Check(model.LicenceTypeID, "Licence Type"),
                //Check(model.DrivingLicenceIssueDate, "Driving Licence Issue Date"),
                //Check(model.DrivingLicenceExpireDate, "Driving Licence Expire Date"),
                //Check(model.SymbolOfVehicleClass, "Symbol of Vehicle Class"),
                //Check(model.DrivingLicencePlaceOfIssue, "Driving Licence Place of Issue"),
                //Check(model.WorkPermaitNumber, "Work Permit Number"),
                //Check(model.WorkPermitType, "Work Permit Type"),
                //Check(model.WorkPermitEffectiveDate, "Work Permit Effective Date"),
                //Check(model.WorkPermitExpireDate, "Work Permit Expire Date"),
                //Check(model.VisaExpireDate, "Visa Expire Date")

            }.Where(x => x != null));

            if (missingFields.Any())
            {
                var message = "The following required fields are missing or invalid: " + string.Join(", ", missingFields);

                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = message
                };
            }


            try
            {

                var existingInfo = await _employeeEducationalRepository.AllActive().Where(e => e.EmployeeEducationalInfoID == model.EmployeeEducationalInfoID).FirstOrDefaultAsync();

                if (existingInfo != null)
                {

                    // Update the existing record with the new values
                    existingInfo.InstitutionName = model.InstitutionName;
                    existingInfo.EducationLevelID = model.EducationLevelID;
                    existingInfo.DegreeID = model.DegreeID;
                    existingInfo.MajorSubject = model.MajorSubject;
                    existingInfo.EducationBoardID = model.EducationBoardID;
                    existingInfo.ResultTypeID = model.ResultTypeID;
                    existingInfo.PassingYearID = model.PassingYearID;
                    existingInfo.YearDuration = model.YearDuration;
                    existingInfo.Achievement = model.Achievement;
                    

                    await _employeeEducationalRepository.UpdateAsync(existingInfo, model);

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee Educational information updated successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
                else
                {
                    var employeeEducationalInfo = new EmployeeEducationalInfo
                    {

                        EmployeeID = model.EmployeePersonalId,
                        EducationLevelID = model.EducationLevelID,
                        DegreeID = model.DegreeID,
                        MajorSubject = model.MajorSubject,
                        EducationBoardID = model.EducationBoardID,
                        InstitutionName = model.InstitutionName,
                        ResultTypeID = model.ResultTypeID,
                        PassingYearID = model.PassingYearID,
                        YearDuration = model.YearDuration,
                        Achievement = model.Achievement,
                       

                    };
                    await _employeeEducationalRepository.AddAsync(employeeEducationalInfo, model);
                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee Educational information Added successfully.",
                        Data = model.EmployeePersonalId

                    };
                }


            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while submitting the employee Educational information.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CommonReturnViewModel> UpdateAsync(EmployeeEducationalPostViewModel model)
        {
            if (model == null || model.EmployeeEducationalInfoID <= 0)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid data provided."
                };
            }
            try
            {
                var existingInfo = await _employeeEducationalRepository.AllActive().Where(e => e.EmployeeEducationalInfoID == model.EmployeeEducationalInfoID).FirstOrDefaultAsync();


                existingInfo.InstitutionName = model.InstitutionName;
                existingInfo.EducationLevelID = model.EducationLevelID;
                existingInfo.DegreeID = model.DegreeID;
                existingInfo.MajorSubject = model.MajorSubject;
                existingInfo.EducationBoardID = model.EducationBoardID;
                existingInfo.ResultTypeID = model.ResultTypeID;
                existingInfo.PassingYearID = model.PassingYearID;
                existingInfo.YearDuration = model.YearDuration;
                existingInfo.Achievement = model.Achievement;


                await _employeeEducationalRepository.UpdateAsync(existingInfo, model);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Educational information updated successfully.",
                    Data = model.EmployeePersonalId
                };
                
            }
            catch (Exception)
            {

                throw;
            }
        }

        private string CheckRequiredField(object value, string propertyDisplayName)
        {
            if (value == null)
                return propertyDisplayName;

            if (value is string str && string.IsNullOrWhiteSpace(str))
                return propertyDisplayName;

            if (value is int i && i <= 0)
                return propertyDisplayName;

            if (value is int?)
            {
                int? nullableInt = (int?)value;
                if (!nullableInt.HasValue || nullableInt.Value <= 0)
                    return propertyDisplayName;
            }

            if (value is DateTime?)
            {
                DateTime? nullableDate = (DateTime?)value;
                if (!nullableDate.HasValue)
                    return propertyDisplayName;
            }

            return null;
        }

    }
}
