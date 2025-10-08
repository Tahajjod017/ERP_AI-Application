using GCTL.Core.ViewModels.CRM;
using GCTL.Service.CommonService;
using GCTL.Service.CRM.AddTeam;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using GCTL_App.ViewModels.AddTeam;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class AddTeamsController : BaseController
    {
        #region Services & Repositories

        private readonly ITranslateService _translationService;
        private readonly IAddTeamService _addTeamService;
        private readonly ICommonService _commonService;

        public AddTeamsController(ITranslateService translateService, IAddTeamService addTeamService, IUserProfileService userProfileService, ICommonService commonService) : base(translateService, userProfileService)
        {
            _translationService = translateService;
            _addTeamService = addTeamService;
            _commonService = commonService;
        }

        #endregion

        #region Index
        //[Permission("View", "AddTeam")]
        public async Task<IActionResult> Index()
        {
            AddTeamPageVM model = new AddTeamPageVM();

            return View(model);
        }
        #endregion

        #region getLastIndexNumber
        //[HttpGet]
        //public async Task<IActionResult> GetLastIndexNumber()
        //{
        //    var nextIndex = await _addTeamService.GetLastIndexNumber();
        //    return Ok(nextIndex);
        //}
        #endregion

        #region getEmployeeList
        [Route("/AddTeams/GetEmployeeList")]
        [HttpGet]
        //public async Task<IActionResult> GetEmployeeList(string search = "", int page = 1, int pageSize = 25)
        //{
        //    var employeeList = await _addTeamService.GetEmployees(search, page, pageSize);
        //    return Ok(employeeList);
        //}
        public async Task<IActionResult> SearchOrganizations(string search, int page = 1, int pageSize = 50)
        {
            var result = await _commonService.SearchEmployees(search, page, pageSize);

            return Json(new
            {
                items = result.Items.Select(x => new {
                    value = x.Id,
                    label = x.Name,
                    group = x.GroupName
                }),
                hasMore = result.HasMore
            });
        }
        #endregion

        #region Create Team
        [HttpPost]
        public async Task<IActionResult> CreateTeam([FromForm] CreateTeamVM createTeamVM)
        {
            if (createTeamVM.TeamID == 0)
            {
                var result = await _addTeamService.CreateTeam(createTeamVM);
                return Ok(result);
            }
            else
            {
                var result = await _addTeamService.UpdateTeam(createTeamVM);
                return Ok(result);
            }
                
        }
        #endregion


        #region get Team List
        public async Task<IActionResult> GetTeamList(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string sortColumn = "CreatedAt", string sortOrder = "asc")
        {
            var result = await _addTeamService.GetTeamListAsync(pageNumber, pageSize, searchTerm, sortColumn, sortOrder);
            return Ok(result);
        }
        #endregion

        #region IndivudialIteamDetails  
        public async Task<IActionResult> GetIndivudialTeamDetails(int id)
        {
            var result = await _addTeamService.IndivudialIteamDetails(id);
            return Ok(result);
        }
        #endregion


        //[HttpGet]
        //public async Task<IActionResult> GetEmployeesByIds([FromQuery] List<int> ids)
        //{
        //    var result = await _addTeamService.GetEmployeesByIds(ids);

        //    return Json(result.Select(x => new {
        //        value = x.Id,
        //        label = x.Name,
        //        group = x.GroupName
        //    }));
        //}

    }
}
