using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.CRM
{
    public interface ICRMService
    {

        Task<List<LeadsTableVM>> GetLeads(string customerType);

    }
}
