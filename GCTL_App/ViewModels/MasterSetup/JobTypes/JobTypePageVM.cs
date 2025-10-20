using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.JobTypes;

namespace GCTL_App.ViewModels.MasterSetup.JobTypes
{
    public class JobTypePageVM
    {
        public JobTypeVM Setup { get; set; } = new JobTypeVM();
        public List<JobTypeVM> List { get; set; } = new List<JobTypeVM>();
    }
}
