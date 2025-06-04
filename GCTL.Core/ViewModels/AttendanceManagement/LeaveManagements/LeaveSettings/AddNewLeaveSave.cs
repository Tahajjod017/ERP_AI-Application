using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveSettings
{
    public class AddNewLeaveSave:BaseViewModel
    {
      
        public string? LeaveTypeName { get; set; }

        
        public int? OrganizationID { get; set; }

        public bool IsApid { get; set; }

        public decimal? LeaveDays { get; set; }
        public string? Code { get; set; }

    }
}
