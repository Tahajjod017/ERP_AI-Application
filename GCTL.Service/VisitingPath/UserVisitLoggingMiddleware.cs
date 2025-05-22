using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.VisitingPath
{
    public class UserVisitLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly string[] IgnoredExtensions = { ".ico", ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg", ".css", ".js", ".json" };
        private static readonly string[] IgnoredPaths = { "/img/", "/css/", "/js/", "/fonts/", "/lib/" };

        public UserVisitLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            var path = context.Request.Path.ToString().ToLower();

            if (ShouldIgnorePath(path))
            {
                await _next(context);
                return;
            }

            var visit = new UserVisitLog
            {
                UserId = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Anonymous",
                Path = path,
                Method = context.Request.Method,
                Ipaddress = GetLocalIP(),
                Lmac = GetMacAddress(),
                VisitTime = DateTime.UtcNow
            };

            dbContext.UserVisitLogs.Add(visit);
            await dbContext.SaveChangesAsync();

            await _next(context);
        }

        private static bool ShouldIgnorePath(string path)
        {
            // Ignore root path or empty path
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                return true;

            return IgnoredExtensions.Any(ext => path.EndsWith(ext)) ||
                   IgnoredPaths.Any(p => path.Contains(p));
        }

        

        private static string GetLocalIP()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(n => n.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                .Select(ip => ip.Address.ToString())
                .FirstOrDefault() ?? string.Empty;
        }

        private static string GetMacAddress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Select(n => n.GetPhysicalAddress().ToString())
                .FirstOrDefault(mac => !string.IsNullOrEmpty(mac)) ?? string.Empty;
        }
    }


}
