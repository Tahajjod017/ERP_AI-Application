using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;

namespace GCTL.Service.Employees.EmployeeResign
{
    public class EmployeeResignService : IEmployeeResign
    {
        #region CTOR

        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
      
        private readonly IGenericRepository<Resignations> _resignationRepository;

        

        private readonly IGenericRepository<EmployeeActionTypes> _employeeActionTypeRepository;
        private readonly IGenericRepository<ResignationsApprovalHistory> _resignationsApprovalHistoryRepository;

        private readonly IGenericRepository<Departments> _deptRepository;
        private readonly IGenericRepository<Designations> _desigRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _empOfficialRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        private readonly IGenericRepository<ApprovalDesignation> _approvalDesignationRepository;
        private readonly IGenericRepository<Alerts> alertsRepository;
        private readonly IGenericRepository<AlertForEmployee> alertForEmployeeRepository;

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

        public EmployeeResignService(IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<Resignations> resignationRepository, IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository, IGenericRepository<ResignationsApprovalHistory> resignationsApprovalHistoryRepository, IGenericRepository<Departments> deptRepository, IGenericRepository<Designations> desigRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<EmployeeOfficeInfo> empOfficialRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<ApprovalDesignation> approvalDesignationRepository, IGenericRepository<Alerts> alertsRepository, IGenericRepository<AlertForEmployee> alertForEmployeeRepository)
        {
            _employeeRepository = employeeRepository;
            _resignationRepository = resignationRepository;
            _employeeActionTypeRepository = employeeActionTypeRepository;
            _resignationsApprovalHistoryRepository = resignationsApprovalHistoryRepository;
            _deptRepository = deptRepository;
            _desigRepository = desigRepository;
            _statusRepository = statusRepository;
            _empOfficialRepository = empOfficialRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _approvalDesignationRepository = approvalDesignationRepository;
            this.alertsRepository = alertsRepository;
            this.alertForEmployeeRepository = alertForEmployeeRepository;
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

        #endregion

       

        #endregion

        #region Get All 
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

        #endregion

        #region Create 

        public async Task<CommonReturnViewModel> InsertResignation(ResignationPostViewModel model)
        {
            #region validato

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

            #endregion


            try
            {
                bool AllowSecondApproval;
                bool AllowThirdApproval;
                bool AllowSelfApproval;
                int firstApproverId = 0;
                int secondApproverId = 0;
                int thirdApproverId = 0;
                int selfApprovalExceptionId = 0;

                #region Action Type and Status Setup



                var status = await _statusRepository.AllActive()
                    .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "pending");
                if (status == null)
                {
                    status = new Statuses
                    {
                        StatusName = "Pending",
                        StatusType = "EmployeeCareerChange",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _statusRepository.AddAsync(status);
                }
                #endregion

                #region Employee Validation

                //var test = await _empOfficialRepository.AllActive().ToListAsync();

                //var employee = test.FirstOrDefault(e => e.EmployeeID == model.EmployeeId);

                var employee = await _empOfficialRepository.AllActive().Where(e => e.EmployeeID == model.EmployeeId).FirstOrDefaultAsync();

                var employeeInfo = await _employeeRepository.AllActive().Where(e => e.EmployeeID == model.EmployeeId).FirstOrDefaultAsync();


                if (employee == null || employeeInfo == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };
                }
                #endregion

                #region Approval Person Determination

                var approvalType = await _approvalTypeRepository.AllActive()
                    .FirstOrDefaultAsync(a => a.ApprovalTypeName.Trim().ToLower() == "resignation approval");
                if (approvalType == null)
                {
                    approvalType = new ApprovalTypes()
                    {
                        ApprovalTypeName = "Resignation Approval",
                        //OrganizationID = employee.OrganizationID,
                        //OrganizationBranchID = employee.OrganizationBranchID
                    };

                    await _approvalTypeRepository.AddAsync(approvalType, model);

                    
                }

                var approvalSettings = await _approvalSettingRepository.AllActive()
                    .FirstOrDefaultAsync(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                        && a.OrganizationID == employee.OrganizationID
                        && a.OrganizationBranchID == employee.OrganizationBranchID);
                if (approvalSettings == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Approval settings not found" };
                }

                int initialApproverId = 0;
                int initialStage = 1;

                // Fill boolean fields from approval settings
                AllowSecondApproval = approvalSettings.IsEnableSecondApproval;
                AllowThirdApproval = approvalSettings.IsEnableThirdApproval;
                AllowSelfApproval = approvalSettings.AllowSelfApproval ?? true;
                selfApprovalExceptionId = approvalSettings.SelfExceptionApprovalID ?? 0;

                bool isSelfApplication = model.CreatedBy == model.EmployeeId; // Check if applying for self

                // Determine first approver
                if (approvalSettings.IsDesignationOrEmpFirstApprovalID)
                {
                    var approvalDesig = await _approvalDesignationRepository.AllActive()
                        .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approvalSettings.FirstApprovalID);
                    firstApproverId = approvalDesig?.Code switch
                    {
                        1 => employee.ImmediateSupervisorId ?? 0,
                        2 => employee.SeniorSupervisorId ?? 0,
                        3 => employee.HeadOfDepartmentId ?? 0,
                        _ => 0
                    };
                }
                else
                {
                    firstApproverId = approvalSettings.FirstApprovalID ?? 0;
                }

                // Determine second approver if enabled
                if (AllowSecondApproval)
                {
                    if (approvalSettings.IsDesignationOrEmpSecondApprovalID)
                    {
                        var approvalDesig = await _approvalDesignationRepository.AllActive()
                            .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approvalSettings.SecondApprovalID);
                        secondApproverId = approvalDesig?.Code switch
                        {
                            1 => employee.ImmediateSupervisorId ?? 0,
                            2 => employee.SeniorSupervisorId ?? 0,
                            3 => employee.HeadOfDepartmentId ?? 0,
                            _ => 0
                        };
                    }
                    else
                    {
                        secondApproverId = approvalSettings.SecondApprovalID ?? 0;
                    }
                }

