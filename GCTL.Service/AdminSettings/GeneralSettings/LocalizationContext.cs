using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public class LocalizationContext:ILocalizationContext
    {
        public DateTimeZone Zone { get; set; } = DateTimeZone.Utc;
        public string DatePattern { get; set; } = "yyyy-MM-dd";
        public string TimePattern { get; set; } = "HH:mm";
        public string DateTimePattern => $"{DatePattern} {TimePattern}";
    }
}
