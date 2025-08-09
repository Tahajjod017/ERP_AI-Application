using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class UpdateSpiralPatternVM : BaseViewModel
    {
        public int UpdateSpiralPatternDetailID { get; set; }
        public int UpdateOrganizationID { get; set; }
        public int UpdateSpiralPatternID { get; set; }
        public int UpdateSpiralPatternTypeID { get; set; }
        public byte DayOfWeek { get; set; }
        public int? DayOfMonth { get; set; }
        public int UpdateShiftID { get; set; }
    }
}
