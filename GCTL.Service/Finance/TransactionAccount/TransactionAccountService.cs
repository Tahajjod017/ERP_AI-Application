using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.Finance.TransactionAccountVM;
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

namespace GCTL.Service.Finance.TransactionAccount
{
    public class TransactionAccountService : AppService<TransactionAccounts>, ITransactionAccountService
    {
        #region Repositories & Services
        public readonly IGenericRepository<TransactionAccounts> _genericRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<SubAccounts> _subAccounts;

        public TransactionAccountService(IGenericRepository<TransactionAccounts> genericRepository, IUserInfoService userInfoService, IGenericRepository<SubAccounts> subAccounts) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
            _subAccounts = subAccounts;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreateTransactionAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.TrxAccName.ToLower() == model.TrxAccName.ToLower() && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.SubAccountID = (int)model.SubAccountID;
                    exixtingEntity.TrxAccCode = model.TrxAccCode;
                    exixtingEntity.TrxAccName = model.TrxAccName;
                    exixtingEntity.IsActive = model.IsActive;
                    exixtingEntity.Description = model.Description;

                    exixtingEntity.CreatedAt = DateTime.UtcNow;
                    exixtingEntity.CreatedBy = model.CreatedBy;
                    exixtingEntity.LIP = model.LIP;
                    exixtingEntity.LMAC = model.LMAC;

                    exixtingEntity.DeletedAt = null;
                    exixtingEntity.DeletedBy = null;
                    exixtingEntity.UpdatedAt = null;
                    exixtingEntity.UpdatedBy = null;

