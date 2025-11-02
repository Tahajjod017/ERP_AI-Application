using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddJournalVM;
using GCTL.Core.ViewModels.Finance.JournalDetailsVM;
using GCTL.Core.ViewModels.Finance.PostingRuleDetailsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
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

                if (model.CreateJournalDetailsVMs != null)
                {
                    foreach (var details in model.CreateJournalDetailsVMs)
                    {
                        if (details?.TrxType == "D")
                        {
                            totalDebit += details.Amount;
                        }
                        else if (details?.TrxType == "C")
                        {
                            totalCredit += details.Amount;
                        }
                    }
                }

                if (totalDebit != totalCredit)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Debit & Credit amounts do not match."
                    };
                }

                await _genericRepository.BeginTransactionAsync();

                Journals entity = new Journals();
                entity.JournalCode = await GenerateSixDigitCodeAsync();
                entity.JournalTypeID = model.JournalTypeID;
                entity.PostingRuleID = model.PostingRuleID;
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
                    var uploadsFolder = Path.Combine("wwwroot", "media", "finance", "journals");
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

                await _genericRepository.AddAsync(entity);

                entity.JournalDetails = new List<JournalDetails>();
                if (model.CreateJournalDetailsVMs != null)
                {
                    foreach (var details in model.CreateJournalDetailsVMs)
                    {
                        JournalDetails journalDetails = new JournalDetails();
                        journalDetails.JournalID = entity.JournalID;
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


        #region GetAllAsync
        public async Task<PaginationService<Journals, GetAllAddJournalVM>.PaginationResult<GetAllAddJournalVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "JournalID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive()
                    .Include(x => x.JournalDetails)
                    .Include(x => x.JournalType)
                    .Include(x => x.PostingRule)
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "JournalID" => sortOrder == "desc" ? query.OrderByDescending(x => x.JournalID) : query.OrderBy(x => x.JournalID),
                        "JournalCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.JournalCode) : query.OrderBy(x => x.JournalCode),
                        "JournalTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.JournalType.JournalTypeName) : query.OrderBy(x => x.JournalType.JournalTypeName),
                        "ScenarioName" => sortOrder == "desc" ? query.OrderByDescending(x => x.PostingRule.ScenarioName) : query.OrderBy(x => x.PostingRule.ScenarioName),
                        "Note" => sortOrder == "desc" ? query.OrderByDescending(x => x.Note) : query.OrderBy(x => x.Note),
                        "JournalDate" => sortOrder == "desc" ? query.OrderByDescending(x => x.JournalDate) : query.OrderBy(x => x.JournalDate),
                        _ => query.OrderBy(x => x.PostingRuleID)
                    };
                }

                return await PaginationService<Journals, GetAllAddJournalVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.JournalCode, $"%{term}%")
                    || EF.Functions.Like(x.JournalType.JournalTypeName, $"%{term}%")
                    || EF.Functions.Like(x.PostingRule.ScenarioName, $"%{term}%")
                    || EF.Functions.Like(x.Note, $"%{term}%")
                    || EF.Functions.Like(x.JournalDate, $"%{term}%"),
                    x => new GetAllAddJournalVM
                    {
                        JournalID = x.JournalID,
                        JournalCode = x.JournalCode,
                        JournalType = x.JournalType?.JournalTypeName ?? "-",
                        PostingRule = x.PostingRule?.ScenarioName ?? "-",
                        JournalDate = x.JournalDate.HasValue ? x.JournalDate.Value.ToString("dd/MM/yyyy") : "-",
                        Note = x.Note ?? "-",
                        FileLink = x.FileLink ?? "-",
                        GetAllJournalDetailsVMs = x.JournalDetails
                        .Select(d => d == null ? null : new CreateJournalDetailsVM
                        {
                            JournalDetailID = d.JournalDetailID,
                            TrxType = d.TrxType ?? "-",
                            Amount = d.Amount ?? 0,
                            Description = d.Description ?? "-",
                        }).ToList()
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Transaction Accounts.", ex);
            }
        }
        #endregion


        #region GetJournalDetailsByIdAsync
        public async Task<GetByIdAddJournalVM> GetJournalDetailsByIdAsync(int id)
        {
            try
            {
                var result = await _genericRepository.AllActive()
                    .Include(x => x.JournalDetails)
                    .AsNoTracking()
                    .Where(x => x.JournalID == id)
                    .Select(x => new GetByIdAddJournalVM
                    {
                        JournalID = x.JournalID,
                        GetByIdJournalDetailsVMs = x.JournalDetails
                        .Select(d => d == null ? null : new CreateJournalDetailsVM
                        {
                            JournalDetailID = d.JournalDetailID,
                            TrxType = d.TrxType ?? "-",
                            Amount = d.Amount ?? 0,
                            Description = d.Description ?? "-",
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
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


        #region GenerateSixDigitCodeAsync
        public async Task<string> GenerateSixDigitCodeAsync()
        {
            try
            {
                var lastCode = await _genericRepository.AllActive().OrderByDescending(x => x.JournalCode).Select(x => x.JournalCode).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(lastCode))
                {
                    return "000001";
                }
                var lastNumber = int.Parse(lastCode.Substring(1));

                var nextCode = lastNumber + 1;

                if (nextCode > 999999)
                    throw new InvalidOperationException("Maximum 6-digit code limit reached.");

                return nextCode.ToString("D6");
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