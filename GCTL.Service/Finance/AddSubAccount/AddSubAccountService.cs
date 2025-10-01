using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Finance.AddSubAccountVM;
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

namespace GCTL.Service.Finance.AddSubAccount
{
    public class AddSubAccountService : AppService<SubAccounts>, IAddSubAccountService
    {
        #region Repositories & Services
        public readonly IGenericRepository<SubAccounts> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public AddSubAccountService(IGenericRepository<SubAccounts> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreateAddSubAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.SubAccountName.ToLower() == model.SubAccountName.ToLower() && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.MainAccountID = (int)model.MainAccountID;
                    exixtingEntity.SubAccountCode = model.SubAccountCode;
                    exixtingEntity.SubAccountName = model.SubAccountName;
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
                    await _userInfoService.ActionLogAsync("Add Sub Account", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.SubAccountID, model);
                }
                else
                {
                    SubAccounts entity = new SubAccounts();
                    entity.MainAccountID = (int)model.MainAccountID;
                    entity.SubAccountCode = model.SubAccountCode;
                    entity.SubAccountName = model.SubAccountName;
                    entity.Description = model.Description;

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Add Sub Account", ActionName.DataAdd, null, entity, entity.SubAccountID, model);
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
        public async Task<bool> UpdateAsync(UpdateAddSubAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.GetByIdAsync(model.SubAccountID);
                if (entity == null)
                    return false;

                var beforeEntity = JsonConvert.DeserializeObject<UpdateAddSubAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.MainAccountID = (int)model.MainAccountID;
                entity.SubAccountCode = model.SubAccountCode;
                entity.SubAccountName = model.SubAccountName;
                entity.Description = model.Description;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateAddSubAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Add Sub Account", ActionName.DataUpdated, beforeEntity, afterEntity, entity.SubAccountID, model);

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
        public async Task<PaginationService<SubAccounts, GetAllAddSubAccountVM>.PaginationResult<GetAllAddSubAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "SubAccountID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive()
                    .Include(x => x.MainAccount)
                    .ThenInclude(x => x.Group)
                    .ThenInclude(x => x.Class)
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "SubAccountID" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccountID) : query.OrderBy(x => x.SubAccountID),
                        "GroupName" => sortOrder == "desc" ? query.OrderByDescending(x => x.MainAccount.Group.GroupName) : query.OrderBy(x => x.MainAccount.Group.GroupName),
                        "ClassName" => sortOrder == "desc" ? query.OrderByDescending(x => x.MainAccount.Group.Class.ClassName) : query.OrderBy(x => x.MainAccount.Group.Class.ClassName),
                        "MainAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.MainAccount.MainAccountName) : query.OrderBy(x => x.MainAccount.MainAccountName),
                        "SubAccountCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccountCode) : query.OrderBy(x => x.SubAccountCode),
                        "SubAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccountName) : query.OrderBy(x => x.SubAccountName),
                        "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                        _ => query.OrderBy(x => x.SubAccountID)
                    };
                }

                return await PaginationService<SubAccounts, GetAllAddSubAccountVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.SubAccountCode, $"%{term}%") || EF.Functions.Like(x.SubAccountName, $"%{term}%") || EF.Functions.Like(x.Description, $"%{term}%"),
                    x => new GetAllAddSubAccountVM
                    {
                        SubAccountID = x.SubAccountID,
                        MainAccountID = x.MainAccountID,
                        MainAccountName = x.MainAccount.MainAccountName ?? "-",
                        GroupID = x.MainAccount.GroupID,
                        GroupName = x.MainAccount.Group.GroupName ?? "-",
                        ClassID = x.MainAccount.Group.ClassID,
                        ClassName = x.MainAccount.Group.Class.ClassName ?? "-",
                        SubAccountCode = x.SubAccountCode ?? "-",
                        SubAccountName = x.SubAccountName ?? "-",
                        Description = x.Description ?? "-"
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Add Sub Accounts.", ex);
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<GetByIdAddSubAccountVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.AllActive()
                    .Include(x => x.MainAccount)
                    .ThenInclude(x => x.Group)
                    .ThenInclude(x => x.Class)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.SubAccountID == id);

                return new GetByIdAddSubAccountVM
                {
                    SubAccountID = data.SubAccountID,
                    MainAccountID = data.MainAccountID,
                    GroupID = data.MainAccount.GroupID,
                    ClassID = data.MainAccount.Group.ClassID,
                    SubAccountCode = data.SubAccountCode ?? "-",
                    SubAccountName = data.SubAccountName ?? "-",
                    Description = data.Description ?? "-"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the Add Sub Account.", ex);
            }
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<DeleteAddSubAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.SubAccountID));
                if (data == null || data.Count == 0)
                {
                    return new DeleteAddSubAccountVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DeleteAddSubAccountVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.SubAccountID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Add Sub Account", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DeleteAddSubAccountVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("An error occurred while deleting the Add Sub Account.", ex);
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            try
            {
                name = name.ToLower();
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SubAccountID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.SubAccountName.ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Add Sub Account name uniqueness.", ex);
            }
        }
        #endregion


        #region IsCodeUniqueAsync
        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            try
            {
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.SubAccountID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.SubAccountCode == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Add Sub Account code uniqueness.", ex);
            }
        }
        #endregion
    }
}
