using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;


namespace GCTL.Service.FieldServices
{
    public class CreateJobService : AppService<LeadActivityTypes>, ICreateJobService
    {
        #region service & repository
        private readonly IGenericRepository<Jobs> _jobsRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly AppDbContext _context;
        public CreateJobService(IGenericRepository<LeadActivityTypes> genericRepository, IGenericRepository<Jobs> jobsRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, AppDbContext context, IGenericRepository<Country> countryRepository) : base(genericRepository)
        {
            _jobsRepository = jobsRepository;
            _customersRepository = customersRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            _context = context;
            _countryRepository = countryRepository;
        } 
        #endregion

        #region AddAsync
        public async Task<bool> AddAsync(CreateJobVM model, string FileLink)
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
        #endregion

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

        #region get Customer List 
        public async Task<ReturnDataView<SelectListItem>> GetCountryList(string search, int page, int pageSize, int organizationID)
        {
            var query = _countryRepository
                .AllActive();
              

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.CountryName, pattern) ||
                        EF.Functions.Like(c.CountryCode, pattern)
                    ));
            }


            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new SelectListItem
                {
                    Value = t.CountryID.ToString(),
                    Text = t.CountryName
                }).ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = items,
                totalItem = totalCount,
                message = "Data loaded"
            };
        }
        #endregion

        #region get Compnay Customer List 
        public async Task<ReturnDataView<CustomerInfoVM>> GetCompanyEmployeesAsync(string search, int page, int pageSize, int organizationID)
        {
            var query = _customersRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID && q.IsPerson == false);

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
        #region get Compnay Customer List 
        public async Task<ReturnDataView<CustomerInfoVM>> GetIndividualEmployeesAsync(string search, int page, int pageSize, int organizationID)
        {
            var query = _customersRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID && q.IsPerson == true);

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

        #region GetAllAsync
        public async Task<ReturnDataView<CreateJobVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5,
            string searchTerm = "", string sortColumn = "CreateJobID", string sortOrder = "asc")
        {
            var query = _context.Jobs
                .AsNoTracking()
                .Where(t => t.Customer.OrganizationID == organizationID);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(t => EF.Functions.Like(t.JobTitle, $"%{searchTerm}%"));
            }

            var totalCount = await query.CountAsync();

            query = sortColumn.ToLower() switch
            {
                "jobtitle" => sortOrder == "asc" ? query.OrderBy(t => t.JobTitle) : query.OrderByDescending(t => t.JobTitle),
                "startdatetime" => sortOrder == "asc" ? query.OrderBy(t => t.StartDateTime) : query.OrderByDescending(t => t.StartDateTime),
                _ => query.OrderByDescending(t => t.StartDateTime)
            };

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new CreateJobVM
                {
                    JobID = t.JobID,
                    CustomerName = t.Customer != null ? t.Customer.FullName : "",
                    JobTitle = t.JobTitle!= null ? t.JobTitle : "",
                    StartDate = t.StartDateTime.ToString(),
                    EndDate = t.EndDateTime.ToString(),
                    StatusName = t.JobStatus != null ? t.JobStatus.StatusName : string.Empty,
                    JobLocation = t.Location != null ? t.Location : string.Empty,
                    Note = t.Note,
                    JobType = t.JobType != null ? t.JobType.JobTypeName : "",
                })
                .ToListAsync();

            return new ReturnDataView<CreateJobVM>
            {
                data = data,
                totalItem = totalCount,
            };
        }
        #endregion

        #region pending
        public async Task<CreateJobVM> GetByIdAsync(int organizationID, int jobId)
        {
            var query = await _context.Jobs
                 .AsNoTracking()
                 .Where(t => t.Customer != null && t.Customer.OrganizationID == organizationID && t.JobID == jobId).Select(t => new CreateJobVM
                 {
                     JobID = t.JobID,
                     CustomerName = t.Customer != null ? t.Customer.FullName : "",
                     JobTitle = t.JobTitle != null ? t.JobTitle : "",
                     StartDate = t.StartDateTime.ToString(),
                     EndDate = t.EndDateTime.ToString(),
                     StatusName = t.JobStatus != null ? t.JobStatus.StatusName : string.Empty,
                     JobLocation = t.Location != null ? t.Location : string.Empty,
                     Note = t.Note,
                     JobType = t.JobType != null ? t.JobType.JobTypeName : "",
                 }).FirstOrDefaultAsync();

            return query ?? new CreateJobVM();
        }

        public Task<CreateJobVM> SoftDeleteAsync(DeleteRequestVM requestVM)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(CreateJobVM model)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
