using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.POS.Product.SingleProduct;

namespace GCTL.Service.POS.Product.ServiceProduct
{
    public interface IServiceProduct
    {
        Task<CommonReturnViewModel> AddServiceAsync(ServiceViewModel model);
    }
}
