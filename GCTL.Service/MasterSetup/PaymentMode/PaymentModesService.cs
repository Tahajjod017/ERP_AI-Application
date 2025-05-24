using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.PaymentModes;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.Pagination;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.MasterSetup.PaymentMode
{
    public class PaymentModesService : AppService<PaymentModes>, IPaymentModeService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<PaymentModes> _genericRepository;

        public PaymentModesService(IGenericRepository<PaymentModes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(PaymentModeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.PaymentModeName == model.PaymentModeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.PaymentModeName = model.PaymentModeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Payment Mode", ActionName.DataAdd, null, entityToRestore, entityToRestore.PaymentModeID, model);
                }
                else
                {
                    PaymentModes entity = new PaymentModes();
                    entity.PaymentModeName = model.PaymentModeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Payment Mode", ActionName.DataAdd, null, entity, entity.PaymentModeID, model);
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
        public async Task<bool> UpdateAsync(PaymentModeVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.PaymentModeID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<PaymentModeVM>(JsonConvert.SerializeObject(entity));

                entity.PaymentModeName = model.PaymentModeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<PaymentModeVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Payment Mode", ActionName.DataUpdated, beforeEntity, afterEntity, entity.PaymentModeID, model);

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
        public async Task<PaymentModeVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new PaymentModeVM
                {
                    PaymentModeID = data.PaymentModeID,
                    PaymentModeName = data.PaymentModeName,
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
            var existingName = await _genericRepository.FindAsync(b => b.PaymentModeName == name);
            return !existingName.Any();
        }
        #endregion


        #region Soft Delete
        public async Task<PaymentModeVM> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.PaymentModeID));
                if (data == null || data.Count == 0)
                {
                    return new PaymentModeVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<PaymentModeVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.PaymentModeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = model.DeletedBy;
                    item.LIP = model.LIP;
                    item.LMAC = model.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Payment Mode", ActionName.DataDeleted, null, beforeEntity, targetIds, model);

                await _genericRepository.CommitTransactionAsync();

                return new PaymentModeVM
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
        public async Task<PaginationService<PaymentModes, PaymentModeVM>.PaginationResult<PaymentModeVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PaymentModeName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "PaymentModeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.PaymentModeID) : query.OrderBy(x => x.PaymentModeID),
                    "PaymentModeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.PaymentModeName) : query.OrderBy(x => x.PaymentModeName),
                    _ => query.OrderBy(x => x.PaymentModeID)
                };
            }

            return await PaginationService<PaymentModes, PaymentModeVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.PaymentModeName, $"%{term}%"),
                x => new PaymentModeVM
                {
                    PaymentModeID = x.PaymentModeID,
                    PaymentModeName = x.PaymentModeName ?? "-",
                });
        }
        #endregion
    }
}
