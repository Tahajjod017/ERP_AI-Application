using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace GCTL.Service.FieldServices
{
    public class CreateJobService : AppService<LeadActivityTypes>, ICreateJobService
    {
        private readonly IGenericRepository<Jobs> _jobsRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        public CreateJobService(IGenericRepository<LeadActivityTypes> genericRepository, IGenericRepository<Jobs> jobsRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository) : base(genericRepository)
        {
            _jobsRepository = jobsRepository;
            _customersRepository = customersRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
        }

        public async Task<bool> AddAsync(CreateJobVM model)
        {
            try
            {
                DateTime startDate = DateTime.ParseExact(model.StartDate?? "", "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(model.EndDate?? "", "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);

                Jobs job = new Jobs
                {
                    JobTitle = model.JobTitle,
                    CustomerID = model.CustomerID,
                    JobTypeID = model.JobID,
                    JobStatusID = model.StatusID,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    Note = model.Note,
                    FileLink = null
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
            var query = _customersRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.FullName, pattern) ||
                        c.CustomerAddresses.Any(ca =>
                            ca.Address != null &&
                            EF.Functions.Like(ca.Address.Email, pattern) ||
                            EF.Functions.Like(ca.Address.Phone, pattern)
                        )
                    ));
            }

            query = query
                .Include(t => t.CustomerAddresses)
                    .ThenInclude(t => t.Address);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new CustomerInfoVM
                {
                    LeadID = t.CustomerID,
                    Email = t.CustomerAddresses.Select(ca=> ca.Address.Email).FirstOrDefault(),
                    LeadName=t.FullName,
                    Phone = t.CustomerAddresses.Select(ca => ca.Address.Phone).FirstOrDefault(),
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
        #region get Technician List 
        public async Task<ReturnDataView<CustomerInfoVM>> GetTechnicianListAsync(string search, int page, int pageSize, int organizationID)
        {
            var query = _employeeOfficeInfoRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.Employee.FirstName, pattern) ||
                        EF.Functions.Like(c.Employee.LastName, pattern) ||
                        EF.Functions.Like(c.Employee.LastName, pattern) ||
                        EF.Functions.Like(c.Employee.Email, pattern) ||
                        EF.Functions.Like(c.Employee.MobileNumber, pattern)
                    ));
            }

            query = query
                .Include(t => t.Employee);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.Employee.FirstName?? "")
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new CustomerInfoVM
                {
                    LeadID = t.EmployeeID ?? 0,
                    Email = t.Employee.Email,
                    LeadName = t.Employee.FirstName + " " + t.Employee.LastName,
                    Phone = t.Employee.MobileNumber,
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
