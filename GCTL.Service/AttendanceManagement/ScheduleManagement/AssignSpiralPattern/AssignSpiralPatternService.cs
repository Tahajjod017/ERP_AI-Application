using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement.ScheduleManagement.AssignSpiralPattern
{
    public class AssignSpiralPatternService : AppService<SpiralPatternAssignList>, IAssignSpiralPatternService
    {
        #region Repositories
        private readonly IGenericRepository<SpiralPatternAssignList> _genericRepository;

        public AssignSpiralPatternService(IGenericRepository<SpiralPatternAssignList> genericRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
        }
        #endregion


        public async Task<bool> AddAsync(AssignSpiralPatternSetupVM model)
        {
            return true;
        }
    }
}
