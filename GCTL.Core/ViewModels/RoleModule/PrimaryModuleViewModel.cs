using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RoleModule
{
    public class PrimaryModuleViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool CanCreate { get; set; }
        public bool CanView { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
        public bool CanExport { get; set; }
        public bool CanDownload { get; set; }
        public List<SecondaryModuleViewModel> SecondaryModules { get; set; }
    }
}
