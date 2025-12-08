using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Sales.PriceQuotation;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Sales.PriceQuotation
{
    public class PriceQuotationService : IPriceQuotation
    {
        private readonly IGenericRepository<PriceQuotations> _priceQuotationRepository;
        private readonly IGenericRepository<PriceQuotationVersionItems> _priceQuotationItemRepository;
        private readonly IGenericRepository<PriceQuotationVersions> _priceQuotationVersionsRepository;
        private readonly IUserInfoService _userInfoService;

        public PriceQuotationService(IGenericRepository<PriceQuotations> priceQuotationRepository, IGenericRepository<PriceQuotationVersionItems> priceQuotationItemRepository, IUserInfoService userInfoService, IGenericRepository<PriceQuotationVersions> priceQuotationVersionsRepository)
        {
            _priceQuotationRepository = priceQuotationRepository;
            _priceQuotationItemRepository = priceQuotationItemRepository;
            _userInfoService = userInfoService;
            _priceQuotationVersionsRepository = priceQuotationVersionsRepository;
        }

        public async Task<string> GetNextPQcode()
        {
            var lastQuote = await _priceQuotationRepository.AllActive()
                .OrderByDescending(i => i.QuotationNumber)
                .Select(i => i.QuotationNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastQuote) && lastQuote.StartsWith("QUO"))
            {
                string numericPart = lastQuote.Substring(3);
                if (int.TryParse(numericPart, out int parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            string nextQuote = $"QUO{nextNumber.ToString("D5")}";
            return nextQuote;
        }


        public async Task<CommonReturnViewModel> SaveAsync(PriceQuotationViewModel vm)
        {
            await _priceQuotationItemRepository.BeginTransactionAsync();

            try
            {
                // ============================================================
                // 1️⃣ CHECK IF EXISTING VERSION
                // ============================================================
                var prevVersion = await _priceQuotationVersionsRepository.AllActive()
                    .FirstOrDefaultAsync(e => e.PriceQuotationVersionID == vm.Id);

                if (prevVersion != null && prevVersion.IsDraft != false)
                {
                    // 🔹 Update header fields
                    prevVersion.CustomerID = vm.SelectedCustomerId;
                    prevVersion.QuotationDate = vm.InvoiceDate;
                    prevVersion.OtherNumber = vm.OtherNumber;
                    prevVersion.Note = vm.Note;
                    prevVersion.VatPercentage = vm.RetentionPercent;
                    prevVersion.IsDraft = vm.IsDraft;
                    prevVersion.UpdatedAt = DateTime.Now;
                    prevVersion.UpdatedBy = vm.CreatedBy;

                    // 🔹 Remove old items
                    var existingItems = await _priceQuotationItemRepository.All()
                        .Where(i => i.PriceQuotationVersionID == prevVersion.PriceQuotationVersionID)
                        .ToListAsync();

                    await _priceQuotationItemRepository.DeleteRangeAsync(existingItems);

                    // 🔹 Add new items
                    foreach (var itemVm in vm.Items)
                    {
                        var newItem = new PriceQuotationVersionItems
                        {
                            PriceQuotationVersionID = prevVersion.PriceQuotationVersionID,
                            Description = itemVm.Description,
                            UnitTypeID = Convert.ToInt32(itemVm.Unit),
                            Area = itemVm.Area,
                            Rate = itemVm.Rate,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _priceQuotationItemRepository.AddAsync(newItem);
                    }

                    await _priceQuotationVersionsRepository.UpdateAsync(prevVersion);
                    await _priceQuotationItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Save Successful",
                        Data = prevVersion.PriceQuotationVersionID
                    };
                }
                else
                {
                    // ============================================================
                    // 2️⃣ CREATE NEW QUOTATION + VERSION
                    // ============================================================

                    // 🔹 Find latest quotation series
                    var previousQuotation = await _priceQuotationRepository.AllActive().Include(e=>e.PriceQuotationVersions)
                        .Where(q => q.QuotationNumber == vm.InvoiceNumber)
                        
                        .FirstOrDefaultAsync();

                    int version = (previousQuotation?.PriceQuotationVersions.Count ?? 0) + 1;
                    bool latest = true;

                    if (previousQuotation == null)
                    {
                        // New quotation series
                        vm.InvoiceNumber = await GetNextPQcode();
                        version = 1;
                        latest = true;

                        previousQuotation = new PriceQuotations
                        {
                            QuotationNumber = vm.InvoiceNumber,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _priceQuotationRepository.AddAsync(previousQuotation);
                    }
                    //else
                    //{
                    //    // Existing quotation → new version
                    //    vm.InvoiceNumber = previousQuotation.QuotationNumber;
                    //    version = previousQuotation.PriceQuotationVersions.Count + 1;
                    //    latest = false;
                    //}

                    // 🔹 Create new version
                    var newVersion = new PriceQuotationVersions
                    {
                        PriceQuotationID = previousQuotation.PriceQuotationID,
                        CustomerID = vm.SelectedCustomerId,
                        QuotationDate = vm.InvoiceDate,
                        OtherNumber = vm.OtherNumber,
                        Note = vm.Note,
                        VatPercentage = vm.RetentionPercent,
                        Version = version,
                        IsFinalVersion = true , // latest,
                        IsDraft = vm.IsDraft,
                        CreatedAt = DateTime.Now,
                        CreatedBy = vm.CreatedBy
                    };

                    var updatedQuotationVer = await _priceQuotationVersionsRepository.AllActive().Where(q => q.PriceQuotationID == previousQuotation.PriceQuotationID).ToListAsync();
                    updatedQuotationVer.ForEach(qv => qv.IsFinalVersion = false);
                    await _priceQuotationVersionsRepository.UpdateRangeAsync(updatedQuotationVer);

                    await _priceQuotationVersionsRepository.AddAsync(newVersion);

                    // 🔹 Add items
                    foreach (var itemVm in vm.Items)
                    {
                        var modelItem = new PriceQuotationVersionItems
                        {
                            PriceQuotationVersionID = newVersion.PriceQuotationVersionID,
                            Description = itemVm.Description,
                            UnitTypeID = Convert.ToInt32(itemVm.Unit),
                            Area = itemVm.Area,
                            Rate = itemVm.Rate,
                            CreatedAt = DateTime.Now,
                            CreatedBy = vm.CreatedBy
                        };

                        await _priceQuotationItemRepository.AddAsync(modelItem);
                    }

                    await _priceQuotationItemRepository.CommitTransactionAsync();

                    return new CommonReturnViewModel
                    {
                        Success = true,
                        Message = "Save Successful",
                        Data = newVersion.PriceQuotationVersionID
                    };
                }
            }
            catch (Exception ex)
            {
                await _priceQuotationItemRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }


        //public async Task<CommonReturnViewModel> SaveAsync(PriceQuotationViewModel vm)
        //{
        //    await _priceQuotationItemRepository.BeginTransactionAsync();

        //    try
        //    {

        //        var prev = await _priceQuotationRepository.AllActive().FirstOrDefaultAsync(e => e.PriceQuotationID == vm.Id);

        //        if (prev != null )
        //        {
        //            //prev.CustomerID = vm.SelectedCustomerId;
        //            //prev.QuotationDate = vm.InvoiceDate;
        //            //prev.QuotationNumber = vm.InvoiceNumber;
        //            //prev.OtherNumber = vm.OtherNumber;
        //            ////prev.Version = version,
        //            //prev.Note = vm.Note;
        //            //prev.VatPercentage = vm.RetentionPercent;

        //            ////prev.IsFinalVersion = latest,
        //            ////prev.ParentQuotationID = parentQuotationID,
        //            //prev.IsDraft = vm.IsDraft;

        //            // Load existing items
        //            var existingItems = await _priceQuotationItemRepository.All().Where(i => i.PriceQuotationVersionID == prev.PriceQuotationID).ToListAsync();

        //            await _priceQuotationItemRepository.DeleteRangeAsync(existingItems);

        //            // Update or add items
        //            foreach (var itemVm in vm.Items)
        //            {

        //                // Add new
        //                var newItem = new PriceQuotationVersionItems
        //                {
        //                    PriceQuotationVersionID = prev.PriceQuotationID,
        //                    Description = itemVm.Description,
        //                    UnitTypeID = Convert.ToInt32(itemVm.Unit),
        //                    Area = itemVm.Area,
        //                    Rate = itemVm.Rate,
        //                    CreatedAt = DateTime.Now,
        //                    CreatedBy = vm.CreatedBy
        //                };

        //                await _priceQuotationItemRepository.AddAsync(newItem);

        //            }

        //            //prev.PriceQuotationItems = vm.Items.Select(e => new PriceQuotationItems
        //            //{

        //            //    Description = e.Description,
        //            //    UnitTypeID = Convert.ToInt32(e.Unit),
        //            //    Area = e.Area,
        //            //    Rate = e.Rate,
        //            //    UpdatedAt = DateTime.Now,
        //            //    UpdatedBy = vm.CreatedBy
        //            //});

        //            await _priceQuotationRepository.UpdateAsync(prev);

        //            await _priceQuotationItemRepository.CommitTransactionAsync();


        //            return new CommonReturnViewModel
        //            {
        //                Success = true,
        //                Message = "Save Successful",
        //                Data = prev.PriceQuotationID
        //            };

        //        }
        //        else
        //        {
        //            // ============================================================
        //            // 1️⃣ GET PREVIOUS LATEST VERSION (IF ANY)
        //            // ============================================================
        //            var previousLatest = await _priceQuotationRepository.AllActive()
        //                .Where(q => q.QuotationNumber == vm.InvoiceNumber)
        //                .OrderByDescending(q => q.PriceQuotationVersions)
        //                .FirstOrDefaultAsync();

        //            int version = 1;
        //            bool latest = true;
        //            int? parentQuotationID = null;

        //            // ============================================================
        //            // 2️⃣ CASE: NEW QUOTATION SERIES → GENERATE NEW NUMBER
        //            // ============================================================
        //            if (previousLatest == null)
        //            {
        //                vm.InvoiceNumber = GetNextPQcode().Result;
        //                version = 1;
        //                latest = true;
        //            }
        //            else
        //            {
        //                // ============================================================
        //                // 3️⃣ CASE: EXISTING QUOTATION → CREATE NEW VERSION
        //                // ============================================================
        //                vm.InvoiceNumber = previousLatest.QuotationNumber;   // keep same series
        //                version = previousLatest.PriceQuotationVersions.Count + 1;                // increment version
        //              //  parentQuotationID = previousLatest.ParentQuotationID ?? previousLatest.PriceQuotationID;

        //                latest = false;


        //                //previousLatest.IsFinalVersion = false;
        //                //await _priceQuotationRepository.UpdateAsync(previousLatest);
        //            }

        //            // ============================================================
        //            // 4️⃣ CREATE NEW QUOTATION HEADER
        //            // ============================================================
        //            var quotation = new PriceQuotations
        //            {
        //                //CustomerID = vm.SelectedCustomerId,
        //                //QuotationDate = vm.InvoiceDate,
        //                //QuotationNumber = vm.InvoiceNumber,
        //                //OtherNumber = vm.OtherNumber,
        //                //Version = version,
        //                //Note = vm.Note,
        //                //VatPercentage = vm.RetentionPercent,

        //                //IsFinalVersion = latest,
        //                //ParentQuotationID = parentQuotationID,
        //                //IsDraft = vm.IsDraft,

        //                CreatedAt = DateTime.Now,
        //                CreatedBy = vm.CreatedBy
        //            };



        //            await _priceQuotationRepository.AddAsync(quotation);
        //            await _userInfoService.ActionLogAsync("PriceQuotation", ActionName.DataAdd, null, quotation, quotation.PriceQuotationID, vm);


        //            // ============================================================
        //            // 5️⃣ SAVE QUOTATION ITEMS
        //            // ============================================================
        //            foreach (var item in vm.Items)
        //            {
        //                var modelItem = new PriceQuotationVersionItems
        //                {
        //                    PriceQuotationVersionID = quotation.PriceQuotationID,
        //                    Description = item.Description,
        //                    UnitTypeID = Convert.ToInt32(item.Unit),
        //                    Area = item.Area,
        //                    Rate = item.Rate,
        //                    CreatedAt = DateTime.Now,
        //                    CreatedBy = vm.CreatedBy
        //                };

        //                await _priceQuotationItemRepository.AddAsync(modelItem);
        //                await _userInfoService.ActionLogAsync("PriceQuotation", ActionName.DataAdd, null, modelItem, modelItem.PriceQuotationVersionItemID, vm);
        //            }


        //            // ============================================================
        //            // 6️⃣ COMMIT TRANSACTION
        //            // ============================================================
        //            await _priceQuotationItemRepository.CommitTransactionAsync();

        //            return new CommonReturnViewModel
        //            {
        //                Success = true,
        //                Message = "Save Successful",
        //                Data = quotation.PriceQuotationID
        //            };

        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        await _priceQuotationItemRepository.RollbackTransactionAsync();
        //        return new CommonReturnViewModel
        //        {
        //            Success = false,
        //            Message = ex.Message
        //        };
        //    }
        //}







    }
}
