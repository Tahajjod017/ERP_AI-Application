using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class BranchVM : BaseViewModel
    {
        public int BID { get; set; }
        public string BName { get; set; }
        public int BCustomerID { get; set; }

        public string? BFirstName { get; set; }

        public string? BLastName { get; set; }

        public string? BFullAddress { get; set; }

        public string? BStreet { get; set; }

        public string? BCity { get; set; }

        public string? BState { get; set; }

        public string? BAdditionaladdress { get; set; }

        public string? BPostalCode { get; set; }

        public int? BCountryID { get; set; }

        public decimal? BLatitude { get; set; }

        public decimal? BLongitude { get; set; }

        public string BPhone { get; set; }

        public string? BOtherPhone { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string? BEmail { get; set; }
    }
}
