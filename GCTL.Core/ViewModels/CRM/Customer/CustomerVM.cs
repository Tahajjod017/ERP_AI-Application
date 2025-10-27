using System.ComponentModel.DataAnnotations;
namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class CustomerVM : BaseViewModel
    {
        public int ID { get; set; }
        public string Name{get; set;}   

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? FullAddress { get; set; }

        public string? Street { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Additionaladdress { get; set; }

        public string? PostalCode { get; set; }

        public int? CountryID { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string Phone { get; set; }

        public string? OtherPhone { get; set; }

        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}
