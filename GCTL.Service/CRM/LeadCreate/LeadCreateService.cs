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

        //public Task<bool> AddAsync(CreateLeadVM model)
        //{
            
        //}

        public async Task<CommonReturnViewModel> SaveLead(CustomerVM customerVM)
        {
            try
            {
                var item = await _addressTypesRepository.GetAllAsync();
                if (!item.Any())
                {
                    List<AddressTypes> addressTypes =new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shiping" };
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
                        // 2. Create new country
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
                    // individual
                    Individuals individuals = new Individuals()
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

                        CreatedAt = DateTime.Now,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,

                        UpdatedAt = DateTime.Now,
                        UpdatedBy = customerVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _addressesRepository.AddAsync(addresses);
                    // AddressType
                    //AddressTypes addressTypes = new AddressTypes()
                    //{
                    //    AddressTypeName = addressType,

                    //    CreatedAt = DateTime.Now,
                    //    CreatedBy = customerVM.CreatedBy,
                    //    LIP = customerVM.LIP,
                    //    LMAC = customerVM.LMAC,

                    //    UpdatedAt = DateTime.Now,
                    //    UpdatedBy = customerVM.UpdatedBy ?? null,
                    //    DeletedAt = null,
                    //};
                    //await _addressTypesRepository.AddAsync(addressTypes);

                    //get all table id
                    int individualsId = individuals.IndividualID;
                    int addressesId = addresses.AddressID;
                    string serverSiteAddressType = obj.TabName == "person" ? "billing" : obj.TabName;

                    var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == serverSiteAddressType);

                    IndividualAddresses individualAddresses = new IndividualAddresses()
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = addressesId,
                        IndividualID = individualsId,

                        CreatedAt = DateTime.Now,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,

                        UpdatedAt = DateTime.Now,
                        UpdatedBy = customerVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _individualAddressesRepository.AddAsync(individualAddresses);
                    //IndividualAddress
                    // 
                    //Customers newCustomer = new Customers()
                    //{
                    //    CustomerID = customerVM.CustomerID,
                    //    FirstName = customerVM.FirstName,
                    //    LastName = customerVM.LastName,
                    //    FullAddress = customerVM.FullAddress,
                    //    Street = customerVM.Street,
                    //    City = customerVM.City,
                    //    State = customerVM.State,
                    //    Additionaladdress = customerVM.Additionaladdress,
                    //    PostalCode = customerVM.PostalCode,
                    //    CountryID = countryId,
                    //    Latitude = customerVM.Latitude,
                    //    Longitude = customerVM.Longitude,
                    //    Phone = customerVM.Phone,
                    //    OtherPhone = customerVM.OtherPhone,
                    //    Email = customerVM.Email,


                    //};
                    //await _customersRepository.AddAsync(newCustomer);

                    
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
