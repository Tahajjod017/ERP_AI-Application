using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmpTransfer
{
    public class PersonTransferStepVM
    {
        public string? ApprovedOrDeclineDate { get; set; }
        public int? ApproverStep { get; set; }
        public string? ApprovarNote { get; set; }
        public string? ApprovarPerson { get; set; }
        public string? StatusName { get; set; }
    }
}
