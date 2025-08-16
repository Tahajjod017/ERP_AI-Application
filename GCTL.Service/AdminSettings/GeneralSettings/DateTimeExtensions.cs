using NodaTime.Text;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public static class DateTimeExtensions
    {
        // Convert UTC DateTime -> ZonedDateTime (org zone)

        public static ZonedDateTime NowZoned(ILocalizationContext ctx)
        {
            var now = SystemClock.Instance.GetCurrentInstant();
            return now.InZone(ctx.Zone);
        }
        public static string NowDateTime(ILocalizationContext ctx)
        {
            var z = NowZoned(ctx);
            var pattern = LocalDateTimePattern.CreateWithInvariantCulture(ctx.DateTimePattern);
            return pattern.Format(z.LocalDateTime);
        }

        public static ZonedDateTime ToOrgZoned(this DateTime utc, ILocalizationContext ctx)
        {
            var u = utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            var instant = Instant.FromDateTimeUtc(u);
            return instant.InZone(ctx.Zone);
        }

        public static string ToOrgDate(this DateTime utc, ILocalizationContext ctx)
        {
            var z = utc.ToOrgZoned(ctx);
            var pattern = LocalDatePattern.CreateWithInvariantCulture(ctx.DatePattern);
            return pattern.Format(z.Date);
        }

        public static string ToOrgTime(this DateTime utc, ILocalizationContext ctx)
        {
            var z = utc.ToOrgZoned(ctx);
            var pattern = LocalTimePattern.CreateWithInvariantCulture(ctx.TimePattern);
            return pattern.Format(z.TimeOfDay);
        }

        public static string ToOrgTimeString(this DateTime utc, ILocalizationContext ctx)
        {
            var z = utc.ToOrgZoned(ctx);
            var localTime = z.LocalDateTime;
            return localTime.ToString(ctx.TimePattern, CultureInfo.InvariantCulture);
        }

        public static string ToOrgDateTime(this DateTime utc, ILocalizationContext ctx)
        {
            var z = utc.ToOrgZoned(ctx);
            var pattern = LocalDateTimePattern.CreateWithInvariantCulture($"{ctx.DatePattern} {ctx.TimePattern}");
            return pattern.Format(z.LocalDateTime);
        }

        // Parse user-input local date+time -> UTC
        public static DateTime ParseLocalToUtc(string localDate, string localTime, ILocalizationContext ctx)
        {
            var dpat = LocalDatePattern.CreateWithInvariantCulture(ctx.DatePattern);
            var tpat = LocalTimePattern.CreateWithInvariantCulture(ctx.TimePattern);

            var d = dpat.Parse(localDate).GetValueOrThrow();
            var t = tpat.Parse(localTime).GetValueOrThrow();

            var ldt = d + t;                  // LocalDateTime
            var zdt = ctx.Zone.AtStrictly(ldt);
            var instant = zdt.ToInstant();
            return instant.ToDateTimeUtc();   // save this in DB
        }
    }
}
