using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;


namespace GCTL.Service.FieldServices
{
    public class CreateJobService : AppService<LeadActivityTypes>, ICreateJobService
    {
        private readonly IGenericRepository<Jobs> _jobsRepository;
        private readonly IGenericRepository<Leads> _LeadsRepository;
        public CreateJobService(IGenericRepository<LeadActivityTypes> genericRepository, IGenericRepository<Jobs> jobsRepository, IGenericRepository<Leads> leadsRepository) : base(genericRepository)
        {
            _jobsRepository = jobsRepository;
            _LeadsRepository = leadsRepository;
        }

        public async Task<bool> AddAsync(CreateJobVM model)
        {
            try
            {
                Jobs job = new Jobs
                {
                    JobTitle = model.JobTitle,
                    JobTypeID = model.JobID,
                    CustomerID = model.CustomerID,
                    JobStatusID = model.StatusID,
                    StartDateTime = model.StartDate,
                    EndDateTime = model.EndDate,
                    Note = model.Note,
                    FileLink = model.FileLink
                };

                if (model.TeamMembers != null)
                {
                    foreach (var memberId in model.TeamMembers)
                    {
                        job.JobTeams.Add(new JobTeams {EmployeeID  = memberId , JobID = job.JobID});
                    }
                }

               await _jobsRepository.AddAsync(job);

                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region get Customer List 
        public async Task<ReturnDataView<CustomerInfoVM>> GetPagedEmployeesAsync(string search, int page, int pageSize, int organizationID)
        {
            // Base query: filter first
            var query = _LeadsRepository
                .AllActive()
                .Where(q => q.Customer.OrganizationID == organizationID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c.Customer != null &&
                    (
                        EF.Functions.Like(c.Customer.FullName, pattern) ||
                        c.Customer.CustomerAddresses.Any(ca =>
                            ca.Address != null &&
                            EF.Functions.Like(ca.Address.Email, pattern) ||
                            EF.Functions.Like(ca.Address.Phone, pattern)
                        )
                    ));
            }

            query = query
                .Include(i => i.Customer)
                    .ThenInclude(t => t.CustomerAddresses)
                        .ThenInclude(t => t.Address);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Customer.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new CustomerInfoVM
                {
                    Email = t.Customer.CustomerAddresses.Select(ca=> ca.Address.Email).FirstOrDefault(),
                    LeadName=t.LeadName,
                    Phone = t.Customer.CustomerAddresses.Select(ca => ca.Address.Phone).FirstOrDefault(),
                })
                .ToListAsync();

            return new ReturnDataView<CustomerInfoVM>
            {
                data = items,
                totalItem = totalCount,
                message = "Data loaded"
            };
        }

        #endregion

        public Task<PaginationService<Grade, CreateJobVM>.PaginationResult<CreateJobVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CreateJobID", string sortOrder = "asc")
        {
            throw new NotImplementedException();
        }

        public Task<CreateJobVM> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CreateJobVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(CreateJobVM model)
        {
            throw new NotImplementedException();
        }

    }
}
