using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM
{
    public class CRMService : ICRMService
    {
        #region Services
        private readonly IGenericRepository<Leads> _leadsGenericRepository;
        private readonly AppDbContext _context;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeesRepository;
        public CRMService(IGenericRepository<Leads> leadsGenericRepository, AppDbContext context, IGenericRepository<Data.Models.Employees> employeesRepository)
        {
            _context = context;
            _leadsGenericRepository = leadsGenericRepository;
            _employeesRepository = employeesRepository;
        }
        #endregion

        #region Task
        public async Task<(List<LeadsTableVM> Leads, int TotalCount)> GetLeads(
            int customerType,
            string dateRange,
            string leadStatus2,
            int pageNumber = 1,
            int pageSize = 10,
            string searchTerm = null,
            string sortColumn = null,
            string sortDirection = null)
        {
            var query = from lead in _leadsGenericRepository.AllActive()
                        join indDddr in _context.CustomerAddresses
                            on lead.CustomerID equals indDddr.CustomerID
                        join address in _context.Addresses
                            on indDddr.AddressID equals address.AddressID
                        join individual in _context.Customers
                            on indDddr.CustomerID equals individual.CustomerID
                        join leadStatus in _context.LeadStatuses
                            on lead.LeadStatusID equals leadStatus.LeadStatusID
                        join leadSource in _context.LeadSources
                            on lead.LeadSourceID equals leadSource.LeadSourceID
                        join leadOwner in _context.Employees
                            on lead.LeadOwnerID equals leadOwner.EmployeeID
                        select new LeadsTableVM
                        {
                            LeadId = lead.LeadID,
                            LeadStatus = lead.IsOwn == true ? "Won": lead.IsOwn == false ? "Lost":  leadStatus.LeadStatusName,
                            LeadSourceName = leadSource.LeadSourceName,
                            LeadOwnerName = leadOwner.FirstName + " " + leadOwner.LastName,
                            ApproximateDealValue = lead.ApproximateDealValue,
                            ProbabilityPercentage = lead.ProbabilityPercentage,
                            LeadName = lead.LeadName,
                            Phone = address.Phone,
                            Email = address.Email,
                            ContactName = address.FirstName + " " + address.LastName,
                            Status = lead.LeadStatus.LeadStatusName,
                            CreatedDate = lead.CreatedAt,
                            CustomerTypeID = indDddr.AddressType.AddressTypeID
                        };

            if (customerType > 0)
                query = query.Where(r => r.CustomerTypeID == customerType);

            if (!string.IsNullOrEmpty(leadStatus2))
                query = query.Where(r => r.LeadStatus == leadStatus2);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    (r.LeadName != null && r.LeadName.ToLower().Contains(searchTerm)) ||
                    (r.Phone != null && r.Phone.ToLower().Contains(searchTerm)) ||
                    (r.ContactName != null && r.ContactName.ToLower().Contains(searchTerm)) ||
                    (r.Email != null && r.Email.ToLower().Contains(searchTerm))
                );
            }

            // Sorting
            query = (sortColumn, sortDirection) switch
            {
                ("leadStatus", "desc") => query.OrderByDescending(r => r.LeadStatus),
                ("leadStatus", _) => query.OrderBy(r => r.LeadStatus),
                ("leadName", "desc") => query.OrderByDescending(r => r.LeadName),
                ("leadName", _) => query.OrderBy(r => r.LeadName),
                _ => query.OrderByDescending(r => r.LeadId)
            };

            var totalCount = await query.CountAsync();

            var leads = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (leads, totalCount);
        }
        #endregion

        #region SearchOrganizations
        public async Task<PaginatedResult<CommonSelectVM>> SearchOrganizations(string search, int page = 1, int pageSize = 50)
        {
            var query = _employeesRepository.AllActive().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query
                 .Where(x => x.FirstName.Contains(search) || x.LastName.Contains(search))
                 .OrderBy(x => x.FirstName.IndexOf(search) == -1 ? int.MaxValue : x.FirstName.IndexOf(search))
                 .ThenBy(x => x.LastName.IndexOf(search) == -1 ? int.MaxValue : x.LastName.IndexOf(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                //.OrderBy(x => x.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = x.FirstName + x.LastName ?? "-"
                })
                .ToListAsync();

            return new PaginatedResult<CommonSelectVM>
            {
                Items = items,
                HasMore = (page * pageSize) < totalCount
            };
        }
        #endregion
    }
}
