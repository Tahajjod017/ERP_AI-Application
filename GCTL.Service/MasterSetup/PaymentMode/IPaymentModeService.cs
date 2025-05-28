using GCTL.Core.ViewModels.MasterSetup.PaymentModes;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.PaymentMode
{
    public interface IPaymentModeService
    {
        #region CRUD
        Task<bool> AddAsync(PaymentModeVM model);
        Task<bool> UpdateAsync(PaymentModeVM model);
        Task<PaymentModeVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<PaymentModeVM> GetByIdAsync(int id);
        Task<PaginationService<PaymentModes, PaymentModeVM>.PaginationResult<PaymentModeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PaymentModeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
