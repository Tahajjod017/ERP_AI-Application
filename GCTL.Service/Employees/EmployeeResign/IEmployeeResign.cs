using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeResign;

namespace GCTL.Service.Employees.EmployeeResign
{
    public interface IEmployeeResign
    {
        object GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate, string imgSrcThumb);
        Task<CommonReturnViewModel> InsertResignation(ResignationPostViewModel model);
        CommonReturnViewModel UpdateResignation(int resignationId, ResignationPostViewModel model);
        CommonReturnViewModel DeleteResignation(int resignationId);
        ResignationViewModel GetResignationById(int resignationId);
    }
}
