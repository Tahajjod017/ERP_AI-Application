using GCTL.Core.ViewModels.CRM;
using GCTL.Core.ViewModels.Employee.EmployeeResign;
using QuestPDF.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;

namespace GCTL.Service.CRM
{
    public interface ICRMService
    {

        Task<(List<LeadsTableVM> Leads, int TotalCount)> GetLeads(
            int customerType,
            string dateRange,
            int pageNumber,
            int pageSize,
            string searchTerm,
            string sortColumn,
            string sortDirection
        );

    }
}
