using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class ReturnView
    {
        public bool? Success { get; set; }
        public int? Id { get; set; }
        public string? Message{ get; set; }
        public string? Name { get; set; }
    }
}
