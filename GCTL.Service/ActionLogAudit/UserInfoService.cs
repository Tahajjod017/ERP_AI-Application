using GCTL.Core.Helpers;
using GCTL.Core.Helpers.LipLmacAddress;
using GCTL.Core.ViewModels;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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

        #region  Base View Model 

        public void SetUserInfo(BaseViewModel model, ClaimsPrincipal user, HttpContext httpContext)
        {
            if (user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var email = user.FindFirstValue(ClaimTypes.Email);
                var userEmail = user.Identity.IsAuthenticated ? user.FindFirstValue(ClaimTypes.Email) : "Unknown";
                var employeeId = _context.Users .Where(u => u.Email == email).Select(u => u.EmployeeId).FirstOrDefault();
                model.UserId = userId;
                model.UserEmail = email;
                model.CreatedBy = employeeId;
                model.UpdatedBy = employeeId;
                model.DeletedBy = employeeId;
                model.LIP =NetworkHelper.GetLocalIP();
                model.LMAC =NetworkHelper.GetMacAddress();

            }
        }

        public async Task<int?> GetOrganizationIdAsync(ClaimsPrincipal user, HttpContext httpContext)
        {
            if (user.Identity.IsAuthenticated)
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

                // Use asynchronous query to avoid blocking
                var orgId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.OrganizationID)
                    .FirstOrDefaultAsync();  // Returns null if not found

                return orgId;
            }

            return null;  // Return null if user is not authenticated
        }


        public async Task ActionLogAsync<T>(string tergetType, string actionName, T before, T after, int? targetID, BaseViewModel entityVM)
        {
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var log = new ActionLogs
            {
                CreatedBy = entityVM.CreatedBy,
                ActionName = actionName,
                ActionBefore = JsonConvert.SerializeObject(before, jsonSettings),
                ActionAfter = JsonConvert.SerializeObject(after, jsonSettings),
                UserEmail = entityVM.UserEmail,
                LIP = entityVM.LIP,
                LMAC = entityVM.LMAC,
                CreatedAt = DateTime.UtcNow,
                TargetType = tergetType,
                TargetID = targetID

            };
            await _context.ActionLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        

        public async Task ActionLogDeleteAsync<T>(string targetType, string actionName, List<T> beforeList, List<T> afterList, List<int?> targetIds, BaseViewModel entityVM)
        {
            var logs = new List<ActionLogs>();

            // Move serializerSettings outside the loop to create it only once
            var serializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 32 // Optional: Add depth limit for better performance
            };

            for (int i = 0; i < targetIds.Count; i++)
            {
                logs.Add(new ActionLogs
                {
                    CreatedBy = entityVM.CreatedBy,
                    ActionName = actionName,
                    ActionBefore = beforeList != null ? JsonConvert.SerializeObject(beforeList[i], serializerSettings) : null,
                    ActionAfter = afterList != null ? JsonConvert.SerializeObject(afterList[i], serializerSettings) : null,
                    UserEmail = entityVM.UserEmail,
                    LIP = entityVM.LIP,
                    LMAC = entityVM.LMAC,
                    CreatedAt = DateTime.UtcNow,
                    TargetType = targetType,
                    TargetID = targetIds[i]
                });
            }

            await _context.ActionLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

        #endregion
        public async Task ActionLogExceptionAsync(string targetType, Exception exception, int? targetId, BaseViewModel entityVM,string actionName)
        {
            try
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                // Serialize exception details
                var exceptionDetails = new
                {
                    exception.Message,
                    exception.StackTrace,
                    InnerException = exception.InnerException?.Message
                };

                var log = new ActionLogs
                {
                    CreatedBy = entityVM?.CreatedBy,
                    UserEmail = entityVM?.UserEmail,
                    LIP = entityVM?.LIP,
                    LMAC = entityVM?.LMAC,
                    ActionName =actionName, 
                    ActionBefore = null,
                    ActionAfter = JsonConvert.SerializeObject(exceptionDetails, jsonSettings),
                    CreatedAt = DateTime.UtcNow,
                    TargetType = targetType,
                    TargetID = targetId
                };

                await _context.ActionLogs.AddAsync(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception logEx)
            {
                // Optional: fallback logging
                Console.WriteLine("Failed to log exception: " + logEx.Message);
            }
        }


    }
}
