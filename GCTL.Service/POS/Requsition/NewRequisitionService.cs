using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Requsition.AddRequisition;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;

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

        public Task<CommonReturnViewModel> DeleteRequisitionAsync(int id, BaseViewModel? baseView, int? empID)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GeneratePDF(int orgid, int id, string? FromDate, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public Task<byte[]> GenerateXL(int orgid, int id, string? FromDate, string? ToDate)
        {
            throw new NotImplementedException();
        }

        public Task<EditRequisitionViewModel> GetRequisitionByIdAsync(int id, int? empID)
        {
            throw new NotImplementedException();
        }

        public async Task<PaginatedResultCommon<RequisitionItemViewModel>> GetRequisitionListAsync(int page, int pageSize, string search, string sortColumn, string sortDirection, int? projectId, int? productTypeId, int? empID, string? FromDate, string? ToDate)
        {
            try
            {
                var query = _requisitionItemRepository.AllActive()
                .Include(e => e.Requisition)
                .Include(t => t.Product).ThenInclude(pt => pt.ProductType)
                .Include(u => u.Product).ThenInclude(ut => ut.UnitType)
                .Where(e => e.CreatedBy == empID)
                .AsQueryable();

               
                //if (projectId.HasValue)
                //    query = query.Where(r => r.Requisition.Project.ProjectID == projectId.Value);

                if (productTypeId.HasValue)
                    query = query.Where(r => r.Product.ProductTypeID == productTypeId.Value);

                // Searching
                if (!string.IsNullOrEmpty(search))
                    query = query.Where(r => r.Product.ProductName.Contains(search) || r.Requisition.RequisitionByNavigation.FirstName.Contains(search));

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

                    case "productName":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.Product.ProductName)
                            : query.OrderBy(r => r.Product.ProductName);
                        break;

                    case "ProductType":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.Product.ProductType.ProductTypeName)
                            : query.OrderBy(r => r.Product.ProductType.ProductTypeName);
                        break;

                    case "RequisitionDate":
                        query = sortDirection == "desc"
                            ? query.OrderByDescending(r => r.CreatedAt)
                            : query.OrderBy(r => r.CreatedAt);
                        break;

                    default:
                        query = query.OrderBy(r => r.RequisitionItemID); // fallback
                        break;
                }


                // Total count
                var totalRecords = await query.CountAsync();

                // Paging
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(r => new RequisitionItemViewModel
                    {
                        Id = r.RequisitionItemID,
                        RequisitionId = r.RequisitionID,
                        ProductName = r.Product.ProductName,
                        ProductType = r.Product.ProductType.ProductTypeName,
                        RequisitionDate = r.CreatedAt,
                        Unit = r.Product.UnitType.UnitTypeName,

                        RequisitionQuantity = r.RequisitionQuantity,
                        ApproveQuantity = r.ApprovedQuantity,
                        Status = r.Requisition.IsFinalApproved == true ? "Approve" : r.Requisition.IsDeclined == true ? "Decline" : "Pending"
                    })
                    .ToListAsync();

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

            if (emp == null)
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

        public Task<CommonReturnViewModel> UpdateRequisitionAsync(EditRequisitionViewModel model, int? empID)
        {
            throw new NotImplementedException();
        }
    }
}
