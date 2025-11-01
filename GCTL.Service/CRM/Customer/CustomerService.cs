using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Core.ViewModels.MasterSetup.Genders;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using GCTL.Service.Pagination;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.CRM.Customer
{
    public class CustomerService : ICustomerService
    {
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<CompanyWarehouses> _companyWarehousesRepository;
        private readonly IGenericRepository<CompanyWarehouseAddresses> _companyWarehouseAddressesRepository;
        private readonly IGenericRepository<CompanyBranches> _companyBranchesRepository;
        private readonly IGenericRepository<CompanyBranchAddresses> _companyBranchAddressesRepository;
        #region Added by Md. Rakib Hasan
        private readonly IGenericRepository<Heads> _heads;
        private readonly IGenericRepository<HeadDetails> _headDetails;
        private readonly IGenericRepository<TransactionAccounts> _transactionAccounts;
        private readonly IGenericRepository<SubAccounts> _subAccounts;
        private readonly ITransactionAccountService _transactionAccountService;
        #endregion
        public CustomerService(IGenericRepository<Country> countryRepository, IGenericRepository<Customers> customersRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, ITransactionAccountService transactionAccountService, IGenericRepository<Heads> heads, IGenericRepository<HeadDetails> headDetails, IGenericRepository<TransactionAccounts> transactionAccounts, IGenericRepository<SubAccounts> subAccounts, IGenericRepository<CustomerAddresses> customerAddressesRepository, IGenericRepository<CompanyWarehouses> companyWarehousesRepository, IGenericRepository<CompanyWarehouseAddresses> companyWarehouseAddressesRepository, IGenericRepository<CompanyBranches> companyBranchesRepository, IGenericRepository<CompanyBranchAddresses> companyBranchAddressesRepository)
        {
            _countryRepository = countryRepository;
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
        }

        #region CreatePerson

        public async Task<bool> InserAddressTypeIntoDB(string? LIP, string? LMAC, int? CreatedBy)
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
        public async Task<ReturnView> CreateCustomer (CustomerVM model)
        {
            // Begin transaction
            await _customersRepository.BeginTransactionAsync();

            try
            {
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
        #region Warehouse
        public async Task<ReturnView> CreateShipping(ShippingVM model)
        {
            // Begin transaction
            await _companyWarehousesRepository.BeginTransactionAsync();

            try
            {
                await InserAddressTypeIntoDB(model.LIP, model.LMAC, model.CreatedBy);
                // Get address type
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");



                //// Save branch
                //var companyShipping = new CompanyWarehouses
                //{
                //    CustomerID = model.SCustomerID,
                //    CreatedAt = DateTime.UtcNow,
                //    CreatedBy = model.CreatedBy,
                //    LIP = model.LIP,
                //    LMAC = model.LMAC,
                //};
                //await _companyWarehousesRepository.AddAsync(companyShipping);

                // Save address
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
        public async Task<PaginationService<Customers, CustomerVM>.PaginationResult<CustomerVM>> GetAllAsync(int organizationID, int pageNumber = 1, int pageSize = 5, string searchTerm = "", string sortColumn = "CustomerName", string sortOrder = "asc")
        {
            var query = _customersRepository.AllActive().Where(t => t.OrganizationID == organizationID);
            query = query.Where(x => x.DeletedAt == null);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                query = sortColumn switch
                {
                    "GenderID" => sortOrder == "desc" ? query.OrderByDescending(x => x.CustomerID) : query.OrderBy(x => x.CustomerID),
                    "CustomerName" => sortOrder == "desc" ? query.OrderByDescending(x => x.FullName) : query.OrderBy(x => x.FullName),
                    _ => query.OrderBy(x => x.CustomerID)
                };
            }

            return await PaginationService<Customers, CustomerVM>.GetPaginatedData(query, pageNumber, pageSize, searchTerm, sortColumn, sortOrder,
                term => x => EF.Functions.Like(x.FullName, $"%{term}%"),
                x => new CustomerVM
                {
                    ID = x.CustomerID,
                    CompnayName = x.FullName,
                });
        }
        #endregion
    }
}
