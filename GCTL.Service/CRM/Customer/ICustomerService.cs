using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.CRM.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM.Customer
{
    public interface ICustomerService
    {
        Task<ReturnView> CreateCustomer(CustomerVM customerVM);
        Task<ReturnView> CreateBranch(AddressVM branchVM);
    }
}
