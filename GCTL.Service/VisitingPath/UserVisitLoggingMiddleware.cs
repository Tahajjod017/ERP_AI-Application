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

        private static readonly string[] IgnoredExtensions =
            { ".ico", ".png", ".jpg", ".jpeg", ".webp", ".gif", ".svg", ".css", ".js", ".json" };

        private static readonly string[] IgnoredPaths =
            { "/img/", "/css/", "/js/", "/fonts/", "/lib/" };

        public UserVisitLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
        {
            try
            {
                var path = context.Request.Path.ToString().ToLower();

                if (ShouldIgnorePath(path))
                {
                    await _next(context);
                    return;
                }


                var startTime = DateTime.UtcNow;

                await _next(context); // Process the request

                var endTime = DateTime.UtcNow;
                var duration = (endTime - startTime).TotalSeconds;


                var visit = new UserVisitLogs
                {
                    UserId = context.User.Identity.IsAuthenticated ? context.User.Identity.Name : "Anonymous",
                    Path = path,
                    Method = context.Request.Method,
                    Ipaddress = GetLocalIP(),
                    Lmac = GetMacAddress(),
                    VisitTime = startTime,
                    DurationInSeconds = duration
                };

                // dbContext.UserVisitLogs.Add(visit);
                // await dbContext.SaveChangesAsync();
                if (visit.DurationInSeconds != null)
                {
                    dbContext.UserVisitLogs.Add(visit);
                    await dbContext.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }

        private static bool ShouldIgnorePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || path == "/")
                return true;

            // Explicitly ignored API paths
            var explicitlyIgnoredPaths = new[]
            {
                "/language/getlanguageonsession",
                "/visitingpath/getall"
            };

            if (explicitlyIgnoredPaths.Any(p => path.Equals(p, StringComparison.OrdinalIgnoreCase)))
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
