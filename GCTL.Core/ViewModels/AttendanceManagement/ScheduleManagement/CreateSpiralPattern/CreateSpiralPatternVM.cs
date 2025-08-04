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

        [Required(ErrorMessage = "Pattern name is required!")]
        public string SpiralPatternName { get; set; }

        [Required(ErrorMessage = "Organization is required!")]
        public int OrganizationID { get; set; }

        [Required(ErrorMessage = "Pattern type is required!")]
        public int SpiralPatternTypeID { get; set; }

        public int SpiralBioWeeklyPatternID { get; set; }

        //[Required(ErrorMessage = "Pattern Name is required!")]
        //public string SpiralBioWeeklyPatternName { get; set; }

        public int SpiralMonthlyPatternID { get; set; }

        //[Required(ErrorMessage = "Pattern Name is required!")]
        //public string SpiralMonthlyPatternName { get; set; }

        public ICollection<SpiralWeeklyPatternDetailsVM>? SpiralWeeklyPatternDetailsVMs { get; set; } = new List<SpiralWeeklyPatternDetailsVM>();
        public ICollection<SpiralBioWeeklyPatternDetailsVM>? SpiralBioWeeklyPatternDetailsVMs { get; set; } = new List<SpiralBioWeeklyPatternDetailsVM>();
        public ICollection<SpiralMonthlyPatternDetailsVM>? SpiralMonthlyPatternDetailsVMs { get; set; } = new List<SpiralMonthlyPatternDetailsVM>();
    }
}
