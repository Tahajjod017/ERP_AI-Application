
#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;


#endregion

namespace GCTL.Service.Employees.EmployeeStatus.Increment
{


    public class IncrementService : IincrementService
    {
        #region CTOR
        private readonly IGenericRepository<EmployeeActionTypes> _employeeActionTypeRepository;
        private readonly IGenericRepository<EmployeeCareerChangeHistory> _employeeCarrerCngHistoryRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _employeeCarrerCngRepository;
        private readonly IGenericRepository<Departments> _deptRepository;
        private readonly IGenericRepository<Designations> _desigRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _empOfficialRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        private readonly IGenericRepository<ApprovalDesignation> _approvalDesignationRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;

        public IncrementService(
            IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository,
            IGenericRepository<EmployeeCareerChangeHistory> employeeCarrerCngHistoryRepository,
            IGenericRepository<EmployeeCareerChanges> employeeCarrerCngRepository,
            IGenericRepository<Departments> deptRepository,
            IGenericRepository<Designations> desigRepository,
            IGenericRepository<Statuses> statusRepository,
            IGenericRepository<EmployeeOfficeInfo> empOfficialRepository,
            IGenericRepository<ApprovalTypes> approvalTypeRepository,
            IGenericRepository<ApprovalSettings> approvalSettingRepository,
            IGenericRepository<ApprovalDesignation> approvalDesignationRepository,
            IGenericRepository<GCTL.Data.Models.Employees> employeeRepository)
        {
            _employeeActionTypeRepository = employeeActionTypeRepository;
            _employeeCarrerCngHistoryRepository = employeeCarrerCngHistoryRepository;
            _employeeCarrerCngRepository = employeeCarrerCngRepository;
            _deptRepository = deptRepository;
            _desigRepository = desigRepository;
            _statusRepository = statusRepository;
            _empOfficialRepository = empOfficialRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _approvalDesignationRepository = approvalDesignationRepository;
            _employeeRepository = employeeRepository;
        }

        #endregion

        #region Get Table and Card

        public async Task<List<IncrementApproveViewModel>> GetAllIncrementPendingList()
        {
            var matches = new[] { "increment", "decrement" };
            var incrementIDs = await _employeeActionTypeRepository.AllActive()
                .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                .Select(x => x.EmployeeActionTypeID)
                .ToListAsync();

            var query = from ecc in _employeeCarrerCngRepository.All()
                        where incrementIDs.Contains(ecc.EmployeeActionTypeID ?? 0)
                        join emp in _employeeRepository.All()
                            on ecc.EmployeeID equals emp.EmployeeID
                        join off in _empOfficialRepository.AllActive()
                            on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                        from office in empOfficeGroup.DefaultIfEmpty()
                        join dept in _deptRepository.AllActive()
                            on office.DepartmentID equals dept.DepartmentID into deptGroup
                        from department in deptGroup.DefaultIfEmpty()
                        join action in _employeeActionTypeRepository.AllActive()
                            on ecc.EmployeeActionTypeID equals action.EmployeeActionTypeID into actionGroup
                        from actionType in actionGroup.DefaultIfEmpty()
                        select new IncrementApproveViewModel
                        {
                            Id = ecc.EmployeeCareerChangeID,
                            EmployeeName = emp.FirstName + " " + emp.LastName,
                            Department = department.DepartmentName ?? "N/A",
                            IncrementType = actionType.EmployeeActionTypeName ?? "N/A",
                            CurrentSalary = ecc.CurrentSalary.HasValue
                                ? ecc.CurrentSalary.Value.ToString("C")
                                : "N/A",
                            ProposedSalary = ecc.NewSalary.HasValue
                                ? ecc.NewSalary.Value.ToString("C")
                                : "N/A",
                            IncrementAmount = ecc.NewSalary.HasValue && ecc.CurrentSalary.HasValue
                                ? (ecc.NewSalary.Value - ecc.CurrentSalary.Value).ToString("C")
                                : "N/A",
                            EffectiveDate = ecc.EffectiveDate.HasValue
                                ? ecc.EffectiveDate.Value.ToString("dd MMM yyyy")
                                : "N/A",
                            YearsOfExperience = office.JoiningDate.HasValue
                                ? $"{(DateOnly.FromDateTime(DateTime.Today).DayNumber - office.JoiningDate.Value.DayNumber) / 365.0:F1} Years"
                                : "N/A",
                            Justification = ecc.Remarks ?? "N/A",
                            AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                ? "../../assets/img/users/user-01.jpg"
                                : emp.EmployeeImageFileName,
                            Status = "Pending"
                        };

            return await query.ToListAsync();
        }

