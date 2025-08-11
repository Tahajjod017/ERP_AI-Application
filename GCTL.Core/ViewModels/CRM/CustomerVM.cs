using GCTL.Data.Models;

namespace GCTL.Core.ViewModels.CRM
{
    public class CustomerVM:BaseViewModel
    {
        public List<CustomerBaseVM> Customers { get; set; }
    }
}
