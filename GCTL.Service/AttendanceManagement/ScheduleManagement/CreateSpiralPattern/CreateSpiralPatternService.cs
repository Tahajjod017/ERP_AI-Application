using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.CreateSpiralPattern;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task<bool> AddAsync(CreateSpiralPatternVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if (model.SpiralPatternTypeID == 1)
                {
                    SpiralWeeklyPattern spiralWeeklyPattern = new SpiralWeeklyPattern();
                    spiralWeeklyPattern.SpiralWeeklyPatternName = model.SpiralWeeklyPatternName;
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
                    spiralBioWeeklyPattern.SpiralBioWeeklyPatternName = model.SpiralBioWeeklyPatternName;
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
                    spiralMonthlyPattern.SpiralMonthlyPatternName = model.SpiralMonthlyPatternName;
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
    }
}
