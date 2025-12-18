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

namespace GCTL_App.Controllers.FieldServices
{
    public class EmployeeAdvancedController : BaseController
    {
        private readonly IGenericRepository<JobTypes> _jobTypeRepository;
        private readonly IEmployeeAdvanced _mainservice;
        private readonly IGenericRepository<GCTL.Data.Models.Employees> _employees;
        private readonly ICommonService _commonService;
      


        public EmployeeAdvancedController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<JobTypes> jobTypeRepository, IEmployeeAdvanced service, IGenericRepository<GCTL.Data.Models.Employees> employees, ICommonService commonService, IGenericRepository<JobTypes> jobtype) : base(translateService, userProfileService)
        {
            _jobTypeRepository = jobTypeRepository;
            _mainservice = service;
            _employees = employees;
            _commonService = commonService;
            
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.JobTypesDD = new SelectList(_jobTypeRepository.AllActive().Select(t => new { t.JobTypeID, t.JobTypeName }), "JobTypeID", "JobTypeName");

            ViewBag.EmployeeDD =  new SelectList(await _mainservice.EmployeeDD(), "Id", "Name");

            ViewBag.JobTypesDDD = new SelectList(_jobTypeRepository.AllActive().Select(j => new { j.JobTypeID, j.JobTypeName }), "JobTypeID", "JobTypeName");




            return View();
        }


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


        [HttpGet]
        public async Task<IActionResult> GetJobsType(string search = "", int page = 1, int pageSize = 20)
        {
            var result = await _mainservice.GetJobTypeAsync (
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

        //Nested Job by Customer


    }
}
