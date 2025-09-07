using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM
{
    public class CustomerInfoVM
    {
        public int LeadID { get; set; }
        public string? LeadName { get; set; }
        public string? FullName { get; set; }
        public int LeadSourceID { get; set; }
        public int LeadStatusID { get; set; }
        public decimal ApproximateDealValue { get; set; }
        public int PriorityID { get; set; }
        public decimal? Probability { get; set; } = 0;
        public DateTime? Created { get; set; }
        public string? AddressTypeName { get; set; }
        public string? LeadDescription { get; set; }
        public string? Priority { get; set; }
        public string? FullAddress { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? Additionaladdress { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public int? CountryID { get; set; }
        public string? CountryCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? Phone { get; set; }
        public string? OtherPhone { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
