using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using GCTL.Service.ActionLogAudit;
using GCTL.Core.ViewModels;
using Microsoft.AspNetCore.Http;
namespace GCTL.Service.ActionLogAudit
{
    public class UserInfoActionFilter : IActionFilter
    {
        private readonly IUserInfoService _userInfoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoActionFilter(IUserInfoService userInfoService, IHttpContextAccessor httpContextAccessor)
        {
            _userInfoService = userInfoService;
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var user = _httpContextAccessor.HttpContext.User;
            var httpContext = _httpContextAccessor.HttpContext;

            foreach (var argument in context.ActionArguments)
            {
                if (argument.Value is BaseViewModel model)
                {
                    _userInfoService.SetUserInfo(model, user, httpContext);
                }
            }
            
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Nothing to do after action
        }
    }

}