        private async Task<IQueryable<IncrementApproveViewModel>> GetPendingIncrementQueryAsync(string imgLink, int? loggedID)
        {
            try
            {
                var matches = new[] { "increment",  "decrement" };
                var incrementIDs = await _employeeActionTypeRepository.AllActive()
                    .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                    .Select(x => x.EmployeeActionTypeID)
                    .ToListAsync();

                var query = from ecc in _employeeCarrerCngRepository.All()
                            where incrementIDs.Contains(ecc.EmployeeActionTypeID ?? 0)
                            && (ecc.IsDecline == null || ecc.IsDecline == false)
                            && (ecc.IsFinalApproved == null || ecc.IsFinalApproved == false)
                            && ecc.ApprovalPersonID == loggedID
                            && ecc.ApprovalPersonID != ecc.UpdatedBy
                            join emp in _employeeRepository.All()
                                on ecc.EmployeeID equals emp.EmployeeID
                            join off in _empOfficialRepository.AllActive()
                                on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                            from office in empOfficeGroup.DefaultIfEmpty()
                            join dept in _deptRepository.AllActive()
                                on office.DepartmentID equals dept.DepartmentID into deptGroup
                            from department in deptGroup.DefaultIfEmpty()
                            join action in _employeeActionTypeRepository.AllActive()
                                on ecc.EmployeeActionTypeID equals action.EmployeeActionTypeID into actionGroup
                            from actionType in actionGroup.DefaultIfEmpty()
                            join status in _statusRepository.AllActive()
                                on ecc.StatusID equals status.StatusID into statusGroup
                            from status in statusGroup.DefaultIfEmpty()
                            select new IncrementApproveViewModel
                            {
                                Id = ecc.EmployeeCareerChangeID,
                                EmployeeName = emp.FirstName + " " + emp.LastName,
                                Department = department.DepartmentName ?? "N/A",
                                IncrementType = actionType.EmployeeActionTypeName ?? "N/A",
                                CurrentSalary = ecc.CurrentSalary.HasValue
                                    ? ecc.CurrentSalary.Value.ToString("C")
                                    : "N/A",
                                ProposedSalary = ecc.NewSalary.HasValue
                                    ? ecc.NewSalary.Value.ToString("C")
                                    : "N/A",
                                IncrementAmount = ecc.NewSalary.HasValue && ecc.CurrentSalary.HasValue
                                    ? (ecc.NewSalary.Value - ecc.CurrentSalary.Value).ToString("C")
                                    : "N/A",
                                EffectiveDate = ecc.EffectiveDate.HasValue
                                    ? ecc.EffectiveDate.Value.ToString("dd MMM yyyy")
                                    : "N/A",
                                EffectiveDateRaw = ecc.EffectiveDate,
                                YearsOfExperience = office.JoiningDate.HasValue
                                    ? $"{(DateTime.Today.Year - office.JoiningDate.Value.Year):F1} Years"
                                    : "N/A",
                                Justification = ecc.Remarks ?? "N/A",
                                AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                    ? "../../assets/img/users/user-01.jpg"
                                    : imgLink + emp.EmployeeImageFileName,
                                Status = status != null ? status.StatusName : "Pending"
                            };

                return query;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<IncrementApproveViewModel>().AsQueryable();
            }
        }

