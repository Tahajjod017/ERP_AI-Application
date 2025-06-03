using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.Employee.EmployeeContact
{
    public class ContactSuggestionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Relationship { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
    }
}
