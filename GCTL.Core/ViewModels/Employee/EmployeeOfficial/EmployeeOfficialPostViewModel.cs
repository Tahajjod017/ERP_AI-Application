using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeOfficial
{
    public class EmployeeOfficialPostViewModel : BaseViewModel
    {
        public int EmployeePersonalId { get; set; }
        public string? PersonalPhone { get; set; }
        public string? PersonalEmail { get; set; }

        public int? EmployeeOfficeInfoID { get; set; }      

        public string? EmployeeOfficeId { get; set; }

        public int? OrganizationID { get; set; }

        public int? OrganizationBranchID { get; set; }

        public int? DepartmentID { get; set; }

        public int? DesignationID { get; set; }

        public int? EmployeeTypeID { get; set; }

        public int? EmploymentNatureID { get; set; }

        public int? SeniorSupervisorId { get; set; }

        public int? ImmediateSupervisorId { get; set; }

        public int? HeadOfDepartmentId { get; set; }

        public string OfficePhone { get; set; }

        public string OfficeEmail { get; set; }

        public string AttendanceId { get; set; }

        public int? EmploymentStatusId { get; set; }

        public string AppointmentLetterNo { get; set; }

        public DateOnly? AppointmentLetterIssueDate { get; set; }

        public DateOnly? JoiningDate { get; set; }

        public DateOnly? ProvisionPeriodStartDate { get; set; }

        public int? ProvisionPeriod { get; set; }

        public int? ProvisionPeriodTtimeTypeID { get; set; }

        public DateOnly? ConfirmationDate { get; set; }

        public string ConfirmationLetterNo { get; set; }

        public DateOnly? ContractEndDate { get; set; }
    }
}
