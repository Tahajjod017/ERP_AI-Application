using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RoleModule
{
    public class RoleManagementViewModel
    {
        [Required]
        [Display(Name = "Role Name")]
        public string? NewRoleName { get; set; }


        public List<UserRoleAssignment>? Users { get; set; }

        public Dictionary<string, List<UserRoleAssignment>>? RoleUserAssignments { get; set; }
    }
}
