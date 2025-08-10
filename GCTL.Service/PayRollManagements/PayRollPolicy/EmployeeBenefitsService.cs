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

        public async Task<CommonReturnViewModel> SaveEmployeeBenefits(EmployeeBenefitsSaveVM entityVM)
        {
            await empBenefits.BeginTransactionAsync();
            try
            {
                if(empBenefits == null)
                {
                    return new CommonReturnViewModel
                    {
                       Success = false,
                       Message=""
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
                    ProvidentFundRate=entityVM.ProvidentFundRate,
                    ProvidentFundOnSalaryTypeID=entityVM.ProvidentFundOnSalaryTypeID,
                    ProvidentFundMinimumServiceYear=entityVM.ProvidentFundMinimumServiceYear,
                   
                };
                await empBenefits.AddAsync(entity);
                await empBenefits.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ""
                };
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
