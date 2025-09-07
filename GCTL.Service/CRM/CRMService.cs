using GCTL.Core.Repository;
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
        private readonly IGenericRepository<Leads> _leadsGenericRepository;
        private readonly AppDbContext _context;
        public CRMService(IGenericRepository<Leads> leadsGenericRepository, AppDbContext context)
        {
            _context = context;
            _leadsGenericRepository = leadsGenericRepository;
        }
        public async Task<(List<LeadsTableVM> Leads, int TotalCount)> GetLeads(
        int customerType,
        string dateRange,
        int pageNumber,
        int pageSize,
        string searchTerm,
        string sortColumn,
        string sortDirection)
        {
            // Base query
            var query = from lead in _leadsGenericRepository.AllActive()
                        join indDddr in _context.CustomerAddresses
                            on lead.CustomerID equals indDddr.CustomerAddressID
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
                            LeadStatus = leadStatus.LeadStatusName,
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


            // Filter by customerType if provided
            if (customerType > 0)
            {
                query = query.Where(r => r.CustomerTypeID == customerType);
            }


            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.Split(" to ", StringSplitOptions.RemoveEmptyEntries);

                // single date case
                if (dates.Length == 1)
                {
                    if (DateTime.TryParseExact(dates[0].Trim(), "dd/MM/yy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var singleDate))
                    {
                        query = query.Where(r => r.CreatedDate == singleDate.Date);
                    }
                }
                // range case
                else if (dates.Length == 2)
                {
                    if (DateTime.TryParseExact(dates[0].Trim(), "dd/MM/yy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var startDate) &&
                        DateTime.TryParseExact(dates[1].Trim(), "dd/MM/yy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None, out var endDate))
                    {
                        query = query.Where(r => r.CreatedDate >= startDate.Date &&
                                                 r.CreatedDate <= endDate.Date);
                    }
                }
            }


            // 🔹 Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(r =>
                    (r.LeadName != null && r.LeadName.ToLower().Contains(searchTerm)) ||
                    (r.Phone != null && r.Phone.ToLower().Contains(searchTerm)) ||
                    (r.ContactName != null && r.ContactName.ToLower().Contains(searchTerm)) ||
                    (r.ApproximateDealValue.ToString().ToLower().Contains(searchTerm)) ||
                    (r.ProbabilityPercentage.ToString().ToLower().Contains(searchTerm)) ||
                    (r.Email != null && r.Email.ToLower().Contains(searchTerm)) ||
                    (r.LeadOwnerName != null && r.LeadOwnerName.ToLower().Contains(searchTerm))
                );
            }

            // 🔹 Sorting
            query = (sortColumn, sortDirection) switch
            {
                ("leadStatus", "desc") => query.OrderByDescending(r => r.LeadStatus),
                ("leadStatus", _) => query.OrderBy(r => r.LeadStatus),

                ("leadSourceName", "desc") => query.OrderByDescending(r => r.LeadSourceName),
                ("leadSourceName", _) => query.OrderBy(r => r.LeadSourceName),

                ("leadOwnerName", "desc") => query.OrderByDescending(r => r.LeadOwnerName),
                ("leadOwnerName", _) => query.OrderBy(r => r.LeadOwnerName),

                ("leadName", "desc") => query.OrderByDescending(r => r.LeadName),
                ("leadName", _) => query.OrderBy(r => r.LeadName),

                ("email", "desc") => query.OrderByDescending(r => r.Email),
                ("email", _) => query.OrderBy(r => r.Email),

                ("phone", "desc") => query.OrderByDescending(r => r.Phone),
                ("phone", _) => query.OrderBy(r => r.Phone),

                ("approximateDealValue", "desc") => query.OrderByDescending(r => r.ApproximateDealValue),
                ("approximateDealValue", _) => query.OrderBy(r => r.ApproximateDealValue),

                ("probabilityPercentage", "desc") => query.OrderByDescending(r => r.ProbabilityPercentage),
                ("probabilityPercentage", _) => query.OrderBy(r => r.ProbabilityPercentage),

                ("status", "desc") => query.OrderByDescending(r => r.Status),
                ("status", _) => query.OrderBy(r => r.Status),

                _ => query.OrderByDescending(r => r.LeadId) // default
            };

            // 🔹 Count before paging
            var totalCount = await query.CountAsync();

            // 🔹 Paging
            var leads = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return (leads, totalCount);
        }
    }
}
