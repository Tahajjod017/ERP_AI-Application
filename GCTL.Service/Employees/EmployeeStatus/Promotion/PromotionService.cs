using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeStatus.Promotion
{
    public class PromotionService : IPromotionService
    {
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeeRepository;

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


        public PromotionService(IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository, IGenericRepository<EmployeeCareerChangeHistory> employeeCarrerCngHistoryRepository, IGenericRepository<EmployeeCareerChanges> employeeCarrerCngRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<EmployeeOfficeInfo> empOfficialRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<ApprovalDesignation> approvalDesignationRepository, IGenericRepository<Departments> deptRepository, IGenericRepository<Designations> desigRepository, IGenericRepository<Data.Models.Employees> employeeRepository)
        {
            _employeeActionTypeRepository = employeeActionTypeRepository;
            _employeeCarrerCngHistoryRepository = employeeCarrerCngHistoryRepository;
            _employeeCarrerCngRepository = employeeCarrerCngRepository;
            _statusRepository = statusRepository;
            _empOfficialRepository = empOfficialRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _approvalDesignationRepository = approvalDesignationRepository;
            _deptRepository = deptRepository;
            _desigRepository = desigRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<List<PromotionApproveViewModel>> GetAllPromotionPendingList()
        {
            var matches = new[] { "promotion", "demotion" };

            var proDemoIDs = await _employeeActionTypeRepository.AllActive()
                .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                .Select(x => x.EmployeeActionTypeID)
                .ToListAsync();

            var query = from ecc in _employeeCarrerCngRepository.All()
                        where proDemoIDs.Contains(ecc.EmployeeActionTypeID ?? 0)
                        join emp in _employeeRepository.All()
                            on ecc.EmployeeID equals emp.EmployeeID

                        join off in _empOfficialRepository.AllActive()
                            on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                        from office in empOfficeGroup.DefaultIfEmpty()

                        join desg in _desigRepository.AllActive()
                            on office.DesignationID equals desg.DesignationID into desgGroup
                        from designation in desgGroup.DefaultIfEmpty()

                        join dept in _deptRepository.AllActive()
                            on office.DepartmentID equals dept.DepartmentID into deptGroup
                        from department in deptGroup.DefaultIfEmpty()

                        join currDesg in _desigRepository.AllActive()
                           on ecc.CurentDesignationID equals currDesg.DesignationID into currDesgGroup
                        from currDesignation in currDesgGroup.DefaultIfEmpty()

                        join proposedDesg in _desigRepository.AllActive()
                            on ecc.CurentDesignationID equals proposedDesg.DesignationID into propDesgGroup
                        from proposedDesignation in propDesgGroup.DefaultIfEmpty()

                        select new PromotionApproveViewModel
                        {
                            Id = ecc.EmployeeCareerChangeID,
                            EmployeeName = emp.FirstName + " " + emp.LastName,
                            Department = department.DepartmentName ?? "N/A",
                            CurrentPosition = currDesignation.DesignationName,
                            ProposedPosition = proposedDesignation.DesignationName ?? "N/A",
                            CurrentSalary = ecc.CurrentSalary.HasValue
                                ? ecc.CurrentSalary.Value.ToString("C")
                                : "N/A",
                            ProposedSalary = ecc.NewSalary.HasValue
                                ? ecc.NewSalary.Value.ToString("C")
                                : "N/A",
                            EffectiveDate = ecc.EffectiveDate.Value.ToString("dd MMM yyyy"),

                            YearsOfExperience = office.JoiningDate.HasValue
                                    ? $"{(DateOnly.FromDateTime(DateTime.Today).DayNumber - office.JoiningDate.Value.DayNumber) / 365.0:F1} Years"
                                    : "N/A",


                            Justification = ecc.Remarks,
                            AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                ? "../../assets/img/users/user-01.jpg"
                                : emp.EmployeeImageFileName,
                            Status = "Pending"
                        };

            return await query.ToListAsync();
        }


        #region Pending And Approval Table

        #region Pending List

        public async Task<object> GetFilteredPromotionsAsync(PromotionFilterModel filter, string imgLink, int? loggedID)
        {
            // Start with a queryable instead of loading all data into memory
            var query = await GetPromotionQueryAsync(true, imgLink , loggedID);

            // Apply filters at database level
            query = ApplyFilters(query, filter);

            // Apply sorting at database level
            query = ApplySorting(query, filter);

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Apply pagination at database level
            int page = filter.Page > 0 ? filter.Page : 1;
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedPromotions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { TotalPages = totalPages, TotalItems = totalItems, Promotions = paginatedPromotions };
        }


        #endregion

        #region Approve List
        public async Task<object> GetFilteredApprovePromotionsAsync(PromotionFilterModel filter, string imgLink, int? loggedID)
        {
            // Start with a queryable instead of loading all data into memory
            var query = await GetPromotionQueryAsync(false , imgLink, loggedID);

            // Apply filters at database level
            query = ApplyFilters(query, filter);

            // Apply sorting at database level
            query = ApplySorting(query, filter);

            // Get total count before pagination
            var totalItems = await query.CountAsync();

            // Apply pagination at database level
            int page = filter.Page > 0 ? filter.Page : 1;
            int pageSize = filter.PageSize > 0 ? filter.PageSize : 10;
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var paginatedPromotions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new { TotalPages = totalPages, TotalItems = totalItems, Promotions = paginatedPromotions };
        }

        #endregion


        #region Common


        private async Task<IQueryable<PromotionApproveViewModel>> GetPromotionQueryAsync(bool IsPending, string imgLink, int? loggedID)
        {
            var matches = new[] { "promotion", "demotion" };
            var proDemoIDs = await _employeeActionTypeRepository.AllActive()
                .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                .Select(x => x.EmployeeActionTypeID)
                .ToListAsync();

            
                var staPending1 = _statusRepository.AllActive().Where(s => s.StatusName.ToLower() == "pending").Select(s => s.StatusID).ToListAsync().Result;
            
                var staPending = _statusRepository.AllActive().Where(s => s.StatusName.ToLower() == "approve" || s.StatusName.ToLower() == "decline").Select(s => s.StatusID).ToListAsync().Result;
            

            



            var query = from ecc in _employeeCarrerCngRepository.All()
                        where proDemoIDs.Contains(ecc.EmployeeActionTypeID ?? 0) 
                        //&& (
                        //       ecc.IsFinalApproved == IsApprove ||
                        //       (!IsApprove && ecc.IsFinalApproved == null)
                        //   )
                        && ecc.ApprovalPersonID == loggedID


                        join emp in _employeeRepository.All()
                            on ecc.EmployeeID equals emp.EmployeeID

                        join off in _empOfficialRepository.AllActive()
                            on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                        from office in empOfficeGroup.DefaultIfEmpty()

                        join desg in _desigRepository.AllActive()
                            on office.DesignationID equals desg.DesignationID into desgGroup
                        from designation in desgGroup.DefaultIfEmpty()

                        join dept in _deptRepository.AllActive()
                            on office.DepartmentID equals dept.DepartmentID into deptGroup
                        from department in deptGroup.DefaultIfEmpty()

                        join currDesg in _desigRepository.AllActive()
                            on ecc.CurentDesignationID equals currDesg.DesignationID into currDesgGroup
                        from currDesignation in currDesgGroup.DefaultIfEmpty()

                        join proposedDesg in _desigRepository.AllActive()
                            on ecc.NewDesignationID equals proposedDesg.DesignationID into propDesgGroup
                        from proposedDesignation in propDesgGroup.DefaultIfEmpty()

                        

                        join status in _statusRepository.AllActive()
                            on ecc.StatusID equals status.StatusID into statusGroup
                        from status in statusGroup.DefaultIfEmpty()

                        where status != null &&
                              (IsPending ? status.StatusName.ToLower() == "pending" :
                              (status.StatusName.ToLower() == "approved" || status.StatusName.ToLower() == "decline"))

                        select new PromotionApproveViewModel
                        {
                            Id = ecc.EmployeeCareerChangeID,
                            EmployeeName = emp.FirstName + " " + emp.LastName,
                            Department = department.DepartmentName ?? "N/A",
                            CurrentPosition = currDesignation.DesignationName ?? "N/A",
                            ProposedPosition = proposedDesignation.DesignationName ?? "N/A",
                            CurrentSalary = ecc.CurrentSalary.HasValue
                                ? ecc.CurrentSalary.Value.ToString("C")
                                : "N/A",
                            ProposedSalary = ecc.NewSalary.HasValue
                                ? ecc.NewSalary.Value.ToString("C")
                                : "N/A",
                            EffectiveDate = ecc.EffectiveDate.HasValue
                                ? ecc.EffectiveDate.Value.ToString("dd MMM yyyy")
                                : "N/A",
                            EffectiveDateRaw = ecc.EffectiveDate, // Add raw date for filtering/sorting
                            YearsOfExperience = office.JoiningDate.HasValue
                            ? $"{(DateTime.Today.Year - office.JoiningDate.Value.Year):F1} Years"
                            : "N/A",
                            Justification = ecc.Remarks ?? "N/A",
                            AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                ? "../../assets/img/users/user-01.jpg"
                                : imgLink + emp.EmployeeImageFileName,
                            Status = status != null ? status.StatusName : "Pending",
                        };


            var a = query.ToListAsync().Result;

            return query;

           
        }

        private IQueryable<PromotionApproveViewModel> ApplyFilters(IQueryable<PromotionApproveViewModel> query, PromotionFilterModel filter)
        {
            if (!string.IsNullOrEmpty(filter.PromotionType))
            {
                query = query.Where(p => p.ProposedPosition.Contains(filter.PromotionType));
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(p => p.Status == filter.Status);
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

        private IQueryable<PromotionApproveViewModel> ApplySorting(IQueryable<PromotionApproveViewModel> query, PromotionFilterModel filter)
        {
            // Handle legacy SortBy first
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

            // Handle modern column-based sorting
            if (!string.IsNullOrEmpty(filter.SortColumn))
            {
                bool isAscending = filter.SortDirection?.ToLower() != "desc";

                query = filter.SortColumn.ToLower() switch
                {
                    "employeename" => isAscending
                        ? query.OrderBy(p => p.EmployeeName)
                        : query.OrderByDescending(p => p.EmployeeName),
                    "currentposition" => isAscending
                        ? query.OrderBy(p => p.CurrentPosition)
                        : query.OrderByDescending(p => p.CurrentPosition),
                    "proposedposition" => isAscending
                        ? query.OrderBy(p => p.ProposedPosition)
                        : query.OrderByDescending(p => p.ProposedPosition),
                    "currentsalary" => isAscending
                        ? query.OrderBy(p => p.CurrentSalaryNumeric)
                        : query.OrderByDescending(p => p.CurrentSalaryNumeric),
                    "proposedsalary" => isAscending
                        ? query.OrderBy(p => p.ProposedSalaryNumeric)
                        : query.OrderByDescending(p => p.ProposedSalaryNumeric),
                    "effectivedate" => isAscending
                        ? query.OrderBy(p => p.EffectiveDateRaw)
                        : query.OrderByDescending(p => p.EffectiveDateRaw),
                    _ => query.OrderBy(p => p.Id)
                };
            }
            else if (string.IsNullOrEmpty(filter.SortBy))
            {
                // Default sorting if no sort specified
                query = query.OrderBy(p => p.Id);
            }

            return query;
        }

        #endregion

        #endregion

        public Task GetPagedPromotionListAsync(PromotionListFilterViewModel filters)
        {
            throw new NotImplementedException();
        }


        #region Save Method

        public async Task<CommonReturnViewModel> SaveAsync(PromotionViewModel model)
        {
         
            if (model == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid model"
                };
            }

            #region Action Type and Status Setup
            // Ensure EmployeeActionType exists (Promotion/Demotion)
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

            // Ensure "Pending" Status exists
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

            #region Approve Hunt

            var employee = await _empOfficialRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeID);

            if (employee == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Employee Official not found"
                };
            }
            bool IsSelfApproval = false; // Default value for self-approval
            int approvelPersonID = 0; 

            var approvalType = await _approvalTypeRepository.AllActive().Where(a => a.ApprovalTypeName.ToLower() == "promotion approval").FirstOrDefaultAsync();

            if (approvalType != null)
            {
                var approvalSettings = await _approvalSettingRepository.AllActive().Where(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                && a.OrganizationID == employee.OrganizationID && a.OrganizationBranchID == employee.OrganizationBranchID).FirstOrDefaultAsync();

                if (approvalSettings != null && approvalSettings.IsEnableSecondApproval)
                {
                    if (approvalSettings.IsDesignationOrEmpFirstApprovalID)
                    {
                        var approvaldesig = _approvalDesignationRepository.AllActive().Where(e => e.ApprovalDesignationID == approvalSettings.FirstApprovalID).FirstOrDefault();

                        switch (approvaldesig?.Code)
                        {
                            case 1:
                                approvelPersonID = employee.ImmediateSupervisorId ?? 0; // Assuming ReportingManagerID is the first approver
                                break;
                            case 2:
                                approvelPersonID = employee.SeniorSupervisorId ?? 0; // Assuming ReportingManagerID is the first approver

                                break;
                            case 3:
                                approvelPersonID = employee.HeadOfDepartmentId ?? 0; // Assuming ReportingManagerID is the first approver

                                break;
                            default:
                                approvelPersonID = 0; // Default case if no valid designation is found
                                break;
                        }

                    }
                    else
                    {
                        if (approvalSettings.FirstApprovalID == employee.EmployeeID)
                        {
                            if (approvalSettings.IsDesignationOrEmpSecondApprovalID)
                            {
                                var approvaldesig = _approvalDesignationRepository.AllActive().Where(e => e.ApprovalDesignationID == approvalSettings.FirstApprovalID).FirstOrDefault();

                                switch (approvaldesig?.Code)
                                {
                                    case 1:
                                        approvelPersonID = employee.ImmediateSupervisorId ?? 0; // Assuming ReportingManagerID is the first approver
                                        break;
                                    case 2:
                                        approvelPersonID = employee.SeniorSupervisorId ?? 0; // Assuming ReportingManagerID is the first approver

                                        break;
                                    case 3:
                                        approvelPersonID = employee.HeadOfDepartmentId ?? 0; // Assuming ReportingManagerID is the first approver

                                        break;
                                    default:
                                        approvelPersonID = 0; // Default case if no valid designation is found
                                        break;
                                }

                            }
                            else
                            {
                                approvelPersonID = approvalSettings.SecondApprovalID ?? 0;
                            }
                        }
                        approvelPersonID = approvalSettings.FirstApprovalID ?? 0; // Assuming FirstApprovalID is the first approver
                    }
                }
                else
                {
                    if (approvalSettings != null && approvalSettings.IsDesignationOrEmpFirstApprovalID)
                    {
                        var approvaldesig = _approvalDesignationRepository.AllActive().Where(e => e.ApprovalDesignationID == approvalSettings.FirstApprovalID).FirstOrDefault();

                        switch (approvaldesig?.Code)
                        {
                            case 1:
                                approvelPersonID = employee.ImmediateSupervisorId ?? 0; // Assuming ReportingManagerID is the first approver
                                break;
                            case 2:
                                approvelPersonID = employee.SeniorSupervisorId ?? 0; // Assuming ReportingManagerID is the first approver

                                break;
                            case 3:
                                approvelPersonID = employee.HeadOfDepartmentId ?? 0; // Assuming ReportingManagerID is the first approver

                                break;
                            default:
                                approvelPersonID = 0; // Default case if no valid designation is found
                                break;
                        }

                    }
                    else
                    {
                        if (approvalSettings != null && approvalSettings.FirstApprovalID == employee.EmployeeID)
                        {
                            if (approvalSettings.AllowSelfApproval == true)
                            {
                                IsSelfApproval = true; // Self-approval is allowed
                            }
                            else
                            {
                                approvelPersonID = approvalSettings.SelfExceptionApprovalID ?? 0; // Assuming SelfExceptionApprovalID is the next approver if self-approval is not allowed
                            }
                        }
                        approvelPersonID = approvalSettings.FirstApprovalID ?? 0; // Assuming FirstApprovalID is the first approver
                    }
                }

                
            }


            #endregion

            try
            {
                

                var promotion = new EmployeeCareerChanges
                {
                    EmployeeID = model.EmployeeID,
                    EmployeeActionTypeID = actionType.EmployeeActionTypeID,
                    CurentDesignationID = model.CurrentDesignationID,
                    NewDesignationID = model.NewDesignationID,
                    
                    EffectiveDate = model.EffectiveDate,
                    Remarks = model.Remarks,
                    CurrentSalary = model.CurrentSalary,
                    NewSalary = model.NewSalary,
                    StatusID = status.StatusID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };

                if (approvelPersonID != 0)
                {
                    promotion.ApprovalPersonID = approvelPersonID;
                }
                if (IsSelfApproval)
                {
                    promotion.IsFinalApproved = true;
                }

                await _employeeCarrerCngRepository.AddAsync(promotion);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Promotion saved successfully"
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the promotion: " + ex.Message
                };
            }
        }



        #endregion

        #region Get Promotion Details By ID
        public async Task<PromotionApproveViewModel> GetPendingPromotionDetailsByID(int id)
        {
            var matches = new[] { "promotion", "demotion" };

            var proDemoIDs = await _employeeActionTypeRepository.AllActive()
                .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                .Select(x => x.EmployeeActionTypeID)
                .ToListAsync();

            var query = from ecc in _employeeCarrerCngRepository.All()
                        where proDemoIDs.Contains(ecc.EmployeeActionTypeID ?? 0) && ecc.EmployeeCareerChangeID == id
                        join emp in _employeeRepository.All()
                            on ecc.EmployeeID equals emp.EmployeeID

                        join off in _empOfficialRepository.AllActive()
                            on emp.EmployeeID equals off.EmployeeID into empOfficeGroup
                        from office in empOfficeGroup.DefaultIfEmpty()

                        join desg in _desigRepository.AllActive()
                            on office.DesignationID equals desg.DesignationID into desgGroup
                        from designation in desgGroup.DefaultIfEmpty()

                        join dept in _deptRepository.AllActive()
                            on office.DepartmentID equals dept.DepartmentID into deptGroup
                        from department in deptGroup.DefaultIfEmpty()

                        join currDesg in _desigRepository.AllActive()
                           on ecc.CurentDesignationID equals currDesg.DesignationID into currDesgGroup
                        from currDesignation in currDesgGroup.DefaultIfEmpty()

                        join proposedDesg in _desigRepository.AllActive()
                            on ecc.CurentDesignationID equals proposedDesg.DesignationID into propDesgGroup
                        from proposedDesignation in propDesgGroup.DefaultIfEmpty()

                        select new PromotionApproveViewModel
                        {
                            Id = ecc.EmployeeCareerChangeID,
                            EmployeeName = emp.FirstName + " " + emp.LastName,
                            Department = department.DepartmentName ?? "N/A",
                            CurrentPosition = currDesignation.DesignationName,
                            ProposedPosition = proposedDesignation.DesignationName ?? "N/A",
                            CurrentSalary = ecc.CurrentSalary.HasValue
                                ? ecc.CurrentSalary.Value.ToString("C")
                                : "N/A",
                            ProposedSalary = ecc.NewSalary.HasValue
                                ? ecc.NewSalary.Value.ToString("C")
                                : "N/A",
                            EffectiveDate = ecc.EffectiveDate.Value.ToString("dd MMM yyyy"),

                            YearsOfExperience = office.JoiningDate.HasValue
                                    ? $"{(DateOnly.FromDateTime(DateTime.Today).DayNumber - office.JoiningDate.Value.DayNumber) / 365.0:F1} Years"
                                    : "N/A",


                            Justification = ecc.Remarks,
                            AvatarUrl = string.IsNullOrEmpty(emp.EmployeeImageFileName)
                                ? "../../assets/img/users/user-01.jpg"
                                : emp.EmployeeImageFileName,
                            Status = "Pending"
                        };

            return await query.FirstOrDefaultAsync();
        }
        #endregion

        #region Approve or Decline Promotion

        public async Task<CommonReturnViewModel> ApprovePromotionAsync(PromotionActionModel action)
        {
            try
            {
                if (action == null || action.PromotionId <= 0 || string.IsNullOrEmpty(action.Action))
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Invalid action model"
                    };
                }

                var carrer = await _employeeCarrerCngRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeCareerChangeID == action.PromotionId);
                if (carrer == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Promotion not found"
                    };
                }

                if (carrer.IsFinalApproved == true)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Promotion already approved"
                    };
                }

                if (carrer.IsDecline == true)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Promotion has been declined"
                    };
                }

                if (carrer.ApprovalPersonID != action.CreatedBy)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "You are not authorized to approve this promotion"
                    };
                }

                if (action.Action == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Action cannot be null"
                    };

                }


                if (action.Action == "approve")
                {

                    var status = await _statusRepository.AllActive()
                        .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "approved");
                    if (status == null)
                    {
                        status = new Statuses
                        {
                            StatusName = "Approved",
                            StatusType = "EmployeeCareerChange",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = action.CreatedBy,
                            LIP = action.LIP,
                            LMAC = action.LMAC
                        };
                        await _statusRepository.AddAsync(status);
                    }
                    carrer.StatusID = status.StatusID;


                    int secondApproverID = 0;

                    #region Second Approve Hunt

                    var employee = await _empOfficialRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == carrer.EmployeeID);

                    if (employee == null)
                    {
                        return new CommonReturnViewModel
                        {
                            Success = false,
                            Message = "Employee Official not found"
                        };
                    }


                    var approvalType = await _approvalTypeRepository.AllActive().Where(a => a.ApprovalTypeName.ToLower() == "promotion approval").FirstOrDefaultAsync();

                    if (approvalType != null)
                    {
                        var approvalSettings = await _approvalSettingRepository.AllActive().Where(a => a.ApprovalTypeID == approvalType.ApprovalTypeID
                                            && a.OrganizationID == employee.OrganizationID && a.OrganizationBranchID == employee.OrganizationBranchID).FirstOrDefaultAsync();


                        if (approvalSettings != null && approvalSettings.IsEnableSecondApproval)
                        {
                            if (approvalSettings.IsDesignationOrEmpSecondApprovalID)
                            {
                                var secondApprovalDesig = _approvalDesignationRepository.AllActive()
                                    .Where(e => e.ApprovalDesignationID == approvalSettings.SecondApprovalID)
                                    .FirstOrDefault();

                                switch (secondApprovalDesig?.Code)
                                {
                                    case 1:
                                        secondApproverID = employee.ImmediateSupervisorId ?? 0;
                                        break;
                                    case 2:
                                        secondApproverID = employee.SeniorSupervisorId ?? 0;
                                        break;
                                    case 3:
                                        secondApproverID = employee.HeadOfDepartmentId ?? 0;
                                        break;
                                    default:
                                        secondApproverID = 0;
                                        break;
                                }
                            }
                            else
                            {
                                // When it's employee-based SecondApprovalID (not designation)
                                if (approvalSettings.SecondApprovalID == employee.EmployeeID)
                                {
                                    if ((bool)approvalSettings.AllowSelfApproval)
                                    {
                                        bool IsSelfApproval = true;
                                        secondApproverID = (int)employee.EmployeeID;
                                    }
                                    else
                                    {
                                        secondApproverID = approvalSettings.SelfExceptionApprovalID ?? 0;
                                    }
                                }
                                else
                                {
                                    secondApproverID = approvalSettings.SecondApprovalID ?? 0;
                                }
                            }
                        }
                    }

                    #endregion

                    if (secondApproverID != 0)
                    {
                        carrer.ApprovalPersonID = secondApproverID; // Set the next approver
                        carrer.IsFinalApproved = false; // Mark as not final approved yet
                    }
                    else
                    {
                        carrer.IsFinalApproved = true; // If no second approver, mark as final approved
                    }





                }
                else
                {


                    var status = await _statusRepository.AllActive()
                        .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "decline");
                    if (status == null)
                    {
                        status = new Statuses
                        {
                            StatusName = "Decline",
                            StatusType = "EmployeeCareerChange",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = action.CreatedBy,
                            LIP = action.LIP,
                            LMAC = action.LMAC
                        };
                        await _statusRepository.AddAsync(status);
                    }
                    carrer.StatusID = status.StatusID;
                    carrer.IsDecline = true;
                }




                carrer.IsFinalApproved = true; // Mark as final approved
                carrer.UpdatedAt = DateTime.UtcNow;
                carrer.UpdatedBy = action.UpdatedBy;
                carrer.LIP = action.LIP;
                carrer.LMAC = action.LMAC;
                await _employeeCarrerCngRepository.UpdateAsync(carrer);
                // Add to history
                var history = new EmployeeCareerChangeHistory
                {
                    EmployeeCareerChangeID = carrer.EmployeeCareerChangeID,
                    EmployeeID = carrer.EmployeeID,
                    StatusID = carrer.StatusID,
                    ApprovalPersonID = carrer.ApprovalPersonID,
                    Remarks = action.Comments,
                    LIP = action.LIP,
                    LMAC = action.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = action.CreatedBy
                };
                await _employeeCarrerCngHistoryRepository.AddAsync(history);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Promotion action completed successfully"
                };
            }
            catch (Exception)
            {
               
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while processing the promotion action"
                };

               
            }
            
            
            
        }

        #endregion


    }
}
