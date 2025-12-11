using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.POS.Purchasess.Purchase.CreatePO
{
    public class AddressDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullAddress { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        // Optional fields (Projects have them, Wirehouses may not)
        public string State { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public int? CountryID { get; set; }
        public string OtherPhone { get; set; }
        public string Street { get; set; }
        public string Additionaladdress { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Latitude { get; set; }
    }

}
