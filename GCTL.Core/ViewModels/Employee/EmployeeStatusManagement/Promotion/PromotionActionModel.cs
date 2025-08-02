using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion
{
    public class PromotionActionModel : BaseViewModel
    {
        public int PromotionId { get; set; }
        public string Action { get; set; }
        public string Comments { get; set; }
    }
}
