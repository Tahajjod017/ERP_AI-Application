using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.Degree
{
    public class DegreeVM : BaseViewModel
    {
        public int DegreeID { get; set; }

        public string DegreeName { get; set; }

        //public virtual ICollection<EmployeeEducation> EmployeeEducation { get; set; } = new List<EmployeeEducation>();
    }
}
