using GCTL.Core.Repository;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace GCTL_App.Controllers.FieldServices
{
    public class CreateJobsController : BaseController
    {
        private readonly ICreateJobService _createJobService;
        public readonly IGenericRepository<Statuses> _statusRepository;
        public readonly IGenericRepository<JobTypes> _jobTypeRepository;
        public CreateJobsController(ITranslateService translateService, IUserProfileService userProfileService, ICreateJobService createJobService, IGenericRepository<Statuses> statusRepository, IGenericRepository<JobTypes> jobTypeRepository) : base(translateService, userProfileService)
        {
            _createJobService = createJobService;
            _statusRepository = statusRepository;
            _jobTypeRepository = jobTypeRepository;
        }
        public IActionResult Index()
        {
            ViewBag.JobTypesDD = new SelectList(_jobTypeRepository.AllActive().Select(t => new {t.JobTypeID, t.JobTypeName}), "JobTypeID", "JobTypeName");
            ViewBag.StatusDD = new SelectList(_statusRepository.AllActive().Where(t => t.StatusType == "FieldService").Select(t => new {t.StatusID, t.StatusName}), "StatusID", "StatusName");
            return View();
        }

        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([FromForm] CreateJobVM createJobVM)
        {
            try
            {
                if (createJobVM.CreateJobID == 0) {
                    var result = await _createJobService.AddAsync(createJobVM);
                    if (result)
                    {
                        return Ok(new { Success = true, Message = "Job Created" });
                    }
                    return Ok(new { Success = false, Message = "Something goes to wrong" });
                }
                return Ok(new { Success = false, Message = "Id is not valid" });
            } catch(Exception ex)
            {
                return Ok(new { Success = false, Message = ex });
            }
        }
        #endregion

        #region get Customer
        [HttpGet]
        public async Task<IActionResult> GetCustomers(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _createJobService.GetPagedEmployeesAsync(
                search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.LeadID,                          
                    text = $"{c.LeadName} {c.Phone} {c.Email}" 
                }),
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion

        #region get Customer
        [HttpGet]
        public async Task<IActionResult> GetCountryList(string search = "", int page = 1, int pageSize = 10)
        {
            var result = await _createJobService.GetCountryList(
                search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.Value,                          
                    text = c.Text
                }),
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion
        #region get Company Customer
        [HttpGet]
        public async Task<IActionResult> GetCompnayCustomers(string search = "", int page = 1, int pageSize = 10)
        {
            var result = await _createJobService.GetCompanyEmployeesAsync(
                search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.LeadID,                          
                    text = $"{c.LeadName} {c.Phone} {c.Email}" 
                }),
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion

        #region get Indivual Customer
        [HttpGet]
        public async Task<IActionResult> GetIndividualCustomers(string search = "", int page = 1, int pageSize = 10)
        {
            var result = await _createJobService.GetIndividualEmployeesAsync(
                search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );

            var more = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.LeadID,                          
                    text = $"{c.LeadName} {c.Phone} {c.Email}" 
                }),
                pagination = new { more }
            };

            return Ok(formatted);
        }
        #endregion

        #region Get Technician Employee List
        [HttpGet]
        public async Task<IActionResult> GetTechnicianList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _createJobService.GetTechnicianListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );
            var hasMore = (page * pageSize) < result.totalItem;
            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.LeadID,      
                    label = $"{c.LeadName} {c.Phone} {c.Email}",
                    group = "" 
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion

        #region LeadOwner List
        [HttpGet]
        public async Task<IActionResult> GetOwnerList(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _createJobService.GetTechnicianListAsync(
               search, page, pageSize, await GetCurrentOrganizationIdAsync() ?? 0
            );

            var hasMore = (page * pageSize) < result.totalItem;

            var formatted = new
            {
                items = result.data.Select(c => new
                {
                    value = c.LeadID,   
                    label = $"{c.LeadName} {c.Phone} {c.Email}",
                    group = ""
                }),
                hasMore
            };

            return Json(formatted);
        }
        #endregion
    }
}
