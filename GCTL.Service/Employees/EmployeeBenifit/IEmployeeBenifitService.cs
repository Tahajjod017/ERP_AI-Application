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
        Task<EmployeeBenifitPostViewModel> GetEmployeeBenefitsAsync(string employeeId);
        Task<CommonReturnViewModel> SaveOrUpdateEmployeeBenefitsAsync1(EmployeeBenifitPostViewModel22 model);


    }

    public class EmployeeBenifitPostViewModel22:BaseViewModel
    {
        public int ? EmployeeId { get; set; }
        public List<EmployeeBenefitItem> Benefits { get; set; } = new();
    }

    public class EmployeeBenefitItem
    {
        public int BenefitID { get; set; }      
        public int? CalculationTypeID { get; set; } 
        public decimal? Value { get; set; }   
        public bool IsBenifitEnabled { get; set; }

        //
        public int BaseBenefitID { get; set; }
        public int? BaseCalculationTypeID { get; set; }
        public decimal? BaseValue { get; set; }
        //
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

        //from BaseBenefit 

        public int BaseBenefitID { get; set; }
        public int? BaseCalculationTypeID { get; set; }
        public decimal? BaseValue { get; set; }
    }

    


}
