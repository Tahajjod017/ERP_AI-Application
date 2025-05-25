using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeSalary
{
    public class EmployeeSalaryPostViewModel
    {
        public int EmployeePersonalId { get; set; }
        public string? PersonalPhone { get; set; }
        public string? PersonalEmail { get; set; }


        public int EmployeeSalarySettingsID { get; set; }

       // public int? EmployeeID { get; set; }

        public string BankName { get; set; }

        public string BranchName { get; set; }

        public string AccountName { get; set; }

        public string AccountNo { get; set; }

        public string Address { get; set; }

        public string ATMCardNo { get; set; }

        public string RoutingNo { get; set; }

        public string SWIFTCode { get; set; }

        public string IFSCCode { get; set; }

        public string bKashAccountNo { get; set; }

        public string RoketAccountNo { get; set; }

        public string NagodAccountNo { get; set; }

        public string EmployeeGID { get; set; }

        public int? GradeID { get; set; }

        public decimal? Salary { get; set; }

        public int? CurrencyID { get; set; }


    }
}
