using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayRollOrganizationBenefitsType
{
    public class OrgaBenefitsTypeGetAllVM
    {
        public int BenefitTypeID { get; set; }
        public string? OrganizationName { get; set; }
        public string? BenefitTypeName { get; set; }
        public string? IsApplyOnGrossSalary { get; set; }
    }
}
