using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class AttendanceYearlyChartDTO
    {
        public List<string> months { get; set; } = new();
        public List<int> present { get; set; } = new();
        public List<int> absent { get; set; } = new();
        public List<int> lateEntry { get; set; } = new();
        public List<int> earlyLeave { get; set; } = new();

     
        public List<int> casualLeave { get; set; } = new();
        public List<int> medicalLeave { get; set; } = new();
    }
}
