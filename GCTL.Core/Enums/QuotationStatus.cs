using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Enums
{
    public enum QuotationStatus
    {
        Draft = 1,
        Sent = 2,
        Approved = 3,
        Rejected = 4,
        Converted = 5
    }

    public enum WorkOrderStatus
    {
        Active = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }

    public enum ChalanStatus
    {
        Partial = 1,
        Complete = 2
    }

    public enum InvoiceStatus
    {
        Draft = 1,
        Sent = 2,
        Paid = 3,
        Overdue = 4,
        Cancelled = 5
    }
}
