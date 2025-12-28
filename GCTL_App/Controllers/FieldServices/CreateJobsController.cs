using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.FieldServices;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace GCTL_App.Controllers.FieldServices
{
    public class CreateJobsController : BaseController
    {
        #region property and constractor
        private readonly ICreateJobService _createJobService;
        public readonly IWebHostEnvironment _webHostEnvironment;
        public CreateJobsController(ITranslateService translateService, IUserProfileService userProfileService, ICreateJobService createJobService, IGenericRepository<Statuses> statusRepository, IGenericRepository<JobTypes> jobTypeRepository, IWebHostEnvironment webHostEnvironment) : base(translateService, userProfileService)
        {
            _createJobService = createJobService;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region Index & Partial Index
        public IActionResult Index()
        {
            //ViewBag.JobTypesDD = new SelectList(_jobTypeRepository.AllActive().Select(t => new {t.JobTypeID, t.JobTypeName}), "JobTypeID", "JobTypeName");

            //ViewBag.StatusDD = new SelectList(_statusRepository.AllActive().Select(t => new {t.StatusID, t.StatusName}), "StatusID", "StatusName");

            return View();
        }
        public IActionResult IndexModal()
        {
            return PartialView("_PartialModal");
        }
        #endregion

        #region StorePhoto
        public async Task<string> StorePhoto(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "media/leads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"); // e.g., 20250903154530123
            var random = new Random().Next(100, 999); // 3-digit random number
            var extension = Path.GetExtension(file.FileName); // Keep original extension
            var uniqueFileName = $"{timestamp}_{random}{extension}";

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/media/leads/{uniqueFileName}";
        }
        #endregion

        #region Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert([FromForm] CreateJobVM model)
        {
            model.JobID ??= 0;
            try
            {
                if (ModelState.IsValid)
                {
                    string fileLocation = model.FileLink is not null ? await StorePhoto(model.FileLink) : string.Empty;
                    if (model.JobID == 0)
                    {
                        var result = await _createJobService.AddAsync(model, fileLocation);
                        if (result)
                        {
                            return Ok(new { Success = true, Message = "Job Created" });
                        }
                        return Ok(new { Success = false, Message = "Something goes to wrong" });
                    } else
                    {
                        var result = await _createJobService.EditAsync(model, fileLocation);
                    }
                        return Ok(new { Success = false, Message = "Id is not valid" });
                }
                return Ok(new { Success = false, Message = "Model state is not valid" });
            } catch(Exception ex)
            {
                return Ok(new { Success = false, Message = ex });
            }
        }
        #endregion

        #region get GetCustomer List
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

        #region get Division List
        [HttpGet]
        public async Task<IActionResult> GetDivisions(string search = "")
        {
            var result = await _createJobService.GetDivisionsAsync( search );

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.Value,                          
                    text = c.Text 
                }),
            };

            return Ok(formatted);
        }
        #endregion

        #region Get Statuse List
        [HttpGet]
        public async Task<IActionResult> GetStatuses(string search = "")
        {
            var result = await _createJobService.GetStatusesAsync( search );

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.Value,                          
                    text = c.Text 
                }),
            };

            return Ok(formatted);
        }
        #endregion

        #region Get JobType List
        [HttpGet]
        public async Task<IActionResult> GetJobTypes(string search = "")
        {
            var result = await _createJobService.GetJobTypesAsync( search );

            var formatted = new
            {
                results = result.data.Select(c => new
                {
                    id = c.Value,                          
                    text = c.Text 
                }),
            };

            return Ok(formatted);
        }
        #endregion

        #region Get Job List
        [HttpGet]
        public async Task<IActionResult> GetJobs(int customerId, string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _createJobService.GetJobAsync(customerId,
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

        #region get Customer Info
        public async Task<IActionResult> GetCustomerInfo(int id)
        {
            var result = _createJobService.GetCustomerInfo(id, await GetCurrentOrganizationIdAsync() ?? 0);
            return Json(new
            {
                id = result.LeadID,
                text = $"{result.LeadName} {result.Phone} {result.Email}"
            });
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
