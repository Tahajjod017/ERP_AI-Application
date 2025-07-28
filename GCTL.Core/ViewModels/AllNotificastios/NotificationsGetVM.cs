using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AllNotificastios
{
    public class NotificationsGetVM
    {
        public int AlertForEmployeeID { get; set; }

        public int? AlertID { get; set; }

        public int? EmployeeID { get; set; }
        public bool? IsChecked { get; set; }

        public int? CreatedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? EmployeeDepartment { get; set; }
        public string? EmployeeImage { get; set; }
        public string? EmployeeName { get; set; }
    }
}
