using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
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
                    OrganizationName = x.Organization.OrganizationName,
                    SpiralWeeklyPatternDetailsVMs = x.SpiralWeeklyPatternDetails.Select(d => new SpiralWeeklyPatternDetailsVM
                    {
                        SpiralWeeklyPatternDetailID = d.SpiralWeeklyPatternDetailID,
                        DayOfWeek = d.DayOfWeek,
                        ShiftID = d.ShiftID,
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
                OrganizationName = x.OrganizationName,
                SpiralWeeklyPatternDetailsVMs = x.SpiralWeeklyPatternDetailsVMs
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
