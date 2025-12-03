using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;

namespace GCTL.Service.POS.Product
{
    public interface IAttributeProduct
    {
        Task<CommonReturnViewModel> AddAttrProductAsync(AttrProductAddViewModel model);
    }
}
