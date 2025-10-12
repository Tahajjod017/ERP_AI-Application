using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.BackgroundServices
{
    public interface IScheduledTimeTask
    {
        TimeSpan ScheduledTime { get; }
    }
}
