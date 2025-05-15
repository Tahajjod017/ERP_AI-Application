using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RoleModule
{
    public class BulkPermissionUpdateModel
    {
        public string RoleId { get; set; }
        public List<PermissionUpdateModel> Permissions { get; set; }

    }
}
