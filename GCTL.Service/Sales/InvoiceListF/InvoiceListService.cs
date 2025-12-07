using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Sales.InvoiceListF;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Sales.InvoiceListF
{
    public class InvoiceListService : IInvoiceList
    {
        private readonly IGenericRepository<Invoices> _invoiceRepository;
        private readonly IGenericRepository<InvoiceVersionItems> _invoiceItemRepository;
        private readonly IGenericRepository<InvoicesVersions> _invoiceVersionRepository;

        public InvoiceListService(IGenericRepository<Invoices> invoiceRepository, IGenericRepository<InvoiceVersionItems> invoiceItemRepository, IGenericRepository<InvoicesVersions> invoiceVersionRepository)
        {
            _invoiceRepository = invoiceRepository;
            _invoiceItemRepository = invoiceItemRepository;
            _invoiceVersionRepository = invoiceVersionRepository;
        }

        public async Task<InvoiceListResultViewModel> GetInvoicesWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            var query = _invoiceVersionRepository.AllActive()
                .Include(inv => inv.Customer)
                .Include(inv => inv.Invoice)
                .ThenInclude(inv => inv.SalesOrders)
                .Include(inv => inv.CreatedByNavigation)
                .Include(inv => inv.InvoiceVersionItems)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(inv =>
                    inv.InvoiceNumber.Contains(searchTerm) ||
                    inv.Customer.FullName.Contains(searchTerm) ||
                    (inv.Invoice.SalesOrders != null && inv.Invoice.SalesOrders.SalesOrderNumber.Contains(searchTerm)) ||
                    (inv.InvoiceNote != null && inv.InvoiceNote.Contains(searchTerm)));
            }

            // Get total count before pagination
            var totalRecords = await query.CountAsync();

            // Sorting
            query = sortColumn switch
            {
                "InvoiceID" => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.InvoiceID)
                    : query.OrderByDescending(inv => inv.InvoiceID),
                "InvoiceNumber" => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.InvoiceNumber)
                    : query.OrderByDescending(inv => inv.InvoiceNumber),
                "CustomerName" => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.Customer.FullName)
                    : query.OrderByDescending(inv => inv.Customer.FullName),
                "InvoiceDate" => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.InvoiceDate)
                    : query.OrderByDescending(inv => inv.InvoiceDate),
                "GrandTotal" => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.GrandTotal)
                    : query.OrderByDescending(inv => inv.GrandTotal),
                "PaidAmount" => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.PaidAmount)
                    : query.OrderByDescending(inv => inv.PaidAmount),
                _ => sortDirection == "asc"
                    ? query.OrderBy(inv => inv.CreatedAt)
                    : query.OrderByDescending(inv => inv.CreatedAt)
            };

            // Pagination
            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(inv => new InvoiceListItemViewModel
                {
                    InvoiceID = inv.InvoicesVersionID,
                    InvoiceNumber = inv.Invoice.InvoiceNumber ?? "",
                    CustomerName = inv.Customer != null ? inv.Customer.FullName : "",
                    SalesOrderNumber = inv.Invoice.SalesOrders != null ? inv.Invoice.SalesOrders.SalesOrderNumber : "",
                    CreatedBy = inv.CreatedByNavigation != null ? inv.CreatedByNavigation.FirstName + " " + inv.CreatedByNavigation.LastName : "",
                    InvoiceDate = inv.InvoiceDate,
                    TotalItems = inv.InvoiceVersionItems.Count,
                    VatPercentage = inv.VatPercentage ?? 0,
                    GrandTotal = inv.GrandTotal ?? 0,
                    PaidAmount = inv.PaidAmount ?? 0,
                    DueAmount = (inv.GrandTotal ?? 0) - (inv.PaidAmount ?? 0),
                    IsDraft = inv.IsDraft ?? false,
                    Status = inv.IsDraft == true ? "Draft" : ((inv.GrandTotal ?? 0) - (inv.PaidAmount ?? 0)) <= 0 ? "Paid" : "Unpaid"
                })
                .ToListAsync();

            return new InvoiceListResultViewModel
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }




    }
}
