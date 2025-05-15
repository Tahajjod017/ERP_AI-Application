using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RoleModule
{
    public class PermissionViewModel
    {
        public List<RoleViewModel> Roles { get; set; }
        public List<PrimaryModuleViewModel> PrimaryModules { get; set; }
        public string? SelectedRoleId { get; set; }
    }
}
