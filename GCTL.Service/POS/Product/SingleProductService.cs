using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;
using Microsoft.AspNetCore.Http;
using static Dapper.SqlMapper;

namespace GCTL.Service.POS.Product
{
    public class SingleProductService : ISingleProduct
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductImages> _productImageRepository;
        private readonly IGenericRepository<ProductPricing> _productPricingRepository;
        private readonly IGenericRepository<ProductCustomFields> _productCustomFieldsRepository;
        private readonly IGenericRepository<ProductAdvancedPricing> _productAdvPriceRepository;
        private readonly IUserInfoService _userInfoService;

        public SingleProductService(IGenericRepository<Products> productRepository, IGenericRepository<ProductImages> productImageRepository, IGenericRepository<ProductPricing> productPricingRepository, IGenericRepository<ProductCustomFields> productCustomFieldsRepository, IGenericRepository<ProductAdvancedPricing> productAdvPriceRepository, IUserInfoService userInfoService)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
            _productPricingRepository = productPricingRepository;
            _productCustomFieldsRepository = productCustomFieldsRepository;
            _productAdvPriceRepository = productAdvPriceRepository;
            _userInfoService = userInfoService;
        }

        public async Task<CommonReturnViewModel> AddProductAsync(SingleProductViewModel model)
        {

            if (model.ProductImages?.Any() == false)
            {
                return new CommonReturnViewModel { Success = false, Message = "Image is Require, Please add least one Image", };
            }

            await _productImageRepository.BeginTransactionAsync();
            try
            {
                var product = new Products
                {
                    ProductTypeID = int.TryParse(model.ProductType, out var typeId) ? typeId : null,
                    ProductName = model.ProductName,
                    SKU = GetNextSKU().Result,
                    ProductCategoryID = int.TryParse(model.Category, out var catId) ? catId : null,
                    ProductSubCategoryID = int.TryParse(model.SubCategory, out var subCatId) ? subCatId : null,
                    ProductBrandID = int.TryParse(model.Brand, out var brandId) ? brandId : null,
                    ProductMode = model.ModelNo,
                    ProductPartNo = model.PartNo,
                    BarcodeGenerateFromID = int.TryParse(model.BarcodeType, out var barcodeTypeId) ? barcodeTypeId : null,
                    Barcode = model.Barcode,
                    Weight = decimal.TryParse(model.Weight, out var weight) ? weight : null,
                    UnitTypeID = int.TryParse(model.Unit, out var unitId) ? unitId : null,
                    AssetsTypeID = int.TryParse(model.AssetsType, out var assetId) ? assetId : null,
                    QuantityAlert = model.QuantityAlert,
                    IsTrackBatch = model.TrackBatch,
                    IsActive = model.IsActive,
                    IsTraceManufacturingDate = model.TraceManufacturingDate,
                    IsTraceExpiryDate = model.TraceExpiryDate,
                    HasSerialNumber = model.SerialNumber,
                    Description = model.Description,
                    CreatedAt = DateTime.UtcNow
                };

                

                if (model.ProductImages?.Any() == true)
                {
                    foreach (var imageFile in model.ProductImages)
                    {
                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

                        // Save each version
                        var thumbnailPath = await SaveImageAsync(imageFile, "thumbnail", fileName);
                        var smallPath = await SaveImageAsync(imageFile, "small", fileName);
                        var largePath = await SaveImageAsync(imageFile, "large", fileName);

                        var imageEntity = new ProductImages
                        {
                            ProductID = product.ProductID,
                            ThumbnailImagePath = thumbnailPath,
                            SmallImagePath = smallPath,
                            LargeImagePath = largePath,
                            IsDefault = false,
                            CreatedAt = DateTime.UtcNow
                        };

                        product.ProductImages.Add(imageEntity);

                        await _userInfoService.ActionLogAsync("SingleProduct/ProductImage", ActionName.DataAdd, null, imageEntity, imageEntity.ProductImageID, model);

                    }
                }


                await _productRepository.AddAsync(product, model);

                await _userInfoService.ActionLogAsync("SingleProduct/Product", ActionName.DataAdd, null, product, product.ProductID, model);


                var price = new ProductPricing()
                {
                    ProductID = product.ProductID,
                    SellingPriceExclVAT = model.SellingPrice,
                    VATPercent = Convert.ToDecimal(model.VATPercentage),
                };

                await _productPricingRepository.AddAsync(price, model);
                await _userInfoService.ActionLogAsync("SingleProduct/ProductPrice", ActionName.DataAdd, null, price, price.PriceID, model);


                var customField = new ProductCustomFields()
                {
                    ProductID = product.ProductID,
                    WarrantyTypeID = model.HasWarranties == true ? model.Warranty : null,
                    ManufacturedDate = model.HasManufacturer == true ? model.ManufacturedDate : null,
                    ManufacturerName = model.HasManufacturer == true ? model.Manufacturer : null,
                    ExpiryDate = model.HasExpiry == true ? model.ExpiryDate : null,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.CreatedBy.ToString()
                };
                
                await _productCustomFieldsRepository.AddAsync(customField);
                await _userInfoService.ActionLogAsync("SingleProduct/PriceCustomField", ActionName.DataAdd, null, customField, customField.CustomFieldID, model);


                var advPrice = new ProductAdvancedPricing()
                {
                    ProductID = product.ProductID,
                    CustomerGroupID = model.CustomerGroup,
                    StartDate = model.SpecialPriceStartDate,
                    EndDate = model.SpecialPriceEndDate,

                };

                await _productAdvPriceRepository.AddAsync(advPrice, model);
                await _userInfoService.ActionLogAsync("SingleProduct/advPrice", ActionName.DataAdd, null, advPrice, advPrice.ProductAdvancedPriceID, model);


                if (model.TierPrices.Any())
                {
                    var advPrice2 = model.TierPrices.Select(e=> new ProductAdvancedPricing()
                    {
                        ProductID = product.ProductID,
                        CustomerGroupID = e.CustomerGroup,
                        MinQuantity = e.MinQuantity,
                        MaxQuantity = e.MaxQuantity,
                        PriceValue = e.Value,
                        CalculationTypeID = e.PriceType,
                        CreatedAt = DateTime.Now,
                        CreatedBy = model.CreatedBy,
                        LIP = model.LIP,
                        LMAC = model.LMAC

                    }).ToList();

                    await _productAdvPriceRepository.AddRangeAsync(advPrice2);
                }

                await _productRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Product added successfully",
                    Data = product.ProductID.ToString()
                };
            }
            catch (Exception ex)
            {
                await _productRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,
                   
                };
               
            }
           
        }


        public async Task<string> SaveImageAsync(IFormFile file, string folderName, string fileName)
        {
            var folderPath = Path.Combine("wwwroot", "media", "products", folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //return $"/media/products/{folderName}/{fileName}";
            return $"{fileName}";
        }



        public async Task<string> GetNextSKU()
        {
            string autoSKU = $"SKU-{DateTime.UtcNow:yyyyMMddHHmmss}";
            return autoSKU;
        }
    }
}
