using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Country;
using GCTL.Core.ViewModels;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.MasterSetup.Countries
{
    public class CountryService : AppService<Country>, ICountryService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Country> _genericRepository;

        public CountryService(IGenericRepository<Country> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CountryVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.CountryName == model.CountryName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.CountryCode = model.CountryCode;
                    entityToRestore.CountryName = model.CountryName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Country", ActionName.DataAdd, null, entityToRestore, entityToRestore.CountryID, model);
                }
                else
                {
                    Country entity = new Country();
                    entity.CountryCode = model.CountryCode;
                    entity.CountryName = model.CountryName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Country", ActionName.DataAdd, null, entity, entity.CountryID, model);
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
        public async Task<bool> UpdateAsync(CountryVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.CountryID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<CountryVM>(JsonConvert.SerializeObject(entity));

                entity.CountryCode = model.CountryCode;
                entity.CountryName = model.CountryName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<CountryVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Country", ActionName.DataUpdated, beforeEntity, afterEntity, entity.CountryID, model);

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
        public async Task<CountryVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new CountryVM
                {
                    CountryID = data.CountryID,
                    CountryCode = data.CountryCode,
                    CountryName = data.CountryName,
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
            var existingName = await _genericRepository.FindAsync(b => b.CountryName == name && b.DeletedAt == null);
            return !existingName.Any();
        }
        #endregion


        #region Soft Delete
        public async Task<CountryVM> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.CountryID));
                if (data == null || data.Count == 0)
                {
                    return new CountryVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<CountryVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.CountryID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = model.DeletedBy;
                    item.LIP = model.LIP;
                    item.LMAC = model.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Country", ActionName.DataDeleted, null, beforeEntity, targetIds, model);

                await _genericRepository.CommitTransactionAsync();

                return new CountryVM
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
        public async Task<PaginationService<Country, CountryVM>.PaginationResult<CountryVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CountryID", string sortOrder = "desc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "CountryID" => sortOrder == "desc" ? query.OrderByDescending(x => x.CountryID) : query.OrderBy(x => x.CountryID),
                    "CountryCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.CountryCode) : query.OrderBy(x => x.CountryCode),
                    "CountryName" => sortOrder == "desc" ? query.OrderByDescending(x => x.CountryName) : query.OrderBy(x => x.CountryName),
                    _ => query.OrderBy(x => x.CountryID)
                };
            }

            return await PaginationService<Country, CountryVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.CountryName, $"%{term}%") || EF.Functions.Like(x.CountryCode, $"%{term}%"),
                x => new CountryVM
                {
                    CountryID = x.CountryID,
                    CountryCode = x.CountryCode ?? "-",
                    CountryName = x.CountryName ?? "-"
                });
        }
        #endregion
    }
}
