using Bogus.DataSets;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkiaSharp;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        #region Repositories
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;
        private readonly IGenericRepository<Priorities> _prioritiesRepository;
        private readonly IGenericRepository<LeadStatuses> _statusesRepository;
        private readonly IGenericRepository<LeadSources> _sourcesRepository;
        private readonly IGenericRepository<LeadServices> _leadServicesRepository;
        private readonly IGenericRepository<GCTL.Data.Models.Services> _servicesRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly IGenericRepository<OtherContacts> _otherContactsRepository;

        #region Added by Md. Rakib Hasan
        private readonly IGenericRepository<Heads> _heads;
        private readonly IGenericRepository<HeadDetails> _headDetails;
        private readonly IGenericRepository<TransactionAccounts> _transactionAccounts;
        private readonly IGenericRepository<SubAccounts> _subAccounts;
        private readonly ITransactionAccountService _transactionAccountService;
        #endregion

        public LeadCreateService(AppDbContext context, IGenericRepository<LeadServices> leadServicesRepository, IGenericRepository<CompanyWarehouses> companyWarehousesRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<Leads> leadsRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<Heads> heads, IGenericRepository<HeadDetails> headDetails, IGenericRepository<TransactionAccounts> transactionAccounts, IGenericRepository<SubAccounts> subAccounts, ITransactionAccountService transactionAccountService, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Priorities> prioritiesRepository, IGenericRepository<LeadStatuses> statusesRepository, IGenericRepository<LeadSources> sourcesRepository, IGenericRepository<Services> servicesRepository, IGenericRepository<OtherContacts> otherContactsRepository)
        {
            _addressTypesRepository = addressTypesRepository;
            _leadsRepository = leadsRepository;
            _customerAddressesRepository = customerAddressesRepository;
            _leadServicesRepository = leadServicesRepository;
            _heads = heads;
            _headDetails = headDetails;
            _transactionAccounts = transactionAccounts;
            _subAccounts = subAccounts;
            _transactionAccountService = transactionAccountService;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            _prioritiesRepository = prioritiesRepository;
            _statusesRepository = statusesRepository;
            _sourcesRepository = sourcesRepository;
            _servicesRepository = servicesRepository;
            _otherContactsRepository = otherContactsRepository;
        }
        #endregion

        #region EnsureAddressTypesExist
        private async Task EnsureAddressTypesExist(int? createdBy, string? LIP, string? LMAC)
        {
            var items = await _addressTypesRepository.GetAllAsync();
            if (!items.Any())
            {
                var listItems = new string[] { "individual", "shipping", "company", "branch", "warehouse" };
                List<AddressTypes> newAddressTypes = new List<AddressTypes>();

                foreach (var item in listItems)
                {
                    newAddressTypes.Add(new AddressTypes
                    {
                        AddressTypeName = item,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        LIP = LIP,
                        LMAC = LMAC
                    });
                }

                await _addressTypesRepository.AddRangeAsync(newAddressTypes);
                // Do NOT commit or start transaction here
            }
        }
        #endregion

        #region CreateLead
        public async Task<CommonReturnViewModel> CreateLead(int orgId, LeadsVM leadsVM)
        {
            // Begin transaction
            await _leadsRepository.BeginTransactionAsync();

            try
            {
                if (orgId > 0)
                {
                    // Get the customer address
                    var individualAddressObj = await _customerAddressesRepository.AllActive().Include(x => x.Customer).Where(u => u.CustomerID == leadsVM.CustomerId).FirstOrDefaultAsync();
                    if (individualAddressObj != null)
                    {
                        var leadObj = new Leads()
                        {
                            CustomerID = individualAddressObj.CustomerID,
                            OrganizationID = orgId,
                            LeadName = leadsVM.LeadName,
                            CompanyBranchID = individualAddressObj.Customer != null && individualAddressObj.Customer.IsPerson == false ? leadsVM.BranchId : null,
                            LeadStatusID = leadsVM.LeadStatusID,
                            LeadSourceID = leadsVM.LeadSourceID,
                            LeadOwnerID = leadsVM.LeadOwnerID,
                            PriorityID = leadsVM.PriorityID,
                            ApproximateDealValue = leadsVM.ApproximateDealValue,
                            ProbabilityPercentage = leadsVM.ProbabilityPercentage,
                            LeadDescription = leadsVM.LeadDescription,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = leadsVM.CreatedBy,
                            LIP = leadsVM.LIP,
                            LMAC = leadsVM.LMAC,
                            UpdatedAt = DateTime.UtcNow,
                        };
                        await _leadsRepository.AddAsync(leadObj);

                        // Add lead services if provided
                        if (leadsVM.ServiceTypeIds != null && leadsVM.ServiceTypeIds.Count > 0)
                        {
                            var services = leadsVM.ServiceTypeIds.Select(serviceId => new LeadServices
                            {
                                LeadID = leadObj.LeadID,
                                ServiceID = serviceId,
                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = leadsVM.CreatedBy,
                                LIP = leadsVM.LIP,
                                LMAC = leadsVM.LMAC,
                            }).ToList();

                            await _leadServicesRepository.AddRangeAsync(services);
                        }

                        // Commit transaction
                        await _leadsRepository.CommitTransactionAsync();

                        return new CommonReturnViewModel
                        {
                            Success = true,
                            Data = new { Id = leadObj?.LeadID ?? 0 },
                            Message = "Data saved successfully",
                        };
                    }
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Customer not found",
                    };
                }
                // Create lead
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "We did not get your organization",
                };
            }
            catch (Exception ex)
            {
                // Rollback on error
                await _leadsRepository.RollbackTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
        #endregion


        #region EditLead
        public async Task<CommonReturnViewModel> EditLead(LeadUpdateVM leadUpdateVM)
        {
            // Begin transaction
            await _leadsRepository.BeginTransactionAsync();

            try
            {
                // Get the lead
                var leadObj = await _leadsRepository.FirstOrDefaultAsync(u => u.LeadID == leadUpdateVM.LeadID);
                if (leadObj == null)
                {
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "Lead not found."
                    };
                }

                // Update lead fields
                leadObj.LeadName = leadUpdateVM.LeadName;
                leadObj.LeadStatusID = leadUpdateVM.LeadStatusID ?? null;
                leadObj.LeadSourceID = leadUpdateVM.LeadSourceID ?? null;
                leadObj.LeadOwnerID = leadUpdateVM.LeadOwnerID ?? null;
                leadObj.PriorityID = leadUpdateVM.PriorityID ?? null;
                leadObj.ApproximateDealValue = leadUpdateVM.ApproximateDealValue;
                leadObj.ProbabilityPercentage = leadUpdateVM.ProbabilityPercentage;
                leadObj.LeadDescription = leadUpdateVM.LeadDescription;

                leadObj.UpdatedAt = DateTime.UtcNow;
                leadObj.UpdatedBy = leadUpdateVM.CreatedBy;
                leadObj.LIP = leadUpdateVM.LIP;
                leadObj.LMAC = leadUpdateVM.LMAC;

                await _leadsRepository.UpdateAsync(leadObj);

                // Delete existing lead services
                var leadServices = await _leadServicesRepository.FindAsync(u => u.LeadID == leadUpdateVM.LeadID);
                await _leadServicesRepository.DeleteRangeAsync(leadServices);

                // Add updated services
                if (leadUpdateVM.ServiceTypeIds != null && leadUpdateVM.ServiceTypeIds.Count > 0)
                {
                    var services = leadUpdateVM.ServiceTypeIds.Select(serviceId => new LeadServices
                    {
                        LeadID = leadObj.LeadID,
                        ServiceID = serviceId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = leadUpdateVM.CreatedBy,
                        LIP = leadUpdateVM.LIP,
                        LMAC = leadUpdateVM.LMAC,
                    }).ToList();

                    await _leadServicesRepository.AddRangeAsync(services);
                }

                // Commit transaction
                await _leadsRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data updated successfully",
                };
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _leadsRepository.RollbackTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
        #endregion


        #region get Owner List 
        public async Task<ReturnDataView<CustomerInfoVM>> GetLeadOwnerListAsync(string search, int page, int pageSize, int organizationID)
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
                .OrderBy(c => c.Employee.FirstName ?? "")
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

        #region get Source List 
        public async Task<ReturnDataView<CommonSelectVM>> GetLeadSourceListAsync(string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var query = _sourcesRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";

                    query = query.Where(c =>
                        c != null &&
                        (
                            EF.Functions.Like(c.LeadSourceName, pattern) ||
                            EF.Functions.Like(c.CreatedAt.ToString(), pattern)
                        ));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).Select(t => new CommonSelectVM
                    {
                        Id = t.LeadSourceID,
                        Name = t.LeadSourceName,
                    })
                    .ToListAsync();

                return new ReturnDataView<CommonSelectVM>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            }
            catch (Exception ex) { return new ReturnDataView<CommonSelectVM>(); }
            
        }
        #endregion

        #region get Status List 
        public async Task<ReturnDataView<CommonSelectVM>> GetLeadStatusListAsync(string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var query = _statusesRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";

                    query = query.Where(c =>
                        c != null &&
                        (
                            EF.Functions.Like(c.LeadStatusName, pattern) ||
                            EF.Functions.Like(c.CreatedAt.ToString(), pattern)
                        ));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).Select(t => new CommonSelectVM
                    {
                        Id = t.LeadStatusID,
                        Name = t.LeadStatusName,
                    })
                    .ToListAsync();

                return new ReturnDataView<CommonSelectVM>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            }
            catch (Exception ex) { return new ReturnDataView<CommonSelectVM>(); }
            
        }
        #endregion

        #region get Priority List 
        public async Task<ReturnDataView<CommonSelectVM>> GetPriorityListAsync(string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var query = _prioritiesRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";

                    query = query.Where(c =>
                        c != null &&
                        (
                            EF.Functions.Like(c.PriorityName, pattern) ||
                            EF.Functions.Like(c.CreatedAt.ToString(), pattern)
                        ));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).Select(t => new CommonSelectVM
                    {
                        Id = t.PriorityID,
                        Name = t.PriorityName,
                    })
                    .ToListAsync();

                return new ReturnDataView<CommonSelectVM>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            }
            catch (Exception ex) { return new ReturnDataView<CommonSelectVM>(); }
        }
        #endregion

        #region get Service List 
        public async Task<ReturnDataView<CommonSelectVM>> GetServiceListAsync(string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var query = _servicesRepository
                .AllActive()
                .Where(q => q.OrganizationID == organizationID);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";

                    query = query.Where(c =>
                        c != null &&
                        (
                            EF.Functions.Like(c.ServiceName, pattern) ||
                            EF.Functions.Like(c.CreatedAt.ToString(), pattern)
                        ));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).Select(t => new CommonSelectVM
                    {
                        Id = t.ServiceID,
                        Name = t.ServiceName,
                    })
                    .ToListAsync();

                return new ReturnDataView<CommonSelectVM>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            }
            catch (Exception ex) { return new ReturnDataView<CommonSelectVM>(); }
        }
        #endregion


        #region GetContactPersonNumberAsync
        public async Task<ReturnDataView<CommonSelectVM>> GetContactPersonNumberAsync(
    int leadId, string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var leadObj = await _leadsRepository.GetByIdAsync(leadId);
                var items = new List<CommonSelectVM>();

                // 1️⃣ Load Customer Address (always)
                var customerAddresses = await _customerAddressesRepository.AllActive()
                    .Where(a => a.CustomerID == leadObj.CustomerID)
                    .Select(a => new
                    {
                        Phone1 = a.Address.Phone,
                        Phone2 = a.Address.OtherPhone,
                        AddressTypeName = a.AddressType.AddressTypeName
                    })
                    .ToListAsync();

                // 2️⃣ Load OtherContacts
                var otherContactsQuery = _otherContactsRepository.AllActive()
                    .Where(x => x.AddressID != null &&
                                _customerAddressesRepository.AllActive()
                                    .Any(a => a.CustomerID == leadObj.CustomerID && a.AddressID == x.AddressID));

                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";
                    otherContactsQuery = otherContactsQuery.Where(c =>
                        EF.Functions.Like(c.FirstName, pattern) ||
                        EF.Functions.Like(c.LastName, pattern) ||
                        EF.Functions.Like(c.Phone1, pattern) ||
                        EF.Functions.Like(c.Phone2, pattern)
                    );

                    // Also filter customer address phones if search matches
                    customerAddresses = customerAddresses
                        .Where(a =>
                            (!string.IsNullOrEmpty(a.Phone1) && EF.Functions.Like(a.Phone1, pattern)) ||
                            (!string.IsNullOrEmpty(a.Phone2) && EF.Functions.Like(a.Phone2, pattern))
                        )
                        .ToList();
                }

                var totalCount = await otherContactsQuery.CountAsync();

                var otherContacts = await otherContactsQuery
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 3️⃣ Add OtherContact phones + address phones
                foreach (var oc in otherContacts)
                {
                    var phoneList = new List<(string Label, string Phone)>();

                    if (!string.IsNullOrEmpty(oc.Phone1)) phoneList.Add(("Phone1", oc.Phone1));
                    if (!string.IsNullOrEmpty(oc.Phone2)) phoneList.Add(("Phone2", oc.Phone2));

                    // Append all customer address phones
                    foreach (var addr in customerAddresses)
                    {
                        if (!string.IsNullOrEmpty(addr.Phone1)) phoneList.Add(("Address Phone", addr.Phone1));
                        if (!string.IsNullOrEmpty(addr.Phone2)) phoneList.Add(("Address Other Phone", addr.Phone2));
                    }

                    // Remove duplicates
                    phoneList = phoneList
                        .GroupBy(p => p.Phone)
                        .Select(g => g.First())
                        .ToList();

                    foreach (var p in phoneList)
                    {
                        items.Add(new CommonSelectVM
                        {
                            Id = oc.OtherContactID,
                            Name = $"{oc.FirstName} {oc.LastName} {p.Label}: {p.Phone}"
                        });
                    }
                }

                // 4️⃣ If no OtherContact, show only address phones
                if (!otherContacts.Any())
                {
                    foreach (var addr in customerAddresses)
                    {
                        if (!string.IsNullOrEmpty(addr.Phone1))
                            items.Add(new CommonSelectVM
                            {
                                Id = 0,
                                Name = $"Address Phone: {addr.Phone1}"
                            });
                        if (!string.IsNullOrEmpty(addr.Phone2))
                            items.Add(new CommonSelectVM
                            {
                                Id = 0,
                                Name = $"Address Other Phone: {addr.Phone2}"
                            });
                    }
                }

                return new ReturnDataView<CommonSelectVM>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            }
            catch (Exception ex)
            {
                return new ReturnDataView<CommonSelectVM>
                {
                    data = new List<CommonSelectVM>(),
                    totalItem = 0,
                    message = $"Error: {ex.Message}"
                };
            }
        }



        #endregion

        #region GetContactPersonEmailAsync
        public async Task<ReturnDataView<CommonSelectVM>> GetContactPersonEmailAsync(
            int leadId, string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var leadObj = await _leadsRepository.GetByIdAsync(leadId);

                var query = _otherContactsRepository.AllActive().Where(x=> 
                x.Address!= null && x.Address.CustomerAddresses.Any(x=>x.CustomerID == leadObj.CustomerID) || x.Address != null && x.Address.CompanyBranchAddresses.Any(x => x.BranchID == leadObj.CompanyBranchID));

                if(!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";

                    query = query.Where(c =>
                        c != null &&
                        (
                            EF.Functions.Like(c.FirstName, pattern) ||
                            EF.Functions.Like(c.LastName, pattern) ||
                            EF.Functions.Like(c.Email, pattern) ||
                            EF.Functions.Like(c.CreatedAt.ToString(), pattern)
                        ));
                }

                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Where(x=> x.Email != null && x.Email != "")
                    .Select(t => new CommonSelectVM
                    {
                        Id = t.OtherContactID,
                        Name =
                            t.FirstName + " " +
                            (t.LastName ?? "") + " " +
                            (t.Email ?? "") + " (" +
                            Capitalize(
                                t.Address.CustomerAddresses
                                    .Select(x => x.AddressType.AddressTypeName)
                                    .FirstOrDefault() ??
                                t.Address.CompanyBranchAddresses
                                    .Select(x => x.AddressType.AddressTypeName)
                                    .FirstOrDefault() ??
                                ""
                            ) + ")"
                    })
                    .ToListAsync();



                return new ReturnDataView<CommonSelectVM>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            }
            catch (Exception ex)
            {
                return new ReturnDataView<CommonSelectVM>
                {
                    data = new List<CommonSelectVM>(),
                    totalItem = 0,
                    message = $"Error: {ex.Message}"
                };
            }
        }
        #endregion

        #region Capitalize function
        public static string Capitalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            return char.ToUpper(value[0]) + value.Substring(1).ToLower();
        }
        #endregion
    }
}
