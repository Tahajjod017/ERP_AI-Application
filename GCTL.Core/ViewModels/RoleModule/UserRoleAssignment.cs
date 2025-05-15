namespace GCTL.Core.ViewModels.RoleModule
{
    public class UserRoleAssignment
    {
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public List<string>? AvailableRoles { get; set; }
        public string? SelectedRole { get; set; }
        public List<string>? AssignedRoles { get; set; }
    }
}