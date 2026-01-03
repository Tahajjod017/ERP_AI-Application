using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ServiceExtensions;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeBulkRegister;
using GCTL.Data.Models;
using GCTL.Service.Employees.EmployeePersonal;
using GCTL.Service.MasterSetup.Statuse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using static System.Net.WebRequestMethods;

namespace GCTL.Service.Employees.EmployeeBulkRegister
{

    public class EmployeeBulkRegisterService : IEmployeeBulkRegisterService
    {
        private readonly IHttpContextAccessor _http;
        private readonly IHubContext<EmployeeUploadHub> _hubContext;
        // Add your repository/dbcontext here
         private readonly IEmployeePersonalService _employeeService;
         private readonly IStatusService _statusService;
        // private readonly ApplicationDbContext _context;

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.EmployeeOfficeInfo> _employeeOfficialRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;
        private readonly IGenericRepository<Designations> _designationRepository;
        private readonly IGenericRepository<Genders> _genderRepository;
        private readonly IGenericRepository<OrganizationBranches> _branchRepository;

        public EmployeeBulkRegisterService(
            IHttpContextAccessor http,
            IHubContext<EmployeeUploadHub> hubContext,
            IGenericRepository<Data.Models.Employees> employeeRepository,
            IGenericRepository<EmployeeOfficeInfo> employeeOfficialRepository,
            IGenericRepository<Departments> departmentRepository,
            IGenericRepository<Designations> designationRepository,
            IGenericRepository<Genders> genderRepository,
            IEmployeePersonalService employeeService,
            IGenericRepository<OrganizationBranches> branchRepository,
            IStatusService statusService)
        {
            _http = http;
            _hubContext = hubContext;
            _employeeRepository = employeeRepository;
            _employeeOfficialRepository = employeeOfficialRepository;
            _departmentRepository = departmentRepository;
            _designationRepository = designationRepository;
            _genderRepository = genderRepository;
            _employeeService = employeeService;
            _branchRepository = branchRepository;
            _statusService = statusService;
        }

        public byte[] GenerateExcelTemplate()
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Employee Upload");

            string[] headers =
            {
                "Employee Code", "First Name", "Last Name", "Gender", "Email",
                "Official Email", "Phone Number", "Branch", "Joining Date",
                "Comment (if Any)", "Designation", "Department Name",
                "Immediate Supervisor Name", "Department Head Name"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                sheet.Cells[1, i + 1].Value = headers[i];
                sheet.Cells[1, i + 1].Style.Font.Bold = true;
                sheet.Column(i + 1).Width = 22;
            }

            sheet.View.FreezePanes(2, 1);

            return package.GetAsByteArray();
        }

        public EmpPagedResult<EmployeeExcelVM> GetPreviewData(int page, int pageSize, string search)
        {
            var session = _http.HttpContext.Session;

            var allData = session.GetObject<List<EmployeeExcelVM>>("ExcelEmployees")
                          ?? new List<EmployeeExcelVM>();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower().Trim();

                allData = allData.Where(x =>
                       (x.EmployeeCode ?? "").ToLower().Contains(search)
                    || (x.FirstName ?? "").ToLower().Contains(search)
                    || (x.LastName ?? "").ToLower().Contains(search)
                    || (x.Email ?? "").ToLower().Contains(search)
                    || (x.PhoneNumber ?? "").ToLower().Contains(search)
                    || (x.DepartmentName ?? "").ToLower().Contains(search)
                    || (x.Designation ?? "").ToLower().Contains(search)
                    || (x.Branch ?? "").ToLower().Contains(search)
                ).ToList();
            }

            var total = allData.Count;

