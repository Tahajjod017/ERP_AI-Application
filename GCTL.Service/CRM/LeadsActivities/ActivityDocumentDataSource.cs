using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.CRM;
using Microsoft.EntityFrameworkCore;

public class ActivityDocumentDataSource
{
    private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
    private readonly TeamPageMainDto _teamList;

    public ActivityDocumentDataSource(IGenericRepository<LeadDetails> leadDetailsRepository, TeamPageMainDto teamList)
    {
        _leadDetailsRepository = leadDetailsRepository;
        _teamList = teamList;
    }

    public async Task<(List<ActivityPDFModel> individual, List<ActivityPDFModel> teamLeader, List<ActivityPDFModel> admin)> GetActivitiesByRole()
    {
        var individualList = new List<ActivityPDFModel>();
        var teamLeaderList = new List<ActivityPDFModel>();
        var adminList = new List<ActivityPDFModel>();

        var teamList = _teamList.Teams;

        // 🔹 We'll collect all activities from all teams here
        var allTeamActivities = new List<Activity>();
        var allTeamMembers = new List<(string TeamName, string MemberName, int? MemberId)>();

        foreach (var team in teamList)
        {
            var members = team.TeamMembers;
            var memberIds = members.Select(m => m.LeadProjectTeamMemberID).ToList();

            var upcomingActivities = await _leadDetailsRepository.AllActive()
                .Include(u => u.Lead).ThenInclude(l => l.Customer)
                .Include(u => u.Lead).ThenInclude(l => l.LeadOwner)
                .Include(u => u.LeadActivityType)
                .Include(u => u.Lead.LeadStatus)
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
                    LeadStage = u.Lead.LeadStatus.LeadStatusName ?? " ",
                    LeadPriority = u.Lead.ProbabilityPercentage.ToString() ?? " ",
                    LeadOwnerId = u.Lead.LeadOwnerID
                })
                .ToListAsync();

            // 🧩 Add this team's activities to the global admin pool
            allTeamActivities.AddRange(upcomingActivities);

            // Store member info (for sub-grouping later)
            foreach (var m in members)
            {
                allTeamMembers.Add((team.TeamName, m.LeadProjectTeamMemberName, m.LeadProjectTeamMemberID));
            }

            // 👇 Individual Members
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
                        CompanyEmail = member.CompanyEmail ?? "",
                        CompanyPhone = member.CompanyPhone ?? "",
                        CompanyLogo = member.LogoLink,
                        TeamName = team.TeamName,
                        EmployeeName = member.LeadProjectTeamMemberName,
                        Email = "debanjandevelopment@gmail.com", // or member email
                        IsTeamHead = member.IsTeamHead,
                        Activities = memberActivities,
                        TotalActivities = memberActivities.Count
                    });
                }
            }

            // 👇 Team Leader
            var leader = members.FirstOrDefault(m => m.IsTeamHead == true);
            if (leader != null && upcomingActivities.Any())
            {
                teamLeaderList.Add(new ActivityPDFModel
                {
                    CompanyName = leader.CompanyName ?? "",
                    CompanyAddress = leader.CompanyAddress ?? "",
                    CompanyEmail = leader.CompanyEmail ?? "",
                    CompanyPhone = leader.CompanyPhone ?? "",
                    CompanyLogo = leader.LogoLink,
                    TeamName = team.TeamName,
                    EmployeeName = leader.LeadProjectTeamMemberName,
                    Email = "debanjandevelopment@gmail.com", // or leader email
                    IsTeamHead = true,
                    Activities = upcomingActivities,
                    TotalActivities = upcomingActivities.Count,
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

        // 🔹 ADMIN SECTION (gets all team activities)
        if (allTeamActivities.Any())
        {

            foreach (var adminUser in _teamList.AdminIds)
            {
                adminList.Add(new ActivityPDFModel
                {
                    CompanyName = adminUser.Organization?.OrganizationName ?? "YourCompany",
                    CompanyAddress = adminUser.Organization?.Address ?? "Your Address",
                    CompanyEmail = adminUser.Email ?? "",
                    CompanyPhone = "",
                    TeamName = "All Teams",
                    EmployeeName = "Admin Overview",
                    Email = "debanjandevelopment@gmail.com", // or adminUser.Email
                    Activities = allTeamActivities,
                    TotalActivities = allTeamActivities.Count,
                    SubEmployees = allTeamMembers
                        .Select(m => new ActivityPDFModel
                        {
                            TeamName = m.TeamName,
                            EmployeeName = m.MemberName,
                            Activities = allTeamActivities
                                .Where(a => a.LeadOwnerId == m.MemberId)
                                .ToList()
                        }).ToList()
                });
            }
        }

        return (individualList, teamLeaderList, adminList);
    }

}
