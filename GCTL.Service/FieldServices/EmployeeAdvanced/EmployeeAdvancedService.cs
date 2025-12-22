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
using GCTL.Service.Pagination;
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
        public readonly IGenericRepository<EmployeeAdvanceFor> _employeeAdvanceForRepository;
        public readonly IGenericRepository<Customers> _customer;
        public readonly IGenericRepository<Jobs> _job;
        public readonly IGenericRepository<GroupEmployee> _groupEmployeeRepository;

        public EmployeeAdvancedService(IGenericRepository<EmployeeAdvances> genericRepository, IGenericRepository<Data.Models.Employees> employees, IGenericRepository<JobTypes> jobtyperepository, IGenericRepository<EmployeeAdvanceFor> employeeAdvanceForRepository, IGenericRepository<Customers> customer, IGenericRepository<Jobs> job, IGenericRepository<GroupEmployee> groupEmployeeRepository) : base(genericRepository)
        {
            _genericRepository = genericRepository;
            _employees = employees;
            _jobtyperepository = jobtyperepository;
            _employeeAdvanceForRepository = employeeAdvanceForRepository;
            _customer = customer;
            _job = job;
            _groupEmployeeRepository = groupEmployeeRepository;
        }

        #region Add
        public async Task<CommonReturnViewModel> AddAsync(EmployeeAdvancedVM emp)
        {
            try
            {

                await _genericRepository.BeginTransactionAsync();
                EmployeeAdvances empadvance = new EmployeeAdvances();

                empadvance.EmployeeAdvanceID = emp.EmployeeAdvanceID;
                empadvance.JobID = emp.JobID;
                empadvance.AmountRequested = emp.AmountRequested;
                empadvance.StartDate = emp.StartDate.HasValue ? DateOnly.FromDateTime(emp.StartDate.Value) : null;
                empadvance.EndDate = emp.EndDate.HasValue ? DateOnly.FromDateTime(emp.EndDate.Value) : null;
                empadvance.LIP = emp.LIP;
                empadvance.LMAC = emp.LMAC;
                empadvance.CreatedAt = DateTime.UtcNow;
                empadvance.CreatedBy = emp.CreatedBy;
                empadvance.UpdatedBy = emp.UpdatedBy;
                empadvance.RequestedByUserID = emp.ApprovedByUserID ;
                empadvance.ApprovalStatusID = 11; // Pending
                
               
                //empadvance.JobID = emp.JobID;
                await _genericRepository.AddAsync(empadvance);

                //(Multiple JobType Save to EmployeeAdvanceFor Table 
                if (emp.RequestedByUserID != null)
                {
                    foreach (var item in emp.RequestedByUserID)
                    {
                        EmployeeAdvanceFor employeeAdvanceFor = new EmployeeAdvanceFor();
                        employeeAdvanceFor.EmployeeAdvanceID = empadvance.EmployeeAdvanceID;
                        employeeAdvanceFor.JobTypeID = item;
                        employeeAdvanceFor.LIP = emp.LIP; 
                        employeeAdvanceFor.LMAC = emp.LMAC;
                        employeeAdvanceFor.CreatedAt = DateTime.Now;
                        employeeAdvanceFor.CreatedBy = emp.CreatedBy;

                        await _employeeAdvanceForRepository.AddAsync(employeeAdvanceFor);

                    }
                    // Multiple Grop Employee Save to GroupEmployee Table
                    if (emp.GroupEmployeeID != null)
                    {
                        foreach (var item in emp.GroupEmployeeID) 
                        {
                            GroupEmployee groupEmployee = new GroupEmployee();
                            groupEmployee.EmployeeAdvanceID = empadvance.EmployeeAdvanceID;
                            groupEmployee.EmployeeID = item;
                            groupEmployee.LIP = emp.LIP;
                            groupEmployee.LMAC = emp.LMAC;
                            groupEmployee.CreatedAt = DateTime.Now;
                            groupEmployee.CreatedBy = emp.CreatedBy;

                            await _groupEmployeeRepository.AddAsync(groupEmployee);


                        }
                    }
                    
                }
                
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
        #endregion

        #region EmployeeDD

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
        #endregion

        #region GetJob Service
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
        #endregion

        #region Approve Service
        public async Task<CommonReturnViewModel> ApproveAsync(int id, int approvedByUserId)
        {
            try
            {
                await _genericRepository.BeginTransactionAsync();

                var empAdvance = await _genericRepository.GetByIdAsync(id);

                if (empAdvance == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Advance not found"
                    };
                }

                if (empAdvance.ApprovalStatusID == 12)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Employee Advance is already approved"
                    };
                }

                empAdvance.ApprovalStatusID = 12; // Approved
                empAdvance.ApprovedByUserID = approvedByUserId;
                empAdvance.ApprovalDate = DateTime.UtcNow;
                empAdvance.UpdatedBy = approvedByUserId;
                empAdvance.UpdatedAt = DateTime.UtcNow;

                await _genericRepository.UpdateAsync(empAdvance);
                await _genericRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Employee Advance approved successfully"
                };
            }
            catch (Exception ex)
            {
                await _genericRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region GetJobsByCusId(Nestesd)
        public async Task<List<EmployeeAdvancedVM>> GetJobByCusId(int customerId)
        {
            try
            {
                var data = await (from j in _job.AllActive()
                                  join c in _customer.AllActive() on j.CustomerID equals c.CustomerID
                                  where j.CustomerID == customerId
                                  select new EmployeeAdvancedVM
                                  {
                                      JobID = j.JobID,
                                      JobTitle = j.JobTitle + " " + (j.StartDateTime.HasValue ? j.StartDateTime.Value.ToString("dd/MM/yyyy") : string.Empty) + " - " + (j.EndDateTime.HasValue ? j.EndDateTime.Value.ToString("dd/MM/yyyy") : string.Empty)
                                  }).ToListAsync();

                return data;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        #endregion

        #region GetAllAsync
        public Task<PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.PaginationResult<EmployeeAdvancedVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainempId = null)
        {
            try
            {
                var query = _genericRepository.AllActive()
                    .Include(e => e.EmployeeAdvanceFor)
                    .Include(e => e.Job)
                    .Include(e => e.GroupEmployee)
                    .AsNoTracking()
                    .Where(x => x.DeletedAt == null && x.DeletedBy == null);

                if (mainempId != null)
                {
                    query = query.Where(x => x.EmployeeAdvanceID == mainempId);
                }

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "EmployeeAdvancedID" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.EmployeeAdvanceID)
                            : query.OrderBy(x => x.EmployeeAdvanceID),

                        //"" => sortOrder == "desc"
                        //    ? query.OrderByDescending(x => x.MainAccount.Class.ClassName)
                        //    : query.OrderBy(x => x.MainAccount.Class.ClassName),

                        "JobID" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.Job.JobID)
                            : query.OrderBy(x => x.Job.JobID),

                        "GroupEmployee" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.GroupEmployee)
                            : query.OrderBy(x => x.GroupEmployee),

                        "Status" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.ApprovalStatus)
                            : query.OrderBy(x => x.ApprovalStatusID),

                        "Description" => sortOrder == "desc"
                            ? query.OrderByDescending(x => x.RequestedByUser)
                            : query.OrderBy(x => x.RequestedByUserID),

                        _ => query.OrderBy(x => x.EmployeeAdvanceID)
                    };
                }

                return PaginationService<EmployeeAdvances, EmployeeAdvancedVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    searchPredicate: (term) => x =>
                        x.Job.JobTitle.ToLower().Contains(term) ||
                        x.AmountRequested.ToString().ToLower().Contains(term),
                    selector: x => new EmployeeAdvancedVM
                    
                    {
                        EmployeeAdvanceID = x.EmployeeAdvanceID,
                        CustomerID2 = x.Job.CustomerID,
                        JobID = x.JobID,
                        JobTitle = x.Job.JobTitle,
                        AmountRequested = x.AmountRequested,
                        // Fix for CS0029: Convert nullable int to non-nullable int using `.Value` and filter out nulls using `.Where`.
                        GroupEmployeeID = x.GroupEmployee
                            .Select(ge => ge.EmployeeID)
                            .Where(id => id.HasValue) // Filter out null values
                            .Select(id => id.Value)  // Convert nullable int to non-nullable int
                            .ToList(),
                        ApprovalStatusID = x.ApprovalStatusID,
                        StartDate = x.StartDate.HasValue ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,


                        //JobID = x.JobID,
                        //JobTitle = x.Job.JobTitle,
                        //AmountRequested = x.AmountRequested,
                        //StartDate = x.StartDate.HasValue ? x.StartDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        //EndDate = x.EndDate.HasValue ? x.EndDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        //ApprovalStatusID = x.ApprovalStatusID,


                        //ApprovalStatusName = x.ApprovalStatusID == 11 ? "Pending" :
                        //                     x.ApprovalStatusID == 12 ? "Approved" : "Unknown",
                        //RequestedByUserID = x.EmployeeAdvanceFor.Select(eaf => eaf.JobTypeID).ToList(),
                        //GroupEmployeeID = x.GroupEmployee.Select(ge => ge.EmployeeID).ToList(),
                    }
                );
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving Add EmployeeAdvanced.", ex);
            }

        }
        #endregion
    }
}
