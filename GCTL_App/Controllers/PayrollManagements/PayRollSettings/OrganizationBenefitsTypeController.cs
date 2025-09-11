using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.PayrollManagements.PayRollOrganizationBenefitsType;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Service.Language;
using GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization;
using GCTL.Service.PayRollManagements.PayRollOrgaBenefitsType;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace GCTL_App.Controllers.PayrollManagements.PayRollSettings
{
    public class OrganizationBenefitsTypeController : BaseController
    {
        private IPayRollOrgaBenefitsTypeService payRollOrgaBenefitsTypeService;
        public OrganizationBenefitsTypeController(ITranslateService translateService, IUserProfileService userProfileService, IPayRollOrgaBenefitsTypeService payRollOrgaBenefitsTypeService) : base(translateService, userProfileService)
        {
            this.payRollOrgaBenefitsTypeService = payRollOrgaBenefitsTypeService;
        }

        public async Task<IActionResult> Index()
        {
            OrgaBenefitsTypeSaveVM model= new OrgaBenefitsTypeSaveVM();

            var orga = await payRollOrgaBenefitsTypeService.GetAllEmploye();
            if (orga.Count == 1)
            {
                ViewBag.SelectedOrgaDD = new SelectList(orga, "Id", "Name", orga.First().Id);
            }
            else
            {
                ViewBag.SelectedOrgaDD = new SelectList(orga, "Id", "Name");
            }

            return View(model);
        }
        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "desc")
        {
            var result = await payRollOrgaBenefitsTypeService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion
        #region Save 
        [Route("OrganizationBenefitsType/Save")]
        [HttpPost]
        public async Task<IActionResult> Save(OrgaBenefitsTypeSaveVM model)
        {
            try
            {
                var data = await payRollOrgaBenefitsTypeService.Save(model);
                return Json(new {Success=data.Success, Message=data.Message});
                  
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Update
        [Route("OrganizationBenefitsType/Update")]
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Update(OrgaBenefitsTypeSaveVM model)
        {

            try
            {

                var data = await payRollOrgaBenefitsTypeService.Update(model);
                return Json(new { Success = data.Success, Message = data.Message });

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
        [Route("OrganizationBenefitsType/GetByID")]
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
                var data = await payRollOrgaBenefitsTypeService.GetByIdAsync(id);
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
        #region Delete
        //[Permission("Delete", "BloodGroups")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {

                var data = await payRollOrgaBenefitsTypeService.SoftDeleteAsync(requestVM);


                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion
    }
}
