using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Hosting;
using OfficeOpenXml.Export.ToDataTable;
using System.Web.Mvc;
using System.Web.Helpers;


namespace GCTL.Service.CRM.LeadDetails
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
                //var items = new[] {
                //    new { pririosValue = 0,priodText = "Phone"},
                //    new { pririosValue = 1,priodText = "Phone"}
                //    };

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

        //public async Task<bool> CreateLeadDeatil(LeadDetailsVM leadDetailsVM, string? fileLocation)
        //{

        //    if (!leadDetailsVM.ActivityDateTime.HasValue)
        //    {
        //        return false; 
        //    }
        //    await _leadDetailsGenericRepository.BeginTransactionAsync();
        //    // Convert local time to UTC
        //    var localDateTime = DateTime.SpecifyKind(leadDetailsVM.ActivityDateTime.Value, DateTimeKind.Local);
        //    var utcDateTime = localDateTime.ToUniversalTime();

        //    var leadTypeObj = await _leadActivityTypesGenericRepository.FirstOrDefaultAsync(u => u.LeadActivityTypeID == leadDetailsVM.LeadActivityTypeID);
        //    var leadTypeObj2 = await _leadActivityTypesGenericRepository.FirstOrDefaultAsync(u => u.LeadActivityName == "Attachment");
        //    var leadTypeID = leadTypeObj.LeadActivityTypeID;
        //    bool checkImageValidation = leadTypeObj.LeadActivityName == leadTypeObj2.LeadActivityName;

        //        if (leadTypeID != 0)
        //    {
        //        var leadObj = new GCTL.Data.Models.LeadDetails()
        //        {
        //            LeadID = leadDetailsVM.LeadID,
        //            ActivityDateTime = utcDateTime,
        //            LeadActivityTypeID = leadTypeID,
        //            ActivityNote = leadDetailsVM.ActivityNote,
        //            FileLink = checkImageValidation && fileLocation != null ? fileLocation : null,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = leadDetailsVM.CreatedBy,
        //            LIP = leadDetailsVM.LIP,
        //            LMAC = leadDetailsVM.LMAC,
        //        };
        //        await _leadDetailsGenericRepository.AddAsync(leadObj);

        //    }


        //    return true;
        //    await _leadDetailsGenericRepository.CommitTransactionAsync();
        //    await _leadDetailsGenericRepository.RollbackTransactionAsync();
        //}

        public async Task<bool> CreateLeadDeatil(LeadDetailsVM leadDetailsVM, string? fileLocation)
        {
            if (!leadDetailsVM.ActivityDateTime.HasValue)
            {
                return false;
            }

            await _leadDetailsGenericRepository.BeginTransactionAsync();

            try
            {
                // Convert local time to UTC
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
                    var leadObj = new GCTL.Data.Models.LeadDetails()
                    {
                        LeadID = leadDetailsVM.LeadID,
                        ActivityDateTime = utcDateTime,
                        LeadActivityTypeID = leadTypeID,
                        ActivityNote = leadDetailsVM.ActivityNote,
                        FileLink = checkImageValidation && fileLocation != null ? fileLocation : null,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = leadDetailsVM.CreatedBy,
                        LIP = leadDetailsVM.LIP,
                        LMAC = leadDetailsVM.LMAC,
                    };

                    await _leadDetailsGenericRepository.AddAsync(leadObj);
                }

                // ✅ Commit transaction if all operations succeed
                await _leadDetailsGenericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                // ✅ Rollback transaction on error
                await _leadDetailsGenericRepository.RollbackTransactionAsync();
                // You can log ex here
                return false;
            }
        }

        //ToDo: How to get user id
        // lead table source, status update service function
        public async Task<bool> UpdateLeadFieldValue(DetailsLeadUpdateVM detailsLeadUpdateVM)
        {
            var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == detailsLeadUpdateVM.LeadID);
            //var userId = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (leadObj != null)
            {
                if (detailsLeadUpdateVM.FieldName == "source")
                {
                    leadObj.LeadSourceID = detailsLeadUpdateVM.FieldValue;
                }
                else if (detailsLeadUpdateVM.FieldName == "stage")
                {
                    leadObj.LeadStatusID = detailsLeadUpdateVM.FieldValue;
                }
                else if (detailsLeadUpdateVM.FieldName == "priority")
                {
                    leadObj.PriorityID = detailsLeadUpdateVM.FieldValue;
                }


                leadObj.UpdatedAt = DateTime.UtcNow;
                leadObj.UpdatedBy = detailsLeadUpdateVM.UpdatedBy;
                await _leadsRepository.UpdateAsync(leadObj);
                return true;
            }

            return false;
        }


        // ==============================
        // Save Won or Loss function
        // ==============================

        public async Task<ReturnView> AddIsWon(IsWonVM isWonVM)
        {     


            var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == isWonVM.LeadID);
            var leadTypeObj = await _leadActivityTypesGenericRepository.FirstOrDefaultAsync(u => u.LeadActivityTypeID == isWonVM.LeadActivityTypeID);
            var leadTypeID = leadTypeObj.LeadActivityTypeID;
            var isWon = leadTypeObj.LeadActivityName == "Won";
            if (leadTypeID != 0)
            {
                if(leadObj.IsWwn != isWon)
                {
                    var leadActivityObj = new GCTL.Data.Models.LeadDetails()
                    {
                        LeadID = leadObj.LeadID,
                        ActivityDateTime = DateTime.UtcNow,
                        LeadActivityTypeID = leadTypeID,
                        ActivityNote = isWonVM.ActivityNote,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = isWonVM.CreatedBy,
                        LIP = isWonVM.LIP,
                        LMAC = isWonVM.LMAC,
                    };
                    await _leadDetailsGenericRepository.AddAsync(leadActivityObj);

                    leadObj.IsWwn = leadTypeObj.LeadActivityName == "Won" ? true : false;
                    leadObj.ClosingDate = DateTime.UtcNow;
                    leadObj.UpdatedAt = DateTime.UtcNow;
                    leadObj.UpdatedBy = isWonVM.UpdatedBy;

                    await _leadsRepository.UpdateAsync(leadObj);

                    return new ReturnView
                    {
                        Success = true,
                        Message = "Lead status updated to " + leadTypeObj.LeadActivityName
                    };
                }

                return new ReturnView
                {
                    Success = false,
                    Message = "You status is already " + leadTypeObj.LeadActivityName
                };
            }
            return new ReturnView
            {
                Success = false,
                Message = "Something went wrong"
            };
        }
    }
}
