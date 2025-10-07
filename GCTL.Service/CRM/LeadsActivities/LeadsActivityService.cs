using Bogus.DataSets;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.FileHandler;
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
        private readonly IPdfFileHandler _pdfFileHandlerService;

        public LeadsActivityService(IGenericRepository<LeadDetails> leadDetailsRepository, IPdfFileHandler pdfFileHandlerService)
        {
            _leadDetailsRepository = leadDetailsRepository;
            _pdfFileHandlerService = pdfFileHandlerService;
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
            string? LeadStatusID,
            int? ActivityTypeID
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
                if (ActivityTypeID > 0)
                {
                    query = query.Where(u => u.LeadActivityTypeID == ActivityTypeID);
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
                var activityDataSource = new ActivityDocumentDataSource(_leadDetailsRepository);
                var activities = await activityDataSource.GetUpCommingActivity();
                var document =new ActivityDocument(activities, _pdfFileHandlerService);

                var pdfBytes = document.GeneratePdf();
                return pdfBytes;
            }
            catch (Exception)
            {
                return new byte[0];
            }
        }


    }
}
