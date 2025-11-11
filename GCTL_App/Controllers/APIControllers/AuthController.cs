using GCTL.Core.ViewModels.APIViewModels;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GCTL_App.Controllers.APIControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Repositories & Services
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthController(UserManager<ApplicationUser> userManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }
        #endregion


        #region AppsLogin
        [HttpPost("AppsLogin")]
        public async Task<IActionResult> AppsLogin([FromBody] LoginVM model)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserName == model.Username)
                    .Select(u => new
                    {
                        Id = u.EmployeeId,
                        UserName = u.UserName,
                        FirstName = u.Employees.FirstName,
                        LastName = u.Employees.LastName,
                        EmployeeId = u.Employees.EmployeeCode,
                        OrgAppVersion = u.Organization.MobileApps.Select(m => m.AppVersion).FirstOrDefault()
                    }).FirstOrDefaultAsync();

                if (user == null)
                    return Unauthorized(new { statusCode = 404, message = "User not found." });

                var dbUser = await _userManager.FindByNameAsync(model.Username);
                if (dbUser == null || !await _userManager.CheckPasswordAsync(dbUser, model.Password))
                    return Unauthorized(new { statusCode = 404, message = "Password is incorrect." });

                // Compare app versions
                var currentVersion = user.OrgAppVersion ?? "1.0.0";
                //if (!string.Equals(currentVersion, model.AppVersion, StringComparison.OrdinalIgnoreCase))
                //{
                //    return Ok(new
                //    {
                //        statusCode = 426,
                //        message = $"Please update your app to version {currentVersion}.",
                //        currentVersion
                //    });
                //}

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = GetToken(authClaims);

                return Ok(new
                {
                    id = user.Id,
                    username = user.UserName,
                    password = "",
                    type = "User",
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    employeeId = user.EmployeeId,
                    role = "User",
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    appVersion = currentVersion,
                });
            }
            catch (Exception ex)
            {
                return Ok(new { message = ex.Message });
                throw;
            }
        }
        #endregion


        #region GetToken
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var tokenValidity = int.Parse(_configuration["Jwt:TokenValidityMins"] ?? "30");

            // Dynamically detect the current host (local or production)
            var request = HttpContext?.Request;
            var currentIssuer = $"{request.Scheme}://{request.Host}/"; // e.g. https://localhost:7086/ or https://yourdomain.com/

            var token = new JwtSecurityToken(
                //issuer: _configuration["Jwt:Issuer"], // This is static from config
                //audience: _configuration["Jwt:Audience"], // This is static from config
                issuer: currentIssuer,
                audience: currentIssuer,
                expires: DateTime.UtcNow.AddMinutes(tokenValidity),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return token;
        }
        #endregion
    }
}
