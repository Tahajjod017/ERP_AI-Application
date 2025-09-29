using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Finance.BaseAccountVM;
using GCTL.Core.ViewModels.MasterSetup.Country;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Finance.BaseAccount
{
    public class BaseAccountService : AppService<BaseAccounts>, IBaseAccountService
    {
        #region Repositories & Services
        public readonly IGenericRepository<BaseAccounts> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public BaseAccountService(IGenericRepository<BaseAccounts> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreateBaseAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.BaseAccountName.ToLower() == model.BaseAccountName.ToLower() && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.BaseAccountCode = model.BaseAccountCode;
                    exixtingEntity.BaseAccountName = model.BaseAccountName;
                    exixtingEntity.Description = model.Description;

                    exixtingEntity.CreatedAt = DateTime.UtcNow;
                    exixtingEntity.CreatedBy = model.CreatedBy;
                    exixtingEntity.LIP = model.LIP;
                    exixtingEntity.LMAC = model.LMAC;

                    exixtingEntity.DeletedAt = null;
                    exixtingEntity.DeletedBy = null;
                    exixtingEntity.UpdatedAt = null;
                    exixtingEntity.UpdatedBy = null;

                    await _genericRepository.UpdateAsync(exixtingEntity);
                    await _userInfoService.ActionLogAsync("Base Account", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.BaseAccountID, model);
                }
                else
                {
                    BaseAccounts entity = new BaseAccounts();
                    entity.BaseAccountCode = model.BaseAccountCode;
                    entity.BaseAccountName = model.BaseAccountName;
                    entity.Description = model.Description;

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Base Account", ActionName.DataAdd, null, entity, entity.BaseAccountID, model);
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
        public async Task<bool> UpdateAsync(UpdateBaseAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.GetByIdAsync(model.BaseAccountID);
                if (entity == null)
                    return false;

                var beforeEntity = JsonConvert.DeserializeObject<UpdateBaseAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.BaseAccountCode = model.BaseAccountCode;
                entity.BaseAccountName = model.BaseAccountName;
                entity.Description = model.Description;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateBaseAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Base Account", ActionName.DataUpdated, beforeEntity, afterEntity, entity.BaseAccountID, model);

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


        #region GetAllAsync
        public async Task<PaginationService<BaseAccounts, GetAllBaseAccountVM>.PaginationResult<GetAllBaseAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "BaseAccountID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive();

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "BaseAccountID" => sortOrder == "desc" ? query.OrderByDescending(x => x.BaseAccountID) : query.OrderBy(x => x.BaseAccountID),
                        "BaseAccountCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.BaseAccountCode) : query.OrderBy(x => x.BaseAccountCode),
                        "BaseAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.BaseAccountName) : query.OrderBy(x => x.BaseAccountName),
                        "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                        _ => query.OrderBy(x => x.BaseAccountID)
                    };
                }

                return await PaginationService<BaseAccounts, GetAllBaseAccountVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.BaseAccountCode, $"%{term}%") || EF.Functions.Like(x.BaseAccountName, $"%{term}%") || EF.Functions.Like(x.Description, $"%{term}%"),
                    x => new GetAllBaseAccountVM
                    {
                        BaseAccountID = x.BaseAccountID,
                        BaseAccountCode = x.BaseAccountCode ?? "-",
                        BaseAccountName = x.BaseAccountName ?? "-",
                        Description = x.Description ?? "-"
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Base Accounts.", ex);
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<GetByIdBaseAccountVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);

                return new GetByIdBaseAccountVM
                {
                    BaseAccountID = data.BaseAccountID,
                    BaseAccountCode = data.BaseAccountCode ?? "-",
                    BaseAccountName = data.BaseAccountName ?? "-",
                    Description = data.Description ?? "-"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the Base Account.", ex);
            }
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<DeleteBaseAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.BaseAccountID));
                if (data == null || data.Count == 0)
                {
                    return new DeleteBaseAccountVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DeleteBaseAccountVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.BaseAccountID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Base Account", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DeleteBaseAccountVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("An error occurred while deleting the Base Account.", ex);
            }
        }
        #endregion


        #region Others
        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            try
            {
                name = name.ToLower();
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.BaseAccountID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.BaseAccountName.ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Base Account name uniqueness.", ex);
            }
        }
        #endregion
    }
}
