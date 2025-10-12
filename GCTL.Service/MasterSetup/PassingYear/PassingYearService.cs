using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.PassingYear;
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

namespace GCTL.Service.MasterSetup.PassingYear
{
    public class PassingYearService : AppService<PassingYears>, IPassingYearService
    {
        #region Services & Repositories
        private readonly IGenericRepository<PassingYears> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public PassingYearService(IGenericRepository<PassingYears> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(PassingYearVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.PassingYearName == model.PassingYearName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.PassingYearName = model.PassingYearName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<PassingYearVM>(JsonConvert.SerializeObject(entityToRestore, JsonSettings.IgnoreReferenceLoop));
                    await _userInfoService.ActionLogAsync("Passing Year", ActionName.DataAdd, null, entityToRestore, entityToRestore.PassingYearID, model);
                }
                else
                {
                    PassingYears entity = new PassingYears();
                    entity.PassingYearName = model.PassingYearName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Passing Year", ActionName.DataAdd, null, entity, entity.PassingYearID, model);
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
        public async Task<bool> UpdateAsync(PassingYearVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.PassingYearID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<PassingYearVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                entity.PassingYearName = model.PassingYearName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<PassingYearVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Passing Year", ActionName.DataUpdated, beforeEntity, afterEntity, entity.PassingYearID, model);
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
        public async Task<PassingYearVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new PassingYearVM
                {
                    PassingYearID = data.PassingYearID,
                    PassingYearName = data.PassingYearName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.PassingYearName != null);

            var nameList = existingNames.Select(b => b.PassingYearName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<PassingYearVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.PassingYearID));
                if (data == null || data.Count == 0)
                {
                    return new PassingYearVM
                    {
                        Message = "No data found to delete."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<List<PassingYearVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.PassingYearID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Passing Year", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new PassingYearVM
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
        public async Task<PaginationService<PassingYears, PassingYearVM>.PaginationResult<PassingYearVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PassingYearID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "PassingYearID" => sortOrder == "desc" ? query.OrderByDescending(x => x.PassingYearID) : query.OrderBy(x => x.PassingYearID),
                    "PassingYearName" => sortOrder == "desc" ? query.OrderByDescending(x => x.PassingYearName) : query.OrderBy(x => x.PassingYearName),
                    _ => query.OrderBy(x => x.PassingYearID)
                };
            }

            var result = await PaginationService<PassingYears, PassingYearVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.PassingYearName, $"%{term}%"),
                x => new PassingYearVM
                {
                    PassingYearID = x.PassingYearID,
                    PassingYearName = x.PassingYearName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
