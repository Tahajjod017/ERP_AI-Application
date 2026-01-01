using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GCTL.Core.ViewModels.FieldServices;
using GCTL.Data.Models;
using GCTL.Service.Pagination;

namespace GCTL.Service.FieldServices.Advanced_Apporval
{
    public interface IAdvancedApprovalService
    {
       

        Task<PaginationService<EmployeeAdvances, ApprovalGetALLVM>.PaginationResult<ApprovalGetALLVM>> GetAllAsync1(int pageNumber = 1, int pageSize = 5,
           string searchTerm = "", string sortColumn = "EmployeeAdvanceID", string sortOrder = "desc", int? mainAccId = null);
    }
}
