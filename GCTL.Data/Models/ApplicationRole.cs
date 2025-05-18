
using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace GCTL.Data.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<RoleModulePermission>? RoleModulePermissions { get; set; }
      
       
    }
    public class ApplicationUser : IdentityUser
    {
       public int? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }
    }

   
}
