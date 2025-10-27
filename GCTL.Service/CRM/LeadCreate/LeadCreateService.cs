using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SkiaSharp;
using System.Security.Cryptography;


namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        #region Repositories
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;
        private readonly IGenericRepository<Priorities> _prioritiesRepository;
        private readonly IGenericRepository<LeadStatuses> _statusesRepository;
        private readonly IGenericRepository<LeadSources> _sourcesRepository;
        private readonly IGenericRepository<CompanyWarehouses> _companyWarehousesRepository;
        private readonly IGenericRepository<CompanyWarehouseAddresses> _companyWarehouseAddressesRepository;
        private readonly IGenericRepository<CompanyBranches> _companyBranchesRepository;
        private readonly IGenericRepository<CompanyBranchAddresses> _companyBranchAddressesRepository;
        private readonly IGenericRepository<LeadServices> _leadServicesRepository;
        private readonly IGenericRepository<EmployeeOfficeInfo> _employeeOfficeInfoRepository;
        private readonly AppDbContext _context;

        #region Added by Md. Rakib Hasan
        private readonly IGenericRepository<Heads> _heads;
        private readonly IGenericRepository<HeadDetails> _headDetails;
        private readonly IGenericRepository<TransactionAccounts> _transactionAccounts;
        private readonly IGenericRepository<SubAccounts> _subAccounts;
        private readonly ITransactionAccountService _transactionAccountService;
        #endregion

        public LeadCreateService(AppDbContext context, IGenericRepository<LeadServices> leadServicesRepository, IGenericRepository<CompanyBranchAddresses> companyBranchAddressesRepository, IGenericRepository<CompanyBranches> companyBranchesRepository, IGenericRepository<CompanyWarehouseAddresses> companyWarehouseAddressesRepository, IGenericRepository<CompanyWarehouses> companyWarehousesRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<Leads> leadsRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<Heads> heads, IGenericRepository<HeadDetails> headDetails, IGenericRepository<TransactionAccounts> transactionAccounts, IGenericRepository<SubAccounts> subAccounts, ITransactionAccountService transactionAccountService, IGenericRepository<EmployeeOfficeInfo> employeeOfficeInfoRepository, IGenericRepository<Priorities> prioritiesRepository, IGenericRepository<LeadStatuses> statusesRepository, IGenericRepository<LeadSources> sourcesRepository)
        {
            _countryRepository = countryRepository;
            _addressesRepository = addressesRepository;
            _addressTypesRepository = addressTypesRepository;
            _leadsRepository = leadsRepository;
            _customersRepository = customersRepository;
            _customerAddressesRepository = customerAddressesRepository;
            _companyWarehousesRepository = companyWarehousesRepository;
            _companyWarehouseAddressesRepository = companyWarehouseAddressesRepository;
            _companyBranchesRepository = companyBranchesRepository;
            _companyBranchAddressesRepository = companyBranchAddressesRepository;
            _leadServicesRepository = leadServicesRepository;
            _context = context;
            _heads = heads;
            _headDetails = headDetails;
            _transactionAccounts = transactionAccounts;
            _subAccounts = subAccounts;
            _transactionAccountService = transactionAccountService;
            _employeeOfficeInfoRepository = employeeOfficeInfoRepository;
            _prioritiesRepository = prioritiesRepository;
            _statusesRepository = statusesRepository;
            _sourcesRepository = sourcesRepository;
        }
        #endregion


        //#region CreatePerson
        //public async Task<ReturnView> CreatePerson(CustomerVM customerVM)
        //{
        //    // Begin transaction
        //    await _customersRepository.BeginTransactionAsync();

        //    try
        //    {
        //        Customers customerObj = new Customers();
        //        var items = await _addressTypesRepository.GetAllAsync();
        //        int returnID = 0;
        //        string returnName = "";

        //        // Create default address types if none exist
        //        if (!items.Any())
        //        {
        //            var addressTypes = new List<AddressTypes>();
        //            var listItem = new string[] { "individual", "shipping", "company", "branch", "warehouse" };

        //            foreach (var typeName in listItem)
        //            {
        //                addressTypes.Add(new AddressTypes()
        //                {
        //                    AddressTypeName = typeName,
        //                    CreatedAt = DateTime.UtcNow,
        //                    CreatedBy = customerVM.CreatedBy,
        //                    LIP = customerVM.LIP,
        //                    LMAC = customerVM.LMAC,
        //                });
        //            }

        //            await _addressTypesRepository.AddRangeAsync(addressTypes);
        //        }

        //        // Fetch country
        //        //var countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == customerVM.CountryId);

        //        // Fetch individual address type
        //        var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "individual");


        //        #region Added by Md. Rakib Hasan
        //        string schemaName = "Customer";
        //        string tableName = "Customers";
        //        int subAccId = 14;

        //        var headDetail = await _headDetails.FirstOrDefaultAsync(hd => hd.SchemaName == schemaName && hd.TableName == tableName);

        //        if (headDetail == null)
        //        {
        //            headDetail = new HeadDetails();
        //            headDetail.SchemaName = schemaName;
        //            headDetail.TableName = tableName;

        //            headDetail.LIP = customerVM.LIP;
        //            headDetail.LMAC = customerVM.LMAC;
        //            headDetail.CreatedAt = DateTime.UtcNow;
        //            headDetail.CreatedBy = customerVM.CreatedBy;

        //            await _headDetails.AddAsync(headDetail);
        //        }

        //        var head = await _heads.FirstOrDefaultAsync(h => h.HeadDetailID == headDetail.HeadDetailID);

        //        if (head == null)
        //        {
        //            head = new Heads();
        //            head.HeadDetailID = headDetail.HeadDetailID;

        //            head.LIP = customerVM.LIP;
        //            head.LMAC = customerVM.LMAC;
        //            head.CreatedAt = DateTime.UtcNow;
        //            head.CreatedBy = customerVM.CreatedBy;

        //            await _heads.AddAsync(head);
        //        }

        //        customerObj.HeadID = head.HeadID;

        //        var subAccDetails = await _subAccounts.AllActive().FirstOrDefaultAsync(x => x.SubAccountID == subAccId);

        //        if (subAccDetails == null)
        //        {
        //            return new ReturnView
        //            {
        //                Success = false,
        //                Message = "Please Add Sub Account first!",
        //            };
        //        }

        //        var generatedTrxAccCode = await _transactionAccountService.GenerateNextCodeAsync((int)subAccDetails.SubAccountID);

        //        TransactionAccounts trxAccount = new TransactionAccounts();
        //        trxAccount.SubAccountID = subAccDetails.SubAccountID;

        //        trxAccount.TrxAccCode = generatedTrxAccCode;

        //        trxAccount.TrxAccName = subAccDetails.SubAccountName;
        //        trxAccount.IsActive = true;
        //        trxAccount.Description = "Customer transaction account";
        //        trxAccount.Head = head;

        //        trxAccount.LIP = customerVM.LIP;
        //        trxAccount.LMAC = customerVM.LMAC;
        //        trxAccount.CreatedAt = DateTime.UtcNow;
        //        trxAccount.CreatedBy = customerVM.CreatedBy;

        //        await _transactionAccounts.AddAsync(trxAccount);
        //        #endregion


        //        // Create customer (individual)
        //        customerObj = new Customers()
        //        {
        //            FullName = customerVM.FirstName + " " + customerVM.LastName,
        //            IsPerson = true,
        //            HeadID = head.HeadID, // Added by Md. Rakib Hasan
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = customerVM.CreatedBy,
        //            LIP = customerVM.LIP,
        //            LMAC = customerVM.LMAC,
        //        };
        //        await _customersRepository.AddAsync(customerObj);
        //        returnName = customerObj.FullName;

        //        // Create address
        //        var addresses = new Addresses()
        //        {
        //            FullAddress = customerVM.FullAddress,
        //            Street = customerVM.Street,
        //            City = customerVM.City,
        //            State = customerVM.State,
        //            Additionaladdress = customerVM.Additionaladdress,
        //            PostalCode = customerVM.PostalCode,
        //            CountryID = countryObj?.CountryID,
        //            Phone = customerVM.Phone,
        //            OtherPhone = customerVM.OtherPhone,
        //            Email = customerVM.Email,
        //            Latitude = customerVM.Latitude,
        //            Longitude = customerVM.Longitude,
        //            FirstName = customerVM.FirstName,
        //            LastName = customerVM.LastName,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = customerVM.CreatedBy,
        //            LIP = customerVM.LIP,
        //            LMAC = customerVM.LMAC,
        //            UpdatedAt = DateTime.UtcNow,
        //            UpdatedBy = customerVM.UpdatedBy ?? null,
        //            DeletedAt = null,
        //        };
        //        await _addressesRepository.AddAsync(addresses);

        //        // Link customer with address
        //        var customerAddress = new CustomerAddresses()
        //        {
        //            AddressTypeID = addressTypeObj.AddressTypeID,
        //            AddressID = addresses.AddressID,
        //            CustomerID = customerObj.CustomerID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = customerVM.CreatedBy,
        //            LIP = customerVM.LIP,
        //            LMAC = customerVM.LMAC,
        //        };
        //        await _customerAddressesRepository.AddAsync(customerAddress);
        //        returnID = customerAddress.CustomerAddressID;

        //        // Commit transaction
        //        await _customersRepository.CommitTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = true,
        //            Message = "Data saved successfully",
        //            Id = returnID,
        //            Name = returnName
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback transaction on error
        //        await _customersRepository.RollbackTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //        };
        //    }
        //}
        //#endregion


        //#region CreateCompany
        //public async Task<ReturnView> CreateCompany(CompanyVM companyVM)
        //{
        //    // Begin transaction
        //    await _customersRepository.BeginTransactionAsync();

        //    try
        //    {
        //        int returnID = 0;
        //        string returnName = "";

        //        // Ensure default address types exist
        //        var items = await _addressTypesRepository.GetAllAsync();
        //        if (!items.Any())
        //        {
        //            var addressTypes = new List<AddressTypes>();
        //            var listItem = new string[] { "individual", "shipping", "company", "branch", "warehouse" };

        //            foreach (var typeName in listItem)
        //            {
        //                addressTypes.Add(new AddressTypes()
        //                {
        //                    AddressTypeName = typeName,
        //                    CreatedAt = DateTime.UtcNow,
        //                    CreatedBy = companyVM.CreatedBy,
        //                    LIP = companyVM.LIP,
        //                    LMAC = companyVM.LMAC,
        //                });
        //            }

        //            await _addressTypesRepository.AddRangeAsync(addressTypes);
        //        }

        //        // Fetch company address type
        //        var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "company");

        //        // Fetch country
        //        var countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == companyVM.CountryID);

        //        // Create address
        //        var addresses = new Addresses()
        //        {
        //            FullAddress = companyVM.FullAddress,
        //            Street = companyVM.Street,
        //            City = companyVM.City,
        //            State = companyVM.State,
        //            Additionaladdress = companyVM.Additionaladdress,
        //            PostalCode = companyVM.PostalCode,
        //            CountryID = countryObj?.CountryID,
        //            Phone = companyVM.Phone,
        //            OtherPhone = companyVM.OtherPhone,
        //            Email = companyVM.Email,
        //            Latitude = companyVM.Latitude,
        //            Longitude = companyVM.Longitude,
        //            FirstName = companyVM.FirstName,
        //            LastName = companyVM.LastName,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = companyVM.CreatedBy,
        //            LIP = companyVM.LIP,
        //            LMAC = companyVM.LMAC,
        //        };
        //        await _addressesRepository.AddAsync(addresses);

        //        // Create company customer
        //        var customer = new Customers()
        //        {
        //            FullName = companyVM.CompanyName,
        //            IsPerson = false,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = companyVM.CreatedBy,
        //            LIP = companyVM.LIP,
        //            LMAC = companyVM.LMAC,
        //        };
        //        await _customersRepository.AddAsync(customer);

        //        // Link customer with address
        //        var companyAddressesObj = new CustomerAddresses()
        //        {
        //            AddressTypeID = addressTypeObj.AddressTypeID,
        //            AddressID = addresses.AddressID,
        //            CustomerID = customer.CustomerID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = companyVM.CreatedBy,
        //            LIP = companyVM.LIP,
        //            LMAC = companyVM.LMAC,
        //        };
        //        await _customerAddressesRepository.AddAsync(companyAddressesObj);

        //        returnID = companyAddressesObj.CustomerAddressID;
        //        returnName = customer.FullName;

        //        // Commit transaction
        //        await _customersRepository.CommitTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = true,
        //            Message = "Data saved successfully",
        //            Id = returnID,
        //            Name = returnName
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback transaction on error
        //        await _customersRepository.RollbackTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //        };
        //    }
        //}
        //#endregion


        //#region SaveAddressType
        //public async Task<int> SaveAddressType(int? createdBy, string? LIP, string? LMAC)
        //{
        //    // Begin transaction
        //    await _addressTypesRepository.BeginTransactionAsync();

        //    try
        //    {
        //        var items = await _addressTypesRepository.GetAllAsync();
        //        int createdCount = 0;

        //        if (!items.Any())
        //        {
        //            var addressTypes = new List<AddressTypes>();
        //            var listItem = new string[] { "individual", "shipping", "company", "branch", "warehouse" };

        //            foreach (var typeName in listItem)
        //            {
        //                addressTypes.Add(new AddressTypes()
        //                {
        //                    AddressTypeName = typeName,
        //                    CreatedAt = DateTime.UtcNow,
        //                    CreatedBy = createdBy,
        //                    LIP = LIP,
        //                    LMAC = LMAC,
        //                });
        //            }

        //            await _addressTypesRepository.AddRangeAsync(addressTypes);
        //            createdCount = addressTypes.Count;
        //        }

        //        // Commit transaction
        //        await _addressTypesRepository.CommitTransactionAsync();

        //        return createdCount;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback on error
        //        await _addressTypesRepository.RollbackTransactionAsync();
        //        // Optional: log ex
        //        return 0;
        //    }
        //}
        //#endregion


        //#region SaveCountry
        //private async Task<int> saveCountry(string? countryCode, string? coutryName, int? CreatedBy, string? LIP, string? LMAC)
        //{
        //    int countryId = 0;
        //    if (coutryName != null)
        //    {
        //        var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == coutryName.ToLower());


        //        if (existingCountry != null)
        //        {
        //            // Country found
        //            countryId = existingCountry.CountryID;
        //        }
        //        else
        //        {
        //            // 2. new country creation
        //            var newCountry = new Country
        //            {
        //                CountryCode = countryCode,
        //                CountryName = coutryName,

        //                CreatedAt = DateTime.UtcNow,
        //                CreatedBy = CreatedBy,
        //                LIP = LIP,
        //                LMAC = LMAC,

        //            };

        //            await _countryRepository.AddAsync(newCountry);
        //            countryId = newCountry.CountryID;
        //        }
        //    }
        //    return countryId;
        //}
        //#endregion


        //#region CreateShippingAddress
        //public async Task<CommonReturnViewModel> CreateShippingAddress(ShippingVM shippingVM)
        //{
        //    // Begin transaction only in this method
        //    await _addressesRepository.BeginTransactionAsync();

        //    try
        //    {
        //        // Ensure address types exist (do NOT start a transaction inside this helper)
        //        await EnsureAddressTypesExist(shippingVM.CreatedBy, shippingVM.LIP, shippingVM.LMAC);

        //        // Get country
        //        var countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == shippingVM.CountryId);

        //        // Get shipping address type
        //        var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");

        //        // Get customer
        //        var customerAddressObj = await _customerAddressesRepository.FirstOrDefaultAsync(u => u.CustomerAddressID == shippingVM.PersonID);
        //        if (customerAddressObj == null)
        //        {
        //            return new CommonReturnViewModel
        //            {
        //                Success = false,
        //                Message = "Customer not found"
        //            };
        //        }

        //        // Create address
        //        var addresses = new Addresses()
        //        {
        //            FullAddress = shippingVM.FullAddress,
        //            Street = shippingVM.Street,
        //            City = shippingVM.City,
        //            State = shippingVM.State,
        //            Additionaladdress = shippingVM.Additionaladdress,
        //            PostalCode = shippingVM.PostalCode,
        //            CountryID = countryObj?.CountryID,
        //            Phone = shippingVM.Phone,
        //            OtherPhone = shippingVM.OtherPhone,
        //            Email = shippingVM.Email,
        //            Latitude = shippingVM.Latitude,
        //            Longitude = shippingVM.Longitude,
        //            FirstName = shippingVM.FirstName,
        //            LastName = shippingVM.LastName,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = shippingVM.CreatedBy,
        //            LIP = shippingVM.LIP,
        //            LMAC = shippingVM.LMAC,
        //            UpdatedAt = DateTime.UtcNow,
        //            UpdatedBy = shippingVM.UpdatedBy ?? null,
        //            DeletedAt = null,
        //        };
        //        await _addressesRepository.AddAsync(addresses);

        //        // Link customer with address
        //        var individualAddresses = new CustomerAddresses()
        //        {
        //            AddressTypeID = addressTypeObj.AddressTypeID,
        //            AddressID = addresses.AddressID,
        //            CustomerID = customerAddressObj.CustomerID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = shippingVM.CreatedBy,
        //            LIP = shippingVM.LIP,
        //            LMAC = shippingVM.LMAC,
        //            UpdatedAt = DateTime.UtcNow,
        //            UpdatedBy = shippingVM.UpdatedBy ?? null,
        //            DeletedAt = null,
        //        };
        //        await _customerAddressesRepository.AddAsync(individualAddresses);

        //        // Commit transaction
        //        await _addressesRepository.CommitTransactionAsync();

        //        return new CommonReturnViewModel
        //        {
        //            Success = true,
        //            Message = "Data saved successfully",
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback transaction
        //        await _addressesRepository.RollbackTransactionAsync();

        //        return new CommonReturnViewModel
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //        };
        //    }
        //}
        //#endregion


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
        public async Task<CommonReturnViewModel> CreateLead(LeadsVM leadsVM)
        {
            // Begin transaction
            await _leadsRepository.BeginTransactionAsync();

            try
            {
                // Get the customer address
                var individualAddressObj = await _customerAddressesRepository.FirstOrDefaultAsync(
                    u => u.CustomerID == leadsVM.CustomerId);

                // Create lead
                var leadObj = new Leads()
                {
                    CustomerID = individualAddressObj.CustomerID,
                    LeadName = leadsVM.LeadName,
                    IsIndividualCustomer = leadsVM.IsIndividualCustomer,
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
                    Message = "Data saved successfully",
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

        //#region CreateBranch
        //public async Task<ReturnView> CreateBranch(BranchVM branchVM)
        //{
        //    // Begin transaction
        //    await _companyBranchesRepository.BeginTransactionAsync();

        //    try
        //    {
        //        // Ensure address types exist WITHOUT starting a new transaction
        //        var addressTypes = await _addressTypesRepository.GetAllAsync();
        //        if (!addressTypes.Any())
        //        {
        //            var listItems = new string[] { "individual", "shipping", "company", "branch", "warehouse" };
        //            List<AddressTypes> newAddressTypes = new List<AddressTypes>();

        //            foreach (var item in listItems)
        //            {
        //                newAddressTypes.Add(new AddressTypes
        //                {
        //                    AddressTypeName = item,
        //                    CreatedAt = DateTime.UtcNow,
        //                    CreatedBy = branchVM.CreatedBy,
        //                    LIP = branchVM.LIP,
        //                    LMAC = branchVM.LMAC
        //                });
        //            }

        //            await _addressTypesRepository.AddRangeAsync(newAddressTypes);
        //            // Note: No CommitTransaction here, we are inside CreateBranch transaction
        //        }

        //        // Get country
        //        var countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == branchVM.CountryID);

        //        // Get address type
        //        var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "branch");

        //        // Get company
        //        var companyAddressObj = await _customerAddressesRepository
        //            .AllActive()
        //            .Include(c => c.Customer)
        //            .FirstOrDefaultAsync(u => u.CustomerAddressID == branchVM.CompanyID);

        //        if (companyAddressObj == null)
        //        {
        //            return new ReturnView
        //            {
        //                Success = false,
        //                Message = "Company not found"
        //            };
        //        }

        //        // Save branch
        //        var companyBranches = new CompanyBranches
        //        {
        //            BranchName = branchVM.BranchName,
        //            CustomerID = companyAddressObj.CustomerID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = branchVM.CreatedBy,
        //            LIP = branchVM.LIP,
        //            LMAC = branchVM.LMAC,
        //        };
        //        await _companyBranchesRepository.AddAsync(companyBranches);

        //        // Save address
        //        var addresses = new Addresses
        //        {
        //            FullAddress = branchVM.FullAddress,
        //            Street = branchVM.Street,
        //            City = branchVM.City,
        //            State = branchVM.State,
        //            Additionaladdress = branchVM.Additionaladdress,
        //            PostalCode = branchVM.PostalCode,
        //            CountryID = countryObj?.CountryID,
        //            Phone = branchVM.Phone,
        //            OtherPhone = branchVM.OtherPhone,
        //            Email = branchVM.Email,
        //            Latitude = branchVM.Latitude,
        //            Longitude = branchVM.Longitude,
        //            FirstName = branchVM.FirstName,
        //            LastName = branchVM.LastName,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = branchVM.CreatedBy,
        //            LIP = branchVM.LIP,
        //            LMAC = branchVM.LMAC,
        //            UpdatedAt = DateTime.UtcNow,
        //            UpdatedBy = branchVM.UpdatedBy ?? null,
        //            DeletedAt = null,
        //        };
        //        await _addressesRepository.AddAsync(addresses);

        //        // Save branch address
        //        var companyBranchAddress = new CompanyBranchAddresses
        //        {
        //            AddressTypeID = addressTypeObj.AddressTypeID,
        //            AddressID = addresses.AddressID,
        //            BranchID = companyBranches.BranchID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = branchVM.CreatedBy,
        //            LIP = branchVM.LIP,
        //            LMAC = branchVM.LMAC,
        //            UpdatedAt = DateTime.UtcNow,
        //            UpdatedBy = branchVM.UpdatedBy ?? null,
        //            DeletedAt = null,
        //        };
        //        await _companyBranchAddressesRepository.AddAsync(companyBranchAddress);

        //        // Commit transaction
        //        await _companyBranchesRepository.CommitTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = true,
        //            Message = "Data saved successfully",
        //            Id = companyAddressObj.CustomerAddressID,
        //            Name = companyAddressObj.Customer.FullName
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback transaction on error
        //        await _companyBranchesRepository.RollbackTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = false,
        //            Message = ex.Message
        //        };
        //    }
        //}
        //#endregion


        //#region CreateWarehouse
        //public async Task<ReturnView> CreateWarehouse(WarehouseVM warehouseVM)
        //{
        //    // Begin transaction
        //    await _companyWarehousesRepository.BeginTransactionAsync();

        //    try
        //    {
        //        // Ensure address types exist without starting a new transaction
        //        var addressTypes = await _addressTypesRepository.GetAllAsync();
        //        if (!addressTypes.Any())
        //        {
        //            var listItems = new string[] { "individual", "shipping", "company", "branch", "warehouse" };
        //            List<AddressTypes> newAddressTypes = new List<AddressTypes>();

        //            foreach (var item in listItems)
        //            {
        //                newAddressTypes.Add(new AddressTypes
        //                {
        //                    AddressTypeName = item,
        //                    CreatedAt = DateTime.UtcNow,
        //                    CreatedBy = warehouseVM.CreatedBy,
        //                    LIP = warehouseVM.LIP,
        //                    LMAC = warehouseVM.LMAC
        //                });
        //            }

        //            await _addressTypesRepository.AddRangeAsync(newAddressTypes);
        //            // Note: Do not commit here, transaction is managed by CreateWarehouse
        //        }

        //        // Get country and address type
        //        var countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == warehouseVM.CountryID);
        //        var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "warehouse");

        //        // Get company
        //        var companyAddressObj = await _customerAddressesRepository
        //            .AllActive()
        //            .Include(c => c.Customer)
        //            .FirstOrDefaultAsync(u => u.CustomerAddressID == warehouseVM.CompanyID);

        //        if (companyAddressObj == null)
        //        {
        //            return new ReturnView
        //            {
        //                Success = false,
        //                Message = "Company not found"
        //            };
        //        }

        //        // Save warehouse
        //        var companyWarehouses = new CompanyWarehouses
        //        {
        //            WarehouseName = warehouseVM.WareHouseName,
        //            CustomerID = companyAddressObj.CustomerID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = warehouseVM.CreatedBy,
        //            LIP = warehouseVM.LIP,
        //            LMAC = warehouseVM.LMAC,
        //        };
        //        await _companyWarehousesRepository.AddAsync(companyWarehouses);

        //        // Save address
        //        var addresses = new Addresses
        //        {
        //            FullAddress = warehouseVM.FullAddress,
        //            Street = warehouseVM.Street,
        //            City = warehouseVM.City,
        //            State = warehouseVM.State,
        //            Additionaladdress = warehouseVM.Additionaladdress,
        //            PostalCode = warehouseVM.PostalCode,
        //            CountryID = countryObj?.CountryID,
        //            Phone = warehouseVM.Phone,
        //            OtherPhone = warehouseVM.OtherPhone,
        //            Email = warehouseVM.Email,
        //            Latitude = warehouseVM.Latitude,
        //            Longitude = warehouseVM.Longitude,
        //            FirstName = warehouseVM.FirstName,
        //            LastName = warehouseVM.LastName,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = warehouseVM.CreatedBy,
        //            LIP = warehouseVM.LIP,
        //            LMAC = warehouseVM.LMAC,
        //        };
        //        await _addressesRepository.AddAsync(addresses);

        //        // Save warehouse address
        //        var companyWarehouseAddress = new CompanyWarehouseAddresses
        //        {
        //            AddressTypeID = addressTypeObj.AddressTypeID,
        //            AddressID = addresses.AddressID,
        //            WarehouseID = companyWarehouses.WarehouseID,
        //            CreatedAt = DateTime.UtcNow,
        //            CreatedBy = warehouseVM.CreatedBy,
        //            LIP = warehouseVM.LIP,
        //            LMAC = warehouseVM.LMAC,
        //        };
        //        await _companyWarehouseAddressesRepository.AddAsync(companyWarehouseAddress);

        //        // Commit transaction
        //        await _companyWarehousesRepository.CommitTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = true,
        //            Message = "Data saved successfully",
        //            Id = companyAddressObj.CustomerAddressID,
        //            Name = companyAddressObj.Customer.FullName
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // Rollback transaction
        //        await _companyWarehousesRepository.RollbackTransactionAsync();

        //        return new ReturnView
        //        {
        //            Success = false,
        //            Message = ex.Message
        //        };
        //    }
        //}
        //#endregion


        #region getcustomerInfo
        public async Task<object?> getcustomerInfo(int? id = 0)
        {
            var customerObj = await(from add in _context.CustomerAddresses
                                    join ind in _context.Customers
                                    on add.CustomerID equals ind.CustomerID
                                    join address in _context.Addresses on add.AddressID equals address.AddressID
                                    join country in _context.Country on address.CountryID equals country.CountryID into countryGroup
                                    from country in countryGroup.DefaultIfEmpty()
                                    where add.CustomerAddressID == id
                                    select new
                                    {
                                        FullName = ind.FullName,
                                        CustomerAddressID = add.CustomerAddressID,
                                        AddressTypeName = add.AddressType.AddressTypeName,
                                        FullAddress = address.FullAddress,
                                        Street = address.Street,
                                        City = address.City,
                                        Additionaladdress = address.Additionaladdress,
                                        State = address.State,
                                        PostalCode = address.PostalCode,
                                        CountryID = country != null ? country.CountryID : 0,
                                        CountryCode = country != null ? country.CountryCode : null,
                                        Latitude = address.Latitude,
                                        Longitude = address.Longitude,
                                        Phone = address.Phone,
                                        OtherPhone = address.OtherPhone,
                                        Email = address.Email,
                                        FirstName = address.FirstName,
                                        LastName = address.LastName
                                    }).FirstOrDefaultAsync();

            return customerObj;
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
    }
}
