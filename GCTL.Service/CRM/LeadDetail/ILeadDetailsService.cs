using GCTL.Core.ViewModels.CRM;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.LeadDetail
{
    public interface ILeadDetailsService
    {
        public Task<bool> CreateLeadActivateTypes();
        Task<bool> CreateLeadDeatil(LeadDetailsVM leadDetailsVM, string? fileLocation);
        Task<ReturnView> AddIsWon(IsWonVM isWonVM);
        Task<bool> UpdateLeadFieldValue(DetailsLeadUpdateVM detailsLeadUpdateVM);
    }
}