                // Determine third approver if enabled
                if (AllowThirdApproval)
                {
                    if (approvalSettings.IsDesignationOrEmpThirdApprovalID)
                    {
                        var approvalDesig = await _approvalDesignationRepository.AllActive()
                            .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approvalSettings.ThirdApprovalID);
                        thirdApproverId = approvalDesig?.Code switch
                        {
                            1 => employee.ImmediateSupervisorId ?? 0,
                            2 => employee.SeniorSupervisorId ?? 0,
                            3 => employee.HeadOfDepartmentId ?? 0,
                            _ => 0
                        };
                    }
                    else
                    {
                        thirdApproverId = approvalSettings.ThirdApprovalID ?? 0;
                    }
                }

                // Set initial approver based on self-approval logic
                if (isSelfApplication && AllowSelfApproval)
                {
                    // If self-approval is allowed and it's a self-application
                    if (selfApprovalExceptionId > 0)
                    {
                        // Use self-approval exception person
                        initialApproverId = selfApprovalExceptionId;
                    }
                    else
                    {
                        // Use first approver
                        initialApproverId = firstApproverId;
                    }
                }
                else
                {
                    // Normal flow - use first approver
                    initialApproverId = firstApproverId;
                }

                if (initialApproverId == 0)
                {
                    return new CommonReturnViewModel { Success = false, Message = "No valid first approver found" };
                }
                #endregion

                #region Save Method

                var toCreate = new Resignations
                {
                    //OrganizationID = model.CompanyId,
                    EmployeeID = model.EmployeeId,
                    ResignationDate = resignationDate,
                    NoticeDate = noticeDate,
                    Reason = model.Reason,

                    StatusID = status.StatusID,
                    ApprovalPersonID = initialApproverId,
                    ApprovalStep = initialStage,

                };
                await _resignationRepository.AddAsync(toCreate, model);

                #endregion


                #region Auto Approve By employee

                if (model.EmployeeId == firstApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);

                    if (AllowSecondApproval)
                    {
                        toCreate.ApprovalPersonID = secondApproverId;
                        toCreate.ApprovalStep = 2;
                    }
                    else if (!AllowSecondApproval && !AllowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 2;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }

