using GCTL.Core.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace GCTL_App.ViewModels.FieldServiceOne
{
    public class SaveActivityRequest
    {
        [Required(ErrorMessage = "Job ID is required")]
        public int JobID { get; set; }

        [Required(ErrorMessage = "Activity step is required")]
        [Range(1, 7, ErrorMessage = "Activity step must be between 1 and 7")]
        public int ActivityStep { get; set; }

        [Required(ErrorMessage = "Remarks are required")]
        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
        public string Remarks { get; set; }
    }

    public class SaveJobTeamActivityRequest : BaseViewModel
    {
        [Required(ErrorMessage = "Job ID is required")]
        public int JobID { get; set; }

        [Required(ErrorMessage = "Activity type is required")]
        [RegularExpression("^(start|push|pause)$", ErrorMessage = "Activity type must be 'start', 'push', or 'pause'")]
        public string ActivityType { get; set; }
    }

    public class TeamMemberVM
    {
        public int EmployeeID { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string ImageUrl { get; set; }
        public string Image { get; set; }
        public string Avatar { get; set; }
    }
}
