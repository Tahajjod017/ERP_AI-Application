using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeAdditional;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeAdditional
{
    public class EmployeeAdditionalService : IEmployeeAdditionalService
    {
        public IGenericRepository<EmployeeAdditionalInfo> _employeeAdditionalRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;


        public EmployeeAdditionalService(IGenericRepository<EmployeeAdditionalInfo> employeeAdditionalRepository, IGenericRepository<Data.Models.Employees> employeeRepository)
        {
            _employeeAdditionalRepository = employeeAdditionalRepository;
            _employeeRepository = employeeRepository;
        }

        #region Get By Id

        public async Task<EmployeeAdditionalPostViewModel> GetEmployeeAdditionalByIdAsync(int employeeId)
        {

            var employee = await (from ea in _employeeAdditionalRepository.AllActive()
                                   join emp in _employeeRepository.AllActive()
                                   on ea.EmployeeID equals emp.EmployeeID into empGroup // First left join
                                   from emp in empGroup.DefaultIfEmpty()

                                   

                                   where ea.EmployeeID == employeeId

                                   select new EmployeeAdditionalPostViewModel
                                   {
                                       EmployeeAdditionalInfoID = ea.EmployeeAdditionalInfoID,
                                       EmployeePersonalId = emp.EmployeeID,
                                       PersonalEmail = emp.Email,
                                       PersonalPhone = emp.MobileNumber,
                                       PasportName = ea.PasportName,
                                       PasportNo = ea.PasportNo,
                                       PasportPlaceOfIssue = ea.PasportPlaceOfIssue,
                                       PasportIssueDate = ea.PasportIssueDate,
                                       PasportExpireDate = ea.PasportExpireDate,
                                       DrivingLicenceNo = ea.DrivingLicenceNo,
                                       LicenceTypeID = ea.LicenceTypeID,
                                       DrivingLicenceIssueDate = ea.DrivingLicenceIssueDate,
                                       DrivingLicenceExpireDate = ea.DrivingLicenceExpireDate,
                                       SymbolOfVehicleClass = ea.SymbolOfVehicleClass,
                                       DrivingLicencePlaceOfIssue = ea.DrivingLicencePlaceOfIssue,
                                       WorkPermaitNumber = ea.WorkPermaitNumber,
                                       WorkPermitType = ea.WorkPermitType,
                                       WorkPermitEffectiveDate = ea.WorkPermitEffectiveDate,
                                       WorkPermitExpireDate = ea.WorkPermitExpireDate,
                                       VisaExpireDate = ea.VisaExpireDate

                                   }).FirstOrDefaultAsync();


            if (employee != null)
            {
                return employee;
            }
            else
            {
                var emp = await _employeeRepository.AllActive().Where(e => e.EmployeeID == employeeId).Select(m => new EmployeeAdditionalPostViewModel
                {
                    EmployeePersonalId = (int)m.EmployeeID,

                    PersonalEmail = m.Email ?? "N/A",
                    PersonalPhone = m.MobileNumber ?? "N/A",
                }).FirstOrDefaultAsync();

                return emp;
            }


            

            
        }


        #endregion


        #region Get Full buy Id

        public async Task<EmployeeAdditionalGetViewModel> GetFullEmployeeAdditionalByIdAsync(int employeeId)
        {

            var employee = await 
                                  //from ea in _employeeAdditionalRepository.AllActive()
                                  //join emp in _employeeRepository.AllActive()
                                  //on ea.EmployeeID equals emp.EmployeeID into empGroup // First left join
                                  //from emp in empGroup.DefaultIfEmpty()

                                    _employeeAdditionalRepository.AllActive().Include(ea => ea.Employee).Where(r=>r.EmployeeID == employeeId)
                                    .Include(r=>r.LicenceType)
                                    .Select(ea => new  EmployeeAdditionalGetViewModel
                                  {
                                      EmployeeAdditionalInfoID = ea.EmployeeAdditionalInfoID,
                                      EmployeePersonalId = (int)ea.EmployeeID,
                                      PersonalEmail = ea.Employee.Email,
                                      PersonalPhone = ea.Employee.MobileNumber,
                                      PasportName = ea.PasportName,
                                      PasportNo = ea.PasportNo,
                                      PasportPlaceOfIssue = ea.PasportPlaceOfIssue,
                                      PasportIssueDate = ea.PasportIssueDate,
                                      PasportExpireDate = ea.PasportExpireDate,
                                      DrivingLicenceNo = ea.DrivingLicenceNo,
                                      LicenceTypeID = ea.LicenceTypeID,
                                      LicenceTypeName = ea.LicenceType.LicenceTypeName,
                                      DrivingLicenceIssueDate = ea.DrivingLicenceIssueDate,
                                      DrivingLicenceExpireDate = ea.DrivingLicenceExpireDate,
                                      SymbolOfVehicleClass = ea.SymbolOfVehicleClass,
                                      DrivingLicencePlaceOfIssue = ea.DrivingLicencePlaceOfIssue,
                                      WorkPermaitNumber = ea.WorkPermaitNumber,
                                      WorkPermitType = ea.WorkPermitType,
                                      WorkPermitEffectiveDate = ea.WorkPermitEffectiveDate,
                                      WorkPermitExpireDate = ea.WorkPermitExpireDate,
                                      VisaExpireDate = ea.VisaExpireDate

                                  }).FirstOrDefaultAsync();


            if (employee != null)
            {
                return employee;
            }
            else
            {
                var emp = await _employeeRepository.AllActive().Where(e => e.EmployeeID == employeeId).Select(m => new EmployeeAdditionalGetViewModel
                {
                    EmployeePersonalId = (int)m.EmployeeID,

                    PersonalEmail = m.Email ?? "N/A",
                    PersonalPhone = m.MobileNumber ?? "N/A",
                }).FirstOrDefaultAsync();

                return emp;
            }





        }

        #endregion

        public async Task<CommonReturnViewModel> SubmitAsync(EmployeeAdditionalPostViewModel model)
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

            // List of checks
            missingFields.AddRange(new[]
            {
                Check(model.PasportName, "Passport Name"),
                Check(model.PasportNo, "Passport Number"),
                Check(model.PasportPlaceOfIssue, "Passport Place of Issue"),
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

                var existingInfo = await _employeeAdditionalRepository.AllActive().Where(e => e.EmployeeID == model.EmployeePersonalId).FirstOrDefaultAsync();

                if (existingInfo != null)
                {
                    existingInfo.LicenceTypeID = model.LicenceTypeID;
                    existingInfo.PasportName = model.PasportName;
                    existingInfo.PasportNo = model.PasportNo;
                    existingInfo.PasportPlaceOfIssue = model.PasportPlaceOfIssue;
                    existingInfo.PasportIssueDate = model.PasportIssueDate;
                    existingInfo.PasportExpireDate = model.PasportExpireDate;
                    existingInfo.DrivingLicenceNo = model.DrivingLicenceNo;
                    existingInfo.DrivingLicenceIssueDate = model.DrivingLicenceIssueDate;
                    existingInfo.DrivingLicenceExpireDate = model.DrivingLicenceExpireDate;
                    existingInfo.SymbolOfVehicleClass = model.SymbolOfVehicleClass;
                    existingInfo.DrivingLicencePlaceOfIssue = model.DrivingLicencePlaceOfIssue;
                    existingInfo.WorkPermaitNumber = model.WorkPermaitNumber;
                    existingInfo.WorkPermitType = model.WorkPermitType;
                    existingInfo.WorkPermitEffectiveDate = model.WorkPermitEffectiveDate;
                    existingInfo.WorkPermitExpireDate = model.WorkPermitExpireDate;
                    existingInfo.VisaExpireDate = model.VisaExpireDate;
                    
                   

                    await _employeeAdditionalRepository.UpdateAsync(existingInfo, model);

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee additional information updated successfully.",
                        Data = model.EmployeePersonalId
                    };
                }
                else 
                {
                    var employeeAdditionalInfo = new EmployeeAdditionalInfo
                    {

                        EmployeeID = model.EmployeePersonalId,
                        LicenceTypeID = model.LicenceTypeID,
                        PasportName = model.PasportName,
                        PasportNo = model.PasportNo,
                        PasportPlaceOfIssue = model.PasportPlaceOfIssue,
                        PasportIssueDate = model.PasportIssueDate,
                        PasportExpireDate = model.PasportExpireDate,
                        DrivingLicenceNo = model.DrivingLicenceNo,
                        DrivingLicenceIssueDate = model.DrivingLicenceIssueDate,
                        DrivingLicenceExpireDate = model.DrivingLicenceExpireDate,
                        SymbolOfVehicleClass = model.SymbolOfVehicleClass,
                        DrivingLicencePlaceOfIssue = model.DrivingLicencePlaceOfIssue,
                        WorkPermaitNumber = model.WorkPermaitNumber,
                        WorkPermitType = model.WorkPermitType,
                        WorkPermitEffectiveDate = model.WorkPermitEffectiveDate,
                        WorkPermitExpireDate = model.WorkPermitExpireDate,
                        VisaExpireDate = model.VisaExpireDate,
                        
                    };
                    await _employeeAdditionalRepository.AddAsync(employeeAdditionalInfo, model);
                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Employee additional information Added successfully.",
                        Data = employeeAdditionalInfo.EmployeeID

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






    }
}
