
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;

namespace GCTL.Service.POS.Product
{
    public interface ISingleProduct
    {
        Task<CommonReturnViewModel> AddProductAsync(SingleProductViewModel model);
        Task<string> GetNextSKU();
       
    }
}