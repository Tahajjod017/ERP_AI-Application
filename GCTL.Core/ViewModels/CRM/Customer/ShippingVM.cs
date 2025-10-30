using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class ShippingVM : BaseViewModel
    {
        public int SID { get; set; }
        public int SCustomerID { get; set; }
        public string? SFirstName { get; set; }

        public string? SLastName { get; set; }

        public string? SFullAddress { get; set; }

        public string? SStreet { get; set; }

        public string? SCity { get; set; }

        public string? SState { get; set; }

        public string? SAdditionaladdress { get; set; }

        public string? SPostalCode { get; set; }

        public int? SCountryID { get; set; }

        public decimal? SLatitude { get; set; }

        public decimal? SLongitude { get; set; }

        public string SPhone { get; set; }

        public string? SOtherPhone { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string? SEmail { get; set; }
    }
}