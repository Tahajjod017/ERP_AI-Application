using NodaTime;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public static class UtcTimeHelper
    {
        private const string NtpServer = "time.google.com"; //check server  online

        public static DateTime GetReliableUtcNow()
        {
            try
            {
                return GetNetworkUtcTime();
            }
            catch
            {
                // If NTP fails, fallback to system clock
                return DateTime.UtcNow;
            }
        }

        private static DateTime GetNetworkUtcTime()
        {
            var ntpData = new byte[48];
            ntpData[0] = 0x1B; // NTP request settings

            var addresses = System.Net.Dns.GetHostEntry(NtpServer).AddressList;
            var ipEndPoint = new System.Net.IPEndPoint(addresses[0], 123);

            using var socket = new System.Net.Sockets.Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveTimeout = 3000 // 3 seconds timeout
            };

            socket.Connect(ipEndPoint);
            socket.Send(ntpData);
            socket.Receive(ntpData);

            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData.Skip(serverReplyTime).Take(4).Reverse().ToArray(), 0);
            ulong fractPart = BitConverter.ToUInt32(ntpData.Skip(serverReplyTime + 4).Take(4).Reverse().ToArray(), 0);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToUniversalTime();
        }
        public static class TimeConversionHelper
        {
            // Convert a TimeOnly to UTC using the user's time zone from ILocalizationContext and return as TimeOnly
            public static TimeOnly ConvertTimeOnlyToUtc(TimeOnly timeOnly, ILocalizationContext ctx)
            {
                // Combine the TimeOnly value with today's date to create a full LocalDateTime
                var localDateTime = new LocalDateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, timeOnly.Hour, timeOnly.Minute, timeOnly.Second);

                // Convert this LocalDateTime to the user's time zone (ZonedDateTime)
                var zonedDateTime = ctx.Zone.AtStrictly(localDateTime);

                // Convert the ZonedDateTime to UTC and return as DateTime
                var utcDateTime = zonedDateTime.ToInstant().ToDateTimeUtc();

                // Extract and return only the time portion as TimeOnly
                return new TimeOnly(utcDateTime.Hour, utcDateTime.Minute, utcDateTime.Second);
            }
            public static string ConvertDateTimeToUtcHHmm(DateTime localDateTime, ILocalizationContext ctx)
            {
                // Interpret the input as a local wall-clock time in the user's zone (ignore DateTime.Kind)
                var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
                var ldt = LocalDateTime.FromDateTime(unspecified);

                // If you prefer strict DST rules (throw on gaps/overlaps), use AtStrictly.
                // Using AtLeniently to avoid exceptions and auto-resolve DST issues.
                var zoned = ctx.Zone.AtLeniently(ldt);

                var utc = zoned.ToInstant().ToDateTimeUtc();
                return utc.ToString("HH:mm", CultureInfo.InvariantCulture);
            }
            public static string ConvertUtcDateTimeToLocalHHmm(DateTime utcDateTime, ILocalizationContext ctx)
            {
                // Ensure the input is treated as UTC
                if (utcDateTime.Kind != DateTimeKind.Utc)
                    utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

                var instant = Instant.FromDateTimeUtc(utcDateTime);
                var localZoned = instant.InZone(ctx.Zone);
                var local = localZoned.ToDateTimeUnspecified();
                //return localZoned.ToDateTimeUnspecified().ToString("HH:mm", CultureInfo.InvariantCulture);
                return local.ToString(ctx.TimePattern, CultureInfo.InvariantCulture);
            }

            public static string ConvertUtcToUserLocalizedDateTimeString(DateTime utcDateTime, ILocalizationContext ctx)
            {
                // Ensure the input is treated as UTC
                if (utcDateTime.Kind != DateTimeKind.Utc)
                    utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

                var instant = Instant.FromDateTimeUtc(utcDateTime);
                var userZonedDateTime = instant.InZone(ctx.Zone);

                // Format the DateTime using the user's localization pattern
                return userZonedDateTime.ToString(ctx.DateTimePattern, CultureInfo.InvariantCulture);
            }


            public static TimeOnly ConvertUtcDateTimeToLocalTimeOnly(DateTime utcDateTime, ILocalizationContext ctx)
            {
                if (utcDateTime.Kind != DateTimeKind.Utc)
                    utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

                var instant = Instant.FromDateTimeUtc(utcDateTime);
                var localZoned = instant.InZone(ctx.Zone);
                var local = localZoned.ToDateTimeUnspecified();

                return new TimeOnly(local.Hour, local.Minute, local.Second, local.Millisecond);
            }
            public static DateTime ToUtcDateTime(TimeOnly time)
            {
                var today = DateTime.UtcNow.Date; 
                return new DateTime(today.Year, today.Month, today.Day,
                                    time.Hour, time.Minute, time.Second,
                                    DateTimeKind.Utc);
            }

            // Helper: TimeOnly(UTC 24h) -> Local TimeOnly
            public static TimeOnly ConvertUtcTimeOnlyToLocal(TimeOnly utcTime, ILocalizationContext ctx)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                //var utcDateTime = utcTime.ToDateTime(today, DateTimeKind.Utc);
                var utcDateTime = ToUtcDateTime(utcTime);
                return ConvertUtcDateTimeToLocalTimeOnly(utcDateTime, ctx);
            }

            // Helper: Directly format as string in user's pattern
            public static string ConvertUtcTimeOnlyToLocalFormatted(TimeOnly utcTime, ILocalizationContext ctx)
            {
                var localTime = ConvertUtcTimeOnlyToLocal(utcTime, ctx);
                var pattern = ctx.TimePattern ?? "HH:mm"; // fallback 24h
                var culture =  CultureInfo.InvariantCulture;

                return localTime.ToString(pattern, culture);
            }


        }

    }

}
