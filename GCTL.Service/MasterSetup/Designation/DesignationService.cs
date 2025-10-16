using GCTL.Core.Helpers;
using GCTL.Core.Helpers.Jsonserialize;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.MasterSetup.Designations;
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

namespace GCTL.Service.MasterSetup.Designation
{
    public class DesignationSettingService : AppService<Designations>, IDesignationService
    {
        #region Repositories
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<Designations> _genericRepository;
        private readonly IGenericRepository<Departments> _departmentRepository;

        public DesignationSettingService(IUserInfoService userInfoService, IGenericRepository<Designations> genericRepository, IGenericRepository<Departments> departmentRepository):base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepository = genericRepository;
            _departmentRepository = departmentRepository;
        }


        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(DesignationVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.DesignationName == model.DesignationName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.DesignationName = model.DesignationName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.LIP = model.LIP;
                    entityToRestore.LMAC = model.LMAC;

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                    await _userInfoService.ActionLogAsync("Designation", ActionName.DataAdd, null, entityToRestore, entityToRestore.DesignationID, model);
                }
                else
                {
                    Designations entity = new Designations();
                    entity.DesignationName = model.DesignationName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.LIP = model.LIP;
                    entity.LMAC = model.LMAC;

                    await _genericRepository.AddAsync(entity);
                    await _userInfoService.ActionLogAsync("Designation", ActionName.DataAdd, null, entity, entity.DesignationID, model);
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
        public async Task<bool> UpdateAsync(DesignationVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.DesignationID);
                if (entity == null)
                {
                    return false;
                }

                var beforeEntity = JsonConvert.DeserializeObject<DesignationVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));

                entity.DesignationName = model.DesignationName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.LIP = model.LIP;
                entity.LMAC = model.LMAC;

                await _genericRepository.UpdateAsync(entity);

                var afterEntity = JsonConvert.DeserializeObject<DesignationVM>(JsonConvert.SerializeObject(entity, JsonSettings.IgnoreReferenceLoop));
                await _userInfoService.ActionLogAsync("Designation", ActionName.DataUpdated, beforeEntity, afterEntity, entity.DesignationID, model);

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
        public async Task<DesignationVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new DesignationVM
                {
                    DesignationID = data.DesignationID,
                    DesignationName = data.DesignationName,
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingNames = await _genericRepository.FindAsync(b => b.DeletedAt == null && b.DesignationName != null);

            var nameList = existingNames.Select(b => b.DesignationName);

            return !DuplicateChecker.IsDuplicate(name, nameList);
        }
        #endregion


        #region Soft Delete
        public async Task<DesignationVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => requestVM.Ids.Contains(x.DesignationID));
                if (data == null || data.Count == 0)
                {
                    return new DesignationVM
                    {
                        Message = "No data found to soft delete."
                    };
                }

                var beforeEntity = JsonConvert.DeserializeObject<List<DesignationVM>>(JsonConvert.SerializeObject(data, JsonSettings.IgnoreReferenceLoop));
                var targetIds = data.Select(x => (int?)x.DesignationID).ToList();

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.DeletedBy = requestVM.DeletedBy;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _userInfoService.ActionLogDeleteAsync("Designation", ActionName.DataDeleted, null, beforeEntity, targetIds, requestVM);

                await _genericRepository.CommitTransactionAsync();

                return new DesignationVM
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
        public async Task<PaginationService<Designations, DesignationVM>.PaginationResult<DesignationVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "DesignationID", string sortOrder = "desc")
        {
            var query = _genericRepository.All().AsNoTracking().Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "DesignationID" => sortOrder == "desc" ? query.OrderByDescending(x => x.DesignationID) : query.OrderBy(x => x.DesignationID),
                    "DesignationName" => sortOrder == "desc" ? query.OrderByDescending(x => x.DesignationName) : query.OrderBy(x => x.DesignationName),
                    _ => query.OrderBy(x => x.DesignationID)
                };
            }

            return await PaginationService<Designations, DesignationVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.DesignationName, $"%{term}%"),
                x => new DesignationVM
                {
                    DesignationID = x.DesignationID,
                    DesignationName = x.DesignationName ?? "-",
                });
        }
        #endregion


        #region GetDepartments
        public IEnumerable<CommonSelectVM> GetDepartments()
        {
            var result = _departmentRepository.All().Select(x => new CommonSelectVM
            {
                Id = x.DepartmentID,
                Name = x.DepartmentName
            }).ToList();

            return result;
        }
        #endregion
    }
}
