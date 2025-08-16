using GCTL.Service.ActionLogAudit;
using Microsoft.AspNetCore.Http;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public class LocalizationMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly Regex UtcOffsetRegex =
            new(@"^UTC(?<sign>[+-])(?<hh>\d{2}):(?<mm>\d{2})$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public LocalizationMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(
            HttpContext http,
            ILocalizationContext ctx,
            ILocalizationSettingService locService,
            IUserInfoService userInfo)
        {
            // Ensure the user is authenticated
            if (!http.User.Identity.IsAuthenticated)
            {
                // Handle unauthenticated access if needed (e.g., redirect, error message, etc.)
                await _next(http);
                return; // Exit middleware if user is not authenticated
            }

            var orgId = await userInfo.GetOrganizationIdAsync(http.User,http);
            //var orgId = 2; // hardcoded for your testing


            var bundle = await locService.GetOrgLocalizationBundleAsync(orgId.Value);

            // Build DateTimeZone from DB value
            DateTimeZone zone;
            var m = UtcOffsetRegex.Match(bundle.TzValueOrIana);
            if (m.Success)
            {
                var sign = m.Groups["sign"].Value == "-" ? -1 : 1;
                var hh = int.Parse(m.Groups["hh"].Value);
                var mm = int.Parse(m.Groups["mm"].Value);
                var offset = Offset.FromHoursAndMinutes(sign * hh, sign * mm);
                zone = DateTimeZone.ForOffset(offset);
            }
            else
            {
                // treat as IANA
                zone = DateTimeZoneProviders.Tzdb[bundle.TzValueOrIana];
            }

            // Fill scoped context
            ctx.Zone = zone;
            ctx.DatePattern = bundle.DatePattern;
            ctx.TimePattern = bundle.TimePattern;

            await _next(http);
        }
    }

}
