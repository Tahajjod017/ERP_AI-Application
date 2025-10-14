using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.AttendanceManagement;
using GCTL.Service.FileHandler;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;


namespace GCTL.Service.CRM.LeadsActivities
{
    public class LeadsActivityService : ILeadsActivityService
    {
        #region services
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
        private readonly IPdfFileHandler _pdfFileHandlerService;
        public readonly IGenericRepository<LeadProjectTeams> _leadProjectTeamsRepository;
        private readonly IEmailService _emailService;
        public readonly IGenericRepository<Organization> _organizationRepository;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadsActivityService(
            IGenericRepository<LeadDetails> leadDetailsRepository,
            IPdfFileHandler pdfFileHandlerService,
            IGenericRepository<LeadProjectTeams> leadProjectTeamsRepository,
            IEmailService emailService, RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, IGenericRepository<Organization> organizationRepository)
        {
            _leadDetailsRepository = leadDetailsRepository;
            _pdfFileHandlerService = pdfFileHandlerService;
            _leadProjectTeamsRepository = leadProjectTeamsRepository;
            _emailService = emailService;
            _roleManager = roleManager;
            _userManager = userManager;
            _organizationRepository = organizationRepository;
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

        public async Task<List<ApplicationUser>> GetAdminUsersAsync(int orgId)
        {
            var adminRole = await _roleManager.Roles
                .FirstOrDefaultAsync(r => r.OrganizationID == orgId && r.Name == "1_1_ADMIN");

            if (adminRole == null)
                return new List<ApplicationUser>();

            var users = await _userManager.GetUsersInRoleAsync(adminRole.Name);
            return users.ToList();
        }


        #region Generate PDF
        public async Task<bool> GenerateAndSendEmployeePDFsAsync(string wwwRootPath)
        {
            try
            {
                var teams = await _leadProjectTeamsRepository.AllActive()
                    .Include(t => t.LeadProjectTeamMembers)
                        .ThenInclude(m => m.Employee)
                            .ThenInclude(e => e.EmployeeOfficeInfoEmployee)
                                .ThenInclude(o => o.Organization)
                    .Select(l => new TeamDto
                    {
                        TeamID = l.LeadProjectTeamID,
                        TeamName = l.LeadProjectTeamName,
                        TeamMembers = l.LeadProjectTeamMembers
                            .Select(m => new TeamMemberDto
                            {
                                ComapanyID = m.Employee.EmployeeOfficeInfoEmployee
                                    .Select(x => x.Organization.OrganizationID)
                                    .FirstOrDefault(),
                                CompanyName = m.Employee.EmployeeOfficeInfoEmployee
                                    .Select(x => x.Organization.OrganizationName)
                                    .FirstOrDefault(),
                                CompanyAddress = m.Employee.EmployeeOfficeInfoEmployee
                                    .Select(x => x.Organization.Address)
                                    .FirstOrDefault(),
                                CompanyEmail = m.Employee.EmployeeOfficeInfoEmployee
                                    .Select(x => x.Organization.EmailAddress)
                                    .FirstOrDefault(),
                                CompanyPhone = m.Employee.EmployeeOfficeInfoEmployee
                                    .Select(x => x.Organization.Phone)
                                    .FirstOrDefault(),
                                LogoLink = !string.IsNullOrEmpty(m.Employee.EmployeeOfficeInfoEmployee
                                    .Select(x => x.Organization.LogoLink).FirstOrDefault())
                                    ? Path.Combine(wwwRootPath, "images", m.Employee.EmployeeOfficeInfoEmployee
                                        .Select(x => x.Organization.LogoLink).FirstOrDefault() ?? "")
                                    : "",
                                LeadProjectTeamMemberID = m.EmployeeID,
                                LeadProjectTeamMemberName = $"{m.Employee.FirstName} {m.Employee.LastName}",
                                LeadProjectTeamMemberEmail = m.Employee.Email,
                                IsTeamHead = m.IsTeamHead.GetValueOrDefault()
                            })
                            .ToList()
                    }).ToListAsync();

                // Get first organization ID from first team member
                int firstOrgId = teams
                    .SelectMany(t => t.TeamMembers)
                    .Select(tm => tm.ComapanyID)
                    .FirstOrDefault() ?? 0; // 0 if no members

                var organization = await _organizationRepository.AllActive().Where(t => t.OrganizationID == firstOrgId).FirstOrDefaultAsync();
                var TeamRowData = new TeamPageMainDto
                {
                    Teams = teams,
                    AdminIds = await GetAdminUsersAsync(firstOrgId),
                    OrganizationID = organization.OrganizationID,
                    OrganizationEmail = organization.EmailAddress,
                    OrganizationName = organization.OrganizationName,
                    OrganizationAddress = organization.Address,
                    OrganizationPhone = organization.Phone,
                    LogoLink = organization.LogoLink,
                };

                var activityDataSource = new ActivityDocumentDataSource(_leadDetailsRepository, TeamRowData);
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

                //string adminEmail = "debanjandevelopment@gmail.com";
                //foreach (var adminModel in adminList)
                //{
                //    if ((adminModel.Activities == null || !adminModel.Activities.Any()) &&
                //        (adminModel.SubEmployees == null || !adminModel.SubEmployees.Any(a => a.Activities != null && a.Activities.Any())))
                //        continue; // skip if nothing

                //    await SendPdfEmail(adminModel, adminEmail);
                //}
                // 🔹 Fetch all admin users dynamically for the organization
                var adminUsers = await GetAdminUsersAsync(firstOrgId);

                if (adminUsers != null && adminUsers.Any())
                {
                    foreach (var admin in adminUsers)
                    {
                        foreach (var adminModel in adminList)
                        {
                            if ((adminModel.Activities == null || !adminModel.Activities.Any()) &&
                                (adminModel.SubEmployees == null || !adminModel.SubEmployees.Any(a => a.Activities != null && a.Activities.Any())))
                                continue;

                            // Send to each admin, personalized by name
                            adminModel.EmployeeName = $"";
                            await SendPdfEmail(adminModel);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ No admin users found for this organization.");
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

            //var emailService = new EmailService1();
            string recipientEmail = emailOverride ?? model.Email;

            // 🔹 Build the table rows using foreach
            var activityRows = new StringBuilder();

            if (model.Activities != null && model.Activities.Any())
            {
                int index = 1;
                foreach (var activity in model.Activities)
                {
                    activityRows.AppendLine($@"
                <tr>
                    <td style=""text-align:center;"">{index}</td>
                    <td>{activity.LeadName}</td>
                    <td>{activity.CustomerName}</td>
                    <td>{activity.ActivityType}</td>
                    <td>{activity.ActivityDateTime:dd-MMM-yyyy hh:mm tt}</td>
                    <td>{activity.ActivityNote}</td>
                    <td>{activity.LeadOwner}</td>
                </tr>");
                    index++;
                }
            }
            else
            {
                activityRows.AppendLine("<tr><td colspan='7' style='text-align:center;'>No activities found</td></tr>");
            }

            string formattedAddress = string.Empty;

            if (model.CompanyAddress != null && !string.IsNullOrWhiteSpace(model.CompanyAddress))
            {
                //var parts = orgainfo.Address.Split(',');
                string addressWithCommas = model.CompanyAddress.Replace("\r\n", ",")
                                           .Replace("\n", ",")
                                           .Replace("\r", ",");

                // Split by comma
                var parts = model.CompanyAddress.Split(',');
                formattedAddress = string.Join("<br>", parts.Select(p => p.Trim()));
            }
            else
            {
                formattedAddress = "No address available";
            }

            string subject = $"Upcoming Activity Report - {model.EmployeeName ?? "Admin"}";
            string body = $@"
              <!DOCTYPE html>
                 <html>
                 <head>
                     <meta charset=""UTF-8"">
                     <title>HR Leave Request</title>
                     <style>
                         /* Reset styles */
                         body, table, td, p, a, li, h1, h2 {{
                             -webkit-text-size-adjust: 100%;
                             -ms-text-size-adjust: 100%;
                             margin: 0;
                             padding: 0;
                         }}
                         body {{
                             font-family: Arial, sans-serif;
                             font-size: 14px;
                             line-height: 20px;
                             color: #333333;
                             background-color: #f4f4f4;
                             padding: 20px;
                         }}
                         table {{
                             border-collapse: collapse;
                         }}

                         /* Main container */
                         .email-container {{
                             width: 800px;
                             margin: auto;
                             background-color: #ffffff;
                             border: 1px solid #e0e0e0;
                             border-radius: 8px;
                             overflow: hidden;
                         }}

                         /* Header */
                         .header-bg {{
                             position: relative;
                             background-color: #3252ff;
                             background-image: linear-gradient(to bottom right, #080301 120px, transparent 0);
                             background-repeat: no-repeat;
                             background-size: 140px 140px;
                             padding: 25px 30px;
                             color: #ffffff;
                         }}
                         .header-bg img {{
                             display: block;
                             border: 0;
                             outline: none;
                             text-decoration: none;
                             max-width: 200px;
                             height: auto;
                         }}
                         .header-bg td {{
                             font-size: 13px;
                             line-height: 18px;
                             text-align: right;
                             color: #ffffff;
                         }}

                         /* Content */
                         .content {{
                             padding: 20px 30px;
                         }}
                         .content p {{
                             margin-bottom: 10px;
                         }}
                         .content h2 {{
                             font-size: 18px;
                             margin-bottom: 10px;
                             color: #3252ff;
                         }}

                         /* Tables for info */
                         .info-table {{
                             width: 100%;
                             width: 400px;
                             border: 1px solid #e0e0e0;
                             border-radius: 5px;
          
                         }}
                         .info-table th, .info-table td {{
                             padding: 10px;
                             border: 1px solid #e0e0e0;
                             text-align: left;
		
                         }}
                         .info-table th {{
                            background-color: #f4f4f4; 
                             font-weight: bold;
                             width:50%;
                         }}

                         /* Approval timeline */
                         .timeline {{
                             width: 100%;
                             margin-top: 20px;
                         }}
                         .timeline td {{
                             vertical-align: top;
                         }}
                         .timeline-dot {{
                             width: 15px;
                             height: 15px;
                             border-radius: 50%;
                             margin-top: 3px;
                         }}
                         .timeline-line {{
                             width: 2px;
                             height: 30px;
                             background-color: #e0e0e0;
                             margin-left: 6px;
                         }}
                         /* Section backgrounds */
                 .section-header {{
                     background-color: #3252ff; /* Blue header */
                     color: #ffffff;
                 }}
                 .section-greeting {{
                     background-color: #f9f9f9; /* light grey */
                 }}
                 .section-timeline {{
                     background-color: #eef4ff; /* soft blue */
                 }}
                 .section-info {{
                  width: 500px;
                     background-color: #ffffff; /* white card */
                 }}
                 .section-footer {{
                     background-color: #000; /* footer grey */
                 }}
                 .section-button {{
                     padding-top: 0;
                 }}

                  /* Footer */
                  .footer {{
                      text-align: center;
                      padding: 20px 30px;
                      font-size: 13px;
                      color: #fff;
                  }}

                    #data-table {{
                        width: 100%;
                        padding: 20px;
                      }}
                      #data-table thead th {{
                        text-align: center;
                        background-color: rgb(236, 236, 236);
                        padding: 7px 0;
                      }}
                      #data-table thead th, #data-table tbody td {{
                        border: 1px solid rgb(218, 218, 218);
    
                      }}
                      #data-table tbody td {{
                        padding: 7px 5px;
    
              }}
                  /* Responsive */
                  @media only screen and (max-width: 600px) {{
                    .header-bg td {{
                        display: block;
                        text-align: center;
                        margin-bottom: 10px;
                    }}
                    .header-bg img {{
                        margin: auto;
                    }}
                  }}
                </style> <!DOCTYPE html>
                 <html>
                 <head>
                     <meta charset=""UTF-8"">
                     <title>HR Leave Request</title>
                     <style>
                         /* Reset styles */
                         body, table, td, p, a, li, h1, h2 {{
                             -webkit-text-size-adjust: 100%;
                             -ms-text-size-adjust: 100%;
                             margin: 0;
                             padding: 0;
                         }}
                         body {{
                             font-family: Arial, sans-serif;
                             font-size: 14px;
                             line-height: 20px;
                             color: #333333;
                             background-color: #f4f4f4;
                             padding: 20px;
                         }}
                         table {{
                             border-collapse: collapse;
                         }}

