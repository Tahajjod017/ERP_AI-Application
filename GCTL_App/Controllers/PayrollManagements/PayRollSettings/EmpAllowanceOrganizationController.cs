using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization;
using GCTL.Service.PayRollManagements.PayRollOrgaBenefitsType;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.PayRollManagements.AllowanceType;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
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
            
            var orga = _organizationRepository.AllActive();
            if(orga.Count()==1)
            {
                ViewBag.OrganizationDD = new SelectList( orga, "OrganizationID", "OrganizationName",orga.First().OrganizationID);
            }
            else
            {
                ViewBag.OrganizationDD = new SelectList(orga, "OrganizationID", "OrganizationName");
            }
           
            return View(model);
        }

        #region Save 
        
        [Route("EmpAllowanceOrganization/Create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpAllowanceTypeOrganizationSaveVM model)
        {
            try
            {
               var data = await empAllowanceTypeOrganizationService.SaveAsync(model);
               return Json(new { Success = data.Success, Message = data.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.ToString()
                });
            }
        }
        #endregion

        #region Update
        [Route("EmpAllowanceOrganization/Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Update(EmpAllowanceTypeOrganizationSaveVM model)
        {

            try
            {
               
                    var data = await empAllowanceTypeOrganizationService.UpdateAsync(model);
                    return Json(new { Success = data.Success, Message =data.Message });
               
            }
            catch (Exception ex)
            {
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

        #region Get By data
        [Route("EmpAllowanceOrganization/GetByID")]
        [HttpGet]
        public async Task<IActionResult> GetByID(int id)
        {
            try
            {

                if (id == 0)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = $"Not Found ID {id}."
                    });
                }
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
        #region Delete
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
        #endregion
        #region Get Company
        [HttpGet]
        public async  Task<IActionResult> GetComapany(int id)
        {
            try
            {
                var data = await empAllowanceTypeOrganizationService.SelectAsync(id);
                return Json(data);
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
