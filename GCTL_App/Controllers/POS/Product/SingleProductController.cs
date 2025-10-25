using GCTL.Core.ViewModels.POS.Product.SingleProduct;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;

namespace GCTL_App.Controllers.POS.Product
{
    public class SingleProductController : BaseController
    {
        public SingleProductController(ITranslateService translateService, IUserProfileService userProfileService) : base(translateService, userProfileService)
        {
        }

        public IActionResult Index()
        {
            var model = new SingleProductPageViewModel();
            //{
            //    SingleProduct = new GCTL.Core.ViewModels.POS.Product.SingleProduct.SingleProductViewModel(),
            //    BGDisOffer = new GCTL.Core.ViewModels.POS.Product.SingleProduct.BGDisOfferViewModel(),
            //    BGFreeOfferViewModel = new GCTL.Core.ViewModels.POS.Product.SingleProduct.BGFreeOfferViewModel(),
            //    BGGiftBuyingProduct = new GCTL.Core.ViewModels.POS.Product.SingleProduct.BGGiftBuyingProductViewModel(),
            //    ComboOfferList = new GCTL.Core.ViewModels.POS.Product.SingleProduct.ComboOfferListViewModel(),
            //    Service = new GCTL.Core.ViewModels.POS.Product.SingleProduct.ServiceViewModel(),
            //    AttrProduct = new GCTL.Core.ViewModels.POS.Product.SingleProduct.AttrProductAddViewModel(),

            //};
            return View(model);
        }
    }
}
