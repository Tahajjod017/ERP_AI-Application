using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;

namespace GCTL.Service.Employees.EmployeeBenifit
{
    public interface IEmployeeBenifitService
    {

        Task<List<CommonSelectVMMM>> SelectAsync(int id);

        Task<CommonReturnViewModel> SaveOrUpdateEmployeeBenefitsAsync(EmployeeBenifitPostViewModel model);
        Task<EmployeeBenifitPostViewModel> GetEmployeeBenefitsAsync(string employeeId);
    }
    public class CommonSelectVMMM
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public List<EmpBenefitVMMM>? EmpBenefitVMM { get; set; } = new List<EmpBenefitVMMM>();
    }

    public class EmpBenefitVMMM
    {
        public int BenefitID { get; set; }
        public int? OrganizationID { get; set; }
        public int? CalculationTypeID { get; set; }
        public decimal? Value { get; set; }
    }

    


}
