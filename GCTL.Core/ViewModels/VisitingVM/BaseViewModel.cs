using GCTL.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.ViewModels
{
    public class BaseViewModel
    {
        public string Message { get; set; } = string.Empty;
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
        public int? OrganizationID { get; set; }
    }
}
