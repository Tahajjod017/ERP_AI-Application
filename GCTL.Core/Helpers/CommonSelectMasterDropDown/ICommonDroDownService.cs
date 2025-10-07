using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers.CommonSelectMasterDropDown
{
    public interface ICommonDroDownService
    {
        Task<List<CommonDropDownVM>> GetAllOrganizationsAsync();
        Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model);

    }
}
