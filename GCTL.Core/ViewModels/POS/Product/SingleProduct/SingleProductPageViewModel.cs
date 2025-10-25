using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Product.SingleProduct
{
    public class SingleProductPageViewModel
    {
        public SingleProductViewModel SingleProduct { get; set; } = new SingleProductViewModel();

        public AttrProductAddViewModel AttrProduct { get; set; } = new AttrProductAddViewModel();

        public BGDisOfferViewModel BGDisOffer { get; set; } = new BGDisOfferViewModel();

        public BGFreeOfferViewModel BGFreeOfferViewModel { get; set; } = new BGFreeOfferViewModel();

        public BGGiftOfferViewModel BGGiftBuyingProduct { get; set; } = new BGGiftOfferViewModel();

        public ComboOfferViewModel ComboOfferList { get; set; } = new ComboOfferViewModel();

        public ServiceViewModel Service { get; set; } = new ServiceViewModel();


    }
}
