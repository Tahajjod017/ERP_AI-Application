using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Grade;
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

namespace GCTL.Service.MasterSetup.Grades
{
    public class GradeService : AppService<Grade>, IGradeService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Grade> _genericRepository;

        public GradeService(IGenericRepository<Grade> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region Add
        public async Task<bool> AddAsync(GradeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.GradeName == model.GradeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.GradeName = model.GradeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Grade", ActionName.DataAdd, null, entityToRestore, entityToRestore.GradeID, model);
                }
                else
                {
                    Grade entity = new Grade();
                    entity.GradeName = model.GradeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Grade", ActionName.DataAdd, null, entity, entity.GradeID, model);
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
        public async Task<bool> UpdateAsync(GradeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.GradeID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<GradeVM>(JsonConvert.SerializeObject(entity));

                entity.GradeName = model.GradeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<GradeVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Grade", ActionName.DataUpdated, beforeEntity, afterEntity, entity.GradeID, model);

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
        public async Task<GradeVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new GradeVM
                {
                    GradeID = data.GradeID,
                    GradeName = data.GradeName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.GradeName != null);

            var nameList = existingNames.Select(b => b.GradeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<GradeVM> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.GradeID));
                if (data == null || data.Count == 0)
                {
                    return new GradeVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<GradeVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.GradeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = model.DeletedBy;
                    item.LIP = model.LIP;
                    item.LMAC = model.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Grade", ActionName.DataDeleted, null, beforeEntity, targetIds, model);

                await _genericRepository.CommitTransactionAsync();

                return new GradeVM
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
        public async Task<PaginationService<Grade, GradeVM>.PaginationResult<GradeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "GradeName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "GradeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.GradeID) : query.OrderBy(x => x.GradeID),
                    "GradeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.GradeName) : query.OrderBy(x => x.GradeName),
                    _ => query.OrderBy(x => x.GradeID)
                };
            }

            return await PaginationService<Grade, GradeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.GradeName, $"%{term}%"),
                x => new GradeVM
                {
                    GradeID = x.GradeID,
                    GradeName = x.GradeName ?? "-",
                });
        }
        #endregion
    }
}