        private async Task<IQueryable<IncrementApproveViewModel>> GetApprovedIncrementQueryAsync(string imgLink, int? loggedID)
        {
            try
            {
                var matches = new[] { "increment", "decrement" };
                var incrementIDs = await _employeeActionTypeRepository.AllActive()
                    .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                    .Select(x => x.EmployeeActionTypeID)
                    .ToListAsync();

                var query = from ecHis in _employeeCarrerCngHistoryRepository.All()
                            join ecc in _employeeCarrerCngRepository.All()
                                on ecHis.EmployeeCareerChangeID equals ecc.EmployeeCareerChangeID
                            where incrementIDs.Contains(ecc.EmployeeActionTypeID ?? 0)
                            && ecHis.ApprovalPersonID == loggedID
                            join emp in _employeeRepository.All()
                                on ecc.EmployeeID equals emp.EmployeeID
                            join off in _empOfficialRepository.AllActive()
                                on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                            from office in empOfficeGroup.DefaultIfEmpty()
                            join dept in _deptRepository.AllActive()
                                on office.DepartmentID equals dept.DepartmentID into deptGroup
                            from department in deptGroup.DefaultIfEmpty()
                            join action in _employeeActionTypeRepository.AllActive()
                                on ecc.EmployeeActionTypeID equals action.EmployeeActionTypeID into actionGroup
                            from actionType in actionGroup.DefaultIfEmpty()
                            join status in _statusRepository.AllActive()
                                on ecc.StatusID equals status.StatusID into statusGroup
                            from status in statusGroup.DefaultIfEmpty()
                            select new IncrementApproveViewModel
                            {
                                Id = ecc.EmployeeCareerChangeID,
                                EmployeeName = emp.FirstName + " " + emp.LastName,
                                Department = department.DepartmentName ?? "N/A",
                                IncrementType = actionType.EmployeeActionTypeName ?? "N/A",
                                CurrentSalary = ecc.CurrentSalary.HasValue
                                    ? ecc.CurrentSalary.Value.ToString("C")
                                    : "N/A",
                                ProposedSalary = ecc.NewSalary.HasValue
                                    ? ecc.NewSalary.Value.ToString("C")
                                    : "N/A",
                                IncrementAmount = ecc.NewSalary.HasValue && ecc.CurrentSalary.HasValue
                                    ? (ecc.NewSalary.Value - ecc.CurrentSalary.Value).ToString("C")
                                    : "N/A",
                                EffectiveDate = ecc.EffectiveDate.HasValue
                                    ? ecc.EffectiveDate.Value.ToString("dd MMM yyyy")
                                    : "N/A",
                                EffectiveDateRaw = ecc.EffectiveDate,
                                ApprovedDate = ecHis.CreatedAt.HasValue
                                    ? ecHis.CreatedAt.Value.ToString("dd MMM yyyy")
                                    : "N/A",
                                YearsOfExperience = office.JoiningDate.HasValue
                                    ? $"{(DateTime.Today.Year - office.JoiningDate.Value.Year):F1} Years"
                                    : "N/A",
                                Justification = ecc.Remarks ?? "N/A",
                                AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                    ? "../../assets/img/users/user-01.jpg"
                                    : imgLink + emp.EmployeeImageFileName,
                                Status = status != null ? status.StatusName : "Pending"
                            };

                return query;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<IncrementApproveViewModel>().AsQueryable();
            }
        }

        public async Task<object> GetFilteredIncrementsAsync(IncrementFilterModel filter, string imgLink, int? loggedID)
        {
            var query = await GetPendingIncrementQueryAsync(imgLink, loggedID);
            query = ApplyFilters(query, filter);
            query = ApplySorting(query, filter);

            var totalItems = await query.CountAsync();
            int page = filter.Page > 0 ? filter.Page : 1;
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedIncrements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { TotalPages = totalPages, TotalItems = totalItems, Increments = paginatedIncrements };
        }

