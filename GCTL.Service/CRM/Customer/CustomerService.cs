using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using GCTL.Service.Pagination;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Index.HPRtree;

namespace GCTL.Service.CRM.Customer
{
    public class CustomerService : ICustomerService
    {
        #region resvices
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<CompanyWarehouses> _companyWarehousesRepository;
        private readonly IGenericRepository<CompanyWarehouseAddresses> _companyWarehouseAddressesRepository;
        private readonly IGenericRepository<CompanyBranches> _companyBranchesRepository;
        private readonly IGenericRepository<CompanyBranchAddresses> _companyBranchAddressesRepository;
        private readonly IGenericRepository<OrganizationTypes> _organizationTypesRepository;
        #region Added by Md. Rakib Hasan
        private readonly IGenericRepository<Heads> _heads;
        private readonly IGenericRepository<HeadDetails> _headDetails;
        private readonly IGenericRepository<TransactionAccounts> _transactionAccounts;
        private readonly IGenericRepository<SubAccounts> _subAccounts;
        private readonly ITransactionAccountService _transactionAccountService;
        #endregion
        public CustomerService(IGenericRepository<Country> countryRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, ITransactionAccountService transactionAccountService, IGenericRepository<Heads> heads, IGenericRepository<HeadDetails> headDetails, IGenericRepository<TransactionAccounts> transactionAccounts, IGenericRepository<SubAccounts> subAccounts, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<CompanyWarehouses> companyWarehousesRepository, IGenericRepository<CompanyWarehouseAddresses> companyWarehouseAddressesRepository, IGenericRepository<CompanyBranches> companyBranchesRepository, IGenericRepository<CompanyBranchAddresses> companyBranchAddressesRepository, IGenericRepository<OrganizationTypes> organizationTypesRepository)
        {
            _customersRepository = customersRepository;
            _addressesRepository = addressesRepository;
            _addressTypesRepository = addressTypesRepository;
            _transactionAccountService = transactionAccountService;
            _heads = heads;
            _headDetails = headDetails;
            _transactionAccounts = transactionAccounts;
            _subAccounts = subAccounts;
            _customerAddressesRepository = customerAddressesRepository;
            _companyWarehousesRepository = companyWarehousesRepository;
            _companyWarehouseAddressesRepository = companyWarehouseAddressesRepository;
            _companyBranchesRepository = companyBranchesRepository;
            _companyBranchAddressesRepository = companyBranchAddressesRepository;
            _organizationTypesRepository = organizationTypesRepository;
        }
        #endregion

        #region CreatePerson

        public async Task<bool> InserAddressTypeIntoDB(string? LIP, string? LMAC, int? CreatedBy)
        {
            try
            {
                var items = await _addressTypesRepository.GetAllAsync();
                if (!items.Any())
                {
                    var addressTypes = new List<AddressTypes>();
                    var listItem = new string[] { "individual", "shipping", "company", "branch", "warehouse" };

                    foreach (var typeName in listItem)
                    {
                        addressTypes.Add(new AddressTypes()
                        {
                            AddressTypeName = typeName,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = CreatedBy,
                            LIP = LIP,
                            LMAC = LMAC,
                        });
                    }

                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }
                return true;
            }
            catch (Exception) { return false; }
            
        }
        public async Task<ReturnView> CreateCustomer (CustomerVM model)
        {
            // Begin transaction
            await _customersRepository.BeginTransactionAsync();

            try
            {
                var nameToMatch = model.CompnayName ?? $"{model.FirstName}{model.LastName}";

                var queryObj = await _customersRepository.AllActive()
                    .Where(q => q.FullName == nameToMatch && q.OrganizationID == model.OrganizationID)
                    .FirstOrDefaultAsync();
                if (queryObj != null)
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Already exists in the database!",
                    };
                Customers customerObj = new Customers();
                var items = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                string returnName = "";


                #region Added by Md. Rakib Hasan
                string schemaName = "Customer";
                string tableName = "Customers";
                int subAccId = 14;

                var headDetail = await _headDetails.FirstOrDefaultAsync(hd => hd.SchemaName == schemaName && hd.TableName == tableName);

                if (headDetail == null)
                {
                    headDetail = new HeadDetails();
                    headDetail.SchemaName = schemaName;
                    headDetail.TableName = tableName;

                    headDetail.LIP = model.LIP;
                    headDetail.LMAC = model.LMAC;
                    headDetail.CreatedAt = DateTime.UtcNow;
                    headDetail.CreatedBy = model.CreatedBy;

                    await _headDetails.AddAsync(headDetail);
                }

                var head = await _heads.FirstOrDefaultAsync(h => h.HeadDetailID == headDetail.HeadDetailID);

                if (head == null)
                {
                    head = new Heads();
                    head.HeadDetailID = headDetail.HeadDetailID;

                    head.LIP = model.LIP;
                    head.LMAC = model.LMAC;
                    head.CreatedAt = DateTime.UtcNow;
                    head.CreatedBy = model.CreatedBy;

                    await _heads.AddAsync(head);
                }

                customerObj.HeadID = head.HeadID;

                var subAccDetails = await _subAccounts.AllActive().FirstOrDefaultAsync(x => x.SubAccountID == subAccId);

                if (subAccDetails == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Please Add Sub Account first!",
                    };
                }

                var generatedTrxAccCode = await _transactionAccountService.GenerateNextCodeAsync((int)subAccDetails.SubAccountID);

                TransactionAccounts trxAccount = new TransactionAccounts();
                trxAccount.SubAccountID = subAccDetails.SubAccountID;

                trxAccount.TrxAccCode = generatedTrxAccCode;

