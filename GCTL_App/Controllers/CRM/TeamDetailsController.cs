using GCTL.Core.ViewModels.CRM.AddTeam;
using GCTL.Service.CRM.AddTeam;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.CRM
{
    public class TeamDetailsController : BaseController
    {
        #region Service
        private readonly IAddTeamService _addTeamService;
        
        public TeamDetailsController(ITranslateService translateService, IUserProfileService userProfileService, IAddTeamService addTeamService) : base(translateService, userProfileService)
        {
            _addTeamService = addTeamService;
        }
        #endregion

        #region Index
        public IActionResult Index(int id)
        {
            return View();
        }
        #endregion

        #region Get Team Details
        public async Task<IActionResult> GetTeamDetails(int id) {
            var result = await _addTeamService.GetIndividualTeamDetails(id);
            return Ok(result);
        }
        #endregion

        #region SET Team Head
        public async Task<IActionResult> SetTeamHead([FromBody] TeamHeadVM teamHeadVM)
        {
            var result = await _addTeamService.SetTeamHead(teamHeadVM);
            return Ok(result);
        }
        #endregion

       
    }
}
