using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.VisitingPath
{
    public class UserVisitLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public UserVisitLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var path = context.Request.Path.ToString().ToLower();

            // ❌ Skip logging for static files or specific paths
            string[] ignorePaths = new[]
            {
        "/favicon.ico",
        ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg", ".css", ".js", ".json",
        "/img/", "/css/", "/js/", "/fonts/", "/lib/"
    };

            if (ignorePaths.Any(p => path.Contains(p)))
            {
                await _next(context);
                return;
            }

            var userId = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Anonymous";
            var method = context.Request.Method;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            var visit = new UserVisitLog
            {
                UserId = userId,
                Path = path,
                Method = method,
                Lmac = ipAddress,
                Ipaddress = ipAddress,
                LoginTime = DateTime.UtcNow,
                LogoutTime = DateTime.UtcNow,
                VisitTime = DateTime.UtcNow
            };

            dbContext.UserVisitLogs.Add(visit);
            await dbContext.SaveChangesAsync();

            await _next(context);
        }

    }

}
