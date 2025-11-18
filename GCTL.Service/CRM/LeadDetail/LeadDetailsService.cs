using Azure;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;


namespace GCTL.Service.CRM.LeadDetail
{
   
    public class LeadDetailsService : ILeadDetailsService
    {
        private readonly IGenericRepository<LeadActivityTypes> _leadActivityTypesGenericRepository;
        private readonly IGenericRepository<GCTL.Data.Models.LeadDetails> _leadDetailsGenericRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;

        public LeadDetailsService(IGenericRepository<GCTL.Data.Models.LeadDetails> leadDetailsGenericRepository, IGenericRepository<LeadActivityTypes> leadActivityTypesGenericRepository, IGenericRepository<Leads> leadsRepository)
        {
            _leadActivityTypesGenericRepository = leadActivityTypesGenericRepository;
            _leadDetailsGenericRepository = leadDetailsGenericRepository;
            _leadsRepository = leadsRepository;
        }


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

        public async Task<ReturnView> CreateLeadDeatil(LeadDetailsVM leadDetailsVM, string? fileLocation)
        {

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
                var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == leadDetailsVM.LeadID);

                if (leadObj != null)
                {
                    if (leadObj.IsOwn == null && leadObj.ClosingDate == null)
                    {
                        var localDateTime = DateTime.SpecifyKind(leadDetailsVM.ActivityDateTime.Value, DateTimeKind.Local);
                        var utcDateTime = localDateTime.ToUniversalTime();

                        var leadTypeObj = await _leadActivityTypesGenericRepository
                            .FirstOrDefaultAsync(u => u.LeadActivityTypeID == leadDetailsVM.LeadActivityTypeID);

                        var leadTypeObj2 = await _leadActivityTypesGenericRepository
                            .FirstOrDefaultAsync(u => u.LeadActivityName == "Attachment");

                        var leadTypeID = leadTypeObj.LeadActivityTypeID;
                        bool checkImageValidation = leadTypeObj.LeadActivityName == leadTypeObj2.LeadActivityName;

                        

                        if (leadTypeID != 0)
                        {
                            var leadDetailsObj = new GCTL.Data.Models.LeadDetails()
                            {
                                LeadID = leadDetailsVM.LeadID,
                                ActivityDateTime = utcDateTime,
                                LeadActivityTypeID = leadTypeID,
                                ActivityNote = leadDetailsVM.ActivityNote,
                                PhoneNumber = leadDetailsVM.ContactNumber,
                                EmailAddress =  leadDetailsVM.ContactEmail,
                                FileLink = checkImageValidation && fileLocation != null ? fileLocation : null,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = leadDetailsVM.CreatedBy,
                                LIP = leadDetailsVM.LIP,
                                LMAC = leadDetailsVM.LMAC,
                            };

                            await _leadDetailsGenericRepository.AddAsync(leadDetailsObj);
                        }

                        await _leadDetailsGenericRepository.CommitTransactionAsync();
                        return new ReturnView
                        {
                            Success = true,
                            Message = "Activity Saved successfully"

                        };
                    }
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Activity has a state. To reopen click on Restore Button"
                    };
                }
                return new ReturnView
                {
                    Success = false,
                    Message = "Lead Not Found"
                };
            }
            catch (Exception ex)
            {
                await _leadDetailsGenericRepository.RollbackTransactionAsync();
                return new ReturnView
                {
                    Success = false,
                    Message = "Something went to wrong"

                };
            }
        }

        //======================
        // getLeadActivityList
        //====================
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

        // lead table source, status update service function
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

        // ==============================
        // Save Won or Loss function
        // ==============================

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

        //=================================
        // restore lead activity
        //=================================
        public async Task<ReturnView> RestoreLead( int id)
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
            catch(Exception)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = "Somethig went to wrong"
                };
            }

        }

    }
}
