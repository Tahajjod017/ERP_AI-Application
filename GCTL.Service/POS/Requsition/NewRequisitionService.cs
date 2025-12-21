using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Enums;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using static Dapper.SqlMapper;

namespace GCTL.Service.POS.Requsition
{
    public class NewRequisitionService : INewRequisitionService
    {
        private readonly IGenericRepository<Requisitions> _requisitionRepository;
        private readonly IGenericRepository<ReqApprovalSettings> _requisitionApprovalSettingRepository;
        private readonly IGenericRepository<ReqApprovalStepApprovers> _requisitionApprovalStepRepository;
        private readonly IGenericRepository<RequisitionItems> _requisitionItemRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ApprovalSettings> _approvalSettingRepository;
        private readonly IGenericRepository<ReqApprovalStepApprovers> _approvalAssignRepository;

        private readonly IGenericRepository<ReqItemApprovalHistory> _reqItemHistoryRepository;
        private readonly IUserInfoService _userInfoService;

        private readonly IGenericRepository<Alerts> _alertsRepository;
        private readonly IGenericRepository<AlertForEmployee> _alertForEmployeeRepository;
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IGenericRepository<ApprovalTypes> _approvalTypeRepository;

        public NewRequisitionService(IGenericRepository<Requisitions> requisitionRepository, IGenericRepository<RequisitionItems> requisitionItemRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<Products> productRepository, IGenericRepository<ApprovalSettings> approvalSettingRepository, IGenericRepository<ReqApprovalStepApprovers> approvalAssignRepository, IGenericRepository<ReqItemApprovalHistory> reqItemHistoryRepository, IUserInfoService userInfoService, IGenericRepository<Alerts> alertsRepository, IGenericRepository<AlertForEmployee> alertForEmployeeRepository, IGenericRepository<Organization> organizationRepository, IGenericRepository<ApprovalTypes> approvalTypeRepository, IGenericRepository<ReqApprovalSettings> requisitionApprovalSettingRepository, IGenericRepository<ReqApprovalStepApprovers> requisitionApprovalStepRepository)
        {
            _requisitionRepository = requisitionRepository;
            _requisitionItemRepository = requisitionItemRepository;
            _statusRepository = statusRepository;
            _productRepository = productRepository;
            _approvalSettingRepository = approvalSettingRepository;
            _approvalAssignRepository = approvalAssignRepository;
            _reqItemHistoryRepository = reqItemHistoryRepository;
            _userInfoService = userInfoService;
            _alertsRepository = alertsRepository;
            _alertForEmployeeRepository = alertForEmployeeRepository;
            _organizationRepository = organizationRepository;
            _approvalTypeRepository = approvalTypeRepository;
            _requisitionApprovalSettingRepository = requisitionApprovalSettingRepository;
            _requisitionApprovalStepRepository = requisitionApprovalStepRepository;
        }

        public async Task<CommonReturnViewModel> DeleteRequisitionAsync(int id, BaseViewModel? baseView, int? empID)
        {
            var response = new CommonReturnViewModel();
            await _requisitionItemRepository.BeginTransactionAsync();

            try
            {
                var requisition = await _requisitionRepository.AllActive()
                    .Include(r => r.RequisitionItems)
                    .FirstOrDefaultAsync(r => r.RequisitionID == id);

                if (requisition == null)
                {
                    await _requisitionItemRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Requisition not found.";
                    return response;
                }

                if (requisition.IsFinalApproved == true || requisition.IsDeclined == true || (requisition.ApprovalStep > 0))
                {
                    await _requisitionItemRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Cannot modify approved requisition.";
                    return response;
                }

                var requisitionItem = await _requisitionItemRepository.AllActive()
                    .Include(r => r.Requisition)
                    .Where(r => r.RequisitionItemID == requisition.RequisitionID).ToListAsync();

                if (requisitionItem == null)
                {
                    response.Success = false;
                    response.Message = "Requisition not found.";
                    return response;
                }

                requisition.DeletedAt = DateTime.UtcNow;
                requisition.DeletedBy = baseView?.CreatedBy ?? null; // Implement this method

                await _requisitionRepository.UpdateAsync(requisition);

                requisitionItem.ForEach(item =>
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = baseView?.CreatedBy;
                });

                await _requisitionItemRepository.UpdateRangeAsync(requisitionItem);

                
               


                await _requisitionItemRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Requisition deleted successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _requisitionItemRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error deleting requisition: " + ex.Message;
                return response;
            }
        }

