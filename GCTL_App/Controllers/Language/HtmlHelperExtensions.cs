using GCTL.Service.Language;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Controllers.Language
{
    //public static class HtmlHelperExtensions
    //{

    //    public static IHtmlContent Translate(this IHtmlHelper htmlHelper, string defaultText, string pageCodeKey = null)
    //    {
    //        var context = htmlHelper.ViewContext.HttpContext;
    //        var translateService = context.RequestServices.GetService<ITranslateService>();

    //        string language = context.Items["Language"] as string ?? "en";

    //        // Optional page code base if needed — can be improved if passed from controller
    //        string pageCode = pageCodeKey ?? "000000";

    //        string translatedText = translateService.GetTranslationInd(defaultText, pageCode, language);
    //        return new HtmlString(translatedText);
    //    }
    //}



    public static class HtmlHelperExtensions
    {
        public static IHtmlContent Translate(this IHtmlHelper htmlHelper, string defaultText, string pageCode = "000000")
        {
            var context = htmlHelper.ViewContext.HttpContext;
            var translateService = context.RequestServices.GetService<ITranslateService>();

            string language = context.Items["Language"] as string ?? "en";

            var translated = translateService?.GetTranslationInd(defaultText, pageCode, language) ?? defaultText;

            return new HtmlString(translated);
        }
    }



}
