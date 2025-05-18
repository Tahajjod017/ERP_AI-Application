using GCTL.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.RolePermissions
{
    public class PermissionAttribute : Attribute, IAsyncActionFilter
    {
        private readonly string _permission;
        private readonly string _controllerName;

        public PermissionAttribute(string permission, string controllerName)
        {
            _permission = permission;
            _controllerName = controllerName;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var user = httpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Resolve services manually
            var userManager = httpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
            var db = httpContext.RequestServices.GetRequiredService<AppDbContext>();
            var permissionService = httpContext.RequestServices.GetRequiredService<PermissionService>();

            var appUser = await userManager.GetUserAsync(user);
            var roles = await userManager.GetRolesAsync(appUser);
            var role = roles.FirstOrDefault();

            if (string.IsNullOrEmpty(role))
            {
                context.Result = new ForbidResult();
                return;
            }

            var roleEntity = await db.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (roleEntity == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var module = await db.MenuTabs.FirstOrDefaultAsync(m => m.ControllerName == _controllerName);
            if (module == null)
            {
                context.Result = new NotFoundResult();
                return;
            }

            bool hasPermission = await permissionService.HasPermissionAsync(roleEntity.Id, module.MenuTabId, _permission);

            if (!hasPermission)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
                return;
            }

            await next(); // permission granted, go to the controller
        }
    }
}
