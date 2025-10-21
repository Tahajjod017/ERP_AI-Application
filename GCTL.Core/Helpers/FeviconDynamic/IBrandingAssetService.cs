using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.FeviconDynamic
{
    public interface IBrandingAssetService
    {
        Task<string> GetFaviconPathForUserAsync(ClaimsPrincipal user);
        Task<string> GetLogoPathForUserAsync(ClaimsPrincipal user);
        Task<string> GetLogoPathForLoginPageAsync();


    }
}
