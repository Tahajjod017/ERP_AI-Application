using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Finance.AddMainAccountVM;
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

namespace GCTL.Service.Finance.AddMainAccount
{
    public class AddMainAccountService : AppService<MainAccounts>, IAddMainAccountService
    {
        #region Repositories & Services
        public readonly IGenericRepository<MainAccounts> _genericRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<MenuTab> _menuTabRepository;

        public AddMainAccountService(IGenericRepository<MainAccounts> genericRepository, IUserInfoService userInfoService, IGenericRepository<MenuTab> menuTabRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
            _menuTabRepository = menuTabRepository;
        }
        #endregion
              

        #region AddAsync
        public async Task<bool> AddAsync(CreateAddMainAccountVM model)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var exixtingEntity = await _genericRepository.FirstOrDefaultAsync(x => x.MainAccountName.ToLower() == model.MainAccountName.ToLower() && x.ClassID == model.ClassID && x.DeletedAt != null);
                if (exixtingEntity != null)
                {
                    exixtingEntity.ClassID = (int)model.ClassID;
                    exixtingEntity.MainAccountCode = model.MainAccountCode;
                    exixtingEntity.MainAccountName = model.MainAccountName;
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
                    await _userInfoService.ActionLogAsync("Add Main Account", ActionName.DataAdd, null, exixtingEntity, exixtingEntity.MainAccountID, model);
                }
                else
                {
                    MainAccounts entity = new MainAccounts();
                    entity.ClassID = (int)model.ClassID;
                    entity.MainAccountCode = model.MainAccountCode;
                    entity.MainAccountName = model.MainAccountName;
                    entity.Description = model.Description;

                    entity.CreatedAt = DateTime.UtcNow;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Add Main Account", ActionName.DataAdd, null, entity, entity.MainAccountID, model);
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
        public async Task<CommonReturnViewModel> UpdateAsync(UpdateAddMainAccountVM model)
        {
            var result = new CommonReturnViewModel();
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var entity = await _genericRepository.GetByIdAsync(model.MainAccountID);
                if (entity == null)
                {
                    result.Success = false;
                    result.Message = "Main account not found.";
                    return result;
                }

                if (model.ClassID != entity.ClassID)
                {
                    result.Success = false;
                    result.Message = "You cannot change the class!";
                    return result;
                }

                var beforeEntity = JsonConvert.DeserializeObject<UpdateAddMainAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.MainAccountCode = model.MainAccountCode;
                entity.MainAccountName = model.MainAccountName;
                entity.Description = model.Description;

                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<UpdateAddMainAccountVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Add Main Account", ActionName.DataUpdated, beforeEntity, afterEntity, entity.MainAccountID, model);

                await _genericRepository.CommitTransactionAsync();

                result.Success = true;
                result.Message = "Updated Successfully.";
                return result;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                result.Success = false;
                result.Message = "An error occurred while updating the main account.";
                result.Errors.Add(ex.Message);
                return result;
            }
        }
        #endregion


        #region GetAllAsync
        public async Task<PaginationService<MainAccounts, GetAllAddMainAccountVM>.PaginationResult<GetAllAddMainAccountVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "MainAccountID", string sortOrder = "desc", int? classId = null)
        {
            try
            {
                var query = _genericRepository.AllActive().Include(x => x.Class).ThenInclude(x => x.BaseAccount).Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if(classId != null)
                {
                    query = query.Where(x => x.ClassID == classId);
                }

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "MainAccountID" => sortOrder == "desc" ? query.OrderByDescending(x => x.MainAccountID) : query.OrderBy(x => x.MainAccountID),
                        "BaseAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Class.BaseAccount.BaseAccountName) : query.OrderBy(x => x.Class.BaseAccount.BaseAccountName),
                        "ClassName" => sortOrder == "desc" ? query.OrderByDescending(x => x.Class.ClassName) : query.OrderBy(x => x.Class.ClassName),
                        "MainAccountCode" => sortOrder == "desc" ? query.OrderByDescending(x => x.MainAccountCode) : query.OrderBy(x => x.MainAccountCode),
                        "MainAccountName" => sortOrder == "desc" ? query.OrderByDescending(x => x.MainAccountName) : query.OrderBy(x => x.MainAccountName),
                        "Description" => sortOrder == "desc" ? query.OrderByDescending(x => x.Description) : query.OrderBy(x => x.Description),
                        _ => query.OrderBy(x => x.MainAccountID)
                    };
                }

                return await PaginationService<MainAccounts, GetAllAddMainAccountVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.MainAccountCode, $"%{term}%") 
                    || EF.Functions.Like(x.MainAccountName, $"%{term}%") 
                    || EF.Functions.Like(x.Class.ClassName, $"%{term}%") 
                    || EF.Functions.Like(x.Class.BaseAccount.BaseAccountName, $"%{term}%") 
                    || EF.Functions.Like(x.Description, $"%{term}%"),
                    x => new GetAllAddMainAccountVM
                    {
                        MainAccountID = x.MainAccountID,
                        BaseAccountID = x.Class.BaseAccountID,
                        BaseAccountName = x.Class.BaseAccount.BaseAccountName ?? "-",
                        ClassID = x.ClassID,
                        ClassName = x.Class.ClassName ?? "-",
                        MainAccountCode = x.MainAccountCode ?? "-",
                        MainAccountName = x.MainAccountName ?? "-",
                        Description = x.Description ?? "-"
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Add Main Accounts.", ex);
            }
        }
        #endregion


        #region GetByIdAsync
        public async Task<GetByIdAddMainAccountVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.AllActive()
                    .Include(x => x.Class)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.MainAccountID == id);

                return new GetByIdAddMainAccountVM
                {
                    MainAccountID = data.MainAccountID,
                    BaseAccountID = data.Class.BaseAccountID,
                    ClassID = data.ClassID,
                    MainAccountCode = data.MainAccountCode ?? "-",
                    MainAccountName = data.MainAccountName ?? "-",
                    Description = data.Description ?? "-"
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the Add Main Account.", ex);
            }
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<DeleteAddMainAccountVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.MainAccountID));
                if (data == null || data.Count == 0)
                {
                    return new DeleteAddMainAccountVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DeleteAddMainAccountVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.MainAccountID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.UtcNow;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Add Main Account", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DeleteAddMainAccountVM
                {
                    Message = $"{data.Count} data(s) deleted successfully."
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception("An error occurred while deleting the Add Main Account.", ex);
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
                    query = query.Where(x => x.MainAccountID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.MainAccountName.ToLower() == name);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Add Main Account name uniqueness.", ex);
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
                    query = query.Where(x => x.MainAccountID != excludeId.Value);
                }

                var exists = await query.AnyAsync(x => x.MainAccountCode == code);

                return !exists;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while checking the Add Main Account code uniqueness.", ex);
            }
        }
        #endregion


        #region GetBodyTabsAsync
        public async Task<List<MenuTab>> GetBodyTabsAsync()
        {
            try
            {
                var allowedControllers = new[] { "AddMainAccount", "AddSubAccount", "TransactionAccount" };

                var menuTabs = await _menuTabRepository.AllActive()
                    .Where(mt => allowedControllers.Contains(mt.ControllerName) && !mt.IsActive)
                    //.OrderBy(mt => mt.TabOrder)
                    .ToListAsync();
                return menuTabs;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving menu tabs.", ex);
            }
        }
        #endregion
    }
}
