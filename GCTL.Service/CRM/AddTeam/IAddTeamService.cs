using GCTL.Core.ViewModels.CRM;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace GCTL.Service.CRM.AddTeam
{
    public interface IAddTeamService
    {
        #region CRUD
        //Task<bool> AddNewTeam(AddTeamVM model);
        //Task<bool> UpdateNewTeam(AddTeamVM model);
        //Task<bool> SoftDeleteAsync(BaseViewModel model, List<int> ids);
        //Task<bool> DeleteNewTeam(int id);
        //Task<AddTeamVM> GetNewTeam(int id);
        //Task<IEnumerable<AddTeamVM>> GetAll();
        ////Task<List<TeamMembers>> GetByTeamIdAsync(int teamMemberId);
        //#endregion


        //#region Others
        public Task<IEnumerable<SelectListItem>> GetCustomers();
        //Task<bool> IsNameUniqueAsync(string name);
        //string GetLocalIP();
        //string GetMacAddress();
        //Task<string> GenerateNextCodeAsync();
        //#endregion


        //#region Reports
        //Task<List<AddTeamVM>> GetReportData();
        //Task<byte[]> GenerateExcelReportAsync(IEnumerable<AddTeamVM> data);
        #endregion
    }
}
