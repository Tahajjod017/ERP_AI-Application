using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Sales.SalesOrderList;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Sales.SalesOrderList
{
    public class SalesOrderListService : ISalesOrderList
    {
        private readonly IGenericRepository<SalesOrders> _salesOrderRepository;
        private readonly IGenericRepository<SalesOrderVersionItems> _salesOrderItemRepository;
        private readonly IGenericRepository<SalesOrdersVersions> _salesOrderVersionRepository;

        public SalesOrderListService(IGenericRepository<SalesOrders> salesOrderRepository, IGenericRepository<SalesOrderVersionItems> salesOrderItemRepository, IGenericRepository<SalesOrdersVersions> salesOrderVersionRepository)
        {
            _salesOrderRepository = salesOrderRepository;
            _salesOrderItemRepository = salesOrderItemRepository;
            _salesOrderVersionRepository = salesOrderVersionRepository;
        }

        public async Task<SalesOrderListResultViewModel> GetSalesOrdersWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            var query = _salesOrderVersionRepository.AllActive()
                .Include(so => so.Customer)
                .Include(so => so.SalesOrders)
                .ThenInclude(so => so.PriceQuotationVersion).ThenInclude(e=>e.PriceQuotation)
                .Include(so => so.CreatedByNavigation)
                .Include(so => so.SalesOrderVersionItems)
                .Where(e=>e.IsDraft == true || e.IsFinal == true)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(so =>
                    so.SalesOrders.SalesOrderNumber.Contains(searchTerm) ||
                    so.Customer.FullName.Contains(searchTerm) ||
                    so.SalesOrders.PriceQuotationVersion != null && so.SalesOrders.PriceQuotationVersion.PriceQuotation.QuotationNumber.Contains(searchTerm) ||
                    so.Note != null && so.Note.Contains(searchTerm));
            }

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Sorting
            query = sortColumn switch
            {
                "SalesOrderID" => sortDirection == "asc"
                    ? query.OrderBy(so => so.SalesOrdersVersionID)
                    : query.OrderByDescending(so => so.SalesOrdersVersionID),
                "SalesOrderNumber" => sortDirection == "asc"
                    ? query.OrderBy(so => so.SalesOrders.SalesOrderNumber)
                    : query.OrderByDescending(so => so.SalesOrders.SalesOrderNumber),
                "CustomerName" => sortDirection == "asc"
                    ? query.OrderBy(so => so.Customer.FullName)
                    : query.OrderByDescending(so => so.Customer.FullName),
                "SalesOrderDate" => sortDirection == "asc"
                    ? query.OrderBy(so => so.SalesOrderDate)
                    : query.OrderByDescending(so => so.SalesOrderDate),
                "TotalItems" => sortDirection == "asc"
                    ? query.OrderBy(so => so.SalesOrderVersionItems.Count)
                    : query.OrderByDescending(so => so.SalesOrderVersionItems.Count),
                "VatPercentage" => sortDirection == "asc"
                    ? query.OrderBy(so => so.VatPercentage)
                    : query.OrderByDescending(so => so.VatPercentage),
                _ => sortDirection == "asc"
                    ? query.OrderBy(so => so.CreatedAt)
                    : query.OrderByDescending(so => so.CreatedAt)
            };

            // Pagination
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(so => new SalesOrderListItemViewModel
                {
                    SalesOrderID = so.SalesOrdersVersionID,
                    SalesOrderNumber = so.SalesOrders.SalesOrderNumber,
                    CustomerName = so.Customer != null ? so.Customer.FullName : "",
                    QuotationNumber = so.SalesOrders != null ? so.SalesOrders.PriceQuotationVersion.PriceQuotation.QuotationNumber : "",
                    CreatedBy = so.CreatedByNavigation != null
                        ? so.CreatedByNavigation.FirstName + " " + so.CreatedByNavigation.LastName
                        : "",
                    SalesOrderDate = so.SalesOrderDate,
                    TotalItems = so.SalesOrderVersionItems.Count,
                    VatPercentage = so.VatPercentage ?? 0,
                    Note = so.Note
                })
                .ToListAsync();

            return new SalesOrderListResultViewModel
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }
    }
}
