using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollEmpAllowance
{
    public class PayRollEmpAllowanceSaveVM : BaseViewModel
    {

        public int? OrganizationID { get; set; }
        public decimal? Value { get; set; }
        public int? EmployeeAllowanceTypeID { get; set; }
        public decimal? MedicalAllowanceRate { get; set; }

        public int? MediAllowDepOnSalaryTypeID { get; set; }

        public bool IsConveyanceAllowanceEnabled { get; set; }

        public bool IsActive { get; set; }
        public decimal? ConveyanceAllowanceRate { get; set; }

        public int? ConAllowDepOnSalaryTypeID { get; set; }
       
        public List<HouseRentAllowanceDetailVM> HouseRentAllowances { get; set; } = new();
    }
    public class HouseRentAllowanceDetailVM
    {
      

        public decimal? SalaryMin { get; set; }

        public decimal? SalaryMax { get; set; }

        public int? CalculationTypeID { get; set; }

        public decimal? Value { get; set; }

        public DateTime? EffectiveDate { get; set; }

    }

}
