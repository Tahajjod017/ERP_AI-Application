using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Enums
{
    public enum Priority
    {
              
        Normal = 1,    // Standard processing
        Important = 2, // Needs attention but not urgent
        Urgent = 3,    // Requires fast action
        Critical = 4   // Top priority, immediate handling
    }
}
