using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.LoanManagement
{
    public class LoanUpdateVM:BaseViewModel
    {
        public int LoanIDEdit { get; set; }
        public int? EmployeeIDEdit { get; set; }
        public decimal? LoanAmountEdit { get; set; }
        public int? LoanInstallmentPeriodIDEdit { get; set; }
        public DateTime? IssueDateEdit { get; set; }
        public DateTime? StartDateEdit { get; set; }
      

    }
}