        public Task<byte[]> GeneratePDF(int orgid, int id, string? FromDate, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateXL(int orgid, int id, string? FromDate, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public async Task<EditRequisitionViewModel> GetRequisitionByIdAsync(int id, int? empID)
        {
            try
            {
                // Load the full Requisition with all related data
                var requisition = await _requisitionRepository.AllActive()
                    .Include(r => r.Organization)
                    .Include(r => r.OrganizationBranch)
                    .Include(r => r.RequisitionByNavigation) // Assuming this is the employee/supervisor
                    .Include(r => r.RequisitionItems)
                        .ThenInclude(ri => ri.Product)
                            .ThenInclude(p => p.ProductType)
                    .Include(r => r.RequisitionItems)
                        .ThenInclude(ri => ri.Product)
                            .ThenInclude(p => p.UnitType)
                    .Include(r => r.RequisitionItems)
                        .ThenInclude(ri => ri.Product)
                            .ThenInclude(p => p.ProductBrand)
                    .FirstOrDefaultAsync(r => r.RequisitionID == id);

                if (requisition == null)
                    return null;

                // Optional: Check if there are approval histories for any item (to determine overall status)
                var hasAnyApproval = await _reqItemHistoryRepository.AllActive()
                    .AnyAsync(h => h.RequisitionItem.RequisitionID == id && h.ApprovalPersonID != empID);

                var status = hasAnyApproval ? "partially_approved" : "pending";
                // Or you can make it more granular: "approved" only if ALL items are approved

                return new EditRequisitionViewModel
                {
                    ReqId = requisition.RequisitionID,
                    OrganizationId = requisition.OrganizationBranch.OrganizationID,
                    OrganizationBranchId = requisition.OrganizationBranchID ?? 0,
                    RequesterId = requisition.RequisitionBy ?? requisition.RequisitionBy ?? 0, // Adjust based on your model
                    Priority = requisition.Priority , // or whatever default
                    RequisitionNote = requisition.RequisitionNote ?? "",

                    // Map all product items
                    Products = requisition.RequisitionItems.Select((item, index) => new EditRequisitionProductViewModel
                    {
                        Index = index, // Optional, for frontend use
                        Id = item.RequisitionItemID, // Important: to update existing items
                        ProductTypeId = item.Product?.ProductTypeID ?? 0,
                        ProductId = item.ProductID,
                        Quantity = item.RequisitionQuantity,
                        Unit = item.Product?.UnitType?.UnitTypeName ?? "N/A",
                        Brand = item.Product?.ProductBrand?.ProductBrandName ?? "N/A"
                        // You can add more fields if needed
                    }).ToList(),

                    Status = status
                };


            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<PaginatedResultCommon<RequisitionItemViewModel>> GetRequisitionListAsync(int page, int pageSize, string search, string sortColumn, string sortDirection, int? projectId, int? productTypeId, int? empID, string? FromDate, string? ToDate)
        {
            try
            {
                var query = _requisitionRepository.AllActive()
                .Include(e => e.RequisitionItems).ThenInclude(t => t.Product).ThenInclude(pt => pt.ProductType)
                .Include(e => e.RequisitionItems).ThenInclude(t => t.Product).ThenInclude(pt => pt.UnitType)
                .Include(e=>e.RequisitionByNavigation)
               
                .Where(e => e.CreatedBy == empID)
                .AsQueryable();

               
                //if (projectId.HasValue)
                //    query = query.Where(r => r.Requisition.Project.ProjectID == projectId.Value);

                //if (productTypeId.HasValue)
                //    query = query.Where(r => r.Product.ProductTypeID == productTypeId.Value);

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(r => r.RequisitionItems.First().Product.ProductName.Contains(search) || r.RequisitionByNavigation.FirstName.Contains(search));

                // 🔍 Date Filter
                if (!string.IsNullOrWhiteSpace(FromDate) || !string.IsNullOrWhiteSpace(ToDate))
                {
                    DateTime? fromDate = null;
                    DateTime? toDate = null;

                    // Define all accepted formats
                    var dateFormats = new[] { "dd/MM/yy", "dd/MM/yyyy", "yyyy-MM-dd" };

                    if (DateTime.TryParseExact(FromDate, dateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsedFrom))
                    {
                        fromDate = parsedFrom;
                    }

                    if (DateTime.TryParseExact(ToDate, dateFormats, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var parsedTo))
                    {
                        // Include the full day (till 23:59:59.9999999)
                        toDate = parsedTo.AddDays(1).AddTicks(-1);
                    }

                    if (fromDate.HasValue)
                        query = query.Where(p => p.CreatedAt >= fromDate.Value);

                    if (toDate.HasValue)
                        query = query.Where(p => p.CreatedAt <= toDate.Value);
                }

                switch (sortColumn)
                {
                    case "RequisitionId":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.RequisitionID)
                            : query.OrderBy(r => r.RequisitionID);
                        break;

                    case "requitionby":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.RequisitionBy)
                            : query.OrderBy(r => r.RequisitionBy);
                        break;

                    case "requisitionItems":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.RequisitionItems.Count)
                            : query.OrderBy(r => r.RequisitionItems.Count);
                        break;

                    case "RequisitionDate":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.CreatedAt)
                            : query.OrderBy(r => r.CreatedAt);
                        break;

                    default:
                        query = query.OrderBy(r => r.RequisitionID); // fallback
                        break;
                }


                // Total count
                var totalRecords = await query.CountAsync();


             


                // Step 1: project raw values
                var itemsRaw = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new
                    {
                        Id = r.RequisitionID,
                        RequisitionId = r.RequisitionID,
                        RequitionCode = r.RequisitionCode,
                        RequisitionDate = r.CreatedAt,
                        Note = r.RequisitionNote,
                        RequisitionItems = r.RequisitionItems.Count(),
                        Priority = r.Priority,   // keep as enum/int here
                        ApprovalStep = r.ApprovalStep,
                        RequisitionBy = r.RequisitionByNavigation.FirstName + " " + r.RequisitionByNavigation.LastName,
                        Status = r.IsFinalApproved == true ? "Approve"
                                 : r.IsDeclined == true ? "Decline"
                                 : (r.ApprovalStep > 0 ? "OnProgress" : "Pending")
                    })
                    .ToListAsync();

                // Step 2: convert to your ViewModel with string Priority
                var items = itemsRaw.Select(r => new RequisitionItemViewModel
                {
                    Id = r.Id,
                    RequisitionId = r.RequisitionId,
                    RequitionCode = r.RequitionCode,
                    RequisitionDate = r.RequisitionDate,
                    Note = r.Note,
                    RequisitionItems = r.RequisitionItems,
                    RequisitionBy = r.RequisitionBy,
                    Priority = ((Priority)(r.Priority ?? (int)Priority.Normal)).ToString(),
                    ApprovalStep = r.ApprovalStep,
                    Status = r.Status
                }).ToList();



                return new PaginatedResultCommon<RequisitionItemViewModel>(items, totalRecords);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async Task<Dictionary<int, string>> GetProductNamesAsync(IEnumerable<int> productIds)
        {
            // Replace with actual DB/service call
            return await _productRepository.AllActive()
                .Where(p => productIds.Contains(p.ProductID))
                .ToDictionaryAsync(p => p.ProductID, p => p.ProductName);
        }

        public async Task<CommonReturnViewModel> SaveRequsitionAsync(CreateRequisitionViewModel model)
        {
            var response = new CommonReturnViewModel();
            response.Success = true;
            response.Message = "Requisition created successfully.";



            // Assuming you have a method or dictionary to get product names by ID
            Dictionary<int, string> productNameMap = await GetProductNamesAsync(model.Products.Select(p => p.ProductId).Distinct());

            var duplicateProductIds = model.Products
                .GroupBy(p => p.ProductId)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateProductIds.Any())
            {
                var duplicateNames = duplicateProductIds
                    .Select(id => productNameMap.ContainsKey(id) ? productNameMap[id] : $"ID {id}")
                    .ToList();

                response.Success = false;
                response.Message = $"Duplicate product(s) selected: {string.Join(", ", duplicateNames)}";
                return response;
            }



            await _requisitionRepository.BeginTransactionAsync();

            

            var ApprovalTypeName = "product" + "_" + model.OrganizationId + "_" + model.OrganizationBranchId;

            var type = _approvalTypeRepository.AllActive().FirstOrDefault(e=>e.ApprovalTypeName == ApprovalTypeName);

            if (type == null) 
            {
                response.Success = false;
                response.Message = "Approval Matrix not found";
                return response;
            }

            var emp = await _requisitionApprovalStepRepository.AllActive().Include(e => e.ReqApprovalSetting)
                         .Where(e => e.ReqApprovalSetting.ApprovalTypeID == type.ApprovalTypeID
                         && e.ReqApprovalSetting.StartDate <= DateTime.UtcNow
                         && e.ReqApprovalSetting.EndDate >= DateTime.UtcNow
                         && e.Step == 1)
                         .Select(e => e.ApproverID).FirstOrDefaultAsync();

            if (emp == null || emp == 0)
            {
                response.Success = false;
                response.Message = "Approver not found";
                return response;
            }


            try
            {
                var list = new List<RequisitionItems>();

                var requisition = new Requisitions
                {

                    RequisitionBy = model.RequesterId,
                    RequisitionDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    ApprovalTypeID = type.ApprovalTypeID,
                    ApprovalStep = 0,
                    IsDeclined = false,
                    IsFinalApproved = false,
                    OrganizationID = model.OrganizationID,
                    OrganizationBranchID = model.OrganizationBranchId,
                    Priority = model.Priority,
                    RequisitionNote = model.RequisitionNote,
                    RequisitionCode = await GetNextRequisitionCodeAsync(),

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,



                };
                await _requisitionRepository.AddAsync(requisition);
                await _userInfoService.ActionLogAsync("requisition", ActionName.DataAdd, null, requisition, requisition.RequisitionID, model);


                #region Alert

                var alert = new Alerts
                {
                    AlertTitle = "Requisition",
                    AlertNote = " New Requisition Added",
                    LMAC = model.LMAC,
                    LIP = model.LIP,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTime.Now,
                };

                await _alertsRepository.AddAsync(alert);

                #endregion

                foreach (var item in model.Products)
                {
                    var requisitionItem = new RequisitionItems
                    {
                        RequisitionID = requisition.RequisitionID,
                        ProductID = item.ProductId,
                        RequisitionQuantity = item.Quantity,
                        ApprovedQuantity = 0,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                       
                    };
                    list.Add(requisitionItem);

                    

                }

                await _requisitionItemRepository.AddRangeAsync(list);


                #region Alert

              
                    
                   

                var empAlert = new AlertForEmployee
                {
                    AlertID = alert.AlertID,
                    EmployeeID = emp,  // for alert Employee
                    IsChecked = false,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy,
                };
                await _alertForEmployeeRepository.AddAsync(empAlert);

                #endregion


                foreach (var reqItem in list)
                {
                    await _userInfoService.ActionLogAsync("requisition item", ActionName.DataAdd, null, reqItem, reqItem.RequisitionItemID, model);
                }

                await _requisitionRepository.CommitTransactionAsync();

                return response;
            }
            catch (Exception ex)
            {
                await _requisitionRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error creating requisition: " + ex.Message;
                return response;
            }
        }

