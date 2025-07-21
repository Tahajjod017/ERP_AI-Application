using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class ApprovalSettingsVM:BaseViewModel
    {
        public int ApprovalSettingID { get; set; }

        public int? OrganizationID { get; set; }
        public string? OrganizationName { get; set; }

        public int? OrganizationBranchID { get; set; }

        public int? ApprovalTypeID { get; set; }
        public string? ApprovalTypeName { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }


        public string? IsDesignationOrEmpFirstApprovalID { get; set; }
        public string? FirstApprovalID { get; set; }
        public string? FirstApprovalName { get; set; }


       
        public string? IsEnableSecondApproval { get; set; }
        public string? IsDesignationOrEmpSecondApprovalID { get; set; }
        public string? SecondApprovalID { get; set; }
        public string? SecondApprovalName { get; set; }



        public string? IsEnableThirdApproval { get; set; }
        public string? IsDesignationOrEmpThirdApprovalID { get; set; }
        public string? ThirdApprovalID { get; set; }
        public string? ThirdApprovalName { get; set; }


        public string? AllowSelfApproval { get; set; }

        public string? SelfExceptionApprovalID { get; set; }
        public string? SelfExceptionApprovalName { get; set; }

    }
}
