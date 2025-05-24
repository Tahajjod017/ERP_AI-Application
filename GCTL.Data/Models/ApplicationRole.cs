
using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace GCTL.Data.Models
{
    public class ApplicationRole : IdentityRole
    {
        public ICollection<RoleModulePermissions>? RoleModulePermissions { get; set; }
      
       
    }
    public class ApplicationUser : IdentityUser
    {
       public int? EmployeeId { get; set; }

        public virtual Employees? Employees { get; set; }

    }

   
}
