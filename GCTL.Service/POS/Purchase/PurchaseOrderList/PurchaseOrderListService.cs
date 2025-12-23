using System;
using System.Linq;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Purchase.PurchaseOrderList;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GCTL.Service.POS.Purchase.PurchaseOrderList
{
    public class PurchaseOrderListService : IPurchaseOrderList
    {
        private readonly IGenericRepository<PurchasOrders> _purchaseOrdersRepository;
        private readonly IGenericRepository<PurchasOrderItemVersions> _purchaseOrderItemsRepository;
        private readonly IGenericRepository<PurchasOrderVersions> _purchaseOrderVersionRepository;

        public PurchaseOrderListService(
            IGenericRepository<PurchasOrders> purchaseOrdersRepository,
            IGenericRepository<PurchasOrderItemVersions> purchaseOrderItemsRepository,
            IGenericRepository<PurchasOrderVersions> purchaseOrderVersionRepository)
        {
            _purchaseOrdersRepository = purchaseOrdersRepository;
            _purchaseOrderItemsRepository = purchaseOrderItemsRepository;
            _purchaseOrderVersionRepository = purchaseOrderVersionRepository;
        }

        public async Task<PaginatedResultData<PurchaseOrderListDto>> GetPurchaseOrdersWithPagination(
            int page,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection)
        {
            try
            {
                //var query = _purchaseOrderVersionRepository.AllActive()
                //    .Include(e => e.PurchasOrder)
                //    .Include(q => q.Supplier)
                //    .Include(q => q.PurchasOrderItemVersions)
                //    .Include(q => q.CreatedByNavigation)
                //    .Include(q => q.Status);
                IQueryable<PurchasOrderVersions> query = _purchaseOrderVersionRepository.AllActive()
                    .Include(e => e.PurchasOrder)
                    .Include(q => q.Supplier)
                    .Include(q => q.PurchasOrderItemVersions)
                    .Include(q => q.CreatedByNavigation)
                   // .Include(q => q.Status)
                    .Where(e=>e.IsDraft == true || e.IsFinal == true);

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(q =>
                        q.PurchasOrder.POID.ToLower().Contains(searchTerm) ||
                        (q.Supplier != null && q.Supplier.FullName.ToLower().Contains(searchTerm)) ||
                        (q.CreatedByNavigation != null && q.CreatedByNavigation.FirstName.ToLower().Contains(searchTerm))
                    );
                }

                // Apply sorting
                query = sortColumn switch
                {
                    "PurchaseOrderID" => sortDirection == "asc" ? query.OrderBy(q => q.PurchasOrderID) : query.OrderByDescending(q => q.PurchasOrderID),
                    "POID" => sortDirection == "asc" ? query.OrderBy(q => q.PurchasOrder.POID) : query.OrderByDescending(q => q.PurchasOrder.POID),
                    "SupplierName" => sortDirection == "asc" ? query.OrderBy(q => q.Supplier.FullName) : query.OrderByDescending(q => q.Supplier.FullName),
                    "CreatedBy" => sortDirection == "asc" ? query.OrderBy(q => q.CreatedByNavigation.FirstName) : query.OrderByDescending(q => q.CreatedByNavigation.FirstName),
                    "PurchaseDate" => sortDirection == "asc" ? query.OrderBy(q => q.PurchaseDate) : query.OrderByDescending(q => q.PurchaseDate),
                    "GrandTotalAmount" => sortDirection == "asc" ? query.OrderBy(q => q.GrandTotalAmount) : query.OrderByDescending(q => q.GrandTotalAmount),
                    _ => sortDirection == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
                };

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(q => new PurchaseOrderListDto
                    {
                        PurchaseOrderID = q.PurchasOrderVersionID,
                        POID = q.PurchasOrder.POID ?? "",
                        SupplierName = q.Supplier != null ? q.Supplier.FullName : "",
                        PurchaseDate = q.PurchaseDate,
                        GrandTotalAmount = q.GrandTotalAmount,
                        PaidAmount = q.PaidAmount,
                        DueAmount = q.DueAmount,
                        TotalItems = q.PurchasOrderItemVersions.Count,
                        CreatedBy = q.CreatedByNavigation != null
                            ? q.CreatedByNavigation.FirstName + " " + q.CreatedByNavigation.LastName
                            : "",
                       // Status = q.Status != null ? q.Status.StatusName : "Draft",
                        Note = q.Note ?? ""
                    })
                    .ToListAsync();

                return new PaginatedResultData<PurchaseOrderListDto>
                {
                    Data = data,
                    TotalRecords = totalRecords,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                };
            }
            catch (Exception)
            {
                throw;
            }
        }
    
    
    
    
    }
}
