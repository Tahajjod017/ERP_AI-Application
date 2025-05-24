using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ServiceExtensions
{
    public static class DateTimeExtensions
    {
        public static DateOnly? ToDateOnly(this DateTime? dateTime)
        {
            return dateTime.HasValue ? DateOnly.FromDateTime(dateTime.Value) : null;
        }
    }
}
