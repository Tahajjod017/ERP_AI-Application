using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class AddSpiralFortMonthlyPatternVM : BaseViewModel
    {
        public int AddSpiralPatternDetailIDFortMonthly { get; set; }
        public int AddOrganizationIDFortMonthly { get; set; }
        public int AddSpiralPatternTypeIDFortMonthly { get; set; }
        public int? AddDayOfMonthFortMonthly { get; set; }
        public int AddShiftIDFortMonthly { get; set; }
    }
}
