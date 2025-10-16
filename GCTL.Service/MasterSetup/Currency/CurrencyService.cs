using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Currencies;
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

namespace GCTL.Service.MasterSetup.Currency
{
    public class CurrencyService : AppService<Currencies>, ICurrencyService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Currencies> _genericRepository;

        public CurrencyService(IGenericRepository<Currencies> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CurrencyVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.CurrencyName == model.CurrencyName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.CurrencyName = model.CurrencyName;
                    entityToRestore.CurrencyCode = model.CurrencyCode ?? string.Empty;
                    entityToRestore.Symbol = model.Symbol;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Currency", ActionName.DataAdd, null, entityToRestore, entityToRestore.CurrencyID, model);
                }
                else
                {
                    Currencies entity = new Currencies();
                    entity.CurrencyName = model.CurrencyName;
                    entity.CurrencyCode = model.CurrencyCode ?? string.Empty;
                    entity.Symbol = model.Symbol;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Currency", ActionName.DataAdd, null, entity, entity.CurrencyID, model);
                }
                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception)
            {
                await _genericRepository.RollbackTransactionAsync();
                //throw ex;
                return false;
            }
        }
        #endregion


        #region UpdateAsync
        public async Task<bool> UpdateAsync(CurrencyVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.CurrencyID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<CurrencyVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.CurrencyName = model.CurrencyName;
                entity.CurrencyCode = (model.CurrencyCode ?? string.Empty).TrimStart();
                entity.Symbol = model.Symbol;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<CurrencyVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Currency", ActionName.DataUpdated, beforeEntity, afterEntity, entity.CurrencyID, model);

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
        public async Task<CurrencyVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new CurrencyVM
                {
                    CurrencyID = data.CurrencyID,
                    CurrencyCode = data.CurrencyCode,
                    CurrencyName = data.CurrencyName,
                    Symbol = data.Symbol
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.CurrencyName != null);

            var nameList = existingNames.Select(b => b.CurrencyName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<CurrencyVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x =>requestVM.Ids.Contains(x.CurrencyID));
                if (data == null || data.Count == 0)
                {
                    return new CurrencyVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<CurrencyVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.CurrencyID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Currency", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new CurrencyVM
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
        public async Task<PaginationService<Currencies, CurrencyVM>.PaginationResult<CurrencyVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CurrencyID", string sortOrder = "desc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "CurrencyID" => sortOrder == "desc" ? query.OrderByDescending(x => x.CurrencyID) : query.OrderBy(x => x.CurrencyID),
                    "CurrencyCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.CurrencyCode) : query.OrderBy(x => x.CurrencyCode),
                    "CurrencyName" => sortOrder == "desc" ? query.OrderByDescending(x => x.CurrencyName) : query.OrderBy(x => x.CurrencyName),
                    "Symbol" => sortOrder == "desc" ? query.OrderByDescending(x => x.Symbol) : query.OrderBy(x => x.Symbol),
                    _ => query.OrderBy(x => x.CurrencyID)
                };
            }

            return await PaginationService<Currencies, CurrencyVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.CurrencyName, $"%{term}%") || EF.Functions.Like(x.CurrencyCode, $"%{term}%") || EF.Functions.Like(x.Symbol, $"%{term}%"),
                x => new CurrencyVM
                {
                    CurrencyID = x.CurrencyID,
                    CurrencyCode = x.CurrencyCode ?? "-",
                    CurrencyName = x.CurrencyName ?? "-",
                    Symbol = x.Symbol ?? "-",
                });
        }
        #endregion
    }
}
