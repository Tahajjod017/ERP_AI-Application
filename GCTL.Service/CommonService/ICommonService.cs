using GCTL.Core.ViewModels;
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
        Task<List<CommonSelectVM>> GetOrganizations();

        Task<List<CommonSelectVM>> GetBranches();

        Task<List<CommonSelectVM>> GetDepartments();

        Task<List<CommonSelectVM>> GetEmpGroupedByDep();

        Task<List<CommonSelectVM>> GetShifts();

        Task<List<CommonSelectVM>> GetCompensation();

        Task<List<CommonSelectVM>> GetSpiralPatternTypes();

        Task<List<CommonSelectVM>> GetSpiralPatterns();

        Task<List<CommonSelectVM>> GetSpiralPatternsByOrgPatternType(int orgId, int? typeId);
        #endregion


        #region Load Paginated
        Task<PaginatedResult<CommonSelectVM>> SearchOrganizations(string search, int page = 1, int pageSize = 10);
        #endregion


        #region Load by OrganizationId
        Task<List<CommonSelectVM>> GetBranchesByOrgId(int? orgId);

        Task<List<CommonSelectVM>> GetDepartmentsByOrgId(int? orgId);

        Task<List<CommonSelectVM>> GetEmployeesByOrgId(int? orgId);

        Task<List<CommonSelectVM>> GetShiftsByOrgId(int? orgId);
        #endregion


        #region Load by OrganizationId, BranchId
        Task<List<CommonSelectVM>> GetEmployeesByOrgBraId(int? orgId, List<int>? branchIds);
        #endregion


        #region Load by OrganizationId, BranchId, DepartmentId
        Task<List<CommonSelectVM>> GetEmployeesByOrgBraDepId(int? orgId, List<int>? branchIds, List<int>? deptIds);
        #endregion


        #region GetWeekendByOrganization, GetWeekDaysByOrganization, GetEmployeeByWeekent
        Task<IEnumerable<object>> GetWeekendByOrganization(int id);
        Task<IEnumerable<object>> GetWeekDaysByOrganization(int id);
        #endregion
    }
}
