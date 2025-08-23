using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.Universal
{
    public class UniversalApprovalToolTip
    {

        public List<ApproveStageDetails> StageDetails { get; set; }
       
        public string approvalDetails { get;set; }
    }

    public class ApproveStageDetails
    {
        public string approverStep { get; set; }
        public string statusName { get; set; }
        public string approvarPerson { get; set; }
        public string approvarNote { get; set; }
        public string approvedOrDeclineDate { get; set; }
    }

    
}