        public async Task<object> GetFilteredApprovedIncrementsAsync(IncrementFilterModel filter, string imgLink, int? loggedID)
        {
            var query = await GetApprovedIncrementQueryAsync(imgLink, loggedID);
            query = ApplyFilters(query, filter);
            query = ApplySorting(query, filter);

            var totalItems = await query.CountAsync();
            int page = filter.Page > 0 ? filter.Page : 1;
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedIncrements = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { TotalPages = totalPages, TotalItems = totalItems, Increments = paginatedIncrements };
        }

        private IQueryable<IncrementApproveViewModel> ApplyFilters(IQueryable<IncrementApproveViewModel> query, IncrementFilterModel filter)
        {
            if (!string.IsNullOrEmpty(filter.IncrementType))
            {
                query = query.Where(p => p.IncrementType.Contains(filter.IncrementType));
            }
            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(p => p.Status == filter.Status);
            }
            if (!string.IsNullOrEmpty(filter.Department))
            {
                query = query.Where(p => p.Department.Contains(filter.Department));
            }
            if (!string.IsNullOrEmpty(filter.EmployeeName))
            {
                query = query.Where(p => p.EmployeeName.Contains(filter.EmployeeName));
            }
            if (!string.IsNullOrEmpty(filter.DateRange))
            {
                var dates = filter.DateRange.Split(" to ");
                if (dates.Length == 2 &&
                    DateTime.TryParse(dates[0], out var startDate) &&
                    DateTime.TryParse(dates[1], out var endDate))
                {
                    query = query.Where(p => p.EffectiveDateRaw >= startDate && p.EffectiveDateRaw <= endDate);
                }
            }
            return query;
        }

        private IQueryable<IncrementApproveViewModel> ApplySorting(IQueryable<IncrementApproveViewModel> query, IncrementFilterModel filter)
        {
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy)
                {
                    case "Recently Added":
                        query = query.OrderByDescending(p => p.Id);
                        break;
                    case "Ascending":
                        query = query.OrderBy(p => p.EmployeeName);
                        break;
                    case "Descending":
                        query = query.OrderByDescending(p => p.EmployeeName);
                        break;
                    case "Last Month":
                        var lastMonth = DateTime.Now.AddMonths(-1);
                        query = query.Where(p => p.EffectiveDateRaw >= lastMonth);
                        break;
                    case "Last 7 days":
                        var lastWeek = DateTime.Now.AddDays(-7);
                        query = query.Where(p => p.EffectiveDateRaw >= lastWeek);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(filter.SortColumn))
            {
                bool isAscending = filter.SortDirection?.ToLower() != "desc";
                query = filter.SortColumn.ToLower() switch
                {
                    "employeename" => isAscending
                        ? query.OrderBy(p => p.EmployeeName)
                        : query.OrderByDescending(p => p.EmployeeName),
                    "incrementtype" => isAscending
                        ? query.OrderBy(p => p.IncrementType)
                        : query.OrderByDescending(p => p.IncrementType),
                    "currentsalary" => isAscending
                        ? query.OrderBy(p => p.CurrentSalaryNumeric)
                        : query.OrderByDescending(p => p.CurrentSalaryNumeric),
                    "proposedsalary" => isAscending
                        ? query.OrderBy(p => p.ProposedSalaryNumeric)
                        : query.OrderByDescending(p => p.ProposedSalaryNumeric),
                    "incrementamount" => isAscending
                        ? query.OrderBy(p => p.IncrementAmountNumeric)
                        : query.OrderByDescending(p => p.IncrementAmountNumeric),
                    "effectivedate" => isAscending
                        ? query.OrderBy(p => p.EffectiveDateRaw)
                        : query.OrderByDescending(p => p.EffectiveDateRaw),
                    "approveddate" => isAscending
                        ? query.OrderBy(p => p.ApprovedDateRaw)
                        : query.OrderByDescending(p => p.ApprovedDateRaw),
                    _ => query.OrderBy(p => p.Id)
                };
            }
            else if (string.IsNullOrEmpty(filter.SortBy))
            {
                query = query.OrderBy(p => p.Id);
            }

