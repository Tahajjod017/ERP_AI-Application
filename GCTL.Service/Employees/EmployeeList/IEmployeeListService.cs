using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeListVM;


namespace GCTL.Service.Employees.EmployeeList
{
    public interface IEmployeeListService
    {
        Task<IQueryable<EmployeeListGetViewModel>> GetEmployees();
    }
}