                    await _resignationRepository.UpdateAsync(toCreate, model);


                }
                else if (model.EmployeeId == secondApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(toCreate, model, secondApproverId, 2);

                    if (AllowThirdApproval)
                    {
                        toCreate.ApprovalPersonID = thirdApproverId;
                        toCreate.ApprovalStep = 3;
                    }
                    else if (!AllowThirdApproval && !AllowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 3;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _resignationRepository.UpdateAsync(toCreate, model);
                }
                else if (model.EmployeeId == thirdApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(toCreate, model, secondApproverId, 2);
                    var c = AddAutoApprovedHistoryAsync(toCreate, model, thirdApproverId, 3);

                    if (!AllowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 4;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _resignationRepository.UpdateAsync(toCreate, model);
                }


                #endregion


                #region Alert


                var alert = new Alerts
                {
                    AlertTitle = "Employee Resign",
                    AlertNote = $"{employeeInfo.FirstName} {employeeInfo.LastName} has requested for Resign.",
                    LMAC = model.LMAC,
                    LIP = model.LIP,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.Now,
                };

                await alertsRepository.AddAsync(alert);
                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = toCreate.ApprovalPersonID,  // for alert Employee
                    IsChecked = false,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                };
                await alertForEmployeeRepository.AddAsync(empAlert);

                #endregion



                result.Success = true;
                result.Message = "Saved successfull";
                return result;

            }
            catch (Exception ex)
            {
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = ex.Message
                    };
                }
            }
        }

        #endregion


        #region Update And 

        public CommonReturnViewModel UpdateResignation(int resignationId, ResignationPostViewModel model)
        {
            try
            {
                var result = new CommonReturnViewModel
                {
                    Success = false
                };

                // Validate Notice Date
                if (!DateTime.TryParseExact(model.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out DateTime noticeDate))
                {
                    result.Message = "Invalid Notice Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                // Validate Resignation Date
                if (!DateTime.TryParseExact(model.ResignationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out DateTime resignationDate))
                {
                    result.Message = "Invalid Resignation Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                // Check if resignation exists
                var resignation = _resignationRepository.AllActive()
                    .FirstOrDefault(r => r.ResignationID == resignationId);

                if (resignation == null)
                {
                    result.Message = "Resignation record not found.";
                    return result;
                }

                // Validate Employee
                var emp = _employeeRepository.AllActive()
                    .FirstOrDefault(e => e.EmployeeID == model.EmployeeId);

                if (emp == null)
                {
                    result.Message = "Employee is invalid.";
                    return result;
                }

                // Update fields
               // resignation.OrganizationID = model.CompanyId;
                resignation.EmployeeID = model.EmployeeId;
                resignation.ResignationDate = resignationDate;
                resignation.NoticeDate = noticeDate;
                resignation.Reason = model.Reason;

                _resignationRepository.UpdateAsync(resignation, model);

                result.Success = true;
                result.Message = "Updated successfully.";
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

        #endregion

        #region Delter And Get by id

        public CommonReturnViewModel DeleteResignation(int resignationId)
        {
            try
            {
                var baseModel = new BaseViewModel();

                var result = new CommonReturnViewModel
                {
                    Success = false
                };

                
                var resignation = _resignationRepository.AllActive()
                    .FirstOrDefault(r => r.ResignationID == resignationId);

                if (resignation == null)
                {
                    result.Message = "Resignation record not found.";
                    return result;
                }

                
                
                resignation.DeletedAt = DateTime.Now; 
                resignation.DeletedBy = baseModel.DeletedBy; 

                _resignationRepository.UpdateAsync(resignation);

                result.Success = true;
                result.Message = "Deleted successfully.";
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


        public ResignationViewModel GetResignationById(int resignationId)
        {
            var resign = _resignationRepository.AllActive().Where(e=>e.ResignationID == resignationId).Select(r=> new ResignationViewModel
            {
                ResigId = r.ResignationID,
                ResignResons = r.Reason,
                ResinDate = r.ResignationDate.Value.ToString("dd/MM/yyyy"),
                ResNoticeDate = r.NoticeDate.Value.ToString("dd/MM/yyyy"),
                EmployeeId = r.EmployeeID,
              //  CompanyId = r.OrganizationID,

            }).FirstOrDefault();

            if (resign == null)
                return new ResignationViewModel();

            return resign;
        }


        #endregion


        #region Helper

        private async Task AddAutoApprovedHistoryAsync(Resignations toCreate, ResignationPostViewModel model, int firstApproverId, int stage)
        {
            try
            {
                var approvedStatus = await GetOrCreateStatusAsync("approve", new PromotionActionModel
                {
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                });

                string comment = "Auto Approved (Stage " + stage + " )";
                var history = new ResignationsApprovalHistory
                {
                    ResignationID = toCreate.ResignationID,
                    EmployeeID = model.EmployeeId,
                    StatusID = approvedStatus.StatusID,
                    ApprovalPersonID = firstApproverId,
                    ApprovalPersonNote = comment,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy
                };

                await _resignationsApprovalHistoryRepository.AddAsync(history);
            }
            catch (Exception)
            {

                throw;
            }


        }

        private async Task<Statuses> GetOrCreateStatusAsync(string action, PromotionActionModel actionModel)
        {
            string statusName = action == "approve" ? "Approved" : "Decline";
            var status = await _statusRepository.AllActive()
                .FirstOrDefaultAsync(s => s.StatusName.ToLower() == statusName.ToLower());

            if (status == null)
            {
                status = new Statuses
                {
                    StatusName = statusName,
                    StatusType = "EmployeeCareerChange",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actionModel.CreatedBy,
                    LIP = actionModel.LIP,
                    LMAC = actionModel.LMAC
                };
                await _statusRepository.AddAsync(status);
            }

            return status;
        }

        private async Task<int> ResolveNextApproverAsync(EmployeeCareerChanges career)
        {
            if (career.EmployeeID == null || career.ApprovalStage == null)
                return 0;

            var employee = await _empOfficialRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeID == career.EmployeeID);
            if (employee == null)
                return 0;

            var approvalType = await _approvalTypeRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeName.ToLower() == "promotion approval");
            if (approvalType == null)
                return 0;

            var approvalSettings = await _approvalSettingRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                    && a.OrganizationID == employee.OrganizationID
                    && a.OrganizationBranchID == employee.OrganizationBranchID);
            if (approvalSettings == null)
                return 0;

            int currentStage = career.ApprovalStage.Value;
            if (currentStage == 1 && approvalSettings.IsEnableSecondApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 2);
            }
            else if (currentStage == 2 && approvalSettings.IsEnableThirdApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 3);
            }

            return 0; // No further approvers needed
        }

        private async Task<int> ResolveApproverAsync(ApprovalSettings settings, EmployeeOfficeInfo employee, int stage)
        {
            bool isDesignationBased;
            int? approverID;
            bool allowSelfApproval = settings.AllowSelfApproval ?? false;
            int? selfExceptionID = settings.SelfExceptionApprovalID;

            if (stage == 2)
            {
                if (!settings.IsEnableSecondApproval) return 0;
                isDesignationBased = settings.IsDesignationOrEmpSecondApprovalID;
                approverID = settings.SecondApprovalID;
            }
            else if (stage == 3)
            {
                if (!settings.IsEnableThirdApproval) return 0;
                isDesignationBased = settings.IsDesignationOrEmpThirdApprovalID;
                approverID = settings.ThirdApprovalID;
            }
            else
            {
                return 0; // Unsupported stage
            }

            if (!approverID.HasValue)
                return 0;

            if (isDesignationBased)
            {
                var approvalDesig = await _approvalDesignationRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.ApprovalDesignationID == approverID);
                return approvalDesig?.Code switch
                {
                    1 => employee.ImmediateSupervisorId ?? 0,
                    2 => employee.SeniorSupervisorId ?? 0,
                    3 => employee.HeadOfDepartmentId ?? 0,
                    _ => 0
                };
            }

            if (approverID == employee.EmployeeID && allowSelfApproval)
                return employee.EmployeeID ?? 0;

            return selfExceptionID ?? approverID ?? 0;
        }

        private async Task LogCareerChangeHistoryAsync(Resignations career, PromotionActionModel action, bool isAuto = false)
        {
            var history = new ResignationsApprovalHistory
            {
                ResignationID = career.ResignationID,
                EmployeeID = career.EmployeeID,
                StatusID = career.StatusID,
                ApprovalPersonID = isAuto ? career.ApprovalPersonID : action.CreatedBy,
                // ApprovalPersonID = career.ApprovalPersonID,
                //ApprovalPersonID = action.CreatedBy,
                ApprovalPersonNote = action.Comments,
                LIP = action.LIP,
                LMAC = action.LMAC,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = action.CreatedBy
            };
            await _resignationsApprovalHistoryRepository.AddAsync(history);
        }

        #endregion


    }
}