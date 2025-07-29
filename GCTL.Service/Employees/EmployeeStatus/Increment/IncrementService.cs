using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Increment;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeStatus.Increment
{
    public class IncrementService : IincrementService
    {
        private readonly IGenericRepository<EmployeeActionTypes> _employeeActionTypeRepository;
        private readonly IGenericRepository<EmployeeCareerChangeHistory> _employeeCarrerCngHistoryRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _employeeCarrerCngRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;

        public IncrementService(IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository, IGenericRepository<EmployeeCareerChangeHistory> employeeCarrerCngHistoryRepository, IGenericRepository<EmployeeCareerChanges> employeeCarrerCngRepository, IGenericRepository<Statuses> statusRepository)
        {
            _employeeActionTypeRepository = employeeActionTypeRepository;
            _employeeCarrerCngHistoryRepository = employeeCarrerCngHistoryRepository;
            _employeeCarrerCngRepository = employeeCarrerCngRepository;
            _statusRepository = statusRepository;
        }

        public async Task<CommonReturnViewModel> SaveSalaryChange(SalaryChangeViewModel model)
        {
            if (model == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid model"
                };
            }

            var actionType = await _employeeActionTypeRepository.AllActive().Where(e => e.EmployeeActionTypeName.ToLower().Contains(model.ChangeType)).FirstOrDefaultAsync();

            if (actionType == null)
            {
                actionType = new EmployeeActionTypes()
                {
                    EmployeeActionTypeName = model.ChangeType,
                    CreatedAt = DateTime.UtcNow,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedBy = model.CreatedBy,

                };
                await _employeeActionTypeRepository.AddAsync(actionType);
            }

            var status = await _statusRepository.AllActive().Where(s => s.StatusName.ToLower() == "pending").FirstOrDefaultAsync();

            if (status == null) {
                status = new Statuses()
                {
                    StatusName = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedBy = model.CreatedBy,
                    StatusType = "EmployeeCareerChange"
                };
                await _statusRepository.AddAsync(status);
            }

            try
            {
                // Create a new EmployeeCareerChanges entity
                var careerChange = new EmployeeCareerChanges
                {
                    EmployeeID = model.EmployeeId,
                    EmployeeActionTypeID = actionType.EmployeeActionTypeID,
                    EffectiveDate = model.EffectiveDate,
                    Remarks = model.Remarks,
                    NewSalary = model.NewSalary,
                    CurrentSalary = model.CurrentSalary,
                    StatusID = status.StatusID,
                    CreatedAt = DateTime.UtcNow,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    CreatedBy = model.CreatedBy, // Assuming you have a CreatedBy field in the model
                   
                  

                };
                // Add the new career change to the repository
                await _employeeCarrerCngRepository.AddAsync(careerChange);
              
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Salary change saved successfully"
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the salary change: " + ex.Message
                };
            }
        }
    }
}
