using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeAdditional
{
    public class EmployeeAdditionalPostViewModel : EmployeeBaseViewModel
    {
        public int EmployeeAdditionalInfoID { get; set; }

        //public int? EmployeeID { get; set; }

        public string PasportName { get; set; }

        public string PasportNo { get; set; }

        public string PasportPlaceOfIssue { get; set; }

        public DateTime? PasportIssueDate { get; set; }

        public DateTime? PasportExpireDate { get; set; }

        public string DrivingLicenceNo { get; set; }

        public int? LicenceTypeID { get; set; }

        public DateTime? DrivingLicenceIssueDate { get; set; }

        public DateTime? DrivingLicenceExpireDate { get; set; }

        public string SymbolOfVehicleClass { get; set; }

        public string DrivingLicencePlaceOfIssue { get; set; }

        public string WorkPermaitNumber { get; set; }

        public string WorkPermitType { get; set; }

        public DateTime? WorkPermitEffectiveDate { get; set; }

        public DateTime? WorkPermitExpireDate { get; set; }

        public DateTime? VisaExpireDate { get; set; }
    }
}
