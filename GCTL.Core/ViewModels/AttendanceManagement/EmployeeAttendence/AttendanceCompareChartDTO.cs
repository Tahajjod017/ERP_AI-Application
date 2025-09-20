using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.EmployeeAttendence
{
    public class AttendanceCompareChartDTO
    {
        // Chart series order: Present, Absent, Early, Late
        public List<int> you { get; set; } = new();
        public List<int> bestEmp { get; set; } = new();
        public BestEmpMetaDTO bestEmpMeta { get; set; } = new();
    }

    public class BestEmpMetaDTO
    {
        public int id { get; set; }
        public string? name { get; set; }
    }

}
