using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using GCTL.Core.ViewModels.Finance.TransactionAccountVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.PostingRule
{
    public class PostingRulesService : AppService<PostingRules>, IPostingRulesService
    {
        #region Repositories & Services
        private readonly IGenericRepository<PostingRules> _genericRepository;
        private readonly IGenericRepository<PostingRuleDetails> _postingRuleDetailsRepository;
        private readonly IGenericRepository<MenuTab> _menuTabRepository;
        private readonly IUserInfoService _userInfoService;


        public PostingRulesService(IGenericRepository<PostingRules> genericRepository, IGenericRepository<MenuTab> menuTabRepository, IUserInfoService userInfoService, IGenericRepository<PostingRuleDetails> postingRuleDetailsRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _menuTabRepository = menuTabRepository;
            _userInfoService = userInfoService;
            _postingRuleDetailsRepository = postingRuleDetailsRepository;
        }
        #endregion


        #region GetBodyTabsAsync
        public async Task<List<MenuTab>> GetBodyTabsAsync()
        {
            try
            {
                var allowedControllers = new[] { "AddMainAccount", "AddSubAccount", "TransactionAccount", "PostingRules" };

                var menuTabs = await _menuTabRepository.AllActive()
                    .Where(mt => allowedControllers.Contains(mt.ControllerName) && !mt.IsActive)
                    //.OrderBy(mt => mt.TabOrder)
                    .ToListAsync();
                return menuTabs;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu tabs.", ex);
            }
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreatePostingRulesVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.ScenarioName.Trim().ToLower() == model.ScenarioName.Trim().ToLower() && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.ScenarioName = model.ScenarioName;
                    exixtingEntity.ScenarioCode = model.ScenarioCode.Trim();

                    exixtingEntity.CreatedAt = DateTime.UtcNow;
                    exixtingEntity.CreatedBy = model.CreatedBy;
                    exixtingEntity.LIP = model.LIP;
                    exixtingEntity.LMAC = model.LMAC;

                    exixtingEntity.DeletedAt = null;
                    exixtingEntity.DeletedBy = null;
                    exixtingEntity.UpdatedAt = null;
                    exixtingEntity.UpdatedBy = null;

                    await _genericRepository.UpdateAsync(exixtingEntity);
                    await _userInfoService.ActionLogAsync("Posting Rules", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.PostingRuleID, model);
                }
                else
                {
                    PostingRules entity = new PostingRules();
                    entity.ScenarioName = model.ScenarioName;
                    entity.ScenarioCode = await GenerateThreeDigitCodeAsync();

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    //await _genericRepository.AddAsync(entity);
                    //await _userInfoService.ActionLogAsync("Posting Rules", ActionName.DataAdd, null, entity, entity.PostingRuleID, model);

                    entity.PostingRuleDetails = new List<PostingRuleDetails>();

                    if (model.PostingRuleDetailsVMs != null)
                    {
                        foreach (var detailVM in model.PostingRuleDetailsVMs)
                        {
                            PostingRuleDetails detailEntity = new PostingRuleDetails();
                            //detailEntity.PostingRuleID = entity.PostingRuleID;
                            detailEntity.SubAccountID = detailVM.SubAccID;
                            detailEntity.TrxAccID = detailVM.TrxAccID;
                            detailEntity.TrxType = detailVM.DebitCredit;

                            entity.PostingRuleDetails.Add(detailEntity);
                        }
                    }

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Posting Rules", ActionName.DataAdd, null, entity, entity.PostingRuleID, model);


                    //// Prepare the details list
                    //var details = model.PostingRuleDetailsVMs?
                    //    .Select(detailVM => new PostingRuleDetails
                    //    {
                    //        PostingRuleID = entity.PostingRuleID,  // FK set here
                    //        SubAccountID = detailVM.SubAccID,
                    //        TrxAccID = detailVM.TrxAccID,
                    //        TrxType = detailVM.DebitCredit
                    //    }).ToList();

                    //// Add all details at once
                    //if (details != null && details.Any())
                    //{
                    //    await _postingRuleDetailsRepository.AddRangeAsync(details);
                    //}
                }

                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
                throw;
            }
        }
        #endregion


        #region UpdateAsync
        public async Task<CommonReturnViewModel> UpdateAsync(UpdatePostingRulesVM model)
        {
            var result = new CommonReturnViewModel();

            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.AllActive()
                    .Include(x => x.PostingRuleDetails)
                    .FirstOrDefaultAsync(x => x.PostingRuleID == model.PostingRuleID);

                if (entity == null)
                {
                    result.Success = false;
                    result.Message = "Posting Rule not found.";
                    return result;
                }

                //if (model.SubAccountID != entity.SubAccountID)
                //{
                //    result.Success = false;
                //    result.Message = "You cannot change the sub account!";
                //    return result;
                //}

                var beforeEntity = JsonConvert.DeserializeObject<UpdatePostingRulesVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.ScenarioName = model.ScenarioName.Trim();

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                // Get IDs from the model (for matching)
                var modelDetailIds = model.PostingRuleDetailsVMs?
                    .Where(d => d.PostingRuleDetailID > 0)
                    .Select(d => d.PostingRuleDetailID)
                    .ToList() ?? new List<int>();

                // 1️⃣ Remove deleted child records
                var detailsToRemove = entity.PostingRuleDetails
                    .Where(d => !modelDetailIds.Contains(d.PostingRuleDetailID))
                    .ToList();

                if (detailsToRemove.Any())
                {
                    await _postingRuleDetailsRepository.DeleteRangeAsync(detailsToRemove);
                }

                // 2️⃣ Update existing / Add new details
                foreach (var detailVM in model.PostingRuleDetailsVMs)
                {
                    if (detailVM.PostingRuleDetailID > 0)
                    {
                        // Update existing record
                        var existingDetail = entity.PostingRuleDetails
                            .FirstOrDefault(d => d.PostingRuleDetailID == detailVM.PostingRuleDetailID);

                        if (existingDetail != null)
                        {
                            existingDetail.SubAccountID = detailVM.SubAccID;
                            existingDetail.TrxAccID = detailVM.TrxAccID;
                            existingDetail.TrxType = detailVM.DebitCredit;

                            await _postingRuleDetailsRepository.UpdateAsync(existingDetail);
                        }
                    }
                    else
                    {
                        // Add new record
                        var newDetail = new PostingRuleDetails
                        {
                            PostingRuleID = entity.PostingRuleID,
                            SubAccountID = detailVM.SubAccID,
                            TrxAccID = detailVM.TrxAccID,
                            TrxType = detailVM.DebitCredit
                        };

                        await _postingRuleDetailsRepository.AddAsync(newDetail);
                    }
                }


                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdatePostingRulesVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Posting Rules", ActionName.DataUpdated, beforeEntity, afterEntity, entity.PostingRuleID, model);

                await _genericRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Updated Successfully.";
                return result;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while updating the main account.";
                result.Errors.Add(ex.Message);
                return result;
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<GetByIdPostingRulesVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.AllActive()
                    .Include(x => x.PostingRuleDetails)
                    .ThenInclude(x => x.SubAccount)
                    .ThenInclude(x => x.MainAccount)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.PostingRuleID == id);

                if (data == null)
                    throw new Exception($"PostingRule with ID {id} not found.");

                var result = new GetByIdPostingRulesVM
                {
                    PostingRuleID = data.PostingRuleID,
                    ScenarioName = data.ScenarioName,
                    ScenarioCode = data.ScenarioCode,
                    PostingRuleDetailsVMs = data.PostingRuleDetails?.Select(detail => new GetByIdPostingRulesDetailsVM
                    {
                        PostingRuleDetailID = detail.PostingRuleDetailID,
                        PostingRuleID = detail.PostingRuleID,
                        SubAccID = detail.SubAccountID,
                        MainAccountID = detail.SubAccount?.MainAccount?.MainAccountID,
                        TrxAccID = detail.TrxAccID,
                        TrxType = detail.TrxType
                    }).ToList() ?? new List<GetByIdPostingRulesDetailsVM>()
                };

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the Transaction Account.", ex);
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<PostingRules, GetAllPostingRulesVM>.PaginationResult<GetAllPostingRulesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PostingRuleID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive()
                    .Include(x => x.PostingRuleDetails)
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "PostingRuleID" => sortOrder == "desc" ? query.OrderByDescending(x => x.PostingRuleID) : query.OrderBy(x => x.PostingRuleID),
                        "ScenarioName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ScenarioName) : query.OrderBy(x => x.ScenarioName),
                        "ScenarioCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.ScenarioCode) : query.OrderBy(x => x.ScenarioCode),
                        _ => query.OrderBy(x => x.PostingRuleID)
                    };
                }

                return await PaginationService<PostingRules, GetAllPostingRulesVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.ScenarioName, $"%{term}%")
                    || EF.Functions.Like(x.ScenarioCode, $"%{term}%"),
                    x => new GetAllPostingRulesVM
                    {
                        PostingRuleID = x.PostingRuleID,
                        ScenarioName = x.ScenarioName,
                        ScenarioCode = x.ScenarioCode
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Transaction Accounts.", ex);
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            try
            {
                name = name.Trim().ToLower();
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.PostingRuleID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.ScenarioName.Trim().ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the scenario name uniqueness.", ex);
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
                    query = query.Where(x => x.PostingRuleID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.ScenarioCode.Trim() == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the scenario code uniqueness.", ex);
            }
        }
        #endregion


        #region GenerateThreeDigitCodeAsync
        public async Task<string> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var lastCode = await _genericRepository.AllActive().OrderByDescending(x => x.ScenarioCode).Select(x => x.ScenarioCode).FirstOrDefaultAsync();
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
