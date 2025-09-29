using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Finance.SecondTabVM;
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

namespace GCTL.Service.Finance.SecondTab
{
    public class SecondTabService : AppService<Classes>, ISecondTabService
    {
        #region Repositories & Services
        public readonly IGenericRepository<Classes> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public SecondTabService(IGenericRepository<Classes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreateSecondTabVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.ClassName.ToLower() == model.ClassName.ToLower() && x.BaseAccountID == model.BaseAccountID && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.ClassCode = model.ClassCode;
                    exixtingEntity.ClassName = model.ClassName;
                    exixtingEntity.Description = model.Description;
                    exixtingEntity.BaseAccountID = model.BaseAccountID;

                    exixtingEntity.CreatedAt = DateTime.UtcNow;
                    exixtingEntity.CreatedBy = model.CreatedBy;
                    exixtingEntity.LIP = model.LIP;
                    exixtingEntity.LMAC = model.LMAC;

                    exixtingEntity.DeletedAt = null;
                    exixtingEntity.DeletedBy = null;
                    exixtingEntity.UpdatedAt = null;
                    exixtingEntity.UpdatedBy = null;

                    await _genericRepository.UpdateAsync(exixtingEntity);
                    await _userInfoService.ActionLogAsync("Class", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.BaseAccountID, model);
                }
                else
                {
                    Classes entity = new Classes();
                    entity.ClassCode = model.ClassCode;
                    entity.ClassName = model.ClassName;
                    entity.Description = model.Description;
                    entity.BaseAccountID = model.BaseAccountID;

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Class", ActionName.DataAdd, null, entity, entity.BaseAccountID, model);
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
        public async Task<bool> UpdateAsync(UpdateSecondTabVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.GetByIdAsync(model.BaseAccountID);
                if (entity == null)
                    return false;

                var beforeEntity = JsonConvert.DeserializeObject<UpdateSecondTabVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.ClassCode = model.ClassCode;
                entity.ClassName = model.ClassName;
                entity.Description = model.Description;
                entity.BaseAccountID = model.BaseAccountID;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateSecondTabVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Class", ActionName.DataUpdated, beforeEntity, afterEntity, entity.BaseAccountID, model);

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
        public async Task<PaginationService<Classes, GetAllSecondTabVM>.PaginationResult<GetAllSecondTabVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ClassID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive();

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "ClassID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ClassID) : query.OrderBy(x => x.ClassID),
                        "ClassCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.ClassCode) : query.OrderBy(x => x.ClassCode),
                        "ClassName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ClassName) : query.OrderBy(x => x.ClassName),
                        "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                        _ => query.OrderBy(x => x.ClassID)
                    };
                }

                return await PaginationService<Classes, GetAllSecondTabVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.ClassCode, $"%{term}%") || EF.Functions.Like(x.ClassName, $"%{term}%") || EF.Functions.Like(x.Description, $"%{term}%"),
                    x => new GetAllSecondTabVM
                    {
                        ClassID = x.ClassID,
                        BaseAccountID = x.BaseAccountID,
                        ClassCode = x.ClassCode ?? "-",
                        ClassName = x.ClassName ?? "-",
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
        public async Task<GetByIdSecondTabVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);

                return new GetByIdSecondTabVM
                {
                    ClassID = data.ClassID,
                    BaseAccountID = data.BaseAccountID,
                    ClassCode = data.ClassCode ?? "-",
                    ClassName = data.ClassName ?? "-",
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
        public async Task<DeleteSecondTabVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ClassID));
                if (data == null || data.Count == 0)
                {
                    return new DeleteSecondTabVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DeleteSecondTabVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
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

                return new DeleteSecondTabVM
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
        public async Task<bool> IsNameUniqueAsync(string name, int baseAccountId, int? excludeId = null)
        {
            try
            {
                name = name.ToLower();

                var query = _genericRepository.AllActive().Include(x => x.BaseAccount);

                var exists = await query.AnyAsync(x =>
                    x.ClassName != null && x.ClassName.ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Class name uniqueness.", ex);
            }
        }
        #endregion
    }
}
