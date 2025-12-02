
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using Microsoft.AspNetCore.Http;

namespace GCTL.Service.POS.Product
{
    public interface ISingleProduct
    {
        Task<CommonReturnViewModel> AddProductAsync(SingleProductViewModel model);
        Task<string> GetNextSKU();
        Task<string> SaveImageAsync(IFormFile file, string folderName, string fileName);


    }
}