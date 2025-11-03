using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.Repository;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;

namespace GCTL.Service.POS.Product
{
    public class SingleProductService : ISingleProduct
    {
        private readonly IGenericRepository<Products> _productRepository;
        private readonly IGenericRepository<ProductImages> _productImageRepository;

        public SingleProductService(IGenericRepository<Products> productRepository, IGenericRepository<ProductImages> productImageRepository)
        {
            _productRepository = productRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<CommonReturnViewModel> AddProductAsync(SingleProductViewModel model)
        {
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
                    }
                }


                await _productRepository.AddAsync(product, model);

                return new CommonReturnViewModel
                {
                    Success = true,
                    Message = "Product added successfully",
                    Data = product.ProductID.ToString()
                };
            }
            catch (Exception)
            {

                throw;
            }
           
        }


        private async Task<string> SaveImageAsync(IFormFile file, string folderName, string fileName)
        {
            var folderPath = Path.Combine("wwwroot", "media", "products", folderName);
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/media/products/{folderName}/{fileName}";
        }



        public async Task<string> GetNextSKU()
        {
            string autoSKU = $"SKU-{DateTime.UtcNow:yyyyMMddHHmmss}";
            return autoSKU;
        }
    }
}
