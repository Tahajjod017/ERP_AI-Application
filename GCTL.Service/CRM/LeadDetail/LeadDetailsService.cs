using Azure;
using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static Dapper.SqlMapper;


namespace GCTL.Service.CRM.LeadDetail
{

    public class LeadDetailsService : ILeadDetailsService
    {
        #region Properties
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypesGenericRepository;
        private readonly IGenericRepository<GCTL.Data.Models.LeadDetails> _leadDetailsGenericRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;
        private readonly IGenericRepository<Statuses> _statusesRepository;
        private readonly IGenericRepository<LeadDetails> _leadDetailsRepository;
        private readonly IUserInfoService _userInfoService;
        #endregion

        #region Constructor
        public LeadDetailsService(IGenericRepository<GCTL.Data.Models.LeadDetails> leadDetailsGenericRepository, IGenericRepository<LeadActivityTypes> leadActivityTypesGenericRepository, IGenericRepository<Leads> leadsRepository, IGenericRepository<LeadDetails> leadDetailsRepository, IUserInfoService userInfoService, IGenericRepository<Statuses> statusesRepository)
        {
            _leadActivityTypesGenericRepository = leadActivityTypesGenericRepository;
            _leadDetailsGenericRepository = leadDetailsGenericRepository;
            _leadsRepository = leadsRepository;
            _leadDetailsRepository = leadDetailsRepository;
            _userInfoService = userInfoService;
            _statusesRepository = statusesRepository;
        }
        #endregion

        #region CreateLeadActivateTypes
        public async Task<bool> CreateLeadActivateTypes()
        {
            var leadActivityObjs = await _leadActivityTypesGenericRepository.GetAllAsync();

            if (!leadActivityObjs.Any())
            {
                var items = new List<LeadActivityTypes>
                {
                    new LeadActivityTypes{LeadActivityIcon = "fa-phone", LeadActivityName = "Call", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-envelope", LeadActivityName = "Email", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-handshake", LeadActivityName = "Offline Meeting", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-vr-cardboard", LeadActivityName = "Online Meeting", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-quote-left", LeadActivityName = "Quatation", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-angles-right", LeadActivityName = "Rev. Quatation", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-file", LeadActivityName = "Attachment", CreatedAt = DateTime.UtcNow},
                    new LeadActivityTypes{LeadActivityIcon = "fa-won-sign", LeadActivityName = "Won", CreatedAt = DateTime.UtcNow, UseFor = "special"},
                    new LeadActivityTypes{LeadActivityIcon = "fa-square-minus", LeadActivityName = "Lost", CreatedAt = DateTime.UtcNow, UseFor = "special"},
                };

                await _leadActivityTypesGenericRepository.AddRangeAsync(items);
                return true;
            }
            return false;
        }
        #endregion

        #region CreateLeadDeatil
        public async Task<ReturnView> SaveLeadDetailAsync(
    LeadDetailsVM leadDetailsVM,
    string? fileLocation)
        {
            // -------------------------------
            // Validation
            // -------------------------------
            if (!leadDetailsVM.ActivityDateTime.HasValue)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = "Please add Date and Time field"
                };
            }

            await _leadDetailsGenericRepository.BeginTransactionAsync();

            try
            {
                // -------------------------------
                // Lead validation
                // -------------------------------
                var leadObj = await _leadsRepository
                    .FirstOrDefaultAsync(x => x.LeadID == leadDetailsVM.LeadID);

                if (leadObj == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Lead Not Found"
                    };
                }

