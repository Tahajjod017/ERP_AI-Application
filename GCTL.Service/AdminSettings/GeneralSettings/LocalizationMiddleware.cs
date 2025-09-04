using GCTL.Service.ActionLogAudit;
using Microsoft.AspNetCore.Http;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            // Fallbacks from the server (machine) itself
            var culture = CultureInfo.CurrentCulture;
            var fallbackZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var fallbackDatePattern = culture.DateTimeFormat.ShortDatePattern;
            var fallbackTimePattern = culture.DateTimeFormat.ShortTimePattern;

            var orgId = await userInfo.GetOrganizationIdAsync(http.User, http);
            //var orgId = 2; // hardcoded for your testing

            // If there's no org, keep the current timezone setup and continue
            if (orgId is null)
            {
                ctx.Zone = fallbackZone;
                ctx.DatePattern = fallbackDatePattern;
                ctx.TimePattern = fallbackTimePattern;
                await _next(http);
                return;

            }

            try
            {
                // Fetch the localization bundle for the organization
                var bundle = await locService.GetOrgLocalizationBundleAsync(orgId.Value);

                // If the localization data is missing or incomplete, fallback to default settings
                if (bundle == null || string.IsNullOrEmpty(bundle.TzValueOrIana) || string.IsNullOrEmpty(bundle.DatePattern) || string.IsNullOrEmpty(bundle.TimePattern))
                {
                    ctx.Zone = fallbackZone;
                    ctx.DatePattern = fallbackDatePattern;
                    ctx.TimePattern = fallbackTimePattern;
                    await _next(http);
                    return;
                }

                // Build DateTimeZone from DB value (handle both UTC offset and IANA time zone)
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
                    // Treat as IANA time zone
                    zone = DateTimeZoneProviders.Tzdb[bundle.TzValueOrIana];
                }

                // Fill scoped context with the resolved values
                ctx.Zone = zone;
                ctx.DatePattern = bundle.DatePattern;
                ctx.TimePattern = bundle.TimePattern;
            }
            catch (InvalidOperationException ex)
            {
                // Handle the case where the organization has no localization setup
                // This will catch the specific exception and apply fallback settings
                ctx.Zone = fallbackZone;
                ctx.DatePattern = fallbackDatePattern;
                ctx.TimePattern = fallbackTimePattern;
                await _next(http);
                return;
            }
        }
    }

}
