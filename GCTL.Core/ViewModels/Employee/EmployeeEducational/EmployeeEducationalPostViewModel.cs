using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeEducational
{
    public class EmployeeEducationalPostViewModel : EmployeeBaseViewModel
    {
        public int EmployeeEducationalInfoID { get; set; }

       // public int? EmployeeID { get; set; }

        public int? EducationLevelID { get; set; }

        public int? DegreeID { get; set; }

        public string MajorSubject { get; set; }

        public int? EducationBoardID { get; set; }

        public string InstitutionName { get; set; }

        public int? ResultTypeID { get; set; }

        public int? PassingYearID { get; set; }

        public string YearDuration { get; set; }

        public string Achievement { get; set; }
    }
}
