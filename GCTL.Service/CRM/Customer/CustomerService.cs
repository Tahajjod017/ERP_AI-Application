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
                    });
            }
            catch (Exception) { return new PaginationService<Customers, CustomerTableDataVM>.PaginationResult<CustomerTableDataVM>(); }
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
                .Include(a => a.CustomerAddresses)
                    .ThenInclude(c => c.AddressType)
                .Where(c => c.CustomerID == id && c.OrganizationID == organizationID)
                .Select(c => new
                {
                    c.CustomerID,
                    c.FullName,
                    c.IsPerson,
                    Address = c.CustomerAddresses
                        .Select(ca => ca.Address)
                        .Where(a => a.CustomerAddresses.Select(x => x.AddressType.AddressTypeName.ToLower() == "individual" || x.AddressType.AddressTypeName.ToLower() == "company")
                        .FirstOrDefault())
                })
                .FirstOrDefaultAsync();

                if (customer == null)
                    return new CustomerVM();

                var vm = new CustomerVM
                {
                    Id = customer.CustomerID,
                    FirstName = customer.Address?.Select(x => x.FirstName).FirstOrDefault() ?? "",
                    LastName = customer.Address?.Select(x => x.LastName).FirstOrDefault() ?? "",
                    CompnayName = customer.IsPerson == false ? customer.FullName : "",
                    FullAddress = customer.Address?.Select(x => x.FullAddress).FirstOrDefault() ?? "",
                    City = customer.Address?.Select(x => x.City).FirstOrDefault() ?? "",
                    Additionaladdress = customer.Address?.Select(x => x.Additionaladdress).FirstOrDefault() ?? "",
                    State = customer.Address?.Select(x => x.State).FirstOrDefault() ?? "",
                    Street = customer.Address?.Select(x => x.Street).FirstOrDefault() ?? "",
                    Phone = customer.Address?.Select(x => x.Phone).FirstOrDefault() ?? "",
                    OtherPhone = customer.Address?.Select(x => x.OtherPhone).FirstOrDefault() ?? "",
                    Email = customer.Address?.Select(x => x.Email).FirstOrDefault() ?? "",
                    CountryID = customer.Address?.Select(x => x.CountryID).FirstOrDefault(),
                    PostalCode = customer.Address?.Select(x => x.PostalCode).FirstOrDefault() ?? "",
                    Longitude = customer.Address?.Select(x => x.Longitude).FirstOrDefault(),
                    Latitude = customer.Address?.Select(x => x.Latitude).FirstOrDefault()
                };

                return vm;
            }
            catch (Exception) { return new CustomerVM(); }
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
            } catch (Exception) { return new List<BranchVM>(); }
            
        }
        #endregion

        #region Get GetBranchInfo
        public async Task<BranchVM> GetBranchInfo(int customerID, int branchId, int organizationID)
        {
            try
            {
                var customer = await _customersRepository.AllActive()
                .Include(q => q.CompanyBranches)
                .ThenInclude(q => q.CompanyBranchAddresses)
                .ThenInclude(q => q.Address)
                .FirstOrDefaultAsync(q => q.CustomerID == customerID && q.OrganizationID == organizationID);

                if (customer == null)
                    return new BranchVM();

                var result = customer.CompanyBranches
                    .Where(b => b.BranchID == branchId)
                    .Select(x => new BranchVM
                    {
                        Bid = x.BranchID,
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
                    }).FirstOrDefault();

                return result;
            } catch(Exception)
            {
                return new BranchVM();
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
