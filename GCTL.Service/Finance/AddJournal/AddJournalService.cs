using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace GCTL.Service.Finance.AddJournal
{
    public class AddJournalService : AppService<Journals>, IAddJournalService
    {
        private readonly IGenericRepository<Journals> _genericRepository;
        private readonly IGenericRepository<JournalDetails> _journalDetails;

        public AddJournalService(IGenericRepository<Journals> genericRepository, IGenericRepository<JournalDetails> journalDetails) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _journalDetails = journalDetails;
        }


        #region AddAsync
        public async Task<CommonReturnViewModel> AddAsync(CreateAddJournalVM model)
        {
            var result = new CommonReturnViewModel();
            try
            {
                await _genericRepository.BeginTransactionAsync();

                Journals entity = new Journals();
                entity.JournalCode = model.JournalCode;
                entity.JournalTypeID = model.JournalTypeID;
                entity.PostingRuleID = model.PostingRuleID;
                entity.FinancialYearID = model.FinancialYearID;
                entity.JournalDate = model.JournalDate;
                entity.Note = model.Note;
                entity.FileLink = model.FileLink;

                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = model.CreatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                entity.JournalDetails = new List<JournalDetails>();
                if(model.CreateJournalDetailsVMs != null)
                {
                    foreach(var details in model.CreateJournalDetailsVMs)
                    {
                        JournalDetails journalDetails = new JournalDetails();
                        journalDetails.Description = details.Description;
                        journalDetails.Debit = details.Debit;
                        journalDetails.Credit = details.Credit;

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
                result.Message = "Update Successfully";

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
    }
}
