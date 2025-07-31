using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeStatusManagement.Promotion;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.Employees.EmployeeStatus.Promotion
{
    public class PromotionService : IPromotionService
    {

        private readonly IGenericRepository<EmployeeActionTypes> _employeeActionTypeRepository;
        private readonly IGenericRepository<EmployeeCareerChangeHistory> _employeeCarrerCngHistoryRepository;
        private readonly IGenericRepository<EmployeeCareerChanges> _employeeCarrerCngRepository;
        private readonly IGenericRepository<Statuses> _statusRepository;

        public PromotionService(IGenericRepository<EmployeeActionTypes> employeeActionTypeRepository, IGenericRepository<EmployeeCareerChangeHistory> employeeCarrerCngHistoryRepository, IGenericRepository<EmployeeCareerChanges> employeeCarrerCngRepository, IGenericRepository<Statuses> statusRepository)
        {
            _employeeActionTypeRepository = employeeActionTypeRepository;
            _employeeCarrerCngHistoryRepository = employeeCarrerCngHistoryRepository;
            _employeeCarrerCngRepository = employeeCarrerCngRepository;
            _statusRepository = statusRepository;
        }

        public Task GetPagedPromotionListAsync(PromotionListFilterViewModel filters)
        {
            throw new NotImplementedException();
        }


        #region Save Method

        public async Task<CommonReturnViewModel> SaveAsync(PromotionViewModel model)
        {
         
            if (model == null)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Invalid model"
                };
            }

            // Ensure EmployeeActionType exists (Promotion/Demotion)
            var actionType = await _employeeActionTypeRepository.AllActive()
                .FirstOrDefaultAsync(e => e.EmployeeActionTypeName.ToLower() == model.ChangeType.ToLower());

            if (actionType == null)
            {
                actionType = new EmployeeActionTypes
                {
                    EmployeeActionTypeName = model.ChangeType,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };
                await _employeeActionTypeRepository.AddAsync(actionType);
            }

            // Ensure "Pending" Status exists
            var status = await _statusRepository.AllActive()
                .FirstOrDefaultAsync(s => s.StatusName.ToLower() == "pending");

            if (status == null)
            {
                status = new Statuses
                {
                    StatusName = "Pending",
                    StatusType = "EmployeeCareerChange",
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };
                await _statusRepository.AddAsync(status);
            }

            try
            {
                var promotion = new EmployeeCareerChanges
                {
                    EmployeeID = model.EmployeeID,
                    EmployeeActionTypeID = actionType.EmployeeActionTypeID,
                    CurentDesignationID = model.CurrentDesignationID,
                    NewDesignationID = model.NewDesignationID,
                    //DepartmentID = model.DepartmentId,
                    //OrganizationID = model.OrganizationId,
                    EffectiveDate = model.EffectiveDate,
                    Remarks = model.Remarks,
                    CurrentSalary = model.CurrentSalary,
                    NewSalary = model.NewSalary,
                    StatusID = status.StatusID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC
                };

                await _employeeCarrerCngRepository.AddAsync(promotion);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Promotion saved successfully"
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving the promotion: " + ex.Message
                };
            }
        }


        #endregion

    }
}
