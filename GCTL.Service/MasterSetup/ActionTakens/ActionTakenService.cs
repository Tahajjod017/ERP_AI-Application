using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.MasterSetup.ActionTakens;
using static Dapper.SqlMapper;
using GCTL.Service.Pagination;

namespace GCTL.Service.MasterSetup.ActionTakens
{
    public class ActionTakenService : AppService<ActionTaken>, IActionTakenService
    {
        #region Repositories
        private readonly IGenericRepository<ActionTaken> _genericRepository;

        public ActionTakenService(IGenericRepository<ActionTaken> genericRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(ActionTakenVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var existingEntity = await _genericRepository.FindAsync(b => b.ActionTakenName == model.ActionTakenName && b.DeletedAt != null);
                if (existingEntity.Any())
                {
                    var entityToRestore = existingEntity.FirstOrDefault();

                    entityToRestore.ActionTakenName = model.ActionTakenName;
                    entityToRestore.CreatedAt = DateTime.Now;
                    entityToRestore.CreatedBy = model.CreatedBy;
                    entityToRestore.Lip = GetLocalIP();
                    entityToRestore.Lmac = GetMacAddress();

                    entityToRestore.DeletedAt = null;
                    entityToRestore.UpdatedAt = DateTime.Now;

                    await _genericRepository.UpdateAsync(entityToRestore);
                }
                else
                {
                    ActionTaken entity = new ActionTaken();
                    entity.ActionTakenName = model.ActionTakenName;
                    entity.CreatedAt = DateTime.Now;
                    entity.CreatedBy = model.CreatedBy;
                    entity.Lip = GetLocalIP();
                    entity.Lmac = GetMacAddress();

                    await _genericRepository.AddAsync(entity);
                }

                await _genericRepository.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                throw new Exception(ex.Message, ex); 
                //return false;
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(ActionTakenVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var entity = await _genericRepository.GetByIdAsync(model.ActionTakenID);
                if (entity == null)
                {
                    return false;
                }

                entity.ActionTakenName = model.ActionTakenName;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = model.UpdatedBy;
                entity.Lip = GetLocalIP();
                entity.Lmac = GetMacAddress();

                await _genericRepository.UpdateAsync(entity);
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
        public async Task<ActionTakenVM> GetByIdAsync(int id)
        {
            try
            {
                var data = await _genericRepository.GetByIdAsync(id);
                if (data == null) return null;

                return new ActionTakenVM
                {
                    ActionTakenID = data.ActionTakenId,
                    ActionTakenName = data.ActionTakenName,
                };
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., to a file or logging service)
                throw; // Rethrow or return an error-specific response
            }
        }
        #endregion


        #region IP & Mac Address
        public string GetLocalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        public string GetMacAddress()
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces();
            var macAddress = string.Empty;
            foreach (var adapter in nics)
            {
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                {
                    macAddress = adapter.GetPhysicalAddress().ToString();
                    break;
                }
            }
            return macAddress;
        }
        #endregion


        #region IsNameUniqueAsync
        public async Task<bool> IsNameUniqueAsync(string name)
        {
            var existingName = await _genericRepository.FindAsync(b => b.ActionTakenName == name && b.DeletedAt == null);
            return !existingName.Any();
        }
        #endregion


        #region Soft Delete
        public async Task<ActionTakenVM> SoftDeleteAsync(List<int> ids)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                var data = await _genericRepository.FindAsync(x => ids.Contains(x.ActionTakenId));
                if (data == null || data.Count == 0)
                {
                    return new ActionTakenVM
                    {
                        Message = "No data found to delete."
                    };
                }

                foreach (var item in data)
                {
                    //item.IsDeleted = true; 
                    item.DeletedAt = DateTime.Now;
                }

                await _genericRepository.UpdateRangeAsync(data);

                await _genericRepository.CommitTransactionAsync();

                return new ActionTakenVM
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
        public async Task<PaginationService<ActionTaken, ActionTakenVM>.PaginationResult<ActionTakenVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "ActionTakenID", string sortOrder = "desc")
        {
            var query = _genericRepository.AllActive();

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "ActionTakenID" => sortOrder == "desc" ? query.OrderByDescending(x => x.ActionTakenId) : query.OrderBy(x => x.ActionTakenId),
                    "ActionTakenName" => sortOrder == "desc" ? query.OrderByDescending(x => x.ActionTakenName) : query.OrderBy(x => x.ActionTakenName),
                    _ => query.OrderBy(x => x.ActionTakenId)
                };
            }

            var result = await PaginationService<ActionTaken, ActionTakenVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.ActionTakenName, $"%{term}%"),
                x => new ActionTakenVM
                {
                    ActionTakenID = x.ActionTakenId,
                    ActionTakenName = x.ActionTakenName ?? "-",
                });

            return result;
        }
        #endregion
    }
}
