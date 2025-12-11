using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Purchasess.Purchase.PurchaseReceived;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GCTL.Service.POS.Purchasess.PurchaseReceived
{
    public class PurchaseReceivedService : IPurchaseReceivedService
    {
        private readonly IGenericRepository<PurchaseReceives> _purchaseReceiveRepository;
        private readonly IGenericRepository<PurchaseReceiveItems> _purchaseReceiveItemRepository;
        private readonly IGenericRepository<Products> _productsRepository;
        private readonly IGenericRepository<PurchasOrders> _purchaseOrderRepository;
        private readonly IGenericRepository<PurchasOrderItems> _purchaseOrderItemRepository;

        private readonly IGenericRepository<Locations> _locationRepository;
       // private readonly IGenericRepository<Projects> _projectRepository;
        private readonly IGenericRepository<Wirehouses> _warehouseRepository;
       // private readonly IGenericRepository<ProductDistributions> _distributionRepository;
       // private readonly IGenericRepository<ProductDistributionDetails> _distributionDetailsRepository;
        private readonly IUserInfoService _userInfoService;


        public PurchaseReceivedService(IGenericRepository<PurchaseReceives> purchaseReceiveRepository, 
            IGenericRepository<Products> productsRepository, 
            IGenericRepository<PurchaseReceiveItems> purchaseReceiveItemRepository, 
            IUserInfoService userInfoService, 
            IGenericRepository<Locations> locationRepository, 
            //IGenericRepository<Projects> projectRepository, 
            IGenericRepository<Wirehouses> warehouseRepository, 
           // IGenericRepository<ProductDistributions> distributionRepository, 
            //IGenericRepository<ProductDistributionDetails> distributionDetailsRepository, 
            IGenericRepository<PurchasOrders> purchaseOrderRepository, 
            IGenericRepository<PurchasOrderItems> purchaseOrderItemRepository)
        {
            _purchaseReceiveRepository = purchaseReceiveRepository;
            _productsRepository = productsRepository;
            _purchaseReceiveItemRepository = purchaseReceiveItemRepository;
            _userInfoService = userInfoService;
            _locationRepository = locationRepository;
           // _projectRepository = projectRepository;
            _warehouseRepository = warehouseRepository;
            //_distributionRepository = distributionRepository;
            //_distributionDetailsRepository = distributionDetailsRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _purchaseOrderItemRepository = purchaseOrderItemRepository;
        }

        public async Task<CommonReturnViewModel> SavePurchaseReceivedAsync(PurchaseReceivedViewModel model)
        {
            await _purchaseReceiveRepository.BeginTransactionAsync();

            try
            {
                if (model.Items == null || !model.Items.Any())
                {
                    await _purchaseReceiveRepository.RollbackTransactionAsync();
                    return new CommonReturnViewModel
                    {
                        Success = false,
                        Message = "At least one item is required"
                    };
                }


                

                await _purchaseReceiveRepository.CommitTransactionAsync();



                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Purchase Received saved successfully",
                   // PurchaseReceivedId = purchaseReceived.PurchaseReceivedID
                };
            }
            catch (Exception ex)
            {
                await _purchaseReceiveRepository.RollbackTransactionAsync();

                Console.Error.WriteLine($"Error in SavePurchaseReceivedAsync: {ex.Message}");
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = "An error occurred while saving"
                };
            }
        }
    }
}
