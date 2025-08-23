using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using static System.Collections.Specialized.BitVector32;

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
        private readonly IGenericRepository<Alerts> alertsRepository;
        private readonly IGenericRepository<AlertForEmployee> alertForEmployeeRepository;


        public PromotionService(IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository, IGenericRepository<EmployeeCareerChangeHistory> employeeCarrerCngHistoryRepository, IGenericRepository<EmployeeCareerChanges> employeeCarrerCngRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<EmployeeOfficeInfo> empOfficialRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<ApprovalDesignation> approvalDesignationRepository, IGenericRepository<Departments> deptRepository, IGenericRepository<Designations> desigRepository, IGenericRepository<Data.Models.Employees> employeeRepository, IGenericRepository<Alerts> alertsRepository, IGenericRepository<AlertForEmployee> alertForEmployeeRepository)
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
            this.alertsRepository = alertsRepository;
            this.alertForEmployeeRepository = alertForEmployeeRepository;
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
            var query = await GetPendingPromotionQueryAsync( imgLink , loggedID);

            
            query = ApplySearch(query, filter);

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
            var query = await GetApprovePromotionQueryAsync( imgLink, loggedID);

            
            query = ApplySearch(query, filter);

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

        private IQueryable<PromotionApproveViewModel> ApplySearch(IQueryable<PromotionApproveViewModel> query, PromotionFilterModel filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.SearchInput))
            {
                var keyword = filter.SearchInput.Trim().ToLower();

                query = query.Where(p =>
                    (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(p.Department) && p.Department.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(p.CurrentSalary) && p.CurrentSalary.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(p.Status) && p.Status.ToLower().Contains(keyword))
                );
            }

            return query;
        }

        #endregion


        #region Common


        private async Task<IQueryable<PromotionApproveViewModel>> GetPendingPromotionQueryAsync( string imgLink, int? loggedID)
        {
            try
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
                            && (ecc.IsDecline == null  || ecc.IsDecline == false)
                            && (ecc.IsFinalApproved == null || ecc.IsFinalApproved == false)
                            && ecc.ApprovalPersonID == loggedID
                            && ecc.ApprovalPersonID != ecc.UpdatedBy

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

                            //where status != null &&
                            //      (IsPending ? status.StatusName.ToLower() == "pending" :
                            //      (status.StatusName.ToLower() == "approved" || status.StatusName.ToLower() == "decline"))

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
                                //Status = status != null ? status.StatusName : "Pending",
                                Status =  status.StatusName,
                            };


                var a = query.ToListAsync().Result;

                return query;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<PromotionApproveViewModel>().AsQueryable(); // Return an empty queryable on error
            }
            

           
        }

        private async Task<IQueryable<PromotionApproveViewModel>> GetApprovePromotionQueryAsync( string imgLink, int? loggedID)
        {
            try
            {
                var matches = new[] { "promotion", "demotion" };
                var proDemoIDs = await _employeeActionTypeRepository.AllActive()
                    .Where(x => matches.Contains(x.EmployeeActionTypeName.ToLower()))
                    .Select(x => x.EmployeeActionTypeID)
                    .ToListAsync();


                var staPending1 = _statusRepository.AllActive().Where(s => s.StatusName.ToLower() == "pending").Select(s => s.StatusID).ToListAsync().Result;

                var staPending = _statusRepository.AllActive().Where(s => s.StatusName.ToLower() == "approve" || s.StatusName.ToLower() == "decline").Select(s => s.StatusID).ToListAsync().Result;


                var test = (from ecHis in _employeeCarrerCngHistoryRepository.All()
                            join ecc in _employeeCarrerCngRepository.All()
                                 on ecHis.EmployeeCareerChangeID equals ecc.EmployeeCareerChangeID

                            where proDemoIDs.Contains(ecc.EmployeeActionTypeID ?? 0)

                            && ecHis.ApprovalPersonID == loggedID
                            select new { ecHis, ecc }).ToList();



                var query = from ecHis in _employeeCarrerCngHistoryRepository.All()
                            join ecc in _employeeCarrerCngRepository.All()
                                on ecHis.EmployeeCareerChangeID equals ecc.EmployeeCareerChangeID

                            where proDemoIDs.Contains(ecc.EmployeeActionTypeID ?? 0)

                            // && (ecc.IsDecline == true
                            //|| ecc.IsFinalApproved == true)

                            && ecHis.ApprovalPersonID == loggedID


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

                                //where status != null &&
                                //      (IsPending ? status.StatusName.ToLower() == "pending" :
                                //      (status.StatusName.ToLower() == "approved" || status.StatusName.ToLower() == "decline"))

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new List<PromotionApproveViewModel>().AsQueryable(); // Return an empty queryable on error
            }



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
            if (model == null || model.EffectiveDate < DateTime.UtcNow.Date)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid model"
                };
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
                    //actionType = new EmployeeActionTypes
                    //{
                    //    EmployeeActionTypeName = model.ChangeType,
                    //    CreatedAt = DateTime.UtcNow,
                    //    CreatedBy = model.CreatedBy,
                    //    LIP = model.LIP,
                    //    LMAC = model.LMAC
                    //};
                    //await _employeeActionTypeRepository.AddAsync(actionType);
                    return new CommonReturnViewModel { Success = false, Message = "Employee Action type not found !" };
                }

                var status = await _statusRepository.AllActive()
                    .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "pending");
                if (status == null)
                {
                    status = new Statuses
                    {
                        StatusName = "Pending",
                        StatusType = "AppDecPen",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _statusRepository.AddAsync(status);
                }
                #endregion

                #region Employee Validation

                var employee = await _empOfficialRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeID);

                var empInfo = await _employeeRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == model.EmployeeID);

                if (employee == null || empInfo == null)
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

                bool isSelfApplication = model.CreatedBy == model.EmployeeID; // Check if applying for self

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


                var increment = new EmployeeCareerChanges
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
                    ApprovalPersonID = firstApproverId,
                    ApprovalStage = 1, // Start at stage 1
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };

                // Save the promotion
                await _employeeCarrerCngRepository.AddAsync(increment);

                #region Auto Approve By employee

                if (model.EmployeeID == firstApproverId)
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
                else if (model.EmployeeID == secondApproverId)
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
                else if (model.EmployeeID == thirdApproverId)
                {
                    var a = AddAutoApprovedHistoryAsync(increment, model, firstApproverId, 1);
                    var b = AddAutoApprovedHistoryAsync(increment, model, secondApproverId, 2);
                    var c = AddAutoApprovedHistoryAsync(increment, model, thirdApproverId, 3);

                    if (!AllowSelfApproval)
                    {
                        increment.ApprovalPersonID = selfApprovalExceptionId;
                        increment.ApprovalStage = 4;
                    }
                    else
                    {
                        increment.IsFinalApproved = true;
                    }
                    await _employeeCarrerCngRepository.UpdateAsync(increment);
                }


                #endregion


                #region Alert


                var alert = new Alerts
                {
                    AlertTitle = "Employee Increment",
                    AlertNote = $"{empInfo.FirstName} {empInfo.LastName} has requested an increment.",
                    LMAC = model.LMAC,
                    LIP = model.LIP,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.Now,
                };

                await alertsRepository.AddAsync(alert);
                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = increment.ApprovalPersonID,  // for alert Employee
                    IsChecked = false,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                };
                await alertForEmployeeRepository.AddAsync(empAlert);

                #endregion


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
            if (action == null || action.PromotionId <= 0 || string.IsNullOrEmpty(action.Action))
            {
                return new CommonReturnViewModel { Success = false, Message = "Invalid action model" };
            }

            try
            {


                // using var transaction = await _employeeCarrerCngRepository.BeginTransactionAsync();


                var career = await _employeeCarrerCngRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.EmployeeCareerChangeID == action.PromotionId);
                if (career == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Promotion not found" };
                }

                if (career.IsFinalApproved == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Promotion already approved" };
                }

                if (career.IsDecline == true)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Promotion has been declined" };
                }

                if (career.ApprovalPersonID != action.CreatedBy)
                {
                    return new CommonReturnViewModel { Success = false, Message = "You are not authorized to approve this promotion" };
                }

                var employee = await _employeeRepository.AllActive().FirstOrDefaultAsync(e => e.EmployeeID == career.EmployeeID);
                if (employee == null)
                {
                    return new CommonReturnViewModel { Success = false, Message = "Employee not found" };
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


                if (career.IsFinalApproved != true && career.IsDecline != true)
                {
                    var alert = new Alerts
                    {
                        AlertTitle = "Employee Increment",
                        AlertNote = $"{employee.FirstName} {employee.LastName} has requested an increment.",
                        LMAC = action.LMAC,
                        LIP = action.LIP,
                        CreatedBy = action.CreatedBy,
                        CreatedAt = DateTime.Now,
                    };

                    await alertsRepository.AddAsync(alert);
                    var empAlert = new AlertForEmployee
                    {
                        AlertID = alert.AlertID,
                        EmployeeID = career.ApprovalPersonID,  // for alert Employee
                        IsChecked = false,
                        LIP = action.LIP,
                        LMAC = action.LMAC,
                        CreatedAt = DateTime.Now,
                        CreatedBy = action.CreatedBy,
                    };
                    await alertForEmployeeRepository.AddAsync(empAlert);
                }


                return new CommonReturnViewModel { Success = true, Message = "Promotion action completed successfully" };
            }





            catch (DbUpdateException ex)
            {
                // Log exception (e.g., using ILogger)
                return new CommonReturnViewModel { Success = false, Message = "Database error occurred while processing the promotion action" };
            }
            catch (Exception ex)
            {
                // Log exception
                return new CommonReturnViewModel { Success = false, Message = "An unexpected error occurred while processing the promotion action" };
            }


        }


        #endregion

        #region Helper

        private async Task AddAutoApprovedHistoryAsync(EmployeeCareerChanges increment, PromotionViewModel model, int firstApproverId, int stage)
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
                var history = new EmployeeCareerChangeHistory
                {
                    EmployeeCareerChangeID = increment.EmployeeCareerChangeID,
                    EmployeeID = model.EmployeeID,
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
                    StatusType = "AppDecPen",
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

        private async Task LogCareerChangeHistoryAsync(EmployeeCareerChanges career, PromotionActionModel action, bool isAuto = false)
        {
            var history = new EmployeeCareerChangeHistory
            {
                EmployeeCareerChangeID = career.EmployeeCareerChangeID,
                EmployeeID = career.EmployeeID,
                StatusID = career.StatusID,
                ApprovalPersonID = isAuto ? career.ApprovalPersonID : action.CreatedBy,
               // ApprovalPersonID = career.ApprovalPersonID,
                //ApprovalPersonID = action.CreatedBy,
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
