using GCTL.Data.Models;

namespace GCTL.Core.ViewModels.CRM
{
    public class CustomerVM:BaseViewModel
    {

        public string TabName { get; set; }
        public int PrimaryID { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? FullAddress { get; set; }

        public string? Street { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Additionaladdress { get; set; }

        public string? PostalCode { get; set; }

        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? Phone { get; set; }

        public string? OtherPhone { get; set; }

        public string? Email { get; set; }
    }
}
