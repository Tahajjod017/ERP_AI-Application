using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GCTL.Service.POS.Product.ServiceProduct
{
    public class ServiceProductService : IServiceProduct
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductTypes> _productTypeRepository;

        public ServiceProductService(IGenericRepository<Products> productRepository, IGenericRepository<ProductTypes> productTypeRepository)
        {
            _productRepository = productRepository;
            _productTypeRepository = productTypeRepository;
        }

        public async Task<CommonReturnViewModel> AddServiceAsync(ServiceViewModel model)
        {
            await _productRepository.BeginTransactionAsync();
            try
            {

                var cat = await _productTypeRepository.AllActive().Where(e => e.ProductTypeName.ToLower() == "service").FirstOrDefaultAsync();

                if (cat == null)
                {
                    cat = new ProductTypes()
                    {
                        ProductTypeName = "Service"

                    };
                    await _productTypeRepository.AddAsync(cat, model);
                }


                var product = new Products
                {
                    ProductName = model.ServiceSelectService,
                    ServiceDailyRate = model.ServiceDailyRate,
                    ServiceHourlyRate = model.ServiceHourlyRate,
                    ServicePerJobRate = model.ServicePerJobRate,
                    ServicePerMeterRate = model.ServicePerMeterRate,
                    ProductTypeID = cat.ProductTypeID,
                    SKU = await GetServiceSku()
                };

                await _productRepository.AddAsync(product , model);


                await _productRepository.CommitTransactionAsync();
                return new CommonReturnViewModel()
                {
                    Success = true,
                    Message = "saved success"
                };

            }
            catch (Exception ex)
            {

                await _productRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel() { 
                    Success = false,
                    Message = ex.Message
                };

            }
        }



        public async Task<string> GetServiceSku()
        {
            var cat = await _productTypeRepository.AllActive().Where(e => e.ProductTypeName.ToLower() == "service").FirstOrDefaultAsync();
            string newSku;
            if (cat != null)
            {
                var lastSku = await _productRepository.AllActive().Where(p => p.ProductCategoryID == cat.ProductTypeID)
                              .OrderByDescending(s => s.ProductID) 
                              .Select(p => p.SKU)
                              .FirstOrDefaultAsync();


                if (string.IsNullOrEmpty(lastSku))
                {
                    // If no SKU exists, start from 0002
                    newSku = "Ser" + 1.ToString("D4");
                }
                else
                {
                    // Extract last 4 digits
                    var last4 = lastSku.Substring(lastSku.Length - 4);

                    if (int.TryParse(last4, out int number))
                    {
                        number++; // increment
                        newSku = "Ser" + number.ToString("D4");
                    }
                    else
                    {
                        // fallback: reset to 0002
                        newSku = "Ser" + 1.ToString("D4");
                    }
                }
            }
            else
            {
                newSku = "Ser" + 1.ToString("D4");
            }


            return newSku;
            


        }


    }
}
