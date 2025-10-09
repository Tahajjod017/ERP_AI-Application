using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AttendanceManagement.LeaveManagements.LeaveRequest;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy;

using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmployeeUpdateVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollPolicy
{
    public interface IEmployeeBenefitsService
    {
        Task<CommonReturnViewModel> SaveEmployeeBenefitsAsync(EmployeeBenefitsVM entityVM);
        Task<List<CommonSelectVMM>> SelectAsync(int id);

    }
}
