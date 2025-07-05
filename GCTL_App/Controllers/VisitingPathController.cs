using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL.Service.VisitingPath;
using GCTL_App.ViewModels.VisitingVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers
{
    public class VisitingPathController : BaseController
    {
        private readonly IVisitingPathService visitingPathService;
        public VisitingPathController(ITranslateService translateService, IUserProfileService userProfileService, IVisitingPathService visitingPathService) : base(translateService, userProfileService)
        {
            this.visitingPathService = visitingPathService;
        }

        public async Task<IActionResult> Index()
        {
           
          
            return View();
        }


        #region GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string currentSortColumn = "", string currentSortOrder = "")
        {
            var result = await visitingPathService.GetAllAsync(pageNumber, pageSize, searchTerm, currentSortColumn, currentSortOrder);

            return Json(result);
        }
        #endregion





    }
}
