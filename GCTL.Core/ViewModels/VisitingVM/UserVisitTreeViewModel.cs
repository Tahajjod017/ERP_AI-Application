using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels.VisitingVM
{
    public class UserVisitTreeViewModel
    {
        public string? UserId { get; set; }
        public List<PageVisit>? Visits { get; set; }

        public class PageVisit
        {
            public string? Path { get; set; }
            public double? DurationInSeconds { get; set; }
            public DateTime VisitTime { get; set; }
        }
    }
}
