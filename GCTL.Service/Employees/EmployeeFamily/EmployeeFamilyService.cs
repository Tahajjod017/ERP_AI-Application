using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeFamily;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeFamily
{
    public class EmployeeFamilyService : IEmployeeFamilyService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeFamilyInfo> _employeeFamilyInfoRepository;

        public EmployeeFamilyService(IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeFamilyInfo> employeeFamilyInfoRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeFamilyInfoRepository = employeeFamilyInfoRepository;
        }

        public async Task<CommonReturnViewModel> DeleteAsync(int id)
        {
            var existingInfo = await _employeeFamilyInfoRepository.AllActive()
                .Where(e => e.EmployeeFamilyInfoID == id)
                .FirstOrDefaultAsync();
            if (existingInfo != null)
            {
                await _employeeFamilyInfoRepository.DeleteAsync(id);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee family information deleted successfully.",
                    Data = existingInfo.EmployeeID
                };
            }
            else
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee family information not found.",
                    Data = null
                };
            }
        }

        public async Task<List<EmployeeFamilyGetViewModel>> GetEmployeeFamilyByIdAsync(int id)
        {
            var employee = await (from ef in _employeeFamilyInfoRepository.AllActive()
                                  join emp in _employeeRepository.AllActive()
                                      on ef.EmployeeID equals emp.EmployeeID into empGroup
                                  from emp in empGroup.DefaultIfEmpty()
                                  where ef.EmployeeID == id
                                  select new EmployeeFamilyGetViewModel
                                  {
                                      EmployeeFamilyInfoID = ef.EmployeeFamilyInfoID,
                                      EmployeePersonalId = (int)ef.EmployeeID,
                                      FullName = ef.FullName ?? "",
                                      RelationToEmployee = ef.RelationToEmployee ?? "",
                                      Occupation = ef.Occupation ?? "",
                                      ContactNumber = ef.ContactNumber ?? "",
                                      Email = ef.Email ?? "",
                                      Address = ef.Address ?? "",
                                      PersonalEmail = emp.Email ?? "",
                                      PersonalPhone = emp.MobileNumber ?? "",
                                      IsActive = true
                                  }).ToListAsync();

            if (employee != null && employee.Any())
            {
                return employee;
            }
            else
            {
                var emp = await _employeeRepository.AllActive()
                    .Where(e => e.EmployeeID == id)
                    .Select(m => new EmployeeFamilyGetViewModel
                    {
                        EmployeePersonalId = (int)m.EmployeeID,
                        PersonalEmail = m.Email ?? "N/A",
                        PersonalPhone = m.MobileNumber ?? "N/A",
                        IsActive = false
                    }).ToListAsync();

                return emp;
            }
        }

        public async Task<EmployeeFamilyPostViewModel> GetEmployeeFamilyData(int id)
        {
            var data = await _employeeFamilyInfoRepository.AllActive()
                .Where(e => e.EmployeeFamilyInfoID == id)
                .Select(e => new EmployeeFamilyPostViewModel
                {
                    EmployeeFamilyInfoID = e.EmployeeFamilyInfoID,
                    EmployeePersonalId = (int)e.EmployeeID,
                    FullName = e.FullName,
                    RelationToEmployee = e.RelationToEmployee,
                    Occupation = e.Occupation,
                    ContactNumber = e.ContactNumber,
                    Email = e.Email,
                    Address = e.Address
                }).FirstOrDefaultAsync();

            return data ?? new EmployeeFamilyPostViewModel
            {
                EmployeeFamilyInfoID = 0,
                EmployeePersonalId = 0,
                FullName = string.Empty,
                RelationToEmployee = string.Empty,
                Occupation = string.Empty,
                ContactNumber = string.Empty,
                Email = string.Empty,
                Address = string.Empty
            };
        }

        public async Task<CommonReturnViewModel> SaveAsync(EmployeeFamilyPostViewModel model)
        {
            if (model == null)
            {
                return new CommonReturnViewModel
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
                Check(model.FullName, "Full Name")
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
                var existingInfo = await _employeeFamilyInfoRepository.AllActive()
                    .Where(e => e.EmployeeFamilyInfoID == model.EmployeeFamilyInfoID)
                    .FirstOrDefaultAsync();

                if (existingInfo != null)
                {
                    existingInfo.EmployeeID = model.EmployeePersonalId;
                    existingInfo.FullName = model.FullName;
                    existingInfo.RelationToEmployee = model.RelationToEmployee;
                    existingInfo.Occupation = model.Occupation;
                    existingInfo.ContactNumber = model.ContactNumber;
                    existingInfo.Email = model.Email;
                    existingInfo.Address = model.Address;

                    await _employeeFamilyInfoRepository.UpdateAsync(existingInfo, model);

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee family information updated successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
                else
                {
                    var employeeFamilyInfo = new EmployeeFamilyInfo
                    {
                        EmployeeID = model.EmployeePersonalId,
                        FullName = model.FullName,
                        RelationToEmployee = model.RelationToEmployee,
                        Occupation = model.Occupation,
                        ContactNumber = model.ContactNumber,
                        Email = model.Email,
                        Address = model.Address
                    };
                    await _employeeFamilyInfoRepository.AddAsync(employeeFamilyInfo, model);
                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee family information added successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while submitting the employee family information.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CommonReturnViewModel> UpdateAsync(EmployeeFamilyPostViewModel model)
        {
            try
            {
                if (model == null || model.EmployeeFamilyInfoID <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid data"
                    };
                }

                var existingInfo = await _employeeFamilyInfoRepository.AllActive()
                    .Where(e => e.EmployeeFamilyInfoID == model.EmployeeFamilyInfoID)
                    .FirstOrDefaultAsync();
                if (existingInfo == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee family information not found.",
                        Data = null
                    };
                }

                existingInfo.FullName = model.FullName;
                existingInfo.RelationToEmployee = model.RelationToEmployee;
                existingInfo.Occupation = model.Occupation;
                existingInfo.ContactNumber = model.ContactNumber;
                existingInfo.Email = model.Email;
                existingInfo.Address = model.Address;

                await _employeeFamilyInfoRepository.UpdateAsync(existingInfo, model);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee family information updated successfully.",
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

            return null;
        }
    }
}
