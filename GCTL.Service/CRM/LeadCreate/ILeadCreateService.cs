using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.LeadCreate
{
    public interface ILeadCreateService
    {
        #region CRUD
       // Task<bool> AddAsync(CreateLeadVM model);
        Task<ReturnView> CreatePerson(CustomerVM customerVM);
        Task<ReturnView> CreateCompany(CompanyVM companyVM);
        Task<CommonReturnViewModel> CreateShippingAddress(ShippingVM shippingVM);
        Task<CommonReturnViewModel> CreateLead(LeadsVM leadsVM);
        Task<ReturnView> CreateBranch(BranchVM branchVM);
        Task<ReturnView> CreateWarehouse(WarehouseVM warehouseVM);
        #endregion
    }
}
