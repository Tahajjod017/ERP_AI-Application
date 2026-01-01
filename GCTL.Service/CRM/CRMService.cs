using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

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
    int currentOrgId,
    int customerType,
    string dateRange, // Note: not used yet — add date filtering if needed later
    string leadStatus2,
    int pageNumber = 1,
    int pageSize = 10,
    string searchTerm = null,
    string sortColumn = null,
    string sortDirection = null)
        {
            IQueryable<LeadsTableVM> query = _leadsGenericRepository.AllActive()
                .AsNoTracking()
                .Where(lead => lead.OrganizationID == currentOrgId)
                .Select(lead => new LeadsTableVM
                {
                    LeadId = lead.LeadID,
                    LeadName = lead.LeadName,
                    ApproximateDealValue = lead.ApproximateDealValue,
                    ProbabilityPercentage = lead.ProbabilityPercentage,
                    CreatedDate = lead.CreatedAt,

                    // LeadStatus with Won/Lost override
                    LeadStatus = lead.IsOwn == true ? "Won" :
                                 lead.IsOwn == false ? "Lost" :
                                 lead.LeadStatus != null ? lead.LeadStatus.LeadStatusName : "",

                    Status = lead.LeadStatus != null ? lead.LeadStatus.LeadStatusName : "",
                    LeadSourceName = lead.LeadSource != null ? lead.LeadSource.LeadSourceName : "",
                    LeadOwnerName = lead.LeadOwner != null
                        ? lead.LeadOwner.FirstName + " " + lead.LeadOwner.LastName
                        : "",

                    // Assuming each customer has at least one address; take the first one
                    // You might want to filter by primary address or specific AddressType if needed
                    Phone = lead.Customer != null && lead.Customer.CustomerAddresses.Any(ca => ca.Address != null)
                        ? lead.Customer.CustomerAddresses
                            .FirstOrDefault(ca => ca.Address != null).Address.Phone ?? ""
                        : "",

                    Email = lead.Customer != null && lead.Customer.CustomerAddresses.Any(ca => ca.Address != null)
                        ? lead.Customer.CustomerAddresses
                            .FirstOrDefault(ca => ca.Address != null).Address.Email ?? ""
                        : "",

                    ContactName = lead.Customer != null && lead.Customer.CustomerAddresses.Any(ca => ca.Address != null)
                        ? (lead.Customer.CustomerAddresses
                            .FirstOrDefault(ca => ca.Address != null).Address.FirstName ?? "") + " " +
                          (lead.Customer.CustomerAddresses
                            .FirstOrDefault(ca => ca.Address != null).Address.LastName ?? "")
                        : "",

                    CustomerTypeID = lead.Customer != null && lead.Customer.CustomerAddresses.Any()
                        ? lead.Customer.CustomerAddresses.FirstOrDefault().AddressType.AddressTypeID
                        : 0
                });

            // Filters
            if (customerType > 0)
                query = query.Where(r => r.CustomerTypeID == customerType);

            if (!string.IsNullOrEmpty(leadStatus2))
                query = query.Where(r => r.LeadStatus == leadStatus2);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.Trim().ToLowerInvariant();
                query = query.Where(r =>
                    (r.LeadName != null && EF.Functions.Like(r.LeadName.ToLower(), $"%{term}%")) ||
                    (r.Phone != null && EF.Functions.Like(r.Phone, $"%{term}%")) ||
                    (r.ContactName != null && EF.Functions.Like(r.ContactName.ToLower(), $"%{term}%")) ||
                    (r.Email != null && EF.Functions.Like(r.Email.ToLower(), $"%{term}%"))
                );
            }

            // Sorting
            var sortKey = sortColumn?.Trim().ToLowerInvariant();
            var sortAsc = sortDirection?.Trim().ToLowerInvariant() != "desc";

            query = sortKey switch
            {
                "leadstatus" when sortAsc => query.OrderBy(r => r.LeadStatus),
                "leadstatus" => query.OrderByDescending(r => r.LeadStatus),
                "leadname" when sortAsc => query.OrderBy(r => r.LeadName),
                "leadname" => query.OrderByDescending(r => r.LeadName),
                _ => query.OrderByDescending(r => r.LeadId)
            };

            // Count and paging in two efficient queries
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
