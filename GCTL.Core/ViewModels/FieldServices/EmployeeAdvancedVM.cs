using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.FieldServices
{
    public class EmployeeAdvancedVM:BaseViewModel
    {
        public int EmployeeAdvanceID { get; set; }

        public int? CustomerID { get; set; }
        public int? JobID { get; set; }

        public int? RequestedByUserID { get; set; }
        public decimal AmountRequested { get; set; }
        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public bool? IsGroupAdvance { get; set; }

        public int? ApprovalStatusID { get; set; }

        public int? ApprovedByUserID { get; set; }

        public DateTime? ApprovalDate { get; set; }


    }
}
