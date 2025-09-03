
using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;

namespace GCTL.Data.Models
{
    public class ApplicationRole : IdentityRole
    {
        public int? TenantInfoId { get; set; }
        public virtual TenantInfo? TenantInfo { get; set; } // check for add application user/Role to tenant info
        public int? OrganizationID { get; set; }
        public virtual Organization? Organization { get; set; } // check for add application user/Role to organization
        public ICollection<RoleModulePermissions>? RoleModulePermissions { get; set; } // check for add application user/Role to organization
        public ICollection<RoleElementPermissions>? RoleElementPermissions { get; set; } // check for add application user/Role to organization
    }
    public class ApplicationUser : IdentityUser
    {
        public int? TenantInfoId { get; set; }
        public virtual TenantInfo? TenantInfo { get; set; }
        public int? OrganizationID { get; set; }
        public virtual Organization? Organization { get; set; }
        public int? EmployeeId { get; set; }

        public virtual Employees? Employees { get; set; } // hrm project ,, reverse engine disi, user
        public bool IsPasswordResetRequired { get; set; } = false;

        // New field for storing the default generated password
        public string? DefaultPass { get; set; }

    }


}
