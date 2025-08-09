using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class AddSpiralWeeklyPatternVM : BaseViewModel
    {
        public int AddSpiralPatternDetailID { get; set; }
        public int AddOrganizationID { get; set; }
        public int AddSpiralPatternTypeID { get; set; }
        public byte? AddDayOfWeek { get; set; }
        public int AddShiftID { get; set; }
    }
}
