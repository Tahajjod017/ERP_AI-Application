using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.VisitingPath;
using GCTL_App.ViewModels.VisitingVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL_App.Controllers
{
    public class VisitingPathController : Controller
    {

       
        private readonly IVisitingPathService visitingPathService;
        public VisitingPathController(IVisitingPathService visitingPathService)
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
        string sortColumn = "", string sortOrder = "")
        {
            var result = await visitingPathService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion





    }
}
