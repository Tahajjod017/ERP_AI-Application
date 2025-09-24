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
            int? userID,
            int? CustomerTypeID, 
            string? LeadStatusID
            )   // current user ID
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

                // Filter by CustomerTypeID
                if (CustomerTypeID.HasValue && CustomerTypeID.Value > 0)
                {
                    query = query.Where(u => u.Lead.Customer.CustomerAddresses.First().AddressTypeID == CustomerTypeID.Value);
                }

                // Filter by LeadStatusID
                if (!string.IsNullOrEmpty(LeadStatusID))
                {
                    query = query.Where(u => u.Lead.LeadStatus.LeadStatusName== LeadStatusID);
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
                        page.DefaultTextStyle(x => x.FontSize(9));

                        // -------------------
                        // Header
                        // -------------------
                        page.Header()
                            .Element(header =>
                            {
                                header
                                    .PaddingBottom(5)
                                    .BorderBottom(1)
                                    .BorderColor(Colors.Grey.Lighten2)
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Text("Lead Activities (Upcoming)")
                                            .FontSize(16)
                                            .SemiBold()
                                            .FontColor(Colors.Blue.Medium);

                                        row.ConstantItem(100)
                                            .AlignRight()
                                            .Text(DateTime.Now.ToString("dd-MM-yyyy"))
                                            .FontSize(9)
                                            .FontColor(Colors.Grey.Darken2);
                                    });
                            });

                        // -------------------
                        // Table content
                        // -------------------
                        page.Content()
                            .Table(table =>
                            {
                                // Column definitions
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);  // SL
                                    columns.RelativeColumn(2);   // Lead Name
                                    columns.RelativeColumn(2);   // Customer Name
                                    columns.RelativeColumn(2);   // Service Type
                                    columns.RelativeColumn(2);   // Date & Time
                                    columns.RelativeColumn(3);   // Note
                                    columns.RelativeColumn(2);   // Owner Name
                                });

                                // Table header (repeats on each page)
                                table.Header(header =>
                                {
                                    header.Cell().Element(HeaderCell).Text("SL");
                                    header.Cell().Element(HeaderCell).Text("Lead Name");
                                    header.Cell().Element(HeaderCell).Text("Customer Name");
                                    header.Cell().Element(HeaderCell).Text("Service Type");
                                    header.Cell().Element(HeaderCell).Text("Date & Time");
                                    header.Cell().Element(HeaderCell).Text("Note");
                                    header.Cell().Element(HeaderCell).Text("Owner Name");
                                });

                                // Table rows
                                int serial = 1;
                                bool isAlternate = false;
                                foreach (var activity in upcomingActivities)
                                {
                                    isAlternate = !isAlternate;

                                    table.Cell().Element(c => BodyCell(c, isAlternate)).Text(serial++.ToString());
                                    table.Cell().Element(c => BodyCell(c, isAlternate)).Text(activity.LeadName);
                                    table.Cell().Element(c => BodyCell(c, isAlternate)).Text(activity.CustomerName);
                                    table.Cell().Element(c => BodyCell(c, isAlternate)).Text(activity.ActivityType);
                                    table.Cell().Element(c => BodyCell(c, isAlternate))
                                         .Text(activity.ActivityDateTime?.ToString("dd-MM-yyyy HH:mm") ?? "");
                                    table.Cell().Element(c => BodyCell(c, isAlternate))
                                         .Text(activity.ActivityNote ?? "").WrapAnywhere();
                                    table.Cell().Element(c => BodyCell(c, isAlternate)).Text(activity.LeadOwner);
                                }

                                // -------------------
                                // Styles
                                // -------------------
                                IContainer HeaderCell(IContainer container) =>
                                     container
                                         .Background(Colors.Grey.Lighten3)
                                         .PaddingVertical(5)
                                         .PaddingHorizontal(3)
                                         .BorderBottom(1)
                                         .BorderColor(Colors.Grey.Lighten2);
                                         //.Text(text =>
                                         //{
                                         //    //text.SemiBold();
                                         //}
                                         //);

                                IContainer BodyCell(IContainer container, bool alternate) =>
                                    container
                                        .Background(alternate ? Colors.Grey.Lighten5 : Colors.White)
                                        .PaddingVertical(4)
                                        .PaddingHorizontal(3)
                                        .BorderBottom(1)
                                        .BorderColor(Colors.Grey.Lighten2);
                            });

                        // -------------------
                        // Footer
                        // -------------------
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("Generated on: ").SemiBold();
                                x.Span(DateTime.Now.ToString("dd-MM-yyyy HH:mm"));
                                //x.Span($"   Page {page.CurrentPageNumber} of {page.TotalPages}");
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
