using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Mathematics;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;


namespace GCTL.Service.CRM.LeadsActivities
{
    public class LeadsActivityService : ILeadsActivityService
    {
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;

        public LeadsActivityService(IGenericRepository<LeadDetails> leadDetailsRepository)
        {
            _leadDetailsRepository = leadDetailsRepository;
        }

        public async Task<ReturnDataView<LeadDetailsDTO>> GetUpcomingActivityList(
            int page,
            int itemPerPage,
            string search,
            string sort,
            string direction,
            string dateRange,
            int? userID)   // current user ID
        {
            try
            {
                // ✅ If userID is null, return empty result immediately
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

                // ✅ Base query: Only current user, all future (from today)
                var baseQuery = _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= today && u.CreatedBy == userID.Value);

                var totalItem = await baseQuery.CountAsync();

                var totalNowItem = await _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= now && u.CreatedBy == userID.Value)
                    .CountAsync();

                var query = _leadDetailsRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= now && u.CreatedBy == userID.Value);

                // ✅ Date filtering
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

                // ✅ Search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u =>
                        EF.Functions.Like(u.ActivityNote, $"%{search}%") ||
                        EF.Functions.Like(u.ActivityDateTime, $"%{search}%") ||
                        EF.Functions.Like(u.CreatedAt, $"%{search}%"));
                }

                var totalSearchItem = await query.CountAsync();

                // ✅ Sorting
                query = direction?.ToLower() == "asc"
                    ? query.OrderBy(u => EF.Property<object>(u, sort ?? "ActivityDateTime"))
                    : query.OrderByDescending(u => EF.Property<object>(u, sort ?? "ActivityDateTime"));

                // ✅ Paging
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


        public async Task<byte[]> GeneratePDF()
        {
            try
            {
                var upcomingActivities = await _leadDetailsRepository.AllActive().Where(u => u.ActivityDateTime >= DateTime.UtcNow)
                   .OrderBy(u => u.ActivityDateTime)
                   .Select(u => new
                   {
                       LeadName = u.Lead.LeadName,
                       ActivityType = u.LeadActivityType.LeadActivityName,
                       ActivityNote = u.ActivityNote,
                       ActivityDateTime = u.ActivityDateTime,
                       CreatedAt = u.CreatedAt,
                       LeadID = u.LeadID,
                       CustomerName = u.Lead.Customer.FullName,
                       LeadStage = u.Lead.LeadStatus.LeadStatusName,
                       LeadPriority = u.Lead.Priority.PriorityName,
                       LeadSource = u.Lead.LeadStatus.LeadStatusName,
                       LeadProbability = u.Lead.ProbabilityPercentage,
                       LeadOwner = u.Lead.LeadOwner.FirstName + " " + u.Lead.LeadOwner.LastName,
                   }).ToListAsync();

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(8));

                        page.Header()
                        .Text("Lead Activities (Upcoming)")
                        .SemiBold().FontSize(18).FontColor(Colors.Blue.Medium);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(CellStyle).Text("SL");
                                header.Cell().Element(CellStyle).Text("Lead Name");
                                header.Cell().Element(CellStyle).Text("Customer Name");
                                header.Cell().Element(CellStyle).Text("Service Type");
                                header.Cell().Element(CellStyle).Text("Date & Time");
                                header.Cell().Element(CellStyle).Text("Note");
                                header.Cell().Element(CellStyle).Text("Owner Name");
                            });
                            int serial = 1;
                            foreach (var activity in upcomingActivities)
                            {
                             
                                table.Cell().Element(CellStyle).Text(serial++.ToString());
                                table.Cell().Element(CellStyle).Text(activity.LeadName);
                                table.Cell().Element(CellStyle).Text(activity.CustomerName);
                                table.Cell().Element(CellStyle).Text(activity.ActivityType);
                                table.Cell().Element(CellStyle).Text(activity.ActivityDateTime?.ToString("dd-MM-yyyy HH:mm:ss") ?? "");
                                table.Cell().Element(CellStyle).Text(activity.ActivityNote);
                                table.Cell().Element(CellStyle).Text(activity.LeadOwner);
                
                               
                            }

                            IContainer CellStyle(IContainer container) =>
                                container.PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);

                        });

                        page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Generated on: ").SemiBold();
                            x.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                        });
                    });
                });
                var pdfBytes = document.GeneratePdf();

                return pdfBytes;
            }
            catch (Exception) {
                return new byte[0];
            }
            
        }
    }
}
