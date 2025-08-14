using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.AdminSettingsVM
{
    public class OrgLocBundle
    {
        // Either "UTC±HH:mm" (e.g., "UTC+06:00") OR IANA ("Asia/Dhaka")
        public string TzValueOrIana { get; init; } = "UTC+00:00";
        public string DatePattern { get; init; } = "yyyy-MM-dd";
        public string TimePattern { get; init; } = "HH:mm";
    }
}
