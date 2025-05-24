namespace GCTL_App.ViewModels.VisitingVM
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
