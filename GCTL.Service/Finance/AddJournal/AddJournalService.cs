using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GCTL.Service.Finance.AddJournal
{
    public class AddJournalService : AppService<Journals>, IAddJournalService
    {
        #region Repositories
        private readonly IGenericRepository<Journals> _genericRepository;
        private readonly IGenericRepository<JournalDetails> _journalDetails;
        private readonly IGenericRepository<PostingRules> _postingRules;
        private readonly IGenericRepository<PostingRuleDetails> _postingRuleDetails;

        public AddJournalService(IGenericRepository<Journals> genericRepository, IGenericRepository<JournalDetails> journalDetails, IGenericRepository<PostingRules> postingRules, IGenericRepository<PostingRuleDetails> postingRuleDetails) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _journalDetails = journalDetails;
            _postingRules = postingRules;
            _postingRuleDetails = postingRuleDetails;
        }
        #endregion


        #region AddAsync
        public async Task<CommonReturnViewModel> AddAsync(CreateAddJournalVM model)
        {
            var result = new CommonReturnViewModel();
            try
            {
                decimal? totalDebit = 0;
                decimal? totalCredit = 0;

                if(model.CreateJournalDetailsVMs != null)
                {
                    foreach(var details in model.CreateJournalDetailsVMs)
                    {
                        if(details?.TrxType == "Debit")
                        {
                            totalDebit += details.Amount;
                        }
                        else if(details?.TrxType == "Credit")
                        {
                            totalCredit += details.Amount;
                        }
                    }
                }

                if(totalDebit != totalCredit)
                {
                    result.Success = false;
                    result.Message = "Debit & Credit amounts not match.";
                    return result;
                }

                await _genericRepository.BeginTransactionAsync();

                Journals entity = new Journals();
                entity.JournalCode = model.JournalCode;
                entity.JournalTypeID = model.JournalTypeID;
                entity.PostingRuleID = model.PostingRuleID;
                entity.FinancialYearID = model.FinancialYearID;
                entity.JournalDate = model.JournalDate;
                entity.Note = model.Note;
                //entity.FileLink = model.FileLink;

                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = model.CreatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                if (model.FileLink != null && model.FileLink.Length > 0)
                {
                    // Get uploads folder
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/media/finance/journals");

                    // Create folder if not exists
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    // Create unique filename
                    var uniqueFileName = $"{model.JournalCode}_{Path.GetFileName(model.FileLink.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the file to server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FileLink.CopyToAsync(stream);
                    }

                    // Store relative path for database (so it works in web URLs)
                    entity.FileLink = $"/media/finance/journals/{uniqueFileName}";
                }

                entity.JournalDetails = new List<JournalDetails>();
                if(model.CreateJournalDetailsVMs != null)
                {
                    foreach(var details in model.CreateJournalDetailsVMs)
                    {
                        JournalDetails journalDetails = new JournalDetails();
                        journalDetails.Description = details.Description;
                        journalDetails.TrxType = details.TrxType;
                        journalDetails.Amount = details.Amount;

                        journalDetails.CreatedAt = DateTime.UtcNow;
                        journalDetails.CreatedBy = model.CreatedBy;
                        journalDetails.LIP = model.LIP;
                        journalDetails.LMAC = model.LMAC;

                        await _journalDetails.AddAsync(journalDetails);
                    }
                }

                await _genericRepository.AddAsync(entity);

                await _genericRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Saved Successfully";

                return result;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "Error Saving Data!";
                result.Errors.Add(ex.Message);
                throw;
            }
        }
        #endregion


        #region IsCodeUniqueAsync
        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            try
            {
                code = code.Trim();
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.JournalID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.JournalCode.Trim() == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the journal code uniqueness.", ex);
            }
        }
        #endregion


        #region GenerateThreeDigitCodeAsync
        public async Task<string> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var lastCode = await _genericRepository.AllActive().OrderByDescending(x => x.JournalCode).Select(x => x.JournalCode).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(lastCode))
                {
                    return "001";
                }
                var lastNumber = int.Parse(lastCode.Substring(1));

                var nextCode = lastNumber + 1;

                if (nextCode > 999)
                    throw new InvalidOperationException("Maximum 3-digit code limit reached.");

                return nextCode.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while generating the three-digit code.", ex);
            }
        }
        #endregion


        #region GetDataByPostingRuleID
        public async Task<GetByPostingRuleIdVM> GetDataByPostingRuleID(int id)
        {
            try
            {
                var data = await _postingRules.AllActive()
                    .Include(x => x.PostingRuleDetails)
                    .ThenInclude(x => x.SubAccount)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PostingRuleID == id);

                return new GetByPostingRuleIdVM
                {
                    PostingRuleID = data.PostingRuleID,
                    ScenarioName = data.ScenarioName,
                    ScenarioCode = data.ScenarioCode,
                    GetByIdPostingRuleDetailsVMs = data.PostingRuleDetails.Select(d => new GetByIdPostingRuleDetailsVM
                    {
                        PostingRuleDetailID = d.PostingRuleDetailID,
                        MainAccountID = d.SubAccount.MainAccountID,
                        SubAccID = d.SubAccountID,
                        TrxAccID = d.TrxAccID,
                        DebitCredit = d.TrxType
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion
    }
}
