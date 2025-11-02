using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class CustomerTableDataVM
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public int? TotalBranch { get; set; }
        public int? TotalWarehouse { get; set; }
    }
}
