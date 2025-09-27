using GCTL.Core.Repository;
using GCTL.Data.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Microsoft.AspNetCore.Mvc.Rendering;

using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Razor;


namespace GCTL.Core.Helpers.CommonSelectMasterDropDown
{
    public class CommonDropDownService : ICommonDroDownService
    {
        private readonly IGenericRepository<Organization> organization;
          private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public CommonDropDownService(IGenericRepository<Organization> organization, IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            this.organization = organization;
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<List<CommonDropDownVM>> GetAllOrganizationsAsync()
        {
            try
            {
                var orgaList = await organization.AllActive()
                    .Select(x => new CommonDropDownVM
                    {
                        Id = x.OrganizationID,
                        Name = x.OrganizationName
                    }).ToListAsync();
                return orgaList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var viewResult = _viewEngine.FindView(actionContext, viewName, false);
            if (viewResult.View == null)
                throw new ArgumentNullException($"{viewName} does not match any available view");
            using var sw = new StringWriter();
            var viewDictionary = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary()) { Model = model };
            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
            var viewContext = new ViewContext(actionContext, viewResult.View, viewDictionary, tempData, sw, new HtmlHelperOptions());
            await viewResult.View.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
