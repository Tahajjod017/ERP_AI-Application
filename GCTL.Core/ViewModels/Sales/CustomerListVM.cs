using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Sales
{
    public class CustomerListVM : BaseViewModel
    {
        public int? CustomerID { get; set; }

        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }
        public int? AddressID { get; set; }
        public string? Street { get; set; }
        public string? FullAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Additionaladdress { get; set; }
        public string? PostalCode { get; set; }
        public string? Phone { get; set; }
        public string? OtherPhone { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

    }

}