                    await _genericRepository.UpdateAsync(exixtingEntity);
                    await _userInfoService.ActionLogAsync("Transaction Account", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.TrxAccID, model);
                }
                else
                {
                    TransactionAccounts entity = new TransactionAccounts();
                    entity.SubAccountID = (int)model.SubAccountID;
                    entity.TrxAccCode = model.TrxAccCode;
                    entity.TrxAccName = model.TrxAccName;
                    entity.IsActive = model.IsActive;
                    entity.Description = model.Description;

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Transaction Account", ActionName.DataAdd, null, entity, entity.TrxAccID, model);
                }

                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
                throw;
            }
        }
        #endregion


        #region UpdateAsync
        public async Task<bool> UpdateAsync(UpdateTransactionAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.GetByIdAsync(model.TrxAccID);
                if (entity == null)
                    return false;

                var beforeEntity = JsonConvert.DeserializeObject<UpdateTransactionAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.SubAccountID = (int)model.SubAccountID;
                entity.TrxAccCode = model.TrxAccCode;
                entity.TrxAccName = model.TrxAccName;
                entity.IsActive = model.IsActive;
                entity.Description = model.Description;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateTransactionAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Transaction Account", ActionName.DataUpdated, beforeEntity, afterEntity, entity.TrxAccID, model);

                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
                throw;
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<TransactionAccounts, GetAllTransactionAccountVM>.PaginationResult<GetAllTransactionAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "TrxAccID", string sortOrder = "desc")
        {
            try
            {
                var query = _genericRepository.AllActive()
                    .Include(x => x.SubAccount)
                    .ThenInclude(x => x.MainAccount)
                    .ThenInclude(x => x.Group)
                    .ThenInclude(x => x.Class)
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "TrxAccID" => sortOrder == "desc" ? query.OrderByDescending(x => x.TrxAccID) : query.OrderBy(x => x.TrxAccID),
                        "ClassName" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccount.MainAccount.Group.Class.ClassName) : query.OrderBy(x => x.SubAccount.MainAccount.Group.Class.ClassName),
                        "GroupName" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccount.MainAccount.Group.GroupName) : query.OrderBy(x => x.SubAccount.MainAccount.Group.GroupName),
                        "SubAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccount.MainAccount.MainAccountName) : query.OrderBy(x => x.SubAccount.MainAccount.MainAccountName),
                        "MainAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.SubAccount.SubAccountName) : query.OrderBy(x => x.SubAccount.SubAccountName),
                        "TrxAccCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.TrxAccCode) : query.OrderBy(x => x.TrxAccCode),
                        "TrxAccName" => sortOrder == "desc" ? query.OrderByDescending(x => x.TrxAccName) : query.OrderBy(x => x.TrxAccName),
                        "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                        _ => query.OrderBy(x => x.TrxAccID)
                    };
                }

                return await PaginationService<TransactionAccounts, GetAllTransactionAccountVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.TrxAccCode, $"%{term}%") || EF.Functions.Like(x.TrxAccName, $"%{term}%") || EF.Functions.Like(x.Description, $"%{term}%"),
                    x => new GetAllTransactionAccountVM
                    {
                        TrxAccID = x.TrxAccID,
                        ClassID = x.SubAccount.MainAccount.Group.ClassID,
                        ClassName = x.SubAccount.MainAccount.Group.Class.ClassName ?? "-",
                        GroupID = x.SubAccount.MainAccount.GroupID,
                        GroupName = x.SubAccount.MainAccount.Group.GroupName ?? "-",
                        MainAccountID = x.SubAccount.MainAccountID,
                        MainAccountName = x.SubAccount.MainAccount.MainAccountName ?? "-",
                        SubAccountID = x.SubAccountID,
                        SubAccountName = x.SubAccount.SubAccountName ?? "-",
                        TrxAccCode = x.TrxAccCode ?? "-",
                        TrxAccName = x.TrxAccName ?? "-",
                        IsActive = x.IsActive,
                        Description = x.Description ?? "-"
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Transaction Accounts.", ex);
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<GetByIdTransactionAccountVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.AllActive()
                    .Include(x => x.SubAccount)
                    .ThenInclude(x => x.MainAccount)
                    .ThenInclude(x => x.Group)
                    .ThenInclude(x => x.Class)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.TrxAccID == id);

                return new GetByIdTransactionAccountVM
                {
                    TrxAccID = data.TrxAccID,
                    SubAccountID = data.SubAccountID,
                    MainAccountID = data.SubAccount.MainAccountID,
                    GroupID = data.SubAccount.MainAccount.GroupID,
                    ClassID = data.SubAccount.MainAccount.Group.ClassID,
                    TrxAccCode = data.TrxAccCode ?? "-",
                    TrxAccName = data.TrxAccName ?? "-",
                    IsActive = data.IsActive,
                    Description = data.Description ?? "-"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the Transaction Account.", ex);
            }
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<DeleteTransactionAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.TrxAccID));
                if (data == null || data.Count == 0)
                {
                    return new DeleteTransactionAccountVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DeleteTransactionAccountVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.TrxAccID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Transaction Account", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DeleteTransactionAccountVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("An error occurred while deleting the Transaction Account.", ex);
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
        {
            try
            {
                name = name.ToLower();
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TrxAccID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.TrxAccName.ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Transaction Account name uniqueness.", ex);
            }
        }
        #endregion


        #region IsCodeUniqueAsync
        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            try
            {
                var query = _genericRepository.AllActive();

                if (excludeId.HasValue)
                {
                    query = query.Where(x => x.TrxAccID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.TrxAccCode == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Transaction Account code uniqueness.", ex);
            }
        }
        #endregion


        #region GenerateNextCodeAsync
        public async Task<string> GenerateNextCodeAsync(int subAccId)
        {
            var subAcc = await _subAccounts.All().Where(x => x.SubAccountID == subAccId).FirstOrDefaultAsync();

            if (subAcc == null)
            {
                throw new Exception("Sub Account not found.");
            }

            var prefix = subAcc.SubAccountCode;

            var result = await _genericRepository.All()
                .Where(x => x.SubAccountID == subAccId)
                .AsNoTracking()
                .OrderByDescending(x => x.TrxAccCode)
                .FirstOrDefaultAsync();

            // If no SubAccountCode exists, start with "0001"
            if (result == null)
            {
                return prefix + "0001";  // "01010001"
            }

            // Step 3: Extract the numeric part from the last SubAccountCode
            var lastCode = result.TrxAccCode;  // E.g., "01010003"
            var lastCodeNumericPart = lastCode.Substring(8);  // E.g., "0003"

            // Step 4: Increment the numeric part
            int lastCodeNumber = int.Parse(lastCodeNumericPart);  // E.g., 3 from "0003"
            var newCodeNumber = lastCodeNumber + 1;  // E.g., 4

            // Ensure the new numeric part is 4 digits long with leading zeros
            var newCodeNumberPadded = newCodeNumber.ToString("D4");  // E.g., "0004"

            // Step 5: Return the new SubAccountCode by combining the prefix and new numeric part
            return prefix + newCodeNumberPadded;
        }
        #endregion
    }
}
