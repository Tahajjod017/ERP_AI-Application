using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Grade;
using GCTL.Core.ViewModels.MasterSetup.ServiceYear;
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

namespace GCTL.Service.MasterSetup.ServiceYear
{
    public class ServiceYearService : AppService<ServiceYears>, IServiceYearService
    {
        #region Services & Repositories
        private readonly IGenericRepository<ServiceYears> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public ServiceYearService(IGenericRepository<ServiceYears> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(ServiceYearVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.ServiceYearName == model.ServiceYearName && b.DeletedAt != null);
                if (existingEntity.Any())
                {

                    var entityToRestore = existingEntity.FirstOrDefault();
                    if (entityToRestore == null) return false;

                    entityToRestore.ServiceYearName = model.ServiceYearName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;
                    entityToRestore.UpdatedBy = model.UpdatedBy ?? null;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    var afterEntity = JsonConvert.DeserializeObject<ServiceYearVM>(JsonConvert.SerializeObject(entityToRestore));
                    await _userInfoService.ActionLogAsync("Service Year", ActionName.DataAdd, null, entityToRestore, entityToRestore.ServiceYearID, model);
                }
                else
                {
                    ServiceYears entity = new ServiceYears();
                    entity.ServiceYearName = model.ServiceYearName;
                    entity.CreatedAt = DateTime.Now;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;
                    entity.CreatedBy = model.CreatedBy ?? null;
                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Service Year", ActionName.DataAdd, null, entity, entity.ServiceYearID, model);
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
        public async Task<bool> UpdateAsync(ServiceYearVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.ServiceYearID);
                if (entity == null)
                {
                    return false;
                }
                var beforeEntity = JsonConvert.DeserializeObject<ServiceYearVM>(JsonConvert.SerializeObject(entity));
                entity.ServiceYearName = model.ServiceYearName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;
                entity.UpdatedBy = model.UpdatedBy ?? null;
                await _genericRepository.UpdateAsync(entity);
                var afterEntity = JsonConvert.DeserializeObject<ServiceYearVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Service Year", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ServiceYearID, model);
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
        public async Task<ServiceYearVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ServiceYearVM
                {
                    ServiceYearID = data.ServiceYearID,
                    ServiceYearName = data.ServiceYearName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ServiceYearName != null);

            var nameList = existingNames.Select(b => b.ServiceYearName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<ServiceYearVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ServiceYearID));
                if (data == null || data.Count == 0)
                {
                    return new ServiceYearVM
                    {
                        Message = "No data found to delete."
                    };
                }
                var beforeEntity = JsonConvert.DeserializeObject<List<ServiceYearVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.ServiceYearID).ToList();
                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _genericRepository.UpdateRangeAsync(data);
                await _userInfoService.ActionLogDeleteAsync("Service Year", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);
                await _genericRepository.CommitTransactionAsync();

                return new ServiceYearVM
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
        public async Task<PaginationService<ServiceYears, ServiceYearVM>.PaginationResult<ServiceYearVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ServiceYearID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ServiceYearID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ServiceYearID) : query.OrderBy(x => x.ServiceYearID),
                    "ServiceYearName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ServiceYearName) : query.OrderBy(x => x.ServiceYearName),
                    _ => query.OrderBy(x => x.ServiceYearID)
                };
            }

            var result = await PaginationService<ServiceYears, ServiceYearVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ServiceYearName, $"%{term}%"),
                x => new ServiceYearVM
                {
                    ServiceYearID = x.ServiceYearID,
                    ServiceYearName = x.ServiceYearName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
