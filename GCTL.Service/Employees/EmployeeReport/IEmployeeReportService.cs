using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.Service.Employees.EmployeeReport
{
    public interface IEmployeeReportService
    {
        Task<byte[]> GenaratePDF(int id);
        Task<byte[]> GenerateEmployeeExcelReportAsync();
    }
}
