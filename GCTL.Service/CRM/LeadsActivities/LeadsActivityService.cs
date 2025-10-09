using Bogus.DataSets;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.FileHandler;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;
using QuestPDF.Fluent;


namespace GCTL.Service.CRM.LeadsActivities
{
    public class LeadsActivityService : ILeadsActivityService
    {
        #region services
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
        private readonly IPdfFileHandler _pdfFileHandlerService;
        public readonly IGenericRepository<LeadProjectTeams> _leadProjectTeamsRepository;

        public LeadsActivityService(IGenericRepository<LeadDetails> leadDetailsRepository, IPdfFileHandler pdfFileHandlerService, IGenericRepository<LeadProjectTeams> leadProjectTeamsRepository)
        {
            _leadDetailsRepository = leadDetailsRepository;
            _pdfFileHandlerService = pdfFileHandlerService;
            _leadProjectTeamsRepository = leadProjectTeamsRepository;
        }
        #endregion

        #region GetUpcomingActivityList
        public async Task<ReturnDataView<LeadDetailsDTO>> GetUpcomingActivityList(
            int page,
            int itemPerPage,
            string search,
            string sort,
            string direction,
            string dateRange,
            int? userID,
            int? CustomerTypeID, 
            string? LeadStatusID,
            int? ActivityTypeID
            )   // current user ID
        {
            try
            {
                if (!userID.HasValue)
                {
                    return new ReturnDataView<LeadDetailsDTO>
                    {
                        success = true,
                        message = "No data available for null user",
                        data = [],
                        totalItem = 0,
                        totalNowItem = 0,
                        totalSearchItem = 0
                    };
                }

                int skip = (page - 1) * itemPerPage;
                var now = DateTime.UtcNow;
                var today = DateTime.Today;

                var baseQuery = _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= today && u.CreatedBy == userID.Value);

                var totalItem = await baseQuery.CountAsync();

                var totalNowItem = await _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= now && u.CreatedBy == userID.Value)
                    .CountAsync();

                var query = _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= now && u.CreatedBy == userID.Value);

                if (!string.IsNullOrEmpty(dateRange))
                {
                    var dates = dateRange.Split(" to ", StringSplitOptions.RemoveEmptyEntries);

                    if (dates.Length == 1 &&
                        DateTime.TryParseExact(dates[0].Trim(), "dd/MM/yy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var singleDate))
                    {
                        query = query.Where(r => r.ActivityDateTime == singleDate.Date &&
                                                 r.ActivityDateTime >= now);
                    }
                    else if (dates.Length == 2 &&
                             DateTime.TryParseExact(dates[0].Trim(), "dd/MM/yy",
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 System.Globalization.DateTimeStyles.None, out var startDate) &&
                             DateTime.TryParseExact(dates[1].Trim(), "dd/MM/yy",
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 System.Globalization.DateTimeStyles.None, out var endDate))
                    {
                        query = query.Where(r => r.ActivityDateTime >= startDate.Date &&
                                                 r.ActivityDateTime <= endDate.Date &&
                                                 r.ActivityDateTime >= now);
                    }
                }

                if (!string.IsNullOrEmpty(search))
                {
                    if (DateTime.TryParse(search, out var searchDate))
                    {
                        query = query.Where(u =>
                            EF.Functions.Like(u.ActivityNote, $"%{search}%") ||
                            u.ActivityDateTime == searchDate.Date ||
                            u.CreatedAt == searchDate.Date
                        );
                    }
                    else
                    {
                        query = query.Where(u =>
                            EF.Functions.Like(u.ActivityNote, $"%{search}%")
                        );
                    }
                }

                if (CustomerTypeID.HasValue && CustomerTypeID.Value > 0)
                {
                    query = query.Where(u => u.Lead.Customer.CustomerAddresses.First().AddressTypeID == CustomerTypeID.Value);
                }

                if (!string.IsNullOrEmpty(LeadStatusID))
                {
                    query = query.Where(u => u.Lead.LeadStatus.LeadStatusName== LeadStatusID);
                }
                if (ActivityTypeID > 0)
                {
                    query = query.Where(u => u.LeadActivityTypeID == ActivityTypeID);
                }


                var totalSearchItem = await query.CountAsync();

                query = direction?.ToLower() == "asc"
                    ? query.OrderBy(u => EF.Property<object>(u, sort ?? "ActivityDateTime"))
                    : query.OrderByDescending(u => EF.Property<object>(u, sort ?? "ActivityDateTime"));

                var data = await query.Skip(skip).Take(itemPerPage).Select(u => new LeadDetailsDTO
                {
                    LeadActivityID = u.LeadDetailID,
                    LeadActivityType = u.LeadActivityType.LeadActivityName,
                    ActivityNote = u.ActivityNote,
                    ActivityDateTime = u.ActivityDateTime,
                    CreatedAt = u.CreatedAt,
                    LeadName = u.Lead.LeadName,
                    LeadID = u.LeadID,
                    File  = u.FileLink,
                    CustomerName = u.Lead.Customer.FullName,
                    LeadStage =  u.Lead.LeadStatus.LeadStatusName,
                    LeadPriority = u.Lead.Priority.PriorityName,
                    LeadSource = u.Lead.LeadStatus.LeadStatusName,
                    LeadProbability = u.Lead.ProbabilityPercentage,
                    LeadOwner = u.Lead.LeadOwner.FirstName + " " + u.Lead.LeadOwner.LastName,

                }).ToListAsync();

                return new ReturnDataView<LeadDetailsDTO>
                {
                    success = true,
                    message = totalSearchItem > 0 ? "Upcoming List Data successfully loaded" : "No records found",
                    data = data,
                    totalItem = totalItem,
                    totalNowItem = totalNowItem,
                    totalSearchItem = totalSearchItem
                   
                };

            }
            catch (Exception ex)
            {
                return new ReturnDataView<LeadDetailsDTO>
                {
                    success = false,
                    message = $"Something went wrong: {ex.Message}"
                };
            }
        }
        #endregion

