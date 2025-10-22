using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.FeviconDynamic
{
    public static class HtmlExtensions
    {
        public static async Task<IHtmlContent> FaviconLinkAsync(this IHtmlHelper? html, ClaimsPrincipal? user, IBrandingAssetService? brandingAssetService)
        {
            // Retrieve the favicon path dynamically using the provided service.  
            var path = await brandingAssetService.GetFaviconPathForUserAsync(user);
            return new HtmlString($"<link rel=\"icon\" href=\"{path}\">");
        }
        public static async Task<IHtmlContent> LoginLogoImage(this IHtmlHelper? html, ClaimsPrincipal? user, IBrandingAssetService? brandingAssetService, string altText = "Logo", int width = 100)
        {
            // Retrieve the favicon path dynamically using the provided service.  
            var path = await brandingAssetService.GetLogoPathForLoginPageAsync();
            return new HtmlString($"<img src=\"{path}\" alt=\"{altText}\" width=\"{width}\" />");


        }
        //public static async Task<IHtmlContent> LogoImageAsync(this IHtmlHelper? html, ClaimsPrincipal? user, IBrandingAssetService? brandingAssetService, string altText = "Logo", string cssClass = "", int width = 50)
        //{
        //    if (brandingAssetService == null || user == null)
        //    {
        //        return new HtmlString($"<img src=\"/media/company/logo/default.png\" alt=\"{altText}\" width=\"{width}\" class=\"{cssClass}\">");
        //    }

        //    var path = await brandingAssetService.GetLogoPathForUserAsync(user);
        //    if (string.IsNullOrWhiteSpace(path))
        //    {
        //        path = "/media/company/logo/default.png";
        //    }

        //    return new HtmlString($"<img src=\"{path}\" alt=\"{altText}\" width=\"{width}\" class=\"{cssClass}\">");
        //}
        public static IHtmlContent LogoImage(
       this IHtmlHelper html,
       string? path,
       string altText = "Logo",
       int width = 50)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "/media/company/logo/default.png";
            }

            return new HtmlString($"<img src=\"{path}\" alt=\"{altText}\" width=\"{width}\" />");
        }






    }
}
