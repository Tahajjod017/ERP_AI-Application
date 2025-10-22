using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Finance.PostingRulesVM;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
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
