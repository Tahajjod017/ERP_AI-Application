using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeNavigtion;

namespace GCTL.Service.Employees.EmployeeNavigation
{
    public interface IEmployeeNavigationService
    {
        EmployeeNavigationViewModel GetEmployeeNavigation(string activeTab = "", string activeSubTab = "");
        Task<EmployeeNavigationViewModel> GetEmployeeNavigationAsync(string activeTab = "", string activeSubTab = "");
        EmployeeNavigationViewModel GetEmployeeNavigationFromDatabase(string userId, string activeTab = "", string activeSubTab = "");
    }
}
