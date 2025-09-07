
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using GCTL.Service.MasterSetup.ServiceType;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GCTL.Service.MasterSetup.ServiceType
{
    public class ServiceTypeService : AppService<Services>, IServiceTypeService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Services> _genericRepository;
        public ServiceTypeService(IGenericRepository<Services> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion

        #region add
        public async Task<bool> AddAsync(ServiceVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.ServiceName == model.ServiceName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.ServiceName = model.ServiceName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.DeletedBy = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Services", ActionName.DataAdd, null, entityToRestore, entityToRestore.ServiceID, model);
                }
                else
                {
                    Services entity = new Services();
                    entity.ServiceName = model.ServiceName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Services", ActionName.DataAdd, null, entity, entity.ServiceID, model);
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

        #region GetAllAsync
        public async Task<PaginationService<Services, ServiceVM>.PaginationResult<ServiceVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ServiceName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ServiceID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ServiceID) : query.OrderBy(x => x.ServiceID),
                    "ServiceName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ServiceName) : query.OrderBy(x => x.ServiceName),
                    _ => query.OrderBy(x => x.ServiceID)
                };
            }

            return await PaginationService<Services, ServiceVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ServiceName, $"%{term}%"),
                x => new ServiceVM
                {
                    ServiceID = x.ServiceID,
                    ServiceName = x.ServiceName ?? "-",
                });
        }
        #endregion

        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.ServiceName != null);

            var nameList = existingNames.Select(b => b.ServiceName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion;


        #region Get
        public async Task<ServiceVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ServiceVM
                {
                    ServiceID = data.ServiceID,
                    ServiceName = data.ServiceName,
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., to a file or logging service)
                throw; // Rethrow or return an error-specific response
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(ServiceVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.ServiceID);
                if (entity == null)
                {
                    return false;
                }

                //var beforeEntity = JsonConvert.DeserializeObject<ServiceVM>(JsonConvert.SerializeObject(entity));

                entity.ServiceName = model.ServiceName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                //var afterEntity = JsonConvert.DeserializeObject<ServiceVM>(JsonConvert.SerializeObject(entity));
                //await _userInfoService.ActionLogAsync("Service", ActionName.DataUpdated, beforeEntity, afterEntity, entity.ServiceID, model);

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

        #region Soft Delete
        public async Task<ServiceVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.ServiceID));
                if (data == null || data.Count == 0)
                {
                    return new ServiceVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                //var beforeEntity = JsonConvert.DeserializeObject<List<ServiceVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.ServiceID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                //await _userInfoService.ActionLogDeleteAsync("Service", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new ServiceVM
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
    }
}
