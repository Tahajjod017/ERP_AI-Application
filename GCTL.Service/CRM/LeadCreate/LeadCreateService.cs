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

        public LeadCreateService(IGenericRepository<Customers> customersRepository, IGenericRepository<Country> countryRepository, IGenericRepository<Individuals> individualsRepository, IGenericRepository<Addresses> addressesRepository, IGenericRepository<AddressTypes> addressTypesRepository, IGenericRepository<IndividualAddresses> individualAddressesRepository)
        {
            _countryRepository = countryRepository;
            _individualsRepository = individualsRepository;
            _addressesRepository = addressesRepository;
            _addressTypesRepository = addressTypesRepository;
            _individualAddressesRepository = individualAddressesRepository;
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
                for (int customerObjCounter=0; customerObjCounter < customerVM.Customers.Count; customerObjCounter++)
                {
                    var customerIndividualAddressObj = await _individualAddressesRepository.FirstOrDefaultAsync(u => u.IndividualAddressID == customerVM.Customers[customerObjCounter].PrimaryID);

                    if (customerIndividualAddressObj != null)
                    {
                        var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == customerVM.Customers[customerObjCounter].CountryName);

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
                                CountryCode = customerVM.Customers[0].CountryCode,
                                CountryName = customerVM.Customers[0].CountryName,

                                LIP = customerVM.LIP,
                                LMAC = customerVM.LMAC,
                                UpdatedAt = DateTime.Now,
                                UpdatedBy = customerVM.UpdatedBy ?? null,
                                DeletedAt = null,
                            };

                            await _countryRepository.AddAsync(newCountry);
                            countryId = newCountry.CountryID;
                        }

                        var addressObj = await _addressesRepository.FirstOrDefaultAsync(u => u.AddressID == customerIndividualAddressObj.AddressID);
                        if (addressObj != null)
                        {
                            addressObj.FullAddress = customerVM.Customers[customerObjCounter].FullAddress;
                            addressObj.Street = customerVM.Customers[customerObjCounter].Street;
                            addressObj.City = customerVM.Customers[customerObjCounter].City;
                            addressObj.State = customerVM.Customers[customerObjCounter].Street;
                            addressObj.Additionaladdress = customerVM.Customers[customerObjCounter].Additionaladdress;
                            addressObj.PostalCode = customerVM.Customers[customerObjCounter].PostalCode;
                            addressObj.CountryID = countryId;
                            addressObj.Phone = customerVM.Customers[customerObjCounter].Phone;
                            addressObj.OtherPhone = customerVM.Customers[customerObjCounter].OtherPhone;
                            addressObj.Email = customerVM.Customers[customerObjCounter].Email;
                            addressObj.Latitude = customerVM.Customers[customerObjCounter].Latitude;
                            addressObj.Longitude = customerVM.Customers[customerObjCounter].Longitude;

                            addressObj.FirstName = customerIndividualAddressObj.AddressType.AddressTypeName == "shipping" ? customerVM.Customers[customerObjCounter].FirstName : null;
                            addressObj.LastName = customerIndividualAddressObj.AddressType.AddressTypeName == "shipping" ? customerVM.Customers[customerObjCounter].LastName : null;

                            addressObj.LIP = customerVM.LIP;
                            addressObj.LMAC = customerVM.LMAC;
                            addressObj.UpdatedAt = DateTime.Now;
                            addressObj.UpdatedBy = customerVM.UpdatedBy ?? null;
                            addressObj.DeletedAt = null;
                        }
                        if (customerIndividualAddressObj.AddressType.AddressTypeName == "billing")
                        {
                            var individualsObj = await _individualsRepository.FirstOrDefaultAsync(u => u.IndividualID == customerIndividualAddressObj.IndividualID);
                            if (individualsObj != null)
                            {
                                individualsObj.FirstName = customerVM.Customers[customerObjCounter].FirstName;
                                individualsObj.LastName = customerVM.Customers[customerObjCounter].LastName;

                                individualsObj.LIP = customerVM.LIP;
                                individualsObj.LMAC = customerVM.LMAC;
                                individualsObj.UpdatedAt = DateTime.Now;
                                individualsObj.UpdatedBy = customerVM.UpdatedBy ?? null;
                                individualsObj.DeletedAt = null;
                            }
                        }
                        

                    }
                    
                }

                


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
