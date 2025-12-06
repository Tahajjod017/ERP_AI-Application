using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Helpers;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using GCTL.Service.ActionLogAudit;

namespace GCTL.Service.POS.Product
{
    public class AttributeProductService : IAttributeProduct
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductAttributes> _productAttributeRepository;
        private readonly IGenericRepository<ProductImages> _productImageRepository;
        private readonly IGenericRepository<ProductPricing> _productPricingRepository;
       // private readonly IGenericRepository<ProductCustomFields> _productCustomFieldsRepository;
        private readonly IGenericRepository<ProductAdvancedPricing> _productAdvPriceRepository;
        private readonly IUserInfoService _userInfoService;
        private readonly ISingleProduct _singleProductService;

        public AttributeProductService(IGenericRepository<Products> productRepository, IGenericRepository<ProductAttributes> productAttributeRepository, IGenericRepository<ProductImages> productImageRepository, IGenericRepository<ProductPricing> productPricingRepository, IGenericRepository<ProductAdvancedPricing> productAdvPriceRepository, IUserInfoService userInfoService, ISingleProduct singleProductService)
        {
            _productRepository = productRepository;
            _productAttributeRepository = productAttributeRepository;
            _productImageRepository = productImageRepository;
            _productPricingRepository = productPricingRepository;
            _productAdvPriceRepository = productAdvPriceRepository;
            _userInfoService = userInfoService;
            _singleProductService = singleProductService;
        }

        public async Task<CommonReturnViewModel> AddAttrProductAsync(AttrProductAddViewModel model)
        {
            if (model.AttrProductImages?.Any() == false)
            {
                return new CommonReturnViewModel { Success = false, Message = "Image is Require, Please add least one Image", };
            }

            await _productAttributeRepository.BeginTransactionAsync();
            try
            {
                var product = new Products
                {
                    ProductTypeID = int.TryParse(model.AttrProductType, out var typeId) ? typeId : null,
                    ProductCategoryID = int.TryParse(model.AttrCategory, out var catId) ? catId : null,
                    ProductSubCategoryID = int.TryParse(model.AttrSubCategory, out var subCatId) ? subCatId : null,
                    ProductBrandID = int.TryParse(model.AttrBrand, out var brandId) ? brandId : null,
                    ProductName = model.AttrProductName,
                    ProductMode = model.AttrModelNo,
                    ProductPartNo = model.AttrPartNo,
                    SKU = await _singleProductService.GetNextSKU(),
                    BarcodeGenerateFromID = int.TryParse(model.AttrBarcodeType, out var barcodeTypeId) ? barcodeTypeId : null,
                    Barcode = model.AttrBarcodeValue,
                    UnitTypeID = int.TryParse(model.AttrUnit, out var unitId) ? unitId : null,
                    AssetsTypeID = int.TryParse(model.AttrAssetsType, out var assetId) ? assetId : null,
                    QuantityAlert = model.AttrQuantityAlert,
                    IsTrackBatch = model.AttrTrackBatch,
                    IsActive = model.AttrIsActive,
                    IsTraceManufacturingDate = model.AttrTraceManufacturingDate,
                    IsTraceExpiryDate = model.AttrTraceExpiryDate,
                    HasSerialNumber = model.AttrSerialNumber,
                    Description = model.AttrDescription,
                   
                };



                if (model.AttrProductImages?.Any() == true)
                {
                    foreach (var imageFile in model.AttrProductImages)
                    {
                        // Generate unique filename
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";

                        // Save each version
                        var thumbnailPath = await _singleProductService.SaveImageAsync(imageFile, "thumbnail", fileName);
                        var smallPath = await _singleProductService.SaveImageAsync(imageFile, "small", fileName);
                        var largePath = await _singleProductService.SaveImageAsync(imageFile, "large", fileName);

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

                        await _userInfoService.ActionLogAsync("AttrProduct/ProductImage", ActionName.DataAdd, null, imageEntity, imageEntity.ProductImageID, model);

                    }
                }


                await _productRepository.AddAsync(product, model);

                await _userInfoService.ActionLogAsync("AttrProduct/Product", ActionName.DataAdd, null, product, product.ProductID, model);


               

                //var customField = new ProductCustomFields()
                //{
                //    ProductID = product.ProductID,
                //    WarrantyTypeID = model.AttrWarrantiesEnabled == true ? model.AttrWarranty : null, 
                //    ManufacturedDate = model.AttrManufacturerEnabled == true ? model.AttrManufacturedDate : null,
                //    ManufacturerName = model.AttrManufacturerEnabled == true ? model.AttrManufacturerName : null,
                //    ExpiryDate = model.AttrExpiryEnabled == true ? model.AttrExpiryDate : null,
                //    CreatedAt = DateTime.Now,
                //    CreatedBy = model.CreatedBy.ToString()
                //};

                //await _productCustomFieldsRepository.AddAsync(customField);
                //await _userInfoService.ActionLogAsync("SingleProduct/PriceCustomField", ActionName.DataAdd, null, customField, customField.CustomFieldID, model);


                if (model.AttrSelectedValues.Any())
                {
                    var list = new List<ProductAttributes>();
                    foreach (var value in model.AttrSelectedValues )
                    {
                        foreach(var item in value.Value)
                        {
                            var attdName = new ProductAttributes()
                            {
                                ProductID = product.ProductID,
                                AttributeID = Convert.ToInt32(item.Name),
                                CreatedAt = DateTime.Now,
                                CreatedBy = model.CreatedBy,
                                LIP = model.LIP,
                                LMAC = model.LMAC
                            };
                            list.Add(attdName);
                            //await _productAttributeRepository.AddAsync(attdName, model);
                        }
                       
                    }
                    await _productAttributeRepository.AddRangeAsync(list);
                }

              
                await _productAttributeRepository.CommitTransactionAsync();

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Product added successfully",
                    Data = product.ProductID.ToString()
                };
            }
            catch (Exception ex)
            {
                await _productAttributeRepository.RollbackTransactionAsync();
                return new CommonReturnViewModel
                {
                    Success = false,
                    Message = ex.Message,

                };

            }
        }
    }
}
