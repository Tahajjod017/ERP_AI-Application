using GCTL.Core.Helpers;
using GCTL.Core.ViewModels.AdminSettingsVM;
using GCTL.Data.Models;
using GCTL.Service.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace GCTL.Service.AdminSettings.OrganizationSettings.ApprovalService
{
    public interface IApprovalSettingService
    {
        Task<bool> AddAsync(ApprovalSettingsVM model);
        Task<bool> UpdateAsync(ApprovalSettingsVM model);
        Task<ApprovalSettingsVM> GetByIdAsync(int id);
        Task<PaginationService<ApprovalSettings, ApprovalSettingsVM>.PaginationResult<ApprovalSettingsVM>> GetAllAsync(int pageNumber = 1, int pageSize = 5, string searchTerm = "",
        string sortColumn = "HolidayID", string sortOrder = "desc", int? organizationID = null);
        Task<ApprovalSettingsVM> SoftDeleteAsync(DeleteRequestVM requestVM);
        Task<bool> IsNameUniqueAsync(int approvalTypeId, int organizationId);
        Task<List<SelectListItem>> GetOrganizationsAsync();
        Task<List<SelectListItem>> GetOrganizationsAsync2();
        Task<List<SelectListItem>> GetApprovalTypesAsync();
        Task<List<SelectListItem>> GetEmployeeAsync();
        Task<List<SelectListItem>> GetDesignationAsync();
        Task<List<SelectListItem>> GetEmployeeWithApprovalDesignationAsync(int organizationId);
        Task<List<SelectListItem>> GetEmployeeWithApprovalDesignationForEditAsync(int organizationId);


    }
}
