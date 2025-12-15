using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.FieldServices;

namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    public interface IEmployeeAdvanced
    {
        Task<CommonReturnViewModel>AddAsync(EmployeeAdvancedVM emp);
        Task<IEnumerable<CommonSelectVM>>EmployeeDD();
        
    }
}
