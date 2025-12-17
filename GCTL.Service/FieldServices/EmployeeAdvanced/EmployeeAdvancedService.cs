using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using NetTopologySuite.Precision;

namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    public class EmployeeAdvancedService : AppService<EmployeeAdvances>, IEmployeeAdvanced
    {
        public readonly IGenericRepository<EmployeeAdvances> _genericRepository;
        public readonly IGenericRepository<GCTL.Data.Models.Employees> _employees;
        public readonly IGenericRepository<GCTL.Data.Models.JobTypes> _jobtyperepository;






        public EmployeeAdvancedService(IGenericRepository<EmployeeAdvances> genericRepository, IGenericRepository<Data.Models.Employees> employees, IGenericRepository<JobTypes> jobtyperepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employees = employees;
            _jobtyperepository = jobtyperepository;
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
                empadvance.StartDate = emp.StartDate.HasValue ? DateOnly.FromDateTime(emp.StartDate.Value) : null;
                empadvance.EndDate = emp.EndDate.HasValue ? DateOnly.FromDateTime(emp.EndDate.Value) : null;
                empadvance.LIP = emp.LIP;
                empadvance.LMAC = emp.LMAC;
                empadvance.CreatedAt = DateTime.UtcNow;
                empadvance.CreatedBy = emp.CreatedBy;
                empadvance.UpdatedBy = emp.UpdatedBy;
                empadvance.ApprovedByUserID = emp.ApprovedByUserID;

                //empadvance.JobID = emp.JobID;
                await _genericRepository.AddAsync(empadvance);
                await _genericRepository.CommitTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Advance Added Successfully"
                };



            }
            catch (Exception ex)
            {

                await _genericRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

        }

        //Modern Dropdown for Employees
        public async Task<IEnumerable<CommonSelectVM>> EmployeeDD()
        {
            var data = await _employees.AllActive()
                .Select(x => new CommonSelectVM
                {
                    Id = x.EmployeeID,
                    Name = $"{x.FirstName} {x.LastName} {x.EmployeeCode}",

                }).ToListAsync();
            return data;
        }

        

        //Get Jobstype service

        public async Task<ReturnDataView<SelectListItem>> GetJobTypeAsync(string search, int page, int pageSize, int organizationID)
        {
            var query = _jobtyperepository.AllActive() 
                .Where(q => q.OrganizationID == organizationID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.JobTypeID, pattern)
                    ));
            }



            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new SelectListItem
                {

                    Value = t.JobTypeID.ToString(),
                    Text = t.JobTypeName,

                })
                .ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = items,
                totalItem = totalCount,
                message = "Data loaded"
            };
        }


    }
}
