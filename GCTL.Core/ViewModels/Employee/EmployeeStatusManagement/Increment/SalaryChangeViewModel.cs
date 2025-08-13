using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment
{
    //public class SalaryChangeViewModel : BaseViewModel
    //{
    //    public int EmployeeId { get; set; }
    //    public int OrganizationId { get; set; }
    //    public int DesignationId { get; set; }
    //    public int DepartmentId { get; set; }

    //    public string ChangeType { get; set; } // "increment" or "decrement"
    //    public DateTime EffectiveDate { get; set; }

    //    public decimal CurrentSalary { get; set; }
    //    public decimal NewSalary { get; set; }

    //    public string Remarks { get; set; }
    //}

    public class SalaryChangeViewModel : BaseViewModel
    {
        public int EmployeeId { get; set; }
        public string ChangeType { get; set; }
        public decimal? CurrentSalary { get; set; }
        public decimal? NewSalary { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public string Remarks { get; set; }

        //public string CreatedBy { get; set; }
        //public string LIP { get; set; }
        //public string LMAC { get; set; }
    }

    public class IncrementApproveViewModel : BaseViewModel
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; }
        public string Department { get; set; }
        public string IncrementType { get; set; }
        public string CurrentSalary { get; set; }
        public decimal? CurrentSalaryNumeric { get; set; }
        public string ProposedSalary { get; set; }
        public decimal? ProposedSalaryNumeric { get; set; }
        public string IncrementAmount { get; set; }
        public decimal? IncrementAmountNumeric { get; set; }
        public string EffectiveDate { get; set; }
        public DateTime? EffectiveDateRaw { get; set; }
        public string ApprovedDate { get; set; }
        public DateTime? ApprovedDateRaw { get; set; }
        public string YearsOfExperience { get; set; }
        public string Justification { get; set; }
        public string AvatarUrl { get; set; }
        public string Status { get; set; }
    }

    public class IncrementFilterModel : BaseViewModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string IncrementType { get; set; }
        public string Department { get; set; }
        public string EmployeeName { get; set; }
        public string Status { get; set; }
        public string DateRange { get; set; }
        public string SortBy { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }
    }

    public class IncrementActionModel : BaseViewModel
    {
        public int IncrementId { get; set; }
        public string Action { get; set; }
        public string Comments { get; set; }

        //public string CreatedBy { get; set; }
        //public string UpdatedBy { get; set; }
        //public string LIP { get; set; }
        //public string LMAC { get; set; }
    }

}
