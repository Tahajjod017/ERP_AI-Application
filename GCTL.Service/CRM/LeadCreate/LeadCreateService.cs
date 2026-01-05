using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using Microsoft.EntityFrameworkCore;


namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        #region properties 
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
        #endregion

        #region Added by Md. Rakib Hasan
        private readonly IGenericRepository<Heads> _heads;
        private readonly IGenericRepository<HeadDetails> _headDetails;
        private readonly IGenericRepository<TransactionAccounts> _transactionAccounts;
        private readonly IGenericRepository<SubAccounts> _subAccounts;
        private readonly ITransactionAccountService _transactionAccountService;
        #endregion

        #region Constructor
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
        public async Task<CommonReturnViewModel> EditLead(LeadsVM leadUpdateVM)
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
                leadObj.CustomerID = leadUpdateVM.CustomerId;
                leadObj.LeadName = leadUpdateVM.LeadName;
                leadObj.LeadStatusID = leadUpdateVM.LeadStatusID;
                leadObj.LeadSourceID = leadUpdateVM.LeadSourceID;
                leadObj.LeadOwnerID = leadUpdateVM.LeadOwnerID;
                leadObj.PriorityID = leadUpdateVM.PriorityID;
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
                        EF.Functions.Like(c.Employee.MobileNumber, pattern) ||
                        EF.Functions.Like(c.Employee.EmployeeCode, pattern) 
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

                if (leadObj == null)
                    return new ReturnDataView<CommonSelectVM>
                    {
                        data = items,
                        totalItem = 0,
                        message = "Lead not found"
                    };

                string pattern = !string.IsNullOrWhiteSpace(search) ? $"%{search}%" : null;

                // 1️⃣ Load OtherContacts
                var otherContactsQuery = _otherContactsRepository.AllActive()
                    .Where(x => x.AddressID != null &&
                                _customerAddressesRepository.AllActive()
                                    .Any(a => a.CustomerID == leadObj.CustomerID && a.AddressID == x.AddressID));

                if (!string.IsNullOrEmpty(pattern))
                {
                    otherContactsQuery = otherContactsQuery.Where(c =>
                        EF.Functions.Like(c.FirstName ?? "", pattern) ||
                        EF.Functions.Like(c.LastName ?? "", pattern) ||
                        EF.Functions.Like(c.Phone1 ?? "", pattern) ||
                        EF.Functions.Like(c.Phone2 ?? "", pattern)
                    );
                }

                var totalOtherContacts = await otherContactsQuery.CountAsync();

                var otherContacts = await otherContactsQuery
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // 2️⃣ Add OtherContact phones (ONLY their own phones)
                foreach (var oc in otherContacts)
                {
                    if (!string.IsNullOrEmpty(oc.Phone1))
                    {
                        items.Add(new CommonSelectVM
                        {
                            Id = oc.OtherContactID,
                            Name = $"{oc.FirstName ?? ""} {oc.LastName ?? ""} Phone1: {oc.Phone1}"
                        });
                    }
                    if (!string.IsNullOrEmpty(oc.Phone2))
                    {
                        items.Add(new CommonSelectVM
                        {
                            Id = oc.OtherContactID,
                            Name = $"{oc.FirstName ?? ""} {oc.LastName ?? ""} Phone2: {oc.Phone2}"
                        });
                    }
                }

                // 3️⃣ Load Customer Address phones (always, independent of OtherContacts)
                var customerAddressesQuery = _customerAddressesRepository.AllActive()
                    .Where(a => a.CustomerID == leadObj.CustomerID && a.Address != null);

                if (!string.IsNullOrEmpty(pattern))
                {
                    customerAddressesQuery = customerAddressesQuery.Where(a =>
                        EF.Functions.Like(a.Address.Phone ?? "", pattern) ||
                        EF.Functions.Like(a.Address.OtherPhone ?? "", pattern)
                    );
                }

                var customerAddresses = await customerAddressesQuery
                    .Select(a => new
                    {
                        Phone1 = a.Address.Phone,
                        Phone2 = a.Address.OtherPhone,
                        AddressTypeName = a.AddressType != null ? a.AddressType.AddressTypeName : null
                    })
                    .ToListAsync();

                var customerName = leadObj.Customer != null ? leadObj.Customer.FullName : "Customer";

                foreach (var addr in customerAddresses)
                {
                    if (!string.IsNullOrEmpty(addr.Phone1))
                    {
                        items.Add(new CommonSelectVM
                        {
                            Id = 0,
                            Name = $"{customerName} - Address Phone: {addr.Phone1}" +
                                   (string.IsNullOrEmpty(addr.AddressTypeName) ? "" : $" ({addr.AddressTypeName})")
                        });
                    }
                    if (!string.IsNullOrEmpty(addr.Phone2))
                    {
                        items.Add(new CommonSelectVM
                        {
                            Id = 0,
                            Name = $"{customerName} - Address Other Phone: {addr.Phone2}" +
                                   (string.IsNullOrEmpty(addr.AddressTypeName) ? "" : $" ({addr.AddressTypeName})")
                        });
                    }
                }

                var totalCount = totalOtherContacts + customerAddresses.Count;

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
        public async Task<ReturnDataView<CommonSelectVM>> GetContactPersonEmailAsync(  int leadId, string search, int page, int pageSize, int organizationID)
        {
            try
            {
                var leadObj = await _leadsRepository.GetByIdAsync(leadId);
                var items = new List<CommonSelectVM>();

                if (leadObj == null)
                    return new ReturnDataView<CommonSelectVM>
                    {
                        data = items,
                        totalItem = 0,
                        message = "Lead not found"
                    };

                string pattern = !string.IsNullOrWhiteSpace(search) ? $"%{search}%" : null;

                // 1️⃣ OtherContacts emails
                var otherContactsQuery = _otherContactsRepository.AllActive()
                    .Where(x => x.AddressID != null &&
                                _customerAddressesRepository.AllActive()
                                    .Any(a => a.CustomerID == leadObj.CustomerID && a.AddressID == x.AddressID));

                if (!string.IsNullOrEmpty(pattern))
                {
                    otherContactsQuery = otherContactsQuery.Where(c =>
                        EF.Functions.Like(c.FirstName ?? "", pattern) ||
                        EF.Functions.Like(c.LastName ?? "", pattern) ||
                        EF.Functions.Like(c.Email ?? "", pattern)
                    );
                }

                var totalOtherContacts = await otherContactsQuery.CountAsync();

                var otherContacts = await otherContactsQuery
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                foreach (var oc in otherContacts)
                {
                    if (!string.IsNullOrEmpty(oc.Email))
                    {
                        items.Add(new CommonSelectVM
                        {
                            Id = oc.OtherContactID,
                            Name = $"{oc.FirstName ?? ""} {oc.LastName ?? ""}: {oc.Email}"
                        });
                    }
                }

                // 2️⃣ Customer Address emails
                var customerAddressesQuery = _customerAddressesRepository.AllActive()
                    .Where(a => a.CustomerID == leadObj.CustomerID &&
                                a.Address != null &&
                                !string.IsNullOrEmpty(a.Address.Email));

                if (!string.IsNullOrEmpty(pattern))
                {
                    customerAddressesQuery = customerAddressesQuery.Where(a =>
                        EF.Functions.Like(a.Address.Email ?? "", pattern)
                    );
                }

                var customerAddresses = await customerAddressesQuery
                    .Select(a => new
                    {
                        Email = a.Address.Email,
                        AddressTypeName = a.AddressType != null ? a.AddressType.AddressTypeName : null
                    })
                    .ToListAsync();

                var customerName = leadObj.Customer != null ? leadObj.Customer.FullName : "Customer";

                foreach (var addr in customerAddresses)
                {
                    items.Add(new CommonSelectVM
                    {
                        Id = 0, // Not tied to OtherContact
                        Name = $"{customerName}: {addr.Email}" +
                               (string.IsNullOrEmpty(addr.AddressTypeName) ? "" : $" ({addr.AddressTypeName})")
                    });
                }

                // totalItem: include OtherContacts + Address emails count
                var totalCount = totalOtherContacts + customerAddresses.Count;

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
