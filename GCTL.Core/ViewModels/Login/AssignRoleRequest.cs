using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Login
{
    public class AssignRoleRequest
    {
        [Required]
        public string RoleId { get; set; }

        [Required]
        public string UserId { get; set; }
    }
}
