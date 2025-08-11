using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollPolicy
{
    public class EmployeeBenefitsService : AppService<EmployeeBenefits>, IEmployeeBenefitsService
    {
        private readonly IGenericRepository<EmployeeBenefits> empBenefits;
        public EmployeeBenefitsService( IGenericRepository<EmployeeBenefits> empBenefits) : base(empBenefits)
        {
            this.empBenefits = empBenefits;
        }

        public async Task<CommonReturnViewModel> SaveEmployeeBenefits(PayRollEmpBenefitsSaveVM entityVM)
        {
            await empBenefits.BeginTransactionAsync();
            try
            {
                if(entityVM == null)
                {
                    return new CommonReturnViewModel
                    {
                       Success = false,
                       Message="Data Not Found"
                    };
                }
                var entity = new EmployeeBenefits
                {
                    OrganizationID = entityVM.OrganizationID,
                    IsHealthInsuranceEnabled = entityVM.IsHealthInsuranceEnabled,
                    IsFastivalBonusEnabled = entityVM.IsFastivalBonusEnabled,
                    PerformanceBonus=entityVM.PerformanceBonus,
                    FastivalBonusRate=entityVM.FastivalBonusRate,
                    FastivalBonusOnSalaryTypeID=entityVM.FastivalBonusOnSalaryTypeID,
                    IsProvidentFundEnabled=entityVM.IsProvidentFundEnabled,
                    ProvidentFundEmployeeContrebution=entityVM.ProvidentFundEmployeeContrebution,
                    ProvidentFundOrganizationContrebution=entityVM.ProvidentFundOrganizationContrebution,
                    ProvidentFundOnSalaryTypeID=entityVM.ProvidentFundOnSalaryTypeID,
                    ProvidentFundMinimumServiceYear=entityVM.ProvidentFundMinimumServiceYear,
                    HealthInsurance=entityVM.HealthInsurance,
                    IsPerformanceBonusEnabled=entityVM.IsPerformanceBonusEnabled,
                    IsYearEndBonusEnabled=entityVM.IsYearEndBonusEnabled,
                    YearlyEndBonusTypeID=entityVM.YearlyEndBonusTypeID,
                    CreatedAt=DateTime.Now,
                    CreatedBy=entityVM.CreatedBy,
                    LIP=entityVM.LIP,
                    LMAC=entityVM.LMAC, 
                };
                await empBenefits.AddAsync(entity);
                await empBenefits.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Save Succssfully"
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