            // Apply pagination
            var data = allData
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new EmpPagedResult<EmployeeExcelVM>
            {
                Data = data,
                PageNumber = page,
                PageSize = pageSize,
                TotalRecords = total
            };
        }

        public DuplicateCheckResult CheckDuplicates(List<EmployeeExcelVM> employees)
        {
            var result = new DuplicateCheckResult
            {
                DuplicateEmployeeCodes = new List<string>(),
                DuplicateEmails = new List<string>()
            };

            // Check for duplicate Employee Codes within the uploaded list
            var employeeCodes = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.EmployeeCode))
                .GroupBy(e => e.EmployeeCode.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            result.DuplicateEmployeeCodes.AddRange(employeeCodes);

            // Check for duplicate Emails within the uploaded list
            var emails = employees
                .Where(e => !string.IsNullOrWhiteSpace(e.Email))
                .GroupBy(e => e.Email.ToLower())
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            result.DuplicateEmails.AddRange(emails);

            // TODO: Check against existing database records
            // Example:
            //var existingCodes = await _employeeRepository.AllActive()
            //    .Where(e => employees.Select(x => x.EmployeeCode).Contains(e.EmployeeCode))
            //    .Select(e => e.EmployeeCode.ToLower())
            //    .ToListAsync();
            //result.DuplicateEmployeeCodes.AddRange(existingCodes.Except(result.DuplicateEmployeeCodes));

            //var existingEmails = await _context.Employees
            //    .Where(e => employees.Select(x => x.Email).Contains(e.Email))
            //    .Select(e => e.Email.ToLower())
            //    .ToListAsync();
            //result.DuplicateEmails.AddRange(existingEmails.Except(result.DuplicateEmails));

            result.HasDuplicates = result.DuplicateEmployeeCodes.Any() || result.DuplicateEmails.Any();

            return result;
        }

        public async Task<CommonReturnViewModel> SaveEmployeeDataAsync(List<EmployeeExcelVM> employees, BaseViewModel? baseView, int? orgaId)
        {
            var result = new CommonReturnViewModel();
            var connectionId = _http.HttpContext.Request.Headers["X-SignalR-ConnectionId"].ToString();

            try
            {
                int totalCount = employees.Count;
                int successCount = 0;
                int failCount = 0;
                var errors = new List<string>();

                await SendProgress(connectionId, 0, totalCount, "Starting employee registration...");

                // Process in batches for better performance
                int batchSize = 50;
                for (int batchStart = 0; batchStart < employees.Count; batchStart += batchSize)
                {
                    var batch = employees.Skip(batchStart).Take(batchSize).ToList();

                    foreach (var employee in batch)
                    {
                        int currentIndex = batchStart + batch.IndexOf(employee);

                        try
                        {
                           
                            var validationErrors = ValidateEmployee(employee);

                            if (validationErrors.Any())
                            {
                                failCount++;
                                errors.Add($"Row {currentIndex + 2}: {string.Join(", ", validationErrors)}");
                                continue;
                            }

                            if (employee.Email != "" || employee.EmployeeCode != "")
                            {
                                // TODO: Check for duplicates in database
                                var existingEmployee = await _employeeRepository.AllActive()
                                    .Where(e => e.EmployeeCode == employee.EmployeeCode || e.Email == employee.Email)
                                    .FirstOrDefaultAsync();

                                var existingEmployeeOff = await _employeeOfficialRepository.AllActive()
                                    .Where(e => e.OfficePhone == employee.PhoneNumber || e.OfficeEmail == employee.OfficialEmail)
                                    .FirstOrDefaultAsync();

                                if (existingEmployee != null || existingEmployeeOff != null)
                                {
                                    failCount++;
                                    errors.Add($"Row {currentIndex + 2}: Duplicate employee code or email or phone found");
                                    continue;
                                }
                            }

                           


                            var GenderId = await GetGenderId(employee.Gender , baseView);

                            var empCode = await GetEmployeeCode(employee.EmployeeCode);

                            // TODO: Save to database
                            // Example:
                            var newEmployee = new GCTL.Data.Models.Employees
                            {
                                EmployeeCode = empCode,
                                FirstName = employee.FirstName,
                                LastName = employee.LastName,
                                Email = employee.Email,
                                GenderID = GenderId != 0 ? GenderId : null,
                                CreatedBy = baseView?.CreatedBy ?? null,
                                CreatedAt = DateTime.Now,
                                LIP = baseView?.LIP ?? "",
                                LMAC = baseView?.LMAC ?? "",
                                IsActive = true
                                
                            };

                            await _employeeRepository.AddAsync(newEmployee);


                            var designation = await GetDesignation(employee.Designation, baseView);
                            var department = await GetDepartment(employee.DepartmentName , baseView);
                            var branch = await GetBranch(employee.Branch , baseView, orgaId);
                            var ststus = await _statusService.GetStatusIDAsync("Active");

                            var sup = await GetImmSup(employee.ImmediateSupervisorName);
                            var dep = await GetDepHead(employee.DepartmentHeadName);

                            var empOfficial = new EmployeeOfficeInfo()
                            {
                                EmployeeID = newEmployee.EmployeeID,
                                OfficeEmail = employee.OfficialEmail,
                                OfficePhone = employee.PhoneNumber,
                                JoiningDate = employee.JoiningDate.HasValue ? DateOnly.FromDateTime(employee.JoiningDate.Value) : (DateOnly?)null,

                                OrganizationID = orgaId,
                                OrganizationBranchID = branch != 0 ? branch : null,
                                DesignationID = designation != 0 ? designation : null,
                                DepartmentID = department != 0 ? department : null,

                                ImmediateSupervisorId = sup,
                                HeadOfDepartmentId = dep,

                                EmploymentStatusId = ststus

                            };

                            await _employeeOfficialRepository.AddAsync(empOfficial, baseView);


                          

                            successCount++;
                        }
                        catch (Exception ex)
                        {
                            failCount++;
                            errors.Add($"Row {currentIndex + 2} ({employee.EmployeeCode}): {ex.Message}");
                        }

                        // Send progress update
                        int progress = (int)((currentIndex + 1) / (double)totalCount * 100);
                        await SendProgress(connectionId, currentIndex + 1, totalCount,
                            $"Processing: {currentIndex + 1}/{totalCount} employees");
                    }

                    // Optional: Commit batch to database here for better performance
                    // await _context.SaveChangesAsync();
                }

                // Send completion message
                await SendProgress(connectionId, totalCount, totalCount, $"Completed! Success: {successCount}, Failed: {failCount}");

                result.Success = successCount > 0;
                result.Message = $"Registration completed. Success: {successCount}, Failed: {failCount}";

                if (errors.Any())
                {
                    result.Message += $"\n\nErrors:\n{string.Join("\n", errors.Take(10))}";
                    if (errors.Count > 10)
                        result.Message += $"\n... and {errors.Count - 10} more errors";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error during bulk registration: {ex.Message}";
                await SendProgress(connectionId, 0, 0, $"Error: {ex.Message}");
            }

            return result;
        }

        private async Task<int?> GetDepHead(string departmentHeadName)
        {
            if (departmentHeadName == null || departmentHeadName == "")
            {
                return null;
            }

            var emp = await _employeeRepository.AllActive().Where(e => e.FirstName.ToLower() + " " + e.LastName.ToLower() == departmentHeadName.ToLower()).FirstOrDefaultAsync();

            if (emp != null)
            {
                return emp.EmployeeID;
            }
            else
            {
               return null;
            }
            
        }

        private async Task<int?> GetImmSup(string immediateSupervisorName)
        {
            if (immediateSupervisorName == null || immediateSupervisorName == "")
            {
                return null;
            }

            var emp = await _employeeRepository.AllActive().Where(e => e.FirstName.ToLower() + " " + e.LastName.ToLower() == immediateSupervisorName.ToLower()).FirstOrDefaultAsync();

            if (emp != null)
            {
                return emp.EmployeeID;
            }
            else
            {
                return null;
            }
        }

        private async Task<int> GetBranch(string branchName, BaseViewModel? baseView, int? orgaId)
        {
            if (branchName == null || branchName == "")
            {
                return 0;
            }

            var branch = await _branchRepository.AllActive().FirstOrDefaultAsync(e=>e.OrganizationID == orgaId && e.OrganizationBranchName == branchName);

            if (branch == null)
            {
                branch = new OrganizationBranches() 
                {
                    OrganizationBranchName = branchName,
                    OrganizationID = orgaId
                };
                await _branchRepository.AddAsync(branch, baseView);

            }

            return branch.OrganizationBranchID;

        }

        private async Task<int> GetDepartment(string departmentName, BaseViewModel? baseView)
        {
            if (departmentName == null || departmentName == "")
            {
                return 0;
            }

            var dept = await _departmentRepository.AllActive().FirstOrDefaultAsync(e=>e.DepartmentName == departmentName);

            if (dept == null)
            {
                dept = new Departments()
                {
                    DepartmentName = departmentName,

                };
                await _departmentRepository.AddAsync(dept, baseView);
            }
            return dept.DepartmentID;
        }

        private async Task<int> GetDesignation(string designation, BaseViewModel? baseView)
        {
            if (designation == null || designation == "")
            {
                return 0;
            }


            var desig = await _designationRepository.AllActive().FirstOrDefaultAsync(e=>e.DesignationName == designation);
            if (desig == null)
            {
                desig = new Designations()
                {
                    DesignationName = designation,

                };
                await _designationRepository.AddAsync(desig, baseView);
                
            }
            
            return desig.DesignationID;

        }

        private async Task<string> GetEmployeeCode(string employeeCode)
        {
            if (employeeCode == null || employeeCode == "")
            {
                var code = await _employeeService.GetEmployeeCode();
                return code;
            }
            else
            {
                var exists = await _employeeRepository.All().FirstOrDefaultAsync(e => e.EmployeeCode == employeeCode);

                if (exists == null)
                {
                    return employeeCode;
                }
                else
                {
                    var code = await _employeeService.GetEmployeeCode();
                    return code;
                }
            }
        }

        private async Task<int> GetGenderId(string genderName, BaseViewModel? baseView)
        {
            if (genderName == null || genderName == "")
            {
                return 0;
            }

            var gender = await _genderRepository.AllActive().FirstOrDefaultAsync(e => e.GenderName == genderName);

            if (gender == null)
            {
                gender = new Genders()
                {
                    GenderName = genderName
                };
                await _genderRepository.AddAsync(gender, baseView);
            }

            return gender.GenderID;
        }

        private List<string> ValidateEmployee(EmployeeExcelVM employee)
        {
            var errors = new List<string>();

            //if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
            //    errors.Add("Employee Code is required");

            //if (string.IsNullOrWhiteSpace(employee.FirstName))
            //    errors.Add("First Name is required");

            //if (string.IsNullOrWhiteSpace(employee.Email))
            //    errors.Add("Email is required");
            //else if (!IsValidEmail(employee.Email))
            //    errors.Add("Invalid email format");

            //if (string.IsNullOrWhiteSpace(employee.DepartmentName))
            //    errors.Add("Department is required");

            //if (string.IsNullOrWhiteSpace(employee.Designation))
            //    errors.Add("Designation is required");

            return errors;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task SendProgress(string connectionId, int current, int total, string message)
        {
            if (!string.IsNullOrEmpty(connectionId))
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveProgress", new
                {
                    current,
                    total,
                    percentage = total > 0 ? (int)((current / (double)total) * 100) : 0,
                    message
                });
            }
        }
    }



}
