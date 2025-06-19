using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Login
{
    public class RoleElementPermissionViewModel
    {
        public string RoleId { get; set; }
        public int PageId { get; set; }
        public List<string> ElementKeys { get; set; }
    }
}
