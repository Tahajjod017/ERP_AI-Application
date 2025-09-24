using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.CRM.LeadsActivities
{

    public class ActivityDocumentDataSource
    {
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;

        public ActivityDocumentDataSource(IGenericRepository<LeadDetails> leadDetailsRepository)
        {
            _leadDetailsRepository = leadDetailsRepository;
        }

        public async Task<ActivityPDFModel> GetUpCommingActivity()
        {
            var upcomingActivities = await _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= DateTime.UtcNow)
                    .OrderBy(u => u.ActivityDateTime)
                    .Select(u => new Activity
                    {
                        LeadName = u.Lead.LeadName,
                        ActivityType = u.LeadActivityType.LeadActivityName,
                        ActivityNote = u.ActivityNote,
                        ActivityDateTime = u.ActivityDateTime,
                        CreatedAt = u.CreatedAt,
                        CustomerName = u.Lead.Customer.FullName,
                        LeadStage = u.Lead.LeadStatus.LeadStatusName,
                        LeadPriority = u.Lead.Priority.PriorityName,
                        LeadSource = u.Lead.LeadStatus.LeadStatusName,
                        LeadProbability = u.Lead.ProbabilityPercentage,
                        LeadOwner = u.Lead.LeadOwner.FirstName + " " + u.Lead.LeadOwner.LastName,
                    }).ToListAsync();

            var result = new ActivityPDFModel()
            {
                CompanyName = "Google",
                CompanyAddress = "Satk",
                CompanyLogo = "",
                Activities = upcomingActivities
            };

            return result;
        }

    }
}
