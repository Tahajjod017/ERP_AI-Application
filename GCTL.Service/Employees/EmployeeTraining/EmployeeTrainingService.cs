using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeEducational;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using GCTL.Core.Repository;
using Microsoft.EntityFrameworkCore;
using GCTL.Core.ViewModels.Employee.EmployeeTraining;

namespace GCTL.Service.Employees.EmployeeTraining
{
    public class EmployeeTrainingService : IEmployeeTrainingService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeTranningInfo> _employeeTranningInfoRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<TrainingYears> _trainingYearsRepository;

        public EmployeeTrainingService(IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeTranningInfo> employeeTranningInfoRepository, IGenericRepository<Country> countryRepository, IGenericRepository<TrainingYears> trainingYearsRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeTranningInfoRepository = employeeTranningInfoRepository;
            _countryRepository = countryRepository;
            _trainingYearsRepository = trainingYearsRepository;
        }

        public async Task<CommonReturnViewModel> DeleteAsync(int id)
        {
            var existingInfo = await _employeeTranningInfoRepository.AllActive().Where(e => e.EmployeeTranningInfoID == id).FirstOrDefaultAsync();
            if (existingInfo != null)
            {
                await _employeeTranningInfoRepository.DeleteAsync(id);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Training information deleted successfully.",
                    Data = existingInfo.EmployeeID
                };
            }
            else
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee additional information not found.",
                    Data = null
                };

            }
        }

        public async Task<List<EmployeeTrainingGetViewModel>> GetEmployeeTrainingByIdAsync(int id)
        {
            var employee = await (from ea in _employeeTranningInfoRepository.AllActive()
                                     
                                  join emp in _employeeRepository.AllActive()
                                      on ea.EmployeeID equals emp.EmployeeID into empGroup
                                  from emp in empGroup.DefaultIfEmpty()
                                     
                                  join country in _countryRepository.AllActive()
                                      on ea.CountryID equals country.CountryID into countryGroup
                                  from country in countryGroup.DefaultIfEmpty()

                                  join trainingYear in _trainingYearsRepository.AllActive()
                                      on ea.TrainingYearID equals trainingYear.TrainingYearID into trainingYearGroup
                                  from trainingYear in trainingYearGroup.DefaultIfEmpty()

                                  where ea.EmployeeID == id
                                  select new EmployeeTrainingGetViewModel
                                  {
                                      EmployeeTranningInfoID = ea.EmployeeTranningInfoID,
                                      EmployeePersonalId = (int)ea.EmployeeID,
                                      TranningTitle = ea.TranningTitle,
                                      CountryID = country.CountryName,
                                      TopicCovered = ea.TopicCovered,
                                      TrainingYearID = trainingYear.TrainingYearName,
                                      InstituteName = ea.InstituteName,
                                      YearDuration = ea.YearDuration,
                                      LocationName = ea.LocationName,

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
                var emp = await _employeeRepository.AllActive().Where(e => e.EmployeeID == id).Select(m => new EmployeeTrainingGetViewModel
                {
                    EmployeePersonalId = (int)m.EmployeeID,

                    PersonalEmail = m.Email ?? "N/A",
                    PersonalPhone = m.MobileNumber ?? "N/A",
                    IsActive = false,
                }).ToListAsync();

                return emp;
            }

        }

        public async Task<EmployeeTrainingPostViewModel> GetEmployeeEduData(int id)
        {
            var data = await _employeeTranningInfoRepository.AllActive().Where(e => e.EmployeeTranningInfoID == id).Select(e => new EmployeeTrainingPostViewModel()
            {
                EmployeeTranningInfoID = e.EmployeeTranningInfoID,
                EmployeePersonalId = (int)e.EmployeeID,
                InstituteName = e.InstituteName,
                TranningTitle = e.TranningTitle,
                CountryID = e.CountryID,
                TopicCovered = e.TopicCovered,
                TrainingYearID = e.TrainingYearID,
                YearDuration = e.YearDuration,
                LocationName = e.LocationName,
              

            }).FirstOrDefaultAsync();


            return data ?? new EmployeeTrainingPostViewModel
            {
                EmployeeTranningInfoID = 0,
                EmployeePersonalId = 0,
                InstituteName = string.Empty,
                TranningTitle = string.Empty,
                CountryID = null,
                TopicCovered = string.Empty,
                TrainingYearID = null,
                YearDuration = string.Empty,
                LocationName = string.Empty
            };


        }

        public async Task<CommonReturnViewModel> SaveAsync(EmployeeTrainingPostViewModel model)
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
                Check(model.TranningTitle, "Tranning Title Name"),
                

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

                var existingInfo = await _employeeTranningInfoRepository.AllActive().Where(e => e.EmployeeTranningInfoID == model.EmployeeTranningInfoID).FirstOrDefaultAsync();

                if (existingInfo != null)
                {

                    existingInfo.EmployeeID = model.EmployeePersonalId;
                    existingInfo.InstituteName = model.InstituteName;
                    existingInfo.TranningTitle = model.TranningTitle;
                    existingInfo.CountryID = model.CountryID;
                    existingInfo.TopicCovered = model.TopicCovered;
                    existingInfo.TrainingYearID = model.TrainingYearID;
                    existingInfo.YearDuration = model.YearDuration;
                    existingInfo.LocationName = model.LocationName;


                    await _employeeTranningInfoRepository.UpdateAsync(existingInfo, model);

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee additional information updated successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
                else
                {
                    var employeeEducationalInfo = new EmployeeTranningInfo
                    {

                        EmployeeID = model.EmployeePersonalId,
                        InstituteName = model.InstituteName,
                        TranningTitle = model.TranningTitle,
                        CountryID = model.CountryID,
                        TopicCovered = model.TopicCovered,
                        TrainingYearID = model.TrainingYearID,
                        YearDuration = model.YearDuration,
                        LocationName = model.LocationName,
                       
                    };
                    await _employeeTranningInfoRepository.AddAsync(employeeEducationalInfo, model);
                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee additional information Added successfully.",
                        Data = model.EmployeePersonalId

                    };
                }


            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while submitting the employee additional information.",
                    Errors = new List<string> { ex.Message }
                };
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

        public async Task<CommonReturnViewModel> UpdateAsync(EmployeeTrainingPostViewModel model)
        {
            try
            {
                if (model == null || model.EmployeeTranningInfoID <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid data"
                    };
                }

                var existingInfo = await _employeeTranningInfoRepository.AllActive().Where(e => e.EmployeeTranningInfoID == model.EmployeeTranningInfoID).FirstOrDefaultAsync();
                if (existingInfo == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee training information not found.",
                        Data = null
                    };
                }
                //existingInfo.EmployeeID = model.EmployeePersonalId;
                existingInfo.InstituteName = model.InstituteName;
                existingInfo.TranningTitle = model.TranningTitle;
                existingInfo.CountryID = model.CountryID;
                existingInfo.TopicCovered = model.TopicCovered;
                existingInfo.TrainingYearID = model.TrainingYearID;
                existingInfo.YearDuration = model.YearDuration;
                existingInfo.LocationName = model.LocationName;

                await _employeeTranningInfoRepository.UpdateAsync(existingInfo, model);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Training information updated successfully.",
                    Data = model.EmployeePersonalId
                };
            }
            catch (Exception)
            {

                throw;
            }
            
        }
    }
}
