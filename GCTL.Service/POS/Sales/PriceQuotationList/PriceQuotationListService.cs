using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Sales.PriceQuotationList;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Sales.PriceQuotationList
{
    public class PriceQuotationListService : IPriceQuotationList
    {
        private readonly IGenericRepository<PriceQuotations> _priceQuotationsRepository;
        private readonly IGenericRepository<PriceQuotationVersionItems> _priceQuotationItemsRepository;
        private readonly IGenericRepository<PriceQuotationVersions> _priceQuotationVersionRepository;

        public PriceQuotationListService(IGenericRepository<PriceQuotations> priceQuotationsRepository, IGenericRepository<PriceQuotationVersionItems> priceQuotationItemsRepository, IGenericRepository<PriceQuotationVersions> priceQuotationVersionRepository)
        {
            _priceQuotationsRepository = priceQuotationsRepository;
            _priceQuotationItemsRepository = priceQuotationItemsRepository;
            _priceQuotationVersionRepository = priceQuotationVersionRepository;
        }


        //public async Task<PaginatedResultData<PriceQuotationListDto>> GetPriceQuotationsWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        //{
        //    var query = _priceQuotationsRepository.AllActive();
        //      //  .Include(q => q.Customer)
        //        //.Include(q => q.PriceQuotationItems)
        //        //.Include(q => q.CreatedByNavigation)
        //        //.Where(q => q.IsFinalVersion == true);

        //    // Apply search filter
        //    if (!string.IsNullOrEmpty(searchTerm))
        //    {
        //        searchTerm = searchTerm.ToLower();
        //        query = query.Where(q =>
        //            q.QuotationNumber.ToLower().Contains(searchTerm) ||
        //         //   q.Customer.FullName.ToLower().Contains(searchTerm) ||
        //            q.CreatedByNavigation.FirstName.ToLower().Contains(searchTerm)
        //        );
        //    }

        //    // Apply sorting
        //    query = sortColumn switch
        //    {
        //       // "PriceQuotationID" => sortDirection == "asc" ? query.OrderBy(q => q.PriceQuotationID) : query.OrderByDescending(q => q.PriceQuotationID),
        //       // "QuotationNumber" => sortDirection == "asc" ? query.OrderBy(q => q.QuotationNumber) : query.OrderByDescending(q => q.QuotationNumber),
        //       // "CustomerName" => sortDirection == "asc" ? query.OrderBy(q => q.Customer.FullName) : query.OrderByDescending(q => q.Customer.FullName),
        //       // "CreatedBy" => sortDirection == "asc" ? query.OrderBy(q => q.CreatedByNavigation.FirstName) : query.OrderByDescending(q => q.CreatedByNavigation.FirstName),
        //       // "QuotationDate" => sortDirection == "asc" ? query.OrderBy(q => q.QuotationDate) : query.OrderByDescending(q => q.QuotationDate),
        //      //  "VatPercentage" => sortDirection == "asc" ? query.OrderBy(q => q.VatPercentage) : query.OrderByDescending(q => q.VatPercentage),
        //        _ => sortDirection == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
        //    };

        //    var totalRecords = await query.CountAsync();
        //    var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        //    var data = await query
        //        .Skip((page - 1) * pageSize)
        //        .Take(pageSize)
        //        .Select(q => new PriceQuotationListDto
        //        {
        //            PriceQuotationID = q.PriceQuotationID,
        //            QuotationNumber = q.QuotationNumber,
        //         //   CustomerName = q.Customer.FullName,
        //           // QuotationDate = q.QuotationDate,
        //           // VatPercentage = q.VatPercentage,
        //            //TotalItems = q.PriceQuotationItems.Count,
        //            //CreatedBy = q.CreatedByNavigation.FirstName,
        //           // Note = q.Note
        //        })
        //        .ToListAsync();

        //    return new PaginatedResultData<PriceQuotationListDto>
        //    {
        //        Data = data,
        //        TotalRecords = totalRecords,
        //        TotalPages = totalPages,
        //        CurrentPage = page,
        //        PageSize = pageSize
        //    };
        //}


        public async Task<PaginatedResultData<PriceQuotationListDto>> GetPriceQuotationsWithPagination(int page, int pageSize, string searchTerm, string sortColumn, string sortDirection)
        {
            try
            {
                var query = _priceQuotationVersionRepository.AllActive()
                    .Include(e => e.PriceQuotation)
                    .Include(q => q.Customer)
                    .Include(q => q.PriceQuotationVersionItems)
                    .Include(q => q.CreatedByNavigation)
                    .Where(q => q.IsFinalVersion == true || q.IsDraft == true);


                // Apply search filter
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    searchTerm = searchTerm.ToLower();
                    query = query.Where(q =>
                        q.PriceQuotation.QuotationNumber.ToLower().Contains(searchTerm) ||
                        q.Customer.FullName.ToLower().Contains(searchTerm) ||
                        q.CreatedByNavigation.FirstName.ToLower().Contains(searchTerm)
                    );
                }

                // Apply sorting
                query = sortColumn switch
                {
                    "PriceQuotationID" => sortDirection == "asc" ? query.OrderBy(q => q.PriceQuotationID) : query.OrderByDescending(q => q.PriceQuotationID),
                    "QuotationNumber" => sortDirection == "asc" ? query.OrderBy(q => q.PriceQuotation.QuotationNumber) : query.OrderByDescending(q => q.PriceQuotation.QuotationNumber),
                    "CustomerName" => sortDirection == "asc" ? query.OrderBy(q => q.Customer.FullName) : query.OrderByDescending(q => q.Customer.FullName),
                    "CreatedBy" => sortDirection == "asc" ? query.OrderBy(q => q.CreatedByNavigation.FirstName) : query.OrderByDescending(q => q.CreatedByNavigation.FirstName),
                    "QuotationDate" => sortDirection == "asc" ? query.OrderBy(q => q.QuotationDate) : query.OrderByDescending(q => q.QuotationDate),
                    "VatPercentage" => sortDirection == "asc" ? query.OrderBy(q => q.VatPercentage) : query.OrderByDescending(q => q.VatPercentage),
                    _ => sortDirection == "asc" ? query.OrderBy(q => q.CreatedAt) : query.OrderByDescending(q => q.CreatedAt)
                };

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

                var data = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(q => new PriceQuotationListDto
                    {
                        PriceQuotationID = q.PriceQuotationVersionID,
                        QuotationNumber = q.PriceQuotation.QuotationNumber ?? "",
                        CustomerName = q.Customer.FullName ?? "",
                        QuotationDate = q.QuotationDate,
                        VatPercentage = q.VatPercentage,
                        TotalItems = q.PriceQuotationVersionItems.Count,
                        CreatedBy = q.CreatedByNavigation != null ? q.CreatedByNavigation.FirstName + " " + q.CreatedByNavigation.LastName : "",
                        Note = q.Note ?? ""
                    })
                    .ToListAsync();

                return new PaginatedResultData<PriceQuotationListDto>
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
