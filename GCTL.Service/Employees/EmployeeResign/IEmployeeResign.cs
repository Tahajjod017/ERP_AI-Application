using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.Employees.EmployeeResign
{
    public interface IEmployeeResign
    {
        object GetResignations(int page, int pageSize, string sortColumn, string sortDirection, string fromDate, string toDate);
    }
}
