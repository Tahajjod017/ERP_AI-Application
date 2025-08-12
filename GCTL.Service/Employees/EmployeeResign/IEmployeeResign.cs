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
        CommonReturnViewModel InsertResignation(ResignationPostViewModel model);
        bool UpdateResignation(int resignationId, ResignationPostViewModel model);
        bool DeleteResignation(int resignationId);
        ResignationViewModel GetResignationById(int resignationId);
    }
}
