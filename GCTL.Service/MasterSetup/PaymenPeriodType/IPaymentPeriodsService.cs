using GCTL.Core.ViewModels.MasterSetup.PaymenPeriodTypes;
using GCTL.Core.ViewModels;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using GCTL.Core.Helpers;

namespace GCTL.Service.MasterSetup.PaymenPeriodType
{
    public interface IPaymentPeriodsService
    {
        #region CRUD
        Task<bool> AddAsync(PaymentPeriodsVM model);
        Task<bool> UpdateAsync(PaymentPeriodsVM model);
        Task<PaymentPeriodsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<PaymentPeriodsVM> GetByIdAsync(int id);
        Task<PaginationService<PaymentPeriodTypes, PaymentPeriodsVM>.PaginationResult<PaymentPeriodsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "PaymentPeriodTypeID", string sortOrder = "desc");
        #endregion


        #region Others
        Task<bool> IsNameUniqueAsync(string name);
        #endregion
    }
}
