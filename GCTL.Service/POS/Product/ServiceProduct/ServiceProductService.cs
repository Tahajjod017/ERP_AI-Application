using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;

namespace GCTL.Service.POS.Product.ServiceProduct
{
    public class ServiceProductService : IServiceProduct
    {
        private readonly IGenericRepository<Products> _productRepository;

        public ServiceProductService(IGenericRepository<Products> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<CommonReturnViewModel> AddServiceAsync(ServiceViewModel model)
        {
            await _productRepository.BeginTransactionAsync();
            try
            {

                var product = new Products
                {
                    ProductName = model.ServiceSelectService,
                    ServiceDailyRate = model.ServiceDailyRate,
                    ServiceHourlyRate = model.ServiceHourlyRate,
                    ServicePerJobRate = model.ServicePerJobRate,
                    ServicePerMeterRate = model.ServicePerMeterRate,
                    ProductTypeID = 3,
                    SKU = DateTime.Now.ToString()
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
    }
}
