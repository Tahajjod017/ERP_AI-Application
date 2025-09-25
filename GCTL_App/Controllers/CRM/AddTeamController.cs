using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.AccessPermissions;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.CRM.AddTeam;
using GCTL.Service.Language;
using GCTL.Service.RolePermissions;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AddTeam;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.CRM
{
    public class AddTeamsController : BaseController
    {
        #region Services & Repositories
        private readonly ITranslateService _translationService;
        private readonly IAddTeamService _addTeamService;

        public AddTeamsController(ITranslateService translateService, IAddTeamService addTeamService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
            _translationService = translateService;
            _addTeamService = addTeamService;
        }


        #endregion


        //#region Index
        //[Permission("View", "AddTeam")]
        public async Task<IActionResult> Index()
        {
            AddTeamPageVM model = new AddTeamPageVM();

            //ViewBag.EmployeeDD = new SelectList(await _addTeamService.GetEmployees(), "Id", "Name");

            #region Language Setup
            var languageCode = HttpContext.Items["Language"] as string ?? "en";
            int PageCode = 340000; // Unique page code for team translations

            //// Adding translations for all labels
            ViewBag.Save = _translationService.GetTranslationInd("Save", (PageCode++).ToString(), languageCode);
            ViewBag.Reset = _translationService.GetTranslationInd("Reset", (PageCode++).ToString(), languageCode);
            ViewBag.TeamName = _translationService.GetTranslationInd("Team Name", (PageCode++).ToString(), languageCode);
            ViewBag.AddTeamMember = _translationService.GetTranslationInd("Add Team Member", (PageCode++).ToString(), languageCode);
            ViewBag.SelectEmployee = _translationService.GetTranslationInd("Select Employee", (PageCode++).ToString(), languageCode);
            ViewBag.AddNewTeam = _translationService.GetTranslationInd("Add New Team", (PageCode++).ToString(), languageCode);
            ViewBag.ViewAllTeam = _translationService.GetTranslationInd("View All Team", (PageCode++).ToString(), languageCode);
            ViewBag.GeneratedID = _translationService.GetTranslationInd("Generated ID", (PageCode++).ToString(), languageCode);
            // Additional translations for table or delete confirmation (if needed)
            ViewBag.Delete = _translationService.GetTranslationInd("Delete", (PageCode++).ToString(), languageCode);
            ViewBag.Action = _translationService.GetTranslationInd("Action", (PageCode++).ToString(), languageCode);
            //#endregion

            //ViewBag.CanExportPermission = await _accessControlService.HasPermissionAsync(4038, "Export");

            return View(model);
        }
        #endregion


        //#region GetAll
        //public async Task<IActionResult> GetAll()
        //{
        //    try
        //    {
        //        var data = await _addTeamService.GetAll();
        //        return Json(new { isSuccess = true, data });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        //#endregion


        //#region Create
        //[ValidateAntiForgeryToken]
        //[Permission("Create", "AddTeam")]
        //[HttpPost]
        //public async Task<IActionResult> Create(AddTeamVM model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            //var uniqueName = await _addTeamService.IsNameUniqueAsync(model.TeamName);
        //            //if (!uniqueName)
        //            //{
        //            //    return Json(new { isSuccess = false, message = "Name already exists!" });
        //            //}
        //            //_userInfoService.SetUserInfoBVM(model, User, HttpContext);
        //            await _addTeamService.AddNewTeam(model);
        //            return Json(new { isSuccess = true, message = "Saved Successfully.", lastId = model.TeamID });
        //        }
        //        var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;

        //        return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        //#endregion


        //#region Update
        //[ValidateAntiForgeryToken]
        //[Permission("Edit", "AddTeam")]
        //[HttpPost]
        //public async Task<IActionResult> Update(AddTeamVM model)
        //{
        //    try
        //    {
        //        if (ModelState.IsValid)
        //        {
        //            //_userInfoService.SetUserInfoBVM(model, User, HttpContext);
        //            await _addTeamService.UpdateNewTeam(model);
        //            return Json(new { isSuccess = true, message = "Updated Successfully." });
        //        }
        //        var errorMessage = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
        //        return Json(new { isSuccess = false, message = errorMessage ?? "Something went wrong." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        //#endregion


        //#region GetById
        //public async Task<IActionResult> GetById(int id)
        //{
        //    try
        //    {
        //        var result = await _addTeamService.GetNewTeam(id);
        //        if (result == null)
        //        {
        //            return Json(new { isSuccess = false, message = "No data found!" });
        //        }

        //        return Json(new { isSuccess = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        //#endregion


        //#region CheckNameUnique
        //[HttpPost]
        //public async Task<IActionResult> CheckNameUnique(string bankName)
        //{
        //    try
        //    {
        //        bool isUnique = await _addTeamService.IsNameUniqueAsync(bankName);
        //        if (!isUnique)
        //        {
        //            return Json("Name already exists.");
        //        }
        //        return Json(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json("Error occurred: " + ex.Message);
        //    }
        //}
        //#endregion


        //#region SoftDelete
        //[Permission("Delete", "AddTeam")]
        //[HttpPost]
        //public async Task<IActionResult> SoftDelete(BaseViewModel model, List<int> ids)
        //{
        //    try
        //    {
        //        if (ids == null || !ids.Any() || ids.Count == 0)
        //        {
        //            return Json(new { isSuccess = false, message = "No id selected to delete." });
        //        }
        //        _userInfoService.SetUserInfoBVM(model, User, HttpContext);
        //        var result = await _addTeamService.SoftDeleteAsync(model, ids);
        //        if (result == null)
        //        {
        //            return Json(new { isSuccess = false, message = "No id found to delete." });
        //        }

        //        return Json(new { isSuccess = true, message = "Deleted Successfully." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { isSuccess = false, message = ex.Message });
        //    }
        //}
        //#endregion


        //#region GenerateNextCode
        //[HttpGet]
        //public async Task<IActionResult> GenerateNextCode()
        //{
        //    var nextCode = await _addTeamService.GenerateNextCodeAsync();
        //    return Json(nextCode);
        //}
        //#endregion


        //#region ExportToExcel
        //[HttpGet]
        //public async Task<IActionResult> ExportToExcel()
        //{
        //    var data = await _addTeamService.GetReportData();

        //    var fileContent = await _addTeamService.GenerateExcelReportAsync(data);

        //    string fileName = $"TeamsReport_{DateTime.Now:ddMMMyyyy}.xlsx";

        //    return File(fileContent, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        //}
        //#endregion
    }
}
