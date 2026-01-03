using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.Employee.EmployeeBulkRegister;

namespace GCTL.Service.Employees.EmployeeBulkRegister
{
    public interface IEmployeeBulkRegisterService
    {
        byte[] GenerateExcelTemplate();
        EmpPagedResult<EmployeeExcelVM> GetPreviewData(int page, int pageSize, string search);
        DuplicateCheckResult CheckDuplicates(List<EmployeeExcelVM> employees);
        Task<CommonReturnViewModel> SaveEmployeeDataAsync(List<EmployeeExcelVM> employees, BaseViewModel? baseView, int? orgaId);
    }
}