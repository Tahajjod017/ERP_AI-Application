using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeEducational
{
    public class EmployeeEducationGetViewModel : EmployeeBaseViewModel
    {
        public int EmployeeEducationalInfoID { get; set; }

        // public int? EmployeeID { get; set; }

        public string? EducationLevelID { get; set; }

        public string? DegreeID { get; set; }

        public string MajorSubject { get; set; }

        public string? EducationBoardID { get; set; }

        public string InstitutionName { get; set; }

        public string? ResultTypeID { get; set; }

        public string? PassingYearID { get; set; }

        public string YearDuration { get; set; }

        public string Achievement { get; set; }
        public bool IsActive { get; set; }
    }
}
