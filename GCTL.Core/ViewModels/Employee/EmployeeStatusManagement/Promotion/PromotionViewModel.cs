using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion
{
    public class PromotionViewModel : BaseViewModel
    {
        public int EmployeeID { get; set; }
        public int? OrganizationID { get; set; }
        public int? DesignationID { get; set; }
        public int? DepartmentID { get; set; }
        public string ChangeType { get; set; } // "promotion" or "demotion"
        public DateTime? EffectiveDate { get; set; }
        public int? CurrentDesignationID { get; set; }
        public int? NewDesignationID { get; set; }
        public decimal? CurrentSalary { get; set; }

        //public decimal? IncrementAmount { get; set; }
        //public decimal? IncrementPercent { get; set; }
        public decimal? NewSalary { get; set; }
        public string Remarks { get; set; }
    }

}
