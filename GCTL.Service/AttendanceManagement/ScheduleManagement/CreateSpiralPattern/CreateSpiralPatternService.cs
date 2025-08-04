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

        public CreateSpiralPatternService(IGenericRepository<SpiralWeeklyPattern> genericRepository, IGenericRepository<SpiralWeeklyPatternDetails> spiralWeeklyPatternDetailsRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _spiralWeeklyPatternDetailsRepository = spiralWeeklyPatternDetailsRepository;
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

                foreach(var detail in model.SpiralWeeklyPatternDetails)
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
