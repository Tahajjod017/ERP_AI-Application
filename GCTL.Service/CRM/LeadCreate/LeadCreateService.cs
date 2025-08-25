using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NetTopologySuite.Index.HPRtree;
using System.ComponentModel;
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

        public LeadCreateService(IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<Leads> leadsRepository, IGenericRepository<CustomerAddresses> customerAddressesRepository)
        {
            _countryRepository = countryRepository;
            _addressesRepository = addressesRepository;
            _addressTypesRepository = addressTypesRepository;
            _leadsRepository = leadsRepository;
            _customersRepository = customersRepository;
            _customerAddressesRepository = customerAddressesRepository;
        }

        public async Task<ReturnView> CreatePerson(CustomerVM customerVM)
        {
            try
            {
                Customers customerObj = new Customers();
                var items = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                string returnName = "";
                int countryId = 0;

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
                if (customerVM.CountryName != null)
                {
                    var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == customerVM.CountryName.ToLower());


                    if (existingCountry != null)
                    {
                        // Country found
                        countryId = existingCountry.CountryID;
                    }
                    else
                    {
                        // 2. new country creation
                        var newCountry = new Country
                        {
                            CountryCode = customerVM.CountryCode,
                            CountryName = customerVM.CountryName,

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = customerVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };

                        await _countryRepository.AddAsync(newCountry);
                        countryId = newCountry.CountryID;
                    }
                }



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
                        DeletedAt = null,
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
                    CountryID = countryId,
                    Phone = customerVM.Phone,
                    OtherPhone = customerVM.OtherPhone,
                    Email = customerVM.Email,
                    Latitude = customerVM.Latitude,
                    Longitude = customerVM.Longitude,
                    FirstName = null,
                    LastName = null,
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
                int countryId = 0;

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
                if (companyVM.CountryName != null)
                {
                    var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == companyVM.CountryName.ToLower());


                    if (existingCountry != null)
                    {
                        // Country found
                        countryId = existingCountry.CountryID;
                    }
                    else
                    {
                        // 2. new country creation
                        var newCountry = new Country
                        {
                            CountryCode = companyVM.CountryCode,
                            CountryName = companyVM.CountryName,

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = companyVM.CreatedBy,
                            LIP = companyVM.LIP,
                            LMAC = companyVM.LMAC,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = companyVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };

                        await _countryRepository.AddAsync(newCountry);
                        countryId = newCountry.CountryID;
                    }
                }



                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "company");

                // individual
                // only this condition will run when item type is billing



                // Address
                Addresses addresses = new Addresses()
                {
                    FullAddress = companyVM.FullAddress,
                    Street = companyVM.Street,
                    City = companyVM.City,
                    State = companyVM.State,
                    Additionaladdress = companyVM.Additionaladdress,
                    PostalCode = companyVM.PostalCode,
                    CountryID = countryId,
                    Phone = companyVM.Phone,
                    OtherPhone = companyVM.OtherPhone,
                    Email = companyVM.Email,
                    Latitude = companyVM.Latitude,
                    Longitude = companyVM.Longitude,
                    FirstName = null,
                    LastName = null,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = companyVM.CreatedBy,
                    LIP = companyVM.LIP,
                    LMAC = companyVM.LMAC,
                };
                await _addressesRepository.AddAsync(addresses);

                //get all table id
                int addressesId = addresses.AddressID;


                Customers companyObj = new Customers()
                {
                    FullName = companyVM.CompanyName,
                    IsPerson = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = companyVM.CreatedBy,
                    LIP = companyVM.LIP,
                    LMAC = companyVM.LMAC,

                };
                await _customersRepository.AddAsync(companyObj);

                CustomerAddresses companyAddressesObj = new CustomerAddresses()
                {
                    AddressTypeID = addressTypeObj.AddressTypeID,
                    AddressID = addresses.AddressID,
                    CustomerID = companyObj.CustomerID,

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = companyVM.CreatedBy,
                    LIP = companyVM.LIP,
                    LMAC = companyVM.LMAC,
                };

                await _customerAddressesRepository.AddAsync(companyAddressesObj);
                returnID = companyAddressesObj.CustomerAddressID;
                returnName = companyObj.FullName;

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

        private async Task<int> saveCountry(string? countryCode, string? coutryName, int? CreatedBy, string? LIP, string? LMAC)
        {
            int countryId = 0;
            if (coutryName != null)
            {
                var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == coutryName.ToLower());


                if (existingCountry != null)
                {
                    // Country found
                    countryId = existingCountry.CountryID;
                }
                else
                {
                    // 2. new country creation
                    var newCountry = new Country
                    {
                        CountryCode = countryCode,
                        CountryName = coutryName,

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = CreatedBy,
                        LIP = LIP,
                        LMAC = LMAC,
                  
                    };

                    await _countryRepository.AddAsync(newCountry);
                    countryId = newCountry.CountryID;
                }
            }
            return countryId;
        }
        public async Task<CommonReturnViewModel> CreateShippingAddress(ShippingVM shippingVM)
        {
            try
            {
                await SaveAddressType(shippingVM.CreatedBy, shippingVM.LIP, shippingVM.LMAC);
                int countryId = 0;

                countryId = await saveCountry(shippingVM.CountryCode, shippingVM.CountryName, shippingVM.CreatedBy, shippingVM.LIP, shippingVM.LMAC);




                var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");

                // individual
                var individualAddressObj = await _customerAddressesRepository.FirstOrDefaultAsync(u => u.CustomerAddressID == shippingVM.CustomerID);


                Addresses addresses = new Addresses()
                {
                    FullAddress = shippingVM.FullAddress,
                    Street = shippingVM.Street,
                    City = shippingVM.City,
                    State = shippingVM.State,
                    Additionaladdress = shippingVM.Additionaladdress,
                    PostalCode = shippingVM.PostalCode,
                    CountryID = countryId,
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
                    CustomerID = individualAddressObj.CustomerID,

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



        Task<ReturnView> ILeadCreateService.CreateBranch(BranchVM branchVM)
        {
            return Task.FromResult(new ReturnView
            {
                Success = false,
                Message = "Data saved succesfull",
            });
        }
    }
}
