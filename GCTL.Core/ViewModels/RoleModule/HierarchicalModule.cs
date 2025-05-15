using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.RoleModule
{
    public class HierarchicalModule
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<HierarchicalModule> Children { get; set; } = new();
    }
}
