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
        foreach (var team in teamList)
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
                    // Convert logo to Base64 for email embedding
                    string base64Image = "";
                    if (!string.IsNullOrEmpty(member.LogoLink) && File.Exists(member.LogoLink))
                    {
                        string extension = Path.GetExtension(member.LogoLink)?.ToLower();
                        string mimeType = extension switch
                        {
                            ".png" => "image/png",
                            ".jpg" => "image/jpeg",
                            ".jpeg" => "image/jpeg",
                            ".gif" => "image/gif",
                            _ => "image/png"
                        };
                        byte[] imageBytes = File.ReadAllBytes(member.LogoLink);
                        string base64 = Convert.ToBase64String(imageBytes);
                        base64Image = $"data:{mimeType};base64,{base64}";
                    }

                    individualList.Add(new ActivityPDFModel
                    {
                        CompanyName = member.CompanyName ?? "",
                        CompanyAddress = member.CompanyAddress ?? "",
                        CompanyEmail = member.CompanyEmail ?? "",
                        CompanyPhone = member.CompanyPhone ?? "",
                        CompanyLogo = member.LogoLink,
                        CompnayLogoBase64 = base64Image,
                        TeamName = team.TeamName,
                        EmployeeName = member.LeadProjectTeamMemberName,
                        Email = "debanjandevelopment@gmail.com",
                        //Email = member.LeadProjectTeamMemberEmail, // ✅ actual email
                        IsTeamHead = member.IsTeamHead,
                        Activities = memberActivities,
                        TotalActivities = memberActivities.Count()
                    });
                }
            }

            // Team Leader
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
                    Email = "debanjandevelopment@gmail.com",
                    //Email = leader.LeadProjectTeamMemberEmail, // ✅ actual email
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

            // Admin PDFs (all team member activities)
            if (upcomingActivities.Any())
            {
                foreach (var adminUser in _teamList.AdminIds) // ✅ fetch admins from team
                {
                    adminList.Add(new ActivityPDFModel
                    {
                        CompanyName = adminUser.Organization?.OrganizationName ?? "YourCompany",
                        CompanyAddress = adminUser.Organization?.Address ?? "Your Address",
                        CompanyEmail = adminUser.Email ?? "",
                        TeamName = team.TeamName,
                        EmployeeName = $"",
                        Email = "debanjandevelopment@gmail.com", // ✅ send to admin
                        //Email = adminUser.Email, // ✅ send to admin
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
        }

        return (individualList, teamLeaderList, adminList);
    }
}
