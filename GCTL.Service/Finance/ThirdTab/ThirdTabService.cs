using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Finance.ThirdTabVM;
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

namespace GCTL.Service.Finance.ThirdTab
{
    public class ThirdTabService : AppService<Groups>, IThirdTabService
    {
        #region Repositories & Services
        public readonly IGenericRepository<Groups> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public ThirdTabService(IGenericRepository<Groups> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreateThirdTabVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.GroupName.ToLower() == model.GroupName.ToLower() && x.ClassID == model.ClassID && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.GroupCode = model.GroupCode;
                    exixtingEntity.GroupName = model.GroupName;
                    exixtingEntity.Description = model.Description;
                    exixtingEntity.ClassID = model.ClassID;

                    exixtingEntity.CreatedAt = DateTime.UtcNow;
                    exixtingEntity.CreatedBy = model.CreatedBy;
                    exixtingEntity.LIP = model.LIP;
                    exixtingEntity.LMAC = model.LMAC;

                    exixtingEntity.DeletedAt = null;
                    exixtingEntity.DeletedBy = null;
                    exixtingEntity.UpdatedAt = null;
                    exixtingEntity.UpdatedBy = null;

                    await _genericRepository.UpdateAsync(exixtingEntity);
                    await _userInfoService.ActionLogAsync("Group", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.ClassID, model);
                }
                else
                {
                    Groups entity = new Groups();
                    entity.GroupCode = model.GroupCode;
                    entity.GroupName = model.GroupName;
                    entity.Description = model.Description;
                    entity.ClassID = model.ClassID;

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Group", ActionName.DataAdd, null, entity, entity.ClassID, model);
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
        public async Task<bool> UpdateAsync(UpdateThirdTabVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.GetByIdAsync(model.ClassID);
                if (entity == null)
                    return false;

                var beforeEntity = JsonConvert.DeserializeObject<UpdateThirdTabVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.GroupCode = model.GroupCode;
                entity.GroupName = model.GroupName;
                entity.Description = model.Description;
                entity.ClassID = model.ClassID;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateThirdTabVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Group", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ClassID, model);

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
        public async Task<PaginationService<Groups, GetAllThirdTabVM>.PaginationResult<GetAllThirdTabVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GroupID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive().Include(x => x.Class).ThenInclude(x => x.BaseAccount).Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "GroupID" => sortOrder == "desc" ? query.OrderByDescending(x => x.GroupID) : query.OrderBy(x => x.GroupID),
                        "ClassName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Class.ClassName) : query.OrderBy(x => x.Class.ClassName),
                        "BaseAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Class.BaseAccount.BaseAccountName) : query.OrderBy(x => x.Class.BaseAccount.BaseAccountName),
                        "GroupCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.GroupCode) : query.OrderBy(x => x.GroupCode),
                        "GroupName" => sortOrder == "desc" ? query.OrderByDescending(x => x.GroupName) : query.OrderBy(x => x.GroupName),
                        "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                        _ => query.OrderBy(x => x.ClassID)
                    };
                }

                return await PaginationService<Groups, GetAllThirdTabVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.GroupCode, $"%{term}%") || EF.Functions.Like(x.GroupName, $"%{term}%") || EF.Functions.Like(x.Description, $"%{term}%"),
                    x => new GetAllThirdTabVM
                    {
                        GroupID = x.GroupID,
                        ClassID = x.ClassID,
                        ClassName = x.Class.ClassName ?? "-",
                        BaseAccountID = x.Class.BaseAccount.BaseAccountID,
                        BaseAccountName = x.Class.BaseAccount.BaseAccountName ?? "-",
                        GroupCode = x.GroupCode ?? "-",
                        GroupName = x.GroupName ?? "-",
                        Description = x.Description ?? "-"

                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Classs.", ex);
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<GetByIdThirdTabVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);

                return new GetByIdThirdTabVM
                {
                    GroupID = data.GroupID,
                    ClassID = data.ClassID,
                    GroupCode = data.GroupCode ?? "-",
                    GroupName = data.GroupName ?? "-",
                    Description = data.Description ?? "-"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the Class.", ex);
            }
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<DeleteThirdTabVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ClassID));
                if (data == null || data.Count == 0)
                {
                    return new DeleteThirdTabVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DeleteThirdTabVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.ClassID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Class", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DeleteThirdTabVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("An error occurred while deleting the Class.", ex);
            }
        }
        #endregion


        #region Others
        public async Task<bool> IsNameUniqueAsync(string name, int id, int? excludeId = null)
        {
            try
            {
                name = name.ToLower();

                var query = _genericRepository.AllActive().Include(x => x.Class);

                var exists = await query.AnyAsync(x =>
                    x.GroupName != null && x.GroupName.ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Class name uniqueness.", ex);
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
                    query = query.Where(x => x.GroupID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.GroupCode == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Base Account code uniqueness.", ex);
            }
        }
        #endregion
    }
}
