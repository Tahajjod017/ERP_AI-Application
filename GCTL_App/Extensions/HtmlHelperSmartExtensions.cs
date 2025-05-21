using GCTL_App.Controllers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GCTL_App.Extensions
{
    public static class HtmlHelperSmartExtensions
    {
        public static IHtmlContent SmartLocalize(this IHtmlHelper htmlHelper, string defaultText)
        {
            if (htmlHelper.ViewData["BaseControllerInstance"] is BaseController controller)
            {
                var method = controller.GetType().GetMethod("SmartLocalizeText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method != null)
                {
                    string translated = (string)method.Invoke(controller, new object[] { defaultText });
                    return new HtmlString(translated);
                }
            }

            // fallback
            return new HtmlString(defaultText);
        }
    }
}
