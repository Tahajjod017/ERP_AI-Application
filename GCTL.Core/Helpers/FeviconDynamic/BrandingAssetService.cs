using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.FeviconDynamic
{
    public class BrandingAssetService : IBrandingAssetService
    {
        private readonly AppDbContext _context;
        private readonly IGenericRepository<Organization> _orgnRepository;

        public BrandingAssetService(AppDbContext context, IGenericRepository<Organization> orgnRepository)
        {
            _context = context;
            _orgnRepository = orgnRepository;
        }

        public async Task<string> GetFaviconPathForUserAsync(ClaimsPrincipal user)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return "/media/company/fevicon/default.png";

            var userFetch = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userFetch?.OrganizationID == null)
                return "/media/company/fevicon/default.png";

            var faviconFileName = await _orgnRepository.AllActive()
                .Where(x => x.OrganizationID == userFetch.OrganizationID)
                .Select(x => x.FaviconLink)
                .FirstOrDefaultAsync();

            return string.IsNullOrEmpty(faviconFileName)
                ? "/media/company/fevicon/default.png"
                : $"/media/company/fevicon/{faviconFileName}";
        }

        public async Task<string> GetLogoPathForUserAsync(ClaimsPrincipal user)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return "/media/company/logo/default.png";

            var userFetch = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userFetch?.OrganizationID == null)
                return "/media/company/logo/default.png";

            var logoFileName = await _orgnRepository.AllActive()
                .Where(x => x.OrganizationID == userFetch.OrganizationID)
                .Select(x => x.LogoLink)
                .FirstOrDefaultAsync();

            return string.IsNullOrEmpty(logoFileName)
                ? "/media/company/logo/default.png"
                : $"/media/company/logo/{logoFileName}";
        }

        public async Task<string> GetLogoPathForLoginPageAsync()
        {
            // Get all active organizations
            var activeOrgs = await _orgnRepository.AllActive()
                .Select(x => x.LogoLink)
                .Where(x => !string.IsNullOrEmpty(x))
                .ToListAsync();

            // If more than one organization, use default logo
            if (activeOrgs.Count != 1)
            {
                return "/media/MultipleCompany/default.png";
            }

            // Only one organization — use its logo
            return $"/media/company/logo/{activeOrgs.First()}";
        }


    }
   
}
