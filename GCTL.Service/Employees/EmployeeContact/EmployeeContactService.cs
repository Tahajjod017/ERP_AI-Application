using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeContact;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeContact
{
    public class EmployeeContactService : IEmployeeContactService
    {
        private readonly IGenericRepository<EmployeeEmeContacts> _employeeContactInfoRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;

        public EmployeeContactService(IGenericRepository<EmployeeEmeContacts> employeeContactInfoRepository, IGenericRepository<Data.Models.Employees> employeeRepository)
        {
            _employeeContactInfoRepository = employeeContactInfoRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<CommonReturnViewModel> DeleteAsync(int id)
        {
            var existingInfo = await _employeeContactInfoRepository.AllActive()
                .Where(e => e.EmployeeEmeContactID == id)
                .FirstOrDefaultAsync();
            if (existingInfo != null)
            {
                await _employeeContactInfoRepository.DeleteAsync(id);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee emergency contact deleted successfully."
                };
            }
            else
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee emergency contact not found.",
                    Data = null
                };
            }
        }

        public async Task<List<EmployeeContactGetViewModel>> GetEmployeeContactByIdAsync(int id)
        {
            var employee = await (from ec in _employeeContactInfoRepository.AllActive()
                                  join emp in _employeeRepository.AllActive()
                                      on ec.EmployeeID equals emp.EmployeeID into empGroup
                                  from emp in empGroup.DefaultIfEmpty()
                                  where ec.EmployeeID == id
                                  select new EmployeeContactGetViewModel
                                  {
                                      EmployeeEmeContactID = ec.EmployeeEmeContactID,
                                      EmployeePersonalId = (int)ec.EmployeeID,
                                      ContactName = ec.ContactName,
                                      Relationship = ec.Relationship,
                                      ContactNumber = ec.ContactNumber,
                                      ContactEmail = ec.ContactEmail,
                                      PersonalEmail = emp.Email ?? "N/A",
                                      PersonalPhone = emp.MobileNumber ?? "N/A",
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
                    .Select(m => new EmployeeContactGetViewModel
                    {
                        EmployeePersonalId = (int)m.EmployeeID,
                        PersonalEmail = m.Email ?? "N/A",
                        PersonalPhone = m.MobileNumber ?? "N/A",
                        IsActive = false
                    }).ToListAsync();

                return emp;
            }
        }

        public async Task<EmployeeContactViewModel> GetEmployeeContactData(int id)
        {
            var data = await _employeeContactInfoRepository.AllActive()
                .Where(e => e.EmployeeEmeContactID == id)
                .Select(e => new EmployeeContactViewModel
                {
                    EmployeeEmeContactID = e.EmployeeEmeContactID,
                    EmployeePersonalId = (int)e.EmployeeID,
                    ContactName = e.ContactName,
                    Relationship = e.Relationship,
                    ContactNumber = e.ContactNumber,
                    ContactEmail = e.ContactEmail
                }).FirstOrDefaultAsync();

            return data ?? new EmployeeContactViewModel
            {
                EmployeeEmeContactID = 0,
                EmployeePersonalId = 0,
                ContactName = string.Empty,
                Relationship = string.Empty,
                ContactNumber = string.Empty,
                ContactEmail = string.Empty
            };
        }

        public async Task<CommonReturnViewModel> SaveAsync(EmployeeContactViewModel model)
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
                Check(model.ContactName, "Contact Name"),
                Check(model.Relationship, "Relationship"),
                Check(model.ContactNumber, "Contact Number")
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
                var existingInfo = await _employeeContactInfoRepository.AllActive()
                    .Where(e => e.EmployeeEmeContactID == model.EmployeeEmeContactID)
                    .FirstOrDefaultAsync();

                if (existingInfo != null)
                {
                    existingInfo.EmployeeID = model.EmployeePersonalId;
                    existingInfo.ContactName = model.ContactName;
                    existingInfo.Relationship = model.Relationship;
                    existingInfo.ContactNumber = model.ContactNumber;
                    existingInfo.ContactEmail = model.ContactEmail;

                    await _employeeContactInfoRepository.UpdateAsync(existingInfo, model);

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee emergency contact updated successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
                else
                {
                    var employeeContactInfo = new EmployeeEmeContacts
                    {
                        EmployeeID = model.EmployeePersonalId,
                        ContactName = model.ContactName,
                        Relationship = model.Relationship,
                        ContactNumber = model.ContactNumber,
                        ContactEmail = model.ContactEmail
                    };
                    await _employeeContactInfoRepository.AddAsync(employeeContactInfo, model);
                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee emergency contact added successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while submitting the employee emergency contact.",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<CommonReturnViewModel> UpdateAsync(EmployeeContactViewModel model)
        {
            try
            {
                if (model == null || model.EmployeeEmeContactID <= 0)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid data"
                    };
                }

                var existingInfo = await _employeeContactInfoRepository.AllActive()
                    .Where(e => e.EmployeeEmeContactID == model.EmployeeEmeContactID)
                    .FirstOrDefaultAsync();
                if (existingInfo == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee emergency contact not found.",
                        Data = null
                    };
                }

                existingInfo.ContactName = model.ContactName;
                existingInfo.Relationship = model.Relationship;
                existingInfo.ContactNumber = model.ContactNumber;
                existingInfo.ContactEmail = model.ContactEmail;

                await _employeeContactInfoRepository.UpdateAsync(existingInfo, model);
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee emergency contact updated successfully.",
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