        public async Task<CommonReturnViewModel> UpdateRequisitionAsync(EditRequisitionViewModel model, int? empID, BaseViewModel? baseView)
        {
            var response = new CommonReturnViewModel();
            await _requisitionItemRepository.BeginTransactionAsync();

            try
            {
                var requisition = await _requisitionRepository.AllActive()
                    .Include(r => r.RequisitionItems)
                    .FirstOrDefaultAsync(r => r.RequisitionID == model.ReqId);

                if (requisition == null)
                {
                    await _requisitionItemRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Requisition not found.";
                    return response;
                }

              

                if (requisition.IsFinalApproved == true || requisition.IsDeclined == true || (requisition.ApprovalStep > 0))
                {
                    await _requisitionItemRepository.RollbackTransactionAsync();
                    response.Success = false;
                    response.Message = "Cannot modify approved requisition.";
                    return response;
                }

                requisition.RequisitionNote = model.RequisitionNote;

                requisition.Priority  = model.Priority;
              
                requisition.UpdatedAt = DateTime.UtcNow;
                requisition.UpdatedBy = baseView?.UpdatedBy ?? 0;
                requisition.LMAC = baseView?.LMAC;
                requisition.LIP = baseView?.LIP;

                //await _requisitionItemRepository.UpdateAsync(requisitionItem);
                await _requisitionRepository.UpdateAsync(requisition);


                var items = await _requisitionItemRepository.AllActive().Where(e=>e.RequisitionID == requisition.RequisitionID).ToListAsync();
                await _requisitionItemRepository.DeleteRangeAsync(items);

                
                var newItems = model.Products.Select(item => new RequisitionItems
                {
                    RequisitionID = requisition.RequisitionID,
                    ProductID = item.ProductId,
                    RequisitionQuantity = item.Quantity,
                    ApprovedQuantity = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = baseView?.CreatedBy ?? 0,
                    LIP = baseView?.LIP,
                    LMAC = baseView?.LMAC,
                    
                }).ToList();

                // Add them all at once
                await _requisitionItemRepository.AddRangeAsync(newItems);




                await _requisitionItemRepository.CommitTransactionAsync();

                response.Success = true;
                response.Message = "Requisition updated successfully.";
                return response;
            }
            catch (Exception ex)
            {
                await _requisitionItemRepository.RollbackTransactionAsync();
                response.Success = false;
                response.Message = "Error updating requisition: " + ex.Message;
                return response;
            }
        }


        public async Task<string> GetNextRequisitionCodeAsync()
        {
            // Get the latest requisition code from DB
            var lastCode = await _requisitionRepository.AllActive()
                .OrderByDescending(r => r.RequisitionID)
                .Select(r => r.RequisitionCode)
                .FirstOrDefaultAsync();

            string yearPart = DateTime.Now.ToString("yy"); // e.g. "25"
            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastCode))
            {
                // Example format: REQ-25-000001
                var parts = lastCode.Split('-');
                if (parts.Length == 3 && parts[1] == yearPart)
                {
                    // Parse the numeric part
                    if (int.TryParse(parts[2], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }
            }

            // Format: REQ-YY-###### (6 digits padded)
            string newCode = $"REQ-{yearPart}-{nextNumber.ToString("D6")}";
            return newCode;
        }

    }
}