            return query;
        }

        #endregion

        #region Entry Save



        #region Entry Save

        public async Task<CommonReturnViewModel> SaveAsync(SalaryChangeViewModel model)
        {
            if (model == null || model.CurrentSalary < 0 || model.NewSalary < 0 || model.EffectiveDate < DateTime.UtcNow.Date)
            {
                return new CommonReturnViewModel { Success = false, Message = "Invalid input data" };
            }

            bool AllowSecondApproval;
            bool AllowThirdApproval;
            bool AllowSelfApproval; // Assuming self-approval is not allowed by default
            int firstApproverId = 0;
            int secondApproverId = 0;
            int thirdApproverId = 0;
            int selfApprovalExceptionId = 0;

            try
            {
                #region Action Type and Status Setup
                var actionType = await _employeeActionTypeRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.EmployeeActionTypeName.ToLower() == model.ChangeType.ToLower());
                if (actionType == null)
                {
                    actionType = new EmployeeActionTypes
                    {
                        EmployeeActionTypeName = model.ChangeType,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _employeeActionTypeRepository.AddAsync(actionType);
                }

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
                var employee = await _empOfficialRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeId);
                if (employee == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };
                }
                #endregion

                #region Approval Person Determination
                var approvalType = await _approvalTypeRepository.AllActive()
                    .FirstOrDefaultAsync(a => a.ApprovalTypeName.ToLower() == "increment approval");
                if (approvalType == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Approval type not found" };
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
                var increment = new EmployeeCareerChanges
                {
                    EmployeeID = model.EmployeeId,
                    EmployeeActionTypeID = actionType.EmployeeActionTypeID,
                    EffectiveDate = model.EffectiveDate,
                    Remarks = model.Remarks,
                    CurrentSalary = model.CurrentSalary,
                    NewSalary = model.NewSalary,
                    StatusID = status.StatusID,
                    ApprovalPersonID = initialApproverId,
                    ApprovalStage = initialStage,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };
                await _employeeCarrerCngRepository.AddAsync(increment);
                #endregion


                #region Auto Approve By employee

                if (model.EmployeeId == firstApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(increment, model, firstApproverId, 1);

                    if (AllowSecondApproval)
                    {
                        increment.ApprovalPersonID = secondApproverId;
                        increment.ApprovalStage = 2;
                    }
                    else if (!AllowSecondApproval && !AllowSelfApproval)
                    {
                        increment.ApprovalPersonID = selfApprovalExceptionId;
                        increment.ApprovalStage = 2;
                    }
                    else
                    {
                        increment.IsFinalApproved = true;
                    }

                    await _employeeCarrerCngRepository.UpdateAsync(increment);


                }
                else if (model.EmployeeId == secondApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(increment, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(increment, model, secondApproverId, 2);

                    if (AllowThirdApproval)
                    {
                        increment.ApprovalPersonID = thirdApproverId;
                        increment.ApprovalStage = 3;
                    }
                    else if (!AllowThirdApproval && !AllowSelfApproval)
                    {
                        increment.ApprovalPersonID = selfApprovalExceptionId;
                        increment.ApprovalStage = 3;
                    }
                    else
                    {
                        increment.IsFinalApproved = true;
                    }
                    await _employeeCarrerCngRepository.UpdateAsync(increment);
                }
                else if (model.EmployeeId == thirdApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(increment, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(increment, model, secondApproverId, 2);
                    var c = AddAutoApprovedHistoryAsync(increment, model, thirdApproverId, 3);

                    if ( !AllowSelfApproval)
                    {
                        increment.ApprovalPersonID = selfApprovalExceptionId;
                        increment.ApprovalStage = 2;
                    }
                    else
                    {
                        increment.IsFinalApproved = true;
                    }
                    await _employeeCarrerCngRepository.UpdateAsync(increment);
                }

                #endregion



                return new CommonReturnViewModel { Success = true, Message = "Increment saved successfully" };
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                //_logger.LogError(ex, "Error saving increment for EmployeeID {EmployeeId}", model.EmployeeId);
                return new CommonReturnViewModel { Success = false, Message = "An unexpected error occurred" };
            }
        }

        private async Task AddAutoApprovedHistoryAsync(EmployeeCareerChanges increment, SalaryChangeViewModel model, int firstApproverId, int stage)
        {
            try
            {
                var approvedStatus = await GetOrCreateStatusAsync("approve", new IncrementActionModel
                {
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                });

                string comment = "Auto Approved (Stage " + stage + " )";
                var history = new EmployeeCareerChangeHistory
                {
                    EmployeeCareerChangeID = increment.EmployeeCareerChangeID,
                    EmployeeID = model.EmployeeId,
                    StatusID = approvedStatus.StatusID,
                    ApprovalPersonID = firstApproverId,
                    Remarks = comment,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy
                };

                await _employeeCarrerCngHistoryRepository.AddAsync(history);
            }
            catch (Exception)
            {

                throw;
            }

            
        }

        #endregion


     


        #endregion

        #region Edit Save
        public async Task<IncrementApproveViewModel> GetPendingIncrementDetailsByID(int id)
        {
            var matches = new[] { "increment", "decrement" };
            var incrementIDs = await _employeeActionTypeRepository.AllActive()
                .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                .Select(x => x.EmployeeActionTypeID)
                .ToListAsync();

            var query = from ecc in _employeeCarrerCngRepository.All()
                        where incrementIDs.Contains(ecc.EmployeeActionTypeID ?? 0) && ecc.EmployeeCareerChangeID == id
                        join emp in _employeeRepository.All()
                            on ecc.EmployeeID equals emp.EmployeeID
                        join off in _empOfficialRepository.AllActive()
                            on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                        from office in empOfficeGroup.DefaultIfEmpty()
                        join dept in _deptRepository.AllActive()
                            on office.DepartmentID equals dept.DepartmentID into deptGroup
                        from department in deptGroup.DefaultIfEmpty()
                        join action in _employeeActionTypeRepository.AllActive()
                            on ecc.EmployeeActionTypeID equals action.EmployeeActionTypeID into actionGroup
                        from actionType in actionGroup.DefaultIfEmpty()
                        select new IncrementApproveViewModel
                        {
                            Id = ecc.EmployeeCareerChangeID,
                            EmployeeName = emp.FirstName + " " + emp.LastName,
                            Department = department.DepartmentName ?? "N/A",
                            IncrementType = actionType.EmployeeActionTypeName ?? "N/A",
                            CurrentSalary = ecc.CurrentSalary.HasValue
                                ? ecc.CurrentSalary.Value.ToString("C")
                                : "N/A",
                            ProposedSalary = ecc.NewSalary.HasValue
                                ? ecc.NewSalary.Value.ToString("C")
                                : "N/A",
                            IncrementAmount = ecc.NewSalary.HasValue && ecc.CurrentSalary.HasValue
                                ? (ecc.NewSalary.Value - ecc.CurrentSalary.Value).ToString("C")
                                : "N/A",
                            EffectiveDate = ecc.EffectiveDate.HasValue
                                ? ecc.EffectiveDate.Value.ToString("dd MMM yyyy")
                                : "N/A",
                            YearsOfExperience = office.JoiningDate.HasValue
                                ? $"{(DateOnly.FromDateTime(DateTime.Today).DayNumber - office.JoiningDate.Value.DayNumber) / 365.0:F1} Years"
                                : "N/A",
                            Justification = ecc.Remarks ?? "N/A",
                            AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                ? "../../assets/img/users/user-01.jpg"
                                : emp.EmployeeImageFileName,
                            Status = "Pending"
                        };

            return await query.FirstOrDefaultAsync();
        }

        public async Task<CommonReturnViewModel> ApproveIncrementAsync(IncrementActionModel action)
        {
            if (action == null || action.IncrementId <= 0 || string.IsNullOrEmpty(action.Action))
            {
                return new CommonReturnViewModel { Success = false, Message = "Invalid action model" };
            }

            try
            {
                var career = await _employeeCarrerCngRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.EmployeeCareerChangeID == action.IncrementId);
                if (career == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Increment not found" };
                }

                if (career.IsFinalApproved == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Increment already approved" };
                }

                if (career.IsDecline == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Increment has been declined" };
                }

                if (career.ApprovalPersonID != action.CreatedBy)
                {
                    return new CommonReturnViewModel { Success = false, Message = "You are not authorized to approve this increment" };
                }

                var status = await GetOrCreateStatusAsync(action.Action.ToLower(), action);
                if (status == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Failed to retrieve or create status" };
                }

                career.StatusID = status.StatusID;
                bool isDecline = action.Action.ToLower() == "decline";
                career.IsDecline = isDecline;

                if (!isDecline)
                {
                    career.ApprovalStage = (career.ApprovalStage ?? 0);
                    int nextApproverID = await ResolveNextApproverAsync(career);
                    if (nextApproverID != 0)
                    {
                        career.ApprovalPersonID = nextApproverID;
                        career.IsFinalApproved = false;
                        career.ApprovalStage = career.ApprovalStage + 1;
                    }
                    else
                    {
                        
                        career.IsFinalApproved = true;
                        career.ApprovalStage = career.ApprovalStage + 1;
                    }
                }
                else
                {
                    career.IsFinalApproved = false;
                    career.ApprovalStage = career.ApprovalStage ?? 1;
                }

                career.UpdatedAt = DateTime.UtcNow;
                career.UpdatedBy = action.UpdatedBy;
                career.LIP = action.LIP;
                career.LMAC = action.LMAC;

                await _employeeCarrerCngRepository.UpdateAsync(career);
                await LogCareerChangeHistoryAsync(career, action);

               

                return new CommonReturnViewModel { Success = true, Message = "Increment action completed successfully" };
            }
            catch (DbUpdateException ex)
            {
                return new CommonReturnViewModel { Success = false, Message = "Database error occurred while processing the increment action" };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel { Success = false, Message = "An unexpected error occurred while processing the increment action: " + ex.Message };
            }
        }

        #endregion

        #region Helper

        private async Task<Statuses> GetOrCreateStatusAsync(string action, IncrementActionModel actionModel)
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
                .FirstOrDefaultAsync(a => a.ApprovalTypeName.ToLower() == "increment approval");
            if (approvalType == null)
                return 0;

            var approvalSettings = await _approvalSettingRepository.AllActive()
                .FirstOrDefaultAsync(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                    && a.OrganizationID == employee.OrganizationID
                    && a.OrganizationBranchID == employee.OrganizationBranchID);
            if (approvalSettings == null)
                return 0;

            int currentStage = career.ApprovalStage.Value ;
            if (currentStage == 1 && approvalSettings.IsEnableSecondApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 2);
            }
            else if (currentStage == 2 && approvalSettings.IsEnableThirdApproval)
            {
                return await ResolveApproverAsync(approvalSettings, employee, 3);
            }
            else if (currentStage == 3 && ((bool)!approvalSettings.AllowSelfApproval))
            {
                return await ResolveApproverAsync(approvalSettings, employee, 4);
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
            else if (stage == 4)
            {
                if ((bool)settings.AllowSelfApproval) return 0;
                approverID = settings.SelfExceptionApprovalID;
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

        private async Task LogCareerChangeHistoryAsync(EmployeeCareerChanges career, IncrementActionModel action, bool isAuto = false)
        {
            var history = new EmployeeCareerChangeHistory
            {
                EmployeeCareerChangeID = career.EmployeeCareerChangeID,
                EmployeeID = career.EmployeeID,
                StatusID = career.StatusID,
                ApprovalPersonID = isAuto ? career.ApprovalPersonID : action.CreatedBy,
                Remarks = action.Comments,
                LIP = action.LIP,
                LMAC = action.LMAC,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = action.CreatedBy
            };
            await _employeeCarrerCngHistoryRepository.AddAsync(history);
        }

        #endregion

    }
}







