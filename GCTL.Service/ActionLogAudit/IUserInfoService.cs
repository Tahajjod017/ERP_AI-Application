using GCTL.Core.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ActionLogAudit
{
    public interface IUserInfoService
    {
        void SetUserInfo(BaseViewModel model, ClaimsPrincipal user, HttpContext httpContext);
        Task<int?> GetOrganizationIdAsync(ClaimsPrincipal user, HttpContext httpContext);

        Task ActionLogAsync<T>(string tergetType, string actionName, T before, T after, int? targetID, BaseViewModel entityVM);

      

        Task ActionLogDeleteAsync<T>(string targetType, string actionName, List<T> beforeList, List<T> afterList, List<int?> targetIds, BaseViewModel entityVM);

        // without BaseViwwmodel
        Task ActionLogAsync<T>(string tergetType, string actionName, T before, T after, int? targetID);
        Task ActionLogDeleteAsync<T>(string targetType, string actionName, List<T> beforeList, List<T> afterList, List<int?> targetIds);
        Task ActionLogExceptionAsync(string targetType, Exception exception, int? targetId, string actionName);
    }
}
