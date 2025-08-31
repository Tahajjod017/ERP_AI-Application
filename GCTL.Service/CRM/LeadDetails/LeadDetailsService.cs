using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.LeadDetails
{
   
    public class LeadDetailsService : ILeadDetailsService
    {
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypesGenericRepository;
        private readonly IGenericRepository<GCTL.Data.Models.LeadDetails> _leadDetailsGenericRepository;
        public LeadDetailsService(IGenericRepository<GCTL.Data.Models.LeadDetails> leadDetailsGenericRepository, IGenericRepository<LeadActivityTypes> leadActivityTypesGenericRepository)
        {
            _leadActivityTypesGenericRepository = leadActivityTypesGenericRepository;
            _leadDetailsGenericRepository = leadDetailsGenericRepository;
        }

        public async Task<bool> CreateLeadActivateTypes()
        {
            var leadActivityObjs = await _leadActivityTypesGenericRepository.GetAllAsync();

            if (!leadActivityObjs.Any())
            {
                //var items = new[] {
                //    new { pririosValue = 0,priodText = "Phone"},
                //    new { pririosValue = 1,priodText = "Phone"}
                //    };

                var items = new List<LeadActivityTypes>
                {
                    new LeadActivityTypes{LeadActivityIcon = "fa-phone", LeadActivityName = "Call", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-envelope", LeadActivityName = "Email", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-handshake", LeadActivityName = "Offline Meeting", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-internet-explorer", LeadActivityName = "Online Meeting", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-quote-left", LeadActivityName = "Quatation", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-angles-right", LeadActivityName = "Rev. Quatation", CreatedAt = DateTime.UtcNow},
                };

                await _leadActivityTypesGenericRepository.AddRangeAsync(items);
                return true;
            }
            return false;
        }

        public async Task<bool> CreateLeadDeatil(LeadDetailsVM leadDetailsVM)
        {
            var leadObj = new GCTL.Data.Models.LeadDetails()
            {
                LeadID = leadDetailsVM.LeadID,
                ActivityDateTime = leadDetailsVM.ActivityDateTime,
                LeadActivityTypeID = leadDetailsVM.LeadActivityTypeID,
                ActivityNote = leadDetailsVM.ActivityNote,

                CreatedAt = DateTime.UtcNow,
                CreatedBy = leadDetailsVM.CreatedBy,
                LIP = leadDetailsVM.LIP,
                LMAC = leadDetailsVM.LMAC,
            };
            await _leadDetailsGenericRepository.AddAsync(leadObj);
            return true;
        }
    }
}
