using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.MasterSetup.LeadSource;
using GCTL.Core.ViewModels.MasterSetup.LeadStatuses;
using GCTL.Core.ViewModels.MasterSetup.Priority;
using GCTL.Core.ViewModels.MasterSetup.ServiceType;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SkiaSharp;
using System.Security.Cryptography;


namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Customers> _customersRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<CustomerAddresses> _customerAddressesRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;
        private readonly IGenericRepository<CompanyWarehouses> _companyWarehousesRepository;
        private readonly IGenericRepository<CompanyWarehouseAddresses> _companyWarehouseAddressesRepository;
        private readonly IGenericRepository<CompanyBranches> _companyBranchesRepository;
        private readonly IGenericRepository<CompanyBranchAddresses> _companyBranchAddressesRepository;
        private readonly IGenericRepository<LeadServices> _leadServicesRepository;
        private readonly AppDbContext _context;

        public LeadCreateService(AppDbContext context, IGenericRepository<LeadServices> leadServicesRepository,  IGenericRepository<CompanyBranchAddresses> companyBranchAddressesRepository, IGenericRepository<CompanyBranches> companyBranchesRepository, IGenericRepository<CompanyWarehouseAddresses> companyWarehouseAddressesRepository,IGenericRepository<CompanyWarehouses> companyWarehousesRepository,IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<Leads> leadsRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository)
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
        }

        public async Task<ReturnView> CreatePerson(CustomerVM customerVM)
        {
            try
            {
                Customers customerObj = new Customers();
                var items = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                string returnName = "";

                if (!items.Any())
                {
                    List<AddressTypes> addressTypes = new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping", "company", "branch", "warehouse" };
                    for (int i = 0; i < listImte.Length; i++)
                    {
                        addressTypes.Add(new AddressTypes()
                        {
                            AddressTypeName = listImte[i],

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,

                        });
                    }
                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }
                Country countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == customerVM.CountryId);


                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "billing");

                // individual
                // only this condition will run when item type is billing

                customerObj = new Customers()
                    {
                        FullName = customerVM.FirstName + " " + customerVM.LastName,
                        IsPerson = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,
                    };
                    await _customersRepository.AddAsync(customerObj);
                    returnName = customerObj.FullName;

                // Address
                Addresses addresses = new Addresses()
                {
                    FullAddress = customerVM.FullAddress,
                    Street = customerVM.Street,
                    City = customerVM.City,
                    State = customerVM.State,
                    Additionaladdress = customerVM.Additionaladdress,
                    PostalCode = customerVM.PostalCode,
                    CountryID = countryObj?.CountryID,
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
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = customerVM.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _addressesRepository.AddAsync(addresses);

                //get all table id
                int addressesId = addresses.AddressID;

                CustomerAddresses customerAddress = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addressesId,
                    CustomerID = customerObj.CustomerID,

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = customerVM.CreatedBy,
                    LIP = customerVM.LIP,
                    LMAC = customerVM.LMAC,
                };
                await _customerAddressesRepository.AddAsync(customerAddress);
                returnID = customerAddress.CustomerAddressID;


                return new ReturnView
                {
                    Success = true,
                    Message = "Data saved succesfull",
                    Id = returnID,
                    Name = returnName
                };

            }
            catch (Exception ex)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

        }
        public async Task<ReturnView> CreateCompany(CompanyVM companyVM)
        {
            try
            {
                var items = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                string returnName = "";

                if (!items.Any())
                {
                    List<AddressTypes> addressTypes = new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping", "company", "branch", "warehouse" };
                    for (int i = 0; i < listImte.Length; i++)
                    {
                        addressTypes.Add(new AddressTypes()
                        {
                            AddressTypeName = listImte[i],

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = companyVM.CreatedBy,
                            LIP = companyVM.LIP,
                            LMAC = companyVM.LMAC,

                        });
                    }
                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }
                



                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "company");

                // individual
                // only this condition will run when item type is billing


                Country countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == companyVM.CountryID);
                // Address
                Addresses addresses = new Addresses()
                {
                    FullAddress = companyVM.FullAddress,
                    Street = companyVM.Street,
                    City = companyVM.City,
                    State = companyVM.State,
                    Additionaladdress = companyVM.Additionaladdress,
                    PostalCode = companyVM.PostalCode,
                    CountryID = countryObj?.CountryID,
                    Phone = companyVM.Phone,
                    OtherPhone = companyVM.OtherPhone,
                    Email = companyVM.Email,
                    Latitude = companyVM.Latitude,
                    Longitude = companyVM.Longitude,
                    FirstName = companyVM.FirstName,
                    LastName = companyVM.LastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = companyVM.CreatedBy,
                    LIP = companyVM.LIP,
                    LMAC = companyVM.LMAC,
                };
                await _addressesRepository.AddAsync(addresses);

                //get all table id
                int addressesId = addresses.AddressID;


                Customers customer = new Customers()
                {
                    FullName = companyVM.CompanyName,
                    IsPerson = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = companyVM.CreatedBy,
                    LIP = companyVM.LIP,
                    LMAC = companyVM.LMAC,

                };
                await _customersRepository.AddAsync(customer);

                CustomerAddresses companyAddressesObj = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    CustomerID = customer.CustomerID,

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = companyVM.CreatedBy,
                    LIP = companyVM.LIP,
                    LMAC = companyVM.LMAC,
                };

                await _customerAddressesRepository.AddAsync(companyAddressesObj);
                returnID = companyAddressesObj.CustomerAddressID;
                returnName = customer.FullName;

                return new ReturnView
                {
                    Success = true,
                    Message = "Data saved succesfull",
                    Id = returnID,
                    Name = returnName
                };

            }
            catch (Exception ex)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

        }
        public async Task<int> SaveAddressType(int? CreatedBy, string? LIP, string? LMAC)
        {
            var items = await _addressTypesRepository.GetAllAsync();
            if (!items.Any())
            {
                List<AddressTypes> addressTypes = new List<AddressTypes>();
                var listImte = new string[] { "billing", "shipping", "company", "branch", "warehouse" };
                for (int i = 0; i < listImte.Length; i++)
                {
                    addressTypes.Add(new AddressTypes()
                    {
                        AddressTypeName = listImte[i],

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = CreatedBy,
                        LIP = LIP,
                        LMAC = LMAC,

                    });
                }
                await _addressTypesRepository.AddRangeAsync(addressTypes);
            }
            return 0;
        }

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


        public async Task<CommonReturnViewModel> CreateShippingAddress(ShippingVM shippingVM)
        {
            try
            {
                await SaveAddressType(shippingVM.CreatedBy, shippingVM.LIP, shippingVM.LMAC);

                Country countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == shippingVM.CountryId);




                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");

                // individual
                var customerAddressObj = await _customerAddressesRepository.FirstOrDefaultAsync(u => u.CustomerAddressID == shippingVM.PersonID);


                Addresses addresses = new Addresses()
                {
                    FullAddress = shippingVM.FullAddress,
                    Street = shippingVM.Street,
                    City = shippingVM.City,
                    State = shippingVM.State,
                    Additionaladdress = shippingVM.Additionaladdress,
                    PostalCode = shippingVM.PostalCode,
                    CountryID = countryObj?.CountryID,
                    Phone = shippingVM.Phone,
                    OtherPhone = shippingVM.OtherPhone,
                    Email = shippingVM.Email,
                    Latitude = shippingVM.Latitude,
                    Longitude = shippingVM.Longitude,
                    FirstName = shippingVM.FirstName,
                    LastName = shippingVM.LastName,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = shippingVM.CreatedBy,
                    LIP = shippingVM.LIP,
                    LMAC = shippingVM.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = shippingVM.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _addressesRepository.AddAsync(addresses);

                //get all table id
                int addressesId = addresses.AddressID;

                CustomerAddresses individualAddresses = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addressesId,
                    CustomerID = customerAddressObj.CustomerID,

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = shippingVM.CreatedBy,
                    LIP = shippingVM.LIP,
                    LMAC = shippingVM.LMAC,

                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = shippingVM.UpdatedBy ?? null,
                    DeletedAt = null,
                };
                await _customerAddressesRepository.AddAsync(individualAddresses);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                };

            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,
                };
            }

        }
        public async Task<CommonReturnViewModel> CreateLead(LeadsVM leadsVM)
        {
            try
            {

                var individualAddressObj = await _customerAddressesRepository.FirstOrDefaultAsync(u => u.CustomerAddressID == leadsVM.CustomerId);

                Leads leadObj = new Leads()
                {
                    CustomerID = individualAddressObj.CustomerAddressID,
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

                if (leadsVM.ServiceTypeIds != null && leadsVM.ServiceTypeIds.Count > 0)
                {
                    List<LeadServices> services = leadsVM.ServiceTypeIds.Select(serviceId => new LeadServices
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

                //List<LeadServices> services = new()
                //{
                //    new LeadServices{LeadID = leadObj.LeadID, ServiceID = leadsVM.ServiceTypeIds[0]}
                //};

                //}
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                };

            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel    
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
        public async Task<CommonReturnViewModel> EditLead(LeadsVM leadsVM)
        {
            try
            {
                var individualAddressObj = await _customerAddressesRepository.FirstOrDefaultAsync(u => u.CustomerAddressID == leadsVM.CustomerId);
                var leadObj = await _leadsRepository.FirstOrDefaultAsync( u => u.LeadID == leadsVM.LeadID );

                leadObj.CustomerID = individualAddressObj.CustomerAddressID;
                leadObj.LeadName = leadsVM.LeadName;
                leadObj.IsIndividualCustomer = leadsVM.IsIndividualCustomer;
                leadObj.LeadStatusID = leadsVM.LeadStatusID;
                leadObj.LeadSourceID = leadsVM.LeadSourceID;
                leadObj.LeadOwnerID = leadsVM.LeadOwnerID;
                leadObj.PriorityID = leadsVM.PriorityID;
                leadObj.ApproximateDealValue = leadsVM.ApproximateDealValue;
                leadObj.ProbabilityPercentage = leadsVM.ProbabilityPercentage;
                leadObj.LeadDescription = leadsVM.LeadDescription;

                leadObj.UpdatedAt = DateTime.UtcNow;
                leadObj.UpdatedBy = leadsVM.CreatedBy;
                leadObj.LIP = leadsVM.LIP;
                leadObj.LMAC = leadsVM.LMAC;
       
                await _leadsRepository.UpdateAsync(leadObj);

                if (leadsVM.ServiceTypeIds != null && leadsVM.ServiceTypeIds.Count > 0)
                {
                    List<LeadServices> services = leadsVM.ServiceTypeIds.Select(serviceId => new LeadServices
                    {
                        LeadID = leadObj.LeadID,
                        ServiceID = serviceId,
                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = leadsVM.CreatedBy,
                        LIP = leadsVM.LIP,
                        LMAC = leadsVM.LMAC,
                    }).ToList();
                    await _leadServicesRepository.AddRangeAsync(services);
                }
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                };

            }
            catch (Exception ex)
            {
                return new CommonReturnViewModel    
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }



        public async Task<ReturnView> CreateBranch(BranchVM branchVM)
        {
            try
            {
                await SaveAddressType(branchVM.CreatedBy, branchVM.LIP, branchVM.LMAC);

                Country countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == branchVM.CountryID);


                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "branch");

                // individual

                var compnayAddressObj = await _customerAddressesRepository.AllActive().Include(c => c.Customer).FirstOrDefaultAsync(u => u.CustomerAddressID == branchVM.CompanyID);
                int returnID = 0;
                    string returnName = "";
                if (compnayAddressObj != null)
                {
                    returnName = compnayAddressObj.Customer.FullName;
                    returnID = compnayAddressObj.CustomerAddressID;

                    CompanyBranches companyBranches = new CompanyBranches
                    {
                        BranchName = branchVM.BranchName,
                        CustomerID = compnayAddressObj.CustomerID,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = branchVM.CreatedBy,
                        LIP = branchVM.LIP,
                        LMAC = branchVM.LMAC,
                    };
                    await _companyBranchesRepository.AddAsync(companyBranches);

                    Addresses addresses = new Addresses()
                    {
                        FullAddress = branchVM.FullAddress,
                        Street = branchVM.Street,
                        City = branchVM.City,
                        State = branchVM.State,
                        Additionaladdress = branchVM.Additionaladdress,
                        PostalCode = branchVM.PostalCode,
                        CountryID = countryObj?.CountryID,
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

                    //get all table id
                    int addressesId = addresses.AddressID;

                    CompanyBranchAddresses companyWarehouseAddress = new CompanyBranchAddresses()
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
                    await _companyBranchAddressesRepository.AddAsync(companyWarehouseAddress);
                    


                    return new ReturnView
                    {
                        Success = true,
                        Message = "Data saved succesfull",
                        Id = returnID,
                        Name = returnName
                    };
                }



                return new ReturnView
                {
                    Success = false,
                    Message = "Data not inserted",
                };


            }
            catch (Exception ex)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }
        public async Task<ReturnView> CreateWarehouse(WarehouseVM warehouseVM)
        {
            try
            {
                await SaveAddressType(warehouseVM.CreatedBy, warehouseVM.LIP, warehouseVM.LMAC);

                Country countryObj = await _countryRepository.FirstOrDefaultAsync(u => u.CountryID == warehouseVM.CountryID);
                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "warehouse");

                // individual
                var compnayAddressObj = await _customerAddressesRepository.AllActive().Include(c => c.Customer).FirstOrDefaultAsync(u => u.CustomerAddressID == warehouseVM.CompanyID);
                int returnID = 0;
                string returnName = "";

                if (compnayAddressObj != null) {

                    returnName = compnayAddressObj.Customer.FullName;
                    returnID = compnayAddressObj.CustomerAddressID;

                    CompanyWarehouses companyWarehouses = new CompanyWarehouses
                    {
                        WarehouseName = warehouseVM.WareHouseName,
                        CustomerID = compnayAddressObj.CustomerID,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = warehouseVM.CreatedBy,
                        LIP = warehouseVM.LIP,
                        LMAC = warehouseVM.LMAC,
                    };
                    await _companyWarehousesRepository.AddAsync(companyWarehouses);

                    Addresses addresses = new Addresses()
                    {
                        FullAddress = warehouseVM.FullAddress,
                        Street = warehouseVM.Street,
                        City = warehouseVM.City,
                        State = warehouseVM.State,
                        Additionaladdress = warehouseVM.Additionaladdress,
                        PostalCode = warehouseVM.PostalCode,
                        CountryID = countryObj?.CountryID,
                        Phone = warehouseVM.Phone,
                        OtherPhone = warehouseVM.OtherPhone,
                        Email = warehouseVM.Email,
                        Latitude = warehouseVM.Latitude,
                        Longitude = warehouseVM.Longitude,
                        FirstName = warehouseVM.FirstName,
                        LastName = warehouseVM.LastName,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = warehouseVM.CreatedBy,
                        LIP = warehouseVM.LIP,
                        LMAC = warehouseVM.LMAC,

                    };
                    await _addressesRepository.AddAsync(addresses);


                    CompanyWarehouseAddresses companyWarehouseAddress = new CompanyWarehouseAddresses()
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = addresses.AddressID,
                        WarehouseID = companyWarehouses.WarehouseID,

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = warehouseVM.CreatedBy,
                        LIP = warehouseVM.LIP,
                        LMAC = warehouseVM.LMAC,
                    };
                    await _companyWarehouseAddressesRepository.AddAsync(companyWarehouseAddress);

                    return new ReturnView
                    {
                        Success = true,
                        Message = "Data saved succesfull",
                        Id = returnID,
                        Name = returnName
                    };
                }
                return new ReturnView
                {
                    Success = false,
                    Message = "Data is not insert",
                };
            }
            catch (Exception ex)
            {
                return new ReturnView
                {
                    Success = false,
                    Message = ex.Message,
                };
            }
        }

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
    }
}
