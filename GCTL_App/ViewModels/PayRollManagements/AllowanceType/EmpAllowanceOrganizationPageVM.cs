using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GCTL_App.ViewModels.PayRollManagements.AllowanceType
{
    public class EmpAllowanceOrganizationPageVM
    {
        public EmpAllowanceTypeOrganizationSaveVM Save {  get; set; }=new EmpAllowanceTypeOrganizationSaveVM();
    }
}
