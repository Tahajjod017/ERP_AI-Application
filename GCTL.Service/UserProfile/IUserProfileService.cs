using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.UserProfile
{
    public interface IUserProfileService
    {
        (string FullName, string ProfilePicturePath) GetUserProfileAsync(string userId);
        // New method to get only the current Employee ID
        Task<int?> GetCurrentEmployeeIdAsync(string userId);
    }

}
