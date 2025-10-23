using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CommonService
{
    public interface ICommonService
    {
        #region Load All
        Task<List<CommonSelectVM>> GetBranches();

        Task<List<CommonSelectVM>> GetDepartments();

        Task<List<CommonSelectVM>> GetEmpGroupedByDep();

        Task<List<CommonSelectVM>> GetShifts();

        Task<List<CommonSelectVM>> GetCompensation();

        Task<List<CommonSelectVM>> GetSpiralPatternTypes();

        Task<List<CommonSelectVM>> GetSpiralPatterns();

        Task<List<CommonSelectVM>> GetSpiralPatternsByOrgPatternType(int orgId, int? typeId);

        Task<List<CommonSelectVM>> GetBaseAccounts();

        Task<List<CommonSelectVM>> GetAccountClass();

        Task<List<CommonSelectVM>> GetMainAccount();

        Task<List<CommonSelectVM>> GetClassByBaseAccId(int baseAccountID);

        Task<List<CommonSelectVM>> GetMainAccByClassId(int classId);

        Task<List<CommonSelectVM>> GetSubAccByMainAccId(int? mainAccId);

        Task<List<CommonSelectVM>> GetTrxAccByMainAccIdSubAccId(int? mainAccId, int? subAccId);

        Task<List<CommonSelectVM>> GetSubAccByClassIdMainAccId(int? classId, int? mainAccId);

        Task<List<CommonSelectVM>> GetTrxAccByClassIdMainAccIdSubAccId(int? classId, int? mainAccId, int? subAccId);
        #endregion
                       

        #region Load Paginated
        Task<PaginatedResult<CommonSelectVM>> GetOrganizations(string search, int page = 1, int pageSize = 50);
        Task<PaginatedResult<CommonSelectVM>> SearchEmployees(string search, int page = 1, int pageSize = 50);
        #endregion


        #region Load by OrganizationId
        Task<List<CommonSelectVM>> GetBranchesByOrgId(int? orgId);

        Task<List<CommonSelectVM>> GetDepartmentsByOrgId(int? orgId);

        Task<List<CommonSelectVM>> GetShiftsByOrgId(int? orgId);
        #endregion


        #region Load by OrganizationId, BranchId, DepartmentId, Date
        Task<PaginatedResult<CommonSelectVM>> GetEmployeesByOrgBraDepId(int? orgId, List<int>? branchIds, List<int>? depIds, string? search, int? page = 1, int? pageSize = 50);
        Task<List<CommonSelectVM>> GetEmployeesByOrgDatesBraDepId(int? orgId, List<DateTime>? dates, List<int>? branchIds, List<int>? depIds, string? search, int? page = 1, int? pageSize = 50);
        Task<PaginatedResult<CommonSelectVM>> GetEmployeesByOrgDatesBraDepId2(int? orgId, List<DateTime>? dates, List<int>? branchIds, List<int>? depIds, string? search, int? page = 1, int? pageSize = 50);
        #endregion


        #region GetWeekendByOrganization, GetWeekDaysByOrganization, GetEmployeeByWeekent
        Task<IEnumerable<object>> GetWeekendByOrganization(int id);
        Task<IEnumerable<object>> GetWeekDaysByOrganization(int id);
        #endregion


        #region BodyTabs
        Task<List<MenuTab>> GetFinanceBodyTabsAsync();
        #endregion
    }
}
