using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.PayrollManagements.PayRollOrganizationBenefitsType;
using GCTL.Core.ViewModels.PayrollManagements.PayrollPolicy.EmpAllowanceOrganization;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using GCTL.Service.PayRollManagements.EmpAllowanceTypeOrgaization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.PayRollManagements.PayRollOrgaBenefitsType
{
    public interface IPayRollOrgaBenefitsTypeService
    {
        Task<CommonReturnViewModel> Save(OrgaBenefitsTypeSaveVM model);
        Task<CommonReturnViewModel> Update(OrgaBenefitsTypeSaveVM model);
        Task<List<CommonSelectVM>> GetAllEmploye();
        Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<OrgaBenefitsTypeSaveVM> GetByIdAsync(int id);
        Task<PaginationService<BenefitTypes, OrgaBenefitsTypeGetAllVM>.PaginationResult<OrgaBenefitsTypeGetAllVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "", string sortOrder = "desc");
    }
}
