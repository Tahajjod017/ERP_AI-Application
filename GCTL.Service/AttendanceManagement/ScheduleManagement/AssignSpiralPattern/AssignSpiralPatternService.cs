using GCTL.Core.Repository;
using GCTL.Core.ViewModels.AttendanceManagement.ScheduleManagement.AssignSpiralPattern;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
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
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;

        public AssignSpiralPatternService(IGenericRepository<SpiralPatternAssignList> genericRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
        }
        #endregion


        #region AddAsync
        public async Task<bool> AddAsync(AssignSpiralPatternSetupVM model)
        {
            await _genericRepository.BeginTransactionAsync();
            try
            {
                if(model.OrganizationID != 0 && model.DepartmentIDs == null && model.EmployeeIDs == null)
                {
                    var employees = await _employeeOfficeInfoRepository.FindAsync(e => e.OrganizationID == model.OrganizationID);

                    foreach (var employee in employees)
                    {
                        var existingEntity = await _genericRepository.All()
                            .Where(e => e.OrganizationID == employee.OrganizationID 
                            && e.EmployeeID == employee.EmployeeID)
                            .FirstOrDefaultAsync();
                        if (existingEntity != null)
                        {

                        }
                        else
                        {
                            SpiralPatternAssignList entity = new SpiralPatternAssignList();
                            entity.OrganizationID = model.OrganizationID;
                            entity.EmployeeID = employee.EmployeeID;
                            entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                            entity.SpiralPatternID = model.SpiralPatternID;
                            entity.StartDate = model.StartDate;
                            entity.EndDate = model.EndDate;
                            entity.CreatedBy = model.CreatedBy;
                            entity.CreatedAt = DateTime.Now;
                            entity.LIP = model.LIP;
                            entity.LMAC = model.LMAC;
                            
                            await _genericRepository.AddAsync(entity);
                        }
                    }
                }
                else if (model.OrganizationID != null && model.DepartmentIDs != null && model.EmployeeIDs == null)
                {
                    foreach(var depId in model.DepartmentIDs)
                    {
                        var employees = await _employeeOfficeInfoRepository.FindAsync(e => e.OrganizationID == model.OrganizationID && e.DepartmentID == depId);
                        foreach (var employee in employees)
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(e => e.OrganizationID == employee.OrganizationID
                                && e.EmployeeID == employee.EmployeeID)
                                .FirstOrDefaultAsync();
                            if (existingEntity != null)
                            {
                            }
                            else
                            {
                                SpiralPatternAssignList entity = new SpiralPatternAssignList();
                                entity.OrganizationID = model.OrganizationID;
                                entity.EmployeeID = employee.EmployeeID;
                                entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                                entity.SpiralPatternID = model.SpiralPatternID;
                                entity.StartDate = model.StartDate;
                                entity.EndDate = model.EndDate;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                await _genericRepository.AddAsync(entity);
                            }
                        }
                    }
                }
                else if (model.EmployeeIDs != null)
                {
                    foreach (var empId in model.EmployeeIDs)
                    {
                        var employee = (await _employeeOfficeInfoRepository.FindAsync(e => e.EmployeeID == empId)).FirstOrDefault();
                        if (employee != null)
                        {
                            var existingEntity = await _genericRepository.All()
                                .Where(e => e.OrganizationID == employee.OrganizationID
                                && e.EmployeeID == employee.EmployeeID)
                                .FirstOrDefaultAsync();
                            if (existingEntity != null)
                            {
                            }
                            else
                            {
                                SpiralPatternAssignList entity = new SpiralPatternAssignList();
                                entity.OrganizationID = model.OrganizationID;
                                entity.EmployeeID = employee.EmployeeID;
                                entity.SpiralPatternTypeID = model.SpiralPatternTypeID;
                                entity.SpiralPatternID = model.SpiralPatternID;
                                entity.StartDate = model.StartDate;
                                entity.EndDate = model.EndDate;
                                entity.CreatedBy = model.CreatedBy;
                                entity.CreatedAt = DateTime.Now;
                                entity.LIP = model.LIP;
                                entity.LMAC = model.LMAC;
                                await _genericRepository.AddAsync(entity);
                            }
                        }
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
    }
}
