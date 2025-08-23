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

        public async Task<ReturnView> CreatePerson(CustomerVM customerVM)
        {
            try
            {
                Individuals individuals = new Individuals();
                var items = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                string returnName = "";
                int countryId = 0;

                if (!items.Any())
                {
                    List<AddressTypes> addressTypes = new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping" };
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

                    if (addressTypeObj.AddressTypeName == "billing")
                    {
                        individuals = new Individuals()
                        {
                            FirstName = customerVM.FirstName,
                            LastName = customerVM.LastName,

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = customerVM.CreatedBy,
                            LIP = customerVM.LIP,
                            LMAC = customerVM.LMAC,
                            DeletedAt = null,
                        };
                        await _individualsRepository.AddAsync(individuals);
                        returnName = individuals.FirstName + " " + individuals.LastName;  
                }

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
                    returnID = individualAddresses.IndividualAddressID;


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
        public async Task<CommonReturnViewModel> CreateShippingAddress(ShippingVM shippingVM)
        {
            try
            {
                var items = await _addressTypesRepository.GetAllAsync();
                int returnID = 0;
                int countryId = 0;

                if (!items.Any())
                {
                    List<AddressTypes> addressTypes = new List<AddressTypes>();
                    var listImte = new string[] { "billing", "shipping" };
                    for (int i = 0; i < listImte.Length; i++)
                    {
                        addressTypes.Add(new AddressTypes()
                        {
                            AddressTypeName = listImte[i],

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = shippingVM.CreatedBy,
                            LIP = shippingVM.LIP,
                            LMAC = shippingVM.LMAC,

                        });
                    }
                    await _addressTypesRepository.AddRangeAsync(addressTypes);
                }
                    if (shippingVM.CountryName != null)
                    {
                        var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == shippingVM.CountryName.ToLower());

                        
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
                                CountryCode = shippingVM.CountryCode,
                                CountryName = shippingVM.CountryName,

                                CreatedAt = DateTime.UtcNow,
                                CreatedBy = shippingVM.CreatedBy,
                                LIP = shippingVM.LIP,
                                LMAC = shippingVM.LMAC,
                                UpdatedAt = DateTime.UtcNow,
                                UpdatedBy = shippingVM.UpdatedBy ?? null,
                                DeletedAt = null,
                            };

                            await _countryRepository.AddAsync(newCountry);
                            countryId = newCountry.CountryID;
                        }
                    }
                    

             
                    var addressTypeObj = await _addressTypesRepository.FirstOrDefaultAsync(u => u.AddressTypeName == "shipping");

                // individual
                    var individualAddressObj = await _individualAddressesRepository.FirstOrDefaultAsync(u => u.IndividualAddressID == shippingVM.CustomerID);


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

                    IndividualAddresses individualAddresses = new IndividualAddresses()
                    {
                        AddressTypeID = addressTypeObj.AddressTypeID,
                        AddressID = addressesId,
                        IndividualID = individualAddressObj.IndividualID,

                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = shippingVM.CreatedBy,
                        LIP = shippingVM.LIP,
                        LMAC = shippingVM.LMAC,

                        UpdatedAt = DateTime.UtcNow,
                        UpdatedBy = shippingVM.UpdatedBy ?? null,
                        DeletedAt = null,
                    };
                    await _individualAddressesRepository.AddAsync(individualAddresses);
                
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
        public async Task<CommonReturnViewModel> UpdateLead(LeadsVM leadsVM)
        {
            //try
            //{
            //    Individuals individuals = new Individuals();
            //    var item = await _addressTypesRepository.GetAllAsync();

            //    if (!item.Any())
            //    {
            //        List<AddressTypes> addressTypes = new List<AddressTypes>();
            //        var listImte = new string[] { "billing", "shipping" };
            //        for (int i = 0; i < listImte.Length; i++)
            //        {
            //            addressTypes.Add(new AddressTypes()
            //            {
            //                AddressTypeName = listImte[i],

            //                CreatedAt = DateTime.UtcNow,
            //                CreatedBy = leadsVM.CreatedBy,
            //                LIP = leadsVM.LIP,
            //                LMAC = leadsVM.LMAC,

            //            });
            //        }
            //        await _addressTypesRepository.AddRangeAsync(addressTypes);
            //    }
            //    IndividualAddresses customerIndividualAddressObj = new IndividualAddresses();
            //    for (int customerObjCounter=0; customerObjCounter < leadsVM.Customers.Count; customerObjCounter++)
            //    {
            //        customerIndividualAddressObj = await _individualAddressesRepository.FirstOrDefaultAsync(u => u.IndividualAddressID == leadsVM.Customers[customerObjCounter].PrimaryID);

            //        if (customerIndividualAddressObj != null)
            //        {
            //            var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == leadsVM.Customers[customerObjCounter].CountryName);

            //            int countryId;
            //            if (existingCountry != null)
            //            {
            //                // Country found
            //                countryId = existingCountry.CountryID;
            //            }
            //            else
            //            {
            //                // 2. new country creation
            //                var newCountry = new Country
            //                {
            //                    CountryCode = leadsVM.Customers[0].CountryCode,
            //                    CountryName = leadsVM.Customers[0].CountryName,

            //                    LIP = leadsVM.LIP,
            //                    LMAC = leadsVM.LMAC,
            //                    UpdatedAt = DateTime.UtcNow,
            //                    UpdatedBy = leadsVM.UpdatedBy ?? null,
            //                    DeletedAt = null,
            //                };

            //                await _countryRepository.AddAsync(newCountry);
            //                countryId = newCountry.CountryID;
            //            }

            //            var addressObj = await _addressesRepository.FirstOrDefaultAsync(u => u.AddressID == customerIndividualAddressObj.AddressID);
            //            if (addressObj != null)
            //            {
            //                addressObj.FullAddress = leadsVM.Customers[customerObjCounter].FullAddress;
            //                addressObj.Street = leadsVM.Customers[customerObjCounter].Street;
            //                addressObj.City = leadsVM.Customers[customerObjCounter].City;
            //                addressObj.State = leadsVM.Customers[customerObjCounter].Street;
            //                addressObj.Additionaladdress = leadsVM.Customers[customerObjCounter].Additionaladdress;
            //                addressObj.PostalCode = leadsVM.Customers[customerObjCounter].PostalCode;
            //                addressObj.CountryID = countryId;
            //                addressObj.Phone = leadsVM.Customers[customerObjCounter].Phone;
            //                addressObj.OtherPhone = leadsVM.Customers[customerObjCounter].OtherPhone;
            //                addressObj.Email = leadsVM.Customers[customerObjCounter].Email;
            //                addressObj.Latitude = leadsVM.Customers[customerObjCounter].Latitude;
            //                addressObj.Longitude = leadsVM.Customers[customerObjCounter].Longitude;

            //                addressObj.FirstName = customerIndividualAddressObj.AddressType.AddressTypeName == "shipping" ? leadsVM.Customers[customerObjCounter].FirstName : null;
            //                addressObj.LastName = customerIndividualAddressObj.AddressType.AddressTypeName == "shipping" ? leadsVM.Customers[customerObjCounter].LastName : null;

            //                addressObj.LIP = leadsVM.LIP;
            //                addressObj.LMAC = leadsVM.LMAC;
            //                addressObj.UpdatedAt = DateTime.UtcNow;
            //                addressObj.UpdatedBy = leadsVM.UpdatedBy ?? null;
            //                addressObj.DeletedAt = null;
            //            }
            //            if (customerIndividualAddressObj.AddressType.AddressTypeName == "billing")
            //            {
            //                var individualsObj = await _individualsRepository.FirstOrDefaultAsync(u => u.IndividualID == customerIndividualAddressObj.IndividualID);
            //                if (individualsObj != null)
            //                {
            //                    individualsObj.FirstName = leadsVM.Customers[customerObjCounter].FirstName;
            //                    individualsObj.LastName = leadsVM.Customers[customerObjCounter].LastName;

            //                    individualsObj.LIP = leadsVM.LIP;
            //                    individualsObj.LMAC = leadsVM.LMAC;
            //                    individualsObj.UpdatedAt = DateTime.UtcNow;
            //                    individualsObj.UpdatedBy = leadsVM.UpdatedBy ?? null;
            //                    individualsObj.DeletedAt = null;
            //                }
            //            }




            //        }


            //    }
            //    Leads leadObj = new Leads()
            //    {
            //        CustomerID = customerIndividualAddressObj.IndividualAddressID,
            //        LeadName = leadsVM.LeadName,
            //        IsIndividualCustomer = leadsVM.IsIndividualCustomer,
            //        LeadStatusID = leadsVM.LeadStatusID,
            //        LeadSourceID = leadsVM.LeadSourceID,
            //        LeadOwnerID = leadsVM.LeadOwnerID,
            //        ApproximateDealValue = leadsVM.ApproximateDealValue,
            //        ProbabilityPercentage = leadsVM.ProbabilityPercentage,
            //        LeadDescription = leadsVM.LeadDescription,

            //        LIP = leadsVM.LIP,
            //        LMAC = leadsVM.LMAC,
            //        UpdatedAt = DateTime.UtcNow,
            //        UpdatedBy = leadsVM.UpdatedBy ?? null,
            //        DeletedAt = null,

            //    };
            //    await _leadsRepository.AddAsync(leadObj);


            //    //foreach (var obj in customerVM.Customers)
            //    //{
            //    //    var existingCountry = await _countryRepository.FirstOrDefaultAsync(c => c.CountryName.ToLower() == obj.CountryName.ToLower());

            //    //    int countryId;
            //    //    if (existingCountry != null)
            //    //    {
            //    //        // Country found
            //    //        countryId = existingCountry.CountryID;
            //    //    }
            //    //    else
            //    //    {
            //    //        // 2. new country creation
            //    //        var newCountry = new Country
            //    //        {
            //    //            CountryCode = obj.CountryCode,
            //    //            CountryName = obj.CountryName,

            //    //            CreatedAt = DateTime.Now,
            //    //            CreatedBy = customerVM.CreatedBy,
            //    //            LIP = customerVM.LIP,
            //    //            LMAC = customerVM.LMAC,
            //    //            UpdatedAt = DateTime.Now,
            //    //            UpdatedBy = customerVM.UpdatedBy ?? null,
            //    //            DeletedAt = null,
            //    //        };

            //    //        await _countryRepository.AddAsync(newCountry);
            //    //        countryId = newCountry.CountryID;
            //    //    }




            //    //}
            //    return new CommonReturnViewModel
            //    {
            //        Success = true,
            //        Message = "Data saved succesfull",
            //        //    Data = newCustomer
            //    };

            //}
            //catch (Exception ex)
            //{
            //    return new CommonReturnViewModel
            //    {
            //        Success = false,
            //        Message = ex.Message,
            //    };
            //}

            return new CommonReturnViewModel
            {
                Success = false,
            };
        }
    }
}
