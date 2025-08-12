using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace GCTL.Service.Employees.EmployeeResign
{
    public class EmployeeResignService : IEmployeeResign
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOffiRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Resignations> _resignationRepository;

        #region Static data storage for test
        private static List<ResignationViewModel> _resignations = new List<ResignationViewModel>
        {
            new ResignationViewModel {ResigId = 1, REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024", EmployeeId = 1, CompanyId = 1 },
            new ResignationViewModel {ResigId = 2, REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024", EmployeeId = 1, CompanyId = 1 },
            new ResignationViewModel {ResigId = 3, REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024", EmployeeId = 1, CompanyId = 1 },
            new ResignationViewModel {ResigId = 4, REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024", EmployeeId = 1, CompanyId = 1 },
            new ResignationViewModel {ResigId = 5, REmpName = "Tanvir Haider", REmpDept = "Finance", ResignResons = "Career Change", ResNoticeDate = "01/01/2024", ResinDate = "30/01/2024", EmployeeId = 1, CompanyId = 1 },
            new ResignationViewModel {ResigId = 6, REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024", EmployeeId = 2, CompanyId = 1 },
            new ResignationViewModel {ResigId = 7, REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024", EmployeeId = 2, CompanyId = 1 },
            new ResignationViewModel {ResigId = 8, REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024", EmployeeId = 2, CompanyId = 1 },
            new ResignationViewModel {ResigId = 9, REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024", EmployeeId = 2, CompanyId = 1 },
            new ResignationViewModel {ResigId = 10, REmpName = "Hasan Tarek", REmpDept = "UI / UX", ResignResons = "Health Reasons", ResNoticeDate = "05/08/2024", ResinDate = "30/12/2024", EmployeeId = 2, CompanyId = 1 },
            new ResignationViewModel {ResigId = 11, REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024", EmployeeId = 3, CompanyId = 1 },
            new ResignationViewModel {ResigId = 12, REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024", EmployeeId = 3, CompanyId = 1 },
            new ResignationViewModel {ResigId = 13, REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024", EmployeeId = 3, CompanyId = 1 },
            new ResignationViewModel {ResigId = 14, REmpName = "Osman Goni", REmpDept = "Marketing", ResignResons = "Personal Development", ResNoticeDate = "25/02/2024", ResinDate = "30/04/2024", EmployeeId = 3, CompanyId = 1 },
            new ResignationViewModel {ResigId = 15, REmpName = "Jashim Uddin", REmpDept = "Application Development", ResignResons = "Entrepreneurial Pursuits", ResNoticeDate = "05/01/2024", ResinDate = "05/02/2024", EmployeeId = 4, CompanyId = 1 }
        };

        private static int _nextId = 16;
        #endregion

        public EmployeeResignService(
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOffiRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Resignations> resignationRepository)
        {
            _employeeRepository = employeeRepository;
            _employeeOffiRepository = employeeOffiRepository;
            _departmentRepository = departmentRepository;
            _resignationRepository = resignationRepository;
        }

        public object GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate, string imgSrcThumb)
        {
            //var resignations = new List<ResignationViewModel>(_resignations);
            var resignations = _resignationRepository.AllActive().Include(e=>e.Employee)
                .ThenInclude(r=>r.EmployeeOfficeInfoEmployee).ThenInclude(p=>p.Department).Select(e=> new ResignationViewModel()
                {
                    ResigId = e.ResignationID,
                    REmpDept = e.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
                    REmpName = e.Employee.FirstName + " " + e.Employee.LastName,
                    ResignResons = e.Reason,
                    ResinDate = e.ResignationDate.Value.ToString("dd/MM/yyyy"),
                    ResNoticeDate = e.NoticeDate.Value.ToString("dd/MM/yyyy"),
                    EmployeeId = e.EmployeeID,
                    CompanyId = e.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().OrganizationID,
                    Image = imgSrcThumb + e.Employee.EmployeeImageFileName

                }).ToList();



            // Apply date range filter
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime from = DateTime.ParseExact(fromDate, "dd/MM/yyyy", null);
                DateTime to = DateTime.ParseExact(toDate, "dd/MM/yyyy", null);
                resignations = resignations
                    .Where(r => {
                        DateTime noticeDate = DateTime.ParseExact(r.ResinDate, "dd/MM/yyyy", null);
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

        public CommonReturnViewModel InsertResignation(ResignationPostViewModel model)
        {
            try
            {
                var result = new CommonReturnViewModel()
                {
                    Success = false,

                };

                if (!DateTime.TryParseExact(model.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out DateTime noticeDate))
                {
                    result.Message = "Invalid Notice Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                if (!DateTime.TryParseExact(model.ResignationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out DateTime resignationDate))
                {
                    result.Message = "Invalid Resignation Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }


                var emp = _employeeRepository.AllActive().FirstOrDefault(e => e.EmployeeID == model.EmployeeId);

                if (emp == null)
                {
                    result.Message = "Employee is invalid";
                    return result;
                }


                var newResignation = new Resignations
                {
                    OrganizationID = model.CompanyId,
                    EmployeeID = model.EmployeeId,
                    ResignationDate = resignationDate,
                    NoticeDate = noticeDate,
                    Reason = model.Reason,


                };

                _resignationRepository.AddAsync(newResignation, model);
                result.Success = true;
                result.Message = "Saved successfull";
                return result;

            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public bool UpdateResignation(int resignationId, ResignationPostViewModel model)
        {
            try
            {
                var existingResignation = _resignations.FirstOrDefault(r => r.ResigId == resignationId);
                if (existingResignation == null)
                    return false;

                // Get employee name and department
                var employee = GetEmployeeById(model.EmployeeId);
                var department = GetDepartmentByEmployeeId(model.EmployeeId);

                // Update the resignation
                existingResignation.EmployeeId = model.EmployeeId;
                existingResignation.CompanyId = model.CompanyId;
                existingResignation.REmpName = employee?.Name ?? existingResignation.REmpName;
                existingResignation.REmpDept = department ?? existingResignation.REmpDept;
                existingResignation.ResignResons = model.Reason;
                existingResignation.ResNoticeDate = model.NoticeDate ?? existingResignation.ResNoticeDate;
                existingResignation.ResinDate = model.ResignationDate ?? existingResignation.ResinDate;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool DeleteResignation(int resignationId)
        {
            try
            {
                var resignation = _resignations.FirstOrDefault(r => r.ResigId == resignationId);
                if (resignation == null)
                    return false;

                _resignations.Remove(resignation);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public ResignationViewModel GetResignationById(int resignationId)
        {
            var resign = _resignationRepository.AllActive().Where(e=>e.ResignationID == resignationId).Select(r=> new ResignationViewModel
            {
                ResigId = r.ResignationID,
                ResignResons = r.Reason,
                ResinDate = r.ResignationDate.Value.ToString("dd/MM/yyyy"),
                ResNoticeDate = r.NoticeDate.Value.ToString("dd/MM/yyyy"),
                EmployeeId = r.EmployeeID,
                CompanyId = r.OrganizationID,

            }).FirstOrDefault();

            if (resign == null)
                return new ResignationViewModel();

            return resign;
        }

        // Helper methods to simulate getting employee and department info
        private EmployeeInfo GetEmployeeById(int employeeId)
        {
            // Static employee data
            var employees = new List<EmployeeInfo>
            {
                new EmployeeInfo { Id = 1, Name = "Tanvir Haider", DepartmentId = 1 },
                new EmployeeInfo { Id = 2, Name = "Hasan Tarek", DepartmentId = 2 },
                new EmployeeInfo { Id = 3, Name = "Osman Goni", DepartmentId = 3 },
                new EmployeeInfo { Id = 4, Name = "Jashim Uddin", DepartmentId = 4 },
                new EmployeeInfo { Id = 5, Name = "Ahmed Rahman", DepartmentId = 1 },
                new EmployeeInfo { Id = 6, Name = "Sarah Khan", DepartmentId = 2 }
            };
            return employees.FirstOrDefault(e => e.Id == employeeId);
        }

        private string GetDepartmentByEmployeeId(int employeeId)
        {
            var departments = new Dictionary<int, string>
            {
                { 1, "Finance" },
                { 2, "UI / UX" },
                { 3, "Marketing" },
                { 4, "Application Development" },
                { 5, "Human Resources" },
                { 6, "Operations" }
            };

            var employee = GetEmployeeById(employeeId);
            if (employee != null && departments.ContainsKey(employee.DepartmentId))
            {
                return departments[employee.DepartmentId];
            }
            return "Unknown Department";
        }

        // Helper class for employee info
        private class EmployeeInfo
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int DepartmentId { get; set; }
        }
    }
}