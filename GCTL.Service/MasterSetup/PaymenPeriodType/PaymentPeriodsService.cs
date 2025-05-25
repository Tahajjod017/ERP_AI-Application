using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.PaymenPeriodTypes;
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

namespace GCTL.Service.MasterSetup.PaymenPeriodType
{
    public class PaymentPeriodsService : AppService<PaymentPeriodTypes>, IPaymentPeriodsService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<PaymentPeriodTypes> _genericRepository;

        public PaymentPeriodsService(IGenericRepository<PaymentPeriodTypes> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(PaymentPeriodsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.PaymentPeriodTypeName == model.PaymentPeriodTypeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.PaymentPeriodTypeName = model.PaymentPeriodTypeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Payment Period", ActionName.DataAdd, null, entityToRestore, entityToRestore.PaymentPeriodTypeID, model);
                }
                else
                {
                    PaymentPeriodTypes entity = new PaymentPeriodTypes();
                    entity.PaymentPeriodTypeName = model.PaymentPeriodTypeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Payment Period", ActionName.DataAdd, null, entity, entity.PaymentPeriodTypeID, model);
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
        public async Task<bool> UpdateAsync(PaymentPeriodsVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.PaymentPeriodTypeID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<PaymentPeriodsVM>(JsonConvert.SerializeObject(entity));

                entity.PaymentPeriodTypeName = model.PaymentPeriodTypeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<PaymentPeriodsVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Payment Period", ActionName.DataUpdated, beforeEntity, afterEntity, entity.PaymentPeriodTypeID, model);

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
        public async Task<PaymentPeriodsVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new PaymentPeriodsVM
                {
                    PaymentPeriodTypeID = data.PaymentPeriodTypeID,
                    PaymentPeriodTypeName = data.PaymentPeriodTypeName,
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
            var existingName = await _genericRepository.FindAsync(b => b.PaymentPeriodTypeName == name);
            return !existingName.Any();
        }
        #endregion


        #region Soft Delete
        public async Task<PaymentPeriodsVM> SoftDeleteAsync(BaseViewModel model, List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.PaymentPeriodTypeID));
                if (data == null || data.Count == 0)
                {
                    return new PaymentPeriodsVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<PaymentPeriodsVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.PaymentPeriodTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = model.DeletedBy;
                    item.LIP = model.LIP;
                    item.LMAC = model.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Payment Period", ActionName.DataDeleted, null, beforeEntity, targetIds, model);

                await _genericRepository.CommitTransactionAsync();

                return new PaymentPeriodsVM
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
        public async Task<PaginationService<PaymentPeriodTypes, PaymentPeriodsVM>.PaginationResult<PaymentPeriodsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "PaymentPeriodTypeID", string sortOrder = "desc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "PaymentPeriodTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.PaymentPeriodTypeID) : query.OrderBy(x => x.PaymentPeriodTypeID),
                    "PaymentPeriodTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.PaymentPeriodTypeName) : query.OrderBy(x => x.PaymentPeriodTypeName),
                    _ => query.OrderBy(x => x.PaymentPeriodTypeID)
                };
            }

            return await PaginationService<PaymentPeriodTypes, PaymentPeriodsVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.PaymentPeriodTypeName, $"%{term}%"),
                x => new PaymentPeriodsVM
                {
                    PaymentPeriodTypeID = x.PaymentPeriodTypeID,
                    PaymentPeriodTypeName = x.PaymentPeriodTypeName ?? "-",
                });
        }
        #endregion
    }
}
