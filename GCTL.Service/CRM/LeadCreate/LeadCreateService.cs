using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;


namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Individuals> _individualsRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<IndividualAddresses> _individualAddressesRepository;

        public LeadCreateService(IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Individuals> individualsRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<IndividualAddresses> individualAddressesRepository)
        {
            _countryRepository = countryRepository;
            _individualsRepository = individualsRepository;
            _addressesRepository = addressesRepository;
            _addressTypesRepository = addressTypesRepository;
            _individualAddressesRepository = individualAddressesRepository;
        }

        public async Task<CommonReturnViewModel> SaveLead(CustomerVM customerVM)
        {
            try
            {
                Individuals individuals = new Individuals();
                var item = await _addressTypesRepository.GetAllAsync();

                if (!item.Any())
                {
                    List<AddressTypes> addressTypes =new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping" };
                    for (int i = 0; i< listImte.Length; i++)
                    {
                        addressTypes.Add(new AddressTypes()
                        {
                            AddressTypeName = listImte[i],

                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,

                        });
                    }
                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }

                foreach(var obj in customerVM.Customers)
                {
                    var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == obj.CountryName.ToLower());

                    int countryId;
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
                            CountryCode = obj.CountryCode,
                            CountryName = obj.CountryName,

                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = customerVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };

                        await _countryRepository.AddAsync(newCountry);
                        countryId = newCountry.CountryID;
                    }

                    string serverSiteAddressType = obj.TabName == "person" ? "billing" : obj.TabName;
                    var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == serverSiteAddressType);
                    
                    // individual
                    // only this condition will run when item type is billing

                    if (addressTypeObj.AddressTypeName == "billing")
                    {
                        individuals = new Individuals()
                        {
                            FirstName = obj.FirstName,
                            LastName = obj.LastName,

                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = customerVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };
                        await _individualsRepository.AddAsync(individuals);
                    }

                        // Address
                        Addresses addresses = new Addresses()
                        {
                            FullAddress = obj.FullAddress,
                            Street = obj.Street,
                            City = obj.City,
                            State = obj.State,
                            Additionaladdress = obj.Additionaladdress,
                            PostalCode = obj.PostalCode,
                            CountryID = countryId,
                            Phone = obj.Phone,
                            OtherPhone = obj.OtherPhone,
                            Email = obj.Email,
                            Latitude = obj.Latitude,
                            Longitude = obj.Longitude,
                            FirstName = addressTypeObj.AddressTypeName == "shipping" ? obj.FirstName : null,
                            LastName = addressTypeObj.AddressTypeName == "shipping" ? obj.LastName : null,
                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = customerVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };
                    await _addressesRepository.AddAsync(addresses);
                    
                    //get all table id
                    int addressesId = addresses.AddressID;

                    IndividualAddresses individualAddresses = new IndividualAddresses()
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = addressesId,
                        IndividualID = individuals.IndividualID,

                        CreatedAt = DateTime.Now,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,

                        UpdatedAt = DateTime.Now,
                        UpdatedBy = customerVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _individualAddressesRepository.AddAsync(individualAddresses);
                      
                }
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                    //    Data = newCustomer
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
        public async Task<CommonReturnViewModel> UpdateLead(CustomerVM customerVM)
        {
            try
            {
                Individuals individuals = new Individuals();
                var item = await _addressTypesRepository.GetAllAsync();

                if (!item.Any())
                {
                    List<AddressTypes> addressTypes = new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping" };
                    for (int i = 0; i < listImte.Length; i++)
                    {
                        addressTypes.Add(new AddressTypes()
                        {
                            AddressTypeName = listImte[i],

                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,

                        });
                    }
                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }

                foreach (var obj in customerVM.Customers)
                {
                    var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == obj.CountryName.ToLower());

                    int countryId;
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
                            CountryCode = obj.CountryCode,
                            CountryName = obj.CountryName,

                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = customerVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };

                        await _countryRepository.AddAsync(newCountry);
                        countryId = newCountry.CountryID;
                    }

                    string serverSiteAddressType = obj.TabName == "person" ? "billing" : obj.TabName;
                    var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == serverSiteAddressType);

                    // individual
                    // only this condition will run when item type is billing

                    if (addressTypeObj.AddressTypeName == "billing")
                    {
                        individuals = new Individuals()
                        {
                            FirstName = obj.FirstName,
                            LastName = obj.LastName,

                            CreatedAt = DateTime.Now,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            UpdatedAt = DateTime.Now,
                            UpdatedBy = customerVM.UpdatedBy ?? null,
                            DeletedAt = null,
                        };
                        await _individualsRepository.AddAsync(individuals);
                    }

                    // Address
                    Addresses addresses = new Addresses()
                    {
                        FullAddress = obj.FullAddress,
                        Street = obj.Street,
                        City = obj.City,
                        State = obj.State,
                        Additionaladdress = obj.Additionaladdress,
                        PostalCode = obj.PostalCode,
                        CountryID = countryId,
                        Phone = obj.Phone,
                        OtherPhone = obj.OtherPhone,
                        Email = obj.Email,
                        Latitude = obj.Latitude,
                        Longitude = obj.Longitude,
                        FirstName = addressTypeObj.AddressTypeName == "shipping" ? obj.FirstName : null,
                        LastName = addressTypeObj.AddressTypeName == "shipping" ? obj.LastName : null,
                        CreatedAt = DateTime.Now,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = customerVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _addressesRepository.AddAsync(addresses);

                    //get all table id
                    int addressesId = addresses.AddressID;

                    IndividualAddresses individualAddresses = new IndividualAddresses()
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = addressesId,
                        IndividualID = individuals.IndividualID,

                        CreatedAt = DateTime.Now,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,

                        UpdatedAt = DateTime.Now,
                        UpdatedBy = customerVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _individualAddressesRepository.AddAsync(individualAddresses);

                }
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                    //    Data = newCustomer
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
    }
}