                trxAccount.TrxAccName = subAccDetails.SubAccountName;
                trxAccount.IsActive = true;
                trxAccount.Description = "Customer transaction account";
                trxAccount.Head = head;

                trxAccount.LIP = model.LIP;
                trxAccount.LMAC = model.LMAC;
                trxAccount.CreatedAt = DateTime.UtcNow;
                trxAccount.CreatedBy = model.CreatedBy;

                await _transactionAccounts.AddAsync(trxAccount);
                #endregion


                // prepare data
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.CreatedBy);
                var addressTypeObj = string.IsNullOrEmpty(model.CompnayName) ? await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "individual") : await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "company");
                // Create customer (individual)
                customerObj = new Customers()
                {
                    FullName = string.IsNullOrEmpty(model.CompnayName) ? model.FirstName + " " + model.LastName : model.CompnayName,
                    OrganizationID = model.OrganizationID, // have to be change later
                    IsPerson = string.IsNullOrEmpty(model.CompnayName) ? true : false,
                    HeadID = head.HeadID, // Added by Md. Rakib Hasan
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                };
                await _customersRepository.AddAsync(customerObj);
                returnName = customerObj.FullName;

                // Create address
                var addresses = new Addresses()
                {
                    FullAddress = model.FullAddress,
                    Street = model.Street,
                    City = model.City,
                    State = model.State,
                    Additionaladdress = model.Additionaladdress,
                    PostalCode = model.PostalCode,
                    CountryID = model.CountryID,
                    Phone = model.Phone,
                    OtherPhone = model.OtherPhone,
                    Email = model.Email,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                };
                await _addressesRepository.AddAsync(addresses);

                // Link customer with address
                var customerAddress = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    CustomerID = customerObj.CustomerID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                };
                await _customerAddressesRepository.AddAsync(customerAddress);
                returnID = customerAddress.CustomerAddressID;

                // Commit transaction
                await _customersRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Data saved successfully",
                    Id = returnID,
                    Name = returnName
                };
            }
            catch (Exception ex)
            {
                // Rollback transaction on error
                await _customersRepository.RollbackTransactionAsync();

                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
        #endregion

        #region update customer
        public async Task<ReturnView> UpdateCustomer(CustomerVM model)
        {
            await _customersRepository.BeginTransactionAsync();

            try
            {
                var nameToMatch = model.CompnayName ?? $"{model.FirstName}{model.LastName}";

                // Check for duplicate name
                var duplicate = await _customersRepository.AllActive()
                    .Where(q => q.FullName == nameToMatch &&
                                q.OrganizationID == model.OrganizationID &&
                                q.CustomerID != model.Id)
                    .FirstOrDefaultAsync();

                if (duplicate != null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Already exists in the database!"
                    };
                }

                // Ensure AddressType exists
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.CreatedBy);
                var addressTypeObj = string.IsNullOrEmpty(model.CompnayName)
                    ? await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "individual")
                    : await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "company");

                // Find existing customer
                var customerObj = await _customersRepository.AllActive()
                    .Include(x => x.CustomerAddresses)
                    .ThenInclude(x => x.Address)
                    .FirstOrDefaultAsync(q => q.CustomerID == model.Id && q.OrganizationID == model.OrganizationID);

                if (customerObj == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Customer not found!"
                    };
                }

                // Update main customer fields
                customerObj.FullName = string.IsNullOrEmpty(model.CompnayName)
                    ? $"{model.FirstName} {model.LastName}"
                    : model.CompnayName;
                customerObj.OrganizationID = model.OrganizationID;
                customerObj.IsPerson = string.IsNullOrEmpty(model.CompnayName);
                customerObj.UpdatedAt = DateTime.UtcNow;
                customerObj.UpdatedBy = model.CreatedBy;
                customerObj.LIP = model.LIP;
                customerObj.LMAC = model.LMAC;

                // Find existing address link (if any)
                var existingCustomerAddress = customerObj.CustomerAddresses
                    .FirstOrDefault(x => x.AddressTypeID == addressTypeObj.AddressTypeID);

                Addresses address;

                if (existingCustomerAddress != null)
                {
                    // Update existing address
                    address = existingCustomerAddress.Address;
                    address.FullAddress = model.FullAddress;
                    address.Street = model.Street;
                    address.City = model.City;
                    address.State = model.State;
                    address.Additionaladdress = model.Additionaladdress;
                    address.PostalCode = model.PostalCode;
                    address.CountryID = model.CountryID;
                    address.Phone = model.Phone;
                    address.OtherPhone = model.OtherPhone;
                    address.Email = model.Email;
                    address.Latitude = model.Latitude;
                    address.Longitude = model.Longitude;
                    address.FirstName = model.FirstName;
                    address.LastName = model.LastName;
                    address.UpdatedAt = DateTime.UtcNow;
                    address.UpdatedBy = model.CreatedBy;
                    address.LIP = model.LIP;
                    address.LMAC = model.LMAC;

                    await _addressesRepository.UpdateAsync(address);
                }
                else
                {
                    // Create new address if missing
                    address = new Addresses
                    {
                        FullAddress = model.FullAddress,
                        Street = model.Street,
                        City = model.City,
                        State = model.State,
                        Additionaladdress = model.Additionaladdress,
                        PostalCode = model.PostalCode,
                        CountryID = model.CountryID,
                        Phone = model.Phone,
                        OtherPhone = model.OtherPhone,
                        Email = model.Email,
                        Latitude = model.Latitude,
                        Longitude = model.Longitude,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                    };
                    await _addressesRepository.AddAsync(address);

                    var newLink = new CustomerAddresses
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = address.AddressID,
                        CustomerID = customerObj.CustomerID,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _customerAddressesRepository.AddAsync(newLink);
                }

                await _customersRepository.UpdateAsync(customerObj);
                await _customersRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Customer updated successfully!",
                    Id = customerObj.CustomerID,
                    Name = customerObj.FullName
                };
            }
            catch (Exception ex)
            {
                await _customersRepository.RollbackTransactionAsync();

                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        #endregion

        #region GetCustomerInfo
        public async Task<CustomerVM> GetCustomerInfo(int id, int organizationID)
        {
            try
            {
                var customer = await _customersRepository.AllActive()
                    .Include(c => c.CustomerAddresses)
                        .ThenInclude(ca => ca.Address)
                            .ThenInclude(a => a.Country)
                    .Include(c => c.CustomerAddresses)
                        .ThenInclude(ca => ca.AddressType)
                    .Where(c => c.CustomerID == id && c.OrganizationID == organizationID)
                    .Select(c => new
                    {
                        c.CustomerID,
                        c.FullName,
                        c.IsPerson,
                        Address = c.CustomerAddresses
                            .Where(ca => ca.AddressType.AddressTypeName.ToLower() == "individual"
                                      || ca.AddressType.AddressTypeName.ToLower() == "company")
                            .Select(ca => ca.Address)
                            .ToList()
                    })
                    .FirstOrDefaultAsync();


                if (customer == null)
                    return new CustomerVM();

                var address = customer.Address.FirstOrDefault();

                var vm = new CustomerVM
                {
                    Id = customer.CustomerID,
                    FirstName = address?.FirstName ?? "",
                    LastName = address?.LastName ?? "",
                    CompnayName = customer.IsPerson == true ? "" : customer.FullName,
                    FullAddress = address?.FullAddress ?? "",
                    City = address?.City ?? "",
                    Additionaladdress = address?.Additionaladdress ?? "",
                    State = address?.State ?? "",
                    Street = address?.Street ?? "",
                    Phone = address?.Phone ?? "",
                    OtherPhone = address?.OtherPhone ?? "",
                    Email = address?.Email ?? "",
                    CountryName = address?.Country?.CountryName ?? "",
                    CountryID = address?.CountryID,
                    PostalCode = address?.PostalCode ?? "",
                    Longitude = address?.Longitude,
                    Latitude = address?.Latitude
                };


                return vm;
            }
            catch (Exception) { return new CustomerVM(); }
        }
        #endregion

        #region get customer List
        public async Task<PaginationService<Customers, CustomerTableDataVM>.PaginationResult<CustomerTableDataVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CustomerName", string sortOrder = "asc")
        {
            try
            {
                var query = _customersRepository.AllActive().Include(x => x.CompanyWarehouses).Include(x => x.CompanyBranches).Where(t => t.OrganizationID == organizationID);
                query = query.Where(x => x.DeletedAt == null);

                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = sortColumn switch
                    {
                        "CustomerID" => sortOrder == "desc" ? query.OrderByDescending(x => x.CustomerID) : query.OrderBy(x => x.CustomerID),
                        "CustomerName" => sortOrder == "desc" ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
                        "CreatedAt" => sortOrder == "desc" ? query.OrderByDescending(x => x.CreatedAt) : query.OrderBy(x => x.CreatedAt),
                        _ => query.OrderBy(x => x.CustomerID)
                    };
                }

                return await PaginationService<Customers, CustomerTableDataVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                    term => x => EF.Functions.Like(x.FullName, $"%{term}%"),
                    x => new CustomerTableDataVM
                    {
                        ID = x.CustomerID,
                        Name = x.FullName,
                        Type = x.IsPerson == false ? "Company" : "Individual",
                        TotalBranch = x.CompanyBranches.Count(),
                        TotalWarehouse = x.CompanyWarehouses.Count(),
                        TotalShipping = x.CustomerAddresses.Count(),
                    });
            }
            catch (Exception) { return new PaginationService<Customers, CustomerTableDataVM>.PaginationResult<CustomerTableDataVM>(); }
        }
        #endregion

        #region CreateBranch
        public async Task<ReturnView> CreateBranch(BranchVM model)
        {
            // Begin transaction
            await _companyBranchesRepository.BeginTransactionAsync();

            try
            {
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.CreatedBy);
                // Get address type
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "branch");



                // Save branch
                var companyBranches = new CompanyBranches
                {
                    BranchName = model.BName,
                    CustomerID = model.BCustomerID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                };
                await _companyBranchesRepository.AddAsync(companyBranches);

                // Save address
                var addresses = new Addresses
                {
                    FullAddress = model.BFullAddress,
                    Street = model.BStreet,
                    City = model.BCity,
                    State = model.BState,
                    Additionaladdress = model.BAdditionaladdress,
                    PostalCode = model.BPostalCode,
                    CountryID = model.BCountryID,
                    Phone = model.BPhone,
                    OtherPhone = model.BOtherPhone,
                    Email = model.BEmail,
                    Latitude = model.BLatitude,
                    Longitude = model.BLongitude,
                    FirstName = model.BFirstName,
                    LastName = model.BLastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = model.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _addressesRepository.AddAsync(addresses);

                // Save branch address
                var companyBranchAddress = new CompanyBranchAddresses
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    BranchID = companyBranches.BranchID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = model.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _companyBranchAddressesRepository.AddAsync(companyBranchAddress);

                // Commit transaction
                await _companyBranchesRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Data saved successfully",
                };
            }
            catch (Exception ex)
            {
                await _companyBranchesRepository.RollbackTransactionAsync();

                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region UpdateBranch
        public async Task<ReturnView> UpdateBranch(BranchVM model)
        {
            // Begin transaction
            await _companyBranchesRepository.BeginTransactionAsync();

            try
            {
                // Ensure AddressType "branch" exists
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.UpdatedBy ?? model.CreatedBy);
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "branch");

                // Fetch existing branch with address and relation
                var branch = await _companyBranchesRepository
                    .AllActive()
                    .Include(b => b.CompanyBranchAddresses)
                        .ThenInclude(ba => ba.Address)
                    .FirstOrDefaultAsync(b => b.BranchID == model.Bid);

                if (branch == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Branch not found."
                    };
                }

                // Update branch main info
                branch.BranchName = model.BName;
                branch.CustomerID = model.BCustomerID;
                branch.UpdatedAt = DateTime.UtcNow;
                branch.UpdatedBy = model.UpdatedBy;
                branch.LIP = model.LIP;
                branch.LMAC = model.LMAC;
                await _companyBranchesRepository.UpdateAsync(branch);

                // Get the existing branch address mapping
                var branchAddress = branch.CompanyBranchAddresses.FirstOrDefault();

                if (branchAddress != null)
                {
                    // Update existing address
                    var address = branchAddress.Address;
                    address.FullAddress = model.BFullAddress;
                    address.Street = model.BStreet;
                    address.City = model.BCity;
                    address.State = model.BState;
                    address.Additionaladdress = model.BAdditionaladdress;
                    address.PostalCode = model.BPostalCode;
                    address.CountryID = model.BCountryID;
                    address.Phone = model.BPhone;
                    address.OtherPhone = model.BOtherPhone;
                    address.Email = model.BEmail;
                    address.Latitude = model.BLatitude;
                    address.Longitude = model.BLongitude;
                    address.FirstName = model.BFirstName;
                    address.LastName = model.BLastName;
                    address.UpdatedAt = DateTime.UtcNow;
                    address.UpdatedBy = model.UpdatedBy;
                    address.LIP = model.LIP;
                    address.LMAC = model.LMAC;
                    await _addressesRepository.UpdateAsync(address);

                    // Update mapping record
                    branchAddress.AddressTypeID = addressTypeObj.AddressTypeID;
                    branchAddress.UpdatedAt = DateTime.UtcNow;
                    branchAddress.UpdatedBy = model.UpdatedBy;
                    branchAddress.LIP = model.LIP;
                    branchAddress.LMAC = model.LMAC;
                    await _companyBranchAddressesRepository.UpdateAsync(branchAddress);
                }
                else
                {
                    // If branch has no address yet, create a new one
                    var newAddress = new Addresses
                    {
                        FullAddress = model.BFullAddress,
                        Street = model.BStreet,
                        City = model.BCity,
                        State = model.BState,
                        Additionaladdress = model.BAdditionaladdress,
                        PostalCode = model.BPostalCode,
                        CountryID = model.BCountryID,
                        Phone = model.BPhone,
                        OtherPhone = model.BOtherPhone,
                        Email = model.BEmail,
                        Latitude = model.BLatitude,
                        Longitude = model.BLongitude,
                        FirstName = model.BFirstName,
                        LastName = model.BLastName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _addressesRepository.AddAsync(newAddress);

                    var newMapping = new CompanyBranchAddresses
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = newAddress.AddressID,
                        BranchID = branch.BranchID,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _companyBranchAddressesRepository.AddAsync(newMapping);
                }

                // Commit transaction
                await _companyBranchesRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Branch updated successfully"
                };
            }
            catch (Exception ex)
            {
                await _companyBranchesRepository.RollbackTransactionAsync();
                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region Get GetBranchInfo
        public async Task<BranchVM> GetBranchInfo(int customerID, int branchId, int organizationID)
        {
            try
            {
                var branch = await _companyBranchesRepository.AllActive()
                    .Include(b => b.Customer)
                    .Include(b => b.CompanyBranchAddresses)
                        .ThenInclude(cba => cba.Address)
                            .ThenInclude(a => a.Country)
                    .Where(b => b.CustomerID == customerID &&
                                b.BranchID == branchId &&
                                b.Customer.OrganizationID == organizationID)
                    .Select(b => new BranchVM
                    {
                        Bid = b.BranchID,
                        BCustomerID = b.CustomerID,
                        BName = b.BranchName ?? "",
                        BCustomerName = b.Customer.FullName,
                        BFirstName = b.CompanyBranchAddresses.Select(a => a.Address.FirstName).FirstOrDefault() ?? "",
                        BLastName = b.CompanyBranchAddresses.Select(a => a.Address.LastName).FirstOrDefault() ?? "",
                        BEmail = b.CompanyBranchAddresses.Select(a => a.Address.Email).FirstOrDefault() ?? "",
                        BFullAddress = b.CompanyBranchAddresses.Select(a => a.Address.FullAddress).FirstOrDefault() ?? "",
                        BAdditionaladdress = b.CompanyBranchAddresses.Select(a => a.Address.Additionaladdress).FirstOrDefault() ?? "",
                        BCity = b.CompanyBranchAddresses.Select(a => a.Address.City).FirstOrDefault() ?? "",
                        BState = b.CompanyBranchAddresses.Select(a => a.Address.State).FirstOrDefault() ?? "",
                        BStreet = b.CompanyBranchAddresses.Select(a => a.Address.Street).FirstOrDefault() ?? "",
                        BPostalCode = b.CompanyBranchAddresses.Select(a => a.Address.PostalCode).FirstOrDefault() ?? "",
                        BPhone = b.CompanyBranchAddresses.Select(a => a.Address.Phone).FirstOrDefault() ?? "",
                        BOtherPhone = b.CompanyBranchAddresses.Select(a => a.Address.OtherPhone).FirstOrDefault() ?? "",
                        BCountryID = b.CompanyBranchAddresses.Select(a => a.Address.CountryID).FirstOrDefault(),
                        BCountryName = b.CompanyBranchAddresses.Select(a => a.Address.Country.CountryName).FirstOrDefault(),
                        BLongitude = b.CompanyBranchAddresses.Select(a => a.Address.Longitude).FirstOrDefault(),
                        BLatitude = b.CompanyBranchAddresses.Select(a => a.Address.Latitude).FirstOrDefault(),
                    })
                    .FirstOrDefaultAsync();

                return branch ?? new BranchVM();
            }
            catch
            {
                return new BranchVM();
            }
        }

        #endregion

        #region Get CompanyBranchList
        public async Task<List<BranchVM>> GetBranchList(int companyID, int organizationID)
        {
            try
            {
                var customer = await _customersRepository.AllActive()
                .Include(q => q.CompanyBranches)
                .ThenInclude(q => q.CompanyBranchAddresses)
                .ThenInclude(q => q.Address)
                .FirstOrDefaultAsync(q => q.CustomerID == companyID && q.OrganizationID == organizationID);

                if (customer == null)
                    return new List<BranchVM>();

                var result = customer.CompanyBranches
                    .Select(x => new BranchVM
                    {
                        Bid = x.BranchID,
                        BCustomerID = x.CustomerID,
                        BName = x.BranchName ?? "",
                        BFirstName = x.CompanyBranchAddresses.Select(x => x.Address.FirstName).FirstOrDefault() ?? "",
                        BLastName = x.CompanyBranchAddresses.Select(x => x.Address.LastName).FirstOrDefault() ?? "",
                        BFullAddress = x.CompanyBranchAddresses.Select(x => x.Address.FullAddress).FirstOrDefault() ?? "",
                        BAdditionaladdress = x.CompanyBranchAddresses.Select(x => x.Address.FullAddress).FirstOrDefault() ?? "",
                        BCity = x.CompanyBranchAddresses.Select(x => x.Address.City).FirstOrDefault() ?? "",
                        BState = x.CompanyBranchAddresses.Select(x => x.Address.State).FirstOrDefault() ?? "",
                        BStreet = x.CompanyBranchAddresses.Select(x => x.Address.Street).FirstOrDefault() ?? "",
                        BPostalCode = x.CompanyBranchAddresses.Select(x => x.Address.PostalCode).FirstOrDefault() ?? "",
                        BPhone = x.CompanyBranchAddresses.Select(x => x.Address.Phone).FirstOrDefault() ?? "",
                        BOtherPhone = x.CompanyBranchAddresses.Select(x => x.Address.OtherPhone).FirstOrDefault() ?? "",
                        BCountryID = x.CompanyBranchAddresses.Select(x => x.Address.CountryID).FirstOrDefault(),
                    })
                    .ToList();

                return result;
            }
            catch (Exception) { return new List<BranchVM>(); }

        }
        #endregion

        #region Warehouse
        public async Task<ReturnView> CreateWarehouse(WarehouseVM model)
        {
            // Begin transaction
            await _companyWarehousesRepository.BeginTransactionAsync();

            try
            {
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.CreatedBy);
                // Get address type
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "warehouse");



                // Save branch
                var companyWarehouse = new CompanyWarehouses
                {
                    WarehouseName = model.WName,
                    CustomerID = model.WCustomerID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                };
                await _companyWarehousesRepository.AddAsync(companyWarehouse);

                // Save address
                var addresses = new Addresses
                {
                    FullAddress = model.WFullAddress,
                    Street = model.WStreet,
                    City = model.WCity,
                    State = model.WState,
                    Additionaladdress = model.WAdditionaladdress,
                    PostalCode = model.WPostalCode,
                    CountryID = model.WCountryID,
                    Phone = model.WPhone,
                    OtherPhone = model.WOtherPhone,
                    Email = model.WEmail,
                    Latitude = model.WLatitude,
                    Longitude = model.WLongitude,
                    FirstName = model.WFirstName,
                    LastName = model.WLastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = model.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _addressesRepository.AddAsync(addresses);

                // Save branch address
                var companyBranchAddress = new CompanyWarehouseAddresses
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    WarehouseID = companyWarehouse.WarehouseID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = model.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _companyWarehouseAddressesRepository.AddAsync(companyBranchAddress);

                // Commit transaction
                await _companyBranchesRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Data saved successfully",
                };
            }
            catch (Exception ex)
            {
                await _companyBranchesRepository.RollbackTransactionAsync();

                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region Update Warehouse
        public async Task<ReturnView> UpdateWarehouse(WarehouseVM model)
        {
            // Begin transaction
            await _companyWarehousesRepository.BeginTransactionAsync();

            try
            {
                // Ensure AddressType "warehouse" exists
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.UpdatedBy ?? model.CreatedBy);
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "warehouse");

                // Fetch existing warehouse with related address
                var warehouse = await _companyWarehousesRepository
                    .AllActive()
                    .Include(w => w.CompanyWarehouseAddresses)
                        .ThenInclude(wa => wa.Address)
                    .FirstOrDefaultAsync(w => w.WarehouseID == model.Wid);

                if (warehouse == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Warehouse not found."
                    };
                }

                // ✅ Update warehouse main info
                warehouse.WarehouseName = model.WName;
                warehouse.CustomerID = model.WCustomerID;
                warehouse.UpdatedAt = DateTime.UtcNow;
                warehouse.UpdatedBy = model.UpdatedBy;
                warehouse.LIP = model.LIP;
                warehouse.LMAC = model.LMAC;
                await _companyWarehousesRepository.UpdateAsync(warehouse);

                // ✅ Get the existing warehouse address link
                var warehouseAddress = warehouse.CompanyWarehouseAddresses.FirstOrDefault();

                if (warehouseAddress != null)
                {
                    // Update existing address
                    var address = warehouseAddress.Address;
                    address.FullAddress = model.WFullAddress;
                    address.Street = model.WStreet;
                    address.City = model.WCity;
                    address.State = model.WState;
                    address.Additionaladdress = model.WAdditionaladdress;
                    address.PostalCode = model.WPostalCode;
                    address.CountryID = model.WCountryID;
                    address.Phone = model.WPhone;
                    address.OtherPhone = model.WOtherPhone;
                    address.Email = model.WEmail;
                    address.Latitude = model.WLatitude;
                    address.Longitude = model.WLongitude;
                    address.FirstName = model.WFirstName;
                    address.LastName = model.WLastName;
                    address.UpdatedAt = DateTime.UtcNow;
                    address.UpdatedBy = model.UpdatedBy;
                    address.LIP = model.LIP;
                    address.LMAC = model.LMAC;
                    await _addressesRepository.UpdateAsync(address);

                    // Update mapping info
                    warehouseAddress.AddressTypeID = addressTypeObj.AddressTypeID;
                    warehouseAddress.UpdatedAt = DateTime.UtcNow;
                    warehouseAddress.UpdatedBy = model.UpdatedBy;
                    warehouseAddress.LIP = model.LIP;
                    warehouseAddress.LMAC = model.LMAC;
                    await _companyWarehouseAddressesRepository.UpdateAsync(warehouseAddress);
                }
                else
                {
                    // If no address exists yet, create a new one
                    var newAddress = new Addresses
                    {
                        FullAddress = model.WFullAddress,
                        Street = model.WStreet,
                        City = model.WCity,
                        State = model.WState,
                        Additionaladdress = model.WAdditionaladdress,
                        PostalCode = model.WPostalCode,
                        CountryID = model.WCountryID,
                        Phone = model.WPhone,
                        OtherPhone = model.WOtherPhone,
                        Email = model.WEmail,
                        Latitude = model.WLatitude,
                        Longitude = model.WLongitude,
                        FirstName = model.WFirstName,
                        LastName = model.WLastName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _addressesRepository.AddAsync(newAddress);

                    var newMapping = new CompanyWarehouseAddresses
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = newAddress.AddressID,
                        WarehouseID = warehouse.WarehouseID,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = model.UpdatedBy ?? model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC
                    };
                    await _companyWarehouseAddressesRepository.AddAsync(newMapping);
                }

                // ✅ Commit transaction
                await _companyWarehousesRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Warehouse updated successfully"
                };
            }
            catch (Exception ex)
            {
                await _companyWarehousesRepository.RollbackTransactionAsync();
                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region get customer List
        public async Task<PaginationService<CompanyWarehouses, WarehouseVM>.PaginationResult<WarehouseVM>>
    GetAllWarehouseAsync(int customerID, int organizationID, int pageNumber = 1, int pageSize = 5,
                         string searchTerm = "", string sortColumn = "WarehouseName", string sortOrder = "asc")
        {
            try
            {
                var query = _companyWarehousesRepository.AllActive()
                    .Include(b => b.Customer)
                    .Include(b => b.CompanyWarehouseAddresses)
                        .ThenInclude(cba => cba.Address)
                            .ThenInclude(a => a.Country)
                    .Where(b => b.CustomerID == customerID && b.Customer.OrganizationID == organizationID);

                // Sorting
                query = sortColumn switch
                {
                    "WarehouseID" => sortOrder == "desc" ? query.OrderByDescending(x => x.WarehouseID) : query.OrderBy(x => x.WarehouseID),
                    "WarehouseName" => sortOrder == "desc" ? query.OrderByDescending(x => x.WarehouseName) : query.OrderBy(x => x.WarehouseName),
                    _ => query.OrderBy(x => x.WarehouseName)
                };

                // Pagination
                return await PaginationService<CompanyWarehouses, WarehouseVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => x => EF.Functions.Like(x.WarehouseName, $"%{term}%"),
                    x => new WarehouseVM
                    {
                        Wid = x.WarehouseID,
                        WCustomerID = x.CustomerID,
                        WName = x.WarehouseName ?? "",
                        WCustomerName = x.Customer.FullName,
                        WFirstName = x.CompanyWarehouseAddresses.Select(a => a.Address.FirstName).FirstOrDefault() ?? "",
                        WLastName = x.CompanyWarehouseAddresses.Select(a => a.Address.LastName).FirstOrDefault() ?? "",
                        WEmail = x.CompanyWarehouseAddresses.Select(a => a.Address.Email).FirstOrDefault() ?? "",
                        WFullAddress = x.CompanyWarehouseAddresses.Select(a => a.Address.FullAddress).FirstOrDefault() ?? "",
                        WAdditionaladdress = x.CompanyWarehouseAddresses.Select(a => a.Address.Additionaladdress).FirstOrDefault() ?? "",
                        WCity = x.CompanyWarehouseAddresses.Select(a => a.Address.City).FirstOrDefault() ?? "",
                        WState = x.CompanyWarehouseAddresses.Select(a => a.Address.State).FirstOrDefault() ?? "",
                        WStreet = x.CompanyWarehouseAddresses.Select(a => a.Address.Street).FirstOrDefault() ?? "",
                        WPostalCode = x.CompanyWarehouseAddresses.Select(a => a.Address.PostalCode).FirstOrDefault() ?? "",
                        WPhone = x.CompanyWarehouseAddresses.Select(a => a.Address.Phone).FirstOrDefault() ?? "",
                        WOtherPhone = x.CompanyWarehouseAddresses.Select(a => a.Address.OtherPhone).FirstOrDefault() ?? "",
                        WCountryID = x.CompanyWarehouseAddresses.Select(a => a.Address.CountryID).FirstOrDefault(),
                        WCountryName = x.CompanyWarehouseAddresses.Select(a => a.Address.Country.CountryName).FirstOrDefault(),
                        WLongitude = x.CompanyWarehouseAddresses.Select(a => a.Address.Longitude).FirstOrDefault(),
                        WLatitude = x.CompanyWarehouseAddresses.Select(a => a.Address.Latitude).FirstOrDefault(),
                    });
            }
            catch (Exception)
            {
                return new PaginationService<CompanyWarehouses, WarehouseVM>.PaginationResult<WarehouseVM>();
            }
        }

        #endregion
        #region get customer List
        public async Task<PaginationService<CustomerAddresses, ShippingVM>.PaginationResult<ShippingVM>>
    GetAllShippingAsync(int customerID, int organizationID, int pageNumber = 1, int pageSize = 5,
                         string searchTerm = "", string sortColumn = "WarehouseName", string sortOrder = "asc")
        {
            try
            {
                var query = _customerAddressesRepository.AllActive()
                    .Include(b => b.Customer)
                    .Include(b => b.Address)
                    .ThenInclude(b => b.Country)
                    .Where(b => b.CustomerID == customerID && b.Customer.OrganizationID == organizationID);

                // Sorting
                query = sortColumn switch
                {
                    "ShippingID" => sortOrder == "desc" ? query.OrderByDescending(x => x.AddressID) : query.OrderBy(x => x.AddressID),
                    "ShippingAddress" => sortOrder == "desc" ? query.OrderByDescending(x => x.Address.FullAddress) : query.OrderBy(x => x.Address.FullAddress),
                    _ => query.OrderBy(x => x.CreatedBy)
                };

                // Pagination
                return await PaginationService<CustomerAddresses, ShippingVM>.GetPaginatedData(
                    query,
                    pageNumber,
                    pageSize,
                    searchTerm,
                    sortColumn,
                    sortOrder,
                    term => x => EF.Functions.Like(x.Address.FullAddress, $"%{term}%"),
                    x => new ShippingVM
                    {
                        Sid = x.AddressID,
                        SCustomerID = x.CustomerID,
                        SCustomerName = x.Customer.FullName,
                        SFirstName = x.Address.FirstName ?? "",
                        SLastName = x.Address.LastName ?? "",
                        SEmail = x.Address.Email ?? "",
                        SFullAddress = x.Address.FullAddress ?? "",
                        SAdditionaladdress = x.Address.Additionaladdress ?? "",
                        SCity = x.Address.City ?? "",
                        SState = x.Address.State ?? "",
                        SStreet = x.Address.Street ?? "",
                        SPostalCode = x.Address.PostalCode ?? "",
                        SPhone = x.Address.Phone ?? "",
                        SOtherPhone = x.Address.OtherPhone ?? "",
                        SCountryID = x.Address.CountryID,
                        SCountryName = x.Address.Country.CountryName,
                        SLongitude = x.Address.Longitude,
                        SLatitude = x.Address.Latitude,
                    });
            }
            catch (Exception)
            {
                return new PaginationService<CustomerAddresses, ShippingVM>.PaginationResult<ShippingVM>();
            }
        }

        #endregion

        #region Get GetBranchInfo
        public async Task<WarehouseVM> GetWarehouseInfo(int customerID, int branchId, int organizationID)
        {
            try
            {
                var branch = await _companyWarehousesRepository.AllActive()
                    .Include(b => b.Customer)
                    .Include(b => b.CompanyWarehouseAddresses)
                        .ThenInclude(cba => cba.Address)
                            .ThenInclude(a => a.Country)
                    .Where(b => b.CustomerID == customerID &&
                                b.WarehouseID == branchId &&
                                b.Customer.OrganizationID == organizationID)
                    .Select(b => new WarehouseVM
                    {
                        Wid = b.WarehouseID,
                        WCustomerID = b.CustomerID,
                        WName = b.WarehouseName ?? "",
                        WCustomerName = b.Customer.FullName,
                        WFirstName = b.CompanyWarehouseAddresses.Select(a => a.Address.FirstName).FirstOrDefault() ?? "",
                        WLastName = b.CompanyWarehouseAddresses.Select(a => a.Address.LastName).FirstOrDefault() ?? "",
                        WEmail = b.CompanyWarehouseAddresses.Select(a => a.Address.Email).FirstOrDefault() ?? "",
                        WFullAddress = b.CompanyWarehouseAddresses.Select(a => a.Address.FullAddress).FirstOrDefault() ?? "",
                        WAdditionaladdress = b.CompanyWarehouseAddresses.Select(a => a.Address.Additionaladdress).FirstOrDefault() ?? "",
                        WCity = b.CompanyWarehouseAddresses.Select(a => a.Address.City).FirstOrDefault() ?? "",
                        WState = b.CompanyWarehouseAddresses.Select(a => a.Address.State).FirstOrDefault() ?? "",
                        WStreet = b.CompanyWarehouseAddresses.Select(a => a.Address.Street).FirstOrDefault() ?? "",
                        WPostalCode = b.CompanyWarehouseAddresses.Select(a => a.Address.PostalCode).FirstOrDefault() ?? "",
                        WPhone = b.CompanyWarehouseAddresses.Select(a => a.Address.Phone).FirstOrDefault() ?? "",
                        WOtherPhone = b.CompanyWarehouseAddresses.Select(a => a.Address.OtherPhone).FirstOrDefault() ?? "",
                        WCountryID = b.CompanyWarehouseAddresses.Select(a => a.Address.CountryID).FirstOrDefault(),
                        WCountryName = b.CompanyWarehouseAddresses.Select(a => a.Address.Country.CountryName).FirstOrDefault(),
                        WLongitude = b.CompanyWarehouseAddresses.Select(a => a.Address.Longitude).FirstOrDefault(),
                        WLatitude = b.CompanyWarehouseAddresses.Select(a => a.Address.Latitude).FirstOrDefault(),
                    })
                    .FirstOrDefaultAsync();

                return branch ?? new WarehouseVM();
            }
            catch
            {
                return new WarehouseVM();
            }
        }

        #endregion

        #region CreateShipping
        public async Task<ReturnView> CreateShipping(ShippingVM model)
        {
            // Begin transaction
            await _companyWarehousesRepository.BeginTransactionAsync();

            try
            {
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.CreatedBy);
                // Get address type
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");

                var addresses = new Addresses()
                {
                    FullAddress = model.SFullAddress,
                    Street = model.SStreet,
                    City = model.SCity,
                    State = model.SState,
                    Additionaladdress = model.SAdditionaladdress,
                    PostalCode = model.SPostalCode,
                    CountryID = model.SCountryID,
                    Phone = model.SPhone,
                    OtherPhone = model.SOtherPhone,
                    Email = model.SEmail,
                    Latitude = model.SLatitude,
                    Longitude = model.SLongitude,
                    FirstName = model.SFirstName,
                    LastName = model.SLastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = model.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _addressesRepository.AddAsync(addresses);

                // Link customer with address
                var individualAddresses = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    CustomerID = model.SCustomerID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = model.CreatedBy,
                    LIP = model.LIP,
                    LMAC = model.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = model.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _customerAddressesRepository.AddAsync(individualAddresses);

                
                

                // Commit transaction
                await _companyBranchesRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Data saved successfully",
                };
            }
            catch (Exception ex)
            {
                await _companyBranchesRepository.RollbackTransactionAsync();

                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region Update shipping Address
        public async Task<ReturnView> UpdateShipping(ShippingVM model)
        {
            // Begin transaction
            await _customerAddressesRepository.BeginTransactionAsync();

            try
            {
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.UpdatedBy);
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");

                // Fetch existing address
                var existingCustomerAddress = await _customerAddressesRepository
                    .FirstOrDefaultAsync(a => a.CustomerAddressID == model.Sid);

                if (existingCustomerAddress == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Shipping address not found."
                    };
                }

                var existingAddress = await _addressesRepository
                    .FirstOrDefaultAsync(a => a.AddressID == existingCustomerAddress.AddressID);

                if (existingAddress == null)
                {
                    return new ReturnView
                    {
                        Success = false,
                        Message = "Address record not found."
                    };
                }

                // Update address
                existingAddress.FullAddress = model.SFullAddress;
                existingAddress.Street = model.SStreet;
                existingAddress.City = model.SCity;
                existingAddress.State = model.SState;
                existingAddress.Additionaladdress = model.SAdditionaladdress;
                existingAddress.PostalCode = model.SPostalCode;
                existingAddress.CountryID = model.SCountryID;
                existingAddress.Phone = model.SPhone;
                existingAddress.OtherPhone = model.SOtherPhone;
                existingAddress.Email = model.SEmail;
                existingAddress.Latitude = model.SLatitude;
                existingAddress.Longitude = model.SLongitude;
                existingAddress.FirstName = model.SFirstName;
                existingAddress.LastName = model.SLastName;
                existingAddress.UpdatedAt = DateTime.UtcNow;
                existingAddress.UpdatedBy = model.UpdatedBy;
                existingAddress.LIP = model.LIP;
                existingAddress.LMAC = model.LMAC;

                await _addressesRepository.UpdateAsync(existingAddress);

                // Update customer address link (if needed)
                existingCustomerAddress.AddressTypeID = addressTypeObj.AddressTypeID;
                existingCustomerAddress.CustomerID = model.SCustomerID;
                existingCustomerAddress.UpdatedAt = DateTime.UtcNow;
                existingCustomerAddress.UpdatedBy = model.UpdatedBy;
                existingCustomerAddress.LIP = model.LIP;
                existingCustomerAddress.LMAC = model.LMAC;

                await _customerAddressesRepository.UpdateAsync(existingCustomerAddress);

                // Commit transaction
                await _customerAddressesRepository.CommitTransactionAsync();

                return new ReturnView
                {
                    Success = true,
                    Message = "Shipping address updated successfully."
                };
            }
            catch (Exception ex)
            {
                await _customerAddressesRepository.RollbackTransactionAsync();

                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        #endregion

        #region get GetOrganizationTypesList 
        public async Task<ReturnDataView<SelectListItem>> GetOrganizationTypesList(string search, int page, int pageSize, int organizationID, string userFor="Branch")
        {
            try
            {
                var query = _organizationTypesRepository
                                .AllActive().Where(x=> x.UseFor == userFor);


                if (!string.IsNullOrWhiteSpace(search))
                {
                    string pattern = $"%{search}%";

                    query = query.Where(c =>
                        c != null &&
                        (
                            EF.Functions.Like(c.OrganizationTypeName, pattern) ||
                            EF.Functions.Like(c.CreatedAt, pattern)
                        ));
                }


                var totalCount = await query.CountAsync();

                var items = await query
                    .OrderBy(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize).Select(t => new SelectListItem
                    {
                        Value = t.OrganizationTypeID.ToString(),
                        Text = t.OrganizationTypeName
                    }).ToListAsync();

                return new ReturnDataView<SelectListItem>
                {
                    data = items,
                    totalItem = totalCount,
                    message = "Data loaded"
                };
            } catch(Exception e) {
                return new ReturnDataView<SelectListItem>();
            }
            
        }
        #endregion
    }
}
