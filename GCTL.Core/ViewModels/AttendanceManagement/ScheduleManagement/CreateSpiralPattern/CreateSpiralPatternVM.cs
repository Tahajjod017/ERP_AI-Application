using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class CreateSpiralPatternVM : BaseViewModel
    {
        public int SpiralWeeklyPatternID { get; set; }

        [Required(ErrorMessage = "Organization is required!")]
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "Pattern type is required!")]
        public int SpiralPatternTypeID { get; set; }

        [Required(ErrorMessage = "Pattern name is required!")]
        public string SpiralWeeklyPatternName { get; set; }

        public ICollection<SpiralWeeklyPatternDetailsVM>? SpiralWeeklyPatternDetails { get; set; } = new List<SpiralWeeklyPatternDetailsVM>();
    }
}
