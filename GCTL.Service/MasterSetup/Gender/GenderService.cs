using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GCTL.Data.Models;

namespace GCTL.Service.MasterSetup.Gender
{
    public class GenderService : AppService<Genders>, IGenderService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Genders> _genericRepository;

        public GenderService(IGenericRepository<Genders> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(GenderVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.GenderName == model.GenderName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.GenderName = model.GenderName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Gender", ActionName.DataAdd, null, entityToRestore, entityToRestore.GenderID, model);
                }
                else
                {
                    Genders entity = new Genders();
                    entity.GenderName = model.GenderName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Gender", ActionName.DataAdd, null, entity, entity.GenderID, model);
                }

                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                //throw ex;
                return false;
            }
        }
        #endregion



        #region Update
        public async Task<bool> UpdateAsync(GenderVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.GenderID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<GenderVM>(JsonConvert.SerializeObject(entity));

                entity.GenderName = model.GenderName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<GenderVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Gender", ActionName.DataUpdated, beforeEntity, afterEntity, entity.GenderID, model);

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


        #region Get
        public async Task<GenderVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new GenderVM
                {
                    GenderID = data.GenderID,
                    GenderName = data.GenderName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.GenderName != null);

            var nameList = existingNames.Select(b => b.GenderName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<GenderVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.GenderID));
                if (data == null || data.Count == 0)
                {
                    return new GenderVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<GenderVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.GenderID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Gender", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new GenderVM
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
        public async Task<PaginationService<Genders, GenderVM>.PaginationResult<GenderVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GenderName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "GenderID" => sortOrder == "desc" ? query.OrderByDescending(x => x.GenderID) : query.OrderBy(x => x.GenderID),
                    "GenderName" => sortOrder == "desc" ? query.OrderByDescending(x => x.GenderName) : query.OrderBy(x => x.GenderName),
                    _ => query.OrderBy(x => x.GenderID)
                };
            }

            return await PaginationService<Genders, GenderVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.GenderName, $"%{term}%"),
                x => new GenderVM
                {
                    GenderID = x.GenderID,
                    GenderName = x.GenderName ?? "-",
                });
        }
        #endregion
    }
}
