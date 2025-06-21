using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;

namespace GCTL.Service.Employees.EmployeeDetails
{
    public interface IEmployeeDetailsService
    {
        Task<CommonReturnViewModel> GetBasicDetail(int empID, string imgURL);
    }
}
