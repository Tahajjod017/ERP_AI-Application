using GCTL.Core.Repository;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using GCTL.Data.Models;
using GCTL.Service.Finance.TransactionAccount;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<bool> InserAddressTypeIntoDB(CustomerVM model)
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
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC,
                    });
                }

                await _addressTypesRepository.AddRangeAsync(addressTypes);
            }
            return true;
        }
        public async Task<ReturnView> CreateCustomer (CustomerVM customerVM)
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

                    headDetail.LIP = customerVM.LIP;
                    headDetail.LMAC = customerVM.LMAC;
                    headDetail.CreatedAt = DateTime.UtcNow;
                    headDetail.CreatedBy = customerVM.CreatedBy;

                    await _headDetails.AddAsync(headDetail);
                }

                var head = await _heads.FirstOrDefaultAsync(h => h.HeadDetailID == headDetail.HeadDetailID);

                if (head == null)
                {
                    head = new Heads();
                    head.HeadDetailID = headDetail.HeadDetailID;

                    head.LIP = customerVM.LIP;
                    head.LMAC = customerVM.LMAC;
                    head.CreatedAt = DateTime.UtcNow;
                    head.CreatedBy = customerVM.CreatedBy;

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

                trxAccount.LIP = customerVM.LIP;
                trxAccount.LMAC = customerVM.LMAC;
                trxAccount.CreatedAt = DateTime.UtcNow;
                trxAccount.CreatedBy = customerVM.CreatedBy;

                await _transactionAccounts.AddAsync(trxAccount);
                #endregion


                // prepare data
                await InserAddressTypeIntoDB(customerVM);
                var addressTypeObj = string.IsNullOrEmpty(customerVM.Name) ? await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "individual") : await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "company");
                // Create customer (individual)
                customerObj = new Customers()
                {
                    FullName = string.IsNullOrEmpty(customerVM.Name) ? customerVM.FirstName + " " + customerVM.LastName : customerVM.Name,
                    OrganizationID = customerVM.OrganizationID, // have to be change later
                    IsPerson = string.IsNullOrEmpty(customerVM.Name) ? true : false,
                    HeadID = head.HeadID, // Added by Md. Rakib Hasan
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = customerVM.CreatedBy,
                    LIP = customerVM.LIP,
                    LMAC = customerVM.LMAC,
                };
                await _customersRepository.AddAsync(customerObj);
                returnName = customerObj.FullName;

                // Create address
                var addresses = new Addresses()
                {
                    FullAddress = customerVM.FullAddress,
                    Street = customerVM.Street,
                    City = customerVM.City,
                    State = customerVM.State,
                    Additionaladdress = customerVM.Additionaladdress,
                    PostalCode = customerVM.PostalCode,
                    CountryID = customerVM.CountryID,
                    Phone = customerVM.Phone,
                    OtherPhone = customerVM.OtherPhone,
                    Email = customerVM.Email,
                    Latitude = customerVM.Latitude,
                    Longitude = customerVM.Longitude,
                    FirstName = customerVM.FirstName,
                    LastName = customerVM.LastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = customerVM.CreatedBy,
                    LIP = customerVM.LIP,
                    LMAC = customerVM.LMAC,
                };
                await _addressesRepository.AddAsync(addresses);

                // Link customer with address
                var customerAddress = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    CustomerID = customerObj.CustomerID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = customerVM.CreatedBy,
                    LIP = customerVM.LIP,
                    LMAC = customerVM.LMAC,
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
        public async Task<ReturnView> CreateBranch(AddressVM branchVM)
        {
            // Begin transaction
            await _companyBranchesRepository.BeginTransactionAsync();

            try
            {
                await InserAddressTypeIntoDB(branchVM);
                // Get address type
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "branch");



                // Save branch
                var companyBranches = new CompanyBranches
                {
                    BranchName = branchVM.Name,
                    CustomerID = branchVM.CustomerID,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = branchVM.CreatedBy,
                    LIP = branchVM.LIP,
                    LMAC = branchVM.LMAC,
                };
                await _companyBranchesRepository.AddAsync(companyBranches);

                // Save address
                var addresses = new Addresses
                {
                    FullAddress = branchVM.FullAddress,
                    Street = branchVM.Street,
                    City = branchVM.City,
                    State = branchVM.State,
                    Additionaladdress = branchVM.Additionaladdress,
                    PostalCode = branchVM.PostalCode,
                    CountryID = branchVM.CountryID,
                    Phone = branchVM.Phone,
                    OtherPhone = branchVM.OtherPhone,
                    Email = branchVM.Email,
                    Latitude = branchVM.Latitude,
                    Longitude = branchVM.Longitude,
                    FirstName = branchVM.FirstName,
                    LastName = branchVM.LastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = branchVM.CreatedBy,
                    LIP = branchVM.LIP,
                    LMAC = branchVM.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = branchVM.UpdatedBy ?? null,
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
                    CreatedBy = branchVM.CreatedBy,
                    LIP = branchVM.LIP,
                    LMAC = branchVM.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = branchVM.UpdatedBy ?? null,
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
    }
}
