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
    public class ShipmentListService : IShipmentList
    {
        private readonly IGenericRepository<Shipments> _shipmentRepository;
        private readonly IGenericRepository<ShipmentItems> _shipmentItemRepository;

        public ShipmentListService(
            IGenericRepository<Shipments> shipmentRepository,
            IGenericRepository<ShipmentItems> shipmentItemRepository)
        {
            _shipmentRepository = shipmentRepository;
            _shipmentItemRepository = shipmentItemRepository;
        }

        public async Task<ShiPaginatedResultData<ShipmentListDto>> GetShipmentsWithPagination(
            int page,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection)
        {
            try
            {
                var query = _shipmentRepository.AllActive()
                    .Include(s => s.ShipmentItems)
                    .Include(s => s.Status)
                    .Include(s => s.SalesOrdersVersion).ThenInclude(e=>e.SalesOrders)
                    .Include(s => s.Invoice)
                    .Include(s => s.ShippingAddress)
                    .Include(s => s.CreatedByNavigation).AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(s =>
                        s.ShipmentNumber.ToLower().Contains(searchTerm) ||
                        (s.TrackingNumber != null && s.TrackingNumber.ToLower().Contains(searchTerm)) ||
                        (s.CreatedByNavigation != null && s.CreatedByNavigation.FirstName.ToLower().Contains(searchTerm))
                    );
                }

                // Apply sorting
                query = sortColumn switch
                {
                    "ShipmentID" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ShipmentID)
                        : query.OrderByDescending(s => s.ShipmentID),
                    "ShipmentNumber" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ShipmentNumber)
                        : query.OrderByDescending(s => s.ShipmentNumber),
                    "ShipmentDate" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ShipmentDate)
                        : query.OrderByDescending(s => s.ShipmentDate),
                    "ExpectedDeliveryDate" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ExpectedDeliveryDate)
                        : query.OrderByDescending(s => s.ExpectedDeliveryDate),
                    "ShippingCost" => sortDirection == "asc"
                        ? query.OrderBy(s => s.ShippingCost)
                        : query.OrderByDescending(s => s.ShippingCost),
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
                        ShipmentID = s.ShipmentID,
                        ShipmentNumber = s.ShipmentNumber ?? "",
                        SourceType = s.SalesOrdersVersionID.HasValue ? "Sales Order" : "Invoice",
                        SourceNumber = s.SalesOrdersVersionID.HasValue
                            ? (s.SalesOrdersVersion != null ? s.SalesOrdersVersion.SalesOrders.SalesOrderNumber : "")
                            : (s.Invoice != null ? s.Invoice.InvoiceNumber : ""),
                        ShipmentDate = s.ShipmentDate,
                        ExpectedDeliveryDate = s.ExpectedDeliveryDate,
                        ActualDeliveryDate = s.ActualDeliveryDate,
                        ShippingMethod = "", // TODO: Add when ShippingMethods table is available
                        TrackingNumber = s.TrackingNumber ?? "",
                        ShippingCost = s.ShippingCost,
                        TotalItems = s.ShipmentItems.Count,
                        Status = s.Status != null ? s.Status.StatusName : "Pending",
                        CreatedBy = s.CreatedByNavigation != null
                            ? s.CreatedByNavigation.FirstName + " " + s.CreatedByNavigation.LastName
                            : "",
                        ShippingAddress = s.ShippingAddress != null
                            ? s.ShippingAddress.FullAddress
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
