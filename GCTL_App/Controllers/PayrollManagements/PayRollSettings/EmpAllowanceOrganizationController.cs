using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.PayRollManagements.AllowanceType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;


namespace GCTL_App.Controllers.PayrollManagements.PayRollSettings
{
    public class EmpAllowanceOrganizationController : BaseController
    {
        private readonly IGenericRepository<Organization> _organizationRepository;
        private readonly IEmpAllowanceTypeOrganizationService empAllowanceTypeOrganizationService;
        public EmpAllowanceOrganizationController(ITranslateService translateService, IUserProfileService userProfileService, IGenericRepository<Organization> organizationRepository, IEmpAllowanceTypeOrganizationService empAllowanceTypeOrganizationService) : base(translateService, userProfileService)
        {
            _organizationRepository = organizationRepository;
            this.empAllowanceTypeOrganizationService = empAllowanceTypeOrganizationService;
        }

        public IActionResult Index()
        {
            EmpAllowanceOrganizationPageVM model = new EmpAllowanceOrganizationPageVM();
            ViewBag.OrganizationDD = new SelectList(_organizationRepository.AllActive(), "OrganizationID", "OrganizationName");
            return View(model);
        }

        #region Save 
        [ValidateAntiForgeryToken]
        [Route("EmpAllowanceOrganization/Create")]
        [HttpPost]
        public async Task<IActionResult> Create(EmpAllowanceTypeOrganizationSaveVM model)
        {
            try
            {
                var data = await empAllowanceTypeOrganizationService.SaveAsync(model);
                return Json(  new {Success=true, Message="Saved Successfully" });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
        #region Get By data
        [Route("EmpAllowanceOrganization/GetByID")]
        [HttpGet]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {
                var data = await empAllowanceTypeOrganizationService.GetByIdAsync(id);

                if (data == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"No record found for ID {id}."
                    });
                }

                return Json(new
                {
                    Success = true,
                    Data = data
                });
            }
            catch (Exception ex)
            {
                // Log the exception if needed
                Console.WriteLine(ex.Message);

                return BadRequest(new
                {
                    Success = false,
                    Message = "An error occurred while fetching the data.",
                    Details = ex.Message
                });
            }
        }

        #endregion
        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "desc")
        {
            var result = await empAllowanceTypeOrganizationService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion
        //[Permission("Delete", "BloodGroups")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                
                var data = await empAllowanceTypeOrganizationService.SoftDeleteAsync(requestVM);
                

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
    }
}
