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

        public CreateSpiralPatternService(IGenericRepository<SpiralWeeklyPattern> genericRepository, IGenericRepository<SpiralWeeklyPatternDetails> spiralWeeklyPatternDetailsRepository, IGenericRepository<SpiralBioWeeklyPattern> spiralBioWeeklyPattern, IGenericRepository<SpiralBioWeeklyPatternDetails> spiralBioWeeklyPatternDetails) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _spiralWeeklyPatternDetailsRepository = spiralWeeklyPatternDetailsRepository;
            _spiralBioWeeklyPattern = spiralBioWeeklyPattern;
            _spiralBioWeeklyPatternDetails = spiralBioWeeklyPatternDetails;
        }
        #endregion

        public async Task<bool> AddAsync(CreateSpiralPatternVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                SpiralWeeklyPattern spiralWeeklyPattern = new SpiralWeeklyPattern();
                spiralWeeklyPattern.SpiralWeeklyPatternName = model.SpiralWeeklyPatternName;
                spiralWeeklyPattern.OrganizationID = model.OrganizationID;
                spiralWeeklyPattern.CreatedAt = DateTime.Now;
                spiralWeeklyPattern.CreatedBy = model.CreatedBy;
                spiralWeeklyPattern.LIP = model.LIP;
                spiralWeeklyPattern.LMAC = model.LMAC;
                await _genericRepository.AddAsync(spiralWeeklyPattern);

                if (model.SpiralPatternTypeID == 1)
                {
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
                    foreach(var detail in model.SpiralBioWeeklyPatternDetailsVMs)
                    {
                        SpiralBioWeeklyPatternDetails spiralBioWeeklyPatternDetails = new SpiralBioWeeklyPatternDetails();
                        spiralBioWeeklyPatternDetails.SpiralBioWeeklyPatternID = spiralWeeklyPattern.SpiralWeeklyPatternID;
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
                    foreach(var detail in model.SpiralMonthlyPatternDetailsVMs)
                    {
                        SpiralMonthlyPatternDetails spiralMonthlyPatternDetails = new SpiralMonthlyPatternDetails();
                        spiralMonthlyPatternDetails.SpiralMonthlyPatternID = spiralWeeklyPattern.SpiralWeeklyPatternID;
                        spiralMonthlyPatternDetails.DayOfMonth = detail.DayOfMonth;
                        spiralMonthlyPatternDetails.ShiftID = detail.ShiftID;
                        spiralMonthlyPatternDetails.CreatedAt = DateTime.Now;
                        spiralMonthlyPatternDetails.CreatedBy = model.CreatedBy;
                        spiralMonthlyPatternDetails.LIP = model.LIP;
                        spiralMonthlyPatternDetails.LMAC = model.LMAC;
                        await _spiralBioWeeklyPatternDetails.AddAsync(spiralMonthlyPatternDetails);
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
