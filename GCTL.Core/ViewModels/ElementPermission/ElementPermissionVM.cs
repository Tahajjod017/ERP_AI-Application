using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.ElementPermission
{
    public class ElementPermissionVM:BaseViewModel
    {
        public int Id { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? PageId { get; set; }
        public string? PageName { get; set; }
        public string? ElementKeys { get; set; }
        public string? ElementNames { get; set; }

    }
}
