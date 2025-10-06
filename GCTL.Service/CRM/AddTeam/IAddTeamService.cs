using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.AddTeam;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GCTL.Service.CRM.AddTeam
{
    public interface IAddTeamService
    {
        #region CRUD
        Task<ReturnView> CreateTeam(CreateTeamVM createTeamVM);
        //Task<bool> AddNewTeam(AddTeamVM model);
        //Task<bool> UpdateNewTeam(AddTeamVM model);
        //Task<bool> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        //Task<bool> DeleteNewTeam(int id);
        //Task<AddTeamVM> GetNewTeam(int id);
        //Task<IEnumerable<AddTeamVM>> GetAll();
        ////Task<List<TeamMembers>> GetByTeamIdAsync(int teamMemberId);
        #endregion


        #region Others
        Task<int> GetLastIndexNumber();
        Task<ReturnDataView<SelectListItem>> GetEmployees(string search = "", int page = 1, int pageSize = 25);
        Task<List<TeamViewVM>> GetTeamListAsync(int pageNumber = 1, int pageSize = 10, string? searchTerm = null, string sortColumn = "CreatedAt", string sortOrder = "asc");
        //Task<bool> IsNameUniqueAsync(string name);
        //string GetLocalIP();
        //string GetMacAddress();
        //Task<string> GenerateNextCodeAsync();
        #endregion


        //#region Reports
        //Task<List<AddTeamVM>> GetReportData();
        //Task<byte[]> GenerateExcelReportAsync(IEnumerable<AddTeamVM> data);
        //#endregion
    }
}
