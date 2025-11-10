using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.CRM.Customer
{
    public class ClintContact : BaseViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Phone { get; set; }
        public string? OtherPhone { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
    }
}
