using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Enums
{
    public enum PurchaseOrderStatus
    {
        Draft = 1,
        Pending = 2,
        Approved = 3,
        PartiallyReceived = 4,
        Received = 5,
        Converted = 6,
        Cancelled = 7
    }
}
