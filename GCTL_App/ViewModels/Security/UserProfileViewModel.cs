namespace GCTL_App.ViewModels.Security
{
    public class UserProfileViewModel 
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Designation { get; set; }
        public string? Department { get; set; }
        public string? Role { get; set; }
        public string? EmployeeCode { get; set; }
        public string? ImageName { get; set; }
        public ChangePasswordRequest? PasswordRequest { get; set; }

    }
}
