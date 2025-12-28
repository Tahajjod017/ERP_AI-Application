using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.POS.Sales.ShipmentList;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Sales.ShipmentList
{
    public class ChallanListService : IChallanList
    {
        private readonly IGenericRepository<Challans> _challanRepository;
        private readonly IGenericRepository<ChallanItems> _challanItemRepository;

        public ChallanListService(IGenericRepository<Challans> shipmentRepository, IGenericRepository<ChallanItems> shipmentItemRepository)
        {
            _challanRepository = shipmentRepository;
            _challanItemRepository = shipmentItemRepository;
        }

        public async Task<ShiPaginatedResultData<ShipmentListDto>> GetChallanWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            try
            {
                var query = _challanRepository.AllActive()
                    .Include(s => s.ChallanItems)
                    .Include(s => s.Status)
                    .Include(s => s.SalesOrdersVersion).ThenInclude(e=>e.SalesOrders)
                    .Include(s => s.Invoice)
                    .Include(s => s.DeliveryAddress)
                    .Include(s => s.CreatedByNavigation).AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(s =>
                        s.ChallanNumber.ToLower().Contains(searchTerm) ||
                        (s.TrackingNumber != null && s.TrackingNumber.ToLower().Contains(searchTerm)) ||
                        (s.CreatedByNavigation != null && s.CreatedByNavigation.FirstName.ToLower().Contains(searchTerm))
                    );
                }

                // Apply sorting
                query = sortColumn switch
                {
                    "ShipmentID" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ChallanID)
                        : query.OrderByDescending(s => s.ChallanID),
                    "ShipmentNumber" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ChallanNumber)
                        : query.OrderByDescending(s => s.ChallanNumber),
                    "ShipmentDate" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ChallanDate)
                        : query.OrderByDescending(s => s.ChallanDate),
                    "ExpectedDeliveryDate" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ExpectedDeliveryDate)
                        : query.OrderByDescending(s => s.ExpectedDeliveryDate),
                    "ShippingCost" => sortDirection == "asc"
                        ? query.OrderBy(s => s.DeliveryCost)
                        : query.OrderByDescending(s => s.DeliveryCost),
                    _ => sortDirection == "asc"
                        ? query.OrderBy(s => s.CreatedAt)
                        : query.OrderByDescending(s => s.CreatedAt)
                };

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(s => new ShipmentListDto
                    {
                        ShipmentID = s.ChallanID,
                        ShipmentNumber = s.ChallanNumber ?? "",
                        SourceType = s.SalesOrdersVersionID.HasValue ? "Sales Order" : "Invoice",
                        SourceNumber = s.SalesOrdersVersionID.HasValue
                            ? (s.SalesOrdersVersion != null ? s.SalesOrdersVersion.SalesOrders.SalesOrderNumber : "")
                            : (s.Invoice != null ? s.Invoice.InvoiceNumber : ""),
                        ShipmentDate = s.ChallanDate,
                        ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                        ActualDeliveryDate = s.ActualDeliveryDate,
                        ShippingMethod = "", // TODO: Add when ShippingMethods table is available
                        TrackingNumber = s.TrackingNumber ?? "",
                        ShippingCost = s.DeliveryCost,
                        TotalItems = s.ChallanItems.Count,
                        Status = s.Status != null ? s.Status.StatusName : "Pending",
                        CreatedBy = s.CreatedByNavigation != null
                            ? s.CreatedByNavigation.FirstName + " " + s.CreatedByNavigation.LastName
                            : "",
                        ShippingAddress = s.DeliveryAddress != null
                            ? s.DeliveryAddress.FullAddress
                            : ""
                    })
                    .ToListAsync();

                return new ShiPaginatedResultData<ShipmentListDto>
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
