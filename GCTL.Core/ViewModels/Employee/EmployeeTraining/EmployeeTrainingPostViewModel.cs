using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeTraining
{
    public class EmployeeTrainingPostViewModel : EmployeeBaseViewModel
    {
        public int EmployeeTranningInfoID { get; set; }

        //public int? EmployeeID { get; set; }

        public string TranningTitle { get; set; }

        public int? CountryID { get; set; }

        public string TopicCovered { get; set; }

        public int? TrainingYearID { get; set; }

        public string InstituteName { get; set; }

        public string YearDuration { get; set; }

        public string LocationName { get; set; }
    }
}
