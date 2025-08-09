using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class SpiralMonthlyPatternListVM : BaseViewModel
    {
        public int SpiralMonthlyPatternID { get; set; }

        public string SpiralMonthlyPatternName { get; set; }

        public int? OrganizationID { get; set; }

        public string OrganizationName { get; set; }

        public int? SpiralPatternTypeID { get; set; }
        
        public string SpiralPatternTypeName { get; set; }
        
        public ICollection<SpiralMonthlyPatternDetailsListVM>? SpiralMonthlyPatternDetailsListVMs { get; set; } = new List<SpiralMonthlyPatternDetailsListVM>();
    }
}
