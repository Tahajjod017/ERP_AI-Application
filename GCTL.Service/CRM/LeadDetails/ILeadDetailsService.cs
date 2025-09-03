using GCTL.Core.ViewModels.CRM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.LeadDetails
{
    public interface ILeadDetailsService
    {
        public Task<bool> CreateLeadActivateTypes();
        Task<bool> CreateLeadDeatil(LeadDetailsVM leadDetailsVM, string? fileLocation);
    }
}
