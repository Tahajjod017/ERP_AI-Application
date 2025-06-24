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


        public bool IsDesignationOrEmpFirstApprovalID { get; set; }
        public int? FirstApprovalID { get; set; }
        public string? FirstApprovalName { get; set; }


       
        public bool IsEnableSecondApproval { get; set; }
        public bool IsDesignationOrEmpSecondApprovalID { get; set; }
        public int? SecondApprovalID { get; set; }
        public string? SecondApprovalName { get; set; }



        public bool IsEnableThirdApproval { get; set; }
        public bool IsDesignationOrEmpThirdApprovalID { get; set; }
        public int? ThirdApprovalID { get; set; }
        public string? ThirdApprovalName { get; set; }

       
    }
}
