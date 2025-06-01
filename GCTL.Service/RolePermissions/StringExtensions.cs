using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.RolePermissions
{
    public static class StringExtensions
    {
        public static string ToCleanRoleName(this string fullRoleName)
        {
            if (string.IsNullOrEmpty(fullRoleName))
                return fullRoleName;

            var parts = fullRoleName.Split('_', 3);

            return parts.Length == 3 ? parts[2] : fullRoleName;
        }
    }

}
