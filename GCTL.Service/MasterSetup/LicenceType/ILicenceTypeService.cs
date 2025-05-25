using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.MasterSetup.LicenceType;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.MasterSetup.LicenceType
{
    public interface ILicenceTypeService
    {
        #region CRUD
        Task<bool> AddAsync(LicenceTypeVM model);
        Task<bool> UpdateAsync(LicenceTypeVM model);
        Task<LicenceTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<LicenceTypeVM> GetByIdAsync(int id);
        Task<PaginationService<LicenceTypes, LicenceTypeVM>.PaginationResult<LicenceTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "LicenceTypeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
