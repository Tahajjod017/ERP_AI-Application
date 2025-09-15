using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayRollOrganizationBenefitsType
{
    public class OrgaBenefitsTypeSaveVM:BaseViewModel
    {
        public int BenefitTypeID { get; set; }
        public List<int>? OrganizatonIDs { get; set; }
        public int? OrganizatonID { get; set; }
        public string ? BenefitTypeName { get; set; }
        public bool ApplyOnBasicSalary { get; set; }
        public bool ApplyOnGrossSalary { get; set; }
    }
}
