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
        Task<CommonReturnViewModel> CreateLead(CustomerVM customerVM);
        Task<CommonReturnViewModel> UpdateLead(LeadsVM leadsVM);
        #endregion
    }
}
