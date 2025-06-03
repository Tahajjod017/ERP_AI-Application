using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers.Entity;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeAllowance;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeAllowance
{
    public class EmployeeAllowanceService : IEmployeeAllowanceService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeBaseAllowances> _employeeAllowancesRepository;
        private readonly IGenericRepository<EmployeeSalarySettings> _employeeSalaryRepository;

        public EmployeeAllowanceService(IGenericRepository<EmployeeBaseAllowances> employeeAllowancesRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<EmployeeSalarySettings> employeeSalaryRepository)
        {
            _employeeAllowancesRepository = employeeAllowancesRepository;
            _employeeRepository = employeeRepository;
            _employeeSalaryRepository = employeeSalaryRepository;
        }

        public async Task<EmployeeAdditionalPostViewModel> GetEmployeeAllowance(int employeeId)
        {
            var allowance = await(from eb in _employeeAllowancesRepository.AllActive()
                                  join emp in _employeeRepository.AllActive()
                                  on eb.EmployeeID equals emp.EmployeeID into empGroup // First left join
                                  from emp in empGroup.DefaultIfEmpty()

                                  join empSaley in _employeeSalaryRepository.AllActive()
                                  on emp.EmployeeID equals empSaley.EmployeeID into empSaleyGroup // Second left join
                                  from empSaley in empSaleyGroup.DefaultIfEmpty()

                                  where eb.EmployeeID == employeeId

                                  select new EmployeeAdditionalPostViewModel
                                 {
                                     EmployeePersonalId = emp.EmployeeID,
                                     PersonalEmail = emp.Email ?? "N/A",
                                     PersonalPhone = emp.MobileNumber ?? "N/A",
                                     EmployeeBaseAllowanceID = eb.EmployeeBaseAllowanceID,

                                    
                                      //MobileInternetAllowance = eb.MobileInternetAllowance,
                                      //IsMobileInternetAllowanceEnabled = eb.IsMobileInternetAllowanceEnabled,

                                      MobileAllowance = eb.MobileAllowance,
                                      IsMobileAllowanceEnabled = eb.IsMobileAllowanceEnabled,
                                      InternetAllowance = eb.InternetAllowance,
                                      IsInternetAllowanceEnabled = eb.IsInternetAllowanceEnabled,

                                      InternetAllowanceEffectiveFrom = eb.InternetAllowanceEffectiveFrom ,
                                      MobileAllowanceEffectiveFrom = eb.MobileAllowanceEffectiveFrom ,

                                      ShiftAllowance = eb.ShiftAllowance,

                                     IsShiftAllowanceEnabled = eb.IsShiftAllowanceEnabled,
                                     HouseRentAllowancePercentage = eb.HouseRentAllowancePercentage,
                                     IsHouseRentAllowancePercentageEnabled = eb.IsHouseRentAllowancePercentageEnabled,
                                     MedicalAllowancePercentage = eb.MedicalAllowancePercentage,
                                     IsMedicalAllowancePercentageEnabled = eb.IsMedicalAllowancePercentageEnabled,
                                     ConveyanceAllowancePercentage = eb.ConveyanceAllowancePercentage,
                                     IsConveyanceAllowancePercentageEnabled = eb.IsConveyanceAllowancePercentageEnabled,
                                     IsEmployeeAllowanceEnabled = empSaley.IsAllowanceEnabled

                                 }).FirstOrDefaultAsync();


            if (allowance != null)
            {
                return allowance;
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

        public async Task<CommonReturnViewModel> SaveEmployeeAllowanceAsync(EmployeeAdditionalPostViewModel model)
        {
            try
            {
                var existingAllowance = await _employeeAllowancesRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                if (existingAllowance == null)
                {

                    var newAllowance = new EmployeeBaseAllowances
                    {
                        //EmployeeBaseAllowanceID = model.EmployeeBaseAllowanceID,
                        EmployeeID = model.EmployeePersonalId,

                       // MobileInternetAllowance = model.MobileInternetAllowance,
                        //IsMobileInternetAllowanceEnabled = model.IsMobileInternetAllowanceEnabled,

                        MobileAllowance = model.MobileAllowance,
                        IsMobileAllowanceEnabled = model.IsMobileAllowanceEnabled,
                        InternetAllowance = model.InternetAllowance,
                        IsInternetAllowanceEnabled = model.IsInternetAllowanceEnabled,

                        ShiftAllowance = model.ShiftAllowance,
                        IsShiftAllowanceEnabled = model.IsShiftAllowanceEnabled,
                        HouseRentAllowancePercentage = model.HouseRentAllowancePercentage,
                        IsHouseRentAllowancePercentageEnabled = model.IsHouseRentAllowancePercentageEnabled,
                        MedicalAllowancePercentage = model.MedicalAllowancePercentage,
                        IsMedicalAllowancePercentageEnabled = model.IsMedicalAllowancePercentageEnabled,
                        ConveyanceAllowancePercentage = model.ConveyanceAllowancePercentage,
                        IsConveyanceAllowancePercentageEnabled = model.IsConveyanceAllowancePercentageEnabled,
                        MobileAllowanceEffectiveFrom = model.MobileAllowanceEffectiveFrom,
                        InternetAllowanceEffectiveFrom =model.InternetAllowanceEffectiveFrom,
                       
                    };


                    var empSalary = await _employeeSalaryRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                    if (empSalary == null)
                    {
                        empSalary = new EmployeeSalarySettings()
                        {
                            EmployeeID = model.EmployeePersonalId,
                            IsAllowanceEnabled = model.IsEmployeeAllowanceEnabled,
                            
                        };
                        await _employeeSalaryRepository.AddAsync(empSalary, model);
                    }
                    else
                    {
                        empSalary.IsAllowanceEnabled = model.IsEmployeeAllowanceEnabled;
                        await _employeeSalaryRepository.UpdateAsync(empSalary, model);
                    }

                    EntityHelper.Create(newAllowance, model);

                    await _employeeAllowancesRepository.AddAsync(newAllowance);
                }
                else
                {


                     //existingAllowance.MobileInternetAllowance = model.MobileInternetAllowance;
                    //existingAllowance.IsMobileInternetAllowanceEnabled = model.IsMobileInternetAllowanceEnabled;

                    existingAllowance.MobileAllowance = model.MobileAllowance;
                    existingAllowance.IsMobileAllowanceEnabled = model.IsMobileAllowanceEnabled;
                    existingAllowance.InternetAllowance = model.InternetAllowance;
                    existingAllowance.IsInternetAllowanceEnabled = model.IsInternetAllowanceEnabled;

                    existingAllowance.ShiftAllowance = model.ShiftAllowance;
                    existingAllowance.IsShiftAllowanceEnabled = model.IsShiftAllowanceEnabled;
                    existingAllowance.HouseRentAllowancePercentage = model.HouseRentAllowancePercentage;
                    existingAllowance.IsHouseRentAllowancePercentageEnabled = model.IsHouseRentAllowancePercentageEnabled;
                    existingAllowance.MedicalAllowancePercentage = model.MedicalAllowancePercentage;
                    existingAllowance.IsMedicalAllowancePercentageEnabled = model.IsMedicalAllowancePercentageEnabled;
                    existingAllowance.ConveyanceAllowancePercentage = model.ConveyanceAllowancePercentage;
                    existingAllowance.IsConveyanceAllowancePercentageEnabled = model.IsConveyanceAllowancePercentageEnabled;



                    var empSalary = await _employeeSalaryRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeePersonalId);

                    if (empSalary == null)
                    {
                        empSalary = new EmployeeSalarySettings()
                        {
                            EmployeeID = model.EmployeePersonalId,
                            IsAllowanceEnabled = model.IsEmployeeAllowanceEnabled,

                        };
                        await _employeeSalaryRepository.AddAsync(empSalary, model);
                    }
                    else
                    {
                        empSalary.IsAllowanceEnabled = model.IsEmployeeAllowanceEnabled;
                        await _employeeSalaryRepository.UpdateAsync(empSalary, model);
                    }

                    EntityHelper.Update(existingAllowance, model);

                    await _employeeAllowancesRepository.UpdateAsync(existingAllowance);
                }

                return new CommonReturnViewModel()
                {
                    Success = true,
                    Message = "Employee Allowances saved successfully.",
                    Data = model.EmployeePersonalId
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel()
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
    }
}
