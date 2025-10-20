using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;


namespace GCTL.Service.FieldServices
{
    public class CreateJobService : AppService<LeadActivityTypes>, ICreateJobService
    {
        private readonly IGenericRepository<Jobs> _jobsRepository;
        public CreateJobService(IGenericRepository<LeadActivityTypes> genericRepository, IGenericRepository<Jobs> jobsRepository) : base(genericRepository)
        {
            _jobsRepository = jobsRepository;
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
