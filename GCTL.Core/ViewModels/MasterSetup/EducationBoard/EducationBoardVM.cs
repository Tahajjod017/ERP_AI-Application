using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.MasterSetup.EducationBoard
{
    public class EducationBoardVM : BaseViewModel
    {
        public int EducationBoardID { get; set; }

        public string EducationBoardName { get; set; }

        //public virtual ICollection<EmployeeEducation> EmployeeEducation { get; set; } = new List<EmployeeEducation>();
    }
}
