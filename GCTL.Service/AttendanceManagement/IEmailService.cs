using GCTL.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Service.AttendanceManagement
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailVM model, int? orgId);
    }
}
