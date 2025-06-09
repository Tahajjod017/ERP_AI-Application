using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeListVM;
using GCTL.Data.Models;
using GCTL.Service.UserProfile;

namespace GCTL.Service.Employees.EmployeeList
{
    public class EmployeeListService : IEmployeeListService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeePersonalRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Departments> _departmentRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Designations> _designationRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Statuses> _statusRepository;

        

        public EmployeeListService(IGenericRepository<Data.Models.Employees> employeePersonalRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository, IGenericRepository<Departments> departmentRepository, IGenericRepository<Designations> designationRepository, IGenericRepository<Statuses> statusRepository)
        {
            _employeePersonalRepository = employeePersonalRepository;
            _employeeOfficialRepository = employeeOfficialRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _statusRepository = statusRepository;
            
        }

        public async Task<IQueryable<EmployeeListGetViewModel>> GetEmployees()
        {
            var employees = await Task.Run(() =>
                from emp in _employeePersonalRepository.AllActive()
                join office in _employeeOfficialRepository.AllActive()
                on emp.EmployeeID equals office.EmployeeID into empOffice
                from office in empOffice.DefaultIfEmpty()
                join dept in _departmentRepository.AllActive()
                on office.DepartmentID equals dept.DepartmentID into empDept
                from dept in empDept.DefaultIfEmpty()
                join desig in _designationRepository.AllActive()
                on office.DesignationID equals desig.DesignationID into empDesig
                from desig in empDesig.DefaultIfEmpty()
                join status in _statusRepository.AllActive()
                on office.EmploymentStatusId equals status.StatusID into empStatus
                from status in empStatus.DefaultIfEmpty()
                select new EmployeeListGetViewModel
                {

                    Id = emp.EmployeeID,
                    Name = emp.FirstName + " " + emp.LastName,
                    Department = dept != null ? dept.DepartmentName : "-",
                   // Designation = desig != null ? desig.DesignationName : "N/A"
                    JoiningDate =  office.JoiningDate ,
                    Email = emp.Email,
                    Phone = emp.MobileNumber,
                    Status = status != null ? status.StatusName : "-",
                    Avatar = emp.EmployeeImageFileName 
                });

            return employees.AsQueryable();
        }
    }
}
