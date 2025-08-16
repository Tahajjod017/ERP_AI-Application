using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance;
using GCTL.Data.Models;
using GCTL.Service.MasterSetup.Gender;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceService : AppService<EmployeeAllowances>, IPayRollEmpAllowanceService
    {
        private readonly IGenericRepository<EmployeeAllowances> empAllowance;

        public PayRollEmpAllowanceService(IGenericRepository<EmployeeAllowances> empAllowance):base(empAllowance) 
        {
            this.empAllowance = empAllowance;
        }

        public async Task<CommonReturnViewModel> SavePayRollEmpAllowance(PayRollEmpAllowanceSaveVM entityVM)
        {
            var result = new CommonReturnViewModel();

            try
            {
                if (entityVM == null || entityVM.OrganizationID==null)
                {
                    result.Success = false;
                    result.Message = "Data Not Found";
                    return result;
                }

                await empAllowance.BeginTransactionAsync();

                var entity = new EmployeeAllowances
                {
                    OrganizationID = entityVM.OrganizationID,
                    IsConveyanceAllowanceEnabled = entityVM.IsConveyanceAllowanceEnabled,
                    IsMobileInternetAllowanceEnabled=entityVM.IsMobileInternetAllowanceEnabled,
                    IsMedicalAllowanceEnabled= entityVM.IsMedicalAllowanceEnabled,
                    MobileInternetAllowance = entityVM.MobileInternetAllowance,
                    IsHouseRentAllowanceEnabled = entityVM.IsHouseRentAllowanceEnabled, 
                    HouseRentAllowanceRate = entityVM.HouseRentAllowanceRate,
                    IsShiftAllowanceEnabled = entityVM.IsShiftAllowanceEnabled,
                    ShiftAllowance = entityVM.ShiftAllowance,
                    HRentDependsOnSalaryTypeID = entityVM.HRentDependsOnSalaryTypeID,
                    MediAllowDepOnSalaryTypeID = entityVM.MediAllowDepOnSalaryTypeID,
                    MedicalAllowanceRate = entityVM.MedicalAllowanceRate,
                    ConAllowDepOnSalaryTypeID = entityVM.ConAllowDepOnSalaryTypeID,
                    ConveyanceAllowanceRate = entityVM.ConveyanceAllowanceRate,
                    LIP=entityVM.LIP,
                    LMAC=entityVM.LMAC,
                    CreatedAt=DateTime.Now, 
                    CreatedBy=entityVM.CreatedBy,
                };

                await empAllowance.AddAsync(entity);

                // You may need to commit the transaction here
                await empAllowance.CommitTransactionAsync();

                result.Success = true;
                result.Data = entity;
                result.Message = "Employee allowance saved successfully.";
               
            }
            catch (Exception ex)
            {
                // Rollback in case of error
                await empAllowance.RollbackTransactionAsync();

                result.Success = false;
                result.Message = "An error occurred while saving.";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

    }
}
