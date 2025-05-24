using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.Grade
{
    public class GradeVM : BaseViewModel
    {
        public int GradeID { get; set; }

        public string GradeName { get; set; }

        // public virtual ICollection<EmployeeOfficeInfo> EmployeeOfficeInfo { get; set; } = new List<EmployeeOfficeInfo>();

    }
}
