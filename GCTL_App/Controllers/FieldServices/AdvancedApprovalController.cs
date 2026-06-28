using System.Security.Claims;
using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.FieldServices.Advanced_Apporval;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.FieldServices
{
    public class AdvancedApprovalController : BaseController
    {

        private readonly IAdvancedApprovalService _mainservice;
        public AdvancedApprovalController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<EmployeeAdvances> genericRepository, IAdvancedApprovalService mainservice) : base(translateService, userProfileService)
        {
            _mainservice = mainservice;
        }

        public IActionResult Index()
        {
            return View();
        }
        //API Pagination
        # region GetAll Employee Advance with Pagination
        [HttpGet("AdvancedApproval/GetAll")]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainempId = null)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _mainservice.GetAllAsync1(pageNumber, pageSize, searchTerm, sortColumn, sortOrder, mainempId,userId);
            return Json(result);
        }
        #endregion
    }
}
