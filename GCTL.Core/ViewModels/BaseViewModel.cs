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
       
       // public decimal Id { get; set; }
       // public string? Code { get; set; }
        public string Message { get; set; } = string.Empty;

        // Audit
        //public DateTime? CreatedAt { get; set; }
      //  public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        //public bool? IsDeleted { get; set; }
       // public DateTime? DeletedAt { get; set; }
        public int? DeletedBy { get; set; }
        public string? LIP { get; set; }
        public string? LMAC { get; set; }
       // public DefaultRoles Role { get; set; }
        //public bool IsAdmin { get; set; }
        public string? UserId { get; set; }
        public string? UserEmail { get; set; }
    }
}
