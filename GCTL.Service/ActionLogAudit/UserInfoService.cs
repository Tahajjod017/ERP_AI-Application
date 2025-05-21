using GCTL.Core.Helpers;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.ActionLogAudit
{
    public class UserInfoService : IUserInfoService
    {
        private readonly AppDbContext _context;

        public UserInfoService(AppDbContext context)
        {
            _context = context;
        }

        //#region UserInfo ViewModel
        //public void SetUserInfo(UserInfoHelper model, ClaimsPrincipal user, HttpContext httpContext)
        //{
        //    if (user.Identity.IsAuthenticated)
        //    {
        //        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        //        var email = user.FindFirstValue(ClaimTypes.Email);


        //        var userEmail = user.Identity.IsAuthenticated ? user.FindFirstValue(ClaimTypes.Email) : "Unknown";

        //        var employeUser = _context.Users.FirstOrDefault(u => u.Employee.EmployeeCode == userEmail);



        //        // var employeeId = _context.Users .Where(u => u.Email == email).Select(u => u.EmployeeId).FirstOrDefault();

        //        model.UserId = userId;
        //        model.Email = email;
        //        model.CreatedBy = employeUser?.EmployeeId;
        //        model.UpdatedBy = employeUser?.EmployeeId;
        //        model.DeletedBy = employeUser?.EmployeeId;
        //        //model.CreatedBy = employeeId;
        //        // model.UpdatedBy = employeeId;
        //        //model.DeletedBy = employeeId;
        //        model.LIP = GetLocalIP();
        //        model.LMAC = GetMacAddress();

        //    }
        //}

        //public void SetUserInfo(UserInfoHelper model, ClaimsPrincipal user, HttpContext httpContext, string actionType)
        //{
        //    if (user.Identity.IsAuthenticated)
        //    {
        //        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        //        var email = user.FindFirstValue(ClaimTypes.Email);
        //        var employeeId = _context.Users.Where(u => u.Email == email).Select(u => u.EmployeeId).FirstOrDefault();

        //        model.UserId = userId;
        //        model.Email = email;
        //        model.LIP = GetLocalIP();
        //        model.LMAC = GetMacAddress();
        //        if (actionType == "create")
        //        {
        //            model.CreatedBy = employeeId;
        //        }
        //        else if (actionType == "update")
        //        {
        //            model.UpdatedBy = employeeId;
        //        }
        //        else if (actionType == "delete")
        //        {
        //            model.DeletedBy = employeeId;
        //        }

        //    }
        //}


        //public async Task ActionLogAsync<T>(string tergetType, string actionName, T before, T after, int? targetID, UserInfoHelper entityVM)
        //{
        //    var log = new ActionLogs
        //    {
        //        CreatedBy = entityVM.CreatedBy,
        //        ActionName = actionName,
        //        ActionBefore = JsonConvert.SerializeObject(before),
        //        ActionAfter = JsonConvert.SerializeObject(after),
        //        UserEmail = entityVM.Email,
        //        LIP = entityVM.LIP,
        //        LMAC = entityVM.LMAC,
        //        CreatedAt = DateTime.Now,
        //        TargetType = tergetType,
        //        TargetID = targetID

        //    };
        //    await _context.ActionLogs.AddAsync(log);
        //    await _context.SaveChangesAsync();
        //}


        //public async Task ActionLogDeleteAsync<T>(string targetType, string actionName, List<T> beforeList, List<T> afterList, List<int?> targetIds, UserInfoHelper entityVM)
        //{
        //    var logs = new List<ActionLogs>();

        //    for (int i = 0; i < targetIds.Count; i++)
        //    {
        //        logs.Add(new ActionLogs
        //        {
        //            CreatedBy = entityVM.CreatedBy,
        //            ActionName = actionName,
        //            ActionBefore = beforeList != null ? JsonConvert.SerializeObject(beforeList[i]) : null,
        //            ActionAfter = afterList != null ? JsonConvert.SerializeObject(afterList[i]) : null,
        //            UserEmail = entityVM.Email,
        //            LIP = entityVM.LIP,
        //            LMAC = entityVM.LMAC,
        //            CreatedAt = DateTime.Now,
        //            TargetType = targetType,
        //            TargetID = targetIds[i]
        //        });
        //    }

        //    await _context.ActionLogs.AddRangeAsync(logs);
        //    await _context.SaveChangesAsync();
        //}


        //#endregion


        #region  Base View Model 

        public void SetUserInfo(BaseViewModel model, ClaimsPrincipal user, HttpContext httpContext)
        {
            if (user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = user.FindFirstValue(ClaimTypes.Email);


                var userEmail = user.Identity.IsAuthenticated ? user.FindFirstValue(ClaimTypes.Email) : "Unknown";

               // var employeUser = _context.Users.FirstOrDefault(u => u.Employee.EmployeeCode == userEmail);



                 var employeeId = _context.Users .Where(u => u.Email == email).Select(u => u.EmployeeId).FirstOrDefault();

                model.UserId = userId;
                model.Email = email;
                model.CreatedBy = employeeId;
                model.UpdatedBy = employeeId;
                model.DeletedBy = employeeId;
                model.LIP = GetLocalIP();
                model.LMAC = GetMacAddress();

            }
        }

        public async Task ActionLogAsync<T>(string tergetType, string actionName, T before, T after, int? targetID, BaseViewModel entityVM)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            //var jsonSettings = new JsonSerializerSettings
            //{
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //    ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver
            //    {
            //        // Optionally: ignore virtual/navigation properties if you know they are all virtual
            //        IgnoreSerializableInterface = true
            //    }
            //};

            //object CleanObject(object obj)
            //{
            //    if (obj == null) return null;

            //    var type = obj.GetType();
            //    var cleanObj = new Dictionary<string, object>();

            //    foreach (var prop in type.GetProperties())
            //    {
            //        // Skip navigation properties
            //        if (prop.GetGetMethod()?.IsVirtual == true && !prop.PropertyType.IsSealed) continue;

            //        var value = prop.GetValue(obj);
            //        cleanObj[prop.Name] = value;
            //    }

            //    return cleanObj;
            //}


            var log = new ActionLog
            {
                CreatedBy = entityVM.CreatedBy,
                ActionName = actionName,
                //ActionBefore = JsonConvert.SerializeObject(CleanObject(before), jsonSettings),
                //ActionAfter = JsonConvert.SerializeObject(CleanObject(after), jsonSettings),
                ActionBefore = JsonConvert.SerializeObject(before, jsonSettings),
                ActionAfter = JsonConvert.SerializeObject(after, jsonSettings),
                UserEmail = entityVM.Email,
                Lip = entityVM.LIP,
                Lmac = entityVM.LMAC,
                CreatedAt = DateTime.Now,
                TargetType = tergetType,
                TargetId = targetID

            };
            await _context.ActionLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task ActionLogDeleteAsync<T>(string targetType, string actionName, List<T> beforeList, List<T> afterList, List<int?> targetIds, BaseViewModel entityVM)
        {
            var logs = new List<ActionLog>();

            for (int i = 0; i < targetIds.Count; i++)
            {
                logs.Add(new ActionLog
                {
                    CreatedBy = entityVM.CreatedBy,
                    ActionName = actionName,
                    ActionBefore = beforeList != null ? JsonConvert.SerializeObject(beforeList[i]) : null,
                    ActionAfter = afterList != null ? JsonConvert.SerializeObject(afterList[i]) : null,
                    UserEmail = entityVM.Email,
                    Lip = entityVM.LIP,
                    Lmac = entityVM.LMAC,
                    CreatedAt = DateTime.Now,
                    TargetType = targetType,
                    TargetId = targetIds[i]
                });
            }

            await _context.ActionLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }
        #endregion



        #region LIP LMAC Adderess Method

        private static string GetLocalIP()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var networkInterface in networkInterfaces)
            {
                var properties = networkInterface.GetIPProperties();
                var ipv4Address = properties.UnicastAddresses
                    .FirstOrDefault(ip => ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                if (ipv4Address != null)
                    return ipv4Address.Address.ToString();
            }

            return string.Empty;
        }

        private static string GetMacAddress()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback);

            foreach (var networkInterface in networkInterfaces)
            {
                var macAddress = networkInterface.GetPhysicalAddress().ToString();
                if (!string.IsNullOrEmpty(macAddress))
                    return macAddress;
            }

            return string.Empty;
        }
        #endregion




    }
}
