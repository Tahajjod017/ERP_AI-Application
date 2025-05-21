using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.ActionLogVM
{
    public class ActionLogSetupVM
    {
        public int ActionLogID { get; set; }

        public string UserEmail { get; set; }

        public string ActionName { get; set; }

        public string ActionBefore { get; set; }

        public string ActionAfter { get; set; }

        public string LIP { get; set; }

        public string LMAC { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? EmployeeUserName { get; set; }
        public string TargetType { get; set; }
        public int? TargetID { get; set; }
    }
}
