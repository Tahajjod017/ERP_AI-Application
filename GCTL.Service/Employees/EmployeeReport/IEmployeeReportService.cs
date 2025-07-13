using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.Employee.EmployeeListVM;
using Microsoft.AspNetCore.Mvc;

namespace GCTL.Service.Employees.EmployeeReport
{
    public interface IEmployeeReportService
    {
        Task<byte[]> GenaratePDF(int id);
        Task<byte[]> GenerateEmployeeExcelReportAsync(Core.ViewModels.Employee.EmployeeListVM.EmployeeFilterModel filters);
        Task<byte[]> GenerateEmployeePdfPreviewAsync(EmployeeFilterModel filters);
    }
}
