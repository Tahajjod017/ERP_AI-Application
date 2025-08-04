using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class SpiralWeeklyPatternList
    {
        public int SpiralWeeklyPatternID { get; set; }
        public string SpiralPatternName { get; set; }
        public int? OrganizationID { get; set; }
        public string OrganizationName { get; set; }
        public int SpiralPatternTypeID { get; set; }
        public string SpiralPatternTypeName { get; set; }
        public ICollection<SpiralWeeklyPatternDetailsVM>? SpiralWeeklyPatternDetailsVMs { get; set; } = new List<SpiralWeeklyPatternDetailsVM>();
    }
}
