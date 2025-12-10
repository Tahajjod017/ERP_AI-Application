using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;

namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    public class EmployeeAdvancedService : AppService<EmployeeAdvances>, IEmployeeAdvanced
    {
        public readonly IGenericRepository<EmployeeAdvances> _genericRepository;

        public EmployeeAdvancedService(IGenericRepository<EmployeeAdvances> genericRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
        }

        public async Task<CommonReturnViewModel> AddAsync(EmployeeAdvancedVM emp)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();
                EmployeeAdvances empadvance = new EmployeeAdvances();

                empadvance.EmployeeAdvanceID = emp.EmployeeAdvanceID;
                empadvance.JobID = emp.JobID;
                empadvance.AmountRequested = emp.AmountRequested;
                empadvance.RequestedByUserID = emp.RequestedByUserID;
                empadvance.StartDate = emp.StartDate;
                empadvance.EndDate = emp.EndDate;
                empadvance.LIP = emp.LIP;
                empadvance.LMAC = emp.LMAC;
                empadvance.CreatedAt = DateTime.UtcNow;
                empadvance.CreatedBy = emp.CreatedBy;
                await _genericRepository.AddAsync(empadvance);
                await _genericRepository.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Advance Added Successfully"
                };



            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
