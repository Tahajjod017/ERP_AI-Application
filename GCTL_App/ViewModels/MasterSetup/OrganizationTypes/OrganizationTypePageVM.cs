using GCTL.Core.ViewModels.MasterSetup.OrganizationTypes;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;

namespace GCTL_App.ViewModels.MasterSetup.OrganizationTypes
{
    public class OrganizationTypePageVM
    {
        public OrganizationTypeVM Setup { get; set; } = new OrganizationTypeVM();
        public List<OrganizationTypeVM> List { get; set; } = new List<OrganizationTypeVM>();
    }
}