                if (leadObj.IsOwn != null || leadObj.ClosingDate != null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Activity has a state. To reopen click on Restore Button"
                    };
                }

                // -------------------------------
                // DateTime → UTC
                // -------------------------------
                var localDateTime = DateTime.SpecifyKind(
                    leadDetailsVM.ActivityDateTime.Value,
                    DateTimeKind.Local);

                var utcDateTime = localDateTime.ToUniversalTime();

                // -------------------------------
                // Activity type validation
                // -------------------------------
                var leadTypeObj = await _leadActivityTypesGenericRepository
                    .FirstOrDefaultAsync(x => x.LeadActivityTypeID == leadDetailsVM.LeadActivityTypeID);

                if (leadTypeObj == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Invalid Activity Type"
                    };
                }

                var attachmentType = await _leadActivityTypesGenericRepository
                    .FirstOrDefaultAsync(x => x.LeadActivityName == "Attachment");

                bool isAttachment =
                    attachmentType != null &&
                    leadTypeObj.LeadActivityTypeID == attachmentType.LeadActivityTypeID;

                // -------------------------------
                // CREATE or UPDATE
                // -------------------------------
                GCTL.Data.Models.LeadDetails leadDetailsObj;

                bool isUpdate =
                    leadDetailsVM.LeadDetailID.HasValue &&
                    leadDetailsVM.LeadDetailID.Value > 0;

                if (isUpdate)
                {
                    // ---------- UPDATE ----------
                    leadDetailsObj = await _leadDetailsGenericRepository
                        .FirstOrDefaultAsync(x =>
                            x.LeadDetailID == leadDetailsVM.LeadDetailID.Value);

                    if (leadDetailsObj == null)
                    {
                        return new ReturnView
                        {
                            Success = false,
                            Message = "Activity not found"
                        };
                    }

                    leadDetailsObj.ActivityDateTime = utcDateTime;
                    leadDetailsObj.LeadActivityTypeID = leadTypeObj.LeadActivityTypeID;
                    leadDetailsObj.ActivityNote = leadDetailsVM.ActivityNote;
                    //leadDetailsObj.PhoneNumber = leadDetailsVM.ContactNumber;
                    //leadDetailsObj.EmailAddress = leadDetailsVM.ContactEmail;

                    if (isAttachment && fileLocation != null)
                    {
                        leadDetailsObj.FileLink = fileLocation;
                    }

                    leadDetailsObj.UpdatedAt = DateTime.UtcNow;
                    leadDetailsObj.UpdatedBy = leadDetailsVM.CreatedBy;
                    leadDetailsObj.LIP = leadDetailsVM.LIP;
                    leadDetailsObj.LMAC = leadDetailsVM.LMAC;

                    await _leadDetailsGenericRepository.UpdateAsync(leadDetailsObj);
                }
                else
                {
                    // ---------- CREATE ----------
                    leadDetailsObj = new GCTL.Data.Models.LeadDetails
                    {
                        LeadID = leadDetailsVM.LeadID,
                        ActivityDateTime = utcDateTime,
                        LeadActivityTypeID = leadTypeObj.LeadActivityTypeID,
                        ActivityNote = leadDetailsVM.ActivityNote,
                        PhoneNumber = leadDetailsVM.ContactNumber,
                        EmailAddress = leadDetailsVM.ContactEmail,
                        FileLink = isAttachment && fileLocation != null ? fileLocation : null,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = leadDetailsVM.CreatedBy,
                        LIP = leadDetailsVM.LIP,
                        LMAC = leadDetailsVM.LMAC
                    };

                    await _leadDetailsGenericRepository.AddAsync(leadDetailsObj);
                }

                await _leadDetailsGenericRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = isUpdate
                        ? "Activity Updated Successfully"
                        : "Activity Saved Successfully"
                };
            }
            catch (Exception)
            {
                await _leadDetailsGenericRepository.RollbackTransactionAsync();
                return new ReturnView
                {
                    Success = false,
                    Message = "Something went wrong"
                };
            }
        }

        #endregion

        #region ActivityList
        public async Task<LeadActivityResultVM> ActivityList(int id, string query, int page, string type)
        {
            int leadDetailsTypeID = 0;
            if (!string.IsNullOrEmpty(type))
            {
                var leadDetailsTypeObj = await _leadActivityTypesGenericRepository
                    .FirstOrDefaultAsync(u => u.LeadActivityName == type);

                leadDetailsTypeID = leadDetailsTypeObj?.LeadActivityTypeID ?? 0;
            }

            const int pageSize = 10;
            int skip = (page - 1) * pageSize;

            // 🔹 Fetch the lead first to get the LeadOwnerID
            var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == id);
            if (leadObj == null)
            {
                return new LeadActivityResultVM
                {
                    IsWon = null,
                    Activities = new List<LeadActivityVM>()
                };
            }

            var leadOwnerId = leadObj.LeadOwnerID;

            // 🔹 Pre-calculate stats ONCE (not inside projection)
            var totalLeads = await _leadsRepository.AllActive()
                .Where(x => x.LeadOwnerID == leadOwnerId)
                .CountAsync();

            if (totalLeads == 0) totalLeads = 1; // avoid division by zero

            var successCount = await _leadsRepository.AllActive()
                .Where(x => x.LeadOwnerID == leadOwnerId && x.IsOwn == true)
                .CountAsync();

            var lostCount = await _leadsRepository.AllActive()
                .Where(x => x.LeadOwnerID == leadOwnerId && x.IsOwn == false)
                .CountAsync();

            var cancelCount = await _leadsRepository.AllActive()
                .Where(x => x.LeadOwnerID == leadOwnerId && x.IsOwn == null)
                .CountAsync();

            var successPercentage = (int)Math.Round(successCount * 100m / totalLeads);
            var lostPercentage = (int)Math.Round(lostCount * 100m / totalLeads);
            var cancelPercentage = (int)Math.Round(cancelCount * 100m / totalLeads);

            // 🔹 Query lead details
            var list = await _leadDetailsGenericRepository
                .AllActive()
                .Where(u => u.LeadID == id &&
                           (leadDetailsTypeID == 0 || u.LeadActivityTypeID == leadDetailsTypeID) &&
                           (string.IsNullOrEmpty(query)
                            || EF.Functions.Like(u.ActivityDateTime.ToString(), $"%{query}%")
                            || EF.Functions.Like(u.ActivityNote, $"%{query}%")
                            || EF.Functions.Like(u.LeadActivityType.LeadActivityName, $"%{query}%")))
                .OrderByDescending(e => e.ActivityDateTime)
                .Skip(skip)
                .Take(pageSize)
                .Select(e => new LeadActivityVM
                {
                    LeadDetailID = e.LeadDetailID,
                    ActivityDateTime = e.ActivityDateTime,
                    ActivityNote = e.ActivityNote,
                    FileLink = e.FileLink,
                    PhoneNumber = e.PhoneNumber,
                    EmailAddress = e.EmailAddress,
                    IsDone = e.IsDone,
                    LeadActivityName = e.LeadActivityType.LeadActivityName,
                    LeadActivityIcon = e.LeadActivityType.LeadActivityIcon,
                    CreatedByName = e.CreatedByNavigation != null
                        ? $"{e.CreatedByNavigation.FirstName} {e.CreatedByNavigation.LastName}"
                        : null,
                })
                .ToListAsync();

            return new LeadActivityResultVM
            {
                IsWon = leadObj.IsOwn,
                Activities = list,
                // ✅ Use precomputed stats
                SuccessPercentage = successPercentage,
                LostPercentage = lostPercentage,
                CancelPercentage = cancelPercentage,
                ClosingDate = leadObj.ClosingDate ?? null
            };
        }
        #endregion

        // lead table source, status update service function
        #region UpdateLeadFieldValue
        public async Task<ReturnView> UpdateLeadFieldValue(DetailsLeadUpdateVM detailsLeadUpdateVM)
        {
            await _leadsRepository.BeginTransactionAsync();

            try
            {
                var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == detailsLeadUpdateVM.LeadID);
                if (leadObj == null)
                {
                    await _leadsRepository.RollbackTransactionAsync();
                    return new ReturnView
                    {
                        Success = false,
                        Message = $"{detailsLeadUpdateVM.FieldName} not updated"

                    };
                }

                switch (detailsLeadUpdateVM.FieldName.ToLower())
                {
                    case "source":
                        leadObj.LeadSourceID = detailsLeadUpdateVM.FieldValue;
                        break;
                    case "stage":
                        leadObj.LeadStatusID = detailsLeadUpdateVM.FieldValue;
                        break;
                    case "priority":
                        leadObj.PriorityID = detailsLeadUpdateVM.FieldValue;
                        break;
                    case "probability":
                        leadObj.ProbabilityPercentage = detailsLeadUpdateVM.FieldValue;
                        break;
                    default:
                        await _leadsRepository.RollbackTransactionAsync();
                        return new ReturnView
                        {
                            Success = false,
                            Message = $"{detailsLeadUpdateVM.FieldName} not updated"

                        };
                }

                // Update audit fields
                leadObj.UpdatedAt = DateTime.UtcNow;
                leadObj.UpdatedBy = detailsLeadUpdateVM.UpdatedBy;

                // Save changes
                await _leadsRepository.UpdateAsync(leadObj);

                // Commit transaction
                await _leadsRepository.CommitTransactionAsync();
                return new ReturnView
                {
                    Success = true,
                    Message = $"{detailsLeadUpdateVM.FieldName} is updated"

                };
            }
            catch (Exception ex)
            {
                await _leadsRepository.RollbackTransactionAsync();
                return new ReturnView
                {
                    Success = false,
                    Message = $"{detailsLeadUpdateVM.FieldName} not updated"

                };
            }
        }
        #endregion

        #region Save Won or Loss function
        public async Task<ReturnView> AddIsWon(IsWonVM isWonVM)
        {
            // Begin transaction
            await _leadsRepository.BeginTransactionAsync();

            try
            {
                // Fetch Lead and LeadType
                var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == isWonVM.LeadID);
                var leadTypeObj = await _leadActivityTypesGenericRepository.FirstOrDefaultAsync(
                    u => u.LeadActivityTypeID == isWonVM.LeadActivityTypeID
                );

                // Validate
                if (leadObj == null || leadTypeObj == null)
                {
                    await _leadsRepository.RollbackTransactionAsync();
                    return new ReturnView { Success = false, Message = "Something went wrong" };
                }

                if (leadObj.IsOwn != null || leadObj.ClosingDate != null)
                {
                    await _leadsRepository.RollbackTransactionAsync();
                    return new ReturnView { Success = false, Message = "Please restart your lead first" };
                }

                var leadTypeID = leadTypeObj.LeadActivityTypeID;
                var isWon = leadTypeObj.UseFor == "Won";

                if (leadTypeID == 0)
                {
                    await _leadsRepository.RollbackTransactionAsync();
                    return new ReturnView { Success = false, Message = "Invalid activity type" };
                }

                if (leadObj.IsOwn == isWon)
                {
                    await _leadsRepository.RollbackTransactionAsync();
                    return new ReturnView
                    {
                        Success = false,
                        Message = $"Your status is already {leadTypeObj.LeadActivityName}"
                    };
                }

                // Create new activity
                var leadActivityObj = new GCTL.Data.Models.LeadDetails
                {
                    LeadID = leadObj.LeadID,
                    ActivityDateTime = DateTime.UtcNow,
                    LeadActivityTypeID = leadTypeID,
                    ActivityNote = isWonVM.ActivityNote,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = isWonVM.CreatedBy,
                    LIP = isWonVM.LIP,
                    LMAC = isWonVM.LMAC
                };
                await _leadDetailsGenericRepository.AddAsync(leadActivityObj);

                // Update Lead info
                leadObj.IsOwn = isWon;
                leadObj.ClosingDate = DateTime.UtcNow;
                leadObj.UpdatedAt = DateTime.UtcNow;
                leadObj.UpdatedBy = isWonVM.UpdatedBy;
                await _leadsRepository.UpdateAsync(leadObj);

                // 🔹 Mark upcoming activities as deleted (inside transaction)
                var utcNow = DateTime.UtcNow;
                var upcomingActivityList = await _leadDetailsGenericRepository.AllActive()
                    .Where(u => u.ActivityDateTime >= utcNow && u.LeadID == leadObj.LeadID)
                    .ToListAsync();

                if (upcomingActivityList.Any())
                {
                    foreach (var item in upcomingActivityList)
                    {
                        item.DeletedAt = utcNow;
                        item.DeletedBy = isWonVM.DeletedBy;
                        item.LIP = isWonVM.LIP;
                        item.LMAC = isWonVM.LMAC;
                    }

                    await _leadDetailsGenericRepository.UpdateRangeAsync(upcomingActivityList);
                }

                // Commit transaction
                await _leadsRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = $"Lead status updated to {leadTypeObj.LeadActivityName}"
                };
            }
            catch (Exception)
            {
                await _leadsRepository.RollbackTransactionAsync();
                return new ReturnView
                {
                    Success = false,
                    Message = "Something went wrong"
                };
            }
        }
        #endregion

        #region  restore lead activity
        public async Task<ReturnView> RestoreLead(int id)
        {
            try
            {
                if (id != 0)
                {
                    var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == id);

                    if (leadObj != null)
                    {
                        if (leadObj.IsOwn != null)
                        {
                            leadObj.IsOwn = null;
                            leadObj.ClosingDate = null;
                            await _leadsRepository.UpdateAsync(leadObj);

                            var deletedActivity = await _leadDetailsGenericRepository.FindAsync(u => u.DeletedAt != null && u.DeletedBy != null && u.LeadID == id);
                            var totalrestoreItem = deletedActivity.Count();
                            if (totalrestoreItem > 0)
                            {
                                deletedActivity.ForEach(item =>
                                {
                                    item.DeletedAt = null;
                                    item.DeletedBy = null;
                                });
                                await _leadDetailsGenericRepository.UpdateRangeAsync(deletedActivity);

                                return new ReturnView
                                {
                                    Success = true,
                                    Message = totalrestoreItem + " Actvity Restored Successfully"
                                };
                            }
                            return new ReturnView
                            {
                                Success = true,
                                Message = "No need to resotre any activity."
                            };
                        }
                        return new ReturnView
                        {
                            Success = false,
                            Message = "Leads already activated. No need to restore this lead."
                        };

                    }
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Lead not found"
                    };
                }
                return new ReturnView
                {
                    Success = false,
                    Message = "Somethig went to wrong"
                };

            }
            catch (Exception)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = "Somethig went to wrong"
                };
            }

        }
        #endregion

        #region CompleteAsync
        public async Task<ReturnView> CompleteAsync(CRMStateModal model)
        {
            await _leadDetailsRepository.BeginTransactionAsync();
            try
            {
                var query = await _leadDetailsRepository.GetByIdAsync(model.LeadDetailID);
                int statusId = await GetStatusId("Completed");
                var beforeEntity = JsonConvert.DeserializeObject<LeadDetailsVM>(JsonConvert.SerializeObject(query, JsonSettings.IgnoreReferenceLoop));
                if (query == null)
                {
                    await _leadDetailsRepository.RollbackTransactionAsync();
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Activity not found."
                    };
                }

                if (query.IsDone?? false)
                {
                    await _leadDetailsRepository.RollbackTransactionAsync();
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Activity is already completed."
                    };
                }

                query.IsDone = true;
                query.StatusID = statusId;
                query.UpdatedAt = DateTime.Now;
                query.UpdatedBy = model.UpdatedBy;
                query.LIP = model.LIP;
                query.LMAC = model.LMAC;
                await _leadDetailsRepository.UpdateAsync(query);
                await _leadDetailsRepository.CommitTransactionAsync();
                var afterEntity = JsonConvert.DeserializeObject<LeadDetailsVM>(JsonConvert.SerializeObject(query, JsonSettings.IgnoreReferenceLoop));

                await _userInfoService.ActionLogAsync("LeadDetails", ActionName.DataUpdated, beforeEntity, afterEntity, query.LeadDetailID, model);

                return new ReturnView
                {
                    Success = true,
                    Message = "Activity completed successfully."
                };
            }
            catch (Exception e)
            {
                await _leadDetailsRepository.RollbackTransactionAsync();

                #if DEBUG
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Error: " + e.Message
                    };
                #else
                    return new ReturnView 
                    { 
                        Success = false, 
                        Message = "An error occurred while completing the activity." 
                    };
                #endif
            }
        }
        #endregion

        #region No Response
        public async Task<ReturnView> NoResponseAsync(CRMStateModal model)
        {
            await _leadDetailsRepository.BeginTransactionAsync();
            try
            {
                
                var query = await _leadDetailsRepository.GetByIdAsync(model.LeadDetailID);
                int statusId = await GetStatusId("No Response");
                var beforeEntity = JsonConvert.DeserializeObject<LeadDetailsVM>(JsonConvert.SerializeObject(query, JsonSettings.IgnoreReferenceLoop));
                if (query == null)
                {
                    await _leadDetailsRepository.RollbackTransactionAsync();
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Activity not found."
                    };
                }

                if (query.IsDone?? false)
                {
                    await _leadDetailsRepository.RollbackTransactionAsync();
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Activity is already completed."
                    };
                }

                query.IsDone = true;
                query.StatusID = statusId;
                query.UpdatedAt = DateTime.Now;
                query.UpdatedBy = model.UpdatedBy;
                query.LIP = model.LIP;
                query.LMAC = model.LMAC;
                await _leadDetailsRepository.UpdateAsync(query);
                await _leadDetailsRepository.CommitTransactionAsync();
                var afterEntity = JsonConvert.DeserializeObject<LeadDetailsVM>(JsonConvert.SerializeObject(query, JsonSettings.IgnoreReferenceLoop));

                await _userInfoService.ActionLogAsync("LeadDetails", ActionName.DataUpdated, beforeEntity, afterEntity, query.LeadDetailID, model);

                return new ReturnView
                {
                    Success = true,
                    Message = "Activity completed successfully."
                };
            }
            catch (Exception e)
            {
                await _leadDetailsRepository.RollbackTransactionAsync();

                #if DEBUG
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Error: " + e.Message
                    };
                #else
                    return new ReturnView 
                    { 
                        Success = false, 
                        Message = "An error occurred while completing the activity." 
                    };
                #endif
            }
        }
        #endregion


        private async Task<int> GetStatusId (string statusName)
        {
            int statudId = await _statusesRepository.AllActive().AsNoTracking().Where(x => x.StatusName == statusName).Select(x => x.StatusID).FirstOrDefaultAsync();

            if (statudId == 0)
            {
                Statuses newObj = new Statuses
                {
                    StatusName = statusName,
                    CreatedAt = DateTime.Now,
                };
                await _statusesRepository.AddAsync(newObj);
                statudId = newObj.StatusID;
            }

            return statudId;
        }

        #region GetLeadDetailsInfoAsync
        public async Task<ReturnDataView<LeadActivityVM>> GetLeadDetailsInfoAsync(int activityId)
        {
            try
            {
                var result = await _leadDetailsGenericRepository
                    .AllActive()
                    .AsNoTracking()
                    .Include(x => x.LeadActivityType)
                    .Where(x => x.LeadDetailID == activityId)
                    .Select(x => new LeadActivityVM
                    {
                        LeadDetailID = x.LeadDetailID,
                        LeadActivityName = x.LeadActivityType.LeadActivityName ?? string.Empty,
                        ActivityNote = x.ActivityNote,
                        EmailAddress = x.EmailAddress,
                        PhoneNumber = x.PhoneNumber,
                        FileLink = x.FileLink,
                        ActivityDateTime = x.ActivityDateTime
                    })
                    .FirstOrDefaultAsync();

                return new ReturnDataView<LeadActivityVM>
                {
                    success = true,
                    data = result != null
                        ? new List<LeadActivityVM> { result }
                        : new List<LeadActivityVM>()
                };
            }
            catch (Exception ex)
            {
                return new ReturnDataView<LeadActivityVM>
                {
                    success = false,
                    message = ex.Message,
                    data = new List<LeadActivityVM>()
                };
            }
        }
        #endregion
    }
}
