using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;

namespace GCTL.Service.Employees.EmployeeStatus.Promotion
{
    public interface IPromotionService
    {
        Task<CommonReturnViewModel> ApprovePromotionAsync(PromotionActionModel action);
        Task<List<PromotionApproveViewModel>> GetAllPromotionPendingList();
        Task<object> GetFilteredPromotionsAsync(PromotionFilterModel filter, string imgLink);
        Task<object> GetFilteredApprovePromotionsAsync(PromotionFilterModel filter, string imgLink);
        Task GetPagedPromotionListAsync(PromotionListFilterViewModel filters);
        Task<PromotionApproveViewModel> GetPendingPromotionDetailsByID(int id);
        Task<CommonReturnViewModel> SaveAsync(PromotionViewModel model);


    }
}
