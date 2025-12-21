using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Core.ViewModels.POS.Requsition.RequisitionApprover;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Requsition.RequisitionApprover
{
    public class RequisitionApproverService : IRequisitionApproverService
    {
        private readonly IGenericRepository<Requisitions> _requisitionRepository;
        private readonly IGenericRepository<RequisitionItems> _requisitionItemRepository;
        private readonly IGenericRepository<ReqItemApprovalHistory> _approvalHistoryRepository;
        private readonly IGenericRepository<ReqApprovalStepApprovers> _approvalStepRepository;
        private readonly IGenericRepository<ReqApprovalSettings> _approvalSettingsRepository;
        private readonly IGenericRepository<Alerts> _alertsRepository;
        private readonly IGenericRepository<AlertForEmployee> _alertForEmployeeRepository;
        private readonly IUserInfoService _userInfoService;

        public RequisitionApproverService(
            IGenericRepository<Requisitions> requisitionRepository,
            IGenericRepository<RequisitionItems> requisitionItemRepository,
            IGenericRepository<ReqItemApprovalHistory> approvalHistoryRepository,
            IGenericRepository<ReqApprovalStepApprovers> approvalStepRepository,
            IGenericRepository<ReqApprovalSettings> approvalSettingsRepository,
            IGenericRepository<Alerts> alertsRepository,
            IGenericRepository<AlertForEmployee> alertForEmployeeRepository,
            IUserInfoService userInfoService)
        {
            _requisitionRepository = requisitionRepository;
            _requisitionItemRepository = requisitionItemRepository;
            _approvalHistoryRepository = approvalHistoryRepository;
            _approvalStepRepository = approvalStepRepository;
            _approvalSettingsRepository = approvalSettingsRepository;
            _alertsRepository = alertsRepository;
            _alertForEmployeeRepository = alertForEmployeeRepository;
            _userInfoService = userInfoService;
        }

        public async Task<PaginatedResultCommon<RequisitionApprovalItemViewModel>> GetPendingApprovalsAsync(
    int? empId, int page, int pageSize, string search, string sortColumn,
    string sortDirection, int? productTypeId, string? fromDate, string? toDate)
        {
            try
            {
                // Get current approval step for this employee
                var approverSteps = await _approvalStepRepository.AllActive()
                    .Where(a => a.ApproverID == empId)
                    .Select(a => a.Step)
                    .ToListAsync();

                if (!approverSteps.Any())
                    return new PaginatedResultCommon<RequisitionApprovalItemViewModel>(
                        new List<RequisitionApprovalItemViewModel>(), 0);

                var query = _requisitionRepository.AllActive()
                    .Include(r => r.RequisitionByNavigation)
                    .Include(r => r.RequisitionItems)
                        .ThenInclude(ri => ri.Product)
                            .ThenInclude(p => p.ProductType)
                    .Where(r => !r.IsFinalApproved.HasValue || r.IsFinalApproved == false)
                    .Where(r => !r.IsDeclined.HasValue || r.IsDeclined == false)
                    .AsQueryable();

                // Filter by approver's step - show only requisitions at their step
                query = query.Where(r =>
                    approverSteps.Contains((r.ApprovalStep ?? 0) + 1) &&
                    r.ApprovalTypeID != null);

                // Search
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(r =>
                        r.RequisitionCode.Contains(search) ||
                        r.RequisitionByNavigation.FirstName.Contains(search) ||
                        r.RequisitionByNavigation.LastName.Contains(search));
                }

                // Product type filter
                if (productTypeId.HasValue)
                {
                    query = query.Where(r => r.RequisitionItems.Any(ri =>
                        ri.Product.ProductTypeID == productTypeId.Value));
                }

                // Date filter
                if (!string.IsNullOrWhiteSpace(fromDate) || !string.IsNullOrWhiteSpace(toDate))
                {
                    var dateFormats = new[] { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };

                    if (DateTime.TryParseExact(fromDate, dateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsedFrom))
                    {
                        query = query.Where(r => r.CreatedAt >= parsedFrom);
                    }

                    if (DateTime.TryParseExact(toDate, dateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsedTo))
                    {
                        var toDateEnd = parsedTo.AddDays(1).AddTicks(-1);
                        query = query.Where(r => r.CreatedAt <= toDateEnd);
                    }
                }

                // Sorting
                query = sortColumn switch
                {
                    "RequisitionId" => sortDirection == "desc"
                        ? query.OrderByDescending(r => r.RequisitionID)
                        : query.OrderBy(r => r.RequisitionID),
                    "RequisitionDate" => sortDirection == "desc"
                        ? query.OrderByDescending(r => r.CreatedAt)
                        : query.OrderBy(r => r.CreatedAt),
                    "RequisitionBy" => sortDirection == "desc"
                        ? query.OrderByDescending(r => r.RequisitionByNavigation.FirstName)
                        : query.OrderBy(r => r.RequisitionByNavigation.FirstName),
                    _ => query.OrderBy(r => r.RequisitionID)
                };

                var totalRecords = await query.CountAsync();

                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RequisitionApprovalItemViewModel
                    {
                        RequisitionId = r.RequisitionID,
                        RequisitionCode = r.RequisitionCode,
                        RequisitionDate = r.CreatedAt ?? DateTime.UtcNow,
                        RequisitionBy = r.RequisitionByNavigation.FirstName + " " + r.RequisitionByNavigation.LastName,
                        TotalItems = r.RequisitionItems.Count,
                        Priority = ((Priority)(r.Priority ?? (int)Priority.Normal)).ToString(),
                        CurrentStep = r.ApprovalStep ?? 0,
                        Status = "Pending",
                        CanEdit = false
                    })
                    .ToListAsync();

                return new PaginatedResultCommon<RequisitionApprovalItemViewModel>(items, totalRecords);
            }
            catch (Exception)
            {
                throw;
            }
        }

       

        public async Task<PaginatedResultCommon<RequisitionApprovalItemViewModel>> GetApprovedHistoryAsync(
            int? empId, int page, int pageSize, string search, string sortColumn,
            string sortDirection, int? productTypeId, string? fromDate, string? toDate)
        {
            // Get requisitions this employee has already approved
            var approvedReqIds = await _approvalHistoryRepository.AllActive()
                .Where(h => h.ApprovalPersonID == empId)
                .Select(h => h.RequisitionItem.RequisitionID)
                .Distinct()
                .ToListAsync();

            var query = _requisitionRepository.AllActive()
                .Include(r => r.RequisitionByNavigation)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductType)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.ReqItemApprovalHistory)
                .Where(r => approvedReqIds.Contains(r.RequisitionID))
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.RequisitionCode.Contains(search) ||
                    r.RequisitionByNavigation.FirstName.Contains(search) ||
                    r.RequisitionByNavigation.LastName.Contains(search));
            }

            // Product type filter
            if (productTypeId.HasValue)
            {
                query = query.Where(r => r.RequisitionItems.Any(ri =>
                    ri.Product.ProductTypeID == productTypeId.Value));
            }

            // Date filter
            if (!string.IsNullOrWhiteSpace(fromDate) || !string.IsNullOrWhiteSpace(toDate))
            {
                var dateFormats = new[] { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };

                if (DateTime.TryParseExact(fromDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedFrom))
                {
                    query = query.Where(r => r.CreatedAt >= parsedFrom);
                }

                if (DateTime.TryParseExact(toDate, dateFormats, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedTo))
                {
                    var toDateEnd = parsedTo.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.CreatedAt <= toDateEnd);
                }
            }

            // Sorting
            query = sortColumn switch
            {
                "RequisitionId" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.RequisitionID)
                    : query.OrderBy(r => r.RequisitionID),
                "RequisitionDate" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.CreatedAt)
                    : query.OrderBy(r => r.CreatedAt),
                "RequisitionBy" => sortDirection == "desc"
                    ? query.OrderByDescending(r => r.RequisitionByNavigation.FirstName)
                    : query.OrderBy(r => r.RequisitionByNavigation.FirstName),
                _ => query.OrderByDescending(r => r.RequisitionID)
            };

            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new RequisitionApprovalItemViewModel
                {
                    RequisitionId = r.RequisitionID,
                    RequisitionCode = r.RequisitionCode,
                    RequisitionDate = r.CreatedAt ?? DateTime.UtcNow,
                    RequisitionBy = r.RequisitionByNavigation.FirstName + " " + r.RequisitionByNavigation.LastName,
                    TotalItems = r.RequisitionItems.Count,
                    Priority = ((Priority)(r.Priority ?? (int)Priority.Normal)).ToString(),
                    CurrentStep = r.ApprovalStep ?? 0,
                    Status = r.IsFinalApproved == true ? "Fully Approved" :
                             r.IsDeclined == true ? "Declined" : "Partially Approved",
                    ApprovedAt = r.RequisitionItems
                        .SelectMany(ri => ri.ReqItemApprovalHistory)
                        .Where(h => h.ApprovalPersonID == empId)
                        .Select(h => h.ApprovedAt)
                        .FirstOrDefault(),
                    CanEdit = !(r.IsFinalApproved == true || r.IsDeclined == true) &&
                              !r.RequisitionItems.Any(ri => ri.ReqItemApprovalHistory
                                  .Any(h => h.ApprovalStep > (r.ApprovalStep ?? 0)))
                })
                .ToListAsync();

            return new PaginatedResultCommon<RequisitionApprovalItemViewModel>(items, totalRecords);
        }

        public async Task<RequisitionDetailsViewModel> GetRequisitionDetailsAsync(int requisitionId, int? empId)
        {
            var requisition = await _requisitionRepository.AllActive()
                .Include(r => r.Organization)
                .Include(r => r.OrganizationBranch)
                .Include(r => r.RequisitionByNavigation)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductType)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.UnitType)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.Product)
                        .ThenInclude(p => p.ProductBrand)
                .Include(r => r.RequisitionItems)
                    .ThenInclude(ri => ri.ReqItemApprovalHistory)
                        .ThenInclude(h => h.ApprovalPerson)
                .FirstOrDefaultAsync(r => r.RequisitionID == requisitionId);

            if (requisition == null)
                return null;

            // Determine approver's step
            var approverStep = await _approvalStepRepository.AllActive()
                .Where(a => a.ApproverID == empId &&
                       a.ReqApprovalSetting.ApprovalTypeID == requisition.ApprovalTypeID)
                .Select(a => a.Step)
                .FirstOrDefaultAsync();

            var isFirstApprover = approverStep == 1;
            var canApprove = approverStep == (requisition.ApprovalStep ?? 0) + 1 &&
                           !(requisition.IsFinalApproved == true || requisition.IsDeclined == true);

            // Check if can edit (only if no next approver has acted yet)
            var hasNextApproval = requisition.RequisitionItems
                .Any(ri => ri.ReqItemApprovalHistory
                    .Any(h => h.ApprovalStep > (requisition.ApprovalStep ?? 0)));

            var canEdit = !hasNextApproval &&
                         !(requisition.IsFinalApproved == true || requisition.IsDeclined == true) &&
                         requisition.RequisitionItems.Any(ri => ri.ReqItemApprovalHistory
                             .Any(h => h.ApprovalPersonID == empId));

            // Get approval history
            var history = requisition.RequisitionItems
                .SelectMany(ri => ri.ReqItemApprovalHistory)
                .GroupBy(h => new { h.ApprovalStep, h.ApprovalPersonID })
                .Select(g => new ApprovalHistoryViewModel
                {
                    Step = g.Key.ApprovalStep ?? 0,
                    ApproverName = g.First().ApprovalPerson.FirstName + " " + g.First().ApprovalPerson.LastName,
                    Status = g.First().StatusID == null ? "Declined" : "Approved",
                    ApprovedAt = g.First().ApprovedAt ?? g.First().DeclineAt,
                    Note = g.First().ApprovalPersonNote
                })
                .OrderBy(h => h.Step)
                .ToList();

            return new RequisitionDetailsViewModel
            {
                RequisitionId = requisition.RequisitionID,
                RequisitionCode = requisition.RequisitionCode,
                RequisitionDate = requisition.CreatedAt ?? DateTime.UtcNow,
                RequisitionBy = requisition.RequisitionByNavigation.FirstName + " " +
                               requisition.RequisitionByNavigation.LastName,
                Organization = requisition.Organization?.OrganizationName ?? "N/A",
                Branch = requisition.OrganizationBranch?.OrganizationBranchName ?? "N/A",
                Priority = ((Priority)(requisition.Priority ?? (int)Priority.Normal)).ToString(),
                RequisitionNote = requisition.RequisitionNote ?? "",
                CurrentStep = requisition.ApprovalStep ?? 0,
                ApproverStep = approverStep,
                IsFirstApprover = isFirstApprover,
                CanApprove = canApprove,
                CanEdit = canEdit,
                Items = requisition.RequisitionItems.Select(ri => new RequisitionItemDetailViewModel
                {
                    ItemId = ri.RequisitionItemID,
                    ProductType = ri.Product?.ProductType?.ProductTypeName ?? "N/A",
                    ProductName = ri.Product?.ProductName ?? "N/A",
                    Unit = ri.Product?.UnitType?.UnitTypeName ?? "N/A",
                    Brand = ri.Product?.ProductBrand?.ProductBrandName ?? "N/A",
                    RequestedQuantity = ri.RequisitionQuantity ?? 0,
                    ApprovedQuantity = ri.ApprovedQuantity,
                    IsApproved = ri.ReqItemApprovalHistory.Any(h => h.ApprovalPersonID == empId)
                }).ToList(),
                ApprovalHistory = history
            };
        }

        public async Task<CommonReturnViewModel> ApproveRequisitionAsync(
            ApproveRequisitionViewModel model, int? empId, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _requisitionRepository.BeginTransactionAsync();

            try
            {
                var requisition = await _requisitionRepository.AllActive()
                    .Include(r => r.RequisitionItems)
                    .Include(r => r.ApprovalType)
                    .FirstOrDefaultAsync(r => r.RequisitionID == model.RequisitionId);

                if (requisition == null)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Requisition not found.";
                    return response;
                }

                // Get approver's step
                var approverStep = await _approvalStepRepository.AllActive()
                    .Include(a => a.ReqApprovalSetting)
                    .Where(a => a.ApproverID == empId &&
                           a.ReqApprovalSetting.ApprovalTypeID == requisition.ApprovalTypeID &&
                           a.ReqApprovalSetting.StartDate <= DateTime.UtcNow &&
                           a.ReqApprovalSetting.EndDate >= DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (approverStep == null || approverStep.Step != (requisition.ApprovalStep ?? 0) + 1)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "You are not authorized to approve at this step.";
                    return response;
                }

                var isFirstApprover = approverStep.Step == 1;

                

                // Create approval history for each item
                foreach (var item in model.Items)
                {
                    var reqItem = requisition.RequisitionItems
                        .FirstOrDefault(ri => ri.RequisitionItemID == item.ItemId);

                    if (reqItem == null) continue;

                    // First approver sets approved quantity
                    if (isFirstApprover)
                    {
                        if (item.ApprovedQuantity > reqItem.RequisitionQuantity)
                        {
                            await _requisitionRepository.RollbackTransactionAsync();
                            response.Success = false;
                            response.Message = "Approved quantity cannot exceed requested quantity.";
                            return response;
                        }
                        reqItem.ApprovedQuantity = item.ApprovedQuantity;
                        await _requisitionItemRepository.UpdateAsync(reqItem);
                    }

                    var history = new ReqItemApprovalHistory
                    {
                        RequisitionItemID = item.ItemId,
                        ApprovalPersonID = empId,
                        ApprovalStep = approverStep.Step,
                        ApprovedAt = DateTime.UtcNow,
                        ApprovalPersonNote = model.ApproverNote,
                        StatusID = 1, // Approved status
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = baseView?.CreatedBy,
                        LIP = baseView?.LIP,
                        LMAC = baseView?.LMAC
                    };

                    await _approvalHistoryRepository.AddAsync(history);
                    await _userInfoService.ActionLogAsync("requisition approval", ActionName.DataAdd,
                        null, history, history.ReqItemApprovalHistoryID, baseView);
                }

                // Update requisition approval step
                requisition.ApprovalStep = approverStep.Step;

                // Check if this is the final approval step
                var maxStep = await _approvalStepRepository.AllActive()
                    .Where(a => a.ReqApprovalSetting.ApprovalTypeID == requisition.ApprovalTypeID &&
                           a.ReqApprovalSetting.StartDate <= DateTime.UtcNow &&
                           a.ReqApprovalSetting.EndDate >= DateTime.UtcNow)
                    .MaxAsync(a => a.Step);

                if (approverStep.Step >= maxStep)
                {
                    requisition.IsFinalApproved = true;
                }

                requisition.UpdatedAt = DateTime.UtcNow;
                requisition.UpdatedBy = baseView?.UpdatedBy;
                await _requisitionRepository.UpdateAsync(requisition);

                // Create alert for next approver or requester
                if (requisition.IsFinalApproved == true)
                {
                    // Alert requester
                    var alert = new Alerts
                    {
                        AlertTitle = "Requisition Approved",
                        AlertNote = $"Your requisition {requisition.RequisitionCode} has been fully approved.",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = baseView?.CreatedBy,
                        LIP = baseView?.LIP,
                        LMAC = baseView?.LMAC
                    };
                    await _alertsRepository.AddAsync(alert);

                    var empAlert = new AlertForEmployee
                    {
                        AlertID = alert.AlertID,
                        EmployeeID = requisition.RequisitionBy,
                        IsChecked = false,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = baseView?.CreatedBy,
                        LIP = baseView?.LIP,
                        LMAC = baseView?.LMAC
                    };
                    await _alertForEmployeeRepository.AddAsync(empAlert);
                }
                else
                {
                    // Alert next approver
                    var nextApprover = await _approvalStepRepository.AllActive()
                        .Where(a => a.ReqApprovalSetting.ApprovalTypeID == requisition.ApprovalTypeID &&
                               a.ReqApprovalSetting.StartDate <= DateTime.UtcNow &&
                               a.ReqApprovalSetting.EndDate >= DateTime.UtcNow &&
                               a.Step == approverStep.Step + 1)
                        .Select(a => a.ApproverID)
                        .FirstOrDefaultAsync();

                    if (nextApprover > 0)
                    {
                        var alert = new Alerts
                        {
                            AlertTitle = "Requisition Approval",
                            AlertNote = $"Requisition {requisition.RequisitionCode} is waiting for your approval.",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = baseView?.CreatedBy,
                            LIP = baseView?.LIP,
                            LMAC = baseView?.LMAC
                        };
                        await _alertsRepository.AddAsync(alert);

                        var empAlert = new AlertForEmployee
                        {
                            AlertID = alert.AlertID,
                            EmployeeID = nextApprover,
                            IsChecked = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = baseView?.CreatedBy,
                            LIP = baseView?.LIP,
                            LMAC = baseView?.LMAC
                        };
                        await _alertForEmployeeRepository.AddAsync(empAlert);
                    }
                }

                await _requisitionRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Requisition approved successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _requisitionRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error approving requisition: " + ex.Message;
                return response;
            }
        }

        public async Task<CommonReturnViewModel> DeclineRequisitionAsync(
            DeclineRequisitionViewModel model, int? empId, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _requisitionRepository.BeginTransactionAsync();

            try
            {
                var requisition = await _requisitionRepository.AllActive()
                    .Include(r => r.RequisitionItems)
                    .FirstOrDefaultAsync(r => r.RequisitionID == model.RequisitionId);

                if (requisition == null)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Requisition not found.";
                    return response;
                }

                // Get approver's step
                var approverStep = await _approvalStepRepository.AllActive()
                    .Include(a => a.ReqApprovalSetting)
                    .Where(a => a.ApproverID == empId &&
                           a.ReqApprovalSetting.ApprovalTypeID == requisition.ApprovalTypeID &&
                           a.ReqApprovalSetting.StartDate <= DateTime.UtcNow &&
                           a.ReqApprovalSetting.EndDate >= DateTime.UtcNow)
                    .FirstOrDefaultAsync();

                if (approverStep == null)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "You are not authorized to decline this requisition.";
                    return response;
                }

                // Create decline history for each item
                foreach (var item in requisition.RequisitionItems)
                {
                    var history = new ReqItemApprovalHistory
                    {
                        RequisitionItemID = item.RequisitionItemID,
                        ApprovalPersonID = empId,
                        ApprovalStep = approverStep.Step,
                        DeclineAt = DateTime.UtcNow,
                        DeclineById = empId,
                        ApprovalPersonNote = model.DeclineNote,
                        StatusID = null, // Declined
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = baseView?.CreatedBy,
                        LIP = baseView?.LIP,
                        LMAC = baseView?.LMAC
                    };

                    await _approvalHistoryRepository.AddAsync(history);
                }

                requisition.IsDeclined = true;
                requisition.UpdatedAt = DateTime.UtcNow;
                requisition.UpdatedBy = baseView?.UpdatedBy;
                await _requisitionRepository.UpdateAsync(requisition);

                // Alert requester
                var alert = new Alerts
                {
                    AlertTitle = "Requisition Declined",
                    AlertNote = $"Your requisition {requisition.RequisitionCode} has been declined. Reason: {model.DeclineNote}",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = baseView?.CreatedBy,
                    LIP = baseView?.LIP,
                    LMAC = baseView?.LMAC
                };
                await _alertsRepository.AddAsync(alert);

                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = requisition.RequisitionBy,
                    IsChecked = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = baseView?.CreatedBy,
                    LIP = baseView?.LIP,
                    LMAC = baseView?.LMAC
                };
                await _alertForEmployeeRepository.AddAsync(empAlert);

                await _requisitionRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Requisition declined successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _requisitionRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error declining requisition: " + ex.Message;
                return response;
            }
        }

        public async Task<CommonReturnViewModel> EditApprovedRequisitionAsync(
            EditApprovedRequisitionViewModel model, int? empId, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _requisitionRepository.BeginTransactionAsync();

            try
            {
                var requisition = await _requisitionRepository.AllActive()
                    .Include(r => r.RequisitionItems)
                        .ThenInclude(ri => ri.ReqItemApprovalHistory)
                    .FirstOrDefaultAsync(r => r.RequisitionID == model.RequisitionId);

                if (requisition == null)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Requisition not found.";
                    return response;
                }

                // Check if next approver has acted
                var hasNextApproval = requisition.RequisitionItems
                    .Any(ri => ri.ReqItemApprovalHistory
                        .Any(h => h.ApprovalStep > (requisition.ApprovalStep ?? 0)));

                if (hasNextApproval)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Cannot edit - next approver has already acted.";
                    return response;
                }

                // Get approver's step to verify they approved this
                var myApprovalStep = requisition.RequisitionItems
                    .SelectMany(ri => ri.ReqItemApprovalHistory)
                    .Where(h => h.ApprovalPersonID == empId)
                    .Select(h => h.ApprovalStep)
                    .FirstOrDefault();

                if (myApprovalStep == null)
                {
                    await _requisitionRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "You have not approved this requisition.";
                    return response;
                }

                var isFirstApprover = myApprovalStep == 1;

                // Update approval histories
                foreach (var item in model.Items)
                {
                    var reqItem = requisition.RequisitionItems
                        .FirstOrDefault(ri => ri.RequisitionItemID == item.ItemId);

                    if (reqItem == null) continue;

                    // Update approved quantity if first approver
                    if (isFirstApprover)
                    {
                        if (item.ApprovedQuantity > reqItem.RequisitionQuantity)
                        {
                            await _requisitionRepository.RollbackTransactionAsync();
                            response.Success = false;
                            response.Message = "Approved quantity cannot exceed requested quantity.";
                            return response;
                        }
                        reqItem.ApprovedQuantity = item.ApprovedQuantity;
                        await _requisitionItemRepository.UpdateAsync(reqItem);
                    }

                    // Update existing approval history
                    var existingHistory = reqItem.ReqItemApprovalHistory
                        .FirstOrDefault(h => h.ApprovalPersonID == empId);

                    if (existingHistory != null)
                    {
                        existingHistory.ApprovalPersonNote = model.ApproverNote;
                        existingHistory.UpdatedAt = DateTime.UtcNow;
                        existingHistory.UpdatedBy = baseView?.UpdatedBy;
                        existingHistory.LMAC = baseView?.LMAC;
                        existingHistory.LIP = baseView?.LIP;

                        await _approvalHistoryRepository.UpdateAsync(existingHistory);
                    }
                }

                requisition.UpdatedAt = DateTime.UtcNow;
                requisition.UpdatedBy = baseView?.UpdatedBy;
                await _requisitionRepository.UpdateAsync(requisition);

                await _requisitionRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Approval updated successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _requisitionRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error updating approval: " + ex.Message;
                return response;
            }
        }

        public Task<byte[]> GeneratePDF(int orgId, int empId, string? fromDate, string? toDate, bool approved)
        {
            // TODO: Implement PDF generation logic
            throw new NotImplementedException("PDF generation not yet implemented");
        }

        public Task<byte[]> GenerateExcel(int orgId, int empId, string? fromDate, string? toDate, bool approved)
        {
            // TODO: Implement Excel generation logic
            throw new NotImplementedException("Excel generation not yet implemented");
        }
    }
}
