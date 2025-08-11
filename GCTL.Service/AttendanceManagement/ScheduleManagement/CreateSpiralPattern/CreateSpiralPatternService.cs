using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.Shift;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.CreateSpiralPattern
{
    public class CreateSpiralPatternService : AppService<SpiralWeeklyPattern>, ICreateSpiralPatternService
    {
        #region Repositories
        private readonly IGenericRepository<SpiralWeeklyPattern> _genericRepository;
        private readonly IGenericRepository<SpiralWeeklyPatternDetails> _spiralWeeklyPatternDetailsRepository;
        public readonly IGenericRepository<SpiralBioWeeklyPattern> _spiralBioWeeklyPattern;
        public readonly IGenericRepository<SpiralBioWeeklyPatternDetails> _spiralBioWeeklyPatternDetails;
        public readonly IGenericRepository<SpiralMonthlyPattern> _spiralMonthlyPattern;
        public readonly IGenericRepository<SpiralMonthlyPatternDetails> _spiralMonthlyPatternDetails;

        public CreateSpiralPatternService(IGenericRepository<SpiralWeeklyPattern> genericRepository, IGenericRepository<SpiralWeeklyPatternDetails> spiralWeeklyPatternDetailsRepository, IGenericRepository<SpiralBioWeeklyPattern> spiralBioWeeklyPattern, IGenericRepository<SpiralBioWeeklyPatternDetails> spiralBioWeeklyPatternDetails, IGenericRepository<SpiralMonthlyPattern> spiralMonthlyPattern, IGenericRepository<SpiralMonthlyPatternDetails> spiralMonthlyPatternDetails) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _spiralWeeklyPatternDetailsRepository = spiralWeeklyPatternDetailsRepository;
            _spiralBioWeeklyPattern = spiralBioWeeklyPattern;
            _spiralBioWeeklyPatternDetails = spiralBioWeeklyPatternDetails;
            _spiralMonthlyPattern = spiralMonthlyPattern;
            _spiralMonthlyPatternDetails = spiralMonthlyPatternDetails;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(CreateSpiralPatternVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.SpiralPatternTypeID == 1)
                {
                    SpiralWeeklyPattern spiralWeeklyPattern = new SpiralWeeklyPattern();
                    spiralWeeklyPattern.SpiralWeeklyPatternName = model.SpiralPatternName;
                    spiralWeeklyPattern.OrganizationID = model.OrganizationID;
                    spiralWeeklyPattern.SpiralPatternTypeID = model.SpiralPatternTypeID;
                    spiralWeeklyPattern.CreatedAt = DateTime.Now;
                    spiralWeeklyPattern.CreatedBy = model.CreatedBy;
                    spiralWeeklyPattern.LIP = model.LIP;
                    spiralWeeklyPattern.LMAC = model.LMAC;
                    await _genericRepository.AddAsync(spiralWeeklyPattern);

                    foreach (var detail in model.SpiralWeeklyPatternDetailsVMs)
                    {
                        SpiralWeeklyPatternDetails spiralWeeklyPatternDetails = new SpiralWeeklyPatternDetails();
                        spiralWeeklyPatternDetails.SpiralWeeklyPatternID = spiralWeeklyPattern.SpiralWeeklyPatternID;
                        spiralWeeklyPatternDetails.DayOfWeek = detail.DayOfWeek;
                        spiralWeeklyPatternDetails.ShiftID = detail.ShiftID;
                        spiralWeeklyPatternDetails.CreatedAt = DateTime.Now;
                        spiralWeeklyPatternDetails.CreatedBy = model.CreatedBy;
                        spiralWeeklyPatternDetails.LIP = model.LIP;
                        spiralWeeklyPatternDetails.LMAC = model.LMAC;
                        await _spiralWeeklyPatternDetailsRepository.AddAsync(spiralWeeklyPatternDetails);
                    }
                }
                else if(model.SpiralPatternTypeID == 2)
                {
                    SpiralBioWeeklyPattern spiralBioWeeklyPattern = new SpiralBioWeeklyPattern();
                    spiralBioWeeklyPattern.SpiralBioWeeklyPatternName = model.SpiralPatternName;
                    spiralBioWeeklyPattern.OrganizationID = model.OrganizationID;
                    spiralBioWeeklyPattern.SpiralPatternTypeID = model.SpiralPatternTypeID;
                    spiralBioWeeklyPattern.CreatedAt = DateTime.Now;
                    spiralBioWeeklyPattern.CreatedBy = model.CreatedBy;
                    spiralBioWeeklyPattern.LIP = model.LIP;
                    spiralBioWeeklyPattern.LMAC = model.LMAC;
                    await _spiralBioWeeklyPattern.AddAsync(spiralBioWeeklyPattern);

                    foreach (var detail in model.SpiralBioWeeklyPatternDetailsVMs)
                    {
                        SpiralBioWeeklyPatternDetails spiralBioWeeklyPatternDetails = new SpiralBioWeeklyPatternDetails();
                        spiralBioWeeklyPatternDetails.SpiralBioWeeklyPatternID = spiralBioWeeklyPattern.SpiralBioWeeklyPatternID;
                        spiralBioWeeklyPatternDetails.DayOfMonth = detail.DayOfMonth;
                        spiralBioWeeklyPatternDetails.ShiftID = detail.ShiftID;
                        spiralBioWeeklyPatternDetails.CreatedAt = DateTime.Now;
                        spiralBioWeeklyPatternDetails.CreatedBy = model.CreatedBy;
                        spiralBioWeeklyPatternDetails.LIP = model.LIP;
                        spiralBioWeeklyPatternDetails.LMAC = model.LMAC;
                        await _spiralBioWeeklyPatternDetails.AddAsync(spiralBioWeeklyPatternDetails);
                    }
                }
                else if(model.SpiralPatternTypeID == 3)
                {
                    SpiralMonthlyPattern spiralMonthlyPattern = new SpiralMonthlyPattern();
                    spiralMonthlyPattern.SpiralMonthlyPatternName = model.SpiralPatternName;
                    spiralMonthlyPattern.OrganizationID = model.OrganizationID;
                    spiralMonthlyPattern.SpiralPatternTypeID = model.SpiralPatternTypeID;
                    spiralMonthlyPattern.CreatedAt = DateTime.Now;
                    spiralMonthlyPattern.CreatedBy = model.CreatedBy;
                    spiralMonthlyPattern.LIP = model.LIP;
                    spiralMonthlyPattern.LMAC = model.LMAC;
                    await _spiralMonthlyPattern.AddAsync(spiralMonthlyPattern);

                    foreach (var detail in model.SpiralMonthlyPatternDetailsVMs)
                    {
                        SpiralMonthlyPatternDetails spiralMonthlyPatternDetails = new SpiralMonthlyPatternDetails();
                        spiralMonthlyPatternDetails.SpiralMonthlyPatternID = spiralMonthlyPattern.SpiralMonthlyPatternID;
                        spiralMonthlyPatternDetails.DayOfMonth = detail.DayOfMonth;
                        spiralMonthlyPatternDetails.ShiftID = detail.ShiftID;
                        spiralMonthlyPatternDetails.CreatedAt = DateTime.Now;
                        spiralMonthlyPatternDetails.CreatedBy = model.CreatedBy;
                        spiralMonthlyPatternDetails.LIP = model.LIP;
                        spiralMonthlyPatternDetails.LMAC = model.LMAC;
                        await _spiralMonthlyPatternDetails.AddAsync(spiralMonthlyPatternDetails);
                    }
                }
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region AddShift
        public async Task<bool> AddShift(AddSpiralWeeklyPatternVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.AddSpiralPatternTypeID == 1)
                {
                    var spiralWeeklyPattern = await _spiralWeeklyPatternDetailsRepository.GetByIdAsync(model.AddSpiralPatternDetailID);
                    if (spiralWeeklyPattern != null)
                    {
                        spiralWeeklyPattern.DayOfWeek = (byte)model.AddDayOfWeek;
                        spiralWeeklyPattern.ShiftID = model.AddShiftID;
                        spiralWeeklyPattern.UpdatedAt = DateTime.Now;
                        spiralWeeklyPattern.UpdatedBy = model.UpdatedBy;
                        spiralWeeklyPattern.LIP = model.LIP;
                        spiralWeeklyPattern.LMAC = model.LMAC;
                        await _spiralWeeklyPatternDetailsRepository.UpdateAsync(spiralWeeklyPattern);
                    }
                }
                else
                {
                    await _genericRepository.RollbackTransactionAsync();
                    return false;
                }
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region AddFortMonthlyShift
        public async Task<bool> AddFortMonthlyShift(AddSpiralFortMonthlyPatternVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.AddSpiralPatternTypeIDFortMonthly == 2)
                {
                    var spiralBioWeeklyPattern = await _spiralBioWeeklyPatternDetails.GetByIdAsync(model.AddSpiralPatternDetailIDFortMonthly);
                    if (spiralBioWeeklyPattern != null)
                    {
                        spiralBioWeeklyPattern.DayOfMonth = model.AddDayOfMonthFortMonthly;
                        spiralBioWeeklyPattern.ShiftID = model.AddShiftIDFortMonthly;
                        spiralBioWeeklyPattern.UpdatedAt = DateTime.Now;
                        spiralBioWeeklyPattern.UpdatedBy = model.UpdatedBy;
                        spiralBioWeeklyPattern.LIP = model.LIP;
                        spiralBioWeeklyPattern.LMAC = model.LMAC;
                        await _spiralBioWeeklyPatternDetails.UpdateAsync(spiralBioWeeklyPattern);
                    }
                }
                else if (model.AddSpiralPatternTypeIDFortMonthly == 3)
                {
                    var spiralMonthlyPattern = await _spiralMonthlyPatternDetails.GetByIdAsync(model.AddSpiralPatternDetailIDFortMonthly);
                    if (spiralMonthlyPattern != null)
                    {
                        spiralMonthlyPattern.DayOfMonth = model.AddDayOfMonthFortMonthly;
                        spiralMonthlyPattern.ShiftID = model.AddShiftIDFortMonthly;
                        spiralMonthlyPattern.UpdatedAt = DateTime.Now;
                        spiralMonthlyPattern.UpdatedBy = model.UpdatedBy;
                        spiralMonthlyPattern.LIP = model.LIP;
                        spiralMonthlyPattern.LMAC = model.LMAC;
                        await _spiralMonthlyPatternDetails.UpdateAsync(spiralMonthlyPattern);
                    }
                }
                else
                {
                    await _genericRepository.RollbackTransactionAsync();
                    return false;
                }
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region Update
        public async Task<bool> UpdateAsync(UpdateSpiralPatternVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.UpdateSpiralPatternTypeID == 1)
                {
                    var spiralWeeklyPattern = await _spiralWeeklyPatternDetailsRepository.GetByIdAsync(model.UpdateSpiralPatternDetailID);
                    if (spiralWeeklyPattern != null)
                    {
                        //spiralWeeklyPattern.SpiralWeeklyPatternID = model.UpdateSpiralPatternID;
                        spiralWeeklyPattern.DayOfWeek = model.UpdateDayOfWeek;
                        spiralWeeklyPattern.ShiftID = model.UpdateShiftID;
                        spiralWeeklyPattern.UpdatedAt = DateTime.Now;
                        spiralWeeklyPattern.UpdatedBy = model.UpdatedBy;
                        spiralWeeklyPattern.LIP = model.LIP;
                        spiralWeeklyPattern.LMAC = model.LMAC;
                        await _spiralWeeklyPatternDetailsRepository.UpdateAsync(spiralWeeklyPattern);
                    }
                }
                else if (model.UpdateSpiralPatternTypeID == 2)
                {
                    var spiralBioWeeklyPattern = await _spiralBioWeeklyPatternDetails.GetByIdAsync(model.UpdateSpiralPatternDetailID);
                    if (spiralBioWeeklyPattern != null)
                    {
                        //spiralBioWeeklyPattern.SpiralBioWeeklyPatternID = model.UpdateSpiralPatternID;
                        spiralBioWeeklyPattern.DayOfMonth = model.UpdateDayOfMonth;
                        spiralBioWeeklyPattern.ShiftID = model.UpdateShiftID;
                        spiralBioWeeklyPattern.UpdatedAt = DateTime.Now;
                        spiralBioWeeklyPattern.UpdatedBy = model.UpdatedBy;
                        spiralBioWeeklyPattern.LIP = model.LIP;
                        spiralBioWeeklyPattern.LMAC = model.LMAC;
                        await _spiralBioWeeklyPatternDetails.UpdateAsync(spiralBioWeeklyPattern);
                    }
                }
                else if (model.UpdateSpiralPatternTypeID == 3)
                {
                    var spiralMonthlyPattern = await _spiralMonthlyPatternDetails.GetByIdAsync(model.UpdateSpiralPatternDetailID);
                    if (spiralMonthlyPattern != null)
                    {
                        //spiralMonthlyPattern.SpiralMonthlyPatternID = model.UpdateSpiralPatternID;
                        spiralMonthlyPattern.DayOfMonth = model.UpdateDayOfMonth;
                        spiralMonthlyPattern.ShiftID = model.UpdateShiftID;
                        spiralMonthlyPattern.UpdatedAt = DateTime.Now;
                        spiralMonthlyPattern.UpdatedBy = model.UpdatedBy;
                        spiralMonthlyPattern.LIP = model.LIP;
                        spiralMonthlyPattern.LMAC = model.LMAC;
                        await _spiralMonthlyPatternDetails.UpdateAsync(spiralMonthlyPattern);
                    }
                }
                await _genericRepository.CommitTransactionAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return false;
            }
        }
        #endregion


        #region SoftDeleteAsync
        public async Task<bool> SoftDeleteFortnightly(DeleteRequestVM requestVM)
        {
            await _spiralBioWeeklyPatternDetails.BeginTransactionAsync();
            try
            {
                var data = await _spiralBioWeeklyPatternDetails.FindAsync(x => requestVM.Ids.Contains(x.SpiralBioWeeklyPatternDetailID));
                if (data == null || data.Count == 0)
                {
                    return false;
                }

                foreach (var item in data)
                {
                    item.DeletedAt = DateTime.Now;
                    item.LIP = requestVM.LIP;
                    item.LMAC = requestVM.LMAC;
                    item.DeletedBy = requestVM.DeletedBy ?? null;
                }

                await _spiralBioWeeklyPatternDetails.UpdateRangeAsync(data);

                await _spiralBioWeeklyPatternDetails.CommitTransactionAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _spiralBioWeeklyPatternDetails.RollbackTransactionAsync();
                throw new Exception("Error occurred during the deletion of data.", ex);
            }
        }
        #endregion


        #region Get Spiral Weekly Patterns List
        public async Task<(List<SpiralWeeklyPatternList> Data, SeparatePaginationInfo Pagination)> GetAllSpiralWeeklyPatternAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralWeeklyPatternID",
            string sortOrder = "desc")
        {
            var rawData = await _genericRepository.AllActive()
                .Select(x => new
                {
                    x.SpiralWeeklyPatternID,
                    x.SpiralWeeklyPatternName,
                    x.OrganizationID,
                    x.SpiralPatternTypeID,
                    OrganizationName = x.Organization.OrganizationName,
                    SpiralWeeklyPatternDetailsListVMs = x.SpiralWeeklyPatternDetails.Select(d => new SpiralWeeklyPatternDetailsListVM
                    {
                        SpiralWeeklyPatternDetailID = d.SpiralWeeklyPatternDetailID,
                        DayOfWeek = d.DayOfWeek,
                        ShiftID = d.ShiftID,
                        ShiftName =d.Shift.ShiftName,
                        ShiftTime = $"{d.Shift.StartTime} - {d.Shift.EndTime}"
                    }).ToList()
                }).AsNoTracking().ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();
                rawData = rawData
                    .Where(x =>
                        x.SpiralWeeklyPatternName.ToLower().Contains(lowerSearch) ||
                        x.OrganizationName.ToLower().Contains(lowerSearch))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(sortColumn))
            {
                rawData = sortColumn.ToLower() switch
                {
                    "spiralweeklypatternname" => (sortOrder == "asc"
                        ? rawData.OrderBy(x => x.SpiralWeeklyPatternName)
                        : rawData.OrderByDescending(x => x.SpiralWeeklyPatternName)).ToList(),

                    "organizationname" => (sortOrder == "asc"
                        ? rawData.OrderBy(x => x.OrganizationName)
                        : rawData.OrderByDescending(x => x.OrganizationName)).ToList(),

                    _ => rawData.OrderByDescending(x => x.SpiralWeeklyPatternID).ToList()
                };
            }

            var totalCount = rawData.Count;
            var pagedResult = pageSize == 0
                ? rawData
                : rawData.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Map anonymous type to SpiralWeeklyPatternList
            var mappedResult = pagedResult.Select(x => new SpiralWeeklyPatternList
            {
                SpiralWeeklyPatternID = x.SpiralWeeklyPatternID,
                SpiralPatternName = x.SpiralWeeklyPatternName,
                OrganizationID = x.OrganizationID,
                SpiralPatternTypeID = x.SpiralPatternTypeID,
                OrganizationName = x.OrganizationName,
                SpiralWeeklyPatternDetailsListVMs = x.SpiralWeeklyPatternDetailsListVMs
            }).ToList();

            var pagination = new SeparatePaginationInfo
            {
                StartItem = (pageNumber - 1) * pageSize + 1,
                EndItem = Math.Min(pageNumber * pageSize, totalCount),
                TotalItems = totalCount,
                CurrentPage = pageNumber,
                TotalPages = pageSize == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize),
                PageNumbers = pageSize == 0
                    ? new List<int> { 1 }
                    : Enumerable.Range(1, (int)Math.Ceiling(totalCount / (double)pageSize)).ToList()
            };

            return (mappedResult, pagination);
        }
        #endregion


        #region Get Spiral Fortnightly Patterns List
        public async Task<(List<SpiralBioWeeklyPatternListVM> Data, SeparatePaginationInfo Pagination)> GetAllSpiralFortnightlyPatternAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralBioWeeklyPatternID",
            string sortOrder = "desc")
        {
            var rawData = await _spiralBioWeeklyPattern.AllActive()
                .Select(x => new
                {
                    x.SpiralBioWeeklyPatternID,
                    x.SpiralBioWeeklyPatternName,
                    x.OrganizationID,
                    x.SpiralPatternTypeID,
                    OrganizationName = x.Organization.OrganizationName,
                    SpiralBioWeeklyPatternDetailsListVMs = x.SpiralBioWeeklyPatternDetails.Select(d => new SpiralBioWeeklyPatternDetailsListVM
                    {
                        SpiralBioWeeklyPatternDetailID = d.SpiralBioWeeklyPatternDetailID,
                        DayOfMonth = d.DayOfMonth,
                        ShiftID = d.ShiftID,
                        ShiftName = d.Shift.ShiftName,
                        ShiftTime = $"{d.Shift.StartTime} - {d.Shift.EndTime}"
                    }).ToList()
                }).AsNoTracking().ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();
                rawData = rawData
                    .Where(x =>
                        x.SpiralBioWeeklyPatternName.ToLower().Contains(lowerSearch) ||
                        x.OrganizationName.ToLower().Contains(lowerSearch))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(sortColumn))
            {
                rawData = sortColumn.ToLower() switch
                {
                    "spiralbioweeklypatternname" => (sortOrder == "asc"
                        ? rawData.OrderBy(x => x.SpiralBioWeeklyPatternName)
                        : rawData.OrderByDescending(x => x.SpiralBioWeeklyPatternName)).ToList(),

                    "organizationname" => (sortOrder == "asc"
                        ? rawData.OrderBy(x => x.OrganizationName)
                        : rawData.OrderByDescending(x => x.OrganizationName)).ToList(),

                    _ => rawData.OrderByDescending(x => x.SpiralBioWeeklyPatternID).ToList()
                };
            }

            var totalCount = rawData.Count;
            var pagedResult = pageSize == 0
                ? rawData
                : rawData.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Map anonymous type to SpiralBioWeeklyPatternListVM
            var mappedResult = pagedResult.Select(x => new SpiralBioWeeklyPatternListVM
            {
                SpiralBioWeeklyPatternID = x.SpiralBioWeeklyPatternID,
                SpiralBioWeeklyPatternName = x.SpiralBioWeeklyPatternName,
                OrganizationID = x.OrganizationID,
                OrganizationName = x.OrganizationName,
                SpiralPatternTypeID = x.SpiralPatternTypeID,
                SpiralBioWeeklyPatternDetailsListVMs = x.SpiralBioWeeklyPatternDetailsListVMs
            }).ToList();

            var pagination = new SeparatePaginationInfo
            {
                StartItem = (pageNumber - 1) * pageSize + 1,
                EndItem = Math.Min(pageNumber * pageSize, totalCount),
                TotalItems = totalCount,
                CurrentPage = pageNumber,
                TotalPages = pageSize == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize),
                PageNumbers = pageSize == 0
                    ? new List<int> { 1 }
                    : Enumerable.Range(1, (int)Math.Ceiling(totalCount / (double)pageSize)).ToList()
            };

            return (mappedResult, pagination);
        }
        #endregion


        #region Get Spiral Fortnightly Patterns List
        public async Task<(List<SpiralMonthlyPatternListVM> Data, SeparatePaginationInfo Pagination)> GetAllSpiralMonthlyPatternAsync(
            int pageNumber = 1,
            int pageSize = 5,
            string searchTerm = "",
            string sortColumn = "SpiralMonthlyPatternID",
            string sortOrder = "desc")
        {
            var rawData = await _spiralMonthlyPattern.AllActive()
                .Select(x => new
                {
                    x.SpiralMonthlyPatternID,
                    x.SpiralMonthlyPatternName,
                    x.OrganizationID,
                    x.SpiralPatternTypeID,
                    OrganizationName = x.Organization.OrganizationName,
                    SpiralMonthlyPatternDetailsListVMs = x.SpiralMonthlyPatternDetails.Select(d => new SpiralMonthlyPatternDetailsListVM
                    {
                        SpiralMonthlyPatternDetailID = d.SpiralMonthlyPatternDetailID,
                        DayOfMonth = d.DayOfMonth,
                        ShiftID = d.ShiftID,
                        ShiftName = d.Shift.ShiftName,
                        ShiftTime = $"{d.Shift.StartTime} - {d.Shift.EndTime}"
                    }).ToList()
                }).AsNoTracking().ToListAsync();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lowerSearch = Regex.Replace(searchTerm, @"\s+", " ").Trim().ToLower();
                rawData = rawData
                    .Where(x =>
                        x.SpiralMonthlyPatternName.ToLower().Contains(lowerSearch) ||
                        x.OrganizationName.ToLower().Contains(lowerSearch))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(sortColumn))
            {
                rawData = sortColumn.ToLower() switch
                {
                    "spiralmonthlypatternname" => (sortOrder == "asc"
                        ? rawData.OrderBy(x => x.SpiralMonthlyPatternName)
                        : rawData.OrderByDescending(x => x.SpiralMonthlyPatternName)).ToList(),

                    "organizationname" => (sortOrder == "asc"
                        ? rawData.OrderBy(x => x.OrganizationName)
                        : rawData.OrderByDescending(x => x.OrganizationName)).ToList(),

                    _ => rawData.OrderByDescending(x => x.SpiralMonthlyPatternID).ToList()
                };
            }

            var totalCount = rawData.Count;
            var pagedResult = pageSize == 0
                ? rawData
                : rawData.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            // Map anonymous type to SpiralMonthlyPatternListVM
            var mappedResult = pagedResult.Select(x => new SpiralMonthlyPatternListVM
            {
                SpiralMonthlyPatternID = x.SpiralMonthlyPatternID,
                SpiralMonthlyPatternName = x.SpiralMonthlyPatternName,
                OrganizationID = x.OrganizationID,
                OrganizationName = x.OrganizationName,
                SpiralPatternTypeID = x.SpiralPatternTypeID,
                SpiralMonthlyPatternDetailsListVMs = x.SpiralMonthlyPatternDetailsListVMs
            }).ToList();

            var pagination = new SeparatePaginationInfo
            {
                StartItem = (pageNumber - 1) * pageSize + 1,
                EndItem = Math.Min(pageNumber * pageSize, totalCount),
                TotalItems = totalCount,
                CurrentPage = pageNumber,
                TotalPages = pageSize == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize),
                PageNumbers = pageSize == 0
                    ? new List<int> { 1 }
                    : Enumerable.Range(1, (int)Math.Ceiling(totalCount / (double)pageSize)).ToList()
            };

            return (mappedResult, pagination);
        }
        #endregion
    }
}
