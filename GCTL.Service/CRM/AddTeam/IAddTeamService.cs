using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.AddTeam;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GCTL.Service.CRM.AddTeam
{
    public interface IAddTeamService
    {
        #region CRUD
        Task<ReturnView> CreateTeam(CreateTeamVM createTeamVM);
        //Task<List<CommonSelectVM>> GetEmployeesByIds(List<int> ids);
        #endregion

        #region Others
        //Task<int> GetLastIndexNumber();
        Task<TeamDetailsVM> GetIndividualTeamDetails(int id);
        Task<TeamEditVM> IndivudialIteamDetails(int id);
        Task<ReturnView> SetTeamHead(TeamHeadVM teamHeadVM);
        Task<ReturnDataView<SelectListItem>> GetEmployees(string search = "", int page = 1, int pageSize = 25);
        Task<List<TeamViewVM>> GetTeamListAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string sortColumn = "CreatedAt", string sortOrder = "asc");
        #endregion
    }
}
