using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ElementPermission
{
    public interface IElementPermissionService
    {
        Task<bool> HasPermissionForElementAsync(string userId, int pageId, string elementKey);
    }
}
