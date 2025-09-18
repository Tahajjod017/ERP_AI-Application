using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using GCTL.Data.Models;
using GCTL.Service.CRM;
using GCTL.Service.CRM.LeadCreate;
using GCTL.Service.CRM.LeadDetail;
using GCTL.Service.Employees.EmployeeResign;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace GCTL_App.Controllers.CRM
{
    [Authorize]
    public class CRMController : BaseController
    {
        private readonly ILeadCreateService _leadCreateService;
        private readonly IGenericRepository<LeadSources> _leadSourceTypeRepository;
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypesRepository;
        private readonly IGenericRepository<LeadStatuses> _leadStatusesRepository;
        private readonly IGenericRepository<Priorities> _prioritiesRepository;
        private readonly IGenericRepository<Services> _serviceTypeRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employeesRepository;


        private readonly AppDbContext _context;
        private readonly ICRMService _crmService;
        private readonly IGenericRepository<AddressTypes> _addressTypeService;
        public CRMController(IGenericRepository<AddressTypes> addressTypeService, ITranslateService translateService, IUserProfileService userProfileService, ICRMService crmService, IGenericRepository<LeadSources> leadSourceTypeRepository, IGenericRepository<LeadActivityTypes> leadActivityTypesRepository, IGenericRepository<LeadStatuses> leadStatusesRepository, IGenericRepository<Priorities> prioritiesRepository, IGenericRepository<Services> serviceTypeRepository, AppDbContext context, ILeadCreateService leadCreateService, IGenericRepository<GCTL.Data.Models.Employees> employeesRepository) : base(translateService, userProfileService)
        {
            _crmService = crmService;
            _addressTypeService = addressTypeService;
            _leadSourceTypeRepository = leadSourceTypeRepository;
            _leadActivityTypesRepository = leadActivityTypesRepository;
            _leadStatusesRepository = leadStatusesRepository;
            _prioritiesRepository = prioritiesRepository;
            _serviceTypeRepository = serviceTypeRepository;
            _context = context;
            _leadCreateService = leadCreateService;
            _employeesRepository = employeesRepository;
        }

        public IActionResult Index()
        {


            ViewBag.ServiceDD = new SelectList(_serviceTypeRepository.AllActive().Select(e => new { e.ServiceID, e.ServiceName }), "ServiceID", "ServiceName");
            ViewBag.LeadSourceDD = new SelectList(_leadSourceTypeRepository.AllActive().Select(e => new { e.LeadSourceID, e.LeadSourceName }), "LeadSourceID", "LeadSourceName");
            ViewBag.LeadActivityTypes = _leadActivityTypesRepository.AllActive().Select(e => new { e.LeadActivityTypeID, e.LeadActivityIcon, e.LeadActivityName }).ToList();
            ViewBag.LeadStatus = new SelectList(_leadStatusesRepository.AllActive().Select(e => new { e.LeadStatusID, e.LeadStatusName }), "LeadStatusID", "LeadStatusName");
            ViewBag.LeadPriorities = new SelectList(_prioritiesRepository.AllActive().Select(e => new { e.PriorityID, e.PriorityName }), "PriorityID", "PriorityName");



            SetSmartPageCode(605000);

            ViewBag.ServiceTypeDD = new SelectList(_addressTypeService.AllActive().Where(u => u.AddressTypeName == "billing" || u.AddressTypeName == "company").Select(e => new { e.AddressTypeID, e.AddressTypeName }), "AddressTypeID", "AddressTypeName");
            return View();
        }

        #region Approved

        [HttpGet]
        public async Task<IActionResult> GetAllLead(
     string dateRange,
     int customerType,
     string designation,
     int pageNumber = 1,
     int pageSize = 10,
     string searchTerm = "",
     string sortColumn = "",
     string sortDirection = "desc")
        {
            var (leads, totalCount) = await _crmService.GetLeads(
                customerType, dateRange, pageNumber, pageSize,
                searchTerm, sortColumn, sortDirection
            );

            var result = new LeadListViewModel
            {
                Leads = leads,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SearchTerm = searchTerm,
                SortColumn = sortColumn,
                SortDirection = sortDirection
            };

            return Json(new { result });
        }

        #endregion


        // =================
        // passing single lead data to view page
        // =================
        public async Task<IActionResult> GetLeadInfo([FromForm] int? id)
        {
            if (id == null)
            {
                return BadRequest("Lead ID is required");
            }

            var customerObj = await (from lead in _context.Leads
                                     join cAddress in _context.CustomerAddresses
                                     on lead.CustomerID equals cAddress.CustomerID
                                     join customer in _context.Customers on cAddress.CustomerID equals customer.CustomerID
                                     join address in _context.Addresses on cAddress.AddressID equals address.AddressID

                                     where lead.LeadID == id
                                     select new CustomerInfoVM
                                     {
                                         LeadID = lead.LeadID,
                                         LeadName = lead.LeadName,
                                         LeadSourceID = lead.LeadSourceID ?? 0,
                                         LeadStatusID = lead.LeadStatusID ?? 0,
                                         PriorityID = lead.PriorityID ?? 0,
                                         ApproximateDealValue = lead.ApproximateDealValue ?? 0m,
                                         Priority = lead.Priority.PriorityName,
                                         Probability = (int)(lead.ProbabilityPercentage ?? 0),
                                         LeadDescription = lead.LeadDescription,
                                         AddressTypeName = cAddress.AddressType.AddressTypeName,
                                         
                                         LeadOwnerId = lead.LeadOwnerID,
                                         LeadOwnerName = lead.LeadOwner.FirstName + " " + lead.LeadOwner.LastName,
                                         ServiceIds = lead.LeadServices.Where(s => s.ServiceID.HasValue).Select(s => s.ServiceID).ToList(),
                                     }).FirstOrDefaultAsync();
            if (customerObj != null)
            {
                return Ok(customerObj);
            }
            else
            {
                CustomerInfoVM obj = new();
                return Ok(obj);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditLeadData([FromBody] LeadUpdateVM leadUpdateVM)
        {
            if (ModelState.IsValid)
            {
                if (leadUpdateVM.LeadID != 0)
                {
                    var result = await _leadCreateService.EditLead(leadUpdateVM);
                    return Ok(result);
                }
            }
            var results = new ReturnView
            {
                Success = false,
                Message = "Data not inserted",
            };
            return Ok(results);

        }



        // ================
        // owner list
        // ================

        [HttpPost]
        public async Task<IActionResult> GetOwnerList([FromForm] string? query, int page)
        {
            query = query?.Trim() ?? "";

            const int pageSize = 10;
            int skip = (page - 1) * pageSize;

            var filtered = _employeesRepository
                .AllActive()
                .AsNoTracking()
                .Select(c => new
                {
                    Id = c.EmployeeID,
                    Name = c.FirstName + " " + c.LastName,
                });

            // Only search if query has value
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(x => EF.Functions.Like(x.Name, $"{query}%"));
            }

            var results = await filtered
                .OrderBy(x => x.Name) // alphabetical
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return Json(new { success = true, result = results });
        }


        // ======================
        // get Employee
        // =====================
        #region SearchOrganizations / OrganizationDD
        [HttpGet]
        public async Task<IActionResult> SearchEmployee(string search, int page = 1, int pageSize = 50)
        {
            var result = await _crmService.SearchOrganizations(search, page, pageSize);
            return Json(new
            {
                items = result.Items.Select(x => new {
                    value = x.Id,
                    label = x.Name,
                    group = x.GroupName // Optional: only if you want to group
                }),
                hasMore = result.HasMore
            });
        }
        #endregion
    }



}
