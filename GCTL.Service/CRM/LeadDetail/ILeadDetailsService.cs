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
        Task<bool> CreateLeadActivateTypes();
        Task<bool> CreateLeadDeatil(LeadDetailsVM leadDetailsVM, string? fileLocation);
        Task<ReturnView> AddIsWon(IsWonVM isWonVM);
        Task<ReturnView> UpdateLeadFieldValue(DetailsLeadUpdateVM detailsLeadUpdateVM);
        Task<LeadActivityResultVM> ActivityList(int id, string query, int page, string type);
        Task<ReturnView> RestoreLead(int id);
    }
}
