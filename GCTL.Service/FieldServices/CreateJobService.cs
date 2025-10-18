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
        public CreateJobService(IGenericRepository<LeadActivityTypes> genericRepository) : base(genericRepository)
        {
        }

        public Task<bool> AddAsync(CreateJobVM model)
        {
            throw new NotImplementedException();
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
