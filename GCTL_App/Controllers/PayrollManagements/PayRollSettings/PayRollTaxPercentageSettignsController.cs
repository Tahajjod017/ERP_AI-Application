using GCTL.Core.Helpers;
using GCTL.Core.Helpers.CommonSelectMasterDropDown;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.BloodGroup;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings;
using GCTL.Data.Models;
using GCTL.Service.Language;
using GCTL.Service.MasterSetup.BloodGroups;
using GCTL.Service.PayRollManagements.PayRollSettings;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.MasterSetup.BloodGroup;
using GCTL_App.ViewModels.PayRollManagements.PayRollSettings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;


namespace GCTL_App.Controllers.PayrollManagements.PayRollSettings
{
    public class PayRollTaxPercentageSettignsController : BaseController
    {
        private readonly IPayRollTaxperCentangeSettingsService payRollTaxperCentangeSettingsService;
        private readonly ICommonDroDownService commonDroDownService;

        #region Services & Repositories



        public PayRollTaxPercentageSettignsController(IPayRollTaxperCentangeSettingsService payRollTaxperCentangeSettingsService, ITranslateService translateService, IUserProfileService userProfileService,  ICommonDroDownService commonDroDownService) : base(translateService, userProfileService)
        {
            this.payRollTaxperCentangeSettingsService = payRollTaxperCentangeSettingsService;
            this.commonDroDownService = commonDroDownService;
        }
        #endregion


        #region Index
        public async Task<IActionResult> Index()
        {
            PayRolltaxpercentagePageVM model = new PayRolltaxpercentagePageVM();
            var orga = await commonDroDownService.GetAllOrganizationsAsync(); 

            if (orga.Count == 1) 
            {
                ViewBag.OrganizationDD = new SelectList(orga, "Id", "Name", orga.First().Id);
            }
            else
            {
                ViewBag.OrganizationDD = new SelectList(orga, "Id", "Name");
            }
          
            return View(model);
        }


        #endregion


        #region GetById
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await payRollTaxperCentangeSettingsService.GetByIdAsync(id);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No data found!" });
                }

                return Json(new { isSuccess = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region GetAll
        public async Task<IActionResult> GetAll(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "", string sortOrder = "desc")
        {
            var result = await payRollTaxperCentangeSettingsService.GetAllAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);

            return Json(result);
        }
        #endregion

        #region Update
        //[Permission("Edit", "BloodGroups")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Update(PayRollTaxpercentageUpdateVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await payRollTaxperCentangeSettingsService.UpdateAsync(model);
                    return Json(new { isSuccess = true, message = "Updated Successfully." });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region Create
        //[Permission("Create", "BloodGroups")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(PayRollTaxPercentageSaveVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //var uniqueName = await payRollTaxperCentangeSettingsService.IsNameUniqueAsync(model.OrganizationID);
                    //if (!uniqueName)
                    //{
                    //    return Json(new { isSuccess = false, message = "This name already exists!" });
                    //}
                    await payRollTaxperCentangeSettingsService.AddAsync(model);
                    return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.PSettingID });
                }
                var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

                return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion


        #region CheckNameUnique
        [HttpPost]
        public async Task<IActionResult> CheckNameUnique(string name)
        {
            try
            {
                bool isUnique = await payRollTaxperCentangeSettingsService.IsNameUniqueAsync(name);
                if (!isUnique)
                {
                    return Json("This name already exists.");
                }
                return Json(true);
            }
            catch (Exception ex)
            {
                return Json("Error occurred: " + ex.Message);
            }
        }
        #endregion


        #region SoftDelete
        [Permission("Delete", "BloodGroups")]
        [HttpPost]
        public async Task<IActionResult> SoftDelete(DeleteRequestVM requestVM)
        {
            try
            {
                if (requestVM.Ids == null || !requestVM.Ids.Any() || requestVM.Ids.Count == 0)
                {
                    return Json(new { isSuccess = false, message = "No id selected to delete." });
                }
                var result = await payRollTaxperCentangeSettingsService.SoftDeleteAsync(requestVM);
                if (result == null)
                {
                    return Json(new { isSuccess = false, message = "No id found to delete." });
                }

                return Json(new { isSuccess = true, message = "Deleted Successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { isSuccess = false, message = ex.Message });
            }
        }
        #endregion

    }
}
