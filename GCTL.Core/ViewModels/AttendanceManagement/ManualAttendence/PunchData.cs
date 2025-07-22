namespace GCTL.Core.ViewModels.AttendanceManagement.ManualAttendence
{
    public class PunchData
    {
        public string Time { get; set; }
        public string Label { get; set; }
        public string Icon { get; set; }
        public bool NotPunched { get; set; }
        public bool Deletable { get; set; }
    }
}