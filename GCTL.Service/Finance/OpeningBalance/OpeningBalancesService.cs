using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.OpeningBalancesVM;
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
using static Dapper.SqlMapper;

namespace GCTL.Service.Finance.OpeningBalance
{
    public class OpeningBalancesService : AppService<OpeningBalances>, IOpeningBalancesService
    {
        #region Repositories and Services
        private readonly IGenericRepository<OpeningBalances> _genericRepository;
        private readonly IUserInfoService _userInfoService;

        public OpeningBalancesService(IGenericRepository<OpeningBalances> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<CommonReturnViewModel> AddAsync(CreateOpeningBalancesVM model)
        {
            var result = new CommonReturnViewModel();
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var existingEntity = await _genericRepository.FirstOrDefaultAsync(b => b.TrxAccID == model.TrxAccID && b.TrxType == model.TrxType && b.DeletedAt != null);
                if (existingEntity != null)
                {
                    existingEntity.TrxAccID = model.TrxAccID;
                    existingEntity.TrxType = model.TrxType;
                    existingEntity.Amount = model.Amount;
                    existingEntity.Description = model.Description;

                    existingEntity.CreatedAt = DateTime.UtcNow;
                    existingEntity.CreatedBy = model.CreatedBy;
                    existingEntity.LIP = model.LIP;
                    existingEntity.LMAC = model.LMAC;

                    existingEntity.DeletedAt = null;
                    existingEntity.DeletedBy = null;

                    await _genericRepository.UpdateAsync(existingEntity);
                    await _userInfoService.ActionLogAsync("Opening Balance", ActionName.DataAdd, null, existingEntity, existingEntity.OpeningBalanceID, model);
                }
                else
                {
                    OpeningBalances entity = new OpeningBalances();
                    entity.TrxAccID = model.TrxAccID;
                    entity.OpeningBalanceCode = model.OpeningBalanceCode;
                    entity.Amount = model.Amount;
                    entity.Description = model.Description;
                    entity.TrxType = model.TrxType;

                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Opening Balance", ActionName.DataAdd, null, entity, entity.OpeningBalanceID, model);
                }

                await _genericRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Saved Successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                //throw ex;
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"{ex.Message}"
                };
            }
        }
        #endregion


        #region Update
        public async Task<CommonReturnViewModel> UpdateAsync(UpdateOpeningBalancesVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.OpeningBalanceID);
                if (entity == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = $"Data not Found!"
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<UpdateOpeningBalancesVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.TrxAccID = model.TrxAccID;
                entity.Amount = model.Amount;
                entity.TrxType = model.TrxType;
                entity.Description = model.Description;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateOpeningBalancesVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Opening Balance", ActionName.DataUpdated, beforeEntity, afterEntity, entity.OpeningBalanceID, model);

                await _genericRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"Updated Successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"{ex.Message}"
                };
            }
        }
        #endregion


        #region GetById
        public async Task<GetByIdOpeningBalancesVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new GetByIdOpeningBalancesVM
                {
                    OpeningBalanceID = data.OpeningBalanceID,
                    TrxAccID = data.TrxAccID,
                    OpeningBalanceCode = data.OpeningBalanceCode,
                    Amount = data.Amount,
                    TrxType = data.TrxType,
                    Description = data.Description,
                };
            }
            catch (Exception ex)
            {
                throw; 
            }
        }
        #endregion


        #region Soft Delete
        public async Task<CommonReturnViewModel> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            var result = new CommonReturnViewModel();
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.OpeningBalanceID));
                
                if (data == null)
                {
                    result.Success = false;
                    result.Message = "Posting Rule not found.";
                    return result;
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<CommonReturnViewModel>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.OpeningBalanceID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Opening Balance", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = $"{ex.Message}"
                };
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<OpeningBalances, GetAllOpeningBalancesVM>.PaginationResult<GetAllOpeningBalancesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "OpeningBalanceID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive()
                .Include(x => x.TrxAcc)
                .Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "OpeningBalanceID" => sortOrder == "desc" ? query.OrderByDescending(x => x.OpeningBalanceID) : query.OrderBy(x => x.OpeningBalanceID),
                    "OpeningBalanceCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.OpeningBalanceCode) : query.OrderBy(x => x.OpeningBalanceCode),
                    "Amount" => sortOrder == "desc" ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
                    "TrxType" => sortOrder == "desc" ? query.OrderByDescending(x => x.TrxType) : query.OrderBy(x => x.TrxType),
                    "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                    "TrxAccName" => sortOrder == "desc" ? query.OrderByDescending(x => x.TrxAcc.TrxAccName) : query.OrderBy(x => x.TrxAcc.TrxAccName),
                    _ => query.OrderBy(x => x.OpeningBalanceID)
                };
            }

            if (pageSize == 0)
            {
                pageSize = await query.CountAsync();
                pageNumber = 1;
            }

            return await PaginationService<OpeningBalances, GetAllOpeningBalancesVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.OpeningBalanceCode, $"%{term}%")
                || EF.Functions.Like(x.TrxAcc.TrxAccName, $"%{term}%")
                || EF.Functions.Like(x.Amount, $"%{term}%")
                || EF.Functions.Like(x.TrxType, $"%{term}%")
                || EF.Functions.Like(x.Description, $"%{term}%"),
                x => new GetAllOpeningBalancesVM
                {
                    OpeningBalanceID = x.OpeningBalanceID,
                    TrxAccName = x.TrxAcc?.TrxAccName ?? "-",
                    OpeningBalanceCode = x.OpeningBalanceCode ?? "-",
                    Amount = x.Amount ?? 0,
                    TrxType = x.TrxType ?? "-",
                    Description = x.Description ?? "-"
                });
        }
        #endregion


        #region IsCodeUniqueAsync
        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            try
            {
                code = code.Trim();
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.OpeningBalanceID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.OpeningBalanceCode.Trim() == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the opening balance code uniqueness.", ex);
            }
        }
        #endregion


        #region GenerateThreeDigitCodeAsync
        public async Task<string> GenerateThreeDigitCodeAsync()
        {
            try
            {
                var lastCode = await _genericRepository.AllActive().OrderByDescending(x => x.OpeningBalanceCode).Select(x => x.OpeningBalanceCode).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(lastCode))
                {
                    return "001";
                }
                var lastNumber = int.Parse(lastCode.Substring(1));

                var nextCode = lastNumber + 1;

                if (nextCode > 999)
                    throw new InvalidOperationException("Maximum 3-digit code limit reached.");

                return nextCode.ToString("D3");
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while generating the three-digit code.", ex);
            }
        }
        #endregion
    }
}
