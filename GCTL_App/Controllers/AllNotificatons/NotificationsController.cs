using GCTL.Service.AllNotifications;
using GCTL.Service.Employees.EmpTransfer;
using GCTL.Service.Language;
using GCTL.Service.UserProfile;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GCTL_App.Controllers.AllNotificatons
{
    public class NotificationsController : BaseController
    {
        private INotificationsService notificationsService;
        public NotificationsController(ITranslateService translateService, IUserProfileService userProfileService, INotificationsService notificationsService) : base(translateService, userProfileService)
        {
            this.notificationsService = notificationsService;
        }

        public IActionResult Index()
        {
            return View();
        }
        #region Get All Data List

        [Route("Notifications/GetAllNotificationsAsync")]

        [HttpGet]
        public async Task<IActionResult> GetAllNotificationsAsync()
        {
            try
            {
                string url = GetEmployeePictureURL();
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var data = await notificationsService.GetAllNotificationsAsync( url, userId);
                return Json(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        #endregion
    }
}
