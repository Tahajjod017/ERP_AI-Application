using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Core.ViewModels.Employee.EmployeeTermination;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeTermination
{
    public class EmployeeTerminationService : IEmployeeTermination
    {
        #region Fields
        private readonly IGenericRepository<TerminationTypes> _terminationTypeRepository;
        private readonly IGenericRepository<TerminationApprovalHistory> _terminationHistoryRepository;
        private readonly IGenericRepository<Terminations> _terminationRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;
        private readonly IGenericRepository<Departments> _deptRepository;
        private readonly IGenericRepository<Designations> _desigRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _empOfficialRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        private readonly IGenericRepository<ApprovalDesignation> _approvalDesignationRepository;
        private readonly IGenericRepository<Alerts> _alertsRepository;
        private readonly IGenericRepository<AlertForEmployee> _alertForEmployeeRepository;
        #endregion

        #region Constructor
        public EmployeeTerminationService(
            IGenericRepository<TerminationTypes> terminationTypeRepository,
            IGenericRepository<TerminationApprovalHistory> terminationHistoryRepository,
            IGenericRepository<Terminations> terminationRepository,
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository,
            IGenericRepository<Departments> deptRepository,
            IGenericRepository<Designations> desigRepository,
            IGenericRepository<Statuses> statusRepository,
            IGenericRepository<EmployeeOfficeInfo> empOfficialRepository,
            IGenericRepository<ApprovalTypes> approvalTypeRepository,
            IGenericRepository<ApprovalSettings> approvalSettingRepository,
            IGenericRepository<ApprovalDesignation> approvalDesignationRepository,
            IGenericRepository<Alerts> alertsRepository,
            IGenericRepository<AlertForEmployee> alertForEmployeeRepository)
        {
            _terminationTypeRepository = terminationTypeRepository;
            _terminationHistoryRepository = terminationHistoryRepository;
            _terminationRepository = terminationRepository;
            _employeeRepository = employeeRepository;
            _deptRepository = deptRepository;
            _desigRepository = desigRepository;
            _statusRepository = statusRepository;
            _empOfficialRepository = empOfficialRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _approvalDesignationRepository = approvalDesignationRepository;
            _alertsRepository = alertsRepository;
            _alertForEmployeeRepository = alertForEmployeeRepository;
        }
        #endregion

        #region Entry Page
        public object GetTerminations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate, string imgSrcThumb)
        {
            var terminations = _terminationRepository.AllActive()
                .Include(e => e.Employee)
                .ThenInclude(r => r.EmployeeOfficeInfoEmployee).ThenInclude(p => p.Department)
                .Select(e => new TerminationGetViewModel
                {
                    Id = e.TerminationID,
                    Department = e.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
                    EmployeeName = e.Employee.FirstName + " " + e.Employee.LastName,
                    EmployeeCode = e.Employee.EmployeeCode,
                    Reason = e.Reason,
                    TerminationDate = e.ResignationDate.Value.ToString("dd/MM/yyyy"),
                    NoticeDate = e.NoticeDate.Value.ToString("dd/MM/yyyy"),
                    EmployeeId = e.EmployeeID,
                    TerminationType = e.TerminationType.TerminationTypeName,
                    ProfileImage = imgSrcThumb + e.Employee.EmployeeImageFileName
                }).ToList();

            // Apply date range filter
            if (!string.IsNullOrEmpty(fromDate) && !string.IsNullOrEmpty(toDate))
            {
                DateTime from = DateTime.ParseExact(fromDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime to = DateTime.ParseExact(toDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                terminations = terminations
                    .Where(r =>
                    {
                        DateTime noticeDate = DateTime.ParseExact(r.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        return noticeDate >= from && noticeDate <= to;
                    })
                    .ToList();
            }

            // Apply sorting
            terminations = sortColumn switch
            {
                "employeeName" => sortDirection == "asc" ? terminations.OrderBy(r => r.EmployeeName).ToList() : terminations.OrderByDescending(r => r.EmployeeName).ToList(),
                "department" => sortDirection == "asc" ? terminations.OrderBy(r => r.Department).ToList() : terminations.OrderByDescending(r => r.Department).ToList(),
                "reason" => sortDirection == "asc" ? terminations.OrderBy(r => r.Reason).ToList() : terminations.OrderByDescending(r => r.Reason).ToList(),
                "noticeDate" => sortDirection == "asc" ? terminations.OrderBy(r => r.NoticeDate).ToList() : terminations.OrderByDescending(r => r.NoticeDate).ToList(),
                "terminationDate" => sortDirection == "asc" ? terminations.OrderBy(r => r.TerminationDate).ToList() : terminations.OrderByDescending(r => r.TerminationDate).ToList(),
                _ => terminations
            };

            // Apply pagination
            var totalRecords = terminations.Count;
            var pagedData = terminations.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new
            {
                data = pagedData,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords
            };
        }

        public async Task<CommonReturnViewModel> InsertTermination(TerminationPostViewModel model)
        {
            var result = new CommonReturnViewModel
            {
                Success = false
            };

            // Validate dates
            if (!DateTime.TryParseExact(model.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime noticeDate))
            {
                result.Message = "Invalid Notice Date format. Expected format is dd/MM/yyyy.";
                return result;
            }

            if (!DateTime.TryParseExact(model.ResignationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resignationDate))
            {
                result.Message = "Invalid Termination Date format. Expected format is dd/MM/yyyy.";
                return result;
            }

            // Validate employee
            var emp = await _employeeRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeId);
            if (emp == null)
            {
                result.Message = "Employee is invalid";
                return result;
            }

            // Validate termination type
            var terminationType = await _terminationTypeRepository.AllActive().FirstOrDefaultAsync(t => t.TerminationTypeID == model.TerminationTypeId);
            if (terminationType == null)
            {
                result.Message = "Invalid Termination Type";
                return result;
            }

            try
            {
                bool allowSecondApproval;
                bool allowThirdApproval;
                bool allowSelfApproval;
                int firstApproverId = 0;
                int secondApproverId = 0;
                int thirdApproverId = 0;
                int selfApprovalExceptionId = 0;

                // Status setup
                var status = await _statusRepository.AllActive()
                    .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "pending");
                if (status == null)
                {
                    status = new Statuses
                    {
                        StatusName = "Pending",
                        StatusType = "EmployeeTermination",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _statusRepository.AddAsync(status);
                }

                // Employee validation
                var employee = await _empOfficialRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeId);
                var employeeInfo = await _employeeRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeId);
                if (employee == null || employeeInfo == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };
                }

                // Approval person determination
                var approvalType = await _approvalTypeRepository.AllActive()
                    .FirstOrDefaultAsync(a => a.ApprovalTypeName.Trim().ToLower() == "termination approval");
                if (approvalType == null)
                {
                    approvalType = new ApprovalTypes
                    {
                        ApprovalTypeName = "Termination Approval"
                    };
                    await _approvalTypeRepository.AddAsync(approvalType);
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

                allowSecondApproval = approvalSettings.IsEnableSecondApproval;
                allowThirdApproval = approvalSettings.IsEnableThirdApproval;
                allowSelfApproval = approvalSettings.AllowSelfApproval ?? true;
                selfApprovalExceptionId = approvalSettings.SelfExceptionApprovalID ?? 0;

                bool isSelfApplication = model.CreatedBy == model.EmployeeId;

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

                if (allowSecondApproval)
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

                if (allowThirdApproval)
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

                if (isSelfApplication && allowSelfApproval)
                {
                    initialApproverId = selfApprovalExceptionId > 0 ? selfApprovalExceptionId : firstApproverId;
                }
                else
                {
                    initialApproverId = firstApproverId;
                }

                if (initialApproverId == 0)
                {
                    return new CommonReturnViewModel { Success = false, Message = "No valid first approver found" };
                }

                // Save termination
                var toCreate = new Terminations
                {
                    EmployeeID = model.EmployeeId,
                    TerminationTypeID = model.TerminationTypeId,
                    ResignationDate = resignationDate,
                    NoticeDate = noticeDate,
                    Reason = model.Reason,
                    StatusID = status.StatusID,
                    ApprovalPersonID = initialApproverId,
                    ApprovalStep = initialStage,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.UtcNow,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };
                await _terminationRepository.AddAsync(toCreate);

                // Auto-approve logic
                if (model.EmployeeId == firstApproverId)
                {
                    await AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    if (allowSecondApproval)
                    {
                        toCreate.ApprovalPersonID = secondApproverId;
                        toCreate.ApprovalStep = 2;
                    }
                    else if (!allowSecondApproval && !allowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 2;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _terminationRepository.UpdateAsync(toCreate);
                }
                else if (model.EmployeeId == secondApproverId)
                {
                    await AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    await AddAutoApprovedHistoryAsync(toCreate, model, secondApproverId, 2);
                    if (allowThirdApproval)
                    {
                        toCreate.ApprovalPersonID = thirdApproverId;
                        toCreate.ApprovalStep = 3;
                    }
                    else if (!allowThirdApproval && !allowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 3;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _terminationRepository.UpdateAsync(toCreate);
                }
                else if (model.EmployeeId == thirdApproverId)
                {
                    await AddAutoApprovedHistoryAsync(toCreate, model, firstApproverId, 1);
                    await AddAutoApprovedHistoryAsync(toCreate, model, secondApproverId, 2);
                    await AddAutoApprovedHistoryAsync(toCreate, model, thirdApproverId, 3);
                    if (!allowSelfApproval)
                    {
                        toCreate.ApprovalPersonID = selfApprovalExceptionId;
                        toCreate.ApprovalStep = 4;
                    }
                    else
                    {
                        toCreate.IsFinalApproved = true;
                    }
                    await _terminationRepository.UpdateAsync(toCreate);
                }

                // Alert
                var alert = new Alerts
                {
                    AlertTitle = "Employee Termination",
                    AlertNote = $"{employeeInfo.FirstName} {employeeInfo.LastName} has been requested for termination.",
                    LMAC = model.LMAC,
                    LIP = model.LIP,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.Now
                };
                await _alertsRepository.AddAsync(alert);
                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = toCreate.ApprovalPersonID,
                    IsChecked = false,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy
                };
                await _alertForEmployeeRepository.AddAsync(empAlert);

                result.Success = true;
                result.Message = "Saved successfully";
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

        public async Task< CommonReturnViewModel> UpdateTermination(int terminationId, TerminationPostViewModel model)
        {
            try
            {
                var result = new CommonReturnViewModel
                {
                    Success = false
                };

                // Validate dates
                if (!DateTime.TryParseExact(model.NoticeDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime noticeDate))
                {
                    result.Message = "Invalid Notice Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                if (!DateTime.TryParseExact(model.ResignationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime resignationDate))
                {
                    result.Message = "Invalid Termination Date format. Expected format is dd/MM/yyyy.";
                    return result;
                }

                // Check if termination exists
                var termination = await _terminationRepository.AllActive()
                    .FirstOrDefaultAsync(r => r.TerminationID == terminationId);
                if (termination == null)
                {
                    result.Message = "Termination record not found.";
                    return result;
                }

                // Validate employee
                var emp = await _employeeRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeId);
                if (emp == null)
                {
                    result.Message = "Employee is invalid.";
                    return result;
                }

                // Validate termination type
                var terminationType = await _terminationTypeRepository.AllActive()
                    .FirstOrDefaultAsync(t => t.TerminationTypeID == model.TerminationTypeId);
                if (terminationType == null)
                {
                    result.Message = "Invalid Termination Type";
                    return result;
                }

                // Update fields
                termination.EmployeeID = model.EmployeeId;
                termination.TerminationTypeID = model.TerminationTypeId;
                termination.ResignationDate = resignationDate;
                termination.NoticeDate = noticeDate;
                termination.Reason = model.Reason;
             

                await _terminationRepository.UpdateAsync(termination, model);

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

        public async Task<CommonReturnViewModel> DeleteTermination(DeleteRequestVM delete)
        {
            try
            {
                var result = new CommonReturnViewModel
                {
                    Success = false
                };

                foreach (var item in delete.Ids)
                {
                    var termination = await _terminationRepository.AllActive()
                        .FirstOrDefaultAsync(r => r.TerminationID == item);
                    if (termination == null)
                    {
                        result.Message = "Termination record not found.";
                        return result;
                    }

                    var approvalCheck = await _terminationHistoryRepository.AllActive()
                        .Where(e => e.TerminationID == item).ToListAsync();
                    if (approvalCheck.Any())
                    {
                        result.Message = "Cannot delete! Termination is already in approval matrix.";
                        return result;
                    }

                    termination.DeletedAt = DateTime.UtcNow;
                    termination.DeletedBy = delete.DeletedBy;

                    await _terminationRepository.UpdateAsync(termination, delete);

                    result.Success = true;
                    result.Message = "Deleted successfully.";
                }

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

        public CommonReturnViewModel GetTerminationById(int terminationId)
        {
            try
            {
                var approvalCheck = _terminationHistoryRepository.AllActive()
                    .Where(e => e.TerminationID == terminationId).ToList();
                var termination = _terminationRepository.AllActive()
                    .Join(_empOfficialRepository.AllActive(),
                        t => t.EmployeeID,
                        o => o.EmployeeID,
                        (t, o) => new { termination = t, office = o })
                    .Where(e => e.termination.TerminationID == terminationId)
                    .Select(r => new TerminationGetViewModel
                    {
                        Id = r.termination.TerminationID,
                        Reason = r.termination.Reason,
                        TerminationDate = r.termination.ResignationDate.Value.ToString("dd/MM/yyyy"),
                        NoticeDate = r.termination.NoticeDate.Value.ToString("dd/MM/yyyy"),
                        EmployeeId = r.termination.EmployeeID,
                        TerminationType = r.termination.TerminationType.TerminationTypeName,
                        TerminationTypeId = r.termination.TerminationType.TerminationTypeID,
                        CompanyId = r.office.OrganizationID


                    }).FirstOrDefault();

                if (termination == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "No termination found" };
                }

                if (approvalCheck.Any())
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Cannot edit, data is already in approval matrix",
                        Data = termination
                    };
                }

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Termination loaded successfully",
                    Data = termination
                };
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

        #region Approve Page
        public async Task<List<TerminationGetViewModel>> GetPendingTerminations(string dateRange, string department, string designation, string terminationType, string imgSrcThumb,  int? currentUser)
        {
            try
            {
                var query = _terminationRepository.AllActive()
                    .Include(r => r.Employee)
                        .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                            .ThenInclude(o => o.Designation)
                    .Include(r => r.Employee)
                        .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                            .ThenInclude(o => o.Department)
                    .Include(r => r.Status)
                    .Include(r => r.TerminationType)
                    .Where(r => r.ApprovalPersonID == currentUser &&
                                ((r.IsFinalApproved == false && r.IsDecline == null) ||
                                 (r.IsFinalApproved == r.IsDecline && r.IsDecline != true) ||
                                 (r.IsDecline == false && r.IsFinalApproved == null) ||
                                 (r.IsFinalApproved == null && r.IsDecline == null)));

                if (!string.IsNullOrEmpty(dateRange))
                {
                    var dates = dateRange.Split(" to ");
                    if (dates.Length == 2 &&
                        DateTime.TryParseExact(dates[0], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) &&
                        DateTime.TryParseExact(dates[1], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                    {
                        query = query.Where(r => r.NoticeDate >= fromDate && r.NoticeDate <= toDate);
                    }
                }

                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DepartmentID == int.Parse(department)));
                }

                if (!string.IsNullOrEmpty(designation))
                {
                    query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DesignationID == int.Parse(designation)));
                }

                if (!string.IsNullOrEmpty(terminationType))
                {
                    query = query.Where(r => r.TerminationTypeID == int.Parse(terminationType));
                }

                var terminations = await query.Select(r => new TerminationGetViewModel
                {
                    Id = r.TerminationID,
                    EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                    EmployeeCode = r.Employee.EmployeeCode,
                    Department = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
                    Position = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName,
                    TerminationType = r.TerminationType.TerminationTypeName,
                    Reason = r.Reason,
                    NoticeDate = r.NoticeDate.Value.ToString("dd/MM/yyyy"),
                    TerminationDate = r.ResignationDate.Value.ToString("dd/MM/yyyy"),
                    Status = r.Status.StatusName ?? "Pending",
                    ProfileImage = r.Employee.EmployeeImageFileName != null ? imgSrcThumb + r.Employee.EmployeeImageFileName : "https://placehold.co/400",
                    EmployeeId = r.EmployeeID,
                    YearsOfService = CalculateYearsOfService(r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().JoiningDate, r.ResignationDate.Value),
                    NoticePeriod = (r.ResignationDate.Value - r.NoticeDate.Value).Days.ToString() + " Days",
                    PendingDues = "৳0" // Placeholder
                }).ToListAsync();

                return terminations;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TerminationGetViewModel>> GetProcessedTerminations(string dateRange, string department, string designation, string terminationType, string imgSrcThumb, int? currentUser)
        {
            var query = _terminationHistoryRepository.AllActive()
                .Include(i => i.Termination)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                        .ThenInclude(o => o.Designation)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                        .ThenInclude(o => o.Department)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeSalarySettingsEmployee)
                       
                .Include(r => r.Status)
                .Include(r => r.Termination).ThenInclude(t => t.TerminationType)
                .Where(r => r.ApprovalPersonID == currentUser &&
                           ((r.Termination.IsFinalApproved == true && r.Termination.IsDecline == null) ||
                            (r.Termination.IsFinalApproved == r.Termination.IsDecline && r.Termination.IsDecline != false && r.Termination.IsDecline != null) ||
                            (r.Termination.IsDecline == true && r.Termination.IsFinalApproved == null) ||
                            (r.Termination.IsFinalApproved != null && r.Termination.IsDecline != null)));

            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split(" to ");
                if (dates.Length == 2 &&
                    DateTime.TryParseExact(dates[0], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fromDate) &&
                    DateTime.TryParseExact(dates[1], "d/M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var toDate))
                {
                    query = query.Where(r => r.Termination.ResignationDate >= fromDate && r.Termination.ResignationDate <= toDate);
                }
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DepartmentID == int.Parse(department)));
            }

            if (!string.IsNullOrEmpty(designation))
            {
                query = query.Where(r => r.Employee.EmployeeOfficeInfoEmployee.Any(o => o.DesignationID == int.Parse(designation)));
            }

            if (!string.IsNullOrEmpty(terminationType))
            {
                query = query.Where(r => r.Termination.TerminationTypeID == int.Parse(terminationType));
            }

            var terminations = await query.Select(r => new TerminationGetViewModel
            {
                Id = r.Termination.TerminationID,
                EmployeeName = r.Employee.FirstName + " " + r.Employee.LastName,
                EmployeeCode = r.Employee.EmployeeCode,
                Department = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Department.DepartmentName,
                Position = r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().Designation.DesignationName,
                TerminationType = r.Termination.TerminationType.TerminationTypeName,
                Reason = r.Termination.Reason,
                NoticeDate = r.Termination.NoticeDate.Value.ToString("dd/MM/yyyy"),
                TerminationDate = r.Termination.ResignationDate.Value.ToString("dd/MM/yyyy"),
                ProcessedDate = r.CreatedAt.Value.ToString("dd/MM/yyyy") ?? "",
                Status = r.Status.StatusName ?? "Approved",
                ProfileImage = r.Employee.EmployeeImageFileName != null ? imgSrcThumb + r.Employee.EmployeeImageFileName : "https://placehold.co/400",
                EmployeeId = r.EmployeeID,
                YearsOfService = CalculateYearsOfService(r.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().JoiningDate, r.Termination.ResignationDate.Value),
                NoticePeriod = (r.Termination.ResignationDate.Value - r.Termination.NoticeDate.Value).Days.ToString() + " Days",
                CurrentSalary = r.Employee.EmployeeSalarySettingsEmployee.FirstOrDefault().Salary.ToString() ?? "৳0",
                PendingDues = "৳0" // Placeholder
            }).ToListAsync();

            return terminations;
        }

        public async Task<TerminationGetViewModel> GetAppTerminationById(int id)
        {
            var termination = await _terminationRepository.AllActive()
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                        .ThenInclude(o => o.Designation)
                .Include(r => r.Employee)
                    .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                        .ThenInclude(o => o.Department)
                .Include(r => r.TerminationType)
                .FirstOrDefaultAsync(r => r.TerminationID == id);

            if (termination == null)
            {
                return null;
            }

            return new TerminationGetViewModel
            {
                Id = termination.TerminationID,
                EmployeeName = termination.Employee.FirstName + " " + termination.Employee.LastName + " (" + termination.Employee.EmployeeCode + ")",
                Department = termination.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Department.DepartmentName ?? "Unknown",
                Position = termination.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault()?.Designation.DesignationName ?? "Unknown",
                TerminationType = termination.TerminationType.TerminationTypeName,
                Reason = termination.Reason,
                NoticeDate = termination.NoticeDate.Value.ToString("dd/MM/yyyy"),
                TerminationDate = termination.ResignationDate.Value.ToString("dd/MM/yyyy"),
                Status = _statusRepository.AllActive().FirstOrDefault(s => s.StatusID == termination.StatusID)?.StatusName ?? (termination.IsFinalApproved == true ? "Approved" : "Pending"),
                ProfileImage = termination.Employee.EmployeeImageFileName != null ? "/images/employees/" + termination.Employee.EmployeeImageFileName : "https://placehold.co/400",
                EmployeeId = termination.EmployeeID,
                YearsOfService = CalculateYearsOfService(termination.Employee.EmployeeOfficeInfoEmployee.FirstOrDefault().JoiningDate, termination.ResignationDate.Value),
                NoticePeriod = (termination.ResignationDate.Value - termination.NoticeDate.Value).Days.ToString() + " Days",
                CurrentSalary = termination.Employee.EmployeeSalarySettingsEmployee.FirstOrDefault()?.Salary.ToString() ?? "৳0",
                PendingDues = "৳0" // Placeholder
            };
        }

        public async Task<CommonReturnViewModel> ProcessTermination(int id, string action, string hrComments, string handoverStatus, bool assetReturned, bool clearanceCompleted, bool documentsPrepared, CommonBaseViewModel? baseModel)
        {
            try
            {
                if (action == null || id <= 0 || string.IsNullOrEmpty(action) || baseModel == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Invalid action model" };
                }

                var termination = await _terminationRepository.AllActive().FirstOrDefaultAsync(r => r.TerminationID == id);
                if (termination == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Termination not found" };
                }

                var employee = await _employeeRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == termination.EmployeeID);
                if (employee == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };
                }

                if (termination.IsFinalApproved == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Termination already approved" };
                }

                if (termination.IsDecline == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Termination has been declined" };
                }

                if (termination.ApprovalPersonID != baseModel.CreatedBy)
                {
                    return new CommonReturnViewModel { Success = false, Message = "You are not authorized to approve this termination" };
                }

                var status = await GetOrCreateStatusAsync(action.ToLower(), new PromotionActionModel
                {
                    CreatedBy = baseModel.CreatedBy,
                    LIP = baseModel.LIP,
                    LMAC = baseModel.LMAC
                });

                if (status == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Failed to retrieve or create status" };
                }

                termination.StatusID = status.StatusID;
                bool isDecline = action.ToLower() == "decline";
                termination.IsDecline = isDecline;

                if (!isDecline)
                {
                    termination.ApprovalStep = termination.ApprovalStep ?? 0;
                    int nextApproverId = await ResolveNextApproverAsync(termination);
                    if (nextApproverId != 0)
                    {
                        termination.ApprovalPersonID = nextApproverId;
                        termination.IsFinalApproved = false;
                        termination.ApprovalStep += 1;
                    }
                    else
                    {
                        termination.IsFinalApproved = true;
                        termination.ApprovalStep += 1;
                    }
                }
                else
                {
                    termination.IsFinalApproved = false;
                    termination.ApprovalStep = termination.ApprovalStep ?? 1;
                }

                termination.UpdatedAt = DateTime.UtcNow;
                termination.UpdatedBy = baseModel.UpdatedBy;
                termination.LIP = baseModel.LIP;
                termination.LMAC = baseModel.LMAC;

                await _terminationRepository.UpdateAsync(termination);
                await LogTerminationHistoryAsync(termination, baseModel, hrComments);

                // Alert
                if (termination.IsFinalApproved != true && termination.IsDecline != true)
                {
                    var alert = new Alerts
                    {
                        AlertTitle = "Employee Termination",
                        AlertNote = $"{employee.FirstName} {employee.LastName} has a pending termination approval.",
                        LMAC = baseModel.LMAC,
                        LIP = baseModel.LIP,
                        CreatedBy = baseModel.CreatedBy,
                        CreatedAt = DateTime.Now
                    };
                    await _alertsRepository.AddAsync(alert);
                    var empAlert = new AlertForEmployee
                    {
                        AlertID = alert.AlertID,
                        EmployeeID = termination.ApprovalPersonID,
                        IsChecked = false,
                        LIP = baseModel.LIP,
                        LMAC = baseModel.LMAC,
                        CreatedAt = DateTime.Now,
                        CreatedBy = baseModel.CreatedBy
                    };
                    await _alertForEmployeeRepository.AddAsync(empAlert);
                }

                return new CommonReturnViewModel { Success = true, Message = "Termination action completed successfully" };
            }
            catch (DbUpdateException ex)
            {
                return new CommonReturnViewModel { Success = false, Message = "Database error occurred while processing the termination action" };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel { Success = false, Message = "An unexpected error occurred while processing the termination action: " + ex.Message };
            }
        }
        #endregion

        #region Helper Methods
        private async Task AddAutoApprovedHistoryAsync(Terminations toCreate, TerminationPostViewModel model, int approverId, int stage)
        {
            try
            {
                var approvedStatus = await GetOrCreateStatusAsync("approve", new PromotionActionModel
                {
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                });

                string comment = $"Auto Approved (Stage {stage})";
                var history = new TerminationApprovalHistory
                {
                    TerminationID = toCreate.TerminationID,
                    EmployeeID = model.EmployeeId,
                    StatusID = approvedStatus.StatusID,
                    ApprovalPersonID = approverId,
                    ApprovalPersonNote = comment,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy
                };

                await _terminationHistoryRepository.AddAsync(history);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<Statuses> GetOrCreateStatusAsync(string action, PromotionActionModel actionModel)
        {
            string statusName = action == "approve" ? "Approved" : "Declined";
            var status = await _statusRepository.AllActive()
                .FirstOrDefaultAsync(s => s.StatusName.ToLower() == statusName.ToLower());

            if (status == null)
            {
                status = new Statuses
                {
                    StatusName = statusName,
                    StatusType = "EmployeeTermination",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = actionModel.CreatedBy,
                    LIP = actionModel.LIP,
                    LMAC = actionModel.LMAC
                };
                await _statusRepository.AddAsync(status);
            }

            return status;
        }

        private async Task<int> ResolveNextApproverAsync(Terminations termination)
        {
            if (termination.EmployeeID == null || termination.ApprovalStep == null)
                return 0;

            var employee = await _empOfficialRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeID == termination.EmployeeID);
            if (employee == null)
                return 0;

            var approvalType = await _approvalTypeRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeName.ToLower() == "termination approval");
            if (approvalType == null)
                return 0;

            var approvalSettings = await _approvalSettingRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                    && a.OrganizationID == employee.OrganizationID
                    && a.OrganizationBranchID == employee.OrganizationBranchID);
            if (approvalSettings == null)
                return 0;

            int currentStage = termination.ApprovalStep.Value;
            if (currentStage == 1 && approvalSettings.IsEnableSecondApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 2);
            }
            else if (currentStage == 2 && approvalSettings.IsEnableThirdApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 3);
            }

            return 0;
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
                return 0;
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

        private async Task LogTerminationHistoryAsync(Terminations termination, CommonBaseViewModel action, string hrComments, bool isAuto = false)
        {
            var history = new TerminationApprovalHistory
            {
                TerminationID = termination.TerminationID,
                EmployeeID = termination.EmployeeID,
                StatusID = termination.StatusID,
                ApprovalPersonID = isAuto ? termination.ApprovalPersonID : action.CreatedBy,
                ApprovalPersonNote = hrComments,
                LIP = action.LIP,
                LMAC = action.LMAC,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = action.CreatedBy
            };
            await _terminationHistoryRepository.AddAsync(history);
        }

        public static string CalculateYearsOfService(DateOnly? joiningDate, DateTime terminationDate)
        {
            try
            {
                if (!joiningDate.HasValue)
                    return "0 Years";

                var joinDateTime = joiningDate.Value.ToDateTime(TimeOnly.MinValue);
                if (terminationDate < joinDateTime)
                    return "0 Years";

                int years = terminationDate.Year - joinDateTime.Year;
                int months = terminationDate.Month - joinDateTime.Month;
                int days = terminationDate.Day - joinDateTime.Day;

                if (days < 0)
                {
                    months--;
                    days += DateTime.DaysInMonth(terminationDate.Year, terminationDate.Month == 1 ? 12 : terminationDate.Month - 1);
                }

                if (months < 0)
                {
                    years--;
                    months += 12;
                }

                return $"{years} Years {months} Months";
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }

}
