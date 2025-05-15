using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AccessPermissions
{
    public interface IAccessControlService
    {
        Task<bool> HasPermissionAsync(int menuTabId, string permissionType);
    }
}
