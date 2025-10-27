
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace GCTL.Core.Helpers
{
    public class UserInfoHelper
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public int? CreatedBy { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public int? OrganizationID { get; set; }

    }
}

