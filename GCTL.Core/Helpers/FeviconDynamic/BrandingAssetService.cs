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

        //public async Task<string> GetFaviconPathForUserAsync(ClaimsPrincipal user)
        //{
        //    try
        //    {
        //        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //            return "/media/company/fevicon/default2.png";

        //        var userFetch = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        //        if (userFetch?.OrganizationID == null)
        //            return "/media/MultipleCompany/default.png";

        //        var faviconFileName = await _orgnRepository.AllActive()
        //            .Where(x => x.OrganizationID == userFetch.OrganizationID)
        //            .Select(x => x.FaviconLink)
        //            .FirstOrDefaultAsync();

        //        return string.IsNullOrEmpty(faviconFileName)
        //            ? "/media/company/fevicon/default2.png"
        //            : $"/media/company/fevicon/{faviconFileName}";
        //    }
        //    catch (Exception ex)
        //    {

        //        throw;
        //    }

        //}



        public async Task<string> GetFaviconPathForUserAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return "/media/company/fevicon/default2.png";

                var userFetch = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (userFetch?.OrganizationID == null)
                    return "/media/MultipleCompany/default.png";

                // ✅ Handle null FaviconLink safely in query
                var faviconFileName = await _orgnRepository.AllActive()
                    .Where(x => x.OrganizationID == userFetch.OrganizationID)
                    .Select(x => x.FaviconLink ?? string.Empty) // prevents SqlNullValueException
                    .FirstOrDefaultAsync();

                // ✅ Return fallback if null or empty
                if (string.IsNullOrWhiteSpace(faviconFileName))
                    return "/media/company/fevicon/default2.png";

                return $"/media/company/fevicon/{faviconFileName}";
            }
            catch (Exception ex)
            {
                // Log exception (avoid rethrowing naked exception)
                Console.WriteLine($"Error in GetFaviconPathForUserAsync: {ex.Message}");
                return "/media/company/fevicon/default2.png";
            }
        }


        public async Task<string> GetLogoPathForUserAsync(ClaimsPrincipal user)
        {
            try
            {
                var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                    return "/media/company/logo/default2.png";

                var userFetch = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (userFetch?.OrganizationID == null)
                    return "/media/MultipleCompany/default.png";

                // ✅ Handle null LogoLink safely
                var logoFileName = await _orgnRepository.AllActive()
                    .Where(x => x.OrganizationID == userFetch.OrganizationID)
                    .Select(x => x.LogoLink ?? string.Empty) // prevents null exception
                    .FirstOrDefaultAsync();

                // ✅ Fallback to default if null or empty
                if (string.IsNullOrWhiteSpace(logoFileName))
                    return "/media/company/logo/default2.png";

                return $"/media/company/logo/{logoFileName}";
            }
            catch (Exception ex)
            {
                // Optional: log the exception for debugging
                Console.WriteLine($"Error in GetLogoPathForUserAsync: {ex.Message}");
                return "/media/company/logo/default2.png";
            }
        }




        //public async Task<string> GetLogoPathForUserAsync(ClaimsPrincipal user)
        //{
        //    try
        //    {
        //        var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId))
        //            return "/media/company/logo/default2.png";

        //        var userFetch = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        //        if (userFetch?.OrganizationID == null)
        //            return "/media/MultipleCompany/default.png";

        //        var logoFileName = await _orgnRepository.AllActive()
        //            .Where(x => x.OrganizationID == userFetch.OrganizationID)
        //            .Select(x => x.LogoLink)
        //            .FirstOrDefaultAsync();

        //        return string.IsNullOrEmpty(logoFileName)
        //            ? "/media/company/logo/default2.png"
        //            : $"/media/company/logo/{logoFileName}";
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }

        //}

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
