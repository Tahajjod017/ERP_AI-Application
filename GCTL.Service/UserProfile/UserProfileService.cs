using GCTL.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.UserProfile
{
    public class UserProfileService : IUserProfileService
    {
        private readonly AppDbContext _context;

        public UserProfileService(AppDbContext context)
        {
            _context = context;
        }

        public (string FullName, string ProfilePicturePath) GetUserProfileAsync(string userId)
        {
            string fullName = "Guest User";
            string profilePicturePath = "/uploads/employee/No_image_available.svg.png";

            if (!string.IsNullOrEmpty(userId))
            {
                var user =  _context.Users
                    .FirstOrDefault(u => u.Id == userId);

                if (user != null)
                {
                    var empId = user.EmployeeId;

                    var employee =  _context.Employees
                        .FirstOrDefault(e => e.EmployeeID == empId);

                    if (employee != null)
                    {
                        fullName = employee.FirstName +" "+employee.LastName ?? fullName;
                        profilePicturePath = !string.IsNullOrEmpty(employee.EmployeeImageFileName)
                            ? employee.EmployeeImageFileName
                            : profilePicturePath;
                    }
                }
            }

            return (fullName, profilePicturePath);
        }
    }

}
