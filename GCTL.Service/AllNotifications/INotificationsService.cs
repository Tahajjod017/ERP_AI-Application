using GCTL.Core.ViewModels;
using GCTL.Core.ViewModels.AllNotificastios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AllNotifications
{
    public interface INotificationsService
    {
        Task<CommonReturnViewModel> GetAllNotificationsAsync(string url, string userId);
        Task<CommonReturnViewModel> IsCheckedAsync(IsCheckedVM entityVM);
    }
}
