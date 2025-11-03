using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class WarehouseVM : BaseViewModel
    {
        public int Wid { get; set; }
        public string WName { get; set; }
        public int? WCustomerID { get; set; }
        public string? WCustomerName { get; set; }

        public string? WFirstName { get; set; }

        public string? WLastName { get; set; }

        public string? WFullAddress { get; set; }

        public string? WStreet { get; set; }

        public string? WCity { get; set; }

        public string? WState { get; set; }

        public string? WAdditionaladdress { get; set; }

        public string? WPostalCode { get; set; }

        public int? WCountryID { get; set; }
        public string? WCountryName { get; set; }

        public decimal? WLatitude { get; set; }

        public decimal? WLongitude { get; set; }

        public string WPhone { get; set; }

        public string? WOtherPhone { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string? WEmail { get; set; }
    }
}