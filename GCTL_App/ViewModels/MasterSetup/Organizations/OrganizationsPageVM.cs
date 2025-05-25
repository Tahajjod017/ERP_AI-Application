using GCTL.Core.ViewModels.MasterSetup.Organizations;

namespace GCTL_App.ViewModels.MasterSetup.Organizations
{
    public class OrganizationsPageVM
    {
        public OrganizationsVM Setup { get; set; } = new OrganizationsVM();
        public List<OrganizationsVM> List { get; set; } = new List<OrganizationsVM>();
    }
}
