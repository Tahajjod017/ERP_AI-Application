using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.YearlyEndBonusType;
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

namespace GCTL.Service.MasterSetup.YearlyEndBonusType
{
    public class YearlyEndBonusTypeService : AppService<YearlyEndBonusTypes>, IYearlyEndBonusTypeService
    {
        private readonly IGenericRepository<YearlyEndBonusTypes> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public YearlyEndBonusTypeService(IGenericRepository<YearlyEndBonusTypes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }

        #region AddAsync
        public async Task<bool> AddAsync(YearlyEndBonusTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.YearlyEndBonusTypeName == model.YearlyEndBonusTypeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.YearlyEndBonusTypeName = model.YearlyEndBonusTypeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<YearlyEndBonusTypeVM>(JsonConvert.SerializeObject(entityToRestore));
                    await _userInfoService.ActionLogAsync("Yearly End Bonus Type", ActionName.DataAdd, null, entityToRestore, entityToRestore.YearlyEndBonusTypeID, model);
                }
                else
                {
                    YearlyEndBonusTypes entity = new YearlyEndBonusTypes();
                    entity.YearlyEndBonusTypeName = model.YearlyEndBonusTypeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Yearly End Bonus Type", ActionName.DataAdd, null, entity, entity.YearlyEndBonusTypeID, model);
                }

                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex);
                //return false;
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(YearlyEndBonusTypeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.YearlyEndBonusTypeID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<YearlyEndBonusTypeVM>(JsonConvert.SerializeObject(entity));
                entity.YearlyEndBonusTypeName = model.YearlyEndBonusTypeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<YearlyEndBonusTypeVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Yearly End Bonus Type", ActionName.DataUpdated, beforeEntity, afterEntity, entity.YearlyEndBonusTypeID, model);
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<YearlyEndBonusTypeVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new YearlyEndBonusTypeVM
                {
                    YearlyEndBonusTypeID = data.YearlyEndBonusTypeID,
                    YearlyEndBonusTypeName = data.YearlyEndBonusTypeName,
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., to a file or logging service)
                throw; // Rethrow or return an error-specific response
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.YearlyEndBonusTypeName != null);

            var nameList = existingNames.Select(b => b.YearlyEndBonusTypeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<YearlyEndBonusTypeVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.YearlyEndBonusTypeID));
                if (data == null || data.Count == 0)
                {
                    return new YearlyEndBonusTypeVM
                    {
                        Message = "No data found to delete."
                    };
                }

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _genericRepository.CommitTransactionAsync();

                return new YearlyEndBonusTypeVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<YearlyEndBonusTypes, YearlyEndBonusTypeVM>.PaginationResult<YearlyEndBonusTypeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "YearlyEndBonusTypeID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "YearlyEndBonusTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.YearlyEndBonusTypeID) : query.OrderBy(x => x.YearlyEndBonusTypeID),
                    "YearlyEndBonusTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.YearlyEndBonusTypeName) : query.OrderBy(x => x.YearlyEndBonusTypeName),
                    _ => query.OrderBy(x => x.YearlyEndBonusTypeID)
                };
            }

            var result = await PaginationService<YearlyEndBonusTypes, YearlyEndBonusTypeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.YearlyEndBonusTypeName, $"%{term}%"),
                x => new YearlyEndBonusTypeVM
                {
                    YearlyEndBonusTypeID = x.YearlyEndBonusTypeID,
                    YearlyEndBonusTypeName = x.YearlyEndBonusTypeName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
