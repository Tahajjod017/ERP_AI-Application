using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.PayRollSettings;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization
{
    public interface IEmpAllowanceTypeOrganizationService
    {
        Task<CommonReturnViewModel> SaveAsync(EmpAllowanceTypeOrganizationSaveVM EntityVM);
        Task<CommonReturnViewModel> UpdateAsync(EmpAllowanceTypeOrganizationSaveVM EntityVM);
        #region CRUD
        Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<EmpAllowanceTypeOrganizationSaveVM> GetByIdAsync(int id);
        Task<PaginationService<EmployeeAllowanceTypes, GetAllTable>.PaginationResult<GetAllTable>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "", string sortOrder = "desc");
        Task<CommonSelectVM> SelectAsync(int  id);
        #endregion

    }

    public class GetAllTable
    {
        public int EmployeeAllowanceTypeID { get; set; }
        public string? EmployeeAllowanceTypeName { get; set; }
        public string? OrganizationName { get; set; }
    }
}
