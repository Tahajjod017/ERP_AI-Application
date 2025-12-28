using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Web.Helpers;


namespace GCTL.Service.FieldServices
{
    public class CreateJobService : AppService<LeadActivityTypes>, ICreateJobService
    {
        #region service & repository
        private readonly IGenericRepository<Jobs> _jobsRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Divisions> _divisionsRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        public readonly IGenericRepository<Statuses> _statusRepository;
        public readonly IGenericRepository<JobTypes> _jobTypeRepository;
        private readonly AppDbContext _context;

        public CreateJobService(IGenericRepository<LeadActivityTypes> genericRepository, IGenericRepository<Jobs> jobsRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, AppDbContext context, IGenericRepository<Country> countryRepository, IGenericRepository<Divisions> divisionsRepository, IGenericRepository<Statuses> statusRepository, IGenericRepository<JobTypes> jobTypeRepository) : base(genericRepository)
        {
            _jobsRepository = jobsRepository;
            _customersRepository = customersRepository;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            _context = context;
            _countryRepository = countryRepository;
            _divisionsRepository = divisionsRepository;
            _statusRepository = statusRepository;
            _jobTypeRepository = jobTypeRepository;
        }
        #endregion

        #region AddAsync
        public async Task<bool> AddAsync(CreateJobVM model, string FileLink)
        {
            try
            {

                DateTime startDate = DateTime.ParseExact(model.StartDate?? "", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(model.EndDate?? "", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                Jobs job = new Jobs
                {
                    JobTitle = model.JobTitle,
                    CustomerID = model.CustomerID,
                    JobTypeID = model.JobTypeID,
                    DivisionID = model.DivisionId,
                    JobStatusID = model.StatusID,
                    StartDateTime = startDate,
                    EndDateTime = endDate,
                    Note = model.Note,
                    FileLink = FileLink
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

        #region EditAsync
        public async Task<bool> EditAsync(CreateJobVM model, string FileLink)
        {
            try
            {
                var items = await _jobsRepository.AllActive().AsNoTracking().FirstOrDefaultAsync(x => x.JobID == model.JobID);

                if (items == null)
                    return false;

                DateTime startDate = DateTime.ParseExact(model.StartDate?? "", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                DateTime endDate = DateTime.ParseExact(model.EndDate?? "", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);


                items.JobTitle = model.JobTitle;
                items.CustomerID = model.CustomerID;
                items.JobTypeID = model.JobTypeID;
                items.JobStatusID = model.StatusID;
                items.StartDateTime = startDate;
                items.EndDateTime = endDate;
                items.Note = model.Note;
                items.FileLink = null;

               await _jobsRepository.UpdateAsync(items);

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

        #region Get Job Async
        public async Task<ReturnDataView<SelectListItem>> GetJobAsync(int customerId, string search, int page, int pageSize, int organizationID)
        {
            var query = _jobsRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID && q.CustomerID == customerId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.JobTitle, pattern)
                    ));
            }



            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize).Select(t => new SelectListItem
                {
                    
                    Value = t.JobID.ToString(),
                    Text = t.JobTitle,
                    
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

        #region Get Customer Info
        public CustomerInfoVM GetCustomerInfo(int jobId, int organizationID)
        {
            return _jobsRepository
                .AllActive().Include(x => x.Customer)
                .Where(q => q.JobID ==jobId && q.OrganizationID == organizationID).Select(t => new CustomerInfoVM
                {
                    LeadID = t.CustomerID ?? 0,
                    Email = t.Customer.CustomerAddresses.Select(ca => ca.Address.Email).FirstOrDefault(),
                    LeadName = t.Customer.FullName,
                    Phone = t.Customer.CustomerAddresses.Select(ca => ca.Address.Phone).FirstOrDefault(),
                }).FirstOrDefault()?? new CustomerInfoVM();
        }
        #endregion

        #region GetDivisionsAsync 
        public async Task<ReturnDataView<SelectListItem>> GetDivisionsAsync(string search)
        {
            var query = _divisionsRepository
                .AllActive();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.DivisionName, pattern)
                    ));
            }

            var items = await query
                .OrderBy(c => c.CreatedAt).Select(t => new SelectListItem
                {
                    Text = t.DivisionName,
                    Value = t.DivisionID.ToString()
                })
                .ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = items,
                message = "Data loaded"
            };
        }
        #endregion

        #region Get JobTypes Async 
        public async Task<ReturnDataView<SelectListItem>> GetJobTypesAsync(string search)
        {
            var query = _jobTypeRepository
                .AllActive();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.JobTypeName, pattern)
                    ));
            }

            var items = await query
                .OrderBy(c => c.CreatedAt).Select(t => new SelectListItem
                {
                    Text = t.JobTypeName,
                    Value = t.JobTypeID.ToString()
                })
                .ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = items,
                message = "Data loaded"
            };
        }
        #endregion

        #region Get Statuses Async 
        public async Task<ReturnDataView<SelectListItem>> GetStatusesAsync(string search)
        {
            var query = _statusRepository
                .AllActive();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string pattern = $"%{search}%";

                query = query.Where(c =>
                    c != null &&
                    (
                        EF.Functions.Like(c.StatusName, pattern)
                    ));
            }

            var items = await query
                .OrderBy(c => c.CreatedAt).Select(t => new SelectListItem
                {
                    Text = t.StatusName,
                    Value = t.StatusID.ToString()
                })
                .ToListAsync();

            return new ReturnDataView<SelectListItem>
            {
                data = items,
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

        #region get Individual Customer List 
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
                    JobTypeID = t.JobID,
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
                     JobTypeID = t.JobID,
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

        #region GetCalenderData
        public async Task<CommonReturnViewModel> GetCalenderData(int organizationID, DateTime start, DateTime end, string searchTerm = "")
        {
            try
            {
                var jobs = await _context.Jobs
                    .AsNoTracking()
                    .Where(t => t.Customer != null && t.Customer.OrganizationID == organizationID)
                    .Where(t => t.StartDateTime >= start && t.StartDateTime <= end) // filter by calendar range
                    .Where(t => string.IsNullOrEmpty(searchTerm) || EF.Functions.Like(t.JobTitle, $"%{searchTerm}%"))
                    .Select(t => new
                    {
                        id = t.JobID,
                        title = t.JobTitle,
                        start = t.StartDateTime.HasValue ? t.StartDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss"): null,
                        end = t.EndDateTime.HasValue ? t.EndDateTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                        allDay = false,
                        customer = t.Customer.FullName,
                        status = t.JobStatus != null ? t.JobStatus.StatusName : ""
                    })
                    .ToListAsync();
                return new CommonReturnViewModel
                {
                    Success = true,
                    Data = jobs
                };
            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "Something goes to wrong"
                };
            }
            
        }
        #endregion
    }
}
