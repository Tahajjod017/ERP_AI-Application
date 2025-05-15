using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RoleModule
{
    public class PermissionUpdateModel
    {
        //public string? RoleId { get; set; }          // ID of the role being modified
        public int MenuTabId { get; set; }            // ID of the module (primary, secondary, tertiary)
        public string? Permission { get; set; }       // Permission type (e.g., "VIEW", "CREATE")
        public bool IsGranted { get; set; }
    }
}