                         /* Main container */
                         .email-container {{
                             width: 800px;
                             margin: auto;
                             background-color: #ffffff;
                             border: 1px solid #e0e0e0;
                             border-radius: 8px;
                             overflow: hidden;
                         }}

                         /* Header */
                         .header-bg {{
                             position: relative;
                             background-color: #3252ff;
                             background-image: linear-gradient(to bottom right, #080301 120px, transparent 0);
                             background-repeat: no-repeat;
                             background-size: 140px 140px;
                             padding: 25px 30px;
                             color: #ffffff;
                         }}
                         .header-bg img {{
                             display: block;
                             border: 0;
                             outline: none;
                             text-decoration: none;
                             max-width: 200px;
                             height: auto;
                         }}
                         .header-bg td {{
                             font-size: 13px;
                             line-height: 18px;
                             text-align: right;
                             color: #ffffff;
                         }}

                         /* Content */
                         .content {{
                             padding: 20px 30px;
                         }}
                         .content p {{
                             margin-bottom: 10px;
                         }}
                         .content h2 {{
                             font-size: 18px;
                             margin-bottom: 10px;
                             color: #3252ff;
                         }}

                         /* Tables for info */
                         .info-table {{
                             width: 100%;
                             width: 400px;
                             border: 1px solid #e0e0e0;
                             border-radius: 5px;
          
                         }}
                         .info-table th, .info-table td {{
                             padding: 10px;
                             border: 1px solid #e0e0e0;
                             text-align: left;
		
                         }}
                         .info-table th {{
                            background-color: #f4f4f4; 
                             font-weight: bold;
                             width:50%;
                         }}

                         /* Approval timeline */
                         .timeline {{
                             width: 100%;
                             margin-top: 20px;
                         }}
                         .timeline td {{
                             vertical-align: top;
                         }}
                         .timeline-dot {{
                             width: 15px;
                             height: 15px;
                             border-radius: 50%;
                             margin-top: 3px;
                         }}
                         .timeline-line {{
                             width: 2px;
                             height: 30px;
                             background-color: #e0e0e0;
                             margin-left: 6px;
                         }}
                         /* Section backgrounds */
                 .section-header {{
                     background-color: #3252ff; /* Blue header */
                     color: #ffffff;
                 }}
                 .section-greeting {{
                     background-color: #f9f9f9; /* light grey */
                 }}
                 .section-timeline {{
                     background-color: #eef4ff; /* soft blue */
                 }}
                 .section-info {{
                  width: 500px;
                     background-color: #ffffff; /* white card */
                 }}
                 .section-footer {{
                     background-color: #000; /* footer grey */
                 }}
                 .section-button {{
                     padding-top: 0;
                 }}

                  /* Footer */
                  .footer {{
                      text-align: center;
                      padding: 20px 30px;
                      font-size: 13px;
                      color: #fff;
                  }}

                  /* table data css */
                  #data-table {{
                    width: 100%;
                    padding: 20px;
                  }}
                  #data-table thead th {{
                    text-align: center;
                    background-color: rgb(236, 236, 236);
                    padding: 7px 0;
                  }}
                  #data-table thead th, #data-table tbody td {{
                    border: 1px solid rgb(218, 218, 218);
        
                  }}
                  #data-table tbody td {{
                    padding: 7px 5px;
        
                  }}
                  /* Responsive */
                  @media only screen and (max-width: 600px) {{
                    .header-bg td {{
                        display: block;
                        text-align: center;
                        margin-bottom: 10px;
                    }}
                    .header-bg img {{
                        margin: auto;
                    }}
                  }}
                </style>
              </head>
              <body>
                <table width=""800"" align=""center"" cellpadding=""0"" cellspacing=""0""
                    style=""width:800px; margin: auto; background-color:#ffffff; border:1px solid #e0e0e0; border-collapse:collapse;"">
                        <!-- Header -->
                  <tr>
                      <td class=""header-bg"">
                          <table width=""100%"">
                              <tr>
                                  <td align=""left"">
                                    <img  src=""cid:CompanyLogo"" alt=""Company Logo"" style=""max-width:85px; height:auto;"" />
                                  </td>
                                  <td align=""right"" style=""color: white"">
                                      <span>{formattedAddress}</span><br>
                                       <a href=""mailto:{{model.CompanyEmail}}"" style=""color:white !important; text-decoration:none;"">
                                            {model.CompanyEmail}
                                        </a><br>
                                      {model.CompanyPhone}
                                  </td>
                              </tr>
                          </table>
                      </td>
                  </tr>

                <!-- Greeting -->
                  <tr>
                      <td class=""content section-greeting"">
                          <p>Dear {model.EmployeeName},</p>
                          <p>This is an automated notification of upcoming activities. Please find the details below:</p>
                      </td>
                  </tr>
                        <!-- Approval Timeline (Horizontal) -->



                        <!-- Employee Info -->
                  <tr>
                      <td class=""content section-info"">
                          <h2>Employee Information</h2>
                          <table class=""info-table"" style=""margin-bottom: 10px;"">
                              <tr>
                                  <th>Name</th>
                                  <td>{model.EmployeeName}</td>
                              </tr>
                              <tr>
                                  <th>Team Name</th>
                                  <td>{model.TeamName}</td>
                              </tr>
                              <tr>
                                  <th>Total Activities</th>
                                  <td> {model.TotalActivities} </td>
                              </tr>
                          </table>
                      </td>
                  </tr>


                  <!-----Table Data --> 
                  <tr>
                    <td style=""padding: 0 20px 20px 20px;"">
                      <table id=""data-table"">
                        <thead>
                          <th>#</th>
                          <th>Lead Name</th>
                          <th>Customer Name</th>
                          <th>Activity Type</th>
                          <th>Date & Time</th>
                          <th>Note</th>
                          <th>Owner Name</th>
                        </thead>
                        <tbody>
                         {activityRows}
                        </tbody>
                      </table>
                    </td>
                  </tr>
      
              <!-- Footer -->
                  <tr>
                    <td class=""footer section-footer"" align=""center"" style=""text-align:center;"">
                      <p>© {model.CompanyName} 2025. All rights reserved.</p>
                    </td>
                  </tr>
                </table>
              </body>
            </html>
               ";
            // 3️⃣ AlternateView for HTML
            var htmlView = AlternateView.CreateAlternateViewFromString(body, null, MediaTypeNames.Text.Html);
            byte[] imageBytes = File.ReadAllBytes(@"D:\HRM\GCTL_App\wwwroot\images\ms.png");
            //byte[] imageBytes = File.ReadAllBytes(@"D:\HRM\GCTL_App\wwwroot\images\ms.png");

            // 2️⃣ Create a MemoryStream from the bytes
            var ms = new MemoryStream(imageBytes);

            // 4️⃣ Attach the logo as LinkedResource
            var logo = new LinkedResource(ms, MediaTypeNames.Image.Png)
            {
                ContentId = "CompanyLogo", // must match HTML src
                TransferEncoding = TransferEncoding.Base64
            };

            await _emailService.SendAsync(
                organizationId: 1,
                //organizationId: model.ComapanyID ?? 0,
                toEmail: recipientEmail,
                subject: subject,
                body: body,
                attachmentBytes: pdfBytes,
                attachmentName: $"{model.EmployeeName ?? "Admin"}_ActivityReport.pdf",
                linkedResources: new List<LinkedResource> { logo }
            );

            Console.WriteLine($"📧 Email sent successfully to {recipientEmail}");
        }

        #endregion

    }
}
