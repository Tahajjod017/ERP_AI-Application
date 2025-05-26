using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.MasterSetup.EmployeeType;
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

namespace GCTL.Service.MasterSetup.EmployeeTypes
{
    public class EmployeeTypesService : AppService<EmployeeType>, IEmployeeTypesService
    {
        #region Repositories & Services
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<EmployeeType> _genericRepository;

        public EmployeeTypesService(IGenericRepository<EmployeeType> genericRepository, IUserInfoService userInfoService) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _userInfoService = userInfoService;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(EmployeeTypesVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.EmployeeTypeName == model.EmployeeTypeName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.EmployeeTypeName = model.EmployeeTypeName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Employee Type", ActionName.DataAdd, null, entityToRestore, entityToRestore.EmployeeTypeID, model);
                }
                else
                {
                    EmployeeType entity = new EmployeeType();
                    entity.EmployeeTypeName = model.EmployeeTypeName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Employee Type", ActionName.DataAdd, null, entity, entity.EmployeeTypeID, model);
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
        public async Task<bool> UpdateAsync(EmployeeTypesVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.EmployeeTypeID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<EmployeeTypesVM>(JsonConvert.SerializeObject(entity));

                entity.EmployeeTypeName = model.EmployeeTypeName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<EmployeeTypesVM>(JsonConvert.SerializeObject(entity));
                await _userInfoService.ActionLogAsync("Employee Type", ActionName.DataUpdated, beforeEntity, afterEntity, entity.EmployeeTypeID, model);

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
        public async Task<EmployeeTypesVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new EmployeeTypesVM
                {
                    EmployeeTypeID = data.EmployeeTypeID,
                    EmployeeTypeName = data.EmployeeTypeName,
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
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.EmployeeTypeName != null);

            var nameList = existingNames.Select(b => b.EmployeeTypeName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<EmployeeTypesVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.EmployeeTypeID));
                if (data == null || data.Count == 0)
                {
                    return new EmployeeTypesVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<EmployeeTypesVM>>(JsonConvert.SerializeObject(data));
                var targetIds = data.Select(x => (int?)x.EmployeeTypeID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Employee Type", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new EmployeeTypesVM
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
        public async Task<PaginationService<EmployeeType, EmployeeTypesVM>.PaginationResult<EmployeeTypesVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeTypeName", string sortOrder = "asc")
        {
            var query = _genericRepository.All();
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "EmployeeTypeID" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmployeeTypeID) : query.OrderBy(x => x.EmployeeTypeID),
                    "EmployeeTypeName" => sortOrder == "desc" ? query.OrderByDescending(x => x.EmployeeTypeName) : query.OrderBy(x => x.EmployeeTypeName),
                    _ => query.OrderBy(x => x.EmployeeTypeID)
                };
            }

            return await PaginationService<EmployeeType, EmployeeTypesVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.EmployeeTypeName, $"%{term}%"),
                x => new EmployeeTypesVM
                {
                    EmployeeTypeID = x.EmployeeTypeID,
                    EmployeeTypeName = x.EmployeeTypeName ?? "-",
                });
        }
        #endregion
    }
}
