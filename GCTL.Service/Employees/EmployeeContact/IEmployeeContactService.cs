using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeContact;
using GCTL.Core.ViewModels;

namespace GCTL.Service.Employees.EmployeeContact
{
    public interface IEmployeeContactService
    {
        Task<CommonReturnViewModel> SaveAsync(EmployeeContactViewModel model);
        Task<CommonReturnViewModel> DeleteAsync(int id);
        Task<List<EmployeeContactGetViewModel>> GetEmployeeContactByIdAsync(int id);
        Task<EmployeeContactViewModel> GetEmployeeContactData(int id);
        Task<CommonReturnViewModel> UpdateAsync(EmployeeContactViewModel model);
    }
}
