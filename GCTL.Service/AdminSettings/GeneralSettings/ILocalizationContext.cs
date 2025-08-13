using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AdminSettings.GeneralSettings
{
    public interface ILocalizationContext
    {
        DateTimeZone Zone { get; set; }
        string DatePattern { get; set; }
        string TimePattern { get; set; }
        string DateTimePattern { get; } // computed
    }
}
