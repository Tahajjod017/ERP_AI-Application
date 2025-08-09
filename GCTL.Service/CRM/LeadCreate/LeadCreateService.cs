using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.LeadCreate
{
    public class LeadCreateService : ILeadCreateService
    {
        private readonly IGenericRepository<Customers> _customersRepository;

        public LeadCreateService(IGenericRepository<Customers> customersRepository)
        {
            _customersRepository = customersRepository;
        }

        //public Task<bool> AddAsync(CreateLeadVM model)
        //{
            
        //}

        public CommonReturnViewModel SaveLead(Customers customers)
        {
            //string country = customer.Country;

            try
            {
                
                Customers newCustomer = new Customers()
                {
                    //CustomerID = customer.CustomerID,
                    //FirstName = customer.FirstName,
                    //LastName = customer.LastName,
                    //FullAddress = customer.FullAddress,
                    //Street = customer.Street,
                    //City = customer.City,
                    //State = customer.State,
                    //Additionaladdress = customer.Additionaladdress,
                    //PostalCode = customer.PostalCode,
                    //CountryID = customer.CountryID,
                    //Latitude = customer.Latitude,
                    //Longitude = customer.Longitude,
                    //Phone = customer.Phone,
                    //OtherPhone = customer.OtherPhone,
                    //Email = customer.Email,
                    
                    
                };
                _customersRepository.AddAsync(newCustomer);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Data saved succesfull",
                    Data = newCustomer

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
