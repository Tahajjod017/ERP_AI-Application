using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Enums
{
    public enum ShipmentStatus
    {
        Pending = 1,
        Packed = 2,
        Shipped = 3,
        InTransit = 4,
        Delivered = 5,
        Cancelled = 6
    }
}
