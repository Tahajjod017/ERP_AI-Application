using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class SpiralBioWeeklyPatternListVM : BaseViewModel
    {
        public int SpiralBioWeeklyPatternID { get; set; }

        public string SpiralBioWeeklyPatternName { get; set; }

        public int? OrganizationID { get; set; }

        public string OrganizationName { get; set; }

        public int? SpiralPatternTypeID { get; set; }

        public string SpiralPatternTypeName { get; set; }

        public ICollection<SpiralBioWeeklyPatternDetailsListVM>? SpiralBioWeeklyPatternDetailsListVMs { get; set; } = new List<SpiralBioWeeklyPatternDetailsListVM>();
    }
}
