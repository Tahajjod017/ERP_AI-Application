using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.FieldServices;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL.Service.FieldServices.EmployeeAdvanced
{
    public interface IEmployeeAdvanced
    {
        Task<CommonReturnViewModel>AddAsync(EmployeeAdvancedVM emp);
        Task<IEnumerable<CommonSelectVM>>EmployeeDD();

        Task<ReturnDataView<SelectListItem>> GetJobTypeAsync(string search, int page, int pageSize, int organizationID);

        Task<CommonReturnViewModel> ApproveAsync(int id, int approvedByUserId);

    }
}
