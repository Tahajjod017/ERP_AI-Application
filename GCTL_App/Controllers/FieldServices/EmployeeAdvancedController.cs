using GCTL.Core.Repository;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GCTL.Service.FieldServices.EmployeeAdvanced;
using GCTL.Service.CommonService;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GCTL.Service.FieldServices;
using GCTL.Service.Pagination;
using GCTL.Core.Helpers;

namespace GCTL_App.Controllers.FieldServices
{
    public class EmployeeAdvancedController : BaseController
    {
        private readonly IGenericRepository<JobTypes> _jobTypeRepository;
        private readonly IEmployeeAdvanced _mainservice;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employees;
        private readonly ICommonService _commonService;
        private readonly IGenericRepository<ApprovalSettings> _approvalsettings;
        private readonly IGenericRepository<EmployeeAdvanceFor> _employeeAdvanceForRepository;
        private readonly ICreateJobService _createJobService;
     



        public EmployeeAdvancedController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<JobTypes> jobTypeRepository, IEmployeeAdvanced service, IGenericRepository<GCTL.Data.Models.Employees> employees, ICommonService commonService, IGenericRepository<JobTypes> jobtype, IGenericRepository<ApprovalSettings> approvalsettings, IGenericRepository<EmployeeAdvanceFor> employeeAdvanceForRepository, ICreateJobService createJobService) : base(translateService, userProfileService)
        {
            _jobTypeRepository = jobTypeRepository;
            _mainservice = service;
            _employees = employees;
            _commonService = commonService;
            _approvalsettings = approvalsettings;
            _employeeAdvanceForRepository = employeeAdvanceForRepository;
            _createJobService = createJobService;
           
        }



        #region Index
        public async Task<IActionResult> Index()
        {
            ViewBag.JobTypesDD = new SelectList(_jobTypeRepository.AllActive().Select(t => new { t.JobTypeID, t.JobTypeName }), "JobTypeID", "JobTypeName");

            ViewBag.EmployeeDD = new SelectList(await _mainservice.EmployeeDD(), "Id", "Name");

            ViewBag.JobTypesDDD = new SelectList(_jobTypeRepository.AllActive().Select(j => new { j.JobTypeID, j.JobTypeName }), "JobTypeID", "JobTypeName");

            ViewBag.ApprovalStatus = new SelectList(_approvalsettings.AllActive().Select(a => new { a.ApprovalSettingID }), "ApprovalSettingsID");


            return View();
        }
        #endregion

        #region Create
        [HttpPost]
        public async Task<IActionResult> Create(EmployeeAdvancedVM emp)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .Select(x => new
                    {
                        Field = x.Key,
                        Message = x.Value.Errors.First().ErrorMessage
                    });

                return Json(new { success = false, errors });
            }

            var result = await _mainservice.AddAsync(emp);

            return Json(new
            {
                success = result.Success,
                message = result.Message
            });
        }
        #endregion

        #region Update
        [HttpPost]
        public async Task<IActionResult> Update(EmployeeAdvancedVM emp) 
        
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _mainservice.UpdateAsync(emp);
                    return Json(new
                    {
                        isSuccess = result.Success,
                        message = result.Message
                    });

                }
                var orederedKeys = new[] { "EmployeeID", "JobID", "AdvanceAmount", "AdvanceDate" };
                foreach (var key in orederedKeys)
                {
                    if (ModelState.TryGetValue(key, out var entry) && entry.Errors.Any())
                    {
                        return Json(new { isSuccess = false, field = key, message = entry.Errors.First().ErrorMessage });
                    }
                    var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                    return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong" });

                }

                return Json(new { isSuccess = false, message = "" ?? "Something went wrong" });
            }

            catch (Exception ex)
            {

                return Json(new { isSuccess = false, message = $"Error:{ex.Message}" });
            }
        }

        #endregion

        #region SoftDelete
        [HttpDelete]
        public async Task<IActionResult>Delete(DeleteRequestVM requestVM)
        {
            try
            {
                var result = await _mainservice.SoftDeleteAsync(requestVM);
                return Json(new
                {
                    isSuccess = result.Success,
                    message = result.Message,
                    
                });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

        #region GetJobsDropdown
        [HttpGet]
        public async Task<IActionResult> GetJobsType(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _mainservice.GetJobTypeAsync(
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

        #region GetJobsByJobType

        [HttpGet]
        public async Task<IActionResult> GetJobsByJobType(List<int> jobTypeIds)
        {
            var jobs = await _jobTypeRepository.AllActive()
                .Where(j => jobTypeIds.Contains(j.JobTypeID))
                .Select(j => new
                {
                    id = j.JobTypeID,
                    text = j.JobTypeName
                })
                .ToListAsync();

            return Ok(jobs);
        }
        #endregion

        #region GetJobByCustomer(Nested)
        public async Task<IActionResult> GetJobByCusId(int customerId)
        {
            try
            {
                var result = await _mainservice.GetJobByCusId(customerId);
                return Json(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region GetAll Employee Advance with Pagination
        [HttpGet("EmployeeAdvanced/GetAllAsync")]
        public async Task<IActionResult> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainempId = null)
        {
            var result = await _mainservice.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, mainempId);
            return Json(result);
        }
        #endregion

        #region GetByID

        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _mainservice.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "Record not found" });
                }
                return Json(new
                {
                    isSuccess = true,
                    data = result
                });
            }
            catch (Exception ex)
            {

                return Json(new { isSuccess = false, message = ex.Message });
                #endregion

            }
        }
    }
}

