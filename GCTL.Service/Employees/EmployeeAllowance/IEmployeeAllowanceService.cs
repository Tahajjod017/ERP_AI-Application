using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeAllowance;
using GCTL.Core.ViewModels.Employee.EmployeeBenifit;
using GCTL.Service.Employees.EmployeeBenifit;

namespace GCTL.Service.Employees.EmployeeAllowance
{
    public interface IEmployeeAllowanceService
    {
        Task<EmployeeAdditionalPostViewModel> GetEmployeeAllowance(int employeeId);


        Task<List<CommonSelectVMMMAllowance>> SelectAsync(int id);
        Task<CommonReturnViewModel> Save(EmployeeAlowancePostViewModel22 model);
    }

    public class EmployeeAlowancePostViewModel22 : BaseViewModel
    {
        public int? EmployeeId { get; set; }
        public List<EmployeeAllowanceItem> Benefits { get; set; } = new();
    }

    public class EmployeeAllowanceItem
    {
        public int BenefitID { get; set; }
        public int? BaseCalculationTypeID { get; set; }
        public decimal? BaseValue { get; set; }
        //
    }

    // for getting ViewModel
    public class CommonSelectVMMMAllowance
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public List<EmpAllowanceVMMM>? EmpBenefitVMM { get; set; } = new List<EmpAllowanceVMMM>();
    }

    public class EmpAllowanceVMMM
    {
        public int BenefitID { get; set; }
        public int? OrganizationID { get; set; }
        public int? CalculationTypeID { get; set; }
        public decimal? Value { get; set; }

        //from BaseBenefit 

        public int BaseBenefitID { get; set; }
        public int? BaseCalculationTypeID { get; set; }
        public decimal? BaseValue { get; set; }
    }
}
