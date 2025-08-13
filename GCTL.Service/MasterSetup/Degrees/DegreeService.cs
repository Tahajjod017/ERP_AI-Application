using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Degree;
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
using GCTL.Core.Helpers.Jsonserialize;

namespace GCTL.Service.MasterSetup.Degrees
{
    public class DegreeService : AppService<Degree>, IDegreeService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Degree> _degreeRepository;
        public DegreeService(IGenericRepository<Degree> degreeRepository, IUserInfoService userInfoService) : base(degreeRepository)
        {
            _degreeRepository = degreeRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(DegreeVM model)
        {
            await _degreeRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _degreeRepository.FindAsync(b => b.DegreeName == model.DegreeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.DegreeName = model.DegreeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _degreeRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Degree", ActionName.DataAdd, null, entityToRestore, entityToRestore.DegreeID, model);
                }
                else
                {
                    Degree entity = new Degree();
                    entity.DegreeName = model.DegreeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _degreeRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Degree", ActionName.DataAdd, null, entity, entity.DegreeID, model);
                }

                await _degreeRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _degreeRepository.RollbackTransactionAsync();
                //throw ex;
                return false;
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(DegreeVM model)
        {
            await _degreeRepository.BeginTransactionAsync();

            try
            {
                //var jsonSettings = new JsonSerializerSettings
                //{
                //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                //};
                var entity = await _degreeRepository.GetByIdAsync(model.DegreeID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<DegreeVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                entity.DegreeName = model.DegreeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _degreeRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<DegreeVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Degree", ActionName.DataUpdated, beforeEntity, afterEntity, entity.DegreeID, model);

                await _degreeRepository.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _degreeRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region Get
        public async Task<DegreeVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _degreeRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new DegreeVM
                {
                    DegreeID = data.DegreeID,
                    DegreeName = data.DegreeName,
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
            var existingNames = await _degreeRepository.FindAsync(b => b.DeletedAt == null && b.DegreeName != null);

            var nameList = existingNames.Select(b => b.DegreeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<DegreeVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _degreeRepository.BeginTransactionAsync();
            try
            {
                var data = await _degreeRepository.FindAsync(x => requestVM.Ids.Contains(x.DegreeID));
                if (data == null || data.Count == 0)
                {
                    return new DegreeVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DegreeVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.DegreeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _degreeRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Degree", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _degreeRepository.CommitTransactionAsync();

                return new DegreeVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _degreeRepository.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<Degree, DegreeVM>.PaginationResult<DegreeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DegreeID", string sortOrder = "desc")
        {
            var query = _degreeRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "DegreeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.DegreeID) : query.OrderBy(x => x.DegreeID),
                    "DegreeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.DegreeName) : query.OrderBy(x => x.DegreeName),
                    _ => query.OrderBy(x => x.DegreeID)
                };
            }

            return await PaginationService<Degree, DegreeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.DegreeName, $"%{term}%"),
                x => new DegreeVM
                {
                    DegreeID = x.DegreeID,
                    DegreeName = x.DegreeName ?? "-",
                });
        }
        #endregion
    }
}
