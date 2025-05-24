using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.Departments
{
    public class DepartmentVM : BaseViewModel
    {
        public int DepartmentID { get; set; }

        public string DepartmentName { get; set; }

        //public virtual ICollection<Designations> Designations { get; set; } = new List<Designations>();

        //public virtual ICollection<EmployeeOfficeInfo> EmployeeOfficeInfo { get; set; } = new List<EmployeeOfficeInfo>();

    }
}
