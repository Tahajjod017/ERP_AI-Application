using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers
{
    public static class DateTimeHelpers
    {
        public static string ToDateFormat(this DateTime date)
        {
            return date.Year > 1905 ? date.ToString(ApplicationConstants.DateFormat) : string.Empty;
        }

        public static string ToDateTimeFormat(this DateTime date)
        {
            return date.Year > 1905 ? date.ToString(ApplicationConstants.DateTimeFormat) : string.Empty;
        }

        public static DateTime ToDate(this string date)
        {
            if (!string.IsNullOrEmpty(date))
                return DateTime.ParseExact(date, ApplicationConstants.DateFormat, CultureInfo.InvariantCulture);

            return DateTime.MinValue.AddYears(1904);
        }

        public static DateTime ToFulCreatedAt(this string date)
        {
            DateTime dateTime = DateTime.MinValue;
            if (!string.IsNullOrEmpty(date))
            {
                dateTime = DateTime.ParseExact(date, ApplicationConstants.DateFormat, CultureInfo.InvariantCulture);
                dateTime = dateTime.AddDays(1).AddSeconds(-1);
                return dateTime;
            }

            return DateTime.MinValue.AddYears(1904);
        }

        public static DateTime ToDateTime(this string date)
        {
            if (!string.IsNullOrEmpty(date))
                return DateTime.ParseExact(date, ApplicationConstants.DateTimeFormat, CultureInfo.InvariantCulture);

            return DateTime.MinValue.AddYears(1904);
        }
    }
}
