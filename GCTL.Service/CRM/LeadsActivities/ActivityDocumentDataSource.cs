using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM;
using Microsoft.EntityFrameworkCore;

public class ActivityDocumentDataSource
{
    private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
    private readonly List<TeamDto> _teamList;

    public ActivityDocumentDataSource(IGenericRepository<LeadDetails> leadDetailsRepository, List<TeamDto> teamList)
    {
        _leadDetailsRepository = leadDetailsRepository;
        _teamList = teamList;
    }

    public async Task<(List<ActivityPDFModel> individual, List<ActivityPDFModel> teamLeader, List<ActivityPDFModel> admin)> GetActivitiesByRole()
    {
        var individualList = new List<ActivityPDFModel>();
        var teamLeaderList = new List<ActivityPDFModel>();
        var adminList = new List<ActivityPDFModel>();

        foreach (var team in _teamList)
        {
            var members = team.TeamMembers;
            var memberIds = members.Select(m => m.LeadProjectTeamMemberID).ToList();

            var upcomingActivities = await _leadDetailsRepository.AllActive()
                .Include(u => u.Lead).ThenInclude(l => l.Customer)
                .Include(u => u.Lead).ThenInclude(l => l.LeadOwner)
                .Include(u => u.LeadActivityType)
                .Where(u => u.Lead.LeadOwnerID.HasValue && memberIds.Contains(u.Lead.LeadOwnerID.Value)
                            && u.ActivityDateTime >= DateTime.UtcNow)
                .OrderBy(u => u.ActivityDateTime)
                .Select(u => new Activity
                {
                    LeadName = u.Lead.LeadName,
                    ActivityType = u.LeadActivityType.LeadActivityName,
                    ActivityNote = u.ActivityNote,
                    ActivityDateTime = u.ActivityDateTime,
                    CustomerName = u.Lead.Customer.FullName,
                    LeadOwner = u.Lead.LeadOwner.FirstName + " " + u.Lead.LeadOwner.LastName,
                    LeadOwnerId = u.Lead.LeadOwnerID
                })
                .ToListAsync();

            foreach (var member in members)
            {
                var memberActivities = upcomingActivities
                    .Where(a => a.LeadOwnerId == member.LeadProjectTeamMemberID)
                    .ToList();

                if (memberActivities.Any() && member.IsTeamHead != true)
                {
                    individualList.Add(new ActivityPDFModel
                    {
                        CompanyName = member.CompanyName ?? "",
                        CompanyAddress = member.CompanyAddress ?? "",
                        CompanyLogo = member.LogoLink,
                        TeamName = team.TeamName,
                        EmployeeName = member.LeadProjectTeamMemberName,
                        Email = "debanjandevelopment@gmail.com",
                        //Email = member.LeadProjectTeamMemberEmail, // send to real email
                        IsTeamHead = member.IsTeamHead,
                        Activities = memberActivities,
                        TotalActivities = memberActivities.Count()
                    });
                }
            }

            var leader = members.FirstOrDefault(m => m.IsTeamHead == true);
            if (leader != null && upcomingActivities.Any())
            {
                teamLeaderList.Add(new ActivityPDFModel
                {
                    CompanyName = leader.CompanyName ?? "",
                    CompanyAddress = leader.CompanyAddress ?? "",
                    CompanyLogo = leader.LogoLink,
                    TeamName = team.TeamName,
                    EmployeeName = leader.LeadProjectTeamMemberName,
                    Email = "debanjandevelopment@gmail.com",
                    //Email = leader.LeadProjectTeamMemberEmail,
                    IsTeamHead = true,
                    Activities = upcomingActivities, // all team activities
                    TotalActivities = upcomingActivities.Count(),
                    SubEmployees = members
                        .Select(m => new ActivityPDFModel
                        {
                            EmployeeName = m.LeadProjectTeamMemberName,
                            Activities = upcomingActivities
                                .Where(a => a.LeadOwnerId == m.LeadProjectTeamMemberID)
                                .ToList()
                        }).ToList()
                });
            }

            // Admin PDFs (all team member activities, if any)
            if (upcomingActivities.Any())
            {
                adminList.Add(new ActivityPDFModel
                {
                    CompanyName = "YourCompany",
                    CompanyAddress = "Your Address",
                    TeamName = team.TeamName,
                    Activities = upcomingActivities,
                    TotalActivities = upcomingActivities.Count(),
                    SubEmployees = members
                        .Select(m => new ActivityPDFModel
                        {
                            EmployeeName = m.LeadProjectTeamMemberName,
                            Activities = upcomingActivities
                                .Where(a => a.LeadOwnerId == m.LeadProjectTeamMemberID)
                                .ToList()
                        }).ToList()
                });
            }
        }

        return (individualList, teamLeaderList, adminList);
    }
}
