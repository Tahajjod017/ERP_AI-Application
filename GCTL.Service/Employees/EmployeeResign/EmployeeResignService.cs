using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;

namespace GCTL.Service.Employees.EmployeeResign
{
    public class EmployeeResignService : IEmployeeResign
    {

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;

        public EmployeeResignService(
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository,
            IGenericRepository<Departments> departmentRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _departmentRepository = departmentRepository;
        }

        public object GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate)
        {
            // Static data for demonstration (replace with actual database query)
            var resignations = new List<ResignationViewModel>
            {
                new ResignationViewModel { REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024" },
                new ResignationViewModel { REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024" },
                new ResignationViewModel { REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024" },
                new ResignationViewModel { REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024" },
                new ResignationViewModel { REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024" },
                new ResignationViewModel { REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024" },
                new ResignationViewModel { REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024" },
                new ResignationViewModel { REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024" },
                new ResignationViewModel { REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024" },
                new ResignationViewModel { REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024" },
                new ResignationViewModel { REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024" },
                new ResignationViewModel { REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024" },
                new ResignationViewModel { REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024" },
                new ResignationViewModel { REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024" },
                new ResignationViewModel { REmpName = "Jashim Uddin", REmpDept = "Application Development", ResignResons = "Entrepreneurial Pursuits", ResNoticeDate = "05/01/2024", ResinDate = "05/02/2024" }
            };

            // Apply date range filter
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime from = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                DateTime to = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                resignations = resignations
                    .Where(r => {
                        DateTime noticeDate = DateTime.ParseExact(r.ResNoticeDate, "dd/MM/yyyy", null);
                        return noticeDate >= from && noticeDate <= to;
                    })
                    .ToList();
            }

            // Apply sorting
            resignations = sortColumn switch
            {
                "rEmpName" => sortDirection == "asc" ? resignations.OrderBy(r => r.REmpName).ToList() : resignations.OrderByDescending(r => r.REmpName).ToList(),
                "rEmpDept" => sortDirection == "asc" ? resignations.OrderBy(r => r.REmpDept).ToList() : resignations.OrderByDescending(r => r.REmpDept).ToList(),
                "resignResons" => sortDirection == "asc" ? resignations.OrderBy(r => r.ResignResons).ToList() : resignations.OrderByDescending(r => r.ResignResons).ToList(),
                "resNoticeDate" => sortDirection == "asc" ? resignations.OrderBy(r => r.ResNoticeDate).ToList() : resignations.OrderByDescending(r => r.ResNoticeDate).ToList(),
                "resinDate" => sortDirection == "asc" ? resignations.OrderBy(r => r.ResinDate).ToList() : resignations.OrderByDescending(r => r.ResinDate).ToList(),
                _ => resignations
            };

            // Apply pagination
            var totalRecords = resignations.Count;
            var pagedData = resignations.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new
            {
                data = pagedData,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords
            };
        }



    }
}
