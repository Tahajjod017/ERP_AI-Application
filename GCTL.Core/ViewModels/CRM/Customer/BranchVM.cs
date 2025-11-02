using System.ComponentModel.DataAnnotations;

namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class BranchVM : BaseViewModel
    {
        public int Bid { get; set; }
        [Required(ErrorMessage = "Branch name is required")]
        public string BName { get; set; }
        [Required(ErrorMessage = "Customer name is required")]
        public int? BCustomerID { get; set; }
        [Required(ErrorMessage = "First Name is required")]
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
        [Required(ErrorMessage = "Phone number is required")]
        public string BPhone { get; set; }

        public string? BOtherPhone { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string? BEmail { get; set; }
    }
}
