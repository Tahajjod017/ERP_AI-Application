using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;


namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        private readonly IGenericRepository<Country> _countryRepository;
        private readonly IGenericRepository<Individuals> _individualsRepository;
        private readonly IGenericRepository<Addresses> _addressesRepository;
        private readonly IGenericRepository<AddressTypes> _addressTypesRepository;
        private readonly IGenericRepository<IndividualAddresses> _individualAddressesRepository;
        private readonly IGenericRepository<Leads> _leadsRepository;

        public LeadCreateService(IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Individuals> individualsRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<IndividualAddresses> individualAddressesRepository, IGenericRepository<Leads> leadsRepository)
        {
            _countryRepository = countryRepository;
            _individualsRepository = individualsRepository;
            _addressesRepository = addressesRepository;
            _addressTypesRepository = addressTypesRepository;
            _individualAddressesRepository = individualAddressesRepository;
            _leadsRepository = leadsRepository;
        }

        public async Task<CommonReturnViewModel> CreateLead(CustomerVM customerVM)
        {
            try
            {
                Individuals individuals = new Individuals();
                var item = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                if (!item.Any())
                {
                    List<AddressTypes> addressTypes =new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping" };
                    for (int i = 0; i< listImte.Length; i++)
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

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
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

                    IndividualAddresses individualAddresses = new IndividualAddresses()
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = addressesId,
                        IndividualID = individuals.IndividualID,

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = customerVM.CreatedBy,
                        LIP = customerVM.LIP,
                        LMAC = customerVM.LMAC,

                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = customerVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _individualAddressesRepository.AddAsync(individualAddresses);
                    if (addressTypeObj.AddressTypeName == "billing")
                    {
                        returnID = individualAddresses.IndividualAddressID;
                    }
                    
                }
                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                    Data = returnID,
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
        public async Task<CommonReturnViewModel> UpdateLead(LeadsVM leadsVM)
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

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = leadsVM.CreatedBy,
                            LIP = leadsVM.LIP,
                            LMAC = leadsVM.LMAC,

                        });
                    }
                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }
                IndividualAddresses customerIndividualAddressObj = new IndividualAddresses();
                for (int customerObjCounter=0; customerObjCounter < leadsVM.Customers.Count; customerObjCounter++)
                {
                    customerIndividualAddressObj = await _individualAddressesRepository.FirstOrDefaultAsync(u => u.IndividualAddressID == leadsVM.Customers[customerObjCounter].PrimaryID);

                    if (customerIndividualAddressObj != null)
                    {
                        var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == leadsVM.Customers[customerObjCounter].CountryName);

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
                                CountryCode = leadsVM.Customers[0].CountryCode,
                                CountryName = leadsVM.Customers[0].CountryName,

                                LIP = leadsVM.LIP,
                                LMAC = leadsVM.LMAC,
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = leadsVM.UpdatedBy ?? null,
                                DeletedAt = null,
                            };

                            await _countryRepository.AddAsync(newCountry);
                            countryId = newCountry.CountryID;
                        }

                        var addressObj = await _addressesRepository.FirstOrDefaultAsync(u => u.AddressID == customerIndividualAddressObj.AddressID);
                        if (addressObj != null)
                        {
                            addressObj.FullAddress = leadsVM.Customers[customerObjCounter].FullAddress;
                            addressObj.Street = leadsVM.Customers[customerObjCounter].Street;
                            addressObj.City = leadsVM.Customers[customerObjCounter].City;
                            addressObj.State = leadsVM.Customers[customerObjCounter].Street;
                            addressObj.Additionaladdress = leadsVM.Customers[customerObjCounter].Additionaladdress;
                            addressObj.PostalCode = leadsVM.Customers[customerObjCounter].PostalCode;
                            addressObj.CountryID = countryId;
                            addressObj.Phone = leadsVM.Customers[customerObjCounter].Phone;
                            addressObj.OtherPhone = leadsVM.Customers[customerObjCounter].OtherPhone;
                            addressObj.Email = leadsVM.Customers[customerObjCounter].Email;
                            addressObj.Latitude = leadsVM.Customers[customerObjCounter].Latitude;
                            addressObj.Longitude = leadsVM.Customers[customerObjCounter].Longitude;

                            addressObj.FirstName = customerIndividualAddressObj.AddressType.AddressTypeName == "shipping" ? leadsVM.Customers[customerObjCounter].FirstName : null;
                            addressObj.LastName = customerIndividualAddressObj.AddressType.AddressTypeName == "shipping" ? leadsVM.Customers[customerObjCounter].LastName : null;

                            addressObj.LIP = leadsVM.LIP;
                            addressObj.LMAC = leadsVM.LMAC;
                            addressObj.UpdatedAt = DateTime.UtcNow;
                            addressObj.UpdatedBy = leadsVM.UpdatedBy ?? null;
                            addressObj.DeletedAt = null;
                        }
                        if (customerIndividualAddressObj.AddressType.AddressTypeName == "billing")
                        {
                            var individualsObj = await _individualsRepository.FirstOrDefaultAsync(u => u.IndividualID == customerIndividualAddressObj.IndividualID);
                            if (individualsObj != null)
                            {
                                individualsObj.FirstName = leadsVM.Customers[customerObjCounter].FirstName;
                                individualsObj.LastName = leadsVM.Customers[customerObjCounter].LastName;

                                individualsObj.LIP = leadsVM.LIP;
                                individualsObj.LMAC = leadsVM.LMAC;
                                individualsObj.UpdatedAt = DateTime.UtcNow;
                                individualsObj.UpdatedBy = leadsVM.UpdatedBy ?? null;
                                individualsObj.DeletedAt = null;
                            }
                        }

                       
                        

                    }
                    

                }
                Leads leadObj = new Leads()
                {
                    CustomerID = customerIndividualAddressObj.IndividualAddressID,
                    LeadName = leadsVM.LeadName,
                    IsIndividualCustomer = leadsVM.IsIndividualCustomer,
                    LeadStatusID = leadsVM.LeadStatusID,
                    LeadSourceID = leadsVM.LeadSourceID,
                    LeadOwnerID = leadsVM.LeadOwnerID,
                    ApproximateDealValue = leadsVM.ApproximateDealValue,
                    ProbabilityPercentage = leadsVM.ProbabilityPercentage,
                    LeadDescription = leadsVM.LeadDescription,

                    LIP = leadsVM.LIP,
                    LMAC = leadsVM.LMAC,
                    UpdatedAt = DateTime.UtcNow,
                    UpdatedBy = leadsVM.UpdatedBy ?? null,
                    DeletedAt = null,

                };
                await _leadsRepository.AddAsync(leadObj);


                //foreach (var obj in customerVM.Customers)
                //{
                //    var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == obj.CountryName.ToLower());

                //    int countryId;
                //    if (existingCountry != null)
                //    {
                //        // Country found
                //        countryId = existingCountry.CountryID;
                //    }
                //    else
                //    {
                //        // 2. new country creation
                //        var newCountry = new Country
                //        {
                //            CountryCode = obj.CountryCode,
                //            CountryName = obj.CountryName,

                //            CreatedAt = DateTime.Now,
                //            CreatedBy = customerVM.CreatedBy,
                //            LIP = customerVM.LIP,
                //            LMAC = customerVM.LMAC,
                //            UpdatedAt = DateTime.Now,
                //            UpdatedBy = customerVM.UpdatedBy ?? null,
                //            DeletedAt = null,
                //        };

                //        await _countryRepository.AddAsync(newCountry);
                //        countryId = newCountry.CountryID;
                //    }




                //}
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