        #region Generate PDF
        public async Task<bool> GenerateAndSendEmployeePDFsAsync()
        {
            try
            {
                var allTeam = await _leadProjectTeamsRepository.AllActive()
                    .Include(t => t.LeadProjectTeamMembers)
                    .ThenInclude(m => m.Employee)
                    .Select(l => new TeamDto
                    {
                        TeamID = l.LeadProjectTeamID,
                        TeamName = l.LeadProjectTeamName,
                        TeamMembers = l.LeadProjectTeamMembers
                            .Select(m => new TeamMemberDto
                            {
                                LeadProjectTeamMemberID = m.EmployeeID,
                                LeadProjectTeamMemberName = $"{m.Employee.FirstName} {m.Employee.LastName}",
                                LeadProjectTeamMemberEmail = m.Employee.Email,
                                IsTeamHead = m.IsTeamHead.GetValueOrDefault() 
                            })
                            .ToList()
                    })
                    .ToListAsync();

                var activityDataSource = new ActivityDocumentDataSource(_leadDetailsRepository, allTeam);
                var result = await activityDataSource.GetActivitiesByRole();

                List<ActivityPDFModel> individualList = result.individual;
                List<ActivityPDFModel> teamLeaderList = result.teamLeader;
                List<ActivityPDFModel> adminList = result.admin;

                foreach (var employeeModel in individualList)
                {
                    if (string.IsNullOrEmpty(employeeModel.Email))
                        continue;

                    if (employeeModel.Activities == null || !employeeModel.Activities.Any())
                        continue;

                    await SendPdfEmail(employeeModel);
                }

                foreach (var leaderModel in teamLeaderList)
                {
                    if (string.IsNullOrEmpty(leaderModel.Email))
                        continue;

                    if ((leaderModel.Activities == null || !leaderModel.Activities.Any()) &&
                        (leaderModel.SubEmployees == null || !leaderModel.SubEmployees.Any(a => a.Activities != null && a.Activities.Any())))
                        continue;

                    await SendPdfEmail(leaderModel);
                }

                string adminEmail = "debanjandevelopment@gmail.com";
                foreach (var adminModel in adminList)
                {
                    if ((adminModel.Activities == null || !adminModel.Activities.Any()) &&
                        (adminModel.SubEmployees == null || !adminModel.SubEmployees.Any(a => a.Activities != null && a.Activities.Any())))
                        continue; // skip if nothing

                    await SendPdfEmail(adminModel, adminEmail);
                }

                Console.WriteLine("✅ All activity reports sent successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Failed to generate and send PDFs: {ex.Message}");
                return false;
            }
        }

        private async Task SendPdfEmail(ActivityPDFModel model, string? emailOverride = null)
        {
            var document = new ActivityDocument(
                new List<ActivityPDFModel> { model },
                _pdfFileHandlerService
            );

            var pdfBytes = document.GeneratePdf();

            var emailService = new EmailService1();
            string recipientEmail = emailOverride ?? model.Email;

            string subject = $"Upcoming Activity Report - {model.EmployeeName ?? "Admin"}";
            string body = $@"
                <h2>Dear {model.EmployeeName ?? "Admin"},</h2>
                <p>Please find attached your upcoming activities report.</p>
                <p>Regards,<br/>CRM System</p>";

            await emailService.SendEmailAsync(
                recipientEmail,
                subject,
                body,
                pdfBytes,
                $"{model.EmployeeName ?? "Admin"}_ActivityReport.pdf"
            );

            Console.WriteLine($"📧 Email sent successfully to {recipientEmail}");
        }

        #endregion

    }
}
