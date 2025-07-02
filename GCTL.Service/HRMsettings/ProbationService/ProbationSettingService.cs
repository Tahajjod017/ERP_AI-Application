using GCTL.Core.Repository;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.HRMsettings.ProbationService
{
    public class ProbationSettingService : AppService<ProbetionPeriodSettings>
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IGenericRepository<ProbetionPeriodSettings> _genericRepository;
        private readonly IGenericRepository<Organization> _genericRepositoryOraganization;
        public ProbationSettingService(IGenericRepository<ProbetionPeriodSettings> genericRepository, IUserInfoService userInfoService, IGenericRepository<Organization> genericRepositoryOraganization) : base(genericRepository)
        {
            _userInfoService = userInfoService;
            _genericRepositoryOraganization = genericRepositoryOraganization;
        }

    }
}
